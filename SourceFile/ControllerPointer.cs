using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[DisallowMultipleComponent]
internal class ControllerPointer : MonoBehaviour
{
	private static RaycastResult? bestResult;

	private PointerEventData eventData;

	private static int ignoreFrame;

	[SerializeField]
	private UnityEvent onPressed;

	[SerializeField]
	private UnityEvent onReleased;

	[SerializeField]
	private UnityEvent onEnter;

	[SerializeField]
	private UnityEvent onExit;

	[SerializeField]
	private float dragThreshold;

	private bool entered;

	private bool pointerDown;

	private bool scrollState;

	public static GraphicRaycaster raycaster;

	public static List<GraphicRaycaster> raycasters = new List<GraphicRaycaster>();

	private List<RaycastResult> results;

	private Vector2? dragPoint;

	private bool dragging;

	public UnityEvent OnPressed => onPressed;

	public UnityEvent OnReleased => onReleased;

	public UnityEvent OnEnter => onEnter;

	public UnityEvent OnExit => onExit;

	private void Awake()
	{
		if (onPressed == null)
		{
			onPressed = new UnityEvent();
		}
		if (onReleased == null)
		{
			onReleased = new UnityEvent();
		}
		if (onEnter == null)
		{
			onEnter = new UnityEvent();
		}
		if (onExit == null)
		{
			onExit = new UnityEvent();
		}
		results = new List<RaycastResult>();
	}

