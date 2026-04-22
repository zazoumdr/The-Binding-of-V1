using System.Collections.Generic;
using SettingsMenu.Components.Pages;
using UnityEngine;
using UnityEngine.AI;

public class Minotaur : EnemyScript, IHitTargetCallback
{
	private Rigidbody rb;

	private EnemyIdentifier eid;

	private Animator anim;

	private Enemy mach;

	private NavMeshAgent nma;

	private int difficulty = -1;

	private bool gotValues;

	private bool dead;

	private float cooldown;

	private int previousAttack = -1;

	private int currentAttacks;

	private float ramCooldown = 15f;

	private bool inAction;

	private bool moveForward;

	private float moveSpeed = 1f;

	private float moveBreakSpeed;

	private bool trackTarget;

	private float trackSpeed = 1f;

	private Transform tempTarget;

	private bool chaseTarget;

	[SerializeField]
	private AudioSource roar;

	[SerializeField]
	private AudioClip roarClip;

	[SerializeField]
	private AudioClip roarShortClip;

	[SerializeField]
	private AudioClip squealClip;

	[SerializeField]
	private AudioClip longGruntClip;

	[SerializeField]
	private AudioClip exhaleClip;

	[SerializeField]
	private SwingCheck2[] hammerSwingChecks;

	[SerializeField]
	private TrailRenderer hammerTrail;

	[SerializeField]
	private Transform hammerPoint;

	[SerializeField]
	private GameObject hammerImpact;

	[SerializeField]
	private GameObject hammerExplosion;

	[SerializeField]
	private GameObject hammerBigExplosion;

	public bool tantrumOnSpawn;

	[SerializeField]
	private GameObject meatInHand;

	[SerializeField]
	private GameObject handBlood;

	[SerializeField]
	private GameObject toxicCloud;

	[SerializeField]
	private GameObject toxicCloudLong;

	[SerializeField]
	private GameObject goop;

	[SerializeField]
	private GameObject goopLong;

	[HideInInspector]
	public float ramTimer;

	[SerializeField]
	private GameObject ramStuff;

	[SerializeField]
	private GameObject fallEffect;

	private Vector3 deathPosition;

	private List<Transform> deathTransforms = new List<Transform>();

	private float deathTimer;

	private GoreZone gz;

	public UltrakillEvent onDeath;

	private float playerAirBias = 0.5f;

	private void Start()
	{
		GetValues();
	}

	public override void OnDamage(ref DamageData data)
	{
		if (ramTimer > 0f && data.hitter == "ground slam")
		{
			GotSlammed();
		}
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			rb = GetComponent<Rigidbody>();
			eid = GetComponent<EnemyIdentifier>();
			anim = GetComponent<Animator>();
			mach = GetComponent<Enemy>();
			nma = GetComponent<NavMeshAgent>();
			gz = GoreZone.ResolveGoreZone(base.transform);
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			SetSpeed();
			if (tantrumOnSpawn)
			{
				currentAttacks++;
				previousAttack = 0;
				QuickTantrum();
			}
			Invoke("SlowUpdate", GetUpdateRate(nma));
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		float num = 1f;
		if (difficulty >= 4)
		{
			num = 1.2f;
		}
		switch (difficulty)
		{
		case 2:
			num = 0.9f;
			break;
		case 1:
			num = 0.85f;
			break;
		case 0:
			num = 0.7f;
			break;
		}
		return new EnemyMovementData
		{
			speed = 50f * num,
			angularSpeed = 12000f,
			acceleration = 100f
		};
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		GetValues();
		float num = 1f;
		if (difficulty >= 4)
		{
			num = 1.2f;
		}
		if (difficulty == 2)
		{
			num = 0.9f;
		}
		else if (difficulty == 1)
		{
			num = 0.85f;
		}
		else if (difficulty == 0)
		{
			num = 0.7f;
		}
		anim.speed = num * eid.totalSpeedModifier;
		nma.speed = 50f * anim.speed;
	}

