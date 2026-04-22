using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CheatsController : MonoSingleton<CheatsController>
{
	private static readonly Logger Log = new Logger("CheatsController");

	public GameObject spawnerArm;

	public GameObject fullBrightLight;

	private static readonly KeyCode[] Sequence = new KeyCode[10]
	{
		KeyCode.UpArrow,
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.DownArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
		KeyCode.B,
		KeyCode.A
	};

	[Space]
	[SerializeField]
	private GameObject consentScreen;

	[SerializeField]
	private GameObject cheatsEnabledPanel;

	[SerializeField]
	private GameObject cheatsInfoPanel;

	[SerializeField]
	public GameObject cheatsTeleportPanel;

	public TMP_Text cheatsInfo;

	[Space]
	[SerializeField]
	private AudioSource cheatEnabledSound;

	[SerializeField]
	private AudioSource cheatDisabledSound;

	private int sequenceIndex;

	public bool cheatsEnabled;

	private bool noclip;

	private bool flight;

	private bool infiniteJumps;

	private bool stayEnabled;

	private static bool TryGetKeyboardButton(int sequenceIndex, out ButtonControl button)
	{
		button = null;
		if (Keyboard.current == null)
		{
			return false;
		}
		switch (Sequence[sequenceIndex])
		{
		case KeyCode.UpArrow:
			button = (ButtonControl)(object)Keyboard.current.upArrowKey;
			break;
		case KeyCode.DownArrow:
			button = (ButtonControl)(object)Keyboard.current.downArrowKey;
			break;
		case KeyCode.LeftArrow:
			button = (ButtonControl)(object)Keyboard.current.leftArrowKey;
			break;
		case KeyCode.RightArrow:
			button = (ButtonControl)(object)Keyboard.current.rightArrowKey;
			break;
		case KeyCode.A:
			button = (ButtonControl)(object)Keyboard.current.aKey;
			break;
		case KeyCode.B:
			button = (ButtonControl)(object)Keyboard.current.bKey;
			break;
		}
		return button != null;
	}

	private static bool TryGetGamepadButton(int sequenceIndex, out ButtonControl button)
	{
		button = null;
		if (Gamepad.current == null)
		{
			return false;
		}
		switch (Sequence[sequenceIndex])
		{
		case KeyCode.UpArrow:
			button = Gamepad.current.dpad.up;
			break;
		case KeyCode.DownArrow:
			button = Gamepad.current.dpad.down;
			break;
		case KeyCode.LeftArrow:
			button = Gamepad.current.dpad.left;
			break;
		case KeyCode.RightArrow:
			button = Gamepad.current.dpad.right;
			break;
		case KeyCode.A:
			button = Gamepad.current.buttonSouth;
			break;
		case KeyCode.B:
			button = Gamepad.current.buttonEast;
			break;
		}
		return button != null;
	}

	public void ShowTeleportPanel()
	{
		cheatsTeleportPanel.SetActive(value: true);
		GameStateManager.Instance.RegisterState(new GameState("teleport-menu", cheatsTeleportPanel)
		{
			cursorLock = LockMode.Unlock
		});
		MonoSingleton<OptionsManager>.Instance.Freeze();
	}

	private void Start()
	{
		if (CheatsManager.KeepCheatsEnabled)
		{
			MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
			consentScreen.SetActive(value: false);
			cheatsEnabled = true;
		}
	}

	public void PlayToggleSound(bool newState)
	{
		if (newState)
		{
			cheatEnabledSound.Play(tracked: true);
		}
		else
		{
			cheatDisabledSound.Play(tracked: true);
		}
	}

	private void ProcessInput()
	{
		TryGetGamepadButton(sequenceIndex, out var button);
		TryGetKeyboardButton(sequenceIndex, out var button2);
		if ((button2 != null && button2.wasPressedThisFrame) || (button != null && button.wasPressedThisFrame))
		{
			sequenceIndex++;
			if (sequenceIndex == Sequence.Length)
			{
				MonoSingleton<OptionsManager>.Instance.Pause();
				consentScreen.SetActive(value: true);
				sequenceIndex = 0;
			}
		}
		else
		{
			Keyboard current = Keyboard.current;
			if ((current != null && ((ButtonControl)current.anyKey).wasPressedThisFrame) || AnyGamepadButtonPressed())
			{
				sequenceIndex = 0;
			}
		}
	}

	private bool AnyGamepadButtonPressed()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (Gamepad.current == null)
		{
			return false;
		}
		Enumerator<InputControl> enumerator = ((InputDevice)Gamepad.current).allControls.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				InputControl current = enumerator.Current;
				ButtonControl val = (ButtonControl)(object)((current is ButtonControl) ? current : null);
				if (val != null && val.wasPressedThisFrame)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		return false;
	}

	private bool GamepadCombo()
	{
		if (Gamepad.current == null)
		{
			return false;
		}
		if (Gamepad.current.selectButton.isPressed)
		{
			return Gamepad.current.rightTrigger.wasPressedThisFrame;
		}
		return false;
	}

	public void Update()
	{
		if (!cheatsEnabled)
		{
			ProcessInput();
		}
		bool active = cheatsEnabled && !HideCheatsStatus.HideStatus;
		if (cheatsEnabled && (bool)MonoSingleton<CheatsManager>.Instance && MonoSingleton<CheatsManager>.Instance.IsMenuOpen())
		{
			active = true;
		}
		cheatsEnabledPanel.SetActive(active);
		cheatsInfoPanel.SetActive(active);
		if (MonoSingleton<CheatBinds>.Instance.isRebinding || !cheatsEnabled || (!Input.GetKeyDown(KeyCode.Home) && !Input.GetKeyDown(KeyCode.Tilde) && !Input.GetKeyDown(KeyCode.BackQuote) && !GamepadCombo()))
		{
			return;
		}
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			Log.Info("Un-Paused", (IEnumerable<Tag>)null, (string)null, (object)null);
			if (SandboxHud.SavesMenuOpen)
			{
				MonoSingleton<SandboxHud>.Instance.HideSavesMenu();
			}
			MonoSingleton<CheatsManager>.Instance.HideMenu();
		}
		else
		{
			Log.Info("Paused", (IEnumerable<Tag>)null, (string)null, (object)null);
			MonoSingleton<OptionsManager>.Instance.Pause();
			MonoSingleton<CheatsManager>.Instance.ShowMenu();
		}
	}

	public void ActivateCheats()
	{
		MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
		consentScreen.SetActive(value: false);
		cheatsEnabled = true;
	}

	public void Cancel()
	{
		consentScreen.SetActive(value: false);
	}
}
