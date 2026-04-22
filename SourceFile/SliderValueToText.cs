using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SliderValueToText : MonoBehaviour
{
	public DecimalType decimalType;

	[FormerlySerializedAs("modifier")]
	public float multiplier = 1f;

	private string decString;

	private Slider targetSlider;

	private Text targetText;

	private TMP_Text targetTextTMP;

	public string suffix;

	public string ifMax;

	public string ifMin;

	public Color minColor;

	public Color maxColor;

	private Color? origColor;

	private Color nullColor;

	private void Start()
	{
		switch (decimalType)
		{
		case DecimalType.Three:
			decString = "F3";
			break;
		case DecimalType.Two:
			decString = "F2";
			break;
		case DecimalType.One:
			decString = "F1";
			break;
		case DecimalType.NoDecimals:
			decString = "F0";
			break;
		}
		if ((Object)(object)targetSlider == null)
		{
			targetSlider = GetComponentInParent<Slider>();
		}
		if ((Object)(object)targetTextTMP == null)
		{
			targetTextTMP = GetComponent<TMP_Text>();
		}
		if ((Object)(object)targetTextTMP == null && (Object)(object)targetText == null)
		{
			targetText = GetComponent<Text>();
		}
		Color valueOrDefault = origColor.GetValueOrDefault();
		if (!origColor.HasValue)
		{
			valueOrDefault = (((Object)(object)targetTextTMP) ? ((Graphic)targetTextTMP).color : ((Graphic)targetText).color);
			origColor = valueOrDefault;
		}
		nullColor = new Color(0f, 0f, 0f, 0f);
	}

	private void Update()
	{
		string text = "";
		Color color = origColor ?? Color.black;
		text = ((ifMax != "" && targetSlider.value == targetSlider.maxValue) ? ifMax : ((!(ifMin != "") || targetSlider.value != targetSlider.minValue) ? ((targetSlider.value * multiplier).ToString(decString) + suffix) : ifMin));
		if (maxColor != nullColor && targetSlider.value == targetSlider.maxValue)
		{
			color = maxColor;
		}
		else if (minColor != nullColor && targetSlider.value == targetSlider.minValue)
		{
			color = minColor;
		}
		if ((bool)(Object)(object)targetTextTMP)
		{
			targetTextTMP.text = text;
			((Graphic)targetTextTMP).color = color;
		}
		else
		{
			targetText.text = text;
			((Graphic)targetText).color = color;
		}
	}

	public void ConfigureFrom(SliderValueToTextConfig config)
	{
		decimalType = config.decimalType;
		multiplier = config.multiplier;
		suffix = config.suffix;
		ifMax = config.ifMax;
		ifMin = config.ifMin;
		minColor = config.minColor;
		maxColor = config.maxColor;
		Start();
	}
}
