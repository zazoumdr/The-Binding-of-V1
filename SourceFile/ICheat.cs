using System.Collections;

public interface ICheat
{
	string LongName { get; }

	string Identifier { get; }

	string ButtonEnabledOverride { get; }

	string ButtonDisabledOverride { get; }

	string Icon { get; }

	bool DefaultState { get; }

	StatePersistenceMode PersistenceMode { get; }

	bool IsActive { get; }

	void Enable(CheatsManager manager);

	void Disable();

	IEnumerator Coroutine(CheatsManager manager)
	{
		yield break;
	}
}
