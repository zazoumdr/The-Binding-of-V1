using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class HudMessageReceiver : MonoSingleton<HudMessageReceiver>
{
	private Image img;

	[HideInInspector]
	public TMP_Text text;

	private AudioSource aud;

	private AudioSource clickAud;

	private HudOpenEffect hoe;

	private string message;

	private string[] inputs;

	private bool inputPreProcessed;

	private bool noSound;

	private Coroutine messageRoutine;

	private bool timer;

	private string fullMessage;

	private void Start()
	{
		img = GetComponent<Image>();
		text = GetComponentInChildren<TMP_Text>();
		aud = GetComponent<AudioSource>();
		clickAud = ((Component)(object)text).GetComponent<AudioSource>();
		hoe = GetComponent<HudOpenEffect>();
	}

	private void Done()
	{
		((Behaviour)(object)img).enabled = false;
		((Behaviour)(object)text).enabled = false;
	}

	public void SendHudMessage(string newmessage, string newinput = "", string newmessage2 = "", int delay = 0, bool silent = false, bool inputBeenProcessed = false, bool automaticTimer = true)
	{
		message = (string.IsNullOrEmpty(newinput) ? newmessage : (newmessage + "{0}" + newmessage2));
		inputs = (string.IsNullOrEmpty(newinput) ? null : new string[1] { newinput });
		noSound = silent;
		timer = automaticTimer;
		inputPreProcessed = inputBeenProcessed;
		Invoke("ShowHudMessage", delay);
	}

	public void SendHudMessage2(string format, string[] newinputs = null, int delay = 0, bool silent = false, bool inputBeenProcessed = false, bool automaticTimer = true)
	{
		message = format;
		inputs = newinputs;
		noSound = silent;
		timer = automaticTimer;
		inputPreProcessed = inputBeenProcessed;
		Invoke("ShowHudMessage", delay);
	}

	private void ShowHudMessage()
	{
		if (inputs == null || inputs.Length == 0)
		{
			fullMessage = message;
		}
		else if (inputPreProcessed)
		{
			string format = message;
			object[] args = inputs;
			fullMessage = string.Format(format, args);
		}
		else
		{
			for (int i = 0; i < inputs.Length; i++)
			{
				string text = "";
				KeyCode keyCode = MonoSingleton<InputManager>.Instance.Inputs[inputs[i]];
				text = keyCode switch
				{
					KeyCode.Mouse0 => "Left Mouse Button", 
					KeyCode.Mouse1 => "Right Mouse Button", 
					KeyCode.Mouse2 => "Middle Mouse Button", 
					_ => keyCode.ToString(), 
				};
				inputs[i] = text;
			}
			string format2 = message;
			object[] args = inputs;
			fullMessage = string.Format(format2, args);
		}
		fullMessage = fullMessage.Replace('$', '\n');
		this.text.text = "";
		hoe.Force();
		if (!noSound)
		{
			aud.Play(tracked: true);
		}
		if (messageRoutine != null)
		{
			StopCoroutine(messageRoutine);
		}
		messageRoutine = StartCoroutine(PrepText());
		CancelInvoke("Done");
		if (timer)
		{
			Invoke("Done", 5f);
		}
	}

	private IEnumerator PrepText()
	{
		((Behaviour)(object)text).enabled = true;
		((Behaviour)(object)img).enabled = true;
		yield return ScrollingText.ShowText(text, fullMessage, 0.005f, clickAud, fillMissingText: false, skipLineBreaks: true, writingCursor: true);
		messageRoutine = null;
	}

	public void ForceEnable()
	{
		((Behaviour)(object)img).enabled = true;
		((Behaviour)(object)text).enabled = true;
	}

	public void ClearMessage()
	{
		CancelInvoke("Done");
		Done();
	}
}
