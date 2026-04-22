using System;
using SettingsMenu.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettingsMenu.Components;

public class SettingsItemBuilder : MonoBehaviour, ISettingsGroupUser
{
	[NonSerialized]
	public SettingsItem asset;

	[SerializeField]
	private TMP_Text label;

	[SerializeField]
	private TMP_Text sideNote;

	[SerializeField]
	private GameObject blocker;

	private RectTransform rectTransform;

	private Image image;

	private CanvasGroup canvasGroup;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		image = GetComponent<Image>();
	}

	public void ConfigureFrom(SettingsItem item, SettingsCategory category, SettingsPageBuilder pageBuilder)
	{
		if (item == null)
		{
			return;
		}
		asset = item;
		SettingsMenuAssets assets = pageBuilder.assets;
		if ((UnityEngine.Object)(object)label == null)
		{
			return;
		}
		label.text = item.GetLabel();
		SettingsRestoreDefaultButton settingsRestoreDefaultButton = null;
		if (item.itemType != SettingsItemType.Button && !item.noResetButton)
		{
			settingsRestoreDefaultButton = UnityEngine.Object.Instantiate((item.style == SettingsItemStyle.Thin) ? assets.smallResetButtonPrefab : assets.resetButtonPrefab, base.transform);
			float valueDisplayMultiplayer = item.valueDisplayMultiplayer;
			settingsRestoreDefaultButton.valueToPrefMultiplier = ((valueDisplayMultiplayer == 0f) ? 1f : (1f / valueDisplayMultiplayer));
		}
		SettingsBuilderBase settingsBuilderBase = UnityEngine.Object.Instantiate(assets.GetBuilderFor(item.itemType), base.transform);
		settingsBuilderBase.ConfigureFrom(this, pageBuilder);
		pageBuilder.AddBuilderInstance(settingsBuilderBase, item);
		if (settingsBuilderBase.TryGetComponent<Selectable>(out var component))
		{
			pageBuilder.AddSelectableRow(component);
			if ((UnityEngine.Object)(object)pageBuilder.navigationButtonSelectable != null)
			{
				((Component)(object)component).gameObject.AddComponent<BackSelectOverride>().Selectable = pageBuilder.navigationButtonSelectable;
			}
			if (settingsRestoreDefaultButton != null)
			{
				settingsRestoreDefaultButton.SetNavigation(component);
			}
		}
		if (settingsRestoreDefaultButton != null)
		{
			settingsBuilderBase.AttachRestoreDefaultButton(settingsRestoreDefaultButton);
		}
		if (item.preferenceKey.IsValid() && settingsRestoreDefaultButton != null)
		{
			settingsRestoreDefaultButton.settingKey = item.preferenceKey.key;
		}
		if ((UnityEngine.Object)(object)sideNote != null)
		{
			if (string.IsNullOrEmpty(item.sideNote))
			{
				UnityEngine.Object.Destroy(((Component)(object)sideNote).gameObject);
			}
			else
			{
				sideNote.text = item.sideNote;
			}
		}
		SettingsGroup settingsGroup = item.group ?? category.group;
		if (settingsGroup != null)
		{
			switch (settingsGroup.togglingMode)
			{
			case SettingsGroupTogglingMode.GrayedOut:
				blocker.SetActive(value: true);
				blocker.transform.SetAsLastSibling();
				canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
				break;
			case SettingsGroupTogglingMode.Hidden:
				UnityEngine.Object.Destroy(blocker);
				break;
			}
			pageBuilder.AddToGroup(settingsGroup, this);
		}
		else
		{
			UnityEngine.Object.Destroy(blocker);
		}
		ApplyStyle(item.style);
	}

	private void ApplyStyle(SettingsItemStyle style)
	{
		if (style == SettingsItemStyle.Thin)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * 0.65f);
			Color color = ((Graphic)image).color;
			color.a *= 0.65f;
			((Graphic)image).color = color;
		}
	}

	private void ResizeBuilderToFitSideNote(RectTransform builderRectTransform)
	{
		if (!((UnityEngine.Object)(object)sideNote == null) && !(builderRectTransform == null))
		{
			float width = builderRectTransform.rect.width;
			LayoutRebuilder.ForceRebuildLayoutImmediate(sideNote.rectTransform);
			float width2 = sideNote.rectTransform.rect.width;
			Vector2 sizeDelta = builderRectTransform.sizeDelta;
			sizeDelta.x = width - width2;
			builderRectTransform.sizeDelta = sizeDelta;
			Vector2 anchoredPosition = builderRectTransform.anchoredPosition;
			anchoredPosition.x -= width2;
			builderRectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void ValueChanged<T>(T value)
	{
		if (asset == null)
		{
			return;
		}
		switch (asset.valueType)
		{
		case global::SettingsMenu.Models.ValueType.Float:
			value = (T)Convert.ChangeType(Convert.ToSingle(value) / asset.valueDisplayMultiplayer, typeof(T));
			break;
		case global::SettingsMenu.Models.ValueType.Int:
		{
			int value2 = Convert.ToInt32(value);
			if (value is float && asset.preferenceKey.IsValid())
			{
				asset.preferenceKey.SetValue(value2);
				return;
			}
			break;
		}
		case global::SettingsMenu.Models.ValueType.BoolCombination:
		{
			int num = Convert.ToInt32(value);
			if (asset.combinationOptions.Count <= num)
			{
				throw new Exception("Dropdown value out of range");
			}
			{
				foreach (DropdownCombinationRestoreDefaultButton.BooleanPrefOption subOption in asset.combinationOptions[num].subOptions)
				{
					Debug.Log(subOption.preferenceKey.key + " " + subOption.expectedValue);
					subOption.preferenceKey.SetValue(subOption.expectedValue);
				}
				return;
			}
		}
		}
		if (asset.preferenceKey.IsValid())
		{
			asset.preferenceKey.SetValue(value);
		}
	}

	public void UpdateGroupStatus(bool groupEnabled, SettingsGroupTogglingMode togglingMode)
	{
		switch (togglingMode)
		{
		case SettingsGroupTogglingMode.GrayedOut:
			if (blocker != null)
			{
				blocker.SetActive(!groupEnabled);
			}
			if ((UnityEngine.Object)(object)canvasGroup != null)
			{
				canvasGroup.interactable = groupEnabled;
			}
			break;
		case SettingsGroupTogglingMode.Hidden:
			base.gameObject.SetActive(groupEnabled);
			break;
		}
	}
}
