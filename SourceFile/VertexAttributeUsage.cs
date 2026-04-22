using System;

[Flags]
public enum VertexAttributeUsage : uint
{
	None = 0u,
	Position = 1u,
	Normal = 2u,
	Tangent = 4u,
	Color = 8u,
	TexCoord0 = 0x10u,
	TexCoord1 = 0x20u,
	TexCoord2 = 0x40u,
	TexCoord3 = 0x80u,
	TexCoord4 = 0x100u,
	TexCoord5 = 0x200u,
	TexCoord6 = 0x400u,
	TexCoord7 = 0x800u,
	BlendWeight = 0x1000u,
	BlendIndices = 0x2000u,
	BoneWeights = 0x3000u
}
