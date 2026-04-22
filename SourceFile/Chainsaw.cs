using System;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

public class Chainsaw : MonoBehaviour
{
	[HideInInspector]
	public Rigidbody rb;

	public float damage;

	public Transform attachedTransform;

	[HideInInspector]
	public Transform lineStartTransform;

	[SerializeField]
	private AudioSource ropeSnapSound;

	private AudioSource aud;

	public AudioSource stoppedAud;

	[SerializeField]
	private GameObject ricochetEffect;

	[SerializeField]
	private AudioClip enemyHitSound;

	[SerializeField]
	private GameObject enemyHitParticle;

	private LineRenderer lr;

	private PortalLineRenderer chainLines;

	[HideInInspector]
	public bool stopped;

	public bool heated;

	public int hitAmount = 1;

	private int currentHitAmount;

	private Transform hitTarget;

	private List<Transform> hitLimbs = new List<Transform>();

	private EnemyIdentifier currentHitEnemy;

	private float multiHitCooldown;

	private float sameEnemyHitCooldown;

	[HideInInspector]
	public Vector3 originalVelocity;

	[HideInInspector]
	public bool beingPunched;

	private bool beenPunched;

	private bool inPlayer;

	private float playerHitTimer;

	private TimeSince ignorePlayerTimer;

	private float raycastBlockedTimer;

	[HideInInspector]
	public string weaponType;

	[HideInInspector]
	public GameObject sourceWeapon;

	public Nail sawbladeVersion;

	[SerializeField]
	private Renderer model;

	[SerializeField]
	private Transform sprite;

	public List<PortalTraversalV2> traversals = new List<PortalTraversalV2>();

	private bool retracting;

	private const float portalEdgeMargin = 0.1f;

	private Vector3 LastAnchorPos
	{
		get
		{
			if (traversals.Count <= 0)
			{
				return attachedTransform.position;
			}
			List<PortalTraversalV2> list = traversals;
			return list[list.Count - 1].exitPoint;
		}
	}

