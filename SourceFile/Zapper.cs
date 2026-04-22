using System;
using System.Collections;
using System.Collections.Generic;
using Train;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

public class Zapper : MonoBehaviour
{
	private LineRenderer lr;

	private Rigidbody rb;

	private AudioSource aud;

	[HideInInspector]
	public float damage = 10f;

	[HideInInspector]
	public GameObject sourceWeapon;

	public Transform lineStartTransform;

	public Rigidbody connectedRB;

	private ConfigurableJoint joint;

	[SerializeField]
	private GameObject openProngs;

	[SerializeField]
	private GameObject closedProngs;

	public float maxDistance;

	[HideInInspector]
	public float distance;

	[HideInInspector]
	public float charge;

	[HideInInspector]
	public float breakTimer;

	[HideInInspector]
	public bool raycastBlocked;

	private bool broken;

	public bool attached;

	public EnemyIdentifier attachedEnemy;

	public EnemyIdentifierIdentifier hitLimb;

	[SerializeField]
	private GameObject attachSound;

	[SerializeField]
	private Transform lightningPulseOrb;

	private LineRenderer pulseLine;

	[SerializeField]
	private GameObject zapParticle;

	[SerializeField]
	private AudioSource[] distanceWarningSounds;

	[SerializeField]
	private AudioSource cableSnap;

	[SerializeField]
	private AudioSource boostSound;

	[SerializeField]
	private GameObject breakParticle;

	private readonly List<PortalTraversalV2> portalTraversals = new List<PortalTraversalV2>();

	private LineRendererPortalHelper lineRendererPortalHelper;

	private bool portalJointBroken;

	private float jointLimit;

	private int playerAddedTraversals;

	[Header("Portal Settings")]
	[SerializeField]
	private float portalEdgeMargin = 0.1f;

	private void Awake()
	{
		lr = GetComponent<LineRenderer>();
		joint = GetComponent<ConfigurableJoint>();
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		pulseLine = lightningPulseOrb.GetComponent<LineRenderer>();
		rb.useGravity = false;
	}

	private void Start()
	{
		if ((bool)joint)
		{
			joint.connectedBody = connectedRB;
			SoftJointLimit linearLimit = joint.linearLimit;
			linearLimit.limit = maxDistance - 5f;
			joint.linearLimit = linearLimit;
			jointLimit = linearLimit.limit;
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<NewMovement>.Instance != null)
		{
			NewMovement? instance = MonoSingleton<NewMovement>.Instance;
			instance.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Combine(instance.onPortalTraversed, new Action<PortalTravelDetails>(OnPlayerPortalTraversed));
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<NewMovement>.Instance != null)
		{
			NewMovement? instance = MonoSingleton<NewMovement>.Instance;
			instance.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Remove(instance.onPortalTraversed, new Action<PortalTravelDetails>(OnPlayerPortalTraversed));
		}
		if (!broken && (bool)attachedEnemy)
		{
			attachedEnemy.StartCoroutine(ZapNextFrame());
		}
		ResetPortalTraversals();
	}

