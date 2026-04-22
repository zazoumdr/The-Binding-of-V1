using UnityEngine;

public class RenderCubemap : MonoBehaviour
{
	public bool manualRender;

	public Camera cam;

	public Cubemap cubemap;

	private void Update()
	{
		if (manualRender)
		{
			manualRender = false;
			if (cam != null && cubemap != null)
			{
				cam.RenderToCubemap(cubemap);
			}
			else
			{
				Debug.LogError("Camera and/or Cubemap not assigned.");
			}
		}
	}
}
