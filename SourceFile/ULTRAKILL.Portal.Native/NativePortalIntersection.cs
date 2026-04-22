using System;
using Unity.Mathematics;

namespace ULTRAKILL.Portal.Native;

public struct NativePortalIntersection : IComparable<NativePortalIntersection>
{
	public PortalHandle handle;

	public float3 point;

	public float distance;

	public int CompareTo(NativePortalIntersection other)
	{
		return distance.CompareTo(other.distance);
	}
}
