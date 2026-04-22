using System;
using System.Collections.Generic;
using plog;
using Sandbox;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class SwordsMachine : EnemyScript, IEnrage, IAlter, IAlterOptions<bool>, IEnemyRelationshipLogic
{
	private static readonly Logger Log = new Logger("SwordsMachine");

	public Transform targetZone;

	private NavMeshAgent nma;

	private Animator anim;

	private Rigidbody rb;

	private Enemy mach;

	public float phaseChangeHealth;

	public bool firstPhase;

	public bool active = true;

	public Transform rightArm;

	[SerializeField]
	private Transform[] aimBones;

	private float aimLerp;

	public bool inAction;

	public bool inSemiAction;

	[HideInInspector]
	public bool moveAtTarget;

	private Vector3 moveDirection;

	private float moveSpeed;

	public TrailRenderer swordTrail;

	public TrailRenderer slapTrail;

	public SkinnedMeshRenderer swordMR;

	public Material enragedSword;

	public Material heatMat;

	private Material origMat;

	private AudioSource swordAud;

	public GameObject swingSound;

	public GameObject head;

	public AssetReference flash;

	public AssetReference gunFlash;

	public float runningAttackCooldown;

	public bool damaging;

	public int damage;

	public float runningAttackChance = 50f;

	private EnemyShotgun shotgun;

	private bool shotgunning;

	private bool gunDelay;

	public GameObject shotgunPickUp;

	public GameObject activateOnPhaseChange;

	private bool usingShotgun;

	public Transform secondPhasePosTarget;

	[SerializeField]
	private GameObject teleportEffect;

	public CheckPoint cpToReset;

	public float swordThrowCharge = 3f;

	public int throwType;

	public GameObject[] thrownSword;

	private GameObject currentThrownSword;

	public Transform handTransform;

	private float swordThrowChance = 50f;

	private float spiralSwordChance = 50f;

	private bool waitingForSword;

	public GameObject bigPainSound;

	private Vector3 targetFuturePos;

	private int difficulty = -1;

	public bool enraged;

	private float rageLeft;

	public EnemySimplifier ensim;

	private float normalAnimSpeed;

	private float normalMovSpeed;

	public GameObject enrageEffect;

	public GameObject currentEnrageEffect;

	private AudioSource enrageAud;

	public Door[] doorsInPath;

	public bool eternalRage;

	public bool bothPhases;

	private bool knockedDown;

	public bool downed;

	[SerializeField]
	private SwingCheck2[] swordSwingCheck;

	[SerializeField]
	private SwingCheck2 slapSwingCheck;

	private GroundCheckEnemy gc;

	private bool bossVersion;

	private EnemyIdentifier eid;

	private BloodsplatterManager bsm;

	private float idleFailsafe = 1f;

	private bool idling;

	private bool inPhaseChange;

	private float moveSpeedMultiplier = 1f;

	private bool breakableInWay;

	private bool targetViewBlocked;

	private bool targetingStalker;

	public float spawnAttackDelay = 0.5f;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private RaycastHit breakable1Result;

	private RaycastHit breakable2Result;

	private VisionQuery targetQuery;

	private VisionQuery breakable1Query;

	private VisionQuery breakable2Query;

	private EnemyTarget target => eid.target;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => mach.chest.transform.position;

	private bool hasVision => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private Vector3 targetPosition
	{
		get
		{
			if (!hasVision)
			{
				return target.position;
			}
			return lastTargetData.position;
		}
	}

	private Vector3 targetVelocity
	{
		get
		{
			if (!hasVision)
			{
				return target.GetVelocity();
			}
			return lastTargetData.velocity;
		}
	}

	public bool isEnraged => enraged;

	public string alterKey => "swordsmachine";

	public string alterCategoryName => "swordsmachine";

	AlterOption<bool>[] IAlterOptions<bool>.options => new AlterOption<bool>[2]
	{
		new AlterOption<bool>
		{
			value = isEnraged,
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
		},
		new AlterOption<bool>
		{
			value = eternalRage,
			callback = delegate(bool value)
			{
				eternalRage = value;
			},
			key = "eternal-rage",
			name = "Eternal Rage"
		}
	};

	public override bool ShouldKnockback(ref DamageData data)
	{
		return false;
	}

	public override void OnParry(ref DamageData data, bool isShotgun)
	{
		if (mach.health > 0f)
		{
			if (!enraged)
			{
				Knockdown(data.fromExplosion);
			}
			else
			{
				Enrage();
			}
		}
		else if (bigPainSound != null)
		{
			UnityEngine.Object.Instantiate(bigPainSound, base.transform);
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		mach.anim.StopPlayback();
		SwingCheck2[] componentsInChildren = mach.GetComponentsInChildren<SwingCheck2>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		CoolSword();
		if (currentEnrageEffect != null)
		{
			UnityEngine.Object.Destroy(currentEnrageEffect);
		}
		UnityEngine.Object.Destroy(this);
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		int num = ((!firstPhase) ? ((difficulty >= 3) ? 23 : ((difficulty == 2) ? 20 : 18)) : ((difficulty >= 3) ? 19 : ((difficulty == 2) ? 16 : 14)));
		return new EnemyMovementData
		{
			speed = num,
			angularSpeed = 1200f,
			acceleration = 160f
		};
	}

	public override void OnLand()
	{
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		mach = GetComponent<Enemy>();
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		swordAud = swordTrail.GetComponent<AudioSource>();
		shotgun = GetComponentInChildren<EnemyShotgun>();
		gc = GetComponentInChildren<GroundCheckEnemy>();
		origMat = swordMR.sharedMaterial;
	}

	private void Start()
	{
		swordTrail.emitting = false;
		slapTrail.emitting = false;
		SetSpeed();
		gunDelay = true;
		BossHealthBar component = GetComponent<BossHealthBar>();
		if (component == null || !component.enabled)
		{
			bossVersion = false;
		}
		else
		{
			bossVersion = true;
		}
		LayerMask lm = LayerMaskDefaults.Get(LMD.Environment);
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, lm));
		breakable1Query = new VisionQuery("Breakable1", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(base.transform.position + Vector3.up * 0.1f, lm, toHead: false, out breakable1Result));
		breakable2Query = new VisionQuery("Breakable2", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(t.position + Vector3.down * 5f, lm, toHead: false, out breakable2Result));
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.5f));
		Invoke("NavigationUpdate", 0.1f);
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	public void SetSpeed()
	{
		if (!(UnityEngine.Object)(object)nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!(UnityEngine.Object)(object)anim)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (!ensim)
		{
			ensim = GetComponentInChildren<EnemySimplifier>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			nma.speed = (firstPhase ? 19 : 23);
			anim.speed = 1.2f;
			anim.SetFloat("ThrowSpeedMultiplier", 1.35f);
			anim.SetFloat("AttackSpeedMultiplier", 1f);
			moveSpeedMultiplier = ((difficulty == 3) ? 1.2f : 1.35f);
			break;
		case 2:
			nma.speed = (firstPhase ? 16 : 20);
			anim.speed = 1f;
			anim.SetFloat("ThrowSpeedMultiplier", 1f);
			anim.SetFloat("AttackSpeedMultiplier", 1f);
			moveSpeedMultiplier = 1f;
			break;
		case 0:
		case 1:
			nma.speed = (firstPhase ? 14 : 18);
			anim.speed = 0.85f;
			anim.SetFloat("ThrowSpeedMultiplier", (difficulty == 1) ? 0.825f : 0.75f);
			anim.SetFloat("AttackSpeedMultiplier", (difficulty == 1) ? 0.825f : 0.75f);
			moveSpeedMultiplier = ((difficulty == 1) ? 0.8f : 0.65f);
			break;
		}
		if (difficulty >= 4)
		{
			anim.SetFloat("RecoverySpeedMultiplier", 2f);
		}
		else
		{
			anim.SetFloat("RecoverySpeedMultiplier", bossVersion ? 1f : 1.5f);
		}
		NavMeshAgent obj = nma;
		obj.speed *= eid.totalSpeedModifier;
		Animator obj2 = anim;
		obj2.speed *= eid.totalSpeedModifier;
		moveSpeedMultiplier *= eid.totalSpeedModifier;
		normalAnimSpeed = anim.speed;
		normalMovSpeed = nma.speed;
		if (enraged)
		{
			anim.speed = normalAnimSpeed * 1.15f;
			nma.speed = normalMovSpeed * 1.25f;
			ensim.enraged = true;
			if (!eid.puppet)
			{
				swordMR.sharedMaterial = enragedSword;
			}
		}
		if ((bool)shotgun)
		{
			shotgun.UpdateBuffs(eid);
		}
	}

	private void OnEnable()
	{
		if ((bool)mach)
		{
			StopAction();
			CoolSword();
			StopMoving();
			DamageStop();
			if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
			{
				PortalManagerV2 portalManagerV = instance;
				portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
			}
		}
	}

	private void OnDisable()
	{
		if (GetComponent<BossHealthBar>() != null)
		{
			GetComponent<BossHealthBar>().DisappearBar();
		}
		if (currentThrownSword != null)
		{
			UnityEngine.Object.Destroy(currentThrownSword);
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		moveDirection = details.enterToExit.MultiplyVector(moveDirection);
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
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.5f));
		CheckForStalkers();
		if (target != null)
		{
			CheckForBreakablesInWay();
		}
	}

	private void CheckForStalkers()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		targetingStalker = false;
		if (!BlindEnemies.Blind && nma.isOnNavMesh && !eid.sandified)
		{
			List<EnemyIdentifier> enemiesOfType = MonoSingleton<EnemyTracker>.Instance.GetEnemiesOfType(EnemyType.Stalker);
			if (enemiesOfType.Count > 0)
			{
				float num = 100f;
				foreach (EnemyIdentifier item in enemiesOfType)
				{
					if (item.blessed)
					{
						continue;
					}
					NavMeshPath val = new NavMeshPath();
					nma.CalculatePath(item.transform.position, val);
					if (val != null && (int)val.status == 0)
					{
						float num2 = 0f;
						for (int i = 1; i < val.corners.Length; i++)
						{
							num2 += Vector3.Distance(val.corners[i - 1], val.corners[i]);
						}
						if (!(num2 >= num))
						{
							eid.target = new EnemyTarget(item.transform);
							targetingStalker = true;
							num = num2;
						}
					}
				}
			}
		}
		if (targetingStalker && shotgunning)
		{
			CancelShotgunShot();
		}
	}

	private void CheckForBreakablesInWay()
	{
		if (!vision.TrySee(breakable1Query, out var data))
		{
			targetViewBlocked = true;
			if (Vector3.Distance(base.transform.position + Vector3.up * 0.1f, breakable1Result.point) < 5f && breakable1Result.transform != null && breakable1Result.transform.TryGetComponent<Breakable>(out var component) && !component.playerOnly)
			{
				breakableInWay = true;
			}
		}
		else
		{
			targetViewBlocked = false;
			if (data.position.y > base.transform.position.y + 2.5f && Vector2.Distance(new Vector2(base.transform.position.x, base.transform.position.z), new Vector3(data.position.x, data.position.z)) < 5f && !vision.TrySee(breakable2Query, out var _) && breakable2Result.transform != null && breakable2Result.transform.TryGetComponent<Breakable>(out var component2) && !component2.playerOnly)
			{
				breakableInWay = true;
			}
		}
	}

	private void NavigationUpdate()
	{
		Invoke("NavigationUpdate", 0.1f);
		lastDimensionalTarget = Vector3.zero;
		if (target != null && !inAction && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			if (mach.TryGetDimensionalTarget(target.position, out lastDimensionalTarget))
			{
				mach.SetDestination(lastDimensionalTarget);
			}
			else
			{
				mach.SetDestination(target.position);
			}
		}
	}

	private void Update()
	{
		UpdateRunningAnimation();
		EnrageUpdate();
		CheckPhases();
		IdleFailSafe();
		if (active && !((UnityEngine.Object)(object)nma == null))
		{
			UpdateVision();
			if (spawnAttackDelay > 0f)
			{
				spawnAttackDelay = Mathf.MoveTowards(spawnAttackDelay, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if (breakableInWay && !inAction)
			{
				breakableInWay = false;
				inAction = true;
				RunningSwing();
			}
			else
			{
				UpdateCooldowns();
				CheckToAttack();
			}
		}
	}

	private void UpdateCooldowns()
	{
		if (runningAttackCooldown > 0f)
		{
			runningAttackCooldown = Mathf.MoveTowards(runningAttackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (!firstPhase)
		{
			if (target != null && Vector3.Distance(base.transform.position, targetPosition) <= 5f)
			{
				swordThrowCharge = 0f;
			}
			else
			{
				swordThrowCharge = Mathf.MoveTowards(swordThrowCharge, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
		}
	}

	private void UpdateVision()
	{
		if (target != null && vision != null && vision.TrySee(targetQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
		}
		else
		{
			targetHandle = null;
		}
	}

	private void CheckToAttack()
	{
		if (inAction || hasDimensionalTarget || !hasVision)
		{
			return;
		}
		Vector3 a = targetPosition;
		float num = Vector3.Distance(a, base.transform.position);
		if ((!enraged || difficulty >= 4) && !targetingStalker)
		{
			if ((bool)shotgun && shotgun.gunReady && !gunDelay && !shotgunning && (firstPhase || bothPhases) && num > 5f)
			{
				shotgunning = true;
				anim.SetLayerWeight(1, 1f);
				anim.SetTrigger("Shoot");
				aimLerp = 0f;
			}
			else if (!firstPhase && !inSemiAction && !targetViewBlocked && (num > 20f || (swordThrowCharge == 0f && num > 10f)))
			{
				swordThrowCharge = 3f;
				if ((float)UnityEngine.Random.Range(0, 100) <= swordThrowChance || a.y > base.transform.position.y + 3f || num > 16f)
				{
					inAction = true;
					SwordThrow();
					if (swordThrowChance > 50f)
					{
						swordThrowChance = 25f;
					}
					else
					{
						swordThrowChance -= 25f;
					}
				}
				else if (swordThrowChance < 50f)
				{
					swordThrowChance = 75f;
				}
				else
				{
					swordThrowChance += 25f;
				}
			}
		}
		if (inSemiAction && difficulty < 4)
		{
			return;
		}
		if (num < 5f)
		{
			inAction = true;
			if (shotgunning)
			{
				CancelShotgunShot();
			}
			if (firstPhase || enraged || targetingStalker || inSemiAction)
			{
				Combo();
			}
			else if ((float)UnityEngine.Random.Range(0, 100) <= spiralSwordChance)
			{
				SwordSpiral();
				if (spiralSwordChance > 50f)
				{
					spiralSwordChance = 25f;
				}
				else
				{
					spiralSwordChance -= 25f;
				}
			}
			else
			{
				Combo();
				if (spiralSwordChance < 50f)
				{
					spiralSwordChance = 75f;
				}
				else
				{
					spiralSwordChance += 25f;
				}
			}
		}
		else
		{
			if (!(num <= 8f) || !(runningAttackCooldown <= 0f))
			{
				return;
			}
			runningAttackCooldown = 3f;
			if ((float)UnityEngine.Random.Range(0, 100) <= runningAttackChance)
			{
				if (shotgunning)
				{
					CancelShotgunShot();
				}
				inAction = true;
				RunningSwing();
				if (runningAttackChance > 50f)
				{
					runningAttackChance = 25f;
				}
				else
				{
					runningAttackChance -= 25f;
				}
			}
			else if (runningAttackChance < 50f)
			{
				runningAttackChance = 75f;
			}
			else
			{
				runningAttackChance += 25f;
			}
		}
	}

	private void CancelShotgunShot()
	{
		anim.SetLayerWeight(1, 0f);
		shotgunning = false;
		if (!gunDelay)
		{
			gunDelay = true;
			Invoke("ShootDelay", UnityEngine.Random.Range(5, 10));
		}
	}

	private void UpdateRunningAnimation()
	{
		if (!inAction && (bool)(UnityEngine.Object)(object)nma && ((((Behaviour)(object)nma).enabled && nma.isOnNavMesh) || mach.isTraversingPortalLink))
		{
			if (nma.velocity.magnitude > 0.1f || mach.isTraversingPortalLink)
			{
				anim.SetBool("Running", true);
			}
			else
			{
				anim.SetBool("Running", false);
			}
		}
	}

	private void EnrageUpdate()
	{
		if (!eternalRage && rageLeft > 0f)
		{
			rageLeft = Mathf.MoveTowards(rageLeft, 0f, Time.deltaTime * eid.totalSpeedModifier);
			if ((UnityEngine.Object)(object)enrageAud != null && rageLeft < 3f)
			{
				enrageAud.SetPitch(rageLeft / 3f);
			}
			if (rageLeft <= 0f)
			{
				UnEnrage();
			}
		}
	}

	private void CheckPhases()
	{
		if (firstPhase && mach.health <= phaseChangeHealth)
		{
			firstPhase = false;
			phaseChangeHealth = 0f;
			if (bossVersion)
			{
				MonoSingleton<NewMovement>.Instance.ResetHardDamage();
				MonoSingleton<NewMovement>.Instance.GetHealth(999, silent: true);
			}
			EndFirstPhase();
		}
		if (!usingShotgun && (bothPhases || (firstPhase && mach.health < 110f)))
		{
			usingShotgun = true;
			gunDelay = false;
		}
		if (mach.health < 95f)
		{
			gunDelay = false;
		}
	}

	private void IdleFailSafe()
	{
		if (idleFailsafe > 0f && (bool)(UnityEngine.Object)(object)anim && (inAction || !active || knockedDown || downed) && anim.GetCurrentAnimatorClipInfo(0).Length != 0 && ((UnityEngine.Object)(object)((AnimatorClipInfo)(ref anim.GetCurrentAnimatorClipInfo(0)[0])).clip).name == "Idle")
		{
			idleFailsafe = Mathf.MoveTowards(idleFailsafe, 0f, Time.deltaTime);
			if (idleFailsafe == 0f)
			{
				StopAction();
				if (knockedDown || downed)
				{
					KnockdownEnd();
				}
			}
		}
		else
		{
			idleFailsafe = 1f;
		}
	}

	private void FixedUpdate()
	{
		if (rb.isKinematic)
		{
			return;
		}
		if (moveAtTarget)
		{
			Vector3 vector = moveDirection * moveSpeed;
			if (!enraged && !mach.IsLedgeSafe())
			{
				vector = Vector3.zero;
			}
			rb.velocity = new Vector3(vector.x, rb.velocity.y, vector.z);
		}
		else
		{
			rb.velocity = new Vector3(0f, Mathf.Min(0f, rb.velocity.y), 0f);
		}
	}

	private void LateUpdate()
	{
		if (!firstPhase && !eternalRage && !bothPhases)
		{
			rightArm.localScale = Vector3.zero;
		}
		if (difficulty < 4 || !usingShotgun || target == null)
		{
			return;
		}
		if (shotgunning)
		{
			aimLerp = Mathf.MoveTowards(aimLerp, 1f, Time.deltaTime * 2f);
		}
		else
		{
			aimLerp = Mathf.MoveTowards(aimLerp, 0f, Time.deltaTime * 8f);
		}
		if (!(aimLerp > 0f))
		{
			return;
		}
		UpdateVision();
		Quaternion[] array = new Quaternion[aimBones.Length];
		for (int i = 0; i < aimBones.Length; i++)
		{
			array[i] = aimBones[i].localRotation;
			aimBones[i].LookAt(targetPosition);
			if (i == 1)
			{
				aimBones[i].transform.Rotate(Vector3.right * 90f, Space.Self);
			}
			aimBones[i].localRotation = Quaternion.Lerp(array[i], aimBones[i].localRotation, aimLerp);
		}
	}

	private void AttackSetup()
	{
		if (target != null)
		{
			Vector3 vector = targetPosition;
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
		}
		nma.updatePosition = false;
		nma.updateRotation = false;
		((Behaviour)(object)nma).enabled = false;
		if (!rb.isKinematic)
		{
			rb.velocity = Vector3.zero;
		}
	}

	public void RunningSwing()
	{
		AttackSetup();
		anim.SetTrigger("RunningSwing");
		moveSpeed = 30f * moveSpeedMultiplier;
		damage = 40;
	}

	private void Combo()
	{
		AttackSetup();
		anim.SetTrigger("Combo");
		moveSpeed = 60f * moveSpeedMultiplier;
		damage = 25;
	}

	private void SwordThrow()
	{
		AttackSetup();
		throwType = 2;
		anim.SetBool("Running", false);
		anim.SetTrigger("SwordThrow");
		damage = 0;
	}

	private void SwordSpiral()
	{
		AttackSetup();
		throwType = 1;
		anim.SetTrigger("SwordSpiral");
		waitingForSword = true;
		damage = 0;
	}

	public void StartMoving()
	{
		if (!knockedDown && !downed && target != null)
		{
			Vector3 vector = targetPosition;
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
			rb.isKinematic = false;
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			moveDirection = base.transform.forward;
			moveAtTarget = true;
		}
	}

	public void StopMoving()
	{
		moveAtTarget = false;
		if (gc.onGround)
		{
			rb.isKinematic = true;
		}
		if (!rb.isKinematic)
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
	}

	public void LookAt()
	{
		if (target != null)
		{
			Vector3 vector = targetPosition;
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
		}
	}

	public void StopAction()
	{
		mach.parryable = false;
		waitingForSword = false;
		if (gc.onGround && (bool)(UnityEngine.Object)(object)nma)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			((Behaviour)(object)nma).enabled = true;
		}
		StopMoving();
		inAction = false;
		runningAttackCooldown = 0f;
	}

	public void SemiStopAction()
	{
		mach.parryable = false;
		if (gc.onGround && (bool)(UnityEngine.Object)(object)nma)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			((Behaviour)(object)nma).enabled = true;
		}
		inSemiAction = true;
		inAction = false;
		anim.SetTrigger("AnimationCancel");
	}

	public void HeatSword()
	{
		if (inSemiAction)
		{
			slapTrail.emitting = true;
		}
		else
		{
			swordTrail.emitting = true;
		}
		if (!eid.puppet)
		{
			swordMR.sharedMaterial = heatMat;
		}
		swordAud.SetPitch(1.5f);
		UnityEngine.Object.Instantiate(flash.ToAsset(), head.transform.position + Vector3.up + head.transform.forward, head.transform.rotation, head.transform);
		mach.ParryableCheck();
	}

	public void HeatSwordThrow()
	{
		if ((bool)swordTrail)
		{
			swordTrail.emitting = true;
		}
		if (!eid.puppet)
		{
			swordMR.sharedMaterial = heatMat;
		}
		swordAud.SetPitch(1.5f);
		UnityEngine.Object.Instantiate(gunFlash.ToAsset(), head.transform);
		Vector3 vector = targetPosition;
		Vector3 vector2 = targetVelocity;
		if (throwType == 2 && target != null)
		{
			if (target.isPlayer)
			{
				targetFuturePos = vector + vector2 * (Vector3.Distance(base.transform.position, vector) / 80f) * Vector3.Distance(base.transform.position, vector) * 0.08f / anim.speed;
			}
			else
			{
				targetFuturePos = vector + vector2;
			}
			base.transform.LookAt(new Vector3(targetFuturePos.x, base.transform.position.y, targetFuturePos.z));
		}
		mach.ParryableCheck();
	}

	public void CoolSword()
	{
		if ((bool)swordTrail)
		{
			swordTrail.emitting = false;
		}
		if ((bool)slapTrail)
		{
			slapTrail.emitting = false;
		}
		if (!eid.puppet)
		{
			swordMR.sharedMaterial = (enraged ? enragedSword : origMat);
		}
		swordAud.SetPitch(1f);
	}

	public void DamageStart()
	{
		damaging = true;
		if (!inSemiAction)
		{
			if ((bool)swordTrail)
			{
				UnityEngine.Object.Instantiate(swingSound, swordTrail.transform);
			}
			SwingCheck2[] array = swordSwingCheck;
			foreach (SwingCheck2 obj in array)
			{
				obj.OverrideEnemyIdentifier(eid);
				obj.damage = damage;
				obj.DamageStart();
			}
		}
		else
		{
			slapSwingCheck.OverrideEnemyIdentifier(eid);
			slapSwingCheck.DamageStart();
		}
	}

	public void DamageStop()
	{
		damaging = false;
		mach.parryable = false;
		SwingCheck2[] array = swordSwingCheck;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
		slapSwingCheck.DamageStop();
	}

	public void ShootGun()
	{
		if (!inAction)
		{
			if (hasVision)
			{
				TargetData value = vision.CalculateData(targetHandle);
				shotgun.UpdateTarget(value);
			}
			else if (target != null)
			{
				lastTargetData.position = target.position;
				shotgun.UpdateTarget(lastTargetData);
			}
			Vector3 direction = shotgun.shootPoint.transform.position - VisionSourcePosition;
			PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, direction, default(LayerMask), out var _, out var _, out var traversals);
			bool instantExplode = false;
			if (traversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV = traversals[0];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				Portal portalObject = portalTraversalV.portalObject;
				instantExplode = !portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile) && !portalObject.passThroughNonTraversals;
			}
			shotgun.Fire(instantExplode);
		}
	}

	public void StopShootAnimation()
	{
		mach.parryable = false;
		anim.SetLayerWeight(1, 0f);
		gunDelay = true;
		shotgunning = false;
		Invoke("ShootDelay", (float)UnityEngine.Random.Range(5, 20) / eid.totalSpeedModifier);
	}

	private void ShootDelay()
	{
		gunDelay = false;
	}

	public void FlashGun()
	{
		UnityEngine.Object.Instantiate(gunFlash.ToAsset(), head.transform.position + Vector3.up + head.transform.forward, head.transform.rotation, head.transform);
	}

	public void SwordSpawn()
	{
		mach.parryable = false;
		if (target == null)
		{
			return;
		}
		Vector3 vector = targetPosition;
		Vector3 vector2 = targetVelocity;
		RaycastHit hitInfo;
		if (throwType == 1)
		{
			targetFuturePos = vector;
		}
		else if (target.isPlayer)
		{
			targetFuturePos = new Vector3(targetFuturePos.x, vector.y + vector2.y * Vector3.Distance(base.transform.position, vector) * 0.01f, targetFuturePos.z);
			if (Physics.Raycast(vector, targetFuturePos - vector, out hitInfo, Vector3.Distance(vector, targetFuturePos), LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				targetFuturePos = hitInfo.point;
			}
		}
		else
		{
			targetFuturePos = vector + vector2 / eid.totalSpeedModifier;
		}
		base.transform.LookAt(new Vector3(targetFuturePos.x, base.transform.position.y, targetFuturePos.z));
		currentThrownSword = UnityEngine.Object.Instantiate(thrownSword[throwType], new Vector3(base.transform.position.x, handTransform.position.y, base.transform.position.z), Quaternion.identity);
		ThrownSword componentInChildren = currentThrownSword.GetComponentInChildren<ThrownSword>();
		componentInChildren.thrownBy = eid;
		if (throwType == 2)
		{
			currentThrownSword.transform.rotation = base.transform.rotation;
		}
		if (!eid.puppet)
		{
			swordMR.sharedMaterial = origMat;
		}
		swordMR.enabled = false;
		swordTrail.emitting = false;
		slapTrail.emitting = false;
		swordAud.SetPitch(0f);
		if (Physics.Raycast(base.transform.position + Vector3.up * 2f, (targetFuturePos - base.transform.position).normalized, out hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
		{
			componentInChildren.SetPoints(hitInfo.point, handTransform);
		}
		else
		{
			componentInChildren.thrownAtVoid = true;
			componentInChildren.SetPoints((targetFuturePos - base.transform.position) * 9999f, handTransform);
		}
		if (throwType == 2)
		{
			SemiStopAction();
		}
		Invoke("SwordCatch", 5f);
	}

	public void SwordCatch()
	{
		mach.parryable = false;
		if ((bool)currentThrownSword)
		{
			UnityEngine.Object.Destroy(currentThrownSword);
		}
		if (!knockedDown && !downed && (difficulty < 4 || !inAction || waitingForSword))
		{
			inAction = true;
			anim.SetTrigger("SwordCatch");
		}
		inSemiAction = false;
		waitingForSword = false;
		swordMR.enabled = true;
		swordAud.SetPitch(1f);
		swordThrowCharge = 3f;
		CancelInvoke("SwordCatch");
	}

	private void KnockdownSetup()
	{
		DamageStop();
		knockedDown = true;
		inAction = true;
		inSemiAction = false;
		waitingForSword = false;
		anim.SetLayerWeight(1, 0f);
		gunDelay = true;
		shotgunning = false;
		swordMR.enabled = true;
		swordTrail.emitting = false;
		slapTrail.emitting = false;
		if (!eid.puppet)
		{
			swordMR.sharedMaterial = origMat;
		}
		swordAud.SetPitch(1f);
		((Behaviour)(object)nma).enabled = true;
		nma.updatePosition = false;
		nma.updateRotation = false;
		((Behaviour)(object)nma).enabled = false;
		SetSpeed();
		moveAtTarget = false;
		if (target != null)
		{
			Vector3 vector = targetPosition;
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
		}
		if (gc.onGround)
		{
			rb.isKinematic = true;
		}
		else
		{
			rb.isKinematic = false;
		}
		if (!rb.isKinematic)
		{
			rb.velocity = Vector3.zero;
		}
		moveAtTarget = false;
		if (!bsm)
		{
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
		}
		if (mach == null)
		{
			mach = GetComponent<Enemy>();
		}
	}

	public void Knockdown(bool fromExplosion = false, bool fromThrownSword = false, bool heavyKnockdown = false, bool enrage = true)
	{
		KnockdownSetup();
		if (heavyKnockdown)
		{
			anim.Play("Knockdown");
			UnityEngine.Object.Instantiate(bigPainSound, base.transform);
		}
		else
		{
			anim.Play("LightKnockdown");
		}
		if (fromThrownSword)
		{
			eid.hitter = "projectile";
			mach.GetHurt(GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, Vector3.zero, (mach.health > 20f) ? 20f : (mach.health - 0.1f), 0f);
		}
		GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
		gore.transform.position = GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position;
		gore.GetComponent<Bloodsplatter>()?.GetReady();
		ParticleSystem component = gore.GetComponent<ParticleSystem>();
		if (component != null)
		{
			component.Play();
		}
		if (enrage)
		{
			Enrage();
		}
	}

	public void Down(bool fromExplosion = false)
	{
		downed = true;
		KnockdownSetup();
		Knockdown(fromExplosion: false, fromThrownSword: false, heavyKnockdown: true, enrage: false);
		Invoke("CheckLoop", 0.5f);
	}

	private void EndFirstPhase()
	{
		KnockdownSetup();
		inPhaseChange = true;
		active = false;
		if (bossVersion)
		{
			if (shotgunPickUp != null)
			{
				shotgunPickUp.transform.SetPositionAndRotation(shotgun.transform.position, shotgun.transform.rotation);
				shotgunPickUp.SetActive(value: true);
			}
			MonoSingleton<TimeController>.Instance.SlowDown(0.15f);
		}
		CharacterJoint[] componentsInChildren = rightArm.GetComponentsInChildren<CharacterJoint>();
		if (componentsInChildren.Length != 0)
		{
			CharacterJoint[] array = componentsInChildren;
			foreach (CharacterJoint obj in array)
			{
				UnityEngine.Object.Destroy(obj);
				obj.transform.localScale = Vector3.zero;
				obj.gameObject.SetActive(value: false);
			}
		}
		GameObject gore = bsm.GetGore(GoreType.Limb, eid);
		if ((bool)gore)
		{
			gore.transform.position = rightArm.position;
		}
		anim.Rebind();
		SetSpeed();
		anim.SetTrigger("Knockdown");
		UnityEngine.Object.Instantiate(bigPainSound, base.transform);
		if (secondPhasePosTarget != null)
		{
			MonoSingleton<MusicManager>.Instance.ArenaMusicEnd();
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		normalMovSpeed = nma.speed;
		if (enraged)
		{
			UnEnrage();
		}
	}

	public void KnockdownEnd()
	{
		knockedDown = false;
		moveAtTarget = false;
		if (gc.onGround)
		{
			rb.isKinematic = true;
		}
		inPhaseChange = false;
		active = true;
		inAction = false;
		inSemiAction = false;
		if (activateOnPhaseChange != null && !firstPhase)
		{
			activateOnPhaseChange.SetActive(value: true);
		}
		GetComponent<AudioSource>().volume = 0f;
		bool flag = secondPhasePosTarget != null && !firstPhase;
		if (gc.onGround || flag)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			((Behaviour)(object)nma).enabled = true;
		}
		if (flag)
		{
			TeleportAway();
		}
	}

	private void TeleportAway()
	{
		BossHealthBar component = GetComponent<BossHealthBar>();
		component.DisappearBar();
		new Vector3(base.transform.position.x, base.transform.position.y + 1.5f, base.transform.position.z);
		teleportEffect.SetActive(value: true);
		teleportEffect.transform.SetParent(null, worldPositionStays: true);
		base.gameObject.SetActive(value: false);
		SwordsMachine[] componentsInChildren = secondPhasePosTarget.GetComponentsInChildren<SwordsMachine>();
		if (componentsInChildren.Length != 0)
		{
			SwordsMachine[] array = componentsInChildren;
			foreach (SwordsMachine obj in array)
			{
				obj.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(obj.gameObject);
			}
		}
		base.transform.position = secondPhasePosTarget.position;
		base.transform.parent = secondPhasePosTarget;
		eid.spawnIn = true;
		base.gameObject.SetActive(value: true);
		component.enabled = true;
		secondPhasePosTarget = null;
		cpToReset.UpdateRooms();
	}

	public void Enrage()
	{
		if (!enraged && !bothPhases)
		{
			enraged = true;
			rageLeft = 10f;
			anim.speed = normalAnimSpeed * 1.15f;
			nma.speed = normalMovSpeed * 1.25f;
			ensim.enraged = true;
			if (!eid.puppet)
			{
				swordMR.sharedMaterial = enragedSword;
			}
			UnityEngine.Object.Instantiate(bigPainSound, base.transform).GetComponent<AudioSource>().SetPitch(2f);
			if (currentEnrageEffect == null)
			{
				currentEnrageEffect = UnityEngine.Object.Instantiate(enrageEffect, mach.chest.transform);
				enrageAud = currentEnrageEffect.GetComponent<AudioSource>();
			}
			enrageAud.SetPitch(1f);
		}
	}

	public void UnEnrage()
	{
		if (enraged)
		{
			rageLeft = 0f;
			anim.speed = normalAnimSpeed;
			nma.speed = normalMovSpeed;
			ensim.enraged = false;
			if (!eid.puppet)
			{
				swordMR.sharedMaterial = origMat;
			}
			enraged = false;
			if (currentEnrageEffect != null)
			{
				UnityEngine.Object.Destroy(currentEnrageEffect);
			}
		}
	}

	public void CheckLoop()
	{
		if (downed)
		{
			anim.Play("Knockdown", 0, 0.25f);
			Invoke("CheckLoop", 0.25f);
		}
	}

	public bool ShouldAttackEnemies()
	{
		return false;
	}

	public bool ShouldIgnorePlayer()
	{
		if (target != null && target.isEnemy)
		{
			return target.enemyIdentifier.enemyType == EnemyType.Stalker;
		}
		return false;
	}
}
