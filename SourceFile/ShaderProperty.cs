using System;
using UnityEngine;

[Serializable]
public class ShaderProperty
{
	public string name;

	public bool setFloat = true;

	public float floatValue;

	public bool setInt = true;

	public int intValue;

	public bool setVector = true;

	public Vector4 vectorValue = Vector4.zero;

	public bool setColor = true;

	public Color colorValue = Color.black;

	public void Set(Material material)
	{
		if (setFloat)
		{
			material.SetFloat(name, floatValue);
		}
		if (setInt)
		{
			material.SetInt(name, intValue);
		}
		if (setVector)
		{
			material.SetVector(name, vectorValue);
		}
		if (setColor)
		{
			material.SetColor(name, colorValue);
		}
	}
}
