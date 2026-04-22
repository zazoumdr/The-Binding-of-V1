using UnityEngine;

public class Radio : MonoBehaviour
{
	public AudioClip[] songs;

	private AudioSource aud;

	private int currentSong;

	public bool dontStartFromMiddle;

	public bool randomizeOrder;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if (randomizeOrder)
		{
			for (int num = songs.Length - 1; num >= 0; num--)
			{
				int num2 = Random.Range(0, songs.Length);
				AudioClip val = songs[num];
				songs[num] = songs[num2];
				songs[num2] = val;
			}
		}
		currentSong = Random.Range(0, songs.Length);
		aud.clip = songs[currentSong];
		aud.Play(tracked: true);
		if (!dontStartFromMiddle)
		{
			aud.time = Random.Range(0f, aud.clip.length);
		}
	}

	private void Update()
	{
		if (aud.time >= aud.clip.length - 0.01f || !aud.isPlaying)
		{
			NextSong();
		}
	}

	public void NextSong()
	{
		currentSong++;
		if (currentSong >= songs.Length)
		{
			currentSong = 0;
		}
		aud.clip = songs[currentSong];
		aud.time = 0f;
		aud.Play(tracked: true);
	}
}
