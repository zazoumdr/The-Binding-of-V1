using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ThreadedParticleCollision : MonoBehaviour
{
	public ParticleSystem particles;

	public Bloodsplatter bloodsplatter;

	public NativeArray<RaycastCommand> raycasts;

	public NativeArray<RaycastHit> results;

	private CommandJob commandJob;

	private JobHandle handle;

	private List<Vector4> customData = new List<Vector4>();

	private BloodsplatterManager bsm;

	private static Matrix4x4 identityMatrix = Matrix4x4.identity;

	public event Action<NativeSlice<RaycastHit>> collisionEvent;

	private void Awake()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		if (StockMapInfo.Instance.continuousGibCollisions)
		{
			layerMask = (int)layerMask | 0x10;
		}
		QueryTriggerInteraction hitTriggers = ((!StockMapInfo.Instance.continuousGibCollisions) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
		commandJob.parameters = new QueryParameters(layerMask, hitMultipleFaces: false, hitTriggers);
		MainModule main = particles.main;
		results = new NativeArray<RaycastHit>(((MainModule)(ref main)).maxParticles, Allocator.Persistent);
		main = particles.main;
		raycasts = new NativeArray<RaycastCommand>(((MainModule)(ref main)).maxParticles, Allocator.Persistent);
		commandJob.raycasts = raycasts;
		commandJob.lastFrameHits = results;
	}

	private void OnEnable()
	{
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
	}

	private void OnDisable()
	{
	}

	private void RegisterPortalData()
	{
	}

	private unsafe void Step(float dt)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (!handle.IsCompleted)
		{
			return;
		}
		handle.Complete();
		if (results.IsCreated)
		{
			int particleCount = particles.particleCount;
			RaycastHit* unsafeBufferPointerWithoutChecks = (RaycastHit*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results);
			for (int i = 0; i < particleCount; i++)
			{
				RaycastHit hit = unsafeBufferPointerWithoutChecks[i];
				if (hit.colliderInstanceID != 0)
				{
					bloodsplatter.CreateBloodstain(in hit, bsm);
				}
			}
		}
		Transform transform = ((Component)(object)particles).transform;
		if (transform.hasChanged)
		{
			transform.hasChanged = false;
			MainModule main = particles.main;
			if ((int)((MainModule)(ref main)).simulationSpace == 0)
			{
				commandJob.transform = float4x4.op_Implicit(transform.localToWorldMatrix);
				commandJob.worldSpace = false;
			}
			else
			{
				commandJob.transform = float4x4.op_Implicit(identityMatrix);
				commandJob.worldSpace = true;
				commandJob.center = transform.position;
			}
		}
	}

	private void OnDestroy()
	{
		raycasts.Dispose(handle);
		results.Dispose(handle);
	}
}
