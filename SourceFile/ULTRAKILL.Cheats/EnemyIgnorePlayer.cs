namespace ULTRAKILL.Cheats;

public class EnemyIgnorePlayer : ICheat
{
	private static EnemyIgnorePlayer _lastInstance;

	private bool active;

	public static bool Active => _lastInstance?.active ?? false;

	public string LongName => "Enemies Ignore Player";

	public string Identifier => "ultrakill.enemy-ignore-player";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "enemy-ignore-player";

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
