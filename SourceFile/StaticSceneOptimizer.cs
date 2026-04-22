using System;
using System.Collections.Generic;
using System.Reflection;
using SettingsMenu.Components.Pages;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[DefaultExecutionOrder(int.MaxValue)]
public class StaticSceneOptimizer : MonoSingleton<StaticSceneOptimizer>
{
	public enum BakingMode
	{
		Stationary
	}

	private struct VertexData
	{
		public Vector3 position;

		public half4 normal;

		public Color32 color;

		public half2 uv0;

		public half4 mainAtlasRect;

		public half4 blendAtlasRect;

		public float lightData;
	}

	public struct LightData
	{
		public Vector4 lightPosition_attenX;

		public Vector4 lightDir_attenY;

		public Vector4 lightColor_attenZ;
	}

	public struct FullLightData
	{
		public Vector4 lightPosition_shadowFormat;

		public Vector4 lightAtten_shadowIndex;

		public Vector4 lightDir_shadowStrength;

		public Vector4 lightColor;

		public Matrix4x4 viewMatrix;

		public Matrix4x4 projectionMatrix;
	}

	public BakingMode bakeMode;

	[Space(10f)]
	[SerializeField]
	private StaticSceneData bakedDataAsset;

	[SerializeField]
	private List<Texture2D> devTexturesToSpot = new List<Texture2D>();

	[SerializeField]
	public List<Type> ignoreTypes = new List<Type>
	{
		typeof(Skull),
		typeof(EnemyIdentifier),
		typeof(SpawnEffect),
		typeof(Animator),
		typeof(NewMovement),
		typeof(IgnoreSceneOptimizer),
		typeof(Torch),
		typeof(FinalDoor),
		typeof(FinalRoom),
		typeof(Joint),
		typeof(SimpleMeshCombiner),
		typeof(MovingPlatform),
		typeof(ChessPiece),
		typeof(BaitItem),
		typeof(ScrollingTexture),
		typeof(AnimatedTexture),
		typeof(BloodAbsorber)
	};

	[HideInInspector]
	[SerializeField]
	private int ignoreCount;

	public bool warnNonStaticLights = true;

	public bool warnLightInGoreZone;

	public bool warnNonStaticObjects;

	public bool warnWrongObjectLayers;

	public bool warnNotUsingMasterShader;

	public bool warnMismatchedShaderKeywords;

	public bool warnMissingMeshFilter;

	public bool warnMissingMesh;

	public bool warnSubmeshIssues;

	public bool warnOddNegativeScaling;

	public bool randomColorAtlas;

	[HideInInspector]
	[SerializeField]
	private BakingMode currentBakedMode;

	[HideInInspector]
	[SerializeField]
	public Light[] globalLights;

	[HideInInspector]
	[SerializeField]
	public List<MeshRenderer> staticMRends = new List<MeshRenderer>();

	[HideInInspector]
	[SerializeField]
	private int bakedTextureCount;

	[HideInInspector]
	[SerializeField]
	private bool nothingBaked = true;

	[HideInInspector]
	[SerializeField]
	private bool isDirty = true;

	[HideInInspector]
	[SerializeField]
	private bool uv0Baked;

	[HideInInspector]
	[SerializeField]
	private bool uv1Baked;

	private Shader masterShader;

	[HideInInspector]
	[SerializeField]
	public Material batchMaterialOutdoors;

	[HideInInspector]
	[SerializeField]
	private Material batchMaterialEnvironment;

	private int enviroLayer = 8;

	private int enviroBakedLayer = 7;

	private int outdoorLayer = 24;

	private int outdoorBakedLayer = 6;

	private List<Material> reusableMaterials = new List<Material>();

	private HashSet<int> testHashes = new HashSet<int>();

	private ComputeBuffer cbMRLightIndices;

	private ComputeBuffer cbGlobalLightsData;

	private ComputeBuffer cbGlobalFullLightsData;

	private RenderTexture directionalShadows;

	private RenderTexture pointSpotShadows;

	private List<string> testKeywords = new List<string>(32);

	private List<Light> testLights = new List<Light>(100);

	private List<MeshRenderer> tempMRends = new List<MeshRenderer>(100);

	private List<Color> reusableColors = new List<Color>();

	private List<Vector3> reusablePositions = new List<Vector3>();

	private List<Vector3> reusableNormals = new List<Vector3>();

	private readonly string VERTEX_LIGHTING = "VERTEX_LIGHTING";

	private readonly string VERTEX_BLENDING = "VERTEX_BLENDING";

	private readonly string FOG_OFF = "_FOG_OFF";

