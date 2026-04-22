using System.Linq;
using UnityEngine;

public class CombinedBossBar : MonoBehaviour, IEnemyHealthDetails
{
	public string fullName;

	public EnemyIdentifier[] enemies;

	public string FullName => fullName;

	public float Health => enemies.Sum((EnemyIdentifier x) => (!(x == null)) ? Mathf.Max(0f, x.Health) : 0f);

	public bool Dead => enemies.All((EnemyIdentifier x) => x == null || x.dead);

	public bool Blessed => enemies.All((EnemyIdentifier x) => x == null || x.blessed);

	private void OnEnable()
	{
		if (!TryGetComponent<BossHealthBar>(out var _))
		{
			base.gameObject.AddComponent<BossHealthBar>();
		}
	}

	public void ForceGetHealth()
	{
		if (enemies != null && enemies.Length != 0)
		{
			EnemyIdentifier[] array = enemies;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceGetHealth();
			}
		}
	}
}
