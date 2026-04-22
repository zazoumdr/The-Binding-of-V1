using System.Runtime.InteropServices;
using UnityEngine;

namespace ULTRAKILL.Portal.Geometry;

[StructLayout(LayoutKind.Sequential)]
public class PortalTransform
{
	public Matrix4x4 toLocal;

	public Matrix4x4 toWorld;

	public Vector3 center;

	public Vector3 forward;

	public Vector3 up;

	public Vector3 right;

	public Vector3 left;

	public Vector3 down;

	public Vector3 back;

	public ref readonly Matrix4x4 GetMatrix(bool toWorld)
	{
		if (!toWorld)
		{
			return ref toLocal;
		}
		return ref this.toWorld;
	}

	public void UpdateTransform(ref Matrix4x4 entryToLocal, ref Matrix4x4 entryToWorld)
	{
		toLocal = entryToLocal;
		toWorld = entryToWorld;
		center = entryToWorld.GetColumn(3);
		forward = entryToWorld.GetColumn(2);
		up = toWorld.GetColumn(1);
		right = toWorld.GetColumn(0);
		left = -right;
		down = -up;
		back = -forward;
	}

	public Vector3 WorldToLocal(Vector3 world)
	{
		return toLocal.MultiplyPoint3x4(world);
	}

	public Vector3 LocalToWorld(Vector3 local)
	{
		return toWorld.MultiplyPoint3x4(local);
	}

	public bool HasMovedForward(PortalTransform other)
	{
		if (other == null)
		{
			return false;
		}
		if (center == other.center)
		{
			return false;
		}
		Vector3 rhs = other.center - center;
		if (Vector3.Dot(back, rhs) < 0f)
		{
			return false;
		}
		return true;
	}

	internal void CopyFrom(PortalTransform copyTransform)
	{
		UpdateTransform(ref copyTransform.toLocal, ref copyTransform.toWorld);
	}
}
