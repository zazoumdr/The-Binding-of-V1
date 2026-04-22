using UnityEngine;

public class AmbientGlow : MonoBehaviour
{
	private SpriteRenderer sr;

	private float originalAlpha;

	public float glowVariance = 0.2f;

	public float glowSpeed = 0.2f;

	private float target;

	private Color clr;

	private void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		originalAlpha = sr.color.a;
		target = originalAlpha + glowVariance;
	}

	private void Update()
	{
		clr = sr.color;
		clr.a = Mathf.MoveTowards(sr.color.a, target, Time.deltaTime * glowSpeed);
		sr.color = clr;
		if (clr.a == target)
		{
			if (target > originalAlpha)
			{
				target = originalAlpha - glowVariance;
			}
			else
			{
				target = originalAlpha + glowVariance;
			}
		}
	}
}
