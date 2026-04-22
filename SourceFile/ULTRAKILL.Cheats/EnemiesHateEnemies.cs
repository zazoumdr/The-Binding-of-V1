namespace ULTRAKILL.Cheats;

public class EnemiesHateEnemies : ICheat
{
	private static EnemiesHateEnemies _lastInstance;

	public static bool Active => _lastInstance?.IsActive ?? false;

	public string LongName => "Enemies Attack Each Other";

	public string Identifier => "ultrakill.enemy-hate-enemy";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "enemy-hate-enemy";

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