	private void Update()
	{
		if (dead)
		{
			deathTimer = Mathf.MoveTowards(deathTimer, 5f, Time.deltaTime);
			base.transform.position = new Vector3(deathPosition.x + Random.Range((0f - deathTimer) / 10f, deathTimer / 10f), deathPosition.y + Random.Range((0f - deathTimer) / 10f, deathTimer / 10f), deathPosition.z + Random.Range((0f - deathTimer) / 10f, deathTimer / 10f));
			if (deathTimer < 4f && Random.Range(0f, 1f) < Time.deltaTime * 5f * deathTimer)
			{
				int index = Random.Range(0, deathTransforms.Count);
				if (deathTransforms[index] != null)
				{
					GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, eid);
					if ((bool)gore && (bool)gz && (bool)gz.goreZone)
					{
						gore.transform.position = deathTransforms[index].position;
						gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					if ((bool)gore && gore.TryGetComponent<Bloodsplatter>(out var component))
					{
						component.GetReady();
					}
				}
				else
				{
					deathTransforms.RemoveAt(index);
				}
			}
			if (deathTimer >= 1f)
			{
				if (!roar.isPlaying)
				{
					Roar(0.75f);
				}
				roar.SetPitch(0.75f - (deathTimer - 1f) / 10f);
			}
			if (deathTimer == 5f)
			{
				BloodExplosion();
			}
			return;
		}
		if (trackTarget && (eid.target != null || tempTarget != null))
		{
			Transform transform = (tempTarget ? tempTarget : eid.target.targetTransform);
			Quaternion quaternion = Quaternion.LookRotation(new Vector3(transform.position.x, base.transform.position.y, transform.position.z) - base.transform.position);
			float num = 5f;
			if (difficulty == 1)
			{
				num = 3f;
			}
			if (difficulty == 0)
			{
				num = 1.5f;
			}
			rb.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, (45f + Quaternion.Angle(base.transform.rotation, quaternion)) * num * trackSpeed * anim.speed * Time.deltaTime);
		}
		if (mach.gc.onGround && ((Behaviour)(object)nma).enabled && nma.velocity.magnitude > 2f)
		{
			anim.SetBool("Running", true);
			anim.SetFloat("RunningSpeed", nma.velocity.magnitude / 25f);
		}
		else
		{
			anim.SetBool("Running", false);
			anim.SetFloat("RunningSpeed", 0f);
		}
		if (cooldown > 0f && !inAction)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		if (ramCooldown > 0f)
		{
			ramCooldown = Mathf.MoveTowards(ramCooldown, 0f, Time.deltaTime);
		}
		if (eid.target != null && eid.target.isPlayer)
		{
			playerAirBias = Mathf.MoveTowards(playerAirBias, (!MonoSingleton<NewMovement>.Instance.gc.onGround) ? 1 : 0, Time.deltaTime / 20f);
		}
	}

	private void FixedUpdate()
	{
		if (dead)
		{
			return;
		}
		rb.isKinematic = !moveForward && mach.gc.onGround;
		if (moveForward)
		{
			rb.velocity = base.transform.forward * 30f * moveSpeed * anim.speed;
			if (!mach.gc.onGround)
			{
				rb.velocity += Vector3.up * rb.velocity.y;
			}
		}
		else if (!mach.gc.onGround)
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		if (moveBreakSpeed != 0f)
		{
			moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, Time.fixedDeltaTime * moveBreakSpeed);
		}
		if (ramTimer > 0f)
		{
			if (Physics.SphereCast(base.transform.position + base.transform.up * 5f + base.transform.forward * 5f, 4f, base.transform.forward, out var hitInfo, rb.velocity.magnitude * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment)))
			{
				if (hitInfo.transform.gameObject.CompareTag("Breakable") && hitInfo.transform.TryGetComponent<Breakable>(out var component) && !component.playerOnly && !component.specialCaseOnly)
				{
					component.Break();
				}
				else
				{
					RamBonk(hitInfo.point);
				}
				return;
			}
			ramTimer = Mathf.MoveTowards(ramTimer, 0f, Time.fixedDeltaTime);
			if (ramTimer == 0f)
			{
				anim.Play("RamSwing");
				Roar(roarShortClip, 1.5f);
				moveBreakSpeed = 5f;
				StopRam();
			}
			else
			{
				anim.SetBool("Ramming", true);
			}
		}
		else
		{
			anim.SetBool("Ramming", false);
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", GetUpdateRate(nma));
		if (!inAction && mach.gc.onGround)
		{
			if (eid.target == null)
			{
				return;
			}
			float num = Vector3.Distance(base.transform.position, eid.target.position);
			RaycastHit hitInfo;
			bool flag = !Physics.Raycast(base.transform.position + Vector3.up, eid.target.position - (base.transform.position + Vector3.up), out hitInfo, Vector3.Distance(eid.target.position, base.transform.position + Vector3.up), LayerMaskDefaults.Get(LMD.Environment));
			if (flag && cooldown <= 0f)
			{
				if (currentAttacks >= 3 || ramCooldown <= 0f)
				{
					Ram();
					ramCooldown = 15f;
					currentAttacks = 0;
					previousAttack = -1;
				}
				else if (num <= 20f)
				{
					int num2 = Random.Range(0, 3);
					if (num2 == previousAttack)
					{
						num2++;
					}
					if (num2 >= 3)
					{
						num2 = 0;
					}
					switch (num2)
					{
					case 0:
						HammerTantrum();
						break;
					case 1:
						if (num < 15f || Vector3.Distance(base.transform.position, eid.target.position + eid.target.rigidbody.velocity.normalized) < num + 0.2f)
						{
							HammerSmash();
						}
						break;
					case 2:
						if (Random.Range(0f, 1f) > playerAirBias)
						{
							playerAirBias = 1f;
							MeatPool();
						}
						else
						{
							playerAirBias = 0f;
							MeatCloud();
						}
						break;
					}
					if (inAction)
					{
						previousAttack = num2;
						currentAttacks++;
					}
				}
				if (inAction)
				{
					return;
				}
			}
			if (!chaseTarget)
			{
				if (num > 20f)
				{
					chaseTarget = true;
				}
				else if (!flag && !hitInfo.transform.gameObject.CompareTag("Breakable"))
				{
					chaseTarget = true;
				}
			}
			if (!chaseTarget)
			{
				return;
			}
			if (flag && num <= 15f)
			{
				chaseTarget = false;
				if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
				{
					mach.SetDestination(base.transform.position);
				}
			}
			else if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
			{
				mach.SetDestination(eid.target.position);
			}
		}
		else
		{
			chaseTarget = false;
		}
	}

	private void Ram()
	{
		anim.Play("RamWindup", -1, 0f);
		inAction = true;
		trackTarget = true;
		Roar(exhaleClip);
		((Behaviour)(object)nma).enabled = false;
	}

	private void RamStart()
	{
		anim.Play("RamRun", -1, 0f);
		anim.SetBool("Ramming", true);
		moveSpeed = 2f;
		moveForward = true;
		trackSpeed = 0.25f;
		ramTimer = 3f;
		ramStuff.SetActive(value: true);
		GameObject obj = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, ramStuff.transform.position + ramStuff.transform.forward * 1.5f, base.transform.rotation);
		obj.transform.localScale *= 15f;
		obj.transform.SetParent(ramStuff.transform, worldPositionStays: true);
		mach.ParryableCheck();
		Roar(0.75f);
	}

	private void RamBonk(Vector3 point)
	{
		if (point != Vector3.zero)
		{
			Object.Instantiate(hammerImpact, point, Quaternion.identity);
			base.transform.LookAt(new Vector3(point.x, base.transform.position.y, point.z));
		}
		anim.Play("RamBonk", -1, 0f);
		anim.SetBool("Ramming", false);
		moveForward = false;
		ramTimer = 0f;
		trackTarget = false;
		ramStuff.SetActive(value: false);
		mach.parryable = false;
		eid.hitter = "enemy";
		mach.GetHurt(GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, Vector3.zero, 6f, 0f);
		MonoSingleton<CameraController>.Instance.CameraShake(2f);
		Roar(longGruntClip, 2f);
	}

	private void StopRam()
	{
		anim.SetBool("Ramming", false);
		ramStuff.SetActive(value: false);
		mach.parryable = false;
		ramTimer = 0f;
	}

	private void MeatCloud()
	{
		((Behaviour)(object)nma).enabled = false;
		anim.Play("MeatHigh", -1, 0f);
		inAction = true;
		trackTarget = true;
		Roar(longGruntClip);
	}

	private void MeatPool()
	{
		((Behaviour)(object)nma).enabled = false;
		anim.Play("MeatLow", -1, 0f);
		inAction = true;
		trackTarget = true;
		Roar(longGruntClip);
	}

	private void HandBlood()
	{
		Object.Instantiate(handBlood, meatInHand.transform.position, Quaternion.identity);
	}

	private void MeatSpawn()
	{
		meatInHand.SetActive(value: true);
		HandBlood();
	}

	private void MeatExplode()
	{
		meatInHand.SetActive(value: false);
		HandBlood();
		GameObject gameObject = Object.Instantiate((difficulty >= 4) ? toxicCloudLong : toxicCloud, meatInHand.transform.position, Quaternion.identity);
		gameObject.transform.SetParent(gz.transform, worldPositionStays: true);
		if (difficulty == 1)
		{
			gameObject.transform.localScale *= 0.85f;
		}
		else if (difficulty == 0)
		{
			gameObject.transform.localScale *= 0.75f;
		}
	}

	private void MeatSplash()
	{
		meatInHand.SetActive(value: false);
		HandBlood();
		GameObject gameObject = Object.Instantiate((difficulty >= 4) ? goopLong : goop, new Vector3(meatInHand.transform.position.x, base.transform.position.y, meatInHand.transform.position.z), Quaternion.identity);
		gameObject.transform.SetParent(gz.transform, worldPositionStays: true);
		if (difficulty == 1)
		{
			gameObject.transform.localScale *= 0.85f;
		}
		else if (difficulty == 0)
		{
			gameObject.transform.localScale *= 0.75f;
		}
	}

	private void MeatThrowThrow()
	{
		meatInHand.SetActive(value: false);
	}

	private void HammerSmash()
	{
		if (!dead)
		{
			((Behaviour)(object)nma).enabled = false;
			anim.Play("HammerSmash", -1, 0f);
			inAction = true;
			trackTarget = true;
			Roar(squealClip, 0.75f);
		}
	}

	private void HammerTantrum()
	{
		if (!dead)
		{
			((Behaviour)(object)nma).enabled = false;
			anim.Play("HammerTantrum", -1, 0f);
			inAction = true;
			trackTarget = true;
			Roar(1.25f);
		}
	}

	public void QuickTantrum()
	{
		if (!dead)
		{
			((Behaviour)(object)nma).enabled = false;
			anim.Play("HammerTantrum", -1, 0.275f);
			inAction = true;
		}
	}

	private void HammerSwingStart()
	{
		if (!dead)
		{
			((Behaviour)(object)nma).enabled = false;
			SwingCheck2[] array = hammerSwingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStart();
			}
			hammerTrail.emitting = true;
			moveForward = true;
			trackTarget = false;
		}
	}

	private void HammerImpact()
	{
		Object.Instantiate(hammerImpact, new Vector3(hammerPoint.position.x, base.transform.position.y + 0.25f, hammerPoint.position.z), Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
	}

	private void HammerExplosion(int size = 0)
	{
		Explosion[] componentsInChildren = Object.Instantiate((size == 0) ? hammerExplosion : hammerBigExplosion, new Vector3(hammerPoint.position.x, base.transform.position.y + 0.25f, hammerPoint.position.z), Quaternion.identity).GetComponentsInChildren<Explosion>();
		foreach (Explosion obj in componentsInChildren)
		{
			obj.toIgnore.Add(EnemyType.Minotaur);
			obj.maxSize *= ((size == 0) ? 2f : 1.75f);
			obj.speed *= ((size == 0) ? 2f : 1.75f);
		}
		MonoSingleton<CameraController>.Instance.CameraShake(1.5f);
	}

	private void HammerSwingStop(int startTrackingTarget = 0)
	{
		SwingCheck2[] array = hammerSwingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
		hammerTrail.emitting = false;
		moveForward = false;
		trackTarget = startTrackingTarget >= 1;
	}

	private void HammerSwingStopImpact(int startTrackingTarget = 0)
	{
		HammerImpact();
		HammerSwingStop(startTrackingTarget);
	}

	private void StartTracking()
	{
		trackTarget = true;
	}

	private void StopMoving()
	{
		moveForward = false;
		moveSpeed = 1f;
		moveBreakSpeed = 0f;
	}

	private void GotParried()
	{
		if (ramTimer > 0f)
		{
			RamBonk(MonoSingleton<NewMovement>.Instance.transform.position);
		}
		anim.Play("RamParried");
		((Behaviour)(object)nma).enabled = false;
		moveForward = true;
		moveSpeed = -2.5f;
		moveBreakSpeed = 5f;
		MonoSingleton<CameraController>.Instance.CameraShake(3f);
		eid.hitter = "";
		mach.GetHurt(GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, Vector3.zero, 20f, 0f);
		ramCooldown = 30f;
		Roar(2.5f);
	}

	public void GotSlammed()
	{
		if (ramTimer > 0f)
		{
			RamBonk(base.transform.position + base.transform.forward);
		}
		anim.Play("RamParried");
		((Behaviour)(object)nma).enabled = false;
		moveForward = true;
		moveSpeed = 2.5f;
		moveBreakSpeed = 1.5f;
		MonoSingleton<CameraController>.Instance.CameraShake(3f);
		eid.hitter = "";
		mach.GetHurt(GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, Vector3.zero, 20f, 0f);
		MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
		mach.parryable = false;
		ramCooldown = 30f;
		Roar(2.5f);
	}

	private void StopAction()
	{
		if ((bool)mach.gc && mach.gc.onGround)
		{
			((Behaviour)(object)nma).enabled = true;
		}
		inAction = false;
		moveForward = false;
		mach.parryable = false;
		trackTarget = true;
		moveSpeed = 1f;
		moveBreakSpeed = 0f;
		trackSpeed = 1f;
	}

	public void TargetBeenHit()
	{
		if (ramTimer > 0f)
		{
			anim.Play("RamSwing");
			moveBreakSpeed = 5f;
			StopRam();
			Roar(roarShortClip, 1.5f);
		}
		SwingCheck2[] array = hammerSwingChecks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
	}

	public void Death()
	{
		if (!dead)
		{
			((Behaviour)(object)nma).enabled = false;
			dead = true;
			deathPosition = base.transform.position;
			deathTransforms.AddRange(GetComponentsInChildren<Transform>());
			HammerSwingStop();
			StopAction();
			meatInHand.SetActive(value: false);
			anim.Play("Death", -1, 0f);
			anim.SetBool("Dead", true);
			anim.speed = 1f;
			inAction = true;
			trackTarget = false;
			roar.Stop();
			Invoke("Roar", 0.9f);
			MonoSingleton<TimeController>.Instance.SlowDown(0.001f);
		}
	}

	private void BloodExplosion()
	{
		List<Transform> list = new List<Transform>();
		BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
		foreach (Transform deathTransform in deathTransforms)
		{
			if (deathTransform != null && Random.Range(0f, 1f) < 0.33f)
			{
				GameObject gore = instance.GetGore(GoreType.Head, eid);
				if ((bool)gore)
				{
					gore.transform.position = deathTransform.position;
					if (gz != null && gz.goreZone != null)
					{
						gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					gore.GetComponent<Bloodsplatter>()?.GetReady();
				}
			}
			else if (deathTransform == null)
			{
				list.Add(deathTransform);
			}
		}
		if (list.Count > 0)
		{
			foreach (Transform item in list)
			{
				deathTransforms.Remove(item);
			}
			list.Clear();
		}
		if (GraphicsSettings.bloodEnabled && base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < 40; i++)
			{
				GameObject gib;
				if (i < 30)
				{
					gib = instance.GetGib(BSType.gib);
					if ((bool)gib && (bool)gz && (bool)gz.gibZone)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							mach.ReadyGib(gib, deathTransforms[Random.Range(0, deathTransforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(4f, 7f);
					}
					else
					{
						i = 30;
					}
					continue;
				}
				if (i < 35)
				{
					gib = instance.GetGib(BSType.eyeball);
					if ((bool)gib && (bool)gz && (bool)gz.gibZone)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							mach.ReadyGib(gib, deathTransforms[Random.Range(0, deathTransforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(3f, 6f);
					}
					else
					{
						i = 35;
					}
					continue;
				}
				gib = instance.GetGib(BSType.brainChunk);
				if (!gib || !gz || !gz.gibZone)
				{
					break;
				}
				if ((bool)gz && (bool)gz.gibZone)
				{
					mach.ReadyGib(gib, deathTransforms[Random.Range(0, deathTransforms.Count)].gameObject);
				}
				gib.transform.localScale *= Random.Range(3f, 4f);
			}
		}
		if (!eid.dontCountAsKills && gz != null)
		{
			gz.AddDeath();
			MonoSingleton<StatsManager>.Instance.kills++;
		}
		ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
		if (componentInParent != null)
		{
			componentInParent.AddDeadEnemy();
		}
		onDeath?.Invoke();
		MonoSingleton<TimeController>.Instance.SlowDown(0.0001f);
		Object.Destroy(base.gameObject);
	}

	private void Roar()
	{
		Roar(roarClip);
	}

	private void Roar(float pitch = 1f)
	{
		Roar(roarClip, pitch);
	}

	private void Roar(AudioClip clip, float pitch = 1f)
	{
		roar.clip = clip;
		roar.SetPitch(Random.Range(pitch - 0.1f, pitch + 0.1f));
		roar.Play(tracked: true);
	}

	private void BodyImpact()
	{
		Object.Instantiate(fallEffect, base.transform.position, Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
	}
}
