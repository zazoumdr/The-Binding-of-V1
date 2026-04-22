using UnityEngine;

public static class RankHelper
{
	public static string GetRankLetter(int rank)
	{
		if (rank < 0)
		{
			return "";
		}
		switch (rank)
		{
		case 12:
			return "P";
		case 1:
			return "C";
		case 2:
			return "B";
		case 3:
			return "A";
		case 4:
		case 5:
		case 6:
			return "S";
		default:
			return "D";
		}
	}

	public static Color GetRankBackgroundColor(int rank)
	{
		if (rank != 12)
		{
			return Color.white;
		}
		return new Color(1f, 0.686f, 0f, 1f);
	}

	public static string GetRankForegroundColor(int rank)
	{
		if (rank >= 0)
		{
			switch (rank)
			{
			case 12:
				break;
			case 1:
				return "#4CFF00";
			case 2:
				return "#FFD800";
			case 3:
				return "#FF6A00";
			case 4:
			case 5:
			case 6:
				return "#FF0000";
			default:
				return "#0094FF";
			}
		}
		return "#FFFFFF";
	}
}
