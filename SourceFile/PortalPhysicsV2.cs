using System;
using System.Collections.Generic;
using System.Linq;
using plog;
using plog.Models;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

public static class PortalPhysicsV2
{
	private static readonly Logger Log = new Logger("PortalManagerV2");

	private static readonly PortalTraversalV2[] EmptyTraversals = Array.Empty<PortalTraversalV2>();

	private static readonly ICastable RaycastCastable = new Raycast();

	private static List<PortalTraversalV2> traversalsList = new List<PortalTraversalV2>();

	private static List<(PortalHandle, Vector3, float)> datas = new List<(PortalHandle, Vector3, float)>();

	public static bool ProjectThroughPortals(Vector3 startPos, Vector3 direction, int layerMask, out PhysicsCastResult hit, out Vector3 endPoint, out PortalTraversalV2[] traversals)
	{
		return Raycast(startPos, direction.normalized, direction.magnitude, layerMask, out hit, out traversals, out endPoint);
	}

	public static void ProjectThroughPortals(PortalScene scene, Vector3 startPos, Vector3 endPos, out Vector3 endPoint, out Quaternion endRotation, out PortalTraversalV2[] traversals)
	{
		Vector3 vector = endPos - startPos;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		Raycast(startPos, vector, magnitude, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out traversals, out endPoint);
		if (traversals != null && traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[^1];
			vector = hitInfo.point - portalTraversalV.exitPoint;
			vector.Normalize();
		}
		endRotation = Quaternion.LookRotation(vector, Vector3.up);
	}

	public static void ProjectThroughPortals(Vector3 startPos, Vector3 endPos, out Vector3 endPoint, out Quaternion endRotation, out PortalTraversalV2[] traversals)
	{
		if (!MonoSingleton<PortalManagerV2>.Instance)
		{
			endPoint = endPos;
			endRotation = Quaternion.LookRotation(endPos - startPos, Vector3.up);
			traversals = EmptyTraversals;
			return;
		}
		Vector3 vector = endPos - startPos;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		Raycast(startPos, vector, magnitude, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out traversals, out endPoint);
		if (traversals != null && traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[^1];
			vector = hitInfo.point - portalTraversalV.exitPoint;
			vector.Normalize();
		}
		endRotation = Quaternion.LookRotation(vector, Vector3.up);
	}

	public static void ProjectThroughPortals(Vector3 startPos, Vector3 endPos, out Vector3 endPoint, out Quaternion endRotation)
	{
		Vector3 vector = endPos - startPos;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		Raycast(startPos, vector, magnitude, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out var portalTraversals, out endPoint);
		if (portalTraversals != null && portalTraversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = portalTraversals[^1];
			vector = hitInfo.point - portalTraversalV.exitPoint;
			vector.Normalize();
		}
		endRotation = Quaternion.LookRotation(vector, Vector3.up);
	}

