using System;
using System.Collections.Generic;
using Interop;
using PrivateAPIBridge;
using ULTRAKILL.Portal.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace ULTRAKILL.Portal;

public class PortalParticles : IDisposable
{
	public PortalScene scene;

	public readonly List<PortalAwareParticleSystem> systems = new List<PortalAwareParticleSystem>();

	public NativeList<RaycastCommand> commands;

	public NativeList<IntersectionAndIndex> intersections;

	public NativeList<RaycastHit> hits;

	public NativeList<JobHandle> commandJobs;

	public NativeList<BloodsplatterMetadata> bloodMeta;

	public JobHandle createBloodHandle;

	private bool disposed;

	public void Initialize(PortalScene scene)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		this.scene = scene;
		systems.Clear();
		commands = new NativeList<RaycastCommand>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		intersections = new NativeList<IntersectionAndIndex>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		hits = new NativeList<RaycastHit>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		commandJobs = new NativeList<JobHandle>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		bloodMeta = new NativeList<BloodsplatterMetadata>(AllocatorHandle.op_Implicit(Allocator.Persistent));
	}

	public unsafe void ScheduleJobs(ref NativeList<JobHandle> portalDependent)
	{
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Invalid comparison between Unknown and I4
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Invalid comparison between Unknown and I4
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		CompleteJobs();
		if (MonoSingleton<BloodsplatterManager>.TryGetInstance(out BloodsplatterManager instance) && instance.stainCreateQueue.IsCreated && !(StockMapInfo.Instance == null) && scene != null && !disposed)
		{
			ref NativePortalScene nativeScene = ref scene.nativeScene;
			int count = systems.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				PortalAwareParticleSystem portalAwareParticleSystem = systems[i];
				portalAwareParticleSystem.responseHandle.Complete();
				int particleCount = portalAwareParticleSystem._system.particleCount;
				((ParticleSystem)(void*)ObjectExtensions.GetCachedPtr((UnityEngine.Object)(object)portalAwareParticleSystem._system)).m_UpdateFence.Complete();
				particleCount = portalAwareParticleSystem._system.particleCount;
				num += particleCount;
			}
			commands.Resize(num, NativeArrayOptions.ClearMemory);
			intersections.Resize(num, NativeArrayOptions.ClearMemory);
			hits.Resize(num, NativeArrayOptions.ClearMemory);
			bloodMeta.Resize(num, NativeArrayOptions.ClearMemory);
			NativeArray<RaycastCommand> nativeArray = commands.AsArray();
			NativeArray<IntersectionAndIndex> nativeArray2 = intersections.AsArray();
			NativeArray<RaycastHit> nativeArray3 = hits.AsArray();
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
			bool continuousGibCollisions = StockMapInfo.Instance.continuousGibCollisions;
			if (continuousGibCollisions)
			{
				layerMask = (int)layerMask | 0x10;
			}
			QueryParameters parameters = new QueryParameters(layerMask, hitMultipleFaces: false, (!continuousGibCollisions) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide, hitBackfaces: true);
			int num2 = 0;
			ParticleCommandJob particleCommandJob = new ParticleCommandJob
			{
				parameters = parameters
			};
			ParticleResponseJob particleResponseJob = new ParticleResponseJob
			{
				scene = nativeScene,
				intersections = nativeArray2,
				hits = nativeArray3
			};
			Span<BloodsplatterMetadata> span = bloodMeta.AsArray().AsSpan();
			MainModule main;
			for (int j = 0; j < count; j++)
			{
				PortalAwareParticleSystem portalAwareParticleSystem2 = systems[j];
				portalAwareParticleSystem2._system.AllocateCustomDataAttribute((ParticleSystemCustomData)0);
				int particleCount2 = portalAwareParticleSystem2._system.particleCount;
				NativeSlice<RaycastCommand> raycasts = nativeArray.Slice(num2, particleCount2);
				bool flag = portalAwareParticleSystem2.blood;
				span.Slice(num2, particleCount2).Fill(new BloodsplatterMetadata
				{
					exists = flag,
					halfChance = (flag && portalAwareParticleSystem2.blood.halfChance),
					instanceId = (flag ? portalAwareParticleSystem2.blood.GetInstanceID() : (-1))
				});
				main = portalAwareParticleSystem2._system.main;
				bool flag2 = (int)((MainModule)(ref main)).simulationSpace == 1;
				particleCommandJob.toWorld = (flag2 ? float4x4.op_Implicit(Matrix4x4.identity) : portalAwareParticleSystem2.toWorld);
				particleCommandJob.raycasts = raycasts;
				JobHandle jobHandle = IParticleSystemJobExtensions.Schedule<ParticleCommandJob>(particleCommandJob, portalAwareParticleSystem2._system, default(JobHandle));
				commandJobs.Add(ref jobHandle);
				num2 += particleCount2;
			}
			JobHandle.ScheduleBatchedJobs();
			JobHandle jobHandle2 = JobHandle.CombineDependencies(commandJobs.AsArray());
			portalDependent.Add(ref jobHandle2);
			JobHandle jobHandle3 = RaycastCommand.ScheduleBatch(commands.AsArray(), hits.AsArray(), 64, 1, jobHandle2);
			ParticleIntersectionJob jobData = new ParticleIntersectionJob
			{
				scene = nativeScene,
				intersections = nativeArray2,
				rays = nativeArray
			};
			JobHandle job = IJobForExtensions.ScheduleParallelByRef(ref jobData, commands.Length, 64, jobHandle2);
			portalDependent.Add(ref job);
			CreateBloodJob jobData2 = new CreateBloodJob
			{
				queue = instance.stainCreateQueue.AsParallelWriter(),
				hits = nativeArray3,
				intersections = nativeArray2,
				shouldCreate = bloodMeta.AsArray()
			};
			createBloodHandle = IJobForExtensions.ScheduleParallelByRef(ref jobData2, commands.Length, 128, jobHandle3);
			JobHandle jobHandle4 = JobHandle.CombineDependencies(job, jobHandle3);
			JobHandle.ScheduleBatchedJobs();
			num2 = 0;
			for (int k = 0; k < count; k++)
			{
				PortalAwareParticleSystem portalAwareParticleSystem3 = systems[k];
				int particleCount3 = portalAwareParticleSystem3._system.particleCount;
				NativeSlice<IntersectionAndIndex> nativeSlice = nativeArray2.Slice(num2, particleCount3);
				NativeSlice<RaycastHit> nativeSlice2 = nativeArray3.Slice(num2, particleCount3);
				main = portalAwareParticleSystem3._system.main;
				bool flag3 = (int)((MainModule)(ref main)).simulationSpace == 1;
				particleResponseJob.toWorld = (flag3 ? float4x4.op_Implicit(Matrix4x4.identity) : portalAwareParticleSystem3.toWorld);
				particleResponseJob.toLocal = (flag3 ? float4x4.op_Implicit(Matrix4x4.identity) : portalAwareParticleSystem3.toLocal);
				particleResponseJob.intersections = nativeSlice;
				particleResponseJob.hits = nativeSlice2;
				particleResponseJob.trails = &((ParticleSystemParticles)((ParticleSystem)(void*)ObjectExtensions.GetCachedPtr((UnityEngine.Object)(object)portalAwareParticleSystem3._system)).m_Particles[0].Value).trails;
				portalAwareParticleSystem3.responseHandle = IParticleSystemJobExtensions.Schedule<ParticleResponseJob>(particleResponseJob, portalAwareParticleSystem3._system, jobHandle4);
				portalDependent.Add(ref portalAwareParticleSystem3.responseHandle);
				num2 += particleCount3;
			}
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public void CompleteJobs()
	{
		createBloodHandle.Complete();
		if (commandJobs.IsCreated)
		{
			JobHandle.CompleteAll(commandJobs.AsArray());
			commandJobs.Clear();
		}
	}

	public void Register(PortalAwareParticleSystem system)
	{
		systems.Add(system);
	}

	public void Deregister(PortalAwareParticleSystem system)
	{
		systems.Remove(system);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			CompleteJobs();
			if (commands.IsCreated)
			{
				commands.Dispose();
			}
			if (intersections.IsCreated)
			{
				intersections.Dispose();
			}
			if (hits.IsCreated)
			{
				hits.Dispose();
			}
			if (commandJobs.IsCreated)
			{
				commandJobs.Dispose();
			}
			if (bloodMeta.IsCreated)
			{
				bloodMeta.Dispose();
			}
		}
	}
}
