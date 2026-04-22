namespace ULTRAKILL.Cheats;

public class PauseTimedBombs : ICheat
{
	private static PauseTimedBombs _lastInstance;

	public static bool Paused
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

	public string LongName => "Pause Timed Bombs";

	public string Identifier => "ultrakill.pause-timed-bombs";

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
