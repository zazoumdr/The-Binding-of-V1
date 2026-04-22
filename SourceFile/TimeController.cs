using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class TimeController : MonoSingleton<TimeController>
{
	[SerializeField]
	private GameObject parryLight;

	[SerializeField]
	private GameObject parryFlash;

	private float currentStop;

	private AudioMixer[] audmix;

	[HideInInspector]
	public bool controlTimeScale = true;

	[HideInInspector]
	public bool controlPitch = true;

	[HideInInspector]
	public bool parryFlashEnabled = true;

	public float timeScale = 1f;

	public float timeScaleModifier = 1f;

	private float slowDown = 1f;

	private void Awake()
	{
		InitializeValues();
	}

	public void InitializeValues()
	{
		audmix = (AudioMixer[])(object)new AudioMixer[4]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.goreSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound,
			MonoSingleton<AudioMixerController>.Instance.doorSound
		};
		if ((bool)MonoSingleton<AssistController>.Instance && MonoSingleton<AssistController>.Instance.majorEnabled)
		{
			timeScale = MonoSingleton<AssistController>.Instance.gameSpeed;
		}
		else
		{
			timeScale = 1f;
		}
		Time.timeScale = timeScale * timeScaleModifier;
		if (MonoSingleton<OptionsManager>.Instance.mainMenu)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", timeScale / (MonoSingleton<AssistController>.Instance.majorEnabled ? MonoSingleton<AssistController>.Instance.gameSpeed : 1f));
			}
		}
		parryFlashEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("parryFlash");
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string id, object value)
	{
		if (id == "parryFlash" && value is bool flag)
		{
			parryFlashEnabled = flag;
		}
	}

	private void Update()
	{
		if (controlTimeScale)
		{
			if (MonoSingleton<AssistController>.Instance.majorEnabled && timeScale != MonoSingleton<AssistController>.Instance.gameSpeed)
			{
				timeScale = MonoSingleton<AssistController>.Instance.gameSpeed;
				Time.timeScale = timeScale * timeScaleModifier;
			}
			else if (!MonoSingleton<AssistController>.Instance.majorEnabled && timeScale != 1f)
			{
				timeScale = 1f;
				Time.timeScale = timeScale * timeScaleModifier;
			}
		}
	}

	private void FixedUpdate()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused && !MonoSingleton<OptionsManager>.Instance.mainMenu)
		{
			return;
		}
		if (slowDown < timeScale * timeScaleModifier)
		{
			slowDown = Mathf.MoveTowards(slowDown, timeScale * timeScaleModifier, 0.02f);
			Time.timeScale = slowDown;
			if (controlPitch)
			{
				AudioMixer[] array = audmix;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetFloat("allPitch", slowDown / timeScale / (MonoSingleton<AssistController>.Instance.majorEnabled ? MonoSingleton<AssistController>.Instance.gameSpeed : 1f));
				}
			}
		}
		else if (controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", timeScale / (MonoSingleton<AssistController>.Instance.majorEnabled ? MonoSingleton<AssistController>.Instance.gameSpeed : 1f));
			}
		}
	}

	public void ParryFlash()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(parryLight, MonoSingleton<PlayerTracker>.Instance.GetTarget().position, Quaternion.identity, MonoSingleton<PlayerTracker>.Instance.GetTarget());
		Light component;
		if (parryFlashEnabled)
		{
			if (parryFlash != null)
			{
				parryFlash.SetActive(value: true);
			}
			Invoke("HideFlash", 0.1f);
		}
		else if (gameObject.TryGetComponent<Light>(out component))
		{
			component.enabled = false;
		}
		TrueStop(0.25f);
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.ParryFlash);
	}

	private void HideFlash()
	{
		parryFlash?.SetActive(value: false);
		if ((bool)MonoSingleton<CrowdReactions>.Instance && MonoSingleton<CrowdReactions>.Instance.enabled)
		{
			MonoSingleton<CrowdReactions>.Instance.React(MonoSingleton<CrowdReactions>.Instance.cheer);
		}
	}

	public void SlowDown(float amount)
	{
		if (amount <= 0f)
		{
			amount = 0.01f;
		}
		slowDown = amount;
	}

	public void HitStop(float length)
	{
		if (length > currentStop)
		{
			currentStop = length;
			Time.timeScale = 0f;
			StartCoroutine(TimeIsStopped(length, trueStop: false));
		}
	}

	public void TrueStop(float length)
	{
		if (!(length > currentStop))
		{
			return;
		}
		currentStop = length;
		if (controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", 0f);
			}
		}
		Time.timeScale = 0f;
		StartCoroutine(TimeIsStopped(length, trueStop: true));
	}

	private IEnumerator TimeIsStopped(float length, bool trueStop)
	{
		yield return new WaitForSecondsRealtime(length);
		ContinueTime(length, trueStop);
	}

	private void ContinueTime(float length, bool trueStop)
	{
		if (!(length >= currentStop))
		{
			return;
		}
		Time.timeScale = timeScale * timeScaleModifier;
		if (trueStop && controlPitch)
		{
			AudioMixer[] array = audmix;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetFloat("allPitch", 1f);
			}
		}
		currentStop = 0f;
	}

	public void RestoreTime()
	{
		Time.timeScale = timeScale * timeScaleModifier;
		currentStop = 0f;
	}

	public void SetAllPitch(float pitch)
	{
		AudioMixer[] array = audmix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat("allPitch", pitch);
		}
	}
}
