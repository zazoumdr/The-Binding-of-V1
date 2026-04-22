using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuEsc : MonoBehaviour
{
	private static MenuEsc current;

	public GameObject previousPage;

	public bool HardClose;

	public bool DontClose;

	public UltrakillEvent OnClose;

	private void OnEnable()
	{
		current = this;
	}

	private void OnDisable()
	{
		if (current == this)
		{
			current = null;
		}
	}

	private void Update()
	{
		if (current != null && current != this)
		{
			return;
		}
		current = this;
		if (OptionsManager.selectedSomethingThisFrame || (!MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame && (Gamepad.current == null || !Gamepad.current.buttonEast.wasPressedThisFrame) && (Gamepad.current == null || !Gamepad.current.buttonSouth.wasPressedThisFrame || !(EventSystem.current.currentSelectedGameObject != null) || !EventSystem.current.currentSelectedGameObject.TryGetComponent<Slider>(out var _))))
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			BackSelectEvent componentInParent = EventSystem.current.currentSelectedGameObject.GetComponentInParent<BackSelectEvent>();
			if (componentInParent != null)
			{
				componentInParent.InvokeOnBack();
			}
			if (EventSystem.current.currentSelectedGameObject.TryGetComponent<BackSelectOverride>(out var component2))
			{
				component2.Selectable.Select();
				return;
			}
		}
		if (DontClose)
		{
			return;
		}
		if (HardClose)
		{
			if (SandboxHud.SavesMenuOpen)
			{
				MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
			}
			MonoSingleton<OptionsManager>.Instance.CloseOptions();
			MonoSingleton<OptionsManager>.Instance.UnPause();
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
		}
		base.gameObject.SetActive(value: false);
		if (previousPage != null)
		{
			previousPage.SetActive(value: true);
			if (previousPage.TryGetComponent<Selectable>(out var component3))
			{
				component3.Select();
			}
		}
		OnClose?.Invoke();
	}
}
