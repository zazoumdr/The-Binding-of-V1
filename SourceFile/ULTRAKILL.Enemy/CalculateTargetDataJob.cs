using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public struct CalculateTargetDataJob : IJobFor
{
	public int targetCount;

	[ReadOnly]
	public NativeArray<float4x4> matrices;

	public TargetDataArrays arrays;

	public void Execute(int index)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		int index2 = index / targetCount;
		float4x4 val = matrices[index2];
		float3 value = math.transform(val, arrays.positions[index]);
		float3 value2 = math.transform(val, arrays.headPositions[index]);
		arrays.positions[index] = value;
		arrays.headPositions[index] = value2;
		arrays.velocities[index] = math.rotate(val, arrays.velocities[index]);
	}
}
