using UnityEngine;

public class GabrielBase : EnemyScript
{
	private Gabriel gabe1;

	private GabrielSecond gabe2;

	[HideInInspector]
	public Animator anim;

	private Enemy mach;

	private Rigidbody rb;

	[HideInInspector]
	public EnemyIdentifier eid;

	private SkinnedMeshRenderer smr;

	private GabrielVoice voice;

	private Collider col;

	public GameObject particles;

	public GameObject particlesEnraged;

	private Material origBody;

	private Material origWing;

	public Material enrageBody;

	public Material enrageWing;

	[HideInInspector]
	public int difficulty = -1;

	private bool valuesSet;

	private bool active = true;

	[HideInInspector]
	public bool inAction;

	private bool goingLeft;

	public bool variableForwardSpeed;

	[HideInInspector]
	public bool goForward;

	[HideInInspector]
	public float forwardSpeed;

	[HideInInspector]
	public float forwardSpeedMinimum;

	[HideInInspector]
	public float forwardSpeedMaximum;

	private float startCooldown = 2f;

	[HideInInspector]
	public float attackCooldown;

	[HideInInspector]
	public float combinedSwordsCooldown;

	[HideInInspector]
	public Projectile currentCombinedSwordsThrown;

	[HideInInspector]
	public int burstLength = 2;

	public bool enraged;

	private GameObject currentEnrageEffect;

	public bool secondPhase;

	public float phaseChangeHealth;

	private float preAttackTeleportCooldown;

	private float outOfSightTime;

	private int teleportAttempts;

	private int teleportInterval = 6;

	public GameObject teleportSound;

	public GameObject decoy;

	private bool overrideRotation;

	private bool stopRotation;

	private Vector3 overrideTarget;

	private LayerMask environmentMask;

	[HideInInspector]
	public bool spearing;

	private bool dashing;

	private float forcedDashTime;

	private Vector3 dashTarget;

	public GameObject dashEffect;

	[HideInInspector]
	public bool juggled;

	[HideInInspector]
	public float juggleHp;

	private float juggleEndHp;

	private float juggleLength;

	public GameObject juggleEffect;

	private bool juggleFalling;

	public GameObject summonedSwords;

	private GameObject currentSwords;

	public GameObject summonedSwordsWindup;

	private GameObject currentWindup;

	private float summonedSwordsCooldown = 15f;

	public Transform head;

	[HideInInspector]
	public bool readyTaunt;

	private float defaultAnimSpeed = 1f;

	private bool bossVersion;

	[SerializeField]
	private GameObject genericOutro;

	private int dashAttempts;

	[Header("Events")]
	public UltrakillEvent onFirstPhaseEnd;

	public UltrakillEvent onSecondPhaseStart;

	public bool attackThroughTarget;

	public bool pitFallChallenge;

	[HideInInspector]
	public Vector3 originalPosition;

