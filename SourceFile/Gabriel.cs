using UnityEngine;

public class Gabriel : EnemyScript
{
	[HideInInspector]
	public GabrielBase gabe;

	private bool valuesSet;

	public Transform rightHand;

	public Transform leftHand;

	private GameObject rightHandWeapon;

	private GameObject leftHandWeapon;

	private WeaponTrail rightHandTrail;

	private WeaponTrail leftHandTrail;

	private SwingCheck2 rightSwingCheck;

	private SwingCheck2 leftSwingCheck;

	public GameObject sword;

	public GameObject zweiHander;

	public GameObject axe;

	public GameObject spear;

	public GameObject glaive;

	private int spearAttacks;

	private int throws;

	private GameObject thrownObject;

	private bool threwAxes;

	private float[] moveChanceBonuses = new float[4];

	private int previousMove = -1;

	private EnemyIdentifier eid => gabe.eid;

	private EnemyTarget target => gabe.eid.target;

	private int difficulty => gabe.difficulty;

	private Animator anim => gabe.anim;

	private void Start()
	{
		gabe = GetComponent<GabrielBase>();
	}

	private void OnDisable()
	{
		if ((bool)rightHandWeapon || (bool)leftHandWeapon)
		{
			DisableWeapon();
		}
		spearAttacks = 0;
	}

