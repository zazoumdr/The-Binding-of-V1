using SettingsMenu.Components.Pages;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PowerUpMeter : MonoSingleton<PowerUpMeter>
{
	public float juice;

	public float latestMaxJuice;

	private Image meter;

	public Image vignette;

	public Color powerUpColor;

	private Color currentColor;

	public GameObject endEffect;

	private bool hasPowerUp;

	private PostProcessV2_Handler pp;

	private void Start()
	{
		((Behaviour)(object)vignette).enabled = false;
		meter = GetComponent<Image>();
		meter.fillAmount = 0f;
		pp = MonoSingleton<PostProcessV2_Handler>.Instance;
	}

	private void Update()
	{
		UpdateMeter();
		UpdateCheats();
	}

	public void UpdateMeter()
	{
		if (juice > 0f)
		{
			hasPowerUp = true;
			if (!InfinitePowerUps.Enabled)
			{
				juice -= Time.deltaTime;
			}
			if (HUDSettings.powerUpMeterEnabled && !HideUI.Active)
			{
				meter.fillAmount = juice / latestMaxJuice;
			}
			else
			{
				meter.fillAmount = 0f;
			}
			if (currentColor != powerUpColor)
			{
				currentColor = powerUpColor;
				currentColor.a = juice / latestMaxJuice;
				((Graphic)vignette).color = currentColor;
				Shader.SetGlobalColor("_VignetteColor", currentColor);
				pp.Vignette(doVignette: true);
			}
		}
		else if (hasPowerUp)
		{
			EndPowerUp();
		}
	}

	public void EndPowerUp()
	{
		hasPowerUp = false;
		juice = 0f;
		latestMaxJuice = 0f;
		meter.fillAmount = 0f;
		if (((Graphic)vignette).color.a != 0f)
		{
			currentColor.a = 0f;
			Shader.SetGlobalColor("_VignetteColor", currentColor);
			((Graphic)vignette).color = currentColor;
			pp.Vignette(doVignette: false);
		}
		if ((bool)endEffect)
		{
			Object.Instantiate(endEffect, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
	}

	private void UpdateCheats()
	{
		if (HideUI.Active)
		{
			Shader.SetGlobalColor("_VignetteColor", Color.clear);
			pp.Vignette(doVignette: false);
			((Graphic)vignette).color = Color.clear;
		}
	}
}
