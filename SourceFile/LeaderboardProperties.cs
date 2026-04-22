using UnityEngine;
using UnityEngine.UI;

public static class LeaderboardProperties
{
	public const string CyberGrindWavePrefix = "Cyber Grind Wave ";

	public const string PrecisePostfix = " Precise";

	public const string FishSize = "Fish Size";

	public static readonly string[] Difficulties = new string[6]
	{
		"Harmless",
		"Lenient",
		"Standard",
		"Violent",
		"Brutal",
		string.Empty
	};

	public const string AnyPercentPostfix = " Any%";

	public const string PRankPostfix = " PRank";

	public static void ScrollToEntry(ScrollRect scrollRect, int entryIndex, int entryCount)
	{
		if (entryIndex >= 0 && entryCount > 0)
		{
			float height = scrollRect.content.rect.height;
			float height2 = scrollRect.viewport.rect.height;
			float num = height - height2;
			if (num > 0f)
			{
				float num2 = height / (float)entryCount;
				float value = (float)entryIndex * num2 - height2 / 2f + num2 / 2f;
				value = Mathf.Clamp(value, 0f, num);
				scrollRect.verticalNormalizedPosition = 1f - value / num;
				return;
			}
		}
		scrollRect.verticalNormalizedPosition = 1f;
	}
}
