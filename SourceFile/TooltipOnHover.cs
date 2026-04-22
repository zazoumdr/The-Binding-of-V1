using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public TooltipManager tooltipManager;

	public float hoverTime = 0.5f;

	[HideInInspector]
	public bool multiline;

	[HideInInspector]
	public string text;

	private bool hovered;

	private UnscaledTimeSince sinceHoverStart;

	private Guid tooltipId = Guid.Empty;

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
		sinceHoverStart = 0f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
		if (tooltipId != Guid.Empty && tooltipManager != null)
		{
			tooltipManager.HideTooltip(tooltipId);
			tooltipId = Guid.Empty;
		}
	}

	private void Update()
	{
		if (hovered && tooltipManager != null && (float)sinceHoverStart > hoverTime && tooltipId == Guid.Empty)
		{
			if (!multiline)
			{
				text = text.Replace("\n", " ");
			}
			tooltipId = tooltipManager.ShowTooltip(((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue(), text);
		}
	}

	private void OnDisable()
	{
		if (tooltipId != Guid.Empty && tooltipManager != null)
		{
			tooltipManager.HideTooltip(tooltipId);
			tooltipId = Guid.Empty;
		}
	}
}
