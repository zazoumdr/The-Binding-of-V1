using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
	public bool changePerDifficulty;

	public float countdownLength;

	public float[] countdownLengthPerDifficulty = new float[6];

	private float time;

	public TextMeshProUGUI countdownText;

	public float decimalFontSize;

	public BossHealthBar bossbar;

	public bool invertBossBarAmount;

	public bool disableBossBarOnDisable;

	public bool paused;

	public bool resetOnEnable;

	public UltrakillEvent onZero;

	private bool done;

	private int difficulty;

	private void Awake()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void Start()
	{
		if (time == 0f && !done)
		{
			time = GetCountdownLength();
		}
	}

	private void OnEnable()
	{
		ResetTime();
	}

	private void OnDisable()
	{
		if ((bool)bossbar && disableBossBarOnDisable)
		{
			bossbar.secondaryBarValue = 0f;
			bossbar.secondaryBar = false;
		}
	}

	private void Update()
	{
		if (!paused)
		{
			time = Mathf.MoveTowards(time, 0f, Time.deltaTime);
		}
		if (!done && time <= 0f)
		{
			onZero?.Invoke();
			done = true;
		}
		if ((bool)(Object)(object)countdownText)
		{
			if (decimalFontSize == 0f)
			{
				((TMP_Text)countdownText).text = time.ToString("F2");
			}
			else
			{
				int num = Mathf.FloorToInt(time % 1f * 100f);
				((TMP_Text)countdownText).text = Mathf.FloorToInt(time) + "<size=" + decimalFontSize + ((num < 10) ? ">.0" : ">.") + num;
			}
		}
		if ((bool)bossbar)
		{
			bossbar.secondaryBar = true;
			bossbar.secondaryBarValue = (invertBossBarAmount ? ((countdownLength - time) / countdownLength) : (time / countdownLength));
		}
	}

	public void PauseState(bool pause)
	{
		paused = pause;
	}

	public void ChangeTime(float newTime)
	{
		time = newTime;
	}

	public void ResetTime()
	{
		time = GetCountdownLength();
		done = false;
	}

	private float GetCountdownLength()
	{
		if (!changePerDifficulty)
		{
			return countdownLength;
		}
		return countdownLengthPerDifficulty[difficulty];
	}
}
