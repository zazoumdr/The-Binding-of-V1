using UnityEngine;

public class GreedDoorTorch : MonoBehaviour
{
	private Door dr;

	private Light lt;

	private SpriteRenderer[] sprs;

	private Color clr;

	private void Start()
	{
		dr = GetComponentInParent<Door>();
		lt = GetComponent<Light>();
		sprs = GetComponentsInChildren<SpriteRenderer>();
		UpdateColor();
	}

	private void Update()
	{
		if (clr != dr.currentLightsColor)
		{
			UpdateColor();
		}
	}

	private void UpdateColor()
	{
		clr = dr.currentLightsColor;
		if (sprs.Length != 0)
		{
			SpriteRenderer[] array = sprs;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.color = new Color(clr.r, clr.g, clr.b, spriteRenderer.color.a);
			}
		}
		if ((bool)lt)
		{
			lt.color = clr;
		}
	}
}
