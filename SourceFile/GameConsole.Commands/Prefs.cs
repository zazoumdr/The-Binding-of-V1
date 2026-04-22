using System.Collections.Generic;
using GameConsole.CommandTree;
using plog;
using plog.Models;

namespace GameConsole.Commands;

public class Prefs : CommandRoot, IConsoleLogger
{
	public Logger Log { get; } = new Logger("Prefs");

	public override string Name => "Prefs";

	public override string Description => "Interfaces with the PrefsManager.";

	public Prefs(Console con)
		: base(con)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	protected override Branch BuildTree(Console con)
	{
		return CommandRoot.Branch("prefs", CommandRoot.Branch("get", CommandRoot.Leaf("bool", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetBool(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("int", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetInt(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("float", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetFloat(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("string", delegate(string key)
		{
			Log.Info(key + " = " + MonoSingleton<PrefsManager>.Instance.GetString(key), (IEnumerable<Tag>)null, (string)null, (object)null);
		})), CommandRoot.Branch("set", CommandRoot.Leaf("bool", delegate(string key, bool value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetBool(key, value);
		}), CommandRoot.Leaf("int", delegate(string key, int value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetInt(key, value);
		}), CommandRoot.Leaf("float", delegate(string key, float value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetFloat(key, value);
		}), CommandRoot.Leaf("string", delegate(string key, string value)
		{
			Log.Info("Set " + key + " to " + value, (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetString(key, value);
		})), CommandRoot.Branch("get_local", CommandRoot.Leaf("bool", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetBoolLocal(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("int", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetIntLocal(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("float", delegate(string key)
		{
			Log.Info($"{key} = {MonoSingleton<PrefsManager>.Instance.GetFloatLocal(key)}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}), CommandRoot.Leaf("string", delegate(string key)
		{
			Log.Info(key + " = " + MonoSingleton<PrefsManager>.Instance.GetStringLocal(key), (IEnumerable<Tag>)null, (string)null, (object)null);
		})), CommandRoot.Branch("set_local", CommandRoot.Leaf("bool", delegate(string key, bool value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal(key, value);
		}), CommandRoot.Leaf("int", delegate(string key, int value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetIntLocal(key, value);
		}), CommandRoot.Leaf("float", delegate(string key, float value)
		{
			Log.Info($"Set {key} to {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal(key, value);
		}), CommandRoot.Leaf("string", delegate(string key, string value)
		{
			Log.Info("Set " + key + " to " + value, (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.SetStringLocal(key, value);
		})), CommandRoot.Leaf("delete", delegate(string key)
		{
			Log.Info("Deleted " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<PrefsManager>.Instance.DeleteKey(key);
		}), CommandRoot.Leaf("list_defaults", delegate
		{
			Log.Info("<b>Default Prefs:</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (KeyValuePair<string, object> defaultValue in MonoSingleton<PrefsManager>.Instance.defaultValues)
			{
				Log.Info($"{defaultValue.Key} = {defaultValue.Value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("list_cached", delegate
		{
			Log.Info("<b>Cached Prefs:</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (KeyValuePair<string, object> item in MonoSingleton<PrefsManager>.Instance.prefMap)
			{
				Log.Info($"{item.Key} = {item.Value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("list_cached_local", delegate
		{
			Log.Info("<b>Local Cached Prefs:</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
			foreach (KeyValuePair<string, object> item2 in MonoSingleton<PrefsManager>.Instance.localPrefMap)
			{
				Log.Info($"{item2.Key} = {item2.Value}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}), CommandRoot.Leaf("last_played", delegate
		{
			Log.Info($"The game has been played {PrefsManager.monthsSinceLastPlayed} months ago last.\nThis is only valid per session.", (IEnumerable<Tag>)null, (string)null, (object)null);
		}));
	}
}
