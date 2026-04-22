using System;
using System.Collections.Generic;
using Discord;
using plog;
using plog.Models;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
	private static readonly Logger Log = new Logger("Discord");

	public static DiscordController Instance;

	[SerializeField]
	private long discordClientId;

	[Space]
	[SerializeField]
	private SerializedActivityAssets customLevelActivityAssets;

	[SerializeField]
	private SerializedActivityAssets missingActivityAssets;

	[SerializeField]
	private ActivityRankIcon[] rankIcons;

	private global::Discord.Discord discord;

	private ActivityManager activityManager;

	private int lastPoints;

	private bool disabled;

	private Activity cachedActivity;

	private void Awake()
	{
		if ((bool)Instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.transform.SetParent(null);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		bool flag = MonoSingleton<PrefsManager>.Instance.GetBool("discordIntegration");
		if (flag)
		{
			Enable();
		}
		disabled = !flag;
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string id, object value)
	{
		if (!(id != "discordIntegration") && value is bool)
		{
			if ((bool)value)
			{
				Enable();
			}
			else
			{
				Disable();
			}
		}
	}

	private void Update()
	{
		if (discord == null || disabled)
		{
			return;
		}
		try
		{
			discord.RunCallbacks();
		}
		catch (Exception)
		{
			Log.Warning("Discord lost", (IEnumerable<Tag>)null, (string)null, (object)null);
			disabled = true;
			discord.Dispose();
		}
	}

	private void OnApplicationQuit()
	{
		if (discord != null && !disabled)
		{
			discord.Dispose();
		}
	}

	public static void UpdateRank(int rank)
	{
		if ((bool)Instance && !Instance.disabled)
		{
			if (Instance.rankIcons.Length <= rank)
			{
				Log.Error("Discord Controller is missing rank names/icons!", (IEnumerable<Tag>)null, (string)null, (object)null);
				return;
			}
			Instance.cachedActivity.Assets.SmallText = Instance.rankIcons[rank].Text;
			Instance.cachedActivity.Assets.SmallImage = Instance.rankIcons[rank].Image;
			Instance.SendActivity();
		}
	}

	public static void UpdateStyle(int points)
	{
		if ((bool)Instance && !Instance.disabled && Instance.lastPoints != points)
		{
			Instance.lastPoints = points;
			Instance.cachedActivity.Details = "STYLE: " + points;
			Instance.SendActivity();
		}
	}

	public static void UpdateWave(int wave)
	{
		if ((bool)Instance && !Instance.disabled && Instance.lastPoints != wave)
		{
			Instance.lastPoints = wave;
			Instance.cachedActivity.Details = "WAVE: " + wave;
			Instance.SendActivity();
		}
	}

	public static void Disable()
	{
		if ((bool)Instance && Instance.discord != null && !Instance.disabled)
		{
			Instance.disabled = true;
			Instance.activityManager.ClearActivity(delegate
			{
			});
		}
	}

	public static void Enable()
	{
		if (!Instance || Instance.discord != null)
		{
			return;
		}
		try
		{
			Instance.discord = new global::Discord.Discord(Instance.discordClientId, 1uL);
			Instance.activityManager = Instance.discord.GetActivityManager();
			Log.Info("Discord initialized!", (IEnumerable<Tag>)null, (string)null, (object)null);
			Instance.disabled = false;
			Instance.ResetActivityCache();
		}
		catch (Exception)
		{
			Log.Info("Couldn't initialize Discord", (IEnumerable<Tag>)null, (string)null, (object)null);
			Instance.disabled = true;
		}
	}

	private void ResetActivityCache()
	{
		cachedActivity = new Activity
		{
			State = "LOADING",
			Assets = 
			{
				LargeImage = "generic",
				LargeText = "LOADING"
			},
			Instance = true
		};
	}

	public void FetchSceneActivity(string scene)
	{
		if (!Instance || Instance.disabled || Instance.discord == null)
		{
			return;
		}
		ResetActivityCache();
		if (SceneHelper.IsPlayingCustom)
		{
			cachedActivity.State = "Playing Custom Level";
			cachedActivity.Assets = customLevelActivityAssets.Deserialize();
		}
		else
		{
			StockMapInfo instance = StockMapInfo.Instance;
			if ((bool)instance)
			{
				cachedActivity.Assets = instance.assets.Deserialize();
				if (string.IsNullOrEmpty(cachedActivity.Assets.LargeImage))
				{
					cachedActivity.Assets.LargeImage = missingActivityAssets.Deserialize().LargeImage;
				}
				if (string.IsNullOrEmpty(cachedActivity.Assets.LargeText))
				{
					cachedActivity.Assets.LargeText = missingActivityAssets.Deserialize().LargeText;
				}
			}
			else
			{
				cachedActivity.Assets = missingActivityAssets.Deserialize();
			}
			if (scene == "Main Menu")
			{
				cachedActivity.State = "Main Menu";
			}
			else
			{
				cachedActivity.State = "DIFFICULTY: " + MonoSingleton<PresenceController>.Instance.diffNames[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")];
			}
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long start = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		cachedActivity.Timestamps = new ActivityTimestamps
		{
			Start = start
		};
		SendActivity();
	}

	private void SendActivity()
	{
		if (discord != null && activityManager != null && !disabled)
		{
			activityManager.UpdateActivity(cachedActivity, delegate
			{
			});
		}
	}
}
