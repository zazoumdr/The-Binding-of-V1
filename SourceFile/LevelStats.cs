using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelStats : MonoBehaviour
{
	public bool cyberGrind;

	public bool secretLevel;

	public TMP_Text levelName;

	private bool ready;

	public TMP_Text time;

	public TMP_Text timeRank;

	private float seconds;

	private float minutes;

	public TMP_Text kills;

	public TMP_Text killsRank;

	public TMP_Text style;

	public TMP_Text styleRank;

	public Image[] secrets;

	private bool checkSecrets = true;

	public Sprite filledSecret;

	public TMP_Text challenge;

	public TMP_Text majorAssists;

	[Header("Cyber Grind")]
	public TMP_Text wave;

	public TMP_Text enemiesLeft;

	private StatsManager sman => MonoSingleton<StatsManager>.Instance;

	private void Start()
	{
		if (secretLevel || cyberGrind)
		{
			levelName.text = (cyberGrind ? "THE CYBER GRIND" : "SECRET MISSION");
			ready = true;
			CheckStats();
			return;
		}
		if (SceneHelper.IsPlayingCustom)
		{
			MapInfo instance = MapInfo.Instance;
			levelName.text = ((instance != null) ? instance.levelName : "???");
			ready = true;
			CheckStats();
		}
		RankData rankData = null;
		if (sman.levelNumber != 0 && !Debug.isDebugBuild)
		{
			rankData = GameProgressSaver.GetRank(returnNull: true);
		}
		if (sman.levelNumber != 0 && (Debug.isDebugBuild || (rankData != null && rankData.levelNumber == sman.levelNumber)))
		{
			StockMapInfo instance2 = StockMapInfo.Instance;
			if (instance2 != null)
			{
				levelName.text = instance2.assets.LargeText;
			}
			else
			{
				levelName.text = "???";
			}
			ready = true;
			CheckStats();
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		if (sman.secretObjects.Length < secrets.Length)
		{
			for (int num = secrets.Length - 1; num >= sman.secretObjects.Length; num--)
			{
				((Component)(object)secrets[num]).gameObject.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		if (ready)
		{
			CheckStats();
		}
	}

	private void CheckStats()
	{
		if ((bool)(Object)(object)time)
		{
			seconds = sman.seconds;
			minutes = 0f;
			while (seconds >= 60f)
			{
				seconds -= 60f;
				minutes += 1f;
			}
			time.text = minutes + ":" + seconds.ToString("00.000");
		}
		if ((bool)(Object)(object)timeRank)
		{
			timeRank.text = sman.GetRanks(sman.timeRanks, sman.seconds, reverse: true);
		}
		if (cyberGrind)
		{
			if ((bool)(Object)(object)wave)
			{
				wave.text = MonoSingleton<EndlessGrid>.Instance.waveNumberText.text;
			}
			if ((bool)(Object)(object)enemiesLeft)
			{
				enemiesLeft.text = MonoSingleton<EndlessGrid>.Instance.enemiesLeftText.text;
			}
			return;
		}
		if ((bool)(Object)(object)kills)
		{
			kills.text = sman.kills.ToString();
		}
		if ((bool)(Object)(object)killsRank)
		{
			killsRank.text = sman.GetRanks(sman.killRanks, sman.kills, reverse: false);
		}
		if ((bool)(Object)(object)style)
		{
			style.text = sman.stylePoints.ToString();
		}
		if ((bool)(Object)(object)styleRank)
		{
			styleRank.text = sman.GetRanks(sman.styleRanks, sman.stylePoints, reverse: false);
		}
		if (checkSecrets && secrets != null && secrets.Length != 0)
		{
			bool flag = true;
			int num = 0;
			for (int num2 = sman.secretObjects.Length - 1; num2 >= 0; num2--)
			{
				if (sman.prevSecrets.Contains(num) || sman.newSecrets.Contains(num))
				{
					secrets[num2].sprite = filledSecret;
				}
				else
				{
					flag = false;
				}
				num++;
			}
			if (flag)
			{
				checkSecrets = false;
			}
		}
		if ((bool)(Object)(object)challenge)
		{
			if (MonoSingleton<ChallengeManager>.Instance.challengeDone && !MonoSingleton<ChallengeManager>.Instance.challengeFailed)
			{
				challenge.text = "<color=#FFAF00>YES</color>";
			}
			else
			{
				challenge.text = "NO";
			}
		}
		if ((bool)(Object)(object)majorAssists)
		{
			if (sman.majorUsed)
			{
				majorAssists.text = "<color=#4C99E6>YES</color>";
			}
			else
			{
				majorAssists.text = "NO";
			}
		}
	}
}
