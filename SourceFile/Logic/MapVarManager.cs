using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Logic;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class MapVarManager : MonoSingleton<MapVarManager>
{
	private static readonly Logger Log = new Logger("MapVarManager");

	private VarStore currentStore = new VarStore();

	private VarStore stashedStore;

	private MapVarSaver saver = new MapVarSaver();

	private readonly Dictionary<string, List<UnityAction<int>>> intSubscribers = new Dictionary<string, List<UnityAction<int>>>();

	private readonly Dictionary<string, List<UnityAction<bool>>> boolSubscribers = new Dictionary<string, List<UnityAction<bool>>>();

	private readonly Dictionary<string, List<UnityAction<float>>> floatSubscribers = new Dictionary<string, List<UnityAction<float>>>();

	private readonly Dictionary<string, List<UnityAction<string>>> stringSubscribers = new Dictionary<string, List<UnityAction<string>>>();

	private readonly List<UnityAction<string, object>> globalSubscribers = new List<UnityAction<string, object>>();

	public static bool LoggingEnabled = false;

	public VarStore Store => currentStore;

	public bool HasStashedStore => stashedStore != null;

	private void Start()
	{
		if (Debug.isDebugBuild)
		{
			LoggingEnabled = true;
		}
	}

	public void ReloadMapVars()
	{
		ResetStores();
		RestorePersistent();
	}

	public void ResetStores()
	{
		if (LoggingEnabled)
		{
			Log.Info("Resetting MapVar stores", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		currentStore.Clear();
		stashedStore = null;
		intSubscribers.Clear();
		boolSubscribers.Clear();
		floatSubscribers.Clear();
		stringSubscribers.Clear();
		LoggingEnabled = false;
	}

	private void RestorePersistent()
	{
		VarStore varStore = VarStore.LoadPersistentStore();
		if (varStore != null)
		{
			currentStore = varStore;
		}
	}

	public void StashStore()
	{
		if (currentStore.intStore.Count == 0 && currentStore.boolStore.Count == 0 && currentStore.floatStore.Count == 0 && currentStore.stringStore.Count == 0)
		{
			stashedStore = null;
			return;
		}
		stashedStore = currentStore.DuplicateStore();
		if (LoggingEnabled)
		{
			Log.Info("Stashed MapVar stores", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}

	public void RestoreStashedStore()
	{
		if (stashedStore == null)
		{
			if (LoggingEnabled)
			{
				Log.Info("No stashed store to restore", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			return;
		}
		currentStore = stashedStore.DuplicateStore();
		if (LoggingEnabled)
		{
			Log.Info("Restored MapVar stores", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}

	public void RegisterIntWatcher(string key, UnityAction<int> callback)
	{
		if (LoggingEnabled)
		{
			Log.Info("Registering int watcher for " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (!intSubscribers.ContainsKey(key))
		{
			intSubscribers.Add(key, new List<UnityAction<int>>());
		}
		intSubscribers[key].Add(callback);
	}

	public void RegisterBoolWatcher(string key, UnityAction<bool> callback)
	{
		if (LoggingEnabled)
		{
			Log.Info("Registering bool watcher for " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (!boolSubscribers.ContainsKey(key))
		{
			boolSubscribers.Add(key, new List<UnityAction<bool>>());
		}
		boolSubscribers[key].Add(callback);
	}

	public void RegisterFloatWatcher(string key, UnityAction<float> callback)
	{
		if (LoggingEnabled)
		{
			Log.Info("Registering float watcher for " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (!floatSubscribers.ContainsKey(key))
		{
			floatSubscribers.Add(key, new List<UnityAction<float>>());
		}
		floatSubscribers[key].Add(callback);
	}

	public void RegisterStringWatcher(string key, UnityAction<string> callback)
	{
		if (LoggingEnabled)
		{
			Log.Info("Registering string watcher for " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (!stringSubscribers.ContainsKey(key))
		{
			stringSubscribers.Add(key, new List<UnityAction<string>>());
		}
		stringSubscribers[key].Add(callback);
	}

	public void RegisterGlobalWatcher(UnityAction<string, object> callback)
	{
		if (LoggingEnabled)
		{
			Log.Info("Registering global watcher", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		globalSubscribers.Add(callback);
	}

	public void SetInt(string key, int value, bool persistent = false)
	{
		currentStore.intStore[key] = value;
		if (intSubscribers.ContainsKey(key))
		{
			if (LoggingEnabled)
			{
				Log.Info(string.Format("Notifying {0} int watcher{1} for {2}", intSubscribers[key].Count, (intSubscribers[key].Count == 1) ? "" : "s", key), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<int> item in intSubscribers[key])
			{
				item?.Invoke(value);
			}
		}
		if (globalSubscribers.Count <= 0)
		{
			return;
		}
		if (LoggingEnabled)
		{
			Log.Info(string.Format("Notifying {0} global watcher{1} for {2}", globalSubscribers.Count, (globalSubscribers.Count == 1) ? "" : "s", key), (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		foreach (UnityAction<string, object> globalSubscriber in globalSubscribers)
		{
			globalSubscriber?.Invoke(key, value);
		}
	}

	public void AddInt(string key, int value, bool persistent = false)
	{
		int valueOrDefault = GetInt(key).GetValueOrDefault();
		SetInt(key, valueOrDefault + value, persistent);
	}

	public void SetBool(string key, bool value, bool persistent = false)
	{
		if (LoggingEnabled)
		{
			Log.Info($"SetBool: {key} - {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		currentStore.boolStore[key] = value;
		if (boolSubscribers.ContainsKey(key))
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {boolSubscribers[key].Count} bool watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<bool> item in boolSubscribers[key])
			{
				item?.Invoke(value);
			}
		}
		if (globalSubscribers.Count > 0)
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {globalSubscribers.Count} global watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<string, object> globalSubscriber in globalSubscribers)
			{
				globalSubscriber?.Invoke(key, value);
			}
		}
		if (persistent)
		{
			currentStore.persistentKeys.Add(key);
			saver.WritePersistent(currentStore);
		}
		else
		{
			currentStore.persistentKeys.Remove(key);
		}
	}

	public void SetFloat(string key, float value, bool persistent = false)
	{
		if (LoggingEnabled)
		{
			Log.Info($"SetFloat: {key} - {value}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		currentStore.floatStore[key] = value;
		if (floatSubscribers.ContainsKey(key))
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {floatSubscribers[key].Count} float watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<float> item in floatSubscribers[key])
			{
				item?.Invoke(value);
			}
		}
		if (globalSubscribers.Count > 0)
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {globalSubscribers.Count} global watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<string, object> globalSubscriber in globalSubscribers)
			{
				globalSubscriber?.Invoke(key, value);
			}
		}
		if (persistent)
		{
			currentStore.persistentKeys.Add(key);
			saver.WritePersistent(currentStore);
		}
		else
		{
			currentStore.persistentKeys.Remove(key);
		}
	}

	public void SetString(string key, string value, bool persistent = false)
	{
		if (LoggingEnabled)
		{
			Log.Info("SetString: " + key + " - " + value, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		currentStore.stringStore[key] = value;
		if (stringSubscribers.ContainsKey(key))
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {stringSubscribers[key].Count} string watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<string> item in stringSubscribers[key])
			{
				item?.Invoke(value);
			}
		}
		if (globalSubscribers.Count > 0)
		{
			if (LoggingEnabled)
			{
				Log.Info($"Notifying {globalSubscribers.Count} global watchers for {key}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			foreach (UnityAction<string, object> globalSubscriber in globalSubscribers)
			{
				globalSubscriber?.Invoke(key, value);
			}
		}
		if (persistent)
		{
			currentStore.persistentKeys.Add(key);
			saver.WritePersistent(currentStore);
		}
		else
		{
			currentStore.persistentKeys.Remove(key);
		}
	}

	public int? GetInt(string key)
	{
		if (currentStore.intStore.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public bool? GetBool(string key)
	{
		if (currentStore.boolStore.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public float? GetFloat(string key)
	{
		if (currentStore.floatStore.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetString(string key)
	{
		if (currentStore.stringStore.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public List<VariableSnapshot> GetAllVariables()
	{
		List<VariableSnapshot> list = new List<VariableSnapshot>();
		foreach (KeyValuePair<string, int> item in currentStore.intStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(int),
				name = item.Key,
				value = item.Value
			});
		}
		foreach (KeyValuePair<string, bool> item2 in currentStore.boolStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(bool),
				name = item2.Key,
				value = item2.Value
			});
		}
		foreach (KeyValuePair<string, float> item3 in currentStore.floatStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(float),
				name = item3.Key,
				value = item3.Value
			});
		}
		foreach (KeyValuePair<string, string> item4 in currentStore.stringStore)
		{
			list.Add(new VariableSnapshot
			{
				type = typeof(string),
				name = item4.Key,
				value = item4.Value
			});
		}
		return list;
	}
}
