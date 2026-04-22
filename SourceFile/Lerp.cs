using UnityEngine;

public class Lerp : MonoBehaviour
{
	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Vector3 rotation;

	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float rotateSpeed;

	[HideInInspector]
	public Quaternion qRot;

	[SerializeField]
	private bool ease;

	[SerializeField]
	private float easeSpeed = 1f;

	[HideInInspector]
	public float currentEaseMultiplier;

	[SerializeField]
	private bool onEnable = true;

	[SerializeField]
	private bool inFixedUpdate;

	[SerializeField]
	private bool inLocalSpace;

	[HideInInspector]
	public bool moving;

	[SerializeField]
	private UltrakillEvent onComplete;

	private void Start()
	{
		if (onEnable)
		{
			Activate();
		}
	}

	private void OnEnable()
	{
		if (onEnable)
		{
			Activate();
		}
	}

	private void Update()
	{
		if (moving && !inFixedUpdate)
		{
			Move(Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (moving && inFixedUpdate)
		{
			Move(Time.fixedDeltaTime);
		}
	}

	private void Move(float amount)
	{
		if (ease)
		{
			currentEaseMultiplier = Mathf.MoveTowards(currentEaseMultiplier, 1f, amount * easeSpeed * Mathf.Max(currentEaseMultiplier, 0.01f));
			amount *= currentEaseMultiplier;
		}
		if (!inLocalSpace)
		{
			Vector3 vector = Vector3.MoveTowards(base.transform.position, position, moveSpeed * amount);
			Quaternion quaternion = Quaternion.RotateTowards(base.transform.rotation, qRot, rotateSpeed * amount);
			base.transform.SetPositionAndRotation(vector, quaternion);
			if ((base.transform.position == position || moveSpeed == 0f) && (base.transform.rotation == qRot || rotateSpeed == 0f))
			{
				moving = false;
				onComplete?.Invoke();
			}
			return;
		}
		if (moveSpeed != 0f)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, position, moveSpeed * amount);
		}
		if (rotateSpeed != 0f)
		{
			base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, qRot, rotateSpeed * amount);
		}
		if ((base.transform.localPosition == position || moveSpeed == 0f) && (base.transform.localRotation == qRot || rotateSpeed == 0f))
		{
			moving = false;
			onComplete?.Invoke();
		}
	}

	public void Activate()
	{
		if (!moving)
		{
			qRot = Quaternion.Euler(rotation);
			moving = true;
		}
	}

	public void Skip()
	{
		Activate();
		Move(99999f);
	}
}
