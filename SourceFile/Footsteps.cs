using UnityEngine;

public class Footsteps : MonoBehaviour
{
	public bool dontInstantiate;

	public GameObject footstep;

	private AudioSource aud;

	public AudioClip[] steps;

	private int previousStep;

	private void Awake()
	{
		if (dontInstantiate)
		{
			aud = GetComponent<AudioSource>();
		}
	}

	public void Footstep()
	{
		if (dontInstantiate)
		{
			int num = Random.Range(0, steps.Length);
			if (steps.Length > 1 && num == previousStep)
			{
				num = (num + 1) % steps.Length;
			}
			aud.clip = steps[num];
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.Play(tracked: true);
		}
		else
		{
			Object.Instantiate(footstep, base.transform.position, base.transform.rotation);
		}
	}
}
