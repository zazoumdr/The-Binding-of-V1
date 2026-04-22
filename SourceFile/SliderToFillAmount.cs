using UnityEngine;
using UnityEngine.UI;

public class SliderToFillAmount : MonoBehaviour
{
	public Slider targetSlider;

	public float maxFill;

	public bool copyColor;

	private Image img;

	public FadeOutBars mama;

	public bool dontFadeUntilEmpty;

	private bool isInvisible;

	private bool lastInvisible;

	private void OnEnable()
	{
		if ((Object)(object)img == null)
		{
			img = GetComponent<Image>();
		}
		lastInvisible = !isInvisible;
	}

	private void Update()
	{
		isInvisible = Mathf.Approximately(((Graphic)img).color.a, 0f);
		if (isInvisible != lastInvisible)
		{
			((Behaviour)(object)img).enabled = !isInvisible;
			lastInvisible = isInvisible;
		}
		float fillAmount = img.fillAmount;
		float num = (targetSlider.value - targetSlider.minValue) / (targetSlider.maxValue - targetSlider.minValue) * maxFill;
		if (num != fillAmount)
		{
			img.fillAmount = num;
			ResetFadeTimer();
		}
		if (copyColor)
		{
			Color color = ((Graphic)img).color;
			Color color2 = ((Selectable)targetSlider).targetGraphic.color;
			if (color2 != color)
			{
				((Graphic)img).color = color2;
			}
		}
		if (mama != null)
		{
			Color color3 = ((Graphic)img).color;
			float num2 = ((mama.fadeOutTime < 1f) ? mama.fadeOutTime : 1f);
			if (num2 != color3.a)
			{
				color3.a = num2;
				((Graphic)img).color = color3;
			}
		}
	}

	private void ResetFadeTimer()
	{
		if ((bool)mama)
		{
			mama.ResetTimer();
		}
	}
}
