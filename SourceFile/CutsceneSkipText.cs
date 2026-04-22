using TMPro;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CutsceneSkipText : MonoSingleton<CutsceneSkipText>
{
	private TMP_Text txt;

	private void Start()
	{
		txt = GetComponent<TMP_Text>();
		Hide();
	}

	public void Show()
	{
		((Behaviour)(object)txt).enabled = true;
	}

	public void Hide()
	{
		((Behaviour)(object)txt).enabled = false;
	}
}
