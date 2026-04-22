using UnityEngine;

public class GabrielSecond : EnemyScript, IHitTargetCallback
{
	[HideInInspector]
	public GabrielBase gabe;

	private bool valuesSet;

	[Header("Swords")]
	public Transform rightHand;

	public Transform leftHand;

	private TrailRenderer rightHandTrail;

	private TrailRenderer leftHandTrail;

	[SerializeField]
	private SwingCheck2 generalSwingCheck;

	private SwingCheck2 rightSwingCheck;

	private SwingCheck2 leftSwingCheck;

	private MeshRenderer rightHandGlow;

	private MeshRenderer leftHandGlow;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private AudioSource kickSwingSound;

	[SerializeField]
	private Renderer[] swordRenderers;

	[SerializeField]
	private GameObject fakeCombinedSwords;

	[SerializeField]
	private Projectile combinedSwordsThrown;

	[HideInInspector]
	public bool swordsCombined;

	[HideInInspector]
	public bool lightSwords;

	[Space(20f)]
	public TrailRenderer kickTrail;

	private float[] moveChanceBonuses = new float[4];

	private int previousMove = -1;

	public bool ceilingHitChallenge;

	[SerializeField]
	private GameObject ceilingHitEffect;

	private float ceilingHitCooldown;

	private EnemyTarget target => gabe.eid.target;

	private EnemyIdentifier eid => gabe.eid;

	private Animator anim => gabe.anim;

