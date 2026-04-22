using UnityEngine;
using UnityEngine.Events;

public class GetPlayerPref : MonoBehaviour
{
	public string pref;

	public int valueToCheckFor;

	public UnityEvent onCheckSuccess;

	public UnityEvent onCheckFail;

	private void Awake()
	{
		switch (pref)
		{
		case "DisCha":
			if (PlayerPrefs.GetInt(pref, 0) == valueToCheckFor)
			{
				onCheckSuccess?.Invoke();
			}
			else
			{
				onCheckFail?.Invoke();
			}
			break;
		case "ShoUseTut":
			pref = "hideShotgunPopup";
			if (MonoSingleton<PrefsManager>.Instance.GetBool(pref))
			{
				onCheckSuccess?.Invoke();
			}
			else
			{
				onCheckFail?.Invoke();
			}
			break;
		case "MainMenuEncorePopUp":
			pref = "MainMenuEncorePopUp";
			if (MonoSingleton<PrefsManager>.Instance.GetInt(pref) == valueToCheckFor)
			{
				onCheckSuccess?.Invoke();
			}
			else
			{
				onCheckFail?.Invoke();
			}
			break;
		default:
			pref = "weapon." + pref;
			if (MonoSingleton<PrefsManager>.Instance.GetInt(pref, 1) == valueToCheckFor)
			{
				onCheckSuccess?.Invoke();
			}
			else
			{
				onCheckFail?.Invoke();
			}
			break;
		}
	}
}
