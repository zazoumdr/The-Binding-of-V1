using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressablesExtensions
{
	private static readonly Logger Log = new Logger("AddressablesExtensions");

	public static GameObject ToAsset(this AssetReference reference)
	{
		return AssetHelper.LoadPrefab(reference);
	}

	public static GameObject[] ToAssets(this AssetReference[] references)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < references.Length; i++)
		{
			if (references[i] == null || !references[i].RuntimeKeyIsValid())
			{
				Log.Warning($"Invalid asset reference at index {i}.", (IEnumerable<Tag>)null, (string)null, (object)null);
				continue;
			}
			GameObject gameObject = references[i].ToAsset();
			if (gameObject == null || gameObject.Equals(null))
			{
				Log.Warning($"Failed to load asset at index {i}.\nRuntime key: {references[i].RuntimeKey}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			else
			{
				list.Add(gameObject);
			}
		}
		return list.ToArray();
	}
}
