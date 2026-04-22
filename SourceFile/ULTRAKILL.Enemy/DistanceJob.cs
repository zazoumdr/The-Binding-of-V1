using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public struct DistanceJob : IJobFor
{
	public int targetCount;

	[ReadOnly]
	public NativeArray<float3> visionOrigins;

	[ReadOnly]
	public NativeArray<VisionTypeFilter> visionFilters;

	[ReadOnly]
	public NativeArray<float3> targetPositions;

	[ReadOnly]
	public NativeArray<TargetType> targetTypes;

	public NativeArray<int> counts;

	[WriteOnly]
	public Writer output;

	public void Execute(int visionIndex)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		((Writer)(ref output)).BeginForEachIndex(visionIndex);
		float3 val = visionOrigins[visionIndex];
		for (int i = 0; i < targetCount; i++)
		{
			if (visionFilters[visionIndex].HasType(targetTypes[i]))
			{
				float distance = math.distancesq(val, targetPositions[i]);
				((Writer)(ref output)).Write<TargetIndexAndDistance>(new TargetIndexAndDistance
				{
					index = i,
					distance = distance
				});
				counts[visionIndex]++;
			}
		}
		((Writer)(ref output)).EndForEachIndex();
	}
}
