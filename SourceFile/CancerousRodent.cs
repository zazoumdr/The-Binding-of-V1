using System.Collections.Generic;
using UnityEngine;

public class CancerousRodent : EnemyScript
{
	private Rigidbody rb;

	private Enemy enemy;

	private EnemyIdentifier eid;

	public bool harmless;

	public GameObject[] activateOnDeath;

	public Transform shootPoint;

	public GameObject projectile;

	private float coolDown = 2f;

	public int projectileAmount;

	private int currentProjectiles;

	private Vector3 origPos;

	private List<Transform> transforms => enemy.transforms;

	private GoreZone gz => enemy.gz;

	private BloodsplatterManager bsm => enemy.bsm;

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		enemy = GetComponent<Enemy>();
		eid = GetComponent<EnemyIdentifier>();
		if (enemy.isStatue)
		{
			enemy.isMassDeath = true;
		}
	}

	private void OnDisable()
	{
		if (harmless || (enemy.isStatue && !(enemy.health <= 0f)))
		{
			return;
		}
		GameObject[] array = activateOnDeath;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void Update()
	{
		if (eid.dead)
		{
			return;
		}
		if (rb != null)
		{
			if (eid.target == null)
			{
				rb.velocity = Vector3.zero;
			}
			else if (eid.target != null)
			{
				base.transform.LookAt(new Vector3(eid.target.position.x, base.transform.position.y, eid.target.position.z));
				rb.velocity = 100f * eid.totalSpeedModifier * Time.deltaTime * base.transform.forward;
			}
		}
		if (harmless)
		{
			if (!(enemy.health <= 0f))
			{
				return;
			}
			GameObject[] array = activateOnDeath;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
			}
			Object.Destroy(GetComponentInChildren<Light>().gameObject);
			Object.Destroy(this);
		}
		else if (enemy.health > 0f && eid.target != null)
		{
			if (coolDown != 0f)
			{
				coolDown = Mathf.MoveTowards(coolDown, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if (!Physics.Raycast(shootPoint.position, eid.target.position - shootPoint.position, Vector3.Distance(eid.target.position, shootPoint.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				coolDown = 3f;
				currentProjectiles = projectileAmount;
				FireBurst();
			}
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		GameObject[] array = activateOnDeath;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
		Object.Destroy(GetComponentInChildren<Light>().gameObject);
		if (enemy.isStatue)
		{
			enemy.isMassDeath = true;
		}
	}

	private void FireBurst()
	{
		GameObject obj = Object.Instantiate(projectile, shootPoint.position, shootPoint.rotation);
		obj.GetComponent<Rigidbody>().AddForce(shootPoint.forward * 2f, ForceMode.VelocityChange);
		if (obj.TryGetComponent<Projectile>(out var component))
		{
			component.target = eid.target;
			component.damage *= eid.totalDamageModifier;
		}
		currentProjectiles--;
		if (currentProjectiles > 0)
		{
			Invoke("FireBurst", 0.1f * eid.totalSpeedModifier);
		}
	}

	private void HandleDeath()
	{
		if (enemy.isStatue)
		{
			origPos = base.transform.position;
			enemy.transforms.AddRange(GetComponentsInChildren<Transform>());
			enemy.isMassDieing = true;
			Invoke("BloodExplosion", 3f);
		}
	}

	private void BloodExplosion()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform transform in transforms)
		{
			if (transform != null && Random.Range(0f, 1f) < 0.33f)
			{
				GameObject gore = bsm.GetGore(GoreType.Head, eid);
				if ((bool)gore)
				{
					gore.transform.position = transform.position;
					if (gz != null && gz.goreZone != null)
					{
						gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					if (gore.TryGetComponent<Bloodsplatter>(out var component))
					{
						component.GetReady();
					}
				}
			}
			else if (transform == null)
			{
				list.Add(transform);
			}
		}
		if (list.Count > 0)
		{
			foreach (Transform item in list)
			{
				transforms.Remove(item);
			}
			list.Clear();
		}
		if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < 40; i++)
			{
				GameObject gib;
				if (i < 30)
				{
					gib = bsm.GetGib(BSType.gib);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							enemy.ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
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
					gib = bsm.GetGib(BSType.eyeball);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							enemy.ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(3f, 6f);
					}
					else
					{
						i = 35;
					}
					continue;
				}
				gib = bsm.GetGib(BSType.brainChunk);
				if (!gib)
				{
					break;
				}
				if ((bool)gz && (bool)gz.gibZone)
				{
					enemy.ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
				}
				gib.transform.localScale *= Random.Range(3f, 4f);
			}
		}
		base.enabled = false;
		DeathEnd();
	}

	protected void OnDestroy()
	{
		if (eid.dead)
		{
			DeathEnd();
		}
	}

	protected void DeathEnd()
	{
		if (!eid.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		if (enemy.musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		if (base.gameObject != null)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
