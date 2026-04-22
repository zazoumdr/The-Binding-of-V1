namespace ULTRAKILL.Cheats;

public class InvincibleEnemies : ICheat
{
	private static InvincibleEnemies _lastInstance;

	public static bool Enabled => _lastInstance?.IsActive ?? false;

	public string LongName => "Invincible Enemies";

	public string Identifier => "ultrakill.invincible-enemies";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "invincible-enemies";

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
