using UnityEngine;
using UnityEngine.Events;

public class OutOfBounds : MonoBehaviour
{
	public AffectedSubjects targets;

	private StatsManager sman;

	public Vector3 overrideResetPosition;

	public GameObject[] toActivate;

	public GameObject[] toDisactivate;

	public Door[] toUnlock;

	public UnityEvent toEvent;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && targets != AffectedSubjects.EnemiesOnly)
		{
			if ((MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.FPS || MonoSingleton<NewMovement>.Instance.hp <= 0) && (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer || MonoSingleton<PlatformerMovement>.Instance.dead))
			{
				return;
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
			}
			if (sman == null)
			{
				sman = MonoSingleton<StatsManager>.Instance;
			}
			if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
			{
				MonoSingleton<NewMovement>.Instance.ridingRocket.PlayerRideEnd();
			}
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Whoops, sorry about that.");
			if (overrideResetPosition != Vector3.zero)
			{
				other.transform.position = overrideResetPosition + Vector3.up * 1.25f;
			}
			else if (sman.currentCheckPoint != null)
			{
				other.transform.position = sman.currentCheckPoint.transform.position + Vector3.up * 1.25f;
				if (sman.currentCheckPoint.toActivate != null)
				{
					sman.currentCheckPoint.toActivate.SetActive(value: true);
				}
				Door[] doorsToUnlock = sman.currentCheckPoint.doorsToUnlock;
				foreach (Door door in doorsToUnlock)
				{
					if (door != null)
					{
						door.Unlock();
					}
				}
			}
			else
			{
				other.transform.position = sman.spawnPos;
				GameObject[] array = toActivate;
				foreach (GameObject gameObject in array)
				{
					if (gameObject != null)
					{
						gameObject.SetActive(value: true);
					}
				}
				array = toDisactivate;
				foreach (GameObject gameObject2 in array)
				{
					if (gameObject2 != null)
					{
						gameObject2.SetActive(value: true);
					}
				}
				Door[] doorsToUnlock = toUnlock;
				foreach (Door door2 in doorsToUnlock)
				{
					if (door2 != null)
					{
						door2.Unlock();
					}
				}
			}
			toEvent?.Invoke();
		}
		else
		{
			if (targets == AffectedSubjects.PlayerOnly)
			{
				return;
			}
			if (other.gameObject.layer == 10 || other.gameObject.layer == 9)
			{
				if (other.gameObject.layer == 10)
				{
					EnemyIdentifier componentInParent = other.gameObject.GetComponentInParent<EnemyIdentifier>();
					if ((bool)componentInParent && !componentInParent.dead)
					{
						return;
					}
					if (!componentInParent && other.gameObject.CompareTag("Coin"))
					{
						other.GetComponent<Coin>()?.GetDeleted();
						return;
					}
				}
				other.gameObject.SetActive(value: false);
				other.transform.position = Vector3.zero;
				other.transform.localScale = Vector3.zero;
			}
			else
			{
				if (!other.gameObject.CompareTag("Enemy") || !(GetComponentInChildren<DeathZone>() == null))
				{
					return;
				}
				EnemyIdentifier component = other.gameObject.GetComponent<EnemyIdentifier>();
				if (!component.dead)
				{
					if (component.specialOob)
					{
						component.SendMessage("OutOfBounds", SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						component.InstaKill();
					}
				}
			}
		}
	}
}
