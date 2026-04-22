using UnityEngine;

public class OnCameraVisible : MonoBehaviour
{
	public UltrakillEvent onVisible;

	private Renderer rend;

	private bool isVisible;

	private bool forceFirstCheck = true;

	private Camera cam;

	private Plane[] frustumPlanes = new Plane[6];

	private void Start()
	{
		rend = GetComponent<Renderer>();
		if (rend != null && rend is SkinnedMeshRenderer skinnedMeshRenderer)
		{
			skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		cam = MonoSingleton<CameraController>.Instance.cam;
	}

	private void Update()
	{
		GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);
		bool flag = GeometryUtility.TestPlanesAABB(frustumPlanes, rend.bounds);
		if (flag != isVisible || forceFirstCheck)
		{
			forceFirstCheck = false;
			isVisible = flag;
			if (isVisible)
			{
				onVisible.Invoke();
			}
			else
			{
				onVisible.Revert();
			}
		}
	}
}
