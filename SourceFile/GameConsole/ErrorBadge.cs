using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GameConsole;

public class ErrorBadge : MonoBehaviour
{
	[SerializeField]
	private GameObject badgeContainer;

	[SerializeField]
	private TMP_Text errorCountText;

	[SerializeField]
	private CanvasGroup flashGroup;

	[SerializeField]
	private CanvasGroup alertGroup;

	public bool hidden;

	private readonly CustomYieldInstruction waitTime = new WaitForSecondsRealtime(0.03f);

	private void OnEnable()
	{
		Console? instance = MonoSingleton<Console>.Instance;
		instance.onError = (Action)Delegate.Combine(instance.onError, new Action(OnError));
	}

	private void OnDisable()
	{
		if (MonoSingleton<Console>.Instance != null)
		{
			Console? instance = MonoSingleton<Console>.Instance;
			instance.onError = (Action)Delegate.Remove(instance.onError, new Action(OnError));
		}
	}

	private void OnError()
	{
		badgeContainer.SetActive(!hidden);
		Update();
		flashGroup.alpha = 0f;
		StopAllCoroutines();
		if (!hidden)
		{
			StartCoroutine(FlashBadge());
		}
	}

	private IEnumerator FlashBadge()
	{
		flashGroup.alpha = 0f;
		while (flashGroup.alpha < 1f)
		{
			CanvasGroup obj = flashGroup;
			obj.alpha += 0.2f;
			if (alertGroup.alpha < flashGroup.alpha)
			{
				alertGroup.alpha = (Console.IsOpen ? 0f : flashGroup.alpha);
			}
			yield return waitTime;
		}
		flashGroup.alpha = 1f;
		alertGroup.alpha = (Console.IsOpen ? 0f : flashGroup.alpha);
		while (flashGroup.alpha > 0f)
		{
			CanvasGroup obj2 = flashGroup;
			obj2.alpha -= 0.1f;
			yield return waitTime;
		}
		flashGroup.alpha = 0f;
	}

	public void SetEnabled(bool enabled, bool hide = true)
	{
		hidden = !enabled;
		if (enabled)
		{
			if (MonoSingleton<Console>.Instance.errorCount > 0)
			{
				badgeContainer.SetActive(value: true);
				Update();
			}
		}
		else
		{
			badgeContainer.SetActive(value: false);
		}
	}

	public void Dismiss()
	{
		StopAllCoroutines();
		alertGroup.alpha = 0f;
	}

	private void Update()
	{
		Console instance = MonoSingleton<Console>.Instance;
		if (!(instance == null))
		{
			int errorCount = instance.errorCount;
			if (errorCount != 0)
			{
				errorCountText.text = errorCount + ((errorCount == 1) ? " error" : " errors");
			}
		}
	}
}
