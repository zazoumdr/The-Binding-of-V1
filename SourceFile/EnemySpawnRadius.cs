using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnRadius : MonoBehaviour
{
	public GameObject[] spawnables;

	private List<GameObject> spawnedObjects = new List<GameObject>();

	private List<EnemyIdentifier> currentEnemies = new List<EnemyIdentifier>();

	public float minimumDistance;

	public float maximumDistance;

	public float spawnCooldown;

	private float cooldown;

	public int maximumEnemyCount;

	public bool spawnAsPuppets = true;

	private GoreZone gz;

	private void Start()
	{
		gz = GoreZone.ResolveGoreZone(base.transform);
		Invoke("SlowUpdate", 1f);
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 1f);
		if (currentEnemies == null || currentEnemies.Count == 0)
		{
			return;
		}
		for (int num = currentEnemies.Count - 1; num >= 0; num--)
		{
			if (spawnedObjects[num] == null || currentEnemies[num].dead)
			{
				spawnedObjects.RemoveAt(num);
				currentEnemies.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		if (cooldown <= 0f)
		{
			if (currentEnemies.Count < maximumEnemyCount)
			{
				SpawnEnemy();
			}
			else
			{
				cooldown = 2f;
			}
		}
	}

	public void SpawnEnemy()
	{
		for (int i = 0; i < 3; i++)
		{
			Vector3 normalized = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
			if (!Physics.Raycast(base.transform.position + normalized * Random.Range(minimumDistance, maximumDistance), Vector3.down, out var hitInfo, 25f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				continue;
			}
			cooldown = spawnCooldown;
			GameObject gameObject = Object.Instantiate(spawnables[Random.Range(0, spawnables.Length)], hitInfo.point, Quaternion.identity);
			gameObject.transform.SetParent(gz.transform, worldPositionStays: true);
			spawnedObjects.Add(gameObject);
			EnemyIdentifier componentInChildren = gameObject.GetComponentInChildren<EnemyIdentifier>();
			if ((bool)componentInChildren)
			{
				currentEnemies.Add(componentInChildren);
				if (spawnAsPuppets)
				{
					componentInChildren.puppet = true;
				}
			}
			else
			{
				currentEnemies.Add(null);
			}
			gameObject.SetActive(value: true);
			return;
		}
		cooldown = 1f;
	}

	public void KillAllEnemies()
	{
		for (int num = currentEnemies.Count - 1; num >= 0; num--)
		{
			if (currentEnemies[num] != null)
			{
				currentEnemies[num].InstaKill();
				spawnedObjects.RemoveAt(num);
				currentEnemies.RemoveAt(num);
			}
		}
	}
}
