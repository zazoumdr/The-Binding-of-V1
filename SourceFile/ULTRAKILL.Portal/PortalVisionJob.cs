using ULTRAKILL.Portal.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ULTRAKILL.Portal;

[BurstCompile]
public struct PortalVisionJob : IJobFor
{
	[ReadOnly]
	public NativeArray<PortalHandle> handles;

	[ReadOnly]
	public NativeArray<float4x4> transforms;

	[ReadOnly]
	public NativeArray<PortalVertices> verts;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<bool> visionPossible;

	public void Execute(int index)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		int length = handles.Length;
		int num = index * length;
		float4x4 val = transforms[index];
		PortalVertices portalVertices = verts[index];
		float4 c = val.c2;
		float4 c2 = val.c3;
		for (int i = 0; i < length; i++)
		{
			float4x4 val2 = transforms[i];
			float4 c3 = val2.c2;
			if (math.dot(c, c3) >= 0.99f)
			{
				visionPossible[num + i] = false;
				continue;
			}
			PortalVertices portalVertices2 = verts[i];
			float4 c4 = val2.c3;
			bool num2 = portalVertices2.IsBehindPlane(((float4)(ref c2)).xyz, -((float4)(ref c)).xyz);
			bool flag = portalVertices.IsBehindPlane(((float4)(ref c4)).xyz, -((float4)(ref c3)).xyz);
			if (num2 || flag)
			{
				visionPossible[num + i] = false;
			}
		}
	}
}
