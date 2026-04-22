public class TempSpider : EnemyScript
{
	private Enemy spiderEnemy;

	public bool knockBack;

	private void Awake()
	{
		spiderEnemy = GetComponent<Enemy>();
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		EnemyMovementData result = default(EnemyMovementData);
		if (difficulty >= 4)
		{
			result.speed = 5f;
			result.angularSpeed = 500f;
			result.acceleration = 25f;
		}
		else
		{
			switch (difficulty)
			{
			case 3:
				result.speed = 4.5f;
				result.angularSpeed = 400f;
				result.acceleration = 20f;
				break;
			case 2:
				result.speed = 3.5f;
				result.angularSpeed = 300f;
				result.acceleration = 15f;
				break;
			case 1:
				result.speed = 2.5f;
				result.angularSpeed = 200f;
				result.acceleration = 10f;
				break;
			default:
				result.speed = 2f;
				result.angularSpeed = 150f;
				result.acceleration = 8f;
				break;
			}
		}
		return result;
	}

	public override bool ShouldKnockback(ref DamageData data)
	{
		return knockBack;
	}

	public override void OnDamage(ref DamageData data)
	{
	}

	public override void OnParry(ref DamageData data, bool isShotgun)
	{
	}

	public override void OnGoLimp(bool fromExplosion)
	{
	}

	public override void OnFall()
	{
	}

	public override void OnLand()
	{
	}
}
