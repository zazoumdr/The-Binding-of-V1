using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;

namespace GameConsole.Commands;

public class Scene : ICommand, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Scene");

	public string Name => "Scene";

	public string Description => "Loads a scene.";

	public string Command => "scene";

	public void Execute(Console con, string[] args)
	{
		if (con.CheatBlocker())
		{
			return;
		}
		if (args.Length == 0)
		{
			Log.Info("Usage: scene <scene name>", (IEnumerable<Tag>)null, (string)null, (object)null);
			return;
		}
		string sceneName = string.Join(" ", args);
		if (!UnityEngine.Debug.isDebugBuild && MonoSingleton<SceneHelper>.Instance.IsSceneSpecial(sceneName))
		{
			Log.Info("Scene is special and cannot be loaded in release mode. \ud83e\udd7a", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		else
		{
			SceneHelper.LoadScene(sceneName);
		}
	}
}
