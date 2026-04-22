using UnityEngine;

public static class TimeHelper
{
	public static string ConvertSecondsToString(float seconds)
	{
		int num = Mathf.FloorToInt(seconds / 60f);
		string text = (seconds % 60f).ToString("00.000");
		return num + ":" + text;
	}
}
