using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace ULTRAKILL.Portal;

[BurstCompile]
public struct ParticleCommandJob : IJobParticleSystem
{
	private PhysicsScene scene;

	public float4x4 toWorld;

	public QueryParameters parameters;

	[NativeDisableContainerSafetyRestriction]
	[WriteOnly]
	public NativeSlice<RaycastCommand> raycasts;

	public void Execute(ParticleSystemJobData jobData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystemNativeArray4 customData = ((ParticleSystemJobData)(ref jobData)).customData1;
		int count = ((ParticleSystemJobData)(ref jobData)).count;
		for (int i = 0; i < count; i++)
		{
			ParticleSystemNativeArray4 customData2 = ((ParticleSystemJobData)(ref jobData)).customData1;
			float4 val = math.float4(float4.op_Implicit(((ParticleSystemNativeArray4)(ref customData2))[i]));
			float3 xyz = ((float4)(ref val)).xyz;
			float4x4 val2 = toWorld;
			ParticleSystemNativeArray3 positions = ((ParticleSystemJobData)(ref jobData)).positions;
			float3 val3 = math.transform(val2, float3.op_Implicit(((ParticleSystemNativeArray3)(ref positions))[i]));
			if (math.all(xyz == new float3(0f, 0f, 0f)))
			{
				xyz = ((float4)(ref toWorld.c3)).xyz;
			}
			((ParticleSystemNativeArray4)(ref customData))[i] = float3.op_Implicit(val3);
			float3 val4 = val3 - xyz;
			raycasts[i] = new RaycastCommand(float3.op_Implicit(xyz), float3.op_Implicit(math.normalizesafe(val4, new float3(0f, 1f, 0f))), parameters, math.length(val4));
		}
	}
}
