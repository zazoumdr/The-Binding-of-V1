using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class Ferryman : EnemyScript, IHitTargetCallback
{
	private Animator anim;

	private Enemy mach;

	private NavMeshAgent nma;

	private Rigidbody rb;

	private GroundCheckEnemy gce;

	private EnemyIdentifier eid;

	private NavMeshPath path;

	private VisionQuery targetQuery;

	private TargetData latestTargetData;

	private TargetHandle targetHandle;

	private Vector3 targetPos;

	private Vector3 targetVel;

	private Vector3 playerPos;

	private bool playerApproaching;

	private bool playerRetreating;

	private bool playerAbove;

	private bool playerBelow;

	private int difficulty = -1;

	private bool inAction;

	private bool tracking;

	private bool moving;

	private float movingSpeed;

	private bool uppercutting;

	private float overheadChance = 0.5f;

	private float stingerChance = 0.5f;

	private float kickComboChance = 0.5f;

	[HideInInspector]
	public float defaultMovementSpeed;

	[SerializeField]
	private GameObject parryableFlash;

	[SerializeField]
	private GameObject unparryableFlash;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private GameObject slamExplosion;

	[SerializeField]
	private GameObject lightningBoltWindup;

	[HideInInspector]
	public GameObject currentWindup;

	[SerializeField]
	private LightningStrikeExplosive lightningBolt;

	[SerializeField]
	private AudioSource lightningBoltChimes;

	public bool lightningOnly;

	[SerializeField]
	private EnemySimplifier oarSimplifier;

	private Material originalOar;

	[SerializeField]
	private Material chargedOar;

	[Header("SwingChecks")]
	[SerializeField]
	private SwingCheck2 mainSwingCheck;

	[SerializeField]
	private SwingCheck2 oarSwingCheck;

	[SerializeField]
	private SwingCheck2 kickSwingCheck;

	private SwingCheck2[] swingChecks;

	[SerializeField]
	private AudioSource swingAudioSource;

	[SerializeField]
	private AudioClip[] swingSounds;

	private bool useMain;

	private bool useOar;

	private bool useKick;

	private bool knockBack;

	[Header("Trails")]
	[SerializeField]
	private TrailRenderer frontTrail;

	[SerializeField]
	private TrailRenderer backTrail;

	[SerializeField]
	private TrailRenderer bodyTrail;

	private bool backTrailActive;

	[Header("Footsteps")]
	[SerializeField]
	private ParticleSystem[] footstepParticles;

	[SerializeField]
	private AudioSource footstepAudio;

	private float rollCooldown;

	private float vaultCooldown;

	[Header("Boss Version")]
	[SerializeField]
	private bool bossVersion;

	[SerializeField]
	private float phaseChangeHealth;

	[SerializeField]
	private Transform[] phaseChangePositions;

	private int currentPosition;

	[SerializeField]
	private UltrakillEvent onPhaseChange;

	private bool inPhaseChange;

	private bool hasPhaseChanged;

	private bool hasReachedFinalPosition;

	private bool jumping;

	private float lightningBoltCooldown = 1.5f;

	private float lightningOutOfReachCharge;

	private bool lightningCancellable;

	private Vector3 lastGroundedPosition;

	public bool downed;

	private Vision vision => mach.vision;

	private void Awake()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		mach = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		path = new NavMeshPath();
		swingChecks = GetComponentsInChildren<SwingCheck2>();
	}

	private void Start()
	{
		if ((bool)oarSimplifier)
		{
			originalOar = oarSimplifier.originalMaterial;
		}
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid));
		SetSpeed();
		SlowUpdate();
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = defaultMovementSpeed,
			angularSpeed = 36000f,
			acceleration = 1000f
		};
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!(Object)(object)nma)
		{
			nma = GetComponent<NavMeshAgent>();
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
		if (defaultMovementSpeed == 0f)
		{
			defaultMovementSpeed = nma.speed;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		nma.speed = defaultMovementSpeed * eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded && (bool)currentWindup)
		{
			currentWindup.SetActive(value: false);
		}
		inAction = false;
		tracking = false;
		moving = false;
		uppercutting = false;
		if (MonoSingleton<EnemyCooldowns>.TryGetInstance(out EnemyCooldowns instance))
		{
			instance.RemoveFerryman(this);
		}
		StopDamage();
	}

	private void OnEnable()
	{
		MonoSingleton<EnemyCooldowns>.Instance.AddFerryman(this);
		if ((bool)currentWindup)
		{
			currentWindup.SetActive(value: true);
		}
	}

	private void SlowUpdate()
	{
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		Invoke("SlowUpdate", GetUpdateRate(nma));
		if ((Object)(object)nma == null)
		{
			return;
		}
		if (inPhaseChange)
		{
			if (!inAction && nma.isOnNavMesh)
			{
				mach.SetDestination(phaseChangePositions[currentPosition].position);
			}
		}
		else if (eid.target == null)
		{
			if (inAction || tracking || lightningCancellable)
			{
				CancelLightningBolt();
			}
		}
		else
		{
			if (!lightningCancellable && !nma.isOnNavMesh)
			{
				return;
			}
			UpdateTargetVision();
			bool flag = false;
			if (!inAction || lightningCancellable)
			{
				if (!eid.target.isOnGround || !NavMesh.CalculatePath(base.transform.position, PredictPlayerPos(vertical: true), nma.areaMask, path))
				{
					if (Physics.Raycast(eid.target.position, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
					{
						NavMeshHit val = default(NavMeshHit);
						if (NavMesh.SamplePosition(hitInfo.point, ref val, 1f, nma.areaMask))
						{
							NavMesh.CalculatePath(base.transform.position, ((NavMeshHit)(ref val)).position, nma.areaMask, path);
						}
						else
						{
							NavMesh.CalculatePath(base.transform.position, hitInfo.point, nma.areaMask, path);
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				if (!inAction && nma.isOnNavMesh)
				{
					nma.path = path;
				}
			}
			if (lightningOnly)
			{
				if (!inAction && MonoSingleton<EnemyCooldowns>.Instance.ferrymanCooldown <= 0f && lightningBoltCooldown <= 0f && eid.zapperer == null)
				{
					LightningBolt();
				}
				return;
			}
			if (targetPos.y > base.transform.position.y + 20f)
			{
				flag = true;
			}
			else if ((int)path.status != 0 && path.corners != null && path.corners.Length != 0 && (!eid.target.isOnGround || Vector3.Distance(path.corners[path.corners.Length - 1], PredictPlayerPos(vertical: true)) > 5f))
			{
				flag = true;
			}
			else if (inAction && lightningCancellable)
			{
				CancelLightningBolt();
			}
			if (difficulty <= 1)
			{
				return;
			}
			if (flag)
			{
				lightningOutOfReachCharge += 0.1f * eid.totalSpeedModifier;
				if (!inAction && lightningOutOfReachCharge > 3f && MonoSingleton<EnemyCooldowns>.Instance.ferrymanCooldown <= 0f && eid.zapperer == null)
				{
					lightningOutOfReachCharge = 0f;
					LightningBolt(quick: true);
				}
			}
			else
			{
				lightningOutOfReachCharge = 0f;
			}
		}
	}

	private void Update()
	{
		if (eid.target != null)
		{
			UpdateTargetVision();
			PlayerStatus();
		}
		UpdateCooldowns();
		anim.SetBool("Falling", !gce.onGround);
		bool flag = (bool)(Object)(object)nma && nma.isOnNavMesh && nma.velocity.magnitude > 2f && gce.onGround;
		anim.SetBool("Running", flag || mach.isTraversingPortalLink);
		if (mach.health < phaseChangeHealth && bossVersion && !hasPhaseChanged)
		{
			PhaseChange();
		}
		if (inPhaseChange && !inAction)
		{
			PhaseChangeUpdate();
		}
		else if (inAction)
		{
			((Behaviour)(object)nma).enabled = false;
			if (tracking)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(PredictPlayerPos() - base.transform.position), Time.deltaTime * 600f * eid.totalSpeedModifier);
			}
			if (moving && eid.target != null)
			{
				rb.velocity = (mach.IsLedgeSafe() ? (base.transform.forward * movingSpeed * anim.speed) : Vector3.zero);
			}
			if (uppercutting)
			{
				bool flag2 = mach.IsLedgeSafe() && Vector3.Distance(base.transform.position, playerPos) > 5f;
				rb.velocity = Vector3.up * 100f * anim.speed + (flag2 ? (base.transform.forward * Mathf.Min(100f, Vector3.Distance(base.transform.position, playerPos) * 40f) * anim.speed) : Vector3.zero);
			}
		}
		else if (eid.target != null && gce.onGround && !lightningOnly)
		{
			AttackCheck();
		}
	}

	private void UpdateTargetVision()
	{
		if (vision.TrySee(targetQuery, out var data))
		{
			latestTargetData = data.ToData();
			targetHandle = latestTargetData.handle;
		}
		if (targetHandle != null)
		{
			targetPos = latestTargetData.position;
			targetVel = latestTargetData.velocity;
		}
		else
		{
			targetPos = eid.target.position;
			targetVel = eid.target.GetVelocity();
		}
	}

	private void UpdateCooldowns()
	{
		if (lightningBoltCooldown > 0f)
		{
			lightningBoltCooldown = Mathf.MoveTowards(lightningBoltCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier * ((inPhaseChange || lightningOnly) ? 1f : 0.4f));
		}
		if (rollCooldown > 0f)
		{
			rollCooldown = Mathf.MoveTowards(rollCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (vaultCooldown > 0f)
		{
			vaultCooldown = Mathf.MoveTowards(vaultCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
	}

	private void PhaseChangeUpdate()
	{
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(base.transform.position, phaseChangePositions[currentPosition].position) < 3.5f)
		{
			if (currentPosition < phaseChangePositions.Length - 1)
			{
				currentPosition++;
				nma.destination = phaseChangePositions[currentPosition].position;
				return;
			}
			if (!hasReachedFinalPosition)
			{
				base.transform.position = phaseChangePositions[phaseChangePositions.Length - 1].position;
				rb.isKinematic = true;
				rb.SetGravityMode(useGravity: false);
				hasReachedFinalPosition = true;
			}
			anim.SetBool("Running", false);
			if (!inAction)
			{
				if (lightningBoltCooldown <= 0f && eid.zapperer == null)
				{
					LightningBolt();
				}
				else
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(playerPos - base.transform.position), Time.deltaTime * 600f * eid.totalSpeedModifier);
				}
			}
		}
		else if (!(Object)(object)nma || !((Behaviour)(object)nma).enabled || !nma.isOnNavMesh || !gce.onGround)
		{
			anim.SetBool("Falling", true);
			anim.SetBool("Running", true);
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
			Vector3 vector = ToPlanePos(phaseChangePositions[currentPosition].position);
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Time.deltaTime * Mathf.Max(10f, Vector3.Distance(base.transform.position, vector) * eid.totalSpeedModifier));
		}
		else if ((int)nma.pathStatus == 0 || jumping)
		{
			((Behaviour)(object)nma).enabled = true;
			anim.SetBool("Running", true);
		}
		else
		{
			anim.SetBool("Falling", true);
			anim.SetBool("Running", true);
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
			rb.AddForce(Vector3.up * Mathf.Abs(base.transform.position.y - phaseChangePositions[currentPosition].position.y) * 2f, ForceMode.VelocityChange);
			jumping = true;
			((Behaviour)(object)nma).enabled = false;
			Land(0.5f);
			swingAudioSource.SetPitch(Random.Range(2.9f, 3f));
			swingAudioSource.volume = 0.5f;
			swingAudioSource.Play(tracked: true);
			base.transform.position += Vector3.up * 5f;
			base.transform.rotation = Quaternion.LookRotation(ToPlanePos(phaseChangePositions[currentPosition].position) - base.transform.position);
		}
	}

	private void AttackCheck()
	{
		if (difficulty >= 4 && lightningBoltCooldown <= 0f && MonoSingleton<EnemyCooldowns>.Instance.ferrymanCooldown <= 0f && eid.zapperer == null)
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				LightningBolt(quick: true);
			}
			else
			{
				lightningBoltCooldown = 0.4f;
			}
			return;
		}
		float num = Vector3.Distance(playerPos, base.transform.position);
		if (num < 8f && (targetPos.y > base.transform.position.y + 5f || (targetVel.y > 5f && !eid.target.isOnGround)))
		{
			if (playerRetreating && rollCooldown <= 0f)
			{
				Roll();
			}
			else if (num < 5f && targetPos.y < base.transform.position.y + 20f)
			{
				Uppercut();
			}
		}
		else if (num > 8f || (playerRetreating && MonoSingleton<NewMovement>.Instance.sliding))
		{
			if (vaultCooldown <= 0f && num < 35f && num > 30f && !playerApproaching && targetPos.y <= base.transform.position.y + 20f)
			{
				vaultCooldown = 2f;
				if (difficulty >= 3)
				{
					VaultSwing();
				}
				else
				{
					Vault();
				}
			}
			else if (num < 14f && playerRetreating && !playerAbove)
			{
				if (Random.Range(0f, 1f) < stingerChance || rollCooldown > 0f)
				{
					stingerChance = Mathf.Min(0.25f, stingerChance - 0.25f);
					Stinger();
				}
				else
				{
					stingerChance = Mathf.Max(0.75f, stingerChance + 0.25f);
					Roll();
				}
			}
		}
		else if (playerApproaching)
		{
			if (Random.Range(0f, 1f) < 0.25f)
			{
				if (Random.Range(0f, 1f) < 0.75f && rollCooldown <= 0f)
				{
					Roll(toPlayerSide: true);
				}
				else if (Random.Range(0f, 1f) < 0.5f)
				{
					KickCombo();
				}
				else
				{
					OarCombo();
				}
			}
			else if (Random.Range(0f, 1f) < overheadChance)
			{
				overheadChance = Mathf.Min(0.25f, overheadChance - 0.25f);
				Downslam();
			}
			else
			{
				overheadChance = Mathf.Max(0.75f, overheadChance + 0.25f);
				BackstepAttack();
			}
		}
		else if (Random.Range(0f, 1f) < kickComboChance)
		{
			kickComboChance = Mathf.Min(0.25f, kickComboChance - 0.25f);
			KickCombo();
		}
		else
		{
			kickComboChance = Mathf.Max(0.75f, kickComboChance + 0.25f);
			OarCombo();
		}
	}

	private void FixedUpdate()
	{
		if (gce.onGround)
		{
			lastGroundedPosition = base.transform.position;
			if (!moving && !uppercutting && !jumping)
			{
				((Behaviour)(object)nma).enabled = !inAction;
				rb.SetGravityMode(useGravity: false);
				rb.isKinematic = true;
			}
		}
		else if (!inAction)
		{
			rb.SetGravityMode(useGravity: true);
			rb.isKinematic = false;
			((Behaviour)(object)nma).enabled = false;
			jumping = false;
			if ((bool)rb)
			{
				rb.AddForce(Vector3.down * 20f * Time.fixedDeltaTime, ForceMode.VelocityChange);
			}
		}
	}

	private void PrepAttack(bool shouldTrack = true, bool stopViaNavMesh = true)
	{
		SnapToGround();
		inAction = true;
		tracking = shouldTrack;
		if (stopViaNavMesh && nma.isOnNavMesh)
		{
			mach.SetDestination(base.transform.position);
		}
	}

	private void UpdateUses(bool main, bool oar, bool kick)
	{
		useMain = main;
		useOar = oar;
		useKick = kick;
	}

	private void Downslam()
	{
		PrepAttack();
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("Downslam");
		backTrailActive = false;
	}

	private void BackstepAttack()
	{
		PrepAttack(shouldTrack: false);
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("BackstepAttack");
		backTrailActive = true;
		knockBack = true;
		StartMoving(-3.5f);
	}

	private void Stinger()
	{
		PrepAttack();
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("Stinger");
		backTrailActive = true;
	}

	private void Vault()
	{
		PrepAttack(shouldTrack: true, stopViaNavMesh: false);
		UpdateUses(main: false, oar: false, kick: true);
		anim.SetTrigger("Vault");
		backTrailActive = false;
		bodyTrail.emitting = true;
		StartMoving(0.5f);
	}

	private void VaultSwing()
	{
		PrepAttack(shouldTrack: true, stopViaNavMesh: false);
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("VaultSwing");
		backTrailActive = true;
		StartMoving(0.5f);
	}

	private void KickCombo()
	{
		PrepAttack();
		UpdateUses(main: true, oar: false, kick: true);
		anim.SetTrigger("KickCombo");
	}

	private void OarCombo()
	{
		PrepAttack();
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("OarCombo");
		backTrailActive = true;
	}

	private void Uppercut()
	{
		PrepAttack();
		UpdateUses(main: true, oar: true, kick: false);
		anim.SetTrigger("Uppercut");
		backTrailActive = true;
	}

	public void Roll(bool toPlayerSide = false)
	{
		PrepAttack(shouldTrack: false);
		((Behaviour)(object)nma).enabled = false;
		anim.SetTrigger("Roll");
		bodyTrail.emitting = true;
		if (!toPlayerSide)
		{
			base.transform.rotation = Quaternion.LookRotation(PredictPlayerPos(vertical: false, 20f) - base.transform.position);
		}
		else
		{
			float num = ((Random.Range(0f, 1f) > 0.5f) ? 5 : (-5));
			base.transform.rotation = Quaternion.LookRotation(playerPos + MonoSingleton<CameraController>.Instance.transform.right * num - base.transform.position);
		}
		StartMoving(5f);
		if (difficulty <= 2)
		{
			rollCooldown = 5.5f - (float)(difficulty * 2);
		}
	}

	public void LightningBolt(bool quick = false)
	{
		MonoSingleton<EnemyCooldowns>.Instance.ferrymanCooldown += 6f;
		inAction = true;
		lightningBoltCooldown = 8 - difficulty * 2;
		if (lightningOnly)
		{
			lightningBoltCooldown = Mathf.Min(lightningBoltCooldown, 6f);
		}
		if (quick && difficulty >= 4 && lightningBoltCooldown < 3f)
		{
			lightningBoltCooldown = 3f;
		}
		tracking = true;
		if (quick && difficulty >= 4)
		{
			anim.SetTrigger("QuickLightningBolt");
			return;
		}
		anim.SetTrigger("LightningBolt");
		if (!lightningOnly)
		{
			lightningCancellable = true;
		}
	}

	public void LightningBoltWindup(int quick = 0)
	{
		if (eid.dead)
		{
			return;
		}
		if (eid.zapperer != null)
		{
			GotParried();
			return;
		}
		if ((bool)currentWindup)
		{
			Object.Destroy(currentWindup);
		}
		currentWindup = Object.Instantiate(lightningBoltWindup, PredictPlayerPos(), Quaternion.identity);
		if ((bool)base.transform.parent)
		{
			currentWindup.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		if ((bool)oarSimplifier)
		{
			oarSimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, chargedOar);
		}
		Follow[] components;
		if (eid.target != null)
		{
			components = currentWindup.GetComponents<Follow>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].target = eid.target.targetTransform;
			}
		}
		components = currentWindup.GetComponents<Follow>();
		foreach (Follow follow in components)
		{
			if (follow.speed == 0f)
			{
				continue;
			}
			if (difficulty == 0)
			{
				follow.enabled = false;
				continue;
			}
			switch (difficulty)
			{
			case 3:
			case 4:
			case 5:
				follow.speed *= 3f;
				break;
			case 2:
				follow.speed *= 2f;
				break;
			case 1:
				follow.speed *= 0.5f;
				break;
			}
			follow.speed *= eid.totalSpeedModifier;
		}
		tracking = false;
		lightningBoltChimes.Play(tracked: true);
		if (quick == 1)
		{
			if (currentWindup.TryGetComponent<ObjectActivator>(out var component))
			{
				component.delay = 3f;
			}
			Invoke("LightningBoltWindupOver", 5f);
		}
	}

	public void LightningBoltWindupOver()
	{
		if ((bool)currentWindup)
		{
			GameObject obj = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, currentWindup.transform.position + Vector3.up * 50f, Quaternion.LookRotation(Vector3.down));
			obj.transform.localScale *= 100f;
			obj.transform.SetParent(currentWindup.transform, worldPositionStays: true);
			Invoke("LightningBoltStrike", 0.5f);
		}
	}

	public void LightningBoltStrike()
	{
		SpawnLightningBolt(currentWindup.transform.position);
		Object.Destroy(currentWindup);
		lightningCancellable = false;
	}

	public void SpawnLightningBolt(Vector3 position, bool safeForPlayer = false)
	{
		if ((bool)oarSimplifier)
		{
			oarSimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalOar);
		}
		LightningStrikeExplosive lightningStrikeExplosive = Object.Instantiate(lightningBolt, position, Quaternion.identity);
		lightningStrikeExplosive.safeForPlayer = safeForPlayer;
		lightningStrikeExplosive.damageMultiplier = eid.totalDamageModifier;
		if ((bool)base.transform.parent)
		{
			lightningStrikeExplosive.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
	}

	public void CancelLightningBolt()
	{
		if ((bool)oarSimplifier)
		{
			oarSimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalOar);
		}
		if ((bool)currentWindup)
		{
			Object.Destroy(currentWindup);
		}
		lightningCancellable = false;
		anim.Play("Idle");
		StopAction();
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		OnDeath();
	}

	public void OnDeath()
	{
		if ((bool)oarSimplifier)
		{
			oarSimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalOar);
		}
		Object.Destroy(this);
	}

	private void StartTracking()
	{
		tracking = true;
	}

	private void StopTracking()
	{
		tracking = false;
	}

	private void StartMoving(float speed)
	{
		movingSpeed = speed * 10f;
		moving = true;
		rb.isKinematic = false;
		Land();
	}

	private void StopMoving()
	{
		bodyTrail.emitting = false;
		moving = false;
		rb.isKinematic = true;
		Land();
	}

	public void SlamHit()
	{
		Object.Instantiate(slamExplosion, ToPlanePos(frontTrail.transform.position), Quaternion.identity);
		Land();
	}

	public void Land(float volume = 0.75f)
	{
		ParticleSystem[] array = footstepParticles;
		foreach (ParticleSystem val in array)
		{
			if (Mathf.Abs(((Component)(object)val).transform.position.y - base.transform.position.y) < 1f)
			{
				val.Play();
			}
		}
		Footstep(volume);
	}

	private void Footstep(float volume = 0.5f)
	{
		if (volume == 0f)
		{
			volume = 0.5f;
		}
		footstepAudio.volume = volume;
		footstepAudio.SetPitch(Random.Range(1.15f, 1.35f));
		footstepAudio.Play(tracked: true);
	}

	private void StartUppercut()
	{
		uppercutting = true;
		rb.isKinematic = false;
		StartDamage();
	}

	private void StopUppercut()
	{
		uppercutting = false;
		rb.SetGravityMode(useGravity: true);
		rb.velocity = Vector3.up * 10f;
		StopDamage();
	}

	private void StartDamage(int damage = 25)
	{
		if (damage == 0)
		{
			damage = 25;
		}
		SwingCheck2[] array = swingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].damage = damage;
		}
		if (useMain)
		{
			mainSwingCheck.DamageStart();
		}
		if (useOar)
		{
			oarSwingCheck.DamageStart();
		}
		if (useKick)
		{
			kickSwingCheck.DamageStart();
		}
		if (useOar || useKick)
		{
			swingAudioSource.SetPitch(useOar ? Random.Range(0.65f, 0.9f) : Random.Range(2.1f, 2.55f));
			swingAudioSource.volume = (useOar ? 1f : 0.75f);
		}
		swingAudioSource.clip = swingSounds[Random.Range(0, swingSounds.Length)];
		swingAudioSource.Play(tracked: true);
		frontTrail.emitting = useMain || useOar;
		backTrail.emitting = backTrailActive;
	}

	private void StopDamage()
	{
		knockBack = false;
		mach.parryable = false;
		frontTrail.emitting = false;
		backTrail.emitting = false;
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
		}
	}

	public void TargetBeenHit()
	{
		if (eid.target != null && eid.target.isPlayer && knockBack)
		{
			MonoSingleton<NewMovement>.Instance.Launch((playerPos - base.transform.position).normalized * 2500f + Vector3.up * 250f);
		}
		SwingCheck2[] array = swingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
	}

	private void StopAction()
	{
		if (moving)
		{
			StopMoving();
		}
		inAction = false;
		((Behaviour)(object)nma).enabled = true;
		tracking = false;
		bodyTrail.emitting = false;
	}

	public void ParryableFlash()
	{
		Flash(parryable: true);
	}

	public void UnparryableFlash()
	{
		Flash(parryable: false);
	}

	public void Flash(bool parryable)
	{
		Object.Instantiate(parryable ? parryableFlash : unparryableFlash, head.position + (MonoSingleton<CameraController>.Instance.defaultPos - head.position).normalized, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.defaultPos - head.position), head).transform.localScale *= 0.025f;
		if (parryable)
		{
			mach.ParryableCheck();
		}
	}

	public void GotParried()
	{
		SpawnLightningBolt(mach.chest.transform.position, safeForPlayer: true);
		eid.hitter = "";
		eid.hitterAttributes.Add(HitterAttribute.Electricity);
		eid.DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, 1E-05f, tryForExplode: false);
		if (currentWindup != null)
		{
			if ((bool)oarSimplifier)
			{
				oarSimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalOar);
			}
			Object.Destroy(currentWindup);
		}
	}

	private void PlayerStatus()
	{
		playerPos = ToPlanePos(targetPos);
		playerAbove = targetPos.y > base.transform.position.y + 3f;
		playerBelow = targetPos.y < base.transform.position.y - 4f;
		Vector3 vector = new Vector3(targetVel.x, 0f, targetVel.z);
		if (vector.magnitude < 1f)
		{
			playerApproaching = false;
			playerRetreating = false;
		}
		else
		{
			float num = Mathf.Abs(Vector3.Angle(vector.normalized, playerPos - base.transform.position));
			playerRetreating = num < 80f;
			playerApproaching = num > 135f;
		}
	}

	private Vector3 PredictPlayerPos(bool vertical = false, float maxPrediction = 5f)
	{
		if (eid.target == null)
		{
			return base.transform.position + base.transform.forward;
		}
		if (vertical)
		{
			if (difficulty <= 1)
			{
				return targetPos;
			}
			Vector3 vector = Vector3.zero;
			if (eid.target != null && eid.target.isPlayer)
			{
				vector = ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer) ? Vector3.down : Vector3.zero);
			}
			return targetPos + targetVel.normalized * Mathf.Min(targetVel.magnitude, 5f) + vector;
		}
		if (difficulty <= 1)
		{
			return playerPos;
		}
		Vector3 vector2 = new Vector3(targetVel.x, 0f, targetVel.z);
		return playerPos + vector2.normalized * Mathf.Min(vector2.magnitude, maxPrediction);
	}

	private void SnapToGround()
	{
		if (!nma.isOnNavMesh && gce.onGround && Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			base.transform.position = hitInfo.point;
		}
		base.transform.rotation = Quaternion.LookRotation(playerPos - base.transform.position);
	}

	public void PhaseChange()
	{
		inPhaseChange = true;
		onPhaseChange.Invoke();
	}

	public void EndPhaseChange()
	{
		inPhaseChange = false;
		hasPhaseChanged = true;
		if (!hasReachedFinalPosition)
		{
			base.transform.position = phaseChangePositions[phaseChangePositions.Length - 1].position;
		}
	}

	public void Knockdown()
	{
		StopAction();
		inAction = true;
		anim.Play("Knockdown", 0, 0f);
		Invoke("CheckLoop", 2.45f);
	}

	public void CheckLoop()
	{
		if (downed)
		{
			anim.Play("Knockdown", 0, 0.25f);
			Invoke("CheckLoop", 1.6f);
		}
	}
}
