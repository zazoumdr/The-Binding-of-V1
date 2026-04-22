using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopZone : ScreenZone
{
	private bool inUse;

	private Canvas shopCanvas;

	public bool firstVariationBuy;

	private ShopMother shom;

	private ShopCategory[] shopcats;

	private float originalMusicVolume;

	private float musicTarget = 1f;

	private bool fading;

	public bool forcedOff;

	public TMP_Text tipOfTheDay;

	private void Start()
	{
		shopCanvas = GetComponentInChildren<Canvas>(includeInactive: true);
		if ((Object)(object)shopCanvas != null)
		{
			((Component)(object)shopCanvas).gameObject.SetActive(value: false);
		}
		originalMusicVolume = MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume;
		musicTarget = originalMusicVolume;
		if ((Object)(object)tipOfTheDay != null)
		{
			MapInfoBase instance = MapInfoBase.Instance;
			if (instance != null && instance.tipOfTheDay != null)
			{
				tipOfTheDay.text = instance.tipOfTheDay.tip;
			}
		}
		MonoSingleton<CheckPointsController>.Instance.AddShop(this);
		onEnterZone.AddListener(TurnOn);
		onExitZone.AddListener(TurnOff);
	}

	protected override void Update()
	{
		base.Update();
		if (muteMusic && fading && MonoSingleton<AudioMixerController>.Instance.musicVolume != musicTarget)
		{
			MonoSingleton<AudioMixerController>.Instance.SetMusicVolume(Mathf.MoveTowards(MonoSingleton<AudioMixerController>.Instance.musicVolume, Mathf.Min(musicTarget, MonoSingleton<AudioMixerController>.Instance.optionsMusicVolume), originalMusicVolume * Time.deltaTime));
			if (MonoSingleton<AudioMixerController>.Instance.musicVolume == musicTarget)
			{
				fading = false;
			}
		}
	}

	public void TurnOn()
	{
		if (!inUse && !forcedOff)
		{
			inUse = true;
			if ((Object)(object)shopCanvas != null)
			{
				((Component)(object)shopCanvas).gameObject.SetActive(value: true);
			}
			if (shopcats == null)
			{
				shopcats = GetComponentsInChildren<ShopCategory>();
			}
			ShopCategory[] array = shopcats;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CheckGear();
			}
			if ((bool)(Object)(object)(raycaster = null))
			{
				raycaster = ((Component)(object)shopCanvas).GetComponent<GraphicRaycaster>();
			}
			if (ControllerPointer.raycasters.Contains(raycaster))
			{
				ControllerPointer.raycasters.Remove(raycaster);
			}
			ControllerPointer.raycasters.Add(raycaster);
			if (muteMusic)
			{
				fading = true;
				musicTarget = 0f;
			}
		}
	}

	public void TurnOff()
	{
		if (inUse)
		{
			if ((Object)(object)shopCanvas != null)
			{
				((Component)(object)shopCanvas).gameObject.SetActive(value: false);
			}
			inUse = false;
			((Component)(object)shopCanvas).gameObject.SetActive(value: false);
			if (ControllerPointer.raycasters.Contains(raycaster))
			{
				ControllerPointer.raycasters.Remove(raycaster);
			}
			if (firstVariationBuy)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Cycle through <color=orange>EQUIPPED</color> variations with '<color=orange>", "ChangeVariation", "</color>'.");
				firstVariationBuy = false;
				PlayerPrefs.SetInt("FirVar", 0);
			}
			if (muteMusic)
			{
				fading = true;
				musicTarget = originalMusicVolume;
			}
		}
	}

	public void ForceOff()
	{
		TurnOff();
		forcedOff = true;
	}

	public void StopForceOff()
	{
		forcedOff = false;
		if (inZone)
		{
			TurnOn();
		}
	}
}
