using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaMeter : MonoBehaviour
{
	private NewMovement nmov;

	private float stamina;

	private Slider stm;

	private TMP_Text stmText;

	public bool changeTextColor;

	public Color normalTextColor;

	private Image staminaFlash;

	private Color flashColor;

	private Image staminaBar;

	private bool full = true;

	private AudioSource aud;

	private Color emptyColor;

	private Color origColor;

	public bool redEmpty;

	private bool intro = true;

	private float lastStamina;

	private Canvas parentCanvas;

	public bool alwaysUpdate;

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		stm = GetComponent<Slider>();
		if ((Object)(object)stm != null)
		{
			staminaBar = base.transform.GetChild(1).GetChild(0).GetComponent<Image>();
			staminaFlash = ((Component)(object)staminaBar).transform.GetChild(0).GetComponent<Image>();
			flashColor = ((Graphic)staminaFlash).color;
			origColor = ((Graphic)staminaBar).color;
		}
		stmText = GetComponent<TMP_Text>();
		nmov = MonoSingleton<NewMovement>.Instance;
		lastStamina = stamina;
		UpdateColors();
		parentCanvas = GetComponentInParent<Canvas>();
	}

	private void OnEnable()
	{
		UpdateColors();
	}

	private void Update()
	{
		if (intro)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((nmov.boostCharge - stamina) * 5f + 10f));
			if (stamina >= nmov.boostCharge)
			{
				intro = false;
			}
		}
		else if (stamina < nmov.boostCharge)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((nmov.boostCharge - stamina) * 25f + 25f));
		}
		else if (stamina > nmov.boostCharge)
		{
			stamina = Mathf.MoveTowards(stamina, nmov.boostCharge, Time.deltaTime * ((stamina - nmov.boostCharge) * 25f + 25f));
		}
		if (!alwaysUpdate && (!(Object)(object)parentCanvas || !((Behaviour)(object)parentCanvas).enabled))
		{
			return;
		}
		if ((Object)(object)stm != null)
		{
			stm.value = stamina;
			if (stm.value >= stm.maxValue && !full)
			{
				full = true;
				((Graphic)staminaBar).color = origColor;
				Flash();
			}
			if (flashColor.a > 0f)
			{
				if (flashColor.a - Time.deltaTime > 0f)
				{
					flashColor.a -= Time.deltaTime;
				}
				else
				{
					flashColor.a = 0f;
				}
				((Graphic)staminaFlash).color = flashColor;
			}
			if (stm.value < stm.maxValue)
			{
				full = false;
				((Graphic)staminaBar).color = emptyColor;
			}
		}
		if (!((Object)(object)stmText != null))
		{
			return;
		}
		if (lastStamina != stamina)
		{
			stmText.text = (stamina / 100f).ToString("0.00");
		}
		lastStamina = stamina;
		if (changeTextColor)
		{
			if (stamina < 100f)
			{
				((Graphic)stmText).color = Color.red;
			}
			else
			{
				((Graphic)stmText).color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.stamina);
			}
		}
		else if (normalTextColor == Color.white)
		{
			if (stamina < 100f)
			{
				((Graphic)stmText).color = Color.red;
			}
			else
			{
				((Graphic)stmText).color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.healthText);
			}
		}
	}

	public void Flash(bool red = false)
	{
		if ((Object)(object)stm != null)
		{
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.Play(tracked: true);
			if (red)
			{
				flashColor = Color.red;
			}
			else
			{
				flashColor = Color.white;
			}
			((Graphic)staminaFlash).color = flashColor;
		}
	}

	public void UpdateColors()
	{
		origColor = MonoSingleton<ColorBlindSettings>.Instance.staminaColor;
		if (redEmpty)
		{
			emptyColor = MonoSingleton<ColorBlindSettings>.Instance.staminaEmptyColor;
		}
		else
		{
			emptyColor = MonoSingleton<ColorBlindSettings>.Instance.staminaChargingColor;
		}
		if ((bool)(Object)(object)staminaBar)
		{
			if (full)
			{
				((Graphic)staminaBar).color = origColor;
			}
			else
			{
				((Graphic)staminaBar).color = emptyColor;
			}
		}
	}
}
