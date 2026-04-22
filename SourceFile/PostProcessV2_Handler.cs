using System;
using System.Collections.Generic;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.Rendering;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PostProcessV2_Handler : MonoSingleton<PostProcessV2_Handler>
{
	public Material postProcessV2_VSRM;

	public Material screenNormal;

	public Material heatWaveMat;

	private CommandBuffer heatCB;

	public Shader outlinePx;

	public Shader outlinePx_SimpleTest;

	private Material outlineMat;

	private CommandBuffer outlineCB;

	[Space(10f)]
	public bool useHeightFog;

	public Texture oilTex;

	public Texture sandTex;

	public Texture buffTex;

	public Texture ditherTexture;

	public Texture vignetteTexture;

	public int distance = 1;

	public Camera mainCam;

	public Camera hudCam;

	public Camera virtualCam;

	public RenderBuffer[] buffers = new RenderBuffer[3];

	public RenderTexture mainTex;

	public RenderTexture reusableBufferA;

	[HideInInspector]
	public RenderTexture reusableBufferB;

	public RenderTexture depthBuffer;

	public RenderTexture viewNormal;

	public RenderTexture portalMask;

	private int width;

	private int height;

	private int lastWidth;

	private int lastHeight;

	private bool reinitializeTextures;

	private bool mainCameraOnly;

	[HideInInspector]
	public float downscaleResolution;

	public Texture CurrentTexture;

	public Texture CurrentMapPaletteOverride;

	public Material radiantBuff;

	private OptionsManager oman;

	public bool debugFooled;

	[SerializeField]
	private ComputeShader paletteCompute;

	[SerializeField]
	private Shader paletteCalc;

	private bool isGLCore;

	private float realDist;

	public Action<bool> onReinitialize;

	public bool usedComputeShadersAtStart = true;

	private CommandBuffer bloodOilCB;

	private CommandBuffer mainPostProcess;

	private Material outlinePx_SimpleTestMat;

	private static Matrix4x4 identityMatrix = Matrix4x4.identity;

	private SpaceSkybox spaceSky;

	private List<LimboSkybox> limboSkyboxes = new List<LimboSkybox>();

	private CommandBuffer clearOutlines;

	private CommandBuffer resetClipPlane;

	private void OnValidate()
	{
		Fooled(debugFooled);
		Shader.SetGlobalFloat("_HeightFog", useHeightFog ? 1 : 0);
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
		Shader.SetGlobalFloat("_HeightFog", useHeightFog ? 1 : 0);
		spaceSky = UnityEngine.Object.FindAnyObjectByType<SpaceSkybox>(FindObjectsInactive.Include);
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void LateUpdate()
	{
		if (!mainCam)
		{
			mainCam = MonoSingleton<CameraController>.Instance.cam;
		}
		if ((bool)mainCam && mainTex == null)
		{
			SetupRTs();
		}
	}

	private void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "outlineThickness":
			if (value is int num)
			{
				distance = num;
				SetupOutlines();
			}
			break;
		case "pixelization":
			if (value is int pixelization)
			{
				SetPixelization(pixelization);
			}
			break;
		case "simplifyEnemies":
			if (value is int)
			{
				if ((int)value == 0)
				{
					SetupOutlines(forceOnePixelOutline: true);
				}
				else
				{
					SetupOutlines();
				}
			}
			break;
		}
	}

	private void SetPixelization(int option)
	{
		float pixelizationValue = SettingsMenu.Components.Pages.GraphicsSettings.GetPixelizationValue(option);
		Shader.SetGlobalFloat("_ResY", pixelizationValue);
		downscaleResolution = pixelizationValue;
	}

	private void Start()
	{
		usedComputeShadersAtStart = !SettingsMenu.Components.Pages.GraphicsSettings.disabledComputeShaders;
		Vignette(doVignette: false);
		postProcessV2_VSRM.DisableKeyword("UNDERWATER");
		DeathEffect(isDead: false);
		Shader.SetGlobalFloat("_Sharpness", 0f);
		Shader.SetGlobalFloat("_Deathness", 0f);
		WickedEffect(doWicked: false);
		Shader.SetGlobalFloat("_RandomNoiseStrength", 0f);
		DateTime now = DateTime.Now;
		bool flag = now.Month == 4 && now.Day == 1;
		flag |= debugFooled;
		Fooled(flag);
		isGLCore = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		if ((bool)mainCam)
		{
			hudCam = mainCam.transform.Find("HUD Camera").GetComponent<Camera>();
			virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		}
		ReinitializeCameras();
		postProcessV2_VSRM.SetTexture("_Dither", ditherTexture);
		postProcessV2_VSRM.SetTexture("_VignetteTex", vignetteTexture);
		if (oilTex != null)
		{
			Shader.SetGlobalTexture("_OilSlick", oilTex);
		}
		if (sandTex != null)
		{
			Shader.SetGlobalTexture("_SandTex", sandTex);
		}
		if (buffTex != null)
		{
			Shader.SetGlobalTexture("_BuffTex", buffTex);
		}
		oman = MonoSingleton<OptionsManager>.Instance;
		distance = MonoSingleton<PrefsManager>.Instance.GetInt("outlineThickness");
		int pixelization = MonoSingleton<PrefsManager>.Instance.GetInt("pixelization");
		SetPixelization(pixelization);
	}

	public void DeathEffect(bool isDead)
	{
		if (isDead)
		{
			postProcessV2_VSRM.EnableKeyword("DEAD");
		}
		else
		{
			postProcessV2_VSRM.DisableKeyword("DEAD");
		}
	}

	public void WickedEffect(bool doWicked)
	{
		if (doWicked)
		{
			postProcessV2_VSRM.EnableKeyword("WICKED");
		}
		else
		{
			postProcessV2_VSRM.DisableKeyword("WICKED");
		}
	}

	public void Vignette(bool doVignette)
	{
		if (doVignette)
		{
			postProcessV2_VSRM.EnableKeyword("VIGNETTE");
		}
		else
		{
			postProcessV2_VSRM.DisableKeyword("VIGNETTE");
		}
	}

	public void Fooled(bool doEnable)
	{
		if (doEnable)
		{
			Shader.EnableKeyword("Fooled");
			Shader.EnableKeyword("FOOLED");
		}
		else
		{
			Shader.DisableKeyword("Fooled");
			Shader.DisableKeyword("FOOLED");
		}
	}

	public void ColorPalette(bool stuff)
	{
		if (!(CurrentMapPaletteOverride != null))
		{
			if (stuff && CurrentTexture != null)
			{
				Shader.EnableKeyword("PALETTIZE");
				Shader.SetGlobalInt("_ColorPrecision", 2048);
				MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)CurrentTexture, paletteCompute, paletteCalc);
			}
			else
			{
				Shader.DisableKeyword("PALETTIZE");
			}
		}
	}

	public void ApplyUserColorPalette(Texture tex)
	{
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"))
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal("colorPalette", content: true);
		}
		CurrentTexture = tex;
		if (!(CurrentMapPaletteOverride != null))
		{
			MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, paletteCompute, paletteCalc);
		}
	}

	public void ApplyMapColorPalette(Texture tex)
	{
		if (tex == null)
		{
			CurrentMapPaletteOverride = null;
			ColorPalette(MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"));
			return;
		}
		MonoSingleton<ConvertPaletteToLUT>.Instance.ConvertPalette((Texture2D)tex, paletteCompute, paletteCalc);
		CurrentMapPaletteOverride = tex;
		Shader.SetGlobalTexture("_PaletteTex", tex);
		Shader.EnableKeyword("PALETTIZE");
		Shader.SetGlobalInt("_ColorPrecision", 2048);
	}

	private void ReinitializeCameras()
	{
		if (Application.isPlaying)
		{
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
			PortalManagerV2 portalManagerV = MonoSingleton<PortalManagerV2>.Instance;
			if (portalManagerV != null)
			{
				portalManagerV.InitCam();
			}
		}
	}

	private void SetupRTs()
	{
		width = Screen.width;
		height = Screen.height;
		if (downscaleResolution != 0f)
		{
			float num = width;
			float num2 = height;
			float num3 = Mathf.Min(num, num2);
			Vector2 vector = new Vector2(num / num3, num2 / num3) * downscaleResolution;
			width = (int)vector.x;
			height = (int)vector.y;
		}
		bool flag = width != lastWidth || height != lastHeight;
		reinitializeTextures |= flag;
		lastWidth = width;
		lastHeight = height;
		Vector2 vector2 = new Vector2(width, height);
		postProcessV2_VSRM.SetVector("_VirtualRes", vector2);
		if (reinitializeTextures)
		{
			MonoSingleton<OptionsManager>.Instance.SetSimplifyEnemies(MonoSingleton<PrefsManager>.Instance.GetInt("simplifyEnemies"));
			if (mainCam == null)
			{
				mainCam = MonoSingleton<CameraController>.Instance.cam;
				hudCam = mainCam.transform.Find("HUD Camera").GetComponent<Camera>();
				virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
			}
			mainCam.targetTexture = null;
			hudCam.targetTexture = null;
			ReleaseTextures();
			float num4 = Mathf.Max(width, height);
			Shader.SetGlobalVector("_ScreenRatio", new Vector2((float)width / num4, (float)height / num4));
			mainTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
			{
				name = "Main",
				antiAliasing = 1,
				filterMode = FilterMode.Point
			};
			depthBuffer = new RenderTexture(width, height, 32, RenderTextureFormat.Depth)
			{
				name = "Depth Buffer",
				antiAliasing = 1,
				filterMode = FilterMode.Point
			};
			reusableBufferA = new RenderTexture(width, height, 0, RenderTextureFormat.RG16)
			{
				name = "Reusable A",
				antiAliasing = 1,
				filterMode = FilterMode.Point
			};
			reusableBufferB = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
			{
				name = "Reusable B",
				antiAliasing = 1,
				filterMode = FilterMode.Point
			};
			viewNormal = new RenderTexture(width, height, 0, RenderTextureFormat.RGB565)
			{
				name = "View Normal",
				antiAliasing = 1,
				filterMode = FilterMode.Point
			};
			buffers[0] = mainTex.colorBuffer;
			buffers[1] = reusableBufferA.colorBuffer;
			buffers[2] = viewNormal.colorBuffer;
			mainCam.SetTargetBuffers(buffers, depthBuffer.depthBuffer);
			mainCam.RemoveCommandBuffers(CameraEvent.AfterForwardAlpha);
			SetupOutlines();
			hudCam.SetTargetBuffers(mainTex.colorBuffer, depthBuffer.depthBuffer);
			postProcessV2_VSRM.SetTexture("_MainTex", mainTex);
			reinitializeTextures = false;
			onReinitialize?.Invoke(obj: true);
		}
	}

	public static Vector4 GetProjectionParams(Camera cam)
	{
		return new Vector4(-1f, cam.nearClipPlane, cam.farClipPlane, 1f / cam.farClipPlane);
	}

	public static Vector4 ZBufferParams(Camera cam)
	{
		float num = cam.farClipPlane / cam.nearClipPlane;
		float num2;
		float num3;
		if (SystemInfo.usesReversedZBuffer)
		{
			num2 = -1f + num;
			num3 = 1f;
		}
		else
		{
			num2 = 1f - num;
			num3 = num;
		}
		return new Vector4(num2, num3, num2 / cam.farClipPlane, num3 / cam.farClipPlane);
	}

	private void ReleaseTextures()
	{
		TryDestroyTexture(mainTex);
		TryDestroyTexture(reusableBufferA);
		TryDestroyTexture(reusableBufferB);
		TryDestroyTexture(depthBuffer);
		TryDestroyTexture(viewNormal);
	}

	public void HeatWaves()
	{
		if (heatCB == null)
		{
			heatCB = new CommandBuffer();
			heatCB.name = "HeatWaves";
		}
		heatCB.Clear();
		heatCB.SetRenderTarget((Texture)null, (Texture)null);
		heatCB.SetGlobalTexture("_Stencil", depthBuffer, RenderTextureSubElement.Stencil);
		heatCB.CopyTexture(mainTex, reusableBufferB);
		heatCB.Blit(reusableBufferB, mainTex, heatWaveMat, 0);
		mainCam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, heatCB);
	}

	public void SetupOutlines(bool forceOnePixelOutline = false)
	{
		Shader.SetGlobalTexture("_OutlineTex", reusableBufferA);
		distance = MonoSingleton<PrefsManager>.Instance.GetInt("outlineThickness");
		if (mainCam == null)
		{
			SetupRTs();
		}
		else if (mainTex == null)
		{
			SetupRTs();
		}
		else
		{
			if (isGLCore)
			{
				return;
			}
			if (outlineCB == null)
			{
				outlineCB = new CommandBuffer();
				outlineCB.name = "Outlines";
			}
			Vector2 vector = new Vector2(mainTex.width, mainTex.height);
			Vector2 vector2 = vector / new Vector2(Screen.width, Screen.height);
			float num = distance;
			if (distance > 1)
			{
				num = (float)distance * Mathf.Max(vector2.x, vector2.y);
			}
			outlineCB.Clear();
			if (outlineMat == null)
			{
				outlineMat = new Material(outlinePx);
			}
			mainCam.RemoveCommandBuffer(CameraEvent.AfterEverything, outlineCB);
			outlineCB.SetGlobalVector("_Resolution", vector);
			outlineCB.SetGlobalVector("_ResolutionDiff", vector2);
			if (!forceOnePixelOutline && distance > 1 && num > 1f && oman.simplifyEnemies)
			{
				distance = Mathf.Min(distance, 16);
				outlineCB.Blit(reusableBufferA, reusableBufferB, outlineMat, 0);
				float num2 = 8f;
				int num3 = 0;
				while (num2 >= 0.5f || reusableBufferA.name == "Reusable B")
				{
					outlineCB.SetGlobalFloat("_TestDistance", num2);
					outlineCB.Blit(reusableBufferB, reusableBufferA, outlineMat, 1);
					RenderTexture renderTexture = reusableBufferB;
					RenderTexture renderTexture2 = reusableBufferA;
					reusableBufferA = renderTexture;
					reusableBufferB = renderTexture2;
					num2 *= 0.5f;
					num3++;
				}
				outlineCB.SetGlobalFloat("_OutlineDistance", distance);
				outlineCB.SetGlobalFloat("_TestDistance", num2);
				outlineCB.Blit(reusableBufferB, mainTex, outlineMat, 2);
				outlineCB.SetRenderTarget(reusableBufferB);
				outlineCB.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
			}
			else
			{
				outlineCB.SetRenderTarget(mainTex);
				outlineCB.SetGlobalTexture("_OutlineTex", reusableBufferA);
				outlineCB.Blit(reusableBufferA, mainTex, outlineMat, 3);
			}
			mainCam.AddCommandBuffer(CameraEvent.AfterEverything, outlineCB);
		}
	}

	public void ChangeCamera(bool hudless)
	{
		mainCameraOnly = hudless;
		mainCam.targetTexture = null;
		MonoSingleton<CameraController>.Instance.cam.clearFlags = mainCam.clearFlags;
		mainCam = MonoSingleton<CameraController>.Instance.cam;
		virtualCam = mainCam.transform.Find("Virtual Camera").GetComponent<Camera>();
		reinitializeTextures = true;
		SetupRTs();
	}

	public void OnPreRenderCallback(Camera cam)
	{
		if (cam == mainCam || cam == hudCam || cam == virtualCam)
		{
			if (resetClipPlane == null)
			{
				resetClipPlane = new CommandBuffer();
				resetClipPlane.name = "Reset Clip Plane";
			}
			resetClipPlane.Clear();
			resetClipPlane.SetGlobalVector("_PortalClipPlane", new Vector4(0f, 0f, 0f, 0f));
			cam.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, resetClipPlane);
			cam.AddCommandBuffer(CameraEvent.BeforeDepthTexture, resetClipPlane);
		}
		if (cam == mainCam)
		{
			SetupRTs();
			if (mainCam != null)
			{
				if (clearOutlines == null)
				{
					clearOutlines = new CommandBuffer();
					clearOutlines.name = "Clear Outlines";
				}
				clearOutlines.Clear();
				clearOutlines.SetRenderTarget(reusableBufferA.colorBuffer);
				clearOutlines.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
				Graphics.ExecuteCommandBuffer(clearOutlines);
				if (bloodOilCB == null)
				{
					bloodOilCB = new CommandBuffer();
					bloodOilCB.name = "Blood and Oil";
					mainCam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, bloodOilCB);
				}
				bloodOilCB.Clear();
				BloodsplatterManager bloodsplatterManager = MonoSingleton<BloodsplatterManager>.Instance;
				StainVoxelManager stainVoxelManager = MonoSingleton<StainVoxelManager>.Instance;
				if (usedComputeShadersAtStart)
				{
					bloodsplatterManager.stainMat.SetBuffer("instanceBuffer", bloodsplatterManager.instanceBuffer);
					bloodsplatterManager.stainMat.SetBuffer("parentBuffer", bloodsplatterManager.parentBuffer);
					stainVoxelManager.gasStainMat.SetBuffer("instanceBuffer", stainVoxelManager.propBuffer);
					stainVoxelManager.gasStainMat.SetBuffer("stainMatrices", stainVoxelManager.stainBuffer);
				}
				Matrix4x4 inverseVP_NonOblique = GetInverseVP_NonOblique(mainCam);
				bloodOilCB.SetGlobalVector("_ProjectionParams_NonOblique", GetProjectionParams(mainCam));
				bloodOilCB.SetGlobalMatrix("_InverseVP", inverseVP_NonOblique);
				bloodOilCB.SetGlobalMatrix("_InvProjection_NonOblique", mainCam.projectionMatrix.inverse);
				bloodOilCB.SetGlobalMatrix("_InverseView", mainCam.cameraToWorldMatrix);
				bloodOilCB.SetGlobalTexture("_DepthBuffer", depthBuffer);
				bloodOilCB.SetGlobalInteger("_FixOblique", 0);
				bloodOilCB.SetGlobalTexture("_WorldNormal", viewNormal.colorBuffer);
				bloodOilCB.SetGlobalTexture("_BloodstainTex", reusableBufferB.colorBuffer);
				bloodOilCB.SetGlobalTexture("_OilStainTex", reusableBufferB.colorBuffer);
				bloodOilCB.SetGlobalVector("_ZBufferParams_NonOblique", ZBufferParams(mainCam));
				bloodOilCB.SetRenderTarget(reusableBufferB.colorBuffer);
				bloodOilCB.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
				if (bloodsplatterManager.usedComputeShadersAtStart)
				{
					bloodOilCB.DrawMeshInstancedIndirect(bloodsplatterManager.optimizedBloodMesh, 0, bloodsplatterManager.stainMat, 0, bloodsplatterManager.argsBuffer, 0, null);
				}
				else
				{
					bloodOilCB.DrawMesh(bloodsplatterManager.totalStainMesh, Matrix4x4.identity, bloodsplatterManager.stainMat);
				}
				bloodOilCB.SetRenderTarget(mainTex.colorBuffer);
				bloodOilCB.DrawProcedural(identityMatrix, bloodsplatterManager.bloodCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
				if (usedComputeShadersAtStart)
				{
					bloodOilCB.SetRenderTarget(reusableBufferB.colorBuffer);
					bloodOilCB.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(1f, 1f, 1f, 0f));
					bloodOilCB.DrawMeshInstancedIndirect(stainVoxelManager.gasStainMesh, 0, stainVoxelManager.gasStainMat, 0, stainVoxelManager.argsBuffer, 0, null);
					bloodOilCB.SetRenderTarget(mainTex.colorBuffer);
					bloodOilCB.DrawProcedural(identityMatrix, stainVoxelManager.gasolineCompositeMaterial, 1, MeshTopology.Triangles, 3, 1);
				}
			}
			if (MonoSingleton<PortalManagerV2>.Instance == null)
			{
				RenderSkyboxes();
			}
		}
		if (cam == hudCam && (bool)mainTex)
		{
			hudCam.SetTargetBuffers(mainTex.colorBuffer, depthBuffer.depthBuffer);
		}
	}

	public void RenderSpaceSky(Camera cam)
	{
		if (spaceSky != null)
		{
			Vector4 globalVector = Shader.GetGlobalVector("_PortalClipPlane");
			Shader.SetGlobalVector("_PortalClipPlane", new Vector4(0f, 0f, 0f, 0f));
			spaceSky.RenderSpaceSky(cam);
			Shader.SetGlobalVector("_PortalClipPlane", globalVector);
		}
	}

	public void RenderLimboSkyboxes(Camera cam)
	{
		Vector4 globalVector = Shader.GetGlobalVector("_PortalClipPlane");
		Shader.SetGlobalVector("_PortalClipPlane", new Vector4(0f, 0f, 0f, 0f));
		foreach (LimboSkybox limboSkybox in limboSkyboxes)
		{
			limboSkybox.RenderLimboSkybox(cam);
		}
		Shader.SetGlobalVector("_PortalClipPlane", globalVector);
	}

	public Matrix4x4 GetInverseVP_NonOblique(Camera cam)
	{
		return (GL.GetGPUProjectionMatrix(mainCam.projectionMatrix, renderIntoTexture: true) * cam.worldToCameraMatrix).inverse;
	}

	public static void TryDestroyTexture(RenderTexture rt, bool destroyImmediate = false)
	{
		if (!rt)
		{
			return;
		}
		rt.Release();
		if (destroyImmediate)
		{
			if ((bool)rt)
			{
				UnityEngine.Object.DestroyImmediate(rt);
			}
		}
		else if ((bool)rt)
		{
			UnityEngine.Object.Destroy(rt);
		}
		rt = null;
	}

	private void OnDestroy()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
	}

	internal void AddLimboSkybox(LimboSkybox limboSkybox)
	{
		if (!limboSkyboxes.Contains(limboSkybox))
		{
			limboSkyboxes.Add(limboSkybox);
		}
	}

	internal void AddSpaceSkybox(SpaceSkybox spaceSkybox)
	{
		spaceSky = spaceSkybox;
	}

	internal void RenderSkyboxes()
	{
		RenderSpaceSky(mainCam);
		RenderLimboSkyboxes(mainCam);
	}
}
