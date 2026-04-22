using System.Collections;
using TMPro;
using UnityEngine;

public class TextAppearByLines : MonoBehaviour
{
	[SerializeField]
	private float delay;

	private AudioSource aud;

	private TMP_Text tmp;

	private string fullText;

	private Coroutine coroutine;

	[SerializeField]
	private AudioClip errorSound;

	[SerializeField]
	private AudioClip warningSound;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
		tmp = GetComponent<TMP_Text>();
		fullText = tmp.text;
	}

	private void OnEnable()
	{
		tmp.text = "";
		coroutine = StartCoroutine(AppearText());
	}

	private IEnumerator AppearText()
	{
		int currentChar = 0;
		while (currentChar < fullText.Length)
		{
			if ((bool)(Object)(object)aud)
			{
				if (fullText[currentChar] == '<')
				{
					aud.clip = warningSound;
				}
				else
				{
					aud.clip = errorSound;
				}
				aud.Play(tracked: true);
			}
			for (; currentChar < fullText.Length && fullText[currentChar] != '\n'; currentChar++)
			{
			}
			tmp.text = fullText.Substring(0, currentChar);
			currentChar++;
			yield return new WaitForSeconds(delay);
		}
		coroutine = null;
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Stop()
	{
		aud.Stop();
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
	}
}
