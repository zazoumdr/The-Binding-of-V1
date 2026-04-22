using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using UnityEngine;

namespace ULTRAKILL.Portal;

public static class PortalExtensions
{
	public static bool IsFacingDown(this Portal portal, PortalSide side)
	{
		return portal.GetTransform(side).IsFacingDown();
	}

	public static bool IsAnyTransformTilted(this Portal portal)
	{
		if (!portal.entryTransform.IsTilted())
		{
			return portal.exitTransform.IsTilted();
		}
		return true;
	}

	public static bool IsAnyTransformRotated(this Portal portal)
	{
		if (!portal.entryTransform.IsRotated())
		{
			return portal.exitTransform.IsRotated();
		}
		return true;
	}

	public static Vector3 AdjustIntercept(this Portal portal, PortalSide side, Vector3 worldIntersect, float padding)
	{
		NativePortalTransform transform = portal.GetTransform(side);
		Vector3 point = transform.toLocalManaged.MultiplyPoint3x4(worldIntersect);
		float x = point.x;
		float y = point.y;
		PlaneShape shape = portal.GetShape();
		float num = shape.width / 2f - padding;
		if (num < 0f)
		{
			num = 0f;
		}
		float num2 = shape.height / 2f - padding;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if ((double)x > 0.0 && x > num)
		{
			point.x = num;
		}
		if ((double)x < 0.0 && x < 0f - num)
		{
			point.x = 0f - num;
		}
		if ((double)y > 0.0 && y > num2)
		{
			point.y = num2;
		}
		if ((double)y < 0.0 && y < 0f - num2)
		{
			point.y = 0f - num2;
		}
		return transform.toWorldManaged.MultiplyPoint3x4(point);
	}
}
