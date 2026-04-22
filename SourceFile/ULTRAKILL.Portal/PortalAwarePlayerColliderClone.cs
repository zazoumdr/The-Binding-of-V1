using UnityEngine;

namespace ULTRAKILL.Portal;

[DefaultExecutionOrder(1001)]
public class PortalAwarePlayerColliderClone : MonoBehaviour
{
	public Rigidbody TargetRigidbody;

	public Matrix4x4 TravelMatrix;

	public Plane PortalPlane;

	private Vector3 accumulatedVelocityChange;

	private int lastFixedFrame;

	private const float MaxVelocityChangePerFrame = 30f;

	private void OnCollisionStay(Collision collision)
	{
		if (MonoSingleton<NewMovement>.Instance.rb.isKinematic)
		{
			return;
		}
		int num = Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
		if (num != lastFixedFrame)
		{
			accumulatedVelocityChange = Vector3.zero;
			lastFixedFrame = num;
		}
		int contactCount = collision.contactCount;
		for (int i = 0; i < contactCount; i++)
		{
			ContactPoint contact = collision.GetContact(i);
			Vector3 impulse = contact.impulse;
			Collider thisCollider = contact.thisCollider;
			Collider otherCollider = contact.otherCollider;
			thisCollider.transform.GetPositionAndRotation(out var position, out var rotation);
			otherCollider.transform.GetPositionAndRotation(out var position2, out var rotation2);
			Vector3 vector;
			if (impulse.sqrMagnitude > 0.001f)
			{
				vector = TravelMatrix.MultiplyVector(impulse).normalized * impulse.magnitude * (1f / Time.fixedDeltaTime);
			}
			else
			{
				if ((bool)collision.rigidbody || !Physics.ComputePenetration(contact.thisCollider, position, rotation, contact.otherCollider, position2, rotation2, out var direction, out var distance))
				{
					continue;
				}
				vector = TravelMatrix.MultiplyVector(direction).normalized * distance;
			}
			_ = accumulatedVelocityChange + vector;
		}
	}
}
