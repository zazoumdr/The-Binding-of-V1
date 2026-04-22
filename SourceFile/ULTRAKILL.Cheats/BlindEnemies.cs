namespace ULTRAKILL.Cheats;

public class BlindEnemies : ICheat
{
	private static BlindEnemies _lastInstance;

	public static bool Blind => _lastInstance?.IsActive ?? false;

	public string LongName => "Blind Enemies";

	public string Identifier => "ultrakill.blind-enemies";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "blind";

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
