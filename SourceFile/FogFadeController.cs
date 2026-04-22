using UnityEngine;

public class FogFadeController : MonoSingleton<FogFadeController>
{
	private bool fading;

	private bool toDisable;

	private bool autoDetect;

	private float speed;

	private Color previousFogColor;

	private float previousFogMin;

	private float previousFogMax;

	private float minTarget;

	private float maxTarget;

	private void Update()
	{
		if (!fading)
		{
			return;
		}
		if (autoDetect && (RenderSettings.fogColor != previousFogColor || previousFogMin != RenderSettings.fogStartDistance || previousFogMax != RenderSettings.fogEndDistance))
		{
			fading = false;
			return;
		}
		previousFogMin = Mathf.MoveTowards(previousFogMin, minTarget, Time.deltaTime * speed);
		previousFogMax = Mathf.MoveTowards(previousFogMax, maxTarget, Time.deltaTime * speed);
		RenderSettings.fogStartDistance = previousFogMin;
		RenderSettings.fogEndDistance = previousFogMax;
		if (previousFogMin == minTarget && previousFogMax == maxTarget)
		{
			if (toDisable)
			{
				RenderSettings.fog = false;
			}
			fading = false;
		}
	}

	public void FadeOut(bool autoDetectFogChange = true, float fadeSpeed = 10f)
	{
		if (RenderSettings.fog)
		{
			autoDetect = autoDetectFogChange;
			speed = fadeSpeed;
			previousFogColor = RenderSettings.fogColor;
			previousFogMin = RenderSettings.fogStartDistance;
			previousFogMax = RenderSettings.fogEndDistance;
			minTarget = previousFogMax;
			maxTarget = previousFogMax;
			toDisable = true;
			fading = true;
		}
	}

	public void FadeIn(float newMin, float newMax, bool autoDetectFogChange = true, float fadeSpeed = 10f)
	{
		autoDetect = autoDetectFogChange;
		speed = fadeSpeed;
		if (!RenderSettings.fog)
		{
			RenderSettings.fogStartDistance = newMax;
			RenderSettings.fogEndDistance = newMax;
			RenderSettings.fog = true;
		}
		previousFogColor = RenderSettings.fogColor;
		previousFogMin = RenderSettings.fogStartDistance;
		previousFogMax = RenderSettings.fogEndDistance;
		minTarget = newMin;
		maxTarget = newMax;
		toDisable = false;
		fading = true;
	}

	public void StopFades()
	{
		fading = false;
	}
}
