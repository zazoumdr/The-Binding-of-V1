using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

public class Guttertank : EnemyScript, IHitTargetCallback
{
	private bool gotValues;

	private EnemyIdentifier eid;

	private NavMeshAgent nma;

	private Enemy mach;

	private Rigidbody rb;

	private Animator anim;

	private AudioSource aud;

	private Collider col;

	private int difficulty = -1;

	public bool stationary;

	public bool punchOnSpawn;

	private Vector3 stationaryPosition;

	private NavMeshPath path;

	private bool walking;

	private Vector3 walkTarget;

	private bool dead;

	[SerializeField]
	private SwingCheck2 sc;

	private bool inAction;

	private bool moveForward;

	private Vector3 moveTarget;

	private bool trackInAction;

	private bool overrideTarget;

	private bool lookAtTarget;

	private bool punching;

	private Vector3 overrideTargetPosition;

	private float aimRotationLerp;

	private float punchCooldown;

	private bool punchHit;

	public Transform shootPoint;

	public Grenade rocket;

	public GameObject rocketParticle;

	public Transform aimBone;

	private Quaternion torsoDefaultRotation;

	private float shootCooldown = 1f;

	private float lineOfSightTimer;

	public Landmine landmine;

	private float mineCooldown = 2f;

	private List<Landmine> placedMines = new List<Landmine>();

	private GoreZone gz;

	public AudioSource punchPrepSound;

	public AudioSource rocketPrepSound;

	public AudioSource minePrepSound;

	public AudioSource fallImpactSound;

	[HideInInspector]
	public bool firstPunch = true;

	public UltrakillEvent onFirstPunch;

	private Vector3 cachedVisionPos;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	private VisionQuery targetQuery;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => cachedVisionPos;

	private bool hasVision => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private Vector3 targetPosition
	{
		get
		{
			if (!hasDimensionalTarget)
			{
				if (!hasVision)
				{
					if (eid.target == null)
					{
						return Vector3.zero;
					}
					return eid.target.position;
				}
				return lastTargetData.position;
			}
			return lastDimensionalTarget;
		}
	}

	private Vector3 targetHeadPosition
	{
		get
		{
			if (!hasVision)
			{
				if (eid.target == null)
				{
					return Vector3.zero;
				}
				return eid.target.headPosition;
			}
			return lastTargetData.headPosition;
		}
	}

	private Vector3 targetVelocity
	{
		get
		{
			if (!hasVision)
			{
				if (eid.target == null)
				{
					return Vector3.zero;
				}
				return eid.target.GetVelocity();
			}
			return lastTargetData.velocity;
		}
	}

	private bool ObstructionCheck(TargetDataRef data)
	{
		return ObstructionCheck(data, toHead: false);
	}

