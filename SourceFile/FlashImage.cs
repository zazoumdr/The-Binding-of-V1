using UnityEngine;
using UnityEngine.UI;

public class FlashImage : MonoBehaviour
{
	private Image img;

	private bool flashing;

	public float speed;

	public float flashAlpha;

	public bool dontFlashOnEnable;

	public bool oneTime;

	private bool flashed;

	private void OnEnable()
	{
		if (!dontFlashOnEnable && (!oneTime || !flashed))
		{
			Flash(flashAlpha);
		}
	}

	public void Flash(float amount)
	{
		if (oneTime)
		{
			if (flashed)
			{
				return;
			}
			flashed = true;
		}
		if (!(Object)(object)img)
		{
			img = GetComponent<Image>();
		}
		((Graphic)img).color = new Color(((Graphic)img).color.r, ((Graphic)img).color.g, ((Graphic)img).color.b, amount);
		flashing = true;
	}

	private void Update()
	{
		if (flashing && (bool)(Object)(object)img)
		{
			((Graphic)img).color = new Color(((Graphic)img).color.r, ((Graphic)img).color.g, ((Graphic)img).color.b, Mathf.MoveTowards(((Graphic)img).color.a, 0f, Time.deltaTime * speed));
			if (((Graphic)img).color.a <= 0f)
			{
				flashing = false;
			}
		}
	}
}
