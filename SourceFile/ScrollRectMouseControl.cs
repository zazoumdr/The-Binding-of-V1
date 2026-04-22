using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectMouseControl : MonoBehaviour
{
	private ScrollRect m_ScrollRect;

	private void OnEnable()
	{
		m_ScrollRect = GetComponent<ScrollRect>();
	}

	private void Update()
	{
		ScrollRect scrollRect = m_ScrollRect;
		scrollRect.verticalNormalizedPosition += ((InputControl<float>)(object)((Vector2Control)Mouse.current.scroll).y).ReadValue() / m_ScrollRect.content.sizeDelta.y;
	}
}
