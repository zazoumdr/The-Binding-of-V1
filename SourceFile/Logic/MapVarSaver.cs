using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Logic;

public class MapVarSaver
{
	public const string BuiltInPrefix = "ultrakill_";

	public static string MapVarDirectory => Path.Combine(GameProgressSaver.SavePath, "MapVars");

	public static string AssembleCurrentFilePath()
	{
		if (!Directory.Exists(MapVarDirectory))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!SceneHelper.IsPlayingCustom)
		{
			stringBuilder.Append("ultrakill_");
			stringBuilder.Append(SceneHelper.CurrentScene);
		}
		else
		{
			MapInfo instance = MapInfo.Instance;
			if (instance == null)
			{
				return null;
			}
			if (instance.uniqueId == null)
			{
				return null;
			}
			if (instance.uniqueId.StartsWith("ultrakill_"))
			{
				return null;
			}
			stringBuilder.Append(instance.uniqueId);
		}
		stringBuilder.Append(".vars.json");
		return Path.Combine(MapVarDirectory, stringBuilder.ToString());
	}

	public void WritePersistent(VarStore store)
	{
		List<SavedVariable> list = new List<SavedVariable>();
		foreach (string key in store.persistentKeys)
		{
			if (list.Any((SavedVariable var) => var.name == key))
			{
				return;
			}
			SavedValue value2;
			bool value3;
			float value4;
			if (store.intStore.TryGetValue(key, out var value))
			{
				value2 = new SavedValue
				{
					type = typeof(int).FullName,
					value = value.ToString()
				};
			}
			else if (store.boolStore.TryGetValue(key, out value3))
			{
				value2 = new SavedValue
				{
					type = typeof(bool).FullName,
					value = value3
				};
			}
			else if (store.floatStore.TryGetValue(key, out value4))
			{
				value2 = new SavedValue
				{
					type = typeof(float).FullName,
					value = value4.ToString(CultureInfo.InvariantCulture)
				};
			}
			else
			{
				if (!store.stringStore.TryGetValue(key, out var value5))
				{
					continue;
				}
				value2 = new SavedValue
				{
					type = typeof(string).FullName,
					value = value5
				};
			}
			list.Add(new SavedVariable
			{
				name = key,
				value = value2
			});
		}
		if (list.Count != 0)
		{
			if (!Directory.Exists(MapVarDirectory))
			{
				Directory.CreateDirectory(MapVarDirectory);
			}
			string text = AssembleCurrentFilePath();
			if (text != null)
			{
				string contents = JsonConvert.SerializeObject((object)new PersistentSavedStore
				{
					variables = list
				});
				File.WriteAllText(text, contents);
			}
		}
	}
}
