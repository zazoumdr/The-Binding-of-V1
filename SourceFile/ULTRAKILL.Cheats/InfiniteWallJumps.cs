using System.Collections;

namespace ULTRAKILL.Cheats;

public class InfiniteWallJumps : ICheat
{
	public string LongName => "Infinite Wall Jumps";

	public string Identifier => "ultrakill.infinite-wall-jumps";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "infinite-wall-jumps";

	public bool IsActive { get; private set; }

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		IsActive = true;
	}

	public void Disable()
	{
		IsActive = false;
	}

	public IEnumerator Coroutine(CheatsManager manager)
	{
		while (IsActive)
		{
			Update();
			yield return null;
		}
	}

	public void Update()
	{
		MonoSingleton<NewMovement>.Instance.currentWallJumps = 0;
	}
}
