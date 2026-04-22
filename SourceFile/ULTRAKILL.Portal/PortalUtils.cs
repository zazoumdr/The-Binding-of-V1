using System;
using System.Linq;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace ULTRAKILL.Portal;

public static class PortalUtils
{
	public static void AddForceAtPositionPortalAware(PhysicsCastResult hit, Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
		if (hit.rigidbody != null)
		{
			hit.rigidbody.AddForceAtPosition(force, position, mode);
		}
		else
		{
			AddForceAtPositionPortalAware(hit.transform.gameObject, force, position, mode);
		}
	}

	public static void AddForceAtPositionPortalAware(RaycastHit hit, Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
		if (hit.rigidbody != null)
		{
			hit.rigidbody.AddForceAtPosition(force, position, mode);
		}
		else
		{
			AddForceAtPositionPortalAware(hit.transform.gameObject, force, position, mode);
		}
	}

	public static void AddForceAtPositionPortalAware(GameObject obj, Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
		if (obj == null)
		{
			return;
		}
		if (obj.TryGetComponent<PortalAwareRendererClone>(out var component))
		{
			if (component.TargetTransform == null || !component.Owner.TryGetPortalHandle(out var result))
			{
				return;
			}
			obj = component.TargetTransform.gameObject;
			Matrix4x4 travelMatrix = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(result).GetTravelMatrix((result.side != PortalSide.Enter) ? PortalSide.Enter : PortalSide.Exit);
			force = travelMatrix.MultiplyVector(force).normalized * force.magnitude;
			position = travelMatrix.MultiplyPoint3x4(position);
		}
		if (obj.TryGetComponent<Rigidbody>(out var component2))
		{
			component2.AddForceAtPosition(force, position, mode);
		}
	}

	public static void AddForcePortalAware(PhysicsCastResult hit, Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (hit.rigidbody != null)
		{
			hit.rigidbody.AddForce(force, mode);
		}
		else
		{
			AddForcePortalAware(hit.transform.gameObject, force, mode);
		}
	}

	public static void AddForcePortalAware(RaycastHit hit, Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (hit.rigidbody != null)
		{
			hit.rigidbody.AddForce(force, mode);
		}
		else
		{
			AddForcePortalAware(hit.transform.gameObject, force, mode);
		}
	}

	public static void AddForcePortalAware(GameObject obj, Vector3 force, ForceMode mode = ForceMode.Force, bool searchParent = false)
	{
		if (obj == null)
		{
			return;
		}
		PortalAwareRendererClone component;
		if (searchParent)
		{
			PortalAwareRendererClone componentInParent = obj.GetComponentInParent<PortalAwareRendererClone>();
			if (componentInParent != null)
			{
				if (componentInParent.TargetTransform == null || !componentInParent.Owner.TryGetPortalHandle(out var result))
				{
					return;
				}
				obj = componentInParent.TargetTransform.gameObject;
				force = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(result).GetTravelMatrix((result.side != PortalSide.Enter) ? PortalSide.Enter : PortalSide.Exit).MultiplyVector(force)
					.normalized * force.magnitude;
			}
		}
		else if (obj.TryGetComponent<PortalAwareRendererClone>(out component))
		{
			if (component.TargetTransform == null)
			{
				return;
			}
			obj = component.TargetTransform.gameObject;
		}
		Rigidbody component2;
		if (searchParent)
		{
			Rigidbody componentInParent2 = obj.GetComponentInParent<Rigidbody>();
			if (componentInParent2 != null)
			{
				componentInParent2.AddForce(force, mode);
			}
		}
		else if (obj.TryGetComponent<Rigidbody>(out component2))
		{
			component2.AddForce(force, mode);
		}
	}

	public static Portal GetPortalObject(PortalHandle handle)
	{
		return MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(handle);
	}

	public static NativePortalTransform GetTransform(PortalHandle handle, bool reverseSide)
	{
		PortalSide side = (reverseSide ? handle.side.Reverse() : handle.side);
		return GetPortalObject(handle).GetTransform(side);
	}

	public static Matrix4x4 GetTravelMatrix(PortalHandle handle)
	{
		return MonoSingleton<PortalManagerV2>.Instance.Scene.GetTravelMatrix(handle);
	}

	public static Matrix4x4 GetTravelMatrix(PortalHandleSequence handles)
	{
		return MonoSingleton<PortalManagerV2>.Instance.Scene.GetTravelMatrix(in handles);
	}

	public static Matrix4x4 GetTravelMatrix(PortalTraversalV2[] traversals)
	{
		return MonoSingleton<PortalManagerV2>.Instance.Scene.GetTravelMatrix(traversals);
	}

	public static PortalIdentifier GetPortalIdentifier(this PortalHandle handle)
	{
		if (MonoSingleton<PortalManagerV2>.Instance.Scene.portalIdentifiersLookup.TryGetValue(handle, out var value))
		{
			return value;
		}
		return null;
	}

