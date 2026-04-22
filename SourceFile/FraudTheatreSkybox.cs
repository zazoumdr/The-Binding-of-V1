using UnityEngine;

[ExecuteInEditMode]
public class FraudTheatreSkybox : MonoBehaviour
{
	public bool lockMinimumHeight;

	public Transform playerStart;

	public Transform fakeCamStart;

	private RenderTexture skybox;

	private Camera fakeCam;

	public float speedScale = 0.5f;

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
		cc = MonoSingleton<CameraController>.Instance;
	}

	private void Update()
	{
		UpdateCamera();
		InitializeRT();
	}

	private void OnRenderObject()
	{
		UpdateCamera();
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

	private void UpdateCamera()
	{
		if (Application.isPlaying)
		{
			playerCam = cc.cam;
		}
		if (!(playerCam == null))
		{
			playerStartPos = playerStart.position;
			Vector3 vector = (playerCam.transform.position - playerStartPos) / 16f;
			vector *= speedScale;
			float y = (lockMinimumHeight ? Mathf.Max(vector.y, 0f) : vector.y);
			fakeCam.transform.position = fakeCamStart.position + new Vector3(vector.x, y, vector.z);
			fakeCam.transform.rotation = playerCam.transform.rotation;
			fakeCam.cullingMask = playerCam.cullingMask;
			fakeCam.fieldOfView = playerCam.fieldOfView;
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
		Shader.SetGlobalTexture("_VoidTex", skybox);
	}

	public void SetNewStartPos(Transform trans)
	{
		playerStart = trans;
	}
}
