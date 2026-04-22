using ULTRAKILL.Portal;
using UnityEngine;

public static class PortalTraversalExtensions
{
	public static Vector3 GetPositionInFront(this PortalTraversalV2 trav, float distance)
	{
		return trav.entrancePoint - trav.entranceDirection * distance;
	}

	public static bool AllHasFlag(this PortalTraversalV2[] arr, PortalTravellerFlags flag)
	{
		bool blocked;
		return arr.AllHasFlag(flag, out blocked);
	}

	public static bool AllHasFlag(this PortalTraversalV2[] arr, PortalTravellerFlags flag, out bool blocked)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			PortalTraversalV2 portalTraversalV = arr[i];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (!portalObject.GetTravelFlags(portalHandle.side).HasFlag(flag))
			{
				blocked = !portalObject.passThroughNonTraversals;
				return false;
			}
		}
		blocked = false;
		return true;
	}
}
