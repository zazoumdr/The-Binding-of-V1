using UnityEngine;
using UnityEngine.UI;

public class PooledSplash : MonoBehaviour
{
	public float defaultPitch = 1f;

	public float pitchVariation = 0.1f;

	public AudioSource aud;

	public float time = 1f;

	public float randomizer;

	public bool scale;

	public bool fade;

	public FadeType ft;

	public float scaleSpeed;

	public float fadeSpeed;

	private SpriteRenderer sr;

	private LineRenderer lr;

	private Light lght;

	private Renderer rend;

	private Image img;

	private bool hasOpacScale;

	private bool hasTint;

	private bool hasColor;

	private Vector3 scaleAmt = Vector3.one;

	public Water.WaterGOType splashType;

	[HideInInspector]
	public PooledWaterStore waterStore;

	private void Start()
	{
		waterStore = MonoSingleton<PooledWaterStore>.Instance;
	}

	private void OnEnable()
	{
		RandomizePitch();
		ScheduleRemove();
		InitializeScaleNFade();
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	private void RandomizePitch()
	{
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if ((Object)(object)aud != null)
		{
			if (pitchVariation == 0f)
			{
				aud.SetPitch(Random.Range(0.8f, 1.2f));
			}
			else
			{
				aud.SetPitch(Random.Range(defaultPitch - pitchVariation, defaultPitch + pitchVariation));
			}
			aud.Play();
		}
	}

	private void ScheduleRemove()
	{
		CancelInvoke();
		Invoke("ReturnToQueue", time + Random.Range(0f - randomizer, randomizer));
	}

	private void ReturnToQueue()
	{
		waterStore.ReturnToQueue(base.gameObject, splashType);
	}

	private void InitializeScaleNFade()
	{
		if (fade)
		{
			switch (ft)
			{
			case FadeType.Sprite:
				sr = GetComponent<SpriteRenderer>();
				break;
			case FadeType.Line:
				lr = GetComponent<LineRenderer>();
				break;
			case FadeType.Light:
				lght = GetComponent<Light>();
				break;
			case FadeType.Renderer:
				rend = GetComponent<Renderer>();
				if (rend == null)
				{
					rend = GetComponentInChildren<Renderer>();
				}
				break;
			case FadeType.UiImage:
				img = GetComponent<Image>();
				break;
			}
		}
		if (rend != null)
		{
			hasOpacScale = rend.material.HasProperty("_OpacScale");
			hasTint = rend.material.HasProperty("_Tint");
			hasColor = rend.material.HasProperty("_Color");
		}
		scaleAmt = base.transform.localScale;
	}

	private void Update()
	{
		if (scale)
		{
			scaleAmt += Vector3.one * Time.deltaTime * scaleSpeed;
			base.transform.localScale = scaleAmt;
		}
		if (fade)
		{
			switch (ft)
			{
			case FadeType.Sprite:
				sr.color = FadeColor(sr.color);
				break;
			case FadeType.UiImage:
				((Graphic)img).color = FadeColor(((Graphic)img).color);
				break;
			case FadeType.Renderer:
				FadeRenderer();
				break;
			case FadeType.Line:
				FadeLine();
				break;
			case FadeType.Light:
				break;
			}
		}
	}

	private void FadeLine()
	{
		if ((bool)lr)
		{
			Color startColor = lr.startColor;
			startColor.a -= fadeSpeed * Time.deltaTime;
			lr.startColor = startColor;
			Color endColor = lr.endColor;
			endColor.a -= fadeSpeed * Time.deltaTime;
			lr.endColor = endColor;
		}
	}

	private Color FadeColor(Color c)
	{
		if (c.a <= 0f && fadeSpeed > 0f)
		{
			return c;
		}
		c.a -= fadeSpeed * Time.deltaTime;
		return c;
	}

	private void FadeRenderer()
	{
		if (hasOpacScale)
		{
			float num = rend.material.GetFloat("_OpacScale");
			num -= fadeSpeed * Time.deltaTime;
			rend.material.SetFloat("_OpacScale", num);
		}
		else if (hasTint || hasColor)
		{
			string text = (hasTint ? "_Tint" : "_Color");
			Color color = rend.material.GetColor(text);
			color.a -= fadeSpeed * Time.deltaTime;
			rend.material.SetColor(text, color);
		}
	}

	public void ChangeFadeSpeed(float newSpeed)
	{
		fadeSpeed = newSpeed;
	}

	public void ChangeScaleSpeed(float newSpeed)
	{
		scaleSpeed = newSpeed;
	}
}
