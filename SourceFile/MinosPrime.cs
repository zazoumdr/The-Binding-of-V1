using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinosPrime : EnemyScript, IHitTargetCallback
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

	private MPAttack lastAttack;

	private Vector3 playerPos;

	private bool tracking;

	private bool fullTracking;

	private bool aiming;

	private bool jumping;

	public GameObject explosion;

	public GameObject rubble;

	public GameObject bigRubble;

	public GameObject groundWave;

	public GameObject swoosh;

	public Transform aimingBone;

	private Transform head;

	public GameObject projectileCharge;

	public GameObject snakeProjectile;

	private bool hasProjectiled;

	public GameObject warningFlash;

	public GameObject parryableFlash;

	private bool gravityInAction;

	private bool hasRiderKicked;

	private bool previouslyRiderKicked;

	private int downSwingAmount;

	private bool ignoreRiderkickAngle;

	public GameObject attackTrail;

	public GameObject swingSnake;

	private List<GameObject> currentSwingSnakes = new List<GameObject>();

	private bool uppercutting;

	private bool hitSuccessful;

	private bool gotParried;

	public Transform[] swingLimbs;

	private bool swinging;

	private bool boxing;

	private int attacksSinceBoxing;

	private SwingCheck2 sc;

	private GoreZone gz;

	private int attackAmount;

	private bool enraged;

	public GameObject passiveEffect;

	private GameObject currentPassiveEffect;

	public GameObject flameEffect;

	public GameObject phaseChangeEffect;

	private int difficulty = -1;

	private MPAttack previousCombo = MPAttack.Jump;

	private bool activated = true;

	private bool ascending;

	private bool vibrating;

	private Vector3 origPos;

	public GameObject lightShaft;

	public GameObject outroExplosion;

	public UltrakillEvent onOutroEnd;

	private Vector3 spawnPoint;

	[Header("Voice clips")]
	public AudioClip[] riderKickVoice;

	public AudioClip[] dropkickVoice;

	public AudioClip[] dropAttackVoice;

	public AudioClip[] boxingVoice;

	public AudioClip[] comboVoice;

	public AudioClip[] overheadVoice;

	public AudioClip[] projectileVoice;

	public AudioClip[] uppercutVoice;

	public AudioClip phaseChangeVoice;

	public AudioClip[] hurtVoice;

	private bool bossVersion;

	private EnemyTarget target => eid.target;

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		nma = GetComponent<NavMeshAgent>();
		mach = GetComponent<Enemy>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		rb = GetComponent<Rigidbody>();
		sc = GetComponentInChildren<SwingCheck2>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
	}

	private void Start()
	{
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
			uppercutting = false;
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
			playerPos = new Vector3(target.position.x, base.transform.position.y, target.position.z);
			if (!inAction)
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
			base.transform.position = new Vector3(origPos.x + Random.Range(-0.1f, 0.1f), origPos.y + Random.Range(-0.1f, 0.1f), origPos.z + Random.Range(-0.1f, 0.1f));
		}
	}

	public void Enrage()
	{
		enraged = true;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("WEAK");
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
	}

	private void FixedUpdate()
	{
		if (!activated)
		{
			return;
		}
		CustomPhysics();
		if (target == null)
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
			if (!inAction && gce.onGround && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && Vector3.Distance(base.transform.position, playerPos) > 2.5f)
			{
				nma.isStopped = false;
				mach.SetDestination(target.GetNavPoint());
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
				base.transform.LookAt(playerPos);
			}
			else
			{
				base.transform.rotation = Quaternion.LookRotation(target.position - new Vector3(base.transform.position.x, aimingBone.position.y, base.transform.position.z));
			}
		}
		if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && !inAction)
		{
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
		if ((difficulty == 3 && !enraged && attackAmount >= 10) || (difficulty <= 2 && (((difficulty <= 1 || !enraged) && attackAmount >= 6) || attackAmount >= 12)))
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
			if (uppercutting)
			{
				uppercutting = false;
				DamageStop();
				if (hitSuccessful && target != null && target.position.y > base.transform.position.y && activated)
				{
					Jump();
					hitSuccessful = false;
				}
			}
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
		if (gce.onGround && !jumping)
		{
			((Behaviour)(object)nma).enabled = true;
			rb.isKinematic = true;
			hasRiderKicked = false;
			hasProjectiled = false;
			downSwingAmount = 0;
			if (activated && target != null && !anim.IsInTransition(0))
			{
				PickGroundedAttack();
			}
		}
		else
		{
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			if (activated && target != null && !(rb.velocity.y >= 0f) && !anim.IsInTransition(0))
			{
				PickAirAttack();
			}
		}
	}

	private void PickGroundedAttack()
	{
		if (cooldown > 0f)
		{
			return;
		}
		float num = Vector3.Distance(base.transform.position, playerPos);
		if (target.isOnGround)
		{
			PickAttack();
			return;
		}
		if (num < 25f)
		{
			if (lastAttack != MPAttack.Jump)
			{
				Jump();
				lastAttack = MPAttack.Jump;
				return;
			}
		}
		else if (lastAttack != MPAttack.ProjectilePunch)
		{
			ProjectilePunch();
			return;
		}
		PickAttack(Random.Range(0, 4));
	}

	private void PickAirAttack()
	{
		if (cooldown > 0f)
		{
			return;
		}
		if (!hasProjectiled && enraged && Random.Range(0f, 1f) < 0.25f && Vector3.Distance(playerPos, base.transform.position) > 6f && !Physics.Raycast(base.transform.position, Vector3.down, 4f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			hasProjectiled = true;
			ProjectilePunch();
		}
		else if (Vector3.Distance(playerPos, base.transform.position) < 5f)
		{
			if (target.position.y < base.transform.position.y)
			{
				DropAttack();
			}
			else if (target.position.y < base.transform.position.y + 10f && downSwingAmount < 2)
			{
				DownSwing();
			}
		}
		else if (Vector3.Distance(base.transform.position, target.position) < 10f || Vector3.Angle(Vector3.up, target.position - base.transform.position) > 90f || ignoreRiderkickAngle)
		{
			if (previouslyRiderKicked && downSwingAmount < 2)
			{
				TeleportAnywhere();
				DownSwing();
				hasRiderKicked = true;
			}
			else if (!hasRiderKicked)
			{
				RiderKick();
			}
		}
		ignoreRiderkickAngle = false;
	}

	private void PickAttack()
	{
		PickAttack(Random.Range(0, 4));
	}

	private void PickAttack(int type)
	{
		if (attacksSinceBoxing >= 5)
		{
			type = 0;
		}
		if (type == (int)lastAttack)
		{
			type = ((type != 3) ? (type + 1) : 0);
		}
		if (type == 0)
		{
			attacksSinceBoxing = 0;
		}
		else
		{
			attacksSinceBoxing++;
		}
		switch (type)
		{
		case 0:
			Boxing();
			break;
		case 1:
			Combo();
			break;
		case 2:
			Dropkick();
			break;
		case 3:
			Uppercut();
			break;
		}
	}

	private void Dropkick()
	{
		lastAttack = MPAttack.Dropkick;
		attackAmount += 2;
		inAction = true;
		tracking = true;
		fullTracking = false;
		aiming = false;
		if ((bool)(Object)(object)nma && nma.isOnNavMesh)
		{
			nma.isStopped = true;
		}
		anim.Play("Dropkick", 0, 0f);
		sc.knockBackForce = 100f;
		cooldown += (enraged ? 0.25f : 1.25f);
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Judgement!");
		PlayVoice(dropkickVoice);
	}

	private void ProjectilePunch()
	{
		lastAttack = MPAttack.ProjectilePunch;
		attackAmount++;
		inAction = true;
		tracking = true;
		fullTracking = false;
		aiming = true;
		if ((bool)(Object)(object)nma && nma.isOnNavMesh)
		{
			nma.isStopped = true;
		}
		anim.Play("ProjectilePunch", 0, 0f);
		ProjectileCharge();
		PlayVoice(projectileVoice);
	}

	private void Jump()
	{
		inAction = true;
		tracking = true;
		fullTracking = false;
		aiming = false;
		base.transform.LookAt(playerPos);
		gravityInAction = true;
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		jumping = true;
		anim.SetBool("Falling", false);
		anim.Play("Jump", 0, 0f);
		Invoke("StopAction", 0.1f);
		rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
	}

	private void Uppercut()
	{
		attackAmount++;
		inAction = true;
		tracking = true;
		fullTracking = false;
		aiming = false;
		base.transform.LookAt(playerPos);
		gravityInAction = false;
		hitSuccessful = false;
		anim.Play("Uppercut", 0, 0f);
		anim.SetBool("Falling", false);
		WarningFlash();
		sc.knockBackForce = 100f;
		PlayVoice(uppercutVoice);
	}

	private void RiderKick()
	{
		if (target != null)
		{
			attackAmount++;
			inAction = true;
			tracking = true;
			fullTracking = true;
			aiming = false;
			base.transform.LookAt(target.position);
			downSwingAmount = 0;
			previouslyRiderKicked = true;
			gravityInAction = false;
			anim.SetTrigger("RiderKick");
			Invoke("StopTracking", ((difficulty >= 2) ? 0.5f : 0.25f) / anim.speed);
			Invoke("RiderKickActivate", 0.75f / anim.speed);
			WarningFlash();
			sc.knockBackForce = 50f;
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Die!");
			PlayVoice(riderKickVoice);
		}
	}

	private void DropAttack()
	{
		attackAmount++;
		inAction = true;
		tracking = true;
		fullTracking = false;
		aiming = false;
		ResetRotation();
		base.transform.LookAt(playerPos);
		downSwingAmount = 0;
		gravityInAction = false;
		anim.SetTrigger("DropAttack");
		Invoke("DropAttackActivate", 0.75f / anim.speed);
		WarningFlash();
		sc.knockBackForce = 50f;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Crush!");
		PlayVoice(dropAttackVoice);
	}

	private void DownSwing()
	{
		if (target != null)
		{
			attackAmount++;
			downSwingAmount++;
			inAction = true;
			tracking = true;
			fullTracking = true;
			aiming = false;
			previouslyRiderKicked = false;
			base.transform.LookAt(target.position);
			gravityInAction = false;
			anim.SetTrigger("DownSwing");
			WarningFlash();
			sc.knockBackForce = 100f;
			sc.knockBackDirectionOverride = true;
			sc.knockBackDirection = Vector3.down;
			PlayVoice(overheadVoice);
		}
	}

	private void WarningFlash()
	{
		Object.Instantiate(warningFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 5f;
	}

	public void UppercutActivate()
	{
		base.transform.LookAt(playerPos);
		uppercutting = true;
		tracking = true;
		fullTracking = false;
		gravityInAction = true;
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		jumping = true;
		anim.SetBool("Falling", false);
		Invoke("StopAction", 0.1f);
		rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
		Object.Instantiate(swoosh, base.transform.position, Quaternion.identity);
		Transform child = Object.Instantiate(swingSnake, aimingBone.position + base.transform.forward * 4f, Quaternion.identity).transform.GetChild(0);
		child.SetParent(base.transform, worldPositionStays: true);
		child.rotation = Quaternion.LookRotation(Vector3.up);
		currentSwingSnakes.Add(child.gameObject);
		SwingCheck2[] componentsInChildren = child.GetComponentsInChildren<SwingCheck2>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OverrideEnemyIdentifier(eid);
		}
		sc.knockBackDirectionOverride = true;
		sc.knockBackDirection = Vector3.up;
		DamageStart();
	}

	public void UppercutCancel(int parryable = 0)
	{
		if (target != null)
		{
			if (target.position.y > base.transform.position.y + 5f)
			{
				DamageStop();
				Uppercut();
			}
			else if (parryable == 1)
			{
				Parryable();
			}
		}
	}

	public void Combo()
	{
		if (previousCombo == MPAttack.Combo)
		{
			Boxing();
			return;
		}
		previousCombo = MPAttack.Combo;
		lastAttack = MPAttack.Combo;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Combo", 0, 0f);
		sc.knockBackForce = 50f;
		aiming = false;
		attackAmount += 3;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Prepare thyself!");
		PlayVoice(comboVoice);
	}

	public void Boxing()
	{
		if (previousCombo == MPAttack.Boxing)
		{
			Combo();
			return;
		}
		previousCombo = MPAttack.Boxing;
		lastAttack = MPAttack.Boxing;
		inAction = true;
		base.transform.LookAt(playerPos);
		tracking = true;
		fullTracking = false;
		gravityInAction = false;
		anim.Play("Boxing", 0, 0f);
		sc.knockBackForce = 30f;
		aiming = false;
		attackAmount += 2;
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle("Thy end is now!");
		PlayVoice(boxingVoice);
	}

	private void RiderKickActivate()
	{
		AirRaycastAttack(base.transform.forward);
	}

	private void DropAttackActivate()
	{
		AirRaycastAttack(Vector3.down);
		Explosion();
	}

	private void AirRaycastAttack(Vector3 direction)
	{
		Physics.Raycast(aimingBone.position, direction, out var hitInfo, 250f, LayerMaskDefaults.Get(LMD.Environment));
		LineRenderer component = Object.Instantiate(attackTrail, aimingBone.position, base.transform.rotation).GetComponent<LineRenderer>();
		component.SetPosition(0, aimingBone.position);
		RaycastHit[] array = Physics.SphereCastAll(aimingBone.position, 5f, direction, Vector3.Distance(aimingBone.position, hitInfo.point), LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
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
					MonoSingleton<NewMovement>.Instance.LaunchFromPoint(MonoSingleton<NewMovement>.Instance.transform.position + direction * -1f, 100f, 100f);
				}
			}
			else if ((raycastHit.transform.gameObject.layer == 10 || raycastHit.transform.gameObject.layer == 11) && raycastHit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component2) && (bool)component2.eid && !(component2.eid == eid) && !list.Contains(component2.eid))
			{
				list.Add(component2.eid);
				component2.eid.DeliverDamage(component2.gameObject, direction * 1000f, raycastHit.point, 5f, tryForExplode: true);
			}
		}
		if (Vector3.Angle(Vector3.up, hitInfo.normal) < 35f)
		{
			base.transform.position = hitInfo.point;
			anim.Play("DropRecovery", 0, 0f);
		}
		else if (Vector3.Angle(Vector3.up, hitInfo.normal) < 145f)
		{
			base.transform.position = hitInfo.point - base.transform.forward;
			inAction = false;
			hasRiderKicked = true;
			anim.Play("Falling", 0, 0f);
		}
		else
		{
			base.transform.position = hitInfo.point - Vector3.up * 6.5f;
			inAction = false;
			hasRiderKicked = true;
			anim.Play("Falling", 0, 0f);
		}
		ResetRotation();
		ResolveStuckness();
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
				component3.enemyType = EnemyType.MinosPrime;
				component3.damage = Mathf.RoundToInt((float)component3.damage * eid.totalDamageModifier);
			}
		}
	}

	public void SnakeSwingStart(int limb)
	{
		Transform child = Object.Instantiate(swingSnake, aimingBone.position + base.transform.forward * 4f, Quaternion.identity).transform.GetChild(0);
		child.SetParent(base.transform, worldPositionStays: true);
		child.LookAt(playerPos);
		currentSwingSnakes.Add(child.gameObject);
		if (!boxing)
		{
			swinging = true;
		}
		SwingCheck2 componentInChildren = child.GetComponentInChildren<SwingCheck2>();
		if ((bool)componentInChildren)
		{
			componentInChildren.OverrideEnemyIdentifier(eid);
			componentInChildren.knockBackDirectionOverride = true;
			componentInChildren.knockBackDirection = (sc.knockBackDirectionOverride ? sc.knockBackDirection : base.transform.forward);
			componentInChildren.knockBackForce = sc.knockBackForce;
		}
		AttackTrail componentInChildren2 = child.GetComponentInChildren<AttackTrail>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.target = swingLimbs[limb];
			componentInChildren2.pivot = aimingBone;
		}
		DamageStart();
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
		if (gotParried && difficulty <= 2 && !enraged)
		{
			gotParried = false;
			return;
		}
		GameObject gameObject = Object.Instantiate(this.explosion, base.transform.position, Quaternion.identity);
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
		Object.Instantiate(projectileCharge, swingLimbs[0].position, swingLimbs[0].rotation).transform.SetParent(swingLimbs[0]);
	}

	public void ProjectileShoot()
	{
		if (target != null)
		{
			GameObject obj = Object.Instantiate(snakeProjectile, mach.chest.transform.position, Quaternion.LookRotation(target.position - (base.transform.position + Vector3.up)));
			obj.transform.SetParent(gz.transform);
			Projectile componentInChildren = obj.GetComponentInChildren<Projectile>();
			if ((bool)componentInChildren)
			{
				componentInChildren.target = (target.isPlayer ? new EnemyTarget(MonoSingleton<CameraController>.Instance.transform) : target);
				componentInChildren.damage *= eid.totalDamageModifier;
			}
			aiming = false;
			tracking = false;
			fullTracking = false;
		}
	}

	public void TeleportOnGround()
	{
		Teleport(playerPos, base.transform.position);
		base.transform.LookAt(playerPos);
	}

	public void TeleportAnywhere()
	{
		if (target != null)
		{
			Teleport(target.position, base.transform.position);
			base.transform.LookAt(target.position);
		}
	}

	public void TeleportSide(int side)
	{
		if (target == null)
		{
			return;
		}
		int num = ((side != 0) ? 1 : (-1));
		boxing = true;
		if (Physics.Raycast(playerPos + Vector3.up, target.right * num + target.forward, out var hitInfo, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
		{
			if (hitInfo.distance >= 2f)
			{
				Teleport(playerPos, playerPos + Vector3.ClampMagnitude(target.right * num + target.forward, 1f) * (hitInfo.distance - 1f));
			}
			else
			{
				Teleport(playerPos, base.transform.position);
			}
		}
		else
		{
			Teleport(playerPos, playerPos + (target.right * num + target.forward) * 10f);
		}
		base.transform.LookAt(playerPos);
	}

	public void Teleport(Vector3 teleportTarget, Vector3 startPos)
	{
		float num = Vector3.Distance(teleportTarget, startPos);
		if (num > (boxing ? 2.5f : 3f))
		{
			num = (boxing ? 2.5f : 3f);
		}
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

	public void Death()
	{
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
			MonoSingleton<TimeController>.Instance.SlowDown(0.01f);
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
		if (difficulty < 3 || !enraged)
		{
			gotParried = false;
			Object.Instantiate(parryableFlash, head.position, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - head.position)).transform.localScale *= 10f;
			mach.ParryableCheck();
		}
	}

	public void GotParried()
	{
		PlayVoice(hurtVoice);
		attackAmount -= 5;
		gotParried = true;
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

	public void StopTracking()
	{
		tracking = false;
		fullTracking = false;
	}

	public void StopAction()
	{
		gotParried = false;
		inAction = false;
		boxing = false;
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
		if (uppercutting)
		{
			ignoreRiderkickAngle = true;
		}
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
