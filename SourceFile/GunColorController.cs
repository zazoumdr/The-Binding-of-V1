using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GunColorController : MonoSingleton<GunColorController>
{
	public static int[] requiredSecrets = new int[5] { 0, 10, 25, 50, 100 };

	public GunColorPreset[] revolverColors;

	public GunColorPreset[] shotgunColors;

	public GunColorPreset[] nailgunColors;

	public GunColorPreset[] railcannonColors;

	public GunColorPreset[] rocketLauncherColors;

	[HideInInspector]
	public GunColorPreset[] currentColors;

	[HideInInspector]
	public GunColorPreset[] currentAltColors;

	[HideInInspector]
	public int[] presets;

	[HideInInspector]
	public int[] altPresets;

	[HideInInspector]
	public bool[] hasUnlockedColors;

	[HideInInspector]
	public MaterialPropertyBlock[] currentPropBlocks;

	[HideInInspector]
	public MaterialPropertyBlock[] currentAltPropBlocks;

	[HideInInspector]
	public int weaponCount;

	private void Start()
	{
		weaponCount = Enum.GetNames(typeof(GameProgressSaver.WeaponCustomizationType)).Length;
		currentColors = new GunColorPreset[weaponCount];
		currentAltColors = new GunColorPreset[weaponCount];
		presets = new int[weaponCount];
		altPresets = new int[weaponCount];
		hasUnlockedColors = new bool[weaponCount];
		currentPropBlocks = new MaterialPropertyBlock[weaponCount];
		currentAltPropBlocks = new MaterialPropertyBlock[weaponCount];
		UpdateGunColors();
	}

	public void UpdateGunColors()
	{
		for (int i = 0; i < weaponCount; i++)
		{
			UpdateColor(i, altVersion: false);
			UpdateColor(i, altVersion: true);
			hasUnlockedColors[i] = GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)i);
		}
	}

	private void UpdateColor(int gunNumber, bool altVersion)
	{
		GetColorPresets((GameProgressSaver.WeaponCustomizationType)gunNumber);
		int[] array = (altVersion ? altPresets : presets);
		string text = (altVersion ? ".a" : "");
		if (MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + (gunNumber + 1) + text) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)gunNumber))
		{
			SetCustomColors(gunNumber, altVersion);
		}
		else
		{
			string key = "gunColorPreset." + (gunNumber + 1) + text;
			array[gunNumber] = MonoSingleton<PrefsManager>.Instance.GetInt(key);
			if (GameProgressSaver.GetTotalSecretsFound() < requiredSecrets[array[gunNumber]])
			{
				array[gunNumber] = 0;
				MonoSingleton<PrefsManager>.Instance.SetInt(key, 0);
			}
			SetCustomColors(gunNumber, altVersion, array);
		}
		if (altVersion)
		{
			altPresets = array;
		}
		else
		{
			presets = array;
		}
	}

	private void SetCustomColors(int gunNumber, bool altVersion, int[] presetArray = null)
	{
		GunColorPreset gunColorPreset = ((presetArray != null) ? GetColorPresets((GameProgressSaver.WeaponCustomizationType)gunNumber)[presetArray[gunNumber]] : CustomGunColorPreset(gunNumber + 1, altVersion));
		if (altVersion)
		{
			currentAltColors[gunNumber] = gunColorPreset;
		}
		else
		{
			currentColors[gunNumber] = gunColorPreset;
		}
	}

	private GunColorPreset[] GetColorPresets(GameProgressSaver.WeaponCustomizationType weaponType)
	{
		switch (weaponType)
		{
		case GameProgressSaver.WeaponCustomizationType.Revolver:
			return revolverColors;
		case GameProgressSaver.WeaponCustomizationType.Shotgun:
			return shotgunColors;
		case GameProgressSaver.WeaponCustomizationType.Nailgun:
			return nailgunColors;
		case GameProgressSaver.WeaponCustomizationType.Railcannon:
			return railcannonColors;
		case GameProgressSaver.WeaponCustomizationType.RocketLauncher:
			return rocketLauncherColors;
		default:
			Debug.LogError($"Invalid WeaponCustomizationType: {weaponType}");
			return null;
		}
	}

	private GunColorPreset CustomGunColorPreset(int gunNumber, bool altVersion)
	{
		return new GunColorPreset(GetGunColor(1, gunNumber, altVersion), GetGunColor(2, gunNumber, altVersion), GetGunColor(3, gunNumber, altVersion));
	}

	private Color GetGunColor(int number, int gunNumber, bool altVersion)
	{
		return new Color(MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "r", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "g", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "b", 1f), MonoSingleton<PrefsManager>.Instance.GetFloat("gunColor." + gunNumber + "." + number + (altVersion ? ".a" : ".") + "a"));
	}
}
