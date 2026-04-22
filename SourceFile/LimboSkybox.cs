using UnityEngine;

[ExecuteInEditMode]
public class LimboSkybox : MonoBehaviour
{
	public bool lockMinimumHeight = true;

	public float downscaleFactor = 2f;

	public Transform playerStart;

	public Transform fakeCamStart;

	private RenderTexture skybox;

	private Camera fakeCam;

	private int lastWidth;

	private int lastHeight;

	private CameraController cc;

	private Camera playerCam;

	private Vector3 playerStartPos;

	[Header("Fog Settings")]
	public bool useFogOverrides;

	public bool overrideFogDistance = true;

	public float fogStart;

	public float fogEnd;

	public bool overrideFogColor = true;

	public Color fogColor = Color.black;

	private float oldFogStart;

	private float oldFogEnd;

	private Color oldFogColor;

	private void OnEnable()
	{
		fakeCam = GetComponent<Camera>();
		playerStartPos = playerStart.position;
		cc = MonoSingleton<CameraController>.Instance;
		fakeCam.enabled = false;
		PostProcessV2_Handler instance = MonoSingleton<PostProcessV2_Handler>.Instance;
		if (instance != null)
		{
			instance.AddLimboSkybox(this);
		}
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			UpdateCamera(null);
		}
	}

	internal void RenderLimboSkybox(Camera cam)
	{
		if (base.isActiveAndEnabled)
		{
			UpdateCamera(cam);
		}
	}

	private void OnPreRender()
	{
		if (!RenderSettings.fog)
		{
			return;
		}
		oldFogStart = RenderSettings.fogStartDistance;
		oldFogEnd = RenderSettings.fogEndDistance;
		oldFogColor = RenderSettings.fogColor;
		if (useFogOverrides)
		{
			if (overrideFogDistance)
			{
				RenderSettings.fogStartDistance = fogStart;
				RenderSettings.fogEndDistance = fogEnd;
			}
			if (overrideFogColor)
			{
				RenderSettings.fogColor = fogColor;
			}
		}
	}

	private void OnPostRender()
	{
		if (RenderSettings.fog)
		{
			RenderSettings.fogStartDistance = oldFogStart;
			RenderSettings.fogEndDistance = oldFogEnd;
			RenderSettings.fogColor = oldFogColor;
		}
	}

	private void UpdateCamera(Camera cam)
	{
		if (base.isActiveAndEnabled)
		{
			InitializeRT();
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
				Vector3 vector = (playerCam.transform.position - playerStartPos) / 16f;
				float y = (lockMinimumHeight ? Mathf.Max(vector.y, 0f) : vector.y);
				fakeCam.transform.position = fakeCamStart.position + new Vector3(vector.x, y, vector.z);
				fakeCam.transform.rotation = playerCam.transform.rotation;
				fakeCam.cullingMask = playerCam.cullingMask;
				fakeCam.fieldOfView = playerCam.fieldOfView;
				fakeCam.targetTexture = skybox;
				Shader.SetGlobalTexture("_LimboSky", skybox);
				Shader.SetGlobalFloat("_LimboSkyWidth", lastWidth);
				Shader.SetGlobalFloat("_LimboSkyHeight", lastHeight);
				fakeCam.Render();
			}
		}
	}

	private void InitializeRT()
	{
		int num = (int)((float)Screen.width / downscaleFactor);
		int num2 = (int)((float)Screen.height / downscaleFactor);
		if (lastWidth == num && lastHeight == num2)
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
		skybox = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32);
		lastWidth = num;
		lastHeight = num2;
		fakeCam.targetTexture = skybox;
	}
}