	public static PortalIdentifier GetPortalIdentifier(this PortalScene scene, PortalHandle handle)
	{
		if (scene.portalIdentifiersLookup.TryGetValue(handle, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool IsPointInFront(this PortalTransform transform, Vector3 point)
	{
		return Vector3.Dot(transform.back, point - transform.center) > 0f;
	}

	public static bool IsPointInFront(this NativePortalTransform transform, Vector3 point)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(float3.op_Implicit(transform.back), point - transform.centerManaged) > 0f;
	}

	public static bool IsInFrontOfPortal(this Vector3 point, PortalTransform transform)
	{
		return transform.IsPointInFront(point);
	}

	public static bool IsInFrontOfPortal(this Vector3 point, NativePortalTransform transform)
	{
		return transform.IsPointInFront(point);
	}

	public static bool IsCompletelyInFrontOf(this PortalTransform portal, Collider col)
	{
		if (col is SphereCollider col2)
		{
			return portal.IsCompletelyInFrontOfSphere(col2);
		}
		if (col is MeshCollider meshCollider)
		{
			Vector3[] vertices = meshCollider.sharedMesh.vertices;
			return !portal.IsAnyPointInFront(vertices);
		}
		Vector3[] points = BoundsToWorldPoints(col.bounds);
		return !portal.IsAnyPointInFront(points);
	}

	public static bool IsCompletelyInFrontOf(this NativePortalTransform portal, Collider col)
	{
		if (col is SphereCollider col2)
		{
			return portal.IsCompletelyInFrontOfSphere(col2);
		}
		if (col is MeshCollider meshCollider)
		{
			Vector3[] vertices = meshCollider.sharedMesh.vertices;
			return !portal.IsAnyPointInFront(vertices);
		}
		Vector3[] points = BoundsToWorldPoints(col.bounds);
		return !portal.IsAnyPointInFront(points);
	}

	public static bool IsPartiallyInFrontOf(this Collider col, PortalTransform portal)
	{
		return !portal.IsCompletelyInFrontOf(col);
	}

	public static bool IsPartiallyInFrontOf(this Collider col, NativePortalTransform portal)
	{
		return !portal.IsCompletelyInFrontOf(col);
	}

	public static bool IsCompletelyInFrontOfSphere(this PortalTransform portal, SphereCollider col)
	{
		Vector3 position = col.transform.position;
		if (portal.IsPointInFront(position))
		{
			return false;
		}
		if (Mathf.Abs(portal.toLocal.MultiplyPoint3x4(position).z) < col.radius)
		{
			return false;
		}
		return true;
	}

	public static bool IsCompletelyInFrontOfSphere(this NativePortalTransform portal, SphereCollider col)
	{
		Vector3 position = col.transform.position;
		if (portal.IsPointInFront(position))
		{
			return false;
		}
		if (Mathf.Abs(portal.toLocalManaged.MultiplyPoint3x4(position).z) < col.radius)
		{
			return false;
		}
		return true;
	}

	public static bool IsPartiallyInFrontOfSphere(this SphereCollider col, PortalTransform portal)
	{
		return !portal.IsCompletelyInFrontOfSphere(col);
	}

	private static bool IsAnyPointInFront(this PortalTransform portal, Vector3[] points)
	{
		return points.Any((Vector3 vert) => vert.IsInFrontOfPortal(portal));
	}

	private static bool IsAnyPointInFront(this NativePortalTransform portal, Vector3[] points)
	{
		return points.Any((Vector3 vert) => vert.IsInFrontOfPortal(portal));
	}

	private static Vector3[] BoundsToWorldPoints(Bounds bounds)
	{
		return new Vector3[8]
		{
			bounds.min,
			new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
			bounds.max
		};
	}

	public static bool FindClosestLinkPositions(this PortalIdentifier portalId, Vector3 point, out Vector3 entryPos, out Vector3 exitPos)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		portalId.GetValidLinks();
		if (portalId.IsValidLinkExist)
		{
			float min = portalId.GetValidLinks().Min((Func<PortalNavMeshLink, float>)distance);
			PortalNavMeshLink portalNavMeshLink = portalId.GetValidLinks().First((PortalNavMeshLink link) => distance(link) == min);
			NavMeshLinkData data = portalNavMeshLink.data;
			entryPos = ((NavMeshLinkData)(ref data)).startPosition;
			data = portalNavMeshLink.data;
			exitPos = ((NavMeshLinkData)(ref data)).endPosition;
			return true;
		}
		entryPos = default(Vector3);
		exitPos = default(Vector3);
		return false;
		float distance(PortalNavMeshLink link)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			NavMeshLinkData data2 = link.data;
			return Vector3.Distance(((NavMeshLinkData)(ref data2)).startPosition, point);
		}
	}

	public static void GenerateLineRendererSegments(this MonoBehaviour caller, LineRenderer lr, PortalTraversalV2[] portalTraversals)
	{
		LineRendererPortalHelper.GetOrCreateHelper(lr).UpdateTraversals(portalTraversals);
		CopyLineRenderer[] componentsInChildren = caller.GetComponentsInChildren<CopyLineRenderer>();
		foreach (CopyLineRenderer copyLineRenderer in componentsInChildren)
		{
			if (!(copyLineRenderer.toCopy != lr))
			{
				LineRendererPortalHelper.GetOrCreateHelper(copyLineRenderer.lr).UpdateTraversals(portalTraversals);
			}
		}
	}
}
