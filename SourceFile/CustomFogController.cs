using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomFogController : MonoBehaviour
{
	[Serializable]
	public enum FogState
	{
		Disabled,
		Static,
		Dynamic
	}

	[Serializable]
	private struct ValuePreset(FogState fogState, float redAmount, float greenAmount, float blueAmount, float startDistance, float endDistance)
	{
		public FogState fogState = fogState;

		public float redAmount = redAmount;

		public float greenAmount = greenAmount;

		public float blueAmount = blueAmount;

		public float startDistance = startDistance;

		public float endDistance = endDistance;
	}

	[SerializeField]
	private FogState fogState;

	[SerializeField]
	private Button disabledButton;

	private ShopButton disabledShopButton;

	[SerializeField]
	private Button staticButton;

	private ShopButton staticShopButton;

	[SerializeField]
	private Button dynamicButton;

	private ShopButton dynamicShopButton;

	private float redAmount;

	private float greenAmount;

	private float blueAmount;

	[Space]
	[SerializeField]
	private Slider redSlider;

	[SerializeField]
	private Slider greenSlider;

	[SerializeField]
	private Slider blueSlider;

	[Space]
	[SerializeField]
	private Image colorImage;

	private float startDistance;

	private float endDistance;

	[Space]
	[SerializeField]
	private Slider startDistanceSlider;

	[SerializeField]
	private Slider endDistanceSlider;

	[Space]
	[SerializeField]
	private GameObject disabledText;

	[SerializeField]
	private GameObject startDistanceSliderGameObject;

	[SerializeField]
	private GameObject endDistanceSliderGameObject;

	[Space]
	[SerializeField]
	private FogSetterBounds fogSetterBounds;

	private bool levelStarted;

	[Header("Preset Values")]
	[SerializeField]
	private ValuePreset[] presets = new ValuePreset[2];

	private bool fogDisabled => fogState == FogState.Disabled;

	private bool fogStatic => fogState == FogState.Static;

	private bool fogDynamic => fogState == FogState.Dynamic;

	private void Start()
	{
		disabledShopButton = ((Component)(object)disabledButton).GetComponent<ShopButton>();
		staticShopButton = ((Component)(object)staticButton).GetComponent<ShopButton>();
		dynamicShopButton = ((Component)(object)dynamicButton).GetComponent<ShopButton>();
		disabledShopButton.PointerClickSuccess += delegate
		{
			SetState(FogState.Disabled);
		};
		staticShopButton.PointerClickSuccess += delegate
		{
			SetState(FogState.Static);
		};
		dynamicShopButton.PointerClickSuccess += delegate
		{
			SetState(FogState.Dynamic);
		};
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.theme");
		fogState = (FogState)MonoSingleton<PrefsManager>.Instance.GetIntLocal("cyberGrind.fogState", (int)presets[num].fogState);
		((Selectable)disabledButton).interactable = !fogDisabled;
		disabledShopButton.deactivated = fogDisabled;
		((Selectable)staticButton).interactable = !fogStatic;
		staticShopButton.deactivated = fogStatic;
		((Selectable)dynamicButton).interactable = !fogDynamic;
		dynamicShopButton.deactivated = fogDynamic;
		redAmount = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.fogColor.r", presets[num].redAmount);
		redSlider.SetValueWithoutNotify(redAmount);
		greenAmount = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.fogColor.g", presets[num].greenAmount);
		greenSlider.SetValueWithoutNotify(greenAmount);
		blueAmount = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.fogColor.b", presets[num].blueAmount);
		blueSlider.SetValueWithoutNotify(blueAmount);
		((Graphic)colorImage).color = new Color(redAmount, greenAmount, blueAmount);
		RenderSettings.fogColor = new Color(redAmount, greenAmount, blueAmount);
		startDistance = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.fogStartDistance", presets[num].startDistance);
		endDistance = MonoSingleton<PrefsManager>.Instance.GetFloatLocal("cyberGrind.fogEndDistance", presets[num].endDistance);
		if (startDistance == endDistance)
		{
			if (startDistance == startDistanceSlider.minValue)
			{
				endDistance = startDistance + 1f;
			}
			else
			{
				startDistance = endDistance - 1f;
			}
		}
		startDistanceSlider.SetValueWithoutNotify(startDistance);
		endDistanceSlider.SetValueWithoutNotify(endDistance);
		disabledText.SetActive(fogDisabled);
		startDistanceSliderGameObject.SetActive(fogStatic);
		endDistanceSliderGameObject.SetActive(!fogDisabled);
		fogSetterBounds.enabled = fogDynamic;
		if (!fogDynamic)
		{
			RenderSettings.fogStartDistance = startDistance;
		}
		RenderSettings.fogEndDistance = endDistance;
	}

	public void SetState(FogState state)
	{
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("cyberGrind.fogState", (int)state);
		fogState = state;
		RenderSettings.fog = !fogDisabled && levelStarted;
		fogSetterBounds.enabled = fogDynamic;
		if (!fogDynamic)
		{
			RenderSettings.fogStartDistance = startDistance;
		}
		RenderSettings.fogEndDistance = endDistance;
		((Selectable)disabledButton).interactable = !fogDisabled;
		disabledShopButton.deactivated = fogDisabled;
		((Selectable)staticButton).interactable = !fogStatic;
		staticShopButton.deactivated = fogStatic;
		((Selectable)dynamicButton).interactable = !fogDynamic;
		dynamicShopButton.deactivated = fogDynamic;
		disabledText.SetActive(fogDisabled);
		startDistanceSliderGameObject.SetActive(fogStatic);
		endDistanceSliderGameObject.SetActive(!fogDisabled);
	}

	public void SetRed(float amount)
	{
		redAmount = amount;
		UpdateColor();
	}

	public void SetGreen(float amount)
	{
		greenAmount = amount;
		UpdateColor();
	}

	public void SetBlue(float amount)
	{
		blueAmount = amount;
		UpdateColor();
	}

	private void UpdateColor()
	{
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogColor.r", redAmount);
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogColor.g", greenAmount);
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogColor.b", blueAmount);
		((Graphic)colorImage).color = new Color(redAmount, greenAmount, blueAmount);
		RenderSettings.fogColor = new Color(redAmount, greenAmount, blueAmount);
	}

	public void SetFogStartDistance(float distance)
	{
		if (distance >= endDistance)
		{
			if (distance + 1f > endDistanceSlider.maxValue)
			{
				distance = endDistanceSlider.maxValue - 1f;
				endDistance = endDistanceSlider.maxValue;
				startDistanceSlider.SetValueWithoutNotify(distance);
			}
			else
			{
				endDistance = distance + 1f;
			}
			endDistanceSlider.SetValueWithoutNotify(endDistance);
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogEndDistance", endDistance);
			fogSetterBounds.ChangeDistance(endDistance);
			if (!fogDynamic)
			{
				RenderSettings.fogEndDistance = endDistance;
			}
		}
		startDistance = distance;
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogStartDistance", startDistance);
		RenderSettings.fogStartDistance = startDistance;
	}

	public void SetFogEndDistance(float distance)
	{
		if (distance <= startDistance)
		{
			if (distance - 1f < startDistanceSlider.minValue)
			{
				distance = startDistanceSlider.minValue + 1f;
				startDistance = startDistanceSlider.minValue;
				endDistanceSlider.SetValueWithoutNotify(distance);
			}
			else
			{
				startDistance = distance - 1f;
			}
			startDistanceSlider.SetValueWithoutNotify(startDistance);
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogStartDistance", startDistance);
			if (!fogDynamic)
			{
				RenderSettings.fogStartDistance = startDistance;
			}
		}
		endDistance = distance;
		MonoSingleton<PrefsManager>.Instance.SetFloatLocal("cyberGrind.fogEndDistance", endDistance);
		fogSetterBounds.ChangeDistance(endDistance);
		if (!fogDynamic)
		{
			RenderSettings.fogEndDistance = endDistance;
		}
	}

	public void ResetValues()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.theme");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogState");
		fogState = presets[num].fogState;
		RenderSettings.fog = !fogDisabled && levelStarted;
		((Selectable)disabledButton).interactable = !fogDisabled;
		disabledShopButton.deactivated = fogDisabled;
		((Selectable)staticButton).interactable = !fogStatic;
		staticShopButton.deactivated = fogStatic;
		((Selectable)dynamicButton).interactable = !fogDynamic;
		dynamicShopButton.deactivated = fogDynamic;
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.r");
		redAmount = presets[num].redAmount;
		redSlider.SetValueWithoutNotify(redAmount);
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.g");
		greenAmount = presets[num].greenAmount;
		greenSlider.SetValueWithoutNotify(greenAmount);
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.b");
		blueAmount = presets[num].blueAmount;
		blueSlider.SetValueWithoutNotify(blueAmount);
		((Graphic)colorImage).color = new Color(redAmount, greenAmount, blueAmount);
		RenderSettings.fogColor = new Color(redAmount, greenAmount, blueAmount);
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogStartDistance");
		startDistance = presets[num].startDistance;
		startDistanceSlider.SetValueWithoutNotify(startDistance);
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogEndDistance");
		endDistance = presets[num].endDistance;
		endDistanceSlider.SetValueWithoutNotify(endDistance);
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogDynamicDistance");
		disabledText.SetActive(fogDisabled);
		startDistanceSliderGameObject.SetActive(fogStatic);
		endDistanceSliderGameObject.SetActive(!fogDisabled);
		fogSetterBounds.enabled = fogDynamic;
		fogSetterBounds.ChangeDistance(endDistance);
		if (!fogDynamic)
		{
			RenderSettings.fogStartDistance = startDistance;
			RenderSettings.fogEndDistance = endDistance;
		}
	}

	public void SetPreset(int index)
	{
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogState");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.r");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.g");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogColor.b");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogStartDistance");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogEndDistance");
		MonoSingleton<PrefsManager>.Instance.DeleteKey("cyberGrind.fogDynamicDistance");
		fogState = (FogState)MonoSingleton<PrefsManager>.Instance.GetIntLocal("cyberGrind.fogState", (int)presets[index].fogState);
		RenderSettings.fog = !fogDisabled && levelStarted;
		redAmount = presets[index].redAmount;
		redSlider.SetValueWithoutNotify(redAmount);
		greenAmount = presets[index].greenAmount;
		greenSlider.SetValueWithoutNotify(greenAmount);
		blueAmount = presets[index].blueAmount;
		blueSlider.SetValueWithoutNotify(blueAmount);
		((Graphic)colorImage).color = new Color(redAmount, greenAmount, blueAmount);
		RenderSettings.fogColor = new Color(redAmount, greenAmount, blueAmount);
		startDistance = presets[index].startDistance;
		startDistanceSlider.SetValueWithoutNotify(startDistance);
		endDistance = presets[index].endDistance;
		endDistanceSlider.SetValueWithoutNotify(endDistance);
		disabledText.SetActive(fogDisabled);
		startDistanceSliderGameObject.SetActive(fogStatic);
		endDistanceSliderGameObject.SetActive(!fogDisabled);
		fogSetterBounds.enabled = fogDynamic;
		fogSetterBounds.ChangeDistance(endDistance);
		if (!fogDynamic)
		{
			RenderSettings.fogStartDistance = startDistance;
			RenderSettings.fogEndDistance = endDistance;
		}
	}

	public void LevelStart()
	{
		levelStarted = true;
		RenderSettings.fog = !fogDisabled && levelStarted;
	}
}
