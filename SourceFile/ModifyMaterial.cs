using UnityEngine;

public class ModifyMaterial : MonoBehaviour
{
	private Renderer rend;

	private MaterialPropertyBlock block;

	private bool valuesSet;

	public void ChangeEmissionIntensity(float value)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetFloat(UKShaderProperties.EmissiveIntensity, value);
			rend.SetPropertyBlock(block, i);
		}
	}

	public void ChangeEmissionColor(string hex)
	{
		if (ColorUtility.TryParseHtmlString(hex, out var color))
		{
			ChangeEmissionColor(color);
		}
		else
		{
			Debug.LogError("Failed to Change Emission Color: " + hex);
		}
	}

	public void ChangeEmissionColor(Color clr)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetColor(UKShaderProperties.EmissiveColor, clr);
			rend.SetPropertyBlock(block, i);
		}
	}

	public void ChangeColor(string hex)
	{
		if (ColorUtility.TryParseHtmlString(hex, out var color))
		{
			ChangeColor(color);
		}
		else
		{
			Debug.LogError("Failed to Change Color: " + hex);
		}
	}

	public void ChangeColor(Color clr)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetColor("Color", clr);
			rend.SetPropertyBlock(block, i);
		}
	}

	public void ChangeColorToWhite()
	{
		ChangeColor(Color.white);
	}

	public void ChangeColorToBlack()
	{
		ChangeColor(Color.black);
	}

	public void ChangeAlpha(float value)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			Color color = block.GetColor("Color");
			color.a = value;
			block.SetColor("Color", color);
			rend.SetPropertyBlock(block, i);
		}
	}

	public void SetSandified(bool sandified)
	{
		SetValues();
		for (int i = 0; i < rend.materials.Length; i++)
		{
			rend.GetPropertyBlock(block, i);
			block.SetFloat(Shader.PropertyToID("_HasSandBuff"), sandified ? 1 : 0);
			block.SetFloat(Shader.PropertyToID("_Sanded"), sandified ? 1 : 0);
			rend.SetPropertyBlock(block, i);
		}
	}

	private void SetValues()
	{
		if (!valuesSet)
		{
			valuesSet = true;
			block = new MaterialPropertyBlock();
			rend = GetComponent<Renderer>();
		}
	}
}
