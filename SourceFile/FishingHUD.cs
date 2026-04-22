using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fishing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FishingHUD : MonoSingleton<FishingHUD>
{
	[SerializeField]
	private GameObject powerMeterContainer;

	[SerializeField]
	private Slider powerMeter;

	[SerializeField]
	private GameObject hookedContainer;

	[Space]
	[SerializeField]
	private GameObject fishCaughtContainer;

	[SerializeField]
	private Text fishCaughtText;

	[SerializeField]
	private GameObject fishRenderContainer;

	[SerializeField]
	private GameObject fishSizeContainer;

	[Space]
	[SerializeField]
	private GameObject struggleContainer;

	[SerializeField]
	private GameObject outOfWaterMessage;

	[SerializeField]
	private Image struggleProgressIcon;

	[SerializeField]
	private Image struggleProgressIconOverlay;

	[SerializeField]
	private Image struggleNub;

	[SerializeField]
	private RectTransform desireBar;

	[SerializeField]
	private RectTransform fishIcon;

	[SerializeField]
	private Slider struggleProgressSlider;

	[SerializeField]
	private Text struggleLMB;

	[SerializeField]
	private Text struggleRMB;

	[SerializeField]
	private Image upArrow;

	[SerializeField]
	private Image downArrow;

	[Space]
	[SerializeField]
	private Image fishIconTemplate;

	[SerializeField]
	private Transform fishIconContainer;

	private Dictionary<FishObject, Image> fishHudIcons = new Dictionary<FishObject, Image>();

	private static Color orangeColor = new Color(1f, 0.5f, 0.1f);

	private TimeSince timeSinceLMBReleased;

	private TimeSince timeSinceRMBReleased;

	[HideInInspector]
	public TimeSince timeSinceFishCaught;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip snareroll;

	[SerializeField]
	private AudioClip snarehit;

	private int amountOfCatches;

	private int longWaits;

	private bool hudDisabled;

	private float containerHeight => ((Graphic)struggleNub).rectTransform.parent.GetComponent<RectTransform>().rect.height;

	private void Start()
	{
		((Component)(object)fishIconTemplate).gameObject.SetActive(value: false);
		foreach (KeyValuePair<FishObject, bool> recognizedFish in MonoSingleton<FishManager>.Instance.recognizedFishes)
		{
			Image val = UnityEngine.Object.Instantiate<Image>(fishIconTemplate, fishIconContainer, false);
			((Component)(object)val).gameObject.SetActive(value: true);
			val.sprite = recognizedFish.Key.blockedIcon;
			((Graphic)val).color = Color.black;
			fishHudIcons.Add(recognizedFish.Key, val);
			Image component = ((Component)(object)val).GetComponentInChildren<FishIconGlow>().GetComponent<Image>();
			component.sprite = recognizedFish.Key.blockedIcon;
			((Graphic)component).color = new Color(1f, 1f, 1f, 0f);
		}
		fishIconContainer.gameObject.SetActive(value: false);
	}

	public void ShowHUD()
	{
		if (!hudDisabled)
		{
			fishIconContainer.gameObject.SetActive(value: true);
		}
	}

	public void DisableHUD()
	{
		fishIconContainer.gameObject.SetActive(value: false);
		hudDisabled = true;
	}

	public void SetFishHooked(bool hooked)
	{
		hookedContainer.SetActive(hooked);
	}

	private void OnFishUnlocked(FishObject obj)
	{
		fishHudIcons[obj].sprite = obj.icon;
		((Graphic)fishHudIcons[obj]).color = Color.white;
		((Component)(object)fishHudIcons[obj]).GetComponentInChildren<FishIconGlow>().Blink();
	}

	private void OnEnable()
	{
		FishManager? fishManager = MonoSingleton<FishManager>.Instance;
		fishManager.onFishUnlocked = (Action<FishObject>)Delegate.Combine(fishManager.onFishUnlocked, new Action<FishObject>(OnFishUnlocked));
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<FishManager>.Instance)
		{
			FishManager? fishManager = MonoSingleton<FishManager>.Instance;
			fishManager.onFishUnlocked = (Action<FishObject>)Delegate.Remove(fishManager.onFishUnlocked, new Action<FishObject>(OnFishUnlocked));
		}
	}

	public void SetState(FishingRodState state)
	{
		if (!struggleContainer.activeSelf && state == FishingRodState.FishStruggle)
		{
			outOfWaterMessage.SetActive(value: false);
		}
		powerMeterContainer.SetActive(state == FishingRodState.SelectingPower || state == FishingRodState.Throwing);
		struggleContainer.SetActive(state == FishingRodState.FishStruggle);
	}

	public void SetPowerMeter(float value, bool canFish)
	{
		powerMeter.value = value;
		((Selectable)powerMeter).targetGraphic.color = (canFish ? Color.white : Color.red);
	}

	private void Update()
	{
		float num = Mathf.Sin(struggleProgressSlider.value * 20f);
		fishIcon.localRotation = Quaternion.Euler(0f, 0f, num * 10f);
		if (struggleContainer.activeSelf)
		{
			Color color = Color.Lerp(orangeColor, Color.white, (float)timeSinceLMBReleased * 4f);
			Color color2 = Color.Lerp(orangeColor, Color.white, (float)timeSinceRMBReleased * 4f);
			((Graphic)struggleLMB).color = color;
			((Graphic)struggleRMB).color = color2;
			((Graphic)upArrow).color = color2;
			((Graphic)downArrow).color = color;
		}
	}

	public void ShowFishCaught(bool show = true, FishObject fish = null)
	{
		if (!show)
		{
			StopAllCoroutines();
		}
		else
		{
			timeSinceFishCaught = 0f;
		}
		fishSizeContainer.SetActive(value: false);
		fishCaughtContainer.SetActive(show);
		if (show && fish != null)
		{
			fishCaughtText.text = "<size=28>You caught</size> <color=orange>" + fish.fishName + "</color>";
		}
		foreach (Transform item in fishRenderContainer.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (show && fish != null)
		{
			amountOfCatches++;
			GameObject obj = fish.InstantiateDumb();
			obj.transform.SetParent(fishRenderContainer.transform);
			obj.transform.localPosition = Vector3.zero;
			SandboxUtils.SetLayerDeep(obj.transform, LayerMask.NameToLayer("VirtualRender"));
			obj.transform.localScale *= fish.previewSizeMulti;
			audioSource.clip = snareroll;
			audioSource.Play(tracked: true);
			StartCoroutine(ShowSize());
		}
	}

	public void ShowOutOfWater()
	{
		outOfWaterMessage.SetActive(value: true);
	}

	public void SetStruggleProgress(float progress, Sprite fishIconLocked, Sprite fishIconUnlocked)
	{
		struggleProgressSlider.value = progress;
		struggleProgressIcon.sprite = fishIconUnlocked;
		struggleProgressIconOverlay.sprite = fishIconLocked;
		Color color = ((Graphic)struggleProgressIconOverlay).color;
		color.a = 1f - progress;
		((Graphic)struggleProgressIconOverlay).color = color;
	}

	public void SetStruggleSatisfied(bool satisfied)
	{
		((Graphic)struggleNub).color = (satisfied ? Color.green : Color.white);
	}

	public void SetPlayerStrugglePosition(float pos)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)struggleNub).rectTransform.anchoredPosition = new Vector2(0f, (0f - pos) * containerHeight);
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
		{
			Text obj = struggleLMB;
			InputBinding val = ((IEnumerable<InputBinding>)(object)MonoSingleton<InputManager>.Instance.InputSource.Fire1.Action.bindings).First();
			obj.text = ((InputBinding)(ref val)).ToDisplayString((DisplayStringOptions)0, (InputControl)null);
			Text obj2 = struggleRMB;
			val = ((IEnumerable<InputBinding>)(object)MonoSingleton<InputManager>.Instance.InputSource.Fire2.Action.bindings).First();
			obj2.text = ((InputBinding)(ref val)).ToDisplayString((DisplayStringOptions)0, (InputControl)null);
		}
		else
		{
			struggleLMB.text = "LMB";
			struggleRMB.text = "RMB";
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			((Graphic)struggleLMB).color = new Color(1f, 0.5f, 0.1f);
			((Graphic)downArrow).color = new Color(1f, 0.5f, 0.1f);
			timeSinceLMBReleased = 0f;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed)
		{
			((Graphic)struggleRMB).color = new Color(1f, 0.5f, 0.1f);
			((Graphic)upArrow).color = new Color(1f, 0.5f, 0.1f);
			timeSinceRMBReleased = 0f;
		}
	}

	public void SetFishDesire(float top, float bottom)
	{
		desireBar.offsetMin = new Vector2(desireBar.offsetMin.x, (1f - bottom) * containerHeight);
		desireBar.offsetMax = new Vector2(desireBar.offsetMax.x, (0f - top) * containerHeight);
	}

	private IEnumerator ShowSize()
	{
		if (longWaits == 2 && UnityEngine.Random.Range(0f, 1f) > 0.75f)
		{
			longWaits++;
			yield return new WaitForSeconds(7.5f);
			ShowFishCaught(show: false);
			MonoSingleton<FishManager>.Instance.UpdateFishCount();
		}
		else
		{
			yield return new WaitForSeconds(RandomizeWaitTime());
			fishSizeContainer.SetActive(value: true);
			audioSource.clip = snarehit;
			audioSource.Play(tracked: true);
			MonoSingleton<FishManager>.Instance.UpdateFishCount();
		}
	}

	private IEnumerator AutoDismissFishCaught(float time)
	{
		yield return new WaitForSeconds(time);
		ShowFishCaught(show: false);
	}

	private float RandomizeWaitTime()
	{
		float num = UnityEngine.Random.Range(0f, 1f);
		if (amountOfCatches < 3 || num <= 0.66f)
		{
			StartCoroutine(AutoDismissFishCaught(4f));
			return 1f;
		}
		float num2 = UnityEngine.Random.Range(2f, 6f);
		StartCoroutine(AutoDismissFishCaught(num2 + 3f));
		if (num2 >= 4.5f && longWaits != 2)
		{
			longWaits++;
		}
		return num2;
	}
}
