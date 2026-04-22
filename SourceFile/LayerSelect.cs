using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerSelect : MonoBehaviour
{
	public SecretMissionPanel secretMissionPanel;

	public int layerNumber;

	public int levelAmount;

	private float totalScore;

	private float scoresChecked;

	private int perfects;

	public int golds;

	private bool secretMission;

	[HideInInspector]
	public TMP_Text rankText;

	[HideInInspector]
	public Image rankImage;

	[HideInInspector]
	public Sprite rankSpriteOriginal;

	public Sprite rankSpriteOnP;

	public bool gold;

	public bool allPerfects;

	public int trueScore;

	public bool complete;

	public bool noSecretMission;

	[HideInInspector]
	public LevelSelectLeaderboard[] childLeaderboards;

	private Color defaultColor = new Color(0f, 0f, 0f, 0.35f);

	private void Awake()
	{
		childLeaderboards = GetComponentsInChildren<LevelSelectLeaderboard>(includeInactive: true);
		Setup();
	}

	private void Setup()
	{
		if ((Object)(object)rankText == null)
		{
			rankText = base.transform.Find("Header").Find("RankPanel").GetComponentInChildren<TMP_Text>();
		}
		if ((Object)(object)rankImage == null && (bool)(Object)(object)rankText)
		{
			rankImage = rankText.transform.parent.GetComponent<Image>();
		}
		if (rankSpriteOriginal == null && (bool)(Object)(object)rankImage)
		{
			rankSpriteOriginal = rankImage.sprite;
		}
	}

	private void OnDisable()
	{
		totalScore = 0f;
		scoresChecked = 0f;
		perfects = 0;
		golds = 0;
		rankText.text = "";
		((Graphic)rankImage).color = Color.white;
		rankImage.sprite = rankSpriteOriginal;
		secretMission = false;
		((Graphic)GetComponent<Image>()).color = defaultColor;
	}

	public void CheckScore()
	{
		Setup();
		totalScore = 0f;
		trueScore = 0;
		scoresChecked = 0f;
		perfects = 0;
		golds = 0;
		complete = false;
		allPerfects = false;
		gold = false;
		rankText.text = "";
		((Graphic)rankImage).color = Color.white;
		rankImage.sprite = rankSpriteOriginal;
		secretMission = false;
		((Graphic)GetComponent<Image>()).color = defaultColor;
		LevelSelectPanel[] componentsInChildren = GetComponentsInChildren<LevelSelectPanel>(includeInactive: true);
		secretMissionPanel?.GotEnabled();
		LevelSelectPanel[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckScore();
		}
	}

	public void AddScore(int score, bool perfect = false)
	{
		Setup();
		if (golds < levelAmount)
		{
			((Graphic)GetComponent<Image>()).color = defaultColor;
		}
		scoresChecked += 1f;
		totalScore += score;
		if (perfect)
		{
			perfects++;
		}
		if (scoresChecked != (float)levelAmount)
		{
			return;
		}
		complete = true;
		if (perfects == levelAmount)
		{
			rankText.text = "<color=#FFFFFF>P</color>";
			((Graphic)rankImage).color = new Color(1f, 0.686f, 0f, 1f);
			rankImage.sprite = rankSpriteOnP;
			allPerfects = true;
			trueScore = Mathf.RoundToInt(totalScore / (float)levelAmount);
			return;
		}
		trueScore = Mathf.RoundToInt(totalScore / (float)levelAmount);
		float num = totalScore / (float)levelAmount;
		Debug.Log("True Score: " + trueScore + ". Real score: " + num);
		switch (trueScore)
		{
		case 1:
			rankText.text = "<color=#4CFF00>C</color>";
			break;
		case 2:
			rankText.text = "<color=#FFD800>B</color>";
			break;
		case 3:
			rankText.text = "<color=#FF6A00>A</color>";
			break;
		case 4:
		case 5:
		case 6:
			rankText.text = "<color=#FF0000>S</color>";
			break;
		default:
			rankText.text = "<color=#0094FF>D</color>";
			break;
		}
		((Graphic)rankImage).color = Color.white;
		rankImage.sprite = rankSpriteOriginal;
	}

	public void Gold()
	{
		golds++;
		if (golds == levelAmount && levelAmount != 0 && (noSecretMission || secretMission))
		{
			((Graphic)GetComponent<Image>()).color = new Color(1f, 0.686f, 0f, 0.75f);
			gold = true;
		}
	}

	public void SecretMissionDone()
	{
		secretMission = true;
		if (golds == levelAmount && secretMission)
		{
			((Graphic)GetComponent<Image>()).color = new Color(1f, 0.686f, 0f, 0.75f);
		}
	}
}
