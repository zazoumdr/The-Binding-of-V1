using ULTRAKILL.Cheats;
using UnityEngine;

[DefaultExecutionOrder(500)]
public class ActivateArena : MonoBehaviour
{
	public bool onlyWave;

	[HideInInspector]
	public bool activated;

	public Door[] doors;

	public GameObject[] enemies;

	private int currentEnemy;

	public bool forEnemy;

	public int waitForStatus;

	public bool activateOnEnable;

	private ArenaStatus astat;

	private bool playerIn;

	private void OnEnable()
	{
		if (activated || !activateOnEnable || DisableEnemySpawns.DisableArenaTriggers)
		{
			return;
		}
		if (waitForStatus > 0)
		{
			if (astat == null)
			{
				astat = GetComponentInParent<ArenaStatus>();
			}
			if (astat == null || astat.currentStatus < waitForStatus)
			{
				return;
			}
		}
		Activate();
	}

	private void Update()
	{
		if ((playerIn || activateOnEnable) && (bool)astat && astat.currentStatus >= waitForStatus && !activated)
		{
			Activate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (DisableEnemySpawns.DisableArenaTriggers)
		{
			return;
		}
		if (waitForStatus > 0)
		{
			if (astat == null)
			{
				astat = GetComponentInParent<ArenaStatus>();
			}
			if (astat == null)
			{
				return;
			}
			if (astat.currentStatus < waitForStatus)
			{
				if ((!forEnemy && other.gameObject.CompareTag("Player") && !activated) || (forEnemy && other.gameObject.CompareTag("Enemy") && !activated))
				{
					playerIn = true;
				}
				return;
			}
		}
		if ((!forEnemy && other.gameObject.CompareTag("Player") && !activated) || (forEnemy && other.gameObject.CompareTag("Enemy") && !activated))
		{
			Activate();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((!forEnemy && other.gameObject.CompareTag("Player") && !activated) || (forEnemy && other.gameObject.CompareTag("Enemy") && !activated))
		{
			playerIn = false;
		}
	}

	public void Activate()
	{
		if (DisableEnemySpawns.DisableArenaTriggers || activated)
		{
			return;
		}
		activated = true;
		if (!onlyWave && !forEnemy)
		{
			MonoSingleton<MusicManager>.Instance.ArenaMusicStart();
		}
		if (doors.Length != 0)
		{
			Door[] array = doors;
			foreach (Door door in array)
			{
				if (!(door == null))
				{
					if (!door.gameObject.activeSelf)
					{
						door.gameObject.SetActive(value: true);
					}
					door.Lock();
				}
			}
			if (enemies.Length != 0)
			{
				Invoke("SpawnEnemy", 1f);
			}
			else
			{
				Object.Destroy(this);
			}
		}
		else if (enemies.Length != 0)
		{
			SpawnEnemy();
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void SpawnEnemy()
	{
		if (currentEnemy >= enemies.Length)
		{
			Object.Destroy(this);
			return;
		}
		float time = 0.1f;
		if (enemies[currentEnemy] != null)
		{
			if (enemies[currentEnemy].activeSelf)
			{
				time = 0f;
			}
			else
			{
				enemies[currentEnemy].SetActive(value: true);
			}
		}
		currentEnemy++;
		if (currentEnemy < enemies.Length)
		{
			Invoke("SpawnEnemy", time);
		}
		else
		{
			Object.Destroy(this);
		}
	}
}
