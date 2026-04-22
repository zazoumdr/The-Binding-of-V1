using System.Collections.Generic;
using Sandbox;
using UnityEngine;
using UnityEngine.AI;

public class V2 : EnemyScript, IEnrage, IAlter, IAlterOptions<bool>
{
	private Animator anim;

	private Transform overrideTarget;

	private Rigidbody overrideTargetRb;

	private Vector3 targetPos;

	private Quaternion targetRot;

	public Transform[] aimAtTarget;

	private Rigidbody rb;

	private NavMeshAgent nma;

	private int currentWeapon;

	public SkinnedMeshRenderer smr;

	private EnemySimplifier[] ensims;

	public Texture[] wingTextures;

	public GameObject wingChangeEffect;

	public Color[] wingColors;

	public GameObject[] weapons;

	private GameObject currentWingChangeEffect;

	private TrailRenderer[] wingTrails;

	private DragBehind[] drags;

	private V2Pattern currentPattern;

	private bool inPattern;

	public GroundCheckEnemy gc;

	public GroundCheckEnemy wc;

	private int circlingDirection = 1;

	public GameObject jumpSound;

	public GameObject dashJumpSound;

	public bool secondEncounter;

	public bool slowMode;

	public float movementSpeed;

	private float originalMovementSpeed;

	public float jumpPower;

	public float wallJumpPower;

	public float airAcceleration;

	public bool intro;

	[HideInInspector]
	public bool inIntro;

	public bool active;

	private bool running;

	private bool aiming;

	private bool sliding;

	private bool dodging;

	private bool jumping;

	private float patternCooldown;

	private float dodgeCooldown = 3f;

	private float dodgeLeft;

	public GameObject dodgeEffect;

	public GameObject slideEffect;

	private int difficulty = -1;

	private float slideStopTimer;

	private TimeSince randomSlideCheck;

	private float shootCooldown;

	private float altShootCooldown;

	public GameObject gunFlash;

	public GameObject altFlash;

	private bool aboutToShoot;

	private bool chargingAlt;

	private float predictAmount;

	private bool aimAtGround;

	public bool dontDie;

	public Transform escapeTarget;

	private bool escaping;

	private bool dead;

	public bool longIntro;

	private bool staringAtPlayer;

	private bool introHitGround;

	private EnemyIdentifierIdentifier[] eidids;

	private BossHealthBar bhb;

	public GameObject shockwave;

	public GameObject KoScream;

	private RaycastHit rhit;

	private float distancePatience;

	private bool enraged;

	public GameObject enrageEffect;

	private GameObject currentEnrageEffect;

	private Enemy mac;

	private EnemyIdentifier eid;

	private float closeRangePatience = 5f;

	public GameObject spawnOnDeath;

	private bool playerInSight;

	private int coinsToThrow;

	private bool shootingForCoin;

	public GameObject coin;

	[HideInInspector]
	public bool firstPhase = true;

	public float knockOutHealth;

	public bool slideOnly;

	public bool dontEnrage;

	public bool alwaysAimAtGround;

	public Vector3 forceSlideDirection;

	private bool cowardPattern;

	public UltrakillEvent onKnockout;

	private float flashTimer;

	private List<Coin> coins = new List<Coin>();

	private bool bossVersion = true;

	private float coinsInSightCooldown;

	private EnemyTarget target => eid.target;

	public bool isEnraged => enraged;

	public string alterKey => "v2";

	public string alterCategoryName => "V2";

