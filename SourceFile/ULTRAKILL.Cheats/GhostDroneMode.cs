namespace ULTRAKILL.Cheats;

public class GhostDroneMode : ICheat
{
	private static GhostDroneMode _lastInstance;

	private bool active;

	public static bool Enabled
	{
		get
		{
			if (_lastInstance != null)
			{
				return _lastInstance.active;
			}
			return false;
		}
	}

	public string LongName => "Drone Haunting";

	public string Identifier => "ultrakill.ghost-drone-mode";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "ghost";

	public bool IsActive => active;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		active = true;
		_lastInstance = this;
	}

	public void Disable()
	{
		active = false;
	}
}
