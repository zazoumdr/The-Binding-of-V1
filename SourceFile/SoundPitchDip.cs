using UnityEngine;

public class SoundPitchDip : MonoBehaviour
{
	private AudioSource aud;

	private bool dipping;

	private float origPitch;

	private float target;

	public float speed;

	public bool onEnable;

	private void OnEnable()
	{
		if (onEnable)
		{
			Dip(0f);
		}
	}

	public void Dip(float pitch)
	{
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origPitch = aud.GetPitch();
				aud.SetPitch(pitch);
				target = origPitch;
				dipping = true;
			}
		}
	}

	public void DipToZero()
	{
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origPitch = aud.GetPitch();
				target = 0f;
				dipping = true;
			}
		}
	}

	private void Update()
	{
		if (!dipping)
		{
			return;
		}
		aud.SetPitch(Mathf.MoveTowards(aud.GetPitch(), target, Time.deltaTime * speed));
		if (aud.GetPitch() == target)
		{
			dipping = false;
			if (aud.GetPitch() == 0f)
			{
				aud.mute = true;
			}
		}
		else
		{
			aud.mute = false;
		}
	}
}