	private bool ObstructionCheck(TargetDataRef data, bool toHead)
	{
		return !data.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.Environment), toHead);
	}

	private void Start()
	{
		GetValues();
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		targetQuery = new VisionQuery("GuttertankTarget", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && ObstructionCheck(t));
	}

	private void GetValues()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		if (!gotValues)
		{
			gotValues = true;
			eid = GetComponent<EnemyIdentifier>();
			nma = GetComponent<NavMeshAgent>();
			mach = GetComponent<Enemy>();
			rb = GetComponent<Rigidbody>();
			anim = GetComponent<Animator>();
			aud = GetComponent<AudioSource>();
			col = GetComponent<Collider>();
			shootCooldown = UnityEngine.Random.Range(0.75f, 1.25f);
			mineCooldown = UnityEngine.Random.Range(2f, 3f);
			stationaryPosition = base.transform.position;
			torsoDefaultRotation = Quaternion.Inverse(base.transform.rotation) * aimBone.rotation;
			path = new NavMeshPath();
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			gz = GoreZone.ResolveGoreZone(base.transform);
			SetSpeed();
			if (punchOnSpawn)
			{
				punchOnSpawn = false;
				Punch();
			}
			SlowUpdate();
		}
	}

	private void OnEnable()
	{
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

	public override void OnTeleport(PortalTravelDetails details)
	{
		targetHandle?.From(details.portalSequence);
		if ((bool)mach && moveForward)
		{
			moveTarget = details.enterToExit.MultiplyVector(moveTarget).normalized;
			sc.knockBackDirection = moveTarget;
		}
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		float num = 1f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			num = 1f;
			break;
		case 2:
			num = 0.9f;
			break;
		case 1:
			num = 0.8f;
			break;
		case 0:
			num = 0.6f;
			break;
		}
		float speed = 20f * num;
		return new EnemyMovementData
		{
			speed = speed,
			angularSpeed = 1200f,
			acceleration = 80f
		};
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		GetValues();
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			anim.speed = 1f;
			break;
		case 2:
			anim.speed = 0.9f;
			break;
		case 1:
			anim.speed = 0.8f;
			break;
		case 0:
			anim.speed = 0.6f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		nma.speed = 20f * anim.speed;
	}

	private void Update()
	{
		punchCooldown = Mathf.MoveTowards(punchCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		anim.SetBool("Walking", nma.velocity.magnitude > 2.5f);
		if (dead)
		{
			return;
		}
		if (eid.target == null)
		{
			UpdateLineOfSightTimer(hasLoS: false);
			return;
		}
		Vector3 position = base.transform.position;
		cachedVisionPos = new Vector3(position.x, mach.chest.transform.position.y, position.z);
		if (vision != null && vision.TrySee(targetQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
		}
		else
		{
			targetHandle = null;
			mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
		}
		if (inAction)
		{
			Vector3 vector = targetHeadPosition;
			if (overrideTarget)
			{
				vector = overrideTargetPosition;
			}
			if (trackInAction || moveForward)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(new Vector3(vector.x, base.transform.position.y, vector.z) - base.transform.position), (float)(trackInAction ? 360 : 90) * Time.deltaTime);
			}
			return;
		}
		if (shootCooldown > 0f)
		{
			shootCooldown = Mathf.MoveTowards(shootCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (mineCooldown > 0f)
		{
			mineCooldown = Mathf.MoveTowards(mineCooldown, 0f, Time.deltaTime * ((lineOfSightTimer >= 0.5f) ? 0.5f : 1f) * eid.totalSpeedModifier);
		}
		bool flag = hasVision;
		if (!flag && hasDimensionalTarget)
		{
			flag = !Physics.Linecast(VisionSourcePosition, lastDimensionalTarget, LayerMaskDefaults.Get(LMD.Environment));
		}
		UpdateLineOfSightTimer(flag);
		if (lineOfSightTimer >= 0.5f)
		{
			if (!hasDimensionalTarget)
			{
				bool flag2 = Vector3.Distance(base.transform.position, targetPosition) < 10f || Vector3.Distance(base.transform.position, PredictTargetPosition(0.5f)) < 10f;
				if (difficulty <= 1 && !flag2)
				{
					punchCooldown = ((difficulty == 1) ? 1 : 2);
				}
				if (punchCooldown <= 0f && flag2)
				{
					Punch();
				}
				else if (shootCooldown <= 0f && Vector3.Distance(base.transform.position, PredictTargetPosition(1f)) > 15f)
				{
					PrepRocket();
				}
			}
			else if (Vector3.Distance(base.transform.position, targetPosition) < 25f)
			{
				Halt();
			}
		}
		if (mineCooldown <= 0f)
		{
			if (CheckMines())
			{
				PrepMine();
			}
			else
			{
				mineCooldown = 0.5f;
			}
		}
	}

	private void UpdateLineOfSightTimer(bool hasLoS)
	{
		lineOfSightTimer = Mathf.MoveTowards(lineOfSightTimer, hasLoS ? 1 : 0, Time.deltaTime * eid.totalSpeedModifier);
	}

	private void LateUpdate()
	{
		if (dead || eid.target == null)
		{
			return;
		}
		aimRotationLerp = Mathf.MoveTowards(aimRotationLerp, (inAction && lookAtTarget) ? 1 : 0, Time.deltaTime * 5f);
		if (aimRotationLerp > 0f)
		{
			Vector3 vector = targetHeadPosition;
			if (overrideTarget)
			{
				vector = overrideTargetPosition;
			}
			if (punching)
			{
				vector = targetPosition;
			}
			Quaternion quaternion = Quaternion.LookRotation(aimBone.position - vector, Vector3.up);
			Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation * torsoDefaultRotation) * aimBone.rotation;
			aimBone.rotation = Quaternion.Lerp(aimBone.rotation, quaternion * quaternion2, aimRotationLerp);
			if (!moveForward)
			{
				sc.knockBackDirection = aimBone.forward * -1f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (dead || !inAction)
		{
			return;
		}
		rb.isKinematic = !moveForward;
		if (moveForward && !Physics.SphereCast(new Ray(base.transform.position + Vector3.up * 3f, moveTarget), 1.5f, 75f * Time.fixedDeltaTime * eid.totalSpeedModifier, LayerMaskDefaults.Get(LMD.Player)))
		{
			if (mach.IsLedgeSafe())
			{
				Vector3 vector = moveTarget * 75f * anim.speed * eid.totalSpeedModifier;
				rb.velocity = new Vector3(vector.x, rb.velocity.y, vector.z);
				sc.knockBackDirection = moveTarget;
			}
			else
			{
				rb.velocity = Vector3.zero;
			}
		}
	}

	private void SlowUpdate()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.25f));
		if (dead || eid.target == null)
		{
			return;
		}
		if (inAction || !mach.grounded || !nma.isOnNavMesh)
		{
			walking = false;
			return;
		}
		if (stationary)
		{
			if (!(Vector3.Distance(base.transform.position, stationaryPosition) > 1f))
			{
				return;
			}
			NavMesh.CalculatePath(base.transform.position, stationaryPosition, nma.areaMask, path);
			if ((int)path.status == 0)
			{
				nma.path = path;
				return;
			}
		}
		bool flag = false;
		lastDimensionalTarget = Vector3.zero;
		if (Vector3.Distance(base.transform.position, targetPosition) > 30f || Physics.CheckSphere(aimBone.position - Vector3.up * 0.5f, 1.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)) || Physics.SphereCast(aimBone.position - Vector3.up * 0.5f, 1.5f, targetPosition + Vector3.up - aimBone.position, out var _, Vector3.Distance(targetPosition + Vector3.up, aimBone.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			Vector3 position = eid.target.position;
			if (mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget))
			{
				position = lastDimensionalTarget;
			}
			position = EnemyTarget.GetNavPoint(position);
			NavMesh.CalculatePath(base.transform.position, position, nma.areaMask, path);
			if ((int)path.status == 0)
			{
				walking = false;
				flag = true;
				nma.path = path;
			}
		}
		if (walking || flag)
		{
			if (Vector3.Distance(base.transform.position, walkTarget) < 1f || (int)nma.path.status != 0)
			{
				walking = false;
			}
			return;
		}
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere = new Vector3(onUnitSphere.x, 0f, onUnitSphere.z);
		RaycastHit hitInfo3;
		if (Physics.Raycast(aimBone.position, onUnitSphere, out var hitInfo2, 25f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(hitInfo2.point, ref val, 5f, nma.areaMask))
			{
				walkTarget = ((NavMeshHit)(ref val)).position;
			}
			else if (Physics.SphereCast(hitInfo2.point, 1f, Vector3.down, out hitInfo2, 25f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				walkTarget = hitInfo2.point;
			}
		}
		else if (Physics.Raycast(aimBone.position + onUnitSphere * 25f, Vector3.down, out hitInfo3, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
		{
			walkTarget = hitInfo3.point;
		}
		NavMesh.CalculatePath(base.transform.position, walkTarget, nma.areaMask, path);
		nma.path = path;
		walking = true;
	}

	private Vector3 PredictTargetPosition(float time)
	{
		if (eid.target == null)
		{
			return Vector3.zero;
		}
		if (hasVision)
		{
			TargetData targetData = vision.CalculateData(targetHandle);
			return targetData.position + targetData.velocity * time;
		}
		return eid.target.PredictTargetPosition(time);
	}

	private bool CheckMines()
	{
		if (placedMines.Count >= 5)
		{
			for (int num = placedMines.Count - 1; num >= 0; num--)
			{
				if (placedMines[num] == null)
				{
					placedMines.RemoveAt(num);
				}
			}
			if (placedMines.Count >= 5)
			{
				return false;
			}
		}
		for (int num2 = MonoSingleton<ObjectTracker>.Instance.landmineList.Count - 1; num2 >= 0; num2--)
		{
			if (MonoSingleton<ObjectTracker>.Instance.landmineList[num2] != null && Vector3.Distance(base.transform.position, MonoSingleton<ObjectTracker>.Instance.landmineList[num2].transform.position) < 15f)
			{
				return false;
			}
		}
		return true;
	}

	private void PrepMine()
	{
		anim.Play("Landmine", 0, 0f);
		UnityEngine.Object.Instantiate<AudioSource>(minePrepSound, base.transform);
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		lookAtTarget = false;
		mineCooldown = UnityEngine.Random.Range(2f, 3f);
	}

	private void PlaceMine()
	{
		Landmine landmine = UnityEngine.Object.Instantiate(this.landmine, base.transform.position, base.transform.rotation, gz.transform);
		placedMines.Add(landmine);
		if (landmine.TryGetComponent<Landmine>(out var component))
		{
			component.originEnemy = eid;
		}
	}

	private void PrepRocket()
	{
		anim.Play("Shoot", 0, 0f);
		UnityEngine.Object.Instantiate<AudioSource>(rocketPrepSound, base.transform);
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		trackInAction = true;
		lookAtTarget = true;
		punching = false;
	}

	private void PredictTarget()
	{
		Vector3 vector = shootPoint.position + base.transform.forward;
		Quaternion quaternion = base.transform.rotation;
		PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, vector - VisionSourcePosition, default(LayerMask), out var _, out var _, out var traversals);
		if (traversals.Length != 0)
		{
			Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
			vector = travelMatrix.MultiplyPoint3x4(vector);
			quaternion = travelMatrix.rotation * quaternion;
		}
		UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, vector, quaternion).transform.localScale *= 10f;
		if (eid.target != null)
		{
			overrideTarget = true;
			float num = 1f;
			if (difficulty <= 1)
			{
				num = ((difficulty == 0) ? 0.5f : 0.75f);
			}
			float num2 = Vector3.Distance(shootPoint.position, targetHeadPosition);
			overrideTargetPosition = targetPosition + targetVelocity * ((UnityEngine.Random.Range(0.75f, 1f) + num2 / 150f) * num);
			if (Physics.Raycast(targetPosition, Vector3.down, 15f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				overrideTargetPosition = new Vector3(overrideTargetPosition.x, targetHeadPosition.y, overrideTargetPosition.z);
			}
			if (Physics.Raycast(aimBone.position, overrideTargetPosition - aimBone.position, out var hitInfo, Vector3.Distance(overrideTargetPosition, aimBone.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)) && (!hitInfo.transform.TryGetComponent<Breakable>(out var component) || component.playerOnly))
			{
				overrideTargetPosition = targetHeadPosition;
			}
			else if (overrideTargetPosition != targetHeadPosition && col.Raycast(new Ray(targetHeadPosition, (overrideTargetPosition - targetHeadPosition).normalized), out hitInfo, Vector3.Distance(targetHeadPosition, overrideTargetPosition)))
			{
				overrideTargetPosition = targetHeadPosition;
			}
		}
	}

	private void FireRocket()
	{
		Vector3 vector = (MonoSingleton<WeaponCharges>.Instance.rocketFrozen ? (shootPoint.position + shootPoint.forward * 2.5f) : shootPoint.position);
		Vector3 vector2 = shootPoint.position;
		Quaternion quaternion = Quaternion.LookRotation(overrideTargetPosition - shootPoint.position);
		Quaternion quaternion2 = quaternion;
		PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, vector - VisionSourcePosition, default(LayerMask), out var hit, out var endPoint, out var traversals);
		bool flag = false;
		if (!MonoSingleton<WeaponCharges>.Instance.rocketFrozen && traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
				vector = travelMatrix.MultiplyPoint3x4(vector);
				vector2 = travelMatrix.MultiplyPoint3x4(vector2);
				quaternion = travelMatrix.rotation * quaternion;
			}
			else
			{
				flag = !portalObject.passThroughNonTraversals;
			}
		}
		else
		{
			if (traversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV2 = traversals[0];
				PortalHandle portalHandle2 = portalTraversalV2.portalHandle;
				Portal portalObject2 = portalTraversalV2.portalObject;
				if (portalObject2.GetTravelFlags(portalHandle2.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
				{
					vector = PortalUtils.GetTravelMatrix(traversals).MultiplyPoint3x4(vector);
				}
				else
				{
					flag = !portalObject2.passThroughNonTraversals;
				}
			}
			PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, vector - VisionSourcePosition, default(LayerMask), out hit, out endPoint, out var traversals2);
			if (traversals2.Length != 0)
			{
				Matrix4x4 travelMatrix2 = PortalUtils.GetTravelMatrix(traversals2);
				vector2 = travelMatrix2.MultiplyPoint3x4(vector2);
				quaternion2 = travelMatrix2.rotation * quaternion2;
			}
		}
		if (!flag)
		{
			Grenade grenade = UnityEngine.Object.Instantiate(rocket, vector, quaternion);
			grenade.proximityTargetHandle = ((traversals.Length != 0) ? targetHandle.Then(new PortalHandleSequence(traversals)) : targetHandle);
			grenade.ignoreEnemyType.Add(eid.enemyType);
			grenade.originEnemy = eid;
			if (eid.totalDamageModifier != 1f)
			{
				grenade.totalDamageMultiplier = eid.totalDamageModifier;
			}
			if (difficulty <= 1)
			{
				grenade.rocketSpeed *= ((difficulty == 0) ? 0.6f : 0.8f);
			}
		}
		UnityEngine.Object.Instantiate(rocketParticle, vector2, quaternion2);
		shootCooldown = UnityEngine.Random.Range(1.25f, 1.75f) - ((difficulty >= 4) ? 0.5f : 0f);
	}

	private void Death()
	{
		PunchStop();
		dead = true;
		if (TryGetComponent<Collider>(out var component))
		{
			component.enabled = false;
		}
		if (mach.gc.onGround)
		{
			rb.isKinematic = true;
		}
		else
		{
			rb.constraints = (RigidbodyConstraints)122;
		}
		mach.parryable = false;
		base.enabled = false;
	}

	private void Halt()
	{
		walking = false;
		nma.SetDestination(base.transform.position);
	}

	private void Punch()
	{
		if (difficulty <= 2)
		{
			punchCooldown = 4.5f - (float)difficulty;
		}
		else if (difficulty == 4)
		{
			punchCooldown = 1.5f;
		}
		anim.Play("Punch", 0, 0f);
		UnityEngine.Object.Instantiate<AudioSource>(punchPrepSound, base.transform);
		UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, sc.transform.position + base.transform.forward, base.transform.rotation).transform.localScale *= 5f;
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		trackInAction = true;
		lookAtTarget = true;
		punching = true;
		punchHit = false;
	}

	private void PunchActive()
	{
		sc.DamageStart();
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = base.transform.forward;
		moveTarget = base.transform.forward;
		moveForward = true;
		trackInAction = false;
		if (rb.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
		{
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
		if (firstPunch)
		{
			firstPunch = false;
			onFirstPunch?.Invoke();
		}
	}

	public void TargetBeenHit()
	{
		punchHit = true;
	}

	private void PunchStop()
	{
		sc.DamageStop();
		moveForward = false;
		if (rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
		if (!punchHit || difficulty < 3)
		{
			bool flag = difficulty < 4 && !punchHit;
			if (!flag && (!punchHit || difficulty <= 2))
			{
				Vector3Int voxelPosition = StainVoxelManager.WorldToVoxelPosition(base.transform.position + Vector3.down * 1.8333334f);
				flag = MonoSingleton<StainVoxelManager>.Instance.HasProxiesAt(voxelPosition, 3, VoxelCheckingShape.VerticalBox, ProxySearchMode.AnyFloor);
			}
			if (flag)
			{
				anim.Play("PunchStagger");
			}
		}
	}

	private void FallImpact()
	{
		UnityEngine.Object.Instantiate<AudioSource>(fallImpactSound, new Vector3(eid.weakPoint.transform.position.x, base.transform.position.y, eid.weakPoint.transform.position.z), Quaternion.identity);
		eid.hitter = "";
		eid.DeliverDamage(mach.chest, Vector3.zero, mach.chest.transform.position, 0.1f, tryForExplode: false);
		if (!eid.dead)
		{
			mach.parryable = true;
			UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, sc.transform.position + base.transform.forward * 5f, base.transform.rotation).transform.localScale *= 10f;
		}
		else
		{
			mach.parryable = false;
			MonoSingleton<StyleHUD>.Instance.AddPoints(50, "SLIPPED");
		}
	}

	private void GotParried()
	{
		if (!eid.dead && (bool)(UnityEngine.Object)(object)anim)
		{
			anim.Play("PunchStagger", -1, 0.7f);
		}
		mach.parryable = false;
	}

	private void StopParryable()
	{
		mach.parryable = false;
	}

	private void StopAction()
	{
		if (!dead)
		{
			inAction = false;
			((Behaviour)(object)nma).enabled = true;
			overrideTarget = false;
			punching = false;
			lookAtTarget = false;
		}
	}
}
