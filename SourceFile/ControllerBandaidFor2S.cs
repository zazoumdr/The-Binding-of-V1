using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControllerBandaidFor2S : MonoBehaviour
{
	public bool a;

	public bool b;

	public bool x;

	public bool y;

	private void Update()
	{
		if (Gamepad.current != null && !(MonoSingleton<OptionsManager>.Instance == null) && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			if (a && Gamepad.current.aButton.wasPressedThisFrame)
			{
				Activate();
			}
			else if (b && Gamepad.current.bButton.wasPressedThisFrame)
			{
				Activate();
			}
			else if (y && Gamepad.current.yButton.wasPressedThisFrame)
			{
				Activate();
			}
			else if (x && Gamepad.current.xButton.wasPressedThisFrame)
			{
				Activate();
			}
		}
	}

	private void Activate()
	{
		Button componentInParent = GetComponentInParent<Button>();
		if ((bool)(Object)(object)componentInParent && ((Selectable)componentInParent).interactable && ((Component)(object)componentInParent).gameObject.activeInHierarchy)
		{
			((UnityEvent)(object)componentInParent.onClick).Invoke();
		}
	}
}
