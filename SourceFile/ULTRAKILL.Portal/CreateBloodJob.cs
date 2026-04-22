using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ULTRAKILL.Portal;

[BurstCompile]
public struct CreateBloodJob : IJobFor
{
	[ReadOnly]
	public NativeArray<RaycastHit> hits;

	[ReadOnly]
	public NativeArray<IntersectionAndIndex> intersections;

	[ReadOnly]
	public NativeArray<BloodsplatterMetadata> shouldCreate;

	[WriteOnly]
	public ParallelWriter<BloodstainCreateCommand> queue;

	public void Execute(int index)
	{
		BloodsplatterMetadata bloodsplatterMetadata = shouldCreate[index];
		if (bloodsplatterMetadata.exists && hits[index].colliderInstanceID != 0 && !(intersections[index].intersection.distance < hits[index].distance))
		{
			queue.Enqueue(new BloodstainCreateCommand
			{
				hit = hits[index],
				halfChance = bloodsplatterMetadata.halfChance,
				splatterId = bloodsplatterMetadata.instanceId
			});
		}
	}
}