	private float DistanceToGun
	{
		get
		{
			float num = 0f;
			Vector3 a = attachedTransform.position;
			Vector3 entrancePoint;
			for (int i = 0; i < traversals.Count; i++)
			{
				entrancePoint = traversals[i].entrancePoint;
				num += Vector3.Distance(a, entrancePoint);
				a = traversals[i].exitPoint;
			}
			entrancePoint = base.transform.position;
			return num + Vector3.Distance(a, entrancePoint);
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		if (lineStartTransform == null)
		{
			lineStartTransform = attachedTransform;
		}
		lr = base.gameObject.GetComponentInChildren<LineRenderer>();
		if ((bool)lr)
		{
			chainLines = new PortalLineRenderer(lr);
			chainLines.SetEnabled(value: true);
		}
		ignorePlayerTimer = 0f;
		if ((bool)MonoSingleton<PortalManagerV2>.Instance && !TryGetComponent<SimplePortalTraveler>(out var _))
		{
			SimplePortalTraveler simplePortalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
			simplePortalTraveler.SetType(PortalTravellerType.PLAYER_PROJECTILE);
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnTravel));
		}
		if ((bool)MonoSingleton<NewMovement>.Instance)
		{
			NewMovement? instance = MonoSingleton<NewMovement>.Instance;
			instance.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Combine(instance.onPortalTraversed, new Action<PortalTravelDetails>(OnPlayerPortalTraversed));
		}
		Invoke("SlowUpdate", 2f);
	}

	private void OnEnable()
	{
		MonoSingleton<WeaponCharges>.Instance.shoSawAmount++;
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<WeaponCharges>.Instance)
		{
			MonoSingleton<WeaponCharges>.Instance.shoSawAmount--;
		}
		if ((bool)MonoSingleton<NewMovement>.Instance)
		{
			NewMovement? instance = MonoSingleton<NewMovement>.Instance;
			instance.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Remove(instance.onPortalTraversed, new Action<PortalTravelDetails>(OnPlayerPortalTraversed));
		}
	}

	private void OnTravel(in PortalTravelDetails details)
	{
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle portalHandle = details.portalSequence[i];
			if (retracting && traversals.Count > 0)
			{
				List<PortalTraversalV2> list = traversals;
				if (list[list.Count - 1].portalHandle.Reverse() != portalHandle)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
			}
			PortalTraversalV2 item;
			if (i == 0)
			{
				Portal portalObject = PortalUtils.GetPortalObject(portalHandle);
				Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(portalHandle.side);
				Vector3 exit = travelMatrix.MultiplyPoint3x4(details.intersection);
				Vector3 exitDir = travelMatrix.MultiplyVector((details.intersection - base.transform.position).normalized);
				item = new PortalTraversalV2(details.intersection, (details.intersection - base.transform.position).normalized, exit, exitDir, portalHandle, portalObject);
			}
			else
			{
				item = details.additionalTraversals[i - 1];
			}
			if (traversals.Count > 0)
			{
				List<PortalTraversalV2> list2 = traversals;
				if (list2[list2.Count - 1].portalHandle.Reverse() == item.portalHandle)
				{
					traversals.RemoveAt(traversals.Count - 1);
					continue;
				}
			}
			traversals.Add(item);
		}
		chainLines.SetLines(lineStartTransform.position, base.transform.position, traversals.ToArray());
		chainLines.SetEnabled(value: true);
	}

	private void OnPlayerPortalTraversed(PortalTravelDetails details)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle portalHandle = details.portalSequence[i];
			if (traversals.Count > 0 && traversals[0].portalHandle == portalHandle)
			{
				traversals.RemoveAt(0);
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
			traversals.Insert(0, item);
		}
		chainLines.SetLines(lineStartTransform.position, base.transform.position, traversals.ToArray());
		chainLines.SetEnabled(value: true);
	}

	private void SlowUpdate()
	{
		if (DistanceToGun > 1000f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Invoke("SlowUpdate", 2f);
		}
	}

	private void Update()
	{
		if (traversals.Count > 0)
		{
			RecalculatePortalIntersections();
		}
		chainLines.SetLines(lineStartTransform.position, base.transform.position, traversals.ToArray());
		chainLines.SetEnabled(value: true);
		if ((bool)rb)
		{
			if (inPlayer)
			{
				base.transform.forward = MonoSingleton<CameraController>.Instance.transform.forward * -1f;
			}
			else
			{
				base.transform.LookAt(base.transform.position + (base.transform.position - LastAnchorPos));
			}
		}
		if (sameEnemyHitCooldown > 0f && !stopped)
		{
			sameEnemyHitCooldown = Mathf.MoveTowards(sameEnemyHitCooldown, 0f, Time.deltaTime);
			if (sameEnemyHitCooldown <= 0f)
			{
				currentHitEnemy = null;
			}
		}
		if (inPlayer)
		{
			base.transform.position = attachedTransform.position;
			playerHitTimer = Mathf.MoveTowards(playerHitTimer, 0.25f, Time.deltaTime);
			stoppedAud.SetPitch(0.5f);
			stoppedAud.volume = 0.75f;
			if (playerHitTimer >= 0.25f && !beingPunched)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (hitAmount <= 1)
			{
				return;
			}
			if (multiHitCooldown > 0f)
			{
				multiHitCooldown = Mathf.MoveTowards(multiHitCooldown, 0f, Time.deltaTime);
			}
			else if (stopped)
			{
				if (!currentHitEnemy.dead && currentHitAmount > 0)
				{
					currentHitAmount--;
					DamageEnemy(hitTarget, currentHitEnemy);
				}
				if (currentHitEnemy.dead || currentHitAmount <= 0)
				{
					stopped = false;
					rb.velocity = originalVelocity.normalized * Mathf.Max(originalVelocity.magnitude, 35f);
					return;
				}
				multiHitCooldown = 0.05f;
			}
			if ((bool)(UnityEngine.Object)(object)stoppedAud)
			{
				if (stopped)
				{
					stoppedAud.SetPitch(1.1f);
					stoppedAud.volume = 0.75f;
				}
				else
				{
					stoppedAud.SetPitch(0.85f);
					stoppedAud.volume = 0.5f;
				}
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (stopped)
		{
			rb.velocity = Vector3.zero;
			return;
		}
		if (traversals.Count > 0)
		{
			List<PortalTraversalV2> list = traversals;
			PortalHandle handle = list[list.Count - 1].portalHandle.Reverse();
			NativePortalTransform nativePortalTransform = PortalUtils.GetPortalObject(handle).GetTransform(handle.side);
			if (!nativePortalTransform.IsPointInFront(base.transform.position) && Vector3.Dot(nativePortalTransform.centerManaged - base.transform.position, float3.op_Implicit(nativePortalTransform.back)) > 2f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}
		Vector3 normalized = (LastAnchorPos - base.transform.position).normalized;
		float distanceToGun = DistanceToGun;
		retracting = Vector3.Dot(rb.velocity.normalized, normalized) >= 0.5f;
		if (!retracting)
		{
			rb.velocity = Vector3.MoveTowards(rb.velocity, normalized * 100f, Time.fixedDeltaTime * distanceToGun * 10f);
		}
		else
		{
			rb.velocity = normalized * Mathf.Min(100f, Mathf.MoveTowards(rb.velocity.magnitude, 100f, Time.fixedDeltaTime * Mathf.Max(10f, distanceToGun) * 10f));
		}
		if ((float)ignorePlayerTimer > 0.66f && !inPlayer && distanceToGun < 1f)
		{
			TouchPlayer();
			return;
		}
		if (Physics.Raycast(base.transform.position, normalized, distanceToGun, LayerMaskDefaults.Get(LMD.Environment)))
		{
			raycastBlockedTimer += Time.fixedDeltaTime;
		}
		else
		{
			raycastBlockedTimer = 0f;
		}
		if (raycastBlockedTimer >= 0.25f)
		{
			TurnIntoSawblade();
			return;
		}
		RaycastHit[] array = rb.SweepTestAll(rb.velocity.normalized, rb.velocity.magnitude * 5f * Time.fixedDeltaTime, QueryTriggerInteraction.Ignore);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Array.Sort(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		bool flag = false;
		bool flag2 = false;
		for (int num = 0; num < array.Length; num++)
		{
			GameObject gameObject = array[num].transform.gameObject;
			if (gameObject.gameObject == MonoSingleton<NewMovement>.Instance.gameObject && (float)ignorePlayerTimer > 0.66f)
			{
				TouchPlayer();
			}
			else if ((gameObject.layer == 10 || gameObject.layer == 11) && (gameObject.gameObject.CompareTag("Head") || gameObject.gameObject.CompareTag("Body") || gameObject.gameObject.CompareTag("Limb") || gameObject.gameObject.CompareTag("EndLimb") || gameObject.gameObject.CompareTag("Enemy")))
			{
				TouchEnemy(gameObject.transform);
			}
			else
			{
				if (!LayerMaskDefaults.IsMatchingLayer(gameObject.layer, LMD.Environment) && gameObject.layer != 26 && !gameObject.CompareTag("Armor"))
				{
					continue;
				}
				if (gameObject.TryGetComponent<Breakable>(out var component) && component.weak && !component.specialCaseOnly)
				{
					component.Break(damage);
					return;
				}
				if (SceneHelper.IsStaticEnvironment(array[num]))
				{
					MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(array[num]);
				}
				base.transform.position = array[num].point;
				rb.velocity = Vector3.Reflect(rb.velocity.normalized, array[num].normal) * (rb.velocity.magnitude / 2f);
				flag = true;
				GameObject gameObject2 = UnityEngine.Object.Instantiate(ricochetEffect, array[num].point, Quaternion.LookRotation(array[num].normal));
				if (flag2 && gameObject2.TryGetComponent<AudioSource>(out var component2))
				{
					((Behaviour)(object)component2).enabled = false;
				}
				else
				{
					flag2 = true;
				}
				ignorePlayerTimer = 1f;
				break;
			}
		}
		if (flag)
		{
			CheckMultipleRicochets();
		}
	}

	private void TouchPlayer()
	{
		inPlayer = true;
		stopped = true;
		originalVelocity = rb.velocity;
		base.transform.position = MonoSingleton<NewMovement>.Instance.transform.position;
		model.gameObject.SetActive(value: false);
		sprite.localScale = Vector3.one * 20f;
		traversals.Clear();
	}

	private void TouchEnemy(Transform other)
	{
		if (hitAmount > 1)
		{
			if (!stopped && other.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid)
			{
				if (component.eid.dead)
				{
					HitEnemy(other, component);
				}
				else if (!(sameEnemyHitCooldown > 0f) || !(currentHitEnemy != null) || !(currentHitEnemy == component.eid))
				{
					stopped = true;
					currentHitAmount = hitAmount;
					hitTarget = other;
					currentHitEnemy = component.eid;
					originalVelocity = rb.velocity;
					sameEnemyHitCooldown = 0.25f;
				}
			}
		}
		else
		{
			HitEnemy(other);
		}
	}

	private void HitEnemy(Transform other, EnemyIdentifierIdentifier eidid = null)
	{
		if (((bool)eidid || other.TryGetComponent<EnemyIdentifierIdentifier>(out eidid)) && (bool)eidid.eid && (!(sameEnemyHitCooldown > 0f) || !(currentHitEnemy != null) || !(currentHitEnemy == eidid.eid)) && !hitLimbs.Contains(other))
		{
			if (!eidid.eid.dead)
			{
				sameEnemyHitCooldown = 0.25f;
				currentHitEnemy = eidid.eid;
				currentHitAmount--;
			}
			if ((UnityEngine.Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = enemyHitSound;
			aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
			aud.volume = 0.2f;
			aud.Play(tracked: true);
			if ((bool)eidid && (bool)eidid.eid)
			{
				DamageEnemy(other, eidid.eid);
			}
		}
	}

	private void DamageEnemy(Transform other, EnemyIdentifier eid)
	{
		eid.hitter = (beenPunched ? "chainsawbounce" : "chainsaw");
		if (!eid.hitterWeapons.Contains(weaponType))
		{
			eid.hitterWeapons.Add(weaponType);
		}
		if (enemyHitParticle != null)
		{
			UnityEngine.Object.Instantiate(enemyHitParticle, other.transform.position, Quaternion.identity).transform.localScale *= 3f;
		}
		bool dead = eid.dead;
		eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 3000f, base.transform.position, damage, tryForExplode: false, 0f, sourceWeapon);
		if (dead)
		{
			hitLimbs.Add(other);
		}
		if (heated)
		{
			Flammable componentInChildren = eid.GetComponentInChildren<Flammable>();
			if (componentInChildren != null)
			{
				componentInChildren.Burn(2f, componentInChildren.burning);
			}
		}
	}

	public void CheckMultipleRicochets(bool onStart = false)
	{
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (!Physics.Raycast(base.transform.position, rb.velocity.normalized, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				break;
			}
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && component.weak && !component.specialCaseOnly)
			{
				component.Break(damage);
				continue;
			}
			base.transform.position = hitInfo.point;
			rb.velocity = Vector3.Reflect(rb.velocity.normalized, hitInfo.normal) * (rb.velocity.magnitude / 2f);
			GameObject gameObject = UnityEngine.Object.Instantiate(ricochetEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			if (flag && gameObject.TryGetComponent<AudioSource>(out var component2))
			{
				((Behaviour)(object)component2).enabled = false;
			}
			else
			{
				flag = true;
			}
			if (SceneHelper.IsStaticEnvironment(hitInfo))
			{
				MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(hitInfo);
			}
		}
		if (onStart)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1.5f, LayerMaskDefaults.Get(LMD.Enemies));
			if (array.Length != 0)
			{
				TouchEnemy(array[0].transform);
			}
		}
	}

	public void GetPunched()
	{
		beenPunched = true;
		beingPunched = false;
		inPlayer = false;
		stopped = false;
		playerHitTimer = 0f;
		ignorePlayerTimer = 0f;
		sameEnemyHitCooldown = 0f;
		sprite.localScale = Vector3.one * 100f;
		traversals.Clear();
		model.gameObject.SetActive(value: true);
		if (hitAmount < 3)
		{
			hitAmount++;
			if (hitAmount == 3)
			{
				heated = true;
			}
		}
	}

	public void TurnIntoSawblade()
	{
		Nail nail = UnityEngine.Object.Instantiate(sawbladeVersion, base.transform.position, base.transform.rotation);
		nail.sourceWeapon = sourceWeapon;
		nail.weaponType = weaponType;
		nail.heated = heated;
		nail.rb.velocity = ((rb.velocity == Vector3.zero) ? base.transform.forward : (stopped ? originalVelocity : rb.velocity)).normalized * 105f;
		AudioSource obj = UnityEngine.Object.Instantiate<AudioSource>(ropeSnapSound, base.transform.position, Quaternion.identity);
		obj.volume /= 2f;
		base.gameObject.SetActive(value: false);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void RecalculatePortalIntersections(bool allowMigration = true)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (traversals.Count == 0)
		{
			return;
		}
		Vector3 position = lineStartTransform.position;
		Vector3 vector = base.transform.position;
		for (int num = traversals.Count - 1; num >= 0; num--)
		{
			PortalTraversalV2 portalTraversalV = traversals[num];
			vector = portalTraversalV.portalObject.GetTravelMatrix(portalTraversalV.portalHandle.side).inverse.MultiplyPoint3x4(vector);
		}
		Vector3 vector2 = position;
		Vector3 vector3 = vector;
		for (int i = 0; i < traversals.Count; i++)
		{
			PortalTraversalV2 value = traversals[i];
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
					break;
				}
				Vector3 vector6 = travelMatrix.MultiplyPoint3x4(worldPoint);
				Vector3 exitDirection = travelMatrix.rotation * normalized;
				value.entrancePoint = worldPoint;
				value.entranceDirection = normalized;
				value.exitPoint = vector6;
				value.exitDirection = exitDirection;
				traversals[i] = value;
				vector2 = vector6;
				vector3 = travelMatrix.MultiplyPoint3x4(vector3);
			}
		}
	}

	private bool TryMigrateToAdjacentPortal(int traversalIndex, Vector3 segmentStart, Vector3 direction, float portalPlaneDistance)
	{
		PortalTraversalV2 portalTraversalV = traversals[traversalIndex];
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		float maxDistance = portalPlaneDistance + 1f;
		PortalPhysicsV2.Raycast(segmentStart, direction, maxDistance, layerMask, out var _, out var portalTraversals, out var _, QueryTriggerInteraction.Ignore);
		if (portalTraversals == null || portalTraversals.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < portalTraversals.Length; i++)
		{
			if (portalTraversals[i].portalHandle != portalTraversalV.portalHandle)
			{
				traversals[traversalIndex] = portalTraversals[i];
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
			if (Mathf.Abs(point.x) > num - 0.1f || Mathf.Abs(point.y) > num2 - 0.1f)
			{
				wasClamped = true;
			}
			point.x = Mathf.Clamp(point.x, 0f - num + 0.1f, num - 0.1f);
			point.y = Mathf.Clamp(point.y, 0f - num2 + 0.1f, num2 - 0.1f);
			point.z = 0f;
		}
		return nativePortalTransform.toWorldManaged.MultiplyPoint3x4(point);
	}
}
