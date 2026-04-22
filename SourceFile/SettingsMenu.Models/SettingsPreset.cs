using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;

namespace SettingsMenu.Models;

[CreateAssetMenu(fileName = "SettingsPreset", menuName = "ULTRAKILL/Settings/Preset")]
public class SettingsPreset : ScriptableObject
{
	private static readonly Logger Log = new Logger("SettingsPreset");

	public PreferenceEntry[] preferences;

	public void Apply()
	{
		Log.Info("Applying settings preset " + base.name, (IEnumerable<Tag>)null, (string)null, (object)null);
		PreferenceEntry[] array = preferences;
		foreach (PreferenceEntry preferenceEntry in array)
		{
			Log.Info($"Applying preference {preferenceEntry}", (IEnumerable<Tag>)null, (string)null, (object)null);
			preferenceEntry.Apply();
		}
	}
}
