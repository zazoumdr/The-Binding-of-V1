using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingText : MonoBehaviour
{
	public bool oneTime;

	[HideInInspector]
	public bool activated;

	[HideInInspector]
	public bool gotValues;

	[HideInInspector]
	public TMP_Text text;

	[HideInInspector]
	public string message;

	[HideInInspector]
	public AudioSource aud;

	private Coroutine messageRoutine;

	public float secondsBetweenLetters = 0.005f;

	public bool fillMissingText;

	public bool writingCursor;

	public UltrakillEvent onComplete;

	private void Awake()
	{
		if (!gotValues)
		{
			GetValues();
		}
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			text = GetComponent<TMP_Text>();
			message = text.text;
			aud = GetComponent<AudioSource>();
		}
	}

	private void OnEnable()
	{
		if (!oneTime || !activated)
		{
			activated = true;
			messageRoutine = StartCoroutine(PrepText());
		}
	}

	private void OnDisable()
	{
		if (messageRoutine != null)
		{
			if (oneTime && activated)
			{
				text.text = message;
				onComplete.Invoke();
			}
			StopCoroutine(messageRoutine);
		}
	}

	private IEnumerator PrepText()
	{
		yield return ShowText(text, message, secondsBetweenLetters, aud, fillMissingText, skipLineBreaks: false, writingCursor);
		onComplete?.Invoke();
		messageRoutine = null;
	}

	public static IEnumerator ShowText(TMP_Text text, string message, float secondsBetweenLetters = 0.005f, AudioSource clickAudio = null, bool fillMissingText = false, bool skipLineBreaks = false, bool writingCursor = false)
	{
		TimeSince textTimer = 0f;
		int currentLetter = 0;
		text.text = "";
		while (currentLetter < message.Length)
		{
			while ((float)textTimer >= secondsBetweenLetters && currentLetter < message.Length)
			{
				textTimer = (float)textTimer - secondsBetweenLetters;
				if (message[currentLetter] == '<')
				{
					for (; message[currentLetter] != '>' && currentLetter <= message.Length; currentLetter++)
					{
					}
				}
				else if (currentLetter < message.Length - 1)
				{
					for (; currentLetter < message.Length - 1 && ((!writingCursor && message[currentLetter + 1] == ' ') || (skipLineBreaks && message[currentLetter + 1] == '\n')); currentLetter++)
					{
					}
				}
				currentLetter++;
				text.text = message.Substring(0, currentLetter);
				if (currentLetter < message.Length)
				{
					if (fillMissingText)
					{
						if (writingCursor)
						{
							text.text = text.text + "<mark=#" + ColorUtility.ToHtmlStringRGB(((Graphic)text).color) + ">" + message[currentLetter] + "</mark><alpha=#00>" + message.Substring(currentLetter + 1);
						}
						else
						{
							text.text = text.text + "<alpha=#00>" + message.Substring(currentLetter);
						}
					}
					else if (writingCursor)
					{
						text.text += "█";
					}
				}
				if ((bool)(Object)(object)clickAudio && message[currentLetter - 1] != '\n' && message[currentLetter - 1] != ' ')
				{
					clickAudio.Play(tracked: true);
				}
			}
			yield return new WaitForSeconds(secondsBetweenLetters);
		}
	}
}
