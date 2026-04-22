using UnityEngine;

public class AudioCopyVolumeAndTime : MonoBehaviour
{
	public AudioSource target;

	private AudioSource aud;

	public bool copyTime = true;

	public bool copyVolume = true;

	public bool copyOnEnable = true;

	public bool copyOnUpdate;

	private bool volumeIsZero;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		if (copyOnEnable)
		{
			Match();
		}
	}

	private void Update()
	{
		if (copyOnUpdate)
		{
			Match();
		}
		else if (aud.volume == 0f)
		{
			volumeIsZero = true;
		}
		else if (volumeIsZero)
		{
			volumeIsZero = false;
			Match();
		}
	}

	public void Match()
	{
		if (!aud.isPlaying)
		{
			aud.Play(tracked: true);
		}
		if (copyTime)
		{
			aud.timeSamples = target.timeSamples;
		}
		if (copyVolume)
		{
			aud.volume = target.volume;
		}
	}
}
