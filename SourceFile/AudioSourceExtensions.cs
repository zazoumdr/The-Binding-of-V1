using Interop;
using PrivateAPIBridge;
using UnityEngine;

public static class AudioSourceExtensions
{
	public static void Play(this AudioSource @this, bool tracked)
	{
		@this.Play();
		if (tracked && MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.AddAudioSource(@this);
		}
	}

	public static void PlayOneShot(this AudioSource @this, AudioClip clip, bool tracked)
	{
		@this.PlayOneShot(clip, 1f, tracked);
	}

	public static void PlayOneShot(this AudioSource @this, AudioClip clip, float volumeScale, bool tracked)
	{
		@this.PlayOneShot(clip, volumeScale);
		if (tracked && MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.AddAudioSource(@this);
		}
	}

	public static void PlayScheduled(this AudioSource @this, double time, bool tracked)
	{
		@this.PlayScheduled(time);
		if (tracked && MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.AddAudioSource(@this);
		}
	}

	public static void PlayDelayed(this AudioSource @this, float delay, bool tracked)
	{
		@this.PlayDelayed(delay);
		if (tracked && MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.AddAudioSource(@this);
		}
	}

	public unsafe static bool IsPaused(this AudioSource @this)
	{
		return ((AudioSource)(void*)ObjectExtensions.GetCachedPtr((Object)(object)@this)).m_pause != 0;
	}

	public static void SetPitch(this AudioSource @this, float pitch)
	{
		if (((Component)(object)@this).TryGetComponent(out VirtualAudioFilter component))
		{
			component.pitch = pitch;
		}
		else
		{
			@this.pitch = pitch;
		}
	}

	public static float GetPitch(this AudioSource @this)
	{
		if (((Component)(object)@this).TryGetComponent(out VirtualAudioFilter component))
		{
			return component.pitch;
		}
		return @this.pitch;
	}

	public static void SetPlayOnAwake(this AudioSource @this, bool playOnAwake)
	{
		@this.playOnAwake = playOnAwake;
		if (playOnAwake && MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager instance))
		{
			instance.AddAudioSource(@this);
		}
	}

	public static void SetSpatialBlend(this AudioSource @this, float spatialBlend)
	{
		if (((Component)(object)@this).TryGetComponent(out VirtualAudioFilter component))
		{
			component.spatialBlend = spatialBlend;
		}
		else
		{
			@this.spatialBlend = spatialBlend;
		}
	}

	public static float GetSpatialBlend(this AudioSource @this)
	{
		if (((Component)(object)@this).TryGetComponent(out VirtualAudioFilter component))
		{
			return component.spatialBlend;
		}
		return @this.spatialBlend;
	}
}
