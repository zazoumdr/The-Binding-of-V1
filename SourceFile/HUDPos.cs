using System;
using UnityEngine;

public class HUDPos : MonoBehaviour
{
	private bool ready;

	public bool active;

	private Vector3 defaultPos;

	private Vector3 defaultRot;

	public Vector3 reversePos;

	public Vector3 reverseRot;

	[Header("Rect Transform")]
	public bool rectTransform;

	private RectTransform rect;

	private Vector2 anchorsMaxDefault;

	public Vector2 anchorsMax;

	private Vector2 anchorsMinDefault;

	public Vector2 anchorsMin;

	private Vector2 pivotDefault;

	public Vector2 pivot;

	private Vector2 anchoredPositionDefault;

	public Vector2 anchoredPosition;

	private void Start()
	{
		CheckPos();
	}

	private void OnEnable()
	{
		CheckPos();
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "weaponHoldPosition")
		{
			CheckPos();
		}
	}

	public void CheckPos()
	{
		if (!active)
		{
			return;
		}
		if (!ready)
		{
			ready = true;
			if (rectTransform)
			{
				rect = GetComponent<RectTransform>();
				anchoredPositionDefault = rect.anchoredPosition;
				anchorsMaxDefault = rect.anchorMax;
				anchorsMinDefault = rect.anchorMin;
				pivotDefault = rect.pivot;
			}
			else
			{
				defaultPos = base.transform.localPosition;
				defaultRot = base.transform.localRotation.eulerAngles;
			}
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition") == 2)
		{
			if (rectTransform)
			{
				rect.anchorMax = anchorsMax;
				rect.anchorMin = anchorsMin;
				rect.pivot = pivot;
				rect.anchoredPosition = anchoredPosition;
			}
			else
			{
				base.transform.localPosition = reversePos;
				base.transform.localRotation = Quaternion.Euler(reverseRot);
			}
		}
		else if (rectTransform)
		{
			rect.anchorMax = anchorsMaxDefault;
			rect.anchorMin = anchorsMinDefault;
			rect.pivot = pivotDefault;
			rect.anchoredPosition = anchoredPositionDefault;
		}
		else
		{
			base.transform.localPosition = defaultPos;
			base.transform.localRotation = Quaternion.Euler(defaultRot);
		}
	}
}
