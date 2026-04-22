using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Portal;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
[BurstCompile]
public class BloodsplatterManager : MonoSingleton<BloodsplatterManager>
{
	public struct InstanceProperties
	{
		public float3 pos;

		public int parentIndex;

		public float3 norm;

		public int clipState;

		public const int SIZE = 32;
	}

	private struct VertexData_Simple
	{
		public float3 position;

		public half4 normal;

		public half2 uv;
	}

	public class CachedColliderData
	{
		public GameObject go;

		public BloodstainParent parent;

		public IBloodstainReceiver receiver;

		public bool displace;

		public bool noLayers;

		public bool is7SObject;

		public CachedColliderData(ComponentsDatabase cdatabase, in RaycastHit hit, GameObject gameObject)
		{
			go = gameObject;
			if (StockMapInfo.Instance.continuousGibCollisions)
			{
				if (go.TryGetComponent<IBloodstainReceiver>(out receiver))
				{
					is7SObject = true;
				}
				else
				{
					is7SObject = false;
				}
			}
			bool num = go.CompareTag("Wall");
			bool flag = !num && go.CompareTag("Floor");
			bool flag2 = !flag && go.CompareTag("Moving");
			bool flag3 = !flag2 && go.CompareTag("Glass");
			bool flag4 = !flag3 && go.CompareTag("GlassFloor");
			bool flag5 = !flag4 && go.CompareTag("Door");
			if (!num && !flag && !flag2 && !flag3 && !flag4 && !flag5)
			{
				noLayers = true;
			}
			if ((num || flag) && go.TryGetComponent<MeshRenderer>(out var component))
			{
				Material sharedMaterial = component.sharedMaterial;
				if ((bool)sharedMaterial && sharedMaterial.IsKeywordEnabled("VERTEX_DISPLACEMENT"))
				{
					displace = true;
				}
			}
			Transform transform = go.transform;
			bool flag6 = flag2 || flag3 || flag4 || flag5;
			if (!flag6)
			{
				flag6 |= (bool)cdatabase && cdatabase.scrollers.Contains(transform);
			}
			if (flag6)
			{
				parent = (go.TryGetComponent<ScrollingTexture>(out var component2) ? component2.parent : go.GetOrAddComponent<BloodstainParent>());
			}
		}
	}

	internal delegate int CreateBloodstain_000002B5_0024PostfixBurstDelegate(ref int propIndex, ref int currentBloodCount, ref NativeArray<InstanceProperties> props, in float3 pos, in float3 norm, bool clipToSurface, int parent = 0);

