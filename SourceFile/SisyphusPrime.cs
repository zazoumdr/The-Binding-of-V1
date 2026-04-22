using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SisyphusPrime : EnemyScript, IHitTargetCallback
{
	private NavMeshAgent nma;

	private Animator anim;

	private Enemy mach;

	private EnemyIdentifier eid;

	private GroundCheckEnemy gce;

	private Rigidbody rb;

	private Collider col;

	private AudioSource aud;

	private float originalHp;

	private bool inAction;

	private float cooldown = 2f;

	private SPAttack lastPrimaryAttack;

	private SPAttack lastSecondaryAttack;

	private int secondariesSinceLastPrimary;

	private int attacksSinceLastExplosion;

	private Vector3 heightAdjustedTargetPos;

	private bool tracking;

	private bool fullTracking;

	private bool aiming;

	private bool jumping;

	public GameObject explosion;

	public GameObject explosionChargeEffect;

	private GameObject currentExplosionChargeEffect;

	public GameObject rubble;

	public GameObject bigRubble;

	public GameObject groundWave;

	public GameObject swoosh;

	public Transform aimingBone;

	private Transform head;

	public GameObject projectileCharge;

	private GameObject currentProjectileCharge;

	public GameObject sparkleExplosion;

	public GameObject warningFlash;

	public GameObject parryableFlash;

	private bool gravityInAction;

	public GameObject attackTrail;

	public GameObject swingSnake;

	private List<GameObject> currentSwingSnakes = new List<GameObject>();

	private bool hitSuccessful;

	private bool gotParried;

	private Vector3 teleportToGroundFailsafe;

	public Transform[] swingLimbs;

	private bool swinging;

	private SwingCheck2 sc;

	private GoreZone gz;

	private int attackAmount;

	private bool enraged;

	public GameObject passiveEffect;

	private GameObject currentPassiveEffect;

	public GameObject flameEffect;

	public GameObject phaseChangeEffect;

	private int difficulty = -1;

	private SPAttack previousCombo = SPAttack.Explosion;

	private bool activated = true;

	private bool ascending;

	private bool vibrating;

	private Vector3 origPos;

	public GameObject lightShaft;

	public GameObject outroExplosion;

	public UltrakillEvent onPhaseChange;

	public UltrakillEvent onOutroEnd;

	private Vector3 spawnPoint;

	[Header("Voice clips")]
	public AudioClip[] uppercutComboVoice;

	public AudioClip[] stompComboVoice;

	public AudioClip phaseChangeVoice;

	public AudioClip[] hurtVoice;

	public AudioClip[] explosionVoice;

	public AudioClip[] tauntVoice;

	public AudioClip[] clapVoice;

	private bool bossVersion;

	private bool taunting;

	private bool tauntCheck;

	private int attacksSinceTaunt;

	private float defaultMoveSpeed;

	private EnemyTarget target => eid.target;

	private void Awake()
	{
		nma = GetComponent<NavMeshAgent>();
		mach = GetComponent<Enemy>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		rb = GetComponent<Rigidbody>();
		sc = GetComponentInChildren<SwingCheck2>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		eid = GetComponent<EnemyIdentifier>();
		sc.OverrideEnemyIdentifier(eid);
	}

	private void Start()
	{
		defaultMoveSpeed = nma.speed;
		SetSpeed();
		head = eid.weakPoint.transform;
		originalHp = mach.health;
		gz = GoreZone.ResolveGoreZone(base.transform);
		spawnPoint = base.transform.position;
		bossVersion = TryGetComponent<BossHealthBar>(out var _);
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
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 5:
			anim.speed = 1.25f;
			break;
		case 4:
			anim.speed = 1.125f;
			break;
		case 2:
		case 3:
			anim.speed = 1f;
			break;
		case 1:
			anim.speed = 0.9f;
			break;
		case 0:
			anim.speed = 0.8f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		if ((bool)mach)
		{
			CancelInvoke();
			StopAction();
			DamageStop();
			ascending = false;
			tracking = false;
			fullTracking = false;
			aiming = false;
			jumping = false;
		}
	}

	private void OnEnable()
	{
		if (!activated)
		{
			OutroEnd();
		}
	}

	private void Update()
	{
		if (activated && target != null)
		{
			heightAdjustedTargetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			if (!inAction || taunting)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			if (!enraged && mach.health < originalHp / 2f)
			{
				Enrage();
			}
		}
		else if (ascending)
		{
			rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.up * 3f, Time.deltaTime);
			MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
		}
		else if (vibrating)
		{
			float num = (activated ? 0.25f : 0.1f);
			base.transform.position = new Vector3(origPos.x + Random.Range(0f - num, num), origPos.y + Random.Range(0f - num, num), origPos.z + Random.Range(0f - num, num));
		}
	}

	public void Enrage()
	{
		enraged = true;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("YES! That's it!");
		aud.clip = phaseChangeVoice;
		aud.SetPitch(1f);
		aud.Play(tracked: true);
		currentPassiveEffect = Object.Instantiate(passiveEffect, base.transform.position + Vector3.up * 3.5f, Quaternion.identity);
		currentPassiveEffect.transform.SetParent(base.transform);
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			Object.Instantiate(flameEffect, enemyIdentifierIdentifier.transform);
		}
		Object.Instantiate(phaseChangeEffect, mach.chest.transform.position, Quaternion.identity);
		onPhaseChange?.Invoke();
	}

	private void FixedUpdate()
	{
		if (!activated)
		{
			return;
		}
		CustomPhysics();
		if (eid.target == null)
		{
			anim.SetBool("Walking", false);
			if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
			{
				nma.isStopped = true;
			}
			return;
		}
		if ((bool)(Object)(object)nma)
		{
			if (!inAction && gce.onGround && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
			{
				if (Vector3.Distance(base.transform.position, heightAdjustedTargetPos) > 10f)
				{
					nma.isStopped = false;
					mach.SetDestination(target.GetNavPoint());
				}
				else if (cooldown > 0f)
				{
					if (!tauntCheck && attacksSinceTaunt >= (enraged ? 15 : 10) && mach.health > 20f)
					{
						Taunt();
					}
					else
					{
						LookAtTarget();
					}
					tauntCheck = true;
				}
			}
			else if (inAction)
			{
				((Behaviour)(object)nma).enabled = false;
				anim.SetBool("Walking", false);
			}
		}
		if (tracking || fullTracking)
		{
			if (!fullTracking)
			{
				base.transform.LookAt(heightAdjustedTargetPos);
			}
			else
			{
				base.transform.rotation = Quaternion.LookRotation(target.position - new Vector3(base.transform.position.x, aimingBone.position.y, base.transform.position.z));
			}
		}
		if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && !inAction)
		{
			bool flag = cooldown > 0f && Vector3.Distance(base.transform.position, heightAdjustedTargetPos) < 20f;
			nma.speed = (flag ? (defaultMoveSpeed / 2f) : defaultMoveSpeed);
			anim.SetBool("Cooldown", flag);
			anim.SetBool("Walking", nma.velocity.magnitude > 2f);
		}
	}

	private void LateUpdate()
	{
		if (aiming && inAction && activated && target != null)
		{
			aimingBone.LookAt(target.position);
			aimingBone.Rotate(Vector3.up * -90f, Space.Self);
		}
	}

	private void CustomPhysics()
	{
		if ((difficulty == 3 && ((!enraged && attackAmount >= 8) || attackAmount >= 16)) || (difficulty <= 2 && (((difficulty <= 1 || !enraged) && attackAmount >= 6) || attackAmount >= 10)))
		{
			attackAmount = 0;
			if (difficulty >= 2)
			{
				cooldown = 2f;
			}
			else
			{
				cooldown = ((difficulty == 0) ? 4 : 3);
			}
			tauntCheck = false;
		}
		if (!rb.isKinematic && rb.GetGravityMode())
		{
			rb.velocity -= Vector3.up * 100f * Time.fixedDeltaTime;
		}
		if (!jumping)
		{
			anim.SetBool("Falling", !rb.isKinematic);
		}
		else if (!inAction && rb.velocity.y < 0f)
		{
			jumping = false;
		}
		if (inAction)
		{
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = !gravityInAction;
			if (swinging && !Physics.Raycast(base.transform.position, base.transform.forward, 1f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				rb.MovePosition(base.transform.position + base.transform.forward * ((target.isPlayer && MonoSingleton<NewMovement>.Instance.sliding) ? 125 : 75) * Time.fixedDeltaTime * eid.totalSpeedModifier);
			}
			return;
		}
		gravityInAction = false;
		bool flag = gce.onGround && !jumping;
		((Behaviour)(object)nma).enabled = flag;
		rb.isKinematic = flag;
		if (!activated || target == null || cooldown > 0f || anim.IsInTransition(0))
		{
			return;
		}
		if (flag)
		{
			if (!Physics.Raycast(target.position, Vector3.down, (MonoSingleton<NewMovement>.Instance.rb.velocity.y > 0f) ? 11 : 15, LayerMaskDefaults.Get(LMD.Environment)))
			{
				PickSecondaryAttack();
			}
			else
			{
				PickAnyAttack();
			}
		}
		else if (secondariesSinceLastPrimary <= (enraged ? 3 : 2))
		{
			PickSecondaryAttack();
		}
		else if (enraged)
		{
			TeleportOnGround();
		}
	}

	private void PickAnyAttack()
	{
		if (secondariesSinceLastPrimary != 0 && (secondariesSinceLastPrimary > ((!enraged) ? 1 : 2) || Random.Range(0f, 1f) > 0.5f))
		{
			PickPrimaryAttack();
		}
		else
		{
			PickSecondaryAttack();
		}
	}

	private void PickPrimaryAttack()
	{
		PickPrimaryAttack(Random.Range(0, 3));
	}

	private void PickPrimaryAttack(int type)
	{
		if (type == (int)lastPrimaryAttack)
		{
			type = ((type != 2) ? (type + 1) : 0);
		}
		if (type == 2 && attacksSinceLastExplosion < 2)
		{
			if (lastPrimaryAttack == SPAttack.Explosion)
			{
				PickPrimaryAttack(Random.Range(0, 2));
				return;
			}
			type = ((lastPrimaryAttack == SPAttack.UppercutCombo) ? 1 : 0);
		}
		if (type == 2)
		{
			attacksSinceLastExplosion = 0;
		}
		else
		{
			attacksSinceLastExplosion++;
		}
		switch (type)
		{
		case 0:
			UppercutCombo();
			break;
		case 1:
			StompCombo();
			break;
		case 2:
			ExplodeAttack();
			break;
		}
		attacksSinceTaunt++;
		secondariesSinceLastPrimary = 0;
	}

	private void PickSecondaryAttack()
	{
		PickSecondaryAttack(Random.Range(0, 4));
	}

	private void PickSecondaryAttack(int type)
	{
		bool flag = false;
		switch (type)
		{
		case 0:
			if (lastSecondaryAttack == SPAttack.Chop)
			{
				flag = true;
			}
			break;
		case 1:
			if (lastSecondaryAttack == SPAttack.Clap)
			{
				flag = true;
			}
			break;
		case 2:
			if (lastSecondaryAttack == SPAttack.AirStomp)
			{
				flag = true;
			}
			break;
		case 3:
			if (lastSecondaryAttack == SPAttack.AirKick)
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			type = ((type != 3) ? (type + 1) : 0);
		}
		switch (type)
		{
		case 0:
			Chop();
			break;
		case 1:
			Clap();
			break;
		case 2:
			AirStomp();
			break;
		case 3:
			AirKick();
			break;
		}
		attacksSinceTaunt++;
		secondariesSinceLastPrimary++;
	}

	public void CancelIntoSecondary()
	{
		if (enraged)
		{
			PickSecondaryAttack();
		}
	}

	public void Taunt()
	{
		attacksSinceTaunt = 0;
		inAction = true;
		base.transform.LookAt(heightAdjustedTargetPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Taunt", 0, 0f);
		aiming = false;
		taunting = true;
		attackAmount += 2;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Nice try!");
		PlayVoice(tauntVoice);
	}

	public void UppercutCombo()
	{
		lastPrimaryAttack = SPAttack.UppercutCombo;
		previousCombo = SPAttack.UppercutCombo;
		inAction = true;
		base.transform.LookAt(heightAdjustedTargetPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("UppercutCombo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("DESTROY!");
		PlayVoice(uppercutComboVoice);
	}

	public void StompCombo()
	{
		lastPrimaryAttack = SPAttack.StompCombo;
		previousCombo = SPAttack.StompCombo;
		inAction = true;
		base.transform.LookAt(heightAdjustedTargetPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("StompCombo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		teleportToGroundFailsafe = base.transform.position;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("You can't escape!");
		PlayVoice(stompComboVoice);
	}

	private void Chop()
	{
		lastSecondaryAttack = SPAttack.Chop;
		TeleportSide(Random.Range(0, 2), inAir: true);
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target.position);
		gravityInAction = false;
		anim.SetTrigger("Chop");
		Unparryable();
		sc.knockBackForce = 50f;
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.down;
		aiming = false;
		attackAmount++;
	}

	private void Clap()
	{
		lastSecondaryAttack = SPAttack.Clap;
		TeleportAnywhere();
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target.position);
		gravityInAction = false;
		anim.SetTrigger("Clap");
		Parryable();
		sc.knockBackForce = 100f;
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("BE GONE!");
		PlayVoice(clapVoice);
	}

	private void AirStomp()
	{
		lastSecondaryAttack = SPAttack.AirStomp;
		TeleportAbove();
		tracking = true;
		fullTracking = false;
		inAction = true;
		base.transform.LookAt(target.position);
		gravityInAction = false;
		anim.SetTrigger("AirStomp");
		Unparryable();
		aiming = false;
		attackAmount++;
	}

	private void AirKick()
	{
		lastSecondaryAttack = SPAttack.AirKick;
		TeleportAnywhere(predictive: true);
		tracking = false;
		fullTracking = false;
		inAction = true;
		gravityInAction = false;
		anim.SetTrigger("AirKick");
		Parryable();
		sc.knockBackForce = 100f;
		sc.ignoreSlidingPlayer = true;
		aiming = false;
		attackAmount++;
	}

	private void ExplodeAttack()
	{
		TeleportAnywhere();
		lastPrimaryAttack = SPAttack.Explosion;
		tracking = true;
		fullTracking = true;
		inAction = true;
		base.transform.LookAt(target.position);
		gravityInAction = false;
		anim.SetTrigger("Explosion");
		aiming = false;
		attackAmount++;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("This will hurt.");
		PlayVoice(explosionVoice);
	}

	public void ClapStart()
	{
		SnakeSwingStart(0);
		SnakeSwingStart(1);
	}

	public void ClapShockwave()
	{
		PhysicalShockwave physicalShockwave = CreateShockwave(Vector3.Lerp(swingLimbs[0].position, swingLimbs[1].position, 0.5f));
		if (!(physicalShockwave == null))
		{
			physicalShockwave.transform.rotation = base.transform.rotation;
			physicalShockwave.transform.Rotate(Vector3.forward * 90f, Space.Self);
			physicalShockwave.speed *= 2f;
		}
	}

	public void StompShockwave()
	{
		CreateShockwave(new Vector3(swingLimbs[2].position.x, base.transform.position.y, swingLimbs[2].position.z));
	}

	private PhysicalShockwave CreateShockwave(Vector3 position)
	{
		DamageStop();
		if (difficulty <= 1 || (difficulty == 2 && gotParried && !enraged))
		{
			gotParried = false;
			return null;
		}
		if (difficulty >= 4 && enraged)
		{
			DelayedExplosion(position);
		}
		GameObject obj = Object.Instantiate(groundWave, position, Quaternion.identity);
		obj.transform.SetParent(gz.transform);
		if (obj.TryGetComponent<PhysicalShockwave>(out var component))
		{
			component.target = target;
			component.enemyType = EnemyType.SisyphusPrime;
			component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
			return component;
		}
		return null;
	}

	private void DropAttackActivate()
	{
		Physics.Raycast(aimingBone.position, Vector3.down, out var hitInfo, 250f, LayerMaskDefaults.Get(LMD.Environment));
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
		RaycastHit[] array = Physics.SphereCastAll(aimingBone.position, 5f, Vector3.down, Vector3.Distance(aimingBone.position, hitInfo.point), LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
		bool flag = false;
		List<EnemyIdentifier> list = new List<EnemyIdentifier>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			EnemyIdentifierIdentifier component2;
			if (raycastHit.transform.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
			{
				if (!flag)
				{
					flag = true;
					MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(30f * eid.totalDamageModifier), invincible: true);
					MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position + Vector3.down * -1f, 100f, 100f);
				}
			}
			else if ((raycastHit.transform.gameObject.layer == 10 || raycastHit.transform.gameObject.layer == 11) && raycastHit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component2) && (bool)component2.eid && !(component2.eid == eid) && !list.Contains(component2.eid))
			{
				list.Add(component2.eid);
				component2.eid.DeliverDamage(component2.gameObject, Vector3.down * 1000f, raycastHit.point, 5f, tryForExplode: true);
			}
		}
		base.transform.position = hitInfo.point;
		component.SetPosition(1, aimingBone.position);
		GameObject gameObject = Object.Instantiate(bigRubble, hitInfo.point, Quaternion.identity);
		if (Vector3.Angle(hitInfo.normal, Vector3.up) < 5f)
		{
			gameObject.transform.LookAt(new Vector3(gameObject.transform.position.x + base.transform.forward.x, gameObject.transform.position.y, gameObject.transform.position.z + base.transform.forward.z));
		}
		else
		{
			gameObject.transform.up = hitInfo.normal;
		}
		if (difficulty >= 2)
		{
			gameObject = Object.Instantiate(groundWave, hitInfo.point, Quaternion.identity);
			gameObject.transform.up = hitInfo.normal;
			gameObject.transform.SetParent(gz.transform);
			if (gameObject.TryGetComponent<PhysicalShockwave>(out var component3))
			{
				component3.enemyType = EnemyType.SisyphusPrime;
				component3.damage = Mathf.RoundToInt((float)component3.damage * eid.totalDamageModifier);
			}
		}
	}

	public void SnakeSwingStart(int limb)
	{
		if (!eid.dead)
		{
			Transform child = Object.Instantiate(swingSnake, aimingBone.position + base.transform.forward * 4f, Quaternion.identity).transform.GetChild(0);
			child.SetParent(base.transform, worldPositionStays: true);
			child.LookAt(heightAdjustedTargetPos);
			currentSwingSnakes.Add(child.gameObject);
			swinging = true;
			SwingCheck2 componentInChildren = child.GetComponentInChildren<SwingCheck2>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OverrideEnemyIdentifier(eid);
				componentInChildren.knockBackDirectionOverride = true;
				componentInChildren.knockBackDirection = (sc.knockBackDirectionOverride ? sc.knockBackDirection : base.transform.forward);
				componentInChildren.knockBackForce = sc.knockBackForce;
				componentInChildren.ignoreSlidingPlayer = sc.ignoreSlidingPlayer;
			}
			AttackTrail componentInChildren2 = child.GetComponentInChildren<AttackTrail>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.target = swingLimbs[limb];
				componentInChildren2.pivot = aimingBone;
			}
			DamageStart();
		}
	}

	public void DamageStart()
	{
		sc.DamageStart();
	}

	public void DamageStop()
	{
		swinging = false;
		sc.DamageStop();
		sc.knockBackDirectionOverride = false;
		sc.ignoreSlidingPlayer = false;
		mach.parryable = false;
		if (currentSwingSnakes.Count <= 0)
		{
			return;
		}
		for (int num = currentSwingSnakes.Count - 1; num >= 0; num--)
		{
			SwingCheck2 componentInChildren = currentSwingSnakes[num].GetComponentInChildren<SwingCheck2>();
			if ((bool)componentInChildren)
			{
				componentInChildren.DamageStop();
			}
			if (base.gameObject.activeInHierarchy && currentSwingSnakes[num].TryGetComponent<AttackTrail>(out var component))
			{
				component.DelayedDestroy(0.5f);
				currentSwingSnakes[num].transform.parent = null;
				component.target = null;
				component.pivot = null;
			}
			else
			{
				Object.Destroy(currentSwingSnakes[num]);
			}
		}
		currentSwingSnakes.Clear();
	}

	public void Explosion()
	{
		vibrating = false;
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
		if (gotParried)
		{
			gotParried = false;
			return;
		}
		GameObject gameObject = Object.Instantiate(this.explosion, aimingBone.position, Quaternion.identity);
		mach.parryable = false;
		if (difficulty >= 2 && eid.totalDamageModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty <= 1)
			{
				explosion.speed *= ((difficulty == 0) ? 0.5f : 0.6f);
				explosion.maxSize *= ((difficulty == 0) ? 0.5f : 0.75f);
			}
			explosion.speed *= eid.totalDamageModifier;
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	public void ProjectileCharge()
	{
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		currentProjectileCharge = Object.Instantiate(projectileCharge, swingLimbs[1].position, swingLimbs[1].rotation);
		currentProjectileCharge.transform.SetParent(swingLimbs[1]);
	}

	public void ProjectileShoot()
	{
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		if (target != null)
		{
			mach.parryable = false;
			Vector3 vector = target.PredictTargetPosition(0.5f);
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
			DelayedExplosion(vector);
			aiming = false;
			tracking = false;
			fullTracking = false;
		}
	}

	public void DelayedExplosion(Vector3 target)
	{
		GameObject obj = Object.Instantiate(sparkleExplosion, target, Quaternion.identity);
		obj.transform.SetParent(gz.transform);
		ObjectActivator component = obj.GetComponent<ObjectActivator>();
		if ((bool)component)
		{
			component.delay /= eid.totalSpeedModifier;
		}
		LineRenderer componentInChildren = obj.GetComponentInChildren<LineRenderer>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetPosition(0, target);
			componentInChildren.SetPosition(1, swingLimbs[1].position);
		}
		Explosion[] componentsInChildren = obj.GetComponentsInChildren<Explosion>();
		foreach (Explosion obj2 in componentsInChildren)
		{
			obj2.damage = Mathf.RoundToInt((float)obj2.damage * eid.totalDamageModifier);
			obj2.maxSize *= eid.totalDamageModifier;
		}
	}

	public void TeleportOnGround(int forceNoPrediction = 0)
	{
		if (target != null)
		{
			ResetRotation();
			Vector3 point = teleportToGroundFailsafe;
			if (Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
			{
				point = hitInfo.point;
			}
			base.transform.position = point;
			heightAdjustedTargetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			Teleport(heightAdjustedTargetPos, base.transform.position);
			if (difficulty < 2 || forceNoPrediction == 1)
			{
				base.transform.LookAt(heightAdjustedTargetPos);
				return;
			}
			Vector3 vector = target.PredictTargetPosition(0.5f);
			base.transform.LookAt(new Vector3(vector.x, base.transform.position.y, vector.z));
		}
	}

	public void TeleportAnywhere()
	{
		TeleportAnywhere(false);
	}

	public void TeleportAnywhere(bool predictive = false)
	{
		if (target != null)
		{
			Teleport(predictive ? target.PredictTargetPosition(0.5f) : target.position, base.transform.position);
			base.transform.LookAt((difficulty <= 1) ? target.position : target.PredictTargetPosition(0.5f));
		}
	}

	public void TeleportAbove()
	{
		TeleportAbove(true);
	}

	public void TeleportAbove(bool predictive = true)
	{
		Vector3 vector = (predictive ? target.PredictTargetPosition(0.5f) : target.position);
		if (vector.y < target.position.y)
		{
			vector.y = target.position.y;
		}
		Teleport(vector + Vector3.up * 25f, vector);
	}

	public void TeleportSideRandom(int predictive)
	{
		TeleportSide(Random.Range(0, 2), inAir: false, predictive == 1);
	}

	public void TeleportSideRandomAir(int predictive)
	{
		TeleportSide(Random.Range(0, 2), inAir: true, predictive == 1);
	}

	public void TeleportSide(int side, bool inAir = false, bool predictive = false)
	{
		int num = ((side != 0) ? 1 : (-1));
		Vector3 vector = (predictive ? target.PredictTargetPosition(0.5f) : target.position);
		if (!inAir)
		{
			vector = new Vector3(vector.x, base.transform.position.y, vector.z);
		}
		if (Physics.Raycast(vector + Vector3.up, target.right * num + target.forward, out var hitInfo, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			if (hitInfo.distance >= 2f)
			{
				Teleport(vector, vector + Vector3.ClampMagnitude(target.right * num + target.forward, 1f) * (hitInfo.distance - 1f));
			}
			else
			{
				Teleport(vector, base.transform.position);
			}
		}
		else
		{
			Teleport(vector, vector + (target.right * num + target.forward) * 10f);
		}
		base.transform.LookAt(vector);
	}

	public void Teleport(Vector3 teleportTarget, Vector3 startPos)
	{
		if ((bool)(Object)(object)nma)
		{
			((Behaviour)(object)nma).enabled = false;
		}
		gce.onGround = false;
		float num = Mathf.Clamp(Vector3.Distance(teleportTarget, startPos), 0f, 6f);
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
		Vector3 vector = teleportTarget + (startPos - teleportTarget).normalized * num;
		Collider[] array = Physics.OverlapCapsule(vector + base.transform.up * 0.75f, vector + base.transform.up * 5.25f, 0.75f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < 6; i++)
			{
				Collider collider = array[0];
				if (!Physics.ComputePenetration(col, vector + base.transform.up * 3f, base.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance))
				{
					break;
				}
				_ = 0.5f;
				vector += direction * distance;
				array = Physics.OverlapCapsule(vector + base.transform.up * 0.75f, vector + base.transform.up * 5.25f, 0.75f, LayerMaskDefaults.Get(LMD.Environment));
				if (array == null || array.Length == 0)
				{
					break;
				}
				if (i == 5)
				{
					ResolveStuckness();
					break;
				}
			}
		}
		component.SetPosition(1, aimingBone.position);
		float num2 = Vector3.Distance(base.transform.position, vector);
		for (int j = 0; (float)j < num2; j += 3)
		{
			if (Physics.Raycast(Vector3.Lerp(base.transform.position, vector, (num2 - (float)j) / num2) + Vector3.up, Vector3.down, out var hitInfo, 3f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Object.Instantiate(rubble, hitInfo.point, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			}
		}
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		base.transform.position = vector;
		tracking = false;
		fullTracking = false;
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
	}

	public void LookAtTarget()
	{
		if (target != null)
		{
			heightAdjustedTargetPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			base.transform.LookAt(heightAdjustedTargetPos);
		}
	}

	public void Death()
	{
		if ((bool)currentProjectileCharge)
		{
			Object.Destroy(currentProjectileCharge);
		}
		DamageStop();
		anim.Play("Outro");
		anim.SetBool("Dead", true);
		anim.speed = (bossVersion ? 1 : 5);
		activated = false;
		if ((bool)currentPassiveEffect)
		{
			Object.Destroy(currentPassiveEffect);
		}
		CancelInvoke();
		Object.Destroy((Object)(object)nma);
		DisableGravity();
		rb.SetGravityMode(useGravity: false);
		rb.isKinematic = true;
		MonoSingleton<TimeController>.Instance.SlowDown(0.0001f);
	}

	public void Ascend()
	{
		if (!bossVersion)
		{
			OutroEnd();
			return;
		}
		rb.isKinematic = false;
		rb.constraints = (RigidbodyConstraints)122;
		ascending = true;
		LightShaft();
		Invoke("LightShaft", 1.5f);
		Invoke("LightShaft", 3f);
		Invoke("LightShaft", 4f);
		Invoke("LightShaft", 5f);
		Invoke("LightShaft", 5.5f);
		Invoke("LightShaft", 6f);
		Invoke("LightShaft", 6.25f);
		Invoke("LightShaft", 6.5f);
		Invoke("LightShaft", 6.7f);
		Invoke("LightShaft", 6.8f);
		Invoke("LightShaft", 6.85f);
		Invoke("LightShaft", 6.9f);
		Invoke("LightShaft", 6.925f);
		Invoke("LightShaft", 6.95f);
		Invoke("LightShaft", 6.975f);
		Invoke("OutroEnd", 7f);
	}

	private void LightShaft()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Object.Instantiate(lightShaft, mach.chest.transform.position, Random.rotation).transform.SetParent(base.transform, worldPositionStays: true);
			MonoSingleton<CameraController>.Instance.CameraShake(1f);
		}
	}

	public void OutroEnd()
	{
		if (base.gameObject.activeInHierarchy)
		{
			onOutroEnd.Invoke();
			Object.Instantiate(outroExplosion, mach.chest.transform.position, Quaternion.identity);
			base.gameObject.SetActive(value: false);
			MonoSingleton<TimeController>.Instance.SlowDown(0.001f);
		}
	}

	public void EnableGravity(int earlyCancel)
	{
		if (!gce.onGround)
		{
			anim.SetBool("Falling", true);
			gravityInAction = true;
			if (earlyCancel == 1)
			{
				inAction = false;
			}
		}
		ResetRotation();
	}

	public void Parryable()
	{
		gotParried = false;
		Object.Instantiate(parryableFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 30f;
		mach.ParryableCheck();
	}

	public void Unparryable()
	{
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 15f;
	}

	public void GotParried()
	{
		PlayVoice(hurtVoice);
		attackAmount -= 5;
		gotParried = true;
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
	}

	public void Rubble()
	{
		Object.Instantiate(bigRubble, base.transform.position + base.transform.forward, base.transform.rotation);
	}

	public void ResetRotation()
	{
		base.transform.LookAt(new Vector3(base.transform.position.x + base.transform.forward.x, base.transform.position.y, base.transform.position.z + base.transform.forward.z));
		ResolveStuckness();
	}

	public void DisableGravity()
	{
		gravityInAction = false;
	}

	public void StartTracking()
	{
		tracking = true;
	}

	public void StopTracking()
	{
		tracking = false;
		fullTracking = false;
	}

	public void StopAction()
	{
		ResetRotation();
		fullTracking = false;
		gotParried = false;
		inAction = false;
		taunting = false;
		sc.knockBackDirectionOverride = false;
		if ((bool)mach)
		{
			mach.parryable = false;
		}
	}

	public void TargetBeenHit()
	{
		sc.DamageStop();
		hitSuccessful = true;
		mach.parryable = false;
		foreach (GameObject currentSwingSnake in currentSwingSnakes)
		{
			SwingCheck2 componentInChildren = currentSwingSnake.GetComponentInChildren<SwingCheck2>();
			if ((bool)componentInChildren)
			{
				componentInChildren.DamageStop();
			}
		}
	}

	public void OutOfBounds()
	{
		base.transform.position = spawnPoint;
	}

	public void Vibrate()
	{
		if ((bool)currentExplosionChargeEffect)
		{
			Object.Destroy(currentExplosionChargeEffect);
		}
		if (activated)
		{
			currentExplosionChargeEffect = Object.Instantiate(explosionChargeEffect, aimingBone.position, Quaternion.identity);
		}
		origPos = base.transform.position;
		vibrating = true;
	}

	public void PlayVoice(AudioClip[] voice)
	{
		if (voice.Length != 0 && (!((Object)(object)aud.clip == (Object)(object)phaseChangeVoice) || !aud.isPlaying))
		{
			aud.clip = voice[Random.Range(0, voice.Length)];
			aud.SetPitch(Random.Range(0.95f, 1f));
			aud.Play(tracked: true);
		}
	}

	public void ForceKnockbackDown()
	{
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.down;
	}

	public void SwingIgnoreSliding()
	{
		sc.ignoreSlidingPlayer = true;
	}

	public void ResolveStuckness()
	{
		Collider[] array = Physics.OverlapCapsule(base.transform.position + base.transform.up * 0.76f, base.transform.position + base.transform.up * 5.24f, 0.74f, LayerMaskDefaults.Get(LMD.Environment));
		if (array == null || array.Length == 0)
		{
			return;
		}
		if (gce.onGround)
		{
			gce.onGround = false;
			if ((bool)(Object)(object)nma)
			{
				((Behaviour)(object)nma).enabled = false;
			}
		}
		for (int i = 0; i < 6; i++)
		{
			RaycastHit[] array2 = Physics.CapsuleCastAll(spawnPoint + base.transform.up * 0.75f, spawnPoint + base.transform.up * 5.25f, 0.75f, base.transform.position - spawnPoint, Vector3.Distance(spawnPoint, base.transform.position), LayerMaskDefaults.Get(LMD.Environment));
			if (array2 == null || array2.Length == 0)
			{
				break;
			}
			RaycastHit[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				RaycastHit raycastHit = array3[j];
				bool flag = false;
				Collider[] array4 = array;
				for (int k = 0; k < array4.Length; k++)
				{
					if (array4[k] == raycastHit.collider)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					base.transform.position = spawnPoint + (base.transform.position - spawnPoint).normalized * raycastHit.distance + raycastHit.normal * 0.1f;
					break;
				}
			}
			if (i == 5)
			{
				base.transform.position = spawnPoint;
			}
		}
	}
}
