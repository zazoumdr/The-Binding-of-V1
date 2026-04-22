using System.Collections.Generic;
using UnityEngine;

namespace ULTRAKILL.Portal.Geometry;

public struct BoundedPlane
{
	public Vector3 center;

	public Vector3 normal;

	public Vector3 up;

	public Vector3 right;

	public float width;

	public float height;

	public Vector4 AsNormalDistance => new Vector4(0f - normal.x, 0f - normal.y, 0f - normal.z, Vector3.Dot(center, normal));

	public Vector3[] Vertices => new Vector3[4]
	{
		center + up * height - right * width,
		center + up * height + right * width,
		center - up * height + right * width,
		center - up * height - right * width
	};

	public BoundedPlane(Transform trans, float width, float height)
	{
		center = trans.position;
		normal = trans.forward;
		up = trans.up;
		right = trans.right;
		this.width = width;
		this.height = height;
	}

	public BoundedPlane(Vector3 center, Vector3 normal, Vector3 up, Vector3 right, float width, float height)
	{
		this.center = center;
		this.normal = normal;
		this.up = up;
		this.right = right;
		this.width = width;
		this.height = height;
	}

	public bool GetSide(params Vector3[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (!GetSide(points[i]))
			{
				return false;
			}
		}
		return true;
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

	public void ClipNormalizedPolygon(Vector2[] points, ref List<Vector2> clippedVerts)
	{
	}

	public Vector3 UnProjectFrom2D(Vector2 point)
	{
		float num = point.x * width;
		float num2 = point.y * height;
		return center + up * num2 + right * num;
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