	private readonly string FOG_ON = "_FOG_ON";

	public bool usedComputeShadersAtStart = true;

	private List<string> failureReasons = new List<string>();

	private Transform[] lightTransforms;

	private bool[] lightIsDirectionalBools;

	private Vector3[] lightAttens;

	private LightData[] lightDataArray;

	private static readonly Action<Renderer, int, int> s_SetStaticBatchInfo = (Action<Renderer, int, int>)typeof(Renderer).GetMethod("SetStaticBatchInfo", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(Action<Renderer, int, int>));

	[HideInInspector]
	[SerializeField]
	private bool bakeCompleted => !nothingBaked;

	public static void SetStaticBatchInfo(Renderer renderer, int firstSubMesh, int subMeshCount)
	{
		s_SetStaticBatchInfo(renderer, firstSubMesh, subMeshCount);
	}

	private void SetupMaterial(bool isBaking = false)
	{
		if (usedComputeShadersAtStart)
		{
			batchMaterialOutdoors.DisableKeyword("NO_COMPUTE");
			batchMaterialEnvironment.DisableKeyword("NO_COMPUTE");
		}
		else
		{
			batchMaterialOutdoors.EnableKeyword("NO_COMPUTE");
			batchMaterialEnvironment.EnableKeyword("NO_COMPUTE");
		}
		LocalKeyword[] enabledKeywords = batchMaterialEnvironment.enabledKeywords;
		foreach (LocalKeyword localKeyword in enabledKeywords)
		{
			Debug.LogWarning($"enviro keyword: {localKeyword}");
		}
		enabledKeywords = batchMaterialOutdoors.enabledKeywords;
		foreach (LocalKeyword localKeyword2 in enabledKeywords)
		{
			Debug.LogWarning($"outdoor keyword: {localKeyword2}");
		}
		if (!isBaking && !(bakedDataAsset == null))
		{
			if (bakedDataAsset.mainTexAtlas != null)
			{
				batchMaterialOutdoors.SetTexture("_MainTex", bakedDataAsset.mainTexAtlas);
				batchMaterialEnvironment.SetTexture("_MainTex", bakedDataAsset.mainTexAtlas);
			}
			if (bakedDataAsset.blendTexAtlas != null)
			{
				batchMaterialOutdoors.SetTexture("_BlendTex", bakedDataAsset.blendTexAtlas);
				batchMaterialEnvironment.SetTexture("_BlendTex", bakedDataAsset.blendTexAtlas);
			}
			else
			{
				Debug.LogWarning("No atlas texture exists for the scene!");
			}
			batchMaterialEnvironment.renderQueue = 2050;
			batchMaterialOutdoors.renderQueue = 2050;
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			usedComputeShadersAtStart = !SettingsMenu.Components.Pages.GraphicsSettings.disabledComputeShaders;
			FixPosition();
			SetupMaterial();
			SetupMeshes();
			if (usedComputeShadersAtStart)
			{
				InitializeGlobalBuffer();
			}
		}
	}

	private void FixPosition()
	{
		base.transform.SetParent(null);
		base.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		base.transform.localScale = Vector3.one;
	}

