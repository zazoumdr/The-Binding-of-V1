using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace BeamHitInterpolation;

[BurstCompile]
public static class BeamHitInterpolator
{
	internal delegate void FindBestTimeAndMinDistSq_00002E0F_0024PostfixBurstDelegate(in float3 point, in float3 startA, in float3 endA, in float3 startB, in float3 endB, int iterations, out float bestTime, out float minDistSq);

	internal static class FindBestTimeAndMinDistSq_00002E0F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FindBestTimeAndMinDistSq_00002E0F_0024PostfixBurstDelegate).TypeHandle);
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

		static FindBestTimeAndMinDistSq_00002E0F_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in float3 point, in float3 startA, in float3 endA, in float3 startB, in float3 endB, int iterations, out float bestTime, out float minDistSq)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, ref float3, int, ref float, ref float, void>)functionPointer)(ref point, ref startA, ref endA, ref startB, ref endB, iterations, ref bestTime, ref minDistSq);
					return;
				}
			}
			FindBestTimeAndMinDistSq_0024BurstManaged(in point, in startA, in endA, in startB, in endB, iterations, out bestTime, out minDistSq);
		}
	}

	internal delegate float DistanceSqPointSegment_00002E10_0024PostfixBurstDelegate(in float3 point, in float3 segmentStart, in float3 segmentEnd);

	internal static class DistanceSqPointSegment_00002E10_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(DistanceSqPointSegment_00002E10_0024PostfixBurstDelegate).TypeHandle);
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

		static DistanceSqPointSegment_00002E10_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static float Invoke(in float3 point, in float3 segmentStart, in float3 segmentEnd)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float>)functionPointer)(ref point, ref segmentStart, ref segmentEnd);
				}
			}
			return DistanceSqPointSegment_0024BurstManaged(in point, in segmentStart, in segmentEnd);
		}
	}

	internal delegate void ClosestPointOnSegment_00002E11_0024PostfixBurstDelegate(in float3 point, in float3 segmentStart, in float3 segmentEnd, out float3 closest);

	internal static class ClosestPointOnSegment_00002E11_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(ClosestPointOnSegment_00002E11_0024PostfixBurstDelegate).TypeHandle);
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

		static ClosestPointOnSegment_00002E11_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in float3 point, in float3 segmentStart, in float3 segmentEnd, out float3 closest)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, void>)functionPointer)(ref point, ref segmentStart, ref segmentEnd, ref closest);
					return;
				}
			}
			ClosestPointOnSegment_0024BurstManaged(in point, in segmentStart, in segmentEnd, out closest);
		}
	}

	internal delegate void CalculateSweptObb_00002E12_0024PostfixBurstDelegate(in float3 prevOrigin, in float3 prevEnd, in float3 currOrigin, in float3 currEnd, in float radius, out float3 center, out float3 halfExtents, out quaternion orientation);

	internal static class CalculateSweptObb_00002E12_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalculateSweptObb_00002E12_0024PostfixBurstDelegate).TypeHandle);
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

		static CalculateSweptObb_00002E12_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in float3 prevOrigin, in float3 prevEnd, in float3 currOrigin, in float3 currEnd, in float radius, out float3 center, out float3 halfExtents, out quaternion orientation)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, ref float, ref float3, ref float3, ref quaternion, void>)functionPointer)(ref prevOrigin, ref prevEnd, ref currOrigin, ref currEnd, ref radius, ref center, ref halfExtents, ref orientation);
					return;
				}
			}
			CalculateSweptObb_0024BurstManaged(in prevOrigin, in prevEnd, in currOrigin, in currEnd, in radius, out center, out halfExtents, out orientation);
		}
	}

	private const float EpsilonSq = 1E-12f;

	private const float ConvergenceThresholdSq = 1E-06f;

	private const int MaxOverlapResults = 50;

	private const int TimeSearchIterations = 7;

	private const int RefinementIterations = 3;

	private const bool EnableDebugDrawing = false;

	private static readonly Color ObbColor = new Color(0.5f, 0.8f, 1f, 0.5f);

	private static readonly Color RayAColor = Color.red;

	private static readonly Color RayBColor = Color.green;

	public static void HitInterpolated(Ray rayA, float distanceA, Ray rayB, float distanceB, float beamRadius, LayerMask hitMask, List<InterpolatedHit> results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		float3 prevOrigin = float3.op_Implicit(rayA.origin);
		float3 val = float3.op_Implicit(rayA.direction);
		float3 prevEnd = prevOrigin + val * distanceA;
		float3 currOrigin = float3.op_Implicit(rayB.origin);
		float3 val2 = float3.op_Implicit(rayB.direction);
		float3 currEnd = currOrigin + val2 * distanceB;
		CalculateSweptObb(in prevOrigin, in prevEnd, in currOrigin, in currEnd, in beamRadius, out var center, out var halfExtents, out var orientation);
		Collider[] colliders = ArrayPool.GetColliders();
		int num = Physics.OverlapBoxNonAlloc(float3.op_Implicit(center), float3.op_Implicit(halfExtents), colliders, quaternion.op_Implicit(orientation), hitMask, queryTriggerInteraction);
		if (num == 0)
		{
			ArrayPool.ReturnColliders(colliders);
			return;
		}
		for (int i = 0; i < num; i++)
		{
			Collider collider = colliders[i];
			if (!(collider == null) && ValidateSweptHit(collider, prevOrigin, prevEnd, currOrigin, currEnd, beamRadius, out var finalPointOnCollider, out var finalClosestPointOnAxis, out var finalMinDistSq, out var _, out var _))
			{
				results.Add(CreateInterpolatedHit(collider, finalPointOnCollider, finalClosestPointOnAxis, finalMinDistSq));
			}
		}
		ArrayPool.ReturnColliders(colliders);
	}

	private static bool ValidateSweptHit(Collider collider, float3 prevOrigin, float3 prevEnd, float3 currOrigin, float3 currEnd, float radius, out float3 finalPointOnCollider, out float3 finalClosestPointOnAxis, out float finalMinDistSq, out float effectiveRadius, out float optimalTime)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		effectiveRadius = radius;
		optimalTime = 0.5f;
		if (collider is SphereCollider sphereCollider)
		{
			float3 point = float3.op_Implicit(sphereCollider.transform.TransformPoint(sphereCollider.center));
			float num2 = math.cmax(math.abs(float3.op_Implicit(sphereCollider.transform.lossyScale)));
			effectiveRadius = radius + sphereCollider.radius * num2;
			float num3 = effectiveRadius * effectiveRadius;
			FindBestTimeAndMinDistSq(in point, in prevOrigin, in prevEnd, in currOrigin, in currEnd, 7, out optimalTime, out finalMinDistSq);
			ClosestPointOnSegment(in point, math.lerp(prevOrigin, currOrigin, optimalTime), math.lerp(prevEnd, currEnd, optimalTime), out finalClosestPointOnAxis);
			finalPointOnCollider = point;
			return finalMinDistSq <= num3;
		}
		if (!(collider is MeshCollider { convex: false }))
		{
			float3 val = (prevOrigin + prevEnd + currOrigin + currEnd) * 0.25f;
			float3 point2 = float3.op_Implicit(collider.ClosestPoint(float3.op_Implicit(val)));
			float3 closest = float3.zero;
			finalMinDistSq = float.PositiveInfinity;
			for (int i = 0; i < 3; i++)
			{
				FindBestTimeAndMinDistSq(in point2, in prevOrigin, in prevEnd, in currOrigin, in currEnd, 7, out var bestTime, out var _);
				optimalTime = bestTime;
				ClosestPointOnSegment(in point2, math.lerp(prevOrigin, currOrigin, optimalTime), math.lerp(prevEnd, currEnd, optimalTime), out var closest2);
				closest = closest2;
				finalMinDistSq = math.distancesq(point2, closest);
				float3 val2 = float3.op_Implicit(collider.ClosestPoint(float3.op_Implicit(closest)));
				if (math.distancesq(val2, point2) < 1E-06f)
				{
					point2 = val2;
					ClosestPointOnSegment(in point2, math.lerp(prevOrigin, currOrigin, optimalTime), math.lerp(prevEnd, currEnd, optimalTime), out closest);
					finalMinDistSq = math.distancesq(point2, closest);
					break;
				}
				point2 = val2;
			}
			finalPointOnCollider = point2;
			finalClosestPointOnAxis = closest;
			return finalMinDistSq <= num;
		}
		finalPointOnCollider = float3.zero;
		finalClosestPointOnAxis = float3.zero;
		finalMinDistSq = float.PositiveInfinity;
		return false;
	}

	[BurstCompile]
	private static void FindBestTimeAndMinDistSq(in float3 point, in float3 startA, in float3 endA, in float3 startB, in float3 endB, int iterations, out float bestTime, out float minDistSq)
	{
		FindBestTimeAndMinDistSq_00002E0F_0024BurstDirectCall.Invoke(in point, in startA, in endA, in startB, in endB, iterations, out bestTime, out minDistSq);
	}

	[BurstCompile]
	private static float DistanceSqPointSegment(in float3 point, in float3 segmentStart, in float3 segmentEnd)
	{
		return DistanceSqPointSegment_00002E10_0024BurstDirectCall.Invoke(in point, in segmentStart, in segmentEnd);
	}

	[BurstCompile]
	private static void ClosestPointOnSegment(in float3 point, in float3 segmentStart, in float3 segmentEnd, out float3 closest)
	{
		ClosestPointOnSegment_00002E11_0024BurstDirectCall.Invoke(in point, in segmentStart, in segmentEnd, out closest);
	}

	[BurstCompile]
	private static void CalculateSweptObb(in float3 prevOrigin, in float3 prevEnd, in float3 currOrigin, in float3 currEnd, in float radius, out float3 center, out float3 halfExtents, out quaternion orientation)
	{
		CalculateSweptObb_00002E12_0024BurstDirectCall.Invoke(in prevOrigin, in prevEnd, in currOrigin, in currEnd, in radius, out center, out halfExtents, out orientation);
	}

	private static InterpolatedHit CreateInterpolatedHit(Collider collider, float3 finalPointOnCollider, float3 finalClosestPointOnAxis, float finalMinDistSq)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float3 val = float3.op_Implicit(collider.ClosestPoint(float3.op_Implicit(finalClosestPointOnAxis)));
		float3 val2 = finalClosestPointOnAxis - val;
		if (math.lengthsq(val2) < 1E-12f)
		{
			float3 val3 = float3.op_Implicit(collider.bounds.center);
			val2 = val - val3;
			if (math.lengthsq(val2) < 1E-12f)
			{
				val2 = float3.op_Implicit(collider.transform.up);
			}
		}
		val2 = math.normalize(val2);
		float distance = math.sqrt(finalMinDistSq);
		return new InterpolatedHit
		{
			point = float3.op_Implicit(val),
			normal = float3.op_Implicit(val2),
			distance = distance,
			collider = collider,
			transform = collider.transform,
			rigidbody = collider.attachedRigidbody
		};
	}

	[Conditional("UNITY_EDITOR")]
	private static void DrawObbDebug(float3 center, float3 halfExtents, quaternion orientation)
	{
	}

	[Conditional("UNITY_EDITOR")]
	private static void DrawRaysDebug(float3 originA, float3 endA, float3 originB, float3 endB)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FindBestTimeAndMinDistSq_0024BurstManaged(in float3 point, in float3 startA, in float3 endA, in float3 startB, in float3 endB, int iterations, out float bestTime, out float minDistSq)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 1f;
		bestTime = 0.5f;
		minDistSq = DistanceSqPointSegment(in point, math.lerp(startA, startB, bestTime), math.lerp(endA, endB, bestTime));
		float num3 = 1f / 3f;
		for (int i = 0; i < iterations; i++)
		{
			float num4 = num + (num2 - num) * num3;
			float num5 = num2 - (num2 - num) * num3;
			float num6 = DistanceSqPointSegment(in point, math.lerp(startA, startB, num4), math.lerp(endA, endB, num4));
			float num7 = DistanceSqPointSegment(in point, math.lerp(startA, startB, num5), math.lerp(endA, endB, num5));
			if (num6 < num7)
			{
				num2 = num5;
				if (num6 < minDistSq)
				{
					minDistSq = num6;
					bestTime = num4;
				}
			}
			else
			{
				num = num4;
				if (num7 < minDistSq)
				{
					minDistSq = num7;
					bestTime = num5;
				}
			}
		}
		float num8 = (num + num2) * 0.5f;
		float num9 = DistanceSqPointSegment(in point, math.lerp(startA, startB, num8), math.lerp(endA, endB, num8));
		if (num9 < minDistSq)
		{
			minDistSq = num9;
			bestTime = num8;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float DistanceSqPointSegment_0024BurstManaged(in float3 point, in float3 segmentStart, in float3 segmentEnd)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float3 val = segmentEnd - segmentStart;
		float num = math.lengthsq(val);
		if (num < 1E-12f)
		{
			return math.distancesq(point, segmentStart);
		}
		float num2 = math.dot(point - segmentStart, val) / num;
		num2 = math.clamp(num2, 0f, 1f);
		float3 val2 = segmentStart + num2 * val;
		return math.distancesq(point, val2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ClosestPointOnSegment_0024BurstManaged(in float3 point, in float3 segmentStart, in float3 segmentEnd, out float3 closest)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float3 val = segmentEnd - segmentStart;
		float num = math.lengthsq(val);
		if (num < 1E-12f)
		{
			closest = segmentStart;
			return;
		}
		float num2 = math.dot(point - segmentStart, val) / num;
		num2 = math.clamp(num2, 0f, 1f);
		closest = segmentStart + num2 * val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateSweptObb_0024BurstManaged(in float3 prevOrigin, in float3 prevEnd, in float3 currOrigin, in float3 currEnd, in float radius, out float3 center, out float3 halfExtents, out quaternion orientation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		center = (prevOrigin + prevEnd + currOrigin + currEnd) * 0.25f;
		float3 val = prevEnd - prevOrigin;
		float3 val2 = currEnd - currOrigin;
		float num = math.length(val);
		float num2 = math.length(val2);
		val = (float3)((num > 1E-12f) ? (val / num) : new float3(0f, 0f, 1f));
		val2 = ((num2 > 1E-12f) ? (val2 / num2) : val);
		float3 val3 = val + val2;
		if (math.lengthsq(val3) < 1E-12f)
		{
			val3 = currOrigin - prevOrigin;
			if (math.lengthsq(val3) < 1E-12f)
			{
				((float3)(ref val3))._002Ector(0f, 0f, 1f);
			}
		}
		val3 = math.normalize(val3);
		float3 val4 = (prevOrigin + prevEnd) * 0.5f;
		float3 val5 = (currOrigin + currEnd) * 0.5f - val4;
		float3 val6 = ((math.abs(math.dot(val3, new float3(0f, 1f, 0f))) > 0.999f) ? new float3(1f, 0f, 0f) : new float3(0f, 1f, 0f));
		float3 val7;
		if (math.lengthsq(val5) > 1E-12f)
		{
			val7 = val5 - math.dot(val5, val3) * val3;
			if (math.lengthsq(val7) < 1E-12f)
			{
				val7 = math.cross(val3, val6);
			}
		}
		else
		{
			val7 = math.cross(val3, val6);
		}
		val7 = math.normalize(val7);
		float3 val8 = math.normalize(math.cross(val7, val3));
		object span = (object)stackalloc float3[4] { prevOrigin, prevEnd, currOrigin, currEnd };
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = float.PositiveInfinity;
		float num6 = float.NegativeInfinity;
		float num7 = float.PositiveInfinity;
		float num8 = float.NegativeInfinity;
		Span<float3> span2 = (Span<float3>)span;
		for (int i = 0; i < span2.Length; i++)
		{
			float3 val9 = span2[i] - center;
			float num9 = math.dot(val9, val8);
			num3 = math.min(num3, num9);
			num4 = math.max(num4, num9);
			float num10 = math.dot(val9, val7);
			num5 = math.min(num5, num10);
			num6 = math.max(num6, num10);
			float num11 = math.dot(val9, val3);
			num7 = math.min(num7, num11);
			num8 = math.max(num8, num11);
		}
		halfExtents = new float3((num4 - num3) * 0.5f + radius, (num6 - num5) * 0.5f + radius, (num8 - num7) * 0.5f);
		orientation = quaternion.LookRotation(val3, val7);
	}
}
