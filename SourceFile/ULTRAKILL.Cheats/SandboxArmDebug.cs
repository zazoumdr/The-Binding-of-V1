namespace ULTRAKILL.Cheats;

public class SandboxArmDebug : ICheat
{
	private bool active;

	private static SandboxArmDebug _lastInstance;

	public static bool DebugActive
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

	public string LongName => "Sandbox Arm Debug";

	public string Identifier => "ultrakill.debug.sandbox-arm";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

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
