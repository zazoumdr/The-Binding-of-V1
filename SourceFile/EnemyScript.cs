using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
	public virtual Vector3 VisionSourcePosition => base.transform.position;

	public virtual EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = -1f
		};
	}

	public virtual bool ShouldKnockback(ref DamageData data)
	{
		return true;
	}

	public virtual void OnGoLimp(bool fromExplosion)
	{
	}

	public virtual void OnFall()
	{
	}

	public virtual void OnLand()
	{
	}

	public virtual void OnDamage(ref DamageData data)
	{
	}

	public virtual void OnParry(ref DamageData data, bool isShotgun)
	{
	}

	public virtual void OnTargetTick()
	{
	}

	public virtual void OnTeleport(PortalTravelDetails details)
	{
	}

	public virtual void SetSortiePos(Vector3 pos)
	{
	}

	protected Vector3 ToPlanePos(Vector3 pos)
	{
		return new Vector3(pos.x, base.transform.position.y, pos.z);
	}

	protected float GetUpdateRate(NavMeshAgent nma = null, float min = 0.2f, float max = 0.5f, float distanceThreshold = 10f)
	{
		if ((Object)(object)nma == null || !((Behaviour)(object)nma).enabled)
		{
			return min;
		}
		if (!nma.pathPending && (!nma.isOnNavMesh || !(nma.remainingDistance >= distanceThreshold)))
		{
			return min;
		}
		return max;
	}

	public static bool CheckTarget(TargetDataRef targetData, EnemyIdentifier eid)
	{
		ITarget target = targetData.target;
		if (eid.target.isPlayer)
		{
			if (eid.ignorePlayer)
			{
				return false;
			}
			if (!target.isPlayer)
			{
				return false;
			}
			return true;
		}
		if (eid.target.isEnemy)
		{
			if (!target.isEnemy)
			{
				return false;
			}
			if (eid.target.enemyIdentifier != null)
			{
				return (object)eid.target.enemyIdentifier == target.EID;
			}
			if (eid.enemyClass == target.EID.enemyClass)
			{
				return false;
			}
			if (!eid.attackEnemies)
			{
				return false;
			}
			return EnemiesHateEnemies.Active;
		}
		return false;
	}

	protected bool ChasePortalTarget(NavMeshAgent nma, TargetData visionData, NavMeshPath path)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		PortalHandle handle = visionData.handle.portals[0];
		if (PortalUtils.GetPortalObject(handle).GetTravelFlags(handle.side).HasFlag(PortalTravellerFlags.Enemy) && handle.GetPortalIdentifier().FindClosestLinkPositions(base.transform.position, out var entryPos, out var exitPos) && nma.isOnNavMesh && nma.CalculatePath(entryPos, path))
		{
			if ((int)path.status == 0)
			{
				nma.SetDestination(exitPos);
				return true;
			}
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(entryPos, ref val, 5f, nma.areaMask))
			{
				nma.SetDestination(((NavMeshHit)(ref val)).position);
				return true;
			}
		}
		return false;
	}

	protected void NavigateInFrontOfPortal(NavMeshAgent nma, PortalHandle handle)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Portal portalObject = PortalUtils.GetPortalObject(handle);
		NativePortalTransform nativePortalTransform = portalObject.GetTransform(handle.side);
		float3 val = nativePortalTransform.center + nativePortalTransform.back;
		PlaneShape planeShape = (PlaneShape)(object)portalObject.shape;
		float num = Mathf.Max(planeShape.height, planeShape.width);
		NavMeshHit val2 = default(NavMeshHit);
		if (NavMesh.SamplePosition(float3.op_Implicit(val), ref val2, num, nma.areaMask))
		{
			nma.SetDestination(((NavMeshHit)(ref val2)).position);
		}
	}

	protected void AcquirePortalVision(NavMeshAgent nma, AcquirePortalVisionState state)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Invalid comparison between Unknown and I4
		if (state.type == AcquirePortalVisionType.NONE)
		{
			return;
		}
		if (state.type == AcquirePortalVisionType.UNDETERMINED)
		{
			if (state.target.handle.portals.Count > 0)
			{
				PortalHandleSequence portals = state.target.handle.portals;
				PortalHandle handle = portals[portals.Count - 1].Reverse();
				if (PortalUtils.GetPortalObject(handle).IsFacingDown(handle.side))
				{
					AcquirePortalVisionType type = (state.target.IsTargetInPortalOrth() ? AcquirePortalVisionType.PROJECT_W_NORMAL : AcquirePortalVisionType.PROJECT_W_CENTER);
					state.SetType(type);
				}
				else
				{
					state.SetType(AcquirePortalVisionType.NONE);
					state.Complete();
				}
			}
			else
			{
				state.SetType(AcquirePortalVisionType.NONE);
				state.Complete();
			}
		}
		if (!nma.pathPending && !state.completed)
		{
			Vector3 closestPos;
			if (state.started && ((double)nma.remainingDistance < 0.1 || (int)nma.pathStatus == 2))
			{
				nma.SetDestination(base.transform.position);
				state.Complete();
			}
			else if (!state.started && nma.GetPortalVisionPos(state, base.transform.position, out closestPos))
			{
				nma.SetDestination(closestPos);
				state.Start();
			}
		}
	}
}
