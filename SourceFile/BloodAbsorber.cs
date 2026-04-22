using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class BloodAbsorber : MonoBehaviour, IBloodstainReceiver
{
	private struct CollisionData
	{
		public Vector3 position;

		public float distance;
	}

	public string painterName;

	public Shader absorptionShader;

	public Texture noiseTex;

	public Texture2D visibilityMask;

	public int texelsPerWorldUnit = 10;

	public float forgivenessCutoff = 0.01f;

	public float bloodUpdateRate = 0.02f;

	public float timeUntilSleep = 4f;

	public bool clearBlood;

	public bool fillBlood;

	[SerializeField]
	private float maxFill;

	[HideInInspector]
	public float fillAmount = 999f;

	public bool isCompleted;

	private float sleepTimer;

	private bool isWashing;

	private float bloodTimer;

	private bool isSleeping = true;

	private CommandBuffer cb;

	private Material absMat;

	public RenderTexture paintBuffer;

	public RenderTexture bloodMap;

	public RenderTexture clampedMap;

	public RenderTexture dilationMask;

	public RenderTexture bloodMapCheckpoint;

	private MeshRenderer[] absorbers;

	private MeshFilter[] absorberMFs;

	private MaterialPropertyBlock propBlock;

	private ComputeBuffer cbuff;

	[HideInInspector]
	public GameObject owningRoom;

	private BloodCheckerManager bcm;

	[SerializeField]
	private AudioSource cleanSuccess;

	public Cubemap cleanedMap;

	private List<CollisionData> collisionDataList;

	private Coroutine checkFillRoutine;

	private float baseAccuracy;

	private void Start()
	{
		bcm = MonoSingleton<BloodCheckerManager>.Instance;
		cb = new CommandBuffer
		{
			name = "PaintRenderer"
		};
		absorbers = GetComponentsInChildren<MeshRenderer>();
		absorberMFs = new MeshFilter[absorbers.Length];
		CombineInstance[] array = new CombineInstance[absorbers.Length];
		for (int i = 0; i < absorbers.Length; i++)
		{
			MeshRenderer meshRenderer = absorbers[i];
			if (meshRenderer.TryGetComponent<MeshFilter>(out var component))
			{
				absorberMFs[i] = component;
				array[i].mesh = component.sharedMesh;
			}
			else
			{
				Debug.LogError("No mesh found for: " + meshRenderer.gameObject.name);
			}
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(array);
		int num = Math.Min(Mathf.CeilToInt(Mathf.Sqrt(mesh.GetUVDistributionMetric(1)) * (float)texelsPerWorldUnit), 4096);
		float f = Mathf.Log(num, 2f);
		f = Mathf.Round(f);
		f = Mathf.Pow(2f, f);
		int num2 = (int)f;
		if (num <= 0)
		{
			Debug.LogError("Invalid texture size calculated for: " + base.gameObject.name);
		}
		propBlock = new MaterialPropertyBlock();
		absMat = new Material(absorptionShader);
		paintBuffer = new RenderTexture(num, num, 0, RenderTextureFormat.R16, 0)
		{
			filterMode = FilterMode.Point
		};
		bloodMap = new RenderTexture(num, num, 0, RenderTextureFormat.R8, 0)
		{
			filterMode = FilterMode.Point
		};
		dilationMask = new RenderTexture(num, num, 0, RenderTextureFormat.R8, 0)
		{
			filterMode = FilterMode.Point
		};
		clampedMap = new RenderTexture(num2, num2, 0, RenderTextureFormat.RHalf)
		{
			filterMode = FilterMode.Point,
			useMipMap = true,
			autoGenerateMips = false
		};
		paintBuffer.Create();
		bloodMap.Create();
		clampedMap.Create();
		dilationMask.Create();
		InitializeRTs();
		InitializeDilationMask();
		propBlock.SetTexture("_BloodBuffer", paintBuffer);
		propBlock.SetTexture("_BloodTex", bloodMap);
		propBlock.SetTexture("_DilationMask", dilationMask);
		for (int j = 0; j < absorbers.Length; j++)
		{
			absorbers[j].SetPropertyBlock(propBlock);
		}
		if (visibilityMask == null)
		{
			visibilityMask = Texture2D.whiteTexture;
		}
		cbuff = new ComputeBuffer(100, 16);
		collisionDataList = new List<CollisionData>(cbuff.count);
		baseAccuracy = forgivenessCutoff;
	}

	public void ToggleHigherAccuracy(bool isOn)
	{
		forgivenessCutoff = (isOn ? 0.0001f : baseAccuracy);
	}

	private void OnEnable()
	{
		BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
		if (instance != null)
		{
			instance.bloodAbsorbers++;
		}
	}

	private void OnDisable()
	{
		BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
		if (instance != null)
		{
			instance.bloodAbsorbers--;
		}
	}

	public void StartCheckingFill()
	{
		if (checkFillRoutine == null)
		{
			StartCoroutine(CheckFill());
		}
	}

	public void StoreBloodCopy()
	{
		if (bloodMapCheckpoint == null)
		{
			bloodMapCheckpoint = new RenderTexture(bloodMap);
		}
		Graphics.CopyTexture(bloodMap, bloodMapCheckpoint);
	}

	public void RestoreBloodCopy()
	{
		if (bloodMapCheckpoint != null)
		{
			Graphics.CopyTexture(bloodMapCheckpoint, bloodMap);
		}
		else
		{
			InitializeRTs();
		}
		if (isCompleted)
		{
			StartCheckingFill();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.TryGetComponent<GoreSplatter>(out var component))
		{
			component.bloodAbsorberCount++;
			bcm.AddGoreToRoom(this, component);
		}
		else
		{
			if (!collision.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
			{
				return;
			}
			component2.bloodAbsorberCount++;
			if (component2.eid == component2.GetComponentInParent<EnemyIdentifier>())
			{
				EnemyIdentifierIdentifier[] componentsInChildren = component2.eid.GetComponentsInChildren<EnemyIdentifierIdentifier>();
				foreach (EnemyIdentifierIdentifier litter in componentsInChildren)
				{
					bcm.AddGibToRoom(this, litter);
				}
			}
			else
			{
				bcm.AddGibToRoom(this, component2);
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		EnemyIdentifierIdentifier component2;
		if (collision.transform.TryGetComponent<GoreSplatter>(out var component))
		{
			component.bloodAbsorberCount--;
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
			{
				component.Invoke("RepoolIfNoAbsorber", StockMapInfo.Instance.gibRemoveTime);
			}
		}
		else if (collision.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component2))
		{
			component2.bloodAbsorberCount--;
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
			{
				component2.SetupForHellBath();
			}
		}
	}

	private IEnumerator CheckFill()
	{
		UnclearedAbsorber();
		yield return new WaitForSeconds(3f);
		if (maxFill == 0f)
		{
			Debug.LogWarning("No max fill data for: " + base.gameObject.name);
			Graphics.Blit(bloodMap, clampedMap, absMat, 5);
			clampedMap.GenerateMips();
			AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(clampedMap, clampedMap.mipmapCount - 1, AsyncGetFilledSpace);
			yield return new WaitUntil(() => request.done);
		}
		fillAmount = 999f;
		int timesChecked = -1;
		while (fillAmount >= forgivenessCutoff)
		{
			timesChecked = Math.Min(2, timesChecked + 1);
			Graphics.Blit(bloodMap, clampedMap, absMat, 5);
			clampedMap.GenerateMips();
			AsyncGPUReadbackRequest request2 = AsyncGPUReadback.Request(clampedMap, clampedMap.mipmapCount - 1, AsyncGetCurrentFillAmount);
			yield return new WaitUntil(() => request2.done);
		}
		if (timesChecked == 2)
		{
			UnityEngine.Object.Instantiate<AudioSource>(cleanSuccess);
		}
		checkFillRoutine = null;
		StartCoroutine(ClearedAbsorber());
		isWashing = true;
		fillAmount = 1f;
		cb.Clear();
		cb.SetRenderTarget(bloodMap);
		cb.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		Graphics.ExecuteCommandBuffer(cb);
	}

	private IEnumerator ClearedAbsorber()
	{
		isCompleted = true;
		float time = 0f;
		propBlock.SetTexture("_CubeTex", cleanedMap);
		while (time < 1f)
		{
			float value = Mathf.Lerp(0f, 0.06f, time);
			propBlock.SetFloat("_ReflectionStrength", value);
			Color value2 = Color.Lerp(new Color(1f, 0.9f, 0.5f), Color.white, time);
			propBlock.SetColor("_EmissiveColor", value2);
			float value3 = Mathf.Lerp(2f, 1f, time);
			propBlock.SetFloat("_EmissiveIntensity", value3);
			int num = absorbers.Length;
			for (int i = 0; i < num; i++)
			{
				absorbers[i].SetPropertyBlock(propBlock);
			}
			time += Time.deltaTime;
			yield return null;
		}
	}

	private void UnclearedAbsorber()
	{
		isCompleted = false;
		propBlock.SetFloat("_ReflectionStrength", 0f);
		propBlock.SetColor("_EmissiveColor", Color.white);
		propBlock.SetFloat("_EmissiveIntensity", 1f);
		int num = absorbers.Length;
		for (int i = 0; i < num; i++)
		{
			absorbers[i].SetPropertyBlock(propBlock);
		}
	}

	private void AsyncGetFilledSpace(AsyncGPUReadbackRequest request)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!clampedMap)
		{
			Debug.LogError("No blood map exists while attempting to calculate absorber max fill.");
		}
		maxFill = half.op_Implicit(request.GetData<half>(0)[0]);
		fillAmount = maxFill;
	}

	private void AsyncGetCurrentFillAmount(AsyncGPUReadbackRequest request)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)clampedMap)
		{
			float num = half.op_Implicit(request.GetData<half>(0)[0]);
			fillAmount = num / maxFill;
		}
	}

	private void OnValidate()
	{
		if (clearBlood)
		{
			InitializeRTs();
			clearBlood = false;
		}
		if (fillBlood)
		{
			Graphics.Blit(visibilityMask, bloodMap, absMat, 7);
			fillBlood = false;
		}
		int num = visibilityMask.width * visibilityMask.height;
		float num2 = 0f;
		for (int i = 0; i < visibilityMask.width; i++)
		{
			for (int j = 0; j < visibilityMask.height; j++)
			{
				num2 += visibilityMask.GetPixel(i, j).r;
			}
		}
		maxFill = num2 / (float)num;
	}

	private void InitializeRTs()
	{
		cb.Clear();
		cb.SetRenderTarget(paintBuffer);
		cb.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
		cb.SetRenderTarget(bloodMap);
		cb.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
		Graphics.ExecuteCommandBuffer(cb);
	}

	private void InitializeDilationMask()
	{
		cb.Clear();
		cb.SetRenderTarget(dilationMask);
		for (int i = 0; i < absorbers.Length; i++)
		{
			MeshRenderer renderer = absorbers[i];
			int subMeshCount = absorberMFs[i].sharedMesh.subMeshCount;
			for (int j = 0; j < subMeshCount; j++)
			{
				cb.DrawRenderer(renderer, absMat, j, 6);
			}
		}
		Graphics.ExecuteCommandBuffer(cb);
	}

	private void Update()
	{
		if (!isSleeping && sleepTimer >= timeUntilSleep)
		{
			cb.Clear();
			cb.SetRenderTarget(paintBuffer);
			cb.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
			Graphics.ExecuteCommandBuffer(cb);
			clearBlood = false;
			isSleeping = true;
			return;
		}
		if (bloodTimer >= bloodUpdateRate)
		{
			Graphics.Blit(null, paintBuffer, absMat, 1);
			absMat.SetTexture("_VisibilityMask", visibilityMask);
			Graphics.Blit(paintBuffer, bloodMap, absMat, isWashing ? 3 : 2);
			bloodTimer = 0f;
		}
		bloodTimer += Time.deltaTime;
		sleepTimer += Time.deltaTime;
	}

	public bool HandleBloodstainHit(in RaycastHit hit)
	{
		if (isCompleted)
		{
			StartCheckingFill();
		}
		if (isWashing)
		{
			cb.Clear();
			cb.SetRenderTarget(paintBuffer);
			cb.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		isWashing = false;
		isSleeping = false;
		sleepTimer = 0f;
		Vector3 point = hit.point;
		Vector3 normal = hit.normal;
		Vector3 vector = normal * -1f;
		Quaternion quaternion = Quaternion.LookRotation(normal, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), normal);
		Matrix4x4 value = Matrix4x4.Rotate(quaternion * quaternion2);
		absMat.SetVector("_HitPos", point);
		absMat.SetVector("_HitNorm", vector);
		absMat.SetMatrix("_RotMat", value);
		absMat.SetTexture("_MainTex", paintBuffer);
		absMat.SetTexture("_NoiseTex", noiseTex);
		absMat.SetTexture("_VisibilityMask", visibilityMask);
		cb.Clear();
		cb.SetRenderTarget(paintBuffer);
		propBlock.SetFloat("_IsWashing", 0f);
		int num = absorbers.Length;
		for (int i = 0; i < num; i++)
		{
			MeshRenderer meshRenderer = absorbers[i];
			int subMeshCount = absorberMFs[i].sharedMesh.subMeshCount;
			for (int j = 0; j < subMeshCount; j++)
			{
				cb.DrawRenderer(meshRenderer, absMat, j, 0);
			}
			meshRenderer.SetPropertyBlock(propBlock);
		}
		Graphics.ExecuteCommandBuffer(cb);
		Graphics.Blit(paintBuffer, bloodMap, absMat, 2);
		return true;
	}

	public void ProcessWasherSpray(ref List<ParticleCollisionEvent> pEvents, Vector3 position, MeshRenderer hitChild)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!isWashing)
		{
			cb.Clear();
			cb.SetRenderTarget(paintBuffer);
			cb.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		isWashing = true;
		collisionDataList.Clear();
		int num = 0;
		foreach (ParticleCollisionEvent pEvent in pEvents)
		{
			ParticleCollisionEvent current = pEvent;
			if (num >= cbuff.count)
			{
				break;
			}
			CollisionData item = new CollisionData
			{
				position = ((ParticleCollisionEvent)(ref current)).intersection,
				distance = Vector3.Distance(((ParticleCollisionEvent)(ref current)).intersection, position)
			};
			collisionDataList.Add(item);
			num++;
		}
		cbuff.SetData(collisionDataList);
		isSleeping = false;
		sleepTimer = 0f;
		absMat.SetFloat("_HitCount", num);
		absMat.SetBuffer("_HitData", cbuff);
		absMat.SetTexture("_MainTex", paintBuffer);
		absMat.SetTexture("_NoiseTex", noiseTex);
		cb.Clear();
		cb.SetRenderTarget(paintBuffer);
		propBlock.SetFloat("_IsWashing", 1f);
		if (hitChild != null)
		{
			int subMeshCount = absorberMFs[Array.IndexOf(absorbers, hitChild)].sharedMesh.subMeshCount;
			for (int i = 0; i < subMeshCount; i++)
			{
				cb.DrawRenderer(hitChild, absMat, i, 4);
			}
			hitChild.SetPropertyBlock(propBlock);
		}
		else
		{
			int num2 = absorbers.Length;
			for (int j = 0; j < num2; j++)
			{
				MeshRenderer meshRenderer = absorbers[j];
				int subMeshCount2 = absorberMFs[j].sharedMesh.subMeshCount;
				for (int k = 0; k < subMeshCount2; k++)
				{
					cb.DrawRenderer(meshRenderer, absMat, k, 4);
				}
				meshRenderer.SetPropertyBlock(propBlock);
			}
		}
		Graphics.ExecuteCommandBuffer(cb);
		absMat.SetTexture("_VisibilityMask", visibilityMask);
		Graphics.Blit(paintBuffer, bloodMap, absMat, 3);
	}

	bool IBloodstainReceiver.HandleBloodstainHit(in RaycastHit hit)
	{
		return HandleBloodstainHit(in hit);
	}
}
