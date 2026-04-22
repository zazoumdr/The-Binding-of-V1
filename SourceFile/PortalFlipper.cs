using UnityEngine;

public class PortalFlipper : MonoBehaviour
{
	public bool invert;

	private void Update()
	{
		if (Vector3.Dot(MonoSingleton<CameraController>.Instance.transform.position - base.transform.position, base.transform.forward) > 0f != invert)
		{
			base.transform.Rotate(Vector3.up * 180f);
		}
	}

	public void SetInvert(bool doInvert)
	{
		invert = doInvert;
	}
}
