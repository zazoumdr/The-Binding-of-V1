using System;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

public class ZombieProjectiles : EnemyScript
{
	public bool stationary;

	public bool alwaysStationary;

	public bool smallRay;

	public bool wanderer;

	public bool afraid;

	public bool chaser;

	public bool hasMelee;

	public bool quickStart;

	private Enemy zmb;

	private GameObject player;

	private GameObject camObj;

	private NavMeshAgent nma;

	private Animator anim;

	private Rigidbody rb;

	private LayerMask lm;

	private float bodySize = 0.75f;

	private float coolDown = 1f;

	private AudioSource aud;

	public TrailRenderer tr;

	public GameObject projectile;

	public ContinuousBeam projectileBeam;

	private GameObject currentProjectile;

	private GameObject previousProjectile;

	public Transform shootPos;

	public GameObject head;

	public bool targetSpotted;

	private RaycastHit rhit;

	private NavMeshPath tempPath;

	private Vector3 wanderTarget;

	private float raySize = 1f;

	private bool musicRequested;

	public GameObject decProjectileSpawner;

	public GameObject decProjectile;

	private GameObject currentDecProjectile;

	public bool swinging;

	[HideInInspector]
	public bool blocking;

	private int _difficulty = -1;

	private float coolDownReduce;

	private EnemyIdentifier eid;

	private GameObject origWP;

	public Transform aimer;

	private Quaternion aimerDefaultRotation;

	private bool aiming;

	private Quaternion origRotation;

	private float aimEase;

	private Vector3 predictedPosition;

	private float predictionLerp;

	private bool predictionLerping;

	private bool moveForward;

	private float forwardSpeed;

	private SwingCheck2[] swingChecks;

	private float fleeStopDuration;

	private bool isFleeingPlayer;

	private bool isWandering;

	private Vector3 spawnPos;

	private bool valuesSet;

	public bool shouldSortie;

	public Vector3 sortiePos;

	private VisionQuery visionQuery;

	private TargetData lastTargetData = TargetData.None;

	private TargetHandle targetHandle;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private Vision vision => zmb.vision;

	[HideInInspector]
	public int difficulty
	{
		get
		{
			return _difficulty;
		}
		set
		{
			_difficulty = value;
		}
	}

	public override Vector3 VisionSourcePosition => zmb.chest.transform.position;

	private bool hasVision => targetHandle != null;

	private bool isVisionThroughPortal
	{
		get
		{
			if (hasVision)
			{
				return targetHandle.portals.Count > 0;
			}
			return false;
		}
	}

	private float targetDistance
	{
		get
		{
			if (!hasDimensionalTarget)
			{
				return lastTargetData.DistanceTo(base.transform.position);
			}
			return Vector3.Distance(base.transform.position, lastDimensionalTarget);
		}
	}

	public float shootRange => (coolDown <= 0f) ? 60 : 30;

	public bool inShootRange => targetDistance < shootRange;

	public bool inMeleeRange => targetDistance < 3f;

