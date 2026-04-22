using System;
using UnityEngine;
using UnityEngine.Audio;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[DefaultExecutionOrder(-10)]
public class AudioMixerController : MonoSingleton<AudioMixerController>
{
	[Header("Mixers")]
	public AudioMixer allSound;

	public AudioMixer goreSound;

	public AudioMixer musicSound;

	public AudioMixer doorSound;

	public AudioMixer unfreezeableSound;

	[Header("Mixer Groups")]
	public AudioMixerGroup allGroup;

	public AudioMixerGroup goreGroup;

	public AudioMixerGroup musicGroup;

	public AudioMixerGroup doorGroup;

	public AudioMixerGroup unfreezeableGroup;

	[HideInInspector]
	public float sfxVolume;

	[HideInInspector]
	public float musicVolume;

	[HideInInspector]
	public float optionsMusicVolume;

	[HideInInspector]
	public bool muffleMusic;

	[Space]
	public bool forceOff;

	private float temporaryDipAmount;

	private bool isUnderWater;

	private void Start()
	{
		sfxVolume = MonoSingleton<PrefsManager>.Instance.GetFloat("sfxVolume");
		SetSFXVolume(sfxVolume);
		optionsMusicVolume = MonoSingleton<PrefsManager>.Instance.GetFloat("musicVolume");
		SetMusicVolume(optionsMusicVolume);
		muffleMusic = MonoSingleton<PrefsManager>.Instance.GetBool("muffleMusic");
		if (!forceOff)
		{
			SetSFXVolume(sfxVolume);
			musicSound.SetFloat("allVolume", CalculateVolume(optionsMusicVolume));
		}
		IsInWater(isInWater: false);
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
		switch (key)
		{
		case "allVolume":
			AudioListener.volume = (float)value;
			break;
		case "sfxVolume":
			if (value is float sFXVolume)
			{
				SetSFXVolume(sFXVolume);
			}
			break;
		case "musicVolume":
			if (value is float num)
			{
				optionsMusicVolume = num;
				SetMusicVolume(num);
			}
			break;
		case "muffleMusic":
			if (value is bool isOn)
			{
				MuffleMusic(isOn);
			}
			break;
		}
	}

	private void Update()
	{
		if (musicVolume > optionsMusicVolume)
		{
			SetMusicVolume(optionsMusicVolume);
		}
		UpdateSFXVolume();
	}

	public void SetMusicVolume(float volume)
	{
		if (!forceOff)
		{
			musicSound.SetFloat("allVolume", CalculateVolume(volume));
		}
		musicVolume = volume;
	}

	public void SetSFXVolume(float volume)
	{
		sfxVolume = volume;
		UpdateSFXVolume();
	}

	public void UpdateSFXVolume()
	{
		float num = default(float);
		if (!forceOff)
		{
			allSound.SetFloat("allVolume", CalculateVolume((allSound.GetFloat("allPitch", ref num) && num == 0f) ? 0f : sfxVolume) + temporaryDipAmount);
		}
		goreSound.SetFloat("allVolume", CalculateVolume((goreSound.GetFloat("allPitch", ref num) && num == 0f) ? 0f : sfxVolume) + temporaryDipAmount);
		doorSound.SetFloat("allVolume", CalculateVolume((doorSound.GetFloat("allPitch", ref num) && num == 0f) ? 0f : sfxVolume) + temporaryDipAmount);
		unfreezeableSound.SetFloat("allVolume", CalculateVolume((unfreezeableSound.GetFloat("allPitch", ref num) && num == 0f) ? 0f : sfxVolume) + temporaryDipAmount);
	}

	public void TemporaryDip(float amount)
	{
		temporaryDipAmount = amount;
		SetSFXVolume(sfxVolume);
	}

	public float CalculateVolume(float volume)
	{
		if (volume > 0f)
		{
			return Mathf.Log10(volume) * 20f;
		}
		return -80f;
	}

	public void IsInWater(bool isInWater)
	{
		float num = ((!isInWater) ? (-80) : 0);
		isUnderWater = isInWater;
		allSound.SetFloat("lowPassVolume", num);
		if (muffleMusic || !isInWater)
		{
			musicSound.SetFloat("lowPassVolume", num);
		}
		goreSound.SetFloat("lowPassVolume", num);
		doorSound.SetFloat("lowPassVolume", num);
		unfreezeableSound.SetFloat("lowPassVolume", num);
	}

	public void MuffleMusic(bool isOn)
	{
		muffleMusic = isOn;
		if (!isOn)
		{
			musicSound.SetFloat("lowPassVolume", -80f);
		}
		else if (isUnderWater)
		{
			musicSound.SetFloat("lowPassVolume", 0f);
		}
	}
}
