using TMPro;
using UnityEngine;

public class EndlessHighScore : MonoBehaviour
{
	private TMP_Text text;

	private void OnEnable()
	{
		if (!(Object)(object)text)
		{
			text = GetComponent<TMP_Text>();
		}
		if ((bool)(Object)(object)text)
		{
			int num = (int)Mathf.Floor(GameProgressSaver.GetBestCyber().preciseWavesByDifficulty[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 2)]);
			if (num <= 0)
			{
				text.text = "";
			}
			else
			{
				text.text = num.ToString() ?? "";
			}
		}
	}
}
