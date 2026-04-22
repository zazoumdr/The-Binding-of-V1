using UnityEngine;

public class SetPlayerPref : MonoBehaviour
{
	public string playerPref;

	public int intValue;

	public bool newSystem;

	private void Start()
	{
		if (newSystem)
		{
			MonoSingleton<PrefsManager>.Instance.SetInt(playerPref, intValue);
		}
		else
		{
			PlayerPrefs.SetInt(playerPref, intValue);
		}
	}
}
