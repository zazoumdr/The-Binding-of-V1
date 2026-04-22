using UnityEngine;

public class RotateByPosition : MonoBehaviour
{
	public float rotateAmount = 360f;

	public Transform start;

	public Transform end;

	public Transform objectToRotate;

	public Vector3 rotationAxis;

	private Transform player;

	private void Start()
	{
		player = MonoSingleton<NewMovement>.Instance.Transform;
	}

	private void Update()
	{
		Vector3 rhs = end.position - start.position;
		Quaternion rotation = Quaternion.AngleAxis(Mathf.Clamp01(Vector3.Dot(player.position - start.position, rhs) / rhs.sqrMagnitude) * rotateAmount, rotationAxis);
		objectToRotate.rotation = rotation;
	}
}
