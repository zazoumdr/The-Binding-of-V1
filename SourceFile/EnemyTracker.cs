using System;
using System.Collections.Generic;
using Sandbox;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.None)]
public class EnemyTracker : MonoSingleton<EnemyTracker>
{
	public List<EnemyIdentifier> enemies = new List<EnemyIdentifier>();

	public List<int> enemyRanks = new List<int>();

	public List<Drone> drones = new List<Drone>();

	public int deathcatcherCount;

	public readonly Dictionary<int, SavedEnemy> spawnedEnemies = new Dictionary<int, SavedEnemy>();

	public static Action<EnemyIdentifier> onEnemyAdded;

	public static Action<EnemyIdentifier> onEnemyRemoved;

	private void Update()
	{
		if (!Debug.isDebugBuild || !Input.GetKeyDown(KeyCode.F9))
		{
			return;
		}
		foreach (EnemyIdentifier currentEnemy in GetCurrentEnemies())
		{
			currentEnemy.gameObject.SetActive(value: false);
			currentEnemy.gameObject.SetActive(value: true);
		}
	}

	public List<EnemyIdentifier> GetCurrentEnemies()
	{
		List<EnemyIdentifier> list = new List<EnemyIdentifier>();
		if (enemies != null && enemies.Count > 0)
		{
			for (int num = enemies.Count - 1; num >= 0; num--)
			{
				if (enemies[num].dead || enemies[num] == null || enemies[num].gameObject == null)
				{
					RemoveEnemy(num);
				}
				else if (enemies[num].gameObject.activeInHierarchy)
				{
					list.Add(enemies[num]);
				}
			}
		}
		return list;
	}

	public bool TryGetEnemy(int eidInstanceID, out EnemyIdentifier eid, bool includePuppetRespawned = false)
	{
		eid = null;
		List<Deathcatcher> list = null;
		if (includePuppetRespawned)
		{
			list = new List<Deathcatcher>();
		}
		foreach (EnemyIdentifier enemy in enemies)
		{
			if (!(enemy == null))
			{
				if (enemy.GetInstanceID() == eidInstanceID)
				{
					eid = enemy;
					return true;
				}
				if (includePuppetRespawned && enemy.enemyType == EnemyType.Deathcatcher && enemy.TryGetComponent<Deathcatcher>(out var component))
				{
					list.Add(component);
				}
			}
		}
		if (includePuppetRespawned && spawnedEnemies.TryGetValue(eidInstanceID, out var _))
		{
			foreach (Deathcatcher item in list)
			{
				if (item.TryGetPuppet(eidInstanceID, out var puppet) && puppet != null && puppet.TryGetComponent<EnemyIdentifier>(out var component2))
				{
					eid = component2;
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEnemySpawnRecorded(int eidInstanceID)
	{
		return spawnedEnemies.ContainsKey(eidInstanceID);
	}

	public void UpdateIdolsNow()
	{
		foreach (EnemyIdentifier currentEnemy in GetCurrentEnemies())
		{
			if (currentEnemy.enemyType == EnemyType.Idol && currentEnemy.idol != null)
			{
				currentEnemy.idol.PickNewTarget();
			}
		}
	}

	public List<EnemyIdentifier> GetEnemiesOfType(EnemyType type)
	{
		List<EnemyIdentifier> currentEnemies = GetCurrentEnemies();
		if (currentEnemies.Count > 0)
		{
			for (int num = currentEnemies.Count - 1; num >= 0; num--)
			{
				if (currentEnemies[num].enemyType != type)
				{
					currentEnemies.RemoveAt(num);
				}
			}
		}
		return currentEnemies;
	}

	public void AddEnemy(EnemyIdentifier eid)
	{
		if (enemies.Contains(eid))
		{
			return;
		}
		enemies.Add(eid);
		enemyRanks.Add(GetEnemyRank(eid));
		if (eid.enemyType == EnemyType.Deathcatcher)
		{
			deathcatcherCount++;
		}
		EnemySpawnableInstance enemySpawnableInstance = null;
		EnemySpawnableInstance component;
		if (eid.enemyType == EnemyType.MaliciousFace)
		{
			enemySpawnableInstance = eid.GetComponentInParent<EnemySpawnableInstance>();
		}
		else if (eid.TryGetComponent<EnemySpawnableInstance>(out component))
		{
			enemySpawnableInstance = component;
		}
		if (enemySpawnableInstance != null)
		{
			if (eid.puppet)
			{
				return;
			}
			EnemySpawnableInstance enemySpawnableInstance2 = new EnemySpawnableInstance();
			Debug.Log(enemySpawnableInstance2.sourceObject?.ToString() + " " + (enemySpawnableInstance2.sourceObject?.enemyType).ToString());
			if (enemySpawnableInstance.sourceObject != null)
			{
				SavedEnemy value = enemySpawnableInstance.SaveEnemy();
				spawnedEnemies[eid.GetInstanceID()] = value;
			}
		}
		onEnemyAdded?.Invoke(eid);
	}

	public void RemoveEnemy(EnemyIdentifier eid)
	{
		if (enemies.Contains(eid))
		{
			int index = enemies.IndexOf(eid);
			RemoveEnemy(index);
		}
	}

	private void RemoveEnemy(int index)
	{
		if (index >= 0 && index < enemies.Count)
		{
			EnemyIdentifier enemyIdentifier = enemies[index];
			enemies.RemoveAt(index);
			enemyRanks.RemoveAt(index);
			if (enemyIdentifier.enemyType == EnemyType.Deathcatcher)
			{
				deathcatcherCount--;
			}
			else if (deathcatcherCount <= 0)
			{
				int instanceID = enemyIdentifier.GetInstanceID();
				spawnedEnemies.Remove(instanceID);
			}
			onEnemyRemoved?.Invoke(enemyIdentifier);
		}
	}

	public int GetEnemyRank(EnemyIdentifier eid)
	{
		return eid.enemyType switch
		{
			EnemyType.Cerberus => 3, 
			EnemyType.Drone => 1, 
			EnemyType.Deathcatcher => 8, 
			EnemyType.Ferryman => 5, 
			EnemyType.Filth => 0, 
			EnemyType.Gabriel => 6, 
			EnemyType.GabrielSecond => 6, 
			EnemyType.Gutterman => 4, 
			EnemyType.Guttertank => 4, 
			EnemyType.HideousMass => 6, 
			EnemyType.MaliciousFace => 3, 
			EnemyType.Mandalore => 5, 
			EnemyType.Mannequin => 2, 
			EnemyType.Mindflayer => 5, 
			EnemyType.Minos => 6, 
			EnemyType.MinosPrime => 7, 
			EnemyType.Minotaur => 6, 
			EnemyType.Power => 6, 
			EnemyType.Providence => 3, 
			EnemyType.Puppet => 0, 
			EnemyType.Schism => 1, 
			EnemyType.Sisyphus => 6, 
			EnemyType.SisyphusPrime => 7, 
			EnemyType.Soldier => 1, 
			EnemyType.Stalker => 4, 
			EnemyType.Stray => 0, 
			EnemyType.Streetcleaner => 2, 
			EnemyType.Swordsmachine => 3, 
			EnemyType.Turret => 3, 
			EnemyType.V2 => 6, 
			EnemyType.V2Second => 6, 
			EnemyType.Virtue => 3, 
			EnemyType.Wicked => 6, 
			_ => -1, 
		};
	}
}
