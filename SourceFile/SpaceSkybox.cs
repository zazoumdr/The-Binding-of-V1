using UnityEngine;

[ExecuteInEditMode]
public class SpaceSkybox : MonoBehaviour
{
	private RenderTexture skybox;

	private Camera fakeCam;

	private int lastWidth;

	private int lastHeight;

	private CameraController cc;

	private Camera playerCam;

	private void OnEnable()
	{
		fakeCam = GetComponent<Camera>();
		fakeCam.enabled = false;
		cc = MonoSingleton<CameraController>.Instance;
		PostProcessV2_Handler instance = MonoSingleton<PostProcessV2_Handler>.Instance;
		if (instance != null)
		{
			instance.AddSpaceSkybox(this);
		}
	}

	private void LateUpdate()
	{
		InitializeRT();
		if (!Application.isPlaying)
		{
			UpdateCamera(null);
		}
	}

	public void RenderSpaceSky(Camera cam)
	{
		UpdateCamera(cam);
	}

	private void UpdateCamera(Camera cam)
	{
		if (base.isActiveAndEnabled)
		{
			if (Application.isPlaying)
			{
				playerCam = cc.cam;
			}
			if (cam != null)
			{
				playerCam = cam;
			}
			if (!(playerCam == null))
			{
				fakeCam.transform.rotation = playerCam.transform.rotation;
				fakeCam.cullingMask = playerCam.cullingMask;
				fakeCam.fieldOfView = playerCam.fieldOfView;
				fakeCam.targetTexture = skybox;
				fakeCam.Render();
			}
		}
	}

	private void InitializeRT()
	{
		int width = Screen.width;
		int height = Screen.height;
		if (lastWidth == width && lastHeight == height)
		{
			return;
		}
		if ((bool)skybox)
		{
			fakeCam.targetTexture = null;
			skybox.Release();
			if (Application.isPlaying)
			{
				Object.Destroy(skybox);
			}
		}
		lastWidth = width;
		lastHeight = height;
		skybox = new RenderTexture(lastWidth, lastHeight, 24, RenderTextureFormat.ARGB32);
		fakeCam.targetTexture = skybox;
		Shader.SetGlobalTexture("_SpaceSky", skybox);
	}
}
