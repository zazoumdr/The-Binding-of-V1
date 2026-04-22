using ULTRAKILL.Portal;
using UnityEngine;

public struct MagnetInfo
{
	public Magnet magnet;

	public PortalHandleSequence sequence;

	public readonly Vector3 GetWorldPosition(PortalScene portalScene = null)
	{
		if (sequence.IsEmpty)
		{
			return magnet.transform.position;
		}
		if (portalScene != null)
		{
			return portalScene.GetTravelMatrix(in sequence).MultiplyPoint3x4(magnet.transform.position);
		}
		if (!MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			return magnet.transform.position;
		}
		portalScene = instance.Scene;
		return portalScene?.GetTravelMatrix(in sequence).MultiplyPoint3x4(magnet.transform.position) ?? magnet.transform.position;
	}
}
