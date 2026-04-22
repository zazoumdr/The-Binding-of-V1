using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class OnLevelStart : MonoSingleton<OnLevelStart>
{
	public UltrakillEvent onStart;

	private bool activated;

	public bool hideFogUntilStart = true;

	private bool fogHidden;

	public bool levelNameOnStart = true;

	private void Awake()
	{
		if (hideFogUntilStart && RenderSettings.fog)
		{
			RenderSettings.fog = false;
			fogHidden = true;
		}
		base.transform.parent = null;
	}

	public void StartLevel(bool startTimer = true, bool startMusic = true)
	{
		if (!activated)
		{
			activated = true;
			MonoSingleton<PlayerTracker>.Instance.LevelStart();
			onStart.Invoke();
			MonoSingleton<OutdoorLightMaster>.Instance?.FirstDoorOpen();
			MonoSingleton<StatsManager>.Instance.levelStarted = true;
			if (startTimer)
			{
				MonoSingleton<StatsManager>.Instance.StartTimer();
			}
			if (startMusic)
			{
				MonoSingleton<MusicManager>.Instance.StartMusic();
			}
			if (fogHidden)
			{
				fogHidden = false;
				RenderSettings.fog = true;
			}
			if (levelNameOnStart)
			{
				MonoSingleton<LevelNamePopup>.Instance.NameAppearDelayed(1f);
			}
			DisableOnLevelStart[] array = Object.FindObjectsOfType<DisableOnLevelStart>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
	}
}
