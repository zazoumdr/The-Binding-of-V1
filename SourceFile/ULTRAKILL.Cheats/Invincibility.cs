namespace ULTRAKILL.Cheats;

public class Invincibility : ICheat
{
	private static Invincibility _lastInstance;

	public static bool Enabled => _lastInstance?.IsActive ?? false;

	public string LongName => "Invincibility";

	public string Identifier => "ultrakill.invincibility";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "invincibility";

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
