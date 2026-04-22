using UnityEngine;

namespace BeamHitInterpolation;

public record InterpolatedHit
{
	public Vector3 point;

	public Vector3 normal;

	public float distance;

	public Transform transform;

	public Rigidbody rigidbody;

	public Collider collider;

	public static InterpolatedHit FromRaycastHit(RaycastHit hit)
	{
		return new InterpolatedHit
		{
			point = hit.point,
			normal = hit.normal,
			distance = hit.distance,
			transform = hit.transform,
			rigidbody = hit.rigidbody,
			collider = hit.collider
		};
	}
}
