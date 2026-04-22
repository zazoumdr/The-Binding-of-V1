using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class StatsManager : MonoSingleton<StatsManager>
{
	[HideInInspector]
	public GameObject[] checkPoints;

	private GameObject player;

	private NewMovement nm;

	[HideInInspector]
	public Vector3 spawnPos;

	[HideInInspector]
	public CheckPoint currentCheckPoint;

	public int levelNumber;

	[HideInInspector]
	public int kills;

	[HideInInspector]
	public int stylePoints;

	[HideInInspector]
	public int restarts;

	[HideInInspector]
	public int secrets;

	[HideInInspector]
	public float seconds;

	public bool timer;

	public bool firstPlayThrough;

	private bool timerOnOnce;

	[HideInInspector]
	public bool levelStarted;

	[HideInInspector]
	public FinalRank fr;

	private StyleHUD shud;

	private GunControl gunc;

	[HideInInspector]
	public bool infoSent;

	private bool casualFR;

	public int[] timeRanks;

	public int[] killRanks;

	public int[] styleRanks;

	[HideInInspector]
	public int rankScore;

	public GameObject[] secretObjects;

	[HideInInspector]
	public List<int> prevSecrets;

	[HideInInspector]
	public List<int> newSecrets = new List<int>();

	[HideInInspector]
	public bool challengeComplete;

	public AudioClip[] rankSounds;

	[HideInInspector]
	public int maxGlassKills;

	[HideInInspector]
	public GameObject crosshair;

	[HideInInspector]
	public bool tookDamage;

	public GameObject bonusGhost;

	public GameObject bonusGhostSupercharge;

	[HideInInspector]
	public bool majorUsed;

	[HideInInspector]
	public bool endlessMode;

	private AssistController asscon;

	public static event Action checkpointRestart;

	private void Awake()
	{
		GameStateManager.Instance.RegisterState(new GameState("game", base.gameObject)
		{
			playerInputLock = LockMode.Unlock,
			cameraInputLock = LockMode.Unlock,
			cursorLock = LockMode.Lock
		});
		int lvl = -1;
		if (levelNumber == 0)
		{
			int? levelIndexAfterIntermission = MonoSingleton<SceneHelper>.Instance.GetLevelIndexAfterIntermission(SceneHelper.CurrentScene);
			if (levelIndexAfterIntermission.HasValue)
			{
				lvl = levelIndexAfterIntermission.Value;
			}
		}
		RankData rank = GameProgressSaver.GetRank(returnNull: true, lvl);
		if (SceneHelper.IsSceneRankless)
		{
			firstPlayThrough = false;
		}
		else
		{
			firstPlayThrough = rank == null || (rank.ranks != null && rank.ranks.All((int num4) => num4 < 0));
		}
		if (rank != null && rank.levelNumber == levelNumber)
		{
			for (int num = 0; num < secretObjects.Length; num++)
			{
				if (secretObjects[num] == null)
				{
					continue;
				}
				Bonus component2;
				if (rank.secretsFound.Length > num && rank.secretsFound[num])
				{
					if (secretObjects[num].TryGetComponent<Bonus>(out var component))
					{
						component.beenFound = true;
						component.BeenFound();
					}
					secretObjects[num] = null;
					prevSecrets.Add(num);
				}
				else if (secretObjects[num].TryGetComponent<Bonus>(out component2))
				{
					component2.secretNumber = num;
				}
			}
			if (rank.challenge)
			{
				challengeComplete = true;
			}
			return;
		}
		bool flag = false;
		for (int num2 = 0; num2 < secretObjects.Length; num2++)
		{
			if (!(secretObjects[num2] != null))
			{
				flag = true;
				break;
			}
		}
		if (secretObjects == null || (secretObjects.Length != 0 && flag))
		{
			secretObjects = (from b in UnityEngine.Object.FindObjectsOfType<Bonus>()
				select b.gameObject).ToArray();
			if (secretObjects.Length == 0)
			{
				Debug.Log("No secret objects found!");
				secretObjects = Array.Empty<GameObject>();
			}
		}
		for (int num3 = 0; num3 < secretObjects.Length; num3++)
		{
			if (secretObjects[num3].TryGetComponent<Bonus>(out var component3))
			{
				component3.secretNumber = num3;
			}
		}
	}

	private void OnDestroy()
	{
		GameStateManager gameStateManager = GameStateManager.Instance;
		if ((bool)gameStateManager)
		{
			gameStateManager.PopState("game");
		}
	}

	private void Start()
	{
		nm = MonoSingleton<NewMovement>.Instance;
		player = nm.gameObject;
		spawnPos = player.transform.position;
		asscon = MonoSingleton<AssistController>.Instance;
		fr = MonoSingleton<FinalRank>.Instance;
		if (fr != null)
		{
			fr.gameObject.SetActive(value: false);
		}
		spawnPos = player.transform.position;
		GameObject gameObject = GameObject.FindWithTag("PlayerPosInfo");
		if (gameObject != null)
		{
			if (!SceneHelper.IsPlayingCustom)
			{
				PlayerPosInfo component = gameObject.GetComponent<PlayerPosInfo>();
				if (!component.noPosition)
				{
					player.transform.position = component.position;
				}
				player.GetComponent<Rigidbody>().velocity = component.velocity;
				player.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time = component.wooshTime;
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			player.GetComponent<Rigidbody>().velocity = Vector3.down * 100f;
		}
		_ = SceneHelper.IsPlayingCustom;
		rankScore = -1;
	}

	private void Update()
	{
		if ((Input.GetKeyDown(KeyCode.R) || (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)) && nm.hp <= 0 && !nm.endlessMode && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			Restart();
		}
		if (timer)
		{
			seconds += Time.deltaTime * GameStateManager.Instance.TimerModifier;
		}
		if (stylePoints < 0)
		{
			stylePoints = 0;
		}
		if (!endlessMode)
		{
			DiscordController.UpdateStyle(stylePoints);
		}
	}

	public void GetCheckPoint(Vector3 position)
	{
		spawnPos = position;
	}

	public void Restart()
	{
		MonoSingleton<MusicManager>.Instance.ArenaMusicEnd();
		if (!PreventTimerStart.Active)
		{
			timer = true;
		}
		if (currentCheckPoint == null)
		{
			if ((bool)MonoSingleton<MapVarManager>.Instance)
			{
				MonoSingleton<MapVarManager>.Instance.ResetStores();
			}
			SceneHelper.RestartSceneAsync();
		}
		else
		{
			currentCheckPoint.OnRespawn();
			restarts++;
			StatsManager.checkpointRestart?.Invoke();
		}
	}

	public void StartTimer()
	{
		if (!PreventTimerStart.Active)
		{
			timer = true;
		}
		if (timerOnOnce)
		{
			return;
		}
		if (asscon.majorEnabled)
		{
			if (!MonoSingleton<AssistController>.Instance.hidePopup)
			{
				MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=#4C99E6>MAJOR ASSISTS ARE ENABLED.</color>", "", "", 0, silent: true);
			}
			MajorUsed();
		}
		MonoSingleton<PlayerTracker>.Instance.LevelStart();
		timerOnOnce = true;
	}

	public void StopTimer()
	{
		timer = false;
	}

	public void HideShit()
	{
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		shud.ComboOver();
		if (gunc == null)
		{
			gunc = MonoSingleton<GunControl>.Instance;
		}
		gunc.NoWeapon();
		if ((bool)crosshair)
		{
			crosshair.transform.parent.gameObject.SetActive(value: false);
		}
		HudController.Instance.gunCanvas.SetActive(value: false);
	}

	public void UnhideShit()
	{
		if ((bool)crosshair)
		{
			crosshair.transform.parent.gameObject.SetActive(value: true);
		}
		HudController.Instance.gunCanvas.SetActive(value: true);
	}

	public void SendInfo()
	{
		if (infoSent)
		{
			return;
		}
		infoSent = true;
		string text = "";
		rankScore = 0;
		if (!fr)
		{
			fr = MonoSingleton<FinalRank>.Instance;
		}
		fr.gameObject.SetActive(value: true);
		if (fr.casual)
		{
			casualFR = true;
		}
		if (!casualFR)
		{
			text = GetRanks(timeRanks, seconds, reverse: true, addToRankScore: true);
			SetRankSound(text, ((Component)(object)fr.timeRank).gameObject);
			fr.SetTime(seconds, text);
			Debug.Log("Rankscore after time: " + rankScore);
			text = GetRanks(killRanks, kills, reverse: false, addToRankScore: true);
			SetRankSound(text, ((Component)(object)fr.killsRank).gameObject);
			fr.SetKills(kills, text);
			Debug.Log("Rankscore after kills: " + rankScore);
			text = GetRanks(styleRanks, stylePoints, reverse: false, addToRankScore: true);
			SetRankSound(text, ((Component)(object)fr.styleRank).gameObject);
			fr.SetStyle(stylePoints, text);
			Debug.Log("Rankscore after style: " + rankScore);
			fr.SetInfo(restarts, tookDamage, majorUsed, asscon.cheatsEnabled);
			GetFinalRank();
			GameProgressSaver.SaveRank();
			fr.SetSecrets(secrets, secretObjects.Length);
			fr.levelSecrets = secretObjects;
			fr.prevSecrets = prevSecrets;
			if (LeaderboardController.CanSubmitScores)
			{
				string currentScene = SceneHelper.CurrentScene;
				if (!string.IsNullOrEmpty(currentScene) && SceneHelper.CurrentScene.StartsWith("Level ") && MonoSingleton<PrefsManager>.Instance.GetBool("levelLeaderboards"))
				{
					int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
					MonoSingleton<LeaderboardController>.Instance.SubmitLevelScore(currentScene, difficulty, seconds, kills, stylePoints, restarts);
					if (rankScore == 12)
					{
						MonoSingleton<LeaderboardController>.Instance.SubmitLevelScore(currentScene, difficulty, seconds, kills, stylePoints, restarts, pRank: true);
					}
				}
			}
		}
		fr.Appear();
	}

	public string GetRanks(int[] ranksToCheck, float value, bool reverse, bool addToRankScore = false)
	{
		int num = 0;
		bool flag = true;
		while (flag)
		{
			if (num < ranksToCheck.Length)
			{
				if ((reverse && value <= (float)ranksToCheck[num]) || (!reverse && value >= (float)ranksToCheck[num]))
				{
					num++;
					continue;
				}
				if (addToRankScore)
				{
					rankScore += num;
				}
				switch (num)
				{
				case 0:
					return "<color=#0094FF>D</color>";
				case 1:
					return "<color=#4CFF00>C</color>";
				case 2:
					return "<color=#FFD800>B</color>";
				case 3:
					return "<color=#FF6A00>A</color>";
				}
				continue;
			}
			if (addToRankScore)
			{
				rankScore += 4;
			}
			return "<color=#FF0000>S</color>";
		}
		return "X";
	}

	private void GetFinalRank()
	{
		string text = "";
		if (restarts != 0)
		{
			rankScore -= restarts;
		}
		if (rankScore < 0)
		{
			rankScore = 0;
		}
		if (majorUsed)
		{
			if (rankScore == 12)
			{
				rankScore = 11;
			}
			((Graphic)fr.totalRank.transform.parent.GetComponent<Image>()).color = new Color(0.3f, 0.6f, 0.9f, 1f);
		}
		if (rankScore == 12 && !asscon.cheatsEnabled)
		{
			text = "<color=#FFFFFF>P</color>";
			((Graphic)fr.totalRank.transform.parent.GetComponent<Image>()).color = new Color(1f, 0.686f, 0f, 1f);
		}
		else
		{
			float f = (float)rankScore / 3f;
			Debug.Log("Float: " + f);
			Debug.Log("PreInt: " + rankScore);
			rankScore = Mathf.RoundToInt(f);
			Debug.Log("PostInt: " + rankScore);
			if (asscon.cheatsEnabled)
			{
				text = "-";
			}
			else if (majorUsed)
			{
				switch (rankScore)
				{
				case 1:
					text = "C";
					break;
				case 2:
					text = "B";
					break;
				case 3:
					text = "A";
					break;
				case 4:
				case 5:
				case 6:
					text = "S";
					break;
				default:
					text = "D";
					break;
				}
			}
			else
			{
				switch (rankScore)
				{
				case 1:
					text = "<color=#4CFF00>C</color>";
					break;
				case 2:
					text = "<color=#FFD800>B</color>";
					break;
				case 3:
					text = "<color=#FF6A00>A</color>";
					break;
				case 4:
				case 5:
				case 6:
					text = "<color=#FF0000>S</color>";
					break;
				default:
					text = "<color=#0094FF>D</color>";
					break;
				}
			}
		}
		if (asscon.cheatsEnabled)
		{
			rankScore = -1;
			text = "<color=#FFFFFF>_</color>";
			((Graphic)fr.totalRank.transform.parent.GetComponent<Image>()).color = new Color(0.25f, 1f, 0.25f);
		}
		fr.SetRank(text);
	}

	private void SetRankSound(string rank, GameObject target)
	{
		switch (rank)
		{
		case "<color=#FFD800>B</color>":
			target.GetComponent<AudioSource>().clip = rankSounds[0];
			break;
		case "<color=#FF6A00>A</color>":
			target.GetComponent<AudioSource>().clip = rankSounds[1];
			break;
		case "<color=#FF0000>S</color>":
			target.GetComponent<AudioSource>().clip = rankSounds[2];
			break;
		}
	}

	public void MajorUsed()
	{
		if (timer && !majorUsed && !MonoSingleton<AssistController>.Instance.hidePopup)
		{
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=#4C99E6>MAJOR ASSISTS ARE ENABLED.</color>", "", "", 0, silent: true);
		}
		if (timer)
		{
			majorUsed = true;
		}
	}

	public void SecretFound(int i)
	{
		if (!prevSecrets.Contains(i) && !newSecrets.Contains(i))
		{
			GameProgressSaver.SecretFound(i);
			newSecrets.Add(i);
			secretObjects[i] = null;
		}
	}

	public static string DivideMoney(int money)
	{
		string text = "";
		int num = money;
		int num2 = 0;
		while (num >= 1000)
		{
			num2++;
			num -= 1000;
		}
		if (num2 > 0)
		{
			if (num < 10)
			{
				return num2 + ",00" + num;
			}
			if (num < 100)
			{
				return num2 + ",0" + num;
			}
			return num2 + "," + num;
		}
		return num.ToString() ?? "";
	}
}
