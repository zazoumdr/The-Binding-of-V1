using System.Collections.Generic;
using System.Reflection;
using GameConsole.CommandTree;
using plog;
using plog.Models;

namespace GameConsole.Commands;

public class Rumble : CommandRoot, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Rumble");

	public override string Name => "Rumble";

	public override string Description => "Command for managing ULTRAKILL's controller rumble system";

	public Rumble(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("rumble", CommandRoot.Leaf("status", delegate
		{
			Log.Info($"Pending Vibrations ({MonoSingleton<RumbleManager>.Instance.pendingVibrations.Count}):", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (KeyValuePair<RumbleKey, PendingVibration> pendingVibration in MonoSingleton<RumbleManager>.Instance.pendingVibrations)
			{
				Log.Info($" - {pendingVibration.Key} ({pendingVibration.Value.Intensity}) for {pendingVibration.Value.Duration} seconds", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			Log.Info(string.Empty, (IEnumerable<Tag>)null, (string)null, (object)null);
			Log.Info($"Current Intensity: {MonoSingleton<RumbleManager>.Instance.currentIntensity}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("list", delegate
		{
			Log.Info("Available Keys:", (IEnumerable<Tag>)null, (string)null, (object)null);
			PropertyInfo[] properties = typeof(RumbleProperties).GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				string text = properties[i].GetValue(null) as string;
				Log.Info(" - " + text, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("vibrate", delegate(string key)
		{
			MonoSingleton<RumbleManager>.Instance.SetVibration(new RumbleKey(key));
		}), CommandRoot.Leaf("stop", delegate(string key)
		{
			MonoSingleton<RumbleManager>.Instance.StopVibration(new RumbleKey(key));
		}), CommandRoot.Leaf("stop_all", delegate
		{
			MonoSingleton<RumbleManager>.Instance.StopAllVibrations();
		}), CommandRoot.Leaf("toggle_preview", delegate
		{
			DebugUI.previewRumble = !DebugUI.previewRumble;
		}));
	}
}
