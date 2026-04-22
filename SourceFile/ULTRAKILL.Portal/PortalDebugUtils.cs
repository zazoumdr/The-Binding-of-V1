using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

public static class PortalDebugUtils
{
	public static void DrawRaycast(Vector3 start, Vector3 end, PortalTraversalV2[] traversals, Color color, float duration)
	{
		for (int i = 0; i < traversals.Length; i++)
		{
			_ = traversals[i];
		}
	}

	public static void DrawPortalSequence(Vector3 start, PortalHandleSequence portalSeq, Color color, float duration)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (PortalHandle item in portalSeq)
		{
			float3.op_Implicit(PortalUtils.GetTransform(item, reverseSide: false).center);
			float3.op_Implicit(PortalUtils.GetTransform(item, reverseSide: true).center);
		}
	}
}
