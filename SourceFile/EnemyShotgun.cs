using System;
using ULTRAKILL.Enemy;
using UnityEngine;

public class EnemyShotgun : MonoBehaviour, IEnemyWeapon
{
	[Obsolete]
	private EnemyTarget target;

	private TargetData? targetData;

	public EnemyType safeEnemyType;

	private AudioSource gunAud;

	public AudioClip shootSound;

	public AudioClip clickSound;

	public AudioClip smackSound;

	private AudioSource heatSinkAud;

	public int variation;

	public GameObject bullet;

	public GameObject grenade;

	public float spread;

	private Animator anim;

	public bool gunReady = true;

	public Transform shootPoint;

	public GameObject muzzleFlash;

	private ParticleSystem[] parts;

	private bool charging;

	private AudioSource chargeSound;

	private float chargeAmount;

	public GameObject warningFlash;

	private int difficulty = -1;

	private EnemyIdentifier eid;

	private float speedMultiplier = 1f;

	private float damageMultiplier = 1f;

	private void Start()
	{
		gunAud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		parts = GetComponentsInChildren<ParticleSystem>();
		heatSinkAud = shootPoint.GetComponent<AudioSource>();
		chargeSound = base.transform.GetChild(0).GetComponent<AudioSource>();
		eid = GetComponentInParent<EnemyIdentifier>();
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (difficulty == 1)
		{
			spread *= 0.75f;
		}
		else if (difficulty == 0)
		{
			spread *= 0.5f;
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
			chargeSound.SetPitch(chargeAmount * 1.25f);
		}
	}

	[Obsolete]
	public void UpdateTarget(EnemyTarget target)
	{
		this.target = target;
	}

	public void UpdateTarget(TargetData? target)
	{
		targetData = target;
	}

	public void Fire(bool instantExplode = false)
	{
		if (!targetData.HasValue && target == null)
		{
			return;
		}
		gunReady = false;
		int num = 12;
		anim.SetTrigger("Shoot");
		Vector3 endPoint = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(targetData.HasValue ? targetData.Value.position : target.position, eid.transform.position))
		{
			endPoint = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<ProjectileSpread>();
		gameObject.transform.position = base.transform.position;
		PortalPhysicsV2.ProjectThroughPortals(new Vector3(eid.transform.position.x, endPoint.y, eid.transform.position.z), endPoint, out endPoint, out var endRotation);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject2;
			if (i == 0)
			{
				gameObject2 = UnityEngine.Object.Instantiate(bullet, endPoint, endRotation, gameObject.transform);
			}
			else
			{
				Quaternion rotation = endRotation * Quaternion.Euler(UnityEngine.Random.Range(0f - spread, spread), UnityEngine.Random.Range(0f - spread, spread), UnityEngine.Random.Range(0f - spread, spread));
				gameObject2 = UnityEngine.Object.Instantiate(bullet, endPoint, rotation, gameObject.transform);
			}
			if (gameObject2.TryGetComponent<Projectile>(out var component))
			{
				if (targetData.HasValue)
				{
					component.targetHandle = targetData.Value.handle;
				}
				else
				{
					component.target = target;
				}
				component.safeEnemyType = safeEnemyType;
				if (difficulty == 1)
				{
					component.speed *= 0.75f;
				}
				else if (difficulty == 0)
				{
					component.speed *= 0.5f;
				}
				component.damage *= damageMultiplier;
				component.spreaded = true;
				if (instantExplode)
				{
					component.Explode();
				}
			}
		}
		gunAud.clip = shootSound;
		gunAud.volume = 0.35f;
		gunAud.panStereo = 0f;
		gunAud.SetPitch(UnityEngine.Random.Range(0.95f, 1.05f));
		gunAud.Play(tracked: true);
		UnityEngine.Object.Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
	}

	public void AltFire(bool instantExplode = false)
	{
		if (!targetData.HasValue && target == null)
		{
			CancelAltCharge();
			return;
		}
		gunReady = false;
		float num = 70f;
		if (difficulty == 1)
		{
			num = 50f;
		}
		else if (difficulty == 0)
		{
			num = 30f;
		}
		if (shootPoint == null)
		{
			return;
		}
		Vector3 position = shootPoint.position;
		if (Vector3.Distance(base.transform.position, eid.transform.position) > Vector3.Distance(targetData.HasValue ? targetData.Value.position : target.position, eid.transform.position))
		{
			position = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
		}
		GameObject obj = UnityEngine.Object.Instantiate(grenade, position, UnityEngine.Random.rotation);
		obj.GetComponent<Rigidbody>().AddForce(shootPoint.forward * num, ForceMode.VelocityChange);
		if (obj.TryGetComponent<Grenade>(out var component))
		{
			component.enemy = true;
			if (instantExplode)
			{
				component.Explode();
			}
		}
		anim.SetTrigger("Secondary Fire");
		gunAud.clip = shootSound;
		gunAud.volume = 0.35f;
		gunAud.panStereo = 0f;
		gunAud.SetPitch(UnityEngine.Random.Range(0.75f, 0.85f));
		gunAud.Play(tracked: true);
		UnityEngine.Object.Instantiate(muzzleFlash, shootPoint.position, shootPoint.rotation);
		CancelAltCharge();
	}

	public void PrepareFire()
	{
		if ((UnityEngine.Object)(object)heatSinkAud == null)
		{
			heatSinkAud = shootPoint.GetComponent<AudioSource>();
		}
		heatSinkAud.Play(tracked: true);
		UnityEngine.Object.Instantiate(warningFlash, shootPoint.position, shootPoint.rotation).transform.localScale *= 2f;
	}

	public void PrepareAltFire()
	{
		if ((UnityEngine.Object)(object)chargeSound == null)
		{
			chargeSound = base.transform.GetChild(0).GetComponent<AudioSource>();
		}
		charging = true;
		chargeAmount = 0f;
		chargeSound.SetPitch(0f);
	}

	public void CancelAltCharge()
	{
		if ((UnityEngine.Object)(object)chargeSound == null)
		{
			chargeSound = base.transform.GetChild(0).GetComponent<AudioSource>();
		}
		charging = false;
		chargeAmount = 0f;
		chargeSound.SetPitch(0f);
	}

	public void ReleaseHeat()
	{
		ParticleSystem[] array = parts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public void ClickSound()
	{
		gunAud.clip = clickSound;
		gunAud.volume = 0.5f;
		gunAud.SetPitch(UnityEngine.Random.Range(0.95f, 1.05f));
		gunAud.Play(tracked: true);
	}

	public void ReadyGun()
	{
		gunReady = true;
	}

	public void Smack()
	{
		gunAud.clip = smackSound;
		gunAud.volume = 0.75f;
		gunAud.SetPitch(UnityEngine.Random.Range(2f, 2.2f));
		gunAud.Play(tracked: true);
	}

	public void UpdateBuffs(EnemyIdentifier eid)
	{
		speedMultiplier = eid.totalSpeedModifier;
		damageMultiplier = eid.totalDamageModifier;
	}
}
