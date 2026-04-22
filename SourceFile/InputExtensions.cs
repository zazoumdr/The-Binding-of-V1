using System.Collections.Generic;
using System.Linq;
using plog;
using UnityEngine.InputSystem;

public static class InputExtensions
{
	private const bool DebugLogging = false;

	private static readonly Logger Log = new Logger("Input");

	public static string GetBindingDisplayStringWithoutOverride(this InputAction action, InputBinding binding, DisplayStringOptions options = (DisplayStringOptions)0)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (((InputBinding)(ref binding)).isPartOfComposite)
		{
			return ((InputBinding)(ref binding)).ToDisplayString((DisplayStringOptions)12, (InputControl)null);
		}
		string overridePath = ((InputBinding)(ref binding)).overridePath;
		((InputBinding)(ref binding)).overridePath = null;
		InputActionRebindingExtensions.ApplyBindingOverride(action, binding);
		string result = InputActionRebindingExtensions.GetBindingDisplayString(action, binding, (DisplayStringOptions)12).ToUpper();
		((InputBinding)(ref binding)).overridePath = overridePath;
		InputActionRebindingExtensions.ApplyBindingOverride(action, binding);
		return result;
	}

	public static void WipeAction(this InputAction action, string controlScheme)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		new List<InputBinding>();
		BindingSyntax val = InputActionSetupExtensions.ChangeBindingWithGroup(action, controlScheme);
		while (((BindingSyntax)(ref val)).valid)
		{
			((BindingSyntax)(ref val)).Erase();
			val = InputActionSetupExtensions.ChangeBindingWithGroup(action, controlScheme);
		}
	}

	public static bool IsActionEqual(this InputAction action, InputAction baseAction, string controlScheme = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		List<InputBinding> list = ((IEnumerable<InputBinding>)(object)action.bindings).ToList();
		List<InputBinding> list2 = ((IEnumerable<InputBinding>)(object)baseAction.bindings).ToList();
		if (controlScheme != null)
		{
			list = list.Where((InputBinding bind) => action.BindingHasGroup(bind, controlScheme)).ToList();
			list2 = list2.Where((InputBinding bind) => baseAction.BindingHasGroup(bind, controlScheme)).ToList();
		}
		if (list.Count != list2.Count)
		{
			return false;
		}
		for (int num = 0; num < list.Count; num++)
		{
			InputBinding binding = list[num];
			InputBinding other = list2[num];
			if (!binding.IsBindingEqual(other))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsBindingEqual(this InputBinding binding, InputBinding other)
	{
		if (AreStringsEqual(((InputBinding)(ref other)).effectivePath, ((InputBinding)(ref binding)).effectivePath) && AreStringsEqual(((InputBinding)(ref other)).effectiveInteractions, ((InputBinding)(ref binding)).effectiveInteractions))
		{
			return AreStringsEqual(((InputBinding)(ref other)).effectiveProcessors, ((InputBinding)(ref binding)).effectiveProcessors);
		}
		return false;
	}

	public static bool BindingHasGroup(this InputAction action, InputBinding binding, string group)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return action.BindingHasGroup(InputActionRebindingExtensions.GetBindingIndex(action, binding), group);
	}

	public static bool BindingHasGroup(this InputAction action, int i, string group)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		InputBinding val = action.bindings[i];
		if (((InputBinding)(ref val)).isComposite && action.bindings.Count > i + 1)
		{
			val = action.bindings[i + 1];
		}
		if (((InputBinding)(ref val)).groups == null)
		{
			return false;
		}
		return ((InputBinding)(ref val)).groups.Contains(group);
	}

	public static int[] GetBindingsWithGroup(this InputAction action, string group)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding val = action.bindings[i];
			if (!((InputBinding)(ref val)).isPartOfComposite && action.BindingHasGroup(i, group))
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	private static bool AreStringsEqual(string str1, string str2)
	{
		if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
		{
			return true;
		}
		return string.Equals(str1, str2);
	}
}