	public void ChooseAttack()
	{
		bool flag = Vector3.Distance(base.transform.position, target.headPosition) < 5f;
		bool flag2 = Vector3.Distance(base.transform.position, target.headPosition) > 10f;
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
					flag3 = !flag && !threwAxes;
					break;
				case 1:
					flag3 = !flag;
					break;
				case 2:
					flag3 = !flag2;
					break;
				case 3:
					flag3 = true;
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
			AxeThrow();
			break;
		case 1:
			SpearCombo();
			break;
		case 2:
			StingerCombo();
			break;
		case 3:
			ZweiDash();
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
			gabe.burstLength = ((difficulty >= 3) ? 3 : 2);
			gabe.attackCooldown = ((difficulty <= 3) ? 3 : (5 - difficulty));
			threwAxes = false;
			gabe.readyTaunt = true;
		}
	}

	private void StingerCombo()
	{
		gabe.forwardSpeed = 100f * anim.speed;
		SpawnLeftHandWeapon(GabrielWeaponType.Sword);
		gabe.inAction = true;
		anim.Play("StingerCombo");
	}

	private void SpearCombo()
	{
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			gabe.forwardSpeed = 150f;
			break;
		case 1:
			gabe.forwardSpeed = 75f;
			break;
		case 0:
			gabe.forwardSpeed = 60f;
			break;
		}
		gabe.forwardSpeed *= eid.totalSpeedModifier;
		spearAttacks = 1;
		if (gabe.enraged)
		{
			spearAttacks++;
		}
		if (gabe.secondPhase)
		{
			spearAttacks++;
		}
		SpawnRightHandWeapon(GabrielWeaponType.Spear);
		gabe.inAction = true;
		anim.Play("SpearReady");
	}

	private void ZweiDash()
	{
		gabe.forwardSpeed = ((difficulty >= 2) ? 100 : 40);
		gabe.forwardSpeed *= eid.totalSpeedModifier;
		anim.Play("ZweiDash");
		gabe.inAction = true;
		SpawnRightHandWeapon(GabrielWeaponType.Zweihander);
	}

	public void ZweiCombo()
	{
		gabe.forwardSpeed = 65f * anim.speed;
		gabe.inAction = true;
		anim.Play("ZweiCombo");
		gabe.LookAtTarget();
		if (gabe.secondPhase || gabe.enraged)
		{
			throws = 1;
		}
	}

	private void AxeThrow()
	{
		threwAxes = true;
		gabe.inAction = true;
		SpawnRightHandWeapon(GabrielWeaponType.Axe);
		SpawnLeftHandWeapon(GabrielWeaponType.Axe);
		anim.Play("AxeThrow");
	}

	private void SpearAttack()
	{
		if (gabe.juggled)
		{
			return;
		}
		if (target == null)
		{
			spearAttacks = 0;
		}
		if (spearAttacks == 0)
		{
			SpearThrow();
			return;
		}
		gabe.spearing = true;
		gabe.goForward = false;
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
		num = 0.75f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			num = 0.5f;
			break;
		case 2:
			num = 0.75f;
			break;
		case 0:
		case 1:
			num = 1f;
			break;
		}
		Vector3 position = target.headPosition;
		bool flag = false;
		if (!Physics.Raycast(target.headPosition, Vector3.up, out var hitInfo, 17f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			position = target.headPosition + Vector3.up * 15f;
			flag = true;
		}
		else if (!Physics.Raycast(target.headPosition, Vector3.down, out hitInfo, 17f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			position = base.transform.position + Vector3.down * 15f;
			flag = true;
		}
		if (!flag || (difficulty >= 4 && gabe.enraged && Random.Range(0f, 1f) > 0.5f))
		{
			anim.Play("SpearStinger");
			gabe.Teleport(closeRange: false, longrange: true, firstTime: true, horizontal: true);
			gabe.FollowTarget();
			Invoke("SpearFlash", num / 2f / eid.totalSpeedModifier);
			Invoke("SpearGoHorizontal", num / eid.totalSpeedModifier);
			return;
		}
		gabe.TeleportTo(position);
		gabe.LookAtTarget();
		Animator obj = anim;
		if (obj != null)
		{
			obj.Play("SpearDown");
		}
		Invoke("SpearFlash", num / 2f / eid.totalSpeedModifier);
		Invoke("SpearGo", num / eid.totalSpeedModifier);
	}

	private void SpearFlash()
	{
		if (!gabe.juggled)
		{
			gabe.AttackFlash(1);
		}
	}

	private void SpearGoHorizontal()
	{
		if (!gabe.juggled)
		{
			gabe.LookAtTarget();
			SpearGo();
		}
	}

	private void SpearGo()
	{
		if (!gabe.juggled)
		{
			Object.Instantiate(gabe.dashEffect, base.transform.position, base.transform.rotation);
			DamageStartRight(25);
		}
	}

	private void SpearThrow()
	{
		if (!gabe.juggled)
		{
			gabe.spearing = false;
			DamageStopRight(0);
			gabe.Teleport();
			gabe.FollowTarget();
			anim.Play("SpearThrow");
		}
	}

	private void ThrowWeapon(GameObject projectile)
	{
		if (gabe.juggled)
		{
			return;
		}
		if ((bool)rightHandWeapon)
		{
			DestroyWeapon(rightHandWeapon, rightHandTrail, rightSwingCheck);
		}
		if ((bool)leftHandWeapon)
		{
			DestroyWeapon(leftHandWeapon, leftHandTrail, leftSwingCheck);
		}
		if (throws > 0)
		{
			throws--;
			Invoke("CheckForThrown", 0.35f / eid.totalSpeedModifier);
		}
		thrownObject = Object.Instantiate(projectile, base.transform.position + base.transform.forward * 3f, base.transform.rotation);
		Projectile componentInChildren = thrownObject.GetComponentInChildren<Projectile>();
		if ((bool)componentInChildren)
		{
			componentInChildren.target = target;
			componentInChildren.damage *= eid.totalDamageModifier;
			if (difficulty <= 1)
			{
				componentInChildren.speed *= 0.5f;
			}
		}
	}

	private void DestroyWeapon(GameObject weapon, WeaponTrail trail, SwingCheck2 swingCheck)
	{
		if (!(weapon == null))
		{
			weapon.SetActive(value: false);
			if ((bool)trail)
			{
				trail.RemoveTrail();
			}
			if ((bool)swingCheck)
			{
				Object.Destroy(swingCheck.gameObject);
			}
			Object.Destroy(weapon);
		}
	}

	private void CheckForThrown()
	{
		if (gabe.juggled)
		{
			return;
		}
		if (thrownObject == null)
		{
			throws = 0;
			return;
		}
		Vector3 position = thrownObject.transform.position;
		Collider[] array = Physics.OverlapCapsule(position + base.transform.up * -2.25f, position + base.transform.up * 1.25f, 1.25f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore);
		if (array != null && array.Length != 0)
		{
			throws = 0;
			return;
		}
		gabe.TeleportTo(position);
		thrownObject.gameObject.SetActive(value: false);
		Object.Destroy(thrownObject);
		base.transform.LookAt(target.headPosition);
		SpearFlash();
		anim.speed = 0f;
		Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
		anim.Play("ZweiCombo", -1, 0.5f);
	}

	public void EnableWeapon()
	{
		if (!gabe.juggled)
		{
			if ((bool)rightHandWeapon)
			{
				rightHandWeapon.SetActive(value: true);
			}
			if ((bool)leftHandWeapon)
			{
				leftHandWeapon.SetActive(value: true);
			}
		}
	}

	public void DisableWeapon()
	{
		if (!gabe.juggled)
		{
			if ((bool)rightHandWeapon)
			{
				DestroyWeapon(rightHandWeapon, rightHandTrail, rightSwingCheck);
			}
			if ((bool)leftHandWeapon)
			{
				DestroyWeapon(leftHandWeapon, leftHandTrail, leftSwingCheck);
			}
		}
	}

	private void SpawnLeftHandWeapon(GabrielWeaponType weapon)
	{
		if (!gabe.juggled)
		{
			GameObject weaponGameObject = GetWeaponGameObject(weapon);
			if (!(weaponGameObject == null))
			{
				leftHandWeapon = Object.Instantiate(weaponGameObject, leftHand.position, leftHand.rotation);
				leftHandWeapon.transform.forward = leftHand.transform.up;
				leftHandWeapon.transform.SetParent(leftHand, worldPositionStays: true);
				leftHandTrail = leftHandWeapon.GetComponentInChildren<WeaponTrail>();
				leftHandWeapon.SetActive(value: false);
				leftSwingCheck = WeaponHitBox(weapon);
			}
		}
	}

	private void SpawnRightHandWeapon(GabrielWeaponType weapon)
	{
		if (!gabe.juggled)
		{
			GameObject weaponGameObject = GetWeaponGameObject(weapon);
			if (!(weaponGameObject == null))
			{
				rightHandWeapon = Object.Instantiate(weaponGameObject, rightHand.position, rightHand.rotation);
				rightHandWeapon.transform.forward = rightHand.transform.up;
				rightHandWeapon.transform.SetParent(rightHand, worldPositionStays: true);
				rightHandTrail = rightHandWeapon.GetComponentInChildren<WeaponTrail>();
				rightHandWeapon.SetActive(value: false);
				rightSwingCheck = WeaponHitBox(weapon);
			}
		}
	}

	private GameObject GetWeaponGameObject(GabrielWeaponType weapon)
	{
		return weapon switch
		{
			GabrielWeaponType.Sword => sword, 
			GabrielWeaponType.Zweihander => zweiHander, 
			GabrielWeaponType.Axe => axe, 
			GabrielWeaponType.Spear => spear, 
			GabrielWeaponType.Glaive => glaive, 
			_ => null, 
		};
	}

	private SwingCheck2 WeaponHitBox(GabrielWeaponType weapon)
	{
		return weapon switch
		{
			GabrielWeaponType.Sword => CreateHitBox(new Vector3(0f, 0f, 1.5f), new Vector3(4f, 5f, 3f)), 
			GabrielWeaponType.Zweihander => CreateHitBox(new Vector3(0f, 0f, 2.5f), new Vector3(8f, 5f, 5f)), 
			GabrielWeaponType.Spear => CreateHitBox(new Vector3(0f, 0f, 2.5f), new Vector3(3.5f, 3.5f, 5f), ignoreSlide: true), 
			_ => null, 
		};
	}

	private SwingCheck2 CreateHitBox(Vector3 position, Vector3 size, bool ignoreSlide = false)
	{
		GameObject obj = new GameObject();
		obj.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		obj.transform.SetParent(base.transform, worldPositionStays: true);
		BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
		boxCollider.enabled = false;
		boxCollider.isTrigger = true;
		boxCollider.center = position;
		boxCollider.size = size;
		SwingCheck2 swingCheck = obj.AddComponent<SwingCheck2>();
		swingCheck.type = EnemyType.Gabriel;
		swingCheck.ignoreSlidingPlayer = ignoreSlide;
		swingCheck.OverrideEnemyIdentifier(eid);
		return swingCheck;
	}

	public void DamageStartLeft(int damage)
	{
		if (!gabe.juggled)
		{
			leftHandTrail.AddTrail();
			leftSwingCheck.damage = damage;
			leftSwingCheck.DamageStart();
			gabe.goForward = true;
		}
	}

	public void DamageStopLeft(int keepMoving)
	{
		if ((bool)leftHandTrail)
		{
			leftHandTrail.RemoveTrail();
		}
		if ((bool)leftSwingCheck)
		{
			leftSwingCheck.DamageStop();
		}
		if ((bool)gabe && keepMoving == 0)
		{
			gabe.goForward = false;
		}
	}

	public void DamageStartRight(int damage)
	{
		if (!gabe.juggled)
		{
			rightHandTrail.AddTrail();
			rightSwingCheck.damage = damage;
			rightSwingCheck.DamageStart();
			gabe.goForward = true;
		}
	}

	public void DamageStopRight(int keepMoving)
	{
		if ((bool)rightHandTrail)
		{
			rightHandTrail.RemoveTrail();
		}
		if ((bool)rightSwingCheck)
		{
			rightSwingCheck.DamageStop();
		}
		if ((bool)gabe && keepMoving == 0)
		{
			gabe.goForward = false;
		}
	}

	private void ResetAnimSpeed()
	{
		gabe.ResetAnimSpeed();
	}
}
