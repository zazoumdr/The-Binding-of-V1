using System;

[Serializable]
public class RankData
{
	public int[] ranks;

	public int secretsAmount;

	public bool[] secretsFound;

	public bool challenge;

	public int levelNumber;

	public bool[] majorAssists;

	public RankScoreData[] stats;

	public RankData(StatsManager sman)
	{
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		levelNumber = sman.levelNumber;
		RankData rank = GameProgressSaver.GetRank(returnNull: true);
		if (rank != null)
		{
			ranks = rank.ranks;
			if (rank.majorAssists != null)
			{
				majorAssists = rank.majorAssists;
			}
			else
			{
				majorAssists = new bool[6];
			}
			if (rank.stats != null)
			{
				stats = rank.stats;
			}
			else
			{
				stats = new RankScoreData[6];
			}
			if ((sman.rankScore >= rank.ranks[num] && (rank.majorAssists == null || (!sman.majorUsed && rank.majorAssists[num]))) || sman.rankScore > rank.ranks[num] || rank.levelNumber != levelNumber)
			{
				majorAssists[num] = sman.majorUsed;
				ranks[num] = sman.rankScore;
				if (stats[num] == null)
				{
					stats[num] = new RankScoreData();
				}
				stats[num].kills = sman.kills;
				stats[num].style = sman.stylePoints;
				stats[num].time = sman.seconds;
			}
			secretsAmount = sman.secretObjects.Length;
			secretsFound = new bool[secretsAmount];
			for (int i = 0; i < secretsAmount && i < rank.secretsFound.Length; i++)
			{
				if (sman.secretObjects[i] == null || rank.secretsFound[i])
				{
					secretsFound[i] = true;
				}
			}
			challenge = rank.challenge;
			return;
		}
		ranks = new int[6];
		stats = new RankScoreData[6];
		if (stats[num] == null)
		{
			stats[num] = new RankScoreData();
		}
		majorAssists = new bool[6];
		for (int j = 0; j < ranks.Length; j++)
		{
			ranks[j] = -1;
		}
		ranks[num] = sman.rankScore;
		majorAssists[num] = sman.majorUsed;
		stats[num].kills = sman.kills;
		stats[num].style = sman.stylePoints;
		stats[num].time = sman.seconds;
		secretsAmount = sman.secretObjects.Length;
		secretsFound = new bool[secretsAmount];
		for (int k = 0; k < secretsAmount; k++)
		{
			if (sman.secretObjects[k] == null)
			{
				secretsFound[k] = true;
			}
		}
	}
}
