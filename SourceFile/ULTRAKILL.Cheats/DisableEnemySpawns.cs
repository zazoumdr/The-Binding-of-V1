namespace ULTRAKILL.Cheats;

public class DisableEnemySpawns : ICheat
{
	private static DisableEnemySpawns _lastInstance;

	public static bool DisableArenaTriggers => _lastInstance?.IsActive ?? false;

	public string LongName => "Disable Enemy Spawns";

	public string Identifier => "ultrakill.disable-enemy-spawns";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "no-enemies";

	public bool IsActive { get; private set; }

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		_lastInstance = this;
		IsActive = true;
	}

	public void Disable()
	{
		IsActive = false;
	}
}
