using UnityEngine;

public class DroneFlesh : MonoBehaviour
{
	public GameObject beam;

	public GameObject warningBeam;

	public GameObject chargeEffect;

	private GameObject currentWarningBeam;

	private GameObject currentChargeEffect;

	private AudioSource ceAud;

	private Light ceLight;

	private float cooldown = 3f;

	private bool inAction;

	private Drone drn;

	private EnemyIdentifier eid;

	private bool tracking;

	public Transform shootPoint;

	public float predictionAmount;

	public Vector3 rotationOffset;

	private int difficulty = -1;

	private float difficultySpeedModifier = 1f;

	private void Awake()
	{
		eid = GetComponentInParent<EnemyIdentifier>();
		drn = GetComponent<Drone>();
	}

	private void Start()
	{
		cooldown = Random.Range(2f, 3f);
		if ((bool)drn)
		{
			drn.fleshDrone = true;
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (difficulty == 1)
		{
			difficultySpeedModifier = 0.8f;
		}
		else if (difficulty == 0)
		{
			difficultySpeedModifier = 0.6f;
		}
	}

	private void Update()
	{
		if ((bool)eid && eid.enemyType == EnemyType.Virtue)
		{
			return;
		}
		if ((bool)drn && drn.crashing)
		{
			drn.Explode();
		}
		else
		{
			if (eid.target == null)
			{
				return;
			}
			if (tracking)
			{
				base.transform.LookAt(eid.target.position);
				if (rotationOffset != Vector3.zero)
				{
					base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles + rotationOffset);
				}
			}
			if (((bool)drn && !drn.targetSpotted) || inAction)
			{
				return;
			}
			if (cooldown > 0f)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				if (cooldown <= 1f && (bool)chargeEffect)
				{
					if (!currentChargeEffect)
					{
						currentChargeEffect = Object.Instantiate(chargeEffect, shootPoint ? shootPoint.position : (base.transform.position + base.transform.forward * 1.5f), shootPoint ? shootPoint.rotation : base.transform.rotation);
						currentChargeEffect.transform.SetParent(base.transform);
						currentChargeEffect.transform.localScale = Vector3.zero;
						ceAud = currentChargeEffect.GetComponent<AudioSource>();
						ceLight = currentChargeEffect.GetComponent<Light>();
					}
					currentChargeEffect.transform.localScale = Vector3.one * (1f - cooldown) * 2.5f;
					if ((bool)(Object)(object)ceAud)
					{
						ceAud.SetPitch((1f - cooldown) * 2f);
					}
					if ((bool)ceLight)
					{
						ceLight.intensity = (1f - cooldown) * 30f;
					}
				}
			}
			else
			{
				inAction = true;
				cooldown = Random.Range(1f, 3f);
				if (difficulty > 2)
				{
					cooldown *= 0.75f;
				}
				if (difficulty == 1)
				{
					cooldown *= 1.5f;
				}
				else if (difficulty == 0)
				{
					cooldown *= 2f;
				}
				PrepareBeam();
			}
		}
	}

	private void PrepareBeam()
	{
		if ((bool)drn)
		{
			drn.lockPosition = true;
			drn.lockRotation = true;
		}
		base.transform.LookAt(eid.target.PredictTargetPosition(0.5f / eid.totalSpeedModifier * predictionAmount));
		if (rotationOffset != Vector3.zero)
		{
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles + rotationOffset);
		}
		currentWarningBeam = Object.Instantiate(warningBeam, shootPoint ? shootPoint : base.transform);
		if (!shootPoint)
		{
			currentWarningBeam.transform.position += base.transform.forward * 1.5f;
		}
		float num = 0.5f;
		if (difficulty == 1)
		{
			num = 1f;
		}
		if (difficulty == 0)
		{
			num = 1.5f;
		}
		Invoke("ShootBeam", num / eid.totalSpeedModifier);
	}

	private void StopTracking()
	{
		tracking = false;
	}

	private void ShootBeam()
	{
		if ((bool)currentWarningBeam)
		{
			Object.Destroy(currentWarningBeam);
		}
		if ((bool)currentChargeEffect)
		{
			Object.Destroy(currentChargeEffect);
		}
		GameObject gameObject = Object.Instantiate(beam, shootPoint ? shootPoint.position : base.transform.position, shootPoint ? shootPoint.rotation : base.transform.rotation);
		Grenade component2;
		if (eid.totalDamageModifier != 1f && gameObject.TryGetComponent<RevolverBeam>(out var component))
		{
			component.damage *= eid.totalDamageModifier;
		}
		else if (gameObject.TryGetComponent<Grenade>(out component2))
		{
			if (eid.totalDamageModifier != 1f)
			{
				component2.totalDamageMultiplier = eid.totalDamageModifier;
			}
			component2.originEnemy = eid;
			component2.rocketSpeed *= difficultySpeedModifier;
		}
		if ((bool)drn)
		{
			drn.lockPosition = false;
			drn.lockRotation = false;
		}
		inAction = false;
	}

	public void Explode()
	{
		drn?.Explode();
	}
}
