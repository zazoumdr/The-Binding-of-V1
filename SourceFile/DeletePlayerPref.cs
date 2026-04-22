using UnityEngine;

public class DeletePlayerPref : MonoBehaviour
{
	public string playerPref;

	private void Start()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			if (playerPref == "cg_custom_pool")
			{
				playerPref = "cyberGrind.customPool";
			}
			MonoSingleton<PrefsManager>.Instance.DeleteKey(playerPref);
		}
	}
}
