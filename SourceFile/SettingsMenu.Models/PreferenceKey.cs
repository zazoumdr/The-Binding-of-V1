using System;

namespace SettingsMenu.Models;

[Serializable]
public struct PreferenceKey
{
	public string key;

	public bool isLocal;

	public readonly bool IsValid()
	{
		return !string.IsNullOrEmpty(key);
	}

	public readonly bool GetBoolValue(bool fallbackValue = false)
	{
		if (!isLocal)
		{
			return MonoSingleton<PrefsManager>.Instance.GetBool(key, fallbackValue);
		}
		return MonoSingleton<PrefsManager>.Instance.GetBoolLocal(key, fallbackValue);
	}

	public readonly int GetIntValue(int fallbackValue = 0)
	{
		if (!isLocal)
		{
			return MonoSingleton<PrefsManager>.Instance.GetInt(key, fallbackValue);
		}
		return MonoSingleton<PrefsManager>.Instance.GetIntLocal(key, fallbackValue);
	}

	public readonly float GetFloatValue(float fallbackValue = 0f)
	{
		if (!isLocal)
		{
			return MonoSingleton<PrefsManager>.Instance.GetFloat(key, fallbackValue);
		}
		return MonoSingleton<PrefsManager>.Instance.GetFloatLocal(key, fallbackValue);
	}

	public readonly string GetStringValue(string fallbackValue = "")
	{
		if (!isLocal)
		{
			return MonoSingleton<PrefsManager>.Instance.GetString(key, fallbackValue);
		}
		return MonoSingleton<PrefsManager>.Instance.GetStringLocal(key, fallbackValue);
	}

	public readonly void SetBoolValue(bool value)
	{
		if (isLocal)
		{
			MonoSingleton<PrefsManager>.Instance.SetBoolLocal(key, value);
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetBool(key, value);
		}
	}

	public readonly void SetIntValue(int value)
	{
		if (isLocal)
		{
			MonoSingleton<PrefsManager>.Instance.SetIntLocal(key, value);
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetInt(key, value);
		}
	}

	public readonly void SetFloatValue(float value)
	{
		if (isLocal)
		{
			MonoSingleton<PrefsManager>.Instance.SetFloatLocal(key, value);
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetFloat(key, value);
		}
	}

	public readonly void SetStringValue(string value)
	{
		if (isLocal)
		{
			MonoSingleton<PrefsManager>.Instance.SetStringLocal(key, value);
		}
		else
		{
			MonoSingleton<PrefsManager>.Instance.SetString(key, value);
		}
	}

	public readonly void SetValue<T>(T value)
	{
		if (!(value is bool boolValue))
		{
			if (!(value is int intValue))
			{
				if (!(value is float floatValue))
				{
					if (!(value is string stringValue))
					{
						throw new ArgumentException("Unsupported value type");
					}
					SetStringValue(stringValue);
				}
				else
				{
					SetFloatValue(floatValue);
				}
			}
			else
			{
				SetIntValue(intValue);
			}
		}
		else
		{
			SetBoolValue(boolValue);
		}
	}

	public override string ToString()
	{
		if (isLocal)
		{
			return "(Local)" + key;
		}
		return key;
	}
}
