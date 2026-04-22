using UnityEngine;
using UnityEngine.Audio;

public static class OneShotAudioExtension
{
	public static AudioSource PlayClipAtPoint(this AudioClip clip, AudioMixerGroup mixGroup, Vector3 position, int priority = 128, float spatialBlend = 0f, float volume = 1f, float pitch = 1f, AudioRolloffMode rolloffMode = (AudioRolloffMode)1, float minimumDistance = 1f, float maximumDistance = 100f)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = new GameObject("TempAudio");
		gameObject.transform.position = position;
		AudioSource obj = gameObject.AddComponent<AudioSource>();
		obj.clip = clip;
		obj.outputAudioMixerGroup = mixGroup;
		obj.priority = priority;
		obj.volume = volume;
		obj.SetPitch(pitch);
		obj.SetSpatialBlend(spatialBlend);
		obj.rolloffMode = rolloffMode;
		obj.minDistance = minimumDistance;
		obj.maxDistance = maximumDistance;
		obj.Play(tracked: true);
		Object.Destroy(gameObject, clip.length / pitch);
		return obj;
	}
}
