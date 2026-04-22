namespace ULTRAKILL.Cheats;

public class KeepEnabled : ICheat
{
	public string LongName => "Keep Cheats Enabled";

	public string Identifier => "ultrakill.keep-enabled";

	public string ButtonEnabledOverride => "STAY ACTIVE";

	public string ButtonDisabledOverride => "DISABLE ON RELOAD";

	public string Icon => "warning";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		IsActive = true;
	}

	public void Disable()
	{
		IsActive = false;
	}
}
