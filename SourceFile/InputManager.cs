using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NewBlood;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class InputManager : MonoSingleton<InputManager>
{
	private sealed class ButtonPressListener : IObserver<InputControl>
	{
		public static ButtonPressListener Instance { get; } = new ButtonPressListener();

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(InputControl value)
		{
			if (!(value.device is LegacyInput))
			{
				MonoSingleton<InputManager>.Instance.LastButtonDevice = value.device;
				if (MonoSingleton<InputManager>.Instance.IsRebinding && value.device is Gamepad)
				{
					MonoSingleton<InputManager>.Instance.CancelRebinding();
				}
			}
		}
	}

	private class BindingInfo
	{
		public InputAction Action;

		public string Name;

		public int Offset;

		public KeyCode DefaultKey;

		public string PrefName => "keyBinding." + Name;
	}

	public Dictionary<string, KeyCode> inputsDictionary = new Dictionary<string, KeyCode>();

	public InputActionAsset defaultActions;

	private IDisposable anyButtonListener;

	public bool ScrOn;

	public bool ScrWep;

	public bool ScrVar;

	public bool ScrRev;

	public Action<InputAction> actionModified;

	private BindingInfo[] legacyBindings;

	private RebindingOperation rebinding;

	private Action currentCancelCallback;

	public PlayerInput InputSource { get; private set; }

	public InputDevice LastButtonDevice { get; private set; }

	public bool IsRebinding { get; private set; }

	private static IObservable<InputControl> onAnyInput => Observable.Where<InputControl>(Observable.Select<InputEventPtr, InputControl>((IObservable<InputEventPtr>)(object)InputSystem.onEvent, (Func<InputEventPtr, InputControl>)((InputEventPtr e) => (((InputEventPtr)(ref e)).type != FourCC.op_Implicit(1398030676) && ((InputEventPtr)(ref e)).type != FourCC.op_Implicit(1145852993)) ? null : InputControlExtensions.GetFirstButtonPressOrNull(e, -1f, false))), (Func<InputControl, bool>)((InputControl c) => c != null && !c.noisy));

	public Dictionary<string, KeyCode> Inputs => inputsDictionary;

	private FileInfo savedBindingsFile => new FileInfo(Path.Combine(PrefsManager.PrefsPath, "Binds.json"));

	private void Awake()
	{
		InputSource = new PlayerInput();
		defaultActions = InputActionAsset.FromJson(InputSource.Actions.asset.ToJson());
		if (savedBindingsFile.Exists)
		{
			JsonConvert.DeserializeObject<JsonBindingMap>(File.ReadAllText(savedBindingsFile.FullName)).ApplyTo(InputSource.Actions.asset);
		}
		legacyBindings = new BindingInfo[20]
		{
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				DefaultKey = KeyCode.W,
				Name = "W"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 1,
				DefaultKey = KeyCode.S,
				Name = "S"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 2,
				DefaultKey = KeyCode.A,
				Name = "A"
			},
			new BindingInfo
			{
				Action = InputSource.Move.Action,
				Offset = 3,
				DefaultKey = KeyCode.D,
				Name = "D"
			},
			new BindingInfo
			{
				Action = InputSource.Jump.Action,
				DefaultKey = KeyCode.Space,
				Name = "Jump"
			},
			new BindingInfo
			{
				Action = InputSource.Dodge.Action,
				DefaultKey = KeyCode.LeftShift,
				Name = "Dodge"
			},
			new BindingInfo
			{
				Action = InputSource.Slide.Action,
				DefaultKey = KeyCode.LeftControl,
				Name = "Slide"
			},
			new BindingInfo
			{
				Action = InputSource.Fire1.Action,
				DefaultKey = KeyCode.Mouse0,
				Name = "Fire1"
			},
			new BindingInfo
			{
				Action = InputSource.Fire2.Action,
				DefaultKey = KeyCode.Mouse1,
				Name = "Fire2"
			},
			new BindingInfo
			{
				Action = InputSource.Punch.Action,
				DefaultKey = KeyCode.F,
				Name = "Punch"
			},
			new BindingInfo
			{
				Action = InputSource.Hook.Action,
				DefaultKey = KeyCode.R,
				Name = "Hook"
			},
			new BindingInfo
			{
				Action = InputSource.LastWeapon.Action,
				DefaultKey = KeyCode.Q,
				Name = "LastUsedWeapon"
			},
			new BindingInfo
			{
				Action = InputSource.NextVariation.Action,
				DefaultKey = KeyCode.E,
				Name = "ChangeVariation"
			},
			new BindingInfo
			{
				Action = InputSource.ChangeFist.Action,
				DefaultKey = KeyCode.G,
				Name = "ChangeFist"
			},
			new BindingInfo
			{
				Action = InputSource.Slot1.Action,
				DefaultKey = KeyCode.Alpha1,
				Name = "Slot1"
			},
			new BindingInfo
			{
				Action = InputSource.Slot2.Action,
				DefaultKey = KeyCode.Alpha2,
				Name = "Slot2"
			},
			new BindingInfo
			{
				Action = InputSource.Slot3.Action,
				DefaultKey = KeyCode.Alpha3,
				Name = "Slot3"
			},
			new BindingInfo
			{
				Action = InputSource.Slot4.Action,
				DefaultKey = KeyCode.Alpha4,
				Name = "Slot4"
			},
			new BindingInfo
			{
				Action = InputSource.Slot5.Action,
				DefaultKey = KeyCode.Alpha5,
				Name = "Slot5"
			},
			new BindingInfo
			{
				Action = InputSource.Slot6.Action,
				DefaultKey = KeyCode.Alpha6,
				Name = "Slot6"
			}
		};
		UpgradeBindings();
		InputSource.Enable();
		ScrOn = MonoSingleton<PrefsManager>.Instance.GetBool("scrollEnabled");
		ScrWep = MonoSingleton<PrefsManager>.Instance.GetBool("scrollWeapons");
		ScrVar = MonoSingleton<PrefsManager>.Instance.GetBool("scrollVariations");
		ScrRev = MonoSingleton<PrefsManager>.Instance.GetBool("scrollReversed");
	}

	private void OnEnable()
	{
		anyButtonListener = onAnyInput.Subscribe(ButtonPressListener.Instance);
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		anyButtonListener?.Dispose();
		SaveBindings(InputSource.Actions.asset);
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "scrollEnabled":
			ScrOn = (bool)value;
			break;
		case "scrollWeapons":
			ScrWep = (bool)value;
			break;
		case "scrollVariations":
			ScrVar = (bool)value;
			break;
		case "scrollReversed":
			ScrRev = (bool)value;
			break;
		}
	}

	public void ResetToDefault()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		JsonBindingMap.From(defaultActions, InputSource.Actions.KeyboardMouseScheme).ApplyTo(InputSource.Actions.asset);
		InputSource.ValidateBindings(InputSource.Actions.KeyboardMouseScheme);
	}

	public void ResetToDefault(InputAction action, InputControlScheme controlScheme)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		InputAction val = defaultActions.FindAction(action.name, false);
		InputSource.Disable();
		action.WipeAction(((InputControlScheme)(ref controlScheme)).bindingGroup);
		for (int i = 0; i < val.bindings.Count; i++)
		{
			if (!val.BindingHasGroup(i, ((InputControlScheme)(ref controlScheme)).bindingGroup))
			{
				continue;
			}
			InputBinding val2 = val.bindings[i];
			if (((InputBinding)(ref val2)).isPartOfComposite)
			{
				continue;
			}
			if (((InputBinding)(ref val2)).isComposite)
			{
				CompositeSyntax val3 = InputActionSetupExtensions.AddCompositeBinding(action, "2DVector", (string)null, (string)null);
				for (int j = i + 1; j < val.bindings.Count; j++)
				{
					InputBinding val4 = val.bindings[j];
					if (((InputBinding)(ref val4)).isPartOfComposite)
					{
						InputBinding val5 = val.bindings[j];
						((CompositeSyntax)(ref val3)).With(((InputBinding)(ref val5)).name, ((InputBinding)(ref val5)).path, ((InputControlScheme)(ref controlScheme)).bindingGroup, (string)null);
						continue;
					}
					break;
				}
			}
			else
			{
				BindingSyntax val6 = InputActionSetupExtensions.AddBinding(action, val2);
				((BindingSyntax)(ref val6)).WithGroup(((InputControlScheme)(ref controlScheme)).bindingGroup);
			}
		}
		actionModified?.Invoke(action);
		SaveBindings(InputSource.Actions.asset);
		InputSource.Enable();
	}

	public bool PerformingCheatMenuCombo()
	{
		if (!MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			return false;
		}
		if (!(LastButtonDevice is Gamepad))
		{
			return false;
		}
		if (Gamepad.current == null)
		{
			return false;
		}
		return Gamepad.current.selectButton.isPressed;
	}

	public void SaveBindings(InputActionAsset asset)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		JsonBindingMap jsonBindingMap = JsonBindingMap.From(asset, defaultActions, InputSource.Actions.KeyboardMouseScheme);
		File.WriteAllText(savedBindingsFile.FullName, JsonConvert.SerializeObject((object)jsonBindingMap, (Formatting)1));
	}

	public void UpgradeBindings()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		BindingInfo[] array = legacyBindings;
		BindingSyntax val2;
		foreach (BindingInfo bindingInfo in array)
		{
			InputBinding val = InputBinding.MaskByGroup("Keyboard & Mouse");
			int bindingIndex = InputActionRebindingExtensions.GetBindingIndex(bindingInfo.Action, val);
			Inputs[bindingInfo.Name] = (KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt(bindingInfo.PrefName, (int)bindingInfo.DefaultKey);
			if (bindingIndex != -1 && MonoSingleton<PrefsManager>.Instance.HasKey(bindingInfo.PrefName))
			{
				KeyCode keyCode = (KeyCode)MonoSingleton<PrefsManager>.Instance.GetInt(bindingInfo.PrefName);
				MonoSingleton<PrefsManager>.Instance.DeleteKey(bindingInfo.PrefName);
				ButtonControl button = LegacyInput.current.GetButton(keyCode);
				val2 = InputActionSetupExtensions.ChangeBinding(bindingInfo.Action, bindingIndex + bindingInfo.Offset);
				val2 = ((BindingSyntax)(ref val2)).WithPath(((InputControl)button).path);
				InputControlScheme keyboardMouseScheme = InputSource.Actions.KeyboardMouseScheme;
				((BindingSyntax)(ref val2)).WithGroup(((InputControlScheme)(ref keyboardMouseScheme)).bindingGroup);
			}
		}
		foreach (InputAction action in InputSource.Actions)
		{
			Enumerator<InputBinding> enumerator2 = action.bindings.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					InputBinding current2 = enumerator2.Current;
					if (((InputBinding)(ref current2)).path.Contains("LegacyInput"))
					{
						string text = ((InputBinding)(ref current2)).path.Replace("/LegacyInput/", "<Keyboard>/").Replace("alpha", "");
						if (InputSystem.FindControl(text) != null)
						{
							val2 = InputActionSetupExtensions.ChangeBinding(action, current2);
							((BindingSyntax)(ref val2)).WithPath(text);
						}
						else
						{
							val2 = InputActionSetupExtensions.ChangeBinding(action, current2);
							((BindingSyntax)(ref val2)).Erase();
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	public void CancelRebinding()
	{
		if (IsRebinding)
		{
			RebindingOperation obj = rebinding;
			if (obj != null)
			{
				obj.Cancel();
			}
			RebindingOperation obj2 = rebinding;
			if (obj2 != null)
			{
				obj2.Dispose();
			}
			rebinding = null;
			currentCancelCallback?.Invoke();
			currentCancelCallback = null;
			IsRebinding = false;
			InputSource.Enable();
		}
	}

	public void WaitForButton(Action<string> onComplete, Action onCancel, List<string> allowedPaths = null)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		InputSource.Disable();
		RebindingOperation obj = rebinding;
		if (obj != null)
		{
			obj.Cancel();
		}
		RebindingOperation obj2 = rebinding;
		if (obj2 != null)
		{
			obj2.Dispose();
		}
		currentCancelCallback = onCancel;
		IsRebinding = true;
		rebinding = new RebindingOperation().OnApplyBinding((Action<RebindingOperation, string>)delegate(RebindingOperation op, string path)
		{
			rebinding = null;
			op.Dispose();
			IsRebinding = false;
			currentCancelCallback = null;
			if ((object)InputControlPath.TryFindControl((InputControl)(object)Keyboard.current, path, 0) == Keyboard.current.escapeKey)
			{
				onCancel?.Invoke();
			}
			else
			{
				onComplete?.Invoke(path);
			}
			InputSource.Enable();
		}).WithControlsExcluding(((InputControl)LegacyInput.current).path).WithExpectedControlType<ButtonControl>()
			.WithMatchingEventsBeingSuppressed(true);
		if (allowedPaths != null)
		{
			foreach (string allowedPath in allowedPaths)
			{
				rebinding.WithControlsHavingToMatchPath(allowedPath);
			}
		}
		rebinding.Start();
	}

	public void WaitForButtonSequence(Queue<string> partNames, Action<string> onBeginPart, Action<string, string> onCompletePart, Action onComplete, Action onCancel, List<string> allowedPaths = null)
	{
		if (partNames.Count == 0)
		{
			onComplete?.Invoke();
			return;
		}
		string part = partNames.Dequeue();
		onBeginPart?.Invoke(part);
		WaitForButton(delegate(string path)
		{
			onCompletePart?.Invoke(part, path);
			WaitForButtonSequence(partNames, onBeginPart, onCompletePart, onComplete, onCancel);
		}, onCancel, allowedPaths);
	}

	public void ClearOtherActions(InputAction action, string path)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		foreach (InputAction action2 in InputSource.Actions)
		{
			if (action2 == action)
			{
				continue;
			}
			int bindingIndex = InputActionRebindingExtensions.GetBindingIndex(action2, (string)null, path);
			if (bindingIndex != -1)
			{
				BindingSyntax val = InputActionSetupExtensions.ChangeBinding(action2, bindingIndex);
				InputBinding binding = ((BindingSyntax)(ref val)).binding;
				if (((InputBinding)(ref binding)).isPartOfComposite)
				{
					val = ((BindingSyntax)(ref val)).PreviousCompositeBinding((string)null);
				}
				((BindingSyntax)(ref val)).Erase();
			}
		}
	}

	public void Rebind(InputAction action, int? existingIndex, Action onComplete, Action onCancel, InputControlScheme scheme)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		List<string> allowedPaths = ((IEnumerable<DeviceRequirement>)(object)((InputControlScheme)(ref scheme)).deviceRequirements).Select((DeviceRequirement requirement) => ((DeviceRequirement)(ref requirement)).controlPath).ToList();
		WaitForButton(delegate(string path)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<InputBinding> enumerator = action.bindings.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					InputBinding current = enumerator.Current;
					if (InputSystem.FindControl(((InputBinding)(ref current)).path) == InputSystem.FindControl(path))
					{
						onComplete?.Invoke();
						return;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
			BindingSyntax val;
			if (existingIndex.HasValue)
			{
				int valueOrDefault = existingIndex.GetValueOrDefault();
				val = InputActionSetupExtensions.ChangeBinding(action, valueOrDefault);
			}
			else
			{
				val = InputActionSetupExtensions.AddBinding(action, default(InputBinding));
			}
			BindingSyntax val2 = val;
			BindingSyntax val3 = ((BindingSyntax)(ref val2)).WithPath(path);
			((BindingSyntax)(ref val3)).WithGroup(((InputControlScheme)(ref scheme)).bindingGroup);
			actionModified?.Invoke(action);
			onComplete?.Invoke();
		}, delegate
		{
			onCancel?.Invoke();
		}, allowedPaths);
	}

	public void RebindComposite(InputAction action, int? existingIndex, Action<string> onBeginPart, Action onComplete, Action onCancel, InputControlScheme scheme)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		List<string> allowedPaths = ((IEnumerable<DeviceRequirement>)(object)((InputControlScheme)(ref scheme)).deviceRequirements).Select((DeviceRequirement requirement) => ((DeviceRequirement)(ref requirement)).controlPath).ToList();
		if (action.expectedControlType == "Vector2")
		{
			string[] collection = new string[4] { "Up", "Down", "Left", "Right" };
			Dictionary<string, string> partPathDict = new Dictionary<string, string>();
			WaitForButtonSequence(new Queue<string>(collection), onBeginPart, delegate(string part, string path)
			{
				partPathDict.Add(part, path);
			}, delegate
			{
				//IL_0098: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0101: Unknown result type (might be due to invalid IL or missing references)
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				BindingSyntax val2;
				if (existingIndex.HasValue)
				{
					int valueOrDefault = existingIndex.GetValueOrDefault();
					BindingSyntax val = InputActionSetupExtensions.ChangeBinding(action, valueOrDefault);
					foreach (KeyValuePair<string, string> item in partPathDict)
					{
						val2 = ((BindingSyntax)(ref val)).NextPartBinding(item.Key);
						val2 = ((BindingSyntax)(ref val2)).WithPath(item.Value);
						((BindingSyntax)(ref val2)).WithGroup(((InputControlScheme)(ref scheme)).bindingGroup);
					}
				}
				else
				{
					CompositeSyntax val3 = InputActionSetupExtensions.AddCompositeBinding(action, "2DVector", (string)null, (string)null);
					foreach (KeyValuePair<string, string> item2 in partPathDict)
					{
						((CompositeSyntax)(ref val3)).With(item2.Key, item2.Value, ((InputControlScheme)(ref scheme)).bindingGroup, (string)null);
					}
					val2 = InputActionSetupExtensions.AddBinding(action, default(InputBinding));
					((BindingSyntax)(ref val2)).Erase();
				}
				actionModified?.Invoke(action);
				onComplete?.Invoke();
			}, onCancel, allowedPaths);
		}
		else
		{
			Debug.LogError("Attempted to call RebindComposite on action with unsupported control type: '" + action.expectedControlType + "'");
		}
	}

	public string GetBindingString(Guid actionId)
	{
		return GetBindingString(actionId.ToString());
	}

	public string GetBindingString(string nameOrId)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyArray<InputBinding> bindings = InputSource.Actions.FindAction(nameOrId).bindings;
		string text = string.Empty;
		int num = 0;
		Queue<string> queue = new Queue<string>();
		InputControlScheme val = InputSource.Actions.KeyboardMouseScheme;
		Enumerator<InputControlScheme> enumerator = InputSource.Actions.controlSchemes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				InputControlScheme current = enumerator.Current;
				if (((InputControlScheme)(ref current)).SupportsDevice(LastButtonDevice))
				{
					val = current;
					break;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		for (int i = 0; i < bindings.Count; i++)
		{
			InputBinding val2 = bindings[i];
			if (((InputBinding)(ref val2)).isComposite)
			{
				num = i;
				continue;
			}
			val2 = bindings[i];
			InputControl val3 = InputSystem.FindControl(((InputBinding)(ref val2)).path);
			if (val3 == null)
			{
				continue;
			}
			val2 = bindings[i];
			if (((InputBinding)(ref val2)).isPartOfComposite)
			{
				for (int j = num + 1; j < bindings.Count; j++)
				{
					val2 = bindings[j];
					if (!((InputBinding)(ref val2)).isPartOfComposite)
					{
						break;
					}
					if (j > num + 1)
					{
						text += " + ";
					}
					string text2 = text;
					val2 = bindings[j];
					text = text2 + (((InputBinding)(ref val2)).ToDisplayString((DisplayStringOptions)0, (InputControl)null) ?? "?");
				}
				return text;
			}
			if (((InputControlScheme)(ref val)).SupportsDevice(val3.device))
			{
				val2 = bindings[i];
				return ((InputBinding)(ref val2)).ToDisplayString((DisplayStringOptions)0, (InputControl)null);
			}
		}
		if (queue.Count == 0)
		{
			return "";
		}
		Debug.Log(queue.Count);
		string text3 = queue.Dequeue() ?? "";
		while (queue.Count > 0)
		{
			text3 = text3 + "/" + queue.Dequeue();
		}
		return text3;
	}
}
