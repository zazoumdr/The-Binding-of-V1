using System.Text;
using ULTRAKILL.Portal;
using UnityEngine;

namespace ULTRAKILL.Enemy;

public struct TargetData
{
	public static readonly TargetData None;

	public TargetHandle handle;

	public Vector3 position;

	public Vector3 headPosition;

	public Quaternion rotation;

	public Vector3 velocity;

	public Vector3 realPosition;

	public Vector3 realHeadPosition;

	public Matrix4x4 portalMatrix;

	public readonly bool isAcrossPortals
	{
		get
		{
			TargetHandle targetHandle = handle;
			if ((object)targetHandle == null)
			{
				return false;
			}
			return targetHandle.portals.Count > 0;
		}
	}

	public readonly ITarget target => handle.target;

	public ref readonly PortalHandleSequence portals => ref handle.portals;

	public readonly bool isValid()
	{
		return handle != null;
	}

	public static TargetData For(ref TargetHandle handle)
	{
		return new TargetData
		{
			handle = handle
		};
	}

	public void ResetToDefault()
	{
		position = Vector3.zero;
		headPosition = Vector3.zero;
		rotation = Quaternion.identity;
		velocity = Vector3.zero;
		realPosition = Vector3.zero;
		realHeadPosition = Vector3.zero;
		portalMatrix = Matrix4x4.identity;
	}

	public static implicit operator TargetHandle(TargetData data)
	{
		return data.handle;
	}

	public readonly float DistanceTo(Vector3 point, bool fromHead = false)
	{
		return Vector3.Distance(fromHead ? headPosition : position, point);
	}

	public readonly bool IsObstructed(Vector3 point, LayerMask layerMask, bool toHead = false)
	{
		PhysicsCastResult obstructionResult;
		PortalTraversalV2[] traversals;
		return IsObstructed(point, layerMask, toHead, out obstructionResult, out traversals);
	}

	public readonly bool IsObstructed(Vector3 point, LayerMask layerMask, bool toHead, out PhysicsCastResult obstructionResult, out PortalTraversalV2[] traversals)
	{
		Vector3 direction = (toHead ? headPosition : position) - point;
		if (PortalPhysicsV2.Raycast(point, direction, direction.magnitude, layerMask, out obstructionResult, out traversals, out var _, QueryTriggerInteraction.Ignore))
		{
			return true;
		}
		return handle.portals.Reversed() != traversals;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ITarget target = handle.target;
		if (target.Type == TargetType.PLAYER)
		{
			stringBuilder.Append("Player ");
		}
		else if (target.Type == TargetType.ENEMY)
		{
			stringBuilder.Append($"{target.EID.enemyType} ");
		}
		else
		{
			stringBuilder.Append((target.GameObject?.name ?? ("Other(" + target.Type.ToString() + ")")) + " ");
		}
		stringBuilder.Append($"Position: {position} ");
		return stringBuilder.ToString();
	}
}
