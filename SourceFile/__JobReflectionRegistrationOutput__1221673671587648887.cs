using System;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__1221673671587648887
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForExtensions.EarlyJobInit<GenerateBloodMeshJob>();
			IJobParallelForExtensions.EarlyJobInit<SeaBodies.VibrateSeaBodiesJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<StainVoxelManager.UpdateMatrixJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<PortalManagerV2.CopyTransformsJob>();
			IJobParallelForTransformExtensions.EarlyJobInit<PortalManagerV2.ApplyTransformsJob>();
			IJobForExtensions.EarlyJobInit<ParticleIntersectionJob>();
			IJobForExtensions.EarlyJobInit<CreateBloodJob>();
			IJobForExtensions.EarlyJobInit<PortalVisionJob>();
			IJobForExtensions.EarlyJobInit<CopyDefaultDataJob>();
			IJobForExtensions.EarlyJobInit<CalculateTargetDataJob>();
			IJobForExtensions.EarlyJobInit<DistanceJob>();
			IJobForExtensions.EarlyJobInit<ObstructionJob>();
			IJobExtensions.EarlyJobInit<CalculateStartIndicesJob>();
			IJobForExtensions.EarlyJobInit<OutputJob>();
			IJobParallelForExtensions.EarlyJobInit<SortJob>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
