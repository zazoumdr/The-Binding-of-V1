using System;
using Sandbox;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class StatueBoss : EnemyScript, IEnrage, IAlter, IAlterOptions<bool>
{
	private Animator anim;

	private NavMeshAgent nma;

	private NavMeshPath nmp;

	private CameraController cc;

	private Rigidbody rb;

	[HideInInspector]
	public bool inAction;

	public bool stationary;

	public Transform stompPos;

	public AssetReference stompWave;

	private bool tracking;

	private bool dashing;

	private float dashPower;

	private GameObject currentStompWave;

	private float meleeRecharge = 1f;

	private float playerInCloseRange;

	private bool dontFall;

	[HideInInspector]
	public bool damaging;

	[HideInInspector]
	public bool launching;

	[HideInInspector]
	public int damage;

	private int tackleChance = 50;

	private int extraTackles;

	private float rangedRecharge = 1f;

	private int throwChance = 50;

	public float attackCheckCooldown = 1f;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	private VisionQuery targetQuery;

	private Vector3 targetPlanePos;

	private Vector3 predictedRealTargetPosition;

	private Vector3 predictedTargetPosition;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	public AssetReference orbProjectile;

	private Light orbLight;

	private bool orbGrowing;

	public GameObject stepSound;

	private ParticleSystem part;

	private AudioSource partAud;

	private Enemy mach;

	public GameObject statueChargeSound;

	public GameObject statueChargeSound2;

	public GameObject statueChargeSound3;

	public bool enraged;

	public GameObject enrageEffect;

	public GameObject currentEnrageEffect;

	private EnemySimplifier[] ensims;

	private int difficulty = -1;

	private SwingCheck2 swingCheck;

	private GroundCheckEnemy gc;

	private EnemyIdentifier eid;

	private Collider enemyCollider;

	private float originalLightRange;

	private float originalNmaRange;

	private float originalNmaSpeed;

	private float originalNmaAcceleration;

	private float originalNmaAngularSpeed;

	private float realSpeedModifier;

	private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");

	private EnemyTarget target => eid.target;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => mach.chest.transform.position;

	private bool hasAttackTarget => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	public bool isEnraged => enraged;

	public string alterKey => "statue";

	public string alterCategoryName => "statue";

	public AlterOption<bool>[] options => new AlterOption<bool>[1]
	{
		new AlterOption<bool>
		{
			value = enraged,
			callback = delegate(bool value)
			{
				if (value)
				{
					Enrage();
				}
				else
				{
					UnEnrage();
				}
			},
			key = "enraged",
			name = "Enraged"
		}
	};

	public override void OnGoLimp(bool fromExplosion)
	{
		mach.anim.StopPlayback();
		SwingCheck2[] componentsInChildren = mach.GetComponentsInChildren<SwingCheck2>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		StatueBoss[] componentsInChildren2 = GoreZone.ResolveGoreZone(mach.transform).GetComponentsInChildren<StatueBoss>();
		foreach (StatueBoss statueBoss in componentsInChildren2)
		{
			if (statueBoss != this)
			{
				statueBoss.EnrageDelayed();
			}
		}
		ForceStopDashSound();
		if (currentEnrageEffect != null)
		{
			UnityEngine.Object.Destroy(currentEnrageEffect);
		}
		UnityEngine.Object.Destroy(this);
	}

	private void Awake()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		rb = GetComponent<Rigidbody>();
		part = base.transform.Find("DodgeParticle").GetComponent<ParticleSystem>();
		partAud = ((Component)(object)part).GetComponent<AudioSource>();
		mach = GetComponent<Enemy>();
		nma = GetComponentInChildren<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		gc = GetComponentInChildren<GroundCheckEnemy>();
		enemyCollider = GetComponent<Collider>();
		orbLight = GetComponentInChildren<Light>();
		originalLightRange = orbLight.range;
		originalNmaRange = nma.stoppingDistance;
		originalNmaSpeed = nma.speed;
		originalNmaAcceleration = nma.acceleration;
		originalNmaAngularSpeed = nma.angularSpeed;
		nmp = new NavMeshPath();
	}

	private void Start()
	{
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		cc = MonoSingleton<CameraController>.Instance;
		SetSpeed();
		if (inAction)
		{
			StopAction();
		}
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.Environment)));
		SlowUpdate();
	}

	private void OnEnable()
	{
		if ((bool)mach)
		{
			StopAction();
			StopDamage();
			StopDash();
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnDisable()
	{
		if (currentStompWave != null)
		{
			UnityEngine.Object.Destroy(currentStompWave);
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		targetHandle?.From(details.portalSequence);
		targetPlanePos = details.enterToExit.MultiplyPoint3x4(targetPlanePos);
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
			targetPlanePos = details.enterToExit.MultiplyPoint3x4(targetPlanePos);
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(UnityEngine.Object)(object)nma)
		{
			nma = GetComponentInChildren<NavMeshAgent>();
		}
		if (!(UnityEngine.Object)(object)anim)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			anim.speed = 1.35f;
			break;
		case 3:
			anim.speed = 1.2f;
			break;
		case 2:
			anim.speed = 1f;
			break;
		case 1:
			anim.speed = 0.8f;
			break;
		case 0:
			anim.speed = 0.6f;
			break;
		}
		realSpeedModifier = eid.totalSpeedModifier;
		if (difficulty == 4 && eid.totalSpeedModifier > 1.2f)
		{
			realSpeedModifier -= 0.2f;
		}
		Animator obj = anim;
		obj.speed *= realSpeedModifier;
		if (enraged)
		{
			if (difficulty > 3)
			{
				anim.speed = 1.5f * realSpeedModifier;
			}
			else if (difficulty == 3)
			{
				anim.speed = 1.25f * realSpeedModifier;
			}
			else
			{
				Animator obj2 = anim;
				obj2.speed *= 1.2f;
			}
			anim.SetFloat("WalkSpeed", 1.5f);
		}
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			nma.speed = (enraged ? (originalNmaSpeed * 5f) : originalNmaSpeed) * realSpeedModifier;
			nma.acceleration = (float)(enraged ? 120 : 24) * realSpeedModifier;
			nma.angularSpeed = (float)(enraged ? 6000 : 1200) * realSpeedModifier;
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		SetSpeed();
		return new EnemyMovementData
		{
			speed = nma.speed,
			angularSpeed = nma.angularSpeed,
			acceleration = nma.acceleration
		};
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", GetUpdateRate(nma));
		if (stationary)
		{
			return;
		}
		if (target == null)
		{
			mach.SetDestination(base.transform.position);
		}
		else if (!inAction && nma.isOnNavMesh)
		{
			if (Vector3.Distance(targetPlanePos, base.transform.position) > 3f)
			{
				mach.SetDestination(hasDimensionalTarget ? EnemyTarget.GetNavPoint(lastDimensionalTarget) : target.GetNavPoint());
			}
			else
			{
				mach.SetDestination(base.transform.position);
			}
		}
	}

	private void Update()
	{
		if (target == null)
		{
			StopAction();
			StopDamage();
			anim.SetBool("Walking", false);
			if (nma.isOnNavMesh && !nma.isStopped)
			{
				nma.isStopped = true;
			}
			return;
		}
		UpdateVision();
		if (!inAction)
		{
			if (nma.isOnNavMesh && Vector3.Distance(targetPlanePos, base.transform.position) <= 3f)
			{
				base.transform.LookAt(targetPlanePos);
			}
			if (((Behaviour)(object)nma).enabled)
			{
				anim.SetBool("Walking", nma.velocity.magnitude > 1f || mach.isTraversingPortalLink);
			}
		}
		if (orbGrowing)
		{
			orbLight.range = Mathf.MoveTowards(orbLight.range, originalLightRange, Time.deltaTime * 20f * realSpeedModifier);
			if (orbLight.range == originalLightRange)
			{
				orbGrowing = false;
			}
		}
		AttackCheck();
		AttackCooldowns();
	}

	private void UpdateVision()
	{
		if (vision.TrySee(targetQuery, out var data))
		{
			targetHandle = lastTargetData.handle;
			lastTargetData = data.ToData();
		}
		else
		{
			targetHandle = null;
		}
		lastDimensionalTarget = Vector3.zero;
		if (targetHandle != null)
		{
			targetPlanePos = ToPlanePos(lastTargetData.position);
		}
		else if (mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget))
		{
			targetPlanePos = ToPlanePos(lastDimensionalTarget);
		}
		else
		{
			targetPlanePos = ToPlanePos(eid.target.position);
		}
	}

	private void UpdateAttackVision()
	{
		if (vision.TrySee(targetQuery, out var data))
		{
			targetHandle = data.CreateHandle();
			lastTargetData = data.ToData();
			targetPlanePos = ToPlanePos(data.position);
		}
	}

	private void AttackCheck()
	{
		if (attackCheckCooldown > 0f)
		{
			attackCheckCooldown = Mathf.MoveTowards(attackCheckCooldown, 0f, Time.deltaTime);
		}
		else
		{
			if (inAction || !gc.onGround)
			{
				return;
			}
			attackCheckCooldown = 0.2f;
			if (!CheckAndSetAttackVision())
			{
				return;
			}
			bool flag = target != null && target.isPlayer && Vector3.Dot(base.transform.up * -1f, MonoSingleton<NewMovement>.Instance.rb.GetGravityDirection()) < 0.5f && lastTargetData.position.y - base.transform.position.y > 3f;
			if (!flag && (meleeRecharge >= 2f || IsEarlyMeleeOk()))
			{
				meleeRecharge = 0f;
				if (stationary || (target.position.y < base.transform.position.y + 5f && UnityEngine.Random.Range(0, 100) > tackleChance))
				{
					if (tackleChance < 50)
					{
						tackleChance = 50;
					}
					tackleChance += 20;
					inAction = true;
					Stomp();
				}
				else
				{
					if (tackleChance > 50)
					{
						tackleChance = 50;
					}
					tackleChance -= 20;
					inAction = true;
					Tackle();
				}
			}
			else if (rangedRecharge >= 1f && (Vector3.Distance(base.transform.position, targetPlanePos) >= 9f || flag))
			{
				Debug.Log("Here");
				rangedRecharge = 0f;
				inAction = true;
				Throw();
			}
		}
	}

	private bool CheckAndSetAttackVision()
	{
		if (hasAttackTarget)
		{
			targetPlanePos = ToPlanePos(lastTargetData.position);
			predictedRealTargetPosition = lastTargetData.realPosition;
			predictedTargetPosition = lastTargetData.position;
			return true;
		}
		return false;
	}

	private bool IsEarlyMeleeOk()
	{
		if (meleeRecharge < 1f)
		{
			return false;
		}
		if (Vector3.Distance(base.transform.position, targetPlanePos) >= 15f)
		{
			return false;
		}
		if (!(Mathf.Abs(base.transform.position.y - lastTargetData.position.y) < 9f))
		{
			if (Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y) > 2f)
			{
				return Mathf.Abs(base.transform.position.y - lastTargetData.position.y) < 19f;
			}
			return false;
		}
		return true;
	}

	private void AttackCooldowns()
	{
		if (rangedRecharge < 1f)
		{
			float num = 1f;
			if (Vector3.Distance(base.transform.position, targetPlanePos) < 15f)
			{
				num = 0.5f;
			}
			if (difficulty >= 4)
			{
				num += 0.5f;
			}
			else if (difficulty == 1)
			{
				num -= 0.2f;
			}
			else if (difficulty == 0)
			{
				num -= 0.35f;
			}
			num *= realSpeedModifier;
			num = (enraged ? (num * 0.4f) : ((mach.health < 60f) ? (num * 0.15f) : ((difficulty >= 4) ? (num * 0.32f) : ((difficulty != 3) ? (num * 0.275f) : (num * 0.285f)))));
			rangedRecharge = Mathf.MoveTowards(rangedRecharge, 1f, Time.deltaTime * num);
		}
		if (!(meleeRecharge < 1f))
		{
			return;
		}
		if (enraged)
		{
			if (difficulty >= 2)
			{
				meleeRecharge = 1f;
			}
			else
			{
				meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * 0.4f);
			}
			return;
		}
		float num2 = 1f;
		if (Vector3.Distance(base.transform.position, targetPlanePos) < 9f)
		{
			playerInCloseRange = Mathf.MoveTowards(playerInCloseRange, 1f, Time.deltaTime);
			if (playerInCloseRange >= 1f)
			{
				num2 = 2f;
			}
		}
		else
		{
			playerInCloseRange = Mathf.MoveTowards(playerInCloseRange, 0f, Time.deltaTime);
		}
		if (difficulty >= 4)
		{
			num2 += 0.5f;
		}
		else if (difficulty == 1)
		{
			num2 -= 0.25f;
		}
		else if (difficulty == 0)
		{
			num2 -= 0.5f;
		}
		num2 *= realSpeedModifier;
		num2 = ((mach.health < 60f) ? (num2 * 0.25f) : ((difficulty >= 4) ? (num2 * 0.4f) : ((difficulty != 3) ? (num2 * 0.375f) : (num2 * 0.385f))));
		meleeRecharge = Mathf.MoveTowards(meleeRecharge, 2f, Time.deltaTime * num2);
	}

	private void FixedUpdate()
	{
		if (dashPower == 0f)
		{
			if (dontFall)
			{
				rb.velocity = Vector3.zero;
			}
			return;
		}
		float num = 1f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			if (difficulty >= 4)
			{
				num = 1.25f;
			}
			dashPower /= 1.075f;
			break;
		case 2:
			num = 0.8f;
			dashPower /= 1.065625f;
			break;
		case 1:
			num = 0.666f;
			dashPower /= 1.05625f;
			break;
		case 0:
			num = 0.5f;
			dashPower /= 1.0375f;
			break;
		}
		if (enraged || mach.IsLedgeSafe())
		{
			num *= dashPower * realSpeedModifier;
			rb.velocity = new Vector3(base.transform.forward.x * num, rb.velocity.y, base.transform.forward.z * num);
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		if (rb.velocity.y > 0f || dontFall)
		{
			rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		}
		if (dashPower < 1f)
		{
			rb.velocity = Vector3.zero;
			dashPower = 0f;
			damaging = false;
		}
	}

	private void AttackPrep()
	{
		nma.updatePosition = false;
		nma.updateRotation = false;
		((Behaviour)(object)nma).enabled = false;
		base.transform.LookAt(targetPlanePos);
	}

	private void Stomp()
	{
		if (target != null)
		{
			AttackPrep();
			anim.SetTrigger("Stomp");
			launching = false;
			UnityEngine.Object.Instantiate(statueChargeSound, base.transform.position, Quaternion.identity);
		}
	}

	private void Tackle()
	{
		if (target != null)
		{
			AttackPrep();
			tracking = true;
			anim.SetTrigger("Tackle");
			if (difficulty >= 4)
			{
				extraTackles = 1;
			}
			damage = 25;
			launching = true;
			UnityEngine.Object.Instantiate(statueChargeSound3, base.transform.position, Quaternion.identity);
		}
	}

	private void Throw()
	{
		if (target != null)
		{
			AttackPrep();
			tracking = true;
			anim.SetTrigger("Throw");
			UnityEngine.Object.Instantiate(statueChargeSound2, base.transform.position, Quaternion.identity);
		}
	}

	public void StompHit()
	{
		cc.CameraShake(1f);
		if (currentStompWave != null)
		{
			UnityEngine.Object.Destroy(currentStompWave);
		}
		int num = 1;
		if (difficulty == 4)
		{
			num = 2;
		}
		if (difficulty == 5)
		{
			num = 3;
		}
		Vector3 vector = base.transform.position + base.transform.up;
		Vector3 vector2 = ToPlanePos(stompPos.position);
		Vector3 direction = vector2 - vector;
		PortalPhysicsV2.ProjectThroughPortals(vector, direction, default(LayerMask), out var _, out var endPoint, out var traversals);
		Vector3 position = vector2;
		bool flag = false;
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				position = endPoint;
			}
			else if (!portalObject.passThroughNonTraversals)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			currentStompWave = UnityEngine.Object.Instantiate(stompWave.ToAsset(), position, Quaternion.identity);
			PhysicalShockwave component = currentStompWave.GetComponent<PhysicalShockwave>();
			switch (difficulty)
			{
			case 4:
			case 5:
				component.speed = 75f;
				break;
			case 3:
				component.speed = 50f;
				break;
			case 2:
				component.speed = 35f;
				break;
			case 1:
				component.speed = 25f;
				break;
			case 0:
				component.speed = 15f;
				break;
			}
			if (i != 0)
			{
				component.speed /= 1 + i * 2;
				if (component.TryGetComponent<AudioSource>(out var component2))
				{
					((Behaviour)(object)component2).enabled = false;
				}
			}
			component.damage = Mathf.RoundToInt(25f * eid.totalDamageModifier);
			component.maxSize = 100f;
			component.enemy = true;
			component.enemyType = EnemyType.Cerberus;
		}
	}

	public void OrbSpawn()
	{
		Vector3 vector = base.transform.position + Vector3.up * 3.5f;
		Vector3 direction = ToPlanePos(orbLight.transform.position) + Vector3.up * 3.5f - vector;
		PortalPhysicsV2.ProjectThroughPortals(vector, direction, default(LayerMask), out var _, out var endPoint, out var traversals);
		Vector3 position = endPoint;
		bool flag = false;
		Vector3 vector2 = predictedTargetPosition;
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				vector2 = PortalUtils.GetTravelMatrix(traversals).MultiplyPoint3x4(vector2);
			}
			else if (!portalObject.passThroughNonTraversals)
			{
				position = portalObject.GetTransform(portalHandle.side).GetPositionInFront(traversals[0].entrancePoint, 0.01f);
				flag = true;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(orbProjectile.ToAsset(), position, Quaternion.identity);
		gameObject.transform.LookAt(vector2);
		if (gameObject.TryGetComponent<Rigidbody>(out var component))
		{
			float num = 10000f;
			if (difficulty > 2)
			{
				num = 20000f;
			}
			else if (difficulty == 2)
			{
				num = 15000f;
			}
			component.AddForce(gameObject.transform.forward * num);
		}
		if (gameObject.TryGetComponent<Projectile>(out var component2))
		{
			component2.target = eid.target;
			component2.damage *= eid.totalDamageModifier;
			if (difficulty <= 2)
			{
				component2.bigExplosion = false;
			}
			if (flag)
			{
				component2.Explode();
			}
		}
		orbGrowing = false;
		orbLight.range = 0f;
		part.Play();
	}

	public void OrbRespawn()
	{
		orbGrowing = true;
	}

	public void StopAction()
	{
		if (gc.onGround)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			((Behaviour)(object)nma).enabled = true;
		}
		tracking = false;
		inAction = false;
	}

	public void StopTracking()
	{
		tracking = false;
		if (target == null)
		{
			return;
		}
		UpdateAttackVision();
		Vector3 velocity = eid.target.GetVelocity();
		if (velocity.magnitude == 0f)
		{
			predictedRealTargetPosition = lastTargetData.realPosition;
			predictedTargetPosition = lastTargetData.position;
		}
		else
		{
			Vector3 realPosition = lastTargetData.realPosition;
			Vector3 vector = velocity * 0.35f / realSpeedModifier;
			if (Physics.Raycast(realPosition, velocity.normalized, out var hitInfo, vector.magnitude, 4096, QueryTriggerInteraction.Collide) && hitInfo.collider == enemyCollider)
			{
				predictedRealTargetPosition = realPosition;
			}
			else if (Physics.Raycast(realPosition, velocity.normalized, out hitInfo, vector.magnitude, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
			{
				predictedRealTargetPosition = hitInfo.point;
			}
			else
			{
				predictedRealTargetPosition = realPosition + vector;
				predictedRealTargetPosition = new Vector3(predictedRealTargetPosition.x, realPosition.y + (realPosition.y - predictedRealTargetPosition.y) * 0.5f, predictedRealTargetPosition.z);
			}
			predictedTargetPosition = TransformAcrossPortals(predictedRealTargetPosition, lastTargetData.handle.portals);
		}
		if (!lastTargetData.handle.portals.IsEmpty)
		{
			base.transform.LookAt(ToPlanePos(predictedTargetPosition));
		}
		else
		{
			base.transform.LookAt(ToPlanePos(predictedRealTargetPosition));
		}
	}

	private static Vector3 TransformAcrossPortals(Vector3 point, PortalHandleSequence portals)
	{
		if (portals.IsEmpty)
		{
			return point;
		}
		return PortalUtils.GetTravelMatrix(portals).MultiplyPoint3x4(point);
	}

	public void Dash()
	{
		if (difficulty >= 4)
		{
			dontFall = true;
		}
		rb.isKinematic = false;
		rb.velocity = Vector3.zero;
		dashPower = 200f;
		damaging = true;
		part.Play();
		partAud.Play(tracked: true);
		StartDamage();
	}

	public void StopDash()
	{
		dashPower = 0f;
		if (gc.onGround)
		{
			rb.isKinematic = true;
		}
		else
		{
			rb.velocity = Vector3.zero;
		}
		damaging = false;
		partAud.Stop();
		StopDamage();
		if (extraTackles > 0)
		{
			dontFall = true;
			extraTackles--;
			tracking = true;
			anim.speed = 0.1f;
			GameObject obj = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, eid.weakPoint.transform.position + base.transform.forward * 1.5f, base.transform.rotation);
			obj.transform.localScale *= 5f;
			obj.transform.SetParent(eid.weakPoint.transform, worldPositionStays: true);
			anim.Play("Tackle", -1, 0.4f);
			Invoke("DelayedTackle", 0.5f / realSpeedModifier);
		}
		else
		{
			dontFall = false;
		}
	}

	private void DelayedTackle()
	{
		dontFall = false;
		SetSpeed();
		StopTracking();
	}

	public void ForceStopDashSound()
	{
		partAud.Stop();
	}

	public void StartDamage()
	{
		damaging = true;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.damage = damage;
		swingCheck.DamageStart();
	}

	public void StopDamage()
	{
		damaging = false;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.DamageStop();
	}

	public void Step()
	{
		UnityEngine.Object.Instantiate(stepSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>().SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
	}

	public void EnrageDelayed()
	{
		if (!enraged)
		{
			Invoke("Enrage", 1f / (eid ? realSpeedModifier : 1f));
		}
	}

	public void UnEnrage()
	{
		if (!eid.dead && enraged)
		{
			enraged = false;
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			if (currentEnrageEffect != null)
			{
				UnityEngine.Object.Destroy(currentEnrageEffect);
			}
			if (difficulty <= 2)
			{
				Animator obj = anim;
				obj.speed /= 1.2f;
			}
			else if (difficulty > 3)
			{
				anim.speed = 1.5f * realSpeedModifier;
			}
			else
			{
				anim.speed = 1.25f * realSpeedModifier;
			}
			orbLight.range = originalLightRange;
			nma.stoppingDistance = originalNmaRange;
			nma.speed = originalNmaSpeed * realSpeedModifier;
			nma.angularSpeed = originalNmaAngularSpeed * realSpeedModifier;
			nma.acceleration = originalNmaAcceleration * realSpeedModifier;
		}
	}

	public void Enrage()
	{
		if (!eid.dead && !enraged && !eid.puppet)
		{
			enraged = true;
			CancelInvoke("Enrage");
			GameObject obj = UnityEngine.Object.Instantiate(statueChargeSound2, base.transform.position, Quaternion.identity);
			obj.GetComponent<AudioSource>().SetPitch(0.3f);
			obj.GetComponent<AudioDistortionFilter>().distortionLevel = 0.5f;
			if (difficulty <= 2)
			{
				Animator obj2 = anim;
				obj2.speed *= 1.2f;
			}
			else if (difficulty > 3)
			{
				anim.speed = 1.5f * realSpeedModifier;
			}
			else
			{
				anim.speed = 1.25f * realSpeedModifier;
			}
			orbLight.range *= 2f;
			originalLightRange *= 2f;
			nma.speed = 25f * realSpeedModifier;
			nma.acceleration = 120f * realSpeedModifier;
			nma.angularSpeed = 6000f * realSpeedModifier;
			anim.SetFloat(WalkSpeed, 1.5f);
			currentEnrageEffect = UnityEngine.Object.Instantiate(enrageEffect, mach.chest.transform);
			currentEnrageEffect.transform.localScale = Vector3.one * 0.04f;
			currentEnrageEffect.transform.localPosition = new Vector3(0f, 0.025f, 0f);
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>(includeInactive: true);
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = true;
			}
		}
	}
}