	private void UpdateSlider()
	{
		if (!TryGetComponent<Slider>(out var component))
		{
			return;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			RectTransform component2 = ((Component)(object)component).GetComponent<RectTransform>();
			Vector2 vector = new Vector2(Screen.width, Screen.height) / 2f;
			Rect rect = component2.rect;
			Vector2 point = default(Vector2);
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component2, vector, ((BaseRaycaster)raycaster).eventCamera, ref point))
			{
				if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && rect.Contains(point))
				{
					scrollState = true;
				}
				else if (!scrollState)
				{
					return;
				}
				float num = Mathf.InverseLerp(rect.x, rect.x + rect.width, point.x);
				component.value = component.minValue + num * (component.maxValue - component.minValue);
			}
		}
		else
		{
			scrollState = false;
		}
	}

	private void UpdateScrollbars()
	{
		if (!TryGetComponent<ScrollRect>(out var component))
		{
			return;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			if (component.horizontal)
			{
				UpdateScrollbar(component.horizontalScrollbar);
			}
			if (component.vertical)
			{
				UpdateScrollbar(component.verticalScrollbar);
			}
		}
		else
		{
			scrollState = false;
		}
		RectTransform content = component.content;
		Vector2 vector = new Vector2(Screen.width, Screen.height) / 2f;
		Vector2 point = default(Vector2);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(content, vector, ((BaseRaycaster)raycaster).eventCamera, ref point) && content.rect.Contains(point))
		{
			if (component.horizontal)
			{
				Scrollbar horizontalScrollbar = component.horizontalScrollbar;
				horizontalScrollbar.value += ((InputControl<float>)(object)((Vector2Control)Mouse.current.scroll).x).ReadValue() / 2f / content.rect.height;
			}
			if (component.vertical)
			{
				Scrollbar verticalScrollbar = component.verticalScrollbar;
				verticalScrollbar.value += ((InputControl<float>)(object)((Vector2Control)Mouse.current.scroll).y).ReadValue() / 2f / content.rect.height;
			}
		}
	}

	private void UpdateScrollbar(Scrollbar scroll)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected I4, but got Unknown
		RectTransform component = ((Component)(object)scroll).GetComponent<RectTransform>();
		Vector2 vector = new Vector2(Screen.width, Screen.height) / 2f;
		Rect rect = component.rect;
		Vector2 point = default(Vector2);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component, vector, ((BaseRaycaster)raycaster).eventCamera, ref point))
		{
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && rect.Contains(point))
			{
				scrollState = true;
			}
			else if (!scrollState)
			{
				return;
			}
			Direction direction = scroll.direction;
			switch ((int)direction)
			{
			case 2:
				scroll.value = Mathf.InverseLerp(rect.y, rect.y + rect.height, point.y);
				break;
			case 0:
				scroll.value = Mathf.InverseLerp(rect.x, rect.x + rect.width, point.x);
				break;
			case 3:
				scroll.value = Mathf.InverseLerp(rect.y + rect.height, rect.y, point.y);
				break;
			case 1:
				scroll.value = Mathf.InverseLerp(rect.x + rect.width, rect.x, point.x);
				break;
			}
		}
	}

	private void UpdateRaycasters()
	{
		if (raycasters != null && raycasters.Count > 0)
		{
			for (int num = raycasters.Count - 1; num >= 0; num--)
			{
				if ((Object)(object)raycasters[num] != null && ((Component)(object)raycasters[num]).gameObject.activeInHierarchy && ((Behaviour)(object)raycasters[num]).enabled)
				{
					raycaster = raycasters[num];
					return;
				}
			}
		}
		raycaster = null;
	}

	private void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		UpdateRaycasters();
		if (!(Object)(object)EventSystem.current || !(Object)(object)raycaster || !((BaseRaycaster)raycaster).eventCamera || ((bool)MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused))
		{
			return;
		}
		eventData = new PointerEventData(EventSystem.current)
		{
			button = (InputButton)0,
			position = new Vector2((float)(((Object)(object)raycaster) ? ((BaseRaycaster)raycaster).eventCamera.pixelWidth : Screen.width) / 2f, (float)(((Object)(object)raycaster) ? ((BaseRaycaster)raycaster).eventCamera.pixelHeight : Screen.height) / 2f)
		};
		if ((bool)(Object)(object)raycaster && ignoreFrame != Time.frameCount)
		{
			ignoreFrame = Time.frameCount;
			bestResult = null;
			results.Clear();
			((BaseRaycaster)raycaster).Raycast(eventData, results);
			foreach (RaycastResult result in results)
			{
				RaycastResult current = result;
				if (!((RaycastResult)(ref current)).gameObject.TryGetComponent<Text>(out var _) && (!bestResult.HasValue || bestResult.Value.depth <= current.depth))
				{
					bestResult = current;
				}
			}
		}
		UpdateEvents();
		UpdateSlider();
		UpdateScrollbars();
	}

	private void UpdateEvents()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		if (!bestResult.HasValue)
		{
			return;
		}
		bool flag = entered;
		RaycastResult value = bestResult.Value;
		entered = ((RaycastResult)(ref value)).gameObject == base.gameObject;
		if (entered && !flag)
		{
			ExecuteEvents.Execute<IPointerEnterHandler>(base.gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerEnterHandler);
			onEnter?.Invoke();
		}
		if (entered && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)
		{
			pointerDown = true;
			ExecuteEvents.Execute<IPointerDownHandler>(base.gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute<IPointerClickHandler>(base.gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerClickHandler);
			onPressed?.Invoke();
			dragPoint = eventData.position;
		}
		if (pointerDown && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasCanceledThisFrame)
		{
			pointerDown = false;
			ExecuteEvents.Execute<IPointerUpHandler>(base.gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerUpHandler);
			onReleased?.Invoke();
		}
		if (flag && !entered)
		{
			ExecuteEvents.Execute<IPointerExitHandler>(base.gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerExitHandler);
			onExit?.Invoke();
		}
		if (dragPoint.HasValue)
		{
			Vector2 delta = eventData.position - dragPoint.Value;
			PointerEventData val = new PointerEventData(EventSystem.current)
			{
				button = (InputButton)0,
				position = eventData.position,
				pressPosition = dragPoint.Value,
				delta = delta
			};
			if (pointerDown && entered && delta.sqrMagnitude >= dragThreshold * dragThreshold)
			{
				ExecuteEvents.Execute<IBeginDragHandler>(base.gameObject, (BaseEventData)(object)val, ExecuteEvents.beginDragHandler);
				dragging = true;
			}
			if (dragging)
			{
				ExecuteEvents.Execute<IDragHandler>(base.gameObject, (BaseEventData)(object)val, ExecuteEvents.dragHandler);
			}
			if (!pointerDown | !entered)
			{
				dragging = false;
				dragPoint = null;
				ExecuteEvents.Execute<IEndDragHandler>(base.gameObject, (BaseEventData)(object)val, ExecuteEvents.endDragHandler);
			}
		}
	}
}
