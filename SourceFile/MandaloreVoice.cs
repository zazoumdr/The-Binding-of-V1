using UnityEngine;

public class MandaloreVoice : MonoBehaviour
{
	private AudioSource aud;

	public bool talking;

	public bool dying;

	public AudioClip secondPhase;

	public AudioClip thirdPhase;

	public AudioClip finalPhase;

	public AudioClip death;

	public AudioClip[] taunts;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!aud.isPlaying)
		{
			talking = false;
		}
	}

	public void SecondPhase()
	{
		aud.Stop();
		aud.PlayOneShot(secondPhase, tracked: true);
		talking = true;
	}

	public void ThirdPhase()
	{
		aud.Stop();
		aud.PlayOneShot(thirdPhase, tracked: true);
		talking = true;
	}

	public void FinalPhase()
	{
		aud.Stop();
		aud.PlayOneShot(finalPhase, tracked: true);
		talking = true;
	}

	public void Death()
	{
		aud.Stop();
		aud.PlayOneShot(death, tracked: true);
		talking = true;
		dying = true;
	}

	public void Taunt(int num)
	{
		aud.Stop();
		if ((Object)(object)taunts[num] != null)
		{
			aud.PlayOneShot(taunts[num], tracked: true);
		}
		talking = true;
	}
}
