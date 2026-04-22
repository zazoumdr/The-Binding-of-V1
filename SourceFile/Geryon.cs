using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geryon : MonoBehaviour
{
	private bool active = true;

	private Animator anim;

	private Rigidbody rb;

	private EnemyIdentifier eid;

	private AudioSource aud;

	private GoreZone gz;

	private BossHealthBar bhb;

	private int difficulty = -1;

	[SerializeField]
	private Transform rotateAround;

	[SerializeField]
	private float minimumAroundDistance;

	[SerializeField]
	private float maximumAroundDistance;

	private float rotateAroundDistance;

	private int moveDirection = 1;

	private float moveSpeed = 20f;

	private bool inAction;

	private float cooldown = 3f;

	private bool trackY;

	private bool slowRotation;

	private Transform projectileParent;

	[SerializeField]
	private Transform leftHandShootPoint;

	[SerializeField]
	private Transform rightHandShootPoint;

	[SerializeField]
	private Transform bowShootPoint;

	[SerializeField]
	private GameObject bowChargeParticle;

	[SerializeField]
	private GameObject bowUpShootParticle;

	[SerializeField]
	private GameObject bowForwardShootParticle;

	[SerializeField]
	private GameObject bowUpBeam;

	[SerializeField]
	private GameObject bowForwardBeam;

	private int beamsAmount;

	private Coroutine beamRoutine;

	[SerializeField]
	private PhysicalShockwave clapShockwave;

	[SerializeField]
	private GameObject clapEffect;

	[SerializeField]
	private GameObject clapChargeParticle;

	private Vector3 clapChargeDefaultPosition;

	private bool clapChargeFollowing;

	[SerializeField]
	private Projectile palmProjectile;

	[SerializeField]
	private GameObject[] palmProjectileChargeParticles;

	private float projectileRotation;

	private int projectileRotationDirection;

	[SerializeField]
	private GameObject playerBlockerShield;

	[SerializeField]
	private GameObject playerPushBacker;

	[SerializeField]
	private GameObject playerProximityExplosion;

	private float playerPushBackerCooldown;

	[SerializeField]
	private GameObject dustParticle;

	private float currentHeat;

	private float maximumHeat;

	private float stunTime;

	private float maximumStunTime;

	private bool stunned;

	[SerializeField]
	private GameObject weakPointHitbox;

	[SerializeField]
	private GameObject fakeBloodSplatter;

	private float flashTimer;

	private float currentBarValue;

	private float originalHealth;

	private bool secondPhase;

	private bool cancelledAction;

	private TimeSince sinceCancelledAction;

	private List<GeryonAttack> previousAttacks = new List<GeryonAttack>();

	[SerializeField]
	private AudioClip bowUpSound;

	[SerializeField]
	private AudioClip bowForwardSound;

	[SerializeField]
	private AudioClip waveClapSound;

	[SerializeField]
	private AudioClip palmProjectilesSound;

	[SerializeField]
	private AudioClip bigHurtSound;

	[SerializeField]
	private AudioClip recoverySound;

	[SerializeField]
	private AudioClip deathSound;

	[Header("Body Part References")]
	public Collider tailBase;

	public Collider tailMid;

	public Collider wingRight;

	public Collider wingLeft;

	private GameObject currentEnrageEffect;

	public UltrakillEvent onPhaseChange;

	private void Start()
	{
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		eid = GetComponent<EnemyIdentifier>();
		aud = GetComponent<AudioSource>();
		gz = GoreZone.ResolveGoreZone(base.transform);
		bhb = GetComponent<BossHealthBar>();
		rotateAroundDistance = maximumAroundDistance;
		projectileParent = new GameObject("GeryonProjectileParent").transform;
		projectileParent.transform.SetParent(base.transform.parent, worldPositionStays: true);
		clapChargeDefaultPosition = clapChargeParticle.transform.localPosition;
		originalHealth = eid.Health;
		UpdateDifficulty();
		RandomizeDirection();
	}

	private void UpdateDifficulty()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			anim.speed = 1.2f;
			maximumHeat = 9f;
			break;
		case 3:
			anim.speed = 1f;
			maximumHeat = 6f;
			break;
		case 2:
			anim.speed = 0.9f;
			maximumHeat = 5f;
			break;
		case 1:
			anim.speed = 0.8f;
			maximumHeat = 4f;
			break;
		case 0:
			anim.speed = 0.7f;
			maximumHeat = 3f;
			break;
		}
		if (secondPhase)
		{
			Animator obj = anim;
			obj.speed *= 1.25f;
		}
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		UpdateCooldowns();
		PlayerBlocker();
		if ((bool)bhb)
		{
			UpdateBossBar();
		}
		MoveUpdate();
		if (eid.health < originalHealth / 2f && !secondPhase)
		{
			secondPhase = true;
			Animator obj = anim;
			obj.speed *= 1.25f;
			if (!currentEnrageEffect)
			{
				currentEnrageEffect = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.enrageEffect, eid.weakPoint.transform.position, Quaternion.identity);
				currentEnrageEffect.transform.SetParent(eid.weakPoint.transform, worldPositionStays: true);
				currentEnrageEffect.transform.localScale *= 10f;
			}
			onPhaseChange?.Invoke();
		}
		if (!inAction && cooldown <= 0f)
		{
			PickAttack();
		}
	}

	private void OnDestroy()
	{
		if ((bool)projectileParent)
		{
			Object.Destroy(projectileParent);
		}
	}

	private void UpdateCooldowns()
	{
		if (!inAction)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		playerPushBackerCooldown = Mathf.MoveTowards(playerPushBackerCooldown, 0f, Time.deltaTime);
		if (stunTime > 0f && stunned)
		{
			eid.hitter = "fire";
			eid.DeliverDamage(weakPointHitbox, Vector3.zero, weakPointHitbox.transform.position, Time.deltaTime * 2f, tryForExplode: false);
			stunTime = Mathf.MoveTowards(stunTime, 0f, Time.deltaTime * (secondPhase ? 1.5f : 1f));
			if (stunTime <= 0f && stunned)
			{
				Unstun();
			}
		}
		if (cancelledAction && (float)sinceCancelledAction >= 0.5f && !anim.IsInTransition(0))
		{
			cancelledAction = false;
		}
	}

	private void PlayerBlocker()
	{
		if (eid.target != null && !stunned && !IsInMaxHeat() && !(playerPushBackerCooldown > 0f) && Vector3.Distance(playerBlockerShield.transform.position, eid.target.position) < 22f)
		{
			Object.Instantiate(playerProximityExplosion, playerBlockerShield.transform.position, Quaternion.identity).transform.SetParent(base.transform, worldPositionStays: true);
			playerPushBackerCooldown = 3f;
			currentHeat += 1.01f;
		}
	}

	private void UpdateBossBar()
	{
		if (stunTime > 0f)
		{
			bhb.UpdateSecondaryBar(stunTime / maximumStunTime);
			flashTimer = Mathf.MoveTowards(flashTimer, 1f, Time.deltaTime * 5f);
			bhb.SetSecondaryBarColor((flashTimer < 0.5f) ? Color.red : Color.black);
			if (flashTimer >= 1f)
			{
				flashTimer = 0f;
			}
			return;
		}
		float num = currentHeat / (maximumHeat + (float)(secondPhase ? 1 : 0));
		if (num > currentBarValue)
		{
			currentBarValue = Mathf.MoveTowards(currentBarValue, num, Time.deltaTime * Mathf.Max(num - currentBarValue, 0.01f) * 10f);
		}
		else
		{
			currentBarValue = num;
		}
		bhb.UpdateSecondaryBar(currentBarValue);
		if (currentBarValue <= 0.33f)
		{
			bhb.SetSecondaryBarColor(Color.green);
		}
		else if (currentBarValue <= 0.66f)
		{
			bhb.SetSecondaryBarColor(Color.yellow);
		}
		else if (currentBarValue < 1f)
		{
			bhb.SetSecondaryBarColor(new Color(1f, 0.35f, 0f));
		}
		else
		{
			bhb.SetSecondaryBarColor(Color.red);
		}
	}

	private void MoveUpdate()
	{
		Quaternion quaternion = Quaternion.LookRotation(new Vector3(eid.target.position.x, trackY ? eid.target.position.y : base.transform.position.y, eid.target.position.z) - base.transform.position);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Mathf.Min(90f, Quaternion.Angle(quaternion, base.transform.rotation)) * (float)(slowRotation ? 2 : 8) * Time.deltaTime);
		if (inAction)
		{
			rb.velocity = Vector3.zero;
			if (clapChargeParticle.activeSelf)
			{
				if (clapChargeFollowing)
				{
					clapChargeParticle.transform.position = Vector3.Lerp(leftHandShootPoint.position, rightHandShootPoint.position, 0.5f);
				}
				else
				{
					clapChargeParticle.transform.localPosition = Vector3.MoveTowards(clapChargeParticle.transform.localPosition, clapChargeDefaultPosition, Time.deltaTime * 20f);
				}
			}
			return;
		}
		Vector3 vector = new Vector3(rotateAround.position.x, base.transform.position.y, rotateAround.position.z) - base.transform.position;
		Vector3 vector2 = Quaternion.Euler(0f, 90 * moveDirection, 0f) * vector.normalized;
		rb.velocity = Vector3.MoveTowards(rb.velocity, vector2 * moveSpeed, Time.deltaTime * moveSpeed * Mathf.Max(1f, Vector3.Distance(rb.velocity, vector2 * moveSpeed)));
		if (vector.magnitude > rotateAroundDistance * 1.25f || vector.magnitude < rotateAroundDistance * 0.75f)
		{
			Vector3 vector3 = rotateAround.position - vector.normalized * rotateAroundDistance;
			rb.MovePosition(Vector3.MoveTowards(rb.position, vector3, Time.deltaTime * 0.075f * moveSpeed * Vector3.Distance(rb.position, vector3)));
		}
		if (base.transform.position.y != eid.target.position.y)
		{
			rb.MovePosition(Vector3.MoveTowards(rb.position, new Vector3(base.transform.position.x, eid.target.position.y, base.transform.position.z), Time.deltaTime * moveSpeed * 0.5f * Mathf.Max(1f, Mathf.Abs(eid.target.position.y - base.transform.position.y) / 10f)));
		}
	}

	private void PickAttack()
	{
		if (IsInMaxHeat())
		{
			currentHeat = 0f;
			Stun();
			return;
		}
		currentHeat += 1.01f;
		int num = Random.Range(0, 4);
		while (previousAttacks.Contains((GeryonAttack)num))
		{
			num++;
			if (num >= 4)
			{
				num = 0;
			}
		}
		switch (num)
		{
		case 0:
			BowUp();
			break;
		case 1:
			BowForward();
			break;
		case 2:
			WaveClap();
			break;
		case 3:
			PalmProjectiles();
			break;
		}
		if (previousAttacks.Count > 1)
		{
			previousAttacks.RemoveAt(0);
		}
		previousAttacks.Add((GeryonAttack)num);
	}

	private void BowUp()
	{
		if (active)
		{
			anim.SetTrigger("BowUp");
			inAction = true;
			cooldown = 2f;
			PlaySound(bowUpSound);
		}
	}

	private void BowForward()
	{
		if (active)
		{
			anim.SetTrigger("BowForward");
			inAction = true;
			cooldown = 0f;
			PlaySound(bowForwardSound);
		}
	}

	private void WaveClap()
	{
		if (active)
		{
			anim.SetTrigger("WaveClap");
			inAction = true;
			cooldown = 0f;
			PlaySound(waveClapSound);
		}
	}

	private void PalmProjectiles()
	{
		if (active)
		{
			anim.SetTrigger("PalmProjectiles");
			inAction = true;
			cooldown = 0f;
			trackY = true;
			slowRotation = true;
			projectileRotation = 0f;
			projectileRotationDirection = ((Random.value > 0.5f) ? 1 : (-1));
			PlaySound(palmProjectilesSound);
		}
	}

	private void BowUpShoot()
	{
		if (active)
		{
			bowChargeParticle.SetActive(value: false);
			Object.Instantiate(bowUpShootParticle, bowShootPoint.position, Quaternion.LookRotation(Vector3.up)).transform.SetParent(base.transform.parent, worldPositionStays: true);
			MonoSingleton<CameraController>.Instance.CameraShake(3f);
			beamsAmount = 5;
			Invoke("BowUpSpawnBeams", 1f);
		}
	}

	private void BowUpSpawnBeams()
	{
		if (active)
		{
			beamRoutine = StartCoroutine(SpawnBeamCoroutine());
		}
	}

	private IEnumerator SpawnBeamCoroutine()
	{
		for (int i = 0; i < beamsAmount; i++)
		{
			Vector3 vector = new Vector3(rotateAround.position.x + Random.Range(0f - maximumAroundDistance, maximumAroundDistance), rotateAround.position.y, rotateAround.position.z + Random.Range(0f - maximumAroundDistance, maximumAroundDistance));
			if (Vector3.Distance(rotateAround.position, vector) > maximumAroundDistance)
			{
				vector = rotateAround.position + (vector - rotateAround.position).normalized * maximumAroundDistance;
			}
			vector.y -= 5f;
			Object.Instantiate(bowUpBeam, (i == 0) ? new Vector3(eid.target.position.x, rotateAround.position.y - 5f, eid.target.position.z) : vector, Quaternion.LookRotation(Vector3.up)).transform.SetParent(projectileParent, worldPositionStays: true);
			yield return new WaitForSecondsRealtime(0.25f);
		}
		yield return null;
	}

	private void BowForwardShoot(int shotNumber)
	{
		if (!active)
		{
			return;
		}
		Object.Instantiate(bowForwardShootParticle, bowShootPoint.position, base.transform.rotation).transform.SetParent(base.transform.parent, worldPositionStays: true);
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
		int num = shotNumber;
		if (difficulty >= 4)
		{
			num += 2;
		}
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = Object.Instantiate(bowForwardBeam, new Vector3(bowShootPoint.position.x, rotateAround.position.y - 5f, bowShootPoint.position.z), base.transform.rotation);
			if (num > 1)
			{
				gameObject.transform.Rotate(Vector3.up * Mathf.Lerp(30f, -30f, (float)i / ((float)num - 1f)), Space.World);
			}
			if (difficulty >= 4)
			{
				gameObject.transform.localScale = new Vector3(0.25f, 0.33f, 0.25f);
			}
			gameObject.transform.SetParent(projectileParent, worldPositionStays: true);
		}
		if (shotNumber == 3)
		{
			StopCharges();
		}
	}

	private void WaveClapShoot()
	{
		if (!active)
		{
			return;
		}
		clapChargeParticle.SetActive(value: false);
		int[] array = new int[3] { 0, 1, 2 };
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i];
			int num2 = Random.Range(i, array.Length);
			array[i] = array[num2];
			array[num2] = num;
		}
		float num3 = 50f;
		Vector3 position = new Vector3(clapChargeParticle.transform.position.x, rotateAround.position.y, clapChargeParticle.transform.position.z);
		for (int j = 0; j < 3; j++)
		{
			PhysicalShockwave physicalShockwave = Object.Instantiate(clapShockwave, position, Quaternion.identity);
			physicalShockwave.transform.SetParent(projectileParent, worldPositionStays: true);
			physicalShockwave.transform.localScale = new Vector3(1f, 60f, 1f);
			physicalShockwave.speed = num3 / (float)(j + 1);
			if (array[j] != 0)
			{
				physicalShockwave.transform.position += Vector3.up * ((array[j] == 1) ? 20 : (-20));
			}
		}
		Object.Instantiate(clapEffect, position, Quaternion.identity);
	}

	private void PalmProjectileCharge()
	{
		GameObject[] array = palmProjectileChargeParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	private void PalmProjectileShootBoth()
	{
		if (active)
		{
			PalmProjectileShoot(0, 0f, 0.5f, 45f);
			PalmProjectileShoot(1, 45f, 0.5f, (difficulty >= 4) ? (-45) : 45);
			GameObject[] array = palmProjectileChargeParticles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
	}

	private void PalmProjectileShoot(int hand)
	{
		if (active)
		{
			PalmProjectileShoot(hand, projectileRotation);
			projectileRotation += 15 * projectileRotationDirection;
		}
	}

	private void PalmProjectileShoot(int hand, float degrees, float speedMultiplier = 1f, float rotationSpeed = 0f)
	{
		if (active)
		{
			Vector3 position = ((hand == 0) ? leftHandShootPoint.position : rightHandShootPoint.position);
			Projectile projectile = Object.Instantiate(palmProjectile, position, base.transform.rotation);
			projectile.transform.Rotate(Vector3.forward * degrees, Space.Self);
			projectile.speed *= speedMultiplier;
			if (difficulty >= 3)
			{
				speedMultiplier *= 1.5f;
			}
			if (projectile.TryGetComponent<Spin>(out var component))
			{
				component.speed = rotationSpeed * (float)(-projectileRotationDirection);
			}
			projectile.transform.SetParent(projectileParent, worldPositionStays: true);
		}
	}

	private void BowCharge()
	{
		bowChargeParticle.SetActive(value: true);
	}

	private void WaveClapCharge()
	{
		clapChargeParticle.SetActive(value: true);
		clapChargeFollowing = true;
	}

	private void WaveClapChargeFreeze()
	{
		clapChargeFollowing = false;
	}

	private void DustHands()
	{
		if (!cancelledAction)
		{
			Object.Instantiate(dustParticle, Vector3.Lerp(rightHandShootPoint.position, leftHandShootPoint.position, 0.5f), base.transform.rotation).transform.Rotate(Vector3.up * 90f, Space.World);
		}
	}

	private void Stun()
	{
		if (active)
		{
			anim.SetTrigger("Stun");
			anim.SetBool("Stunned", true);
			inAction = true;
			slowRotation = true;
			switch (difficulty)
			{
			case 4:
			case 5:
				stunTime = 5f;
				break;
			case 3:
				stunTime = 6f;
				break;
			case 2:
				stunTime = 7f;
				break;
			case 0:
			case 1:
				stunTime = 9f;
				break;
			}
			maximumStunTime = stunTime;
			PlaySound(bigHurtSound);
			HeadHort();
			Invoke("HeadHort", 0.5f);
			Invoke("HeadHort", 1f);
		}
	}

	private void HeadHort()
	{
		GameObject obj = Object.Instantiate(fakeBloodSplatter);
		obj.transform.position = weakPointHitbox.transform.position + new Vector3(Random.value, Random.value, Random.value);
		obj.transform.SetParent(gz.transform, worldPositionStays: true);
		obj.SetActive(value: true);
		eid.hitter = "secret";
		eid.DeliverDamage(weakPointHitbox, Vector3.zero, weakPointHitbox.transform.position, 1f, tryForExplode: false);
	}

	private void HeadOpen()
	{
		eid.totalDamageTakenMultiplier = 0.66f;
		HeadHort();
		HeadHort();
		stunned = true;
		weakPointHitbox.SetActive(value: true);
		playerBlockerShield.SetActive(value: false);
	}

	private void Unstun()
	{
		if (active)
		{
			anim.SetBool("Stunned", false);
			stunned = false;
			PlaySound(recoverySound);
		}
	}

	private void UnstunClose()
	{
		weakPointHitbox.SetActive(value: false);
		Object.Instantiate(playerPushBacker, playerBlockerShield.transform.position, Quaternion.identity).transform.SetParent(projectileParent, worldPositionStays: true);
		playerPushBackerCooldown = 3f;
		eid.totalDamageTakenMultiplier = 1f;
		playerBlockerShield.SetActive(value: true);
	}

	private void SkipRecovery()
	{
		if (secondPhase)
		{
			StopAction();
			cancelledAction = true;
			sinceCancelledAction = 0f;
		}
	}

	private void EndAction()
	{
		if (!cancelledAction)
		{
			StopAction();
		}
	}

	private void StopAction()
	{
		StopCharges();
		inAction = false;
		trackY = false;
		slowRotation = false;
		RandomizeDirection();
	}

	private void StopCharges()
	{
		bowChargeParticle.SetActive(value: false);
		clapChargeParticle.SetActive(value: false);
		GameObject[] array = palmProjectileChargeParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	private void RandomizeDirection()
	{
		moveDirection = ((!(Random.Range(0f, 1f) > 0.5f)) ? 1 : (-1));
		rotateAroundDistance = Random.Range(minimumAroundDistance, maximumAroundDistance);
	}

	private void PlaySound(AudioClip clip, float pitch = 1f)
	{
		aud.SetPitch(Random.Range(pitch - 0.1f, pitch + 0.1f));
		aud.clip = clip;
		aud.Play(tracked: true);
	}

	public void Death()
	{
		CancelInvoke();
		StopAction();
		if (beamRoutine != null)
		{
			StopCoroutine(beamRoutine);
		}
		active = false;
		if ((bool)projectileParent)
		{
			Object.Destroy(projectileParent.gameObject);
		}
		anim.Play("Death", 0, 0f);
		if ((bool)currentEnrageEffect)
		{
			Object.Destroy(currentEnrageEffect);
		}
		rb.isKinematic = true;
		PlaySound(deathSound);
	}

	private bool IsInMaxHeat()
	{
		return currentHeat >= maximumHeat + (float)(secondPhase ? 1 : 0);
	}
}
