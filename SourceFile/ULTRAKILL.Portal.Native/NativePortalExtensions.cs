using System;
using System.Runtime.CompilerServices;
using ULTRAKILL.Portal.Geometry;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;

namespace ULTRAKILL.Portal.Native;

[BurstCompile]
public static class NativePortalExtensions
{
	internal delegate void CalculateData_00002B15_0024PostfixBurstDelegate(ref NativePortal enter, ref NativePortal exit, in PortalHandle enterHandle, in PortalHandle exitHandle, in PlaneShape plane, in float3 enterPos, in quaternion enterRot, in float3 exitPos, in quaternion exitRot);

	internal static class CalculateData_00002B15_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalculateData_00002B15_0024PostfixBurstDelegate).TypeHandle);
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

		static CalculateData_00002B15_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativePortal enter, ref NativePortal exit, in PortalHandle enterHandle, in PortalHandle exitHandle, in PlaneShape plane, in float3 enterPos, in quaternion enterRot, in float3 exitPos, in quaternion exitRot)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativePortal, ref NativePortal, ref PortalHandle, ref PortalHandle, ref PlaneShape, ref float3, ref quaternion, ref float3, ref quaternion, void>)functionPointer)(ref enter, ref exit, ref enterHandle, ref exitHandle, ref plane, ref enterPos, ref enterRot, ref exitPos, ref exitRot);
					return;
				}
			}
			CalculateData_0024BurstManaged(ref enter, ref exit, in enterHandle, in exitHandle, in plane, in enterPos, in enterRot, in exitPos, in exitRot);
		}
	}

	internal delegate bool Raycast_00002B16_0024PostfixBurstDelegate(in NativePortal portal, in PortalRay ray, out NativePortalIntersection intersection, bool allowBackfaces = false);

	internal static class Raycast_00002B16_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(Raycast_00002B16_0024PostfixBurstDelegate).TypeHandle);
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

		static Raycast_00002B16_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static bool Invoke(in NativePortal portal, in PortalRay ray, out NativePortalIntersection intersection, bool allowBackfaces = false)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativePortal, ref PortalRay, ref NativePortalIntersection, bool, bool>)functionPointer)(ref portal, ref ray, ref intersection, allowBackfaces);
				}
			}
			return portal.Raycast_0024BurstManaged(in ray, out intersection, allowBackfaces);
		}
	}

	[BurstCompile]
	public static void CalculateData(ref NativePortal enter, ref NativePortal exit, in PortalHandle enterHandle, in PortalHandle exitHandle, in PlaneShape plane, in float3 enterPos, in quaternion enterRot, in float3 exitPos, in quaternion exitRot)
	{
		CalculateData_00002B15_0024BurstDirectCall.Invoke(ref enter, ref exit, in enterHandle, in exitHandle, in plane, in enterPos, in enterRot, in exitPos, in exitRot);
	}

	[BurstCompile]
	public static bool Raycast(this in NativePortal portal, in PortalRay ray, out NativePortalIntersection intersection, bool allowBackfaces = false)
	{
		return Raycast_00002B16_0024BurstDirectCall.Invoke(in portal, in ray, out intersection, allowBackfaces);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateData_0024BurstManaged(ref NativePortal enter, ref NativePortal exit, in PortalHandle enterHandle, in PortalHandle exitHandle, in PlaneShape plane, in float3 enterPos, in quaternion enterRot, in float3 exitPos, in quaternion exitRot)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		float2 val = default(float2);
		((float2)(ref val))._002Ector(plane.width, plane.height);
		float2 dim = val * 0.5f;
		float4x4 val2 = float4x4.TRS(enterPos, enterRot, float3.op_Implicit(1f));
		float4x4 val3 = math.fastinverse(val2);
		enter.handle = enterHandle;
		enter.valid = true;
		enter.dimensions = val;
		enter.transform = new NativePortalTransform
		{
			toWorld = val2,
			toLocal = val3
		};
		enter.vertices = new PortalVertices(val2, dim);
		enter.plane = new Plane(((float4)(ref val2.c2)).xyz, ((float4)(ref val2.c3)).xyz);
		float4x4 val4 = float4x4.TRS(exitPos, exitRot, float3.op_Implicit(1f));
		float4x4 val5 = math.fastinverse(val4);
		exit.handle = exitHandle;
		exit.valid = true;
		exit.dimensions = val;
		exit.transform = new NativePortalTransform
		{
			toWorld = val4,
			toLocal = val5
		};
		exit.vertices = new PortalVertices(val4, dim);
		exit.plane = new Plane(((float4)(ref val4.c2)).xyz, ((float4)(ref val4.c3)).xyz);
		float4x4 val6 = math.mul(val4, NativePortal.ScaleMatrix);
		float4x4 val7 = math.mul(val2, NativePortal.ScaleMatrix);
		enter.travelMatrix = math.mul(val6, val3);
		exit.travelMatrix = math.mul(val7, val5);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool Raycast_0024BurstManaged(this in NativePortal portal, in PortalRay ray, out NativePortalIntersection intersection, bool allowBackfaces = false)
	{
		intersection = default(NativePortalIntersection);
		intersection.handle = portal.handle;
		return PortalMath.Raycast(in ray, in portal.transform.toWorld, in portal.dimensions, out intersection.point, out intersection.distance, allowBackfaces);
	}
}
