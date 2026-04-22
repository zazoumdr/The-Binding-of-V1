namespace ULTRAKILL.Cheats;

public class ClearMap : ICheat
{
	public string LongName => "Clear Map";

	public string Identifier => "ultrakill.sandbox.clear";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => "CLEAR";

	public string Icon => "delete";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		SandboxSaver.Clear();
	}

	public void Disable()
	{
		IsActive = false;
	}
}
