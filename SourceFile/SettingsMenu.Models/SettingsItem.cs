using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SettingsMenu.Models;

public class SettingsItem : ScriptableObject
{
	public string label;

	public SettingsItemType itemType;

	public SettingsItemStyle style;

	public SettingsGroup group;

	public PlatformRequirements platformRequirements;

	[HideInInspector]
	public bool noResetButton;

	[HideInInspector]
	public SettingsDropdownType dropdownType = SettingsDropdownType.List;

	[HideInInspector]
	public string dropdownEnumType;

	[HideInInspector]
	public string[] dropdownList;

	[HideInInspector]
	public SliderConfig sliderConfig;

	[HideInInspector]
	public string buttonLabel;

	[HideInInspector]
	public string sideNote;

	[HideInInspector]
	public PreferenceKey preferenceKey;

	[HideInInspector]
	[Tooltip("The multiplier applied to values before displaying them to the user, that is also reversed before saving.\n\nFor example a multiplier of 100, will cause the value of 0.5 to be displayed as 50 on a slider, while still being saved as 0.5")]
	public float valueDisplayMultiplayer = 100f;

	[HideInInspector]
	public ValueType valueType = ValueType.Bool;

	[FormerlySerializedAs("dropdownCombinationOptions")]
	[HideInInspector]
	public List<DropdownCombinationRestoreDefaultButton.CombinationOption> combinationOptions;

	[HideInInspector]
	public int defaultCombination;

	public string GetLabel(bool capitalize = true)
	{
		if (!capitalize)
		{
			return label;
		}
		return label.ToUpper();
	}

	public List<string> GetDropdownItems()
	{
		List<string> list = new List<string>();
		switch (dropdownType)
		{
		case SettingsDropdownType.Enum:
		{
			Type type = Type.GetType(dropdownEnumType);
			if (type == null)
			{
				list.Add("Invalid Type");
				break;
			}
			foreach (object value in Enum.GetValues(type))
			{
				list.Add(value.ToString());
			}
			break;
		}
		case SettingsDropdownType.List:
			list.AddRange(dropdownList);
			break;
		}
		return list;
	}
}
