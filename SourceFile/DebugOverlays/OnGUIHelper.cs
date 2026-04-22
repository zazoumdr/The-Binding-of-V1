using UnityEngine;

namespace DebugOverlays;

public static class OnGUIHelper
{
	public static Rect? GetOnScreenRect(Vector3 worldPosition, float width = 300f, float height = 100f)
	{
		Vector3 vector = MonoSingleton<CameraController>.Instance.cam.WorldToScreenPoint(worldPosition);
		if (vector.z < 0f)
		{
			return null;
		}
		return new Rect(vector.x - 50f, (float)Screen.height - vector.y - 50f, width, height);
	}
}
