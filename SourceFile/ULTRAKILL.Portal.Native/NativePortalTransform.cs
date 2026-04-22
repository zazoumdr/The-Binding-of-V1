using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal.Native;

public struct NativePortalTransform
{
	public float4x4 toWorld;

	public float4x4 toLocal;

	public readonly Matrix4x4 toWorldManaged => Unsafe.As<float4x4, Matrix4x4>(ref Unsafe.AsRef(in toWorld));

	public readonly Matrix4x4 toLocalManaged => Unsafe.As<float4x4, Matrix4x4>(ref Unsafe.AsRef(in toLocal));

	public readonly float3 right
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c0;
			return ((float4)(ref c)).xyz;
		}
	}

	public readonly float3 left
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c0;
			return -((float4)(ref c)).xyz;
		}
	}

	public readonly float3 up
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c1;
			return ((float4)(ref c)).xyz;
		}
	}

	public readonly float3 down
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c1;
			return -((float4)(ref c)).xyz;
		}
	}

	public readonly float3 forward
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c2;
			return ((float4)(ref c)).xyz;
		}
	}

	public readonly float3 back
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c2;
			return -((float4)(ref c)).xyz;
		}
	}

	public readonly float3 center
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c3;
			return ((float4)(ref c)).xyz;
		}
	}

	public readonly Vector3 rightManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c0;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 leftManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c0;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(-((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 upManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c1;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 downManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c1;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(-((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 forwardManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c2;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 backManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c2;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(-((float4)(ref c)).xyz));
		}
	}

	public readonly Vector3 centerManaged
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			float4 c = toWorld.c3;
			return Unsafe.As<float3, Vector3>(ref Unsafe.AsRef<float3>(((float4)(ref c)).xyz));
		}
	}

	[BurstCompile]
	public Vector3 WorldToLocal(in float3 world)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return float3.op_Implicit(math.transform(toLocal, world));
	}

	[BurstCompile]
	public Vector3 LocalToWorld(in float3 local)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return float3.op_Implicit(math.transform(toWorld, local));
	}
}
