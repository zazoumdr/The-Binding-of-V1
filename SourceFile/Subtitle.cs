using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Subtitle : MonoBehaviour
{
	public AudioSource distanceCheckObject;

	public Subtitle nextInChain;

	[SerializeField]
	private float fadeInSpeed = 0.001f;

	[SerializeField]
	private float holdForBase = 2f;

	[SerializeField]
	private float holdForPerChar = 0.1f;

	[SerializeField]
	private float fadeOutSpeed = 0.0001f;

	[SerializeField]
	private float paddingHorizontal;

	[SerializeField]
	private TMP_Text uiText;

	private CanvasGroup group;

	private float currentAlpha;

	private bool isFadingIn;

	private bool chainContinue;

	private float holdFor;

	private bool isFadingOut;

	private TimeSince holdingSince;

	private RectTransform rectTransform;

	private float baseAlpha = 1f;

	private void Awake()
	{
		group = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		Fit();
		string text = new Regex("<[^>]*>").Replace(uiText.text, "");
		holdFor = holdForBase + (float)text.Length * holdForPerChar;
		currentAlpha = 0f;
		isFadingIn = true;
	}

	public void ContinueChain()
	{
		chainContinue = true;
	}

	private void Update()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected I4, but got Unknown
		if ((Object)(object)distanceCheckObject == null)
		{
			baseAlpha = 1f;
		}
		else
		{
			float num = Vector3.Distance(MonoSingleton<CameraController>.Instance.transform.position, ((Component)(object)distanceCheckObject).transform.position);
			float num2 = distanceCheckObject.minDistance + (distanceCheckObject.maxDistance - distanceCheckObject.minDistance) * 0.5f;
			if (num <= num2)
			{
				baseAlpha = 1f;
			}
			else
			{
				float num3 = num - num2;
				float num4 = distanceCheckObject.maxDistance - num2;
				float num5 = Mathf.Clamp01(num3 / num4);
				AudioRolloffMode rolloffMode = distanceCheckObject.rolloffMode;
				switch ((int)rolloffMode)
				{
				case 2:
					baseAlpha = distanceCheckObject.GetCustomCurve((AudioSourceCurveType)0).Evaluate(num5);
					break;
				case 1:
					baseAlpha = 1f - num5;
					break;
				case 0:
					baseAlpha = 1f - Mathf.Clamp01(Mathf.Log10(num3) / Mathf.Log10(num4));
					break;
				}
			}
		}
		if (isFadingIn)
		{
			currentAlpha += fadeInSpeed * Time.deltaTime;
			if (currentAlpha >= 1f)
			{
				currentAlpha = 1f;
				isFadingIn = false;
				holdingSince = 0f;
			}
			group.alpha = currentAlpha * baseAlpha;
			return;
		}
		if (isFadingOut)
		{
			currentAlpha -= fadeOutSpeed * Time.deltaTime;
			if (currentAlpha <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			group.alpha = currentAlpha * baseAlpha;
			return;
		}
		if ((Object)(object)distanceCheckObject != null)
		{
			group.alpha = currentAlpha * baseAlpha;
		}
		if ((float)holdingSince > holdFor && chainContinue)
		{
			isFadingOut = true;
			MonoSingleton<SubtitleController>.Instance.NotifyHoldEnd(this);
			if ((bool)nextInChain)
			{
				nextInChain.ContinueChain();
			}
		}
	}

	private void Fit()
	{
		StartCoroutine(FitAsync());
	}

	private IEnumerator FitAsync()
	{
		yield return new WaitForFixedUpdate();
		float preferredSize = LayoutUtility.GetPreferredSize(uiText.rectTransform, 0);
		uiText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize + paddingHorizontal * 2f);
	}
}
