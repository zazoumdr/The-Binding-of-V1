using ULTRAKILL.Portal.Native;
using UnityEngine;

namespace ULTRAKILL.Portal;

public struct ParticleHit
{
	public bool raycast;

	public int index;

	public NativePortalIntersection intersection;

	public RaycastHit hit;
}
