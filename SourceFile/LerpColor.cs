using UnityEngine;

public class LerpColor : MonoBehaviour
{
	public bool onEnable = true;

	public bool oneTime;

	[HideInInspector]
	public bool beenActivated;

	[HideInInspector]
	public bool hasOriginalColor;

	public LerpColorType type;

	private Light lit;

	private Material mat;

	private Water wtr;

	private SpriteRenderer spr;

	private bool activated;

	private bool reverted;

	public bool rainbow;

	private Color originalColor;

	private Color originalTargetColor;

	private Color startColor;

	public Color targetColor;

	private Color currentColor;

	public float time;

	private float currentTime;

	public bool dontOverrideAlpha;

	private void OnEnable()
	{
		if (onEnable)
		{
			Activate();
		}
	}

	private void Update()
	{
		if (activated)
		{
			if (rainbow)
			{
				currentColor = RainbowShift(currentColor, Time.deltaTime / time);
			}
			else
			{
				currentTime = Mathf.MoveTowards(currentTime, 1f, Time.deltaTime / time);
				currentColor = Color.Lerp(startColor, targetColor, currentTime);
			}
			UpdateColor();
			if (currentTime >= 1f)
			{
				activated = false;
			}
		}
	}

	private void UpdateColor()
	{
		switch (type)
		{
		case LerpColorType.Light:
			lit.color = new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? lit.color.a : currentColor.a);
			break;
		case LerpColorType.MaterialColor:
			mat.color = new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? mat.color.a : currentColor.a);
			break;
		case LerpColorType.MaterialEmissive:
			mat.SetColor(UKShaderProperties.EmissiveColor, new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? mat.color.a : currentColor.a));
			break;
		case LerpColorType.MaterialSecondary:
			mat.SetColor(UKShaderProperties.BlendColor, new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? mat.color.a : currentColor.a));
			break;
		case LerpColorType.Fog:
			RenderSettings.fogColor = new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? RenderSettings.fogColor.a : currentColor.a);
			break;
		case LerpColorType.Water:
			wtr.UpdateColor(new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? wtr.clr.a : currentColor.a));
			break;
		case LerpColorType.SpriteRenderer:
			spr.color = new Color(currentColor.r, currentColor.g, currentColor.b, dontOverrideAlpha ? spr.color.a : currentColor.a);
			break;
		}
	}

	public void Activate()
	{
		if (beenActivated && oneTime)
		{
			return;
		}
		beenActivated = true;
		GetValues();
		if (reverted)
		{
			startColor = originalColor;
			if (currentTime > 0f)
			{
				currentTime = 1f - currentTime;
			}
		}
		currentColor = startColor;
		targetColor = originalTargetColor;
		activated = true;
		reverted = false;
	}

	public void Revert()
	{
		if (beenActivated)
		{
			Color color = startColor;
			startColor = targetColor;
			targetColor = color;
			currentTime = 1f - currentTime;
			currentColor = Color.Lerp(startColor, targetColor, currentTime);
			activated = true;
			reverted = true;
		}
	}

	public void ResetFull()
	{
		if (beenActivated)
		{
			activated = false;
			currentColor = originalColor;
			currentTime = 0f;
			UpdateColor();
			reverted = true;
		}
	}

	public void Skip()
	{
		if (!beenActivated || !oneTime)
		{
			beenActivated = true;
			GetValues();
			currentColor = targetColor;
			activated = true;
			reverted = false;
			currentTime = 1f;
		}
	}

	private void GetValues()
	{
		switch (type)
		{
		case LerpColorType.Light:
			lit = GetComponent<Light>();
			startColor = lit.color;
			break;
		case LerpColorType.MaterialColor:
		{
			Renderer component = GetComponent<Renderer>();
			for (int j = 0; j < component.materials.Length; j++)
			{
				if (component.materials[j].HasColor(UKShaderProperties.Color))
				{
					mat = component.materials[j];
					startColor = mat.color;
					break;
				}
			}
			break;
		}
		case LerpColorType.MaterialEmissive:
		{
			Renderer component = GetComponent<Renderer>();
			for (int k = 0; k < component.materials.Length; k++)
			{
				if (component.materials[k].IsKeywordEnabled("EMISSIVE"))
				{
					mat = component.materials[k];
					startColor = mat.GetColor(UKShaderProperties.EmissiveColor);
					break;
				}
			}
			break;
		}
		case LerpColorType.MaterialSecondary:
		{
			Renderer component = GetComponent<Renderer>();
			for (int i = 0; i < component.materials.Length; i++)
			{
				if (component.materials[i].IsKeywordEnabled("VERTEX_BLENDING"))
				{
					mat = component.materials[i];
					startColor = mat.GetColor(UKShaderProperties.BlendColor);
					break;
				}
			}
			break;
		}
		case LerpColorType.Fog:
			startColor = RenderSettings.fogColor;
			break;
		case LerpColorType.Water:
			wtr = GetComponent<Water>();
			startColor = wtr.clr;
			break;
		case LerpColorType.SpriteRenderer:
			spr = GetComponent<SpriteRenderer>();
			startColor = spr.color;
			break;
		}
		if (!hasOriginalColor)
		{
			hasOriginalColor = true;
			originalColor = startColor;
			originalTargetColor = targetColor;
		}
	}

	public static Color RainbowShift(Color color, float amount)
	{
		Color.RGBToHSV(color, out var H, out var S, out var V);
		H += amount;
		S = 1f;
		V = 1f;
		return Color.HSVToRGB(H, S, V);
	}
}
