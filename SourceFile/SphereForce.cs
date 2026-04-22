using UnityEngine;

public class SphereForce : MonoBehaviour
{
	public float strength = 10f;

	private void OnTriggerStay(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (!(attachedRigidbody == null))
		{
			Vector3 vector = base.transform.position - attachedRigidbody.position;
			float magnitude = vector.magnitude;
			vector = vector.normalized / magnitude;
			other.attachedRigidbody.AddForce(vector * strength, ForceMode.Force);
		}
	}
}