	private void OnPlayerPortalTraversed(PortalTravelDetails details)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (broken)
		{
			return;
		}
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle portalHandle = details.portalSequence[i];
			if (portalTraversals.Count > 0 && portalTraversals[0].portalHandle == portalHandle)
			{
				portalTraversals.RemoveAt(0);
				if (playerAddedTraversals > 0)
				{
					playerAddedTraversals--;
				}
				continue;
			}
			PortalHandle handle = portalHandle.Reverse();
			Portal portalObject = PortalUtils.GetPortalObject(handle);
			NativePortalTransform nativePortalTransform = portalObject.GetTransform(handle.side);
			Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(handle.side);
			float3 center = nativePortalTransform.center;
			Vector3 exit = travelMatrix.MultiplyPoint3x4(float3.op_Implicit(center));
			float3 forward = nativePortalTransform.forward;
			Vector3 exitDir = travelMatrix.MultiplyVector(float3.op_Implicit(forward));
			PortalTraversalV2 item = new PortalTraversalV2(float3.op_Implicit(center), float3.op_Implicit(forward), exit, exitDir, handle, portalObject);
			portalTraversals.Insert(0, item);
			playerAddedTraversals++;
			if ((bool)joint)
			{
				joint.connectedBody = null;
				UnityEngine.Object.Destroy(joint);
				joint = null;
				portalJointBroken = true;
			}
			if (lineRendererPortalHelper == null)
			{
				lineRendererPortalHelper = LineRendererPortalHelper.GetOrCreateHelper(lr);
			}
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
		}
	}

	private IEnumerator ZapNextFrame()
	{
		yield return null;
		Zap();
	}

	private void Update()
	{
		lr.SetPosition(0, lineStartTransform.position);
		lr.SetPosition(lr.positionCount - 1, base.transform.position);
		distance = GetTotalLineDistance();
		Color color = new Color(0.5f, 0.5f, 0.5f);
		if (breakTimer > 0f)
		{
			color = ((breakTimer % 0.1f > 0.05f) ? Color.black : Color.white);
		}
		else if (distance > maxDistance - 10f)
		{
			color = Color.Lerp(Color.red, color, (maxDistance - distance) / 10f);
		}
		lr.startColor = color;
		lr.endColor = color;
		if (attached)
		{
			charge = Mathf.MoveTowards(charge, 5f, Time.deltaTime);
			aud.SetPitch(1f + charge / 5f);
			float t = charge % (0.25f / (charge / 4f)) * charge;
			Vector3 segmentStart;
			Vector3 positionAlongCable = GetPositionAlongCable(t, out segmentStart);
			lightningPulseOrb.position = positionAlongCable;
			pulseLine.SetPosition(0, positionAlongCable);
			pulseLine.SetPosition(1, segmentStart);
			if (charge >= 5f || attachedEnemy.dead)
			{
				Zap();
			}
		}
	}

	private Vector3 GetPositionAlongCable(float t, out Vector3 segmentStart)
	{
		t = Mathf.Clamp01(t);
		if (portalTraversals.Count == 0)
		{
			segmentStart = lineStartTransform.position;
			return Vector3.Lerp(lineStartTransform.position, base.transform.position, t);
		}
		List<float> list = new List<float>();
		float num = 0f;
		Vector3 a = lineStartTransform.position;
		foreach (PortalTraversalV2 portalTraversal in portalTraversals)
		{
			float num2 = Vector3.Distance(a, portalTraversal.entrancePoint);
			list.Add(num2);
			num += num2;
			a = portalTraversal.exitPoint;
		}
		float num3 = Vector3.Distance(a, base.transform.position);
		list.Add(num3);
		num += num3;
		float num4 = t * num;
		float num5 = 0f;
		a = lineStartTransform.position;
		for (int i = 0; i < list.Count; i++)
		{
			float num6 = list[i];
			if (num5 + num6 >= num4)
			{
				float t2 = (num4 - num5) / num6;
				Vector3 b = ((i >= portalTraversals.Count) ? base.transform.position : portalTraversals[i].entrancePoint);
				segmentStart = a;
				return Vector3.Lerp(a, b, t2);
			}
			num5 += num6;
			if (i < portalTraversals.Count)
			{
				a = portalTraversals[i].exitPoint;
			}
		}
		segmentStart = a;
		return base.transform.position;
	}

	private float GetTotalLineDistance()
	{
		if (portalTraversals.Count == 0)
		{
			return Vector3.Distance(base.transform.position, lineStartTransform.position);
		}
		float num = 0f;
		Vector3 a = lineStartTransform.position;
		foreach (PortalTraversalV2 portalTraversal in portalTraversals)
		{
			num += Vector3.Distance(a, portalTraversal.entrancePoint);
			a = portalTraversal.exitPoint;
		}
		return num + Vector3.Distance(a, base.transform.position);
	}

	private bool CheckLineOfSight()
	{
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		if (portalTraversals.Count == 0)
		{
			return !Physics.Raycast(base.transform.position, lineStartTransform.position - base.transform.position, distance, layerMask);
		}
		Vector3 vector = base.transform.position;
		for (int num = portalTraversals.Count - 1; num >= 0; num--)
		{
			PortalTraversalV2 portalTraversalV = portalTraversals[num];
			Vector3 exitPoint = portalTraversalV.exitPoint;
			if (Physics.Raycast(vector, exitPoint - vector, Vector3.Distance(vector, exitPoint), layerMask))
			{
				return false;
			}
			vector = portalTraversalV.entrancePoint;
		}
		return !Physics.Raycast(vector, lineStartTransform.position - vector, Vector3.Distance(vector, lineStartTransform.position), layerMask);
	}

	private void FixedUpdate()
	{
		if (!attached && !broken)
		{
			Vector3 vector = MonoSingleton<NewMovement>.Instance.rb.GetGravityVector();
			for (int i = playerAddedTraversals; i < portalTraversals.Count; i++)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[i];
				vector = portalTraversalV.portalObject.GetTravelMatrix(portalTraversalV.portalHandle.side).MultiplyVector(vector);
			}
			rb.AddForce(vector, ForceMode.Acceleration);
			float num = rb.velocity.magnitude * Time.fixedDeltaTime;
			Vector3 endPoint = rb.velocity;
			Vector3 normalized = endPoint.normalized;
			if (num > 0.01f)
			{
				LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
				if (!PortalPhysicsV2.Raycast(base.transform.position, normalized, num, layerMask, out var _, out var array, out endPoint, QueryTriggerInteraction.Ignore) && array != null && array.Length != 0)
				{
					PortalTraversalV2 item = array[0];
					portalTraversals.Add(item);
					Portal portalObject = item.portalObject;
					PortalSide side = item.portalHandle.side;
					Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(side);
					Vector3 exitPoint = item.exitPoint;
					rb.position = exitPoint;
					base.transform.position = exitPoint;
					rb.velocity = travelMatrix.MultiplyVector(rb.velocity);
					Quaternion rotation = travelMatrix.rotation * base.transform.rotation;
					rb.rotation = rotation;
					base.transform.rotation = rotation;
					if ((bool)joint)
					{
						joint.connectedBody = null;
						UnityEngine.Object.Destroy(joint);
						joint = null;
						portalJointBroken = true;
					}
					if (lineRendererPortalHelper == null)
					{
						lineRendererPortalHelper = LineRendererPortalHelper.GetOrCreateHelper(lr);
					}
					lineRendererPortalHelper.UpdateTraversals(portalTraversals);
				}
			}
			if (portalTraversals.Count > 0)
			{
				RecalculatePortalIntersections();
			}
			if (portalJointBroken)
			{
				float totalLineDistance = GetTotalLineDistance();
				if (totalLineDistance > jointLimit)
				{
					Vector3 vector2;
					if (portalTraversals.Count <= 0)
					{
						vector2 = lineStartTransform.position;
					}
					else
					{
						List<PortalTraversalV2> list = portalTraversals;
						vector2 = list[list.Count - 1].exitPoint;
					}
					Vector3 vector3 = vector2;
					Vector3 normalized2 = (base.transform.position - vector3).normalized;
					float num2 = Vector3.Dot(rb.velocity, normalized2);
					if (num2 > 0f)
					{
						rb.velocity -= normalized2 * num2;
					}
					float num3 = totalLineDistance - jointLimit;
					rb.position -= normalized2 * num3;
					base.transform.position = rb.position;
				}
			}
		}
		if (!attached)
		{
			return;
		}
		RecalculatePortalIntersections();
		raycastBlocked = !CheckLineOfSight();
		if (distance > maxDistance || raycastBlocked)
		{
			AudioSource[] array2 = distanceWarningSounds;
			foreach (AudioSource val in array2)
			{
				if (breakTimer == 0f)
				{
					val.Play(tracked: true);
				}
				val.SetPitch((!raycastBlocked) ? 1 : 2);
			}
			breakTimer = Mathf.MoveTowards(breakTimer, 1f, Time.fixedDeltaTime * (float)((!raycastBlocked) ? 1 : 2));
			if (breakTimer >= 1f)
			{
				Break();
			}
			return;
		}
		if (breakTimer != 0f)
		{
			AudioSource[] array2 = distanceWarningSounds;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Stop();
			}
		}
		breakTimer = 0f;
	}

	private void RecalculatePortalIntersections(bool allowMigration = true)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (portalTraversals.Count == 0)
		{
			return;
		}
		Vector3 position = lineStartTransform.position;
		Vector3 vector = base.transform.position;
		for (int num = portalTraversals.Count - 1; num >= 0; num--)
		{
			PortalTraversalV2 portalTraversalV = portalTraversals[num];
			vector = portalTraversalV.portalObject.GetTravelMatrix(portalTraversalV.portalHandle.side).inverse.MultiplyPoint3x4(vector);
		}
		Vector3 vector2 = position;
		Vector3 vector3 = vector;
		for (int i = 0; i < portalTraversals.Count; i++)
		{
			PortalTraversalV2 value = portalTraversals[i];
			Portal portalObject = value.portalObject;
			PortalSide side = value.portalHandle.side;
			Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(side);
			NativePortalTransform nativePortalTransform = portalObject.GetTransform(side);
			Vector3 vector4 = float3.op_Implicit(nativePortalTransform.center);
			Vector3 vector5 = float3.op_Implicit(nativePortalTransform.forward);
			Vector3 normalized = (vector3 - vector2).normalized;
			float num2 = Vector3.Dot(vector5, normalized);
			if (!(Mathf.Abs(num2) > 0.0001f))
			{
				continue;
			}
			float num3 = Vector3.Dot(vector4 - vector2, vector5) / num2;
			if (num3 > 0f)
			{
				Vector3 worldPoint = vector2 + normalized * num3;
				worldPoint = ClampToPortalBounds(portalObject, side, worldPoint, out var wasClamped);
				if (wasClamped && allowMigration && TryMigrateToAdjacentPortal(i, vector2, normalized, num3))
				{
					RecalculatePortalIntersections(allowMigration: false);
					return;
				}
				Vector3 vector6 = travelMatrix.MultiplyPoint3x4(worldPoint);
				Vector3 exitDirection = travelMatrix.rotation * normalized;
				value.entrancePoint = worldPoint;
				value.entranceDirection = normalized;
				value.exitPoint = vector6;
				value.exitDirection = exitDirection;
				portalTraversals[i] = value;
				vector2 = vector6;
				vector3 = travelMatrix.MultiplyPoint3x4(vector3);
			}
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
		}
	}

	private bool TryMigrateToAdjacentPortal(int traversalIndex, Vector3 segmentStart, Vector3 direction, float portalPlaneDistance)
	{
		PortalTraversalV2 portalTraversalV = portalTraversals[traversalIndex];
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		float num = portalPlaneDistance + 1f;
		PortalPhysicsV2.Raycast(segmentStart, direction, num, layerMask, out var _, out var array, out var _, QueryTriggerInteraction.Ignore);
		if (array == null || array.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].portalHandle != portalTraversalV.portalHandle)
			{
				portalTraversals[traversalIndex] = array[i];
				return true;
			}
		}
		return false;
	}

	private Vector3 ClampToPortalBounds(Portal portal, PortalSide side, Vector3 worldPoint, out bool wasClamped)
	{
		NativePortalTransform nativePortalTransform = portal.GetTransform(side);
		Vector3 point = nativePortalTransform.toLocalManaged.MultiplyPoint3x4(worldPoint);
		wasClamped = false;
		if (portal.shape is PlaneShape planeShape)
		{
			float num = planeShape.width / 2f;
			float num2 = planeShape.height / 2f;
			if (Mathf.Abs(point.x) > num - portalEdgeMargin || Mathf.Abs(point.y) > num2 - portalEdgeMargin)
			{
				wasClamped = true;
			}
			point.x = Mathf.Clamp(point.x, 0f - num + portalEdgeMargin, num - portalEdgeMargin);
			point.y = Mathf.Clamp(point.y, 0f - num2 + portalEdgeMargin, num2 - portalEdgeMargin);
			point.z = 0f;
		}
		return nativePortalTransform.toWorldManaged.MultiplyPoint3x4(point);
	}

	private Vector3 ClampToPortalBounds(Portal portal, PortalSide side, Vector3 worldPoint)
	{
		bool wasClamped;
		return ClampToPortalBounds(portal, side, worldPoint, out wasClamped);
	}

	private void ResetPortalTraversals()
	{
		portalTraversals.Clear();
		playerAddedTraversals = 0;
		if (lineRendererPortalHelper != null)
		{
			UnityEngine.Object.Destroy(lineRendererPortalHelper);
			lineRendererPortalHelper = null;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!attached && !broken)
		{
			CheckAttach(other, Vector3.zero);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (attached || broken)
		{
			return;
		}
		if (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
		{
			if (other.gameObject.CompareTag("Moving") && other.gameObject.TryGetComponent<Tram>(out var component) && (bool)component.controller)
			{
				component.controller.Zap();
			}
			Break();
		}
		else
		{
			CheckAttach(other.collider, other.contacts[0].point);
		}
	}

	private void CheckAttach(Collider other, Vector3 position)
	{
		if (other.gameObject.layer != 10 && other.gameObject.layer != 11)
		{
			return;
		}
		AttributeChecker component2;
		if (other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out hitLimb) && (bool)hitLimb.eid && !hitLimb.eid.dead)
		{
			attached = true;
			attachedEnemy = hitLimb.eid;
			attachedEnemy.zapperer = this;
			base.transform.SetParent(other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform, worldPositionStays: true);
			if (!attachedEnemy.bigEnemy)
			{
				base.transform.position = other.bounds.center;
			}
			else
			{
				if (position == Vector3.zero)
				{
					position = ((!Physics.Raycast(base.transform.position - (other.bounds.center - base.transform.position).normalized, other.bounds.center - base.transform.position, out var hitInfo, Vector3.Distance(other.bounds.center, base.transform.position) + 1f, LayerMaskDefaults.Get(LMD.Enemies))) ? other.bounds.center : hitInfo.point);
				}
				base.transform.LookAt(position);
				base.transform.position = position;
			}
			rb.isKinematic = true;
			aud.Play(tracked: true);
			UnityEngine.Object.Instantiate(attachSound, base.transform.position, Quaternion.identity);
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			lightningPulseOrb.position = lineStartTransform.position;
			lightningPulseOrb.gameObject.SetActive(value: true);
			openProngs.SetActive(value: false);
			closedProngs.SetActive(value: true);
			if ((bool)joint)
			{
				joint.connectedBody = null;
				UnityEngine.Object.Destroy(joint);
				joint = null;
			}
			if (attachedEnemy.enemyType == EnemyType.Ferryman && attachedEnemy.TryGetComponent<Ferryman>(out var component) && (bool)component.currentWindup)
			{
				component.GotParried();
			}
		}
		else if (other.gameObject.TryGetComponent<AttributeChecker>(out component2) && component2.targetAttribute == HitterAttribute.Electricity)
		{
			UnityEngine.Object.Instantiate(zapParticle, component2.transform.position, Quaternion.identity);
			component2.Activate();
		}
	}

	private void Zap()
	{
		if ((bool)attachedEnemy)
		{
			attachedEnemy.hitter = "zapper";
			attachedEnemy.hitterAttributes.Add(HitterAttribute.Electricity);
			attachedEnemy.DeliverDamage(hitLimb.gameObject, Vector3.up * 1000f, broken ? hitLimb.transform.position : base.transform.position, damage, tryForExplode: true, 0f, sourceWeapon);
			MonoSingleton<WeaponCharges>.Instance.naiZapperRecharge = 0f;
			EnemyIdentifierIdentifier[] componentsInChildren = attachedEnemy.GetComponentsInChildren<EnemyIdentifierIdentifier>();
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
			{
				if (enemyIdentifierIdentifier != hitLimb && enemyIdentifierIdentifier.gameObject != attachedEnemy.gameObject)
				{
					attachedEnemy.DeliverDamage(enemyIdentifierIdentifier.gameObject, Vector3.zero, enemyIdentifierIdentifier.transform.position, Mathf.Epsilon, tryForExplode: false);
				}
				UnityEngine.Object.Instantiate(zapParticle, enemyIdentifierIdentifier.transform.position, Quaternion.identity).transform.localScale *= 0.5f;
			}
		}
		Break(successful: true);
	}

	public void Break(bool successful = false)
	{
		if (!broken)
		{
			broken = true;
			UnityEngine.Object.Instantiate(breakParticle, base.transform.position, Quaternion.identity);
			if (attached && !successful)
			{
				UnityEngine.Object.Instantiate<AudioSource>(cableSnap, base.transform.position, Quaternion.identity);
			}
			if ((bool)attachedEnemy)
			{
				attachedEnemy.zapperer = this;
			}
			ResetPortalTraversals();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void ChargeBoost(float amount)
	{
		charge += amount;
		LineRenderer lineRenderer = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, base.transform.position, Quaternion.identity);
		lineRenderer.SetPosition(0, base.transform.position);
		lineRenderer.SetPosition(1, lineStartTransform.position);
		UnityEngine.Object.Instantiate<AudioSource>(boostSound, base.transform.position, Quaternion.identity);
		if (lineRenderer.TryGetComponent<ElectricityLine>(out var component))
		{
			component.minWidth = 8f;
			component.maxWidth = 15f;
		}
	}
}