	private void SetupMeshes()
	{
		for (int i = 0; i < staticMRends.Count; i++)
		{
			MeshRenderer meshRenderer = staticMRends[i];
			if (meshRenderer == null)
			{
				continue;
			}
			if (meshRenderer.TryGetComponent<ProBuilderMesh>(out var component))
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
			}
			Bounds bounds = meshRenderer.bounds;
			ushort index = bakedDataAsset.mrMeshIndices[i];
			meshRenderer.GetComponent<MeshFilter>().sharedMesh = bakedDataAsset.bakedMeshes[index];
			int firstSubMesh = bakedDataAsset.firstSubMesh[i];
			SetStaticBatchInfo(meshRenderer, firstSubMesh, 1);
			GameObject gameObject = meshRenderer.gameObject;
			int layer = gameObject.layer;
			reusableMaterials.Clear();
			reusableMaterials.Add((layer == 24) ? batchMaterialOutdoors : batchMaterialEnvironment);
			meshRenderer.SetSharedMaterials(reusableMaterials);
			if (usedComputeShadersAtStart)
			{
				if (layer == enviroLayer)
				{
					gameObject.layer = enviroBakedLayer;
				}
				if (layer == outdoorLayer)
				{
					gameObject.layer = outdoorBakedLayer;
				}
			}
			meshRenderer.bounds = bounds;
		}
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (Application.isPlaying && usedComputeShadersAtStart && SystemInfo.supportsComputeShaders)
		{
			UpdateLightBuffer();
		}
	}

	public void UpdateRain(bool doEnable)
	{
		if (doEnable)
		{
			batchMaterialOutdoors.EnableKeyword("RAIN");
		}
		else
		{
			batchMaterialOutdoors.DisableKeyword("RAIN");
		}
	}

	private void InitializeGlobalBuffer()
	{
		if (nothingBaked)
		{
			base.enabled = false;
			return;
		}
		int num = globalLights.Length;
		int stride = UnsafeUtility.SizeOf<LightData>();
		cbGlobalLightsData = new ComputeBuffer(num, stride, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
		Shader.SetGlobalBuffer("_GlobalLightData", cbGlobalLightsData);
		cbMRLightIndices = new ComputeBuffer(bakedDataAsset.mrLightIndices.Count, 4, ComputeBufferType.Structured);
		cbMRLightIndices.SetData(bakedDataAsset.mrLightIndices);
		Shader.SetGlobalBuffer("_GlobalLightIndices", cbMRLightIndices);
		lightTransforms = new Transform[num];
		lightIsDirectionalBools = new bool[num];
		lightAttens = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			Light light = globalLights[i];
			if (light == null)
			{
				light = null;
				continue;
			}
			light.gameObject.AddComponent<CancellationTokenHelper>().destroyCancellationToken.Register(delegate
			{
				light = null;
			});
			lightTransforms[i] = globalLights[i].transform;
			LightType type = light.type;
			bool flag = type == LightType.Directional;
			lightIsDirectionalBools[i] = flag;
			bool num2 = type == LightType.Spot;
			float num3 = light.spotAngle * (MathF.PI / 180f);
			float x = (num2 ? Mathf.Cos(num3 / 2f) : (-1f));
			float y = (num2 ? (1f / (Mathf.Cos(num3 / 4f) - Mathf.Cos(num3 / 2f))) : 1f);
			float z = (flag ? 0f : light.range);
			lightAttens[i] = new Vector3(x, y, z);
			light.cullingMask &= ~((1 << enviroBakedLayer) | (1 << outdoorBakedLayer));
		}
	}

	private unsafe void UpdateLightBuffer()
	{
		if (cbGlobalLightsData == null)
		{
			return;
		}
		int num = globalLights.Length;
		LightData* unsafeBufferPointerWithoutChecks = (LightData*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(cbGlobalLightsData.BeginWrite<LightData>(0, num));
		for (int i = 0; i < num; i++)
		{
			Light light = globalLights[i];
			ref LightData reference = ref unsafeBufferPointerWithoutChecks[i];
			if (light == null)
			{
				light = null;
				reference.lightColor_attenZ.x = -1000f;
				continue;
			}
			if (!light.isActiveAndEnabled)
			{
				reference.lightColor_attenZ.x = -1000f;
				continue;
			}
			Transform transform = lightTransforms[i];
			Vector3 forward = transform.forward;
			if (lightIsDirectionalBools[i])
			{
				reference.lightPosition_attenX.x = 0f - forward.x;
				reference.lightPosition_attenX.y = 0f - forward.y;
				reference.lightPosition_attenX.z = 0f - forward.z;
			}
			else
			{
				Vector3 position = transform.position;
				reference.lightPosition_attenX.x = position.x;
				reference.lightPosition_attenX.y = position.y;
				reference.lightPosition_attenX.z = position.z;
				reference.lightDir_attenY.x = 0f - forward.x;
				reference.lightDir_attenY.y = 0f - forward.y;
				reference.lightDir_attenY.z = 0f - forward.z;
			}
			Color color = light.color;
			float intensity = light.intensity;
			reference.lightColor_attenZ.x = color.r * intensity;
			reference.lightColor_attenZ.y = color.g * intensity;
			reference.lightColor_attenZ.z = color.b * intensity;
			Vector3 vector = lightAttens[i];
			reference.lightPosition_attenX.w = vector.x;
			reference.lightDir_attenY.w = vector.y;
			reference.lightColor_attenZ.w = vector.z;
		}
		cbGlobalLightsData.EndWrite<LightData>(num);
	}

	public void GetSurfaceType(MeshRenderer mRend)
	{
		int num = staticMRends.IndexOf(mRend);
		Debug.Log($"found {mRend} at index {num}");
	}

	private void OnDestroy()
	{
		if (cbGlobalLightsData != null)
		{
			cbMRLightIndices.Release();
		}
		if (cbGlobalLightsData != null)
		{
			cbGlobalLightsData.Release();
		}
	}
}
