namespace ULTRAKILL.Cheats;

public class GunControlDebug : ICheat
{
	private static GunControlDebug _lastInstance;

	public static bool GunControlActivated => _lastInstance?.IsActive ?? false;

	public string LongName => "Gun Control Debug";

	public string Identifier => "ultrakill.debug.gun-control";

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
