using UnityEngine;
using UnityEngine.Events;

public class PlayerPrefButton : MonoBehaviour
{
	[Header("ONLY FOR CYBER GRIND THEME FUCK YOU FIELD IGNORED FUCK YOU")]
	public string playerPref;

	public int defaultValue;

	public UnityEvent[] onIntValues;

	private void Start()
	{
		CheckPref();
	}

	public void SetValue(int value)
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("cyberGrind.theme", value);
		CheckPref();
	}

	public void CheckPref()
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("cyberGrind.theme", defaultValue);
		if (onIntValues != null && onIntValues.Length != 0)
		{
			if (num < onIntValues.Length)
			{
				onIntValues[num]?.Invoke();
			}
			else if (num == 0)
			{
				onIntValues[0]?.Invoke();
			}
			else
			{
				onIntValues[onIntValues.Length - 1]?.Invoke();
			}
		}
	}
}
