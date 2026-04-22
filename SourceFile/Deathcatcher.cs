using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using ULTRAKILL.Cheats;
using UnityEngine;

public class Deathcatcher : MonoBehaviour
{
	public bool active = true;

	public bool canRespawnIdols = true;

	private int difficulty = -1;

	private EnemyIdentifier eid;

	private CapsuleCollider col;

	private GoreZone affectedGoreZone;

	private readonly List<CaughtEnemy> deadCaughtEnemies = new List<CaughtEnemy>();

	public float respawnDelay;

	[HideInInspector]
	public float countdownToRespawn;

	private TimeSince timeSinceRespawn;

	public MeshRenderer chargeSphere;

	private AudioSource chargeAud;

	private MaterialPropertyBlock block;

	public GameObject respawnEffect;

	private bool dead;

	[SerializeField]
	private GameObject deathParticle;

	public bool killPuppetsOnDeath = true;

	public GameObject closedModel;

	public GameObject openingModel;

	public GameObject openModel;

	private Animator anim;

	public GameObject[] effectsWhenOpen;

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		col = GetComponent<CapsuleCollider>();
		anim = GetComponent<Animator>();
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if ((bool)chargeSphere)
		{
			block = new MaterialPropertyBlock();
			chargeAud = chargeSphere.GetComponent<AudioSource>();
		}
	}

	private void Start()
	{
		affectedGoreZone = GoreZone.ResolveGoreZone(base.transform);
		if ((bool)affectedGoreZone && !affectedGoreZone.deathCatchers.Contains(this))
		{
			affectedGoreZone.deathCatchers.Add(this);
		}
		ChangeAnimationState(active);
		SlowUpdate();
	}

	public void EnemyDeath(EnemyIdentifier eid)
	{
		if (eid == null)
		{
			Debug.LogWarning("EnemyIdentifier is null. Cannot track death.");
			return;
		}
		Debug.Log("EnemyDeath called for " + eid.name);
		if (canRespawnIdols || eid.enemyType != EnemyType.Idol)
		{
			EnemyTracker instance = MonoSingleton<EnemyTracker>.Instance;
			SavedEnemy value;
			if (instance == null)
			{
				Debug.LogWarning("EnemyTracker instance not found. Cannot track enemy death.");
			}
			else if (!instance.spawnedEnemies.TryGetValue(eid.GetInstanceID(), out value))
			{
				Debug.LogWarning("Enemy " + eid.name + " not found in spawned enemies. Cannot track death.");
			}
			else if (!deadCaughtEnemies.Any((CaughtEnemy c) => c.original == eid))
			{
				CaughtEnemy caughtEnemy = new CaughtEnemy(eid, value);
				Debug.Log($"Tracking death of enemy {eid.name} at position {caughtEnemy.position} with rotation {caughtEnemy.rotation}");
				deadCaughtEnemies.Add(caughtEnemy);
			}
		}
	}

	private void OnGUI()
	{
		if (DeathCatcherDebug.Active)
		{
			GUI.BeginGroup(new Rect(320f, 0f, 300f, Screen.height));
			GUILayout.Label($"Deathcatcher Caught Enemies: {deadCaughtEnemies.Count}", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUI.EndGroup();
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.2f);
		if (!active || deadCaughtEnemies == null || deadCaughtEnemies.Count == 0)
		{
			return;
		}
		for (int num = deadCaughtEnemies.Count - 1; num >= 0; num--)
		{
			if (deadCaughtEnemies[num] == null)
			{
				Debug.LogWarning($"CaughtEnemy at index {num} is null, removing from list.");
				deadCaughtEnemies.RemoveAt(num);
			}
			else if (deadCaughtEnemies[num].puppet == null)
			{
				if (countdownToRespawn <= 0f && (float)timeSinceRespawn > TimeUntilRespawn())
				{
					countdownToRespawn = respawnDelay;
				}
			}
			else
			{
				deadCaughtEnemies[num].UpdatePosition(deadCaughtEnemies[num].puppet.transform.position, deadCaughtEnemies[num].puppet.transform.rotation);
			}
		}
	}

	private float TimeUntilRespawn()
	{
		switch (difficulty)
		{
		case 4:
		case 5:
			return 5f;
		case 3:
			return 6f;
		case 2:
			return 8f;
		case 1:
			return 10f;
		case 0:
			return 12f;
		default:
			return 5f;
		}
	}

	private void Update()
	{
		if (!(countdownToRespawn > 0f))
		{
			return;
		}
		countdownToRespawn = Mathf.MoveTowards(countdownToRespawn, 0f, Time.deltaTime);
		if ((bool)chargeSphere)
		{
			if (!chargeSphere.gameObject.activeSelf)
			{
				chargeSphere.gameObject.SetActive(value: true);
			}
			chargeSphere.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 30f, countdownToRespawn / respawnDelay);
			chargeSphere.GetPropertyBlock(block, 0);
			block.SetFloat("_Opacity", 1f - countdownToRespawn / respawnDelay);
			chargeSphere.SetPropertyBlock(block, 0);
			if ((bool)(UnityEngine.Object)(object)chargeAud)
			{
				chargeAud.SetPitch((1f - countdownToRespawn / respawnDelay) * 2f + 1f);
				chargeAud.volume = 1f - countdownToRespawn / respawnDelay * 0.66f;
			}
		}
		if (countdownToRespawn <= 0f)
		{
			StartCoroutine(RespawnPuppets());
			if ((bool)chargeSphere)
			{
				chargeSphere.gameObject.SetActive(value: false);
				UnityEngine.Object.Instantiate(respawnEffect, chargeSphere.transform.position, Quaternion.identity);
			}
			timeSinceRespawn = 0f;
		}
	}

	private IEnumerator RespawnPuppets()
	{
		if (!active || deadCaughtEnemies == null || deadCaughtEnemies.Count == 0)
		{
			yield break;
		}
		for (int i = deadCaughtEnemies.Count - 1; i >= 0; i--)
		{
			if (deadCaughtEnemies[i] == null)
			{
				deadCaughtEnemies.RemoveAt(i);
			}
			else if (deadCaughtEnemies[i].puppet == null)
			{
				SavedEnemy savedEnemy = deadCaughtEnemies[i].savedEnemy;
				SpawnableInstance spawnableInstance = savedEnemy.Spawnable.InstantiateSpawnable(savedEnemy, base.transform.parent);
				spawnableInstance.transform.position = deadCaughtEnemies[i].position;
				spawnableInstance.transform.rotation = deadCaughtEnemies[i].rotation;
				spawnableInstance.ApplyAlterOptions(new AlterOption[1]
				{
					new AlterOption
					{
						targetKey = "puppeted",
						useBool = true,
						boolValue = true
					}
				});
				if (eid.healthBuff || eid.speedBuff || eid.damageBuff)
				{
					EnemyIdentifier componentInChildren = spawnableInstance.GetComponentInChildren<EnemyIdentifier>(includeInactive: true);
					if ((bool)componentInChildren)
					{
						componentInChildren.radianceTier = eid.radianceTier;
						if (eid.healthBuff)
						{
							componentInChildren.healthBuff = true;
							componentInChildren.healthBuffRequests++;
							componentInChildren.healthBuffModifier = eid.healthBuffModifier;
						}
						if (eid.speedBuff)
						{
							componentInChildren.speedBuff = true;
							componentInChildren.speedBuffRequests++;
							componentInChildren.speedBuffModifier = eid.speedBuffModifier;
						}
						if (eid.damageBuff)
						{
							componentInChildren.damageBuff = true;
							componentInChildren.damageBuffRequests++;
							componentInChildren.damageBuffModifier = eid.damageBuffModifier;
						}
					}
				}
				deadCaughtEnemies[i].UpdatePuppet(spawnableInstance.gameObject);
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	public void Death()
	{
		if (dead)
		{
			return;
		}
		dead = true;
		try
		{
			if ((bool)affectedGoreZone && affectedGoreZone.deathCatchers.Contains(this))
			{
				affectedGoreZone.deathCatchers.Remove(this);
			}
			GoreZone goreZone = (affectedGoreZone ? affectedGoreZone : GoreZone.ResolveGoreZone(base.transform));
			if ((bool)eid)
			{
				eid.Death();
			}
			if ((bool)deathParticle && (bool)col && (bool)goreZone)
			{
				UnityEngine.Object.Instantiate(deathParticle, col.bounds.center, Quaternion.identity, goreZone.gibZone);
			}
			if ((bool)col && (bool)goreZone)
			{
				GameObject gameObject = null;
				for (int i = 0; i < 3; i++)
				{
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, eid);
					if (!gameObject)
					{
						break;
					}
					gameObject.transform.position = col.bounds.center;
					if ((bool)goreZone.goreZone)
					{
						gameObject.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
					}
					gameObject.SetActive(value: true);
					if (gameObject.TryGetComponent<Bloodsplatter>(out var component))
					{
						component.GetReady();
					}
				}
			}
			if ((bool)eid && !eid.dontCountAsKills)
			{
				ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
				if (componentInParent != null)
				{
					componentInParent.AddDeadEnemy();
				}
			}
			if (killPuppetsOnDeath && deadCaughtEnemies != null && deadCaughtEnemies.Count > 0)
			{
				foreach (CaughtEnemy deadCaughtEnemy in deadCaughtEnemies)
				{
					if (deadCaughtEnemy.puppet != null)
					{
						deadCaughtEnemy.puppet.GetComponentInChildren<EnemyIdentifier>()?.InstaKill();
					}
				}
			}
			if ((bool)MonoSingleton<StyleHUD>.Instance)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.heartbreak", null, eid);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Deathcatcher.Death() encountered an error during death effects: {arg}");
		}
		finally
		{
			base.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void IsActive(bool newState)
	{
		active = newState;
		ChangeAnimationState(newState);
	}

	public bool TryGetPuppet(int originalID, out GameObject puppet)
	{
		puppet = null;
		if (deadCaughtEnemies == null || deadCaughtEnemies.Count == 0)
		{
			return false;
		}
		foreach (CaughtEnemy deadCaughtEnemy in deadCaughtEnemies)
		{
			if (deadCaughtEnemy.original.GetInstanceID() == originalID)
			{
				puppet = deadCaughtEnemy.puppet;
				return true;
			}
		}
		return false;
	}

	public void ChangeAnimationState(bool open)
	{
		if (open)
		{
			if (closedModel.activeSelf)
			{
				anim.Play("Open", 0, 0f);
				openingModel.SetActive(value: true);
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, eid);
				if ((bool)gore)
				{
					gore.transform.position = base.transform.position + base.transform.up * 4f;
					if (eid.gz != null && eid.gz.goreZone != null)
					{
						gore.transform.SetParent(eid.gz.goreZone, worldPositionStays: true);
					}
					if (gore.TryGetComponent<Bloodsplatter>(out var component))
					{
						component.GetReady();
					}
				}
			}
			else
			{
				openModel.SetActive(value: true);
				GameObject[] array = effectsWhenOpen;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
			}
		}
		else
		{
			openingModel.SetActive(value: false);
			openModel.SetActive(value: false);
			GameObject[] array = effectsWhenOpen;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
		((Behaviour)(object)anim).enabled = open;
		closedModel.SetActive(!open);
	}

	public void Opened()
	{
		openingModel.SetActive(value: false);
		openModel.SetActive(value: true);
		GameObject[] array = effectsWhenOpen;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}
}
