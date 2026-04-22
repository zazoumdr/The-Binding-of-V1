using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshExtensions
{
	public static VertexAttributeDescriptor GetVertexAttributeDescriptor(this Mesh.MeshData data, VertexAttribute attribute)
	{
		return new VertexAttributeDescriptor(attribute, data.GetVertexAttributeFormat(attribute), data.GetVertexAttributeDimension(attribute), data.GetVertexAttributeStream(attribute));
	}

	public static NativeSlice<T> GetVertexAttributeSlice<T>(this Mesh.MeshData data, VertexAttribute attribute) where T : struct
	{
		VertexAttributeDescriptor vertexAttributeDescriptor = data.GetVertexAttributeDescriptor(attribute);
		if (UnsafeUtility.SizeOf<T>() != VertexAttributeUtility.GetAttributeSize(vertexAttributeDescriptor))
		{
			ThrowExceptionForInvalidVertexAttributeSliceType<T>();
		}
		return InternalGetVertexAttributeSlice<T>(data, vertexAttributeDescriptor);
	}

	private unsafe static NativeSlice<T> InternalGetVertexAttributeSlice<T>(Mesh.MeshData data, VertexAttributeDescriptor format) where T : struct
	{
		int vertexAttributeOffset = data.GetVertexAttributeOffset(format.attribute);
		int vertexBufferStride = data.GetVertexBufferStride(format.stream);
		return NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>((byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data.GetVertexData<byte>(format.stream)) + vertexAttributeOffset, vertexBufferStride, data.vertexCount);
	}

	[DoesNotReturn]
	private static void ThrowExceptionForInvalidVertexAttributeSliceType<T>()
	{
		throw new InvalidOperationException($"{typeof(T)} and the vertex attribute are different sizes.");
	}
}
