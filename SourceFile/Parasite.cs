using UnityEngine;

public class Parasite : MonoBehaviour
{
	public EnemyIdentifier parent;

	private EnemyTarget localTarget;

	public Transform projectileSpawnPos;

	private Animator anim;

	public GameObject[] decProjectiles;

	public GameObject[] projectiles;

	private GameObject currentDecProjectile;

	private bool inAction = true;

	private float cooldown;

	public GameObject windUpSound;

	private int shootType;

	private GoreZone gz;

	private int difficulty;

	public float speedMultiplier = 1f;

	public float damageMultiplier = 1f;

	private EnemyTarget target
	{
		get
		{
			object obj = localTarget;
			if (obj == null)
			{
				if (!(parent == null))
				{
					return parent.target;
				}
				obj = null;
			}
			return (EnemyTarget)obj;
		}
	}

	private void Start()
	{
		cooldown = Random.Range(0, 3);
		anim = GetComponent<Animator>();
		gz = GoreZone.ResolveGoreZone(base.transform);
		if (parent == null)
		{
			localTarget = new EnemyTarget(MonoSingleton<CameraController>.Instance.transform);
		}
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void Update()
	{
		if (target == null)
		{
			return;
		}
		Quaternion quaternion = Quaternion.LookRotation(target.position - base.transform.position, Vector3.up);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Time.deltaTime * (Quaternion.Angle(base.transform.rotation, quaternion) + 1f) * speedMultiplier);
		if (inAction)
		{
			return;
		}
		float num = 1f;
		if (difficulty == 1)
		{
			num = 0.75f;
		}
		else if (difficulty == 0)
		{
			num = 0.5f;
		}
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * num * speedMultiplier);
		}
		else if (!anim.IsInTransition(0))
		{
			if (Random.Range(0f, 1f) > 0.5f || difficulty < 2)
			{
				Shoot1();
			}
			else
			{
				Shoot2();
			}
		}
	}

	private void Shoot1()
	{
		Object.Instantiate(windUpSound, projectileSpawnPos);
		inAction = true;
		cooldown = Random.Range(2, 4);
		shootType = 0;
		anim.SetTrigger("Shoot1");
	}

	private void Shoot2()
	{
		Object.Instantiate(windUpSound, projectileSpawnPos);
		inAction = true;
		cooldown = Random.Range(2, 4);
		shootType = 1;
		anim.SetTrigger("Shoot2");
	}

	public void SpawnProjectile()
	{
		if ((bool)currentDecProjectile)
		{
			Object.Destroy(currentDecProjectile);
		}
		currentDecProjectile = Object.Instantiate(decProjectiles[shootType], projectileSpawnPos.position, projectileSpawnPos.rotation);
		currentDecProjectile.transform.localScale *= 5f;
		currentDecProjectile.transform.SetParent(projectileSpawnPos, worldPositionStays: true);
	}

	public void ShootProjectile()
	{
		if ((bool)currentDecProjectile)
		{
			Object.Destroy(currentDecProjectile);
		}
		GameObject obj = Object.Instantiate(projectiles[shootType], projectileSpawnPos.position, Quaternion.LookRotation(target.position - projectileSpawnPos.position));
		obj.transform.localScale *= 5f;
		obj.transform.SetParent(gz.transform, worldPositionStays: true);
		Projectile componentInChildren = obj.GetComponentInChildren<Projectile>();
		componentInChildren.target = target;
		componentInChildren.safeEnemyType = EnemyType.Minos;
		float num = Vector3.Distance(projectileSpawnPos.position, target.position);
		if (num > 65f)
		{
			componentInChildren.speed = num;
		}
		if (difficulty == 1)
		{
			componentInChildren.speed *= 0.75f;
		}
		else if (difficulty == 0)
		{
			componentInChildren.speed *= 0.5f;
		}
		componentInChildren.damage *= damageMultiplier;
		ProjectileSpread componentInChildren2 = obj.GetComponentInChildren<ProjectileSpread>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.spreadAmount = 3f;
			componentInChildren2.projectileAmount = 8;
		}
	}

	public void StopAction()
	{
		inAction = false;
	}
}
