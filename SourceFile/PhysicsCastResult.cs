using System;
using UnityEngine;

public struct PhysicsCastResult : IComparable<PhysicsCastResult>, IEquatable<PhysicsCastResult>
{
	public float distance;

	public Vector3 point;

	public Vector3 direction;

	public Vector3 normal;

	public Transform transform;

	public Collider collider;

	public Rigidbody rigidbody;

	public bool IsValid => transform != null;

	public PhysicsCastResult(RaycastHit hit)
	{
		distance = hit.distance;
		point = hit.point;
		direction = default(Vector3);
		normal = hit.normal;
		transform = hit.transform;
		collider = hit.collider;
		rigidbody = hit.rigidbody;
	}

	public static PhysicsCastResult Empty()
	{
		return new PhysicsCastResult
		{
			distance = float.MaxValue,
			point = Vector3.zero,
			direction = default(Vector3),
			normal = Vector3.zero,
			transform = null,
			collider = null,
			rigidbody = null
		};
	}

	public static PhysicsCastResult FromRaycastHit(RaycastHit hit)
	{
		return new PhysicsCastResult
		{
			distance = hit.distance,
			point = hit.point,
			direction = default(Vector3),
			normal = hit.normal,
			transform = hit.transform,
			collider = hit.collider,
			rigidbody = hit.rigidbody
		};
	}

	public static PhysicsCastResult FromPoints(Vector3 start, Vector3 end)
	{
		return new PhysicsCastResult
		{
			distance = Vector3.Distance(start, end),
			point = end,
			direction = default(Vector3),
			normal = Vector3.zero,
			transform = null,
			collider = null,
			rigidbody = null
		};
	}

	public static PhysicsCastResult FromCollider(Collider col)
	{
		return new PhysicsCastResult
		{
			distance = 0f,
			point = col.transform.position,
			direction = default(Vector3),
			normal = Vector3.zero,
			transform = col.transform,
			collider = col,
			rigidbody = null
		};
	}

	public static PhysicsCastResult FromDirectionDistance(Vector3 origin, Vector3 direction, float distance)
	{
		return new PhysicsCastResult
		{
			distance = distance,
			point = origin + direction * distance,
			direction = direction,
			normal = Vector3.zero,
			transform = null,
			collider = null,
			rigidbody = null
		};
	}

	public int CompareTo(PhysicsCastResult other)
	{
		return distance.CompareTo(other.distance);
	}

	public bool Equals(PhysicsCastResult other)
	{
		if (!IsValid && !other.IsValid)
		{
			return true;
		}
		if (IsValid != other.IsValid)
		{
			return false;
		}
		if (transform == other.transform && Mathf.Approximately(distance, other.distance) && point == other.point && normal == other.normal && collider == other.collider)
		{
			return rigidbody == other.rigidbody;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PhysicsCastResult other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!IsValid)
		{
			return 0;
		}
		return (((((17 * 23 + ((transform != null) ? transform.GetHashCode() : 0)) * 23 + distance.GetHashCode()) * 23 + point.GetHashCode()) * 23 + normal.GetHashCode()) * 23 + ((collider != null) ? collider.GetHashCode() : 0)) * 23 + ((rigidbody != null) ? rigidbody.GetHashCode() : 0);
	}

	public static bool operator ==(PhysicsCastResult left, PhysicsCastResult right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PhysicsCastResult left, PhysicsCastResult right)
	{
		return !left.Equals(right);
	}

	public static implicit operator bool(PhysicsCastResult result)
	{
		return result.IsValid;
	}
}
