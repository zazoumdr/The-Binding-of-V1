using UnityEngine;
using UnityEngine.UI;

namespace SettingsMenu.Components.Pages;

public class HUDSettings : SettingsLogicBase
{
	public static bool powerUpMeterEnabled;

	public static bool railcannonMeterEnabled;

	public static bool weaponIconEnabled;

	public Material hudMaterial;

	public Material hudTextMaterial;

	private HudController[] hudCons;

	private Mask[] masks;

	public override void Initialize(SettingsMenu settingsMenu)
	{
		hudCons = Object.FindObjectsOfType<HudController>();
		HudController[] array = hudCons;
		foreach (HudController hudController in array)
		{
			if (!hudController.altHud)
			{
				masks = hudController.GetComponentsInChildren<Mask>(includeInactive: true);
				break;
			}
		}
		bool stuff = MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop");
		AlwaysOnTop(stuff);
		bool powerUpMeter = MonoSingleton<PrefsManager>.Instance.GetBool("powerUpMeter");
		SetPowerUpMeter(powerUpMeter);
		weaponIconEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons");
		railcannonMeterEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("railcannonMeter");
	}

	public override void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "hudAlwaysOnTop":
			if (value is bool stuff)
			{
				AlwaysOnTop(stuff);
			}
			break;
		case "powerUpMeter":
			if (value is bool powerUpMeter)
			{
				SetPowerUpMeter(powerUpMeter);
			}
			break;
		case "weaponIcons":
			weaponIconEnabled = (bool)value;
			MonoSingleton<RailcannonMeter>.Instance?.CheckStatus();
			break;
		case "railcannonMeter":
			railcannonMeterEnabled = (bool)value;
			MonoSingleton<RailcannonMeter>.Instance?.CheckStatus();
			break;
		}
	}

	private void SetPowerUpMeter(bool value)
	{
		powerUpMeterEnabled = value;
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.UpdateMeter();
		}
	}

	private void AlwaysOnTop(bool stuff)
	{
		if (stuff)
		{
			hudMaterial.SetFloat("_ZTest", 8f);
			hudTextMaterial.SetFloat("_ZTest", 8f);
		}
		else
		{
			hudMaterial.SetFloat("_ZTest", 4f);
			hudTextMaterial.SetFloat("_ZTest", 4f);
		}
		if (masks == null)
		{
			return;
		}
		Mask[] array = masks;
		foreach (Mask val in array)
		{
			if (((Behaviour)(object)val).enabled)
			{
				((Behaviour)(object)val).enabled = false;
				((Behaviour)(object)val).enabled = true;
			}
		}
	}
}