	internal static class CreateBloodstain_000002B5_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CreateBloodstain_000002B5_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static CreateBloodstain_000002B5_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static int Invoke(ref int propIndex, ref int currentBloodCount, ref NativeArray<InstanceProperties> props, in float3 pos, in float3 norm, bool clipToSurface, int parent = 0)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref int, ref int, ref NativeArray<InstanceProperties>, ref float3, ref float3, bool, int, int>)functionPointer)(ref propIndex, ref currentBloodCount, ref props, ref pos, ref norm, clipToSurface, parent);
				}
			}
			return CreateBloodstain_0024BurstManaged(ref propIndex, ref currentBloodCount, ref props, in pos, in norm, clipToSurface, parent);
		}
	}

	public float normalForgiveness = 10f;

	public bool forceOn;

	public bool forceGibs;

	public bool neverFreezeGibs;

	public bool overrideBloodstainChance;

	public float bloodstainChance;

	public GameObject head;

	public GameObject limb;

	public GameObject body;

	public GameObject small;

	public GameObject smallest;

	public GameObject splatter;

	public GameObject underwater;

	public GameObject sand;

	public GameObject blessing;

	public GameObject chestExplosion;

	public GameObject brainChunk;

	public GameObject skullChunk;

	public GameObject eyeball;

	public GameObject jawChunk;

	public GameObject[] gib;

	private int currentBloodCount;

	public GameObject bloodStain;

	public Shader bloodCompositeShader;

	public Material bloodCompositeMaterial;

	public Mesh stainMesh;

	public Material stainMat;

	public NativeArray<InstanceProperties> checkpointProps;

	public NativeArray<Matrix4x4> checkpointParents;

	private int checkpointPropIndex;

	public int checkpointParentIndex;

	public int checkpointBloodCount;

	public BloodstainParent[] parents;

	private BloodstainParent[] checkpointParentsArray;

	public NativeArray<InstanceProperties> props;

	public NativeArray<Matrix4x4> parentMatrices;

	private int propIndex;

	private int parentIndex = 1;

	public ComputeBuffer instanceBuffer;

	public ComputeBuffer parentBuffer;

	public ComputeBuffer argsBuffer;

	private uint[] argsData = new uint[5];

	internal int[] parentOfStain;

	internal int[] indexInParentList;

	private int[] checkpointParentOfStain;

	private int[] checkpointIndexInParentList;

	private Dictionary<BSType, Queue<GameObject>> gorePool = new Dictionary<BSType, Queue<GameObject>>();

	private Dictionary<BSType, int> defaultHPValues = new Dictionary<BSType, int>();

	private int order;

	private VertexAttributeDescriptor[] vertexLayoutInstanced;

	private VertexAttributeDescriptor[] vertexLayoutNonInstanced;

	private Transform goreStore;

	public bool hasBloodFillers;

	public HashSet<GameObject> bloodFillers = new HashSet<GameObject>();

	public AudioMixerGroup goreAudioGroup;

	public AudioClip splatterClip;

	[HideInInspector]
	public int bloodDestroyers;

	[HideInInspector]
	public int bloodAbsorbers;

	[HideInInspector]
	public int bloodAbsorberChildren;

	public const float PARTICLE_COLLISION_STEP_DT = 0.128f;

	public TimeSince sinceLastStep;

	private OptionsManager opm;

	public Dictionary<int, Bloodsplatter> splatters = new Dictionary<int, Bloodsplatter>();

	public NativeQueue<BloodstainCreateCommand> stainCreateQueue;

	public bool usedComputeShadersAtStart = true;

	public bool meshDirty;

	public Mesh totalStainMesh;

	public GenerateBloodMeshJob generateBloodMeshJob;

	private JobHandle generateBloodMeshJobHandle;

	private Mesh.MeshDataArray meshDataArray;

	public Mesh optimizedBloodMesh;

	private ComponentsDatabase cdatabase;

	private const string VERTEX_DISPLACEMENT = "VERTEX_DISPLACEMENT";

	private Dictionary<int, CachedColliderData> colliderDatas = new Dictionary<int, CachedColliderData>();

	internal bool instanceBufferDirty;

	public bool goreOn
	{
		get
		{
			if (!forceOn && !forceGibs)
			{
				if (MonoSingleton<PrefsManager>.TryGetInstance(out PrefsManager prefsManager))
				{
					return prefsManager.GetBoolLocal("bloodEnabled");
				}
				return false;
			}
			return true;
		}
	}

	public event Action<int> reuseParentIndex;

	public event Action StainsCleared;

	public event Action<float> ParticleCollisionStep;

	public event Action PostCollisionStep;

	public void SaveBloodstains()
	{
		checkpointPropIndex = propIndex;
		checkpointProps.CopyFrom(props);
		checkpointBloodCount = currentBloodCount;
		checkpointParentIndex = parentIndex;
		checkpointParents.CopyFrom(parentMatrices);
		Array.Copy(parents, checkpointParentsArray, parents.Length);
		Array.Copy(parentOfStain, checkpointParentOfStain, parentOfStain.Length);
		Array.Copy(indexInParentList, checkpointIndexInParentList, indexInParentList.Length);
		for (int i = 0; i < parents.Length; i++)
		{
			if (parents[i] != null)
			{
				parents[i].SaveState();
			}
		}
	}

	public void LoadBloodstains()
	{
		props.CopyFrom(checkpointProps);
		currentBloodCount = checkpointBloodCount;
		propIndex = checkpointPropIndex;
		parentMatrices.CopyFrom(checkpointParents);
		parentIndex = checkpointParentIndex;
		Array.Copy(checkpointParentsArray, parents, parents.Length);
		Array.Copy(checkpointParentOfStain, parentOfStain, parentOfStain.Length);
		Array.Copy(checkpointIndexInParentList, indexInParentList, indexInParentList.Length);
		for (int i = 0; i < parents.Length; i++)
		{
			if (parents[i] != null)
			{
				parents[i].LoadState();
			}
		}
		if (usedComputeShadersAtStart)
		{
			parentBuffer.SetData(parentMatrices);
			instanceBufferDirty = true;
			argsData[1] = (uint)currentBloodCount;
			argsBuffer.SetData(argsData);
		}
		else
		{
			generateBloodMeshJob.props = props;
			meshDirty = true;
		}
	}

	public int CreateBloodstain(Vector3 pos, Vector3 norm, bool clipToSurface, BloodstainParent parent)
	{
		generateBloodMeshJobHandle.Complete();
		int num = CreateBloodstain(ref propIndex, ref currentBloodCount, ref props, in Unsafe.As<Vector3, float3>(ref pos), in Unsafe.As<Vector3, float3>(ref norm), clipToSurface, parent.parentIndex);
		parent.AddStain(num);
		if (usedComputeShadersAtStart)
		{
			instanceBuffer.SetData(props, num, num, 1);
		}
		else
		{
			meshDirty = true;
		}
		return num;
	}

	[BurstCompile]
	private static int CreateBloodstain(ref int propIndex, ref int currentBloodCount, ref NativeArray<InstanceProperties> props, in float3 pos, in float3 norm, bool clipToSurface, int parent = 0)
	{
		return CreateBloodstain_000002B5_0024BurstDirectCall.Invoke(ref propIndex, ref currentBloodCount, ref props, in pos, in norm, clipToSurface, parent);
	}

	public int CreateParent(Matrix4x4 initialMatrix)
	{
		int num = parentIndex;
		parentIndex++;
		if (num >= parentMatrices.Length)
		{
			num = (parentIndex = 1);
		}
		this.reuseParentIndex?.Invoke(num);
		parentMatrices[num] = initialMatrix;
		return num;
	}

	public float GetBloodstainChance()
	{
		if (overrideBloodstainChance)
		{
			return bloodstainChance;
		}
		return opm.bloodstainChance;
	}

	private void Start()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		totalStainMesh = new Mesh();
		totalStainMesh.name = "totalStainMesh";
		totalStainMesh.MarkDynamic();
		stainCreateQueue = new NativeQueue<BloodstainCreateCommand>(AllocatorHandle.op_Implicit(Allocator.Persistent));
		cdatabase = MonoSingleton<ComponentsDatabase>.Instance;
		usedComputeShadersAtStart = !SettingsMenu.Components.Pages.GraphicsSettings.disabledComputeShaders;
		Shader.SetGlobalFloat("_StainWarping", MonoSingleton<PrefsManager>.Instance.GetInt("vertexWarping"));
		bloodCompositeMaterial = new Material(bloodCompositeShader);
		props = new NativeArray<InstanceProperties>((int)MonoSingleton<PrefsManager>.Instance.GetFloatLocal("bloodStainMax", 100000f), Allocator.Persistent);
		int num = props.Length + 1;
		parentOfStain = new int[num];
		Array.Fill(parentOfStain, -1);
		indexInParentList = new int[num];
		checkpointParentOfStain = new int[num];
		checkpointIndexInParentList = new int[num];
		checkpointProps = new NativeArray<InstanceProperties>(props.Length, Allocator.Persistent);
		parentMatrices = new NativeArray<Matrix4x4>(1024, Allocator.Persistent);
		checkpointParents = new NativeArray<Matrix4x4>(1024, Allocator.Persistent);
		parentMatrices[0] = Matrix4x4.identity;
		parents = new BloodstainParent[parentMatrices.Length];
		checkpointParentsArray = new BloodstainParent[parentMatrices.Length];
		vertexLayoutInstanced = new VertexAttributeDescriptor[3]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
		};
		vertexLayoutNonInstanced = new VertexAttributeDescriptor[4]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord1)
		};
		if (usedComputeShadersAtStart)
		{
			instanceBuffer = new ComputeBuffer(props.Length, 32, ComputeBufferType.Structured);
			instanceBuffer.SetData(props);
			argsData[0] = stainMesh.GetIndexCount(0);
			argsData[1] = 0u;
			argsData[2] = stainMesh.GetIndexStart(0);
			argsData[3] = stainMesh.GetBaseVertex(0);
			argsData[4] = 0u;
			argsBuffer = new ComputeBuffer(1, argsData.Length * 4, ComputeBufferType.DrawIndirect);
			argsBuffer.SetData(argsData);
			parentBuffer = new ComputeBuffer(parentMatrices.Length, 64, ComputeBufferType.Structured);
			Vector3[] vertices = stainMesh.vertices;
			Vector3[] normals = stainMesh.normals;
			Vector2[] uv = stainMesh.uv;
			optimizedBloodMesh = new Mesh();
			optimizedBloodMesh.name = "BloodStainMesh";
			optimizedBloodMesh.SetVertexBufferParams(vertices.Length, vertexLayoutInstanced);
			List<VertexData_Simple> list = new List<VertexData_Simple>();
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vector = normals[i];
				list.Add(new VertexData_Simple
				{
					position = float3.op_Implicit(vertices[i]),
					normal = new half4((half)vector.x, (half)vector.y, (half)vector.z, (half)1f),
					uv = (half2)float2.op_Implicit(uv[i])
				});
			}
			ushort[] array = Array.ConvertAll(stainMesh.GetIndices(0), (int num3) => (ushort)num3);
			optimizedBloodMesh.subMeshCount = 1;
			optimizedBloodMesh.SetVertexBufferData(list, 0, 0, vertices.Length);
			optimizedBloodMesh.SetIndexBufferParams(array.Length, IndexFormat.UInt16);
			optimizedBloodMesh.SetIndexBufferData(array, 0, 0, array.Length);
			optimizedBloodMesh.SetSubMesh(0, new SubMeshDescriptor(0, array.Length));
			optimizedBloodMesh.RecalculateBounds();
			optimizedBloodMesh.Optimize();
			optimizedBloodMesh.UploadMeshData(markNoLongerReadable: false);
		}
		goreStore = base.transform.GetChild(0);
		float num2 = 0f;
		foreach (BSType value in Enum.GetValues(typeof(BSType)))
		{
			if (value != BSType.dontpool && value != BSType.unknown)
			{
				gorePool.Add(value, new Queue<GameObject>());
				num2 += 1f;
			}
		}
		opm = MonoSingleton<OptionsManager>.Instance;
		generateBloodMeshJob = new GenerateBloodMeshJob
		{
			props = props
		};
		InitPools();
	}

	private void PlayBloodSound(Vector3 position)
	{
		if (UnityEngine.Random.value < 0.1f)
		{
			splatterClip.PlayClipAtPoint(goreAudioGroup, position, 256, 1f, 1f, 0.5f, (AudioRolloffMode)0);
		}
	}

	private void Update()
	{
		Shader.SetGlobalFloat("_NormalForgiveness", normalForgiveness);
		MonoSingleton<PortalManagerV2>.Instance.Particles.createBloodHandle.Complete();
		colliderDatas.Clear();
		bool continuousGibCollisions = StockMapInfo.Instance.continuousGibCollisions;
		float num = GetBloodstainChance();
		float num2 = num / 2f;
		BloodstainCreateCommand bloodstainCreateCommand = default(BloodstainCreateCommand);
		while (stainCreateQueue.TryDequeue(ref bloodstainCreateCommand))
		{
			if (!continuousGibCollisions && (float)UnityEngine.Random.Range(0, 100) >= (bloodstainCreateCommand.halfChance ? num2 : num))
			{
				continue;
			}
			ref RaycastHit hit = ref bloodstainCreateCommand.hit;
			if (!colliderDatas.TryGetValue(hit.colliderInstanceID, out var value))
			{
				Rigidbody rigidbody;
				GameObject gameObject;
				if ((rigidbody = hit.rigidbody) != null)
				{
					gameObject = rigidbody.gameObject;
				}
				else
				{
					Collider collider;
					if (!((collider = hit.collider) != null))
					{
						continue;
					}
					gameObject = collider.gameObject;
				}
				CachedColliderData cachedColliderData = (colliderDatas[hit.colliderInstanceID] = new CachedColliderData(cdatabase, in hit, gameObject));
				value = cachedColliderData;
			}
			if (continuousGibCollisions)
			{
				if (value.is7SObject && value.receiver is UnityEngine.Object obj && (bool)obj)
				{
					value.receiver.HandleBloodstainHit(in hit);
				}
				continue;
			}
			if (value.receiver is UnityEngine.Object obj2 && (bool)obj2)
			{
				value.receiver.HandleBloodstainHit(in hit);
				continue;
			}
			Bloodsplatter bloodsplatter = splatters[bloodstainCreateCommand.splatterId];
			if (!bloodsplatter.gz)
			{
				bloodsplatter.gz = GoreZone.ResolveGoreZone(bloodsplatter.transform);
			}
			bool num3 = value.parent;
			BloodstainParent bloodstainParent = value.parent;
			if (!num3)
			{
				bloodstainParent = bloodsplatter.gz.stains;
			}
			bloodstainParent.CreateChild(hit.point, hit.normal, !value.displace, fromStep: false);
		}
		if ((float)sinceLastStep >= 0.128f)
		{
			sinceLastStep = 0f;
			this.ParticleCollisionStep?.Invoke(0.128f);
			this.PostCollisionStep?.Invoke();
		}
		if (goreOn && !usedComputeShadersAtStart && meshDirty)
		{
			RebuildMesh();
		}
	}

	private void RebuildMesh()
	{
		generateBloodMeshJobHandle.Complete();
		totalStainMesh.Clear();
		meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];
		int vertexCount = 4 * currentBloodCount;
		int num = 6 * currentBloodCount;
		bool flag = num <= 65535;
		generateBloodMeshJob.isUInt16 = flag;
		meshData.SetVertexBufferParams(vertexCount, vertexLayoutNonInstanced);
		if (flag)
		{
			meshData.SetIndexBufferParams(num, IndexFormat.UInt16);
			totalStainMesh.SetIndexBufferParams(num, IndexFormat.UInt16);
		}
		else
		{
			meshData.SetIndexBufferParams(num, IndexFormat.UInt32);
			totalStainMesh.SetIndexBufferParams(num, IndexFormat.UInt32);
		}
		generateBloodMeshJob.meshData = meshData;
		generateBloodMeshJobHandle = generateBloodMeshJob.Schedule(currentBloodCount, 256);
	}

	private void LateUpdate()
	{
		if (usedComputeShadersAtStart)
		{
			parentBuffer.SetData(parentMatrices);
			argsData[1] = (uint)currentBloodCount;
			argsBuffer.SetData(argsData);
			if (instanceBufferDirty)
			{
				instanceBuffer.SetData(props);
				instanceBufferDirty = false;
			}
			return;
		}
		generateBloodMeshJobHandle.Complete();
		if (meshDirty && meshDataArray.Length == 1)
		{
			meshDirty = false;
			Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, totalStainMesh);
			totalStainMesh.subMeshCount = 1;
			MeshUpdateFlags meshUpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;
			totalStainMesh.SetSubMesh(0, new SubMeshDescriptor(0, 6 * currentBloodCount), meshUpdateFlags);
			meshDataArray = default(Mesh.MeshDataArray);
			totalStainMesh.RecalculateBounds();
		}
	}

	public void ClearStains()
	{
		propIndex = 0;
		this.StainsCleared?.Invoke();
	}

	private void OnDestroy()
	{
		MonoSingleton<PortalManagerV2>.Instance.Particles.CompleteJobs();
		generateBloodMeshJobHandle.Complete();
		checkpointProps.Dispose();
		props.Dispose();
		parentMatrices.Dispose();
		checkpointParents.Dispose();
		stainCreateQueue.Dispose();
		instanceBuffer?.Release();
		parentBuffer?.Release();
		argsBuffer?.Release();
	}

	private GameObject GetPrefabByBSType(BSType bloodType)
	{
		return bloodType switch
		{
			BSType.head => head, 
			BSType.limb => limb, 
			BSType.body => body, 
			BSType.small => small, 
			BSType.smallest => smallest, 
			BSType.splatter => splatter, 
			BSType.underwater => underwater, 
			BSType.sand => sand, 
			BSType.blessing => blessing, 
			BSType.chestExplosion => chestExplosion, 
			BSType.brainChunk => brainChunk, 
			BSType.skullChunk => skullChunk, 
			BSType.eyeball => eyeball, 
			BSType.jawChunk => jawChunk, 
			BSType.gib => gib[UnityEngine.Random.Range(0, gib.Length)], 
			_ => null, 
		};
	}

	private void InitPools()
	{
		InitPool(BSType.head);
		InitPool(BSType.limb);
		InitPool(BSType.body);
		InitPool(BSType.small);
		InitPool(BSType.splatter);
		InitPool(BSType.underwater);
		InitPool(BSType.smallest);
		InitPool(BSType.sand);
		InitPool(BSType.blessing);
		InitPool(BSType.brainChunk);
		InitPool(BSType.skullChunk);
		InitPool(BSType.eyeball);
		InitPool(BSType.jawChunk);
		InitPool(BSType.gib);
		InitPool(BSType.chestExplosion);
	}

	private void InitPool(BSType bloodSplatterType)
	{
		StartCoroutine(AsyncInit(bloodSplatterType));
	}

	private IEnumerator AsyncInit(BSType bloodSplatterType)
	{
		Queue<GameObject> queue = gorePool[bloodSplatterType];
		GameObject prefabByBSType = GetPrefabByBSType(bloodSplatterType);
		if (prefabByBSType.TryGetComponent<Bloodsplatter>(out var component))
		{
			defaultHPValues.Add(bloodSplatterType, component.hpAmount);
			component.bsm = this;
			prefabByBSType.SetActive(value: false);
		}
		int amount = ((bloodSplatterType == BSType.body) ? 200 : 100);
		if (bloodSplatterType == BSType.gib || bloodSplatterType == BSType.brainChunk || bloodSplatterType == BSType.skullChunk || bloodSplatterType == BSType.eyeball || bloodSplatterType == BSType.jawChunk)
		{
			amount = 200;
		}
		AsyncInstantiateOperation<GameObject> asyncOp = UnityEngine.Object.InstantiateAsync(prefabByBSType, amount, goreStore);
		while (!asyncOp.isDone)
		{
			yield return null;
		}
		GameObject[] result = asyncOp.Result;
		for (int i = 0; i < amount; i++)
		{
			GameObject item = result[i];
			queue.Enqueue(item);
		}
	}

	public void RepoolGore(Bloodsplatter bs, BSType type)
	{
		if (type != BSType.dontpool && defaultHPValues.TryGetValue(type, out var value))
		{
			bs.hpAmount = value;
		}
		RepoolGore(bs.gameObject, type);
	}

	public void RepoolGore(GameObject go, BSType type)
	{
		if ((bool)go)
		{
			if (type != BSType.dontpool)
			{
				ReturnToQueue(go, type);
			}
			else
			{
				UnityEngine.Object.Destroy(go);
			}
		}
	}

	private void ReturnToQueue(GameObject go, BSType type)
	{
		if (type == BSType.unknown || type == BSType.dontpool)
		{
			UnityEngine.Object.Destroy(go);
		}
		go.SetActive(value: false);
		gorePool[type].Enqueue(go);
		go.transform.SetParent(goreStore);
		go.transform.localScale = Vector3.one;
	}

	public GameObject GetFromQueue(BSType type)
	{
		GameObject gameObject = null;
		Queue<GameObject> queue = gorePool[type];
		while (gameObject == null && queue.Count > 0)
		{
			gameObject = queue.Dequeue();
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(GetPrefabByBSType(type), goreStore);
		}
		if (gameObject == null)
		{
			return null;
		}
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public GameObject GetGore(GoreType got, EnemyIdentifier eid, bool fromExplosion = false)
	{
		return GetGore(got, eid.underwater, eid.sandified, eid.blessed, eid, fromExplosion);
	}

	public GameObject GetGore(GoreType got, bool isUnderwater = false, bool isSandified = false, bool isBlessed = false, EnemyIdentifier eid = null, bool fromExplosion = false)
	{
		if (isBlessed)
		{
			GameObject fromQueue = GetFromQueue(BSType.blessing);
			AudioSource component = fromQueue.GetComponent<AudioSource>();
			float splatterWeight = GetSplatterWeight(got);
			component.SetPitch(1.15f + UnityEngine.Random.Range(-0.15f, 0.15f));
			component.volume = splatterWeight * 0.9f + 0.1f;
			fromQueue.transform.localScale *= splatterWeight * splatterWeight * 3f;
			return fromQueue;
		}
		if (isSandified)
		{
			GameObject fromQueue = GetFromQueue(BSType.sand);
			if (got == GoreType.Head)
			{
				return fromQueue;
			}
			AudioSource component2 = fromQueue.GetComponent<AudioSource>();
			AudioSource component3 = fromQueue.transform.GetChild(0).GetComponent<AudioSource>();
			AudioSource originalAudio = GetOriginalAudio(got);
			if ((bool)(UnityEngine.Object)(object)originalAudio)
			{
				component2.clip = originalAudio.clip;
				component2.volume = originalAudio.volume - 0.35f;
				component3.volume = originalAudio.volume - 0.2f;
			}
			return fromQueue;
		}
		switch (got)
		{
		case GoreType.Head:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				PrepareGore(fromQueue, -1, eid, fromExplosion);
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.head);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		case GoreType.Limb:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				fromQueue.transform.localScale *= 0.75f;
				PrepareGore(fromQueue, 20, eid, fromExplosion);
				AudioSource component8 = fromQueue.GetComponent<AudioSource>();
				AudioSource component9 = limb.GetComponent<AudioSource>();
				component8.clip = component9.clip;
				component8.volume = component9.volume;
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.limb);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		case GoreType.Body:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				fromQueue.transform.localScale *= 0.5f;
				PrepareGore(fromQueue, 10, eid, fromExplosion);
				AudioSource component12 = fromQueue.GetComponent<AudioSource>();
				AudioSource component13 = body.GetComponent<AudioSource>();
				component12.clip = component13.clip;
				component12.volume = component13.volume;
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.body);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		case GoreType.Small:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				fromQueue.transform.localScale *= 0.25f;
				PrepareGore(fromQueue, 10, eid, fromExplosion);
				AudioSource component6 = fromQueue.GetComponent<AudioSource>();
				AudioSource component7 = small.GetComponent<AudioSource>();
				component6.clip = component7.clip;
				component6.volume = component7.volume;
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.small);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		case GoreType.Smallest:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				fromQueue.transform.localScale *= 0.15f;
				PrepareGore(fromQueue, 5, eid, fromExplosion);
				AudioSource component10 = fromQueue.GetComponent<AudioSource>();
				AudioSource component11 = smallest.GetComponent<AudioSource>();
				component10.clip = component11.clip;
				component10.volume = component11.volume;
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.smallest);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		case GoreType.Splatter:
		{
			GameObject fromQueue;
			if (isUnderwater)
			{
				fromQueue = GetFromQueue(BSType.underwater);
				PrepareGore(fromQueue, -1, eid, fromExplosion);
				AudioSource component4 = fromQueue.GetComponent<AudioSource>();
				AudioSource component5 = splatter.GetComponent<AudioSource>();
				component4.clip = component5.clip;
				component4.volume = component5.volume;
				return fromQueue;
			}
			fromQueue = GetFromQueue(BSType.splatter);
			PrepareGore(fromQueue, -1, eid, fromExplosion);
			return fromQueue;
		}
		default:
			return null;
		}
	}

	private void PrepareGore(GameObject gob, int healthChange = -1, EnemyIdentifier eid = null, bool fromExplosion = false)
	{
		if ((healthChange >= 0 || !(eid == null) || fromExplosion) && gob.TryGetComponent<Bloodsplatter>(out var component))
		{
			if (healthChange >= 0)
			{
				component.hpAmount = healthChange;
			}
			if ((bool)eid)
			{
				component.eid = eid;
			}
			if (fromExplosion)
			{
				component.fromExplosion = true;
			}
		}
	}

	public GameObject GetGib(BSType type)
	{
		Queue<GameObject> queue = gorePool[type];
		GameObject gameObject = null;
		while (queue.Count > 0 && gameObject == null)
		{
			gameObject = queue.Dequeue();
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(GetPrefabByBSType(type));
		}
		return gameObject;
	}

	private AudioSource GetOriginalAudio(GoreType got)
	{
		return (AudioSource)(got switch
		{
			GoreType.Limb => limb.GetComponent<AudioSource>(), 
			GoreType.Body => body.GetComponent<AudioSource>(), 
			GoreType.Small => small.GetComponent<AudioSource>(), 
			GoreType.Smallest => smallest.GetComponent<AudioSource>(), 
			_ => null, 
		});
	}

	private float GetSplatterWeight(GoreType got)
	{
		return got switch
		{
			GoreType.Limb => 0.75f, 
			GoreType.Body => 0.5f, 
			GoreType.Small => 0.125f, 
			GoreType.Smallest => 0.075f, 
			_ => 1f, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static int CreateBloodstain_0024BurstManaged(ref int propIndex, ref int currentBloodCount, ref NativeArray<InstanceProperties> props, in float3 pos, in float3 norm, bool clipToSurface, int parent = 0)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int result = propIndex;
		props[propIndex] = new InstanceProperties
		{
			pos = pos,
			norm = float3.op_Implicit(math.half3(-norm)),
			parentIndex = parent,
			clipState = (ushort)(clipToSurface ? 1 : 0)
		};
		propIndex = (propIndex + 1) % props.Length;
		currentBloodCount = math.min(currentBloodCount + 1, props.Length);
		return result;
	}
}
