using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HeatResistance : MonoSingleton<HeatResistance>
{
	[SerializeField]
	private Slider meter;

	[SerializeField]
	private TMP_Text meterLabel;

	[SerializeField]
	private TMP_Text meterPercentage;

	[SerializeField]
	private Image greenFlash;

	[SerializeField]
	private GameObject hurtingSound;

	[SerializeField]
	private Image screenShatter;

	[SerializeField]
	private CanvasGroup heatResistanceGroup;

	[SerializeField]
	private CanvasGroup heatFixedGroup;

	public float speed;

	private float difficultySpeedModifier = 1f;

	private float heatResistance;

	private TimeSince hurtTimer;

	private bool recharging;

	private float rechargeSpeed;

	private void Awake()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (num >= 2)
		{
			difficultySpeedModifier = 1f;
		}
		else if (num == 1)
		{
			difficultySpeedModifier = 0.75f;
		}
		else
		{
			difficultySpeedModifier = 0.5f;
		}
	}

	private void OnEnable()
	{
		RechargeOnce();
		StatsManager.checkpointRestart += RechargeOnce;
	}

	private void OnDisable()
	{
		StatsManager.checkpointRestart -= RechargeOnce;
	}

	private void Update()
	{
		if (recharging)
		{
			heatResistance = Mathf.MoveTowards(heatResistance, 100f, Time.deltaTime * speed * 12f * rechargeSpeed);
		}
		else if (heatResistance > 0f)
		{
			heatResistance = Mathf.MoveTowards(heatResistance, 0f, Time.deltaTime * speed * difficultySpeedModifier);
		}
		else
		{
			if (MonoSingleton<NewMovement>.Instance.hp > 100)
			{
				MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.Abs(100 - MonoSingleton<NewMovement>.Instance.hp), invincible: false, 0f, explosion: false, instablack: false, 0f, ignoreInvincibility: true);
			}
			MonoSingleton<NewMovement>.Instance.ForceAntiHP((float)hurtTimer * 5f * difficultySpeedModifier, silent: true, dontOverwriteHp: false, addToCooldown: true, stopInstaHeal: true);
		}
		meter.value = Mathf.MoveTowards(meter.value, heatResistance, Time.deltaTime * 10f * Mathf.Max(1f, Mathf.Abs(Mathf.Abs(meter.value) - Mathf.Abs(heatResistance))));
		meterPercentage.text = meter.value.ToString("00.00") + "%";
		if (recharging || ((Graphic)greenFlash).color.a != 0f)
		{
			((Graphic)greenFlash).color = new Color(((Graphic)greenFlash).color.r, ((Graphic)greenFlash).color.g, ((Graphic)greenFlash).color.b, recharging ? 1f : Mathf.MoveTowards(((Graphic)greenFlash).color.a, 0f, Time.deltaTime));
		}
		((Graphic)meterLabel).color = ((heatResistance > 0f) ? Color.white : Color.Lerp(Color.red, Color.white, (float)hurtTimer % 0.5f));
		if (heatResistance > 0f)
		{
			hurtTimer = 0f;
		}
		if (heatResistance == 0f)
		{
			((Graphic)screenShatter).color = new Color(1f, 1f, 1f, 0.5f);
		}
		if (hurtingSound.activeSelf != (heatResistance == 0f))
		{
			hurtingSound.SetActive(heatResistance == 0f);
		}
	}

	public void RechargeOnce()
	{
		heatResistance = 100f;
	}

	public void SetRechargeMode(float targetSpeedMultiplier)
	{
		recharging = targetSpeedMultiplier != 0f;
		rechargeSpeed = targetSpeedMultiplier;
	}
}
