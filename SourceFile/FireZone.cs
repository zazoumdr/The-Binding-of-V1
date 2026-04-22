using UnityEngine;

public class FireZone : MonoBehaviour
{
	public HurtCooldownCollection HurtCooldownCollection;

	public FlameSource source;

	public bool canHurtPlayer = true;

	public int playerDamage = 20;

	private Streetcleaner sc;

	private void Start()
	{
		if (HurtCooldownCollection == null)
		{
			HurtCooldownCollection = new HurtCooldownCollection();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (source == FlameSource.None)
		{
			return;
		}
		float num = 1f;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = true;
		switch (source)
		{
		case FlameSource.Streetcleaner:
			if (sc == null)
			{
				sc = GetComponentInParent<Streetcleaner>();
			}
			if (!sc.damaging || sc.eid.target == null)
			{
				return;
			}
			flag = sc.eid.target.isPlayer;
			flag2 = !flag;
			num = sc.eid.totalDamageModifier;
			break;
		case FlameSource.Napalm:
			flag2 = true;
			break;
		}
		EnemyIdentifierIdentifier component;
		Flammable component2;
		if (other.gameObject.CompareTag("Player"))
		{
			if (canHurtPlayer && flag && HurtCooldownCollection.TryHurtCheckPlayer())
			{
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
				{
					MonoSingleton<PlatformerMovement>.Instance.Burn();
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.GetHurt((int)((float)playerDamage * num), invincible: true, (source == FlameSource.Napalm) ? 0f : 0.35f, explosion: false, instablack: false, (source == FlameSource.Napalm) ? 0f : 0.35f);
				}
			}
		}
		else if (other.TryGetComponent<EnemyIdentifierIdentifier>(out component) && component != null && component.eid != null)
		{
			if (flag2)
			{
				EnemyIdentifier eid = component.eid;
				if (HurtCooldownCollection.TryHurtCheckEnemy(eid))
				{
					eid.hitter = "fire";
					eid.DeliverDamage(other.gameObject, Vector3.zero, Vector3.zero, 1f, tryForExplode: false);
				}
			}
		}
		else if (other.TryGetComponent<Flammable>(out component2) && flag3 && HurtCooldownCollection.TryHurtCheckFlammable(component2))
		{
			component2.Burn(10f);
		}
	}
}
