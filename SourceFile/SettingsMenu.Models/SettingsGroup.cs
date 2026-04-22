using UnityEngine;

namespace SettingsMenu.Models;

[CreateAssetMenu(fileName = "SettingsGroup", menuName = "ULTRAKILL/Settings/Group")]
public class SettingsGroup : ScriptableObject
{
	public SettingsGroupTogglingMode togglingMode;

	public PreferenceKey preferenceKey;

	public SettingsGroupValueType valueType;

	public bool invertValue;

	[HideInInspector]
	public int minValue;

	public bool GetEnabled()
	{
		if (!preferenceKey.IsValid())
		{
			return false;
		}
		bool flag = false;
		switch (valueType)
		{
		case SettingsGroupValueType.Bool:
			flag = preferenceKey.GetBoolValue();
			break;
		case SettingsGroupValueType.Int:
			flag = preferenceKey.GetIntValue() >= minValue;
			break;
		}
		if (!invertValue)
		{
			return flag;
		}
		return !flag;
	}

	public void SetEnabledBool(bool enabled)
	{
		if (preferenceKey.IsValid() && valueType == SettingsGroupValueType.Bool)
		{
			preferenceKey.SetBoolValue(invertValue ? (!enabled) : enabled);
		}
	}
}
