using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanHead : MonoBehaviour
{
	[HideInInspector]
	public bool active = true;

	private Animator anim;

	[SerializeField]
	private Transform shootPoint;

	private bool projectileBursting;

	private int projectilesLeftInBurst;

	private int projectileBurstMaximum;

	private float projectileBurstCooldown;

	public float projectileSpreadAmount;

	[SerializeField]
	private GameObject beam;

	[SerializeField]
	private GameObject beamCharge;

	private float beamTime;

	private bool forceBeam;

	public Transform tracker;

	private List<Transform> trackerBones = new List<Transform>();

	[SerializeField]
	private Transform tailBone;

	private Transform[] tailBones;

	private bool inAction = true;

	private float attackCooldown;

	public bool lookAtPlayer;

	private bool predictPlayer;

	private Quaternion defaultHeadRotation = new Quaternion(-0.645012f, 0.2603323f, 0.6614516f, 0.2804788f);

	private Quaternion previousHeadRotation;

	private bool notAtDefaultHeadRotation;

	private bool trackerOverrideAnimation;

	private bool trackerIgnoreLimits;

	private float cantTurnToPlayer;

	private float headRotationSpeedMultiplier = 1f;

	private bool freezeTail;

	private Vector3[] defaultTailPositions;

	private Quaternion[] defaultTailRotations;

	private bool rotateBody;

	private Quaternion defaultBodyRotation;

	private Vector3 defaultPosition;

	private bool bodyRotationOverride;

	private Vector3 bodyRotationOverrideTarget;

	[SerializeField]
	private SwingCheck2 biteSwingCheck;

	[SerializeField]
	private GameObject parryHelper;

	public Vector3[] spawnPositions;

	public Vector3 centerSpawnPosition;

	private int previousSpawnPosition;

	private int previousAttack = -1;

	private int recentAttacks;

	[HideInInspector]
	public LeviathanController lcon;

	[SerializeField]
	private UltrakillEvent onRoar;

	[SerializeField]
	private AudioSource projectileWindupSound;

	[SerializeField]
	private AudioSource biteWindupSound;

	[SerializeField]
	private AudioSource beamWindupSound;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private AudioSource hurtSound;

	[SerializeField]
	private GameObject warningFlash;

	private bool headExploded;

	public UltrakillEvent onHeadExplode;

	[HideInInspector]
	public EnemyTarget Target => lcon.eid.target;

	private void Start()
	{
		SetSpeed();
		previousHeadRotation = tracker.rotation;
		defaultBodyRotation = base.transform.rotation;
		defaultPosition = base.transform.position;
		tailBones = tailBone.GetComponentsInChildren<Transform>();
		defaultTailPositions = new Vector3[tailBones.Length];
		for (int i = 0; i < tailBones.Length; i++)
		{
			defaultTailPositions[i] = tailBones[i].position;
		}
		defaultTailRotations = new Quaternion[tailBones.Length];
		for (int j = 0; j < tailBones.Length; j++)
		{
			defaultTailRotations[j] = tailBones[j].rotation;
		}
		if (!BlindEnemies.Blind)
		{
			anim.Play("AscendLong");
		}
		lookAtPlayer = false;
	}

	public void SetSpeed()
	{
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		if (lcon.difficulty == 2)
		{
			anim.speed = 0.9f;
		}
		else if (lcon.difficulty == 1)
		{
			anim.speed = 0.8f;
		}
		else if (lcon.difficulty == 0)
		{
			anim.speed = 0.65f;
		}
		else if (lcon.difficulty == 3)
		{
			anim.speed = 1f;
		}
		else
		{
			anim.speed = 1.25f;
		}
		if (lcon.difficulty >= 2)
		{
			projectileBurstMaximum = 80;
		}
		else if (lcon.difficulty == 1)
		{
			projectileBurstMaximum = 60;
		}
		else
		{
			projectileBurstMaximum = 40;
		}
		Animator obj = anim;
		obj.speed *= lcon.eid.totalSpeedModifier;
	}

	private void OnEnable()
	{
		ResetDefaults();
	}

	private void ResetDefaults()
	{
		defaultBodyRotation = base.transform.rotation;
		headRotationSpeedMultiplier = 1f;
		defaultPosition = base.transform.position;
	}

	private void OnDisable()
	{
		trackerOverrideAnimation = false;
		trackerIgnoreLimits = false;
		projectileBursting = false;
		if ((bool)(Object)(object)anim)
		{
			anim.SetBool("ProjectileBurst", false);
		}
		bodyRotationOverride = false;
	}

	private void LateUpdate()
	{
		if (headExploded)
		{
			tracker.transform.localScale = Vector3.zero;
		}
		if (!active)
		{
			return;
		}
		if (beamTime > 0f)
		{
			beamTime = Mathf.MoveTowards(beamTime, 0f, Time.deltaTime);
			float num = Mathf.Clamp(Mathf.Pow(10f - beamTime, 2f) * 5f, 0f, 180f);
			base.transform.Rotate(Vector3.up * num * Time.deltaTime);
			anim.SetFloat("BeamSpeed", num / 180f);
			if (beamTime <= 0f)
			{
				BeamStop();
			}
		}
		else if ((rotateBody || lcon.secondPhase) && Target != null)
		{
			Vector3 vector = (bodyRotationOverride ? bodyRotationOverrideTarget : Target.position);
			Quaternion quaternion = Quaternion.LookRotation(base.transform.position - ((vector.y < base.transform.position.y) ? new Vector3(vector.x, base.transform.position.y, vector.z) : vector));
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(base.transform.rotation, quaternion) * 13.5f), 10f) * lcon.eid.totalSpeedModifier);
			Quaternion rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(base.transform.rotation, quaternion) * 13.5f), 10f) * lcon.eid.totalSpeedModifier);
			Vector3 position = defaultPosition + Vector3.up * (Mathf.Max(0f, base.transform.localRotation.eulerAngles.x) * 0.85f);
			base.transform.SetPositionAndRotation(position, rotation);
		}
		else
		{
			Quaternion rotation2 = Quaternion.RotateTowards(base.transform.rotation, defaultBodyRotation, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(base.transform.rotation, defaultBodyRotation) * 13.5f), 10f) * lcon.eid.totalSpeedModifier);
			Vector3 position2 = Vector3.MoveTowards(base.transform.position, defaultPosition, Time.deltaTime * Mathf.Max(10f, Vector3.Distance(base.transform.position, defaultPosition) * 5f) * lcon.eid.totalSpeedModifier);
			base.transform.SetPositionAndRotation(position2, rotation2);
		}
		if (lookAtPlayer && Target != null)
		{
			Quaternion quaternion2 = Quaternion.LookRotation(Target.position - tracker.position);
			if (predictPlayer)
			{
				quaternion2 = Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(1.5f, aimAtHead: true, ignoreCollision: true) - tracker.position);
			}
			quaternion2 *= Quaternion.Euler(Vector3.right * 90f);
			if (!trackerOverrideAnimation)
			{
				Quaternion quaternion3 = Quaternion.Inverse(tracker.parent.rotation * defaultHeadRotation) * tracker.rotation;
				quaternion2 *= quaternion3;
				if (!trackerIgnoreLimits)
				{
					float num2 = Quaternion.Angle(quaternion2, tracker.rotation);
					if (num2 > 50f)
					{
						quaternion2 = Quaternion.Lerp(tracker.rotation, quaternion2, 50f / num2);
						cantTurnToPlayer = Mathf.MoveTowards(cantTurnToPlayer, 5f, Time.deltaTime);
					}
					else
					{
						cantTurnToPlayer = Mathf.MoveTowards(cantTurnToPlayer, 0f, Time.deltaTime);
					}
				}
				quaternion2 = Quaternion.RotateTowards(previousHeadRotation, quaternion2, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(previousHeadRotation, quaternion2) * 13.5f), 10f) * headRotationSpeedMultiplier * lcon.eid.totalSpeedModifier);
			}
			tracker.rotation = quaternion2;
			previousHeadRotation = tracker.rotation;
			notAtDefaultHeadRotation = true;
		}
		else if (notAtDefaultHeadRotation)
		{
			if (Quaternion.Angle(previousHeadRotation, tracker.rotation) > 1f)
			{
				tracker.rotation = Quaternion.RotateTowards(previousHeadRotation, tracker.rotation, Time.deltaTime * Mathf.Max(Mathf.Min(270f, Quaternion.Angle(previousHeadRotation, tracker.rotation) * 13.5f), 10f) * headRotationSpeedMultiplier * lcon.eid.totalSpeedModifier);
				previousHeadRotation = tracker.rotation;
			}
			else
			{
				previousHeadRotation = tracker.rotation;
				notAtDefaultHeadRotation = false;
			}
		}
		else
		{
			previousHeadRotation = tracker.rotation;
		}
		if (freezeTail)
		{
			for (int i = 0; i < tailBones.Length; i++)
			{
				tailBones[i].SetPositionAndRotation(defaultTailPositions[i], defaultTailRotations[i]);
			}
		}
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (lcon.secondPhase)
		{
			defaultBodyRotation = Quaternion.LookRotation(base.transform.position - new Vector3(Target.position.x, base.transform.position.y, Target.position.z));
		}
		if (inAction || Target == null)
		{
			return;
		}
		attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * lcon.eid.totalSpeedModifier);
		if (lcon.readyForSecondPhase)
		{
			forceBeam = true;
			Descend();
		}
		else
		{
			if (!(attackCooldown <= 0f))
			{
				return;
			}
			if (recentAttacks >= 3)
			{
				if (lcon.secondPhase)
				{
					recentAttacks = 0;
					forceBeam = true;
				}
				else
				{
					Descend();
				}
				return;
			}
			if (Vector3.Distance(Target.position, tracker.position) < 50f)
			{
				Bite();
				previousAttack = 1;
				recentAttacks++;
				return;
			}
			int num = Random.Range(0, 2);
			if (num == previousAttack)
			{
				num++;
			}
			if (num > 1)
			{
				num = 0;
			}
			if (forceBeam)
			{
				num = 2;
				forceBeam = false;
			}
			switch (num)
			{
			case 0:
				ProjectileBurst();
				break;
			case 1:
				Bite();
				break;
			case 2:
				BeamAttack();
				break;
			}
			previousAttack = num;
			recentAttacks++;
		}
	}

	private void FixedUpdate()
	{
		if (!active)
		{
			return;
		}
		if (projectileBursting)
		{
			if (projectileBurstCooldown > 0f)
			{
				projectileBurstCooldown = Mathf.MoveTowards(projectileBurstCooldown, 0f, Time.deltaTime * lcon.eid.totalSpeedModifier);
			}
			else
			{
				if (lcon.difficulty >= 2)
				{
					projectileBurstCooldown = 0.025f;
				}
				else
				{
					projectileBurstCooldown = ((lcon.difficulty == 1) ? 0.0375f : 0.05f);
				}
				projectilesLeftInBurst--;
				GameObject gameObject = Object.Instantiate((lcon.difficulty >= 2 && projectilesLeftInBurst % 20 == 0) ? MonoSingleton<DefaultReferenceManager>.Instance.projectileExplosive : MonoSingleton<DefaultReferenceManager>.Instance.projectile, shootPoint.position, shootPoint.rotation);
				if (gameObject.TryGetComponent<Projectile>(out var component))
				{
					component.safeEnemyType = EnemyType.Leviathan;
					if (lcon.difficulty == 0)
					{
						component.speed *= 0.75f;
					}
					component.enemyDamageMultiplier = 0.5f;
					component.damage *= lcon.eid.totalDamageModifier;
				}
				if (projectilesLeftInBurst % 10 != 0)
				{
					gameObject.transform.Rotate(Vector3.forward * (projectilesLeftInBurst % 10) * 36f);
					if (projectilesLeftInBurst > projectileBurstMaximum / 2)
					{
						gameObject.transform.Rotate(Vector3.up * projectileSpreadAmount * ((lcon.difficulty < 2) ? 1 : 2) * 1.5f * (1f - (float)projectilesLeftInBurst / (float)projectileBurstMaximum));
					}
					else
					{
						gameObject.transform.Rotate(Vector3.up * projectileSpreadAmount * ((lcon.difficulty < 2) ? 1 : 2) * 1.5f * ((float)projectilesLeftInBurst / (float)projectileBurstMaximum));
					}
				}
				else if (Target != null)
				{
					gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.LookRotation(Target.headPosition - gameObject.transform.position), 5f);
				}
				gameObject.transform.localScale *= 2f;
			}
		}
		if (projectileBursting && (projectilesLeftInBurst <= 0 || Target == null))
		{
			projectileBursting = false;
			predictPlayer = false;
			trackerIgnoreLimits = false;
			anim.SetBool("ProjectileBurst", false);
		}
	}

	private void Descend()
	{
		if (active)
		{
			inAction = true;
			headRotationSpeedMultiplier = 0.5f;
			lookAtPlayer = false;
			rotateBody = false;
			anim.SetBool("Sunken", true);
			Object.Instantiate<AudioSource>(biteWindupSound, tracker.position, Quaternion.identity, tracker).SetPitch(0.5f);
			recentAttacks = 0;
			previousAttack = -1;
		}
	}

	private void DescendEnd()
	{
		if (active)
		{
			base.gameObject.SetActive(value: false);
			lcon.MainPhaseOver();
		}
	}

	public void ChangePosition()
	{
		if (active)
		{
			int num = Random.Range(0, spawnPositions.Length);
			if (spawnPositions.Length > 1 && num == previousSpawnPosition)
			{
				num++;
			}
			if (num >= spawnPositions.Length)
			{
				num = 0;
			}
			if ((bool)lcon.tail && lcon.tail.gameObject.activeInHierarchy && Vector3.Distance(spawnPositions[num], new Vector3(lcon.tail.transform.localPosition.x, spawnPositions[num].y, lcon.tail.transform.localPosition.z)) < 10f)
			{
				num++;
			}
			if (num >= spawnPositions.Length)
			{
				num = 0;
			}
			base.transform.localPosition = spawnPositions[num];
			previousSpawnPosition = num;
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(base.transform.parent.position.x, base.transform.position.y, base.transform.parent.position.z));
			base.gameObject.SetActive(value: true);
			ResetDefaults();
			Ascend();
		}
	}

	public void CenterPosition()
	{
		if (active)
		{
			base.transform.localPosition = centerSpawnPosition;
			base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(Target.position.x, base.transform.position.y, Target.position.z));
			base.gameObject.SetActive(value: true);
			ResetDefaults();
			Ascend();
		}
	}

	private void Ascend()
	{
		if (active)
		{
			inAction = true;
			headRotationSpeedMultiplier = 0.5f;
			lookAtPlayer = false;
			rotateBody = false;
			anim.SetBool("Sunken", false);
			BigSplash();
			if (lcon.secondPhase)
			{
				attackCooldown = 3f;
			}
			else if (lcon.difficulty <= 2)
			{
				attackCooldown = 1 + (2 - lcon.difficulty);
			}
		}
	}

	private void StartHeadTracking()
	{
		lookAtPlayer = true;
	}

	private void StartBodyTracking()
	{
		rotateBody = true;
	}

	private void Bite()
	{
		if (active)
		{
			rotateBody = true;
			anim.SetTrigger("Bite");
			trackerOverrideAnimation = true;
			inAction = true;
			Object.Instantiate<AudioSource>(biteWindupSound, tracker.position, Quaternion.identity, tracker);
			if (lcon.difficulty <= 2)
			{
				attackCooldown = 0.2f + (float)(2 - lcon.difficulty);
			}
		}
	}

	private void BiteStopTracking()
	{
		if (!active)
		{
			return;
		}
		lookAtPlayer = false;
		trackerOverrideAnimation = false;
		bodyRotationOverride = true;
		if (Target != null)
		{
			if (lcon.difficulty == 0)
			{
				bodyRotationOverrideTarget = Target.position;
			}
			else
			{
				bodyRotationOverrideTarget = Target.position + Target.GetVelocity() * ((lcon.difficulty >= 2) ? 0.85f : 0.4f);
			}
			GameObject gameObject = Object.Instantiate(warningFlash, lcon.eid.weakPoint.transform.position + lcon.eid.weakPoint.transform.up, Quaternion.LookRotation(MonoSingleton<CameraController>.Instance.transform.position - tracker.position), tracker);
			gameObject.transform.localScale *= 0.05f;
			gameObject.transform.position += gameObject.transform.forward * 10f;
		}
	}

	private void BiteDamageStart()
	{
		if (active)
		{
			biteSwingCheck.DamageStart();
			parryHelper.SetActive(value: true);
			Object.Instantiate<AudioSource>(swingSound, base.transform.position, Quaternion.identity);
			if (trackerBones == null || trackerBones.Count == 0)
			{
				trackerBones.AddRange(tracker.GetComponentsInChildren<Transform>());
			}
			lcon.stat.parryables = trackerBones;
			lcon.stat.ParryableCheck(partial: true);
		}
	}

	public void BiteDamageStop()
	{
		biteSwingCheck.DamageStop();
		parryHelper.SetActive(value: false);
		lcon.stat.partiallyParryable = false;
	}

	private void BiteResetRotation()
	{
		rotateBody = false;
		bodyRotationOverride = false;
	}

	private void BiteEnd()
	{
		headRotationSpeedMultiplier = 1f;
		lookAtPlayer = true;
		StopAction();
	}

	private void ProjectileBurst()
	{
		if (active)
		{
			anim.SetBool("ProjectileBurst", true);
			projectilesLeftInBurst = projectileBurstMaximum;
			inAction = true;
			lookAtPlayer = true;
			if (lcon.secondPhase)
			{
				predictPlayer = true;
			}
			if (lcon.difficulty <= 2)
			{
				attackCooldown = 0.5f + (float)(2 - lcon.difficulty);
			}
			Object.Instantiate<AudioSource>(projectileWindupSound, tracker.position, Quaternion.identity, tracker);
		}
	}

	private void ProjectileBurstStart()
	{
		projectileBursting = true;
	}

	private void BeamAttack()
	{
		if (active)
		{
			inAction = true;
			lcon.stopTail = true;
			anim.SetBool("BeamAttack", true);
			Object.Instantiate<AudioSource>(beamWindupSound, tracker.position, Quaternion.identity, tracker);
			bodyRotationOverride = true;
			bodyRotationOverrideTarget = new Vector3(Target.position.x, defaultPosition.y, Target.position.z);
			BeamCharge();
		}
	}

	private void BeamCharge()
	{
		beamCharge.SetActive(value: true);
		lookAtPlayer = false;
	}

	private void BeamTurn()
	{
		bodyRotationOverride = true;
		lookAtPlayer = false;
		trackerOverrideAnimation = false;
		bodyRotationOverrideTarget = base.transform.position + Quaternion.AngleAxis(-90f, Vector3.up) * (new Vector3(Target.position.x, defaultPosition.y, Target.position.z) - base.transform.position);
	}

	private void BeamStart()
	{
		beamCharge.SetActive(value: false);
		beam.SetActive(value: true);
		beamTime = 10f;
		lookAtPlayer = false;
		trackerOverrideAnimation = false;
		notAtDefaultHeadRotation = false;
		previousHeadRotation = tracker.rotation;
		base.transform.position = defaultPosition;
	}

	private void BeamStop()
	{
		beam.SetActive(value: false);
		anim.SetBool("BeamAttack", false);
		anim.SetFloat("BeamSpeed", 0f);
		lookAtPlayer = true;
		bodyRotationOverride = false;
		bodyRotationOverrideTarget = Target.position;
		lcon.stopTail = false;
	}

	private void StopAction()
	{
		inAction = false;
	}

	private void Roar()
	{
		onRoar?.Invoke();
	}

	private void BigSplash()
	{
		Object.Instantiate(lcon.bigSplash, new Vector3(tracker.position.x, base.transform.position.y, tracker.position.z), Quaternion.LookRotation(Vector3.up));
	}

	public void GotParried()
	{
		BiteDamageStop();
		BiteResetRotation();
		anim.Play("BiteParried", -1, 0f);
		Object.Instantiate<AudioSource>(hurtSound, tracker.position, Quaternion.identity, tracker);
		MonoSingleton<StyleHUD>.Instance.AddPoints(500, "ultrakill.downtosize", null, lcon.eid);
	}

	public void Death()
	{
		BiteDamageStop();
		anim.Play("Death", -1, 0f);
		anim.SetBool("Death", true);
	}

	public void DeathEnd()
	{
		lcon.DeathEnd();
	}

	public void HeadExplode()
	{
		headExploded = true;
		onHeadExplode?.Invoke();
		beam.SetActive(value: false);
		beamCharge.SetActive(value: false);
		lcon.FinalExplosion();
	}
}
