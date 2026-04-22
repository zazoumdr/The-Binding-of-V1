using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace ULTRAKILL.Portal.Native;

[BurstCompile]
public struct NativePortal
{
	public static readonly float4x4 ScaleMatrix = float4x4.Scale(new float3(-1f, 1f, -1f));

	public PortalHandle handle;

	public float2 dimensions;

	public NativePortalTransform transform;

	public float4x4 travelMatrix;

	public PortalVertices vertices;

	public Plane plane;

	public NativePortalRenderData renderData;

	public NativePortalAudioData audioData;

	public PortalTravellerFlags travellerFlags;

	[MarshalAs(UnmanagedType.U1)]
	public bool valid;

	public readonly Vector3 v0Managed => Unsafe.As<float3, Vector3>(ref Unsafe.AsRef(in vertices.v0));

	public readonly Vector3 v1Managed => Unsafe.As<float3, Vector3>(ref Unsafe.AsRef(in vertices.v1));

	public readonly Vector3 v2Managed => Unsafe.As<float3, Vector3>(ref Unsafe.AsRef(in vertices.v2));

	public readonly Vector3 v3Managed => Unsafe.As<float3, Vector3>(ref Unsafe.AsRef(in vertices.v3));

	public readonly Matrix4x4 travelMatrixManaged => Unsafe.As<float4x4, Matrix4x4>(ref Unsafe.AsRef(in travelMatrix));
}
