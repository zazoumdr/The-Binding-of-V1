using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ULTRAKILL.Enemy;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.LowLevel;

namespace ULTRAKILL.Portal;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[RequireComponent(typeof(PortalRenderV2))]
[DefaultExecutionOrder(999)]
public class PortalManagerV2 : MonoSingleton<PortalManagerV2>
{
	public delegate void TravelCallback(in PortalTravelDetails details);

	public static class PrintDefaultPlayerLoop
	{
		[RuntimeInitializeOnLoadMethod]
		private static void AppStart()
		{
			StringBuilder stringBuilder = new StringBuilder();
			RecursivePlayerLoopPrint(PlayerLoop.GetDefaultPlayerLoop(), stringBuilder, 0);
			Debug.Log(stringBuilder.ToString());
		}

		private static void RecursivePlayerLoopPrint(PlayerLoopSystem playerLoopSystem, StringBuilder sb, int depth)
		{
			if (depth == 0)
			{
				sb.AppendLine("ROOT NODE");
			}
			else if (playerLoopSystem.type != null)
			{
				for (int i = 0; i < depth; i++)
				{
					sb.Append("\t");
				}
				sb.AppendLine(playerLoopSystem.type.Name);
			}
			if (playerLoopSystem.subSystemList != null)
			{
				depth++;
				PlayerLoopSystem[] subSystemList = playerLoopSystem.subSystemList;
				for (int j = 0; j < subSystemList.Length; j++)
				{
					RecursivePlayerLoopPrint(subSystemList[j], sb, depth);
				}
				depth--;
			}
		}
	}

	[BurstCompile]
	private struct CopyTransformsJob : IJobParallelForTransform
	{
		[WriteOnly]
		public NativeArray<TransformData> TransformData;

