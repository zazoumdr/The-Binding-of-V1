using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine.InputSystem;

namespace GameConsole;

public class Binds
{
	public Dictionary<string, InputActionState> registeredBinds;

	public Dictionary<string, string> defaultBinds = new Dictionary<string, string>
	{
		{ "open", "/Keyboard/f8" },
		{ "submit", "/Keyboard/enter" },
		{ "command_history_up", "/Keyboard/upArrow" },
		{ "command_history_down", "/Keyboard/downArrow" },
		{ "scroll_up", "/Keyboard/pageUp" },
		{ "scroll_down", "/Keyboard/pageUp" },
		{ "scroll_to_bottom", "/Keyboard/home" },
		{ "scroll_to_top", "/Keyboard/end" },
		{ "autocomplete", "/Keyboard/tab" }
	};

	private Logger Log { get; } = new Logger("Binds");

	public bool OpenPressed => SafeWasPerformed("open");

	public bool SubmitPressed => SafeWasPerformed("submit");

	public bool AutocompletePressed => SafeWasPerformed("autocomplete");

	public bool CommandHistoryUpPressed => SafeWasPerformed("command_history_up");

	public bool CommandHistoryDownPressed => SafeWasPerformed("command_history_down");

	public bool ScrollUpPressed => SafeWasPerformed("scroll_up");

	public bool ScrollDownPressed => SafeWasPerformed("scroll_down");

	public bool ScrollToBottomPressed => SafeWasPerformed("scroll_to_bottom");

	public bool ScrollToTopPressed => SafeWasPerformed("scroll_to_top");

	public bool ScrollUpHeld => SafeIsHeld("scroll_up");

	public bool ScrollDownHeld => SafeIsHeld("scroll_down");

	private bool SafeWasPerformed(string key)
	{
		if (registeredBinds != null && registeredBinds.TryGetValue(key, out var value))
		{
			return value.WasPerformedThisFrame;
		}
		return false;
	}

	private bool SafeIsHeld(string key)
	{
		if (registeredBinds != null && registeredBinds.TryGetValue(key, out var value))
		{
			return value.IsPressed;
		}
		return false;
	}

	public void Initialize()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Log.Parent = Console.Log;
		registeredBinds = new Dictionary<string, InputActionState>();
		foreach (KeyValuePair<string, string> defaultBind in defaultBinds)
		{
			InputActionState inputActionState = new InputActionState(new InputAction(defaultBind.Key, (InputActionType)0, (string)null, (string)null, (string)null, (string)null));
			registeredBinds.Add(defaultBind.Key, inputActionState);
			BindingSyntax val = InputActionSetupExtensions.AddBinding(inputActionState.Action, MonoSingleton<PrefsManager>.Instance.GetString("consoleBinding." + defaultBind.Key, defaultBind.Value), (string)null, (string)null, (string)null);
			((BindingSyntax)(ref val)).WithGroup("Keyboard");
			inputActionState.Action.Enable();
		}
	}

	public void Rebind(string key, string bind)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!defaultBinds.ContainsKey(key))
		{
			Log.Info("Invalid console bind key: " + key, (IEnumerable<Tag>)null, (string)null, (object)null);
			return;
		}
		string key2 = "consoleBinding." + key;
		MonoSingleton<PrefsManager>.Instance.SetString(key2, bind);
		if (registeredBinds.TryGetValue(key, out var value))
		{
			value.Action.Disable();
			value.Action.Dispose();
		}
		value = new InputActionState(new InputAction(key, (InputActionType)0, (string)null, (string)null, (string)null, (string)null));
		registeredBinds[key] = value;
		BindingSyntax val = InputActionSetupExtensions.AddBinding(value.Action, bind, (string)null, (string)null, (string)null);
		((BindingSyntax)(ref val)).WithGroup("Keyboard");
		value.Action.Enable();
		MonoSingleton<Console>.Instance.UpdateDisplayString();
	}
}
