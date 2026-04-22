namespace ULTRAKILL.Cheats;

public class EnemyIdentifierDebug : ICheat
{
	private static EnemyIdentifierDebug _lastInstance;

	public static bool Active => _lastInstance?.IsActive ?? false;

	public string LongName => "Enemy Identifier Debug";

	public string Identifier => "ultrakill.debug.enemy-identifier";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => null;

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public bool IsActive { get; private set; }

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
