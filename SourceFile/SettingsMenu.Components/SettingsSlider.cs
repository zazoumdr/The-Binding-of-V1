using System.Collections.Generic;
using plog;
using plog.Models;
using SettingsMenu.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SettingsMenu.Components;

public class SettingsSlider : SettingsBuilderBase
{
	private static readonly Logger Log = new Logger("SettingsSlider");

	[SerializeField]
	private Button containerButton;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private SliderValueToText sliderValueToText;

	private void Awake()
	{
		((UnityEvent)(object)containerButton.onClick).AddListener((UnityAction)OnContainerButtonClicked);
	}

	private void OnContainerButtonClicked()
	{
		EventSystem.current.SetSelectedGameObject(((Component)(object)slider).gameObject);
	}

	public override void ConfigureFrom(SettingsItemBuilder itemBuilder, SettingsPageBuilder pageBuilder)
	{
		if ((Object)(object)slider == null)
		{
			return;
		}
		SettingsItem asset = itemBuilder.asset;
		if (asset.sliderConfig != null)
		{
			slider.minValue = asset.sliderConfig.minValue;
			slider.maxValue = asset.sliderConfig.maxValue;
			slider.wholeNumbers = asset.sliderConfig.wholeNumbers;
			if (asset.sliderConfig.textConfig != null)
			{
				sliderValueToText.ConfigureFrom(asset.sliderConfig.textConfig);
			}
			else
			{
				sliderValueToText.gameObject.SetActive(value: false);
				Log.Warning("No textConfig found for slider '" + asset.label + "'", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}
		if (asset.preferenceKey.IsValid())
		{
			float valueWithoutNotify = asset.preferenceKey.GetFloatValue() * asset.valueDisplayMultiplayer;
			slider.SetValueWithoutNotify(valueWithoutNotify);
		}
		((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)itemBuilder.ValueChanged);
	}

	public void SelectInnerSlider()
	{
		SettingsMenu.SetSelected((Selectable)(object)slider);
	}

	public override void SetSelected()
	{
		SettingsMenu.SetSelected((Selectable)(object)containerButton);
	}

	public override void AttachRestoreDefaultButton(SettingsRestoreDefaultButton restoreDefaultButton)
	{
		restoreDefaultButton.slider = slider;
		restoreDefaultButton.integerSlider = slider.wholeNumbers;
	}
}
