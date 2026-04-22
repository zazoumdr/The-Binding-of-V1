using BeamHitInterpolation;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using UnityEngine;

internal static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		BloodsplatterManager.CreateBloodstain_000002B5_0024BurstDirectCall.Initialize();
		ColliderUtility.BurstClosestPoint_Int16_0000049A_0024BurstDirectCall.Initialize();
		ColliderUtility.BurstClosestPoint_Int32_0000049B_0024BurstDirectCall.Initialize();
		CameraData.Create_000017BF_0024BurstDirectCall.Initialize();
		CameraData.CalculateObliqueMatrix_000017C0_0024BurstDirectCall.Initialize();
		FrustumClipper.ClipQuadToCameraFrustum_000017C4_0024BurstDirectCall.Initialize();
		VirtualAudioFilter.UpdateVelocityBurst_0000223A_0024BurstDirectCall.Initialize();
		VirtualAudioFilter.AddOutputBurst_0000223F_0024BurstDirectCall.Initialize();
		VirtualAudioFilter.ProcessStereo_00002242_0024BurstDirectCall.Initialize();
		VirtualAudioManager.LoopOverListeners_00002261_0024BurstDirectCall.Initialize();
		Portal.UpdatePortalBurst_00002973_0024BurstDirectCall.Initialize();
		PortalRenderV2.RebuildMesh_000029FC_0024BurstDirectCall.Initialize();
		PortalRenderV2.BurstRenderData_000029FD_0024BurstDirectCall.Initialize();
		PortalRenderV2.SortPortals_000029FE_0024BurstDirectCall.Initialize();
		PortalRenderV2.ExtractFrustumPlanes_00002A00_0024BurstDirectCall.Initialize();
		PortalRenderV2.GetOnscreenPortalsBurst_00002A01_0024BurstDirectCall.Initialize();
		PortalRenderV2.CalculateCullingData_00002A03_0024BurstDirectCall.Initialize();
		PortalRenderV2.UpdateOcclusionBurst_00002A0B_0024BurstDirectCall.Initialize();
		PortalScene.CalculateMatrices_00002A24_0024BurstDirectCall.Initialize();
		PortalScene.Internal_FindCrossedPortals_00002A29_0024BurstDirectCall.Initialize();
		PortalScene.Internal_FindPortalsBetween_00002A2C_0024BurstDirectCall.Initialize();
		PortalScene.Internal_TraversePortalSequence_00002A2D_0024BurstDirectCall.Initialize();
		NativePortalExtensions.CalculateData_00002B15_0024BurstDirectCall.Initialize();
		NativePortalExtensions.Raycast_00002B16_0024BurstDirectCall.Initialize();
		PlaneShapeExtensions.GetClosestPoint_00002B34_0024BurstDirectCall.Initialize();
		TargetTracker.CountPermutations_00002B8F_0024BurstDirectCall.Initialize();
		BeamHitInterpolator.FindBestTimeAndMinDistSq_00002E0F_0024BurstDirectCall.Initialize();
		BeamHitInterpolator.DistanceSqPointSegment_00002E10_0024BurstDirectCall.Initialize();
		BeamHitInterpolator.ClosestPointOnSegment_00002E11_0024BurstDirectCall.Initialize();
		BeamHitInterpolator.CalculateSweptObb_00002E12_0024BurstDirectCall.Initialize();
	}
}
