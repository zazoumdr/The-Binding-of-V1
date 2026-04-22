using System.Collections.Generic;
using UnityEngine;

public class SimpleMeshCombiner : MonoBehaviour
{
	public bool removeAllChildren = true;

	public void CombineMeshes()
	{
		Transform parent = base.transform.parent;
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		Vector3 localScale = base.transform.localScale;
		base.transform.parent = null;
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>(includeInactive: true);
		Dictionary<Material, List<CombineInstance>> dictionary = new Dictionary<Material, List<CombineInstance>>();
		MeshFilter[] array = componentsInChildren;
		foreach (MeshFilter meshFilter in array)
		{
			if (meshFilter.sharedMesh == null)
			{
				continue;
			}
			if (meshFilter.gameObject.isStatic)
			{
				Debug.LogWarning("cannot process static mesh " + meshFilter.gameObject);
				continue;
			}
			MeshRenderer component = meshFilter.GetComponent<MeshRenderer>();
			if (component == null || component.sharedMaterials.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < meshFilter.sharedMesh.subMeshCount; j++)
			{
				Material key = component.sharedMaterials[j];
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new List<CombineInstance>();
				}
				CombineInstance item = new CombineInstance
				{
					mesh = meshFilter.sharedMesh,
					subMeshIndex = j,
					transform = meshFilter.transform.localToWorldMatrix
				};
				dictionary[key].Add(item);
			}
		}
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		foreach (KeyValuePair<Material, List<CombineInstance>> item2 in dictionary)
		{
			List<CombineInstance> value = item2.Value;
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(value.ToArray(), mergeSubMeshes: true, useMatrices: true);
			list.Add(new CombineInstance
			{
				mesh = mesh,
				subMeshIndex = 0,
				transform = Matrix4x4.identity
			});
			list2.Add(item2.Key);
		}
		Mesh mesh2 = new Mesh();
		mesh2.CombineMeshes(list.ToArray(), mergeSubMeshes: false, useMatrices: false);
		MeshFilter meshFilter2 = base.gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		meshFilter2.sharedMesh = mesh2;
		meshRenderer.materials = list2.ToArray();
		base.transform.SetParent(parent, worldPositionStays: false);
		base.transform.SetPositionAndRotation(position, rotation);
		base.transform.localScale = localScale;
		if (removeAllChildren)
		{
			for (int k = 0; k < base.transform.childCount; k++)
			{
				Object.Destroy(base.transform.GetChild(k).gameObject);
			}
		}
	}
}
