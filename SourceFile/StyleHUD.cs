using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class StyleHUD : MonoSingleton<StyleHUD>
{
	public Image rankImage;

	public List<StyleRank> ranks;

	public bool showStyleMeter;

	public bool forceMeterOn;

	private int _rankIndex;

	public int maxReachedRank;

	private Queue<string> hudItemsQueue = new Queue<string>();

	private float currentMeter;

	private GameObject styleHud;

	private Slider styleSlider;

	private TMP_Text styleInfo;

	private float rankShaking;

	private Vector3 defaultPos;

	private float rankScale;

	private bool comboActive;

	private StatsManager sman;

	private GunControl gc;

	private float styleNameTime = 0.1f;

	private AudioSource aud;

	[Header("Multipliers")]
	public float bossStyleGainMultiplier = 1.5f;

	public float bossFreshnessDecayMultiplier = 1.5f;

	[Header("Freshness")]
	public bool dualWieldScale;

	public float freshnessDecayPerMove = 0.5f;

	public float freshnessDecayPerSec = 0.25f;

	[Space]
	public float freshnessRegenPerMove = 1f;

	public float freshnessRegenPerSec = 0.5f;

	[Space]
	public List<StyleFreshnessData> freshnessStateData = new List<StyleFreshnessData>();

	private Dictionary<StyleFreshnessState, StyleFreshnessData> freshnessStateDict;

	public TMP_Text freshnessSliderText;

	private float freshnessSliderValue;

	private Dictionary<GameObject, float> weaponFreshness = new Dictionary<GameObject, float>();

	private float minFreshnessCache;

	private int weaponCountCache;

	private Dictionary<int, (float, float)> slotFreshnessLock = new Dictionary<int, (float, float)>();

	public Dictionary<string, float> freshnessDecayMultiplierDict = new Dictionary<string, float>
	{
		{ "ultrakill.shotgunhit", 0.15f },
		{ "ultrakill.nailhit", 0.1f },
		{ "ultrakill.explosionhit", 0.75f },
		{ "ultrakill.exploded", 1.25f },
		{ "ultrakill.kill", 0.3f },
		{ "ultrakill.firehit", 0f },
		{ "ultrakill.quickdraw", 0f },
		{ "ultrakill.projectileboost", 0f },
		{ "ultrakill.doublekill", 0.1f },
		{ "ultrakill.triplekill", 0.1f },
		{ "ultrakill.multikill", 0.1f },
		{ "ultrakill.arsenal", 0f },
		{ "ultrakill.drillhit", 0.025f },
		{ "ultrakill.drillpunch", 1f },
		{ "ultrakill.drillpunchkill", 1f },
		{ "ultrakill.hammerhit", 3f },
		{ "ultrakill.hammerhitheavy", 6f },
		{ "ultrakill.hammerhitred", 8f },
		{ "ultrakill.hammerhityellow", 5f },
		{ "ultrakill.hammerhitgreen", 3f }
	};

	private Dictionary<string, string> idNameDict = new Dictionary<string, string>
	{
		{ "ultrakill.kill", "KILL" },
		{ "ultrakill.doublekill", "<color=orange>DOUBLE KILL</color>" },
		{ "ultrakill.triplekill", "<color=orange>TRIPLE KILL</color>" },
		{ "ultrakill.bigkill", "BIG KILL" },
		{ "ultrakill.bigfistkill", "BIG FISTKILL" },
		{ "ultrakill.headshot", "HEADSHOT" },
		{ "ultrakill.bigheadshot", "BIG HEADSHOT" },
		{ "ultrakill.headshotcombo", "<color=#00ffffff>HEADSHOT COMBO</color>" },
		{ "ultrakill.criticalpunch", "CRITICAL PUNCH" },
		{ "ultrakill.ricoshot", "<color=#00ffffff>RICOSHOT</color>" },
		{ "ultrakill.limbhit", "LIMB HIT" },
		{ "ultrakill.secret", "<color=#00ffffff>SECRET</color>" },
		{ "ultrakill.cannonballed", "CANNONBALLED" },
		{ "ultrakill.cannonballedfrombounce", "<color=green>DUNKED</color>" },
		{ "ultrakill.cannonboost", "<color=green>CANNONBOOST</color>" },
		{ "ultrakill.insurrknockdown", "<color=green>TIME OUT</color>" },
		{ "ultrakill.quickdraw", "<color=#00ffffff>QUICKDRAW</color>" },
		{ "ultrakill.interruption", "<color=green>INTERRUPTION</color>" },
		{ "ultrakill.fistfullofdollar", "<color=#00ffffff>FISTFUL OF DOLLAR</color>" },
		{ "ultrakill.homerun", "HOMERUN" },
		{ "ultrakill.arsenal", "<color=#00ffffff>ARSENAL</color>" },
		{ "ultrakill.catapulted", "<color=#00ffffff>CATAPULTED</color>" },
		{ "ultrakill.splattered", "SPLATTERED" },
		{ "ultrakill.enraged", "<color=red>ENRAGED</color>" },
		{ "ultrakill.instakill", "<color=green>INSTAKILL</color>" },
		{ "ultrakill.fireworks", "<color=#00ffffff>FIREWORKS</color>" },
		{ "ultrakill.fireworksweak", "<color=#00ffffff>JUGGLE</color>" },
		{ "ultrakill.airslam", "<color=#00ffffff>AIR SLAM</color>" },
		{ "ultrakill.airshot", "<color=#00ffffff>AIRSHOT</color>" },
		{ "ultrakill.downtosize", "<color=#00ffffff>DOWN TO SIZE</color>" },
		{ "ultrakill.projectileboost", "<color=green>PROJECTILE BOOST</color>" },
		{ "ultrakill.parry", "<color=green>PARRY</color>" },
		{ "ultrakill.chargeback", "CHARGEBACK" },
		{ "ultrakill.disrespect", "DISRESPECT" },
		{ "ultrakill.groundslam", "GROUND SLAM" },
		{ "ultrakill.overkill", "OVERKILL" },
		{ "ultrakill.friendlyfire", "FRIENDLY FIRE" },
		{ "ultrakill.exploded", "EXPLODED" },
		{ "ultrakill.fried", "FRIED" },
		{ "ultrakill.finishedoff", "<color=#00ffffff>FINISHED OFF</color>" },
		{ "ultrakill.halfoff", "<color=#00ffffff>HALF OFF</color>" },
		{ "ultrakill.mauriced", "MAURICED" },
		{ "ultrakill.bipolar", "BIPOLAR" },
		{ "ultrakill.attripator", "<color=#00ffffff>ATTRAPTOR</color>" },
		{ "ultrakill.nailbombed", "NAILBOMBED" },
		{ "ultrakill.nailbombedalive", "<color=grey>NAILBOMBED</color>" },
		{ "ultrakill.multikill", "<color=orange>MULTIKILL</color>" },
		{ "ultrakill.shotgunhit", "" },
		{ "ultrakill.nailhit", "" },
		{ "ultrakill.explosionhit", "" },
		{ "ultrakill.firehit", "" },
		{ "ultrakill.zapperhit", "" },
		{ "ultrakill.compressed", "COMPRESSED" },
		{ "ultrakill.strike", "<color=#00ffffff>STRIKE!</color>" },
		{ "ultrakill.rocketreturn", "<color=#00ffffff>ROCKET RETURN</color>" },
		{ "ultrakill.roundtrip", "<color=green>ROUND TRIP</color>" },
		{ "ultrakill.serve", "<color=#00ffffff>SERVED</color>" },
		{ "ultrakill.landyours", "<color=green>LANDYOURS</color>" },
		{ "ultrakill.iconoclasm", "ICONOCLASM" },
		{ "ultrakill.heartbreak", "HEARTBREAK" },
		{ "ultrakill.drillhit", "" },
		{ "ultrakill.drillpunch", "<color=green>CORKSCREW BLOW</color>" },
		{ "ultrakill.drillpunchkill", "<color=green>GIGA DRILL BREAK</color>" },
		{ "ultrakill.hammerhit", "" },
		{ "ultrakill.hammerhitheavy", "BLASTING AWAY" },
		{ "ultrakill.hammerhitred", "FULL IMPACT" },
		{ "ultrakill.hammerhityellow", "HEAVY HITTER" },
		{ "ultrakill.hammerhitgreen", "BLUNT FORCE" },
		{ "ultrakill.lightningbolt", "<color=green>RIDE THE LIGHTNING</color>" },
		{ "ultrakill.terminalvelocity", "TERMINAL VELOCITY" },
		{ "ultrakill.insurrstomp", "STOMPED" }
	};

	private Coroutine updateItemsRoutine;

	private WaitForSeconds styleWait = new WaitForSeconds(0.05f);

	public StyleRank currentRank => ranks[rankIndex];

	public int rankIndex
	{
		get
		{
			return _rankIndex;
		}
		private set
		{
			_rankIndex = Mathf.Clamp(value, 0, ranks.Count - 1);
			rankImage.sprite = currentRank.sprite;
		}
	}

	private bool freshnessEnabled
	{
		get
		{
			if (!(MonoSingleton<AssistController>.Instance == null))
			{
				if (MonoSingleton<AssistController>.Instance.majorEnabled)
				{
					return !MonoSingleton<AssistController>.Instance.disableWeaponFreshness;
				}
				return true;
			}
			return true;
		}
	}

	public string GetLocalizedName(string id)
	{
		if (!idNameDict.TryGetValue(id, out var value))
		{
			return id;
		}
		return value;
	}

	private void Start()
	{
		styleHud = base.transform.GetChild(0).gameObject;
		styleSlider = GetComponentInChildren<Slider>();
		styleInfo = GetComponentInChildren<TMP_Text>();
		freshnessStateDict = freshnessStateData.ToDictionary((StyleFreshnessData x) => x.state, (StyleFreshnessData x) => x);
		sman = MonoSingleton<StatsManager>.Instance;
		gc = MonoSingleton<GunControl>.Instance;
		weaponFreshness.Clear();
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			if (allWeapon != null)
			{
				weaponFreshness.Add(allWeapon, 10f);
			}
		}
		foreach (StyleFreshnessData freshnessStateDatum in freshnessStateData)
		{
			freshnessStateDatum.slider.minValue = freshnessStateDatum.min;
			freshnessStateDatum.slider.maxValue = freshnessStateDatum.max;
		}
		ComboOver();
		defaultPos = ((Component)(object)rankImage).transform.localPosition;
		aud = GetComponent<AudioSource>();
	}

	private void Awake()
	{
		defaultPos = ((Component)(object)rankImage).transform.localPosition;
	}

	private void OnEnable()
	{
		if (updateItemsRoutine != null)
		{
			StopCoroutine(updateItemsRoutine);
		}
		updateItemsRoutine = StartCoroutine(UpdateItems());
	}

	private void OnDisable()
	{
		if (updateItemsRoutine != null)
		{
			StopCoroutine(updateItemsRoutine);
		}
	}

	private void Update()
	{
		UpdateMeter();
		UpdateFreshness();
		UpdateHUD();
	}

	private IEnumerator UpdateItems()
	{
		while (true)
		{
			if (hudItemsQueue.Count > 0)
			{
				string text = hudItemsQueue.Dequeue();
				styleInfo.text = text + "\n" + styleInfo.text;
				aud?.Play(tracked: true);
				Invoke("RemoveText", 3f);
				yield return styleWait;
			}
			else
			{
				yield return null;
			}
		}
	}

	private void UpdateMeter()
	{
		if (currentMeter > 0f && !comboActive)
		{
			ComboStart();
		}
		if (currentMeter < 0f)
		{
			DescendRank();
		}
		else
		{
			currentMeter -= Time.deltaTime * (currentRank.drainSpeed * 15f);
		}
		bool flag = comboActive || forceMeterOn;
		if (styleHud.activeSelf != flag)
		{
			styleHud.SetActive(flag);
		}
	}

	private void UpdateFreshness()
	{
		if (!comboActive || !freshnessEnabled || !gc.activated)
		{
			return;
		}
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			if (allWeapon == gc.currentWeapon)
			{
				AddFreshness(allWeapon, (0f - freshnessDecayPerSec) * Time.deltaTime);
				if (slotFreshnessLock.TryGetValue(gc.currentSlotIndex, out var value))
				{
					weaponFreshness[allWeapon] = Mathf.Clamp(weaponFreshness[allWeapon], value.Item1, value.Item2);
				}
			}
			else
			{
				AddFreshness(allWeapon, freshnessRegenPerSec * Time.deltaTime);
			}
		}
	}

	private void UpdateHUD()
	{
		styleSlider.value = currentMeter / (float)currentRank.maxMeter;
		if (freshnessEnabled)
		{
			if (!((Component)(object)freshnessSliderText).gameObject.activeSelf)
			{
				((Component)(object)freshnessSliderText).gameObject.SetActive(value: true);
			}
			if ((bool)gc.currentWeapon)
			{
				float value;
				bool flag = weaponFreshness.TryGetValue(gc.currentWeapon, out value);
				if (!flag)
				{
					Debug.LogWarning("Current weapon not in StyleHUD weaponFreshness dict!!!");
				}
				float t = 30f * Time.deltaTime;
				foreach (KeyValuePair<StyleFreshnessState, StyleFreshnessData> item in freshnessStateDict)
				{
					Slider slider = item.Value.slider;
					if (gc.activated)
					{
						if ((Object)(object)slider != null && gc != null && gc.allWeapons.Count > 0 && gc.currentWeapon != null && flag)
						{
							freshnessSliderValue = Mathf.Lerp(freshnessSliderValue, value, t);
						}
						slider.value = freshnessSliderValue;
					}
				}
			}
		}
		else if (((Component)(object)freshnessSliderText).gameObject.activeSelf)
		{
			((Component)(object)freshnessSliderText).gameObject.SetActive(value: false);
			foreach (StyleFreshnessData freshnessStateDatum in freshnessStateData)
			{
				((Component)(object)freshnessStateDatum.slider).gameObject.SetActive(value: false);
			}
		}
		if (styleNameTime > 0f)
		{
			styleNameTime = Mathf.MoveTowards(styleNameTime, 0f, Time.deltaTime * 2f);
		}
		else
		{
			styleNameTime = 0.1f;
		}
		if (rankShaking > 0f)
		{
			((Component)(object)rankImage).transform.localPosition = new Vector3(defaultPos.x + rankShaking * (float)Random.Range(-3, 3), defaultPos.y + rankShaking * (float)Random.Range(-3, 3), defaultPos.z);
			rankShaking -= Time.deltaTime * 5f;
		}
		else
		{
			((Component)(object)rankImage).transform.localPosition = defaultPos;
		}
		if (rankScale > 0f)
		{
			((Component)(object)rankImage).transform.localScale = new Vector3(1f, 1f, 1f) + Vector3.one * rankScale;
			rankScale -= Time.deltaTime;
		}
		else
		{
			((Component)(object)rankImage).transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void RegisterStyleItem(string id, string name)
	{
		idNameDict.Add(id, name);
	}

	public void ComboStart()
	{
		CancelInvoke("ResetFreshness");
		currentMeter = Mathf.Max(currentMeter, currentRank.maxMeter / 4);
		comboActive = true;
	}

	public void ComboOver()
	{
		currentMeter = 0f;
		rankIndex = 0;
		Invoke("ResetFreshness", 10f);
		comboActive = false;
	}

	private void AscendRank()
	{
		while (currentMeter >= (float)currentRank.maxMeter)
		{
			currentMeter -= currentRank.maxMeter;
			rankIndex++;
			if (rankIndex + 1 == ranks.Count - 1)
			{
				break;
			}
		}
		currentMeter = Mathf.Max(currentMeter, currentRank.maxMeter / 4);
		maxReachedRank = Mathf.Max(maxReachedRank, rankIndex);
		DiscordController.UpdateRank(rankIndex);
	}

	private void UpdateFreshnessSlider()
	{
		StyleFreshnessState freshnessState = GetFreshnessState(gc.currentWeapon);
		freshnessSliderText.text = freshnessStateDict[freshnessState].text;
	}

	public void ResetFreshness()
	{
		gc = gc ?? MonoSingleton<GunControl>.Instance;
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			weaponFreshness[allWeapon] = 10f;
		}
	}

	public void SnapFreshnessSlider()
	{
		if (!(gc == null) && !(gc.currentWeapon == null) && weaponFreshness.TryGetValue(gc.currentWeapon, out var value))
		{
			freshnessSliderValue = value;
		}
	}

	public StyleFreshnessState GetFreshnessState(GameObject sourceWeapon)
	{
		StyleFreshnessState result = StyleFreshnessState.Dull;
		if (!weaponFreshness.TryGetValue(sourceWeapon, out var value))
		{
			Debug.LogWarning("Current weapon not in StyleHUD weaponFreshness dict!!!");
			return StyleFreshnessState.Fresh;
		}
		foreach (KeyValuePair<StyleFreshnessState, StyleFreshnessData> item in freshnessStateDict)
		{
			if (value >= item.Value.min)
			{
				result = item.Key;
			}
		}
		return result;
	}

	public void LockFreshness(int slot, float? min = null, float? max = null)
	{
		if (slotFreshnessLock.TryGetValue(slot, out var value))
		{
			Dictionary<int, (float, float)> dictionary = slotFreshnessLock;
			float? num = min;
			float item;
			if (!num.HasValue)
			{
				(item, _) = value;
			}
			else
			{
				item = num.GetValueOrDefault();
			}
			dictionary[slot] = (item, max ?? value.Item2);
		}
		else
		{
			slotFreshnessLock.Add(slot, (min.GetValueOrDefault(), max ?? 10f));
		}
	}

	public void LockFreshness(int slot, StyleFreshnessState? minState = null, StyleFreshnessState? maxState = null)
	{
		StyleFreshnessData styleFreshnessData = (maxState.HasValue ? freshnessStateDict[maxState.Value] : null);
		StyleFreshnessData styleFreshnessData2 = (minState.HasValue ? freshnessStateDict[minState.Value] : null);
		float value = 0f;
		float value2 = 10f;
		if (styleFreshnessData2 != null)
		{
			value = styleFreshnessData2.justAboveMin;
		}
		if (styleFreshnessData != null)
		{
			value2 = styleFreshnessData.max - 0.01f;
		}
		LockFreshness(slot, value, value2);
	}

	public void UnlockFreshness(int slot)
	{
		slotFreshnessLock.Remove(slot);
	}

	private void ClampFreshness(GameObject sourceWeapon, float amt)
	{
		float max = 10f;
		UpdateMinFreshnessCache(gc.allWeapons.Count);
		float num = minFreshnessCache;
		if (sourceWeapon == gc.currentWeapon && slotFreshnessLock.TryGetValue(gc.currentSlotIndex, out var value))
		{
			num = Mathf.Max(num, value.Item1);
			max = value.Item2;
		}
		weaponFreshness[sourceWeapon] = Mathf.Clamp(amt, num, max);
	}

	public void UpdateMinFreshnessCache(int count)
	{
		if (weaponCountCache == count)
		{
			return;
		}
		weaponCountCache = count;
		if (SummonSandboxArm.armSlot.Count > 0)
		{
			count--;
		}
		if (count <= 1)
		{
			minFreshnessCache = freshnessStateDict[StyleFreshnessState.Fresh].max;
			return;
		}
		switch (count)
		{
		case 2:
			minFreshnessCache = freshnessStateDict[StyleFreshnessState.Used].justAboveMin;
			break;
		case 3:
		case 4:
			minFreshnessCache = freshnessStateDict[StyleFreshnessState.Stale].justAboveMin;
			break;
		default:
			minFreshnessCache = freshnessStateDict[StyleFreshnessState.Dull].justAboveMin;
			break;
		}
	}

	public float GetFreshness(GameObject sourceWeapon)
	{
		return weaponFreshness[sourceWeapon];
	}

	public void SetFreshness(GameObject sourceWeapon, float amt)
	{
		ClampFreshness(sourceWeapon, amt);
		if (sourceWeapon == gc?.currentWeapon)
		{
			UpdateFreshnessSlider();
		}
	}

	public void AddFreshness(GameObject sourceWeapon, float amt)
	{
		float num = amt;
		int dualWieldCount = gc.dualWieldCount;
		if (dualWieldScale && dualWieldCount > 0)
		{
			num /= (float)(dualWieldCount + 1);
		}
		SetFreshness(sourceWeapon, GetFreshness(sourceWeapon) + num);
	}

	public void DecayFreshness(GameObject sourceWeapon, string pointID, bool boss)
	{
		if (!weaponFreshness.TryGetValue(sourceWeapon, out var value))
		{
			Debug.LogWarning($"Weapon {sourceWeapon} not in StyleHUD weaponFreshness dict");
			return;
		}
		float num = freshnessDecayPerMove;
		float num2 = gc.dualWieldCount;
		if (dualWieldScale && num2 > 0f)
		{
			num /= num2 + 1f;
		}
		if (freshnessDecayMultiplierDict.TryGetValue(pointID, out var value2))
		{
			num *= value2;
		}
		if (boss)
		{
			num *= bossFreshnessDecayMultiplier;
		}
		SetFreshness(sourceWeapon, value - num);
		int num3 = gc.slotDict[sourceWeapon];
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			if (!(allWeapon == sourceWeapon) && gc.slotDict[allWeapon] != num3)
			{
				float num4 = freshnessRegenPerMove;
				if (value2 > 0f)
				{
					num4 *= value2;
				}
				AddFreshness(allWeapon, num4);
			}
		}
		if (sourceWeapon == gc?.currentWeapon)
		{
			UpdateFreshnessSlider();
		}
	}

	public void DescendRank()
	{
		if (comboActive)
		{
			if (rankIndex > 0)
			{
				currentMeter = currentRank.maxMeter;
				rankIndex--;
				rankImage.sprite = ranks[rankIndex].sprite;
				currentMeter = currentRank.maxMeter - currentRank.maxMeter / 4;
			}
			else if (rankIndex == 0)
			{
				ComboOver();
			}
			DiscordController.UpdateRank(rankIndex);
		}
	}

	public void AddPoints(int points, string pointID, GameObject sourceWeapon = null, EnemyIdentifier eid = null, int count = -1, string prefix = "", string postfix = "")
	{
		GameObject gameObject = ((pointID == "ultrakill.arsenal") ? gc.currentWeapon : sourceWeapon);
		if ((bool)eid && eid.puppet)
		{
			return;
		}
		bool flag = false;
		if ((bool)eid)
		{
			flag = eid.isBoss;
		}
		if (points > 0)
		{
			float num = points;
			if (freshnessEnabled && gameObject != null)
			{
				StyleFreshnessState freshnessState = GetFreshnessState(gameObject);
				num *= freshnessStateDict[freshnessState].scoreMultiplier;
				DecayFreshness(gameObject, pointID, flag);
			}
			if (flag)
			{
				num *= bossStyleGainMultiplier;
			}
			sman.stylePoints += Mathf.RoundToInt(num);
			currentMeter += num;
			rankScale = 0.2f;
		}
		string localizedName = GetLocalizedName(pointID);
		if (localizedName != "")
		{
			if (count >= 0)
			{
				hudItemsQueue.Enqueue("+ " + prefix + localizedName + postfix + " x" + count);
			}
			else
			{
				hudItemsQueue.Enqueue("+ " + prefix + localizedName + postfix);
			}
		}
		if (currentMeter >= (float)currentRank.maxMeter && rankIndex < 7)
		{
			AscendRank();
		}
		else if (currentMeter > (float)currentRank.maxMeter)
		{
			currentMeter = currentRank.maxMeter;
		}
	}

	public void RemovePoints(int points)
	{
		rankShaking = 5f;
		currentMeter -= points;
	}

	public void ResetFreshness(GameObject weapon)
	{
		if (weaponFreshness.ContainsKey(weapon))
		{
			weaponFreshness[weapon] = 10f;
		}
	}

	public void ResetAllFreshness()
	{
		foreach (GameObject allWeapon in gc.allWeapons)
		{
			ResetFreshness(allWeapon);
		}
	}

	private void RemoveText()
	{
		styleInfo.text = styleInfo.text.Substring(0, styleInfo.text.LastIndexOf("+"));
	}
}