	private EnemyTarget target => eid.target;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		mach = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		eid = GetComponent<EnemyIdentifier>();
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		voice = GetComponent<GabrielVoice>();
		col = GetComponent<Collider>();
		gabe1 = GetComponent<Gabriel>();
		gabe2 = GetComponent<GabrielSecond>();
	}

	private void Start()
	{
		SetValues();
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			origBody = smr.sharedMaterials[0];
			origWing = smr.sharedMaterials[1];
			origWing.SetFloat("_OpacScale", 1f);
			originalPosition = base.transform.position;
			if (enraged)
			{
				EnrageNow();
			}
			environmentMask = LayerMaskDefaults.Get(LMD.Environment);
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			if (difficulty >= 3)
			{
				burstLength = 3;
			}
			UpdateSpeed();
			RandomizeDirection();
			bossVersion = TryGetComponent<BossHealthBar>(out var _);
		}
	}

	private void UpdateBuff()
	{
		SetValues();
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
			anim.speed = 1f;
			break;
		case 1:
			anim.speed = 0.85f;
			break;
		case 0:
			anim.speed = 0.75f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		defaultAnimSpeed = anim.speed;
	}

	private void OnDisable()
	{
		CancelInvoke();
		DamageStopLeft();
		DamageStopRight();
		StopAction();
		ResetAnimSpeed();
		overrideRotation = false;
		spearing = false;
		dashing = false;
		if ((bool)currentSwords)
		{
			currentSwords.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (juggled)
		{
			JuggleStop();
		}
		if ((bool)currentSwords)
		{
			currentSwords.SetActive(value: true);
		}
	}

	private void UpdateRigidbodySettings()
	{
		rb.drag = ((target == null) ? 3 : 0);
		rb.angularDrag = ((target == null) ? 3 : 0);
	}

	private void Update()
	{
		UpdateRigidbodySettings();
		if (target == null)
		{
			return;
		}
		if (!secondPhase && mach.health <= phaseChangeHealth)
		{
			secondPhase = true;
			voice.secondPhase = true;
			if (!juggled)
			{
				JuggleStart();
			}
		}
		if (juggled)
		{
			JuggleUpdate();
		}
		if (!active)
		{
			return;
		}
		UpdateCooldowns();
		UpdateRotation();
		if (inAction || startCooldown > 0f || attackCooldown > 0f)
		{
			return;
		}
		if (Physics.Raycast(base.transform.position, target.headPosition - base.transform.position, Vector3.Distance(base.transform.position, target.headPosition), LayerMaskDefaults.Get(LMD.Environment)))
		{
			if (preAttackTeleportCooldown <= 0f)
			{
				preAttackTeleportCooldown = 0.5f;
				Teleport();
			}
		}
		else if (!gabe2 || (!(combinedSwordsCooldown > 0f) && (!currentCombinedSwordsThrown || !(Vector3.Distance(base.transform.position, currentCombinedSwordsThrown.transform.position) < Vector3.Distance(base.transform.position, eid.target.position)))))
		{
			ChooseAttack();
		}
	}

	private void ChooseAttack()
	{
		if ((bool)gabe1)
		{
			gabe1.ChooseAttack();
		}
		if ((bool)gabe2)
		{
			gabe2.ChooseAttack();
		}
	}

	private void UpdateCooldowns()
	{
		if (startCooldown > 0f)
		{
			startCooldown = Mathf.MoveTowards(startCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (secondPhase && difficulty >= 3 && !currentSwords)
		{
			summonedSwordsCooldown = Mathf.MoveTowards(summonedSwordsCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			if (summonedSwordsCooldown == 0f && !inAction && readyTaunt)
			{
				summonedSwordsCooldown = 15f;
				SpawnSummonedSwordsWindup();
				Invoke("SpawnSummonedSwords", 1f / eid.totalSpeedModifier);
			}
		}
		if ((bool)gabe2 && combinedSwordsCooldown > 0f)
		{
			combinedSwordsCooldown = Mathf.MoveTowards(combinedSwordsCooldown, 0f, Time.deltaTime);
			if (secondPhase || !currentCombinedSwordsThrown)
			{
				combinedSwordsCooldown = 0f;
			}
		}
		if (startCooldown <= 0f)
		{
			bool flag = Vector3.Distance(base.transform.position, target.headPosition) > 20f || base.transform.position.y > target.headPosition.y + 15f || Physics.Raycast(base.transform.position, target.headPosition - base.transform.position, Vector3.Distance(base.transform.position, target.headPosition), environmentMask);
			outOfSightTime = Mathf.MoveTowards(outOfSightTime, flag ? 3 : 0, Time.deltaTime * (float)(flag ? 1 : 2) * eid.totalSpeedModifier);
			if (outOfSightTime >= 3f && !inAction)
			{
				Teleport();
			}
		}
		if (preAttackTeleportCooldown > 0f)
		{
			preAttackTeleportCooldown = Mathf.MoveTowards(preAttackTeleportCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (!inAction && attackCooldown > 0f)
		{
			attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			if (readyTaunt && (bool)voice)
			{
				voice.Taunt();
				readyTaunt = false;
			}
		}
	}

	private void UpdateRotation()
	{
		if (!stopRotation)
		{
			Quaternion quaternion = Quaternion.LookRotation((overrideRotation ? overrideTarget : target.headPosition) - base.transform.position, Vector3.up);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * ((float)(overrideRotation ? 2500 : 10) * Quaternion.Angle(quaternion, base.transform.rotation) + (float)(overrideRotation ? 10 : 2)) * eid.totalSpeedModifier);
		}
	}

	private void FixedUpdate()
	{
		if (juggled)
		{
			rb.velocity = new Vector3(0f, (rb.velocity.y < 35f) ? rb.velocity.y : 35f, 0f);
			if ((bool)gabe2)
			{
				gabe2.CeilingCheck(rb, mach, voice);
			}
			if (juggleFalling && Physics.SphereCast(base.transform.position, 1.25f, Vector3.down, out var _, 3.6f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				JuggleStop();
			}
			juggleFalling = rb.velocity.y < 0f;
			return;
		}
		if (inAction)
		{
			AttackMovement();
			return;
		}
		col.enabled = true;
		if (target == null)
		{
			rb.velocity = Vector3.zero;
			return;
		}
		Vector3 zero = Vector3.zero;
		float num = Vector3.Distance(base.transform.position, target.headPosition);
		if (num > 5f)
		{
			zero += base.transform.forward * 7.5f * ((num > 10f) ? 1f : (num / 10f));
		}
		if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right * ((!goingLeft) ? 1 : (-1)), out var hitInfo2, 3f, environmentMask))
		{
			zero += base.transform.right * (goingLeft ? (-5) : 5);
		}
		else if (!Physics.SphereCast(base.transform.position, 1.25f, base.transform.right * (goingLeft ? 1 : (-1)), out hitInfo2, 3f, environmentMask))
		{
			goingLeft = !goingLeft;
		}
		else
		{
			zero += base.transform.forward * 5f;
		}
		rb.velocity = zero * eid.totalSpeedModifier;
	}

	private void AttackMovement()
	{
		rb.velocity = (goForward ? (base.transform.forward * forwardSpeed * ((difficulty >= 4) ? 1.25f : 1f)) : Vector3.zero);
		if (!attackThroughTarget && goForward)
		{
			if (!MonoSingleton<NewMovement>.Instance.playerCollider.Raycast(new Ray(base.transform.position, base.transform.forward), out var hitInfo, forwardSpeed * Time.fixedDeltaTime))
			{
				rb.velocity = base.transform.forward * forwardSpeed * ((difficulty >= 4) ? 1.25f : 1f);
			}
			else
			{
				if (hitInfo.distance > 1f)
				{
					base.transform.position += base.transform.forward * (hitInfo.distance - 1f);
				}
				rb.velocity = Vector3.zero;
			}
		}
		if (spearing)
		{
			if (goForward)
			{
				if (Physics.Raycast(base.transform.position, base.transform.forward, 2f, environmentMask))
				{
					spearing = false;
					DamageStopRight();
				}
			}
			else
			{
				base.transform.position = target.headPosition + Vector3.up * 15f;
			}
		}
		if (!dashing)
		{
			col.enabled = true;
			dashAttempts = 0;
			return;
		}
		col.enabled = false;
		if (forcedDashTime > 0f)
		{
			forcedDashTime = Mathf.MoveTowards(forcedDashTime, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (Vector3.Distance(base.transform.position, dashTarget) > 5f && dashAttempts < 5)
		{
			if (Physics.SphereCast(base.transform.position, 0.75f, dashTarget - base.transform.position, out var hitInfo2, Vector3.Distance(base.transform.position, dashTarget) - 0.75f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore) && !IsHitTarget(hitInfo2.collider))
			{
				dashAttempts++;
				col.enabled = true;
				dashTarget = target.headPosition;
				Teleport(closeRange: false, longrange: true);
				forcedDashTime = 0.35f;
				LookAtTarget();
			}
			else
			{
				rb.velocity = base.transform.forward * 100f * eid.totalSpeedModifier;
			}
		}
		else if (forcedDashTime <= 0f)
		{
			dashAttempts = 0;
			dashing = false;
			DashAttack();
		}
	}

	private void DashAttack()
	{
		if ((bool)gabe1)
		{
			gabe1.ZweiCombo();
		}
		if ((bool)gabe2)
		{
			gabe2.FastCombo();
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

	private void OnTriggerEnter(Collider other)
	{
		if (!juggleFalling || other.gameObject.layer != 0)
		{
			return;
		}
		DeathZone deathZone = (other.attachedRigidbody ? other.attachedRigidbody.GetComponent<DeathZone>() : other.GetComponent<DeathZone>());
		if ((bool)deathZone)
		{
			if ((bool)voice)
			{
				voice.BigHurt();
			}
			base.transform.position = deathZone.respawnTarget;
			eid.DeliverDamage(head.gameObject, Vector3.zero, head.position, 15f, tryForExplode: false);
			juggleFalling = false;
			JuggleStop();
			if (pitFallChallenge)
			{
				MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
			}
		}
	}

	public void Teleport(bool closeRange = false, bool longrange = false, bool firstTime = true, bool horizontal = false, bool vertical = false)
	{
		if (target == null)
		{
			return;
		}
		if (firstTime)
		{
			teleportAttempts = 0;
			outOfSightTime = 0f;
			spearing = false;
		}
		Vector3 normalized = Random.onUnitSphere.normalized;
		if (normalized.y < 0f)
		{
			normalized.y *= -1f;
		}
		Vector3 vector = target.headPosition + Vector3.up;
		float num = Random.Range(8, 15);
		if (closeRange)
		{
			num = Random.Range(5, 8);
		}
		else if (longrange)
		{
			num = Random.Range(15, 20);
		}
		vector = ((!Physics.Raycast(target.headPosition + Vector3.up, normalized, out var hitInfo, num, environmentMask, QueryTriggerInteraction.Ignore)) ? (target.headPosition + Vector3.up + normalized * num) : (hitInfo.point - normalized * 3f));
		RaycastHit hitInfo2;
		bool flag = Physics.Raycast(vector, Vector3.up, out hitInfo2, 8f, environmentMask, QueryTriggerInteraction.Ignore);
		RaycastHit hitInfo3;
		bool flag2 = Physics.Raycast(vector, Vector3.down, out hitInfo3, 8f, environmentMask, QueryTriggerInteraction.Ignore);
		Vector3 position = base.transform.position;
		if (!(flag && flag2))
		{
			position = (flag ? (hitInfo2.point + Vector3.down * Random.Range(5, 10)) : (flag2 ? ((!horizontal) ? (hitInfo3.point + Vector3.up * Random.Range(5, 10)) : new Vector3(hitInfo3.point.x, hitInfo3.point.y + 3.5f, hitInfo3.point.z)) : ((!horizontal) ? vector : new Vector3(vector.x, target.headPosition.y, vector.z))));
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
			position = ((!horizontal) ? new Vector3(vector.x, (hitInfo3.point.y + hitInfo2.point.y) / 2f, vector.z) : new Vector3(vector.x, hitInfo3.point.y + 3.5f, vector.z));
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
			obj.enraged = ((bool)gabe1 && secondPhase) || ((bool)gabe2 && !secondPhase);
		}
		return gameObject;
	}

	private void Parryable()
	{
		if (!juggled)
		{
			mach.ParryableCheck();
			AttackFlash();
		}
	}

	public void Unparryable()
	{
		mach.parryable = false;
	}

	public void AttackFlash(int unparryable = 0)
	{
		if (!juggled)
		{
			Object.Instantiate((unparryable == 0) ? MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash : MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash, head).transform.localScale *= 3f;
		}
	}

	private void StartDash()
	{
		if (eid.target != null)
		{
			inAction = true;
			overrideRotation = true;
			dashTarget = eid.target.position;
			overrideTarget = dashTarget;
			dashing = true;
			Object.Instantiate(dashEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void JuggleStart()
	{
		DamageStopLeft();
		DamageStopRight();
		MonoSingleton<TimeController>.Instance.SlowDown(0.25f);
		voice.BigHurt();
		inAction = true;
		if ((bool)gabe1)
		{
			gabe1.DisableWeapon();
		}
		CancelInvoke();
		dashing = false;
		spearing = false;
		rb.velocity = Vector3.zero;
		rb.AddForce(Vector3.up * 35f, ForceMode.VelocityChange);
		rb.SetGravityMode(useGravity: true);
		origWing.SetFloat("_OpacScale", 0f);
		if (target != null)
		{
			base.transform.LookAt(new Vector3(target.headPosition.x, base.transform.position.y, target.headPosition.z));
		}
		else
		{
			base.transform.rotation = Quaternion.identity;
		}
		overrideRotation = false;
		stopRotation = true;
		juggled = true;
		juggleHp = mach.health;
		juggleEndHp = mach.health - (gabe2 ? 7.5f : 15f);
		juggleLength = 5f;
		juggleFalling = false;
		Object.Instantiate(juggleEffect, base.transform.position, base.transform.rotation);
		eid.totalDamageTakenMultiplier = 0.5f;
		particles.SetActive(value: false);
		particlesEnraged.SetActive(value: false);
		ResetAnimSpeed();
		anim.Play("Juggle");
	}

	private void JuggleUpdate()
	{
		juggleLength = Mathf.MoveTowards(juggleLength, 0f, Time.deltaTime * eid.totalSpeedModifier);
		if (!(mach.health >= juggleHp))
		{
			if (rb.velocity.y < 0f)
			{
				rb.velocity = Vector3.zero;
			}
			rb.AddForce(Vector3.up * (juggleHp - mach.health) * (gabe2 ? 10 : 5), ForceMode.VelocityChange);
			juggleHp = mach.health;
			anim.Play("Juggle", 0, 0f);
			base.transform.LookAt(new Vector3(target.headPosition.x, base.transform.position.y, target.headPosition.z));
			voice.Hurt();
			if (mach.health < juggleEndHp || juggleLength <= 0f)
			{
				JuggleStop(enrage: true);
			}
		}
	}

	private void JuggleStop(bool enrage = false)
	{
		rb.SetGravityMode(useGravity: false);
		burstLength = ((difficulty == 0) ? 1 : difficulty);
		voice.PhaseChange();
		origWing.SetFloat("_OpacScale", 1f);
		stopRotation = false;
		juggled = false;
		if (enraged)
		{
			particlesEnraged.SetActive(value: true);
		}
		else
		{
			particles.SetActive(value: true);
		}
		anim.Play("Idle");
		spearing = false;
		eid.totalDamageTakenMultiplier = 1f;
		if ((enrage || mach.health <= phaseChangeHealth) && (((bool)gabe1 && !currentEnrageEffect) || ((bool)gabe2 && (bool)currentEnrageEffect)))
		{
			EnrageAnimation();
			return;
		}
		inAction = false;
		attackCooldown = 1f;
		Teleport();
	}

	private void EnrageAnimation()
	{
		anim.Play("Enrage");
		if (difficulty >= 3)
		{
			SpawnSummonedSwordsWindup();
		}
		if ((bool)gabe2)
		{
			Invoke("ForceUnEnrage", 3f * anim.speed);
		}
	}

	public void EnrageNow()
	{
		Material[] materials = smr.materials;
		materials[0] = enrageBody;
		materials[1] = enrageWing;
		smr.materials = materials;
		eid.UpdateBuffs(visualsOnly: true);
		if (!currentEnrageEffect)
		{
			currentEnrageEffect = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.enrageEffect, base.transform);
		}
		if ((bool)gabe2)
		{
			FadeOut fadeOut = currentEnrageEffect.AddComponent<FadeOut>();
			fadeOut.activateOnEnable = true;
			fadeOut.speed = 0.1f;
		}
		if (difficulty >= 3)
		{
			SpawnSummonedSwords();
		}
		if (particles.activeSelf)
		{
			particlesEnraged.SetActive(value: true);
			particles.SetActive(value: false);
		}
		if (secondPhase)
		{
			burstLength = ((difficulty == 0) ? 1 : difficulty);
			attackCooldown = 0f;
			readyTaunt = false;
		}
	}

	private void ForceUnEnrage()
	{
		UnEnrage();
		anim.Play("Idle");
		StopAction();
	}

	public void UnEnrage()
	{
		CancelInvoke("ForceUnEnrage");
		Material[] materials = smr.materials;
		materials[0] = origBody;
		materials[1] = origWing;
		smr.materials = materials;
		eid.totalDamageTakenMultiplier = 1f;
		enraged = false;
		if (particlesEnraged.activeSelf)
		{
			particlesEnraged.SetActive(value: false);
			particles.SetActive(value: true);
		}
		if (secondPhase && difficulty >= 3)
		{
			SpawnSummonedSwords();
		}
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
		if (secondPhase)
		{
			burstLength = ((difficulty == 0) ? 1 : difficulty);
			onSecondPhaseStart?.Invoke();
			attackCooldown = 0f;
		}
	}

	private void RandomizeDirection()
	{
		goingLeft = Random.Range(0f, 1f) > 0.5f;
	}

	public void SetForwardSpeed(int newSpeed)
	{
		if (!variableForwardSpeed)
		{
			forwardSpeed = (float)newSpeed * defaultAnimSpeed;
			return;
		}
		forwardSpeedMinimum = newSpeed;
		forwardSpeedMaximum = newSpeed + 50;
		DecideMovementSpeed();
	}

	public void EnrageTeleport(int teleportType = 0)
	{
		if (secondPhase && !currentCombinedSwordsThrown)
		{
			if (teleportType >= 10)
			{
				if (difficulty <= 2)
				{
					return;
				}
				teleportType -= 10;
			}
			if (teleportType <= 0)
			{
				teleportType = 2;
			}
			switch (teleportType)
			{
			case 1:
				Teleport(closeRange: true);
				break;
			case 2:
				Teleport();
				break;
			case 3:
				Teleport(closeRange: true, longrange: false, firstTime: true, horizontal: true);
				break;
			case 4:
				Teleport(closeRange: false, longrange: false, firstTime: true, horizontal: true);
				break;
			case 5:
				Teleport(closeRange: false, longrange: false, firstTime: true, horizontal: false, vertical: true);
				break;
			}
			anim.speed = 0f;
			Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
		}
		if (target != null)
		{
			base.transform.LookAt(target.headPosition);
		}
	}

	public void ResetAnimSpeed()
	{
		if ((bool)(Object)(object)anim)
		{
			anim.speed = defaultAnimSpeed;
		}
	}

	public void LookAtTarget(int instant = 0)
	{
		overrideRotation = true;
		if (target == null)
		{
			base.transform.rotation = Quaternion.identity;
			return;
		}
		overrideTarget = base.transform.position + (target.headPosition - base.transform.position).normalized * 999f;
		base.transform.LookAt(base.transform.position + (target.headPosition - base.transform.position).normalized * 999f);
	}

	public void FollowTarget()
	{
		if (!juggled)
		{
			overrideRotation = false;
		}
	}

	public void StopAction()
	{
		if (!juggled)
		{
			FollowTarget();
			inAction = false;
		}
	}

	public void ResetWingMat()
	{
		origWing.SetFloat("_OpacScale", 1f);
	}

	public void Death()
	{
		if ((bool)currentSwords)
		{
			Object.Destroy(currentSwords);
		}
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
		if (!bossVersion)
		{
			Object.Instantiate(genericOutro, base.transform.position, Quaternion.LookRotation(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z)));
			Object.Destroy(base.gameObject);
		}
	}

	private void SpawnSummonedSwordsWindup()
	{
		currentWindup = Object.Instantiate(summonedSwordsWindup, base.transform.position, Quaternion.identity);
		currentWindup.transform.SetParent(base.transform, worldPositionStays: true);
	}

	private void SpawnSummonedSwords()
	{
		if ((bool)currentWindup)
		{
			Object.Destroy(currentWindup);
		}
		currentSwords = Object.Instantiate(summonedSwords, base.transform.position, Quaternion.identity);
		currentSwords.transform.SetParent(base.transform.parent, worldPositionStays: true);
		if (currentSwords.TryGetComponent<SummonedSwords>(out var component))
		{
			component.target = new EnemyTarget(base.transform);
			component.speed *= eid.totalSpeedModifier;
			component.targetEnemy = eid.target;
		}
		Projectile[] componentsInChildren = currentSwords.GetComponentsInChildren<Projectile>();
		foreach (Projectile projectile in componentsInChildren)
		{
			projectile.target = target;
			if (eid.totalDamageModifier != 1f)
			{
				projectile.damage *= eid.totalDamageModifier;
			}
		}
	}

	public void DecideMovementSpeed()
	{
		if (eid.target != null)
		{
			if (difficulty <= 1)
			{
				forwardSpeed = forwardSpeedMinimum * anim.speed;
			}
			forwardSpeed = ((Vector3.Distance(eid.target.position + eid.target.GetVelocity() * 0.25f, base.transform.position) > 20f) ? (forwardSpeedMaximum * anim.speed * (currentSwords ? 0.85f : 1f)) : (forwardSpeedMinimum * anim.speed));
		}
	}

	private void DamageStopRight(int num = 0)
	{
		if ((bool)gabe1)
		{
			gabe1.DamageStopRight(num);
		}
		if ((bool)gabe2)
		{
			gabe2.DamageStopRight(num);
		}
	}

	private void DamageStopLeft(int num = 0)
	{
		if ((bool)gabe1)
		{
			gabe1.DamageStopLeft(num);
		}
		if ((bool)gabe2)
		{
			gabe2.DamageStopLeft(num);
		}
	}
}
