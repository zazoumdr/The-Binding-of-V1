using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Enemy;

[BurstCompile]
public class TargetTracker : IDisposable
{
	internal delegate int CountPermutations_00002B8F_0024PostfixBurstDelegate(int count, int depth);

	internal static class CountPermutations_00002B8F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CountPermutations_00002B8F_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static CountPermutations_00002B8F_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static int Invoke(int count, int depth)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, int, int>)functionPointer)(count, depth);
				}
			}
			return CountPermutations_0024BurstManaged(count, depth);
		}
	}

	private PortalScene scene;

	public NativeArray<float> distances;

	public NativeArray<bool> valid;

	private TargetDataArrays defaults;

	private TargetDataArrays arrays;

	public readonly List<ITarget> targets = new List<ITarget>();

	public readonly List<ITarget> newTargets = new List<ITarget>();

	public readonly List<ITarget> removalTargets = new List<ITarget>();

	public List<Vision> visions = new List<Vision>();

	private CalculateTargetDataJob calculateTargetDataJob;

	public JobHandle calculateTargetDataHandle;

	public JobHandle outputVisionHandle;

	private NativeList<TargetIndexAndDistance> targetsList;

	private NativeArray<int> startIndices;

	private NativeArray<int> counts;

	private NativeStream targetStream;

	private NativeArray<float3> visionOrigins;

	private NativeArray<VisionTypeFilter> visionFilters;

	public int targetCount { get; private set; }

	public void SetScene(PortalScene scene)
	{
		this.scene = scene;
	}

	public TargetDataRef GetTargetDataRef(Span<float4x4> matricesSpan, Span<PortalHandleSequence> sequenceSpan, int index)
	{
		int elementOffset = index / targetCount;
		int index2 = index % targetCount;
		return new TargetDataRef(scene, targets[index2], MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(matricesSpan), elementOffset), 1), MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(sequenceSpan), elementOffset), 1), MemoryMarshal.CreateSpan(ref arrays, index));
	}

	private Matrix4x4 GetTravelMatrix(PortalHandleSequence sequence)
	{
		if (scene == null)
		{
			return Matrix4x4.identity;
		}
		return scene.GetTravelMatrix(in sequence);
	}

	[BurstCompile]
	private static int CountPermutations(int count, int depth)
	{
		return CountPermutations_00002B8F_0024BurstDirectCall.Invoke(count, depth);
	}

	public int IndexFromSequence(NativeList<NativePortal> portals, PortalHandleSequence sequence)
	{
		int count = sequence.Count;
		Span<int> span = stackalloc int[sequence.Count];
		int length = portals.Length;
		for (int i = 0; i < length; i++)
		{
			PortalHandle handle = portals[i].handle;
			for (int j = 0; j < sequence.Count; j++)
			{
				if (sequence[j] == handle)
				{
					span[j] = i;
				}
			}
		}
		int num = CountPermutations(length, count - 1);
		for (int k = 0; k < count; k++)
		{
			int num2 = span[k];
			int num3 = CountPermutations(length, count - k);
			num += num3 * num2;
		}
		return num;
	}

	public TargetData CalculateData(TargetHandle handle)
	{
		Matrix4x4 travelMatrix = GetTravelMatrix(handle.portals);
		TargetData data = TargetData.For(ref handle);
		handle.target.SetData(ref data);
		data.portalMatrix = travelMatrix;
		data.position = travelMatrix.MultiplyPoint3x4(data.position);
		data.headPosition = travelMatrix.MultiplyPoint3x4(data.headPosition);
		data.velocity = travelMatrix.MultiplyVector(data.velocity);
		data.rotation = travelMatrix.rotation * data.rotation;
		return data;
	}

	public TargetDataRef GetDataReference(TargetHandle handle)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log("GetDataReference");
		int num = IndexFromSequence(scene.nativeScene.portals, handle.portals);
		Span<float4x4> span = scene.sequenceMatrices.AsSpan();
		Span<PortalHandleSequence> span2 = CollectionsMarshal.AsSpan(scene.portalSequences);
		int num2 = targets.IndexOf(handle.target);
		int length = num * targets.Count + num2;
		return new TargetDataRef(scene, handle.target, span.Slice(num, 1), span2.Slice(num, (!scene.culledSequences[num]) ? 1 : 0), MemoryMarshal.CreateSpan(ref arrays, length));
	}

	public void RegisterTarget(ITarget target, CancellationToken token)
	{
		newTargets.Add(target);
		token.Register(delegate
		{
			newTargets.Remove(target);
			removalTargets.Add(target);
		});
	}

	public void RegisterVision(Vision vision, CancellationToken token)
	{
		visions.Add(vision);
		token.Register(delegate
		{
			visions.Remove(vision);
			vision.Dispose();
		});
	}

	public void UpdateData()
	{
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Unknown result type (might be due to invalid IL or missing references)
		//IL_0665: Unknown result type (might be due to invalid IL or missing references)
		UpdateTargets();
		int count = scene.portalSequences.Count;
		int count2 = targets.Count;
		int num = count * count2;
		outputVisionHandle.Complete();
		if (defaults.positions.IsCreated)
		{
			defaults.Dispose();
		}
		defaults.Allocate(count2, NativeArrayOptions.UninitializedMemory);
		if (arrays.positions.IsCreated)
		{
			arrays.Dispose();
		}
		arrays.Allocate(num, NativeArrayOptions.UninitializedMemory);
		_ = Matrix4x4.identity;
		TargetData data = default(TargetData);
		NativeArray<Vector3> nativeArray = defaults.positions.Reinterpret<Vector3>();
		Span<Vector3> span = nativeArray.AsSpan();
		nativeArray = defaults.headPositions.Reinterpret<Vector3>();
		Span<Vector3> span2 = nativeArray.AsSpan();
		nativeArray = defaults.velocities.Reinterpret<Vector3>();
		Span<Vector3> span3 = nativeArray.AsSpan();
		Span<Quaternion> span4 = defaults.rotations.Reinterpret<Quaternion>().AsSpan();
		Span<TargetType> span5 = defaults.types.AsSpan();
		for (int i = 0; i < count2; i++)
		{
			ITarget target = targets[i];
			target.UpdateCachedTransformData();
			target.SetData(ref data);
			span[i] = data.position;
			span2[i] = data.headPosition;
			span3[i] = data.velocity;
			span4[i] = data.rotation;
			span5[i] = target.Type;
		}
		CopyDefaultDataJob jobData = new CopyDefaultDataJob
		{
			stride = defaults.positions.Length,
			defaults = defaults,
			targets = arrays
		};
		calculateTargetDataJob = new CalculateTargetDataJob
		{
			targetCount = count2,
			matrices = scene.sequenceMatrices,
			arrays = arrays
		};
		JobHandle dependency = IJobForExtensions.ScheduleParallelByRef(ref jobData, arrays.positions.Length, 64, default(JobHandle));
		calculateTargetDataHandle = IJobForExtensions.ScheduleParallelByRef(ref calculateTargetDataJob, arrays.positions.Length, count2, dependency);
		if (distances.IsCreated)
		{
			distances.Dispose();
		}
		distances = new NativeArray<float>(num, Allocator.TempJob);
		if (valid.IsCreated)
		{
			valid.Dispose();
		}
		valid = new NativeArray<bool>(num, Allocator.TempJob);
		if (visionOrigins.IsCreated)
		{
			visionOrigins.Dispose();
		}
		visionOrigins = new NativeArray<float3>(visions.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		if (visionFilters.IsCreated)
		{
			visionFilters.Dispose();
		}
		visionFilters = new NativeArray<VisionTypeFilter>(visions.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		if (counts.IsCreated)
		{
			counts.Dispose();
		}
		if (startIndices.IsCreated)
		{
			startIndices.Dispose();
		}
		if (((NativeStream)(ref targetStream)).IsCreated)
		{
			((NativeStream)(ref targetStream)).Dispose();
		}
		counts = new NativeArray<int>(visions.Count, Allocator.TempJob);
		startIndices = new NativeArray<int>(visions.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		targetStream = new NativeStream(visions.Count, AllocatorHandle.op_Implicit(Allocator.TempJob));
		if (!targetsList.IsCreated)
		{
			targetsList = new NativeList<TargetIndexAndDistance>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		}
		targetsList.Clear();
		Span<float3> span6 = visionOrigins.AsSpan();
		Span<VisionTypeFilter> span7 = visionFilters.AsSpan();
		for (int j = 0; j < visions.Count; j++)
		{
			span6[j] = float3.op_Implicit(visions[j].sourcePos);
			span7[j] = visions[j].filter;
		}
		DistanceJob jobData2 = new DistanceJob
		{
			targetCount = num,
			visionOrigins = visionOrigins,
			visionFilters = visionFilters,
			targetPositions = arrays.positions,
			targetTypes = arrays.types,
			output = ((NativeStream)(ref targetStream)).AsWriter(),
			counts = counts
		};
		CalculateStartIndicesJob jobData3 = new CalculateStartIndicesJob
		{
			startIndices = startIndices,
			counts = counts,
			targetList = targetsList
		};
		OutputJob jobData4 = new OutputJob
		{
			output = ((NativeStream)(ref targetStream)).AsReader(),
			array = targetsList
		};
		NativeArray<TargetIndexAndDistance> targetList = targetsList.AsDeferredJobArray();
		SortJob jobData5 = new SortJob
		{
			count = counts,
			startIndices = startIndices,
			comparer = default(DistanceComparer),
			targetList = targetList
		};
		JobHandle jobHandle = IJobForExtensions.ScheduleParallelByRef(ref jobData2, visions.Count, 2, calculateTargetDataHandle);
		visionOrigins.Dispose(jobHandle);
		visionFilters.Dispose(jobHandle);
		JobHandle dependency2 = IJobExtensions.ScheduleByRef(ref jobData3, jobHandle);
		JobHandle dependsOn = IJobForExtensions.ScheduleByRef(ref jobData4, visions.Count, dependency2);
		JobHandle jobHandle2 = IJobParallelForExtensions.ScheduleByRef(ref jobData5, visions.Count, 2, dependsOn);
		outputVisionHandle = jobHandle2;
		distances.Dispose(outputVisionHandle);
		valid.Dispose(outputVisionHandle);
		((NativeStream)(ref targetStream)).Dispose(outputVisionHandle);
		for (int k = 0; k < visions.Count; k++)
		{
			visions[k].visionIndex = k;
			visions[k].counts = counts;
			visions[k].startIndices = startIndices;
			visions[k].targetsArray = targetsList;
			visions[k].handle = outputVisionHandle;
		}
	}

	private void UpdateTargets()
	{
		targets.AddRange(newTargets);
		for (int i = 0; i < removalTargets.Count; i++)
		{
			targets.Remove(removalTargets[i]);
		}
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (targets[num] == null || targets[num].GameObject == null)
			{
				targets.RemoveAt(num);
			}
		}
		targetCount = targets.Count;
		newTargets.Clear();
		removalTargets.Clear();
	}

	public void Dispose()
	{
		outputVisionHandle.Complete();
		if (distances.IsCreated)
		{
			distances.Dispose();
		}
		if (valid.IsCreated)
		{
			valid.Dispose();
		}
		defaults.Dispose();
		arrays.Dispose();
		defaults.Dispose();
		if (counts.IsCreated)
		{
			counts.Dispose();
		}
		if (startIndices.IsCreated)
		{
			startIndices.Dispose();
		}
		if (((NativeStream)(ref targetStream)).IsCreated)
		{
			((NativeStream)(ref targetStream)).Dispose();
		}
		if (targetsList.IsCreated)
		{
			targetsList.Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static int CountPermutations_0024BurstManaged(int count, int depth)
	{
		int num = 0;
		for (int i = 0; i < depth; i++)
		{
			int num2 = count;
			for (int j = 0; j < i; j++)
			{
				num2 *= count;
			}
			num += num2;
		}
		return num;
	}
}
