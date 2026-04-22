using UnityEngine;

public class SoundPitch : MonoBehaviour
{
	private AudioSource aud;

	public float targetPitch;

	public float speed;

	public bool notOnEnable;

	private bool activated;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if (!notOnEnable)
		{
			activated = true;
		}
	}

	private void Update()
	{
		if ((bool)(Object)(object)aud && activated)
		{
			aud.SetPitch(Mathf.MoveTowards(aud.GetPitch(), targetPitch, speed * Time.deltaTime));
		}
	}

	public void Activate()
	{
		activated = true;
	}

	public void Deactivate()
	{
		activated = false;
	}

	public void ChangePitch(float newPitch)
	{
		targetPitch = newPitch;
	}

	public void ChangeSpeed(float newSpeed)
	{
		speed = newSpeed;
	}
}
