using UnityEngine;

public class RandomAudioclip : MonoBehaviour
{
	public AudioClip[] clips;

	private AudioSource aud;

	public bool playOnChange;

	public bool activateOnEnable = true;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		if (activateOnEnable)
		{
			Activate();
		}
	}

	public void Activate()
	{
		aud.clip = clips[Random.Range(0, clips.Length)];
		if (playOnChange)
		{
			aud.Play(tracked: true);
		}
	}
}
