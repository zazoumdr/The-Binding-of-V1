using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashingText : MonoBehaviour
{
	private TextMeshProUGUI text;

	private Color originalColor;

	public Color flashColor;

	public float fadeTime;

	private float fading = 1f;

	public float delay;

	private float cooldown;

	public bool forcePreciseTiming;

	private float previousLerp;

	public AudioSource[] matchToMusic;

	public UltrakillEvent onFlash;

	private void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
		originalColor = ((Graphic)text).color;
		((Graphic)text).color = flashColor;
		if (forcePreciseTiming)
		{
			Invoke("Flash", fadeTime + delay);
		}
		else
		{
			Flash();
		}
	}

	private void Update()
	{
		if (matchToMusic.Length != 0)
		{
			for (int num = matchToMusic.Length - 1; num >= 0; num--)
			{
				if (matchToMusic[num].isPlaying)
				{
					float num2 = matchToMusic[num].time % (fadeTime + delay);
					((Graphic)text).color = Color.Lerp(flashColor, originalColor, num2);
					if (previousLerp > num2)
					{
						onFlash?.Invoke();
					}
					previousLerp = num2;
					break;
				}
			}
			return;
		}
		fading = Mathf.MoveTowards(fading, 0f, Time.deltaTime / fadeTime);
		((Graphic)text).color = Color.Lerp(originalColor, flashColor, fading);
		if (fading == 0f)
		{
			if (cooldown != 0f)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
			}
			if (cooldown == 0f)
			{
				Flash();
			}
		}
	}

	private void Flash()
	{
		fading = 1f;
		cooldown = delay;
		((Graphic)text).color = Color.Lerp(originalColor, flashColor, 1f);
		if (forcePreciseTiming)
		{
			Invoke("Flash", fadeTime + delay);
		}
		onFlash?.Invoke();
	}
}
