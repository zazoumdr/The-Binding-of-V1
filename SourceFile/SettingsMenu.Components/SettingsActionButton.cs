using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsMenu.Components;

public class SettingsActionButton : SettingsBuilderBase
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TMP_Text label;

	public override void ConfigureFrom(SettingsItemBuilder itemBuilder, SettingsPageBuilder pageBuilder)
	{
		label.text = itemBuilder.asset.buttonLabel.ToUpper();
		if (pageBuilder.buttonEvents != null)
		{
			SettingsButtonEvent settingsButtonEvent = pageBuilder.buttonEvents.Find((SettingsButtonEvent x) => x.buttonItem == itemBuilder.asset);
			((UnityEvent)(object)button.onClick).AddListener((UnityAction)settingsButtonEvent.onClickEvent.Invoke);
		}
	}

	public override void SetSelected()
	{
		SettingsMenu.SetSelected((Selectable)(object)button);
	}
}
