using System.Collections.Generic;
using UnityEngine;

public class BakeVertexLights : MonoBehaviour
{
	public List<Renderer> bakedRenderers;

	private MaterialPropertyBlock[] rendPropBlocks;

	[HideInInspector]
	public int UVTargetChannel = 2;

	private float _strength;

	public float Strength
	{
		get
		{
			return _strength;
		}
		set
		{
			if (_strength != value)
			{
				_strength = value;
				UpdateChannelStrength(UVTargetChannel, _strength);
			}
		}
	}

	private void Start()
	{
		rendPropBlocks = new MaterialPropertyBlock[bakedRenderers.Count];
		for (int i = 0; i < bakedRenderers.Count; i++)
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			bakedRenderers[i].GetPropertyBlock(materialPropertyBlock);
			MonoBehaviour.print(materialPropertyBlock.isEmpty);
			rendPropBlocks[i] = materialPropertyBlock;
		}
	}

	private void Update()
	{
		float value = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
		float value2 = Mathf.Sin(Time.time * 10f + 3.14f) * 0.5f + 0.5f;
		for (int i = 0; i < bakedRenderers.Count; i++)
		{
			MaterialPropertyBlock materialPropertyBlock = rendPropBlocks[i];
			materialPropertyBlock.SetFloat("_BakedLights1Strength", value);
			materialPropertyBlock.SetFloat("_BakedLights2Strength", value2);
			bakedRenderers[i].SetPropertyBlock(materialPropertyBlock);
		}
	}

	private void UpdateChannelStrength(int targetChannel, float strength)
	{
		int num = Mathf.Clamp(targetChannel - 2, 1, 6);
		string text = $"_BakedLights{num}Strength";
		strength = Mathf.Clamp01(strength);
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		foreach (Renderer bakedRenderer in bakedRenderers)
		{
			bakedRenderer.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat(text, strength);
			bakedRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
