using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal.Native;

[BurstCompile]
public struct NativePortalAudioData
{
	public AudioListenerMode listenerMode;

	public AudioVelocityUpdateMode audioVelocityUpdateMode;

	public Vector3 lastPosition;

	public Vector3 velocity;

	public float4x4 travelMatrix;

	public int updateIndex;
}
