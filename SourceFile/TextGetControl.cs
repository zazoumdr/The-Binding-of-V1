using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextGetControl : MonoBehaviour
{
	private TMP_Text text;

	public InputActionReference actionReference;

	private void Start()
	{
		text = GetComponent<TMP_Text>();
		string bindingString = MonoSingleton<InputManager>.Instance.GetBindingString(actionReference.action.id);
		text.text = bindingString;
	}
}
