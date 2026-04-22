using System;
using System.Text;

namespace SettingsMenu.Models;

[Serializable]
public class PreferenceEntry
{
	public PreferenceKey key;

	public PreferenceType type;

	public bool boolValue;

	public int intValue;

	public float floatValue;

	public bool IsBool()
	{
		return type == PreferenceType.Bool;
	}

	public bool IsInt()
	{
		return type == PreferenceType.Int;
	}

	public bool IsFloat()
	{
		return type == PreferenceType.Float;
	}

	public void Apply()
	{
		switch (type)
		{
		case PreferenceType.Bool:
			key.SetBoolValue(boolValue);
			break;
		case PreferenceType.Int:
			key.SetIntValue(intValue);
			break;
		case PreferenceType.Float:
			key.SetFloatValue(floatValue);
			break;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"[{key}] ({type}) = ");
		switch (type)
		{
		case PreferenceType.Bool:
			stringBuilder.Append(boolValue);
			break;
		case PreferenceType.Int:
			stringBuilder.Append(intValue);
			break;
		case PreferenceType.Float:
			stringBuilder.Append(floatValue);
			break;
		}
		return stringBuilder.ToString();
	}
}
