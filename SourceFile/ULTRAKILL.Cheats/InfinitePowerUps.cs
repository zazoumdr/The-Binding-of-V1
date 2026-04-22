namespace ULTRAKILL.Cheats;

public class InfinitePowerUps : ICheat
{
	private static InfinitePowerUps _lastInstance;

	public static bool Enabled => _lastInstance?.IsActive ?? false;

	public string LongName => "Infinite Power-Ups";

	public string Identifier => "ultrakill.infinite-power-ups";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "infinite-power-ups";

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
