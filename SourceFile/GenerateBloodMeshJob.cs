using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct GenerateBloodMeshJob : IJobParallelFor
{
	public struct VertexData
	{
		public float3 position;

		public half4 normal_Offset;

		public half2 uv;

		public float3 center;
	}

	[ReadOnly]
	public NativeArray<BloodsplatterManager.InstanceProperties> props;

	public Mesh.MeshData meshData;

	public bool isUInt16;

	public void Execute(int index)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<VertexData> vertexData = meshData.GetVertexData<VertexData>();
		float3 pos = props[index].pos;
		float3 norm = props[index].norm;
		float3 val = default(float3);
		float3 val2 = default(float3);
		math.orthonormal_basis(norm, ref val, ref val2);
		float4x4 val3 = float4x4.TRS(pos, math.mul(quaternion.LookRotation(norm, val), quaternion.RotateZ((float)(index % 359))), new float3(1.28f, 1.28f, 1f));
		float4 val4 = math.mul(val3, new float4(-1f, 1f, 0f, 1f));
		float4 val5 = math.mul(val3, new float4(1f, 1f, 0f, 1f));
		float4 val6 = math.mul(val3, new float4(1f, -1f, 0f, 1f));
		float4 val7 = math.mul(val3, new float4(-1f, -1f, 0f, 1f));
		int num = index * 4;
		int num2 = num + 1;
		int num3 = num + 2;
		int num4 = num + 3;
		half4 normal_Offset = (half4)new float4(norm, (float)index);
		half val8 = default(half);
		((half)(ref val8))._002Ector(0f);
		half val9 = default(half);
		((half)(ref val9))._002Ector(1f);
		vertexData[num] = new VertexData
		{
			position = ((float4)(ref val4)).xyz,
			normal_Offset = normal_Offset,
			uv = new half2(val8, val8),
			center = pos
		};
		vertexData[num2] = new VertexData
		{
			position = ((float4)(ref val5)).xyz,
			normal_Offset = normal_Offset,
			uv = new half2(val9, val8),
			center = pos
		};
		vertexData[num3] = new VertexData
		{
			position = ((float4)(ref val6)).xyz,
			normal_Offset = normal_Offset,
			uv = new half2(val9, val9),
			center = pos
		};
		vertexData[num4] = new VertexData
		{
			position = ((float4)(ref val7)).xyz,
			normal_Offset = normal_Offset,
			uv = new half2(val8, val9),
			center = pos
		};
		int num5 = index * 6;
		int index2 = num5 + 1;
		int index3 = num5 + 2;
		int index4 = num5 + 3;
		int index5 = num5 + 4;
		int index6 = num5 + 5;
		if (isUInt16)
		{
			NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
			indexData[num5] = Convert.ToUInt16(num);
			indexData[index2] = Convert.ToUInt16(num2);
			indexData[index3] = Convert.ToUInt16(num3);
			indexData[index4] = Convert.ToUInt16(num);
			indexData[index5] = Convert.ToUInt16(num3);
			indexData[index6] = Convert.ToUInt16(num4);
		}
		else
		{
			NativeArray<uint> indexData2 = meshData.GetIndexData<uint>();
			indexData2[num5] = Convert.ToUInt32(num);
			indexData2[index2] = Convert.ToUInt32(num2);
			indexData2[index3] = Convert.ToUInt32(num3);
			indexData2[index4] = Convert.ToUInt32(num);
			indexData2[index5] = Convert.ToUInt32(num3);
			indexData2[index6] = Convert.ToUInt32(num4);
		}
	}
}
