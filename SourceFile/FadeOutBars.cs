using System;
using ULTRAKILL.Cheats;
using UnityEngine;

public class FadeOutBars : MonoBehaviour
{
	private bool fadeOut;

	public float fadeOutTime;

	private SliderToFillAmount[] slids;

	private void Start()
	{
		CheckState();
		slids = GetComponentsInChildren<SliderToFillAmount>();
		SliderToFillAmount[] array = slids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mama = this;
		}
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "crossHairHudFade" && value is bool)
		{
			_ = (bool)value;
			CheckState();
		}
	}

	private void Update()
	{
		if (fadeOut)
		{
			fadeOutTime = Mathf.MoveTowards(fadeOutTime, 0f, Time.unscaledDeltaTime);
		}
	}

	public void CheckState()
	{
		fadeOut = MonoSingleton<PrefsManager>.Instance.GetBool("crossHairHudFade");
		ResetTimer();
	}

	public void ResetTimer()
	{
		bool flag = MonoSingleton<PrefsManager>.Instance.GetInt("crossHairHud") == 0;
		if (!flag && HideUI.Active)
		{
			flag = true;
		}
		fadeOutTime = ((!flag) ? 2 : 0);
	}
}
