using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine.InputSystem;

public class JsonBindingMap
{
	public static readonly Logger Log = new Logger("JsonBindingMap");

	public string controlScheme;

	public static Dictionary<string, string> bindAliases = new Dictionary<string, string>
	{
		{ "Slot 1", "Revolver" },
		{ "Slot 2", "Shotgun" },
		{ "Slot 3", "Nailgun" },
		{ "Slot 4", "Railcannon" },
		{ "Slot 5", "Rocket Launcher" },
		{ "Change Variation", "Next Variation" },
		{ "Last Weapon", "Last Used Weapon" }
	};

	public Dictionary<string, List<JsonBinding>> modifiedActions = new Dictionary<string, List<JsonBinding>>();

	public static JsonBindingMap From(InputActionAsset asset, InputControlScheme scheme)
	{
		JsonBindingMap jsonBindingMap = new JsonBindingMap
		{
			controlScheme = ((InputControlScheme)(ref scheme)).bindingGroup
		};
		foreach (InputAction item in asset)
		{
			jsonBindingMap.AddAction(item);
		}
		return jsonBindingMap;
	}

	public static JsonBindingMap From(InputActionAsset asset, InputActionAsset baseAsset, InputControlScheme scheme)
	{
		JsonBindingMap jsonBindingMap = new JsonBindingMap
		{
			controlScheme = ((InputControlScheme)(ref scheme)).bindingGroup
		};
		foreach (InputAction item in asset)
		{
			InputAction baseAction = baseAsset.FindAction(item.id);
			if (!item.IsActionEqual(baseAction, ((InputControlScheme)(ref scheme)).bindingGroup))
			{
				jsonBindingMap.AddAction(item);
			}
		}
		return jsonBindingMap;
	}

	public void ApplyTo(InputActionAsset asset)
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, List<JsonBinding>> modifiedAction in modifiedActions)
		{
			string text = modifiedAction.Key;
			List<JsonBinding> value = modifiedAction.Value;
			if (bindAliases.TryGetValue(text, out var value2))
			{
				text = value2;
			}
			InputAction val = asset.FindAction(text, false);
			if (val == null)
			{
				Log.Warning("Action " + text + " was found in saved bindings, but does not exist (action == null). Ignoring...", (IEnumerable<Tag>)null, (string)null, (object)null);
				break;
			}
			val.WipeAction(controlScheme);
			foreach (JsonBinding item in value)
			{
				if (item.isComposite)
				{
					if (item.parts.Count == 0)
					{
						continue;
					}
					CompositeSyntax val2 = InputActionSetupExtensions.AddCompositeBinding(val, item.path, (string)null, (string)null);
					foreach (KeyValuePair<string, string> part in item.parts)
					{
						((CompositeSyntax)(ref val2)).With(part.Key, part.Value, controlScheme, (string)null);
					}
					BindingSyntax val3 = InputActionSetupExtensions.ChangeBinding(val, ((CompositeSyntax)(ref val2)).bindingIndex);
					((BindingSyntax)(ref val3)).WithGroup(controlScheme);
				}
				else
				{
					InputActionSetupExtensions.AddBinding(val, item.path, (string)null, (string)null, controlScheme);
				}
			}
		}
	}

	public void AddAction(InputAction action)
	{
		modifiedActions.Add(action.name, JsonBinding.FromAction(action, controlScheme));
	}
}
