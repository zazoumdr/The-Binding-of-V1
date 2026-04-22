using UnityEngine;

namespace ULTRAKILL.Portal;

public class DimensionalArena : MonoBehaviour
{
	[Header("Enter portal side")]
	[SerializeField]
	private Transform volumeA;

	[Header("Exit portal side")]
	[SerializeField]
	private Transform volumeB;

	[Space]
	[SerializeField]
	private Portal portal;

	private Bounds boundsA;

	private Bounds boundsB;

	private void Start()
	{
		boundsA = CalculateBounds(volumeA);
		boundsB = CalculateBounds(volumeB);
	}

	private Bounds CalculateBounds(Transform volume)
	{
		return new Bounds(volume.position, volume.lossyScale);
	}

	public bool TryGetPortalSide(Vector3 position, out PortalSide side)
	{
		if (boundsA.Contains(position))
		{
			side = PortalSide.Enter;
			return true;
		}
		if (boundsB.Contains(position))
		{
			side = PortalSide.Exit;
			return true;
		}
		side = (PortalSide)0;
		return false;
	}

	public Vector3 TransformPoint(Vector3 position, PortalSide fromSide)
	{
		return portal.GetTravelMatrix(fromSide).MultiplyPoint3x4(position);
	}

	private void OnDrawGizmos()
	{
		if (volumeA != null)
		{
			Gizmos.color = Color.red;
			Gizmos.matrix = volumeA.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		if (volumeB != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.matrix = volumeB.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}
}
