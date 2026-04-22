using UnityEngine;

public class RotateBy : MonoBehaviour
{
	public Vector3 rotation;

	public float speed;

	[HideInInspector]
	public bool gotValues;

	[HideInInspector]
	public Vector3 targetRotation;

	[HideInInspector]
	public bool rotating;

	[HideInInspector]
	public float rotationLeft;

	[HideInInspector]
	public Vector3 originalRotation;

	[HideInInspector]
	public AudioSource aud;

	[HideInInspector]
	public float origPitch;

	public AudioClip rotateSound;

	public AudioClip stopSound;

	public Transform[] setToTarget;

	public UltrakillEvent onDoneRotating;

	private void Start()
	{
		GetValues();
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			originalRotation = base.transform.rotation.eulerAngles;
			targetRotation = base.transform.rotation.eulerAngles;
			aud = GetComponent<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origPitch = aud.GetPitch();
			}
		}
	}

	private void Update()
	{
		Tick(Time.deltaTime);
	}

	private void Tick(float time)
	{
		if (rotating)
		{
			float num = Mathf.Min(rotationLeft, speed * time);
			base.transform.Rotate(targetRotation.normalized * num);
			rotationLeft -= num;
			if (rotationLeft <= 0f)
			{
				onDoneRotating?.Invoke();
				rotating = false;
				PlaySound(stopSound, loop: false);
			}
		}
	}

	public void AddRotation()
	{
		Rotate(rotation);
	}

	public void AddRotationCustom(float degrees)
	{
		Rotate(rotation.normalized * degrees);
	}

	public void AddRotationCustom(Vector3 customRotation)
	{
		Rotate(customRotation);
	}

	private void Rotate(Vector3 rotation)
	{
		if (targetRotation.normalized != rotation.normalized)
		{
			Tick(rotationLeft);
		}
		targetRotation = rotation;
		rotationLeft += targetRotation.magnitude;
		rotating = true;
		PlaySound(rotateSound, loop: true);
		Transform[] array = setToTarget;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Rotate(rotation);
		}
	}

	private void PlaySound(AudioClip sound, bool loop)
	{
		if (!((Object)(object)aud == null) && !((Object)(object)sound == null))
		{
			aud.clip = sound;
			aud.loop = loop;
			aud.SetPitch(Random.Range(origPitch - 0.1f, origPitch + 0.1f));
			aud.Play(tracked: true);
		}
	}

	public void ResetRotation(bool instant)
	{
		if (!gotValues)
		{
			return;
		}
		targetRotation = originalRotation;
		if (instant)
		{
			base.transform.rotation = Quaternion.Euler(originalRotation);
			rotationLeft = 0f;
			if (rotating)
			{
				rotating = false;
				AudioSource obj = aud;
				if (obj != null)
				{
					obj.Stop();
				}
			}
		}
		else
		{
			rotating = true;
			rotationLeft = targetRotation.magnitude;
			PlaySound(rotateSound, loop: true);
		}
		Transform[] array = setToTarget;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].rotation = Quaternion.Euler(originalRotation);
		}
	}

	public void Skip()
	{
		Tick(999999f);
	}
}
