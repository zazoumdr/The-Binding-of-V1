using System;
using System.Collections.Generic;
using Sandbox;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HookArm : MonoSingleton<HookArm>
{
	public bool equipped;

	private LineRenderer lr;

	private Animator anim;

	private Vector3 hookPoint;

	private Vector3 previousHookPoint;

	[HideInInspector]
	public HookState state;

	private bool returning;

	[SerializeField]
	private GameObject model;

	private CapsuleCollider playerCollider;

	public Transform hand;

	public Transform hook;

	public GameObject hookModel;

	private Vector3 throwDirection;

	private float returnDistance;

	private LayerMask throwMask;

	private LayerMask enviroMask;

	private LayerMask enemyMask;

	private float throwWarp;

	private Transform caughtTransform;

	private Vector3 caughtPoint;

	private Collider caughtCollider;

	private EnemyIdentifier caughtEid;

	private List<EnemyType> deadIgnoreTypes = new List<EnemyType>();

	private List<EnemyType> lightEnemies = new List<EnemyType>();

	private GroundCheckEnemy enemyGroundCheck;

	private Rigidbody enemyRigidbody;

	private HookPoint caughtHook;

	private Vector3 caughtDir;

	private bool lightTarget;

	[SerializeField]
	private LineRenderer inspectLr;

	private bool forcingGroundCheck;

	private bool forcingFistControl;

	private AudioSource aud;

	[Header("Sounds")]
	public GameObject throwSound;

	public GameObject hitSound;

	public GameObject pullSound;

	public GameObject pullDoneSound;

	public GameObject catchSound;

	public GameObject errorSound;

	public AudioClip throwLoop;

	public AudioClip pullLoop;

	public GameObject wooshSound;

	private GameObject currentWoosh;

	public GameObject clinkSparks;

	public GameObject clinkObjectSparks;

	private float cooldown;

	private CameraFrustumTargeter targeter;

	[HideInInspector]
	public bool beingPulled;

	private List<Rigidbody> caughtObjects = new List<Rigidbody>();

	private float semiBlocked;

	private Grenade caughtGrenade;

	private Cannonball caughtCannonball;

	private readonly List<PortalTraversalV2> portalTraversals = new List<PortalTraversalV2>();

	private LineRendererPortalHelper lineRendererPortalHelper;

	private int portalTraversalGraceFrames;

	[Header("Portal Hook Settings")]
	[SerializeField]
	private float maxPortalBendAngle = 60f;

	[SerializeField]
	private float portalEdgeMargin = 0.1f;

	private MaterialPropertyBlock propBlock;

	private Vector3 currentForward
	{
		get
		{
			if (portalTraversals.Count <= 0)
			{
				return throwDirection;
			}
			List<PortalTraversalV2> list = portalTraversals;
			return list[list.Count - 1].exitDirection;
		}
	}

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		anim = GetComponent<Animator>();
		playerCollider = MonoSingleton<NewMovement>.Instance.GetComponent<CapsuleCollider>();
		aud = GetComponent<AudioSource>();
		throwMask = (int)throwMask | 0x400;
		throwMask = (int)throwMask | 0x800;
		throwMask = (int)throwMask | 0x1000;
		throwMask = (int)throwMask | 0x4000;
		throwMask = (int)throwMask | 0x10000;
		throwMask = (int)throwMask | 0x400000;
		throwMask = (int)throwMask | 0x4000000;
		enviroMask = (int)enviroMask | 0x40;
		enviroMask = (int)enviroMask | 0x80;
		enviroMask = (int)enviroMask | 0x100;
		enviroMask = (int)enviroMask | 0x10000;
		enviroMask = (int)enviroMask | 0x40000;
		enviroMask = (int)enviroMask | 0x1000000;
		enemyMask = (int)enemyMask | 0x800;
		enemyMask = (int)enemyMask | 0x4000000;
		enemyMask = (int)enemyMask | 0x1000;
		deadIgnoreTypes.Add(EnemyType.Drone);
		deadIgnoreTypes.Add(EnemyType.MaliciousFace);
		deadIgnoreTypes.Add(EnemyType.Mindflayer);
		deadIgnoreTypes.Add(EnemyType.Gutterman);
		deadIgnoreTypes.Add(EnemyType.Virtue);
		deadIgnoreTypes.Add(EnemyType.HideousMass);
		lightEnemies.Add(EnemyType.Drone);
		lightEnemies.Add(EnemyType.Filth);
		lightEnemies.Add(EnemyType.Schism);
		lightEnemies.Add(EnemyType.Soldier);
		lightEnemies.Add(EnemyType.Stray);
		lightEnemies.Add(EnemyType.Streetcleaner);
		propBlock = new MaterialPropertyBlock();
		model.SetActive(value: false);
	}

	public void Inspect()
	{
		model.SetActive(value: true);
		inspectLr.enabled = true;
		anim.Play("Inspect", -1, 0f);
	}

	private void OnEnable()
	{
		NewMovement newMovement = MonoSingleton<NewMovement>.Instance;
		if ((bool)newMovement)
		{
			newMovement.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Combine(newMovement.onPortalTraversed, new Action<PortalTravelDetails>(OnPortalTraversed));
		}
	}

	private void OnDisable()
	{
		NewMovement? newMovement = MonoSingleton<NewMovement>.Instance;
		newMovement.onPortalTraversed = (Action<PortalTravelDetails>)Delegate.Remove(newMovement.onPortalTraversed, new Action<PortalTravelDetails>(OnPortalTraversed));
	}

	private void OnPortalTraversed(PortalTravelDetails details)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (state == HookState.Ready)
		{
			return;
		}
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle portalHandle = details.portalSequence[i];
			if (portalTraversals.Count > 0 && portalTraversals[0].portalHandle == portalHandle)
			{
				portalTraversals.RemoveAt(0);
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
			if (lineRendererPortalHelper == null)
			{
				lineRendererPortalHelper = LineRendererPortalHelper.GetOrCreateHelper(lr);
			}
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
			UpdateLineRendererPositions();
		}
		portalTraversalGraceFrames = 3;
	}

	private void UpdateLineRendererPositions()
	{
		throwWarp = Mathf.MoveTowards(throwWarp, 0f, Time.deltaTime * 6.5f);
		lr.SetPosition(0, hand.position);
		float num = GetTotalLineDistance() / 300f;
		float x = num + 1f;
		lr.textureScale = new Vector2(x, 1f);
		propBlock.SetVector("_MainTex_ST", new Vector4(1f, 1f, (0f - num) * 10f, 1f));
		lr.SetPropertyBlock(propBlock);
		for (int i = 1; i < lr.positionCount - 1; i++)
		{
			float num2 = 3f;
			if (i % 2 == 0)
			{
				num2 = -3f;
			}
			Vector3 vector = 1f / (float)i * num2 * throwWarp * base.transform.up;
			if ((bool)lineRendererPortalHelper)
			{
				lr.SetPosition(i, vector);
			}
			else
			{
				lr.SetPosition(i, Vector3.Lerp(hand.position, hookPoint, (float)i / (float)lr.positionCount) + vector);
			}
		}
		lr.SetPosition(lr.positionCount - 1, hookPoint);
	}

	private Vector3 GetNextCaughtPoint()
	{
		Vector3 result = caughtTransform.position + caughtPoint;
		List<PortalTraversalV2> list = portalTraversals;
		if (list != null && list.Count > 0)
		{
			result = portalTraversals[0].entrancePoint;
		}
		return result;
	}

	private bool TryMigrateToAdjacentPortal(int traversalIndex, Vector3 segmentStart, Vector3 direction, float portalPlaneDistance)
	{
		PortalTraversalV2 portalTraversalV = portalTraversals[traversalIndex];
		float maxDistance = portalPlaneDistance + 1f;
		PortalPhysicsV2.Raycast(segmentStart, direction, maxDistance, enviroMask, out var _, out var array, out var _, QueryTriggerInteraction.Ignore);
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

	private void RecalculateThrowingIntersections(bool allowMigration = true)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (portalTraversals.Count == 0)
		{
			return;
		}
		Vector3 position = hand.position;
		Vector3 vector = hookPoint;
		for (int num = portalTraversals.Count - 1; num >= 0; num--)
		{
			PortalTraversalV2 value = portalTraversals[num];
			Portal portalObject = value.portalObject;
			PortalSide side = value.portalHandle.side;
			Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(side);
			Vector3 vector2 = travelMatrix.inverse.MultiplyPoint3x4(vector);
			NativePortalTransform nativePortalTransform = portalObject.GetTransform(side);
			Vector3 vector3 = float3.op_Implicit(nativePortalTransform.center);
			Vector3 vector4 = float3.op_Implicit(nativePortalTransform.forward);
			Vector3 vector5 = ((num == 0) ? position : portalTraversals[num - 1].exitPoint);
			Vector3 normalized = (vector2 - vector5).normalized;
			float num2 = Vector3.Dot(vector4, normalized);
			if (Mathf.Abs(num2) > 0.0001f)
			{
				float num3 = Vector3.Dot(vector3 - vector5, vector4) / num2;
				if (num3 > 0f)
				{
					Vector3 worldPoint = vector5 + normalized * num3;
					worldPoint = ClampToPortalBounds(portalObject, side, worldPoint, out var wasClamped);
					if (wasClamped && allowMigration && TryMigrateToAdjacentPortal(num, vector5, normalized, num3))
					{
						RecalculateThrowingIntersections(allowMigration: false);
						return;
					}
					Vector3 vector6 = travelMatrix.MultiplyPoint3x4(worldPoint);
					Vector3 normalized2 = (vector - vector6).normalized;
					value.entrancePoint = worldPoint;
					value.entranceDirection = normalized;
					value.exitPoint = vector6;
					value.exitDirection = normalized2;
					portalTraversals[num] = value;
					vector = worldPoint;
				}
			}
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
		}
	}

	private bool RecalculatePortalIntersections(bool allowMigration = true)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (portalTraversals.Count == 0)
		{
			return true;
		}
		bool flag = state == HookState.Pulling || state == HookState.Caught;
		Vector3 position = hand.position;
		Vector3 vector = hookPoint;
		for (int num = portalTraversals.Count - 1; num >= 0; num--)
		{
			PortalTraversalV2 value = portalTraversals[num];
			Portal portalObject = value.portalObject;
			PortalSide side = value.portalHandle.side;
			Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(side);
			Vector3 vector2 = travelMatrix.inverse.MultiplyPoint3x4(vector);
			NativePortalTransform nativePortalTransform = portalObject.GetTransform(side);
			Vector3 vector3 = float3.op_Implicit(nativePortalTransform.center);
			Vector3 vector4 = float3.op_Implicit(nativePortalTransform.forward);
			Vector3 vector5 = ((num == 0) ? position : portalTraversals[num - 1].exitPoint);
			Vector3 normalized = (vector2 - vector5).normalized;
			float num2 = Vector3.Dot(vector4, normalized);
			if (Mathf.Abs(num2) > 0.0001f)
			{
				float num3 = Vector3.Dot(vector3 - vector5, vector4) / num2;
				if (num3 > 0f)
				{
					Vector3 vector6 = vector5 + normalized * num3;
					bool wasClamped = false;
					Vector3 vector7 = ClampToPortalBounds(portalObject, side, vector6, out wasClamped);
					if (wasClamped && allowMigration && TryMigrateToAdjacentPortal(num, vector5, normalized, num3))
					{
						return RecalculatePortalIntersections(allowMigration: false);
					}
					if (wasClamped && flag && (num != 0 || !(Vector3.Distance(position, value.entrancePoint) < 5f)))
					{
						float num4 = Vector3.Distance(vector6, vector7);
						float boundingRadius = portalObject.GetShape().GetBoundingRadius();
						if (num4 > boundingRadius)
						{
							return false;
						}
					}
					Vector3 exitPoint = travelMatrix.MultiplyPoint3x4(vector7);
					Vector3 exitDirection = travelMatrix.rotation * normalized;
					if (wasClamped && state != HookState.Throwing && state != HookState.Pulling && !CheckPortalBendAngle(vector5, vector7, exitPoint, vector, travelMatrix))
					{
						return false;
					}
					value.entrancePoint = vector7;
					value.entranceDirection = normalized;
					value.exitPoint = exitPoint;
					value.exitDirection = exitDirection;
					portalTraversals[num] = value;
					vector = vector7;
				}
				else if (flag && num == 0 && Vector3.Distance(position, value.entrancePoint) > 5f)
				{
					return false;
				}
			}
			else if (flag && num == 0 && Vector3.Distance(position, value.entrancePoint) > 5f)
			{
				return false;
			}
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
		}
		return true;
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

	private bool CheckPortalBendAngle(Vector3 playerPos, Vector3 entrancePoint, Vector3 exitPoint, Vector3 targetPos, Matrix4x4 travelMatrix)
	{
		Vector3 normalized = (entrancePoint - playerPos).normalized;
		Vector3 normalized2 = (targetPos - exitPoint).normalized;
		Vector3 to = travelMatrix.inverse.MultiplyVector(normalized2);
		return Vector3.Angle(normalized, to) <= maxPortalBendAngle;
	}

	private bool ValidatePortalTraversals()
	{
		for (int i = 0; i < portalTraversals.Count; i++)
		{
			PortalTraversalV2 portalTraversalV = portalTraversals[i];
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject == null || !portalObject.isActiveAndEnabled)
			{
				return false;
			}
			if (portalObject.entry == null || portalObject.exit == null || !portalObject.entry.gameObject.activeInHierarchy || !portalObject.exit.gameObject.activeInHierarchy)
			{
				return false;
			}
			PortalTravellerFlags travelFlags = portalObject.GetTravelFlags(portalTraversalV.portalHandle.side);
			if (!travelFlags.HasFlag(PortalTravellerFlags.Player) && !travelFlags.HasFlag(PortalTravellerFlags.PlayerProjectile))
			{
				return false;
			}
		}
		return true;
	}

	private void Update()
	{
		if (!MonoSingleton<OptionsManager>.Instance || MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (!equipped || MonoSingleton<FistControl>.Instance.shopping || !MonoSingleton<FistControl>.Instance.activated)
		{
			if (state != HookState.Ready || returning)
			{
				Cancel();
			}
			model.SetActive(value: false);
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Hook.WasPerformedThisFrame)
		{
			if (state == HookState.Pulling)
			{
				StopThrow();
			}
			else if (cooldown <= 0f)
			{
				ResetTraversals();
				cooldown = 0.5f;
				model.SetActive(value: true);
				if (!forcingFistControl)
				{
					if ((bool)MonoSingleton<FistControl>.Instance.currentPunch)
					{
						MonoSingleton<FistControl>.Instance.currentPunch.CancelAttack();
					}
					MonoSingleton<FistControl>.Instance.forceNoHold++;
					forcingFistControl = true;
					MonoSingleton<FistControl>.Instance.transform.localRotation = Quaternion.identity;
					if (MonoSingleton<FistControl>.Instance.fistCooldown > 0.1f)
					{
						MonoSingleton<FistControl>.Instance.fistCooldown = 0.1f;
					}
				}
				lr.enabled = true;
				hookPoint = base.transform.position;
				previousHookPoint = hookPoint;
				if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
				{
					throwDirection = (targeter.CurrentTarget.bounds.center - base.transform.position).normalized;
				}
				else
				{
					throwDirection = base.transform.forward;
				}
				returning = false;
				if (caughtObjects.Count > 0)
				{
					foreach (Rigidbody caughtObject in caughtObjects)
					{
						if ((bool)caughtObject)
						{
							caughtObject.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - caughtObject.transform.position).normalized * (100f + returnDistance / 2f);
						}
					}
					caughtObjects.Clear();
				}
				state = HookState.Throwing;
				lightTarget = false;
				throwWarp = 1f;
				anim.Play("Throw", -1, 0f);
				inspectLr.enabled = false;
				hand.transform.localPosition = new Vector3(0.09f, -0.051f, 0.045f);
				if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
				{
					hand.transform.localPosition += new Vector3(0.225f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), -0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f));
				}
				caughtPoint = Vector3.zero;
				caughtTransform = null;
				caughtCollider = null;
				caughtEid = null;
				UnityEngine.Object.Instantiate(throwSound);
				aud.clip = throwLoop;
				aud.panStereo = 0f;
				aud.Play(tracked: true);
				aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
				semiBlocked = 0f;
				MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.WhiplashThrow, model);
			}
		}
		if (cooldown != 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		if (lr.enabled || (bool)lineRendererPortalHelper)
		{
			UpdateLineRendererPositions();
		}
		if (state == HookState.Pulling && !lightTarget && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
		{
			if (MonoSingleton<NewMovement>.Instance.rb.velocity.y < 1f)
			{
				MonoSingleton<NewMovement>.Instance.rb.velocity = new Vector3(MonoSingleton<NewMovement>.Instance.rb.velocity.x, 1f, MonoSingleton<NewMovement>.Instance.rb.velocity.z);
			}
			MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.ClampMagnitude(MonoSingleton<NewMovement>.Instance.rb.velocity, 30f);
			if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround && !Physics.Raycast(MonoSingleton<NewMovement>.Instance.gc.transform.position, Vector3.down, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				MonoSingleton<NewMovement>.Instance.rb.AddForce(Vector3.up * 15f, ForceMode.VelocityChange);
			}
			else if (!MonoSingleton<NewMovement>.Instance.jumping)
			{
				MonoSingleton<NewMovement>.Instance.Jump();
			}
			StopThrow(1f);
		}
		if (!MonoSingleton<FistControl>.Instance.currentPunch || !MonoSingleton<FistControl>.Instance.currentPunch.holding || !forcingFistControl)
		{
			return;
		}
		MonoSingleton<FistControl>.Instance.currentPunch.heldItem.transform.position = hook.position + hook.up * 0.2f;
		if (state != HookState.Ready || returning)
		{
			MonoSingleton<FistControl>.Instance.heldObject.hooked = true;
			if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 22)
			{
				Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.layer = 22;
				}
			}
			return;
		}
		MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
		if (MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer != 13)
		{
			Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = 13;
			}
		}
	}

	private void LateUpdate()
	{
		if (state != HookState.Ready || returning)
		{
			hook.position = hookPoint;
			hook.up = throwDirection;
			hookModel.layer = 2;
		}
		else
		{
			hookModel.layer = 13;
		}
	}

	private void TraversePortal(PortalTraversalV2 traversal)
	{
		hookPoint = traversal.exitPoint;
		throwDirection = traversal.exitDirection;
		if (lineRendererPortalHelper == null)
		{
			lineRendererPortalHelper = LineRendererPortalHelper.GetOrCreateHelper(lr);
			lineRendererPortalHelper.useIntermediatePositionsAsOffsets = true;
		}
		lineRendererPortalHelper.UpdateTraversals(portalTraversals);
	}

	private void ResetTraversals()
	{
		portalTraversals.Clear();
		portalTraversalGraceFrames = 0;
		if (lineRendererPortalHelper != null)
		{
			UnityEngine.Object.Destroy(lineRendererPortalHelper);
			lineRendererPortalHelper = null;
		}
	}

	private float GetTotalLineDistance()
	{
		if (portalTraversals.Count <= 0)
		{
			return Vector3.Distance(hand.position, hookPoint);
		}
		float num = 0f;
		Vector3 a = hand.position;
		foreach (PortalTraversalV2 portalTraversal in portalTraversals)
		{
			num += Vector3.Distance(a, portalTraversal.entrancePoint);
			a = portalTraversal.exitPoint;
		}
		return num + Vector3.Distance(a, hookPoint);
	}

	private void FixedUpdate()
	{
		//IL_159e: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_22c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e73: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e99: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)caughtGrenade && caughtGrenade.playerRiding)
		{
			if (caughtObjects.Contains(caughtGrenade.rb))
			{
				caughtGrenade.hooked = false;
				caughtObjects.Remove(caughtGrenade.rb);
			}
			else
			{
				caughtObjects.Clear();
			}
			caughtGrenade = null;
		}
		if (portalTraversalGraceFrames > 0)
		{
			portalTraversalGraceFrames--;
		}
		if ((state != HookState.Ready || returning) && state == HookState.Ready && returning)
		{
			Vector3 vector = hookPoint;
			if (portalTraversals.Count > 0)
			{
				if (!ValidatePortalTraversals())
				{
					StopThrow();
					return;
				}
				RecalculateThrowingIntersections();
			}
			float num = Time.fixedDeltaTime * (100f + returnDistance / 2f);
			Vector3 vector2 = MonoSingleton<CameraController>.Instance.transform.position + MonoSingleton<CameraController>.Instance.transform.forward * 1.5f;
			while (num > 0f)
			{
				if (portalTraversals.Count > 0)
				{
					List<PortalTraversalV2> list = portalTraversals;
					vector2 = list[list.Count - 1].exitPoint;
				}
				float num2 = Vector3.Distance(hookPoint, vector2);
				if (num2 <= num && portalTraversals.Count > 0)
				{
					num -= num2;
					List<PortalTraversalV2> list2 = portalTraversals;
					PortalTraversalV2 portalTraversalV = list2[list2.Count - 1];
					hookPoint = portalTraversalV.entrancePoint;
					throwDirection = portalTraversalV.entranceDirection;
					portalTraversals.RemoveAt(portalTraversals.Count - 1);
					if (lineRendererPortalHelper != null)
					{
						lineRendererPortalHelper.UpdateTraversals(portalTraversals);
					}
					if (portalTraversals.Count > 0)
					{
						RecalculateThrowingIntersections();
					}
				}
				else
				{
					hookPoint = Vector3.MoveTowards(hookPoint, vector2, num);
					num = 0f;
				}
			}
			for (int num3 = caughtObjects.Count - 1; num3 >= 0; num3--)
			{
				if (caughtObjects[num3] != null)
				{
					caughtObjects[num3].position = hookPoint;
				}
				else
				{
					caughtObjects.RemoveAt(num3);
				}
			}
			if (hookPoint == vector2 && portalTraversals.Count == 0)
			{
				lr.enabled = false;
				returning = false;
				anim.Play("Catch", -1, 0f);
				UnityEngine.Object.Instantiate(catchSound);
				aud.Stop();
				ResetTraversals();
				if (caughtObjects.Count > 0)
				{
					for (int num4 = caughtObjects.Count - 1; num4 >= 0; num4--)
					{
						if (caughtObjects[num4] != null)
						{
							if (caughtObjects[num4].TryGetComponent<Grenade>(out var component) && component.rocket)
							{
								NewMovement newMovement = MonoSingleton<NewMovement>.Instance;
								Vector3 position = newMovement.transform.position;
								component.transform.position = position;
								component.hooked = false;
								component.ignoreEnemyType.Clear();
								if (!newMovement.ridingRocket && (Vector3.Angle(newMovement.rb.GetGravityDirection(), vector - position) < 45f || MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed))
								{
									component.PlayerRideStart();
								}
								else
								{
									component.Explode(big: false, harmless: false, component.rocket && !newMovement.gc.onGround, 1f, ultrabooster: false, null, fup: true);
								}
							}
							Rigidbody rb = MonoSingleton<NewMovement>.Instance.rb;
							Vector3 gravityDirection = rb.GetGravityDirection();
							Vector3 vector3 = rb.velocity;
							if (Vector3.Dot(vector3, gravityDirection) > 0f)
							{
								vector3 = Vector3.ProjectOnPlane(vector3, gravityDirection);
							}
							caughtObjects[num4].velocity = vector3;
						}
					}
					caughtObjects.Clear();
				}
				caughtGrenade = null;
				caughtCannonball = null;
			}
		}
		Vector3 vector4 = -MonoSingleton<NewMovement>.Instance.rb.GetGravityDirection();
		if (state == HookState.Throwing)
		{
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed && (cooldown <= 0.1f || caughtObjects.Count > 0))
			{
				StopThrow();
			}
			else
			{
				if (portalTraversals.Count > 0 && !ValidatePortalTraversals())
				{
					StopThrow();
					return;
				}
				float num5 = 250f * Time.fixedDeltaTime;
				bool flag = false;
				if (PortalPhysicsV2.Raycast(hookPoint, throwDirection, num5, enviroMask, out var hitInfo, out var array, out var _, QueryTriggerInteraction.Ignore))
				{
					flag = true;
					num5 = hitInfo.distance;
				}
				bool flag2 = false;
				if (!flag && array.Length != 0)
				{
					PortalTraversalV2 portalTraversalV2 = array[0];
					portalTraversals.Add(portalTraversalV2);
					TraversePortal(portalTraversalV2);
					flag2 = true;
				}
				RaycastHit[] array2 = ((!flag2) ? Physics.SphereCastAll(hookPoint, Mathf.Min(Vector3.Distance(base.transform.position, hookPoint) / 15f, 5f), throwDirection, num5, throwMask, QueryTriggerInteraction.Collide) : Array.Empty<RaycastHit>());
				Array.Sort(array2, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
				bool flag3 = false;
				for (int num6 = 0; num6 < array2.Length; num6++)
				{
					RaycastHit rhit = array2[num6];
					bool flag4 = false;
					switch (rhit.transform.gameObject.layer)
					{
					case 26:
						if (!rhit.collider.isTrigger)
						{
							StopThrow();
							UnityEngine.Object.Instantiate(clinkSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
							flag3 = true;
							flag4 = true;
						}
						goto default;
					case 14:
						if (caughtObjects.Count < 5 && MonoSingleton<ObjectTracker>.Instance.HasTransform(rhit.transform) && rhit.collider.attachedRigidbody != null && !caughtObjects.Contains(rhit.collider.attachedRigidbody))
						{
							if (rhit.transform.TryGetComponent<ParryHelper>(out var component5))
							{
								_ = component5.target;
							}
							if (caughtGrenade == null && MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0 && (bool)MonoSingleton<ObjectTracker>.Instance.GetGrenade(rhit.transform) && !MonoSingleton<ObjectTracker>.Instance.GetGrenade(rhit.transform).playerRiding)
							{
								caughtObjects.Add(rhit.collider.attachedRigidbody);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								caughtGrenade = MonoSingleton<ObjectTracker>.Instance.GetGrenade(rhit.transform);
								caughtGrenade.CanCollideWithPlayer(can: false);
								caughtGrenade.rideable = true;
								caughtGrenade.hooked = true;
								caughtGrenade.ignoreEnemyType.Clear();
							}
							else if (MonoSingleton<ObjectTracker>.Instance.cannonballList.Count > 0 && (bool)MonoSingleton<ObjectTracker>.Instance.GetCannonball(rhit.transform) && MonoSingleton<ObjectTracker>.Instance.GetCannonball(rhit.transform).physicsCannonball)
							{
								Cannonball cannonball = MonoSingleton<ObjectTracker>.Instance.GetCannonball(rhit.transform);
								caughtObjects.Add(rhit.collider.attachedRigidbody);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								caughtCannonball = cannonball;
								cannonball.Unlaunch();
								cannonball.forceMaxSpeed = true;
								cannonball.InstaBreakDefenceCancel();
							}
						}
						goto default;
					case 16:
					{
						if (rhit.collider.isTrigger && rhit.transform.TryGetComponent<BulletCheck>(out var component7))
						{
							component7.ForceDodge();
						}
						flag4 = true;
						goto default;
					}
					case 11:
					case 12:
					{
						if (Physics.Raycast(hookPoint, rhit.collider.bounds.center - hookPoint, Vector3.Distance(hookPoint, rhit.collider.bounds.center), enviroMask, QueryTriggerInteraction.Ignore))
						{
							continue;
						}
						caughtEid = rhit.transform.GetComponentInParent<EnemyIdentifier>();
						if ((bool)caughtEid && caughtEid.enemyType == EnemyType.Providence)
						{
							Drone drone = (caughtEid.drone ? caughtEid.drone : caughtEid.GetComponent<Drone>());
							if ((bool)drone && !drone.CanBeHooked())
							{
								caughtEid.drone.RandomDodge(force: true);
								caughtEid.drone.DodgeLaugh();
								caughtEid = null;
								continue;
							}
						}
						if ((bool)caughtEid && (caughtEid.enemyType == EnemyType.MaliciousFace || caughtEid.enemyType == EnemyType.Gutterman || caughtEid.enemyType == EnemyType.HideousMass) && caughtEid.dead && !rhit.collider.Raycast(new Ray(hookPoint, throwDirection), out var _, num5))
						{
							caughtEid = null;
							continue;
						}
						if (caughtEid == null && rhit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component6))
						{
							caughtEid = component6.eid;
						}
						if ((bool)caughtEid)
						{
							if (caughtEid.hookIgnore)
							{
								caughtEid = null;
								goto default;
							}
							if ((bool)caughtCannonball && caughtCannonball.hitEnemies.Contains(caughtEid))
							{
								caughtEid = null;
								flag3 = true;
								StopThrow();
								return;
							}
							if (caughtEid.blessed)
							{
								caughtEid.hitter = "hook";
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 1f, tryForExplode: false);
								caughtEid = null;
								continue;
							}
							if (caughtEid.enemyType == EnemyType.Idol || caughtEid.enemyType == EnemyType.Deathcatcher)
							{
								caughtEid.hitter = "hook";
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 1f, tryForExplode: false);
								UnityEngine.Object.Instantiate(clinkObjectSparks, rhit.point, Quaternion.LookRotation(rhit.normal));
								continue;
							}
							caughtEid.hitter = "hook";
							caughtEid.hooked = true;
							if (((caughtEid.enemyType != EnemyType.Drone && caughtEid.enemyType != EnemyType.Virtue && caughtEid.enemyType != EnemyType.Providence) || !caughtEid.dead) && caughtEid.enemyType != EnemyType.Stalker)
							{
								caughtEid.DeliverDamage(caughtEid.gameObject, Vector3.zero, rhit.point, 0.2f, tryForExplode: false);
							}
							if (caughtEid == null)
							{
								return;
							}
							if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
							{
								GameObject gameObject = rhit.transform.gameObject;
								if (rhit.transform.gameObject.layer == 12)
								{
									EnemyIdentifierIdentifier componentInChildren = gameObject.GetComponentInChildren<EnemyIdentifierIdentifier>();
									if ((bool)componentInChildren)
									{
										gameObject = componentInChildren.gameObject;
									}
								}
								MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", gameObject, SendMessageOptions.DontRequireReceiver);
								if (MonoSingleton<FistControl>.Instance.heldObject.dropOnHit)
								{
									MonoSingleton<FistControl>.Instance.currentPunch.ForceDrop();
								}
							}
							if (caughtEid.dead)
							{
								if (!deadIgnoreTypes.Contains(caughtEid.enemyType))
								{
									goto default;
								}
								if (caughtEid.enemyType == EnemyType.Virtue || lightEnemies.Contains(caughtEid.enemyType))
								{
									lightTarget = true;
								}
								caughtEid = null;
							}
							else if (lightEnemies.Contains(caughtEid.enemyType))
							{
								lightTarget = true;
							}
						}
						flag3 = true;
						flag4 = true;
						caughtTransform = rhit.transform;
						hookPoint = rhit.collider.bounds.center;
						caughtPoint = hookPoint - caughtTransform.position;
						caughtDir = (caughtEid ? caughtEid.transform.up : caughtTransform.up);
						state = HookState.Caught;
						caughtCollider = rhit.collider;
						aud.Stop();
						UnityEngine.Object.Instantiate(hitSound, rhit.point, Quaternion.identity);
						goto default;
					}
					case 10:
					{
						if (rhit.transform.gameObject.CompareTag("Coin") && rhit.transform.TryGetComponent<Coin>(out var component4))
						{
							rhit.transform.position = hookPoint + throwDirection.normalized * rhit.distance;
							component4.Bounce();
						}
						goto default;
					}
					case 22:
					{
						if (rhit.transform.TryGetComponent<HookPoint>(out var component2))
						{
							if (component2.active && Vector3.Distance(base.transform.position, rhit.transform.position) > 5f)
							{
								flag3 = true;
								flag4 = true;
								caughtTransform = rhit.transform;
								hookPoint = rhit.transform.position;
								caughtPoint = Vector3.zero;
								state = HookState.Caught;
								caughtCollider = rhit.collider;
								aud.Stop();
								caughtHook = component2;
								component2.Hooked();
								goto default;
							}
						}
						else if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && !MonoSingleton<FistControl>.Instance.currentPunch.holding)
						{
							if (rhit.transform.TryGetComponent<ItemIdentifier>(out var component3))
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								if (component3.infiniteSource)
								{
									component3 = component3.CreateCopy();
								}
								flag3 = true;
								if (component3.ipz == null || (component3.ipz.CheckDoorBounds(component3.transform.position, previousHookPoint, reverseBounds: false) && component3.ipz.CheckDoorBounds(component3.transform.position, base.transform.position, reverseBounds: false)))
								{
									MonoSingleton<FistControl>.Instance.currentPunch.ForceHold(component3);
								}
								else
								{
									ItemGrabError(rhit);
								}
								previousHookPoint = hookPoint;
							}
						}
						else
						{
							ItemPlaceZone[] components = rhit.transform.GetComponents<ItemPlaceZone>();
							bool flag5 = false;
							ItemPlaceZone[] array3 = components;
							foreach (ItemPlaceZone itemPlaceZone in array3)
							{
								if (itemPlaceZone.acceptedItemType == MonoSingleton<FistControl>.Instance.heldObject.itemType && !itemPlaceZone.CheckDoorBounds(itemPlaceZone.transform.position, previousHookPoint, reverseBounds: true) && !itemPlaceZone.CheckDoorBounds(itemPlaceZone.transform.position, base.transform.position, reverseBounds: true))
								{
									flag5 = true;
								}
							}
							if (components.Length != 0)
							{
								if (Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
								{
									continue;
								}
								flag3 = true;
								if (!flag5)
								{
									MonoSingleton<FistControl>.Instance.currentPunch.PlaceHeldObject(components, rhit.transform);
								}
								else
								{
									ItemGrabError(rhit);
								}
								previousHookPoint = hookPoint;
							}
						}
						if (flag3)
						{
							flag4 = true;
							StopThrow();
						}
						else if (!Physics.Raycast(hookPoint, rhit.transform.position - hookPoint, Vector3.Distance(hookPoint, rhit.transform.position), enviroMask, QueryTriggerInteraction.Ignore))
						{
							flag4 = true;
						}
						goto default;
					}
					default:
						if (flag4 && (bool)MonoSingleton<FistControl>.Instance.heldObject)
						{
							MonoSingleton<FistControl>.Instance.heldObject.SendMessage("HitWith", rhit.transform.gameObject, SendMessageOptions.DontRequireReceiver);
						}
						if (!flag3)
						{
							continue;
						}
						break;
					}
					break;
				}
				Vector3 point = hookPoint;
				if (flag && !flag3)
				{
					if (hitInfo.transform.TryGetComponent<Breakable>(out var component8) && component8.weak && !component8.precisionOnly && !component8.specialCaseOnly)
					{
						component8.Break(0.2f);
					}
					if (hitInfo.transform.gameObject.TryGetComponent<SandboxProp>(out var component9) && (bool)component9.rigidbody)
					{
						component9.rigidbody.AddForceAtPosition(currentForward * -100f, hitInfo.point, ForceMode.VelocityChange);
					}
					else
					{
						UnityEngine.Object.Instantiate(clinkSparks, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
					}
					point = hitInfo.point;
					StopThrow();
					flag3 = true;
				}
				if (!flag3 && GetTotalLineDistance() > 300f)
				{
					StopThrow();
				}
				else if (!flag3)
				{
					hookPoint += throwDirection * num5;
					point = hookPoint;
					if (portalTraversals.Count > 0)
					{
						RecalculateThrowingIntersections();
					}
				}
				for (int num8 = caughtObjects.Count - 1; num8 >= 0; num8--)
				{
					if (caughtObjects[num8] != null)
					{
						caughtObjects[num8].position = point;
						if (flag3)
						{
							if (caughtObjects[num8].TryGetComponent<Grenade>(out var component10))
							{
								if ((bool)caughtEid && (bool)component10.originEnemy && caughtEid == component10.originEnemy)
								{
									MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.rocketreturn", null, caughtEid);
								}
								component10.hooked = false;
								component10.ignoreEnemyType.Clear();
								component10.Explode();
							}
							else
							{
								caughtObjects.RemoveAt(num8);
							}
						}
					}
					else
					{
						caughtObjects.RemoveAt(num8);
					}
				}
			}
		}
		else if (state == HookState.Caught)
		{
			if (!caughtTransform)
			{
				StopThrow();
				return;
			}
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow();
					return;
				}
				SolveDeadIgnore();
			}
			else
			{
				Vector3 nextCaughtPoint = GetNextCaughtPoint();
				if (!caughtTransform || (portalTraversals.Count == 0 && portalTraversalGraceFrames <= 0 && Physics.Raycast(hand.position, nextCaughtPoint - hand.position, out var _, Vector3.Distance(hand.position, nextCaughtPoint), enviroMask, QueryTriggerInteraction.Ignore)))
				{
					StopThrow();
					return;
				}
			}
			hookPoint = caughtTransform.position + caughtPoint;
			if (portalTraversals.Count > 0)
			{
				if (!ValidatePortalTraversals())
				{
					StopThrow();
					return;
				}
				if (!RecalculatePortalIntersections())
				{
					StopThrow();
					return;
				}
				for (int num9 = portalTraversals.Count - 1; num9 >= 0; num9--)
				{
					PortalTraversalV2 portalTraversalV3 = portalTraversals[num9];
					Portal portalObject = portalTraversalV3.portalObject;
					Vector3 a = float3.op_Implicit(portalObject.GetTransform(portalTraversalV3.portalHandle.side).center);
					Vector3 b = float3.op_Implicit(portalObject.GetTransform(portalTraversalV3.portalHandle.side.Reverse()).center);
					if (Vector3.Distance(a, b) < 1f)
					{
						portalTraversals.RemoveAt(num9);
					}
				}
				if (portalTraversals.Count == 0 && lineRendererPortalHelper != null)
				{
					lineRendererPortalHelper.UpdateTraversals(portalTraversals);
				}
			}
			if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed)
			{
				anim.Play("Pull", -1, 0f);
				hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
				state = HookState.Pulling;
				MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.WhiplashPull, model);
				currentWoosh = UnityEngine.Object.Instantiate(wooshSound);
				UnityEngine.Object.Instantiate(pullSound);
				aud.clip = pullLoop;
				aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
				aud.panStereo = -0.5f;
				aud.Play(tracked: true);
				if ((bool)caughtHook && caughtHook.type == hookPointType.Switch)
				{
					caughtHook.SwitchPulled();
					if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround)
					{
						if (MonoSingleton<UnderwaterController>.Instance.inWater)
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
						}
						else
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = vector4 * 15f;
						}
					}
					StopThrow();
				}
				else if (!forcingGroundCheck && !lightTarget)
				{
					ForceGroundCheck();
				}
				else if (lightTarget)
				{
					Rigidbody component11;
					if ((bool)caughtEid)
					{
						if (enemyGroundCheck != null)
						{
							enemyGroundCheck.StopForceOff();
						}
						enemyGroundCheck = caughtEid.gce;
						if ((bool)enemyGroundCheck)
						{
							enemyGroundCheck.ForceOff();
						}
						enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
					}
					else if (caughtTransform.TryGetComponent<Rigidbody>(out component11))
					{
						enemyRigidbody = component11;
					}
					else
					{
						StopThrow();
					}
					if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround)
					{
						if (!MonoSingleton<UnderwaterController>.Instance.inWater)
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = vector4 * 15f;
						}
						else
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = vector4 * 5f;
						}
					}
				}
			}
		}
		if (state == HookState.Pulling)
		{
			if (!caughtTransform || !caughtCollider)
			{
				StopThrow(1f);
				return;
			}
			Vector3 nextCaughtPoint = GetNextCaughtPoint();
			Vector3 vector5 = nextCaughtPoint - base.transform.position;
			if (portalTraversals.Count == 0 && portalTraversalGraceFrames <= 0 && Physics.Raycast(base.transform.position, vector5.normalized, out var hitInfo4, vector5.magnitude, enviroMask, QueryTriggerInteraction.Ignore))
			{
				bool flag6 = true;
				EnemyIdentifier component12 = hitInfo4.transform.GetComponent<EnemyIdentifier>();
				if ((bool)component12 && component12.blessed)
				{
					flag6 = false;
				}
				if (flag6)
				{
					StopThrow(1f);
					return;
				}
			}
			if (caughtEid != null && (caughtEid.dead || caughtEid.hookIgnore || caughtEid.blessed))
			{
				if (!caughtEid.dead || !deadIgnoreTypes.Contains(caughtEid.enemyType))
				{
					StopThrow(1f);
					return;
				}
				SolveDeadIgnore();
			}
			if ((bool)caughtEid && !MonoSingleton<UnderwaterController>.Instance.inWater && (!MonoSingleton<AssistController>.Instance || !MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.disableWhiplashHardDamage))
			{
				if (MonoSingleton<NewMovement>.Instance.antiHp + Time.fixedDeltaTime * 66f <= 50f)
				{
					MonoSingleton<NewMovement>.Instance.ForceAddAntiHP(Time.fixedDeltaTime * 66f, silent: true, dontOverwriteHp: true);
				}
				else if (MonoSingleton<NewMovement>.Instance.antiHp <= 50f)
				{
					MonoSingleton<NewMovement>.Instance.ForceAntiHP(50f, silent: true, dontOverwriteHp: true);
				}
			}
			Vector3 vector6 = playerCollider.ClosestPoint(hookPoint);
			Collider collider = caughtCollider;
			if (Physics.Raycast(base.transform.position, caughtCollider.bounds.center - base.transform.position, out var hitInfo5, Vector3.Distance(caughtCollider.bounds.center, base.transform.position), enemyMask))
			{
				collider = hitInfo5.collider;
			}
			float num10 = Vector3.Distance(vector6, collider.ClosestPoint(vector6));
			if (portalTraversals.Count == 0 && (num10 < 0.25f || (!lightTarget && Vector3.Distance(vector6 + MonoSingleton<NewMovement>.Instance.rb.velocity * Time.fixedDeltaTime, collider.ClosestPoint(vector6)) < 0.25f)))
			{
				if (!lightTarget && Vector3.Distance(vector6, collider.ClosestPoint(vector6)) >= 0.25f)
				{
					MonoSingleton<NewMovement>.Instance.rb.MovePosition(collider.ClosestPoint(vector6) - MonoSingleton<NewMovement>.Instance.rb.velocity.normalized * 0.25f);
				}
				if ((bool)enemyRigidbody)
				{
					if (enemyGroundCheck == null || enemyGroundCheck.touchingGround || ((bool)caughtEid && caughtEid.underwater) || caughtEid.enemyType == EnemyType.Mannequin)
					{
						enemyRigidbody.velocity = Vector3.zero;
					}
					else
					{
						enemyRigidbody.velocity = vector4 * 15f;
					}
				}
				bool flag7 = false;
				if ((bool)caughtHook)
				{
					if (caughtHook.type == hookPointType.Slingshot)
					{
						flag7 = true;
						if (caughtHook.slingShotForce != 0f)
						{
							MonoSingleton<NewMovement>.Instance.rb.velocity = caughtHook.slingShotForce * MonoSingleton<NewMovement>.Instance.rb.velocity.normalized;
						}
					}
					caughtHook.Reached(MonoSingleton<NewMovement>.Instance.rb.velocity.normalized);
				}
				StopThrow(1f);
				float num11 = Vector3.Dot(hookPoint - base.transform.position, vector4);
				if (!MonoSingleton<NewMovement>.Instance.gc.touchingGround && !flag7)
				{
					if (MonoSingleton<UnderwaterController>.Instance.inWater)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
					}
					else if (base.transform.position.y < hookPoint.y)
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = vector4 * (15f + num11);
					}
					else
					{
						MonoSingleton<NewMovement>.Instance.rb.velocity = vector4 * 15f;
					}
				}
				return;
			}
			if ((bool)caughtEid && (bool)enemyRigidbody && caughtEid.enemyType == EnemyType.Drone)
			{
				if (enemyRigidbody.isKinematic)
				{
					lightTarget = false;
				}
				else
				{
					lightTarget = true;
				}
			}
			if (lightTarget && forcingGroundCheck)
			{
				StopForceGroundCheck();
			}
			else if (!lightTarget && !forcingGroundCheck)
			{
				ForceGroundCheck();
			}
			hookPoint = caughtTransform.position + caughtPoint;
			if (portalTraversals.Count > 0)
			{
				if (!ValidatePortalTraversals())
				{
					StopThrow(1f);
					return;
				}
				if (!RecalculatePortalIntersections())
				{
					StopThrow(1f);
					return;
				}
				for (int num12 = portalTraversals.Count - 1; num12 >= 0; num12--)
				{
					PortalTraversalV2 portalTraversalV4 = portalTraversals[num12];
					Portal portalObject2 = portalTraversalV4.portalObject;
					Vector3 a2 = float3.op_Implicit(portalObject2.GetTransform(portalTraversalV4.portalHandle.side).center);
					Vector3 b2 = float3.op_Implicit(portalObject2.GetTransform(portalTraversalV4.portalHandle.side.Reverse()).center);
					if (Vector3.Distance(a2, b2) < 1f)
					{
						portalTraversals.RemoveAt(num12);
					}
				}
				if (portalTraversals.Count == 0 && lineRendererPortalHelper != null)
				{
					lineRendererPortalHelper.UpdateTraversals(portalTraversals);
				}
			}
			if (lightTarget)
			{
				if (!enemyRigidbody)
				{
					StopThrow(1f);
					return;
				}
				Vector3 vector7 = MonoSingleton<NewMovement>.Instance.transform.position;
				bool flag8 = false;
				Vector3 vector8 = MonoSingleton<CameraController>.Instance.transform.position - vector7;
				if (portalTraversals.Count > 0)
				{
					flag8 = true;
					List<PortalTraversalV2> list3 = portalTraversals;
					PortalTraversalV2 portalTraversalV5 = list3[list3.Count - 1];
					vector7 = portalTraversalV5.exitPoint;
					Vector3 vector9 = caughtCollider.ClosestPoint(vector7) - hookPoint;
					if (Vector3.Distance(hookPoint, vector7) - vector9.magnitude <= 0.3f)
					{
						Portal portalObject3 = portalTraversalV5.portalObject;
						PortalSide side = portalTraversalV5.portalHandle.side.Reverse();
						Matrix4x4 travelMatrix = portalObject3.GetTravelMatrix(side);
						Vector3 center = caughtCollider.bounds.center;
						Vector3 vector10 = enemyRigidbody.position - center;
						Vector3 position2 = travelMatrix.MultiplyPoint3x4(center) + vector10;
						enemyRigidbody.transform.position = position2;
						enemyRigidbody.position = position2;
						if (enemyRigidbody.TryGetComponent<CustomGravity>(out var component13))
						{
							component13.gravity = travelMatrix.MultiplyVector(component13.gravity);
						}
						hookPoint = caughtTransform.position + caughtPoint;
						throwDirection = portalTraversalV5.entranceDirection;
						portalTraversals.RemoveAt(portalTraversals.Count - 1);
						if (lineRendererPortalHelper != null)
						{
							lineRendererPortalHelper.UpdateTraversals(portalTraversals);
						}
						IPortalTraveller componentInParent = caughtTransform.GetComponentInParent<IPortalTraveller>();
						if (componentInParent != null && MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 portalManagerV))
						{
							portalManagerV.UpdateTraveller(componentInParent);
						}
						if (portalTraversals.Count > 0)
						{
							List<PortalTraversalV2> list4 = portalTraversals;
							vector7 = list4[list4.Count - 1].exitPoint;
						}
						else
						{
							vector7 = MonoSingleton<NewMovement>.Instance.transform.position;
							flag8 = false;
						}
					}
				}
				Vector3 vector11 = vector7;
				if (!flag8)
				{
					vector11 += vector8;
				}
				if (enemyGroundCheck != null)
				{
					enemyRigidbody.velocity = (vector7 - hookPoint).normalized * 60f;
					caughtEid.transform.LookAt(new Vector3(vector11.x, caughtEid.transform.position.y, vector11.z), caughtDir);
					return;
				}
				enemyRigidbody.velocity = (vector11 - hookPoint).normalized * 60f;
				if ((bool)caughtEid)
				{
					caughtEid.transform.LookAt(vector11, caughtDir);
				}
				else
				{
					caughtTransform.LookAt(vector11, caughtDir);
				}
				return;
			}
			hookPoint = caughtTransform.position + caughtPoint;
			beingPulled = true;
			if (!MonoSingleton<NewMovement>.Instance.boost || MonoSingleton<NewMovement>.Instance.sliding)
			{
				nextCaughtPoint = hookPoint;
				List<PortalTraversalV2> list5 = portalTraversals;
				if (list5 != null && list5.Count > 0)
				{
					PortalTraversalV2 portalTraversalV6 = portalTraversals[0];
					Portal portalObject4 = portalTraversalV6.portalObject;
					PortalSide side2 = portalTraversalV6.portalHandle.side;
					NativePortalTransform nativePortalTransform = portalObject4.GetTransform(side2);
					nextCaughtPoint = portalTraversalV6.entrancePoint;
					Vector3 lhs = hand.position - MonoSingleton<NewMovement>.Instance.transform.position;
					Vector3 vector12 = float3.op_Implicit(nativePortalTransform.right);
					Vector3 vector13 = float3.op_Implicit(nativePortalTransform.up);
					Vector3 vector14 = Vector3.Dot(lhs, vector12) * vector12 + Vector3.Dot(lhs, vector13) * vector13;
					nextCaughtPoint -= vector14;
					PlaneShape shape = portalObject4.GetShape();
					float num13 = (playerCollider ? (playerCollider.radius + 0.1f) : 0.6f);
					Vector3 point2 = nativePortalTransform.toLocalManaged.MultiplyPoint3x4(nextCaughtPoint);
					float num14 = shape.width / 2f;
					float num15 = shape.height / 2f;
					point2.x = Mathf.Clamp(point2.x, 0f - num14 + num13, num14 - num13);
					point2.y = Mathf.Clamp(point2.y, 0f - num15 + num13, num15 - num13);
					point2.z = 0f;
					nextCaughtPoint = nativePortalTransform.toWorldManaged.MultiplyPoint3x4(point2);
					nextCaughtPoint += portalTraversalV6.entranceDirection * 1f;
				}
				MonoSingleton<NewMovement>.Instance.rb.velocity = (nextCaughtPoint - MonoSingleton<NewMovement>.Instance.transform.position).normalized * 60f;
			}
		}
		else
		{
			beingPulled = false;
		}
	}

	private void SolveDeadIgnore()
	{
		if (!caughtEid)
		{
			return;
		}
		switch (caughtEid.enemyType)
		{
		case EnemyType.Virtue:
			lightTarget = true;
			enemyRigidbody = caughtEid.GetComponent<Rigidbody>();
			break;
		case EnemyType.MaliciousFace:
		{
			EnemyIdentifierIdentifier[] componentsInChildren = caughtEid.GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (componentsInChildren.Length == 0)
			{
				break;
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.layer == 11)
				{
					caughtTransform = componentsInChildren[i].transform;
					break;
				}
			}
			break;
		}
		}
		caughtEid = null;
	}

	private void ItemGrabError(RaycastHit rhit)
	{
		UnityEngine.Object.Instantiate(errorSound);
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>ERROR: BLOCKING DOOR WOULD CLOSE</color>", "", "", 0, silent: true);
	}

	public void StopThrow(float animationTime = 0f, bool sparks = false)
	{
		MonoSingleton<RumbleManager>.Instance.StopVibration(RumbleProperties.WhiplashThrow);
		MonoSingleton<RumbleManager>.Instance.StopVibration(RumbleProperties.WhiplashPull);
		if (animationTime == 0f)
		{
			UnityEngine.Object.Instantiate(pullSound);
			aud.clip = pullLoop;
			aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
			aud.panStereo = -0.5f;
			aud.Play(tracked: true);
		}
		else
		{
			UnityEngine.Object.Instantiate(pullDoneSound);
		}
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		if (sparks)
		{
			UnityEngine.Object.Instantiate(clinkSparks, hookPoint, Quaternion.LookRotation(base.transform.position - hookPoint));
		}
		state = HookState.Ready;
		anim.Play("Pull", -1, animationTime);
		hand.transform.localPosition = new Vector3(-0.015f, 0.071f, 0.04f);
		if (MonoSingleton<CameraController>.Instance.defaultFov > 105f)
		{
			hand.transform.localPosition += new Vector3(0.25f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 55f), 0f, 0.05f * ((MonoSingleton<CameraController>.Instance.defaultFov - 105f) / 60f));
		}
		else if (MonoSingleton<CameraController>.Instance.defaultFov < 105f)
		{
			hand.transform.localPosition -= new Vector3(0.05f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.075f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f), 0.125f * ((105f - MonoSingleton<CameraController>.Instance.defaultFov) / 60f));
		}
		returnDistance = Mathf.Max(Vector3.Distance(base.transform.position, hookPoint), 25f);
		returning = true;
		throwWarp = 0f;
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
	}

	public void Cancel()
	{
		MonoSingleton<RumbleManager>.Instance.StopVibration(RumbleProperties.WhiplashThrow);
		MonoSingleton<RumbleManager>.Instance.StopVibration(RumbleProperties.WhiplashPull);
		if (forcingGroundCheck)
		{
			StopForceGroundCheck();
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.gameObject.layer = 13;
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if (caughtObjects.Count > 0)
		{
			foreach (Rigidbody caughtObject in caughtObjects)
			{
				if ((bool)caughtObject)
				{
					caughtObject.velocity = (MonoSingleton<NewMovement>.Instance.transform.position - caughtObject.transform.position).normalized * (100f + returnDistance / 2f);
					Grenade component2;
					if (caughtObject.TryGetComponent<Cannonball>(out var component))
					{
						component.hitEnemies.Clear();
						component.forceMaxSpeed = false;
					}
					else if (caughtObject.TryGetComponent<Grenade>(out component2))
					{
						component2.hooked = false;
					}
				}
			}
			caughtObjects.Clear();
		}
		caughtGrenade = null;
		caughtCannonball = null;
		if (lightTarget)
		{
			if ((bool)enemyGroundCheck)
			{
				enemyGroundCheck.StopForceOff();
			}
			lightTarget = false;
			enemyGroundCheck = null;
			enemyRigidbody = null;
		}
		if ((bool)caughtEid)
		{
			caughtEid.hooked = false;
			caughtEid = null;
		}
		if ((bool)caughtHook)
		{
			caughtHook.Unhooked();
			caughtHook = null;
		}
		state = HookState.Ready;
		anim.Play("Idle", -1, 0f);
		returning = false;
		throwWarp = 0f;
		lr.enabled = false;
		hookPoint = hand.position;
		ResetTraversals();
		aud.Stop();
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		if ((bool)currentWoosh)
		{
			UnityEngine.Object.Destroy(currentWoosh);
		}
		model.SetActive(value: false);
	}

	public void CatchOver()
	{
		if (state != HookState.Ready || returning)
		{
			return;
		}
		if (forcingFistControl)
		{
			MonoSingleton<FistControl>.Instance.forceNoHold--;
			forcingFistControl = false;
			if ((bool)MonoSingleton<FistControl>.Instance.heldObject)
			{
				MonoSingleton<FistControl>.Instance.heldObject.hooked = false;
			}
		}
		if ((bool)MonoSingleton<FistControl>.Instance.currentPunch && MonoSingleton<FistControl>.Instance.currentPunch.holding)
		{
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
		}
		model.SetActive(value: false);
	}

	private void ForceGroundCheck()
	{
		if (MonoSingleton<NewMovement>.Instance.sliding)
		{
			MonoSingleton<NewMovement>.Instance.StopSlide();
		}
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			MonoSingleton<NewMovement>.Instance.ridingRocket.PlayerRideEnd();
		}
		forcingGroundCheck = true;
		MonoSingleton<NewMovement>.Instance.gc.ForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.ForceOff();
	}

	private void StopForceGroundCheck()
	{
		forcingGroundCheck = false;
		MonoSingleton<NewMovement>.Instance.gc.StopForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.StopForceOff();
	}

	private void SemiBlockCheck()
	{
		if (Physics.Raycast(hand.position, caughtTransform.position + caughtPoint - hand.position, out var hitInfo, Vector3.Distance(hand.position, caughtTransform.position + caughtPoint), 2048, QueryTriggerInteraction.Ignore) && hitInfo.collider.transform != caughtCollider.transform)
		{
			semiBlocked = Mathf.MoveTowards(semiBlocked, 1f, Time.fixedDeltaTime);
			if (semiBlocked >= 1f)
			{
				StopThrow();
			}
		}
		else
		{
			semiBlocked = 0f;
		}
	}
}