	private void Awake()
	{
		SetValues();
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			gabe = GetComponent<GabrielBase>();
			rightHandTrail = rightHand.GetComponentInChildren<TrailRenderer>();
			rightSwingCheck = rightHand.GetComponentInChildren<SwingCheck2>();
			rightHandGlow = rightHand.GetComponentInChildren<MeshRenderer>(includeInactive: true);
			leftHandTrail = leftHand.GetComponentInChildren<TrailRenderer>();
			leftSwingCheck = leftHand.GetComponentInChildren<SwingCheck2>();
			leftHandGlow = leftHand.GetComponentInChildren<MeshRenderer>(includeInactive: true);
		}
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	public void ChooseAttack()
	{
		bool flag = Vector3.Distance(base.transform.position, target.position) < 5f;
		bool flag2 = Vector3.Distance(base.transform.position, target.position) > 20f;
		float[] array = new float[4];
		int num = -1;
		bool flag3 = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (previousMove != i)
			{
				switch (i)
				{
				case 0:
					flag3 = !flag;
					break;
				case 1:
					flag3 = !flag;
					break;
				case 2:
					flag3 = !flag2;
					break;
				case 3:
					flag3 = !flag2;
					break;
				}
				if (flag3)
				{
					array[i] = Random.Range(0f, 1f) + moveChanceBonuses[i];
				}
			}
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
			CombineSwords();
			break;
		case 1:
			FastComboDash();
			break;
		case 2:
			BasicCombo();
			break;
		case 3:
			ThrowCombo();
			break;
		}
		gabe.ResetAnimSpeed();
		previousMove = num;
		for (int k = 0; k < array.Length; k++)
		{
			moveChanceBonuses[k] = ((k == num) ? 0f : (moveChanceBonuses[k] + 0.25f));
		}
		if (num != 0)
		{
			if (gabe.burstLength > 1)
			{
				gabe.burstLength--;
				return;
			}
			gabe.burstLength = ((gabe.difficulty >= 3) ? 3 : 2);
			gabe.attackCooldown = ((gabe.difficulty <= 3) ? 3 : (5 - gabe.difficulty));
			gabe.readyTaunt = true;
		}
	}

	private void BasicCombo()
	{
		if (!gabe.juggled && target != null)
		{
			CheckIfSwordsCombined();
			gabe.forwardSpeedMinimum = 125f;
			gabe.forwardSpeedMaximum = 175f;
			gabe.inAction = true;
			anim.Play("BasicCombo");
		}
	}

	private void FastComboDash()
	{
		if (!gabe.juggled && target != null)
		{
			CheckIfSwordsCombined();
			gabe.forwardSpeed = ((gabe.difficulty >= 2) ? 100 : 40);
			gabe.forwardSpeed *= eid.totalSpeedModifier;
			gabe.inAction = true;
			anim.Play("FastComboDash");
		}
	}

	public void FastCombo()
	{
		if (!gabe.juggled && target != null)
		{
			gabe.forwardSpeedMinimum = 75f;
			gabe.forwardSpeedMaximum = 125f;
			gabe.inAction = true;
			anim.Play("FastCombo");
			gabe.LookAtTarget();
		}
	}

	private void ThrowCombo()
	{
		if (!gabe.juggled && target != null)
		{
			CheckIfSwordsCombined();
			gabe.forwardSpeedMinimum = 125f;
			gabe.forwardSpeedMaximum = 175f;
			gabe.inAction = true;
			anim.Play("ThrowCombo");
			gabe.LookAtTarget();
		}
	}

	private void CombineSwords()
	{
		if (!gabe.juggled && target != null)
		{
			if (swordsCombined)
			{
				UnGattai();
			}
			gabe.inAction = true;
			anim.Play("SwordsCombine");
		}
	}

	private void Gattai()
	{
		if (swordsCombined)
		{
			UnGattai();
		}
		swordsCombined = true;
		Renderer[] array = swordRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		fakeCombinedSwords.SetActive(value: true);
	}

	private void CombinedSwordAttack()
	{
		if (!gabe.juggled)
		{
			anim.Play("SwordsCombinedThrow");
		}
	}

	public void UnGattai(bool destroySwords = true)
	{
		swordsCombined = false;
		Renderer[] array = swordRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		fakeCombinedSwords.SetActive(value: false);
		if (destroySwords && (bool)gabe.currentCombinedSwordsThrown)
		{
			Object.Destroy(gabe.currentCombinedSwordsThrown.gameObject);
		}
		if (lightSwords)
		{
			lightSwords = false;
			if (!leftSwingCheck.damaging)
			{
				leftHandGlow.enabled = false;
			}
			if (!rightSwingCheck.damaging)
			{
				rightHandGlow.enabled = false;
			}
		}
	}

	private void CheckIfSwordsCombined()
	{
		if (swordsCombined)
		{
			if (gabe.secondPhase || gabe.currentCombinedSwordsThrown.friendly)
			{
				CreateLightSwords();
			}
			else
			{
				UnGattai();
			}
		}
	}

	private void CreateLightSwords()
	{
		lightSwords = true;
		leftHandGlow.enabled = true;
		rightHandGlow.enabled = true;
	}

	private void ThrowSwords()
	{
		if (!gabe.juggled)
		{
			Object.Instantiate<AudioSource>(kickSwingSound, base.transform);
			fakeCombinedSwords.SetActive(value: false);
			gabe.currentCombinedSwordsThrown = Object.Instantiate(combinedSwordsThrown, fakeCombinedSwords.transform.position, base.transform.rotation, base.transform.parent);
			gabe.currentCombinedSwordsThrown.target = target;
			gabe.combinedSwordsCooldown = ((gabe.difficulty > 2) ? 1 : 2);
			if (gabe.difficulty >= 4)
			{
				gabe.currentCombinedSwordsThrown.speed *= 1.75f;
			}
			gabe.currentCombinedSwordsThrown.damage *= eid.totalDamageModifier;
			if (gabe.currentCombinedSwordsThrown.TryGetComponent<GabrielCombinedSwordsThrown>(out var component))
			{
				component.gabe = this;
			}
		}
	}

	public void DamageStartLeft(int damage)
	{
		if (!gabe.juggled)
		{
			leftHandTrail.emitting = true;
			leftHandGlow.gameObject.SetActive(value: true);
			SetDamage(damage);
			leftSwingCheck.DamageStart();
			generalSwingCheck.DamageStart();
			Object.Instantiate<AudioSource>(swingSound, base.transform);
			gabe.DecideMovementSpeed();
			gabe.goForward = true;
		}
	}

	public void DamageStopLeft(int keepMoving)
	{
		leftHandTrail.emitting = false;
		leftSwingCheck.DamageStop();
		if (!lightSwords)
		{
			leftHandGlow.gameObject.SetActive(value: false);
		}
		DamageStopped(keepMoving);
	}

	public void DamageStartRight(int damage)
	{
		if (!gabe.juggled)
		{
			rightHandTrail.emitting = true;
			rightHandGlow.gameObject.SetActive(value: true);
			SetDamage(damage);
			rightSwingCheck.DamageStart();
			generalSwingCheck.DamageStart();
			Object.Instantiate<AudioSource>(swingSound, base.transform);
			gabe.DecideMovementSpeed();
			gabe.goForward = true;
		}
	}

	public void DamageStopRight(int keepMoving)
	{
		rightHandTrail.emitting = false;
		rightSwingCheck.DamageStop();
		if (!lightSwords)
		{
			rightHandGlow.gameObject.SetActive(value: false);
		}
		DamageStopped(keepMoving);
	}

	public void DamageStartKick(int damage)
	{
		if (!gabe.juggled)
		{
			kickTrail.emitting = true;
			SetDamage(damage);
			generalSwingCheck.DamageStart();
			Object.Instantiate<AudioSource>(kickSwingSound, base.transform);
			gabe.DecideMovementSpeed();
			gabe.goForward = true;
		}
	}

	public void DamageStopKick(int keepMoving)
	{
		if ((bool)kickTrail)
		{
			kickTrail.emitting = false;
		}
		DamageStopped(keepMoving);
	}

	private void DamageStopped(int keepMoving)
	{
		if (keepMoving == 0)
		{
			gabe.goForward = false;
		}
		gabe.Unparryable();
		if ((!leftSwingCheck || !leftSwingCheck.damaging) && (!rightSwingCheck || !rightSwingCheck.damaging))
		{
			generalSwingCheck.DamageStop();
		}
	}

	public void DamageStartBoth(int damage)
	{
		DamageStartLeft(damage);
		DamageStartRight(damage);
	}

	public void DamageStopBoth(int keepMoving)
	{
		DamageStopLeft(keepMoving);
		DamageStopRight(keepMoving);
		DamageStopKick(keepMoving);
	}

	private void SetDamage(int damage)
	{
		leftSwingCheck.damage = damage;
		rightSwingCheck.damage = damage;
		generalSwingCheck.damage = damage;
	}

	public void TargetBeenHit()
	{
		leftSwingCheck.DamageStop();
		rightSwingCheck.DamageStop();
		generalSwingCheck.DamageStop();
		gabe.goForward = false;
	}

	public void CeilingCheck(Rigidbody rb, Enemy mach, GabrielVoice voice)
	{
		RaycastHit hitInfo;
		if (ceilingHitCooldown > 0f)
		{
			ceilingHitCooldown = Mathf.MoveTowards(ceilingHitCooldown, 0f, Time.fixedDeltaTime);
		}
		else if (rb.velocity.y > 1f && Physics.Raycast(base.transform.position, Vector3.up, out hitInfo, 3f + rb.velocity.y * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment)))
		{
			ceilingHitCooldown = 0.5f;
			base.transform.position = hitInfo.point - Vector3.up * 3f;
			mach.GetHurt(base.gameObject, Vector3.zero, Mathf.Min(rb.velocity.y, 5f), 0f);
			rb.velocity = new Vector3(0f, 0f - rb.velocity.y, 0f);
			anim.Play("Juggle", 0, 0f);
			gabe.juggleHp = mach.health;
			voice.Hurt();
			Object.Instantiate(ceilingHitEffect, hitInfo.point - Vector3.up, Quaternion.LookRotation(Vector3.down));
			MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
			if (ceilingHitChallenge)
			{
				MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
			}
		}
	}
}
