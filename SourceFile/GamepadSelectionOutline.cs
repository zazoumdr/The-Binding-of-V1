using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamepadSelectionOutline : MonoBehaviour
{
	private static readonly Vector3[] s_Corners = new Vector3[4];

	[SerializeField]
	private Image image;

	[SerializeField]
	private float scrollSpeedPixelsPerSecond = 800f;

	[SerializeField]
	private Vector2 outlineSize = new Vector2(4f, 4f);

	private ScrollRect lastScrollRect;

	private void Update()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		Canvas val = (currentSelectedGameObject ? currentSelectedGameObject.GetComponentInParent<Canvas>() : null);
		if (currentSelectedGameObject == null || !currentSelectedGameObject.activeInHierarchy || !currentSelectedGameObject.TryGetComponent<Selectable>(out var component) || MonoSingleton<InputManager>.Instance == null || !(MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad) || (int)val.renderMode != 0)
		{
			((Behaviour)(object)image).enabled = false;
			return;
		}
		if (((Component)(object)component).TryGetComponent(out ControllerDisallowedSelection component2))
		{
			component2.ApplyFallbackSelection();
			return;
		}
		((Behaviour)(object)image).enabled = true;
		RectTransform component3 = currentSelectedGameObject.GetComponent<RectTransform>();
		RectTransform rect;
		Bounds selectedBounds = GetSelectedBounds(component3, out rect);
		((Graphic)image).rectTransform.anchoredPosition = selectedBounds.center;
		((Graphic)image).rectTransform.sizeDelta = selectedBounds.size + (Vector3)outlineSize;
		ScrollRect componentInParent = component3.GetComponentInParent<ScrollRect>();
		if ((Object)(object)componentInParent != null && !currentSelectedGameObject.TryGetComponent<Scrollbar>(out var _))
		{
			EnsureVisibility(componentInParent, component3, (Object)(object)componentInParent != (Object)(object)lastScrollRect);
		}
		lastScrollRect = componentInParent;
	}

	private Bounds GetSelectedBounds(RectTransform selected, out RectTransform rect)
	{
		if (selected.TryGetComponent<Selectable>(out var component) && (bool)(Object)(object)component.targetGraphic)
		{
			rect = component.targetGraphic.rectTransform;
			return GetRelativeBounds(((Component)(object)image).transform.parent, rect);
		}
		rect = selected;
		return GetRelativeBounds(((Component)(object)image).transform.parent, selected);
	}

	private Bounds GetRelativeBounds(Transform root, RectTransform child)
	{
		child.GetWorldCorners(s_Corners);
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 4; i++)
		{
			Vector3 lhs = root.InverseTransformPoint(s_Corners[i]);
			vector = Vector3.Min(lhs, vector);
			vector2 = Vector3.Max(lhs, vector2);
		}
		Bounds result = new Bounds(vector, Vector3.zero);
		result.Encapsulate(vector2);
		return result;
	}

	private void EnsureVisibility(ScrollRect scrollRect, RectTransform child, bool instantScroll = false)
	{
		Bounds relativeBounds = GetRelativeBounds(scrollRect.content, child);
		if (child.TryGetComponent<GamepadSelectionBoundsExtension>(out var component) && component.Transforms != null)
		{
			RectTransform[] transforms = component.Transforms;
			foreach (RectTransform child2 in transforms)
			{
				relativeBounds.Encapsulate(GetRelativeBounds(scrollRect.content, child2));
			}
		}
		relativeBounds.min -= (Vector3)scrollRect.content.rect.min;
		relativeBounds.max -= (Vector3)scrollRect.content.rect.min;
		float num = scrollRect.content.rect.height - scrollRect.content.rect.height * scrollRect.verticalNormalizedPosition;
		float num2 = scrollRect.content.rect.height - relativeBounds.min.y;
		RectTransform component2;
		float num3 = ((((Component)(object)scrollRect).TryGetComponent(out component2) && num2 < component2.rect.height * 0.75f) ? 1f : ((!(relativeBounds.min.y < num)) ? (relativeBounds.max.y / scrollRect.content.rect.height) : (relativeBounds.min.y / scrollRect.content.rect.height)));
		if (instantScroll)
		{
			scrollRect.verticalNormalizedPosition = num3;
			return;
		}
		float num4 = scrollSpeedPixelsPerSecond / scrollRect.content.rect.height * Time.unscaledDeltaTime;
		if (scrollRect.verticalNormalizedPosition < num3)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Min(scrollRect.verticalNormalizedPosition + num4, num3);
		}
		else if (scrollRect.verticalNormalizedPosition > num3)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Max(scrollRect.verticalNormalizedPosition - num4, num3);
		}
	}
}
