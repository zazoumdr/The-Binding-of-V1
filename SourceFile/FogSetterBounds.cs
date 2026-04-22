using UnityEngine;

public class FogSetterBounds : MonoBehaviour
{
	private Collider col;

	private float fogMaxDistance;

	private Vector3 closestPoint;

	private void Start()
	{
		col = GetComponent<Collider>();
		fogMaxDistance = RenderSettings.fogEndDistance - RenderSettings.fogStartDistance;
	}

	private void Update()
	{
		closestPoint = col.ClosestPoint(MonoSingleton<CameraController>.Instance.transform.position);
		RenderSettings.fogStartDistance = Vector3.Distance(MonoSingleton<CameraController>.Instance.transform.position, closestPoint);
		RenderSettings.fogEndDistance = RenderSettings.fogStartDistance + fogMaxDistance;
	}

	public void ChangeDistance()
	{
		ChangeDistance(RenderSettings.fogEndDistance - RenderSettings.fogStartDistance);
	}

	public void ChangeDistance(float min, float max)
	{
		ChangeDistance(max - min);
	}

	public void ChangeDistance(float newDistance)
	{
		fogMaxDistance = newDistance;
	}
}
