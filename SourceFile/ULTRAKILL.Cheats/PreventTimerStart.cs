namespace ULTRAKILL.Cheats;

public class PreventTimerStart : ICheat
{
	private static PreventTimerStart _lastInstance;

	public static bool Active => _lastInstance?.IsActive ?? false;

	public string LongName => "Prevent timer and leaderboard submit validation";

	public string Identifier => "ultrakill.debug.prevent-timer-start";

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
