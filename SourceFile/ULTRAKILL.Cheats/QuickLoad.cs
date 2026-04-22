namespace ULTRAKILL.Cheats;

public class QuickLoad : ICheat
{
	public string LongName => "Quick Load";

	public string Identifier => "ultrakill.sandbox.quick-load";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "LOAD LATEST SAVE";

	public string Icon => "quick-load";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		MonoSingleton<SandboxSaver>.Instance.QuickLoad();
	}

	public void Disable()
	{
		IsActive = false;
	}
}
