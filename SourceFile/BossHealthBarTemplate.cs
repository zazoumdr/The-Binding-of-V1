using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarTemplate : MonoBehaviour
{
	private static readonly Color IdolProtectedColor = new Color(0.25f, 0.75f, 1f);

	public BossHealthSliderTemplate sliderTemplate;

	public TMP_Text bossNameText;

	public BossHealthSliderTemplate thinSliderTemplate;

	private TMP_Text[] textInstances;

	private Slider[] hpSlider;

	private Slider[] hpAfterImage;

	private Color[] hpColors;

	private float[] healFadeLerps;

	private float introCharge;

	private float waitForDamage;

	private GameObject filler;

	private float shakeTime;

	private Vector3 originalPosition;

	private bool done;

	public bool visibilityDeferred;

	private Slider secondarySlider;

	private GameObject secondaryObject;

	private int currentHpSlider;

	private int currentAfterImageSlider;

	private IEnemyHealthDetails source;

	public void Initialize(BossHealthBar bossBar, SliderLayer[] colorLayers)
	{
		source = bossBar.source;
		List<Slider> list = new List<Slider>();
		List<Slider> list2 = new List<Slider>();
		HealthLayer[] healthLayers = bossBar.healthLayers;
		bossNameText.text = bossBar.bossName.ToUpper();
		float num = 0f;
		for (int i = 0; i < healthLayers.Length; i++)
		{
			BossHealthSliderTemplate bossHealthSliderTemplate = Object.Instantiate(sliderTemplate, sliderTemplate.transform.parent);
			bossHealthSliderTemplate.name = "Health After Image " + bossBar.bossName;
			list2.Add(bossHealthSliderTemplate.slider);
			bossHealthSliderTemplate.slider.minValue = num;
			bossHealthSliderTemplate.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate.gameObject.SetActive(value: true);
			bossHealthSliderTemplate.background.SetActive(i == 0);
			((Graphic)bossHealthSliderTemplate.fill).color = colorLayers[i].afterImageColor;
			BossHealthSliderTemplate bossHealthSliderTemplate2 = Object.Instantiate(sliderTemplate, sliderTemplate.transform.parent);
			bossHealthSliderTemplate2.name = "Health Slider " + bossBar.bossName;
			list.Add(bossHealthSliderTemplate2.slider);
			bossHealthSliderTemplate2.slider.minValue = num;
			bossHealthSliderTemplate2.slider.maxValue = num + healthLayers[i].health;
			bossHealthSliderTemplate2.gameObject.SetActive(value: true);
			bossHealthSliderTemplate2.background.SetActive(value: false);
			((Graphic)bossHealthSliderTemplate2.fill).color = colorLayers[i].color;
			num += healthLayers[i].health;
		}
		hpSlider = list.ToArray();
		hpAfterImage = list2.ToArray();
		textInstances = GetComponentsInChildren<TMP_Text>(includeInactive: true);
		filler = sliderTemplate.filler;
		originalPosition = filler.transform.localPosition;
		for (int num2 = hpSlider.Length - 1; num2 >= 0; num2--)
		{
			if (bossBar.source.Health > hpSlider[num2].minValue)
			{
				currentHpSlider = num2;
				currentAfterImageSlider = currentHpSlider;
				break;
			}
		}
		Slider[] array = hpSlider;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].value = 0f;
		}
		array = hpAfterImage;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].value = 0f;
		}
		hpColors = new Color[hpSlider.Length];
		healFadeLerps = new float[hpSlider.Length];
		for (int k = 0; k < hpColors.Length; k++)
		{
			hpColors[k] = ((Selectable)hpSlider[k]).targetGraphic.color;
			((Selectable)hpSlider[k]).targetGraphic.color = GetHPColor(k);
		}
		for (int l = 0; l < healFadeLerps.Length; l++)
		{
			healFadeLerps[l] = 1f;
		}
		if (bossBar.secondaryBar)
		{
			CreateSecondaryBar(bossBar);
		}
		visibilityDeferred = true;
	}

	public void SetVisible(bool isVisible)
	{
		base.gameObject.SetActive(isVisible);
	}

	private void CreateSecondaryBar(BossHealthBar bossBar)
	{
		if (!secondaryObject)
		{
			BossHealthSliderTemplate bossHealthSliderTemplate = Object.Instantiate(thinSliderTemplate, thinSliderTemplate.transform.parent);
			secondarySlider = bossHealthSliderTemplate.slider;
			secondaryObject = bossHealthSliderTemplate.gameObject;
			((Selectable)secondarySlider).targetGraphic.color = bossBar.secondaryBarColor;
			secondarySlider.value = bossBar.secondaryBarValue;
			secondaryObject.SetActive(value: true);
			MonoSingleton<BossBarManager>.Instance.ForceLayoutRebuild();
		}
	}

	public void UpdateSecondaryBar(BossHealthBar bossBar)
	{
		if (!secondaryObject || !(Object)(object)secondarySlider)
		{
			CreateSecondaryBar(bossBar);
		}
		secondarySlider.value = bossBar.secondaryBarValue;
		((Selectable)secondarySlider).targetGraphic.color = bossBar.secondaryBarColor;
	}

	public void ResetSecondaryBar()
	{
		if ((bool)secondaryObject || (bool)(Object)(object)secondarySlider)
		{
			if ((bool)secondaryObject)
			{
				Object.Destroy(secondaryObject);
			}
			secondaryObject = null;
			secondarySlider = null;
			MonoSingleton<BossBarManager>.Instance.ForceLayoutRebuild();
		}
	}

	public void ScaleChanged(float scale)
	{
		TMP_Text[] array = textInstances;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.localScale = new Vector3(scale, 1f, 1f);
		}
	}

	public void UpdateState(IEnemyHealthDetails details)
	{
		if (source == null || source != details)
		{
			source = details;
		}
	}

	private Color GetHPColor(int index)
	{
		if (source != null && source.Blessed)
		{
			return IdolProtectedColor;
		}
		return hpColors[index];
	}

	private void Update()
	{
		if (hpSlider[currentHpSlider].value != source.Health)
		{
			if (introCharge < source.Health)
			{
				introCharge = Mathf.MoveTowards(introCharge, source.Health, (source.Health - introCharge) * Time.deltaTime * 3f);
				Slider[] array = hpSlider;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].value = introCharge;
				}
			}
			else
			{
				if (hpSlider[currentHpSlider].value < source.Health)
				{
					((Selectable)hpSlider[currentHpSlider]).targetGraphic.color = Color.green;
					healFadeLerps[currentHpSlider] = 0f;
				}
				shakeTime = 5f * (hpSlider[currentHpSlider].value - source.Health);
				hpSlider[currentHpSlider].value = source.Health;
				waitForDamage = 0.15f;
				if (hpSlider[currentHpSlider].minValue > source.Health && currentHpSlider > 0)
				{
					currentHpSlider--;
					hpSlider[currentHpSlider].value = source.Health;
				}
				else if (hpSlider[currentHpSlider].maxValue < source.Health && currentHpSlider < hpSlider.Length - 1)
				{
					hpSlider[currentHpSlider].value = hpSlider[currentHpSlider].value;
					currentHpSlider++;
				}
			}
		}
		if (hpAfterImage[currentAfterImageSlider].value != hpSlider[currentHpSlider].value)
		{
			if (waitForDamage > 0f && hpSlider[0].value > 0f)
			{
				waitForDamage = Mathf.MoveTowards(waitForDamage, 0f, Time.deltaTime);
			}
			else if (hpAfterImage[currentAfterImageSlider].value > hpSlider[currentHpSlider].value)
			{
				hpAfterImage[currentAfterImageSlider].value = Mathf.MoveTowards(hpAfterImage[currentAfterImageSlider].value, hpSlider[currentHpSlider].value, Time.deltaTime * (Mathf.Abs((hpAfterImage[currentAfterImageSlider].value - hpSlider[currentHpSlider].value) * 5f) + 0.5f));
			}
			else
			{
				hpAfterImage[currentAfterImageSlider].value = hpSlider[currentHpSlider].value;
			}
			if (hpAfterImage[currentAfterImageSlider].value <= hpAfterImage[currentAfterImageSlider].minValue && currentAfterImageSlider > 0)
			{
				currentAfterImageSlider--;
			}
		}
		for (int j = 0; j < hpColors.Length; j++)
		{
			if (((Selectable)hpSlider[j]).targetGraphic.color != GetHPColor(j))
			{
				healFadeLerps[j] = Mathf.MoveTowards(healFadeLerps[j], 1f, Time.deltaTime * 2f);
				((Selectable)hpSlider[j]).targetGraphic.color = Color.Lerp(Color.green, GetHPColor(j), healFadeLerps[j]);
			}
		}
		if (shakeTime != 0f)
		{
			if (shakeTime > 10f)
			{
				shakeTime = 10f;
			}
			shakeTime = Mathf.MoveTowards(shakeTime, 0f, Time.deltaTime * 10f);
			if (shakeTime <= 0f)
			{
				shakeTime = 0f;
				filler.transform.localPosition = originalPosition;
			}
			else
			{
				filler.transform.localPosition = new Vector3(originalPosition.x + Random.Range(0f - shakeTime, shakeTime), originalPosition.y + Random.Range(0f - shakeTime, shakeTime), originalPosition.z);
			}
		}
	}

	public void ChangeName(string text)
	{
		bossNameText.text = text;
		TMP_Text[] array = textInstances;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = text;
		}
	}
}
