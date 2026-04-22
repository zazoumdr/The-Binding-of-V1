using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

public class Turret : EnemyScript
{
	private PortalScene scene;

	private Vector3 cachedVisionPos;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	private TargetHandle lastTargetHandle;

	private VisionQuery sightQuery;

	private Vector3 delayedLastPlayerPosition;

	private Vector3 aimPos;

	private Vector3 aimDirection;

	private readonly float aimDistance = 10000f;

	private Vector3 barrelPos;

	private float barrelOffSetDist;

	private Quaternion shootRotation;

	private PortalTraversalV2[] aimTraversals = Array.Empty<PortalTraversalV2>();

	private bool isBarrelPortalBlocked;

	private bool isBarrelPortalCrossed;

	private NavMeshPath tempPath;

	public bool stationary;

	public bool quickStart;

	private Vector3 stationaryPosition;

	[HideInInspector]
	public bool lodged;

	[HideInInspector]
	public bool aiming;

	private PortalLineRenderer aimLines;

	private int numAimLine;

	private float outOfSightTimer;

	private float aimTime;

	private float maxAimTime = 5f;

	private float flashTime;

	private float nextBeepTime;

	private bool whiteLine;

	private readonly Color defaultColor = new Color(1f, 0.44f, 0.74f);

	private int shotsInARow;

	private TimeSince sinceLastBeep;

	private int difficulty = -1;

	private float cooldown = 2f;

	private float kickCooldown = 1f;

	[HideInInspector]
	public bool inAction;

	private bool bodyRotate;

	private bool bodyTrackPlayer;

	private bool bodyReset;

	private Quaternion currentBodyRotation;

	private bool wandering;

	public Color defaultLightsColor;

	public Color attackingLightsColor;

	private float lightsIntensityTarget = 1.5f;

	private float currentLightsIntensity = 1.25f;

	[Header("Defaults")]
	[SerializeField]
	private Transform torso;

	[SerializeField]
	private Transform turret;

	[SerializeField]
	private Transform barrelTip;

	[SerializeField]
	private LineRenderer aimLine;

	[SerializeField]
	private RevolverBeam beam;

	[SerializeField]
	private GameObject warningFlash;

	[SerializeField]
	private ParticleSystem antennaFlash;

	[SerializeField]
	private Light antennaLight;

	[SerializeField]
	private AudioSource antennaSound;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Enemy mach;

	[SerializeField]
	private EnemyIdentifier eid;

	[SerializeField]
	private GameObject head;

	[SerializeField]
	private NavMeshAgent nma;

	public GameObject antenna;

	public List<Transform> interruptables = new List<Transform>();

	[SerializeField]
	private AudioSource interruptSound;

	[SerializeField]
	private AudioSource cancelSound;

	[SerializeField]
	private AudioSource footStep;

	[SerializeField]
	private AudioSource extendSound;

	[SerializeField]
	private AudioSource thunkSound;

	[SerializeField]
	private AudioSource kickWarningSound;

	[SerializeField]
	private AudioSource aimWarningSound;

	private AudioSource currentSound;

	[SerializeField]
	private GameObject rubble;

	[SerializeField]
	private GameObject rubbleLeft;

	[SerializeField]
	private GameObject rubbleRight;

	private bool leftLodged;

	private bool rightLodged;

	[SerializeField]
	private SkinnedMeshRenderer smr;

	[SerializeField]
	private GameObject unparryableFlash;

	[SerializeField]
	private SwingCheck2 sc;

	[SerializeField]
	private TrailRenderer tr;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	public override Vector3 VisionSourcePosition => cachedVisionPos;

	private bool hasVision => targetHandle != null;

	private bool isAimFlashing => flashTime > 0f;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	public override bool ShouldKnockback(ref DamageData data)
	{
		if (lodged)
		{
			switch (data.hitter)
			{
			case "heavypunch":
			case "railcannon":
			case "cannonball":
			case "hammer":
				CancelAim(instant: true);
				Unlodge();
				return true;
			}
		}
		return !lodged;
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		OnDeath();
	}

