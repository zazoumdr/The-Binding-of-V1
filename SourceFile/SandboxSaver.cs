using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using plog;
using plog.Models;
using Sandbox;
using UnityEngine;
using UnityEngine.SceneManagement;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SandboxSaver : MonoSingleton<SandboxSaver>
{
	private static readonly Logger Log = new Logger("SandboxSaver");

	public const string SaveExtension = ".pitr";

	[SerializeField]
	private SpawnableObjectsDatabase objects;

	private Dictionary<string, SpawnableObject> registeredObjects;

	public string activeSave;

	public static string SavePath => Path.Combine(GameProgressSaver.BaseSavePath, "Sandbox");

	private static void SetupDirs()
	{
		if (!Directory.Exists(SavePath))
		{
			Directory.CreateDirectory(SavePath);
		}
	}

	public string[] ListSaves()
	{
		SetupDirs();
		return (from f in new DirectoryInfo(SavePath).GetFileSystemInfos()
			orderby f.LastWriteTime descending
			select Path.GetFileNameWithoutExtension(f.Name)).ToArray();
	}

	public void QuickSave()
	{
		Save($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}");
	}

	public void QuickLoad()
	{
		string[] array = ListSaves();
		if (array.Length != 0)
		{
			Load(array[0]);
		}
	}

	public void Delete(string name)
	{
		SetupDirs();
		string path = Path.Combine(SavePath, name + ".pitr");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void Save(string name)
	{
		SetupDirs();
		activeSave = name;
		MonoSingleton<CheatsManager>.Instance.RefreshCheatStates();
		CreateSaveAndWrite(name);
	}

	public void Load(string name)
	{
		Log.Info("Loading save: " + name, (IEnumerable<Tag>)null, (string)null, (object)null);
		SetupDirs();
		Clear();
		activeSave = name;
		MonoSingleton<CheatsManager>.Instance.RefreshCheatStates();
		RebuildObjectList();
		SandboxSaveData sandboxSaveData = JsonConvert.DeserializeObject<SandboxSaveData>(File.ReadAllText(Path.Combine(SavePath, name + ".pitr")));
		Log.Fine($"Loaded {sandboxSaveData.Blocks.Length} blocks\nLoaded {sandboxSaveData.Props.Length} props", (IEnumerable<Tag>)null, (string)null, (object)null);
		Log.Fine("Save Version: " + sandboxSaveData.SaveVersion, (IEnumerable<Tag>)null, (string)null, (object)null);
		Vector3? vector = null;
		Vector3 position = MonoSingleton<NewMovement>.Instance.transform.position;
		SavedProp[] props = sandboxSaveData.Props;
		foreach (SavedProp savedProp in props)
		{
			RecreateProp(savedProp, sandboxSaveData.SaveVersion > 1);
			if (!(savedProp.ObjectIdentifier != "ultrakill.spawn-point"))
			{
				if (!vector.HasValue)
				{
					vector = savedProp.Position.ToVector3();
				}
				else if (Vector3.Distance(position, savedProp.Position.ToVector3()) < Vector3.Distance(position, vector.Value))
				{
					vector = savedProp.Position.ToVector3();
				}
			}
		}
		if (vector.HasValue)
		{
			MonoSingleton<NewMovement>.Instance.transform.position = vector.Value;
			MonoSingleton<NewMovement>.Instance.rb.velocity = Vector3.zero;
		}
		SavedBlock[] blocks = sandboxSaveData.Blocks;
		foreach (SavedBlock block in blocks)
		{
			RecreateBlock(block);
		}
		MonoSingleton<SandboxNavmesh>.Instance.Rebake();
		List<EnemySpawnableInstance> list = new List<EnemySpawnableInstance>();
		SavedEnemy[] enemies = sandboxSaveData.Enemies;
		foreach (SavedEnemy genericObject in enemies)
		{
			EnemySpawnableInstance enemySpawnableInstance = RecreateEnemy(genericObject, sandboxSaveData.SaveVersion > 1);
			enemySpawnableInstance.Pause(freeze: false);
			list.Add(enemySpawnableInstance);
		}
		StartCoroutine(PostLoadAndBake(list));
	}

	private IEnumerator PostLoadAndBake(List<EnemySpawnableInstance> enemies)
	{
		yield return new WaitForEndOfFrame();
		List<EnemySpawnableInstance> enemiesToFreezeBack = new List<EnemySpawnableInstance>();
		foreach (EnemySpawnableInstance enemy in enemies)
		{
			bool frozen = enemy.frozen;
			enemy.Resume();
			if (frozen)
			{
				enemiesToFreezeBack.Add(enemy);
			}
		}
		yield return new WaitForEndOfFrame();
		foreach (EnemySpawnableInstance item in enemiesToFreezeBack)
		{
			item.Pause();
		}
	}

	public EnemySpawnableInstance RecreateEnemy(SavedGeneric genericObject, bool newSizing)
	{
		if (!registeredObjects.TryGetValue(genericObject.ObjectIdentifier, out var value))
		{
			Log.Error(genericObject.ObjectIdentifier + " missing from registered objects", (IEnumerable<Tag>)null, (string)null, (object)null);
			return null;
		}
		return value.InstantiateSpawnable(genericObject, null, newSizing) as EnemySpawnableInstance;
	}

	private void RecreateProp(SavedProp prop, bool newSizing)
	{
		if (!registeredObjects.TryGetValue(prop.ObjectIdentifier, out var value))
		{
			Log.Error(prop.ObjectIdentifier + " missing from registered objects", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		else
		{
			value.InstantiateSpawnable(prop, null, newSizing);
		}
	}

	private void RecreateBlock(SavedBlock block)
	{
		if (!registeredObjects.TryGetValue(block.ObjectIdentifier, out var value))
		{
			Log.Error(block.ObjectIdentifier + " missing from registered objects", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		else
		{
			value.InstantiateSpawnable(block);
		}
	}

	public void RebuildObjectList()
	{
		if (registeredObjects == null)
		{
			registeredObjects = new Dictionary<string, SpawnableObject>();
		}
		registeredObjects.Clear();
		RegisterObjects(objects.objects);
		RegisterObjects(objects.enemies);
		RegisterObjects(objects.sandboxTools);
		RegisterObjects(objects.sandboxObjects);
		RegisterObjects(objects.specialSandbox);
	}

	private void RegisterObjects(SpawnableObject[] objs)
	{
		foreach (SpawnableObject spawnableObject in objs)
		{
			if (!string.IsNullOrEmpty(spawnableObject.identifier) && !registeredObjects.ContainsKey(spawnableObject.identifier))
			{
				registeredObjects.Add(spawnableObject.identifier, spawnableObject);
			}
		}
	}

	public static void Clear()
	{
		DefaultSandboxCheckpoint defaultSandboxCheckpoint = MonoSingleton<DefaultSandboxCheckpoint>.Instance;
		if (defaultSandboxCheckpoint == null)
		{
			MonoSingleton<StatsManager>.Instance.currentCheckPoint = null;
		}
		else
		{
			MonoSingleton<StatsManager>.Instance.currentCheckPoint = defaultSandboxCheckpoint.checkpoint;
		}
		SpawnableInstance[] array = UnityEngine.Object.FindObjectsOfType<SpawnableInstance>();
		foreach (SpawnableInstance spawnableInstance in array)
		{
			if (spawnableInstance.enabled)
			{
				UnityEngine.Object.Destroy(spawnableInstance.gameObject);
			}
		}
		Resources.UnloadUnusedAssets();
		MonoSingleton<SandboxNavmesh>.Instance.ResetSizeToDefault();
		MonoSingleton<SandboxSaver>.Instance.activeSave = null;
		MonoSingleton<CheatsManager>.Instance.RefreshCheatStates();
	}

	private static void CreateSaveAndWrite(string name)
	{
		Log.Info("Creating save", (IEnumerable<Tag>)null, (string)null, (object)null);
		SandboxProp[] array = UnityEngine.Object.FindObjectsOfType<SandboxProp>();
		Log.Fine($"{array.Length} props found", (IEnumerable<Tag>)null, (string)null, (object)null);
		BrushBlock[] array2 = UnityEngine.Object.FindObjectsOfType<BrushBlock>();
		Log.Fine($"{array2.Length} procedural blocks found", (IEnumerable<Tag>)null, (string)null, (object)null);
		EnemySpawnableInstance[] array3 = UnityEngine.Object.FindObjectsOfType<EnemySpawnableInstance>();
		Log.Fine($"{array3.Length} sandbox enemies found", (IEnumerable<Tag>)null, (string)null, (object)null);
		List<SavedBlock> list = new List<SavedBlock>();
		BrushBlock[] array4 = array2;
		foreach (BrushBlock brushBlock in array4)
		{
			if (brushBlock.enabled)
			{
				Log.Fine($"Position: {brushBlock.transform.position}\nRotation: {brushBlock.transform.rotation}\nSize: {brushBlock.DataSize}\nType: {brushBlock.Type}", (IEnumerable<Tag>)null, (string)null, (object)null);
				list.Add(brushBlock.SaveBrushBlock());
			}
		}
		List<SavedProp> list2 = new List<SavedProp>();
		SandboxProp[] array5 = array;
		foreach (SandboxProp sandboxProp in array5)
		{
			if (!sandboxProp.GetComponent<BrushBlock>() && sandboxProp.enabled)
			{
				Log.Fine($"Position: {sandboxProp.transform.position}\nRotation: {sandboxProp.transform.rotation}", (IEnumerable<Tag>)null, (string)null, (object)null);
				list2.Add(sandboxProp.SaveProp());
			}
		}
		List<SavedEnemy> list3 = new List<SavedEnemy>();
		EnemySpawnableInstance[] array6 = array3;
		foreach (EnemySpawnableInstance enemySpawnableInstance in array6)
		{
			if (enemySpawnableInstance.enabled && !(enemySpawnableInstance.eid.health < 0f) && !enemySpawnableInstance.eid.dead)
			{
				SavedEnemy savedEnemy = enemySpawnableInstance.SaveEnemy();
				if (savedEnemy != null)
				{
					list3.Add(savedEnemy);
				}
			}
		}
		string contents = JsonConvert.SerializeObject((object)new SandboxSaveData
		{
			MapName = SceneManager.GetActiveScene().name,
			Blocks = list.ToArray(),
			Props = list2.ToArray(),
			Enemies = list3.ToArray()
		});
		File.WriteAllText(Path.Combine(SavePath, name + ".pitr"), contents);
	}
}
