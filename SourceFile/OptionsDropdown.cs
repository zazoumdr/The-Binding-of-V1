using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OptionsDropdown : MonoBehaviour
{
	public TMP_Dropdown dropdown;

	public string prefName;

	private void Awake()
	{
		dropdown.SetValueWithoutNotify(MonoSingleton<PrefsManager>.Instance.GetInt(prefName, dropdown.value));
		((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)OnValueChanged);
	}

	private void OnValueChanged(int value)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt(prefName, value);
	}
}