	public override void OnFall()
	{
		CancelAim(instant: true);
	}

	public override void OnDamage(ref DamageData data)
	{
		if (aiming && interruptables.Contains(data.hitTarget.transform))
		{
			string hitter = data.hitter;
			if (hitter == "revolver" || hitter == "coin")
			{
				Interrupt();
			}
		}
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		eid = GetComponent<EnemyIdentifier>();
		tempPath = new NavMeshPath();
		aimLines = new PortalLineRenderer(aimLine);
	}

	private void Start()
	{
		scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		currentBodyRotation = base.transform.rotation;
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (stationary)
		{
			stationaryPosition = base.transform.position;
		}
		if (quickStart)
		{
			cooldown = 0.5f;
		}
		Invoke("NavigationUpdate", 0.5f);
		switch (difficulty)
		{
		case 4:
		case 5:
			maxAimTime = 3f;
			break;
		case 3:
			maxAimTime = 4f;
			break;
		case 2:
			maxAimTime = 5f;
			break;
		case 1:
			maxAimTime = 5f;
			break;
		case 0:
			maxAimTime = 7.5f;
			break;
		}
		sightQuery = new VisionQuery("TurretSight", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)));
		SetAnimatorSpeed();
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = 20f,
			angularSpeed = 1200f,
			acceleration = 360f
		};
	}

	private void UpdateBuff()
	{
		SetAnimatorSpeed();
	}

	private void SetAnimatorSpeed()
	{
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			anim.speed = 1f;
			break;
		case 1:
			anim.speed = 0.75f;
			break;
		case 0:
			anim.speed = 0.5f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
	}

	private void OnEnable()
	{
		Unlodge();
		CancelAim(instant: true);
		DamageStop();
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
			delayedLastPlayerPosition = details.enterToExit.MultiplyPoint3x4(delayedLastPlayerPosition);
		}
		if (lastTargetHandle != null && traveller.id == lastTargetHandle.id)
		{
			lastTargetHandle = lastTargetHandle.Then(details.portalSequence);
		}
	}

	private void VisionUpdate()
	{
		UpdateVisionPosition();
		if (mach.vision.TrySee(sightQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
			lastTargetHandle = targetHandle;
			lastDimensionalTarget = Vector3.zero;
		}
		else
		{
			targetHandle = null;
			mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
		}
	}

	private void UpdateVisionPosition()
	{
		Vector3 position = base.transform.position;
		cachedVisionPos = new Vector3(position.x, head.transform.position.y, position.z);
		barrelOffSetDist = Vector3.Distance(barrelTip.transform.position, cachedVisionPos);
	}

	private void NavigationUpdate()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Invalid comparison between Unknown and I4
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Invalid comparison between Unknown and I4
		Invoke("NavigationUpdate", GetUpdateRate(nma, 0.25f));
		if (inAction || !mach.grounded || !nma.isOnNavMesh)
		{
			wandering = false;
			return;
		}
		if (stationary)
		{
			if (Vector3.Distance(base.transform.position, stationaryPosition) <= 1f)
			{
				return;
			}
			NavMesh.CalculatePath(base.transform.position, stationaryPosition, nma.areaMask, tempPath);
			if ((int)tempPath.status == 0)
			{
				mach.nma.SetPath(tempPath);
				return;
			}
		}
		if (eid.target == null)
		{
			return;
		}
		Vector3 vector = (hasDimensionalTarget ? lastDimensionalTarget : eid.target.position);
		bool flag = nma.isOnNavMesh && nma.CalculatePath(vector, tempPath) && (int)tempPath.status == 0;
		Vector3 pos;
		if (!wandering && !hasVision && flag)
		{
			nma.path = tempPath;
		}
		else if (wandering)
		{
			if (!nma.pathPending && (nma.remainingDistance < 1f || (int)nma.path.status == 2))
			{
				wandering = false;
			}
		}
		else if (GetWanderPosition(out pos))
		{
			mach.SetDestination(pos);
			wandering = true;
		}
	}

	private bool GetWanderPosition(out Vector3 pos)
	{
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere = new Vector3(onUnitSphere.x, 0f, onUnitSphere.z);
		bool flag = PortalPhysicsV2.Raycast(torso.position, onUnitSphere, 25f, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out var portalTraversals, out var endPoint);
		pos = endPoint;
		if (portalTraversals.Length != 0)
		{
			if (flag)
			{
				pos = hitInfo.point - portalTraversals[^1].exitDirection * 0.25f;
			}
			for (int i = 0; i < portalTraversals.Length; i++)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[i];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				Portal portalObject = portalTraversalV.portalObject;
				if (!portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.Enemy))
				{
					flag = true;
					pos = portalObject.GetTransform(portalHandle.side).GetPositionInFront(portalTraversals[i].entrancePoint, 0.25f);
					break;
				}
			}
		}
		else if (flag)
		{
			pos = hitInfo.point - onUnitSphere * 0.25f;
		}
		if (Physics.Raycast(pos, Vector3.down, out var hitInfo2, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
		{
			pos = hitInfo2.point;
			return true;
		}
		NavMeshHit val = default(NavMeshHit);
		if (NavMesh.SamplePosition(pos, ref val, 5f, nma.areaMask))
		{
			pos = ((NavMeshHit)(ref val)).position;
			return true;
		}
		if (Physics.SphereCast(pos, 1f, Vector3.down, out var hitInfo3, 25f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			pos = hitInfo3.point;
			return true;
		}
		return false;
	}

	private void Update()
	{
		anim.SetBool("Running", !inAction && mach.grounded && (nma.velocity.magnitude >= 1f || mach.isTraversingPortalLink));
		if (eid.target != null)
		{
			VisionUpdate();
		}
		else
		{
			targetHandle = null;
			lastDimensionalTarget = Vector3.zero;
		}
		if (aiming)
		{
			if (mach.grounded && eid.target != null && lastTargetHandle != null)
			{
				Aiming();
			}
			else
			{
				CancelAim(instant: true);
			}
		}
		else if (!hasVision)
		{
			if ((bool)(UnityEngine.Object)(object)currentSound)
			{
				UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
			}
			if (!hasDimensionalTarget)
			{
				return;
			}
		}
		currentLightsIntensity = Mathf.MoveTowards(currentLightsIntensity, lightsIntensityTarget, Time.deltaTime / 4f);
		if (currentLightsIntensity == lightsIntensityTarget)
		{
			lightsIntensityTarget = ((lightsIntensityTarget == 1.5f) ? (lightsIntensityTarget = 1.25f) : (lightsIntensityTarget = 1.5f));
		}
		ChangeLightsIntensity(currentLightsIntensity);
		if (inAction || !mach.grounded)
		{
			return;
		}
		rubbleLeft.SetActive(value: false);
		rubbleRight.SetActive(value: false);
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		kickCooldown = Mathf.MoveTowards(kickCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		if (stationary && Vector3.Distance(base.transform.position, stationaryPosition) <= 1f)
		{
			base.transform.LookAt(ToPlanePos(lastTargetData.position));
		}
		if (eid.target != null && mach.gc != null && mach.gc.onGround)
		{
			if (!hasDimensionalTarget && lastTargetData.DistanceTo(base.transform.position) < 5f && kickCooldown <= 0f && difficulty >= 2)
			{
				Kick();
			}
			else if (cooldown <= 0f && !IsWindUpObstructed())
			{
				StartWindup();
			}
		}
	}

	public bool IsWindUpObstructed()
	{
		float radius = 1.5f;
		return IsTargetObstructed(radius);
	}

	public bool IsTargetObstructed(float radius = 0f)
	{
		Vector3 vector = cachedVisionPos;
		Vector3 vector2 = (hasDimensionalTarget ? lastDimensionalTarget : lastTargetData.position);
		float num = (hasDimensionalTarget ? Vector3.Distance(vector, vector2) : lastTargetData.DistanceTo(vector));
		num -= radius * 1.5f;
		GameObject obj = (hasDimensionalTarget ? MonoSingleton<NewMovement>.Instance.GameObject : lastTargetData.target.GameObject);
		float num2 = 0.5f;
		if (obj.TryGetComponent<Collider>(out var component))
		{
			num2 = component.bounds.extents.x;
		}
		num -= num2 * 1.5f;
		Vector3 vector3 = vector2 - vector;
		PhysicsCastResult hitInfo;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		if (radius <= 0f)
		{
			return PortalPhysicsV2.Raycast(vector, vector3.normalized, num, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), out hitInfo, out portalTraversals, out endPoint);
		}
		return PortalPhysicsV2.SphereCast(vector, vector3.normalized, num, radius, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), out hitInfo, out portalTraversals, out endPoint);
	}

	private void Aiming()
	{
		bool flag = hasVision;
		if (!flag && hasDimensionalTarget)
		{
			flag = !IsTargetObstructed();
		}
		if (flag)
		{
			outOfSightTimer = 0f;
		}
		else if (!flag && !isAimFlashing)
		{
			outOfSightTimer += Time.deltaTime;
		}
		if (difficulty <= 1 && aimTime >= maxAimTime)
		{
			if (difficulty == 1)
			{
				delayedLastPlayerPosition = Vector3.MoveTowards(delayedLastPlayerPosition, lastTargetData.position, Time.deltaTime * Vector3.Distance(delayedLastPlayerPosition, lastTargetData.position) * 5f * eid.totalSpeedModifier);
			}
			aimPos = delayedLastPlayerPosition;
		}
		else
		{
			Vector3 position = lastTargetHandle.target.Position;
			aimPos = scene.GetTravelMatrix(in lastTargetHandle.portals).MultiplyPoint3x4(position);
		}
		aimDirection = (aimPos - VisionSourcePosition).normalized;
		if (PortalPhysicsV2.Raycast(VisionSourcePosition, aimDirection, aimDistance, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out var portalTraversals, out var _))
		{
			aimPos = hitInfo.point;
		}
		aimTraversals = portalTraversals;
		if (outOfSightTimer >= 1f)
		{
			if ((bool)(UnityEngine.Object)(object)currentSound)
			{
				UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
			}
			currentSound = UnityEngine.Object.Instantiate<AudioSource>(cancelSound, torso);
			CancelAim();
			return;
		}
		float target = (hasDimensionalTarget ? 0f : maxAimTime);
		aimTime = Mathf.MoveTowards(aimTime, target, Time.deltaTime * eid.totalSpeedModifier);
		if (aimTime >= maxAimTime && (hasVision || isAimFlashing))
		{
			if (!isAimFlashing)
			{
				UnityEngine.Object.Instantiate(warningFlash, barrelTip.transform).transform.localScale *= 2.5f;
				ChangeLineColor(new Color(1f, 0.75f, 0.5f));
				delayedLastPlayerPosition = lastTargetData.position;
				ChangeLightsColor(attackingLightsColor);
				mach.ParryableCheck();
			}
			flashTime = Mathf.MoveTowards(flashTime, 1f, Time.deltaTime * (float)((difficulty < 2) ? 1 : 2) * eid.totalSpeedModifier);
			if (flashTime >= 1f)
			{
				Shoot();
			}
		}
		else if (aimTime >= nextBeepTime && (float)sinceLastBeep >= 0.075f)
		{
			ChangeLineColor(whiteLine ? Color.white : defaultColor);
			if ((UnityEngine.Object)(object)antennaFlash != null)
			{
				antennaFlash.Play();
			}
			if ((UnityEngine.Object)(object)antennaSound != null)
			{
				antennaSound.Play(tracked: true);
			}
			whiteLine = !whiteLine;
			nextBeepTime = aimTime + (maxAimTime - aimTime) / 6f;
			sinceLastBeep = 0f;
		}
	}

	private void LateUpdate()
	{
		if (aiming)
		{
			Vector3 position = aimPos;
			if (aimTraversals.Length == 0 && (difficulty > 1 || !(aimTime >= maxAimTime)) && eid.target != null && hasVision)
			{
				position = eid.target.position;
			}
			AimAt(position, aimDirection, aimDistance, aimTraversals);
		}
		else
		{
			if (!hasVision || !bodyRotate)
			{
				return;
			}
			if (bodyTrackPlayer || bodyReset)
			{
				Quaternion quaternion = (bodyReset ? base.transform.rotation : Quaternion.LookRotation(aimPos - torso.position));
				float num = (bodyTrackPlayer ? 35 : 10);
				currentBodyRotation = Quaternion.RotateTowards(currentBodyRotation, quaternion, Time.deltaTime * (Quaternion.Angle(quaternion, currentBodyRotation) * num + num) * eid.totalSpeedModifier);
				if (bodyReset && currentBodyRotation == quaternion)
				{
					bodyRotate = false;
					bodyReset = false;
				}
			}
			torso.rotation = currentBodyRotation;
			torso.Rotate(Vector3.up * -90f, Space.Self);
		}
	}

	private void StartWindup()
	{
		anim.SetBool("Aiming", true);
		if (nma.isOnNavMesh)
		{
			mach.SetDestination(base.transform.position);
		}
		base.transform.LookAt(ToPlanePos(lastTargetData.position));
		inAction = true;
		kickCooldown = 0f;
		if ((bool)(UnityEngine.Object)(object)currentSound)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
		}
		currentSound = UnityEngine.Object.Instantiate<AudioSource>(aimWarningSound, torso);
	}

	private void BodyTrack()
	{
		bodyRotate = true;
		bodyTrackPlayer = true;
		bodyReset = false;
	}

	private void BodyFreeze()
	{
		bodyRotate = true;
		bodyTrackPlayer = false;
		bodyReset = false;
	}

	private void BodyReset()
	{
		bodyRotate = true;
		bodyTrackPlayer = false;
		bodyReset = true;
	}

	private void StartAiming()
	{
		aiming = true;
		whiteLine = false;
		ChangeLineColor(defaultColor);
		nextBeepTime = aimTime + (maxAimTime - aimTime) / 6f;
		flashTime = 0f;
		eid.weakPoint = antenna;
		shotsInARow = 0;
		if ((UnityEngine.Object)(object)antennaFlash != null)
		{
			antennaFlash.Play();
		}
		if ((UnityEngine.Object)(object)antennaSound != null)
		{
			antennaSound.Play(tracked: true);
		}
	}

	private void Kick()
	{
		anim.SetTrigger("Kick");
		if (nma.isOnNavMesh)
		{
			mach.SetDestination(base.transform.position);
		}
		base.transform.LookAt(ToPlanePos(lastTargetData.position));
		inAction = true;
		ChangeLightsColor(new Color(0.35f, 0.55f, 1f));
		kickCooldown = 1f;
		if ((bool)(UnityEngine.Object)(object)currentSound)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
		}
		currentSound = UnityEngine.Object.Instantiate<AudioSource>(kickWarningSound, torso);
		UnparryableFlash();
	}

	private void StopAction()
	{
		inAction = false;
		rubbleLeft.SetActive(value: false);
		rubbleRight.SetActive(value: false);
	}

	private void AimAt(Vector3 position, Vector3 direction, float distance, PortalTraversalV2[] traversals)
	{
		Vector3 worldPosition = ((traversals.Length != 0) ? (VisionSourcePosition + direction * distance) : position);
		torso.LookAt(worldPosition);
		currentBodyRotation = torso.rotation;
		torso.Rotate(Vector3.up * -90f, Space.Self);
		turret.LookAt(worldPosition, torso.up);
		Vector3 forward = turret.forward;
		turret.Rotate(Vector3.up * -90f, Space.Self);
		barrelPos = barrelTip.position;
		isBarrelPortalBlocked = false;
		isBarrelPortalCrossed = false;
		Vector3 vector = position;
		PortalTraversalV2[] array = traversals;
		shootRotation = Quaternion.LookRotation(forward, Vector3.up);
		if (traversals.Length != 0)
		{
			PortalPhysicsV2.Raycast(VisionSourcePosition, direction.normalized, barrelOffSetDist, default(LayerMask), out var _, out var portalTraversals, out var endPoint);
			barrelPos = endPoint;
			if (portalTraversals.Length != 0)
			{
				isBarrelPortalCrossed = true;
				PortalTraversalV2 portalTraversalV = traversals[0];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				Portal portalObject = portalTraversalV.portalObject;
				direction = scene.GetTravelMatrix(portalTraversals).MultiplyVector(direction);
				PhysicsCastResult hitInfo2;
				PortalTraversalV2[] portalTraversals2;
				bool num = PortalPhysicsV2.Raycast(barrelPos, direction, distance - barrelOffSetDist, default(LayerMask), out hitInfo2, out portalTraversals2, out endPoint);
				array = portalTraversals2;
				vector = (num ? hitInfo2.point : endPoint);
				isBarrelPortalBlocked = !portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile);
				shootRotation = Quaternion.LookRotation(direction, Vector3.up);
			}
		}
		Vector3 end = vector;
		if (traversals.Length != 0 && array == traversals && eid.target != null)
		{
			end = eid.target.position;
		}
		aimLines.SetLines(barrelPos, end, array);
		aimLines.SetEnabled(value: true);
	}

	private void Shoot()
	{
		if (!isBarrelPortalBlocked)
		{
			Vector3 position = (isBarrelPortalCrossed ? barrelPos : new Vector3(base.transform.position.x, barrelTip.transform.position.y, base.transform.position.z));
			RevolverBeam revolverBeam = UnityEngine.Object.Instantiate(beam, position, shootRotation);
			revolverBeam.alternateStartPoint = (isBarrelPortalCrossed ? Vector3.zero : barrelPos);
			if (revolverBeam.TryGetComponent<RevolverBeam>(out var component))
			{
				component.target = eid.target;
				component.damage *= eid.totalDamageModifier;
			}
		}
		anim.Play("Shoot");
		CancelAim();
		BodyFreeze();
		cooldown = UnityEngine.Random.Range(2.5f, 3.5f);
		shotsInARow++;
		if ((difficulty == 4 && shotsInARow < 2) || difficulty == 5)
		{
			Invoke("PreReAim", 0.25f);
		}
	}

	private void PreReAim()
	{
		anim.SetBool("Aiming", true);
		anim.Play("Aiming", -1, 0f);
		Invoke("ReAim", 0.25f);
	}

	private void ReAim()
	{
		flashTime = 0f;
		aiming = true;
		aimLines.SetEnabled(value: true);
		aimTime = maxAimTime;
		eid.weakPoint = antenna;
	}

	private void ChangeLineColor(Color clr)
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[1];
		array[0].color = clr;
		GradientAlphaKey[] array2 = new GradientAlphaKey[1];
		array2[0].alpha = 1f;
		gradient.SetKeys(array, array2);
		aimLines.SetGradient(gradient);
		nextBeepTime = (maxAimTime - aimTime) / 2f;
	}

	public void CancelAim(bool instant = false)
	{
		ChangeLightsColor(defaultLightsColor);
		aiming = false;
		aimLines.SetEnabled(value: false);
		aimTime = 0f;
		outOfSightTimer = 0f;
		anim.SetBool("Aiming", false);
		BodyReset();
		eid.weakPoint = head;
		mach.parryable = false;
		CancelInvoke("PreReAim");
		CancelInvoke("ReAim");
		if (instant)
		{
			inAction = false;
			if (mach.grounded)
			{
				anim.Play("Idle");
			}
		}
		if (cooldown < 1f)
		{
			cooldown = 1f;
		}
	}

	public void LodgeFoot(int type)
	{
		if (type == 0)
		{
			leftLodged = true;
			rubbleLeft.SetActive(value: true);
		}
		else
		{
			rightLodged = true;
			rubbleRight.SetActive(value: true);
		}
		if (leftLodged && rightLodged)
		{
			lodged = true;
		}
	}

	public void UnlodgeFoot(int type)
	{
		if (type == 0 && leftLodged)
		{
			leftLodged = false;
			rubbleLeft.SetActive(value: false);
			UnityEngine.Object.Instantiate(rubble, rubbleLeft.transform.position, base.transform.rotation);
		}
		else if (type == 1 && rightLodged)
		{
			rightLodged = false;
			rubbleRight.SetActive(value: false);
			UnityEngine.Object.Instantiate(rubble, rubbleRight.transform.position, base.transform.rotation);
		}
		lodged = false;
	}

	public void Unlodge()
	{
		UnlodgeFoot(0);
		UnlodgeFoot(1);
		kickCooldown = 0.25f;
	}

	public void Interrupt()
	{
		if (!mach.limp)
		{
			anim.SetTrigger("Interrupt");
			CancelAim();
			BodyFreeze();
			cooldown = 3f;
			if ((bool)(UnityEngine.Object)(object)currentSound)
			{
				UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
			}
			currentSound = UnityEngine.Object.Instantiate<AudioSource>(interruptSound, torso);
		}
	}

	public void OnDeath()
	{
		CancelAim();
		if ((bool)(UnityEngine.Object)(object)currentSound)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentSound).gameObject);
		}
		ChangeLightsColor(new Color(0.05f, 0.05f, 0.05f, 1f));
		if ((bool)antennaLight)
		{
			antennaLight.enabled = false;
		}
		Unlodge();
		if ((bool)sc)
		{
			sc.gameObject.SetActive(value: false);
		}
		UnityEngine.Object.Destroy(this);
	}

	private void FootStep(float targetPitch)
	{
		if (targetPitch == 0f)
		{
			targetPitch = 1.5f;
		}
		UnityEngine.Object.Instantiate<AudioSource>(footStep, base.transform.position, Quaternion.identity).SetPitch(UnityEngine.Random.Range(targetPitch - 0.1f, targetPitch + 0.1f));
	}

	private void Thunk()
	{
		UnityEngine.Object.Instantiate<AudioSource>(thunkSound, base.transform.position, Quaternion.identity);
	}

	private void ExtendBarrel()
	{
		UnityEngine.Object.Instantiate<AudioSource>(extendSound, base.transform.position, Quaternion.identity);
	}

	private void GotParried()
	{
		Interrupt();
	}

	public void UnparryableFlash()
	{
		UnityEngine.Object.Instantiate(unparryableFlash, torso.position + base.transform.forward, base.transform.rotation).transform.localScale *= 2.5f;
	}

	public void DamageStart()
	{
		sc.DamageStart();
		tr.enabled = true;
	}

	public void DamageStop()
	{
		sc.DamageStop();
		tr.enabled = false;
		ChangeLightsColor(defaultLightsColor);
	}

	public void ChangeLightsColor(Color target)
	{
		if ((bool)smr && (bool)smr.sharedMaterial && smr.sharedMaterial.HasProperty("_EmissiveColor"))
		{
			smr.material.SetColor("_EmissiveColor", target);
			if ((bool)antennaLight)
			{
				antennaLight.color = target;
			}
		}
	}

	public void ChangeLightsIntensity(float amount)
	{
		if ((bool)smr && (bool)smr.sharedMaterial && smr.sharedMaterial.HasProperty("_EmissiveIntensity"))
		{
			smr.material.SetFloat("_EmissiveIntensity", amount);
			if ((bool)antennaLight)
			{
				antennaLight.intensity = amount * 8f;
			}
		}
	}
}
