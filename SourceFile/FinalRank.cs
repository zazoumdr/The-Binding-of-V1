using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class FinalRank : MonoSingleton<FinalRank>
{
	public bool casual;

	public bool dontSavePos;

	public bool reachedSecondPit;

	public TMP_Text time;

	private float savedTime;

	public TMP_Text timeRank;

	private bool countTime;

	private int minutes;

	private float seconds;

	private float checkedSeconds;

	public TMP_Text kills;

	private int savedKills;

	public TMP_Text killsRank;

	private bool countKills;

	private float checkedKills;

	public TMP_Text style;

	private int savedStyle;

	public TMP_Text styleRank;

	private bool countStyle;

	private float checkedStyle;

	public TMP_Text extraInfo;

	public TMP_Text totalRank;

	public TMP_Text secrets;

	public Image[] secretsInfo;

	private int secretsFound;

	public GameObject[] levelSecrets;

	private int checkedSecrets;

	private int secretsCheckProgress;

	private int allSecrets;

	public List<int> prevSecrets;

	public Image[] challenges;

	public GameObject[] toAppear;

	private int i;

	private bool flashFade;

	private Image flashPanel;

	private Color flashColor;

	private float flashMultiplier = 1f;

	private Vector3 maxPos;

	private Vector3 startingPos;

	private Vector3 goalPos;

	public bool complete;

	public GameObject playerPosInfo;

	public Vector3 finalPitPos;

	private AsyncOperation asyncLoad;

	private string oldBundle;

	private bool rankless;

	public GameObject ppiObject;

	public string targetLevelName;

	public TMP_Text pointsText;

	public int totalPoints;

	private bool loadAndActivateScene;

	public bool dependenciesLoaded;

	private bool sceneBundleLoaded;

	private bool skipping;

	private float timeBetween = 0.25f;

	private bool noRestarts;

	private bool noDamage;

	private bool majorAssists;

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		GameObject[] array = toAppear;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		startingPos = base.transform.parent.localPosition;
		maxPos = new Vector3(startingPos.x, startingPos.y - 0.15f, startingPos.z);
	}

	private void Update()
	{
		if (base.transform.parent.localPosition == maxPos)
		{
			goalPos = startingPos;
		}
		else if (base.transform.parent.localPosition == startingPos)
		{
			goalPos = maxPos;
		}
		base.transform.parent.localPosition = Vector3.MoveTowards(base.transform.parent.localPosition, goalPos, Time.deltaTime / 100f);
		if (countTime)
		{
			if (savedTime >= checkedSeconds)
			{
				if (savedTime > checkedSeconds)
				{
					float num = savedTime - checkedSeconds;
					checkedSeconds += Time.deltaTime * 20f + Time.deltaTime * num * 1.5f;
					seconds += Time.deltaTime * 20f + Time.deltaTime * num * 1.5f;
				}
				if (checkedSeconds >= savedTime || skipping)
				{
					checkedSeconds = savedTime;
					seconds = savedTime;
					minutes = 0;
					while (seconds >= 60f)
					{
						seconds -= 60f;
						minutes++;
					}
					countTime = false;
					((Component)(object)time).GetComponent<AudioSource>().Stop();
					Invoke("Appear", timeBetween * 2f);
				}
				if (seconds >= 60f)
				{
					seconds -= 60f;
					minutes++;
				}
				time.text = minutes + ":" + seconds.ToString("00.000");
			}
		}
		else if (countKills)
		{
			if ((float)savedKills >= checkedKills)
			{
				if ((float)savedKills > checkedKills)
				{
					checkedKills += Time.deltaTime * 45f;
				}
				if (checkedKills >= (float)savedKills || skipping)
				{
					checkedKills = savedKills;
					countKills = false;
					((Component)(object)kills).GetComponent<AudioSource>().Stop();
					Invoke("Appear", timeBetween * 2f);
				}
				kills.text = checkedKills.ToString("0");
			}
		}
		else if (countStyle && (float)savedStyle >= checkedStyle)
		{
			_ = checkedStyle;
			if ((float)savedStyle > checkedStyle)
			{
				checkedStyle += Time.deltaTime * 4500f;
			}
			if (checkedStyle >= (float)savedStyle || skipping)
			{
				checkedStyle = savedStyle;
				countStyle = false;
				((Component)(object)style).GetComponent<AudioSource>().Stop();
				Invoke("Appear", timeBetween * 2f);
				totalPoints += savedStyle;
				PointsShow();
			}
			else
			{
				int num2 = totalPoints + Mathf.RoundToInt(checkedStyle);
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
			style.text = checkedStyle.ToString("0");
		}
		if (flashFade)
		{
			flashColor.a -= Time.deltaTime * flashMultiplier;
			((Graphic)flashPanel).color = flashColor;
			if (flashColor.a <= 0f)
			{
				flashFade = false;
			}
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && complete && Time.timeScale != 0f && reachedSecondPit)
		{
			LevelChange();
		}
		else if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && !complete && Time.timeScale != 0f)
		{
			skipping = true;
			timeBetween = 0.01f;
		}
		if (rankless && asyncLoad != null && asyncLoad.progress >= 0.9f)
		{
			LevelChange();
		}
	}

	public void SetTime(float seconds, string rank)
	{
		savedTime = seconds;
		timeRank.text = rank;
		SceneManager.GetSceneByName(targetLevelName);
	}

	public void SetKills(int killAmount, string rank)
	{
		savedKills = killAmount;
		killsRank.text = rank;
	}

	public void SetStyle(int styleAmount, string rank)
	{
		savedStyle = styleAmount;
		styleRank.text = rank;
	}

	public void SetInfo(int restarts, bool damage, bool majorUsed, bool cheatsUsed)
	{
		extraInfo.text = "";
		int num = 1;
		if (!damage)
		{
			num++;
		}
		if (majorUsed)
		{
			num++;
		}
		if (cheatsUsed)
		{
			num++;
		}
		if (cheatsUsed)
		{
			TMP_Text obj = extraInfo;
			obj.text += "- <color=#44FF45>CHEATS USED</color>\n";
		}
		if (majorUsed)
		{
			if (!MonoSingleton<AssistController>.Instance.hidePopup)
			{
				TMP_Text obj2 = extraInfo;
				obj2.text += "- <color=#4C99E6>MAJOR ASSISTS USED</color>\n";
			}
			majorAssists = true;
		}
		if (restarts == 0)
		{
			if (num >= 3)
			{
				TMP_Text obj3 = extraInfo;
				obj3.text += "+ NO RESTARTS\n";
			}
			else
			{
				TMP_Text obj4 = extraInfo;
				obj4.text += "+ NO RESTARTS\n  (+500<color=orange>P</color>)\n";
			}
			noRestarts = true;
		}
		else
		{
			TMP_Text obj5 = extraInfo;
			obj5.text = obj5.text + "- <color=red>" + restarts + "</color> RESTARTS\n";
		}
		if (!damage)
		{
			if (num >= 3)
			{
				TMP_Text obj6 = extraInfo;
				obj6.text += "+ <color=orange>NO DAMAGE</color>\n";
			}
			else
			{
				TMP_Text obj7 = extraInfo;
				obj7.text += "+ <color=orange>NO DAMAGE\n  (</color>+5,000<color=orange>P)</color>\n";
			}
			noDamage = true;
		}
	}

	public void SetRank(string rank)
	{
		totalRank.text = rank;
	}

	public void SetSecrets(int secretsAmount, int maxSecrets)
	{
		secrets.text = 0 + " / " + maxSecrets;
		allSecrets = maxSecrets;
		secretsFound = secretsAmount;
	}

	public void Appear()
	{
		if (i < toAppear.Length)
		{
			if (!casual)
			{
				if (skipping)
				{
					HudOpenEffect component = toAppear[i].GetComponent<HudOpenEffect>();
					if (component != null)
					{
						component.skip = true;
					}
				}
				if (toAppear[i] == ((Component)(object)time).gameObject)
				{
					if (skipping)
					{
						checkedSeconds = savedTime;
						seconds = savedTime;
						minutes = 0;
						while (seconds >= 60f)
						{
							seconds -= 60f;
							minutes++;
						}
						((Component)(object)time).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
						Invoke("Appear", timeBetween * 2f);
						time.text = minutes + ":" + seconds.ToString("00.000");
					}
					else
					{
						countTime = true;
					}
				}
				else if (toAppear[i] == ((Component)(object)kills).gameObject)
				{
					if (skipping)
					{
						checkedKills = savedKills;
						((Component)(object)kills).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
						Invoke("Appear", timeBetween * 2f);
						kills.text = checkedKills.ToString("0");
					}
					else
					{
						countKills = true;
					}
				}
				else if (toAppear[i] == ((Component)(object)style).gameObject)
				{
					if (skipping)
					{
						checkedStyle = savedStyle;
						style.text = checkedStyle.ToString("0");
						((Component)(object)style).GetComponent<AudioSource>().SetPlayOnAwake(playOnAwake: false);
						Invoke("Appear", timeBetween * 2f);
						totalPoints += savedStyle;
						PointsShow();
					}
					else
					{
						countStyle = true;
					}
				}
				else if (toAppear[i] == ((Component)(object)secrets).gameObject)
				{
					if (prevSecrets.Count > 0)
					{
						foreach (int prevSecret in prevSecrets)
						{
							((Graphic)secretsInfo[prevSecret]).color = new Color(0.5f, 0.5f, 0.5f);
							checkedSecrets++;
							secrets.text = checkedSecrets + " / " + levelSecrets.Length;
						}
					}
					toAppear[i].gameObject.SetActive(value: true);
					Invoke("CountSecrets", timeBetween);
				}
				else if (toAppear[i] == ((Component)(object)timeRank).gameObject || toAppear[i] == ((Component)(object)killsRank).gameObject || toAppear[i] == ((Component)(object)styleRank).gameObject)
				{
					switch (toAppear[i].GetComponent<TMP_Text>().text)
					{
					case "<color=#0094FF>D</color>":
						AddPoints(500);
						break;
					case "<color=#4CFF00>C</color>":
						AddPoints(1000);
						break;
					case "<color=#FFD800>B</color>":
						AddPoints(1500);
						break;
					case "<color=#FF6A00>A</color>":
						AddPoints(2000);
						break;
					case "<color=#FF0000>S</color>":
						AddPoints(2500);
						break;
					}
					FlashPanel(toAppear[i].transform.parent.GetChild(1).gameObject);
					Invoke("Appear", timeBetween * 2f);
					if (skipping)
					{
						toAppear[i].GetComponentInChildren<AudioSource>().SetPlayOnAwake(playOnAwake: false);
					}
				}
				else if (toAppear[i] == ((Component)(object)totalRank).gameObject)
				{
					FlashPanel(toAppear[i].transform.parent.GetChild(1).gameObject);
					flashMultiplier = 0.5f;
					Invoke("Appear", timeBetween * 4f);
				}
				else if (toAppear[i] == ((Component)(object)extraInfo).gameObject)
				{
					if (noRestarts)
					{
						AddPoints(500);
					}
					if (noDamage)
					{
						AddPoints(5000);
					}
					Invoke("Appear", timeBetween);
				}
				else
				{
					Invoke("Appear", timeBetween);
				}
			}
			else
			{
				Invoke("Appear", timeBetween);
			}
			toAppear[i].gameObject.SetActive(value: true);
			i++;
			if (!toAppear[0].gameObject.activeSelf)
			{
				toAppear[0].gameObject.SetActive(value: true);
			}
			if (i >= toAppear.Length && !complete)
			{
				complete = true;
				GameProgressSaver.AddMoney(totalPoints);
			}
		}
		else if (!complete)
		{
			complete = true;
			GameProgressSaver.AddMoney(totalPoints);
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

	private void CountSecrets()
	{
		if (levelSecrets.Length != 0)
		{
			if (levelSecrets[secretsCheckProgress] == null && !prevSecrets.Contains(secretsCheckProgress))
			{
				checkedSecrets++;
				secrets.text = checkedSecrets + " / " + levelSecrets.Length;
				((Component)(object)secrets).GetComponent<AudioSource>().Play(tracked: true);
				((Graphic)secretsInfo[secretsCheckProgress]).color = Color.white;
				secretsCheckProgress++;
				AddPoints(1000);
				if (secretsCheckProgress < levelSecrets.Length)
				{
					Invoke("CountSecrets", timeBetween);
				}
				else
				{
					Invoke("Appear", timeBetween);
				}
			}
			else if (secretsCheckProgress < levelSecrets.Length - 1)
			{
				secretsCheckProgress++;
				CountSecrets();
			}
			else
			{
				secretsCheckProgress++;
				Invoke("Appear", timeBetween);
			}
		}
		else
		{
			Invoke("Appear", timeBetween);
		}
	}

	public void RanklessNextLevel(string lvlname)
	{
		if (lvlname != "")
		{
			rankless = true;
			targetLevelName = lvlname;
			SceneHelper.LoadScene(targetLevelName, noBlocker: true);
		}
	}

	public void LevelChange(bool force = false)
	{
		if (SceneHelper.IsPlayingCustom)
		{
			if (force)
			{
				MonoSingleton<OptionsManager>.Instance.QuitMission();
			}
			else if ((bool)MonoSingleton<AdditionalMapDetails>.Instance && MonoSingleton<AdditionalMapDetails>.Instance.hasAuthorLinks)
			{
				base.gameObject.SetActive(value: false);
				MonoSingleton<WorkshopMapEndLinks>.Instance.Show();
			}
			else if (GameStateManager.Instance.currentCustomGame != null && GameStateManager.Instance.currentCustomGame.workshopId.HasValue)
			{
				MonoSingleton<WorkshopMapEndRating>.Instance.enabled = true;
				base.gameObject.SetActive(value: false);
			}
			else
			{
				MonoSingleton<OptionsManager>.Instance.QuitMission();
			}
			return;
		}
		if (playerPosInfo != null)
		{
			if (ppiObject == null)
			{
				ppiObject = Object.Instantiate(playerPosInfo);
			}
			PlayerPosInfo component = ppiObject.GetComponent<PlayerPosInfo>();
			Rigidbody component2 = MonoSingleton<NewMovement>.Instance.gameObject.GetComponent<Rigidbody>();
			component.velocity = component2.velocity;
			component.position = component2.transform.position - finalPitPos;
			component.position = new Vector3(component.position.x, component.position.y, component.position.z - 990f);
			component.wooshTime = component2.GetComponentInChildren<WallCheck>().GetComponent<AudioSource>().time;
			if (dontSavePos || targetLevelName == "Main Menu")
			{
				component.noPosition = true;
			}
		}
		base.gameObject.SetActive(value: false);
		SceneHelper.LoadScene(targetLevelName, noBlocker: true);
	}

	public void AddPoints(int points)
	{
		totalPoints += points;
		PointsShow();
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
}