	public AlterOption<bool>[] options => new AlterOption<bool>[1]
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
		}
	};

	public override bool ShouldKnockback(ref DamageData data)
	{
		return !inIntro;
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		active = false;
		Die();
	}

	public override void OnDamage(ref DamageData data)
	{
		if (secondEncounter && data.hitter == "heavypunch")
		{
			InstaEnrage();
		}
	}

	private void Awake()
	{
		anim = GetComponentInChildren<Animator>();
		bhb = GetComponent<BossHealthBar>();
		mac = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		gc = GetComponentInChildren<GroundCheckEnemy>();
	}

	private void Start()
	{
		ensims = GetComponentsInChildren<EnemySimplifier>();
		if ((bool)MonoSingleton<StatueIntroChecker>.Instance && MonoSingleton<StatueIntroChecker>.Instance.beenSeen)
		{
			longIntro = false;
		}
		if (!intro)
		{
			active = true;
			if ((bool)bhb)
			{
				bhb.enabled = true;
			}
		}
		else
		{
			inIntro = true;
			rb.AddForce(base.transform.forward * 20f, ForceMode.VelocityChange);
			anim.SetBool("InAir", true);
			if (anim.layerCount > 1)
			{
				anim.SetLayerWeight(1, 1f);
				anim.SetLayerWeight(2, 0f);
			}
			if (longIntro)
			{
				eidids = GetComponentsInChildren<EnemyIdentifierIdentifier>();
				EnemyIdentifierIdentifier[] array = eidids;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].GetComponent<Collider>().enabled = false;
				}
				if ((bool)bhb)
				{
					bhb.enabled = false;
				}
			}
			else if ((bool)bhb)
			{
				bhb.enabled = true;
			}
		}
		SetSpeed();
		running = true;
		aiming = true;
		inPattern = true;
		wingTrails = GetComponentsInChildren<TrailRenderer>();
		drags = GetComponentsInChildren<DragBehind>();
		ChangeDirection(Random.Range(-90f, 90f));
		SwitchPattern(V2Pattern.Straight);
		shootCooldown = 1f;
		altShootCooldown = 5f;
		if (!weapons[currentWeapon].activeInHierarchy)
		{
			GameObject[] array2 = weapons;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetActive(value: false);
			}
			weapons[currentWeapon].SetActive(value: true);
		}
		if (!bhb)
		{
			bossVersion = false;
		}
		if (secondEncounter)
		{
			CoinUpdate();
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(Object)(object)nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (originalMovementSpeed != 0f)
		{
			movementSpeed = originalMovementSpeed;
		}
		else
		{
			switch (difficulty)
			{
			case 4:
			case 5:
				movementSpeed *= 1.5f;
				break;
			case 3:
				movementSpeed *= 1f;
				break;
			case 2:
				movementSpeed *= 0.85f;
				break;
			case 1:
				movementSpeed *= 0.75f;
				break;
			case 0:
				movementSpeed *= 0.65f;
				break;
			}
			movementSpeed *= eid.totalSpeedModifier;
			originalMovementSpeed = movementSpeed;
		}
		if (enraged)
		{
			movementSpeed *= 2f;
		}
		if ((bool)(Object)(object)nma)
		{
			nma.speed = originalMovementSpeed;
		}
		GameObject[] array = weapons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.GetChild(0).SendMessage("UpdateBuffs", eid, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Update()
	{
		if (firstPhase && mac.health <= knockOutHealth && knockOutHealth != 0f)
		{
			firstPhase = false;
			KnockedOut();
			eid.totalDamageTakenMultiplier = 0f;
		}
		if (inIntro)
		{
			IntroUpdate();
			return;
		}
		UpdateBossSecondaryBar();
		UpdateCooldowns();
		if (target == null)
		{
			running = false;
			if (gc.onGround)
			{
				rb.velocity = Vector3.zero;
			}
			if (anim.layerCount > 1)
			{
				anim.SetLayerWeight(1, 0f);
				anim.SetLayerWeight(2, 0f);
			}
			if (sliding)
			{
				StopSlide();
			}
			return;
		}
		running = true;
		if (!active || escaping)
		{
			return;
		}
		if ((bool)eid)
		{
			slowMode = eid.drillers.Count > 0;
		}
		if (slideOnly)
		{
			if (!sliding && gc.onGround && !dodging)
			{
				if (eid.enemyType != EnemyType.BigJohnator)
				{
					anim.Play("Slide");
				}
				base.transform.LookAt(new Vector3(base.transform.position.x + forceSlideDirection.x, base.transform.position.y + forceSlideDirection.y, base.transform.position.z + forceSlideDirection.z));
				Slide();
			}
		}
		else
		{
			UpdateAnimations();
		}
		targetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
		if (sliding)
		{
			if (slideOnly)
			{
				((Component)(object)anim).transform.localRotation = Quaternion.identity;
			}
			else if ((bool)(Object)(object)nma && !playerInSight)
			{
				StopSlide();
			}
			else
			{
				Quaternion a = Quaternion.LookRotation(base.transform.forward, Vector3.up);
				Quaternion b = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
				if (Quaternion.Angle(a, b) > (float)((distancePatience >= 5f) ? 45 : 90))
				{
					if (enraged || (difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp < 50))
					{
						StopSlide();
					}
					else
					{
						slideStopTimer = Mathf.MoveTowards(slideStopTimer, 0f, Time.deltaTime * eid.totalSpeedModifier);
						if (slideStopTimer <= 0f)
						{
							StopSlide();
						}
					}
				}
			}
		}
		else
		{
			targetRot = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
			((Component)(object)anim).transform.rotation = Quaternion.RotateTowards(((Component)(object)anim).transform.rotation, targetRot, Time.deltaTime * 10f * Quaternion.Angle(((Component)(object)anim).transform.rotation, targetRot) * eid.totalSpeedModifier);
			if (inPattern)
			{
				if (cowardPattern)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(base.transform.position - targetPos, Vector3.up), Time.deltaTime * 350f * eid.totalSpeedModifier);
				}
				else if (currentPattern == V2Pattern.Circle || (currentPattern == V2Pattern.Chase && Vector3.Distance(base.transform.position, targetPos) < 10f))
				{
					float num = 90f;
					if (Vector3.Distance(base.transform.position, targetPos) > 10f)
					{
						num = 80f;
					}
					else if (Vector3.Distance(base.transform.position, targetPos) < 5f)
					{
						num = 100f;
					}
					Quaternion rotation = targetRot;
					rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y + num * (float)circlingDirection, rotation.eulerAngles.z);
					base.transform.rotation = rotation;
				}
				else if (currentPattern == V2Pattern.Chase)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * 350f * eid.totalSpeedModifier);
					bool flag = playerInSight && gc.onGround && !jumping;
					bool flag2 = Vector3.Distance(base.transform.position, targetPos) > 20f || (difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp > 50 && Vector3.Distance(base.transform.position, targetPos) > 10f);
					if (base.transform.rotation == targetRot && flag && flag2)
					{
						Slide();
					}
				}
				if (currentPattern != V2Pattern.Circle && gc.onGround && difficulty >= 4 && (float)randomSlideCheck > 0.5f)
				{
					randomSlideCheck = 0f;
					if (Random.Range(0f, 1f) > 0.75f)
					{
						Slide();
					}
				}
			}
		}
		if (secondEncounter)
		{
			LookForCoins();
		}
		if (shootCooldown <= 0f && aiming && (!(Object)(object)nma || playerInSight))
		{
			ShootCheck();
		}
		if (slideOnly)
		{
			return;
		}
		if (inPattern && playerInSight)
		{
			if (!jumping)
			{
				if (gc.onGround)
				{
					if (Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, 4f, LayerMaskDefaults.Get(LMD.Environment)))
					{
						Jump();
					}
				}
				else if (wc.onGround)
				{
					WallJump();
				}
			}
			else if (wc.onGround && gc.onGround)
			{
				ChangeDirection(Random.Range(100, 260));
			}
		}
		if ((currentPattern == V2Pattern.Circle && !cowardPattern) || Vector3.Distance(base.transform.position, target.position) < 10f)
		{
			closeRangePatience = Mathf.MoveTowards(closeRangePatience, 0f, Time.deltaTime * ((currentPattern == V2Pattern.Circle) ? 1f : 1.5f) * eid.totalSpeedModifier);
			if (closeRangePatience <= 0f && !dodging && dodgeLeft <= 0f && !enraged && (MonoSingleton<NewMovement>.Instance.hp > 33 || difficulty >= 4))
			{
				closeRangePatience = 1f;
				ForceDodge(base.transform.position - targetPos);
				if (!cowardPattern && currentPattern != V2Pattern.Circle)
				{
					cowardPattern = true;
					SwitchPattern(V2Pattern.Coward);
				}
			}
		}
		else
		{
			closeRangePatience = Mathf.MoveTowards(closeRangePatience, 5f, Time.deltaTime * eid.totalSpeedModifier);
			if (cowardPattern && closeRangePatience > 2f)
			{
				cowardPattern = false;
				CheckPattern();
				SwitchPattern(currentPattern);
			}
		}
	}

	private void IntroUpdate()
	{
		if (staringAtPlayer)
		{
			targetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			targetRot = Quaternion.LookRotation(targetPos - base.transform.position, Vector3.up);
			((Component)(object)anim).transform.rotation = Quaternion.RotateTowards(((Component)(object)anim).transform.rotation, targetRot, Time.deltaTime * 10f * Quaternion.Angle(((Component)(object)anim).transform.rotation, targetRot));
		}
		if (!gc.onGround)
		{
			return;
		}
		GameObject gameObject = null;
		if (longIntro)
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			if (!introHitGround)
			{
				if (eid.enemyType != EnemyType.BigJohnator)
				{
					anim.SetTrigger("Intro");
				}
				introHitGround = true;
				if (anim.layerCount > 1)
				{
					anim.SetLayerWeight(1, 0f);
					anim.SetLayerWeight(2, 0f);
				}
				gameObject = Object.Instantiate(shockwave, base.transform.position, Quaternion.identity);
			}
		}
		else
		{
			inIntro = false;
			active = true;
			if ((bool)bhb)
			{
				bhb.enabled = true;
			}
			gameObject = Object.Instantiate(shockwave, base.transform.position, Quaternion.identity);
		}
		if ((bool)gameObject && gameObject.TryGetComponent<PhysicalShockwave>(out var component))
		{
			component.enemyType = EnemyType.V2;
		}
	}

	private void UpdateCooldowns()
	{
		if (patternCooldown > 0f)
		{
			patternCooldown = Mathf.MoveTowards(patternCooldown, 0f, Time.deltaTime);
		}
		if (shootCooldown > 0f || altShootCooldown > 0f)
		{
			float num = 1f;
			if (difficulty == 1)
			{
				num = 0.85f;
			}
			if (difficulty == 0)
			{
				num = 0.75f;
			}
			if (shootCooldown > 0f)
			{
				shootCooldown = Mathf.MoveTowards(shootCooldown, 0f, Time.deltaTime * num * (cowardPattern ? 0.5f : 1f) * eid.totalSpeedModifier);
			}
			if (altShootCooldown > 0f)
			{
				altShootCooldown = Mathf.MoveTowards(altShootCooldown, 0f, Time.deltaTime * num * eid.totalSpeedModifier);
			}
		}
		if (dodgeCooldown < 6f)
		{
			float num2 = 1f;
			switch (difficulty)
			{
			case 4:
			case 5:
				num2 = 1f;
				break;
			case 3:
				num2 = 0.5f;
				break;
			case 0:
			case 1:
			case 2:
				num2 = 0.1f;
				break;
			}
			dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 6f, Time.deltaTime * num2 * eid.totalSpeedModifier);
		}
		if (dodgeLeft > 0f)
		{
			dodgeLeft = Mathf.MoveTowards(dodgeLeft, 0f, Time.deltaTime * 3f * eid.totalSpeedModifier);
			if (dodgeLeft <= 0f)
			{
				DodgeEnd();
			}
		}
		if (secondEncounter && (coins.Count == 0 || (aboutToShoot && shootingForCoin)))
		{
			switch (difficulty)
			{
			case 4:
			case 5:
				coinsInSightCooldown = 0f;
				break;
			case 3:
				coinsInSightCooldown = 0.2f;
				break;
			case 2:
				coinsInSightCooldown = 0.4f;
				break;
			case 1:
				coinsInSightCooldown = 0.6f;
				break;
			case 0:
				coinsInSightCooldown = 0.8f;
				break;
			}
		}
		if (inPattern)
		{
			DistancePatience();
		}
	}

	private void DistancePatience()
	{
		if (target == null)
		{
			return;
		}
		bool flag = Vector3.Distance(base.transform.position, target.position) > 15f || !playerInSight;
		distancePatience = Mathf.MoveTowards(distancePatience, flag ? 12 : 0, Time.deltaTime * (float)(enraged ? 1 : 2) * eid.totalSpeedModifier);
		if (flag)
		{
			if (currentPattern != V2Pattern.Chase && (distancePatience >= 4f || Vector3.Distance(base.transform.position, target.position) > 30f))
			{
				currentPattern = V2Pattern.Chase;
				SwitchPattern(V2Pattern.Chase);
			}
			if (distancePatience == 12f && !enraged)
			{
				Enrage();
			}
		}
		else if (enraged && distancePatience < 10f)
		{
			UnEnrage();
		}
	}

	private void UpdateAnimations()
	{
		anim.SetBool("InAir", dodging || !gc.onGround);
		bool flag = !dodging && gc.onGround && !sliding && running;
		bool flag2 = dodging || sliding || !gc.onGround;
		if (drags.Length != 0 && drags[0].active != flag2)
		{
			DragBehind[] array = drags;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].active = flag2;
			}
		}
		if (!flag)
		{
			if (anim.layerCount > 1)
			{
				anim.SetLayerWeight(1, 0f);
			}
			if (anim.layerCount > 2)
			{
				anim.SetLayerWeight(2, 0f);
			}
			return;
		}
		float num = Quaternion.Angle(((Component)(object)anim).transform.rotation, base.transform.rotation);
		anim.SetBool("RunningLeft", ((Component)(object)anim).transform.rotation.eulerAngles.y > base.transform.rotation.eulerAngles.y);
		anim.SetBool("RunningBack", num > 90f);
		if (anim.layerCount > 1)
		{
			anim.SetLayerWeight(1, 1f);
		}
		if (anim.layerCount > 2)
		{
			if (num <= 90f)
			{
				anim.SetLayerWeight(2, num / 90f);
			}
			else
			{
				anim.SetLayerWeight(2, Mathf.Abs(-180f + num) / 90f);
			}
		}
	}

	private void UpdateBossSecondaryBar()
	{
		if (!bhb)
		{
			return;
		}
		if (enraged)
		{
			bhb.UpdateSecondaryBar((distancePatience - 10f) / 2f);
			flashTimer = Mathf.MoveTowards(flashTimer, 1f, Time.deltaTime * 5f);
			bhb.SetSecondaryBarColor((flashTimer < 0.5f) ? Color.red : Color.black);
			if (flashTimer >= 1f)
			{
				flashTimer = 0f;
			}
		}
		else
		{
			bhb.UpdateSecondaryBar(distancePatience / 12f);
			if (distancePatience < 4f)
			{
				bhb.SetSecondaryBarColor(Color.green);
			}
			else if (distancePatience < 8f)
			{
				bhb.SetSecondaryBarColor(Color.yellow);
			}
			else
			{
				bhb.SetSecondaryBarColor(new Color(1f, 0.35f, 0f));
			}
		}
	}

	private void ShootCheck()
	{
		float num = Vector3.Distance(target.position, base.transform.position);
		if (!aboutToShoot)
		{
			if (num <= 15f)
			{
				SwitchWeapon(1);
			}
			else if (weapons.Length > 2 && num < 25f && eid.stuckMagnets.Count <= 0)
			{
				SwitchWeapon(2);
			}
			else
			{
				SwitchWeapon(0);
			}
		}
		if (Physics.Raycast(base.transform.position + Vector3.up * 2f, target.position - base.transform.position, out rhit, Vector3.Distance(base.transform.position, target.position), LayerMaskDefaults.Get(LMD.Environment)))
		{
			if (altShootCooldown <= 0f && rhit.transform != null && rhit.transform.gameObject.CompareTag("Breakable"))
			{
				predictAmount = 0f;
				aimAtGround = false;
				if (distancePatience >= 4f)
				{
					shootCooldown = 1f;
				}
				else
				{
					shootCooldown = ((difficulty > 2) ? Random.Range(1f, 2f) : 2f);
				}
				altShootCooldown = 5f;
				weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire");
				aboutToShoot = true;
				chargingAlt = true;
				Invoke("AltShootWeapon", 1f / eid.totalSpeedModifier);
			}
			return;
		}
		aboutToShoot = true;
		if (altShootCooldown <= 0f || (distancePatience >= 8f && currentWeapon == 0 && !dontEnrage))
		{
			aimAtGround = currentWeapon != 0 || weapons.Length == 1;
			if (currentWeapon == 0)
			{
				predictAmount = 0.15f / eid.totalSpeedModifier;
			}
			else if (currentWeapon == 1 || difficulty > 2)
			{
				predictAmount = 0.25f / eid.totalSpeedModifier;
			}
			else
			{
				predictAmount = -0.25f / eid.totalSpeedModifier;
			}
			shootCooldown = ((difficulty > 2) ? Random.Range(1f, 2f) : 2f);
			altShootCooldown = 5f;
			if (secondEncounter && num >= 8f && !enraged && Random.Range(0f, 1f) < 0.5f)
			{
				SwitchWeapon(0);
				coinsToThrow = ((difficulty < 2) ? 1 : 3);
				ThrowCoins();
				return;
			}
			chargingAlt = true;
			weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareAltFire", SendMessageOptions.DontRequireReceiver);
			float num2 = 1f;
			switch (difficulty)
			{
			case 2:
			case 3:
			case 4:
			case 5:
				num2 = 1f;
				break;
			case 1:
				num2 = 1.25f;
				break;
			case 0:
				num2 = 1.5f;
				break;
			}
			Invoke("AltShootWeapon", num2 / eid.totalSpeedModifier);
			return;
		}
		if (currentWeapon == 0)
		{
			predictAmount = 0f;
		}
		else if (currentWeapon == 1 || difficulty > 2)
		{
			predictAmount = 0.15f / eid.totalSpeedModifier;
		}
		else
		{
			predictAmount = -0.25f / eid.totalSpeedModifier;
		}
		if (currentWeapon == 0 && distancePatience >= 4f)
		{
			shootCooldown = 1f;
		}
		else
		{
			shootCooldown = ((difficulty > 2) ? Random.Range(1.5f, 2f) : 2f);
		}
		weapons[currentWeapon].transform.GetChild(0).SendMessage("PrepareFire", SendMessageOptions.DontRequireReceiver);
		if (currentWeapon == 0)
		{
			shootingForCoin = false;
			Flash();
			if (difficulty >= 2)
			{
				Invoke("ShootWeapon", 0.75f / eid.totalSpeedModifier);
			}
			if (difficulty >= 1)
			{
				Invoke("ShootWeapon", 0.95f / eid.totalSpeedModifier);
			}
			Invoke("ShootWeapon", 1.15f / eid.totalSpeedModifier);
			return;
		}
		float num3 = 1f;
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			num3 = 0.75f;
			break;
		case 1:
			num3 = 1f;
			break;
		case 0:
			num3 = 1.25f;
			break;
		}
		Invoke("ShootWeapon", num3 / eid.totalSpeedModifier);
	}

	private void CoinUpdate()
	{
		Invoke("CoinUpdate", 0.1f);
		coins = MonoSingleton<CoinTracker>.Instance.revolverCoinsList;
	}

	private void LookForCoins()
	{
		if ((bool)overrideTarget && (enraged || coins.Count == 0))
		{
			DontLookForCoins();
		}
		else if (coins.Count == 0)
		{
			shootingForCoin = false;
		}
		else
		{
			if (coinsToThrow > 0 || enraged)
			{
				return;
			}
			Coin coin = null;
			float num = 60f;
			foreach (Coin coin2 in coins)
			{
				if (eid.difficultyOverride >= 0)
				{
					coin2.difficulty = eid.difficultyOverride;
				}
				if (!coin2.shot)
				{
					float num2 = Vector3.Distance(coin2.transform.position, aimAtTarget[1].position);
					if (num2 < num && !Physics.Raycast(aimAtTarget[1].position, coin2.transform.position - aimAtTarget[1].position, num2, LayerMaskDefaults.Get(LMD.Environment)))
					{
						num = num2;
						coin = coin2;
					}
				}
			}
			if (coin == null)
			{
				DontLookForCoins();
				return;
			}
			if (coinsInSightCooldown > 0f)
			{
				coinsInSightCooldown = Mathf.MoveTowards(coinsInSightCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				return;
			}
			if (overrideTarget != coin.transform)
			{
				overrideTarget = coin.transform;
				overrideTargetRb = coin.GetComponent<Rigidbody>();
			}
			if (currentWeapon == 0 && aboutToShoot && shootingForCoin)
			{
				return;
			}
			if (currentWeapon != 0 || !shootingForCoin)
			{
				CancelInvoke("ShootWeapon");
				CancelInvoke("AltShootWeapon");
				weapons[currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
				if (currentWeapon != 0)
				{
					SwitchWeapon(0);
				}
			}
			shootCooldown = 1f;
			shootingForCoin = true;
			aboutToShoot = true;
			Flash();
			Invoke("ShootWeapon", 0.4f / eid.totalSpeedModifier);
		}
	}

	private void DontLookForCoins()
	{
		if (shootingForCoin && aboutToShoot)
		{
			CancelInvoke("ShootWeapon");
		}
		shootingForCoin = false;
		overrideTarget = null;
	}

	private void Flash()
	{
		Object.Instantiate(gunFlash, aimAtTarget[aimAtTarget.Length - 1].transform.position, Quaternion.LookRotation(target.position - aimAtTarget[aimAtTarget.Length - 1].transform.position)).transform.localScale *= 20f;
	}

	private void ThrowCoins()
	{
		if (coinsToThrow != 0)
		{
			GameObject gameObject = Object.Instantiate(coin, base.transform.position, base.transform.rotation);
			if (gameObject.TryGetComponent<Rigidbody>(out var component))
			{
				component.AddForce((target.position - ((Component)(object)anim).transform.position).normalized * 20f + Vector3.up * 30f, ForceMode.VelocityChange);
			}
			if (gameObject.TryGetComponent<Coin>(out var component2))
			{
				GameObject obj = Object.Instantiate(component2.flash, component2.transform.position, MonoSingleton<CameraController>.Instance.transform.rotation);
				obj.transform.localScale *= 2f;
				obj.transform.SetParent(gameObject.transform, worldPositionStays: true);
			}
			coinsToThrow--;
			if (coinsToThrow > 0)
			{
				Invoke("ThrowCoins", 0.2f / eid.totalSpeedModifier);
			}
			else
			{
				aboutToShoot = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (escaping)
		{
			EscapeUpdate();
		}
		else if (target != null && active && (inPattern || dodging))
		{
			playerInSight = !(Object)(object)nma || !Physics.Raycast(base.transform.position + Vector3.up, target.position - (base.transform.position + Vector3.up), Vector3.Distance(target.position, base.transform.position + Vector3.up), LayerMaskDefaults.Get(LMD.Environment));
			if (running)
			{
				Move();
			}
		}
	}

	private void EscapeUpdate()
	{
		if ((bool)(Object)(object)nma && nma.isOnNavMesh)
		{
			rb.isKinematic = true;
			if ((bool)mac && !mac.knockedBack)
			{
				nma.updatePosition = true;
				nma.updateRotation = true;
			}
			nma.SetDestination(escapeTarget.position);
			return;
		}
		rb.isKinematic = false;
		targetPos = new Vector3(escapeTarget.position.x, base.transform.position.y, escapeTarget.position.z);
		if (Vector3.Distance(base.transform.position, targetPos) > 8f || (escapeTarget.position.y < base.transform.position.y + 5f && Vector3.Distance(base.transform.position, escapeTarget.position) > 1f))
		{
			aiming = false;
			inPattern = false;
			base.transform.LookAt(targetPos);
			((Component)(object)anim).transform.LookAt(targetPos);
			rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed);
		}
		else
		{
			GetComponent<Collider>().enabled = false;
			if (escapeTarget.position.y > base.transform.position.y)
			{
				if (!jumping && gc.onGround && !slideOnly)
				{
					Jump();
				}
				rb.velocity = targetPos - base.transform.position + Vector3.up * 50f;
			}
		}
		if (base.transform.position.y > escapeTarget.position.y - 20f && spawnOnDeath != null)
		{
			spawnOnDeath.SetActive(value: true);
			spawnOnDeath.transform.position = base.transform.position;
			spawnOnDeath = null;
		}
	}

	private void ShootWeapon()
	{
		SharedShootWeapon(isAlt: false);
		predictAmount = 0f;
	}

	private void AltShootWeapon()
	{
		SharedShootWeapon(isAlt: true);
		chargingAlt = false;
	}

	private void SharedShootWeapon(bool isAlt)
	{
		if (!aiming)
		{
			return;
		}
		IEnemyWeapon component = weapons[currentWeapon].transform.GetChild(0).GetComponent<IEnemyWeapon>();
		if (component != null)
		{
			component.UpdateTarget(target);
			if (isAlt)
			{
				component.AltFire();
			}
			else
			{
				component.Fire();
			}
		}
		if (!enraged || !isAlt)
		{
			predictAmount = 0f;
		}
		aboutToShoot = false;
		aimAtGround = false;
	}

	private void Move()
	{
		if (eid.target == null)
		{
			return;
		}
		if ((bool)(Object)(object)nma)
		{
			if (nma.isOnNavMesh && gc.onGround && !playerInSight && !dodging && !sliding)
			{
				((Behaviour)(object)nma).enabled = true;
				mac.SetDestination(target.position);
				nma.speed = movementSpeed * ((distancePatience > 4f && !enraged) ? 1.5f : 1f);
				return;
			}
			if (!nma.isOnOffMeshLink)
			{
				((Behaviour)(object)nma).enabled = false;
			}
		}
		rb.isKinematic = false;
		if (dodging)
		{
			rb.velocity = new Vector3(base.transform.forward.x * (movementSpeed * 5f * dodgeLeft), 0f, base.transform.forward.z * (movementSpeed * 5f * dodgeLeft));
		}
		else if (sliding)
		{
			if (!slideOnly)
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed * 2f, rb.velocity.y, base.transform.forward.z * movementSpeed * (float)((distancePatience > 4f && !enraged) ? 3 : 2));
				return;
			}
			Vector3 vector = target.position + (base.transform.position - target.position).normalized * 10f;
			Vector3 normalized = new Vector3(vector.x - base.transform.position.x, 0f, vector.z - base.transform.position.z).normalized;
			rb.velocity = Vector3.MoveTowards(rb.velocity, normalized * movementSpeed * Mathf.Max(1f, (float)difficulty / 1.75f), Time.fixedDeltaTime * 75f);
			Quaternion to = Quaternion.LookRotation(forceSlideDirection, Vector3.up);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 360f);
			if (difficulty >= 2)
			{
				closeRangePatience = Mathf.MoveTowards(closeRangePatience, (Vector3.Distance(target.position, base.transform.position) < 8f) ? 1 : 0, Time.deltaTime * eid.totalSpeedModifier);
				if (closeRangePatience >= 1f)
				{
					closeRangePatience = 0.65f;
					ForceDodge((base.transform.position - targetPos).normalized + base.transform.right * Random.Range(-1f, 1f));
				}
			}
		}
		else if (gc.onGround && !jumping)
		{
			if (distancePatience > 4f && !enraged)
			{
				rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed * 1.5f);
				return;
			}
			float num = 1f;
			if (MonoSingleton<NewMovement>.Instance.hp <= 33 && difficulty <= 3)
			{
				num -= 0.1f;
			}
			if (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && distancePatience < 4f)
			{
				num -= 0.1f;
			}
			rb.velocity = new Vector3(base.transform.forward.x * movementSpeed, rb.velocity.y, base.transform.forward.z * movementSpeed * num * (slowMode ? 0.75f : 1f));
		}
		else
		{
			bool flag = Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2;
			Vector3 vector2 = ((!(slowMode || flag)) ? new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * ((distancePatience >= 4f && !enraged) ? 3f : 2.5f), rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * ((distancePatience >= 4f && !enraged) ? 3f : 2.5f)) : new Vector3(base.transform.forward.x * movementSpeed * Time.deltaTime * 1.25f, rb.velocity.y, base.transform.forward.z * movementSpeed * Time.deltaTime * ((distancePatience >= 4f) ? 2f : 1.25f)));
			Vector3 zero = Vector3.zero;
			if ((vector2.x > 0f && rb.velocity.x < vector2.x) || (vector2.x < 0f && rb.velocity.x > vector2.x))
			{
				zero.x = vector2.x;
			}
			if ((vector2.z > 0f && rb.velocity.z < vector2.z) || (vector2.z < 0f && rb.velocity.z > vector2.z))
			{
				zero.z = vector2.z;
			}
			rb.AddForce(zero.normalized * airAcceleration);
		}
	}

	private void LateUpdate()
	{
		if (target != null && (escaping || (active && aiming)))
		{
			Vector3 position = target.position;
			Rigidbody rigidbody = eid.target.rigidbody;
			if (difficulty <= 1)
			{
				predictAmount = 0f;
			}
			if (escaping)
			{
				predictAmount = 0f;
				position = escapeTarget.position;
			}
			else if ((bool)overrideTarget)
			{
				predictAmount = 0.05f * (Vector3.Distance(overrideTarget.position, base.transform.position) / 20f);
				position = overrideTarget.position;
				rigidbody = overrideTargetRb;
			}
			else if (Vector3.Distance(base.transform.position, targetPos) < 8f)
			{
				predictAmount *= 0.2f;
			}
			if (aimAtTarget.Length == 1 && (aimAtGround || alwaysAimAtGround))
			{
				aimAtTarget[0].LookAt(position + Vector3.down * 2.5f + rigidbody.velocity * (Vector3.Distance(position, aimAtTarget[0].position) * (predictAmount / 10f)));
			}
			else
			{
				aimAtTarget[0].LookAt(position + rigidbody.velocity * (Vector3.Distance(position, aimAtTarget[0].position) * (predictAmount / 10f)));
			}
			aimAtTarget[0].Rotate(Vector3.right, 10f, Space.Self);
			if (aimAtTarget.Length > 1)
			{
				Quaternion quaternion = Quaternion.LookRotation(((aimAtGround || alwaysAimAtGround) ? rigidbody.transform.position : position) + rigidbody.velocity * predictAmount - aimAtTarget[1].position, Vector3.up);
				quaternion = Quaternion.Euler(quaternion.eulerAngles.x + 90f, quaternion.eulerAngles.y, quaternion.eulerAngles.z);
				aimAtTarget[1].rotation = quaternion;
				aimAtTarget[1].Rotate(Vector3.up, 180f, Space.Self);
			}
		}
	}

	private void Jump()
	{
		jumping = true;
		Invoke("NotJumping", 0.25f);
		if (anim.layerCount > 1)
		{
			anim.SetLayerWeight(1, 1f);
			anim.SetLayerWeight(2, 0f);
		}
		if (eid.enemyType != EnemyType.BigJohnator)
		{
			anim.SetTrigger("Jump");
		}
		bool flag = slowMode || (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp <= 33 && !enraged);
		if (dodging)
		{
			Object.Instantiate(dashJumpSound);
			rb.AddForce(Vector3.up * jumpPower * 1500f * (flag ? 0.75f : 1.5f));
			return;
		}
		Object.Instantiate(jumpSound, base.transform.position, Quaternion.identity);
		if (sliding)
		{
			rb.AddForce(Vector3.up * jumpPower * 1500f * (flag ? 1 : 2));
			StopSlide();
		}
		else
		{
			rb.AddForce(Vector3.up * jumpPower * 1500f * (flag ? 1.25f : 2.5f));
		}
	}

	private void WallJump()
	{
		jumping = true;
		Invoke("NotJumping", 0.25f);
		if (sliding)
		{
			StopSlide();
		}
		if (Object.Instantiate(jumpSound, base.transform.position, Quaternion.identity).TryGetComponent<AudioSource>(out var component))
		{
			component.SetPitch(2f);
		}
		CheckPattern();
		Vector3 vector = base.transform.position - wc.ClosestPoint();
		Vector3 vector2 = new Vector3(vector.normalized.x * 3f, 0.75f, vector.normalized.z * 3f);
		rb.velocity = Vector3.zero;
		switch (currentPattern)
		{
		case V2Pattern.Straight:
		{
			Quaternion rotation = ((Component)(object)anim).transform.rotation;
			Vector3 forward = new Vector3(vector.normalized.x, 0f, vector.normalized.z);
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			((Component)(object)anim).transform.rotation = rotation;
			ChangeDirection(Random.Range(-90, 90));
			break;
		}
		case V2Pattern.Circle:
			circlingDirection = ((circlingDirection < 0) ? 1 : (-1));
			break;
		case V2Pattern.Chase:
		{
			Quaternion rotation = ((Component)(object)anim).transform.rotation;
			base.transform.LookAt(targetPos);
			((Component)(object)anim).transform.rotation = rotation;
			break;
		}
		}
		bool flag = slowMode || (Vector3.Distance(base.transform.position, targetPos) < 10f && difficulty <= 2 && MonoSingleton<NewMovement>.Instance.hp <= 33 && !enraged);
		float num = 2000f;
		if (difficulty == 0)
		{
			num = 500f;
		}
		else if (difficulty == 1 || flag)
		{
			num = 1000f;
		}
		rb.AddForce(vector2 * wallJumpPower * num);
	}

	private void NotJumping()
	{
		jumping = false;
	}

	private void CheckPattern()
	{
		if (cowardPattern || patternCooldown > 0f || distancePatience >= 4f)
		{
			return;
		}
		int num = (int)currentPattern;
		int num2 = Random.Range(0, 3);
		if (num2 == num)
		{
			patternCooldown = Random.Range(0.5f, 1f);
			if (num2 == 1)
			{
				closeRangePatience += 1f;
			}
			return;
		}
		patternCooldown = Random.Range(2, 5);
		SwitchPattern((V2Pattern)num2);
		if (currentPattern == V2Pattern.Circle)
		{
			circlingDirection = ((!(Random.Range(0f, 1f) > 0.5f)) ? 1 : (-1));
		}
	}

	private void ChangeDirection(float degrees)
	{
		Quaternion rotation = ((Component)(object)anim).transform.rotation;
		base.transform.Rotate(base.transform.up, degrees, Space.World);
		((Component)(object)anim).transform.rotation = rotation;
	}

	public void Dodge(Transform projectile)
	{
		if (target == null || !active || dodgeLeft > 0f || chargingAlt || Vector3.Distance(base.transform.position, target.position) <= 15f)
		{
			return;
		}
		if (dodgeCooldown >= (float)(6 - difficulty))
		{
			dodgeCooldown -= 6 - difficulty;
			Vector3 direction = new Vector3(base.transform.position.x - projectile.position.x, 0f, base.transform.position.z - projectile.position.z);
			if (currentPattern == V2Pattern.Chase)
			{
				direction = direction.normalized + (targetPos - base.transform.position).normalized;
			}
			DodgeNow(direction);
			ChangeDirection((Random.Range(0f, 1f) > 0.5f) ? 90 : (-90));
		}
		else
		{
			if (!gc.onGround || jumping || slideOnly)
			{
				return;
			}
			if (cowardPattern)
			{
				Jump();
				return;
			}
			float num = Random.Range(0f, (difficulty >= 3) ? 2f : 3f);
			if (!(num > 1f))
			{
				if (num > 0.75f)
				{
					Jump();
				}
				else
				{
					Slide();
				}
			}
		}
	}

	public void ForceDodge(Vector3 direction)
	{
		DodgeNow(direction);
	}

	private void DodgeNow(Vector3 direction)
	{
		if (sliding && !slideOnly)
		{
			StopSlide();
		}
		dodgeLeft = 1f;
		dodging = true;
		eid.hookIgnore = true;
		inPattern = false;
		Object.Instantiate(dodgeEffect, base.transform.position + Vector3.up * 2f, base.transform.rotation);
		direction = new Vector3(direction.x, 0f, direction.z);
		base.transform.LookAt(base.transform.position + direction);
		if (!slideOnly && eid.enemyType != EnemyType.BigJohnator)
		{
			anim.SetTrigger("Jump");
		}
	}

	private void DodgeEnd()
	{
		dodgeLeft = 0f;
		dodging = false;
		eid.hookIgnore = false;
		inPattern = true;
		CheckPattern();
		if (currentPattern == V2Pattern.Chase && !cowardPattern)
		{
			Quaternion rotation = ((Component)(object)anim).transform.rotation;
			base.transform.LookAt(targetPos);
			((Component)(object)anim).transform.rotation = rotation;
		}
	}

	private void Slide()
	{
		anim.SetBool("Sliding", true);
		sliding = true;
		slideEffect.SetActive(value: true);
		slideStopTimer = 0.2f;
	}

	private void StopSlide()
	{
		anim.SetBool("Sliding", false);
		sliding = false;
		slideEffect.SetActive(value: false);
		CheckPattern();
	}

	private void SwitchWeapon(int weapon)
	{
		if (currentWeapon != weapon && weapons.Length > weapon)
		{
			currentWeapon = weapon;
			for (int i = 0; i < weapons.Length; i++)
			{
				weapons[i].SetActive(i == weapon);
			}
		}
	}

	public void SwitchPattern(V2Pattern targetPattern)
	{
		if (currentWingChangeEffect != null)
		{
			Object.Destroy(currentWingChangeEffect);
		}
		EnemySimplifier[] array = ensims;
		foreach (EnemySimplifier enemySimplifier in array)
		{
			if (enemySimplifier.matList != null && enemySimplifier.matList.Length > 1)
			{
				enemySimplifier.matList[1].mainTexture = wingTextures[(int)targetPattern];
			}
		}
		TrailRenderer[] array2 = wingTrails;
		foreach (TrailRenderer trailRenderer in array2)
		{
			if ((bool)trailRenderer)
			{
				trailRenderer.startColor = new Color(wingColors[(int)targetPattern].r, wingColors[(int)targetPattern].g, wingColors[(int)targetPattern].b, 0.5f);
			}
		}
		currentWingChangeEffect = Object.Instantiate(wingChangeEffect, base.transform.position + Vector3.up * 2f, Quaternion.identity);
		if (currentWingChangeEffect.TryGetComponent<Light>(out var component))
		{
			component.color = wingColors[(int)targetPattern];
		}
		if (targetPattern < V2Pattern.Chase && currentWingChangeEffect.TryGetComponent<AudioSource>(out var component2))
		{
			component2.SetPitch((targetPattern == V2Pattern.Straight) ? 1.5f : 1.25f);
		}
	}

	public void Die()
	{
		if (!dontDie || dead)
		{
			return;
		}
		dead = true;
		if (bossVersion)
		{
			MonoSingleton<MusicManager>.Instance.off = true;
			KnockedOut(secondEncounter ? "Flailing" : "KnockedDown");
			return;
		}
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			eid.DeliverDamage(enemyIdentifierIdentifier.gameObject, Vector3.zero, enemyIdentifierIdentifier.transform.position, 10f, tryForExplode: false);
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	public void KnockedOut(string triggerName = "KnockedDown")
	{
		active = false;
		inPattern = false;
		aiming = false;
		inIntro = false;
		((Component)(object)anim).transform.LookAt(new Vector3(target.position.x, ((Component)(object)anim).transform.position.y, target.position.z));
		if (eid.enemyType != EnemyType.BigJohnator)
		{
			anim.SetTrigger(triggerName);
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
		}
		if (secondEncounter && dead)
		{
			rb.constraints = RigidbodyConstraints.None;
			rb.velocity = new Vector3(0f, 15f, 0f);
			rb.AddTorque(-180f, Random.Range(-35, 35), Random.Range(-35, 35), ForceMode.VelocityChange);
			rb.SetGravityMode(useGravity: false);
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		if ((bool)KoScream)
		{
			Object.Instantiate(KoScream, base.transform.position, Quaternion.identity);
		}
		weapons[currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
		eidids = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<Collider>(out var component))
			{
				component.enabled = false;
			}
		}
		onKnockout.Invoke();
		UnEnrage();
		if ((bool)(Object)(object)nma)
		{
			mac.StopKnockBack();
			nma.speed = 25f;
		}
	}

	public void Undie()
	{
		active = true;
		inPattern = true;
		aiming = true;
		eid.totalDamageTakenMultiplier = 1f;
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<Collider>(out var component))
			{
				component.enabled = true;
			}
		}
	}

	public void IntroEnd()
	{
		inIntro = false;
		active = true;
		staringAtPlayer = false;
		EnemyIdentifierIdentifier[] array = eidids;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<Collider>(out var component))
			{
				component.enabled = true;
			}
		}
		if ((bool)bhb)
		{
			bhb.enabled = true;
		}
		longIntro = false;
		MonoSingleton<StatueIntroChecker>.Instance.beenSeen = true;
	}

	public void StareAtPlayer()
	{
		staringAtPlayer = true;
	}

	public void BeginEscape()
	{
		escaping = true;
		anim.SetLayerWeight(1, 1f);
		anim.SetLayerWeight(2, 0f);
		anim.SetBool("RunningBack", false);
		anim.SetBool("InAir", false);
		base.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
		((Component)(object)anim).transform.LookAt(new Vector3(target.position.x, ((Component)(object)anim).transform.position.y, target.position.z));
		if (gc.onGround && (bool)(Object)(object)nma && !mac.knockedBack)
		{
			((Behaviour)(object)nma).enabled = true;
		}
	}

	public void InstaEnrage()
	{
		distancePatience = 12f;
		Enrage("STOP HITTING YOURSELF");
	}

	public void Enrage()
	{
		Enrage("");
	}

	public void Enrage(string enrageName)
	{
		if (!dontEnrage && !enraged)
		{
			enraged = true;
			currentEnrageEffect = Object.Instantiate(enrageEffect, mac.chest.transform.position, base.transform.rotation);
			currentEnrageEffect.transform.SetParent(mac.chest.transform, worldPositionStays: true);
			if (!string.IsNullOrEmpty(enrageName) && currentEnrageEffect.TryGetComponent<EnrageEffect>(out var component))
			{
				component.styleNameOverride = enrageName;
			}
			if (!currentEnrageEffect.activeSelf)
			{
				currentEnrageEffect.SetActive(value: true);
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = true;
			}
			movementSpeed = originalMovementSpeed * 2f;
		}
	}

	public void UnEnrage()
	{
		if (enraged)
		{
			if (currentEnrageEffect != null)
			{
				Object.Destroy(currentEnrageEffect);
			}
			enraged = false;
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			movementSpeed = originalMovementSpeed;
		}
	}

	public void SlideOnly(bool value)
	{
		slideOnly = value;
		if (value)
		{
			rb.constraints = (RigidbodyConstraints)116;
			anim.Play("Slide", 0, 0f);
		}
		else
		{
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}
}
