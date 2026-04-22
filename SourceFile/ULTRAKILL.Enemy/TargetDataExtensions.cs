using System;
using ULTRAKILL.Portal;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Enemy;

public static class TargetDataExtensions
{
	public static TargetData ToData(this TargetDataRef src)
	{
		return new TargetData
		{
			handle = src.CreateHandle(),
			portalMatrix = src.portalMatrix,
			position = src.position,
			headPosition = src.headPosition,
			realPosition = src.target.Position,
			realHeadPosition = src.target.HeadPosition,
			velocity = src.velocity
		};
	}

	public static float SqrDist(this TargetDataRef @this, Vector3 point)
	{
		return (@this.position - point).sqrMagnitude;
	}

	public static float DistanceTo(this TargetDataRef @this, Vector3 point)
	{
		return (@this.position - point).magnitude;
	}

	public static bool IsObstructed(this TargetDataRef @this, Vector3 point, LayerMask layerMask, bool toHead = false)
	{
		RaycastHit obstructionResult;
		return @this.IsObstructed(point, layerMask, toHead, out obstructionResult);
	}

	public static bool IsObstructed(this TargetDataRef @this, Vector3 point, LayerMask layerMask, bool toHead, out RaycastHit obstructionResult)
	{
		PortalTraversalV2[] traversals;
		return @this.IsObstructed(point, layerMask, toHead, out obstructionResult, out traversals);
	}

	public static bool IsObstructed(this TargetDataRef @this, Vector3 point, LayerMask layerMask, bool toHead, out RaycastHit obstructionResult, out PortalTraversalV2[] traversals)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		obstructionResult = default(RaycastHit);
		traversals = Array.Empty<PortalTraversalV2>();
		if (@this.isSequenceCulled)
		{
			return true;
		}
		Vector3 end = (toHead ? @this.headPosition : @this.position);
		PortalScene scene = @this.scene;
		PortalHandleSequence portals = @this.portals;
		if (portals.Count == 0)
		{
			if (scene.FindPortalBetween(point, end, out var _, out var _, out var _, allowBackfaces: true))
			{
				return true;
			}
			RaycastHit hitInfo;
			bool result = Physics.Linecast(point, toHead ? @this.headPosition : @this.position, out hitInfo, layerMask, QueryTriggerInteraction.Ignore);
			obstructionResult = hitInfo;
			return result;
		}
		PortalHandleSequence sequence = portals.Reversed();
		if (!scene.TraversePortalSequence(point, end, @this.target.Position, sequence, out var outSegments))
		{
			return true;
		}
		for (int i = 0; i < outSegments.Length; i++)
		{
			PortalScene.PortalRaySegment portalRaySegment = outSegments[i];
			float3 val = portalRaySegment.direction * 0.1f;
			float3 val2 = portalRaySegment.start + val;
			float3 val3 = portalRaySegment.end - val;
			if (scene.FindPortalBetween(float3.op_Implicit(val2), float3.op_Implicit(val3), out var _, out var _, out var _, allowBackfaces: true))
			{
				return true;
			}
		}
		for (int j = 0; j < outSegments.Length; j++)
		{
			PortalScene.PortalRaySegment portalRaySegment2 = outSegments[j];
			if (Physics.Linecast(float3.op_Implicit(portalRaySegment2.start), float3.op_Implicit(portalRaySegment2.end), out var hitInfo2, layerMask, QueryTriggerInteraction.Ignore))
			{
				obstructionResult = hitInfo2;
				return true;
			}
		}
		traversals = new PortalTraversalV2[outSegments.Length - 1];
		int length = outSegments.Length;
		PortalScene.PortalRaySegment portalRaySegment3 = outSegments[0];
		float3 val4 = math.normalize(portalRaySegment3.direction);
		for (int k = 1; k < length; k++)
		{
			PortalScene.PortalRaySegment portalRaySegment4 = outSegments[k];
			float3 val5 = math.normalize(portalRaySegment4.direction);
			traversals[k - 1] = new PortalTraversalV2(float3.op_Implicit(portalRaySegment3.end), float3.op_Implicit(val4), float3.op_Implicit(portalRaySegment4.start), float3.op_Implicit(val5), portalRaySegment3.handle.Reverse(), scene.GetPortalObject(portalRaySegment3.handle));
			portalRaySegment3 = portalRaySegment4;
			val4 = val5;
		}
		return false;
	}

	public static Vector3 PredictTargetPosition(this TargetData data, float time, bool includeGravity = false, bool assumeGroundMovement = false)
	{
		return PredictTargetPosition(data.target, in data.position, in data.portalMatrix, in data.velocity, time, includeGravity, assumeGroundMovement);
	}

	public static Vector3 PredictTargetPosition(this TargetDataRef data, float time, bool includeGravity = false, bool assumeGroundMovement = false)
	{
		return PredictTargetPosition(data.target, in data.position, in data.portalMatrix, in data.velocity, time, includeGravity, assumeGroundMovement);
	}

	public static Vector3 PredictTargetPosition(ITarget target, in Vector3 position, in Matrix4x4 portalMatrix, in Vector3 velocity, float time, bool includeGravity = false, bool assumeGroundMovement = false)
	{
		Vector3 vector = velocity * time;
		if (includeGravity)
		{
			bool flag = false;
			if (target.isPlayer)
			{
				flag = MonoSingleton<PlayerTracker>.Instance.GetOnGround();
			}
			else if (target.EID != null && (bool)target.Rigidbody)
			{
				flag = target.Rigidbody.isKinematic;
			}
			if (!flag)
			{
				Vector3 vector2 = portalMatrix.MultiplyVector(Physics.gravity);
				vector += 0.5f * vector2 * (time * time);
			}
		}
		Vector3 position2 = target.Position;
		Vector3 direction = portalMatrix.inverse.MultiplyVector(vector);
		if (PortalPhysicsV2.Raycast(position2, direction, direction.magnitude, LayerMaskDefaults.Get(LMD.Environment), out var hitInfo, out var portalTraversals, out var _, QueryTriggerInteraction.Ignore))
		{
			Vector3 point = hitInfo.point;
			if (portalTraversals.Length != 0)
			{
				for (int num = portalTraversals.Length - 1; num >= 0; num--)
				{
					PortalTraversalV2 portalTraversalV = portalTraversals[num];
					PortalSide side = portalTraversalV.portalHandle.side;
					point = portalTraversalV.portalObject.GetTravelMatrix(side.Reverse()).MultiplyPoint3x4(point);
				}
			}
			if (assumeGroundMovement)
			{
				direction = position2 + new Vector3(direction.x, point.y - position2.y, direction.z);
				return portalMatrix.MultiplyPoint3x4(direction);
			}
			return portalMatrix.MultiplyPoint3x4(point);
		}
		return vector + position;
	}
}
