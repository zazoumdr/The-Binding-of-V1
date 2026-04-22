using UnityEngine;

public class TeleportItem : MonoBehaviour
{
	public Vector3 position;

	public bool resetVelocity;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 22)
		{
			other.transform.position = position;
			if (resetVelocity && (bool)other.attachedRigidbody)
			{
				other.attachedRigidbody.velocity = Vector3.zero;
			}
		}
	}
}
