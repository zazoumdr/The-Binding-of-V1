using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[DefaultExecutionOrder(600)]
public class MusicManager : MonoSingleton<MusicManager>
{
	public bool off;

	public bool dontMatch;

	public bool useBossTheme;

	public AudioSource battleTheme;

	public AudioSource cleanTheme;

	public AudioSource bossTheme;

	public AudioSource targetTheme;

	private AudioSource[] allThemes;

	public float volume = 1f;

	public float requestedThemes;

	private bool arenaMode;

	private float defaultVolume;

	public float fadeSpeed;

	private float fadeOutSpeed;

	public bool forcedOff;

	private bool filtering;

	private bool falseStartToken;

	private void OnEnable()
	{
		if (fadeSpeed == 0f)
		{
			fadeSpeed = 1f;
		}
		allThemes = GetComponentsInChildren<AudioSource>();
		defaultVolume = volume;
		if (!off)
		{
			AudioSource[] array = allThemes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play(tracked: true);
			}
			cleanTheme.volume = volume;
			targetTheme = cleanTheme;
		}
		else
		{
			targetTheme = GetComponent<AudioSource>();
		}
		if ((bool)(Object)(object)MonoSingleton<AudioMixerController>.Instance.musicSound)
		{
			MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0f);
		}
	}

	private void Update()
	{
		if (!off && targetTheme.volume != volume)
		{
			AudioSource[] array = allThemes;
			foreach (AudioSource val in array)
			{
				if ((Object)(object)val == (Object)(object)targetTheme)
				{
					if (val.volume > volume)
					{
						val.volume = volume;
					}
					if (Time.timeScale == 0f)
					{
						val.volume = volume;
					}
					else
					{
						val.volume = Mathf.MoveTowards(val.volume, volume, fadeSpeed * Time.deltaTime);
					}
				}
				else if (Time.timeScale == 0f)
				{
					val.volume = 0f;
				}
				else
				{
					val.volume = Mathf.MoveTowards(val.volume, 0f, fadeSpeed * Time.deltaTime);
				}
			}
			if (targetTheme.volume == volume)
			{
				array = allThemes;
				foreach (AudioSource val2 in array)
				{
					if ((Object)(object)val2 != (Object)(object)targetTheme)
					{
						val2.volume = 0f;
					}
				}
			}
		}
		if (filtering)
		{
			float current = default(float);
			MonoSingleton<AudioMixerController>.Instance.musicSound.GetFloat("highPassVolume", ref current);
			current = Mathf.MoveTowards(current, 0f, 1200f * Time.unscaledDeltaTime);
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", current);
			if (current == 0f)
			{
				filtering = false;
			}
		}
		if (volume == 0f || off)
		{
			AudioSource[] array = allThemes;
			foreach (AudioSource obj in array)
			{
				obj.volume = Mathf.MoveTowards(obj.volume, 0f, Time.deltaTime / 5f * fadeSpeed * fadeOutSpeed);
			}
		}
	}

	public void ForceStartBattleMusic()
	{
		forcedOff = false;
		ArenaMusicStart();
	}

	public void ForceStartMusic()
	{
		forcedOff = false;
		StartMusic();
	}

	public void StartMusic()
	{
		if (forcedOff || !off)
		{
			return;
		}
		AudioSource[] array = allThemes;
		foreach (AudioSource val in array)
		{
			if ((Object)(object)val.clip != null)
			{
				val.Play(tracked: true);
				if (off && val.time != 0f)
				{
					val.time = 0f;
				}
			}
		}
		off = false;
		fadeOutSpeed = 1f;
		if (!arenaMode && requestedThemes <= 0f)
		{
			cleanTheme.volume = volume;
			targetTheme = cleanTheme;
			battleTheme.volume = 0f;
			bossTheme.volume = 0f;
		}
		else
		{
			battleTheme.volume = volume;
			targetTheme = battleTheme;
			cleanTheme.volume = 0f;
			bossTheme.volume = 0f;
		}
	}

	public void PlayBattleMusic()
	{
		if (!dontMatch && (Object)(object)targetTheme != (Object)(object)battleTheme && (Object)(object)cleanTheme.clip != null)
		{
			battleTheme.time = cleanTheme.time;
		}
		if ((Object)(object)targetTheme != (Object)(object)bossTheme)
		{
			targetTheme = battleTheme;
		}
		if (falseStartToken)
		{
			falseStartToken = false;
		}
		else
		{
			requestedThemes += 1f;
		}
	}

	public void PlayCleanMusic()
	{
		requestedThemes -= 1f;
		if (requestedThemes <= 0f && !arenaMode)
		{
			requestedThemes = 0f;
			if (!dontMatch && (Object)(object)targetTheme != (Object)(object)cleanTheme && (Object)(object)battleTheme.clip != null)
			{
				cleanTheme.time = battleTheme.time;
			}
			if (battleTheme.volume == volume)
			{
				cleanTheme.time = battleTheme.time;
			}
			targetTheme = cleanTheme;
		}
	}

	public void PlayBossMusic()
	{
		if ((Object)(object)targetTheme != (Object)(object)bossTheme)
		{
			bossTheme.time = cleanTheme.time;
		}
		targetTheme = bossTheme;
	}

	public void ArenaMusicStart(bool goIntoArenaMode = true)
	{
		if (forcedOff)
		{
			return;
		}
		if (off)
		{
			AudioSource[] array = allThemes;
			foreach (AudioSource val in array)
			{
				if ((Object)(object)val.clip != null)
				{
					val.Play(tracked: true);
					if (off && val.time != 0f)
					{
						val.time = 0f;
					}
				}
			}
			off = false;
			fadeOutSpeed = 1f;
			battleTheme.volume = volume;
			targetTheme = battleTheme;
		}
		if (!battleTheme.isPlaying)
		{
			AudioSource[] array = allThemes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play(tracked: true);
			}
			battleTheme.volume = volume;
		}
		if ((Object)(object)targetTheme != (Object)(object)bossTheme)
		{
			targetTheme = battleTheme;
		}
		if (goIntoArenaMode)
		{
			arenaMode = true;
		}
		else if (requestedThemes <= 0f)
		{
			falseStartToken = true;
			requestedThemes += 1f;
		}
	}

	public void ArenaMusicEnd()
	{
		requestedThemes = 0f;
		targetTheme = cleanTheme;
		arenaMode = false;
	}

	public void ForceStopMusic()
	{
		forcedOff = true;
		StopMusic();
	}

	public void StopMusic()
	{
		off = true;
		if (allThemes != null && allThemes.Length != 0)
		{
			AudioSource[] array = allThemes;
			foreach (AudioSource obj in array)
			{
				obj.volume = 0f;
				obj.Stop();
			}
		}
	}

	public void FadeOut(float newFadeoutSpeed)
	{
		off = true;
		fadeOutSpeed = newFadeoutSpeed;
	}

	public void FilterMusic()
	{
		MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
		CancelInvoke("RemoveHighPass");
		MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Paused").TransitionTo(0f);
		filtering = true;
	}

	public void UnfilterMusic()
	{
		filtering = false;
		MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0.5f);
		Invoke("RemoveHighPass", 0.5f);
	}

	private void RemoveHighPass()
	{
		MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
	}

	public bool IsInBattle()
	{
		if (!arenaMode)
		{
			return requestedThemes > 0f;
		}
		return true;
	}

	public void ChangeFadeSpeed(float speed)
	{
		fadeSpeed = speed;
	}
}
