using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using plog;
using plog.Models;

namespace Logic;

public class VarStore
{
	private static readonly Logger Log = new Logger("VarStore");

	public HashSet<string> persistentKeys = new HashSet<string>();

	public Dictionary<string, int> intStore = new Dictionary<string, int>();

	public Dictionary<string, bool> boolStore = new Dictionary<string, bool>();

	public Dictionary<string, float> floatStore = new Dictionary<string, float>();

	public Dictionary<string, string> stringStore = new Dictionary<string, string>();

	public void Clear()
	{
		intStore.Clear();
		boolStore.Clear();
		floatStore.Clear();
		stringStore.Clear();
	}

	public VarStore DuplicateStore()
	{
		return new VarStore
		{
			intStore = new Dictionary<string, int>(intStore),
			boolStore = new Dictionary<string, bool>(boolStore),
			floatStore = new Dictionary<string, float>(floatStore),
			stringStore = new Dictionary<string, string>(stringStore)
		};
	}

	public static VarStore LoadPersistentStore()
	{
		VarStore varStore = new VarStore();
		string path = MapVarSaver.AssembleCurrentFilePath();
		if (!File.Exists(path))
		{
			return null;
		}
		string text = File.ReadAllText(path);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		PersistentSavedStore persistentSavedStore = JsonConvert.DeserializeObject<PersistentSavedStore>(text);
		if (persistentSavedStore == null)
		{
			return null;
		}
		if (persistentSavedStore.variables == null)
		{
			return null;
		}
		foreach (SavedVariable variable in persistentSavedStore.variables)
		{
			if (variable?.value != null && !string.IsNullOrEmpty(variable.value.type))
			{
				LoadVariable(variable, varStore);
				varStore.persistentKeys.Add(variable.name);
			}
		}
		return varStore;
	}

	public static void LoadVariable(SavedVariable variable, VarStore store)
	{
		Log.Info("Loading variable: $" + variable.name, (IEnumerable<Tag>)null, (string)null, (object)null);
		if (variable.value.type == "System.String")
		{
			if (variable.value.type == "System.String")
			{
				store.stringStore[variable.value.type] = variable.value.value.ToString();
			}
		}
		else if (variable.value.type == "System.Boolean")
		{
			if (variable.value.type == "System.Boolean")
			{
				store.boolStore[variable.value.type] = variable.value.value as bool? == true;
			}
		}
		else if (variable.value.type == "System.Int32")
		{
			if (variable.value.type == "System.Int32")
			{
				store.intStore[variable.value.type] = int.Parse(variable.value.value.ToString());
			}
		}
		else if (variable.value.type == "System.Single")
		{
			if (variable.value.type == "System.Single")
			{
				store.floatStore[variable.value.type] = float.Parse(variable.value.value.ToString());
			}
		}
		else
		{
			Log.Warning("Unknown variable type: " + variable.value.type + ", on variable: " + variable.name, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}
}
