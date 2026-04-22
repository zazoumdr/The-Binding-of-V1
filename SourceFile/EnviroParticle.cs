using System;
using UnityEngine;

public class EnviroParticle : MonoBehaviour
{
	private ParticleSystem part;

	[HideInInspector]
	public bool stopped;

	private void Start()
	{
		part = GetComponent<ParticleSystem>();
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableEnvironmentParticles") && part.isPlaying && !stopped)
		{
			stopped = true;
			part.Stop();
			part.Clear();
		}
	}

	private void Update()
	{
		if (stopped && part.isPlaying)
		{
			part.Stop();
			part.Clear();
		}
	}

	private void OnEnable()
	{
		CheckEnviroParticles();
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "disableEnvironmentParticles")
		{
			CheckEnviroParticles();
		}
	}

	public void CheckEnviroParticles()
	{
		if ((UnityEngine.Object)(object)part == null)
		{
			part = GetComponent<ParticleSystem>();
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disableEnvironmentParticles") && part.isPlaying && !stopped)
		{
			stopped = true;
			part.Stop();
			part.Clear();
		}
		else if (stopped)
		{
			stopped = false;
			part.Play();
		}
	}
}
