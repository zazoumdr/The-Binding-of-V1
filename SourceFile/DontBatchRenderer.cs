using UnityEngine;

public class DontBatchRenderer : MonoBehaviour
{
	private void Start()
	{
		Renderer component = GetComponent<Renderer>();
		int num = component.sharedMaterials.Length;
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		int num2 = Random.Range(0, 99999999);
		int num3 = 0;
		if (num3 < num)
		{
			component.GetPropertyBlock(materialPropertyBlock, num3);
			materialPropertyBlock.SetInteger("_BatchingID", num2 + num3);
			component.SetPropertyBlock(materialPropertyBlock, num3);
		}
	}
}
