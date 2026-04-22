using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class GamepadObjectSelector : MonoBehaviour
{
	private static Stack<GamepadObjectSelector> s_Selectors = new Stack<GamepadObjectSelector>();

	[SerializeField]
	private bool selectOnEnable = true;

	[SerializeField]
	private bool firstChild;

	[SerializeField]
	private bool allowNonInteractable;

	[SerializeField]
	private bool topOnly;

	[SerializeField]
	private bool dontMarkTop;

	[SerializeField]
	[FormerlySerializedAs("target")]
	private GameObject mainTarget;

	[SerializeField]
	private GameObject fallbackTarget;

	private GameObject target
	{
		get
		{
			if (!(mainTarget != null) || !mainTarget.activeInHierarchy)
			{
				return fallbackTarget;
			}
			return mainTarget;
		}
	}

	private void OnEnable()
	{
		if (!dontMarkTop)
		{
			SetTop();
		}
		if (selectOnEnable && MonoSingleton<InputManager>.Instance?.LastButtonDevice is Gamepad)
		{
			Activate();
		}
	}

	private void OnDisable()
	{
		if (s_Selectors.Count > 0 && s_Selectors.Peek() == this && (Object)(object)EventSystem.current != null && MonoSingleton<InputManager>.Instance?.LastButtonDevice is Gamepad)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		PopTop();
	}

	private void Update()
	{
		if (s_Selectors.Count == 0 || s_Selectors.Peek() != this || !(MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad) || (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.activeInHierarchy))
		{
			return;
		}
		foreach (GamepadObjectSelector s_Selector in s_Selectors)
		{
			if (topOnly && s_Selectors.Peek() != this)
			{
				break;
			}
			if ((bool)s_Selector)
			{
				if (s_Selector.firstChild && s_Selector.SelectFirstChild(s_Selector.target) != null)
				{
					break;
				}
				if (!(s_Selector.target == null) && s_Selector.target.TryGetComponent<Selectable>(out var component) && ((Behaviour)(object)component).isActiveAndEnabled && (s_Selector.allowNonInteractable || component.interactable))
				{
					EventSystem.current.SetSelectedGameObject(s_Selector.target);
					break;
				}
			}
		}
	}

	public void Activate()
	{
		if (target == null)
		{
			mainTarget = base.gameObject;
		}
		GameObject gameObject = target;
		if (firstChild && base.gameObject.activeInHierarchy && gameObject != null)
		{
			StartCoroutine(SelectFirstChildOnNextFrame(gameObject));
		}
		else if (base.gameObject.activeInHierarchy && gameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}

	public static void DisableTop()
	{
		GamepadObjectSelector gamepadObjectSelector = s_Selectors.Peek();
		if (gamepadObjectSelector != null)
		{
			gamepadObjectSelector.gameObject.SetActive(value: false);
		}
	}

	public void PopTop()
	{
		if (s_Selectors.Count > 0 && s_Selectors.Peek() == this)
		{
			s_Selectors.Pop();
		}
	}

	public void SetTop()
	{
		if (s_Selectors.Count == 0 || s_Selectors.Peek() != this)
		{
			s_Selectors.Push(this);
		}
	}

	public void SetMainTarget(Selectable sel)
	{
		mainTarget = ((Component)(object)sel).gameObject;
	}

	private IEnumerator SelectFirstChildOnNextFrame(GameObject obj)
	{
		yield return null;
		SelectFirstChild(obj);
	}

	private GameObject SelectFirstChild(GameObject obj)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (obj == null)
		{
			return null;
		}
		Selectable[] componentsInChildren = obj.GetComponentsInChildren<Selectable>();
		foreach (Selectable val in componentsInChildren)
		{
			if (!(((Component)(object)val).gameObject == obj) && ((Behaviour)(object)val).isActiveAndEnabled)
			{
				Navigation navigation = val.navigation;
				if ((int)((Navigation)(ref navigation)).mode != 0 && (allowNonInteractable || val.interactable))
				{
					obj = ((Component)(object)val).gameObject;
					break;
				}
			}
		}
		EventSystem.current.SetSelectedGameObject(obj);
		return obj;
	}
}