	public bool inFleeRange => targetDistance < 15f;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	public override void SetSortiePos(Vector3 pos)
	{
		shouldSortie = true;
		sortiePos = pos;
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		float num = default(float);
		if (eid.enemyType == EnemyType.Soldier)
		{
			switch (difficulty)
			{
			case 0:
				num = 7.5f;
				break;
			case 1:
				num = 11.25f;
				break;
			case 2:
				num = 15f;
				break;
			case 3:
				num = 18.75f;
				break;
			case 4:
				num = 26.25f;
				break;
			case 5:
				num = 30f;
				break;
			default:
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(difficulty);
				break;
			}
			float speed = num;
			return new EnemyMovementData
			{
				speed = speed,
				angularSpeed = 480f,
				acceleration = 480f
			};
		}
		if (difficulty < 4)
		{
			switch (difficulty)
			{
			case 0:
				num = 5f;
				break;
			case 1:
				num = 7.5f;
				break;
			case 2:
				num = 10f;
				break;
			case 3:
				num = 12.5f;
				break;
			default:
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(difficulty);
				break;
			}
		}
		else
		{
			num = 15f;
		}
		float speed2 = num;
		return new EnemyMovementData
		{
			speed = speed2,
			angularSpeed = 800f,
			acceleration = 30f
		};
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		DamageEnd();
		if (!zmb.chestExploding)
		{
			zmb.anim.StopPlayback();
		}
		if (hasMelee)
		{
			MeleeDamageEnd();
		}
		UnityEngine.Object.Destroy(this);
		Projectile componentInChildren = zmb.GetComponentInChildren<Projectile>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren.gameObject);
		}
	}

	public override void OnFall()
	{
		CancelAttack();
	}

	private void Awake()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		zmb = GetComponent<Enemy>();
		nma = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		tempPath = new NavMeshPath();
		lm = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			if (TryGetComponent<Collider>(out var component))
			{
				bodySize = component.bounds.extents.x;
			}
			player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
			camObj = MonoSingleton<PlayerTracker>.Instance.GetTarget().gameObject;
			if ((bool)aimer)
			{
				aimerDefaultRotation = Quaternion.Inverse(base.transform.rotation) * aimer.rotation;
			}
			if (hasMelee && (swingChecks == null || swingChecks.Length == 0))
			{
				swingChecks = GetComponentsInChildren<SwingCheck2>();
			}
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			origWP = eid.weakPoint;
			spawnPos = base.transform.position;
			if (stationary || smallRay)
			{
				raySize = 0.25f;
			}
			if (quickStart)
			{
				coolDown = 0f;
			}
			if (difficulty >= 3)
			{
				coolDownReduce = 1f;
			}
			_ = base.transform.position;
			visionQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, lm));
		}
	}

	private void Start()
	{
		if (eid.stationary)
		{
			alwaysStationary = true;
			stationary = true;
		}
		else if (alwaysStationary)
		{
			eid.stationary = true;
			stationary = true;
		}
		SetValues();
		if (!stationary && wanderer && eid.target != null)
		{
			Invoke("Wander", 0.5f);
		}
		SlowUpdate();
	}

	private void OnEnable()
	{
		SetValues();
		if (!zmb.musicRequested && targetSpotted && (bool)zmb && !eid.IgnorePlayer)
		{
			musicRequested = true;
			zmb.musicRequested = true;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if (instance != null)
			{
				instance.PlayBattleMusic();
			}
		}
		if (hasMelee)
		{
			MeleeDamageEnd();
		}
		if (tr != null)
		{
			tr.emitting = false;
		}
		if (currentDecProjectile != null)
		{
			UnityEngine.Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		swinging = false;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance2))
		{
			PortalManagerV2 portalManagerV = instance2;
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
		if (musicRequested && !eid.IgnorePlayer && !zmb.limp)
		{
			musicRequested = false;
			zmb.musicRequested = false;
			MusicManager instance2 = MonoSingleton<MusicManager>.Instance;
			if (instance2 != null)
			{
				instance2.PlayCleanMusic();
			}
		}
		coolDown = UnityEngine.Random.Range(1f, 2.5f) - coolDownReduce;
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		targetHandle?.From(details.portalSequence);
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
		}
	}

	private void SlowUpdate()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Invalid comparison between Unknown and I4
		if (!eid.dead)
		{
			if (eid.enemyType == EnemyType.Soldier)
			{
				Invoke("SlowUpdate", GetUpdateRate(nma));
			}
			else
			{
				Invoke("SlowUpdate", 0.5f);
			}
		}
		if (stationary && !alwaysStationary && Vector3.Distance(base.transform.position, spawnPos) > 5f)
		{
			stationary = false;
		}
		if (!base.gameObject.activeInHierarchy || eid.target == null)
		{
			return;
		}
		bool flag = (bool)(UnityEngine.Object)(object)nma && nma.isOnNavMesh;
		bool flag2 = flag && nma.CalculatePath(eid.target.position, tempPath) && (int)tempPath.status == 0;
		lastDimensionalTarget = Vector3.zero;
		if (!flag2 && flag && zmb.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget))
		{
			flag2 = nma.CalculatePath(lastDimensionalTarget, tempPath) && (int)tempPath.status == 0;
		}
		if (zmb.grounded && !zmb.limp && !swinging)
		{
			if ((bool)(UnityEngine.Object)(object)nma && (hasVision || hasDimensionalTarget) && afraid)
			{
				Flee();
			}
			if (hasVision && (!isFleeingPlayer || !afraid))
			{
				AttackCheck((!flag || flag2) && !hasDimensionalTarget);
			}
		}
		Navigate(flag2);
	}

	private void Flee()
	{
		if (inFleeRange && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && !nma.pathPending)
		{
			isFleeingPlayer = true;
			Vector3 vector = (hasDimensionalTarget ? lastDimensionalTarget : lastTargetData.position) - base.transform.position;
			PhysicsCastResult hitInfo;
			PortalTraversalV2[] portalTraversals;
			Vector3 endPoint;
			bool flag = PortalPhysicsV2.Raycast(head.transform.position, -vector.normalized, 5f, lm, out hitInfo, out portalTraversals, out endPoint, QueryTriggerInteraction.Ignore);
			Vector3 vector2;
			if (portalTraversals.Length != 0)
			{
				vector2 = endPoint;
				for (int i = 0; i < portalTraversals.Length; i++)
				{
					PortalHandle portalHandle = portalTraversals[i].portalHandle;
					Portal portalObject = PortalUtils.GetPortalObject(portalHandle);
					if (!portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.Enemy))
					{
						vector2 = portalObject.GetTransform(portalHandle.side).GetPositionInFront(portalTraversals[i].entrancePoint, 0.25f);
						break;
					}
				}
			}
			else
			{
				vector2 = (flag ? hitInfo.point : endPoint);
			}
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(vector2, ref val, 1f, nma.areaMask))
			{
				vector2 = ((NavMeshHit)(ref val)).position;
			}
			else if (NavMesh.FindClosestEdge(vector2, ref val, nma.areaMask))
			{
				vector2 = ((NavMeshHit)(ref val)).position;
			}
			zmb.SetDestination(vector2);
			if (nma.velocity.magnitude < 1f)
			{
				fleeStopDuration += 0.5f;
			}
			else
			{
				fleeStopDuration = 0f;
			}
		}
		if (!inFleeRange || fleeStopDuration > 0.75f || (inFleeRange && !nma.pathPending && nma.isOnNavMesh && nma.remainingDistance < 3f))
		{
			isFleeingPlayer = false;
			fleeStopDuration = 0f;
		}
	}

	private void AttackCheck(bool isPlayerPathable)
	{
		if (!targetSpotted)
		{
			return;
		}
		if (hasMelee && inMeleeRange)
		{
			Melee();
			return;
		}
		bool flag = !(UnityEngine.Object)(object)nma || !((Behaviour)(object)nma).enabled || nma.velocity.magnitude <= 2.5f;
		if (coolDown <= 0f && flag && (inShootRange || !isPlayerPathable))
		{
			Swing();
		}
	}

	private void Navigate(bool isPlayerPathable)
	{
		if (stationary || zmb.isTraversingPortalLink || !(UnityEngine.Object)(object)nma || !((Behaviour)(object)nma).enabled || !nma.isOnNavMesh)
		{
			return;
		}
		if (shouldSortie)
		{
			nma.SetDestination(sortiePos);
			if (Vector3.Distance(sortiePos, base.transform.position) < 0.1f)
			{
				shouldSortie = false;
			}
		}
		else
		{
			if (isFleeingPlayer)
			{
				return;
			}
			if (swinging)
			{
				nma.SetDestination(base.transform.position);
				return;
			}
			if (isWandering)
			{
				if (nma.remainingDistance > bodySize)
				{
					nma.SetDestination(wanderTarget);
					return;
				}
				isWandering = false;
			}
			if (!isPlayerPathable)
			{
				bool flag = false;
				if (lastTargetData.isValid() && lastTargetData.isAcrossPortals && lastTargetData.IsTargetInPortalOrth())
				{
					PortalHandleSequence portals = lastTargetData.handle.portals;
					PortalHandle handle = portals[portals.Count - 1];
					flag = PortalUtils.GetPortalObject(handle).GetTransform(handle.side).IsFacingDown();
				}
				if (flag && PortalEnemyUtils.CalcNavPosToPortalProjection_TargetWithPortalNormal(base.transform.position, lastTargetData, nma.areaMask, out var navPos))
				{
					nma.SetDestination(navPos);
					return;
				}
			}
			if (nma.pathPending)
			{
				return;
			}
			if (hasVision || hasDimensionalTarget)
			{
				if (wanderer)
				{
					if (!chaser && coolDown <= 0f)
					{
						nma.SetDestination(base.transform.position);
						return;
					}
					if ((bool)(UnityEngine.Object)(object)nma && nma.velocity.magnitude < 1f)
					{
						Wander();
						return;
					}
				}
				if (inShootRange && !chaser)
				{
					nma.SetDestination(base.transform.position);
					return;
				}
				if (isVisionThroughPortal && isPlayerPathable && lastTargetData.handle.portals[0].GetPortalIdentifier().IsValidLinkExist && ChasePortalTarget(nma, lastTargetData, tempPath))
				{
					return;
				}
			}
			Vector3 destination = (hasDimensionalTarget ? lastDimensionalTarget : eid.target.position);
			if (isPlayerPathable)
			{
				nma.SetDestination(destination);
				return;
			}
			if (Physics.Raycast(eid.target.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, lm))
			{
				nma.SetDestination(hitInfo.point);
			}
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.FindClosestEdge(eid.target.position, ref val, nma.areaMask))
			{
				nma.SetDestination(((NavMeshHit)(ref val)).position);
			}
			else
			{
				nma.SetDestination(base.transform.position);
			}
		}
	}

	private void Update()
	{
		if (eid.dead || !zmb.grounded || zmb.limp)
		{
			return;
		}
		if (coolDown > 0f)
		{
			coolDown = Mathf.MoveTowards(coolDown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (eid.target != null)
		{
			VisionUpdate();
			if (!targetSpotted && hasVision)
			{
				isFleeingPlayer = false;
				targetSpotted = true;
				coolDown = (float)UnityEngine.Random.Range(1, 2) - coolDownReduce / 2f;
				if (eid.target != null && eid.target.isPlayer && !musicRequested && !zmb.musicRequested)
				{
					musicRequested = true;
					zmb.musicRequested = true;
					MusicManager instance = MonoSingleton<MusicManager>.Instance;
					if ((bool)instance)
					{
						instance.PlayBattleMusic();
					}
				}
			}
		}
		else
		{
			targetHandle = null;
			lastDimensionalTarget = Vector3.zero;
		}
		if (!((UnityEngine.Object)(object)nma != null))
		{
			return;
		}
		if (((Behaviour)(object)nma).enabled && (nma.velocity.magnitude > 2.5f || zmb.isTraversingPortalLink))
		{
			anim.SetBool("Running", true);
			nma.updateRotation = true;
			return;
		}
		anim.SetBool("Running", false);
		nma.updateRotation = false;
		Vector3 vector = (hasDimensionalTarget ? lastDimensionalTarget : lastTargetData.position);
		Vector3 vector2 = vector - base.transform.position;
		Quaternion quaternion = Quaternion.LookRotation(new Vector3(vector2.x, 0f, vector2.z), Vector3.up);
		if (wanderer && swinging)
		{
			if (difficulty < 2)
			{
				return;
			}
			Quaternion b = Quaternion.LookRotation((ToPlanePos(vector) - base.transform.position).normalized);
			Transform transform = base.transform;
			int num = difficulty;
			Quaternion rotation = default(Quaternion);
			if (num <= 3)
			{
				switch (num)
				{
				case 2:
					rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * 3.5f * eid.totalSpeedModifier);
					break;
				case 3:
					rotation = quaternion;
					break;
				default:
					global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(num);
					break;
				}
			}
			else
			{
				rotation = quaternion;
			}
			transform.rotation = rotation;
		}
		else
		{
			base.transform.rotation = quaternion;
		}
	}

	private void VisionUpdate()
	{
		Vector3 position = base.transform.position;
		vision.sourcePos = new Vector3(position.x, zmb.chest.transform.position.y, position.z);
		if (vision.TrySee(visionQuery, out var data))
		{
			if (!data.target.isPlayer)
			{
				Debug.DrawLine(base.transform.position, data.position);
			}
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
			shouldSortie = false;
		}
		else
		{
			targetHandle = null;
			isWandering = false;
		}
	}

	private void LateUpdate()
	{
		if ((bool)aimer && aiming && hasVision)
		{
			Vector3 vector = ((difficulty >= 4) ? lastTargetData.headPosition : lastTargetData.position);
			Vector3 vector2 = (hasDimensionalTarget ? lastDimensionalTarget : vector);
			Vector3 vector3 = vector2 - head.transform.position;
			if (predictionLerping)
			{
				predictionLerp = Mathf.MoveTowards(predictionLerp, 1f, Time.deltaTime * 0.75f * anim.speed * eid.totalSpeedModifier);
				vector2 = Vector3.Lerp(vector2, predictedPosition, predictionLerp);
				vector3 = vector2 - head.transform.position;
			}
			Quaternion quaternion = Quaternion.LookRotation(vector3.normalized);
			Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation * aimerDefaultRotation) * aimer.rotation;
			aimer.rotation = quaternion * quaternion2;
			if (aimEase < 1f)
			{
				aimEase = Mathf.MoveTowards(aimEase, 1f, Time.deltaTime * (20f - aimEase * 20f) * eid.totalSpeedModifier);
			}
			aimer.rotation = Quaternion.Slerp(origRotation, quaternion, aimEase);
		}
	}

	private void FixedUpdate()
	{
		if (moveForward)
		{
			float num = forwardSpeed * anim.speed * eid.totalSpeedModifier;
			forwardSpeed /= 1f + Time.fixedDeltaTime * forwardSpeed / 3f;
			if (Physics.Raycast(base.transform.position + Vector3.up + base.transform.forward * 2.5f, Vector3.down, out var hitInfo, GetFixedUpdateMaxDistance(targetHandle), lm, QueryTriggerInteraction.Ignore) && Vector3.Dot(base.transform.up, hitInfo.normal) > 0.25f)
			{
				rb.velocity = new Vector3(base.transform.forward.x * num, Mathf.Min(0f, rb.velocity.y), base.transform.forward.z * num);
			}
			else
			{
				rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			}
		}
	}

	private float GetFixedUpdateMaxDistance(TargetHandle handle)
	{
		if (handle == null)
		{
			return 11f;
		}
		TargetData targetData = vision.CalculateData(handle);
		return Mathf.Max(11f, base.transform.position.y - targetData.position.y + 2.5f);
	}

	public void MoveForward(float speed)
	{
		forwardSpeed = speed * 10f;
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			((Behaviour)(object)nma).enabled = false;
		}
		moveForward = true;
		rb.isKinematic = false;
	}

	private void StopMoveForward()
	{
		moveForward = false;
		if (zmb.grounded)
		{
			if ((bool)(UnityEngine.Object)(object)nma)
			{
				((Behaviour)(object)nma).enabled = true;
			}
			rb.isKinematic = true;
		}
	}

	public void Melee()
	{
		swinging = true;
		isFleeingPlayer = false;
		nma.updateRotation = false;
		base.transform.LookAt(ToPlanePos(lastTargetData.position));
		((Behaviour)(object)nma).enabled = false;
		if (tr == null)
		{
			tr = GetComponentInChildren<TrailRenderer>();
		}
		tr.GetComponent<AudioSource>().Play(tracked: true);
		anim.SetTrigger("Melee");
	}

	public void MeleePrep()
	{
		zmb.ParryableCheck();
	}

	public void MeleeDamageStart()
	{
		if (tr == null)
		{
			tr = GetComponentInChildren<TrailRenderer>();
		}
		if (tr != null)
		{
			tr.enabled = true;
			tr.emitting = true;
		}
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStart();
			}
		}
	}

	public void MeleeDamageEnd()
	{
		if (tr != null)
		{
			tr.emitting = false;
		}
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			foreach (SwingCheck2 swingCheck in array)
			{
				if ((bool)swingCheck)
				{
					swingCheck.DamageStop();
				}
			}
		}
		zmb.parryable = false;
	}

	public void Swing()
	{
		swinging = true;
		isFleeingPlayer = false;
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			nma.updateRotation = false;
			((Behaviour)(object)nma).enabled = false;
		}
		base.transform.LookAt(ToPlanePos(lastTargetData.position));
		currentProjectile = null;
		previousProjectile = null;
		if (difficulty >= 4 && eid.enemyType == EnemyType.Schism)
		{
			aiming = true;
			predictionLerp = 0f;
			predictionLerping = false;
			anim.SetFloat("AttackType", 0f);
		}
		else if (lastTargetData.position.y - 12f > base.transform.position.y || lastTargetData.position.y + 5f < base.transform.position.y)
		{
			anim.SetFloat("AttackType", 1f);
		}
		else
		{
			anim.SetFloat("AttackType", (float)((UnityEngine.Random.Range(0f, 1f) > 0.66f) ? 1 : 0));
		}
		if (!stationary && zmb.grounded && eid.enemyType == EnemyType.Soldier && difficulty >= 4)
		{
			MoveForward(25f);
			anim.Play("RollShoot", -1, 0f);
		}
		else
		{
			anim.SetTrigger("Swing");
		}
		coolDown = 99f;
	}

	public void SwingEnd()
	{
		swinging = false;
		aiming = false;
		if (zmb.grounded)
		{
			((Behaviour)(object)nma).enabled = true;
		}
		coolDown = UnityEngine.Random.Range(1f, 2.5f) - coolDownReduce;
		if (wanderer)
		{
			if (difficulty >= 4 && UnityEngine.Random.Range(0f, 1f) > 0.66f)
			{
				coolDown = 1f;
			}
			else
			{
				Wander();
				coolDown = Mathf.Max(UnityEngine.Random.Range(0.5f, 2f) - coolDownReduce, 0.5f);
			}
		}
		if (blocking)
		{
			coolDown = 0f;
		}
		blocking = false;
		moveForward = false;
		if (tr != null)
		{
			tr.enabled = false;
		}
	}

	public void SpawnProjectile()
	{
		if (base.gameObject.scene.isLoaded && swinging)
		{
			currentDecProjectile = UnityEngine.Object.Instantiate(decProjectile, decProjectileSpawner.transform.position, decProjectileSpawner.transform.rotation);
			currentDecProjectile.transform.SetParent(decProjectileSpawner.transform, worldPositionStays: true);
			currentDecProjectile.GetComponentInChildren<Breakable>().interruptEnemy = eid;
			eid.weakPoint = currentDecProjectile;
		}
	}

	public void DamageStart()
	{
		if (!hasMelee)
		{
			if (tr == null)
			{
				tr = GetComponentInChildren<TrailRenderer>();
			}
			if (tr != null)
			{
				tr.enabled = true;
			}
		}
		zmb.ParryableCheck();
		if (aimer != null && (eid.enemyType != EnemyType.Schism || difficulty >= 4))
		{
			origRotation = aimer.rotation;
			aiming = true;
		}
	}

	public void ThrowProjectile()
	{
		swinging = true;
		if (currentDecProjectile != null)
		{
			UnityEngine.Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		Vector3 vector = shootPos.position;
		Quaternion quaternion = base.transform.rotation;
		Vector3 direction = vector - VisionSourcePosition;
		PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, direction, lm, out var _, out var endPoint, out var traversals);
		bool flag = false;
		Vector3 vector2;
		if (targetHandle != null)
		{
			TargetData targetData = vision.CalculateData(targetHandle);
			flag = targetData.target.isPlayer;
			vector2 = (flag ? targetData.headPosition : targetData.position);
		}
		else
		{
			if (eid.target != null)
			{
				flag = eid.target.isPlayer;
			}
			vector2 = vector + base.transform.forward;
		}
		bool flag2 = false;
		if (traversals.Length != 0)
		{
			PortalHandle portalHandle = traversals[0].portalHandle;
			Portal portalObject = PortalUtils.GetPortalObject(portalHandle);
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
				vector2 = travelMatrix.MultiplyPoint3x4(vector2);
				vector = endPoint;
				quaternion = travelMatrix.rotation * quaternion;
			}
			else if (!portalObject.passThroughNonTraversals)
			{
				vector = portalObject.GetTransform(portalHandle.side).GetPositionInFront(traversals[0].entrancePoint, 0.05f);
				flag2 = true;
			}
		}
		currentProjectile = UnityEngine.Object.Instantiate(projectile, vector, quaternion);
		currentProjectile.transform.LookAt(vector2);
		ProjectileSpread componentInChildren = currentProjectile.GetComponentInChildren<ProjectileSpread>();
		if ((bool)componentInChildren)
		{
			componentInChildren.targetHandle = targetHandle;
			if (difficulty <= 2)
			{
				if (difficulty == 2)
				{
					componentInChildren.spreadAmount = 5f;
				}
				else if (difficulty == 1)
				{
					componentInChildren.spreadAmount = 3f;
				}
				else if (difficulty == 0)
				{
					componentInChildren.spreadAmount = 2f;
				}
				componentInChildren.projectileAmount = 3;
			}
		}
		Projectile componentInChildren2 = currentProjectile.GetComponentInChildren<Projectile>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.targetHandle = targetHandle;
			componentInChildren2.isTargetPlayer = flag;
			componentInChildren2.safeEnemyType = EnemyType.Stray;
			if (difficulty > 2)
			{
				componentInChildren2.speed *= 1.35f;
			}
			else if (difficulty == 1)
			{
				componentInChildren2.speed *= 0.75f;
			}
			else if (difficulty == 0)
			{
				componentInChildren2.speed *= 0.5f;
			}
			componentInChildren2.damage *= eid.totalDamageModifier;
			if (flag2)
			{
				componentInChildren2.Explode();
			}
		}
	}

	public void ShootProjectile(int skipOnEasy)
	{
		if (skipOnEasy > 0 && difficulty < 2)
		{
			return;
		}
		swinging = true;
		if (difficulty >= 4 && eid.enemyType == EnemyType.Schism && !predictionLerping && targetHandle != null)
		{
			predictedPosition = (eid.target.isPlayer ? MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(4f, aimAtHead: true, ignoreCollision: true) : lastTargetData.headPosition);
			if (eid.target.isPlayer)
			{
				predictedPosition.y = lastTargetData.headPosition.y;
			}
			predictionLerp = 0f;
			predictionLerping = true;
		}
		if (currentDecProjectile != null)
		{
			UnityEngine.Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		if ((bool)currentProjectile)
		{
			previousProjectile = currentProjectile;
		}
		currentProjectile = UnityEngine.Object.Instantiate(projectile, decProjectileSpawner.transform.position, decProjectileSpawner.transform.rotation);
		Projectile component = currentProjectile.GetComponent<Projectile>();
		component.safeEnemyType = EnemyType.Schism;
		component.targetHandle = targetHandle;
		component.isTargetPlayer = lastTargetData.target.isPlayer;
		if (difficulty > 2)
		{
			component.speed *= 1.25f;
		}
		else if (difficulty == 1)
		{
			component.speed *= 0.75f;
		}
		else if (difficulty == 0)
		{
			component.speed *= 0.5f;
		}
		component.damage *= eid.totalDamageModifier;
		if (projectileBeam != null && (bool)previousProjectile && difficulty > 1)
		{
			ContinuousBeam continuousBeam = UnityEngine.Object.Instantiate(projectileBeam, currentProjectile.transform.position, currentProjectile.transform.rotation, currentProjectile.transform);
			continuousBeam.useProjectileRef = true;
			continuousBeam.projectile = component;
			component.connectedBeams.Add(continuousBeam);
			if (previousProjectile.TryGetComponent<Projectile>(out var component2))
			{
				component2.connectedBeams.Add(continuousBeam);
				continuousBeam.endProjectile = component2;
			}
		}
	}

	public void StopTracking()
	{
	}

	public void DamageEnd()
	{
		if (!hasMelee && tr != null)
		{
			tr.enabled = false;
		}
		if (currentDecProjectile != null)
		{
			UnityEngine.Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		zmb.parryable = false;
		moveForward = false;
		if (aimer != null)
		{
			aimEase = 0f;
			aiming = false;
		}
	}

	public void CancelAttack()
	{
		swinging = false;
		blocking = false;
		aiming = false;
		coolDown = 0f;
		moveForward = false;
		if (currentDecProjectile != null)
		{
			UnityEngine.Object.Destroy(currentDecProjectile);
			eid.weakPoint = origWP;
		}
		if (tr != null)
		{
			tr.enabled = false;
		}
		zmb.parryable = false;
	}

	private void Wander()
	{
		if (!hasVision || !(UnityEngine.Object)(object)nma || !((Behaviour)(object)nma).enabled || !nma.isOnNavMesh)
		{
			return;
		}
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere.y = 0f;
		onUnitSphere = onUnitSphere.normalized;
		PhysicsCastResult hitInfo;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		bool flag = PortalPhysicsV2.Raycast(base.transform.position + Vector3.up, onUnitSphere, 15f, lm, out hitInfo, out portalTraversals, out endPoint);
		wanderTarget = endPoint;
		if (portalTraversals.Length != 0)
		{
			if (flag)
			{
				wanderTarget = hitInfo.point + hitInfo.normal * bodySize;
			}
			for (int i = 0; i < portalTraversals.Length; i++)
			{
				PortalHandle portalHandle = portalTraversals[i].portalHandle;
				Portal portalObject = PortalUtils.GetPortalObject(portalHandle);
				if (!portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.Enemy))
				{
					wanderTarget = portalObject.GetTransform(portalHandle.side).GetPositionInFront(portalTraversals[i].entrancePoint, bodySize);
					break;
				}
			}
		}
		else if (flag)
		{
			wanderTarget = hitInfo.point + hitInfo.normal * bodySize - onUnitSphere * bodySize;
		}
		else
		{
			wanderTarget = hitInfo.point - onUnitSphere * bodySize;
		}
		NavMeshHit val = default(NavMeshHit);
		if ((Physics.Raycast(wanderTarget, Vector3.down, out var hitInfo2, 15f, lm) && NavMesh.SamplePosition(hitInfo2.point, ref val, 0.5f, 1)) || NavMesh.SamplePosition(wanderTarget, ref val, 15f, 1))
		{
			isWandering = true;
			wanderTarget = ((NavMeshHit)(ref val)).position;
			nma.SetDestination(wanderTarget);
		}
		else
		{
			isWandering = false;
		}
	}

	public void Block(Vector3 attackPosition)
	{
		if (swinging)
		{
			CancelAttack();
		}
		swinging = true;
		blocking = true;
		aiming = false;
		isFleeingPlayer = false;
		nma.updateRotation = false;
		base.transform.LookAt(ToPlanePos(attackPosition));
		zmb.KnockBack(base.transform.forward * -1f * 500f);
		UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, base.transform.position, Quaternion.identity);
		((Behaviour)(object)nma).enabled = false;
		anim.Play("Block", -1, 0f);
	}
}
