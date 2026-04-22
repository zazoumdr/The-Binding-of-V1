using UnityEngine;

public class MortarLauncher : MonoBehaviour
{
	private EnemyIdentifier eid;

	public Transform shootPoint;

	public Projectile mortar;

	private float cooldown = 1f;

	public float firingDelay;

	public float firstFireDelay = 1f;

	public float projectileForce;

	public UltrakillEvent onFire;

	private Animator anim;

	private int difficulty = -1;

	private float difficultySpeedModifier = 1f;

	private void Start()
	{
		eid = GetComponentInParent<EnemyIdentifier>();
		anim = GetComponent<Animator>();
		cooldown = firstFireDelay;
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
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier * difficultySpeedModifier);
		if (cooldown == 0f && eid.target != null)
		{
			cooldown = firingDelay;
			ShootHoming();
			onFire?.Invoke();
		}
	}

	public void ShootHoming()
	{
		if (eid.target != null)
		{
			Projectile projectile = Object.Instantiate(mortar, shootPoint.position, shootPoint.rotation);
			projectile.target = eid.target;
			projectile.GetComponent<Rigidbody>().velocity = shootPoint.forward * projectileForce;
			projectile.damage *= eid.totalDamageModifier;
			projectile.safeEnemyType = eid.enemyType;
			projectile.turningSpeedMultiplier *= difficultySpeedModifier;
			projectile.gameObject.SetActive(value: true);
			if ((bool)(Object)(object)anim)
			{
				anim.Play("Shoot", 0, 0f);
			}
		}
	}

	public void ChangeFiringDelay(float target)
	{
		firingDelay = target;
		if (cooldown > firingDelay)
		{
			cooldown = firingDelay;
		}
	}
}
