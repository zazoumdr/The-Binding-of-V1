using System;
using System.Collections.Generic;
using GameConsole.CommandTree;
using plog;
using plog.Models;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace GameConsole.Commands;

internal class InputCommands : CommandRoot, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Input");

	public override string Name => "Input";

	public override string Description => "Modify inputs";

	public InputCommands(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("input", CommandRoot.Branch("mouse", CommandRoot.Leaf("sensitivity", delegate(float amount)
		{
			Log.Info($"Set mouse sensitivity to {amount}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal("mouseSensitivity", amount);
		})), CommandRoot.Leaf("bindings", delegate(string name)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			InputAction val = MonoSingleton<InputManager>.Instance.InputSource.Actions.FindAction(name);
			if (val == null)
			{
				Log.Error("No action found with name or id '" + name + "'", (IEnumerable<Tag>)null, (string)null, (object)null);
				return;
			}
			Log.Info("'" + name + "' has the following bindings:", (IEnumerable<Tag>)null, (string)null, (object)null);
			Enumerator<InputBinding> enumerator = val.bindings.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					InputBinding current = enumerator.Current;
					if (((InputBinding)(ref current)).isPartOfComposite)
					{
						Log.Info("-- " + ((InputBinding)(ref current)).path, (IEnumerable<Tag>)null, (string)null, (object)null);
					}
					else
					{
						Log.Info("- " + ((InputBinding)(ref current)).path, (IEnumerable<Tag>)null, (string)null, (object)null);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
		}));
	}
}
