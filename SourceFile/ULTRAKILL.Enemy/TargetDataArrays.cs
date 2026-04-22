using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Enemy;

public struct TargetDataArrays : IDisposable
{
	[NoAlias]
	public NativeArray<float3> positions;

	[NoAlias]
	public NativeArray<float3> headPositions;

	[NoAlias]
	public NativeArray<float3> velocities;

	[NoAlias]
	public NativeArray<quaternion> rotations;

	[NoAlias]
	public NativeArray<TargetType> types;

	public void Set(int index, Vector3 position, Vector3 headPosition, Vector3 velocity, Quaternion rotation, TargetType type)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		positions[index] = float3.op_Implicit(position);
		headPositions[index] = float3.op_Implicit(headPosition);
		velocities[index] = float3.op_Implicit(velocity);
		rotations[index] = quaternion.op_Implicit(rotation);
		types[index] = type;
	}

	public void Allocate(int count, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		Allocator allocator = Allocator.Persistent;
		positions = new NativeArray<float3>(count, allocator, options);
		headPositions = new NativeArray<float3>(count, allocator, options);
		velocities = new NativeArray<float3>(count, allocator, options);
		rotations = new NativeArray<quaternion>(count, allocator, options);
		types = new NativeArray<TargetType>(count, allocator, options);
	}

	public void Dispose(JobHandle handle)
	{
		positions.Dispose(handle);
		headPositions.Dispose(handle);
		velocities.Dispose(handle);
		rotations.Dispose(handle);
		types.Dispose(handle);
	}

	public void Dispose()
	{
		if (positions.IsCreated)
		{
			positions.Dispose();
		}
		if (headPositions.IsCreated)
		{
			headPositions.Dispose();
		}
		if (velocities.IsCreated)
		{
			velocities.Dispose();
		}
		if (rotations.IsCreated)
		{
			rotations.Dispose();
		}
		if (types.IsCreated)
		{
			types.Dispose();
		}
	}

	public void CopyTo(ref TargetDataArrays other, int index)
	{
		int length = positions.Length;
		other.positions.Slice<float3>(index, length).CopyFrom(positions);
		other.headPositions.Slice<float3>(index, length).CopyFrom(headPositions);
		other.velocities.Slice<float3>(index, length).CopyFrom(velocities);
		other.rotations.Slice<quaternion>(index, length).CopyFrom(rotations);
		other.types.Slice(index, length).CopyFrom(types);
	}
}
