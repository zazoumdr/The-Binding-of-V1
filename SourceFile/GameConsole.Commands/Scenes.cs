using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GameConsole.Commands;

public class Scenes : ICommand, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Scenes");

	public string Name => "Scenes";

	public string Description => "Lists all scenes.";

	public string Command => "scenes";

	public void Execute(Console con, string[] args)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (con.CheatBlocker())
		{
			return;
		}
		Log.Info("<b>Available Scenes:</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
		foreach (IResourceLocation item in Addressables.LoadResourceLocationsAsync((object)"Assets/Scenes", (Type)null).WaitForCompletion())
		{
			string text = item.InternalId;
			if (item.InternalId.StartsWith("Assets/Scenes/"))
			{
				text = item.InternalId.Substring(14);
			}
			if (item.InternalId.EndsWith(".unity"))
			{
				text = text.Substring(0, text.Length - 6);
			}
			Log.Info(text + " [<color=orange>" + item.InternalId + "</color>]", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}
}
