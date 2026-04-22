using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RumbleSettingsMenu : MonoBehaviour
{
	[SerializeField]
	private RumbleKeyOptionEntry optionTemplate;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private Button totalWrapper;

	[SerializeField]
	private Button quitButton;

	[SerializeField]
	private Slider totalSlider;

	private void Start()
	{
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		optionTemplate.gameObject.SetActive(value: false);
		Button val = totalWrapper;
		RumbleKey[] all = RumbleProperties.All;
		foreach (RumbleKey key in all)
		{
			RumbleKeyOptionEntry option = Object.Instantiate(optionTemplate, container);
			option.gameObject.SetActive(value: true);
			option.key = key;
			option.keyName.text = RumbleManager.ResolveFullName(key);
			float num = MonoSingleton<RumbleManager>.Instance.ResolveDuration(key);
			if (num >= float.PositiveInfinity)
			{
				option.durationContainer.gameObject.SetActive(value: false);
			}
			else
			{
				option.durationSlider.SetValueWithoutNotify(num);
				((UnityEvent<float>)(object)option.durationSlider.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
				{
					option.SetDuration(value);
				});
			}
			option.intensitySlider.SetValueWithoutNotify(MonoSingleton<RumbleManager>.Instance.ResolveIntensity(key));
			((UnityEvent<float>)(object)option.intensitySlider.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
			{
				option.SetIntensity(value);
			});
			Navigation navigation = ((Selectable)option.intensityWrapper).navigation;
			((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)val;
			((Selectable)option.intensityWrapper).navigation = navigation;
			navigation = ((Selectable)val).navigation;
			((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)option.intensityWrapper;
			((Selectable)val).navigation = navigation;
			val = (option.durationContainer.gameObject.activeSelf ? option.durationWrapper : option.intensityWrapper);
			if ((Object)(object)val == null)
			{
				Debug.LogError("Previous button is null");
			}
		}
		Navigation navigation2 = ((Selectable)val).navigation;
		((Navigation)(ref navigation2)).selectOnDown = (Selectable)(object)quitButton;
		((Selectable)val).navigation = navigation2;
		Navigation navigation3 = ((Selectable)quitButton).navigation;
		((Navigation)(ref navigation3)).selectOnUp = (Selectable)(object)val;
		((Selectable)quitButton).navigation = navigation3;
		totalSlider.SetValueWithoutNotify(MonoSingleton<PrefsManager>.Instance.GetFloat("totalRumbleIntensity"));
	}

	public void ChangeMasterMulti(float value)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("totalRumbleIntensity", value);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}
}
