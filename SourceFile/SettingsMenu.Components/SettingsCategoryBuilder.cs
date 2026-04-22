using System.Text;
using SettingsMenu.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SettingsMenu.Components;

public class SettingsCategoryBuilder : MonoBehaviour, ISettingsGroupUser
{
	[SerializeField]
	private TMP_Text title;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private SettingsRestoreDefaultButton restoreDefaultButton;

	private SettingsPageBuilder pageBuilder;

	private SettingsGroup group;

	public void ConfigureFrom(SettingsCategory category, SettingsPageBuilder pageBuilder)
	{
		if (category == null)
		{
			return;
		}
		this.pageBuilder = pageBuilder;
		if ((Object)(object)title != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(category.GetLabel());
			if (!string.IsNullOrEmpty(category.description))
			{
				stringBuilder.Append("\n<size=16>");
				stringBuilder.Append(category.description);
				stringBuilder.Append("</size>");
			}
			title.text = stringBuilder.ToString();
		}
		if (category.group == null)
		{
			((Component)(object)button).gameObject.SetActive(value: false);
			return;
		}
		bool isOnWithoutNotify = category.group.GetEnabled();
		toggle.SetIsOnWithoutNotify(isOnWithoutNotify);
		if (restoreDefaultButton != null)
		{
			restoreDefaultButton.settingKey = category.group.preferenceKey.key;
		}
		((Component)(object)button).gameObject.SetActive(value: true);
		group = category.group;
		pageBuilder.AddToGroup(group, this);
		pageBuilder.AddSelectableRow((Selectable)(object)button);
	}

	public void ToggleGroup()
	{
		if (!(group == null))
		{
			bool groupEnabled = !group.GetEnabled();
			SetGroupEnabled(groupEnabled);
		}
	}

	public void SetGroupEnabled(bool groupEnabled)
	{
		pageBuilder.SetGroupEnabled(group, groupEnabled);
	}

	public void UpdateGroupStatus(bool groupEnabled, SettingsGroupTogglingMode togglingMode)
	{
		toggle.isOn = groupEnabled;
	}
}
