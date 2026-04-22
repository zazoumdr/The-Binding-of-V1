using System;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
	private NewMovement pm;

	public GameObject sawSound;

	public string deathType;

	public bool dontExplode;

	public bool splatter;

	public bool enemiesCanDodge;

	public bool teleportingEnemiesReturnToOrigin;

	public bool aliveOnly;

	public bool deleteLimbs;

	public bool requireMatchingUpDirection;

	public AffectedSubjects affected;

	private bool playerAffected;

	private bool enemyAffected;

	public bool checkForPlayerOutsideTrigger;

	[Space(10f)]
	public bool notInstakill;

	public Vector3 respawnTarget;

	public bool dontChangeRespawnTarget;

	public int damage = 50;

	public int styleAmount = 80;

	private Transform player;

	public EnemyType[] unaffectedEnemyTypes;

	private List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();

	private List<EnemyIdentifier> noLongerHitEnemies = new List<EnemyIdentifier>();

	public UltrakillEvent onHitPlayer;

	private void Start()
	{
		if (unaffectedEnemyTypes == null)
		{
			unaffectedEnemyTypes = Array.Empty<EnemyType>();
		}
		player = MonoSingleton<NewMovement>.Instance.transform;
		switch (affected)
		{
		case AffectedSubjects.All:
			enemyAffected = true;
			playerAffected = true;
			break;
		case AffectedSubjects.EnemiesOnly:
			enemyAffected = true;
			playerAffected = false;
			break;
		case AffectedSubjects.PlayerOnly:
			enemyAffected = false;
			playerAffected = true;
			break;
		}
		Invoke("SlowUpdate", 1f);
	}

	private void SlowUpdate()
	{
		if (base.gameObject.activeInHierarchy && checkForPlayerOutsideTrigger && player.transform.position.y < base.transform.position.y)
		{
			GotHit(player.GetComponent<Collider>());
		}
		Invoke("SlowUpdate", 1f);
	}

	private void OnTriggerEnter(Collider other)
	{
		GotHit(other);
	}

	private void OnCollisionEnter(Collision collision)
	{
		GotHit(collision.collider);
	}

	private void GotHit(Collider other)
	{
		if (other.gameObject.layer == 20 && (bool)other.transform.parent && other.transform.parent != MonoSingleton<NewMovement>.Instance.transform && other.transform.parent.TryGetComponent<Collider>(out var component))
		{
			other = component;
		}
		IgnoreDeathZones component2;
		if (other.gameObject.CompareTag("Player") && playerAffected)
		{
			if (requireMatchingUpDirection)
			{
				if (MonoSingleton<NewMovement>.Instance.rb.TryGetCustomGravity(out var gravity))
				{
					if (Vector3.Dot(gravity, base.transform.up * -1f) < 0.33f)
					{
						return;
					}
				}
				else if (Vector3.Dot(Vector3.up, base.transform.up) < 0.33f)
				{
					return;
				}
			}
			onHitPlayer?.Invoke();
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				if (dontExplode || deathType.ToLower() == "fall")
				{
					MonoSingleton<PlatformerMovement>.Instance.Fall();
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode(ignoreInvincible: true);
				}
				return;
			}
			if (!notInstakill)
			{
				if (pm == null)
				{
					pm = other.GetComponent<NewMovement>();
				}
				pm.GetHurt(999999, invincible: false, 1f, explosion: false, instablack: true);
				if (sawSound != null)
				{
					UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
				}
				base.enabled = false;
				return;
			}
			if (pm == null)
			{
				pm = other.GetComponent<NewMovement>();
			}
			if (pm.hp > 0)
			{
				if (damage == 0 || pm.hp == 1)
				{
					pm.FakeHurt();
				}
				else if (pm.hp > damage)
				{
					pm.GetHurt(damage, invincible: true);
				}
				else if (pm.hp > 1)
				{
					pm.GetHurt(pm.hp - 1, invincible: true);
				}
				if (sawSound != null)
				{
					UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
				}
				other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				StatsManager instance = MonoSingleton<StatsManager>.Instance;
				_ = Vector3.zero;
				if (respawnTarget != Vector3.zero)
				{
					other.transform.position = respawnTarget + base.transform.up * 1.25f;
				}
				else if (instance.currentCheckPoint != null)
				{
					other.transform.position = instance.currentCheckPoint.transform.position + instance.currentCheckPoint.transform.up * 1.25f;
				}
				else
				{
					other.transform.position = instance.spawnPos;
				}
				if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance2))
				{
					instance2.UpdateTraveller(MonoSingleton<NewMovement>.Instance);
				}
			}
		}
		else if ((other.gameObject.CompareTag("Enemy") || other.gameObject.layer == 10 || other.gameObject.layer == 11) && enemyAffected && !other.TryGetComponent<IgnoreDeathZones>(out component2))
		{
			EnemyIdentifier enemyIdentifier = other.gameObject.GetComponentInParent<EnemyIdentifier>();
			if (enemyIdentifier == null)
			{
				EnemyIdentifierIdentifier component3 = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if (component3 != null)
				{
					if (component3.eid != null)
					{
						enemyIdentifier = component3.eid;
					}
					else
					{
						UnityEngine.Object.Destroy(component3.gameObject);
					}
				}
			}
			if (!(enemyIdentifier != null) || alreadyHitEnemies.Contains(enemyIdentifier))
			{
				return;
			}
			alreadyHitEnemies.Add(enemyIdentifier);
			if (unaffectedEnemyTypes.Length != 0)
			{
				for (int i = 0; i < unaffectedEnemyTypes.Length; i++)
				{
					if (unaffectedEnemyTypes[i] == enemyIdentifier.enemyType)
					{
						return;
					}
				}
			}
			if ((!dontExplode && !aliveOnly && !enemyIdentifier.exploded) || !enemyIdentifier.dead)
			{
				if (sawSound != null)
				{
					UnityEngine.Object.Instantiate(sawSound, other.transform.position, Quaternion.identity);
				}
				enemyIdentifier.hitter = "deathzone";
				StyleHUD instance3 = MonoSingleton<StyleHUD>.Instance;
				if ((bool)instance3 && !enemyIdentifier.puppet)
				{
					if (!enemyIdentifier.dead)
					{
						instance3.AddPoints(styleAmount, deathType, null, enemyIdentifier);
					}
					else
					{
						instance3.AddPoints(styleAmount / 4, (deathType == "") ? "" : ("<color=grey>" + deathType + "</color>"), null, enemyIdentifier);
					}
				}
				if (enemiesCanDodge)
				{
					EnemyIdentifier.FallOnEnemy(enemyIdentifier, returnToOrigin: true);
					if (!enemyIdentifier.dead)
					{
						noLongerHitEnemies.Add(enemyIdentifier);
						CancelInvoke("RemoveNoLongerHitEnemies");
						Invoke("RemoveNoLongerHitEnemies", 1f);
					}
				}
				else if (splatter)
				{
					enemyIdentifier.Splatter(styleBonus: false);
				}
				else if (!dontExplode)
				{
					enemyIdentifier.Explode();
					if (enemyIdentifier.enemyType == EnemyType.Gutterman && enemyIdentifier.TryGetComponent<Gutterman>(out var component4))
					{
						component4.Explode();
					}
				}
				else
				{
					enemyIdentifier.InstaKill();
				}
			}
			if (deleteLimbs && enemyIdentifier.dead && !enemyIdentifier.DestroyLimb(other.transform))
			{
				other.gameObject.SetActive(value: false);
				other.transform.position = new Vector3(-100f, -100f, -100f);
				other.transform.localScale = Vector3.zero;
			}
		}
		else if (other.gameObject.CompareTag("Coin"))
		{
			other.GetComponent<Coin>()?.GetDeleted();
		}
	}

	private void RemoveNoLongerHitEnemies()
	{
		if (noLongerHitEnemies == null || noLongerHitEnemies.Count == 0)
		{
			return;
		}
		for (int num = noLongerHitEnemies.Count - 1; num >= 0; num--)
		{
			if (alreadyHitEnemies.Contains(noLongerHitEnemies[num]))
			{
				alreadyHitEnemies.Remove(noLongerHitEnemies[num]);
			}
			noLongerHitEnemies.RemoveAt(num);
		}
	}
}
