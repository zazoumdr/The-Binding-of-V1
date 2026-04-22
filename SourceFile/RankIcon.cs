using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankIcon : MonoBehaviour
{
	[SerializeField]
	private bool useDefaultRank;

	[SerializeField]
	[Range(0f, 12f)]
	private int defaultRank;

	[SerializeField]
	private TMP_Text mainRankLetter;

	[SerializeField]
	private Image mainRankBackground;

	private void Start()
	{
		if (useDefaultRank)
		{
			SetRank(defaultRank);
		}
	}

	public void SetRank(int rank)
	{
		base.gameObject.SetActive(value: true);
		mainRankLetter.text = "<color=" + RankHelper.GetRankForegroundColor(rank) + ">" + RankHelper.GetRankLetter(rank) + "</color>";
		mainRankBackground.fillCenter = rank == 12;
		((Graphic)mainRankBackground).color = RankHelper.GetRankBackgroundColor(rank);
	}

	public void SetEmpty()
	{
		base.gameObject.SetActive(value: true);
		mainRankLetter.text = string.Empty;
		mainRankBackground.fillCenter = false;
		((Graphic)mainRankBackground).color = Color.white;
	}
}
