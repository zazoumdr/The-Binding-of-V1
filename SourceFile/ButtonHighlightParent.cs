using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHighlightParent : MonoBehaviour
{
	private Image[] buttons;

	private TMP_Text[] buttonTexts;

	private Sprite[] buttonSprites;

	[SerializeField]
	private Sprite pressedVersion;

	[SerializeField]
	private Image targetOnStart;

	private void Start()
	{
		buttons = GetComponentsInChildren<Image>();
		buttonTexts = (TMP_Text[])(object)new TMP_Text[buttons.Length];
		buttonSprites = new Sprite[buttons.Length];
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonTexts[i] = ((Component)(object)buttons[i]).GetComponentInChildren<TMP_Text>();
			buttonSprites[i] = buttons[i].sprite;
		}
		if ((bool)(Object)(object)targetOnStart)
		{
			ChangeButton(targetOnStart);
		}
	}

	public void ChangeButton(Image target)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (!((Object)(object)buttons[i] == null))
			{
				if ((bool)pressedVersion)
				{
					buttons[i].sprite = (((Object)(object)buttons[i] == (Object)(object)target) ? pressedVersion : buttonSprites[i]);
				}
				else
				{
					buttons[i].fillCenter = (Object)(object)buttons[i] == (Object)(object)target;
				}
				if ((Object)(object)buttonTexts[i] != null)
				{
					((Graphic)buttonTexts[i]).color = (((Object)(object)buttons[i] == (Object)(object)target) ? Color.black : Color.white);
				}
			}
		}
	}
}
