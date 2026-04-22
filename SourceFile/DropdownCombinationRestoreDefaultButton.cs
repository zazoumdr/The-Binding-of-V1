using System;
using System.Collections.Generic;
using SettingsMenu.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DropdownCombinationRestoreDefaultButton : MonoBehaviour
{
	[Serializable]
	public struct CombinationOption
	{
		public List<BooleanPrefOption> subOptions;
	}

	[Serializable]
	public struct BooleanPrefOption
	{
		public PreferenceKey preferenceKey;

		public bool expectedValue;
	}

	public GameObject buttonContainer;

	public int defaultCombination;

	public List<CombinationOption> combinations;

	public TMP_Dropdown dropdown;

	private bool isValueDirty;

	private void Start()
	{
		((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)delegate
		{
			isValueDirty = true;
		});
		UpdateSelf();
	}

	public void RestoreDefault()
	{
		dropdown.value = defaultCombination;
	}

	private void UpdateSelf()
	{
		CombinationOption combinationOption = combinations[defaultCombination];
		bool flag = true;
		foreach (BooleanPrefOption subOption in combinationOption.subOptions)
		{
			if (subOption.preferenceKey.GetBoolValue() != subOption.expectedValue)
			{
				flag = false;
				break;
			}
		}
		buttonContainer.SetActive(!flag);
	}

	private void LateUpdate()
	{
		if (isValueDirty)
		{
			isValueDirty = false;
			UpdateSelf();
		}
	}
}
