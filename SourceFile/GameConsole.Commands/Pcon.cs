using GameConsole.CommandTree;

namespace GameConsole.Commands;

public class Pcon : CommandRoot
{
	public override string Name => "pcon";

	public override string Description => "pcon commands";

	public Pcon(Console con)
		: base(con)
	{
	}

	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("pcon", CommandRoot.Leaf("connect", delegate
		{
			MonoSingleton<Console>.Instance.StartPCon();
		}), BoolMenu("autostart", () => MonoSingleton<PrefsManager>.Instance.GetBoolLocal("pcon.autostart"), delegate(bool value)
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("pcon.autostart", value);
			if (value)
			{
				MonoSingleton<Console>.Instance.StartPCon();
			}
		}));
	}
}
