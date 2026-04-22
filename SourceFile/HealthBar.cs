using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	private NewMovement nmov;

	public Slider[] hpSliders;

	public Slider[] afterImageSliders;

	public Slider antiHpSlider;

	public Image antiHpSliderFill;

	public TMP_Text hpText;

	private float hp;

	private float antiHp;

	public bool changeTextColor;

	public Color normalTextColor;

	public bool yellowColor;

	public bool antiHpText;

	private int difficulty;

	private float lastHP;

	private float lastAntiHP;

	private string lowDifHealth = "/200";

	private ColorBlindSettings colorBlindSettings;

	private void Start()
	{
		nmov = MonoSingleton<NewMovement>.Instance;
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		colorBlindSettings = MonoSingleton<ColorBlindSettings>.Instance;
	}

	private void Update()
	{
		if (hp < (float)nmov.hp)
		{
			hp = Mathf.MoveTowards(hp, nmov.hp, Time.deltaTime * (((float)nmov.hp - hp) * 5f + 5f));
		}
		else if (hp > (float)nmov.hp)
		{
			hp = nmov.hp;
		}
		if (hpSliders.Length != 0)
		{
			Slider[] array = hpSliders;
			foreach (Slider val in array)
			{
				if (val.value != hp)
				{
					val.value = hp;
				}
			}
		}
		if (afterImageSliders != null)
		{
			Slider[] array = afterImageSliders;
			foreach (Slider val2 in array)
			{
				if (val2.value < hp)
				{
					val2.value = hp;
				}
				else if (val2.value > hp)
				{
					val2.value = Mathf.MoveTowards(val2.value, hp, Time.deltaTime * ((val2.value - hp) * 5f + 5f));
				}
			}
		}
		if ((Object)(object)antiHpSlider != null)
		{
			if (antiHpSlider.value != nmov.antiHp)
			{
				antiHpSlider.value = Mathf.MoveTowards(antiHpSlider.value, nmov.antiHp, Time.deltaTime * (Mathf.Abs(antiHpSlider.value - nmov.antiHp) * 5f + 5f));
			}
			if ((Object)(object)antiHpSliderFill != null)
			{
				((Behaviour)(object)antiHpSliderFill).enabled = antiHpSlider.value > 0f;
			}
		}
		if (!((Object)(object)hpText != null))
		{
			return;
		}
		if (!antiHpText)
		{
			if (lastHP != hp)
			{
				hpText.text = hp.ToString("F0");
				lastHP = hp;
			}
			if (changeTextColor)
			{
				if (hp <= 30f)
				{
					((Graphic)hpText).color = Color.red;
				}
				else if (hp <= 50f && yellowColor)
				{
					((Graphic)hpText).color = Color.yellow;
				}
				else
				{
					((Graphic)hpText).color = normalTextColor;
				}
			}
			else if (normalTextColor == Color.white)
			{
				if (hp <= 30f)
				{
					((Graphic)hpText).color = Color.red;
				}
				else
				{
					((Graphic)hpText).color = colorBlindSettings.GetHudColor(HudColorType.healthText);
				}
			}
		}
		else if (difficulty == 0)
		{
			hpText.text = lowDifHealth;
		}
		else
		{
			antiHp = Mathf.MoveTowards(antiHp, nmov.antiHp, Time.deltaTime * (Mathf.Abs(antiHp - nmov.antiHp) * 5f + 5f));
			float num = 100f - antiHp;
			if (lastAntiHP != num)
			{
				hpText.text = "/" + num.ToString("F0");
				lastAntiHP = num;
			}
		}
	}
}
