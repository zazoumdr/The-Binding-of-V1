using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

[BurstCompile]
public class PortalScene : IDisposable
{
	public struct PortalRaySegment
	{
		public PortalHandle handle;

		public float3 start;

		public float3 end;

		public float3 direction;
	}

	internal delegate void CalculateMatrices_00002A24_0024PostfixBurstDelegate(int depth, in NativeArray<NativePortal> singleMatrices, ref NativeArray<float4x4> matrices);

	internal static class CalculateMatrices_00002A24_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalculateMatrices_00002A24_0024PostfixBurstDelegate).TypeHandle);
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

		static CalculateMatrices_00002A24_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(int depth, in NativeArray<NativePortal> singleMatrices, ref NativeArray<float4x4> matrices)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<int, ref NativeArray<NativePortal>, ref NativeArray<float4x4>, void>)functionPointer)(depth, ref singleMatrices, ref matrices);
					return;
				}
			}
			CalculateMatrices_0024BurstManaged(depth, in singleMatrices, ref matrices);
		}
	}

	internal delegate void Internal_FindCrossedPortals_00002A29_0024PostfixBurstDelegate(in NativePortalScene lastScene, in NativePortalScene currentScene, in float3 a, in float3 b, ref NativeList<NativePortalIntersection> intersections);

	internal static class Internal_FindCrossedPortals_00002A29_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Internal_FindCrossedPortals_00002A29_0024PostfixBurstDelegate).TypeHandle);
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

		static Internal_FindCrossedPortals_00002A29_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in NativePortalScene lastScene, in NativePortalScene currentScene, in float3 a, in float3 b, ref NativeList<NativePortalIntersection> intersections)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativePortalScene, ref NativePortalScene, ref float3, ref float3, ref NativeList<NativePortalIntersection>, void>)functionPointer)(ref lastScene, ref currentScene, ref a, ref b, ref intersections);
					return;
				}
			}
			Internal_FindCrossedPortals_0024BurstManaged(in lastScene, in currentScene, in a, in b, ref intersections);
		}
	}

	internal delegate void Internal_FindPortalsBetween_00002A2C_0024PostfixBurstDelegate(in NativePortalScene currentScene, in float3 start, in float3 end, ref NativeList<NativePortalIntersection> intersections, bool allowBackfaces = false);

	internal static class Internal_FindPortalsBetween_00002A2C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Internal_FindPortalsBetween_00002A2C_0024PostfixBurstDelegate).TypeHandle);
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

		static Internal_FindPortalsBetween_00002A2C_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in NativePortalScene currentScene, in float3 start, in float3 end, ref NativeList<NativePortalIntersection> intersections, bool allowBackfaces = false)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativePortalScene, ref float3, ref float3, ref NativeList<NativePortalIntersection>, bool, void>)functionPointer)(ref currentScene, ref start, ref end, ref intersections, allowBackfaces);
					return;
				}
			}
			Internal_FindPortalsBetween_0024BurstManaged(in currentScene, in start, in end, ref intersections, allowBackfaces);
		}
	}

	internal delegate void Internal_TraversePortalSequence_00002A2D_0024PostfixBurstDelegate(in NativePortalScene currentScene, in float3 start, in float3 end, in float3 realEnd, ref NativeList<PortalRaySegment> segments, in NativeList<PortalHandle> handles, out bool result);

	internal static class Internal_TraversePortalSequence_00002A2D_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Internal_TraversePortalSequence_00002A2D_0024PostfixBurstDelegate).TypeHandle);
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

		static Internal_TraversePortalSequence_00002A2D_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in NativePortalScene currentScene, in float3 start, in float3 end, in float3 realEnd, ref NativeList<PortalRaySegment> segments, in NativeList<PortalHandle> handles, out bool result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativePortalScene, ref float3, ref float3, ref float3, ref NativeList<PortalRaySegment>, ref NativeList<PortalHandle>, ref bool, void>)functionPointer)(ref currentScene, ref start, ref end, ref realEnd, ref segments, ref handles, ref result);
					return;
				}
			}
			Internal_TraversePortalSequence_0024BurstManaged(in currentScene, in start, in end, in realEnd, ref segments, in handles, out result);
		}
	}

	public const int visionPortalDepth = 2;

	public List<PortalIdentifier> identifiers = new List<PortalIdentifier>();

	public Dictionary<PortalHandle, PortalIdentifier> portalIdentifiersLookup = new Dictionary<PortalHandle, PortalIdentifier>();

	public List<PortalHandleSequence> portalSequences = new List<PortalHandleSequence>();

	public List<bool> culledSequences = new List<bool>();

	public NativeArray<float4x4> sequenceMatrices;

	private NativeList<PortalHandle> sequenceHandleCache;

	private NativeList<PortalRaySegment> raySegmentCache;

	public NativeList<bool> visionPossible;

	public NativePortalScene lastScene;

	public NativePortalScene nativeScene;

	private PortalVisionJob visionJob;

	private NativeList<NativePortalIntersection> intersections;

	[BurstCompile]
	public static void CalculateMatrices(int depth, in NativeArray<NativePortal> singleMatrices, ref NativeArray<float4x4> matrices)
	{
		CalculateMatrices_00002A24_0024BurstDirectCall.Invoke(depth, in singleMatrices, ref matrices);
	}

	private void UpdatePortalSequences(int depth, int lastNumAddedSquences, NativeList<NativePortal> portals)
	{
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		Span<bool> span = visionPossible.AsArray().AsSpan();
		Span<NativePortal> span2 = portals.AsArray().AsSpan();
		int length = span2.Length;
		int num;
		switch (depth)
		{
		case 0:
			portalSequences.Clear();
			culledSequences.Clear();
			portalSequences.Add(default(PortalHandleSequence));
			culledSequences.Add(item: false);
			num = 1;
			break;
		case 1:
		{
			for (int k = 0; k < length; k++)
			{
				PortalHandle handle2 = span2[k].handle;
				portalSequences.Add(new PortalHandleSequence(handle2));
				culledSequences.Add(item: false);
			}
			num = 1;
			break;
		}
		default:
		{
			num = portalSequences.Count;
			Span<PortalHandleSequence> span3 = CollectionsMarshal.AsSpan(portalSequences);
			for (int i = lastNumAddedSquences; i < num; i++)
			{
				PortalHandleSequence portalHandleSequence = span3[i];
				PortalHandle portalHandle = portalHandleSequence[portalHandleSequence.Count - 1];
				int num2 = (i - lastNumAddedSquences) % length;
				int num3 = ((portalHandle.side == PortalSide.Enter) ? (num2 + 1) : (num2 - 1));
				PortalHandleSequence item = portalHandleSequence.Append(PortalHandle.None);
				for (int j = 0; j < length; j++)
				{
					PortalHandle handle = span2[j].handle;
					bool flag = span[num3 * length + j];
					culledSequences.Add(num3 == j || flag);
					item[item.Count - 1] = handle;
					portalSequences.Add(item);
				}
			}
			break;
		}
		}
		if (depth < 2)
		{
			UpdatePortalSequences(depth + 1, num, portals);
		}
	}

	public PortalScene()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		sequenceHandleCache = new NativeList<PortalHandle>(0, AllocatorHandle.op_Implicit(Allocator.Persistent));
		raySegmentCache = new NativeList<PortalRaySegment>(0, AllocatorHandle.op_Implicit(Allocator.Persistent));
		visionPossible = new NativeList<bool>(0, AllocatorHandle.op_Implicit(Allocator.Persistent));
	}

	public bool IsTraversable(Portal portal, PortalSide side, bool asEnemy = false)
	{
		PortalTravellerFlags travelFlags = portal.GetTravelFlags(side);
		return travelFlags.HasFlag(asEnemy ? PortalTravellerFlags.Enemy : PortalTravellerFlags.Player);
	}

	public float GetMinimumPassThroughSpeed(Portal portal, PortalSide side)
	{
		if (!portal)
		{
			return 0f;
		}
		if (side != PortalSide.Enter)
		{
			return portal.minimumExitSideSpeed;
		}
		return portal.minimumEntrySideSpeed;
	}

	[BurstCompile]
	private static void Internal_FindCrossedPortals(in NativePortalScene lastScene, in NativePortalScene currentScene, in float3 a, in float3 b, ref NativeList<NativePortalIntersection> intersections)
	{
		Internal_FindCrossedPortals_00002A29_0024BurstDirectCall.Invoke(in lastScene, in currentScene, in a, in b, ref intersections);
	}

	public bool FindCrossedPortals(Vector3 lastPos, Vector3 pos, out List<(PortalHandle, Vector3, float)> hitPortals)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		hitPortals = new List<(PortalHandle, Vector3, float)>();
		intersections.Clear();
		Internal_FindCrossedPortals(in lastScene, in nativeScene, float3.op_Implicit(lastPos), float3.op_Implicit(pos), ref intersections);
		foreach (NativePortalIntersection intersection in intersections)
		{
			hitPortals.Add((intersection.handle, float3.op_Implicit(intersection.point), intersection.distance));
		}
		hitPortals.Sort(((PortalHandle, Vector3, float) a, (PortalHandle, Vector3, float) b) => a.Item3.CompareTo(b.Item3));
		return hitPortals.Count > 0;
	}

	public bool FindCrossedPortal(Vector3 lastPos, Vector3 pos, out PortalHandle handle, out Vector3 intersection)
	{
		handle = PortalHandle.None;
		intersection = Vector3.zero;
		if (!FindCrossedPortals(lastPos, pos, out var hitPortals))
		{
			return false;
		}
		(PortalHandle, Vector3, float) tuple = hitPortals[0];
		PortalHandle item = tuple.Item1;
		Vector3 item2 = tuple.Item2;
		handle = item;
		intersection = item2;
		return true;
	}

	[BurstCompile]
	private static void Internal_FindPortalsBetween(in NativePortalScene currentScene, in float3 start, in float3 end, ref NativeList<NativePortalIntersection> intersections, bool allowBackfaces = false)
	{
		Internal_FindPortalsBetween_00002A2C_0024BurstDirectCall.Invoke(in currentScene, in start, in end, ref intersections, allowBackfaces);
	}

	[BurstCompile]
	private static void Internal_TraversePortalSequence(in NativePortalScene currentScene, in float3 start, in float3 end, in float3 realEnd, ref NativeList<PortalRaySegment> segments, in NativeList<PortalHandle> handles, out bool result)
	{
		Internal_TraversePortalSequence_00002A2D_0024BurstDirectCall.Invoke(in currentScene, in start, in end, in realEnd, ref segments, in handles, out result);
	}

	public unsafe bool TraversePortalSequence(Vector3 start, Vector3 end, Vector3 realEnd, PortalHandleSequence sequence, out NativeList<PortalRaySegment> outSegments)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		Span<PortalHandle> span = sequence.AsSpan();
		int length = span.Length;
		raySegmentCache.GetUnsafeList()->m_length = 0;
		UnsafeList<PortalHandle>* unsafeList = sequenceHandleCache.GetUnsafeList();
		if (unsafeList->m_capacity < length)
		{
			unsafeList->Resize(length * 2, NativeArrayOptions.UninitializedMemory);
		}
		unsafeList->m_length = length;
		PortalHandle* ptr = unsafeList->Ptr;
		for (int i = 0; i < length; i++)
		{
			PortalHandle portalHandle = span[i];
			ptr[i] = portalHandle;
		}
		Internal_TraversePortalSequence(in nativeScene, in Unsafe.As<Vector3, float3>(ref start), in Unsafe.As<Vector3, float3>(ref end), in Unsafe.As<Vector3, float3>(ref realEnd), ref raySegmentCache, in sequenceHandleCache, out var result);
		outSegments = raySegmentCache;
		return result;
	}

	public bool FindPortalsBetween(Vector3 start, Vector3 end, ref List<(PortalHandle, Vector3, float)> hitPortals, bool allowBackfaces = false)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		hitPortals.Clear();
		intersections.Clear();
		Internal_FindPortalsBetween(in nativeScene, in Unsafe.As<Vector3, float3>(ref start), in Unsafe.As<Vector3, float3>(ref end), ref intersections, allowBackfaces);
		foreach (NativePortalIntersection intersection in intersections)
		{
			hitPortals.Add((intersection.handle, float3.op_Implicit(intersection.point), intersection.distance));
		}
		hitPortals.Sort(((PortalHandle, Vector3, float) a, (PortalHandle, Vector3, float) b) => a.Item3.CompareTo(b.Item3));
		return hitPortals.Count > 0;
	}

	public bool FindPortalBetween(Vector3 start, Vector3 end, out PortalHandle hitPortal, out Vector3 intersection, out float distance, bool allowBackfaces = false)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (nativeScene.portals.Length == 0)
		{
			hitPortal = PortalHandle.None;
			intersection = default(Vector3);
			distance = 0f;
			return false;
		}
		intersections.Clear();
		Internal_FindPortalsBetween(in nativeScene, in Unsafe.As<Vector3, float3>(ref start), in Unsafe.As<Vector3, float3>(ref end), ref intersections);
		if (intersections.IsEmpty)
		{
			hitPortal = PortalHandle.None;
			intersection = default(Vector3);
			distance = 0f;
			return false;
		}
		NativeSortExtension.Sort<NativePortalIntersection>(intersections);
		NativePortalIntersection nativePortalIntersection = intersections[0];
		hitPortal = nativePortalIntersection.handle;
		intersection = float3.op_Implicit(nativePortalIntersection.point);
		distance = nativePortalIntersection.distance;
		return true;
	}

	public Matrix4x4 GetTravelMatrix(PortalHandle handle)
	{
		NativePortal nativePortal = nativeScene.LookupPortal(in handle);
		return nativePortal.travelMatrixManaged;
	}

	public Matrix4x4 GetTravelMatrix(in PortalHandleSequence travelHandles)
	{
		Matrix4x4 matrix4x = Matrix4x4.identity;
		int count = travelHandles.Count;
		for (int i = 0; i < count; i++)
		{
			PortalHandle handle = travelHandles[i];
			matrix4x = GetTravelMatrix(handle) * matrix4x;
		}
		return matrix4x;
	}

	public Matrix4x4 GetTravelMatrix(PortalTraversalV2[] travelHandles)
	{
		Matrix4x4 matrix4x = Matrix4x4.identity;
		int num = travelHandles.Length;
		for (int i = 0; i < num; i++)
		{
			PortalHandle portalHandle = travelHandles[i].portalHandle;
			matrix4x = GetTravelMatrix(portalHandle) * matrix4x;
		}
		return matrix4x;
	}

	public Portal GetPortalObject(PortalHandle handle)
	{
		return Resources.InstanceIDToObject(handle.instanceId) as Portal;
	}

	public string GetPortalName(PortalHandle handle)
	{
		Portal portalObject = GetPortalObject(handle);
		if (handle.side != PortalSide.Enter)
		{
			return portalObject.exit.name;
		}
		return portalObject.entry.name;
	}

	public void Sync(List<Portal> portals)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (lastScene.valid)
		{
			lastScene.Dispose();
		}
		lastScene = nativeScene;
		identifiers.Clear();
		portalIdentifiersLookup.Clear();
		nativeScene = NativePortalScene.Create(portals, identifiers, portalIdentifiersLookup);
		int length = nativeScene.portals.Length;
		visionPossible.Resize(length * length, NativeArrayOptions.UninitializedMemory);
		visionPossible.AsArray().AsSpan().Fill(value: true);
		if (!intersections.IsCreated)
		{
			intersections = new NativeList<NativePortalIntersection>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		}
		else
		{
			intersections.Clear();
		}
		portalSequences.Clear();
		culledSequences.Clear();
		UpdatePortalSequences(0, 0, nativeScene.portals);
		if (sequenceMatrices.IsCreated)
		{
			sequenceMatrices.Dispose();
		}
		sequenceMatrices = new NativeArray<float4x4>(portalSequences.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		CalculateMatrices(2, nativeScene.portals.AsArray(), ref sequenceMatrices);
	}

	public void Dispose()
	{
		if (lastScene.valid)
		{
			lastScene.Dispose();
		}
		if (nativeScene.valid)
		{
			nativeScene.Dispose();
		}
		sequenceHandleCache.Dispose();
		raySegmentCache.Dispose();
		sequenceMatrices.Dispose();
		visionPossible.Dispose();
		intersections.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateMatrices_0024BurstManaged(int depth, in NativeArray<NativePortal> singleMatrices, ref NativeArray<float4x4> matrices)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		int length = singleMatrices.Length;
		int num = 0;
		int num2 = length;
		int num3 = length;
		for (int i = 0; i <= depth; i++)
		{
			switch (i)
			{
			case 0:
				matrices[0] = float4x4.identity;
				continue;
			case 1:
			{
				for (int j = 0; j < length; j++)
				{
					matrices[j + 1] = singleMatrices[j].travelMatrix;
				}
				num = 1;
				num2 = length + 1;
				num3 = length;
				continue;
			}
			}
			for (int k = 0; k < num3; k++)
			{
				float4x4 val = matrices[num + k];
				for (int l = 0; l < length; l++)
				{
					int index = num2 + k * length + l;
					matrices[index] = math.mul(singleMatrices[l].travelMatrix, val);
				}
			}
			num3 *= length;
			num = num2;
			num2 += num3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Internal_FindCrossedPortals_0024BurstManaged(in NativePortalScene lastScene, in NativePortalScene currentScene, in float3 a, in float3 b, ref NativeList<NativePortalIntersection> intersections)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		float3 val = b - a;
		int length = currentScene.portals.Length;
		for (int i = 0; i < length; i++)
		{
			NativePortal nativePortal = currentScene.portals[i];
			NativePortal nativePortal2 = nativePortal;
			if (lastScene.valid)
			{
				ref NativePortal reference = ref lastScene.LookupPortal(in nativePortal.handle);
				if (reference.valid)
				{
					nativePortal2 = reference;
				}
			}
			float4x4 toWorld = nativePortal.transform.toWorld;
			float4x4 toWorld2 = nativePortal2.transform.toWorld;
			float3 xyz = ((float4)(ref toWorld.c3)).xyz;
			float3 xyz2 = ((float4)(ref toWorld2.c3)).xyz;
			float3 val2 = math.normalizesafe(((float4)(ref toWorld.c2)).xyz, default(float3));
			float3 val3 = math.normalizesafe(((float4)(ref toWorld2.c2)).xyz, default(float3));
			quaternion val4 = math.quaternion(toWorld2);
			quaternion val5 = math.quaternion(toWorld);
			quaternion val6 = math.mul(val5, math.inverse(val4));
			float3 val7 = math.normalizesafe(((float4)(ref val6.value)).xyz, default(float3));
			float3 val8 = xyz - xyz2;
			float3 val9 = val - val8;
			float num = math.dot(a - xyz2, val3);
			float num2 = math.dot(b - xyz, val2);
			bool flag = num <= 0.001f && num2 > -0.001f && (num * num2 < 0f || (num <= 0f && num2 > 0f));
			if (math.abs(num) > 5f || !flag || num * num2 > 0f)
			{
				continue;
			}
			float num3 = 0.5f;
			for (int j = 0; j < 20; j++)
			{
				float3 val10 = math.lerp(xyz2, xyz, num3);
				float3 val11 = math.lerp(a, b, num3);
				float3 val12 = math.normalizesafe(math.lerp(val3, val2, num3), default(float3));
				float3 val13 = val11 - val10;
				float num4 = math.dot(val13, val12);
				float num5 = math.dot(val9, val12);
				float3 val14 = math.cross(val7, val12);
				float num6 = math.dot(val13, val14);
				float num7 = num5 + num6;
				if (math.abs(num7) < 0.0001f)
				{
					break;
				}
				num3 = math.clamp(num3 - num4 / num7, 0f, 1f);
				if (math.abs(num4) < 0.001f)
				{
					break;
				}
			}
			float3 val15 = math.lerp(xyz2, xyz, num3);
			float3 val16 = math.lerp(a, b, num3);
			quaternion val17 = math.slerp(val4, val5, num3);
			float3 val18 = math.transform(math.fastinverse(new float4x4(val17, val15)), val16);
			if (!(math.abs(val18.x) > nativePortal.dimensions.x / 2f) && !(math.abs(val18.y) > nativePortal.dimensions.y / 2f))
			{
				NativePortalIntersection nativePortalIntersection = new NativePortalIntersection
				{
					handle = nativePortal.handle,
					distance = math.length(b - a) * num3,
					point = val16
				};
				intersections.Add(ref nativePortalIntersection);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Internal_FindPortalsBetween_0024BurstManaged(in NativePortalScene currentScene, in float3 start, in float3 end, ref NativeList<NativePortalIntersection> intersections, bool allowBackfaces = false)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		PortalRay ray = new PortalRay(start, end);
		foreach (NativePortal portal in currentScene.portals)
		{
			if (portal.Raycast(in ray, out var intersection, allowBackfaces))
			{
				intersections.Add(ref intersection);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Internal_TraversePortalSequence_0024BurstManaged(in NativePortalScene currentScene, in float3 start, in float3 end, in float3 realEnd, ref NativeList<PortalRaySegment> segments, in NativeList<PortalHandle> handles, out bool result)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		result = true;
		PortalRay ray = new PortalRay(start, end);
		foreach (PortalHandle handle2 in handles)
		{
			PortalHandle handle = handle2;
			NativePortal portal = currentScene.LookupPortal(in handle);
			if (portal.valid)
			{
				if (!portal.Raycast(in ray, out var intersection))
				{
					result = false;
					return;
				}
				float num = intersection.distance * intersection.distance;
				if (num > ray.distanceSq)
				{
					result = false;
					return;
				}
				PortalRaySegment portalRaySegment = new PortalRaySegment
				{
					start = ray.start,
					end = intersection.point,
					direction = ray.direction,
					handle = portal.handle
				};
				segments.Add(ref portalRaySegment);
				float4x4 travelMatrix = portal.travelMatrix;
				float3 start2 = math.transform(travelMatrix, intersection.point);
				float3 direction = math.rotate(travelMatrix, ray.direction);
				ray.start = start2;
				ray.direction = direction;
				ray.distanceSq -= num;
			}
		}
		PortalRaySegment portalRaySegment2 = new PortalRaySegment
		{
			start = ray.start,
			end = realEnd,
			direction = ray.direction,
			handle = PortalHandle.None
		};
		segments.Add(ref portalRaySegment2);
	}
}
