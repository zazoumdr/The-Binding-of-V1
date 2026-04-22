using System.Collections.Generic;
using System.Linq;
using GameConsole.CommandTree;
using plog;
using plog.Models;
using UnityEngine.InputSystem;

namespace GameConsole.Commands;

public class ConsoleCmd(Console con) : CommandRoot(con), IConsoleLogger
{
	public Logger Log { get; } = new Logger("ConsoleCmd");

	public override string Name => "Console";

	public override string Description => "Used for configuring the console";

	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("console", BoolMenu("hide_badge", () => con.errorBadge.hidden, delegate(bool value)
		{
			con.errorBadge.SetEnabled(value);
		}), BoolMenu("force_stacktrace_extraction", () => con.ExtractStackTraces, con.SetForceStackTraceExtraction), CommandRoot.Leaf("change_bind", delegate(string bind, string key)
		{
			if (con.binds.defaultBinds.ContainsKey(bind.ToLower()))
			{
				con.binds.Rebind(bind.ToLower(), key);
			}
			else
			{
				Log.Error(bind.ToLower() + " is not a valid bind.", (IEnumerable<Tag>)null, (string)null, (object)null);
				Log.Info("Listing valid binds:", (IEnumerable<Tag>)null, (string)null, (object)null);
				ListDefaults(con);
			}
		}, requireCheats: true), CommandRoot.Leaf("list_binds", delegate
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			Log.Info("Listing binds:", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (KeyValuePair<string, InputActionState> registeredBind in con.binds.registeredBinds)
			{
				Logger log = Log;
				string key = registeredBind.Key;
				InputBinding val = ((IEnumerable<InputBinding>)(object)registeredBind.Value.Action.bindings).First();
				log.Info(key + "  -  " + ((InputBinding)(ref val)).path, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("reset", delegate
		{
			MonoSingleton<Console>.Instance.consoleWindow.ResetWindow();
		}));
	}

	private void ListDefaults(Console con)
	{
		foreach (KeyValuePair<string, string> defaultBind in con.binds.defaultBinds)
		{
			Log.Info(defaultBind.Key + "  -  " + defaultBind.Value, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}
}
