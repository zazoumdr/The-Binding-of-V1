using UnityEngine;

public class PulsateEmissiveMaterial : MonoBehaviour
{
	[HideInInspector]
	public bool valuesSet;

	[HideInInspector]
	public MeshRenderer rend;

	[HideInInspector]
	public Material[] sharedMaterials;

	[HideInInspector]
	public MaterialPropertyBlock block;

	[HideInInspector]
	public int emissiveID;

	[HideInInspector]
	public float defaultIntensity;

	[HideInInspector]
	public float targetIntensity;

	[HideInInspector]
	public float currentIntensity;

	public float intensityRange;

	public float pulseSpeed;

	private void Start()
	{
		if (!valuesSet)
		{
			SetValues();
		}
	}

	private void Update()
	{
		currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, pulseSpeed * Time.deltaTime);
		if (currentIntensity == targetIntensity)
		{
			targetIntensity = ((targetIntensity > defaultIntensity) ? (defaultIntensity - intensityRange) : (defaultIntensity + intensityRange));
		}
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetFloat(emissiveID, currentIntensity);
			rend.SetPropertyBlock(block, i);
		}
	}

	private void SetValues()
	{
		if (valuesSet)
		{
			return;
		}
		valuesSet = true;
		rend = GetComponent<MeshRenderer>();
		sharedMaterials = rend.sharedMaterials;
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		emissiveID = Shader.PropertyToID("_EmissiveIntensity");
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if (sharedMaterials[i] != null && sharedMaterials[i].HasProperty(emissiveID))
			{
				defaultIntensity = sharedMaterials[i].GetFloat(emissiveID);
				currentIntensity = defaultIntensity;
				targetIntensity = ((Random.Range(0, 2) == 1) ? (targetIntensity + intensityRange) : (targetIntensity - intensityRange));
				break;
			}
		}
	}
}
