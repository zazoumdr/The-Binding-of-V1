using ULTRAKILL.Enemy;
using UnityEngine;

public class Power : EnemyScript
{
	private enum SpearAttackState
	{
		None,
		Vertical,
		Horizontal
	}

	private Animator anim;

	private EnemyIdentifier eid;

	private Rigidbody rb;

	private Enemy mach;

	private LayerMask environmentMask;

	private int difficulty = -1;

	private bool active = true;

	private bool goingLeft;

	[HideInInspector]
	public bool inAction;

	[HideInInspector]
	public bool juggled;

	private bool juggleFalling;

	private float juggleHp;

	private TimeSince sinceJuggleStart;

	[SerializeField]
	private SkinnedMeshRenderer cape;

	[SerializeField]
	private GameObject particles;

	[SerializeField]
	private GameObject particlesEnraged;

	[SerializeField]
	private GameObject juggleEffect;

	private bool goForward;

	private float forwardSpeed;

	[SerializeField]
	private GameObject dashEffect;

	[SerializeField]
	private GameObject sword;

	[SerializeField]
	private GameObject zwei;

	[SerializeField]
	private GameObject zweiProjectiles;

	[SerializeField]
	private GameObject glaive;

	[SerializeField]
	private GameObject spinner;

	[SerializeField]
	private Projectile spinnerThrown;

	[SerializeField]
	private GameObject spear;

	[SerializeField]
	private Projectile spearThrown;

	private float[] moveChanceBonuses = new float[5];

	private float attackCooldown = 2f;

	private int burstLength = 2;

	private SpearAttackState spearing;

	private bool aboutToThrowSpear;

	private bool projectilesOnSwing;

	private bool verticalSwing;

	private TimeSince sinceLastAttacked;

	private bool checkingForSelfDefend;

	private float healthSinceLastAttacked;

	private float preAttackHealth;

	private bool hasAttacked;

	private bool teleportAfterAction;

	[SerializeField]
	private SwingCheck2 swingCheck;

	[SerializeField]
	private SwingCheck2[] zweiChecks;

	[SerializeField]
	private GameObject weaponSpawnEffect;

	[SerializeField]
	private GameObject weaponBreakEffect;

	[SerializeField]
	private GameObject stabEffect;

	private ParticleSystem stabParticle;

	private TrailRenderer stabTrail;

	private AudioSource stabAudio;

	private WeaponTrail currentWeaponTrail;

	private float outOfSightTime;

	private int teleportAttempts;

	private int teleportInterval = 6;

	public GameObject teleportSound;

	public GameObject decoy;

	public bool enraged;

	private EnemySimplifier[] ensims;

	private bool overrideRotation;

	private bool stopRotation;

	private Vector3 overrideTarget;

	private int spearAttacks;

	private GameObject currentSpearFlash;

	private VisionQuery targetQuery;

	private TargetHandle targetHandle;

	private TargetHandle throwStartTargetHandle;

	private TargetData lastTargetData;

	public Collider aggroBounds;

	private bool targetInAggroBounds;

	private AudioSource aud;

	public float voicePitch = -1f;

	private bool highPriorityVoice;

	private bool dying;

	private Vector3 deathPosition;

	[SerializeField]
	private Rigidbody[] deathLimbs;

	private TimeSince sincePreviousLimb;

	private int currentLimb;

	[SerializeField]
	private SkinnedMeshRenderer bodyMeshRenderer;

	[SerializeField]
	private Mesh deadMesh;

	[SerializeField]
	private Transform decorativeArm;

	[SerializeField]
	private Transform physicsArm;

	private bool armDetached;

	private Vector3 pullStartPosition;

	private TimeSince sinceArmPullStart;

	private GameObject currentEnrageParticle;

	[HideInInspector]
	public Vector3 originalPosition;

	private TimeSince sinceLastVision;

	private EnemyTarget target => eid.target;

	private Vision vision => mach.vision;

	private bool hasVision => targetHandle != null;

	private bool isJuggled
	{
		get
		{
			if (juggled)
			{
				return mach.health < juggleHp;
			}
			return false;
		}
	}

	private void Awake()
	{
		anim = GetComponent<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		mach = GetComponent<Enemy>();
		environmentMask = LayerMaskDefaults.Get(LMD.Environment);
		ensims = GetComponentsInChildren<EnemySimplifier>();
		aud = GetComponent<AudioSource>();
		if (voicePitch == -1f)
		{
			voicePitch = Random.Range(0.95f, 1.05f);
		}
		if (originalPosition == Vector3.zero)
		{
			originalPosition = base.transform.position;
		}
		if ((bool)stabEffect)
		{
			stabParticle = stabEffect.GetComponentInChildren<ParticleSystem>();
			stabTrail = stabEffect.GetComponentInChildren<TrailRenderer>();
			stabAudio = stabEffect.GetComponentInChildren<AudioSource>();
		}
		physicsArm.SetParent(base.transform.parent);
	}

