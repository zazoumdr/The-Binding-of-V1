using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretMissionPanel : MonoBehaviour
{
	public LayerSelect layerSelect;

	public int missionNumber;

	[HideInInspector]
	public Image img;

	[HideInInspector]
	public Sprite origSprite;

	public Sprite spriteOnComplete;

	[HideInInspector]
	public TMP_Text txt;

	[HideInInspector]
	public Button btn;

	private void Start()
	{
		GotEnabled();
	}

	private void OnEnable()
	{
		GotEnabled();
	}

	private void Setup()
	{
		if ((Object)(object)img == null)
		{
			img = GetComponent<Image>();
		}
		if (origSprite == null && (bool)(Object)(object)img)
		{
			origSprite = img.sprite;
		}
		if ((Object)(object)txt == null)
		{
			txt = GetComponentInChildren<TMP_Text>();
		}
		if ((Object)(object)btn == null)
		{
			btn = GetComponent<Button>();
		}
	}

	public void GotEnabled()
	{
		Setup();
		switch (GameProgressSaver.GetSecretMission(missionNumber))
		{
		case 2:
			img.sprite = spriteOnComplete;
			((Graphic)txt).color = Color.black;
			((Selectable)btn).interactable = true;
			layerSelect.SecretMissionDone();
			break;
		case 1:
			img.sprite = origSprite;
			((Graphic)txt).color = Color.white;
			((Selectable)btn).interactable = true;
			break;
		default:
			img.sprite = origSprite;
			((Graphic)txt).color = new Color(0.5f, 0.5f, 0.5f);
			((Selectable)btn).interactable = false;
			break;
		}
	}
}
