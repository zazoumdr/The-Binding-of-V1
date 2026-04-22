using UnityEngine;

public class MoveTowards : MonoBehaviour
{
	public Vector3 targetPosition;

	public float speed;

	public float easeAtEnd;

	public bool useRigidBody;

	private Rigidbody rb;

	public bool pitchAudioWithSpeed;

	private AudioSource aud;

	private float originalPitch;

	public UltrakillEvent onReachTarget;

	private void Start()
	{
		if (useRigidBody)
		{
			UseRigidbody(use: true);
		}
	}

	private void FixedUpdate()
	{
		if (base.transform.position != targetPosition)
		{
			float num = speed;
			if (easeAtEnd != 0f)
			{
				num = Mathf.Min(num, Vector3.Distance(base.transform.position, targetPosition) * 2f / easeAtEnd);
			}
			Vector3 vector = Vector3.MoveTowards(base.transform.position, targetPosition, num * Time.fixedDeltaTime);
			if (Vector3.Distance(vector, targetPosition) < 0.1f)
			{
				if (useRigidBody)
				{
					rb.MovePosition(targetPosition);
				}
				else
				{
					base.transform.position = targetPosition;
				}
				onReachTarget?.Invoke();
			}
			else if (useRigidBody)
			{
				rb.MovePosition(vector);
			}
			else
			{
				base.transform.position = vector;
			}
			if (pitchAudioWithSpeed && (bool)(Object)(object)aud)
			{
				aud.SetPitch(num / speed * originalPitch);
			}
		}
		else if (pitchAudioWithSpeed && (bool)(Object)(object)aud)
		{
			aud.SetPitch(0f);
		}
	}

	public void SnapToTarget()
	{
		base.transform.position = targetPosition;
	}

	public void ChangeTarget(Vector3 target)
	{
		targetPosition = target;
	}

	public void ChangeX(float number)
	{
		ChangeTarget(new Vector3(number, targetPosition.y, targetPosition.z));
	}

	public void ChangeY(float number)
	{
		ChangeTarget(new Vector3(targetPosition.x, number, targetPosition.z));
	}

	public void ChangeZ(float number)
	{
		ChangeTarget(new Vector3(targetPosition.x, targetPosition.y, number));
	}

	public void UseRigidbody(bool use)
	{
		if (!rb && use)
		{
			rb = GetComponent<Rigidbody>();
		}
		useRigidBody = use;
	}

	public void PitchAudioWithSpeed(bool use)
	{
		if (!(Object)(object)aud && use)
		{
			aud = GetComponent<AudioSource>();
			originalPitch = aud.GetPitch();
		}
		pitchAudioWithSpeed = use;
	}

	public void UpdateAudioOriginalPitch()
	{
		if ((bool)(Object)(object)aud)
		{
			originalPitch = aud.GetPitch();
		}
	}

	public void EaseAtEnd(float newEase)
	{
		easeAtEnd = newEase;
	}
}
