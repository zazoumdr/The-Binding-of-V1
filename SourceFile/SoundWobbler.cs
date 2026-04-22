using UnityEngine;

public class SoundWobbler : MonoBehaviour
{
	private AudioSource aud;

	public float wobbleTime;

	public float wobbleSpeed;

	public bool wobbleUp;

	private void Awake()
	{
		aud = GetComponentInChildren<AudioSource>();
		Invoke("ChangeWobble", wobbleTime);
	}

	private void Update()
	{
		if (wobbleUp)
		{
			aud.SetPitch(aud.GetPitch() + wobbleSpeed * Time.deltaTime);
		}
		else
		{
			aud.SetPitch(aud.GetPitch() - wobbleSpeed * Time.deltaTime);
		}
	}

	private void ChangeWobble()
	{
		if (wobbleUp)
		{
			wobbleUp = false;
		}
		else
		{
			wobbleUp = true;
		}
		Invoke("ChangeWobble", wobbleTime);
	}
}
