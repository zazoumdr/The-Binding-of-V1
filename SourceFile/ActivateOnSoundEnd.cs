using UnityEngine;

public class ActivateOnSoundEnd : MonoBehaviour
{
	private AudioSource aud;

	private bool hasStarted;

	[SerializeField]
	private UltrakillEvent events;

	[SerializeField]
	private bool dontWaitForStart;

	[SerializeField]
	private bool oneTime;

	private bool activated;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (aud.isPlaying || dontWaitForStart)
		{
			hasStarted = true;
		}
		if (hasStarted && (!activated || oneTime) && ((!aud.isPlaying && aud.time == 0f) || aud.time > aud.clip.length - 0.025f))
		{
			activated = true;
			hasStarted = false;
			events.Invoke();
			if (oneTime)
			{
				base.enabled = false;
			}
		}
	}
}
