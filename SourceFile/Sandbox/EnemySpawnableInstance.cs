using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;
using UnityEngine.AI;

namespace Sandbox;

public class EnemySpawnableInstance : SpawnableInstance
{
	private static readonly Logger Log = new Logger("EnemySpawnableInstance");

	[NonSerialized]
	public EnemyIdentifier eid;

	public EnemyRadianceConfig radiance;

	private bool lastSpeedBuffState;

	private bool lastDamageBuffState;

	private bool lastHealthBuffState;

	private bool lastKinematicState;

	public override void Awake()
	{
		base.Awake();
		eid = GetComponent<EnemyIdentifier>();
		if (eid == null)
		{
			eid = GetComponentInChildren<EnemyIdentifier>(includeInactive: true);
		}
		if (eid == null)
		{
			eid = GetComponentInParent<EnemyIdentifier>(includeInactive: true);
		}
		if (eid == null)
		{
			Log.Error("EnemyIdentifier component not found on this enemy instance. Whar....", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (eid != null)
		{
			radiance = new EnemyRadianceConfig(eid);
		}
	}

	public void RestoreRadiance(EnemyRadianceConfig config)
	{
		radiance = config;
		if (config != null)
		{
			UpdateRadiance();
		}
	}

	public void UpdateRadiance()
	{
		eid.radianceTier = radiance.tier;
		if (!lastSpeedBuffState && radiance.speedEnabled)
		{
			eid.SpeedBuff(radiance.speedBuff);
		}
		else if (lastSpeedBuffState && !radiance.speedEnabled)
		{
			eid.SpeedUnbuff();
		}
		eid.speedBuffModifier = radiance.speedBuff;
		if (!lastDamageBuffState && radiance.damageEnabled)
		{
			eid.DamageBuff(radiance.damageBuff);
		}
		else if (lastDamageBuffState && !radiance.damageEnabled)
		{
			eid.DamageUnbuff();
		}
		eid.damageBuffModifier = radiance.damageBuff;
		if (!lastHealthBuffState && radiance.healthEnabled)
		{
			eid.HealthBuff(radiance.healthBuff);
		}
		else if (lastHealthBuffState && !radiance.healthEnabled)
		{
			eid.HealthUnbuff();
		}
		eid.healthBuffModifier = radiance.healthBuff;
		lastSpeedBuffState = radiance.speedEnabled;
		lastDamageBuffState = radiance.damageEnabled;
		lastHealthBuffState = radiance.healthEnabled;
		eid.UpdateBuffs();
	}

	private void OnEnable()
	{
		if (eid != null)
		{
			return;
		}
		eid = GetComponent<EnemyIdentifier>();
		if (!eid)
		{
			eid = GetComponentInChildren<EnemyIdentifier>();
			if (eid == null)
			{
				eid = GetComponentInParent<EnemyIdentifier>();
			}
		}
	}

	public SavedEnemy SaveEnemy()
	{
		if (!eid)
		{
			return null;
		}
		SavedEnemy savedEnemy = new SavedEnemy
		{
			Radiance = radiance
		};
		SavedGeneric saveObject = savedEnemy;
		BaseSave(ref saveObject);
		if (eid.originalScale != Vector3.zero)
		{
			savedEnemy.Scale = SavedVector3.One;
		}
		return savedEnemy;
	}

	public override void Pause(bool freeze = true)
	{
		base.Pause(freeze);
		GameObject gameObject = collider.gameObject;
		EnemyIdentifier enemyIdentifier = null;
		if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out var component))
		{
			enemyIdentifier = component;
			enemyIdentifier.enabled = false;
		}
		else
		{
			enemyIdentifier = collider.gameObject.GetComponentInChildren<EnemyIdentifier>();
			if (enemyIdentifier != null)
			{
				enemyIdentifier.enabled = false;
				gameObject = enemyIdentifier.gameObject;
			}
		}
		foreach (Behaviour enemyComponent in GetEnemyComponents(gameObject))
		{
			enemyComponent.enabled = false;
		}
		if (gameObject.TryGetComponent<NavMeshAgent>(out var component2))
		{
			((Behaviour)(object)component2).enabled = false;
		}
		if (gameObject.TryGetComponent<Animator>(out var component3))
		{
			((Behaviour)(object)component3).enabled = false;
		}
		if (gameObject.TryGetComponent<Rigidbody>(out var component4))
		{
			lastKinematicState = component4.isKinematic;
			if (component4.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic)
			{
				component4.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			}
			component4.isKinematic = true;
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (collider == null)
		{
			return;
		}
		foreach (Behaviour enemyComponent in GetEnemyComponents(collider.gameObject))
		{
			enemyComponent.enabled = true;
		}
		if (collider.gameObject.TryGetComponent<NavMeshAgent>(out var component))
		{
			((Behaviour)(object)component).enabled = true;
		}
		if (collider.gameObject.TryGetComponent<EnemyIdentifier>(out var component2))
		{
			component2.enabled = true;
		}
		if (collider.gameObject.TryGetComponent<Animator>(out var component3))
		{
			((Behaviour)(object)component3).enabled = true;
		}
		if (collider.gameObject.TryGetComponent<Rigidbody>(out var component4))
		{
			component4.isKinematic = lastKinematicState;
		}
	}

	private IEnumerable<Component> GetEnemyComponents(GameObject obj)
	{
		foreach (Type type in EnemyTypes.Types)
		{
			Component component;
			if (sourceObject.fullEnemyComponent)
			{
				Component[] componentsInChildren = obj.GetComponentsInChildren(type);
				Component[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
			}
			else if (obj.TryGetComponent(type, out component))
			{
				yield return component;
			}
		}
	}

	private void Update()
	{
		if (!(eid != null))
		{
			Log.Fine("Destroying sandbox enemy due to missing EnemyIdentifier component.", (IEnumerable<Tag>)null, (string)null, (object)null);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
