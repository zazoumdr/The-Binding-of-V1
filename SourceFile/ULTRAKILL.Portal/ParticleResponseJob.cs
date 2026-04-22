using System;
using Interop;
using Interop.core;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace ULTRAKILL.Portal;

[BurstCompile]
public struct ParticleResponseJob : IJobParticleSystem
{
	public float4x4 toLocal;

	public float4x4 toWorld;

	[ReadOnly]
	public NativePortalScene scene;

	[NativeDisableContainerSafetyRestriction]
	[ReadOnly]
	public NativeSlice<IntersectionAndIndex> intersections;

	[NativeDisableContainerSafetyRestriction]
	[ReadOnly]
	public NativeSlice<RaycastHit> hits;

	[NativeDisableUnsafePtrRestriction]
	public unsafe ParticleTrails* trails;

	public unsafe void Execute(ParticleSystemJobData jobData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystemNativeArray4 customData = ((ParticleSystemJobData)(ref jobData)).customData1;
		int count = ((ParticleSystemJobData)(ref jobData)).count;
		for (int i = 0; i < count; i++)
		{
			NativePortalIntersection intersection = intersections[i].intersection;
			RaycastHit raycastHit = hits[i];
			bool flag = intersection.handle.IsValid();
			bool flag2 = raycastHit.colliderInstanceID != 0;
			if (!flag && !flag2)
			{
				continue;
			}
			if (flag && (!flag2 || intersection.distance < raycastHit.distance))
			{
				PortalHandle handle = intersection.handle;
				NativePortal nativePortal = scene.LookupPortal(in handle);
				if (nativePortal.valid)
				{
					float4x4 travelMatrix = nativePortal.travelMatrix;
					((ParticleSystemNativeArray4)(ref customData))[i] = float4.op_Implicit(math.float4(math.transform(travelMatrix, intersection.point), 1f));
					ParticleSystemNativeArray3 positions = ((ParticleSystemJobData)(ref jobData)).positions;
					ParticleSystemNativeArray3 velocities = ((ParticleSystemJobData)(ref jobData)).velocities;
					Vector3 vector = ((ParticleSystemNativeArray3)(ref positions))[i];
					Vector3 vector2 = ((ParticleSystemNativeArray3)(ref velocities))[i];
					float3 val = math.transform(toWorld, float3.op_Implicit(vector));
					val = math.transform(travelMatrix, val);
					float3 val2 = math.rotate(toWorld, float3.op_Implicit(vector2));
					val2 = math.rotate(travelMatrix, val2);
					((ParticleSystemNativeArray3)(ref positions))[i] = float3.op_Implicit(math.transform(toLocal, val));
					((ParticleSystemNativeArray3)(ref velocities))[i] = float3.op_Implicit(math.rotate(toLocal, val2));
					vector<Vector4> positions2 = ((ParticleTrails)trails).m_Positions;
					int num = (int)(nuint)((ParticleTrails)trails).m_MaxPositionsPerTrail;
					Span<float4> span = new Span<float4>(positions2.data(), (int)(nuint)positions2.size()).Slice(num * i, num);
					for (int j = 0; j < span.Length; j++)
					{
						ref float4 reference = ref span[j];
						float3 val3 = math.transform(toWorld, ((float4)(ref reference)).xyz);
						val3 = math.transform(travelMatrix, val3);
						reference = new float4(math.transform(toLocal, val3), reference.w);
					}
				}
			}
			else
			{
				NativeArray<float> aliveTimePercent = ((ParticleSystemJobData)(ref jobData)).aliveTimePercent;
				aliveTimePercent[i] = 100f;
			}
		}
	}
}
