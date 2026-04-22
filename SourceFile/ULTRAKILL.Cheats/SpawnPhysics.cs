namespace ULTRAKILL.Cheats;

public class SpawnPhysics : ICheat
{
	private static SpawnPhysics _lastInstance;

	public static bool PhysicsDynamic => _lastInstance?.IsActive ?? false;

	public string LongName => "Spawn With Physics";

	public string Identifier => "ultrakill.sandbox.physics";

	public string ButtonEnabledOverride => "DYNAMIC";

	public string ButtonDisabledOverride => "STATIC";

	public string Icon => "physics";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

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
