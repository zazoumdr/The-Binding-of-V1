using UnityEngine;

public class BigDoor : MonoBehaviour
{
	public bool open;

	[HideInInspector]
	public bool gotPos;

	public Vector3 openRotation;

	[HideInInspector]
	public Quaternion targetOpenRotation;

	[HideInInspector]
	public Quaternion origRotation;

	public float speed;

	private float tempSpeed;

	public float gradualSpeedMultiplier;

	private CameraController cc;

	public bool screenShake;

	private AudioSource aud;

	public AudioClip openSound;

	public AudioClip closeSound;

	private float origPitch;

	public Light openLight;

	public bool reverseDirection;

	private Door controller;

	public bool playerSpeedMultiplier;

	private void Awake()
	{
		if (!gotPos)
		{
			targetOpenRotation.eulerAngles = base.transform.localRotation.eulerAngles + openRotation;
			origRotation = base.transform.localRotation;
			gotPos = true;
		}
		cc = MonoSingleton<CameraController>.Instance;
		aud = GetComponent<AudioSource>();
		if ((bool)(Object)(object)aud)
		{
			origPitch = aud.GetPitch();
		}
		controller = GetComponentInParent<Door>();
		tempSpeed = speed;
		if (open)
		{
			base.transform.localRotation = targetOpenRotation;
		}
	}

	private void Update()
	{
		if (gradualSpeedMultiplier != 0f)
		{
			if ((open && base.transform.localRotation != targetOpenRotation) || (!open && base.transform.localRotation != origRotation))
			{
				tempSpeed += Time.deltaTime * tempSpeed * gradualSpeedMultiplier;
			}
			else
			{
				tempSpeed = speed;
			}
		}
		if (open && base.transform.localRotation != targetOpenRotation)
		{
			base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, targetOpenRotation, Time.deltaTime * (playerSpeedMultiplier ? Mathf.Max(tempSpeed, tempSpeed * (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(trueVelocity: true).magnitude / (float)(MonoSingleton<NewMovement>.Instance.ridingRocket ? 5 : 15))) : tempSpeed));
			if (screenShake)
			{
				cc.CameraShake(0.05f);
			}
			if (base.transform.localRotation == targetOpenRotation)
			{
				if ((bool)(Object)(object)aud)
				{
					aud.clip = closeSound;
					aud.loop = false;
					aud.SetPitch(Random.Range(origPitch - 0.1f, origPitch + 0.1f));
					aud.Play(tracked: true);
				}
				if ((bool)controller)
				{
					controller.onFullyOpened?.Invoke();
					controller.isFullyOpened = true;
				}
			}
		}
		else
		{
			if (open || !(base.transform.localRotation != origRotation))
			{
				return;
			}
			base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, origRotation, Time.deltaTime * (playerSpeedMultiplier ? Mathf.Max(tempSpeed, tempSpeed * (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(trueVelocity: true).magnitude / (float)(MonoSingleton<NewMovement>.Instance.ridingRocket ? 5 : 15))) : tempSpeed));
			if (screenShake)
			{
				cc.CameraShake(0.05f);
			}
			if (base.transform.localRotation == origRotation)
			{
				if ((bool)(Object)(object)aud)
				{
					aud.clip = closeSound;
					aud.loop = false;
					aud.SetPitch(Random.Range(origPitch - 0.1f, origPitch + 0.1f));
					aud.Play(tracked: true);
				}
				if ((bool)controller && controller.doorType != DoorType.Normal)
				{
					controller.BigDoorClosed();
				}
				if (openLight != null)
				{
					openLight.enabled = false;
				}
			}
		}
	}

	public void Open()
	{
		if (open)
		{
			return;
		}
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origPitch = aud.GetPitch();
			}
		}
		open = true;
		if ((bool)controller)
		{
			controller.isFullyClosed = false;
		}
		if ((bool)(Object)(object)aud)
		{
			aud.clip = openSound;
			aud.loop = true;
			aud.SetPitch(Random.Range(origPitch - 0.1f, origPitch + 0.1f));
			aud.Play(tracked: true);
		}
		if (Quaternion.Angle(base.transform.localRotation, origRotation) < 20f)
		{
			targetOpenRotation.eulerAngles = origRotation.eulerAngles + openRotation * ((!reverseDirection) ? 1 : (-1));
		}
	}

	public void Close()
	{
		if (open)
		{
			open = false;
			if ((bool)controller)
			{
				controller.isFullyOpened = false;
			}
			if ((bool)(Object)(object)aud)
			{
				aud.clip = openSound;
				aud.loop = true;
				aud.SetPitch(Random.Range(origPitch / 2f - 0.1f, origPitch / 2f + 0.1f));
				aud.Play(tracked: true);
			}
		}
	}
}
