using System.Collections.Generic;
using System.Linq;
using plog;
using UnityEngine;

public class EnemyScanner
{
	private static readonly Logger Log = new Logger("EnemyScanner");

	private const bool DebugMode = false;

	private readonly EnemyIdentifier owner;

	private readonly Transform ownerRaycastOrigin;

	private readonly float tickInterval;

	private TimeSince? timeSinceLastTick;

	private Queue<EnemyIdentifier> pendingLineOfSightChecks;

	public EnemyScanner(EnemyIdentifier owner)
	{
		this.owner = owner;
		ownerRaycastOrigin = owner.GetCenter();
		tickInterval = 0.5f + Random.Range(0f, 0.3f);
	}

	public void Update()
	{
		if (owner == null || owner.dead)
		{
			Reset();
		}
		else if (!owner.AttackEnemies)
		{
			Reset();
		}
		else if (owner.target != null && !owner.IsCurrentTargetFallback && owner.target.isValid)
		{
			Reset();
		}
		else if (pendingLineOfSightChecks != null)
		{
			if (pendingLineOfSightChecks.Count <= 0)
			{
				Reset();
				return;
			}
			EnemyIdentifier enemyIdentifier = pendingLineOfSightChecks.Dequeue();
			if (!(enemyIdentifier == null) && !enemyIdentifier.dead && !enemyIdentifier.ignoredByEnemies)
			{
				Vector3 position = ownerRaycastOrigin.position;
				Vector3 position2 = enemyIdentifier.GetCenter().position;
				Ray ray = new Ray(position, position2 - position);
				float maxDistance = Vector3.Distance(position, position2);
				if (!Physics.Raycast(ray, out var _, maxDistance, LayerMaskDefaults.Get(LMD.Environment)))
				{
					SetTarget(enemyIdentifier);
					pendingLineOfSightChecks.Clear();
				}
			}
		}
		else if (!timeSinceLastTick.HasValue)
		{
			timeSinceLastTick = 0f;
		}
		else if ((float?)timeSinceLastTick > tickInterval)
		{
			Tick();
		}
	}

	public void Reset()
	{
		pendingLineOfSightChecks = null;
		timeSinceLastTick = 0f;
	}

	private void Tick()
	{
		timeSinceLastTick = 0f;
		IEnumerable<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies != null)
		{
			currentEnemies = currentEnemies.Where(CanBeTargeted);
			currentEnemies = currentEnemies.OrderBy((EnemyIdentifier e) => Vector3.Distance(owner.GetCenter().position, e.GetCenter().position)).ToList();
			pendingLineOfSightChecks = new Queue<EnemyIdentifier>(currentEnemies);
		}
	}

	private bool CanBeTargeted(EnemyIdentifier enemy)
	{
		if (enemy == null || enemy.dead || enemy.ignoredByEnemies)
		{
			return false;
		}
		if (enemy == owner)
		{
			return false;
		}
		if (owner.IsTypeFriendly(enemy))
		{
			return false;
		}
		return true;
	}

	private void SetTarget(EnemyIdentifier enemy)
	{
		EnemyTarget enemyTarget = new EnemyTarget(enemy);
		if (enemyTarget.isValid)
		{
			owner.target = enemyTarget;
		}
	}
}
