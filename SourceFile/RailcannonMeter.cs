using SettingsMenu.Components.Pages;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class RailcannonMeter : MonoSingleton<RailcannonMeter>
{
	public Image meterBackground;

	public Image[] meters;

	private Image[] trueMeters;

	public Image colorlessMeter;

	private Image self;

	public GameObject[] altHudPanels;

	private float flashAmount;

	public GameObject miniVersion;

	private bool hasFlashed;

	private void Start()
	{
		CheckStatus();
	}

	private void OnEnable()
	{
		CheckStatus();
	}

	private void Update()
	{
		if (((Behaviour)(object)self).enabled || miniVersion.activeSelf)
		{
			for (int i = 0; i < trueMeters.Length; i++)
			{
				if (((Behaviour)(object)self).enabled || i != 0)
				{
					((Behaviour)(object)trueMeters[i]).enabled = true;
				}
				else
				{
					((Behaviour)(object)trueMeters[i]).enabled = false;
				}
			}
			if (MonoSingleton<WeaponCharges>.Instance.raicharge > 4f)
			{
				if (!hasFlashed && Time.timeScale > 0f)
				{
					flashAmount = 1f;
				}
				hasFlashed = true;
				if (!MonoSingleton<ColorBlindSettings>.Instance)
				{
					return;
				}
				Color color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.railcannonFull);
				if (flashAmount > 0f)
				{
					color = Color.Lerp(color, Color.white, flashAmount);
					flashAmount = Mathf.MoveTowards(flashAmount, 0f, Time.deltaTime);
				}
				Image[] array = trueMeters;
				foreach (Image val in array)
				{
					val.fillAmount = 1f;
					if ((Object)(object)val != (Object)(object)colorlessMeter)
					{
						((Graphic)val).color = color;
					}
					else
					{
						((Graphic)val).color = Color.white;
					}
				}
			}
			else
			{
				flashAmount = 0f;
				hasFlashed = false;
				Image[] array = trueMeters;
				foreach (Image obj in array)
				{
					((Graphic)obj).color = MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.railcannonCharging);
					obj.fillAmount = MonoSingleton<WeaponCharges>.Instance.raicharge / 4f;
				}
			}
			if (MonoSingleton<WeaponCharges>.Instance.raicharge > 4f || !((Behaviour)(object)self).enabled)
			{
				((Behaviour)(object)meterBackground).enabled = false;
			}
			else
			{
				((Behaviour)(object)meterBackground).enabled = true;
			}
		}
		else
		{
			flashAmount = 0f;
			((Behaviour)(object)meterBackground).enabled = false;
			hasFlashed = false;
			Image[] array = trueMeters;
			for (int j = 0; j < array.Length; j++)
			{
				((Behaviour)(object)array[j]).enabled = false;
			}
		}
	}

	public void CheckStatus()
	{
		if (trueMeters == null || trueMeters.Length == 0)
		{
			trueMeters = (Image[])(object)new Image[meters.Length + 1];
			for (int i = 0; i < trueMeters.Length; i++)
			{
				if (i < meters.Length)
				{
					trueMeters[i] = meters[i];
				}
				else
				{
					trueMeters[i] = colorlessMeter;
				}
			}
		}
		if (!(Object)(object)self)
		{
			self = GetComponent<Image>();
		}
		if (HUDSettings.railcannonMeterEnabled && RailcannonStatus())
		{
			if (HUDSettings.weaponIconEnabled)
			{
				((Behaviour)(object)self).enabled = true;
				miniVersion.SetActive(value: false);
			}
			else
			{
				((Behaviour)(object)self).enabled = false;
				miniVersion.SetActive(value: true);
			}
			GameObject[] array = altHudPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}
		else
		{
			((Behaviour)(object)self).enabled = false;
			miniVersion.SetActive(value: false);
			GameObject[] array = altHudPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
		}
	}

	private bool RailcannonStatus()
	{
		if (!MonoSingleton<PrefsManager>.TryGetInstance(out PrefsManager _) || !MonoSingleton<GunControl>.TryGetInstance(out GunControl _))
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			string text = "rai" + i;
			if (GameProgressSaver.CheckGear(text) == 1 && MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + text, 1) == 1 && !MonoSingleton<GunControl>.Instance.noWeapons)
			{
				return true;
			}
		}
		return false;
	}
}
