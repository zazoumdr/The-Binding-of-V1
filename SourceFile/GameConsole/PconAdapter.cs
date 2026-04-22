using System;
using System.Collections.Generic;
using System.Reflection;
using GameConsole.pcon;
using pcon.core;
using pcon.core.Models;
using plog;
using plog.Models;

namespace GameConsole;

public class PconAdapter
{
	private static readonly Logger Log = new Logger("PconAdapter");

	private Assembly pconAssmebly;

	private Type pconClientType;

	private bool startCalled;

	public bool PConLibraryExists()
	{
		if (pconAssmebly != null)
		{
			return true;
		}
		Log.Info("Looking for the pcon.unity library...", (IEnumerable<Tag>)null, (string)null, (object)null);
		string value = "pcon.unity";
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.StartsWith(value))
			{
				Log.Info("Found the pcon.unity library!", (IEnumerable<Tag>)null, (string)null, (object)null);
				pconAssmebly = assembly;
				pconClientType = pconAssmebly.GetType("pcon.PConClient");
				return true;
			}
		}
		return false;
	}

	public void StartPConClient(Action<string> onExecute, Action onGameModified)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		if (PConLibraryExists() && !startCalled)
		{
			Log.Info("Starting the pcon.unity client...", (IEnumerable<Tag>)null, (string)null, (object)null);
			startCalled = true;
			MethodInfo method = pconClientType.GetMethod("StartClient", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				Log.Info("Starting the pcon.unity client!", (IEnumerable<Tag>)null, (string)null, (object)null);
				PCon.MountHandler(new Handler
				{
					onExecute = onExecute,
					onGameModified = onGameModified
				});
				method.Invoke(null, new object[1]);
				MonoSingleton<MapVarRelay>.Instance.enabled = true;
				PCon.RegisterFeature("ultrakill");
			}
			else
			{
				Log.Info("Could not find the pcon.unity client's StartClient method!", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}
	}
}
