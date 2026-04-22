using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlayerAnimations : MonoSingleton<PlayerAnimations>
{
	private Animator anim;

	private NewMovement nm;

	private GroundCheckGroup gc;

	private float directionLerp;

	[SerializeField]
	private Transform chest;

	[SerializeField]
	private Transform head;

	[SerializeField]
	private Transform rightArm;

	[SerializeField]
	private Transform leftArm;

	private float aimerWeight;

	private float aimerWeightSpeed;

	private float leftArmWeight;

	[SerializeField]
	private GameObject[] weapons;

	[SerializeField]
	private GameObject[] altweapons;

	private bool bigWeapon;

	private void Start()
	{
		anim = GetComponent<Animator>();
		nm = MonoSingleton<NewMovement>.Instance;
		gc = nm.gc;
		MonoSingleton<GunControl>.Instance.OnWeaponChange += UpdateWeapon;
		UpdateWeapon();
	}

	private void LateUpdate()
	{
		bool flag = nm.boost && !nm.sliding;
		bool sliding = nm.sliding;
		anim.SetBool("Dodging", flag);
		anim.SetBool("InAir", !gc.onGround);
		anim.SetBool("Sliding", sliding);
		float layerWeight = anim.GetLayerWeight(1);
		if (layerWeight > 0f)
		{
			anim.SetLayerWeight(1, Mathf.MoveTowards(layerWeight, 0f, Time.deltaTime * 3f));
		}
		if (leftArmWeight > 0f)
		{
			leftArmWeight = Mathf.MoveTowards(leftArmWeight, 0f, Time.deltaTime * 3f);
			anim.SetLayerWeight(3, leftArmWeight);
		}
		directionLerp = Mathf.MoveTowards(directionLerp, sliding ? 1 : 0, Time.deltaTime * 10f);
		if (directionLerp != 0f)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.parent.rotation, Quaternion.LookRotation(nm.rb.velocity.normalized), directionLerp);
		}
		Vector3 vector = nm.transform.InverseTransformVector(nm.rb.velocity / 15f);
		anim.SetFloat("VelX", flag ? MonoSingleton<NewMovement>.Instance.dodgeDirection.normalized.x : vector.x);
		anim.SetFloat("VelY", flag ? MonoSingleton<NewMovement>.Instance.dodgeDirection.normalized.z : vector.z);
		Quaternion rotation = chest.rotation;
		Quaternion rotation2 = head.rotation;
		if (directionLerp > 0f)
		{
			Quaternion rotation3 = leftArm.rotation;
			chest.localRotation *= MonoSingleton<CameraController>.Instance.transform.localRotation;
			chest.rotation = Quaternion.RotateTowards(chest.transform.rotation, Quaternion.Inverse(base.transform.localRotation), 45f);
			leftArm.rotation = rotation3;
		}
		else
		{
			chest.localRotation *= MonoSingleton<CameraController>.Instance.transform.localRotation;
		}
		head.rotation = MonoSingleton<CameraController>.Instance.transform.rotation;
		if (aimerWeight < 1f)
		{
			aimerWeight = Mathf.MoveTowards(aimerWeight, 1f, Time.deltaTime * (5f * aimerWeightSpeed));
		}
		if (!bigWeapon)
		{
			AimRightArm(rightArm);
		}
		AimRightArm(rightArm.GetChild(0));
		if (leftArmWeight > 0f)
		{
			chest.rotation = Quaternion.Lerp(chest.rotation, rotation, leftArmWeight);
			head.rotation = Quaternion.Lerp(head.rotation, rotation2, leftArmWeight);
		}
	}

	private void AimRightArm(Transform aimer)
	{
		Quaternion rotation = aimer.rotation;
		aimer.rotation = MonoSingleton<CameraController>.Instance.transform.rotation;
		aimer.Rotate(Vector3.right, 90f, Space.Self);
		aimer.Rotate(Vector3.up, 180f, Space.Self);
		if (aimerWeight < 1f)
		{
			aimer.rotation = Quaternion.Lerp(rotation, aimer.rotation, aimerWeight);
		}
	}

	public void Jump()
	{
		anim.SetTrigger("Jump");
	}

	public void Land(float force)
	{
		anim.SetLayerWeight(1, force);
	}

	public void Shoot(float speed = 1f)
	{
		anim.SetTrigger("Shoot");
		anim.SetFloat("ShootSpeed", speed);
		aimerWeight = 0f;
		aimerWeightSpeed = speed;
	}

	public void Punch(float speed = 1f)
	{
		anim.SetTrigger("Punch");
		anim.SetFloat("PunchSpeed", speed);
		leftArmWeight = 2f / speed;
	}

	public void CoinToss()
	{
		anim.SetTrigger("CoinToss");
		leftArmWeight = 1f;
	}

	public void UpdateWeapon(GameObject _)
	{
		UpdateWeapon();
		anim.SetTrigger("ChangeWeapon");
		aimerWeight = 0f;
	}

	public void UpdateWeapon()
	{
		if (MonoSingleton<GunControl>.Instance == null || MonoSingleton<GunControl>.Instance.currentWeapon == null)
		{
			return;
		}
		int num = MonoSingleton<GunControl>.Instance.currentSlotIndex - 1;
		WeaponIdentifier component;
		bool flag = MonoSingleton<GunControl>.Instance.currentWeapon.TryGetComponent<WeaponIdentifier>(out component) && component.alternateVersion;
		for (int i = 0; i < weapons.Length; i++)
		{
			weapons[i].SetActive(i == num && !flag);
			if (i < altweapons.Length)
			{
				altweapons[i].SetActive(i == num && flag);
			}
		}
		bigWeapon = num >= 2;
		anim.SetBool("BigWeapon", bigWeapon);
	}
}
