using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public struct SortJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> startIndices;

	[ReadOnly]
	public NativeArray<int> count;

	[NativeDisableParallelForRestriction]
	public NativeArray<TargetIndexAndDistance> targetList;

	public DistanceComparer comparer;

	public void Execute(int index)
	{
		NativeSortExtension.Sort<TargetIndexAndDistance, DistanceComparer>(targetList.Slice(startIndices[index], count[index]), comparer);
	}
}
