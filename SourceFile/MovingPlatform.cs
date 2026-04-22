using UnityEngine;
using UnityEngine.Serialization;

public class MovingPlatform : MonoBehaviour
{
	[HideInInspector]
	public bool infoSet;

	public Vector3[] relativePoints;

	[HideInInspector]
	public Vector3 originalPosition;

	[HideInInspector]
	public Vector3 currentPosition;

	[HideInInspector]
	public Vector3 targetPosition;

	[HideInInspector]
	public int currentPoint;

	public bool ignoreStartPosition;

	public bool useRigidbody;

	private Rigidbody rb;

	public float speed;

	[HideInInspector]
	public AudioSource aud;

	[HideInInspector]
	public float origPitch;

	[HideInInspector]
	public float origDoppler;

	[HideInInspector]
	public float intendedPitch;

	[Header("Audio")]
	public AudioClip moveSound;

	public AudioClip stopSound;

	public bool stopAudioOnDisable;

	public bool changePitchPerSpeed;

	[Header("Ease")]
	[FormerlySerializedAs("ease")]
	public bool easeIn;

	[FormerlySerializedAs("ease")]
	public bool easeOut;

	public float easeSpeedMultiplier = 1f;

	public bool highPrecisionEase;

	public bool ignoreEaseOnStart;

	[HideInInspector]
	public bool onFirstMove = true;

	[Header("Behavior")]
	public bool moveOnEnable = true;

	public bool reverseAtEnd;

	public bool teleportBackToStart;

	public bool stopAtEnd;

	public bool resetOnDisable;

	[HideInInspector]
	public bool forward = true;

	[HideInInspector]
	public bool moving;

	[HideInInspector]
	public bool waiting;

	[HideInInspector]
	public bool waitOnEnable;

	public bool randomStartPoint;

	public int startPoint;

	public float startOffset;

	public float moveDelay;

	private bool quickStartCurrentMove;

	[Header("Events")]
	public UltrakillEvent[] onReachPoint;

