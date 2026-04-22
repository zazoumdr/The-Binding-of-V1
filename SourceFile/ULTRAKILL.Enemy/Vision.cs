using System;
using System.Runtime.InteropServices;
using ULTRAKILL.Portal;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Enemy;

public class Vision : IDisposable
{
	public VisionTypeFilter filter;

	public JobHandle handle;

	public NativeList<TargetIndexAndDistance> targetsArray;

	public NativeArray<int> startIndices;

	public NativeArray<int> counts;

	public int visionIndex = -1;

	public Vector3 sourcePos;

	public float maxDistance = 120f;

	public Vision(Vector3 sourcePos, VisionTypeFilter typeFilter)
	{
		this.sourcePos = sourcePos;
		filter = typeFilter;
	}

	public void UpdateFilter(VisionTypeFilter filter)
	{
		this.filter = filter;
	}

	public void UpdateSourcePos(Vector3 sourcePos)
	{
		this.sourcePos = sourcePos;
	}

	public static bool ValidateEIDTarget(TargetData data, EnemyTarget eidTarget)
	{
		if (eidTarget == null)
		{
			return false;
		}
		ITarget target = data.target;
		if (target.isPlayer && eidTarget.isPlayer)
		{
			return true;
		}
		if (target.Type == TargetType.ENEMY && target.EID == eidTarget.enemyIdentifier)
		{
			return true;
		}
		return false;
	}

	private bool IsVisibleToLastPortal(ref PortalHandleSequence sequence, PortalScene scene, Vector3 sourcePos)
	{
		PortalHandle portalHandle = sequence[sequence.Count - 1];
		return scene.GetPortalObject(portalHandle).GetTransform(portalHandle.side.Reverse()).IsPointInFront(sourcePos);
	}

	public bool TrySee(VisionQuery query, out TargetDataRef data)
	{
		data = default(TargetDataRef);
		if (visionIndex == -1)
		{
			return false;
		}
		TargetTracker targetTracker = MonoSingleton<PortalManagerV2>.Instance.TargetTracker;
		handle.Complete();
		int num = startIndices[visionIndex];
		int num2 = counts[visionIndex];
		PortalScene scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		Span<TargetIndexAndDistance> span = targetsArray.AsArray().AsSpan();
		Span<PortalHandleSequence> sequenceSpan = CollectionsMarshal.AsSpan(scene.portalSequences);
		Span<float4x4> matricesSpan = scene.sequenceMatrices.AsSpan();
		for (int i = 0; i < num2; i++)
		{
			TargetDataRef targetDataRef = targetTracker.GetTargetDataRef(matricesSpan, sequenceSpan, span[num + i].index);
			if (query.predicate(targetDataRef))
			{
				data = targetDataRef;
				return true;
			}
		}
		return false;
	}

	public TargetDataRef GetDataRef(TargetHandle target)
	{
		return MonoSingleton<PortalManagerV2>.Instance.TargetTracker.GetDataReference(target);
	}

	public TargetData CalculateData(TargetHandle target)
	{
		return MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(target);
	}

	public void Dispose()
	{
	}
}
