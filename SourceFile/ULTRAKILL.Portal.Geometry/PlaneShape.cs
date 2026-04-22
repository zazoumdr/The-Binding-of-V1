using UnityEngine;

namespace ULTRAKILL.Portal.Geometry;

public struct PlaneShape : IPortalShape
{
	public static readonly Plane LocalPlane = new Plane(Vector3.forward, 0f);

	public float width;

	public float height;

	public bool IsValid
	{
		get
		{
			if (width != 0f)
			{
				return height != 0f;
			}
			return false;
		}
	}

	public readonly float GetBoundingRadius()
	{
		return 1.4142135f * Mathf.Max(width, height);
	}

	public static bool DidCross(in PlaneShape shape, in Vector3 a, in Vector3 b, out Vector3 intersection)
	{
		Vector3 direction = b - a;
		intersection = a;
		if (LocalPlane.GetSide(a) == LocalPlane.GetSide(b))
		{
			return false;
		}
		if (Vector3.Dot(direction.normalized, Vector3.forward) < 0f)
		{
			intersection = a;
			return false;
		}
		if (!LocalPlane.Raycast(new Ray(a, direction), out var enter) || enter * enter > direction.sqrMagnitude)
		{
			intersection = Vector3.zero;
			return false;
		}
		intersection = a + direction.normalized * enter;
		if (Mathf.Abs(intersection.x) > shape.width / 2f || Mathf.Abs(intersection.y) > shape.height / 2f)
		{
			return false;
		}
		return true;
	}

	public bool DidCross(Vector3 a, Vector3 b, out Vector3 intersection)
	{
		return DidCross(this, in a, in b, out intersection);
	}

	public void DrawDebug(PortalTransform trans, float duration = 0f, Color color = default(Color))
	{
		trans.toWorld.MultiplyPoint3x4(Vector3.zero);
		float num = width / 2f;
		float num2 = height / 2f;
		Vector3 point = new Vector3(0f - num, num2, 0f);
		Vector3 point2 = new Vector3(num, num2, 0f);
		Vector3 point3 = new Vector3(0f - num, 0f - num2, 0f);
		Vector3 point4 = new Vector3(num, 0f - num2, 0f);
		point = trans.toWorld.MultiplyPoint3x4(point);
		point2 = trans.toWorld.MultiplyPoint3x4(point2);
		point3 = trans.toWorld.MultiplyPoint3x4(point3);
		point4 = trans.toWorld.MultiplyPoint3x4(point4);
		if (color == default(Color))
		{
			color = Color.white;
		}
	}

	public readonly PortalMeshData GenerateMesh(PortalTransform trans)
	{
		float num = width / 2f;
		float num2 = height / 2f;
		trans.toWorld.MultiplyPoint3x4(Vector3.zero);
		Vector3[] vertices = new Vector3[4]
		{
			trans.LocalToWorld(new Vector3(0f - num, num2, 0f)),
			trans.LocalToWorld(new Vector3(num, num2, 0f)),
			trans.LocalToWorld(new Vector3(num, 0f - num2, 0f)),
			trans.LocalToWorld(new Vector3(0f - num, 0f - num2, 0f))
		};
		int[] indices = new int[6] { 0, 1, 2, 0, 2, 3 };
		return new PortalMeshData
		{
			vertices = vertices,
			indices = indices
		};
	}

	public readonly Vector3[] GetVertices(PortalTransform trans)
	{
		float num = width / 2f;
		float num2 = height / 2f;
		return new Vector3[4]
		{
			trans.LocalToWorld(new Vector3(0f - num, num2, 0f)),
			trans.LocalToWorld(new Vector3(num, num2, 0f)),
			trans.LocalToWorld(new Vector3(num, 0f - num2, 0f)),
			trans.LocalToWorld(new Vector3(0f - num, 0f - num2, 0f))
		};
	}

	public readonly Vector3 GetClosestLocalPoint(Vector3 point)
	{
		return new Vector3(Mathf.Clamp(point.x, width * -0.5f, width * 0.5f), Mathf.Clamp(point.y, height * -0.5f, height * 0.5f));
	}
}
