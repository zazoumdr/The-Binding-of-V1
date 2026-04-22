using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManagementEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text levelNameLabel;

	[SerializeField]
	private TMP_Text anyPercentScoreLabel;

	[SerializeField]
	private TMP_Text pRankScoreLabel;

	public Button anyPercentResetButton;

	public Button pRankResetButton;

	[Header("Colors")]
	[SerializeField]
	private Color normalScoreColor = Color.white;

	[SerializeField]
	private Color noScoreColor = new Color(0.29f, 0.29f, 0.29f);

	[SerializeField]
	private Color invalidScoreColor = Color.red;

	[HideInInspector]
	public int missionNumber;

	public void UpdateEntry(int missionNum, LeaderboardManagementScreen.CachedLevelScore cached)
	{
		string missionNumberOnly = GetMissionName.GetMissionNumberOnly(missionNum);
		levelNameLabel.text = "Level " + missionNumberOnly;
		bool hasValue = cached.anyPercentScore.HasValue;
		string text = (hasValue ? FormatTime(cached.anyPercentScore.Value) : "---");
		Color color = (cached.anyPercentInvalid ? invalidScoreColor : (hasValue ? normalScoreColor : noScoreColor));
		anyPercentScoreLabel.text = text;
		((Graphic)anyPercentScoreLabel).color = color;
		bool hasValue2 = cached.pRankScore.HasValue;
		string text2 = (hasValue2 ? FormatTime(cached.pRankScore.Value) : "---");
		Color color2 = (cached.pRankInvalid ? invalidScoreColor : (hasValue2 ? normalScoreColor : noScoreColor));
		pRankScoreLabel.text = text2;
		((Graphic)pRankScoreLabel).color = color2;
		GameObject obj = ((Component)(object)anyPercentResetButton).gameObject;
		int? anyPercentScore = cached.anyPercentScore;
		obj.SetActive(anyPercentScore.HasValue);
		GameObject obj2 = ((Component)(object)pRankResetButton).gameObject;
		anyPercentScore = cached.pRankScore;
		obj2.SetActive(anyPercentScore.HasValue);
	}

	private static string FormatTime(int milliseconds)
	{
		int num = milliseconds / 60000;
		float num2 = (float)(milliseconds - num * 60000) / 1000f;
		return $"{num}:{num2:00.000}";
	}
}
