using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public static class ArmatureUtility
{
	public static void ComputeLocalBounds(Mesh mesh, Span<Bounds> bounds)
	{
		ComputeLocalBounds(mesh, bounds, mesh.GetBindposes());
	}

	public static void ComputeLocalBounds(Mesh mesh, Span<Bounds> bounds, ReadOnlySpan<Matrix4x4> bindPoses)
	{
		if (mesh.indexFormat == IndexFormat.UInt16)
		{
			ComputeLocalBounds<ushort>(mesh, bounds, bindPoses);
		}
		else
		{
			ComputeLocalBounds<int>(mesh, bounds, bindPoses);
		}
	}

	private static void ComputeLocalBounds<TIndex>(Mesh mesh, Span<Bounds> bounds, ReadOnlySpan<Matrix4x4> bindPoses) where TIndex : unmanaged, IConvertible
	{
		using Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
		Mesh.MeshData data = meshDataArray[0];
		ReadOnlySpan<TIndex> triangles = data.GetIndexData<TIndex>();
		NativeArray<byte> source;
		if (data.GetVertexAttributeFormat(VertexAttribute.Position) != VertexAttributeFormat.Float32 || data.GetVertexAttributeDimension(VertexAttribute.Position) != 3)
		{
			using (NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory))
			{
				data.GetVertices(nativeArray);
				source = mesh.GetBonesPerVertex();
				ComputeLocalBounds(source, mesh.GetAllBoneWeights(), bindPoses, nativeArray.Slice(), triangles, bounds);
				return;
			}
		}
		NativeSlice<Vector3> vertexAttributeSlice = data.GetVertexAttributeSlice<Vector3>(VertexAttribute.Position);
		source = mesh.GetBonesPerVertex();
		ComputeLocalBounds(source, mesh.GetAllBoneWeights(), bindPoses, vertexAttributeSlice, triangles, bounds);
	}

	public static void ComputeLocalBounds(ReadOnlySpan<byte> bonesPerVertex, ReadOnlySpan<BoneWeight1> boneWeights, ReadOnlySpan<Matrix4x4> bindPoses, NativeSlice<Vector3> vertices, ReadOnlySpan<int> triangles, Span<Bounds> bounds)
	{
		ComputeLocalBounds(bonesPerVertex, boneWeights, bindPoses, vertices, triangles, bounds);
	}

	public static void ComputeLocalBounds(ReadOnlySpan<byte> bonesPerVertex, ReadOnlySpan<BoneWeight1> boneWeights, ReadOnlySpan<Matrix4x4> bindPoses, NativeSlice<Vector3> vertices, ReadOnlySpan<ushort> triangles, Span<Bounds> bounds)
	{
		ComputeLocalBounds(bonesPerVertex, boneWeights, bindPoses, vertices, triangles, bounds);
	}

	private static void ComputeLocalBounds<TIndex>(ReadOnlySpan<byte> bonesPerVertex, ReadOnlySpan<BoneWeight1> boneWeights, ReadOnlySpan<Matrix4x4> bindPoses, NativeSlice<Vector3> vertices, ReadOnlySpan<TIndex> triangles, Span<Bounds> bounds) where TIndex : unmanaged, IConvertible
	{
		NativeArray<int> nativeArray = new NativeArray<int>(vertices.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < bounds.Length; i++)
		{
			bounds[i].center = Vector3.positiveInfinity;
			bounds[i].extents = Vector3.negativeInfinity;
		}
		int j = 0;
		int num = 0;
		for (; j < vertices.Length; j++)
		{
			nativeArray[j] = num;
			num += bonesPerVertex[j];
		}
		int k = 0;
		for (int num2 = triangles.Length / 3; k < num2; k++)
		{
			for (int l = 0; l < 3; l++)
			{
				int index = triangles[3 * k + l].ToInt32(null);
				int num3 = nativeArray[index];
				int m = 0;
				for (int num4 = bonesPerVertex[index]; m < num4; m++)
				{
					int boneIndex = boneWeights[num3 + m].boneIndex;
					for (int n = 0; n < 3; n++)
					{
						Vector3 point = vertices[triangles[3 * k + n].ToInt32(null)];
						point = bindPoses[boneIndex].MultiplyPoint3x4(point);
						bounds[boneIndex].center = Vector3.Min(bounds[boneIndex].center, point);
						bounds[boneIndex].extents = Vector3.Max(bounds[boneIndex].extents, point);
					}
				}
			}
		}
		for (int num5 = 0; num5 < bounds.Length; num5++)
		{
			Vector3 center = bounds[num5].center;
			Vector3 extents = bounds[num5].extents;
			if (center.x == float.PositiveInfinity)
			{
				bounds[num5] = default(Bounds);
			}
			else
			{
				bounds[num5].SetMinMax(center, extents);
			}
		}
		nativeArray.Dispose();
	}
}
