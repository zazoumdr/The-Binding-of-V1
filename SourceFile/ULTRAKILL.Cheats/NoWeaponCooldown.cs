namespace ULTRAKILL.Cheats;

public class NoWeaponCooldown : ICheat
{
	private static NoWeaponCooldown _lastInstance;

	public static bool NoCooldown
	{
		get
		{
			if (_lastInstance != null)
			{
				return _lastInstance.IsActive;
			}
			return false;
		}
	}

	public string LongName => "No Weapon Cooldown";

	public string Identifier => "ultrakill.no-weapon-cooldown";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "no-weapon-cooldown";

	public bool IsActive { get; private set; }

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		IsActive = true;
		_lastInstance = this;
	}

	public void Disable()
	{
		IsActive = false;
	}
}
