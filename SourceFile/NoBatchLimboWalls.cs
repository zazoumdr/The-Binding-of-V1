using UnityEngine;

public class NoBatchLimboWalls : MonoBehaviour
{
	private void Start()
	{
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>(includeInactive: true);
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			MeshRenderer meshRenderer = array[i];
			Material[] sharedMaterials = array[i].sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (material != null && material.IsKeywordEnabled("LIMBO_WALLS") && material.GetFloat("_OffAxisPanelFeature") == 1f)
				{
					meshRenderer.GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetInteger("_BatchingID", num);
					meshRenderer.SetPropertyBlock(materialPropertyBlock);
					num++;
					break;
				}
			}
		}
	}
}