	private void Start()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		UpdateSpeed();
		RandomizeDirection();
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, environmentMask, toHead: true));
		preAttackHealth = mach.health;
		UpdateVision();
	}

	private void UpdateBuff()
	{
		UpdateSpeed();
	}

	private void UpdateSpeed()
	{
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			anim.speed = 0.95f;
			break;
		case 1:
			anim.speed = 0.8f;
			break;
		case 0:
			anim.speed = 0.7f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
	}

	private void OnEnable()
	{
		MonoSingleton<EnemyCooldowns>.Instance.AddPower(this);
	}

	private void OnDisable()
	{
		MonoSingleton<EnemyCooldowns>.Instance.RemovePower(this);
	}

	private void UpdateVision()
	{
		if (active && !((float)sinceLastVision < 0.5f))
		{
			sinceLastVision = 0f;
			if (vision.TrySee(targetQuery, out var data))
			{
				lastTargetData = data.ToData();
				targetHandle = lastTargetData.handle;
			}
			else
			{
				targetHandle = null;
			}
		}
	}

	private void Update()
	{
		UpdateRigidbodySettings();
		if (dying)
		{
			DyingUpdate();
		}
		if (!active)
		{
			return;
		}
		if (target != null)
		{
			UpdateVision();
		}
		if (cape.enabled && armDetached)
		{
			physicsArm.position = Vector3.Lerp(pullStartPosition, decorativeArm.position, (float)sinceArmPullStart * 2f);
			if ((float)sinceArmPullStart >= 0.5f)
			{
				armDetached = false;
				physicsArm.gameObject.SetActive(value: false);
				decorativeArm.localScale = Vector3.one;
			}
		}
		bool flag = MonoSingleton<EnemyCooldowns>.Instance.powers.Count > 0 && (MonoSingleton<EnemyCooldowns>.Instance.attackingPower != null || MonoSingleton<EnemyCooldowns>.Instance.powers[0] != this);
		if (!hasAttacked && mach.health < preAttackHealth)
		{
			preAttackHealth = mach.health;
			MonoSingleton<EnemyCooldowns>.Instance.PrioritizePower(this);
		}
		if (target == null)
		{
			targetInAggroBounds = false;
		}
		else
		{
			targetInAggroBounds = aggroBounds == null || aggroBounds.bounds.Contains(target.position);
		}
		if (!inAction)
		{
			if (attackCooldown > 0f)
			{
				attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if (attackCooldown <= 0f && (enraged || !flag))
			{
				if (hasVision && target != null)
				{
					PickAttack();
				}
				else if (targetInAggroBounds && outOfSightTime > 1f)
				{
					Debug.Log("AggroBounds");
					Teleport();
				}
			}
		}
		if (targetInAggroBounds)
		{
			if (!hasVision)
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 3f, Time.deltaTime * eid.totalSpeedModifier);
				if (outOfSightTime >= 3f && !inAction)
				{
					Teleport();
				}
			}
			else
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 0f, Time.deltaTime * 2f * eid.totalSpeedModifier);
			}
		}
		if (!stopRotation)
		{
			Quaternion quaternion = Quaternion.LookRotation((overrideRotation ? overrideTarget : lastTargetData.headPosition) - base.transform.position, Vector3.up);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * ((float)(overrideRotation ? 2500 : 10) * Quaternion.Angle(quaternion, base.transform.rotation) + (float)(overrideRotation ? 10 : 2)) * eid.totalSpeedModifier);
		}
		if (isJuggled)
		{
			if (MonoSingleton<EnemyCooldowns>.Instance.powers.Count > 1 && MonoSingleton<EnemyCooldowns>.Instance.powers[0] == this)
			{
				MonoSingleton<EnemyCooldowns>.Instance.DeprioritizePower(this);
			}
			if (rb.velocity.y < 0f)
			{
				rb.velocity = (((float)sinceJuggleStart < 5f) ? Vector3.zero : Vector3.Lerp(Vector3.zero, rb.velocity, ((float)sinceJuggleStart - 3f) / 3f));
			}
			rb.AddForce((juggleHp - mach.health) * Mathf.Clamp(3f - ((float)sinceJuggleStart - 3f), 0f, 5f) * Vector3.up, ForceMode.VelocityChange);
			anim.Play("Juggle", 0, 0f);
			juggleHp = mach.health;
			base.transform.LookAt(new Vector3(lastTargetData.headPosition.x, base.transform.position.y, lastTargetData.headPosition.z));
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Hurt(), randomPitch: true);
			}
		}
		if (juggled)
		{
			if (rb.velocity.y <= -100f)
			{
				AudioClip val = MonoSingleton<PowerVoiceController>.Instance.FallScream();
				if ((Object)(object)aud.clip != (Object)(object)val)
				{
					aud.Stop();
					PlaySound(val, randomPitch: false, loop: true, 0f);
				}
				aud.volume = Mathf.InverseLerp(100f, 200f, Mathf.Abs(rb.velocity.y));
				aud.dopplerLevel = Mathf.InverseLerp(150f, 1000f, Mathf.Abs(rb.velocity.y));
			}
			else if (aud.isPlaying && aud.loop)
			{
				aud.Stop();
			}
		}
		if (MonoSingleton<EnemyCooldowns>.Instance.attackingPower == this || MonoSingleton<EnemyCooldowns>.Instance.powers.Count <= 1 || (float)sinceLastAttacked <= 1f)
		{
			return;
		}
		if (!checkingForSelfDefend)
		{
			checkingForSelfDefend = true;
			healthSinceLastAttacked = mach.health;
		}
		else if (mach.health <= healthSinceLastAttacked - 1f && !inAction)
		{
			healthSinceLastAttacked = mach.health;
			if (MonoSingleton<EnemyCooldowns>.Instance.attackingPower == null)
			{
				MonoSingleton<EnemyCooldowns>.Instance.PrioritizePower(this);
			}
			else if (eid.hitter != "enemy" && eid.hitter != "ffexplosion" && eid.hitter != "drill" && eid.hitter != "fire")
			{
				checkingForSelfDefend = false;
				sinceLastAttacked = 0f;
				Throw(cheapShot: true);
				teleportAfterAction = true;
			}
		}
	}

	private void UpdateRigidbodySettings()
	{
		if ((bool)rb)
		{
			rb.drag = ((target == null) ? 3 : 0);
			rb.angularDrag = ((target == null) ? 3 : 0);
		}
	}

	private void DyingUpdate()
	{
		base.transform.position = new Vector3(deathPosition.x + Random.Range(-0.2f, 0.2f), deathPosition.y + Random.Range(-0.2f, 0.2f), deathPosition.z + Random.Range(-0.2f, 0.2f));
		if ((float)sincePreviousLimb > 0.25f)
		{
			if (deathLimbs[currentLimb] != null)
			{
				GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
				sincePreviousLimb = (float)sincePreviousLimb - 0.25f;
				deathLimbs[currentLimb].isKinematic = false;
				deathLimbs[currentLimb].useGravity = true;
				deathLimbs[currentLimb].transform.SetParent(goreZone.gibZone, worldPositionStays: true);
			}
			if (currentLimb < deathLimbs.Length - 1)
			{
				currentLimb++;
				return;
			}
			((Behaviour)(object)anim).enabled = false;
			base.enabled = false;
		}
	}

	private void PickAttack()
	{
		MonoSingleton<EnemyCooldowns>.Instance.PowerAttacking(this);
		hasAttacked = true;
		bool flag = Vector3.Distance(base.transform.position, lastTargetData.headPosition) < 5f;
		bool flag2 = Vector3.Distance(base.transform.position, lastTargetData.headPosition) > 10f;
		float[] array = new float[moveChanceBonuses.Length];
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (MonoSingleton<EnemyCooldowns>.Instance.previousPowerMove != i && !(i <= 1 && flag))
			{
				array[i] = Random.Range(0f, 1f) + moveChanceBonuses[i] * ((i < 2) ? 1f : (flag2 ? 0.5f : 1f));
			}
		}
		if (MonoSingleton<EnemyCooldowns>.Instance.powers.Count > 1)
		{
			array[0] = 0f;
		}
		float num2 = 0f;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num2)
			{
				num2 = array[j];
				num = j;
			}
		}
		switch (num)
		{
		case 0:
			Throw();
			break;
		case 1:
			Spear();
			break;
		case 2:
			Debug.Log("Rapier");
			if (flag2)
			{
				Teleport(closeRange: true, longrange: false, firstTime: true, horizontal: false, vertical: false, forceOnScreen: true);
			}
			Rapier();
			break;
		case 3:
			Debug.Log("Zwei");
			if (flag2)
			{
				Teleport(closeRange: true, longrange: false, firstTime: true, horizontal: false, vertical: false, forceOnScreen: true);
			}
			Zwei();
			break;
		case 4:
			Debug.Log("Glaive");
			if (flag2)
			{
				Teleport(closeRange: true, longrange: false, firstTime: true, horizontal: false, vertical: false, forceOnScreen: true);
			}
			Glaive();
			break;
		}
		MonoSingleton<EnemyCooldowns>.Instance.previousPowerMove = num;
		for (int k = 0; k < array.Length; k++)
		{
			if (k == num)
			{
				moveChanceBonuses[k] = 0f;
			}
			else
			{
				moveChanceBonuses[k] += 0.25f;
			}
		}
		if (num == 0)
		{
			return;
		}
		if (burstLength > 1)
		{
			burstLength--;
			return;
		}
		burstLength = ((difficulty >= 3) ? 3 : 2);
		if (enraged)
		{
			attackCooldown = 0f;
		}
		else
		{
			attackCooldown = ((difficulty <= 3) ? 3 : (6 - difficulty));
		}
	}

	private void LateUpdate()
	{
		if (armDetached)
		{
			decorativeArm.localScale = Vector3.zero;
		}
	}

	private void FixedUpdate()
	{
		if (!active || target == null)
		{
			return;
		}
		PhysicsCastResult hitInfo;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		if (!juggled)
		{
			if (!inAction)
			{
				Vector3 zero = Vector3.zero;
				if (target == null)
				{
					zero = Vector3.zero;
				}
				else if (goingLeft)
				{
					if (!PortalPhysicsV2.SphereCast(base.transform.position, base.transform.right * -1f, 3f, 1.25f, environmentMask, out hitInfo, out portalTraversals, out endPoint))
					{
						zero += base.transform.right * -5f;
					}
					else if (!PortalPhysicsV2.SphereCast(base.transform.position, base.transform.right, 3f, 1.25f, environmentMask, out hitInfo, out portalTraversals, out endPoint))
					{
						goingLeft = false;
					}
					else
					{
						zero += base.transform.forward * 5f;
					}
				}
				else if (!PortalPhysicsV2.SphereCast(base.transform.position, base.transform.right, 3f, 1.25f, environmentMask, out hitInfo, out portalTraversals, out endPoint))
				{
					zero += base.transform.right * 5f;
				}
				else if (!PortalPhysicsV2.SphereCast(base.transform.position, base.transform.right * 1f, 3f, 1.25f, environmentMask, out hitInfo, out portalTraversals, out endPoint))
				{
					goingLeft = true;
				}
				else
				{
					zero += base.transform.forward * 5f;
				}
				rb.velocity = zero * eid.totalSpeedModifier;
			}
			else if (goForward)
			{
				rb.velocity = ((difficulty >= 4) ? 1.25f : 1f) * forwardSpeed * base.transform.forward;
			}
			else
			{
				rb.velocity = Vector3.zero;
			}
		}
		else
		{
			if (rb.velocity.y < 35f)
			{
				rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			}
			else
			{
				rb.velocity = new Vector3(0f, 35f, 0f);
			}
			if (juggleFalling && PortalPhysicsV2.SphereCast(base.transform.position, Vector3.down, 3.6f, 1.25f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), out hitInfo, out portalTraversals, out endPoint))
			{
				JuggleStop();
			}
			if (rb.velocity.y < 0f)
			{
				juggleFalling = true;
			}
		}
		if (spearing != SpearAttackState.None)
		{
			if (!goForward && spearing == SpearAttackState.Vertical && targetInAggroBounds)
			{
				base.transform.position = lastTargetData.target.HeadPosition + Vector3.up * 15f;
			}
			else if (PortalPhysicsV2.Raycast(base.transform.position, base.transform.forward, 2f, environmentMask))
			{
				spearing = SpearAttackState.None;
				StopStab();
			}
		}
	}

	public void Rapier()
	{
		if (active && !juggled)
		{
			anim.Play("Rapier", 0, 0f);
			inAction = true;
			forwardSpeed = 100f;
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Rapier());
			}
		}
	}

	public void Zwei()
	{
		if (active && !juggled)
		{
			anim.Play("Zweihander", 0, 0f);
			inAction = true;
			forwardSpeed = 100f;
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Greatsword());
			}
			projectilesOnSwing = true;
		}
	}

	public void Glaive()
	{
		if (active && !juggled)
		{
			anim.Play("Glaive", 0, 0f);
			inAction = true;
			forwardSpeed = 125f;
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Glaive());
			}
		}
	}

	public void Throw(bool cheapShot = false)
	{
		if (active && !juggled)
		{
			anim.Play("Throw", 0, 0f);
			inAction = true;
			throwStartTargetHandle = targetHandle;
			if (CanPlaySound())
			{
				PlaySound(cheapShot ? MonoSingleton<PowerVoiceController>.Instance.CheapShot() : MonoSingleton<PowerVoiceController>.Instance.GlaiveThrow());
			}
		}
	}

	public void Spear()
	{
		if (active && !juggled)
		{
			anim.Play("SpearSpawn", 0, 0f);
			inAction = true;
			switch (difficulty)
			{
			case 2:
			case 3:
			case 4:
			case 5:
				forwardSpeed = 150f;
				break;
			case 1:
				forwardSpeed = 75f;
				break;
			case 0:
				forwardSpeed = 60f;
				break;
			}
			forwardSpeed *= eid.totalSpeedModifier;
			spearAttacks = ((!enraged) ? 1 : 2);
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Spear());
			}
		}
	}

	public void SpawnWeapon(GameObject weapon)
	{
		if (active)
		{
			weapon.SetActive(value: true);
			currentWeaponTrail = weapon.GetComponentInChildren<WeaponTrail>();
			Object.Instantiate(weaponSpawnEffect, weapon.transform.position, Quaternion.identity);
		}
	}

	public void DespawnWeapon(GameObject weapon, bool breakEffect = true)
	{
		if (weapon.activeSelf)
		{
			weapon.SetActive(value: false);
			currentWeaponTrail = null;
			if (breakEffect)
			{
				Object.Instantiate(weaponBreakEffect, weapon.transform.position, Quaternion.identity);
			}
			if (difficulty >= 2 && MonoSingleton<EnemyCooldowns>.Instance.attackingPower == this)
			{
				AttackEnd();
			}
		}
	}

	public void ThrowSpinner()
	{
		if (active && !juggled)
		{
			DespawnWeapon(spinner, breakEffect: false);
			Projectile projectile = Object.Instantiate(spinnerThrown, spinner.transform.position, base.transform.rotation, base.transform.parent);
			UpdateVision();
			if (targetHandle != null)
			{
				projectile.targetHandle = targetHandle;
			}
			else if (throwStartTargetHandle != null)
			{
				projectile.targetHandle = throwStartTargetHandle;
			}
			else
			{
				projectile.target = eid.target;
			}
			if (difficulty >= 4)
			{
				projectile.speed *= 1.75f;
			}
			projectile.damage *= eid.totalDamageModifier;
		}
	}

	private void SpearAttack()
	{
		if (!active || juggled)
		{
			return;
		}
		if (target == null)
		{
			spearAttacks = 0;
		}
		if (spearAttacks == 0)
		{
			ToSpearThrow();
			return;
		}
		spearing = SpearAttackState.Vertical;
		goForward = false;
		spearAttacks--;
		float num = 1.5f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			num = 0.75f;
			break;
		case 2:
			num = 1.5f;
			break;
		case 0:
		case 1:
			num = 2f;
			break;
		}
		Invoke("SpearAttack", num / eid.totalSpeedModifier);
		bool flag = false;
		Vector3 vector = lastTargetData.realHeadPosition;
		float num2 = Random.Range(0f, 1f);
		if (!PortalPhysicsV2.Raycast(lastTargetData.realHeadPosition, Vector3.up, out var hitInfo, out var endPoint, 17f, environmentMask, QueryTriggerInteraction.Ignore))
		{
			PortalPhysicsV2.Raycast(lastTargetData.realHeadPosition, Vector3.up, out hitInfo, out endPoint, 15f, environmentMask, QueryTriggerInteraction.Ignore);
			vector = endPoint;
			flag = true;
		}
		else if (!PortalPhysicsV2.Raycast(lastTargetData.realHeadPosition, Vector3.down, out hitInfo, out endPoint, 17f, environmentMask, QueryTriggerInteraction.Ignore))
		{
			PortalPhysicsV2.Raycast(base.transform.position, Vector3.down, out hitInfo, out endPoint, 15f, environmentMask, QueryTriggerInteraction.Ignore);
			vector = endPoint;
			flag = true;
		}
		if (!flag || ((difficulty >= 4 || enraged) && num2 > 0.5f))
		{
			spearing = SpearAttackState.Horizontal;
			anim.Play("SpearStinger");
			Teleport(closeRange: false, longrange: true, firstTime: true, horizontal: true);
			FollowTarget();
			Invoke("SpearFlash", 0.25f / eid.totalSpeedModifier);
			Invoke("SpearGoHorizontal", 0.5f / eid.totalSpeedModifier);
			return;
		}
		if ((Object)(object)anim != null)
		{
			anim.Play("SpearDrop");
		}
		int num3 = Mathf.RoundToInt(Vector3.Distance(base.transform.position, vector) / 2.5f);
		for (int i = 0; i < num3; i++)
		{
			CreateDecoy(Vector3.Lerp(base.transform.position, vector, (float)i / (float)num3), (float)i / (float)num3 + 0.1f);
		}
		base.transform.position = vector;
		teleportAttempts = 0;
		Object.Instantiate(teleportSound, base.transform.position, Quaternion.identity);
		if (eid.hooked)
		{
			MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
		}
		LookAtTarget();
		float num4 = 0.75f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			num4 = 0.5f;
			break;
		case 2:
			num4 = 0.75f;
			break;
		case 0:
		case 1:
			num4 = 1f;
			break;
		}
		Invoke("SpearFlash", num4 / 2f / eid.totalSpeedModifier);
		Invoke("SpearGo", num4 / eid.totalSpeedModifier);
	}

	private void SpearFlash()
	{
		if (active && !juggled)
		{
			currentSpearFlash = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, spear.transform);
		}
	}

	private void SpearGoHorizontal()
	{
		if (active && !juggled)
		{
			LookAtTarget();
			SpearGo();
			spearing = SpearAttackState.Horizontal;
		}
	}

	private void SpearGo()
	{
		if (active && !juggled)
		{
			Object.Instantiate(dashEffect, base.transform.position, base.transform.rotation);
			StartStab();
			spearing = SpearAttackState.Vertical;
		}
	}

	private void ToSpearThrow()
	{
		if (active && !juggled)
		{
			spearing = SpearAttackState.None;
			StopStab();
			Teleport();
			FollowTarget();
			anim.Play("SpearThrow");
			aboutToThrowSpear = true;
			if (CanPlaySound())
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.SpearThrow());
			}
		}
	}

	private void ThrowSpear()
	{
		if (!active || juggled)
		{
			return;
		}
		Projectile projectile = Object.Instantiate(spearThrown, base.transform.position + base.transform.forward * 3f, base.transform.rotation);
		if (difficulty <= 1 || eid.totalSpeedModifier != 1f || eid.totalDamageModifier != 1f)
		{
			projectile.target = target;
			if ((bool)projectile)
			{
				if (difficulty <= 1)
				{
					projectile.speed *= 0.5f;
				}
				projectile.damage *= eid.totalDamageModifier;
			}
		}
		aboutToThrowSpear = false;
		DespawnSpear();
	}

	private void ChangeForwardSpeed(float speed)
	{
		forwardSpeed = speed;
	}

	private void StartDamage(bool stab)
	{
		if (!active || juggled)
		{
			return;
		}
		if (!stab && (bool)currentWeaponTrail)
		{
			currentWeaponTrail.AddTrail();
		}
		else if (stab)
		{
			stabParticle.Play();
			stabAudio.Play(tracked: true);
			stabTrail.emitting = true;
		}
		swingCheck.ignoreSlidingPlayer = stab;
		if (zwei.activeSelf)
		{
			ZweiDamage(enabled: true);
		}
		else
		{
			swingCheck.DamageStart();
		}
		goForward = true;
		if (projectilesOnSwing && difficulty >= 3)
		{
			GameObject gameObject = Object.Instantiate(zweiProjectiles, zweiProjectiles.transform.position, zweiProjectiles.transform.rotation);
			gameObject.transform.SetParent(base.transform.parent ? base.transform.parent : GoreZone.ResolveGoreZone(gameObject.transform).transform);
			gameObject.SetActive(value: true);
			if (verticalSwing)
			{
				gameObject.transform.Rotate(Vector3.forward * 90f, Space.Self);
				verticalSwing = false;
			}
		}
	}

	private void StopDamage(bool stab)
	{
		if (!stab && (bool)currentWeaponTrail)
		{
			currentWeaponTrail.RemoveTrail();
		}
		else if (stab)
		{
			stabParticle.Stop();
			stabAudio.Stop();
			stabTrail.emitting = false;
		}
		mach.parryable = false;
		verticalSwing = false;
		if (zwei.activeSelf)
		{
			ZweiDamage(enabled: false);
		}
		else
		{
			swingCheck.DamageStop();
		}
		goForward = false;
	}

	private void Backdash()
	{
		Backdash(animation: false);
	}

	private void Backdash(bool animation)
	{
		if (active && !juggled)
		{
			if (animation)
			{
				inAction = true;
				anim.Play("Backdash");
			}
			goForward = true;
			forwardSpeed = -85f;
			LookAtTarget();
			Object.Instantiate(dashEffect, base.transform.position, Quaternion.Inverse(base.transform.rotation));
		}
	}

	private void StopMoving()
	{
		goForward = false;
	}

	public void Teleport(bool closeRange = false, bool longrange = false, bool firstTime = true, bool horizontal = false, bool vertical = false, bool forceOnScreen = false)
	{
		if (!active || juggled || target == null || !targetInAggroBounds || !lastTargetData.isValid())
		{
			return;
		}
		if (firstTime)
		{
			teleportAttempts = 0;
			outOfSightTime = 0f;
			spearing = SpearAttackState.None;
		}
		Vector3 normalized = Random.onUnitSphere.normalized;
		if (normalized.y < 0f)
		{
			normalized.y *= -1f;
		}
		float num = Random.Range(8, 15);
		if (closeRange)
		{
			num = Random.Range(5, 8);
		}
		else if (longrange)
		{
			num = Random.Range(15, 20);
		}
		if (forceOnScreen && Vector3.Dot(MonoSingleton<CameraController>.Instance.transform.forward, normalized) < 0.5f)
		{
			normalized = (MonoSingleton<CameraController>.Instance.cam.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), num)) - MonoSingleton<CameraController>.Instance.transform.position).normalized;
			if (normalized.y < 0f)
			{
				normalized.y *= -1f;
			}
		}
		Vector3 vector = lastTargetData.target.HeadPosition + Vector3.up;
		vector = ((!PortalPhysicsV2.Raycast(lastTargetData.target.HeadPosition + Vector3.up, normalized, out var hitInfo, num, environmentMask, QueryTriggerInteraction.Ignore)) ? (lastTargetData.target.HeadPosition + Vector3.up + normalized * num) : (hitInfo.point - normalized * 3f));
		PhysicsCastResult hitInfo2;
		Vector3 endPoint;
		bool flag = PortalPhysicsV2.Raycast(vector, Vector3.up, out hitInfo2, out endPoint, 8f, environmentMask, QueryTriggerInteraction.Ignore);
		PhysicsCastResult hitInfo3;
		Vector3 endPoint2;
		bool flag2 = PortalPhysicsV2.Raycast(vector, Vector3.down, out hitInfo3, out endPoint2, 8f, environmentMask, QueryTriggerInteraction.Ignore);
		Vector3 position = base.transform.position;
		if (!(flag && flag2))
		{
			position = (flag ? (endPoint + Vector3.down * Random.Range(5, 10)) : (flag2 ? ((!horizontal) ? (endPoint2 + Vector3.up * Random.Range(5, 10)) : new Vector3(endPoint2.x, endPoint2.y + 3.5f, endPoint2.z)) : ((!horizontal) ? vector : new Vector3(vector.x, lastTargetData.target.HeadPosition.y, vector.z))));
		}
		else
		{
			if (!(Vector3.Distance(hitInfo2.point, hitInfo3.point) > 7f))
			{
				teleportAttempts++;
				if (teleportAttempts <= 10)
				{
					Teleport(closeRange, longrange, firstTime: false, horizontal, vertical);
				}
				return;
			}
			position = ((!horizontal) ? new Vector3(vector.x, (endPoint2.y + endPoint.y) / 2f, vector.z) : new Vector3(vector.x, endPoint.y + 3.5f, vector.z));
		}
		Collider[] array = Physics.OverlapCapsule(position + base.transform.up * -2.25f, position + base.transform.up * 1.25f, 1.25f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore);
		if (array != null && array.Length != 0)
		{
			for (int num2 = array.Length - 1; num2 >= 0; num2--)
			{
				if (!IsHitTarget(array[num2]))
				{
					teleportAttempts++;
					if (teleportAttempts <= 10)
					{
						Teleport(closeRange, longrange, firstTime: false, horizontal, vertical);
					}
					return;
				}
			}
		}
		TeleportTo(position);
	}

	public void TeleportTo(Vector3 position)
	{
		if (eid.hooked)
		{
			MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
		}
		CreateDecoyTrail(position);
		base.transform.position = position;
		rb.position = position;
		Object.Instantiate(teleportSound, base.transform.position, Quaternion.identity);
		teleportAttempts = 0;
		goingLeft = !goingLeft;
	}

	public void CreateDecoyTrail(Vector3 target)
	{
		int num = Mathf.RoundToInt(Vector3.Distance(base.transform.position, target) / 2.5f);
		for (int i = 0; i < num; i++)
		{
			CreateDecoy(Vector3.Lerp(base.transform.position, target, (float)i / (float)num), (float)i / (float)num + 0.1f);
		}
	}

	public GameObject CreateDecoy(Vector3 position, float transparencyOverride = 1f, Animator animatorOverride = null)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((!(Object)(object)anim && !(Object)(object)animatorOverride) || target == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(decoy, position, base.transform.GetChild(0).rotation, base.transform.parent);
		Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
		AnimatorStateInfo val = (((Object)(object)animatorOverride) ? animatorOverride.GetCurrentAnimatorStateInfo(0) : anim.GetCurrentAnimatorStateInfo(0));
		componentInChildren.Play(((AnimatorStateInfo)(ref val)).shortNameHash, 0, ((AnimatorStateInfo)(ref val)).normalizedTime);
		componentInChildren.speed = 0f;
		MindflayerDecoy[] componentsInChildren = gameObject.GetComponentsInChildren<MindflayerDecoy>();
		foreach (MindflayerDecoy obj in componentsInChildren)
		{
			obj.fadeOverride = transparencyOverride;
			obj.enraged = enraged;
		}
		return gameObject;
	}

	public void GotParried()
	{
		JuggleStart();
	}

	public void JuggleStart()
	{
		if (active)
		{
			if (inAction)
			{
				StopAction();
			}
			inAction = true;
			CancelInvoke();
			spearing = SpearAttackState.None;
			rb.velocity = Vector3.zero;
			rb.AddForce(Vector3.up * 35f, ForceMode.VelocityChange);
			rb.SetGravityMode(useGravity: true);
			CapeDisable();
			base.transform.LookAt(new Vector3(lastTargetData.headPosition.x, base.transform.position.y, lastTargetData.headPosition.z));
			overrideRotation = false;
			stopRotation = true;
			juggled = true;
			juggleHp = mach.health;
			sinceJuggleStart = 0f;
			juggleFalling = false;
			Object.Instantiate(juggleEffect, base.transform.position, base.transform.rotation);
			eid.totalDamageTakenMultiplier = 0.75f;
			anim.Play("Juggle");
			if (CanPlaySound(highPriority: true))
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.HurtBig());
			}
		}
	}

	private void JuggleStop(bool enrage = false)
	{
		if (active)
		{
			rb.SetGravityMode(useGravity: false);
			stopRotation = false;
			juggled = false;
			StopAction();
			inAction = true;
			anim.Play("Enrage");
			eid.totalDamageTakenMultiplier = 1f;
			if (CanPlaySound(highPriority: true))
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Enrage());
			}
		}
	}

	public void StopAction()
	{
		inAction = false;
		goForward = false;
		spearing = SpearAttackState.None;
		aboutToThrowSpear = false;
		mach.parryable = false;
		projectilesOnSwing = false;
		verticalSwing = false;
		StopDamage(stab: true);
		StopDamage(stab: false);
		DespawnAll();
		RandomizeDirection();
		FollowTarget();
		if (MonoSingleton<EnemyCooldowns>.Instance.attackingPower == this)
		{
			AttackEnd();
		}
		if (teleportAfterAction)
		{
			teleportAfterAction = false;
			Teleport(closeRange: false, longrange: true);
		}
	}

	private void AttackEnd()
	{
		MonoSingleton<EnemyCooldowns>.Instance.PowerAttackEnd();
		sinceLastAttacked = 0f;
		checkingForSelfDefend = false;
		if (!enraged)
		{
			if (MonoSingleton<EnemyCooldowns>.Instance.powers.Count > 1)
			{
				Backdash(animation: true);
			}
			else if (attackCooldown > 0f && CanPlaySound(highPriority: true))
			{
				PlaySound(MonoSingleton<PowerVoiceController>.Instance.Taunt());
			}
		}
	}

	private void DespawnAll()
	{
		if (glaive.activeSelf)
		{
			DespawnWeapon(glaive);
		}
		if (spinner.activeSelf)
		{
			DespawnWeapon(spinner);
		}
		if (sword.activeSelf)
		{
			DespawnWeapon(sword);
		}
		if (zwei.activeSelf)
		{
			DespawnWeapon(zwei);
		}
		if (spear.activeSelf)
		{
			DespawnWeapon(spear);
		}
	}

	public void ParryFlash()
	{
		Flash(parryable: true);
	}

	private void Flash(bool parryable)
	{
		Object.Instantiate(parryable ? MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash : MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, eid.weakPoint.transform.position + base.transform.forward * 0.5f, base.transform.rotation).transform.localScale *= 4f;
		mach.parryable = parryable;
	}

	public void LookAtTarget(int flash = 0)
	{
		overrideRotation = true;
		if (flash != 0)
		{
			Flash(flash == 1);
		}
		if (target == null)
		{
			base.transform.rotation = Quaternion.identity;
			return;
		}
		if (difficulty >= 2 && target.isPlayer && (!spear.activeSelf || aboutToThrowSpear))
		{
			overrideTarget = base.transform.position + (MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.2f, aimAtHead: true) - base.transform.position).normalized * 999f;
		}
		else
		{
			overrideTarget = base.transform.position + (lastTargetData.realHeadPosition - base.transform.position).normalized * 999f;
		}
		base.transform.LookAt(overrideTarget);
	}

	public void FollowTarget()
	{
		if (!juggled)
		{
			overrideRotation = false;
		}
	}

	public void CapeDisable()
	{
		cape.enabled = false;
		particles.SetActive(value: false);
		particlesEnraged.SetActive(value: false);
		physicsArm.SetPositionAndRotation(decorativeArm.position, decorativeArm.rotation);
		physicsArm.gameObject.SetActive(value: true);
		armDetached = true;
	}

	public void CapeReset()
	{
		cape.enabled = true;
		pullStartPosition = physicsArm.position;
		sinceArmPullStart = 0f;
		if (difficulty >= 2)
		{
			EnrageNow();
		}
		if (enraged)
		{
			particlesEnraged.SetActive(value: true);
		}
		else
		{
			particles.SetActive(value: true);
		}
		Object.Instantiate(weaponSpawnEffect, particles.transform.position, Quaternion.identity);
	}

	public void EnrageNow()
	{
		enraged = true;
		EnemySimplifier[] array = ensims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enraged = true;
		}
		eid.UpdateBuffs(visualsOnly: true);
		if (currentEnrageParticle == null)
		{
			currentEnrageParticle = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.enrageEffect, base.transform);
		}
		if (particles.activeSelf)
		{
			particlesEnraged.SetActive(value: true);
			particles.SetActive(value: false);
		}
		attackCooldown = 0f;
	}

	public bool CanPlaySound(bool highPriority = false)
	{
		if (highPriority)
		{
			highPriorityVoice = true;
			return true;
		}
		if (highPriorityVoice && aud.isPlaying && aud.time < aud.clip.length * 0.9f)
		{
			return false;
		}
		highPriorityVoice = false;
		return true;
	}

	public void PlaySound(AudioClip clip, bool randomPitch = false, bool loop = false, float volume = 1f)
	{
		if (!((Object)(object)clip == null))
		{
			aud.clip = clip;
			aud.SetPitch(randomPitch ? Random.Range(0.95f, 1.05f) : voicePitch);
			aud.loop = loop;
			aud.volume = volume;
			aud.Play(tracked: true);
		}
	}

	private bool IsHitTarget(Collider col)
	{
		if (col.gameObject.layer != 11)
		{
			return false;
		}
		if (target == null)
		{
			return false;
		}
		if (target.enemyIdentifier == null)
		{
			return false;
		}
		if (!col.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			return false;
		}
		if (!component.eid)
		{
			return false;
		}
		if (target.enemyIdentifier != component.eid)
		{
			return false;
		}
		return true;
	}

	public void IntroContinuation()
	{
		anim.Play("Intro", 0, 0.85f);
	}

	public void Death()
	{
		active = false;
		dying = true;
		MonoSingleton<EnemyCooldowns>.Instance.RemovePower(this);
		anim.Play("Death", 0, 0f);
		if (CanPlaySound(highPriority: true))
		{
			PlaySound(MonoSingleton<PowerVoiceController>.Instance.Death());
		}
		sincePreviousLimb = 0f;
		deathPosition = base.transform.position;
		GetComponentsInChildren<EnemySimplifier>();
		bodyMeshRenderer.sharedMesh = deadMesh;
		rb.isKinematic = true;
		if (TryGetComponent<CapsuleCollider>(out var component))
		{
			component.enabled = false;
		}
		if (TryGetComponent<SoundPitch>(out var component2))
		{
			component2.Activate();
		}
		CapeDisable();
		if ((bool)currentEnrageParticle)
		{
			Object.Destroy(currentEnrageParticle);
		}
		StopAction();
	}

	public void SpawnSword()
	{
		SpawnWeapon(sword);
	}

	public void DespawnSword()
	{
		DespawnWeapon(sword);
	}

	public void SpawnZwei()
	{
		SpawnWeapon(zwei);
	}

	public void DespawnZwei()
	{
		DespawnWeapon(zwei);
	}

	public void SpawnGlaive()
	{
		SpawnWeapon(glaive);
	}

	public void DespawnGlaive()
	{
		DespawnWeapon(glaive);
	}

	public void SpawnSpinner()
	{
		SpawnWeapon(spinner);
	}

	public void DespawnSpinner()
	{
		DespawnWeapon(spinner);
	}

	public void SpawnSpear()
	{
		SpawnWeapon(spear);
	}

	public void DespawnSpear()
	{
		if ((bool)currentSpearFlash)
		{
			Object.Destroy(currentSpearFlash);
		}
		spear.SetActive(value: false);
	}

	public void GlaiveToSpinner()
	{
		if (active)
		{
			glaive.SetActive(value: false);
			spinner.SetActive(value: true);
		}
	}

	public void SpinnerToGlaive()
	{
		if (active)
		{
			spinner.SetActive(value: false);
			glaive.SetActive(value: true);
		}
	}

	public void StartSwing()
	{
		StartDamage(stab: false);
	}

	public void StopSwing()
	{
		StopDamage(stab: false);
	}

	public void StartStab()
	{
		StartDamage(stab: true);
	}

	public void StopStab()
	{
		StopDamage(stab: true);
	}

	private void VerticalSwing()
	{
		verticalSwing = true;
	}

	public void ZweiDamage(bool enabled)
	{
		SwingCheck2[] array = zweiChecks;
		foreach (SwingCheck2 swingCheck in array)
		{
			if (enabled)
			{
				swingCheck.DamageStart();
			}
			else
			{
				swingCheck.DamageStop();
			}
		}
	}

	private void RandomizeDirection()
	{
		if (Random.Range(0f, 1f) > 0.5f)
		{
			goingLeft = true;
		}
		else
		{
			goingLeft = false;
		}
	}
}
