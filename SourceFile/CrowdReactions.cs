using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CrowdReactions : MonoSingleton<CrowdReactions>
{
	private AudioSource aud;

	public AudioClip cheer;

	public AudioClip cheerLong;

	public AudioClip aww;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	public void React(AudioClip clip)
	{
		if ((Object)(object)aud.clip != (Object)(object)cheerLong || !aud.isPlaying)
		{
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.clip = clip;
			aud.Play(tracked: true);
		}
	}
}
