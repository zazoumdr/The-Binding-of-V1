using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
	[SerializeField]
	private float speed = 180f;

	private void LateUpdate()
	{
		base.transform.Rotate(Vector3.up, Time.deltaTime * speed);
	}
}
