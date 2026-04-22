using System;

namespace SettingsMenu.Models;

[Serializable]
public class SliderConfig
{
	public float minValue;

	public float maxValue = 100f;

	public bool wholeNumbers = true;

	public SliderValueToTextConfig textConfig;
}