		public void Execute(int index, TransformAccess transform)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (!transform.isValid)
			{
				TransformData[index] = new TransformData
				{
					Position = default(float3),
					Rotation = quaternion.identity,
					Valid = false
				};
			}
			else
			{
				transform.GetPositionAndRotation(out var position, out var rotation);
				TransformData[index] = new TransformData
				{
					Position = float3.op_Implicit(position),
					Rotation = quaternion.op_Implicit(rotation),
					Valid = true
				};
			}
		}
	}

	[BurstCompile]
	private struct ApplyTransformsJob : IJobParallelForTransform
	{
		[ReadOnly]
		public NativeArray<TransformData> TransformData;

		public void Execute(int index, TransformAccess transform)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (transform.isValid)
			{
				TransformData transformData = TransformData[index];
				if (transformData.Valid && transform.isValid)
				{
					transform.SetLocalPositionAndRotation(float3.op_Implicit(transformData.Position), quaternion.op_Implicit(transformData.Rotation));
				}
			}
		}
	}

	private struct TransformData
	{
		public float3 Position;

		public quaternion Rotation;

		public bool Valid;
	}

	public int maxRecursions = 3;

	public Camera mainCamera;

	public Camera portalCamera;

	[HideInInspector]
	public PortalRenderV2 render;

	public Action<IPortalTraveller, PortalTravelDetails> OnTargetTravelled;

	private LayerMask empty_lm;

	private TransformAccessArray portalTransformAccess;

	private List<IPortalTraveller> travellers = new List<IPortalTraveller>();

	private Dictionary<int, Vector3> lastTravellerPositions = new Dictionary<int, Vector3>();

	private List<Portal> portalComponents = new List<Portal>();

	private readonly Dictionary<Transform, int> parTransformKeys = new Dictionary<Transform, int>();

	private readonly Dictionary<Transform, int> parTargetTransformKeys = new Dictionary<Transform, int>();

	private TransformAccessArray parTransformAccess;

	private TransformAccessArray parTargetTransformAccess;

	private Transform[] parTransforms;

	private Transform[] parTargetTransforms;

	private IPortalTraveller playerTraveller;

	public List<PortalAwareParticleSystem> systems = new List<PortalAwareParticleSystem>();

	public NativeList<JobHandle> jobHandles;

	public NativeList<RaycastCommand> commands;

	private List<(IPortalTraveller traveller, PortalTravelDetails details)> travellerTraversals = new List<(IPortalTraveller, PortalTravelDetails)>();

	private bool initialized;

	public PortalScene Scene { get; private set; }

	public TargetTracker TargetTracker { get; private set; }

	public PortalNavigation Navigation { get; private set; }

	public PortalParticles Particles { get; private set; }

	public int portalCount => portalComponents.Count;

	public event Action<Camera> RenderFrom;

	public void Reset()
	{
		lastTravellerPositions.Clear();
	}

	private void OnDisable()
	{
		JobHandle.CompleteAll(jobHandles.AsArray());
		jobHandles.Clear();
		parTransformAccess.Dispose();
		parTargetTransformAccess.Dispose();
		Debug.Log("Disposing");
		TargetTracker.Dispose();
		Scene.Dispose();
		Particles.Dispose();
		PostProcessV2_Handler postProcessV2_Handler = MonoSingleton<PostProcessV2_Handler>.Instance;
		if (postProcessV2_Handler != null)
		{
			postProcessV2_Handler.onReinitialize = (Action<bool>)Delegate.Remove(postProcessV2_Handler.onReinitialize, new Action<bool>(Reinitialize));
		}
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
		jobHandles.Dispose();
	}

	public void AddTraveller(IPortalTraveller traveller, CancellationToken token)
	{
		travellers.Add(traveller);
		lastTravellerPositions[traveller.id] = traveller.travellerPosition;
		token.Register(delegate
		{
			RemoveTraveller(traveller);
		});
	}

	public void AddPlayer(IPortalTraveller traveller)
	{
		playerTraveller = traveller;
	}

	public void RemoveTraveller(IPortalTraveller traveller)
	{
		travellers.Remove(traveller);
	}

	public void UpdateTraveller(IPortalTraveller traveller)
	{
		lastTravellerPositions[traveller.id] = traveller.travellerPosition;
	}

	public void AddTransformAccessPair(Transform clone, Transform target)
	{
		if (!(clone == null) && !(target == null) && base.enabled)
		{
			int length = parTransformAccess.length;
			parTransformAccess.Add(clone);
			parTransforms[length] = clone;
			parTransformKeys[clone] = length;
			parTargetTransformAccess.Add(target);
			parTargetTransforms[length] = clone;
			parTargetTransformKeys[clone] = length;
		}
	}

	public void RemoveTransformAccessPair(Transform clone)
	{
		if (base.enabled)
		{
			if (parTransformKeys.TryGetValue(clone, out var value))
			{
				Transform transform = parTransforms[parTransformAccess.length - 1];
				parTransforms[value] = transform;
				parTransforms[parTransformAccess.length - 1] = null;
				parTransformKeys[transform] = value;
				parTransformAccess.RemoveAtSwapBack(value);
				parTransformKeys.Remove(clone);
			}
			if (parTargetTransformKeys.TryGetValue(clone, out var value2))
			{
				Transform transform2 = parTargetTransforms[parTargetTransformAccess.length - 1];
				parTargetTransforms[value2] = transform2;
				parTargetTransforms[parTargetTransformAccess.length - 1] = null;
				parTargetTransformKeys[transform2] = value2;
				parTargetTransformAccess.RemoveAtSwapBack(value2);
				parTargetTransformKeys.Remove(clone);
			}
		}
	}

	private void TraverseAndCallBack(in IPortalTraveller traveller, in PortalTravelDetails details)
	{
		if (traveller is UnityEngine.Object obj && !obj)
		{
			Debug.LogWarning("Travel called on destroyed traveller");
			return;
		}
		if (details.blocked)
		{
			traveller.OnTeleportBlocked(details);
			return;
		}
		bool? flag = traveller.OnTravel(details);
		if (flag.HasValue)
		{
			if (flag.Value)
			{
				TravellerCallback(in traveller, in details);
			}
			else
			{
				traveller.OnTeleportBlocked(details);
			}
		}
	}

	public void TravellerCallback(in IPortalTraveller traveller, in PortalTravelDetails details)
	{
		PortalHandleSequence portalSequence = details.portalSequence;
		for (int i = 0; i < portalSequence.Count; i++)
		{
			PortalHandle handle = portalSequence[i];
			PortalSide side = handle.side;
			Scene.GetPortalObject(handle).onTravel(side)?.Invoke(traveller, details);
		}
		OnTargetTravelled?.Invoke(traveller, details);
	}

	private void Update()
	{
		JobHandle.CompleteAll(jobHandles.AsArray());
		jobHandles.Clear();
		if (portalComponents.Count > 0)
		{
			PlayerTracker playerTracker = MonoSingleton<PlayerTracker>.Instance;
			if ((bool)playerTracker && playerTracker.playerType == PlayerType.Platformer)
			{
				playerTracker.ChangeToFPS();
				MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.clash-mode");
			}
		}
		TargetTracker.outputVisionHandle.Complete();
		TargetTracker.calculateTargetDataHandle.Complete();
		if (MonoSingleton<VirtualAudioManager>.TryGetInstance(out VirtualAudioManager virtualAudioManager))
		{
			if (portalComponents.Count == 0)
			{
				virtualAudioManager.enabled = false;
			}
			else
			{
				virtualAudioManager.enabled = true;
			}
		}
		Scene.Sync(portalComponents);
		JobHandle.ScheduleBatchedJobs();
		TargetTracker.UpdateData();
		Navigation.Sync(Scene);
		portalCamera.cullingMask = mainCamera.cullingMask;
		portalCamera.clearFlags = mainCamera.clearFlags;
	}

	private void OnParticleUpdateJobScheduled()
	{
		Particles.ScheduleJobs(ref jobHandles);
	}

	private void FixedUpdate()
	{
		foreach (Portal portalComponent in portalComponents)
		{
			_ = portalComponent;
		}
		JobHandle.CompleteAll(jobHandles.AsArray());
		jobHandles.Clear();
		Scene.nativeScene.Recalculate(portalComponents);
		CheckTravellerTraversals(travellerTraversals);
		Span<(IPortalTraveller, PortalTravelDetails)> span = CollectionsMarshal.AsSpan(travellerTraversals);
		for (int i = 0; i < span.Length; i++)
		{
			ref(IPortalTraveller, PortalTravelDetails) reference = ref span[i];
			TraverseAndCallBack(in reference.Item1, in reference.Item2);
		}
		if (CheckPlayerTraversals(out (IPortalTraveller, PortalTravelDetails) result))
		{
			TraverseAndCallBack(in result.Item1, in result.Item2);
		}
		lastTravellerPositions.Clear();
		lastTravellerPositions[playerTraveller.id] = playerTraveller.travellerPosition;
		foreach (IPortalTraveller traveller in travellers)
		{
			lastTravellerPositions.Add(traveller.id, traveller.travellerPosition);
		}
	}

	private void OnEnable()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		ushort num = ushort.MaxValue;
		parTransformAccess = new TransformAccessArray(num);
		parTransforms = new Transform[num];
		parTargetTransformAccess = new TransformAccessArray(num);
		parTargetTransforms = new Transform[num];
		jobHandles = new NativeList<JobHandle>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		Scene = new PortalScene();
		TargetTracker = new TargetTracker();
		Navigation = new PortalNavigation();
		Particles = new PortalParticles();
		Particles.Initialize(Scene);
		TargetTracker.SetScene(Scene);
		render = GetComponent<PortalRenderV2>();
		PostProcessV2_Handler? postProcessV2_Handler = MonoSingleton<PostProcessV2_Handler>.Instance;
		postProcessV2_Handler.onReinitialize = (Action<bool>)Delegate.Combine(postProcessV2_Handler.onReinitialize, new Action<bool>(Reinitialize));
		empty_lm = default(LayerMask);
		Update();
	}

	private void LateUpdate()
	{
		JobHandle.CompleteAll(jobHandles.AsArray());
		jobHandles.Clear();
		Navigation.RemoveQueuedLinks(Scene);
		if (playerTraveller is NewMovement newMovement && Time.frameCount != newMovement.lastTraversalFrame && CheckPlayerTraversals(out (IPortalTraveller, PortalTravelDetails) result))
		{
			TraverseAndCallBack(in result.Item1, in result.Item2);
			lastTravellerPositions[playerTraveller.id] = playerTraveller.travellerPosition;
		}
		UpdatePortalAwareRenderers();
		if (initialized)
		{
			render.Setup(Scene, mainCamera, portalCamera);
		}
	}

	private void UpdatePortalAwareRenderers()
	{
		if (parTransformAccess.length > 0 && parTargetTransformAccess.length > 0)
		{
			if (parTransformAccess.length != parTargetTransformAccess.length)
			{
				Debug.LogError($"Transform access array lengths are not the same! {parTransformAccess.length} != {parTargetTransformAccess.length}");
				return;
			}
			NativeArray<TransformData> transformData = new NativeArray<TransformData>(parTransformAccess.length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			CopyTransformsJob jobData = new CopyTransformsJob
			{
				TransformData = transformData
			};
			JobHandle dependsOn = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, parTargetTransformAccess, 16);
			ApplyTransformsJob jobData2 = new ApplyTransformsJob
			{
				TransformData = transformData
			};
			IJobParallelForTransformExtensions.ScheduleByRef(ref jobData2, parTransformAccess, dependsOn).Complete();
			transformData.Dispose();
		}
	}

	private void CheckTravellerTraversals(List<(IPortalTraveller, PortalTravelDetails)> traversalCallbacks)
	{
		if (!Scene.nativeScene.valid || Scene.nativeScene.portals.Length == 0)
		{
			return;
		}
		traversalCallbacks.Clear();
		int count = travellers.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				IPortalTraveller portalTraveller = travellers[i];
				if (!lastTravellerPositions.TryGetValue(portalTraveller.id, out var value) || !Scene.FindPortalBetween(value, portalTraveller.travellerPosition, out var hitPortal, out var intersection, out var _))
				{
					continue;
				}
				PortalTravellerFlags portalTravellerFlags = portalTraveller.travellerType.ToFlags();
				Portal portalObject = Scene.GetPortalObject(hitPortal);
				PortalTravellerFlags travelFlags = portalObject.GetTravelFlags(hitPortal.side);
				bool flag = travelFlags.HasFlag(portalTravellerFlags);
				PortalHandleSequence travelHandles = new PortalHandleSequence(hitPortal);
				if (!flag)
				{
					goto IL_01d4;
				}
				Matrix4x4 travelMatrix = Scene.GetTravelMatrix(hitPortal);
				Vector3 vector = travelMatrix.MultiplyPoint3x4(intersection);
				Vector3 direction = travelMatrix.MultiplyPoint3x4(portalTraveller.travellerPosition) - vector;
				PortalPhysicsV2.ProjectThroughPortals(vector, direction, empty_lm, out var _, out var _, out var traversals);
				for (int j = 0; j < traversals.Length; j++)
				{
					PortalHandle portalHandle = traversals[j].portalHandle;
					if (!Scene.GetPortalObject(portalHandle).GetTravelFlags(traversals[j].portalHandle.side).HasFlag(portalTravellerFlags))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					goto IL_01d4;
				}
				if (traversals.Length != 0)
				{
					travelHandles = PortalHandleSequence.Prepend(hitPortal, traversals);
					travelMatrix = Scene.GetTravelMatrix(in travelHandles);
				}
				PortalTravelDetails item = PortalTravelDetails.WithInteresction(travelHandles, traversals, travelMatrix, intersection);
				traversalCallbacks.Add((portalTraveller, item));
				goto end_IL_0044;
				IL_01d4:
				if (!flag && !portalObject.passThroughNonTraversals)
				{
					PortalTravelDetails item2 = PortalTravelDetails.WithInteresction(travelHandles, Array.Empty<PortalTraversalV2>(), Matrix4x4.identity, intersection);
					item2.blocked = true;
					traversalCallbacks.Add((portalTraveller, item2));
				}
				end_IL_0044:;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private bool CheckPlayerTraversals(out (IPortalTraveller traveller, PortalTravelDetails details) result)
	{
		IPortalTraveller portalTraveller = playerTraveller;
		if (portalTraveller != null)
		{
			if (!lastTravellerPositions.TryGetValue(portalTraveller.id, out var value))
			{
				result = default((IPortalTraveller, PortalTravelDetails));
				result.details.blocked = false;
				return false;
			}
			if (Scene.FindCrossedPortal(value, portalTraveller.travellerPosition, out var handle, out var intersection))
			{
				PortalTravellerFlags portalTravellerFlags = portalTraveller.travellerType.ToFlags();
				Portal portalObject = Scene.GetPortalObject(handle);
				PortalTravellerFlags travelFlags = portalObject.GetTravelFlags(handle.side);
				bool flag = travelFlags.HasFlag(portalTravellerFlags);
				PortalHandleSequence travelHandles = new PortalHandleSequence(handle);
				if (flag)
				{
					Matrix4x4 travelMatrix = Scene.GetTravelMatrix(handle);
					Vector3 vector = travelMatrix.MultiplyPoint3x4(intersection);
					Vector3 direction = travelMatrix.MultiplyPoint3x4(portalTraveller.travellerPosition) - vector;
					PortalPhysicsV2.ProjectThroughPortals(vector, direction, empty_lm, out var _, out var _, out var traversals);
					for (int i = 0; i < traversals.Length; i++)
					{
						PortalHandle portalHandle = traversals[i].portalHandle;
						if (!Scene.GetPortalObject(portalHandle).GetTravelFlags(traversals[i].portalHandle.side).HasFlag(portalTravellerFlags))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						if (traversals.Length != 0)
						{
							travelHandles = PortalHandleSequence.Prepend(handle, traversals);
							travelMatrix = Scene.GetTravelMatrix(in travelHandles);
						}
						result.traveller = portalTraveller;
						result.details = PortalTravelDetails.WithInteresction(travelHandles, traversals, travelMatrix, intersection);
						return true;
					}
				}
				if (!flag && !portalObject.passThroughNonTraversals)
				{
					result.traveller = portalTraveller;
					result.details = PortalTravelDetails.WithInteresction(travelHandles, Array.Empty<PortalTraversalV2>(), Matrix4x4.identity, intersection);
					result.details.blocked = true;
					return true;
				}
			}
		}
		result = default((IPortalTraveller, PortalTravelDetails));
		return false;
	}

	private void OnPreRenderCallback(Camera cam)
	{
		PostProcessV2_Handler postProcessV2_Handler = MonoSingleton<PostProcessV2_Handler>.Instance;
		if (postProcessV2_Handler == null)
		{
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
			return;
		}
		if (cam == postProcessV2_Handler.mainCam)
		{
			if (MonoSingleton<PortalManagerV2>.Instance == null)
			{
				Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
				return;
			}
			render.Render(cam);
			Shader.SetGlobalVector("_PortalClipPlane", new Vector4(0f, 0f, 0f, 0f));
			postProcessV2_Handler.RenderSkyboxes();
		}
		this.RenderFrom?.Invoke(cam);
	}

	private void Reinitialize(bool resize)
	{
		initialized = true;
	}

	internal void InitCam()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
	}

	public void AddPortal(Portal portal)
	{
		if (!portalComponents.Contains(portal))
		{
			portalComponents.Add(portal);
		}
	}

	internal void SetPortalOcclusion(bool enabled)
	{
		render.SetPortalOcclusion(enabled);
	}
}
