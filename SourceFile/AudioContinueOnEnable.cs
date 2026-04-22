using UnityEngine;

public class AudioContinueOnEnable : MonoBehaviour
{
	public bool autoStartIfNotPlaying = true;

	private bool wasPlaying;

	private float currentTime;

	private AudioSource aud;

	private void OnEnable()
	{
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (!aud.isPlaying && (autoStartIfNotPlaying || wasPlaying))
		{
			aud.Play(tracked: true);
			aud.time = currentTime;
		}
	}

	private void Update()
	{
		if (aud.isPlaying)
		{
			currentTime = aud.time;
			wasPlaying = true;
		}
		else
		{
			wasPlaying = false;
		}
	}
}