	private void Start()
	{
		if (useRigidbody && !rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (!rb)
		{
			useRigidbody = false;
		}
		SetInfo();
		if (!moving && moveOnEnable)
		{
			if (startOffset > 0f)
			{
				waiting = true;
			}
			Invoke("NextPoint", startOffset);
		}
	}

	private void OnEnable()
	{
		if (!infoSet)
		{
			SetInfo();
		}
		if (waitOnEnable)
		{
			waitOnEnable = false;
			waiting = true;
			Invoke("NextPoint", moveDelay);
		}
	}

	private void OnDisable()
	{
		if (!infoSet)
		{
			SetInfo();
		}
		if (stopAudioOnDisable)
		{
			aud.Stop();
		}
		if (resetOnDisable)
		{
			ResetPosition();
		}
	}

	public void ResetPosition()
	{
		if (infoSet)
		{
			if (ignoreStartPosition)
			{
				onFirstMove = true;
				currentPoint = 0;
				base.transform.position = originalPosition;
				currentPosition = originalPosition;
				targetPosition = originalPosition;
				forward = true;
				NextPoint();
			}
			else
			{
				TeleportToPoint(startPoint);
			}
			if (waiting)
			{
				waiting = false;
				CancelInvoke("NextPoint");
				waitOnEnable = true;
			}
		}
	}

	private void SetInfo()
	{
		if (infoSet)
		{
			return;
		}
		infoSet = true;
		if (relativePoints.Length >= 1 && !ignoreStartPosition)
		{
			bool flag = false;
			for (int i = 0; i < relativePoints.Length; i++)
			{
				if (relativePoints[i] == Vector3.zero)
				{
					flag = true;
					break;
				}
			}
			if (!flag && !stopAtEnd)
			{
				Vector3[] array = new Vector3[relativePoints.Length + 1];
				for (int j = 0; j < relativePoints.Length; j++)
				{
					array[j] = relativePoints[j];
				}
				array[^1] = Vector3.zero;
				relativePoints = array;
			}
		}
		if (!reverseAtEnd)
		{
			forward = true;
		}
		if (relativePoints.Length > 1)
		{
			infoSet = true;
			aud = GetComponentInChildren<AudioSource>();
			if ((bool)(Object)(object)aud)
			{
				origPitch = aud.GetPitch();
				origDoppler = aud.dopplerLevel;
				aud.velocityUpdateMode = (AudioVelocityUpdateMode)1;
			}
			originalPosition = base.transform.position;
			currentPosition = originalPosition;
			targetPosition = originalPosition;
			if (randomStartPoint)
			{
				startPoint = Random.Range(0, relativePoints.Length);
			}
			if (startPoint != 0)
			{
				currentPoint = startPoint;
				base.transform.position = base.transform.position + relativePoints[currentPoint];
			}
		}
	}

	private void FixedUpdate()
	{
		if (!moving)
		{
			return;
		}
		float num = speed;
		float num2 = (highPrecisionEase ? 0.01f : 0.1f);
		if (easeOut && Vector3.Distance(base.transform.position, targetPosition) < speed / 2f)
		{
			num *= (Vector3.Distance(base.transform.position, targetPosition) / (speed / 2f) + num2) * easeSpeedMultiplier;
		}
		if (!quickStartCurrentMove && (!ignoreEaseOnStart || !onFirstMove) && easeIn && Vector3.Distance(base.transform.position, currentPosition) < speed / 2f)
		{
			num *= (Vector3.Distance(base.transform.position, currentPosition) / (speed / 2f) + num2) * easeSpeedMultiplier;
		}
		if ((bool)(Object)(object)aud && changePitchPerSpeed)
		{
			aud.SetPitch(intendedPitch * (num / speed));
		}
		Vector3 position = Vector3.MoveTowards(base.transform.position, targetPosition, Time.deltaTime * num);
		if (useRigidbody && (bool)rb)
		{
			rb.MovePosition(position);
		}
		else
		{
			base.transform.position = position;
		}
		if (Vector3.Distance(base.transform.position, targetPosition) < (highPrecisionEase ? 0.001f : 0.1f))
		{
			base.transform.position = targetPosition;
			moving = false;
			onFirstMove = false;
			quickStartCurrentMove = false;
			if (onReachPoint.Length > currentPoint)
			{
				onReachPoint[currentPoint].Invoke();
			}
			if (moveDelay > 0f)
			{
				waiting = true;
			}
			Invoke("NextPoint", moveDelay);
			if ((bool)(Object)(object)aud && (bool)(Object)(object)stopSound)
			{
				aud.clip = stopSound;
				aud.loop = false;
				intendedPitch = origPitch + Random.Range(origPitch * -0.1f, origPitch * 0.1f);
				if (!changePitchPerSpeed)
				{
					aud.SetPitch(intendedPitch);
				}
				if ((Object)(object)aud.clip != null)
				{
					aud.Play(tracked: true);
				}
			}
			else if ((bool)(Object)(object)aud && (bool)(Object)(object)moveSound)
			{
				aud.Stop();
			}
		}
		else if ((bool)(Object)(object)aud && !aud.isPlaying && (bool)(Object)(object)moveSound)
		{
			aud.clip = moveSound;
			aud.loop = true;
			intendedPitch = origPitch + Random.Range(origPitch * -0.1f, origPitch * 0.1f);
			if (!changePitchPerSpeed)
			{
				aud.SetPitch(intendedPitch);
			}
			if ((Object)(object)aud.clip != null)
			{
				aud.Play(tracked: true);
			}
		}
	}

	private void NextPoint()
	{
		int num = 1;
		if (!forward)
		{
			num = -1;
		}
		if ((forward && currentPoint < relativePoints.Length - 1) || (!forward && currentPoint > 0))
		{
			currentPoint += num;
		}
		else
		{
			if (teleportBackToStart)
			{
				TeleportToPoint(0);
				return;
			}
			if (stopAtEnd)
			{
				return;
			}
			if (!reverseAtEnd)
			{
				currentPoint = 0;
			}
			else if (forward)
			{
				forward = false;
			}
			else
			{
				forward = true;
			}
		}
		quickStartCurrentMove = false;
		currentPosition = targetPosition;
		MoveToCurrentPoint();
	}

	private void MoveToCurrentPoint()
	{
		targetPosition = originalPosition + relativePoints[currentPoint];
		waiting = false;
		moving = true;
		if ((bool)(Object)(object)aud && (bool)(Object)(object)moveSound && base.enabled)
		{
			aud.clip = moveSound;
			aud.loop = true;
			intendedPitch = origPitch + Random.Range(origPitch * -0.1f, origPitch * 0.1f);
			if (!changePitchPerSpeed)
			{
				aud.SetPitch(intendedPitch);
			}
			if ((Object)(object)aud.clip != null)
			{
				aud.Play(tracked: true);
			}
		}
	}

	public void TeleportToPoint(int newPoint)
	{
		if (!infoSet)
		{
			SetInfo();
		}
		Invoke("ReEnableDoppler", 0.1f);
		if ((bool)(Object)(object)aud)
		{
			aud.dopplerLevel = 0f;
		}
		base.transform.position = originalPosition + relativePoints[newPoint];
		currentPoint = newPoint - 1;
		quickStartCurrentMove = false;
		NextPoint();
	}

	public void StartMoving()
	{
		if (!infoSet)
		{
			SetInfo();
		}
		if (!moving)
		{
			NextPoint();
		}
	}

	public void StopMoving()
	{
		moving = false;
	}

	private void ReEnableDoppler()
	{
		if ((bool)(Object)(object)aud)
		{
			aud.dopplerLevel = origDoppler;
		}
	}

	public void SkipWait()
	{
		if (waiting)
		{
			CancelInvoke("NextPoint");
			NextPoint();
		}
	}

	public void MoveFastTo(int newPoint)
	{
		if (!(base.transform.position == originalPosition + relativePoints[newPoint]))
		{
			SkipWait();
			currentPoint = newPoint;
			quickStartCurrentMove = true;
			MoveToCurrentPoint();
		}
	}
}
