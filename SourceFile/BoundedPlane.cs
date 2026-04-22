using UnityEngine;

public struct BoundedPlane
{
	public Vector3 center;

	public Vector3 normal;

	public Vector3 up;

	public Vector3 right;

	public float width;

	public float height;

	public Vector4 AsNormalDistance => new Vector4(normal.x, normal.y, normal.z, Vector3.Dot(normal, center));

	public Vector4 AsFlippedNormalDistance => new Vector4(0f - normal.x, 0f - normal.y, 0f - normal.z, Vector3.Dot(normal, center));

	public Vector3 min => center - width * right - height * up;

	public Vector3 max => center + width * right + height * up;

	public BoundedPlane(Vector3 center, Vector3 normal, Vector3 up, Vector3 right, float width, float height)
	{
		this.center = center;
		this.normal = normal;
		this.up = up;
		this.right = right;
		this.width = width;
		this.height = height;
	}

	public BoundedPlane(Transform transform, float width, float height)
	{
		center = transform.position;
		normal = transform.forward;
		up = transform.up;
		right = transform.right;
		this.width = width;
		this.height = height;
	}

	public Vector3 ToBasis(Vector3 point, bool position = true)
	{
		Vector3 lhs = (position ? (point - center) : point);
		return new Vector3(Vector3.Dot(lhs, right), Vector3.Dot(lhs, up), Vector3.Dot(lhs, normal));
	}

	public Vector3 FromBasis(Vector3 basis, bool position = true)
	{
		return (position ? center : Vector3.zero) + basis.x * right + basis.y * up + basis.z * normal;
	}

	public float GetDistance(Vector3 point)
	{
		return Mathf.Abs(Vector3.Dot(point - center, normal));
	}

	public bool GetSide(Vector3 point)
	{
		return Vector3.Dot(point - center, normal) >= 0f;
	}

	public Vector2 ProjectTo2D(Vector3 point)
	{
		Vector3 lhs = point - center;
		float num = Vector3.Dot(lhs, right);
		float num2 = Vector3.Dot(lhs, up);
		return new Vector2(num / width + 0.5f, num2 / height + 0.5f);
	}

	public bool Contains(Vector3 point)
	{
		Vector2 vector = ProjectTo2D(point);
		if (vector.x >= 0f && vector.x <= 1f && vector.y >= 0f)
		{
			return vector.y <= 1f;
		}
		return false;
	}
}
