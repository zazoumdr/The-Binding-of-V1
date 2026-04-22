using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SavedAlterOption
{
	public string Key;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public float? FloatValue;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public bool? BoolValue;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public Vector3? VectorData;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int? IntValue;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		stringBuilder.Append(Key);
		stringBuilder.Append(") ");
		if (FloatValue.HasValue)
		{
			stringBuilder.Append(FloatValue.Value);
		}
		else if (BoolValue.HasValue)
		{
			stringBuilder.Append(BoolValue.Value);
		}
		else if (VectorData.HasValue)
		{
			stringBuilder.Append(VectorData.Value);
		}
		else if (IntValue.HasValue)
		{
			stringBuilder.Append(IntValue.Value);
		}
		else
		{
			stringBuilder.Append("null");
		}
		return stringBuilder.ToString();
	}
}
