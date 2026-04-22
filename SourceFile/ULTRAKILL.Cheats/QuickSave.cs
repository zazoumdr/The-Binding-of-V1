namespace ULTRAKILL.Cheats;

public class QuickSave : ICheat
{
	public string LongName => "Quick Save";

	public string Identifier => "ultrakill.sandbox.quick-save";

	public string ButtonEnabledOverride => "SAVE";

	public string ButtonDisabledOverride => "NEW SAVE";

	public string Icon => "save";

	public bool IsActive
	{
		get
		{
			if (MonoSingleton<SandboxSaver>.Instance != null)
			{
				return !string.IsNullOrEmpty(MonoSingleton<SandboxSaver>.Instance.activeSave);
			}
			return false;
		}
	}

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		MonoSingleton<SandboxSaver>.Instance.QuickSave();
	}

	public void Disable()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("sandboxSaveOverwriteWarnings"))
		{
			MonoSingleton<SandboxSaveConfirmation>.Instance.DisplayDialog();
		}
		else
		{
			MonoSingleton<SandboxSaver>.Instance.Save(MonoSingleton<SandboxSaver>.Instance.activeSave);
		}
	}
}
