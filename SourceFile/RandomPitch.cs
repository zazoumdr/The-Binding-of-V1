using UnityEngine;

public class RandomPitch : MonoBehaviour
{
	public float defaultPitch = 1f;

	public float pitchVariation = 0.1f;

	public bool oneTime = true;

	public bool playOnEnable = true;

	public bool nailgunOverheatFix;

	[HideInInspector]
	public bool beenPlayed;

	public AudioSource aud;

	private void Start()
	{
		if (nailgunOverheatFix)
		{
			Randomize();
		}
	}

	private void OnEnable()
	{
		if (!nailgunOverheatFix)
		{
			Randomize();
		}
	}

	public void Randomize()
	{
		if (oneTime && beenPlayed)
		{
			return;
		}
		beenPlayed = true;
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if ((Object)(object)aud != null)
		{
			if (pitchVariation == 0f)
			{
				aud.SetPitch(Random.Range(0.8f, 1.2f));
			}
			else
			{
				aud.SetPitch(Random.Range(defaultPitch - pitchVariation, defaultPitch + pitchVariation));
			}
			if (playOnEnable)
			{
				aud.Play(tracked: true);
			}
		}
	}
}