	public static bool SphereCast(Vector3 origin, Vector3 direction, float maxDistance, float radius, int layerMask, out PhysicsCastResult hitInfo, out PortalTraversalV2[] portalTraversals, out Vector3 endPoint, bool ignorePortalMargin = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		List<PortalTraversalV2> list = new List<PortalTraversalV2>();
		Raycast(origin, direction, maxDistance, default(LayerMask), out var _, out var portalTraversals2, out var endPoint2, queryTriggerInteraction);
		Vector3 segStart = origin;
		float num = 0f;
		PortalTraversalV2 portalTraversalV = default(PortalTraversalV2);
		NativePortalTransform? nativePortalTransform = null;
		NativePortalTransform? nativePortalTransform2 = null;
		for (int i = 0; i < portalTraversals2.Length + 1; i++)
		{
			bool flag = i == 0;
			bool flag2 = i >= portalTraversals2.Length;
			Vector3 vector;
			if (!flag2)
			{
				portalTraversalV = portalTraversals2[i];
				vector = portalTraversalV.entrancePoint;
				nativePortalTransform = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side);
			}
			else
			{
				vector = endPoint2;
				nativePortalTransform = null;
			}
			Vector3 direction2 = vector - segStart;
			float num2 = Vector3.Distance(segStart, vector);
			num += num2;
			RaycastHit[] array = Physics.SphereCastAll(segStart, radius, direction2, num2, layerMask, queryTriggerInteraction);
			List<RaycastHit> list2 = new List<RaycastHit>();
			RaycastHit[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				RaycastHit item = array2[j];
				if (!flag || item.distance != 0f)
				{
					Vector3 point = ((item.distance > 0f) ? item.point : item.collider.ClosestPoint(segStart));
					if ((!nativePortalTransform.HasValue || point.IsInFrontOfPortal(nativePortalTransform.Value)) && (!nativePortalTransform2.HasValue || point.IsInFrontOfPortal(nativePortalTransform2.Value)))
					{
						list2.Add(item);
					}
					else if ((!nativePortalTransform.HasValue || item.collider.IsPartiallyInFrontOf(nativePortalTransform.Value)) && (!nativePortalTransform2.HasValue || item.collider.IsPartiallyInFrontOf(nativePortalTransform2.Value)))
					{
						list2.Add(item);
					}
				}
			}
			if (list2.Count > 0)
			{
				float min = ((IEnumerable<RaycastHit>)list2).Min((Func<RaycastHit, float>)FixHitDistance);
				RaycastHit raycastHit = list2.First((RaycastHit h) => FixHitDistance(h) == min);
				bool flag3 = raycastHit.distance == 0f;
				Vector3 vector2 = (flag3 ? raycastHit.collider.ClosestPoint(segStart) : raycastHit.point);
				float num3 = (flag3 ? Vector3.Distance(segStart, raycastHit.collider.ClosestPoint(segStart)) : raycastHit.distance);
				hitInfo = new PhysicsCastResult
				{
					distance = num + num3,
					point = vector2,
					direction = (vector2 - segStart).normalized,
					normal = (flag3 ? Vector3.zero : raycastHit.normal),
					transform = raycastHit.transform,
					collider = raycastHit.collider,
					rigidbody = raycastHit.rigidbody
				};
				portalTraversals = list.ToArray();
				endPoint = segStart + Vector3.Dot(direction2.normalized, vector2 - segStart) * direction2.normalized;
				return true;
			}
			if (!flag2)
			{
				if (!ignorePortalMargin && !IsInPortalGeometryMargin(portalTraversalV.portalObject, portalTraversalV.portalHandle.side, vector, radius))
				{
					hitInfo = PhysicsCastResult.FromPoints(segStart, vector);
					portalTraversals = list.ToArray();
					endPoint = vector;
					return true;
				}
				list.Add(portalTraversals2[i]);
				segStart = portalTraversalV.exitPoint;
				nativePortalTransform2 = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side.Reverse());
			}
		}
		hitInfo = PhysicsCastResult.Empty();
		portalTraversals = list.ToArray();
		endPoint = endPoint2;
		return false;
		float FixHitDistance(RaycastHit hit)
		{
			if (hit.distance != 0f)
			{
				return hit.distance;
			}
			return Vector3.Distance(segStart, hit.collider.ClosestPoint(segStart));
		}
	}

	public static PhysicsCastResult[] SphereCastAll(Vector3 origin, Vector3 direction, float maxDistance, float radius, int layerMask, out PortalTraversalV2[] portalTraversals, out Vector3 endPoint, bool ignorePortalMargin = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		HashSet<PhysicsCastResult> confirmedHits = new HashSet<PhysicsCastResult>();
		List<PortalTraversalV2> list = new List<PortalTraversalV2>();
		Raycast(origin, direction, maxDistance, default(LayerMask), out var _, out var portalTraversals2, out var endPoint2, queryTriggerInteraction);
		Vector3 vector = origin;
		PortalTraversalV2 portalTraversalV = default(PortalTraversalV2);
		NativePortalTransform? nativePortalTransform = null;
		NativePortalTransform? nativePortalTransform2 = null;
		float num = 0f;
		for (int i = 0; i < portalTraversals2.Length + 1; i++)
		{
			bool flag = i >= portalTraversals2.Length;
			Vector3 vector2;
			if (!flag)
			{
				portalTraversalV = portalTraversals2[i];
				vector2 = portalTraversalV.entrancePoint;
				nativePortalTransform = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side);
			}
			else
			{
				vector2 = endPoint2;
				nativePortalTransform = null;
			}
			Vector3 direction2 = vector2 - vector;
			float num2 = Vector3.Distance(vector, vector2);
			RaycastHit[] array = Physics.SphereCastAll(vector, radius, direction2, num2, layerMask, queryTriggerInteraction);
			for (int j = 0; j < array.Length; j++)
			{
				RaycastHit rayHit = array[j];
				Vector3 point = ((rayHit.distance > 0f) ? rayHit.point : rayHit.collider.transform.position);
				if ((!nativePortalTransform.HasValue || point.IsInFrontOfPortal(nativePortalTransform.Value)) && (!nativePortalTransform2.HasValue || point.IsInFrontOfPortal(nativePortalTransform2.Value)))
				{
					ConfirmHit(rayHit, direction2, num);
				}
				else if ((!nativePortalTransform.HasValue || rayHit.collider.IsPartiallyInFrontOf(nativePortalTransform.Value)) && (!nativePortalTransform2.HasValue || rayHit.collider.IsPartiallyInFrontOf(nativePortalTransform2.Value)))
				{
					ConfirmHit(rayHit, direction2, num);
				}
			}
			if (!flag)
			{
				if (!ignorePortalMargin && !IsInPortalGeometryMargin(portalTraversalV.portalObject, portalTraversalV.portalHandle.side, vector2, radius))
				{
					break;
				}
				list.Add(portalTraversals2[i]);
				vector = portalTraversalV.exitPoint;
				nativePortalTransform2 = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side.Reverse());
				num += num2;
			}
		}
		portalTraversals = list.ToArray();
		endPoint = endPoint2;
		return confirmedHits.ToArray();
		void ConfirmHit(RaycastHit hit, Vector3 vector3, float additionalDistance)
		{
			PhysicsCastResult item = PhysicsCastResult.FromRaycastHit(hit);
			item.distance += additionalDistance;
			item.direction = vector3.normalized;
			confirmedHits.Add(item);
		}
	}

	public static bool IsInPortalGeometryMargin(Portal portal, PortalSide side, Vector3 worldIntersect, float margin = 0f)
	{
		Vector3 localIntersect = portal.GetTransform(side).toLocalManaged.MultiplyPoint3x4(worldIntersect);
		return IsInPortalShapeMargin(portal.GetShape(), localIntersect, margin);
	}

	public static bool IsInPortalShapeMargin(PlaneShape shape, Vector3 localIntersect, float margin = 0f)
	{
		if (Mathf.Abs(localIntersect.x) + margin <= shape.width / 2f)
		{
			return Mathf.Abs(localIntersect.y) + margin <= shape.height / 2f;
		}
		return false;
	}

	public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PhysicsCastResult hitInfo;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		return Raycast(origin, direction, maxDistance, layerMask, out hitInfo, out portalTraversals, out endPoint, queryTriggerInteraction);
	}

	public static bool Raycast(Vector3 origin, Vector3 direction, out PhysicsCastResult hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		return Raycast(origin, direction, maxDistance, layerMask, out hitInfo, out portalTraversals, out endPoint, queryTriggerInteraction);
	}

	public static bool Raycast(Vector3 origin, Vector3 direction, out PhysicsCastResult hitInfo, out Vector3 endPoint, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PortalTraversalV2[] portalTraversals;
		return Raycast(origin, direction, maxDistance, layerMask, out hitInfo, out portalTraversals, out endPoint, queryTriggerInteraction);
	}

	public static bool Raycast(PortalScene scene, Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out PhysicsCastResult hitInfo, out PortalTraversalV2[] portalTraversals, out Vector3 endPoint, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PortalCastStateV2 state = new PortalCastStateV2
		{
			layerMask = layerMask,
			queryTriggerInteraction = queryTriggerInteraction,
			origin = origin,
			direction = direction,
			maxDistance = maxDistance
		};
		PhysicsCastResult result2;
		bool result = PortalCast(scene, RaycastCastable, state, out portalTraversals, out result2, out endPoint);
		hitInfo = result2;
		return result;
	}

	public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out PhysicsCastResult hitInfo, out PortalTraversalV2[] portalTraversals, out Vector3 endPoint, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PortalCastStateV2 state = new PortalCastStateV2
		{
			layerMask = layerMask,
			queryTriggerInteraction = queryTriggerInteraction,
			origin = origin,
			direction = direction.normalized,
			maxDistance = maxDistance
		};
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		if (!instance)
		{
			endPoint = state.origin + state.direction * state.maxDistance;
			bool result = RaycastCastable.Cast(state, out hitInfo);
			portalTraversals = EmptyTraversals;
			return result;
		}
		PhysicsCastResult result3;
		bool result2 = PortalCast(instance.Scene, RaycastCastable, state, out portalTraversals, out result3, out endPoint);
		hitInfo = result3;
		return result2;
	}

	public static bool Raycast(PortalCastStateV2 state, out PortalTraversalV2[] portalTraversals, out PhysicsCastResult result, out Vector3 endPoint)
	{
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		if (!instance)
		{
			PhysicsCastResult result3;
			bool result2 = RaycastCastable.Cast(state, out result3);
			portalTraversals = EmptyTraversals;
			result = result3;
			endPoint = state.origin + state.direction.normalized * state.maxDistance;
			return result2;
		}
		return PortalCast(instance.Scene, RaycastCastable, state, out portalTraversals, out result, out endPoint);
	}

	public static PhysicsCastResult[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out PortalTraversalV2[] portalTraversals, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		PortalCastStateV2 state = new PortalCastStateV2
		{
			layerMask = layerMask,
			queryTriggerInteraction = queryTriggerInteraction,
			origin = origin,
			direction = direction,
			maxDistance = maxDistance
		};
		return PortalCastAll(RaycastCastable, state, out portalTraversals);
	}

	public static PhysicsCastResult[] RaycastAll(PortalCastStateV2 state, out PortalTraversalV2[] portalTraversals)
	{
		return PortalCastAll(RaycastCastable, state, out portalTraversals);
	}

	internal static bool PortalCast(PortalScene scene, ICastable castable, PortalCastStateV2 state, out PortalTraversalV2[] portalTraversals, out PhysicsCastResult result, out Vector3 endPoint)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		traversalsList.Clear();
		endPoint = state.origin + state.direction * state.maxDistance;
		Vector3 vector = state.origin;
		Vector3 vector2 = state.direction;
		float num = state.maxDistance;
		float num2 = 0f;
		int num3 = 0;
		if (!float.IsFinite(num))
		{
			num = 10000000f;
		}
		while (num3 < 5 && num > 0f)
		{
			PortalCastStateV2 state2 = state;
			state2.origin = vector;
			state2.direction = vector2;
			state2.maxDistance = num;
			PhysicsCastResult result2;
			bool flag = castable.Cast(state2, out result2);
			Vector3 zero = Vector3.zero;
			endPoint = vector + vector2 * num;
			result2.direction = vector2;
			PortalHandle hitPortal;
			Vector3 intersection;
			float distance;
			bool flag2 = scene.FindPortalBetween(vector, endPoint, out hitPortal, out intersection, out distance);
			if (flag && flag2)
			{
				if (distance >= result2.distance)
				{
					flag2 = false;
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				result = result2;
				result.distance = num2 + result2.distance;
				portalTraversals = traversalsList.ToArray();
				return true;
			}
			if (!flag2 || !float.IsFinite(distance) || distance > num)
			{
				break;
			}
			zero = intersection;
			NativePortal nativePortal = scene.nativeScene.LookupPortal(in hitPortal);
			Matrix4x4 travelMatrixManaged = nativePortal.travelMatrixManaged;
			Vector3 entrance = zero;
			Vector3 entranceDir = vector2;
			float3 val = math.transform(float4x4.op_Implicit(travelMatrixManaged), float3.op_Implicit(zero));
			zero = float3.op_Implicit(val);
			vector = zero;
			vector2 = float3.op_Implicit(math.rotate(float4x4.op_Implicit(travelMatrixManaged), float3.op_Implicit(vector2)));
			num -= distance;
			num2 += distance;
			num3++;
			PortalTraversalV2 item = new PortalTraversalV2(entrance, entranceDir, float3.op_Implicit(val), vector2, hitPortal, scene.GetPortalObject(hitPortal));
			traversalsList.Add(item);
		}
		result = PhysicsCastResult.FromDirectionDistance(vector, vector2, num);
		portalTraversals = traversalsList.ToArray();
		return false;
	}

	internal static PhysicsCastResult[] PortalCastAll(ICastable castable, PortalCastStateV2 state, out PortalTraversalV2[] portalTraversals)
	{
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		if (!instance)
		{
			PhysicsCastResult[] result = castable.CastAll(state);
			portalTraversals = EmptyTraversals;
			return result;
		}
		state.direction.Normalize();
		PortalScene scene = instance.Scene;
		List<PortalTraversalV2> list = new List<PortalTraversalV2>();
		List<PhysicsCastResult> list2 = new List<PhysicsCastResult>();
		Vector3 vector = state.origin;
		Vector3 vector2 = state.direction;
		float num = state.maxDistance;
		int num2 = 0;
		if (!float.IsFinite(num))
		{
			num = 10000000f;
		}
		while (num2 < 5 && num > 0f)
		{
			PortalCastStateV2 state2 = state;
			state2.origin = vector;
			state2.direction = vector2;
			state2.maxDistance = num;
			PhysicsCastResult[] array = castable.CastAll(state2);
			Vector3 end = vector + vector2 * num;
			if (scene.FindPortalBetween(vector, end, out var hitPortal, out var intersection, out var _))
			{
				float num3 = Vector3.Distance(vector, intersection);
				Portal portalObject = scene.GetPortalObject(hitPortal);
				Matrix4x4 travelMatrix = portalObject.GetTravelMatrix(hitPortal.side);
				Vector3 entrance = intersection;
				Vector3 entranceDir = vector2;
				Vector3 vector3 = travelMatrix.MultiplyPoint3x4(intersection);
				Vector3 vector4 = travelMatrix.rotation * vector2;
				for (int i = 0; i < array.Length; i++)
				{
					PhysicsCastResult item = array[i];
					if (item.distance < num3)
					{
						item.direction = vector4;
						list2.Add(item);
						Log.Info($"Added physics hit before portal: {item.collider.name} at distance {item.distance}", (IEnumerable<Tag>)null, (string)null, (object)null);
					}
				}
				vector = vector3;
				vector2 = vector4;
				num -= num3;
				num2++;
				PortalTraversalV2 item2 = new PortalTraversalV2(entrance, entranceDir, vector3, vector4, hitPortal, portalObject);
				list.Add(item2);
				Log.Info($"Traversed portal at {intersection}, new origin {vector}, new direction {vector2}", (IEnumerable<Tag>)null, (string)null, (object)null);
				continue;
			}
			for (int j = 0; j < array.Length; j++)
			{
				PhysicsCastResult item3 = array[j];
				item3.direction = vector2;
				list2.Add(item3);
				Log.Info($"Added physics hit: {item3.collider.name} at distance {item3.distance}", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			break;
		}
		portalTraversals = list.ToArray();
		PhysicsCastResult[] array2 = list2.ToArray();
		if (array2.Length > 1)
		{
			Array.Sort(array2);
		}
		return array2;
	}
}
