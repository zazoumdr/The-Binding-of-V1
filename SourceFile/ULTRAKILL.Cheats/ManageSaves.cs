namespace ULTRAKILL.Cheats;

public class ManageSaves : ICheat
{
	public string LongName => "Manage Saves";

	public string Identifier => "ultrakill.sandbox.save-menu";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "OPEN";

	public string Icon => "load";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		if (!GameStateManager.Instance.IsStateActive("sandbox-spawn-menu"))
		{
			MonoSingleton<CheatsManager>.Instance.ShowMenu();
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
		MonoSingleton<SandboxHud>.Instance.ShowSavesMenu();
	}

	public void Disable()
	{
		IsActive = false;
	}
}
