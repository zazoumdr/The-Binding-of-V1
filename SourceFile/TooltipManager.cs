using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
	public GameObject tooltipTemplate;

	private Dictionary<Guid, GameObject> dict = new Dictionary<Guid, GameObject>();

	private RectTransform canvasRect;

	private void Awake()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if ((UnityEngine.Object)(object)componentInParent != null)
		{
			canvasRect = ((Component)(object)componentInParent).GetComponent<RectTransform>();
		}
	}

	public Guid ShowTooltip(Vector2 position, string text = "")
	{
		Guid guid = Guid.NewGuid();
		GameObject gameObject = UnityEngine.Object.Instantiate(tooltipTemplate);
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.SetActive(value: true);
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		((TMP_Text)componentInChildren).text = text;
		((TMP_Text)componentInChildren).ForceMeshUpdate(false, false);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.position = position;
		Vector2 preferredValues = ((TMP_Text)componentInChildren).GetPreferredValues();
		component.sizeDelta = preferredValues;
		EnsureWithinBounds(component);
		dict.Add(guid, gameObject);
		return guid;
	}

	public void HideTooltip(Guid id)
	{
		if (dict.TryGetValue(id, out var value))
		{
			UnityEngine.Object.Destroy(value);
			dict.Remove(id);
		}
	}

	private void EnsureWithinBounds(RectTransform rect)
	{
		if (!(canvasRect == null) && !(rect == null))
		{
			Vector2 sizeDelta = canvasRect.sizeDelta;
			Vector2 sizeDelta2 = rect.sizeDelta;
			Vector2 anchoredPosition = rect.anchoredPosition;
			if (anchoredPosition.x + sizeDelta2.x > sizeDelta.x)
			{
				anchoredPosition.x = sizeDelta.x - sizeDelta2.x;
			}
			if (anchoredPosition.y - sizeDelta2.y < 0f - sizeDelta.y)
			{
				anchoredPosition.y = 0f - sizeDelta.y + sizeDelta2.y;
			}
			rect.anchoredPosition = anchoredPosition;
		}
	}
}
