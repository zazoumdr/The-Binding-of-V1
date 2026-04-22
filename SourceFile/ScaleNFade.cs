using UnityEngine;
using UnityEngine.UI;

public class ScaleNFade : MonoBehaviour
{
	public bool scale;

	public bool fade;

	public FadeType ft;

	public float scaleSpeed;

	public float fadeSpeed;

	private SpriteRenderer sr;

	private LineRenderer lr;

	private Light lght;

	private Renderer rend;

	private Image img;

	public bool dontDestroyOnZero;

	public bool lightUseIntensityInsteadOfRange;

	public bool fadeToBlack;

	private Vector3 scaleAmt = Vector3.one;

	private bool hasOpacScale;

	private bool hasTint;

	private bool hasColor;

	public bool clampFade;

	public float clampMinimum;

	public float clampMaximum;

	private void Start()
	{
		if (fade)
		{
			switch (ft)
			{
			case FadeType.Sprite:
				sr = GetComponent<SpriteRenderer>();
				break;
			case FadeType.Line:
				lr = GetComponent<LineRenderer>();
				break;
			case FadeType.Light:
				lght = GetComponent<Light>();
				break;
			case FadeType.Renderer:
				rend = GetComponent<Renderer>();
				if (rend == null)
				{
					rend = GetComponentInChildren<Renderer>();
				}
				break;
			case FadeType.UiImage:
				img = GetComponent<Image>();
				break;
			}
		}
		if (rend != null)
		{
			hasOpacScale = rend.material.HasProperty("_OpacScale");
			hasTint = rend.material.HasProperty("_Tint");
			hasColor = rend.material.HasProperty("_Color");
		}
		scaleAmt = base.transform.localScale;
	}

	private void Update()
	{
		if (scale)
		{
			scaleAmt += Vector3.one * Time.deltaTime * scaleSpeed;
			base.transform.localScale = scaleAmt;
		}
		if (fade)
		{
			switch (ft)
			{
			case FadeType.Sprite:
				sr.color = UpdateColor(sr.color);
				break;
			case FadeType.UiImage:
				((Graphic)img).color = UpdateColor(((Graphic)img).color);
				break;
			case FadeType.Light:
				UpdateLightFade();
				break;
			case FadeType.Renderer:
				UpdateRendererFade();
				break;
			case FadeType.Line:
				break;
			}
		}
	}

	private Color UpdateColor(Color newColor)
	{
		if (newColor.a <= 0f && fadeSpeed > 0f)
		{
			if (!dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
			return newColor;
		}
		newColor.a -= fadeSpeed * Time.deltaTime;
		if (clampFade)
		{
			newColor.a = Mathf.Clamp(newColor.a, clampMinimum, clampMaximum);
		}
		return newColor;
	}

	private void UpdateLightFade()
	{
		float num = (lightUseIntensityInsteadOfRange ? lght.intensity : lght.range);
		if (num <= 0f && fadeSpeed > 0f)
		{
			if (!dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
			return;
		}
		num -= fadeSpeed * Time.deltaTime;
		if (clampFade)
		{
			num = Mathf.Clamp(num, clampMinimum, clampMaximum);
		}
		if (lightUseIntensityInsteadOfRange)
		{
			lght.intensity = num;
		}
		else
		{
			lght.range = num;
		}
	}

	private void UpdateRendererFade()
	{
		if (hasOpacScale)
		{
			UpdateOpacityScale();
		}
		else if (hasTint || hasColor)
		{
			UpdateColorFade();
		}
	}

	private void UpdateOpacityScale()
	{
		float num = rend.material.GetFloat("_OpacScale");
		if (num <= 0f && fadeSpeed > 0f && !dontDestroyOnZero)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		num = Mathf.Max(num - fadeSpeed * Time.deltaTime, 0f);
		if (clampFade)
		{
			num = Mathf.Clamp(num, clampMinimum, clampMaximum);
		}
		rend.material.SetFloat("_OpacScale", num);
	}

	private void UpdateColorFade()
	{
		string text = (hasTint ? "_Tint" : "_Color");
		Color color = rend.material.GetColor(text);
		if (fadeToBlack)
		{
			color = Color.Lerp(color, Color.black, fadeSpeed * Time.deltaTime);
		}
		else
		{
			color.a = Mathf.Max(color.a - fadeSpeed * Time.deltaTime, 0f);
			if (clampFade)
			{
				color.a = Mathf.Clamp(color.a, clampMinimum, clampMaximum);
			}
		}
		if (color.a <= 0f && fadeSpeed > 0f && !dontDestroyOnZero)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			rend.material.SetColor(text, color);
		}
	}

	private void FixedUpdate()
	{
		if (fade && ft == FadeType.Line)
		{
			Color startColor = lr.startColor;
			startColor.a -= fadeSpeed * Time.deltaTime;
			if (clampFade)
			{
				startColor.a = Mathf.Clamp(startColor.a, clampMinimum, clampMaximum);
			}
			lr.startColor = startColor;
			startColor = lr.endColor;
			startColor.a -= fadeSpeed * Time.deltaTime;
			if (clampFade)
			{
				startColor.a = Mathf.Clamp(startColor.a, clampMinimum, clampMaximum);
			}
			lr.endColor = startColor;
			if (lr.startColor.a <= 0f && lr.endColor.a <= 0f && fadeSpeed > 0f && !dontDestroyOnZero)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void ChangeFadeSpeed(float newSpeed)
	{
		fadeSpeed = newSpeed;
	}

	public void ChangeScaleSpeed(float newSpeed)
	{
		scaleSpeed = newSpeed;
	}
}
