using UnityEngine;
using UnityEngine.UI;

public class CrossFadeColorProxy : Graphic
{
	public ButtonTextColorSetter setter;

	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
	{
		setter.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
	}
}
