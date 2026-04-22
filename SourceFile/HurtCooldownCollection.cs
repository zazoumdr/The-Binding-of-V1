using System.Collections.Generic;
using plog;

public class HurtCooldownCollection
{
	private static readonly Logger Log = new Logger("HurtCooldownCollection");

	private const float HurtDelay = 0.5f;

	private readonly Dictionary<EnemyIdentifier, TimeSince> timeSinceHurtEnemies = new Dictionary<EnemyIdentifier, TimeSince>();

	private readonly Dictionary<Flammable, TimeSince> timeSinceHurtFlammables = new Dictionary<Flammable, TimeSince>();

	private TimeSince? timeSinceHurtPlayer;

	public bool TryHurtCheckEnemy(EnemyIdentifier eid, bool autoUpdate = true)
	{
		if (timeSinceHurtEnemies.TryGetValue(eid, out var value))
		{
			if ((float)value < 0.5f)
			{
				return false;
			}
			if (autoUpdate)
			{
				timeSinceHurtEnemies[eid] = 0f;
			}
		}
		else if (autoUpdate)
		{
			timeSinceHurtEnemies.Add(eid, 0f);
		}
		return true;
	}

	public void ResetEnemyCooldown(EnemyIdentifier eid)
	{
		timeSinceHurtEnemies.Remove(eid);
	}

	public bool TryHurtCheckPlayer(bool autoUpdate = true)
	{
		if (timeSinceHurtPlayer.HasValue)
		{
			if ((float?)timeSinceHurtPlayer < 0.5f)
			{
				return false;
			}
			if (autoUpdate)
			{
				timeSinceHurtPlayer = 0f;
			}
		}
		else if (autoUpdate)
		{
			timeSinceHurtPlayer = 0f;
		}
		return true;
	}

	public void ResetPlayerCooldown()
	{
		timeSinceHurtPlayer = null;
	}

	public bool TryHurtCheckFlammable(Flammable flammable, bool autoUpdate = true)
	{
		if (timeSinceHurtFlammables.TryGetValue(flammable, out var value))
		{
			if ((float)value < 0.5f)
			{
				return false;
			}
			if (autoUpdate)
			{
				timeSinceHurtFlammables[flammable] = 0f;
			}
		}
		else if (autoUpdate)
		{
			timeSinceHurtFlammables.Add(flammable, 0f);
		}
		return true;
	}

	public void ResetFlammableCooldown(Flammable flammable)
	{
		timeSinceHurtFlammables.Remove(flammable);
	}
}
