using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageFadeIn : MonoBehaviour
{
	private Image img;

	private TMP_Text text;

	public float speed;

	public float maxAlpha = 1f;

	public bool startAt0;

	public UnityEvent onFull;

	private void Start()
	{
		img = GetComponent<Image>();
		text = GetComponent<TMP_Text>();
		if (maxAlpha == 0f)
		{
			maxAlpha = 1f;
		}
		if (startAt0)
		{
			if ((bool)(Object)(object)img)
			{
				((Graphic)img).color = new Color(((Graphic)img).color.r, ((Graphic)img).color.g, ((Graphic)img).color.b, 0f);
			}
			if ((bool)(Object)(object)text)
			{
				((Graphic)text).color = new Color(((Graphic)text).color.r, ((Graphic)text).color.g, ((Graphic)text).color.b, 0f);
			}
		}
	}

	private void Update()
	{
		if ((bool)(Object)(object)img && ((Graphic)img).color.a != maxAlpha)
		{
			Color color = ((Graphic)img).color;
			color.a = Mathf.MoveTowards(color.a, maxAlpha, Time.deltaTime * speed);
			((Graphic)img).color = color;
			if (((Graphic)img).color.a == maxAlpha)
			{
				onFull?.Invoke();
			}
		}
		if ((bool)(Object)(object)text && ((Graphic)text).color.a != maxAlpha)
		{
			Color color2 = ((Graphic)text).color;
			color2.a = Mathf.MoveTowards(color2.a, maxAlpha, Time.deltaTime * speed);
			((Graphic)text).color = color2;
			if (((Graphic)text).color.a == maxAlpha)
			{
				onFull?.Invoke();
			}
		}
	}

	public void ResetFade()
	{
		if ((bool)(Object)(object)img)
		{
			((Graphic)img).color = new Color(((Graphic)img).color.r, ((Graphic)img).color.g, ((Graphic)img).color.b, 0f);
		}
		if ((bool)(Object)(object)text)
		{
			((Graphic)text).color = new Color(((Graphic)text).color.r, ((Graphic)text).color.g, ((Graphic)text).color.b, 0f);
		}
	}

	public void CancelFade()
	{
		ResetFade();
		base.enabled = false;
	}
}
