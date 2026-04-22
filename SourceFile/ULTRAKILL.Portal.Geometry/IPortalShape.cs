using UnityEngine;

namespace ULTRAKILL.Portal.Geometry;

public interface IPortalShape
{
	bool IsValid { get; }

	float GetBoundingRadius();

	PortalMeshData GenerateMesh(PortalTransform trans);

	bool DidCross(Vector3 a, Vector3 b, out Vector3 intersection);

	void DrawDebug(PortalTransform trans, float duration = 0f, Color color = default(Color));
}
