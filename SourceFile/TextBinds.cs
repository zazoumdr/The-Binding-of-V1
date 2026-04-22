using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBinds : MonoBehaviour
{
	public string text1;

	public string input;

	public string text2;

	private Text text;

	private TMP_Text textmp;

	private void OnEnable()
	{
		string text = "";
		if (input == "")
		{
			text = text1;
		}
		else
		{
			string text2 = "";
			KeyCode keyCode = MonoSingleton<InputManager>.Instance.Inputs[input];
			switch (keyCode)
			{
			case KeyCode.Mouse0:
				text2 = "Left Mouse Button";
				break;
			case KeyCode.Mouse1:
				text2 = "Right Mouse Button";
				break;
			case KeyCode.Mouse2:
				text2 = "Middle Mouse Button";
				break;
			case KeyCode.Mouse3:
			case KeyCode.Mouse4:
			case KeyCode.Mouse5:
			case KeyCode.Mouse6:
			{
				text2 = keyCode.ToString();
				string s = text2.Substring(text2.Length - 1, 1);
				text2 = text2.Substring(0, text2.Length - 1);
				text2 += int.Parse(s) + 1;
				break;
			}
			default:
				text2 = keyCode.ToString();
				break;
			}
			text2 = MonoSingleton<InputManager>.Instance.GetBindingString(input) ?? text2;
			text = text1 + text2 + this.text2;
		}
		text = text.Replace('$', '\n');
		if (!(Object)(object)this.text && !(Object)(object)textmp)
		{
			this.text = GetComponent<Text>();
			if (!(Object)(object)this.text)
			{
				textmp = GetComponent<TMP_Text>();
			}
		}
		if ((bool)(Object)(object)this.text)
		{
			this.text.text = text;
		}
		else if ((bool)(Object)(object)textmp)
		{
			textmp.text = text;
		}
	}
}
