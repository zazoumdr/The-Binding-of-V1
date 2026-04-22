using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextColorSetter : MonoBehaviour
{
	public bool onlyDisabledState;

	private Button button;

	private Graphic originalGraphic;

	private TMP_Text[] texts;

	private void Awake()
	{
		button = GetComponent<Button>();
		texts = GetComponentsInChildren<TMP_Text>();
		GameObject obj = new GameObject("CrossFadeColorProxy");
		obj.SetActive(value: false);
		obj.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		obj.transform.hideFlags = HideFlags.HideInHierarchy;
		CrossFadeColorProxy crossFadeColorProxy = obj.AddComponent<CrossFadeColorProxy>();
		obj.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
		crossFadeColorProxy.setter = this;
		originalGraphic = ((Selectable)button).targetGraphic;
		((Selectable)button).targetGraphic = (Graphic)(object)crossFadeColorProxy;
	}

	public void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		originalGraphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
		if (onlyDisabledState)
		{
			ColorBlock colors;
			Color color;
			if (!((Selectable)button).interactable)
			{
				colors = ((Selectable)button).colors;
				color = ((ColorBlock)(ref colors)).disabledColor;
			}
			else
			{
				colors = ((Selectable)button).colors;
				color = ((ColorBlock)(ref colors)).normalColor;
			}
			targetColor = color;
			TMP_Text[] array = texts;
			for (int i = 0; i < array.Length; i++)
			{
				((Graphic)array[i]).color = targetColor;
			}
		}
		else
		{
			TMP_Text[] array = texts;
			for (int i = 0; i < array.Length; i++)
			{
				((Graphic)array[i]).CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
			}
		}
	}
}
