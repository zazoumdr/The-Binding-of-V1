using UnityEngine;
using UnityEngine.AI;

public class PortalNavMeshLink
{
	public readonly NavMeshLinkInstance instance;

	public readonly NavMeshLinkData data;

	public readonly Vector3 portalPos;

	public PortalNavMeshLink(in NavMeshLinkInstance instance, in NavMeshLinkData data, in Vector3 portalPos)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		this.instance = instance;
		this.data = data;
		this.portalPos = portalPos;
	}
}
