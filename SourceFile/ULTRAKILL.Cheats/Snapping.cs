namespace ULTRAKILL.Cheats;

public class Snapping : ICheat
{
	private static Snapping _lastInstance;

	public static bool SnappingEnabled => _lastInstance?.IsActive ?? false;

	public string LongName => "Snapping";

	public string Identifier => "ultrakill.sandbox.snapping";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "grid";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

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
