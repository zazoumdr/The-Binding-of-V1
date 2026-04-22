using System.Collections.Generic;
using UnityEngine;

public class StaticSceneData : ScriptableObject
{
	public Texture2D mainTexAtlas;

	public Texture2D blendTexAtlas;

	public List<Mesh> bakedMeshes = new List<Mesh>();

	public List<int> backingMeshHashes = new List<int>();

	public List<int> mrLightIndices = new List<int>();

	public List<ushort> mrMeshIndices = new List<ushort>();

	public List<ushort> firstSubMesh = new List<ushort>();

	public void ClearData()
	{
		mainTexAtlas = null;
		blendTexAtlas = null;
		bakedMeshes.Clear();
		backingMeshHashes.Clear();
		mrLightIndices.Clear();
		mrMeshIndices.Clear();
		firstSubMesh.Clear();
	}
}
