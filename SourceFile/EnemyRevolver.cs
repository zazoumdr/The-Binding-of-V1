using UnityEngine;

public class EnemyRevolver : MonoBehaviour, IEnemyWeapon
{
	private EnemyTarget target;

	public EnemyType safeEnemyType;

	public int variation;

	public GameObject bullet;

	public GameObject altBullet;

	public GameObject primaryPrepare;

	private GameObject currentpp;

	private GameObject altCharge;

	private AudioSource altChargeAud;

	private float chargeAmount;

	private bool charging;

	public Transform shootPoint;

	public GameObject muzzleFlash;

	public GameObject muzzleFlashAlt;

	private int difficulty = -1;

	private EnemyIdentifier eid;

	private float speedMultiplier = 1f;

	private float damageMultiplier = 1f;

	private void Start()
	{
		altCharge = shootPoint.GetChild(0).gameObject;
		altChargeAud = altCharge.GetComponent<AudioSource>();
		eid = GetComponentInParent<EnemyIdentifier>();
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
	}

	private void Update()
	{
		if (charging)
		{
			float num = 2f;
			if (difficulty == 1)
			{
				num = 1.5f;
			}
			if (difficulty == 0)
			{
				num = 1f;
			}
			chargeAmount = Mathf.MoveTowards(chargeAmount, 1f, Time.deltaTime * num * speedMultiplier);
			altChargeAud.SetPitch(chargeAmount / 1.75f);
			altCharge.transform.localScale = Vector3.one * chargeAmount * 10f;
		}
	}

	public void UpdateTarget(EnemyTarget target)
	{
		this.target = target;
	}

	public void Fire(bool instantExplode)
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
		Vector3 position = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position))
		{
			position = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject obj = Object.Instantiate(bullet, position, shootPoint.rotation);
		Object.Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
		if (obj.TryGetComponent<Projectile>(out var component))
		{
			component.safeEnemyType = safeEnemyType;
			component.target = target;
			if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.damage *= damageMultiplier;
			if (instantExplode)
			{
				component.Explode();
			}
		}
	}

	public void AltFire(bool instantExplode)
	{
		CancelAltCharge();
		Vector3 position = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, eid.transform.position))
		{
			position = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject obj = Object.Instantiate(altBullet, position, shootPoint.rotation);
		Object.Instantiate(muzzleFlashAlt, shootPoint.position, shootPoint.rotation);
		if (obj.TryGetComponent<Projectile>(out var component))
		{
			component.target = target;
			component.safeEnemyType = safeEnemyType;
			if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.damage *= damageMultiplier;
			if (instantExplode)
			{
				component.Explode();
			}
		}
	}

	public void PrepareFire()
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
		currentpp = Object.Instantiate(primaryPrepare, shootPoint);
		currentpp.transform.Rotate(Vector3.up * 90f);
	}

	public void PrepareAltFire()
	{
		if ((bool)altCharge)
		{
			charging = true;
			altCharge.SetActive(value: true);
		}
	}

	public void CancelAltCharge()
	{
		if ((bool)(Object)(object)altChargeAud)
		{
			charging = false;
			chargeAmount = 0f;
			altChargeAud.SetPitch(0f);
			altCharge.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (currentpp != null)
		{
			Object.Destroy(currentpp);
		}
	}

	private void UpdateBuffs(EnemyIdentifier eid)
	{
		speedMultiplier = eid.totalSpeedModifier;
		damageMultiplier = eid.totalDamageModifier;
	}
}
