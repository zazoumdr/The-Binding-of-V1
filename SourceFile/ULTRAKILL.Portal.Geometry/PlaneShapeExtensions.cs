using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace ULTRAKILL.Portal.Geometry;

[BurstCompile]
public static class PlaneShapeExtensions
{
	internal delegate void GetClosestPoint_00002B34_0024PostfixBurstDelegate(float width, float height, in float3 center, in float3 right, in float3 up, in float3 forward, in float3 point, out float3 closest);

	internal static class GetClosestPoint_00002B34_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(GetClosestPoint_00002B34_0024PostfixBurstDelegate).TypeHandle);
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

		static GetClosestPoint_00002B34_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(float width, float height, in float3 center, in float3 right, in float3 up, in float3 forward, in float3 point, out float3 closest)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, ref float3, ref float3, ref float3, ref float3, ref float3, ref float3, void>)functionPointer)(width, height, ref center, ref right, ref up, ref forward, ref point, ref closest);
					return;
				}
			}
			GetClosestPoint_0024BurstManaged(width, height, in center, in right, in up, in forward, in point, out closest);
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	public static void GetClosestPoint(float width, float height, in float3 center, in float3 right, in float3 up, in float3 forward, in float3 point, out float3 closest)
	{
		GetClosestPoint_00002B34_0024BurstDirectCall.Invoke(width, height, in center, in right, in up, in forward, in point, out closest);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal static void GetClosestPoint_0024BurstManaged(float width, float height, in float3 center, in float3 right, in float3 up, in float3 forward, in float3 point, out float3 closest)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		float3 val = point - center;
		float num = math.dot(val, right);
		float num2 = math.dot(val, up);
		float num3 = width * 0.5f;
		float num4 = height * 0.5f;
		float num5 = math.clamp(num, 0f - num3, num3);
		float num6 = math.clamp(num2, 0f - num4, num4);
		closest = center + right * num5 + up * num6;
	}
}
