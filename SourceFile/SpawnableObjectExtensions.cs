using System;
using System.Collections.Generic;
using System.Linq;
using plog;
using plog.Models;
using Sandbox;
using UnityEngine;

public static class SpawnableObjectExtensions
{
	private static readonly Logger Log = new Logger("SpawnableObjectExtensions");

	public static SpawnableInstance InstantiateSpawnable(this SpawnableObject spawnable, SavedGeneric saved, Transform parent = null, bool newSizing = true)
	{
		GameObject gameObject;
		bool flag;
		if (saved is SavedBlock savedBlock)
		{
			gameObject = SandboxUtils.CreateFinalBlock(spawnable, savedBlock.Position.ToVector3(), savedBlock.BlockSize.ToVector3(), spawnable.isWater);
			flag = true;
			if (parent != null)
			{
				gameObject.transform.SetParent(parent, worldPositionStays: true);
			}
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(spawnable.gameObject, parent, worldPositionStays: true);
			flag = false;
		}
		gameObject.transform.position = saved.Position.ToVector3();
		if (!flag && !newSizing)
		{
			gameObject.transform.localScale = saved.Scale.ToVector3();
		}
		if (gameObject.TryGetComponent<KeepInBounds>(out var component))
		{
			component.ForceApproveNewPosition();
		}
		SpawnableInstance orAddComponent;
		if (spawnable.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy)
		{
			if (!(saved is SavedEnemy savedEnemy))
			{
				Log.Error("Provided SavedGeneric object is not of type SavedEnemy, while SpawnableObject.spawnableObjectType is Enemy.", (IEnumerable<Tag>)null, (string)null, (object)null);
				throw new ArgumentException("Invalid SavedGeneric type for Enemy spawnable object.");
			}
			SpawnableInstance spawnableInstance = (orAddComponent = gameObject.GetOrAddComponent<EnemySpawnableInstance>());
			spawnableInstance.sourceObject = spawnable;
			((EnemySpawnableInstance)spawnableInstance).eid.checkingSpawnStatus = false;
			((EnemySpawnableInstance)spawnableInstance).RestoreRadiance(savedEnemy.Radiance);
		}
		else
		{
			orAddComponent = gameObject.GetOrAddComponent<SpawnableInstance>();
			orAddComponent.sourceObject = spawnable;
		}
		if (saved is SavedPhysical { Kinematic: not false })
		{
			orAddComponent.Pause();
		}
		if (!flag && newSizing)
		{
			orAddComponent.SetSize(saved.Scale.ToVector3());
		}
		orAddComponent.disallowManipulation = saved.DisallowManipulation;
		orAddComponent.disallowFreezing = saved.DisallowFreezing;
		ApplyData(gameObject, saved.Data);
		if (spawnable.spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Enemy && (bool)MonoSingleton<SandboxNavmesh>.Instance)
		{
			MonoSingleton<SandboxNavmesh>.Instance.EnsurePositionWithinBounds(gameObject.transform.position);
		}
		return orAddComponent;
	}

	private static void ApplyData(GameObject go, SavedAlterData[] data)
	{
		if (data == null)
		{
			return;
		}
		IAlter[] componentsInChildren = go.GetComponentsInChildren<IAlter>();
		foreach (IAlter alterComponent in componentsInChildren)
		{
			if (alterComponent.alterKey == null)
			{
				continue;
			}
			if (!data.Select((SavedAlterData d) => d.Key).Contains(alterComponent.alterKey))
			{
				Log.Warning("No data for " + alterComponent.alterKey + " on " + go.name, (IEnumerable<Tag>)null, (string)null, (object)null);
				continue;
			}
			SavedAlterData savedAlterData = data.FirstOrDefault((SavedAlterData d) => d.Key == alterComponent.alterKey);
			if (savedAlterData == null)
			{
				continue;
			}
			SavedAlterOption[] options = savedAlterData.Options;
			foreach (SavedAlterOption options2 in options)
			{
				if (options2.BoolValue.HasValue && alterComponent is IAlterOptions<bool> alterOptions)
				{
					AlterOption<bool> alterOption = alterOptions.options.FirstOrDefault((AlterOption<bool> o) => o.key == options2.Key);
					if (alterOption == null)
					{
						continue;
					}
					alterOption.callback?.Invoke(options2.BoolValue.Value);
				}
				if (options2.FloatValue.HasValue && alterComponent is IAlterOptions<float> alterOptions2)
				{
					AlterOption<float> alterOption2 = alterOptions2.options.FirstOrDefault((AlterOption<float> o) => o.key == options2.Key);
					if (alterOption2 == null)
					{
						continue;
					}
					alterOption2.callback?.Invoke(options2.FloatValue.Value);
				}
				if (options2.IntValue.HasValue && alterComponent is IAlterOptions<int> alterOptions3)
				{
					alterOptions3.options.FirstOrDefault((AlterOption<int> o) => o.key == options2.Key)?.callback?.Invoke(options2.IntValue.Value);
				}
			}
		}
	}
}
