using System;
using Unity.Mathematics;
using UnityEngine.Rendering;

public static class VertexAttributeUtility
{
	public const VertexAttribute MaxVertexAttribute = VertexAttribute.BlendIndices;

	public static int GetFormatSize(VertexAttributeFormat format)
	{
		return format switch
		{
			VertexAttributeFormat.Float32 => 4, 
			VertexAttributeFormat.Float16 => 2, 
			VertexAttributeFormat.UNorm8 => 1, 
			VertexAttributeFormat.SNorm8 => 1, 
			VertexAttributeFormat.UNorm16 => 2, 
			VertexAttributeFormat.SNorm16 => 2, 
			VertexAttributeFormat.UInt8 => 1, 
			VertexAttributeFormat.SInt8 => 1, 
			VertexAttributeFormat.UInt16 => 2, 
			VertexAttributeFormat.SInt16 => 2, 
			VertexAttributeFormat.UInt32 => 4, 
			VertexAttributeFormat.SInt32 => 4, 
			_ => -1, 
		};
	}

	public static int GetAttributeCount(VertexAttributeUsage usage)
	{
		return math.countbits((uint)usage);
	}

	public static int GetAttributeSize(VertexAttributeDescriptor attribute)
	{
		return GetFormatSize(attribute.format) * attribute.dimension;
	}

	public static VertexAttributeDescriptor GetAttributeDescriptor(VertexAttribute attribute, int stream)
	{
		switch (attribute)
		{
		case VertexAttribute.Position:
		case VertexAttribute.Normal:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.Float32, 3, stream);
		case VertexAttribute.Tangent:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.Float32, 4, stream);
		case VertexAttribute.Color:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.UNorm8, 4, stream);
		case VertexAttribute.TexCoord0:
		case VertexAttribute.TexCoord1:
		case VertexAttribute.TexCoord2:
		case VertexAttribute.TexCoord3:
		case VertexAttribute.TexCoord4:
		case VertexAttribute.TexCoord5:
		case VertexAttribute.TexCoord6:
		case VertexAttribute.TexCoord7:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.Float32, 2, stream);
		case VertexAttribute.BlendIndices:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.UInt8, 4, stream);
		case VertexAttribute.BlendWeight:
			return new VertexAttributeDescriptor(attribute, VertexAttributeFormat.UNorm8, 4, stream);
		default:
			throw new ArgumentOutOfRangeException("attribute");
		}
	}

	public static int GetVertexStreamForSkinning(VertexAttribute attribute)
	{
		switch (attribute)
		{
		case VertexAttribute.Position:
		case VertexAttribute.Normal:
		case VertexAttribute.Tangent:
			return 0;
		case VertexAttribute.Color:
		case VertexAttribute.TexCoord0:
		case VertexAttribute.TexCoord1:
		case VertexAttribute.TexCoord2:
		case VertexAttribute.TexCoord3:
		case VertexAttribute.TexCoord4:
		case VertexAttribute.TexCoord5:
		case VertexAttribute.TexCoord6:
		case VertexAttribute.TexCoord7:
			return 1;
		case VertexAttribute.BlendWeight:
		case VertexAttribute.BlendIndices:
			return 2;
		default:
			return -1;
		}
	}

	public static void UpdateVertexStreamsForSkinning(Span<VertexAttributeDescriptor> attributes)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			attributes[i].stream = GetVertexStreamForSkinning(attributes[i].attribute);
		}
	}

	public static void GetAttributesForSkinning(VertexAttributeUsage usage, Span<VertexAttributeDescriptor> attributes)
	{
		VertexAttribute vertexAttribute = VertexAttribute.Position;
		int num = 0;
		for (; vertexAttribute <= VertexAttribute.BlendIndices; vertexAttribute++)
		{
			if (((uint)usage & (uint)(1 << (int)vertexAttribute)) != 0)
			{
				attributes[num++] = GetAttributeDescriptor(vertexAttribute, GetVertexStreamForSkinning(vertexAttribute));
			}
		}
	}

	public static VertexAttributeDescriptor[] GetAttributesForSkinning(VertexAttributeUsage usage)
	{
		VertexAttributeDescriptor[] array = new VertexAttributeDescriptor[GetAttributeCount(usage)];
		GetAttributesForSkinning(usage, array);
		return array;
	}

	public static VertexAttributeFormat GetNormalizedFormat(VertexAttributeFormat format)
	{
		return format switch
		{
			VertexAttributeFormat.SInt8 => VertexAttributeFormat.SNorm8, 
			VertexAttributeFormat.UInt8 => VertexAttributeFormat.UNorm8, 
			VertexAttributeFormat.SInt16 => VertexAttributeFormat.SNorm16, 
			VertexAttributeFormat.UInt16 => VertexAttributeFormat.UNorm16, 
			_ => format, 
		};
	}
}
