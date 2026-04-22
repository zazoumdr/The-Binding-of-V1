using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public struct CalculateStartIndicesJob : IJob
{
	[ReadOnly]
	public NativeArray<int> counts;

	[WriteOnly]
	public NativeArray<int> startIndices;

	[WriteOnly]
	public NativeList<TargetIndexAndDistance> targetList;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < counts.Length; i++)
		{
			startIndices[i] = num;
			num += counts[i];
		}
		targetList.SetCapacity(num);
	}
}
