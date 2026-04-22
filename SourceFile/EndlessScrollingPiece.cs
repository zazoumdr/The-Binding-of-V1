using UnityEngine;

public class EndlessScrollingPiece : MonoBehaviour
{
	private Rigidbody rb;

	public Vector3 velocity;

	public float maxDistance;

	public bool moveContinuously = true;

	public bool inLocalSpace;

	[HideInInspector]
	public float moveAmountLeft;

	[HideInInspector]
	public Vector3 stoppingSpot;

	private Vector3 originalPosition;

	private void Awake()
	{
		originalPosition = (inLocalSpace ? base.transform.localPosition : base.transform.position);
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (moveContinuously)
		{
			Move();
		}
		else if (moveAmountLeft > 0f)
		{
			float num = Mathf.Min(moveAmountLeft, velocity.magnitude * Time.fixedDeltaTime);
			Move(velocity.normalized * num);
			moveAmountLeft -= num;
			if (moveAmountLeft <= 0f)
			{
				rb.MovePosition(stoppingSpot);
			}
		}
	}

	private void Move()
	{
		Move(velocity * Time.fixedDeltaTime);
	}

	private void Move(Vector3 amount)
	{
		if (inLocalSpace)
		{
			amount = base.transform.rotation * amount;
		}
		rb.MovePosition(base.transform.position + amount);
		if (Vector3.Distance(base.transform.position, base.transform.parent.position) > maxDistance)
		{
			Vector3 vector = velocity.normalized * -2f * maxDistance;
			if (inLocalSpace)
			{
				vector = base.transform.rotation * vector;
			}
			base.transform.position += vector;
		}
	}

	public void AddMovement(float units)
	{
		moveAmountLeft = units;
		stoppingSpot = base.transform.position + velocity.normalized * units;
		while (Vector3.Distance(stoppingSpot, base.transform.parent.position) > maxDistance)
		{
			stoppingSpot += velocity.normalized * -2f * maxDistance;
		}
	}

	public void ResetPosition()
	{
		if ((bool)rb && base.enabled)
		{
			rb.MovePosition(inLocalSpace ? base.transform.TransformPoint(originalPosition) : originalPosition);
		}
	}
}
