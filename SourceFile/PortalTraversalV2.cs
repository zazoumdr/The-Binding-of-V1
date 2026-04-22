using ULTRAKILL.Portal;
using UnityEngine;

public struct PortalTraversalV2
{
	public Vector3 entrancePoint;

	public Vector3 entranceDirection;

	public Vector3 exitPoint;

	public Vector3 exitDirection;

	public readonly PortalHandle portalHandle;

	public Portal portalObject;

	public override string ToString()
	{
		return portalHandle.ToString();
	}

	public PortalTraversalV2(Vector3 entrance, Vector3 entranceDir, Vector3 exit, Vector3 exitDir, PortalHandle handle, Portal portal)
	{
		entrancePoint = entrance;
		entranceDirection = entranceDir;
		exitPoint = exit;
		exitDirection = exitDir;
		portalHandle = handle;
		portalObject = portal;
	}
}
