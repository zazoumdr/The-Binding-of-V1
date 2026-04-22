using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SeaBodies : MonoBehaviour
{
	[BurstCompile]
	public struct VibrateSeaBodiesJob : IJobParallelFor
	{
		public float intensity;

		public float deltaTime;

		public float3 cameraPosition;

		public NativeArray<float3> originalPos;

		public NativeArray<float3> originalScale;

		public NativeArray<float3> targetPos;

		public NativeArray<float> speeds;

		public NativeArray<float3> currentPos;

		public NativeArray<Matrix4x4> instanceMatrices;

		public NativeArray<Random> randomArray;

		public void Execute(int i)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			float3 val = currentPos[i];
			float3 val2 = targetPos[i];
			float num = speeds[i];
			float3 val3 = originalPos[i];
			Random value = randomArray[i];
			math.distance(val, val2);
			if (math.distance(val, val2) < 0.001f)
			{
				val = val2;
				float3 val4 = ((Random)(ref value)).NextFloat3Direction() * intensity;
				val2 = val3 + val4;
				targetPos[i] = val2;
			}
			else
			{
				val = float3.op_Implicit(Vector3.MoveTowards(float3.op_Implicit(val), float3.op_Implicit(val2), num * deltaTime));
			}
			currentPos[i] = val;
			float3 val5 = new float3(cameraPosition.x, val.y, cameraPosition.z) - val;
			quaternion val6 = quaternion.identity;
			if (math.lengthsq(val5) > 0.0001f)
			{
				val6 = quaternion.LookRotationSafe(val5, math.up());
			}
			randomArray[i] = value;
			instanceMatrices[i] = Matrix4x4.TRS(float3.op_Implicit(val), quaternion.op_Implicit(val6), float3.op_Implicit(originalScale[i]));
		}
	}

	public float intensity = 1f;

	public float speedMin = 4f;

	public float speedMax = 5f;

	public Texture2D textureAtlas;

	private NativeArray<float3> originalPositions;

	private NativeArray<float3> originalScales;

	private NativeArray<float3> targetPositions;

	private NativeArray<float> speeds;

	private NativeArray<float3> currentPositions;

	private NativeArray<Random> randomStates;

	private NativeArray<Matrix4x4> instanceMatricesNative;

	private JobHandle jobHandle;

	public Mesh seaBodyMesh;

	public int atlasCount = 2;

	public Material seaBodyMaterial;

	private int[] instanceAtlasOffset;

	private Vector4[] instanceColors;

	private int instanceCount;

	private MaterialPropertyBlock mpb;

	private float[] atlasOffsetBuffer = new float[1023];

	private Vector4[] colorsBuffer = new Vector4[1023];

	private Matrix4x4[] subMatrices = new Matrix4x4[1023];

	private void Start()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		List<Transform> leafChildrenOfAllChunks = GetLeafChildrenOfAllChunks(base.transform);
		seaBodyMaterial.SetFloat("_AtlasCount", atlasCount);
		instanceCount = leafChildrenOfAllChunks.Count;
		originalPositions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
		originalScales = new NativeArray<float3>(instanceCount, Allocator.Persistent);
		targetPositions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
		speeds = new NativeArray<float>(instanceCount, Allocator.Persistent);
		randomStates = new NativeArray<Random>(instanceCount, Allocator.Persistent);
		currentPositions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
		instanceMatricesNative = new NativeArray<Matrix4x4>(instanceCount, Allocator.Persistent);
		instanceAtlasOffset = new int[instanceCount];
		instanceColors = new Vector4[instanceCount];
		uint num = 12345u;
		for (int i = 0; i < instanceCount; i++)
		{
			SpriteRenderer component = leafChildrenOfAllChunks[i].GetComponent<SpriteRenderer>();
			instanceColors[i] = component.color;
			instanceAtlasOffset[i] = ((!component.sprite.name.Contains("1")) ? 1 : 0);
			float3 val = float3.op_Implicit(leafChildrenOfAllChunks[i].position);
			originalPositions[i] = val;
			targetPositions[i] = val;
			speeds[i] = Random.Range(speedMin, speedMax);
			quaternion val2 = quaternion.op_Implicit(leafChildrenOfAllChunks[i].rotation);
			currentPositions[i] = val;
			float3 val3 = float3.op_Implicit(leafChildrenOfAllChunks[i].lossyScale) * new float3(1f, 2f, 1f);
			originalScales[i] = val3;
			instanceMatricesNative[i] = Matrix4x4.TRS(float3.op_Implicit(val), quaternion.op_Implicit(val2), float3.op_Implicit(val3));
			randomStates[i] = new Random((uint)((int)(num + i * 31) | 1));
			component.enabled = false;
		}
	}

	private static List<Transform> FindAllChildrenContainingName(Transform parent, string substring)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in parent)
		{
			if (item.name.Contains(substring))
			{
				list.Add(item);
			}
			list.AddRange(FindAllChildrenContainingName(item, substring));
		}
		return list;
	}

	public static List<Transform> GetAllLeafChildren(Transform parent)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in parent)
		{
			if (item.childCount == 0)
			{
				list.Add(item);
			}
			else
			{
				list.AddRange(GetAllLeafChildren(item));
			}
		}
		return list;
	}

	public static List<Transform> GetLeafChildrenOfAllChunks(Transform root)
	{
		List<Transform> list = FindAllChildrenContainingName(root, "Chunk");
		List<Transform> list2 = new List<Transform>();
		foreach (Transform item in list)
		{
			list2.AddRange(GetAllLeafChildren(item));
		}
		return list2;
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		CameraController instance = MonoSingleton<CameraController>.Instance;
		if ((bool)instance)
		{
			VibrateSeaBodiesJob jobData = new VibrateSeaBodiesJob
			{
				intensity = intensity,
				deltaTime = Time.deltaTime,
				cameraPosition = float3.op_Implicit(instance.cam.transform.position),
				originalPos = originalPositions,
				originalScale = originalScales,
				targetPos = targetPositions,
				speeds = speeds,
				currentPos = currentPositions,
				instanceMatrices = instanceMatricesNative,
				randomArray = randomStates
			};
			jobHandle = jobData.Schedule(instanceCount, 64);
		}
	}

	private void LateUpdate()
	{
		jobHandle.Complete();
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		int num = 0;
		int num2 = instanceCount;
		while (num2 > 0)
		{
			int num3 = Mathf.Min(1023, num2);
			for (int i = 0; i < num3; i++)
			{
				int num4 = num + i;
				atlasOffsetBuffer[i] = instanceAtlasOffset[num4];
				colorsBuffer[i] = instanceColors[num4];
				subMatrices[i] = instanceMatricesNative[num4];
			}
			mpb.Clear();
			mpb.SetFloatArray("_AtlasOffsetArray", atlasOffsetBuffer);
			mpb.SetVectorArray("_PerInstanceColor", colorsBuffer);
			Graphics.DrawMeshInstanced(seaBodyMesh, 0, seaBodyMaterial, subMatrices, num3, mpb, ShadowCastingMode.Off, receiveShadows: false);
			num += num3;
			num2 -= num3;
		}
	}

	private void OnDestroy()
	{
		originalPositions.Dispose();
		originalScales.Dispose();
		targetPositions.Dispose();
		speeds.Dispose();
		currentPositions.Dispose();
		randomStates.Dispose();
		instanceMatricesNative.Dispose();
	}
}
