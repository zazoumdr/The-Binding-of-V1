using System;
using System.Runtime.CompilerServices;
using ULTRAKILL.Cheats;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public static class ColliderUtility
{
	internal delegate void BurstClosestPoint_Int16_0000049A_0024PostfixBurstDelegate(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint);

	internal static class BurstClosestPoint_Int16_0000049A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(BurstClosestPoint_Int16_0000049A_0024PostfixBurstDelegate).TypeHandle);
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

		static BurstClosestPoint_Int16_0000049A_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Mesh.MeshData, ref float3, ref float3, bool, ref float3, void>)functionPointer)(ref data, ref testPosition, ref localUp, ignoreVerticalTriangles, ref closestPoint);
					return;
				}
			}
			BurstClosestPoint_Int16_0024BurstManaged(ref data, in testPosition, in localUp, ignoreVerticalTriangles, out closestPoint);
		}
	}

	internal delegate void BurstClosestPoint_Int32_0000049B_0024PostfixBurstDelegate(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint);

	internal static class BurstClosestPoint_Int32_0000049B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(BurstClosestPoint_Int32_0000049B_0024PostfixBurstDelegate).TypeHandle);
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

		static BurstClosestPoint_Int32_0000049B_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Mesh.MeshData, ref float3, ref float3, bool, ref float3, void>)functionPointer)(ref data, ref testPosition, ref localUp, ignoreVerticalTriangles, ref closestPoint);
					return;
				}
			}
			BurstClosestPoint_Int32_0024BurstManaged(ref data, in testPosition, in localUp, ignoreVerticalTriangles, out closestPoint);
		}
	}

	private static bool InTriangleBurst(float3 a, float3 b, float3 c, float3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float3 val = b - a;
		float3 val2 = c - a;
		float3 val3 = p - a;
		float num = math.dot(val, val);
		float num2 = math.dot(val, val2);
		float num3 = math.dot(val, val3);
		float num4 = math.dot(val2, val2);
		float num5 = math.dot(val2, val3);
		float num6 = 1f / (num * num4 - num2 * num2);
		float num7 = (num4 * num3 - num2 * num5) * num6;
		float num8 = (num * num5 - num2 * num3) * num6;
		if (num7 >= 0f && num8 >= 0f)
		{
			return num7 + num8 < 1f;
		}
		return false;
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position)
	{
		return FindClosestPoint(collider, position, ignoreVerticalTriangles: false);
	}

	public static Vector3 FindClosestPoint(Collider collider, Vector3 position, bool ignoreVerticalTriangles)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (NonConvexJumpDebug.Active)
		{
			NonConvexJumpDebug.Reset();
		}
		if (collider is MeshCollider { convex: false } meshCollider)
		{
			Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(meshCollider.sharedMesh);
			Transform transform = meshCollider.transform;
			position = transform.InverseTransformPoint(position);
			Vector3 source = transform.InverseTransformDirection(Vector3.up);
			Mesh.MeshData data = meshDataArray[0];
			float3 closestPoint = float3.zero;
			if (data.indexFormat == IndexFormat.UInt16)
			{
				BurstClosestPoint_Int16(ref data, in Unsafe.As<Vector3, float3>(ref position), in Unsafe.As<Vector3, float3>(ref source), ignoreVerticalTriangles, out closestPoint);
			}
			else
			{
				BurstClosestPoint_Int32(ref data, in Unsafe.As<Vector3, float3>(ref position), in Unsafe.As<Vector3, float3>(ref source), ignoreVerticalTriangles, out closestPoint);
			}
			meshDataArray.Dispose();
			return transform.TransformPoint(Unsafe.As<float3, Vector3>(ref closestPoint));
		}
		return collider.ClosestPoint(position);
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private static void BurstClosestPoint_Int16(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
	{
		BurstClosestPoint_Int16_0000049A_0024BurstDirectCall.Invoke(ref data, in testPosition, in localUp, ignoreVerticalTriangles, out closestPoint);
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private static void BurstClosestPoint_Int32(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
	{
		BurstClosestPoint_Int32_0000049B_0024BurstDirectCall.Invoke(ref data, in testPosition, in localUp, ignoreVerticalTriangles, out closestPoint);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static void BurstClosestPoint_Int16_0024BurstManaged(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		closestPoint = float3.zero;
		float num = float.PositiveInfinity;
		NativeSlice<float3> vertexAttributeSlice = data.GetVertexAttributeSlice<float3>(VertexAttribute.Position);
		NativeArray<ushort> indexData = data.GetIndexData<ushort>();
		int i = 0;
		for (int num2 = indexData.Length / 3; i < num2; i++)
		{
			float3 val = vertexAttributeSlice[indexData[3 * i]];
			float3 val2 = vertexAttributeSlice[indexData[3 * i + 1]];
			float3 val3 = vertexAttributeSlice[indexData[3 * i + 2]];
			float3 val4 = math.normalize(math.cross(val2 - val, val3 - val));
			float num3 = 0f - math.dot(val4, val);
			if (ignoreVerticalTriangles && math.abs(math.dot(val4, localUp)) >= 0.9f)
			{
				continue;
			}
			float num4 = math.dot(val4, testPosition) + num3;
			float num5 = math.abs(num4);
			if (!(num5 >= num))
			{
				float3 val5 = testPosition - val4 * num4;
				if (InTriangleBurst(val, val2, val3, val5))
				{
					num = num5;
					closestPoint = val5;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static void BurstClosestPoint_Int32_0024BurstManaged(ref Mesh.MeshData data, in float3 testPosition, in float3 localUp, bool ignoreVerticalTriangles, out float3 closestPoint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		closestPoint = float3.zero;
		float num = float.PositiveInfinity;
		NativeSlice<float3> vertexAttributeSlice = data.GetVertexAttributeSlice<float3>(VertexAttribute.Position);
		NativeArray<int> indexData = data.GetIndexData<int>();
		int i = 0;
		for (int num2 = indexData.Length / 3; i < num2; i++)
		{
			float3 val = vertexAttributeSlice[indexData[3 * i]];
			float3 val2 = vertexAttributeSlice[indexData[3 * i + 1]];
			float3 val3 = vertexAttributeSlice[indexData[3 * i + 2]];
			float3 val4 = math.normalize(math.cross(val2 - val, val3 - val));
			float num3 = 0f - math.dot(val4, val);
			if (ignoreVerticalTriangles && math.abs(math.dot(val4, localUp)) >= 0.9f)
			{
				continue;
			}
			float num4 = math.dot(val4, testPosition) + num3;
			float num5 = math.abs(num4);
			if (!(num5 >= num))
			{
				float3 val5 = testPosition - val4 * num4;
				if (InTriangleBurst(val, val2, val3, val5))
				{
					num = num5;
					closestPoint = val5;
				}
			}
		}
	}
}
