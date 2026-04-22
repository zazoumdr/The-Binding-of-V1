using UnityEngine;

public class SphereGizmo : MonoBehaviour
{
	public static bool Enabled = true;

	public float size = 0.1f;

	public Color color = Color.red;

	private void OnDrawGizmos()
	{
		if (Enabled)
		{
			Gizmos.color = color;
			Gizmos.DrawSphere(base.transform.position, size);
		}
	}
}
