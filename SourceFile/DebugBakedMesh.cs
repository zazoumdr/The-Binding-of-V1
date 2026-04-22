using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteAlways]
public class DebugBakedMesh : MonoBehaviour
{
	public int lookupIndex;

	public ushort firstSubmesh;

	public ushort subMeshCount;

	public StaticSceneData sceneData;

	private int previousLookupIndex;

	private ushort previousFirstSubmesh;

	private ushort previousSubmeshCount;

	private void Update()
	{
		if (!(sceneData == null) && (lookupIndex != previousLookupIndex || firstSubmesh != previousFirstSubmesh || subMeshCount != previousSubmeshCount))
		{
			SetData();
			previousLookupIndex = lookupIndex;
			previousFirstSubmesh = firstSubmesh;
			previousSubmeshCount = subMeshCount;
		}
	}

	private void SetData()
	{
		GetComponent<MeshFilter>().mesh = sceneData.bakedMeshes[lookupIndex];
		GetComponent<MeshRenderer>();
	}
}
