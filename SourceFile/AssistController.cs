using System;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class AssistController : MonoSingleton<AssistController>
{
	public bool majorEnabled;

	[HideInInspector]
	public bool cheatsEnabled;

	[HideInInspector]
	public bool hidePopup;

	[HideInInspector]
	public float gameSpeed;

	[HideInInspector]
	public float damageTaken;

	[HideInInspector]
	public bool infiniteStamina;

	[HideInInspector]
	public bool disableHardDamage;

	[HideInInspector]
	public bool disableWhiplashHardDamage;

	[HideInInspector]
	public bool disableWeaponFreshness;

	public int punchAssistFrames = 6;

	[HideInInspector]
	public int difficultyOverride = -1;

	private StatsManager sman;

	private void Start()
	{
		InitializeValues();
	}

	public void InitializeValues()
	{
		majorEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("majorAssist");
		if (majorEnabled)
		{
			MajorEnabled();
		}
		hidePopup = MonoSingleton<PrefsManager>.Instance.GetBool("hideMajorAssistPopup");
		gameSpeed = MonoSingleton<PrefsManager>.Instance.GetFloat("gameSpeed");
		damageTaken = MonoSingleton<PrefsManager>.Instance.GetFloat("damageTaken");
		infiniteStamina = MonoSingleton<PrefsManager>.Instance.GetBool("infiniteStamina");
		disableHardDamage = MonoSingleton<PrefsManager>.Instance.GetBool("disableHardDamage");
		disableWhiplashHardDamage = MonoSingleton<PrefsManager>.Instance.GetBool("disableWhiplashHardDamage");
		disableWeaponFreshness = MonoSingleton<PrefsManager>.Instance.GetBool("disableWeaponFreshness");
		difficultyOverride = MonoSingleton<PrefsManager>.Instance.GetInt("bossDifficultyOverride") - 1;
	}

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
		switch (key)
		{
		case "majorAssist":
			if (value is bool flag)
			{
				majorEnabled = flag;
				if (majorEnabled)
				{
					MajorEnabled();
				}
			}
			break;
		case "hideMajorAssistPopup":
			if (value is bool flag4)
			{
				hidePopup = flag4;
			}
			break;
		case "gameSpeed":
			if (value is float num3)
			{
				gameSpeed = num3;
			}
			break;
		case "damageTaken":
			if (value is float num2)
			{
				damageTaken = num2;
			}
			break;
		case "infiniteStamina":
			if (value is bool flag6)
			{
				infiniteStamina = flag6;
			}
			break;
		case "disableHardDamage":
			if (value is bool flag5)
			{
				disableHardDamage = flag5;
			}
			break;
		case "disableWhiplashHardDamage":
			if (value is bool flag3)
			{
				disableWhiplashHardDamage = flag3;
			}
			break;
		case "disableWeaponFreshness":
			if (value is bool flag2)
			{
				disableWeaponFreshness = flag2;
			}
			break;
		case "bossDifficultyOverride":
			if (value is int num)
			{
				difficultyOverride = num - 1;
			}
			break;
		}
	}

	public void MajorEnabled()
	{
		majorEnabled = true;
		if (sman == null)
		{
			sman = MonoSingleton<StatsManager>.Instance;
		}
		sman.MajorUsed();
	}
}
