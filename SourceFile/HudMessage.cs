using UnityEngine;
using UnityEngine.InputSystem;

public class HudMessage : MonoBehaviour
{
	public InputActionReference actionReference;

	public InputActionReference[] actionReferences;

	public bool timed;

	public bool deactivating;

	public bool notOneTime;

	public bool dontActivateOnTriggerEnter;

	public bool silent;

	public bool deactiveOnTriggerExit;

	public bool deactiveOnDisable;

	public bool advancedMessage;

	private bool activated;

	[TextArea]
	[Multiline]
	public string message;

	[TextArea]
	[Multiline]
	public string message2;

	public string playerPref;

	private bool colliderless;

	public float timerTime = 5f;

	private string PlayerPref
	{
		get
		{
			string text = playerPref;
			if (!(text == "SecMisTut"))
			{
				if (text == "ShoUseTut")
				{
					return "hideShotgunPopup";
				}
				return playerPref;
			}
			return "secretMissionPopup";
		}
	}

	private void Start()
	{
		if (GetComponent<Collider>() == null)
		{
			colliderless = true;
			if (PlayerPref == "" || playerPref == null)
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void OnEnable()
	{
		if (colliderless && (!activated || notOneTime))
		{
			if (PlayerPref == "")
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded && deactiveOnDisable && activated)
		{
			Done();
		}
	}

	private void Update()
	{
		if (activated && timed)
		{
			MonoSingleton<HudMessageReceiver>.Instance.ForceEnable();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!dontActivateOnTriggerEnter && other.gameObject.CompareTag("Player") && (!activated || notOneTime))
		{
			if (PlayerPref == "")
			{
				PlayMessage();
			}
			else if (!MonoSingleton<PrefsManager>.Instance.GetBool(PlayerPref))
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(PlayerPref, content: true);
				PlayMessage();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!dontActivateOnTriggerEnter && other.gameObject.CompareTag("Player") && activated && deactiveOnTriggerExit)
		{
			Done();
		}
	}

	private void Done()
	{
		activated = false;
		MonoSingleton<HudMessageReceiver>.Instance.ClearMessage();
		Begone();
	}

	private void Begone()
	{
		if (!notOneTime)
		{
			Object.Destroy(this);
		}
	}

	public void PlayMessage(bool hasToBeEnabled = false)
	{
		if (deactivating)
		{
			Done();
		}
		else
		{
			if ((activated && !notOneTime) || (hasToBeEnabled && (!base.gameObject.activeInHierarchy || !base.enabled)))
			{
				return;
			}
			activated = true;
			if (advancedMessage)
			{
				string[] array = ((actionReferences != null) ? new string[actionReferences.Length] : null);
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						string text = MonoSingleton<InputManager>.Instance.GetBindingString(actionReferences[i].action.id);
						if (string.IsNullOrEmpty(text))
						{
							text = "NO BINDING";
						}
						array[i] = text;
					}
					MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage2(message, array, 0, silent, inputBeenProcessed: true, automaticTimer: false);
				}
			}
			else if ((Object)(object)actionReference == null)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message, "", message2, 0, silent, inputBeenProcessed: true, automaticTimer: false);
			}
			else
			{
				string text2 = MonoSingleton<InputManager>.Instance.GetBindingString(actionReference.action.id);
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "NO BINDING";
				}
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message, text2, message2, 0, silent, inputBeenProcessed: true, automaticTimer: false);
			}
			if (timed && notOneTime)
			{
				CancelInvoke("Done");
				Invoke("Done", timerTime);
			}
			else if (timed)
			{
				Invoke("Done", timerTime);
			}
			else if (!deactiveOnTriggerExit && !deactiveOnDisable)
			{
				Invoke("Begone", 1f);
			}
		}
	}

	public void ChangeMessage(string newMessage)
	{
		message = newMessage;
		actionReference = null;
		message2 = "";
	}
}
