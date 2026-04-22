using UnityEngine;

public class MuteWhenPaused : MonoBehaviour
{
	private AudioSource aud;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
		aud.mute = Time.deltaTime == 0f;
	}

	private void Update()
	{
		aud.mute = Time.deltaTime == 0f;
	}
}
