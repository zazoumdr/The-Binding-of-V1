using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[BurstCompile]
internal struct CommandJob : IJobParticleSystemParallelFor
{
	public float4x4 transform;

	public NativeArray<RaycastCommand> raycasts;

	[ReadOnly]
	public NativeArray<RaycastHit> lastFrameHits;

	public QueryParameters parameters;

	public float deltaTime;

	public bool worldSpace;

	public Vector3 center;

	public void Execute(ParticleSystemJobData jobData, int i)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystemNativeArray4 customData = ((ParticleSystemJobData)(ref jobData)).customData1;
		Vector4 vector = ((ParticleSystemNativeArray4)(ref customData))[i];
		if (worldSpace && vector == Vector4.zero)
		{
			vector = center;
		}
		int index = (int)vector.w;
		Vector3 point = lastFrameHits[index].point;
		if (point.x != 0f && point.y != 0f && point.z != 0f)
		{
			NativeArray<float> aliveTimePercent = ((ParticleSystemJobData)(ref jobData)).aliveTimePercent;
			aliveTimePercent[i] = 100f;
		}
		float4 val = math.mul(transform, new float4(vector.x, vector.y, vector.z, 1f));
		float3 xyz = ((float4)(ref val)).xyz;
		float4x4 val2 = transform;
		ParticleSystemNativeArray3 positions = ((ParticleSystemJobData)(ref jobData)).positions;
		val = math.mul(val2, new float4(float3.op_Implicit(((ParticleSystemNativeArray3)(ref positions))[i]), 1f));
		float3 val3 = ((float4)(ref val)).xyz - xyz;
		float distance = math.length(val3);
		raycasts[i] = new RaycastCommand(float3.op_Implicit(xyz), float3.op_Implicit(math.normalizesafe(val3, default(float3))), parameters, distance);
		positions = ((ParticleSystemJobData)(ref jobData)).positions;
		((ParticleSystemNativeArray4)(ref customData))[i] = float4.op_Implicit(new float4(float3.op_Implicit(((ParticleSystemNativeArray3)(ref positions))[i]), (float)i));
	}
}
