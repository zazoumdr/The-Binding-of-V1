using System.Collections.Generic;
using UnityEngine;

public class WaveMenu : MonoBehaviour
{
	public List<WaveSetter> setters = new List<WaveSetter>();

	public WaveCustomSetter customSetter;

	private int _highestWave = -1;

	private int startWave;

	public int highestWave
	{
		get
		{
			return _highestWave;
		}
		private set
		{
			_highestWave = value;
		}
	}

	private void Start()
	{
		int? highestWaveForDifficulty = WaveUtils.GetHighestWaveForDifficulty(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));
		foreach (WaveSetter setter in setters)
		{
			if (WaveUtils.IsWaveSelectable(setter.wave, highestWaveForDifficulty.GetValueOrDefault()))
			{
				setter.state = ButtonState.Unselected;
			}
			else if (setter.wave == startWave)
			{
				setter.state = ButtonState.Selected;
			}
			else
			{
				setter.state = ButtonState.Locked;
			}
		}
		if (!highestWaveForDifficulty.HasValue)
		{
			customSetter.gameObject.SetActive(value: false);
			SetCurrentWave(0);
			return;
		}
		highestWave = highestWaveForDifficulty.Value;
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.startingWave");
		startWave = (WaveUtils.IsWaveSelectable(num, highestWave) ? num : 0);
		if (highestWave >= 60)
		{
			customSetter.gameObject.SetActive(value: true);
			customSetter.wave = ((startWave >= 30) ? startWave : 30);
		}
		else
		{
			customSetter.gameObject.SetActive(value: false);
		}
		SetCurrentWave(startWave);
	}

	private void GetHighestWave()
	{
		int? highestWaveForDifficulty = WaveUtils.GetHighestWaveForDifficulty(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));
		if (highestWaveForDifficulty.HasValue)
		{
			highestWave = highestWaveForDifficulty.Value;
			int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.startingWave");
			Debug.Log("Wanted wave: " + num);
			startWave = (WaveUtils.IsWaveSelectable(num, highestWave) ? num : 0);
			MonoSingleton<EndlessGrid>.Instance.startWave = startWave;
		}
	}

	public void SetCurrentWave(int wave)
	{
		if (wave * 2 > highestWave)
		{
			return;
		}
		startWave = wave;
		MonoSingleton<EndlessGrid>.Instance.startWave = startWave;
		MonoSingleton<PrefsManager>.Instance.SetInt("cyberGrind.startingWave", wave);
		foreach (WaveSetter setter in setters)
		{
			if (setter.state != ButtonState.Locked)
			{
				setter.state = ((setter.wave != wave) ? ButtonState.Unselected : ButtonState.Selected);
			}
		}
		customSetter.state = ((wave < 30) ? WaveCustomSetter.ButtonState.Unselected : WaveCustomSetter.ButtonState.Selected);
	}
}
