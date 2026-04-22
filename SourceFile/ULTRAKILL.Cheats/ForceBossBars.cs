namespace ULTRAKILL.Cheats;

public class ForceBossBars : ICheat
{
	private static ForceBossBars _lastInstance;

	public static bool Active
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

	public string LongName => "Force Enemy Boss Bars";

	public string Identifier => "ultrakill.debug.force-boss-bars";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

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
