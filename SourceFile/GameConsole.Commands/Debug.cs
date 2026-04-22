using System;
using System.Collections;
using System.Collections.Generic;
using GameConsole.CommandTree;
using plog;
using plog.Models;
using UnityEngine;

namespace GameConsole.Commands;

public class Debug : CommandRoot, IConsoleLogger
{
	public static bool AgonyDebugOverlay = true;

	public Logger Log { get; } = new Logger("Debug");

	public override string Name => "Debug";

	public override string Description => "Console debug stuff.";

	public Debug(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("debug", CommandRoot.Leaf("burst_print", delegate(string count)
		{
			con.StartCoroutine(BurstPrint(con, int.Parse(count), (Level)200));
		}), CommandRoot.Leaf("bulk_print", delegate(string count)
		{
			int num = int.Parse(count);
			for (int i = 0; i < num; i++)
			{
				Log.Info("Bulk print " + i, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("print_logger_test", delegate(string count)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			int num = int.Parse(count);
			for (int i = 0; i < num; i++)
			{
				new Logger(Guid.NewGuid().ToString()).Info("Bulk print " + i, (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("toggle_overlay", delegate
		{
			AgonyDebugOverlay = !AgonyDebugOverlay;
			Log.Info("AgonyDebugOverlay: " + AgonyDebugOverlay, (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("error", delegate
		{
			throw new Exception("Umm, ermm, guuh!!");
		}), CommandRoot.Leaf("log", delegate(string text)
		{
			UnityEngine.Debug.Log(text);
		}), CommandRoot.Leaf("freeze_game", delegate(string confrm)
		{
			if (confrm == "pretty_please")
			{
				while (true)
				{
				}
			}
			Log.Info("Usage: freeze_game pretty_please", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("timescale", delegate(string timescale)
		{
			Time.timeScale = float.Parse(timescale);
		}, requireCheats: true), CommandRoot.Leaf("die_respawn", delegate
		{
			Log.Info("Killing and immediately respawning player...", (IEnumerable<Tag>)null, (string)null, (object)null);
			bool paused = MonoSingleton<OptionsManager>.Instance.paused;
			if (paused)
			{
				MonoSingleton<OptionsManager>.Instance.UnPause();
			}
			con.StartCoroutine(KillRespawnDelayed(paused));
		}, requireCheats: true), CommandRoot.Leaf("total_secrets", delegate
		{
			Log.Info(GameProgressSaver.GetTotalSecretsFound().ToString(), (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("auto_register", delegate
		{
			Log.Info("Attempting to auto register all commands...", (IEnumerable<Tag>)null, (string)null, (object)null);
			List<ICommand> list = new List<ICommand>();
			Type[] types = typeof(ICommand).Assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!con.registeredCommandTypes.Contains(type) && typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface)
				{
					list.Add((ICommand)Activator.CreateInstance(type));
				}
			}
			con.RegisterCommands(list);
		}));
	}

	private IEnumerator BurstPrint(Console console, int count, Level type)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < count; i++)
		{
			Log.Record("Hello World " + i, type, (IEnumerable<Tag>)null, (string)null, (object)null);
			yield return new WaitForSecondsRealtime(3f / (float)count);
		}
	}

	private IEnumerator KillRespawnDelayed(bool wasPaused)
	{
		yield return new WaitForEndOfFrame();
		MonoSingleton<NewMovement>.Instance.GetHurt(999999, invincible: false, 1f, explosion: false, instablack: true);
		MonoSingleton<StatsManager>.Instance.Restart();
		if (wasPaused)
		{
			MonoSingleton<OptionsManager>.Instance.Pause();
		}
	}
}
