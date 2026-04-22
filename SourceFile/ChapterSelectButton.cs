using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterSelectButton : MonoBehaviour
{
	public LayerSelect[] layersInChapter;

	public TMP_Text rankText;

	private Image buttonBg;

	private Sprite originalSprite;

	[SerializeField]
	private Sprite buttonOnP;

	private Image rankButton;

	private Sprite originalRankSprite;

	[SerializeField]
	private Sprite rankOnP;

	public int totalScore;

	public bool notComplete;

	public int golds;

	public int allPerfects;

	private void Awake()
	{
		buttonBg = GetComponent<Image>();
		originalSprite = buttonBg.sprite;
		rankButton = rankText.transform.parent.GetComponent<Image>();
		originalRankSprite = rankButton.sprite;
	}

	private void OnEnable()
	{
		CheckScore();
	}

	private void OnDisable()
	{
		totalScore = 0;
		notComplete = false;
		golds = 0;
		allPerfects = 0;
		((Graphic)buttonBg).color = Color.white;
		buttonBg.sprite = originalSprite;
		rankText.text = "";
		((Graphic)rankButton).color = Color.white;
		rankButton.sprite = originalRankSprite;
	}

	public void CheckScore()
	{
		totalScore = 0;
		notComplete = false;
		golds = 0;
		allPerfects = 0;
		if ((Object)(object)buttonBg == null)
		{
			buttonBg = GetComponent<Image>();
		}
		((Graphic)buttonBg).color = Color.white;
		buttonBg.sprite = originalSprite;
		LayerSelect[] array = layersInChapter;
		foreach (LayerSelect layerSelect in array)
		{
			layerSelect.CheckScore();
			totalScore += layerSelect.trueScore;
			if (!layerSelect.complete)
			{
				notComplete = true;
			}
			if (layerSelect.allPerfects)
			{
				allPerfects++;
			}
			if (layerSelect.gold)
			{
				golds++;
			}
		}
		if (notComplete)
		{
			return;
		}
		if (allPerfects == layersInChapter.Length)
		{
			rankText.text = "<color=#FFFFFF>P</color>";
			((Graphic)rankButton).color = new Color(1f, 0.686f, 0f, 1f);
			rankButton.sprite = rankOnP;
			if (golds == layersInChapter.Length)
			{
				((Graphic)buttonBg).color = new Color(1f, 0.686f, 0f, 1f);
				buttonBg.sprite = buttonOnP;
			}
			return;
		}
		totalScore /= layersInChapter.Length;
		switch (totalScore)
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
		case 7:
		case 8:
		case 9:
			rankText.text = "<color=#FF0000>S</color>";
			break;
		default:
			rankText.text = "<color=#0094FF>D</color>";
			break;
		}
		((Graphic)rankButton).color = Color.white;
		rankButton.sprite = originalRankSprite;
	}
}
