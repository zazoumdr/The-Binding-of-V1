using System.Collections;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class FinalCyberRank : MonoBehaviour
{
	public TMP_Text waveText;

	public TMP_Text killsText;

	public TMP_Text styleText;

	public TMP_Text timeText;

	public TMP_Text bestWaveText;

	public TMP_Text bestKillsText;

	public TMP_Text bestStyleText;

	public TMP_Text bestTimeText;

	public TMP_Text pointsText;

	public int totalPoints;

	public GameObject[] toAppear;

	private bool skipping;

	private float timeBetween = 0.25f;

	private bool countTime;

	public float savedTime;

	private float checkedSeconds;

	private float seconds;

	private float minutes;

	private bool countWaves;

	public float savedWaves;

	private float checkedWaves;

	private bool countKills;

	public int savedKills;

	private float checkedKills;

	private bool countStyle;

	public int savedStyle;

	private float checkedStyle;

	private bool flashFade;

	private Color flashColor;

	private Image flashPanel;

	private int i;

	private bool gameOver;

	private bool complete;

	private CyberRankData previousBest;

	private bool newBest;

	private TimeController timeController;

	private OptionsManager opm;

	private bool wasPaused;

	private StatsManager sman;

	private bool highScoresDisplayed;

	[SerializeField]
	private GameObject[] previousElements;

	[SerializeField]
	private GameObject highScoreElement;

	[SerializeField]
	private GameObject friendContainer;

	[SerializeField]
	private GameObject globalContainer;

	[SerializeField]
	private GameObject friendPlaceholder;

	[SerializeField]
	private GameObject globalPlaceholder;

	[SerializeField]
	private GameObject template;

	[SerializeField]
	private TMP_Text tRank;

	[SerializeField]
	private TMP_Text tUsername;

	[SerializeField]
	private TMP_Text tScore;

	[SerializeField]
	private TMP_Text tPercent;

	[SerializeField]
	private GameObject[] templateHighlight;

	[SerializeField]
	private ScrollRect friendScrollRect;

	[SerializeField]
	private float controllerScrollSpeed = 250f;

	private int ownEntryIndex = -1;

	private void Start()
	{
		sman = MonoSingleton<StatsManager>.Instance;
		GameObject[] array = toAppear;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		MonoSingleton<NewMovement>.Instance.endlessMode = true;
	}

	public void GameOver()
	{
		if (gameOver)
		{
			return;
		}
		if (sman == null)
		{
			sman = MonoSingleton<StatsManager>.Instance;
		}
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		gameOver = true;
		sman.StopTimer();
		sman.HideShit();
		MonoSingleton<TimeController>.Instance.controlTimeScale = false;
		savedTime = sman.seconds;
		savedKills = sman.kills;
		savedStyle = sman.stylePoints;
		if (savedStyle < 0)
		{
			savedStyle = 0;
		}
		ActivateNextWave activateNextWave = Object.FindObjectOfType<ActivateNextWave>();
		savedWaves = (float)MonoSingleton<EndlessGrid>.Instance.currentWave + (float)activateNextWave.deadEnemies / (float)MonoSingleton<EndlessGrid>.Instance.enemyAmount;
		previousBest = GameProgressSaver.GetBestCyber();
		bestWaveText.text = Mathf.FloorToInt(previousBest.preciseWavesByDifficulty[num]) + $"\n<color=#616161><size=20>{CalculatePerc(previousBest.preciseWavesByDifficulty[num])}%</size></color>";
		bestKillsText.text = previousBest.kills[num].ToString() ?? "";
		bestStyleText.text = previousBest.style[num].ToString() ?? "";
		int num2 = 0;
		float num3;
		for (num3 = previousBest.time[num]; num3 >= 60f; num3 -= 60f)
		{
			num2++;
		}
		bestTimeText.text = num2 + ":" + num3.ToString("00.000");
		if (sman.majorUsed || MonoSingleton<AssistController>.Instance.cheatsEnabled || MonoSingleton<EndlessGrid>.Instance.customPatternMode)
		{
			return;
		}
		if (LeaderboardController.CanSubmitScores)
		{
			MonoSingleton<LeaderboardController>.Instance.SubmitCyberGrindScore(num, savedWaves, savedKills, savedStyle, sman.seconds);
		}
		if (savedWaves > previousBest.preciseWavesByDifficulty[num])
		{
			NewBest();
		}
		else
		{
			if (savedWaves < previousBest.preciseWavesByDifficulty[num])
			{
				return;
			}
			if (savedKills > previousBest.kills[num])
			{
				NewBest();
			}
			else if (savedKills >= previousBest.kills[num])
			{
				if (savedStyle > previousBest.style[num])
				{
					NewBest();
				}
				else if (savedStyle >= previousBest.style[num] && savedTime > previousBest.time[num])
				{
					NewBest();
				}
			}
		}
	}

	private void NewBest()
	{
		GameProgressSaver.SetBestCyber(this);
		newBest = true;
	}

	private void Update()
	{
		if (gameOver)
		{
			if (timeController == null)
			{
				timeController = MonoSingleton<TimeController>.Instance;
			}
			if (timeController.timeScale > 0f)
			{
				timeController.timeScale = Mathf.MoveTowards(timeController.timeScale, 0f, Time.unscaledDeltaTime * (timeController.timeScale + 0.01f));
				Time.timeScale = timeController.timeScale * timeController.timeScaleModifier;
				MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", timeController.timeScale);
				if (timeController.timeScale < 0.1f)
				{
					MonoSingleton<AudioMixerController>.Instance.forceOff = true;
					MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", MonoSingleton<AudioMixerController>.Instance.CalculateVolume(timeController.timeScale * 10f * MonoSingleton<AudioMixerController>.Instance.sfxVolume));
					MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allVolume", MonoSingleton<AudioMixerController>.Instance.CalculateVolume(timeController.timeScale * 10f * MonoSingleton<AudioMixerController>.Instance.musicVolume));
				}
				MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allPitch", timeController.timeScale);
				MonoSingleton<MusicManager>.Instance.volume = 0.5f + timeController.timeScale / 2f;
				if (timeController.timeScale <= 0f)
				{
					Appear();
					MonoSingleton<MusicManager>.Instance.forcedOff = true;
					MonoSingleton<MusicManager>.Instance.StopMusic();
				}
			}
		}
		if (countTime)
		{
			if (savedTime >= checkedSeconds)
			{
				if (savedTime > checkedSeconds)
				{
					float num = savedTime - checkedSeconds;
					checkedSeconds += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * num * 1.5f;
					seconds += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * num * 1.5f;
				}
				if (checkedSeconds >= savedTime || skipping)
				{
					checkedSeconds = savedTime;
					seconds = savedTime;
					minutes = 0f;
					while (seconds >= 60f)
					{
						seconds -= 60f;
						minutes += 1f;
					}
					countTime = false;
					((Component)(object)timeText).GetComponent<AudioSource>().Stop();
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				if (seconds >= 60f)
				{
					seconds -= 60f;
					minutes += 1f;
				}
				timeText.text = minutes + ":" + seconds.ToString("00.000");
			}
		}
		else if (countWaves)
		{
			if (savedWaves >= checkedWaves)
			{
				if (savedWaves > checkedWaves)
				{
					checkedWaves += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * (savedWaves - checkedWaves) * 1.5f;
				}
				if (checkedWaves >= savedWaves || skipping)
				{
					checkedWaves = savedWaves;
					countWaves = false;
					((Component)(object)waveText).GetComponent<AudioSource>().Stop();
					totalPoints += Mathf.FloorToInt(savedWaves) * 100;
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				else
				{
					int num2 = totalPoints + Mathf.RoundToInt(checkedWaves) * 100;
					int num3 = 0;
					while (num2 >= 1000)
					{
						num3++;
						num2 -= 1000;
					}
					if (num3 > 0)
					{
						if (num2 < 10)
						{
							pointsText.text = "+" + num3 + ",00" + num2 + "<color=orange>P</color>";
						}
						else if (num2 < 100)
						{
							pointsText.text = "+" + num3 + ",0" + num2 + "<color=orange>P</color>";
						}
						else
						{
							pointsText.text = "+" + num3 + "," + num2 + "<color=orange>P</color>";
						}
					}
					else
					{
						pointsText.text = "+" + num2 + "<color=orange>P</color>";
					}
				}
				waveText.text = Mathf.FloorToInt(checkedWaves) + $"\n<color=#616161><size=20>{CalculatePerc(savedWaves)}%</size></color>";
			}
		}
		else if (countKills)
		{
			if ((float)savedKills >= checkedKills)
			{
				if ((float)savedKills > checkedKills)
				{
					checkedKills += Time.unscaledDeltaTime * 20f + Time.unscaledDeltaTime * ((float)savedKills - checkedKills) * 1.5f;
				}
				if (checkedKills >= (float)savedKills || skipping)
				{
					checkedKills = savedKills;
					countKills = false;
					((Component)(object)killsText).GetComponent<AudioSource>().Stop();
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				}
				killsText.text = checkedKills.ToString("0");
			}
		}
		else if (countStyle && (float)savedStyle >= checkedStyle)
		{
			_ = checkedStyle;
			if ((float)savedStyle > checkedStyle)
			{
				checkedStyle += Time.unscaledDeltaTime * 2500f + Time.unscaledDeltaTime * ((float)savedStyle - checkedStyle) * 1.5f;
			}
			if (checkedStyle >= (float)savedStyle || skipping)
			{
				checkedStyle = savedStyle;
				countStyle = false;
				((Component)(object)styleText).GetComponent<AudioSource>().Stop();
				StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
				totalPoints += savedStyle;
				PointsShow();
			}
			else
			{
				int num4 = totalPoints + Mathf.RoundToInt(checkedStyle);
				int num5 = 0;
				while (num4 >= 1000)
				{
					num5++;
					num4 -= 1000;
				}
				if (num5 > 0)
				{
					if (num4 < 10)
					{
						pointsText.text = "+" + num5 + ",00" + num4 + "<color=orange>P</color>";
					}
					else if (num4 < 100)
					{
						pointsText.text = "+" + num5 + ",0" + num4 + "<color=orange>P</color>";
					}
					else
					{
						pointsText.text = "+" + num5 + "," + num4 + "<color=orange>P</color>";
					}
				}
				else
				{
					pointsText.text = "+" + num4 + "<color=orange>P</color>";
				}
			}
			styleText.text = checkedStyle.ToString("0");
		}
		if (flashFade)
		{
			flashColor.a = Mathf.MoveTowards(flashColor.a, 0f, Time.unscaledDeltaTime * 0.5f);
			((Graphic)flashPanel).color = flashColor;
			if (flashColor.a <= 0f)
			{
				flashFade = false;
			}
		}
		if (!gameOver)
		{
			return;
		}
		if (timeController == null)
		{
			timeController = MonoSingleton<TimeController>.Instance;
		}
		if (opm == null)
		{
			opm = MonoSingleton<OptionsManager>.Instance;
		}
		if (opm.paused && !wasPaused)
		{
			wasPaused = true;
		}
		else if (!opm.paused && wasPaused)
		{
			wasPaused = false;
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allVolume", MonoSingleton<AudioMixerController>.Instance.CalculateVolume(timeController.timeScale * 10f * MonoSingleton<AudioMixerController>.Instance.sfxVolume));
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allPitch", 0f);
			MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("allVolume", MonoSingleton<AudioMixerController>.Instance.CalculateVolume(timeController.timeScale * 10f * MonoSingleton<AudioMixerController>.Instance.musicVolume));
		}
		if (highScoresDisplayed && (bool)(Object)(object)friendScrollRect && (bool)friendScrollRect.content)
		{
			float num6 = ((Mouse.current != null) ? ((InputControl<float>)(object)((Vector2Control)Mouse.current.scroll).y).ReadValue() : 0f);
			if (num6 != 0f)
			{
				ScrollRect obj = friendScrollRect;
				obj.verticalNormalizedPosition += num6 / friendScrollRect.content.sizeDelta.y;
			}
			if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad && Gamepad.current != null)
			{
				Vector2 vector = ((InputControl<Vector2>)(object)Gamepad.current.rightStick).ReadValue();
				if (vector.y != 0f)
				{
					ScrollRect obj2 = friendScrollRect;
					obj2.verticalNormalizedPosition += 0.01f * Time.unscaledDeltaTime * controllerScrollSpeed * Mathf.Sign(vector.y);
				}
			}
		}
		if (!LeaderboardController.CanSubmitScores || MonoSingleton<EndlessGrid>.Instance.customPatternMode)
		{
			highScoresDisplayed = true;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame) && complete && !opm.paused)
		{
			if (highScoresDisplayed)
			{
				SceneHelper.RestartSceneAsync();
				return;
			}
			highScoresDisplayed = true;
			GameObject[] array = previousElements;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			highScoreElement.SetActive(value: true);
			FetchTheScores();
		}
		else if (timeController.timeScale <= 0f && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame) && !complete && !opm.paused)
		{
			skipping = true;
			timeBetween = 0.01f;
		}
	}

	private int CalculatePerc(float value)
	{
		return Mathf.FloorToInt((value - (float)Mathf.FloorToInt(value)) * 100f);
	}

	private async void FetchTheScores()
	{
		int difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		LeaderboardEntry[] array = await MonoSingleton<LeaderboardController>.Instance.GetCyberGrindScores(difficulty, LeaderboardType.Friends);
		if (!template)
		{
			return;
		}
		ownEntryIndex = -1;
		int rankCounter = 1;
		LeaderboardEntry[] array2 = array;
		Friend user;
		foreach (LeaderboardEntry val in array2)
		{
			TMP_Text obj = tUsername;
			user = val.User;
			obj.text = TruncateUsername(((Friend)(ref user)).Name, 18);
			tScore.text = Mathf.FloorToInt((float)val.Score / 1000f).ToString();
			tPercent.text = $"<color=#616161>{CalculatePerc((float)val.Score / 1000f)}%</color>";
			tRank.text = rankCounter.ToString();
			GameObject[] array3 = templateHighlight;
			if (array3 != null && array3.Length > 0)
			{
				array3 = templateHighlight;
				foreach (GameObject gameObject in array3)
				{
					if (!(gameObject == null))
					{
						user = val.User;
						gameObject.SetActive(((Friend)(ref user)).IsMe);
					}
				}
			}
			user = val.User;
			if (((Friend)(ref user)).IsMe)
			{
				ownEntryIndex = rankCounter - 1;
			}
			GameObject obj2 = Object.Instantiate(template, friendContainer.transform);
			SteamController.FetchAvatar(obj2.GetComponentInChildren<RawImage>(), val.User);
			obj2.SetActive(value: true);
			rankCounter++;
		}
		friendPlaceholder.SetActive(value: false);
		friendContainer.SetActive(value: true);
		if ((bool)(Object)(object)friendScrollRect)
		{
			await Task.Yield();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)friendContainer.transform);
			LeaderboardProperties.ScrollToEntry(friendScrollRect, ownEntryIndex, rankCounter - 1);
		}
		array = await MonoSingleton<LeaderboardController>.Instance.GetCyberGrindScores(difficulty, LeaderboardType.GlobalAround);
		if (!template)
		{
			return;
		}
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			LeaderboardEntry val2 = array2[i];
			TMP_Text obj3 = tUsername;
			user = val2.User;
			obj3.text = TruncateUsername(((Friend)(ref user)).Name, 18);
			tScore.text = Mathf.FloorToInt((float)val2.Score / 1000f).ToString();
			tPercent.text = $"<color=#616161>{CalculatePerc((float)val2.Score / 1000f)}%</color>";
			tRank.text = val2.GlobalRank.ToString();
			GameObject[] array3 = templateHighlight;
			if (array3 != null && array3.Length > 0)
			{
				array3 = templateHighlight;
				foreach (GameObject gameObject2 in array3)
				{
					if (!(gameObject2 == null))
					{
						user = val2.User;
						gameObject2.SetActive(((Friend)(ref user)).IsMe);
					}
				}
			}
			GameObject obj4 = Object.Instantiate(template, globalContainer.transform);
			SteamController.FetchAvatar(obj4.GetComponentInChildren<RawImage>(), val2.User);
			obj4.SetActive(value: true);
		}
		globalPlaceholder.SetActive(value: false);
		globalContainer.SetActive(value: true);
	}

	private static string TruncateUsername(string value, int maxChars)
	{
		if (value.Length > maxChars)
		{
			return value.Substring(0, maxChars);
		}
		return value;
	}

	public void Appear()
	{
		if (i < toAppear.Length)
		{
			if (skipping)
			{
				HudOpenEffect component = toAppear[i].GetComponent<HudOpenEffect>();
				if (component != null)
				{
					component.skip = true;
				}
			}
			if (toAppear[i] == ((Component)(object)timeText).gameObject)
			{
				if (skipping)
				{
					checkedSeconds = savedTime;
					seconds = savedTime;
					minutes = 0f;
					while (seconds >= 60f)
					{
						seconds -= 60f;
						minutes += 1f;
					}
					((Component)(object)timeText).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					timeText.text = minutes + ":" + seconds.ToString("00.000");
				}
				else
				{
					countTime = true;
				}
			}
			else if (toAppear[i] == ((Component)(object)killsText).gameObject)
			{
				if (skipping)
				{
					checkedKills = savedKills;
					((Component)(object)killsText).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					killsText.text = checkedKills.ToString("0");
				}
				else
				{
					countKills = true;
				}
			}
			else if (toAppear[i] == ((Component)(object)waveText).gameObject)
			{
				if (skipping)
				{
					checkedWaves = savedWaves;
					((Component)(object)waveText).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					waveText.text = Mathf.FloorToInt(savedWaves) + $"\n<color=#616161><size=20>{CalculatePerc(savedWaves)}%</size></color>";
				}
				else
				{
					countWaves = true;
				}
			}
			else if (toAppear[i] == ((Component)(object)styleText).gameObject)
			{
				if (skipping)
				{
					checkedStyle = savedStyle;
					styleText.text = checkedStyle.ToString("0");
					((Component)(object)styleText).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
					StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween * 2f));
					totalPoints += savedStyle;
					PointsShow();
				}
				else
				{
					countStyle = true;
				}
			}
			else
			{
				StartCoroutine(InvokeRealtimeCoroutine(Appear, timeBetween));
			}
			toAppear[i].gameObject.SetActive(value: true);
			i++;
		}
		else
		{
			if (newBest)
			{
				GameObject gameObject = bestWaveText.transform.parent.parent.parent.GetChild(1).gameObject;
				FlashPanel(gameObject);
				gameObject.GetComponent<AudioSource>().Play(tracked: true);
				bestWaveText.text = waveText.text;
				bestKillsText.text = killsText.text;
				bestStyleText.text = styleText.text;
				bestTimeText.text = timeText.text;
			}
			if (!complete)
			{
				complete = true;
				GameProgressSaver.AddMoney(totalPoints);
			}
		}
	}

	public void FlashPanel(GameObject panel)
	{
		if (flashFade)
		{
			flashColor.a = 0f;
			((Graphic)flashPanel).color = flashColor;
		}
		flashPanel = panel.GetComponent<Image>();
		flashColor = ((Graphic)flashPanel).color;
		flashColor.a = 1f;
		((Graphic)flashPanel).color = flashColor;
		flashFade = true;
	}

	private void PointsShow()
	{
		int num = totalPoints;
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
				pointsText.text = "+" + num2 + ",00" + num + "<color=orange>P</color>";
			}
			else if (num < 100)
			{
				pointsText.text = "+" + num2 + ",0" + num + "<color=orange>P</color>";
			}
			else
			{
				pointsText.text = "+" + num2 + "," + num + "<color=orange>P</color>";
			}
		}
		else
		{
			pointsText.text = "+" + num + "<color=orange>P</color>";
		}
	}

	private IEnumerator InvokeRealtimeCoroutine(UnityAction action, float seconds)
	{
		yield return new WaitForSecondsRealtime(seconds);
		action?.Invoke();
	}
}
