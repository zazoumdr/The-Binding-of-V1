using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuActSelect : MonoBehaviour
{
	public int requiredLevels;

	public bool forceOff;

	public bool hideWhenOff;

	public bool primeLevels;

	private Transform[] children;

	private Image img;

	private TMP_Text text;

	private string originalName;

	public string nameWhenDisabled;

	private void OnEnable()
	{
		int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if ((Object)(object)text == null)
		{
			text = base.transform.GetChild(0).GetComponent<TMP_Text>();
			originalName = text.text;
		}
		bool flag = false;
		if (primeLevels)
		{
			for (int i = 1; i < 4; i++)
			{
				if (GameProgressSaver.GetPrime(difficulty, i) > 0)
				{
					Debug.Log("Found Primes");
					flag = true;
					break;
				}
			}
		}
		if (forceOff || (GameProgressSaver.GetProgress(difficulty) <= requiredLevels && !flag))
		{
			((Selectable)GetComponent<Button>()).interactable = false;
			((Graphic)text).color = new Color(0.3f, 0.3f, 0.3f);
			if (nameWhenDisabled != "")
			{
				text.text = nameWhenDisabled;
			}
			base.transform.GetChild(1).gameObject.SetActive(value: false);
			if (hideWhenOff)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			((Selectable)GetComponent<Button>()).interactable = true;
			((Graphic)text).color = new Color(1f, 1f, 1f);
			text.text = originalName;
			base.transform.GetChild(1).gameObject.SetActive(value: true);
		}
	}
}
