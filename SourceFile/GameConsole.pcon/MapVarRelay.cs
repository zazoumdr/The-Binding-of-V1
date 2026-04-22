using Logic;
using pcon.core;
using pcon.core.Interfaces;

namespace GameConsole.pcon;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class MapVarRelay : MonoSingleton<MapVarRelay>
{
	private void Start()
	{
		MonoSingleton<MapVarManager>.Instance.RegisterGlobalWatcher(ReceiveChange);
	}

	private void ReceiveChange(string name, object value)
	{
		UpdateMapVars(MonoSingleton<MapVarManager>.Instance.Store);
	}

	public void UpdateMapVars(VarStore store)
	{
		PCon.SendMessage((ISend)(object)new MapVarsMessage(store));
	}
}
