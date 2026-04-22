using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class PlayerInput
{
	public InputActions Actions;

	public InputActionState Move;

	public InputActionState Look;

	public InputActionState WheelLook;

	public InputActionState Punch;

	public InputActionState Hook;

	public InputActionState Fire1;

	public InputActionState Fire2;

	public InputActionState Jump;

	public InputActionState Slide;

	public InputActionState Dodge;

	public InputActionState ChangeFist;

	public InputActionState NextVariation;

	public InputActionState PreviousVariation;

	public InputActionState NextWeapon;

	public InputActionState PrevWeapon;

	public InputActionState LastWeapon;

	public InputActionState SelectVariant1;

	public InputActionState SelectVariant2;

	public InputActionState SelectVariant3;

	public InputActionState Pause;

	public InputActionState Stats;

	public InputActionState Slot1;

	public InputActionState Slot2;

	public InputActionState Slot3;

	public InputActionState Slot4;

	public InputActionState Slot5;

	public InputActionState Slot6;

	private Dictionary<InputControl, InputBinding[]> conflicts = new Dictionary<InputControl, InputBinding[]>();

	public PlayerInput()
	{
		Actions = new InputActions();
		RebuildActions();
	}

	public void ValidateBindings(InputControlScheme scheme)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		conflicts.Clear();
		IEnumerable<InputAction> source = from action in ((IEnumerable<InputActionMap>)(object)new InputActionMap[4]
			{
				(InputActionMap)Actions.Movement,
				(InputActionMap)Actions.Weapon,
				(InputActionMap)Actions.Fist,
				(InputActionMap)Actions.HUD
			}).SelectMany((InputActionMap map) => (IEnumerable<InputAction>)(object)map.actions)
			where action.name != "Look" && action.name != "WheelLook"
			select action;
		InputActionRebindingExtensions.RemoveAllBindingOverrides((IInputActionCollection2)(object)Actions);
		foreach (IGrouping<InputControl, InputBinding> item in from binding in source.SelectMany((InputAction action) => (IEnumerable<InputBinding>)(object)action.bindings)
			where ((InputBinding)(ref binding)).groups != null
			where ((InputBinding)(ref binding)).groups.Contains(((InputControlScheme)(ref scheme)).bindingGroup)
			where !((InputBinding)(ref binding)).isComposite
			group binding by InputSystem.FindControl(((InputBinding)(ref binding)).path))
		{
			if (item.Key == null)
			{
				continue;
			}
			InputBinding[] array = item.ToArray();
			if (array.Length > 1)
			{
				conflicts.Add(item.Key, array);
				for (int num = 0; num < array.Length; num++)
				{
					InputBinding val = array[num];
					InputAction obj = Actions.FindAction(((InputBinding)(ref val)).action);
					((InputBinding)(ref val)).overridePath = "";
					InputActionRebindingExtensions.ApplyBindingOverride(obj, val);
				}
			}
		}
	}

	private void RebuildActions()
	{
		Move = new InputActionState(Actions.Movement.Move);
		Look = new InputActionState(Actions.Movement.Look);
		WheelLook = new InputActionState(Actions.Weapon.WheelLook);
		Punch = new InputActionState(Actions.Fist.Punch);
		Hook = new InputActionState(Actions.Fist.Hook);
		Fire1 = new InputActionState(Actions.Weapon.PrimaryFire);
		Fire2 = new InputActionState(Actions.Weapon.SecondaryFire);
		Jump = new InputActionState(Actions.Movement.Jump);
		Slide = new InputActionState(Actions.Movement.Slide);
		Dodge = new InputActionState(Actions.Movement.Dodge);
		ChangeFist = new InputActionState(Actions.Fist.ChangeFist);
		NextVariation = new InputActionState(Actions.Weapon.NextVariation);
		PreviousVariation = new InputActionState(Actions.Weapon.PreviousVariation);
		NextWeapon = new InputActionState(Actions.Weapon.NextWeapon);
		PrevWeapon = new InputActionState(Actions.Weapon.PreviousWeapon);
		LastWeapon = new InputActionState(Actions.Weapon.LastUsedWeapon);
		SelectVariant1 = new InputActionState(Actions.Weapon.VariationSlot1);
		SelectVariant2 = new InputActionState(Actions.Weapon.VariationSlot2);
		SelectVariant3 = new InputActionState(Actions.Weapon.VariationSlot3);
		Pause = new InputActionState(Actions.UI.Pause);
		Stats = new InputActionState(Actions.HUD.Stats);
		Slot1 = new InputActionState(Actions.Weapon.Revolver);
		Slot2 = new InputActionState(Actions.Weapon.Shotgun);
		Slot3 = new InputActionState(Actions.Weapon.Nailgun);
		Slot4 = new InputActionState(Actions.Weapon.Railcannon);
		Slot5 = new InputActionState(Actions.Weapon.RocketLauncher);
		Slot6 = new InputActionState(Actions.Weapon.SpawnerArm);
	}

	public InputBinding[] GetConflicts(InputBinding binding)
	{
		InputControl val = InputSystem.FindControl(((InputBinding)(ref binding)).path);
		if (val == null)
		{
			return (InputBinding[])(object)new InputBinding[0];
		}
		if (conflicts.TryGetValue(val, out var value))
		{
			return value;
		}
		return (InputBinding[])(object)new InputBinding[0];
	}

	public void Enable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Actions.Enable();
		ValidateBindings(Actions.KeyboardMouseScheme);
	}

	public void Disable()
	{
		Actions.Disable();
	}
}
