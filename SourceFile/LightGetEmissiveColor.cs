using UnityEngine;

public class LightGetEmissiveColor : MonoBehaviour
{
	private Light lit;

	[SerializeField]
	private MeshRenderer targetRenderer;

	private MaterialPropertyBlock block;

	private void Start()
	{
		lit = GetComponent<Light>();
		block = new MaterialPropertyBlock();
		targetRenderer.GetPropertyBlock(block);
		lit.color = block.GetColor(UKShaderProperties.EmissiveColor);
	}

	private void Update()
	{
		targetRenderer.GetPropertyBlock(block);
		lit.color = block.GetColor(UKShaderProperties.EmissiveColor);
	}
}
