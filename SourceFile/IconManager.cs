using System;
using System.Linq;
using Sandbox.Arm;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class IconManager : MonoSingleton<IconManager>
{
	[SerializeField]
	private CheatAssetObject[] iconPacks;

	private int currentIconPack;

	private bool prefFetched;

	public int CurrentIconPackId
	{
		get
		{
			if (!prefFetched)
			{
				return FetchSavedPref();
			}
			return currentIconPack;
		}
	}

	public CheatAssetObject CurrentIcons => iconPacks[CurrentIconPackId];

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "iconPack" && value is int pack)
		{
			SetIconPack(pack, setPref: false);
			Reload();
		}
	}

	private int FetchSavedPref()
	{
		prefFetched = true;
		currentIconPack = MonoSingleton<PrefsManager>.Instance.GetInt("iconPack");
		return currentIconPack;
	}

	public string[] AvailableIconPacks()
	{
		if (iconPacks == null)
		{
			return Array.Empty<string>();
		}
		return (from ip in iconPacks
			where ip != null
			select ip.name).ToArray();
	}

	public void SetIconPack(int pack, bool setPref = true)
	{
		Debug.Log("Selecting icon pack " + pack);
		currentIconPack = pack;
		if (setPref)
		{
			MonoSingleton<PrefsManager>.Instance.SetInt("iconPack", pack);
		}
	}

	public void Reload()
	{
		MonoSingleton<CheatsManager>.Instance.RebuildIcons();
		MonoSingleton<CheatsManager>.Instance.RebuildMenu();
		if ((bool)MonoSingleton<SpawnMenu>.Instance)
		{
			MonoSingleton<SpawnMenu>.Instance.RebuildIcons();
			MonoSingleton<SpawnMenu>.Instance.RebuildMenu();
		}
		if ((bool)MonoSingleton<SandboxArm>.Instance)
		{
			MonoSingleton<SandboxArm>.Instance.ReloadIcon();
		}
	}
}
