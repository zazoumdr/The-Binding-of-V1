using Unity.Mathematics;

namespace ULTRAKILL.Portal.Native;

public static class PortalMath
{
	public static bool Raycast(in PortalRay ray, in float4x4 mat, in float2 dimensions, out float3 point, out float distance, bool allowBackface = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		distance = float.PositiveInfinity;
		point = default(float3);
		float4 val = mat.c3;
		float3 xyz = ((float4)(ref val)).xyz;
		val = mat.c2;
		float3 xyz2 = ((float4)(ref val)).xyz;
		float num = math.dot(xyz2, ray.direction);
		if (!allowBackface && num <= 0f)
		{
			return false;
		}
		float num2 = math.dot(ray.start - xyz, -xyz2);
		if (!allowBackface && num2 < 0f)
		{
			return false;
		}
		float num3 = num2 / num;
		if (num3 < 0f)
		{
			return false;
		}
		if (num3 * num3 > ray.distanceSq)
		{
			return false;
		}
		point = ray.start + ray.direction * num3;
		distance = num3;
		val = mat.c0;
		float3 xyz3 = ((float4)(ref val)).xyz;
		val = mat.c1;
		float3 xyz4 = ((float4)(ref val)).xyz;
		float3 val2 = point - xyz;
		float2 val3 = dimensions * 0.5f;
		bool result = true;
		if (math.abs(math.dot(val2, xyz3)) > val3.x || math.abs(math.dot(val2, xyz4)) > val3.y)
		{
			result = false;
		}
		return result;
	}
}
