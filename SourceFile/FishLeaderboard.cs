using System.Text;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;

public class FishLeaderboard : MonoBehaviour
{
	[SerializeField]
	private TMP_Text globalText;

	[SerializeField]
	private TMP_Text friendsText;

	private void OnEnable()
	{
		Fetch();
	}

	private async void Fetch()
	{
		LeaderboardEntry[] array = await MonoSingleton<LeaderboardController>.Instance.GetFishScores(LeaderboardType.Global);
		StringBuilder strBlrd = new StringBuilder();
		Friend user;
		if (array != null)
		{
			strBlrd.AppendLine("<b>GLOBAL</b>");
			int num = 1;
			LeaderboardEntry[] array2 = array;
			foreach (LeaderboardEntry val in array2)
			{
				user = val.User;
				string arg = ((Friend)(ref user)).Name;
				user = val.User;
				if (((Friend)(ref user)).IsMe)
				{
					strBlrd.Append("<color=orange>");
				}
				strBlrd.Append("<noparse>");
				string text = $"[{num}] {val.Score} - {arg}";
				if (text.Length > 25)
				{
					text = text.Substring(0, 25);
				}
				strBlrd.AppendLine(text);
				strBlrd.Append("</noparse>");
				user = val.User;
				if (((Friend)(ref user)).IsMe)
				{
					strBlrd.Append("</color>");
				}
				num++;
			}
		}
		else
		{
			strBlrd.Append("Error fetching leaderboard data.");
		}
		globalText.text = strBlrd.ToString();
		LeaderboardEntry[] array3 = await MonoSingleton<LeaderboardController>.Instance.GetFishScores(LeaderboardType.Friends);
		strBlrd.Clear();
		if (array3 != null)
		{
			strBlrd.AppendLine("<b>FRIENDS</b>");
			LeaderboardEntry[] array2 = array3;
			foreach (LeaderboardEntry val2 in array2)
			{
				user = val2.User;
				string arg2 = ((Friend)(ref user)).Name;
				user = val2.User;
				if (((Friend)(ref user)).IsMe)
				{
					strBlrd.Append("<color=orange>");
				}
				strBlrd.Append("<noparse>");
				string text2 = $"[{val2.GlobalRank}] {val2.Score} - {arg2}";
				if (text2.Length > 25)
				{
					text2 = text2.Substring(0, 25);
				}
				strBlrd.AppendLine(text2);
				strBlrd.Append("</noparse>");
				user = val2.User;
				if (((Friend)(ref user)).IsMe)
				{
					strBlrd.Append("</color>");
				}
			}
		}
		else
		{
			strBlrd.Append("Error fetching leaderboard data.");
		}
		friendsText.text = strBlrd.ToString();
	}
}
