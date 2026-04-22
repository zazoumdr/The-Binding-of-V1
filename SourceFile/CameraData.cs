using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CameraData
{
	internal delegate void Create_000017BF_0024PostfixBurstDelegate(in float3 pos, in quaternion rot, int cullingMask, out CameraData data);

	internal static class Create_000017BF_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Create_000017BF_0024PostfixBurstDelegate).TypeHandle);
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

		static Create_000017BF_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in float3 pos, in quaternion rot, int cullingMask, out CameraData data)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref quaternion, int, ref CameraData, void>)functionPointer)(ref pos, ref rot, cullingMask, ref data);
					return;
				}
			}
			Create_0024BurstManaged(in pos, in rot, cullingMask, out data);
		}
	}

	internal delegate void CalculateObliqueMatrix_000017C0_0024PostfixBurstDelegate(in float4x4 projection, in float4 clipPlane, out float4x4 obliqueMatrix);

	internal static class CalculateObliqueMatrix_000017C0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalculateObliqueMatrix_000017C0_0024PostfixBurstDelegate).TypeHandle);
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

		static CalculateObliqueMatrix_000017C0_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in float4x4 projection, in float4 clipPlane, out float4x4 obliqueMatrix)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float4x4, ref float4, ref float4x4, void>)functionPointer)(ref projection, ref clipPlane, ref obliqueMatrix);
					return;
				}
			}
			CalculateObliqueMatrix_0024BurstManaged(in projection, in clipPlane, out obliqueMatrix);
		}
	}

	public float4x4 WorldToCamera;

	public float4x4 CameraToWorld;

	public float4x4 CullingMatrix;

	public float3 Position;

	public float3 Forward;

	public float3 Up;

	public int cullingMask;

	public static CameraData FromCamera(Camera cam)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 source = cam.transform.position;
		Quaternion source2 = cam.transform.rotation;
		Create(in Unsafe.As<Vector3, float3>(ref source), in Unsafe.As<Quaternion, quaternion>(ref source2), cam.cullingMask, out var data);
		Matrix4x4 source3 = cam.cullingMatrix;
		data.CullingMatrix = Unsafe.As<Matrix4x4, float4x4>(ref source3);
		return data;
	}

	public static CameraData FromValues(float3 pos, quaternion rot, int cullingMask)
	{
		Create(in pos, in rot, cullingMask, out var data);
		return data;
	}

	[BurstCompile]
	public static void Create(in float3 pos, in quaternion rot, int cullingMask, out CameraData data)
	{
		Create_000017BF_0024BurstDirectCall.Invoke(in pos, in rot, cullingMask, out data);
	}

	[BurstCompile]
	public static void CalculateObliqueMatrix(in float4x4 projection, in float4 clipPlane, out float4x4 obliqueMatrix)
	{
		CalculateObliqueMatrix_000017C0_0024BurstDirectCall.Invoke(in projection, in clipPlane, out obliqueMatrix);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Create_0024BurstManaged(in float3 pos, in quaternion rot, int cullingMask, out CameraData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		data.Position = pos;
		data.CameraToWorld = float4x4.TRS(pos, rot, new float3(1f, 1f, -1f));
		data.WorldToCamera = math.inverse(data.CameraToWorld);
		data.Forward = math.rotate(rot, new float3(0f, 0f, 1f));
		data.Up = math.rotate(rot, new float3(0f, 1f, 0f));
		data.cullingMask = cullingMask;
		data.CullingMatrix = default(float4x4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateObliqueMatrix_0024BurstManaged(in float4x4 projection, in float4 clipPlane, out float4x4 obliqueMatrix)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		float4 val = default(float4);
		((float4)(ref val))._002Ector((math.sign(clipPlane.x) + projection.c2.x) / projection.c0.x, (math.sign(clipPlane.y) + projection.c2.y) / projection.c1.y, -1f, (1f + projection.c2.z) / projection.c3.z);
		float4 val2 = clipPlane * (2f / math.dot(clipPlane, val));
		obliqueMatrix = projection;
		obliqueMatrix.c0.z = val2.x;
		obliqueMatrix.c1.z = val2.y;
		obliqueMatrix.c2.z = val2.z + 1f;
		obliqueMatrix.c3.z = val2.w;
	}
}
