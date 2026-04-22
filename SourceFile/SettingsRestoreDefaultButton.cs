using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsRestoreDefaultButton : MonoBehaviour
{
	public GameObject buttonContainer;

	public string settingKey;

	[Header("Float")]
	public Slider slider;

	public float valueToPrefMultiplier = 1f;

	public float sliderTolerance = 0.01f;

	public bool integerSlider;

	[Header("Integer")]
	public TMP_Dropdown dropdown;

	[Header("Boolean")]
	public Toggle toggle;

	[SerializeField]
	private UnityEvent customToggleEvent;

	private float? defaultFloat;

	private int? defaultInt;

	private bool? defaultBool;

	public void RestoreDefault()
	{
		customToggleEvent?.Invoke();
		if (defaultFloat.HasValue)
		{
			slider.value = defaultFloat.Value / valueToPrefMultiplier;
		}
		else if (defaultBool.HasValue)
		{
			toggle.isOn = defaultBool.Value;
		}
		else if (defaultInt.HasValue)
		{
			if ((UnityEngine.Object)(object)dropdown != null)
			{
				dropdown.value = defaultInt.Value;
			}
			if (integerSlider && (UnityEngine.Object)(object)slider != null)
			{
				slider.value = defaultInt.Value;
			}
		}
	}

	public void SetNavigation(Selectable mainSelectable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = mainSelectable.navigation;
		Selectable component = buttonContainer.GetComponent<Selectable>();
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnRight = component;
		mainSelectable.navigation = navigation;
		Navigation navigation2 = component.navigation;
		((Navigation)(ref navigation2)).mode = (Mode)4;
		((Navigation)(ref navigation2)).selectOnLeft = mainSelectable;
		component.navigation = navigation2;
	}

	private void Start()
	{
		if (MonoSingleton<PrefsManager>.Instance.defaultValues.ContainsKey(settingKey))
		{
			object obj = MonoSingleton<PrefsManager>.Instance.defaultValues[settingKey];
			if (!(obj is float value))
			{
				if (!(obj is bool value2))
				{
					if (obj is int value3)
					{
						defaultInt = value3;
					}
				}
				else
				{
					defaultBool = value2;
				}
			}
			else
			{
				defaultFloat = value;
			}
		}
		if ((UnityEngine.Object)(object)slider != null)
		{
			if (integerSlider)
			{
				if (!defaultInt.HasValue)
				{
					defaultInt = 0;
				}
			}
			else if (!defaultFloat.HasValue)
			{
				defaultFloat = 0f;
			}
			((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)delegate
			{
				UpdateSelf();
			});
		}
		if ((UnityEngine.Object)(object)toggle != null)
		{
			if (!defaultBool.HasValue)
			{
				defaultBool = false;
			}
			((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
			{
				UpdateSelf();
			});
		}
		if ((UnityEngine.Object)(object)dropdown != null)
		{
			if (!defaultInt.HasValue)
			{
				defaultInt = 0;
			}
			((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)delegate
			{
				UpdateSelf();
			});
		}
		UpdateSelf();
	}

	private void UpdateSelf()
	{
		if (!defaultInt.HasValue && !defaultBool.HasValue && !defaultFloat.HasValue)
		{
			buttonContainer.SetActive(value: false);
		}
		else if (defaultFloat.HasValue && (UnityEngine.Object)(object)slider != null)
		{
			if (Math.Abs(defaultFloat.Value - slider.value * valueToPrefMultiplier) < sliderTolerance)
			{
				buttonContainer.SetActive(value: false);
			}
			else
			{
				buttonContainer.SetActive(value: true);
			}
		}
		else if (defaultBool.HasValue && (UnityEngine.Object)(object)toggle != null)
		{
			if (defaultBool.Value == toggle.isOn)
			{
				buttonContainer.SetActive(value: false);
			}
			else
			{
				buttonContainer.SetActive(value: true);
			}
		}
		else if (defaultInt.HasValue && ((UnityEngine.Object)(object)dropdown != null || (integerSlider && (UnityEngine.Object)(object)slider != null)))
		{
			int? num = ReadCurrentInt();
			if (!num.HasValue || defaultInt.Value == num)
			{
				buttonContainer.SetActive(value: false);
			}
			else
			{
				buttonContainer.SetActive(value: true);
			}
		}
	}

	private int? ReadCurrentInt()
	{
		if ((UnityEngine.Object)(object)dropdown != null)
		{
			return dropdown.value;
		}
		if ((UnityEngine.Object)(object)slider != null && integerSlider)
		{
			return (int)slider.value;
		}
		return null;
	}
}
