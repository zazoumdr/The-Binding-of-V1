using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public struct OutputJob : IJobFor
{
	public Reader output;

	[WriteOnly]
	public NativeList<TargetIndexAndDistance> array;

	public void Execute(int index)
	{
		((Reader)(ref output)).BeginForEachIndex(index);
		int remainingItemCount = ((Reader)(ref output)).RemainingItemCount;
		for (int i = 0; i < remainingItemCount; i++)
		{
			array.AddNoResize(((Reader)(ref output)).Read<TargetIndexAndDistance>());
		}
		((Reader)(ref output)).EndForEachIndex();
	}
}
