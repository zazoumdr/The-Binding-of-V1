using System;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

[BurstCompile]
public struct ParticleIntersectionJob : IJobFor
{
	[ReadOnly]
	public NativePortalScene scene;

	[NativeDisableContainerSafetyRestriction]
	[ReadOnly]
	public NativeArray<RaycastCommand> rays;

	[NativeDisableContainerSafetyRestriction]
	public NativeArray<IntersectionAndIndex> intersections;

	public void Execute(int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		RaycastCommand raycastCommand = rays[index];
		PortalRay ray = new PortalRay
		{
			start = float3.op_Implicit(raycastCommand.from),
			direction = float3.op_Implicit(raycastCommand.direction),
			distanceSq = raycastCommand.distance * raycastCommand.distance
		};
		intersections[index] = new IntersectionAndIndex
		{
			index = index,
			intersection = new NativePortalIntersection
			{
				handle = PortalHandle.None,
				distance = float.PositiveInfinity
			}
		};
		ReadOnlySpan<NativePortal> readOnlySpan = scene.portals.AsArray().AsReadOnlySpan();
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i].Raycast(in ray, out var intersection) && intersection.distance < intersections[index].intersection.distance)
			{
				intersections[index] = new IntersectionAndIndex
				{
					index = index,
					intersection = intersection
				};
			}
		}
	}
}
