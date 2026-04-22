using ULTRAKILL.Portal;
using UnityEngine;

public class PhysicalShockwaveCollisionProxy : MonoBehaviour
{
	public PhysicalShockwave receiver;

	public PortalHandle portalHandle;

	public Transform previousOrigin;

	public Collider collider;

	private void Awake()
	{
		collider = GetComponent<Collider>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		receiver.HandleReplicaCollision(collision.collider, portalHandle, previousOrigin.position, collision.GetContact(0).point);
	}

	private void OnTriggerEnter(Collider col)
	{
		if (collider == null)
		{
			Debug.LogWarning("Collider is null on PhysicalShockwaveCollisionProxy attached to " + base.gameObject.name, this);
			return;
		}
		Vector3 closestPoint = collider.ClosestPoint(col.transform.position);
		receiver.HandleReplicaCollision(col, portalHandle, previousOrigin.position, closestPoint);
	}
}
