using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsOptionsKey : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public TextMeshProUGUI actionText;

	public Button restoreDefaultsButton;

	public GameObject bindingButtonTemplate;

	public Transform bindingButtonParent;

	public Selectable selectable;

	public GameObject blocker;

	private List<Button> bindingButtons = new List<Button>();

	private bool selected;

	private readonly Color faintTextColor = new Color(1f, 1f, 1f, 0.15f);

	public void OnSelect(BaseEventData eventData)
	{
		selected = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		selected = false;
	}

	private void SubmitPressed(CallbackContext ctx)
	{
		if (selected && bindingButtons.Count > 0)
		{
			((Selectable)bindingButtons[0]).Select();
		}
	}

	private void OnEnable()
	{
		MonoSingleton<InputManager>.Instance.InputSource.Actions.UI.Submit.performed += SubmitPressed;
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<InputManager>.Instance)
		{
			MonoSingleton<InputManager>.Instance.InputSource.Actions.UI.Submit.performed -= SubmitPressed;
		}
	}

	private void Update()
	{
		bool active = MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad;
		blocker.SetActive(active);
	}

	public void RebuildBindings(InputAction action, InputControlScheme controlScheme)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		foreach (Button bindingButton in bindingButtons)
		{
			Object.Destroy(((Component)(object)bindingButton).gameObject);
		}
		bindingButtons.Clear();
		int num = 0;
		int[] bindingsWithGroup = action.GetBindingsWithGroup(((InputControlScheme)(ref controlScheme)).bindingGroup);
		foreach (int num2 in bindingsWithGroup)
		{
			InputBinding binding = action.bindings[num2];
			num++;
			string bindingDisplayStringWithoutOverride = action.GetBindingDisplayStringWithoutOverride(binding, (DisplayStringOptions)4);
			(Button, TextMeshProUGUI, Image, TooltipOnHover) tuple = BuildBindingButton(bindingDisplayStringWithoutOverride);
			Button item = tuple.Item1;
			TextMeshProUGUI txt = tuple.Item2;
			Image img = tuple.Item3;
			TooltipOnHover item2 = tuple.Item4;
			string text = ((TMP_Text)txt).text + "<br>";
			bool flag = false;
			if (((InputBinding)(ref binding)).isComposite)
			{
				BindingSyntax val = InputActionSetupExtensions.ChangeBinding(action, binding);
				BindingSyntax val2 = ((BindingSyntax)(ref val)).NextBinding();
				HashSet<string> hashSet = new HashSet<string>();
				while (((BindingSyntax)(ref val2)).valid)
				{
					InputBinding binding2 = ((BindingSyntax)(ref val2)).binding;
					if (!((InputBinding)(ref binding2)).isPartOfComposite)
					{
						break;
					}
					InputBinding[] conflicts = MonoSingleton<InputManager>.Instance.InputSource.GetConflicts(((BindingSyntax)(ref val2)).binding);
					if (conflicts.Length != 0)
					{
						binding2 = ((BindingSyntax)(ref val2)).binding;
						if (!hashSet.Contains(((InputBinding)(ref binding2)).path))
						{
							flag = true;
							text = text + "<br>" + GenerateTooltip(action, ((BindingSyntax)(ref val2)).binding, conflicts);
							binding2 = ((BindingSyntax)(ref val2)).binding;
							hashSet.Add(((InputBinding)(ref binding2)).path);
						}
					}
					val2 = ((BindingSyntax)(ref val2)).NextBinding();
				}
			}
			else
			{
				InputBinding[] conflicts2 = MonoSingleton<InputManager>.Instance.InputSource.GetConflicts(binding);
				if (conflicts2.Length != 0)
				{
					flag = true;
					text = text + "<br>" + GenerateTooltip(action, binding, conflicts2);
				}
			}
			item2.text = text;
			item2.enabled = true;
			if (flag)
			{
				((Graphic)txt).color = Color.red;
			}
			int index = num2;
			((UnityEvent)(object)item.onClick).AddListener((UnityAction)delegate
			{
				//IL_0132: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				_ = ((Graphic)img).color;
				((Graphic)img).color = Color.red;
				if (((InputBinding)(ref binding)).isComposite)
				{
					MonoSingleton<InputManager>.Instance.RebindComposite(action, index, delegate(string part)
					{
						((TMP_Text)txt).text = "PRESS " + part.ToUpper();
					}, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, delegate
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						BindingSyntax val4 = InputActionSetupExtensions.ChangeBinding(action, index);
						((BindingSyntax)(ref val4)).Erase();
						MonoSingleton<InputManager>.Instance.actionModified?.Invoke(action);
					}, controlScheme);
				}
				else
				{
					MonoSingleton<InputManager>.Instance.Rebind(action, index, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, delegate
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						BindingSyntax val4 = InputActionSetupExtensions.ChangeBinding(action, index);
						((BindingSyntax)(ref val4)).Erase();
						MonoSingleton<InputManager>.Instance.actionModified?.Invoke(action);
					}, controlScheme);
				}
			});
		}
		if (num < 4)
		{
			var (val3, txt2, img2) = BuildNewBindButton();
			((UnityEvent)(object)val3.onClick).AddListener((UnityAction)delegate
			{
				//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
				//IL_018e: Unknown result type (might be due to invalid IL or missing references)
				((Graphic)img2).color = Color.red;
				((Graphic)txt2).color = Color.white;
				((TMP_Text)txt2).text = "...";
				if (action.expectedControlType == "Button")
				{
					MonoSingleton<InputManager>.Instance.Rebind(action, null, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, controlScheme);
				}
				else if (action.expectedControlType == "Vector2")
				{
					MonoSingleton<InputManager>.Instance.RebindComposite(action, null, delegate(string part)
					{
						((TMP_Text)txt2).text = "PRESS " + part.ToUpper();
					}, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, delegate
					{
						//IL_000d: Unknown result type (might be due to invalid IL or missing references)
						RebuildBindings(action, controlScheme);
					}, controlScheme);
				}
			});
		}
		bool flag2 = action.IsActionEqual(MonoSingleton<InputManager>.Instance.defaultActions.FindAction(action.id), ((InputControlScheme)(ref controlScheme)).bindingGroup);
		((Component)(object)restoreDefaultsButton).gameObject.SetActive(!flag2);
		((UnityEventBase)(object)restoreDefaultsButton.onClick).RemoveAllListeners();
		((UnityEvent)(object)restoreDefaultsButton.onClick).AddListener((UnityAction)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			MonoSingleton<InputManager>.Instance.ResetToDefault(action, controlScheme);
			RebuildBindings(action, controlScheme);
		});
		Navigation navigation = selectable.navigation;
		((Navigation)(ref navigation)).mode = (Mode)4;
		((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)bindingButtons[0];
		selectable.navigation = navigation;
	}

	private (Button, TextMeshProUGUI, Image) BuildNewBindButton()
	{
		var (item, val, item2, _) = BuildBindingButton("+");
		((Graphic)val).color = faintTextColor;
		((TMP_Text)val).fontSizeMax = 27f;
		return (item, val, item2);
	}

	private string GenerateTooltip(InputAction action, InputBinding binding, InputBinding[] conflicts)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		string text = action.GetBindingDisplayStringWithoutOverride(binding, (DisplayStringOptions)4).ToUpper();
		string text2 = "<color=red>" + text + " IS BOUND MULTIPLE TIMES:";
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < conflicts.Length; i++)
		{
			InputBinding val = conflicts[i];
			if (!hashSet.Contains(((InputBinding)(ref val)).action))
			{
				text2 += "<br>";
				text2 = text2 + "- " + ((InputBinding)(ref val)).action.ToUpper();
				hashSet.Add(((InputBinding)(ref val)).action);
			}
		}
		return text2 + "</color>";
	}

	private (Button, TextMeshProUGUI, Image, TooltipOnHover) BuildBindingButton(string text)
	{
		GameObject obj = Object.Instantiate(bindingButtonTemplate, bindingButtonParent);
		TextMeshProUGUI componentInChildren = obj.GetComponentInChildren<TextMeshProUGUI>();
		Button component = obj.GetComponent<Button>();
		Image component2 = obj.GetComponent<Image>();
		TooltipOnHover component3 = obj.GetComponent<TooltipOnHover>();
		((TMP_Text)componentInChildren).text = text;
		bindingButtons.Add(component);
		obj.SetActive(value: true);
		return (component, componentInChildren, component2, component3);
	}
}
