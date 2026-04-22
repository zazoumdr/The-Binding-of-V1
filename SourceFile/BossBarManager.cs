using System.Collections.Generic;
using plog;
using plog.Models;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class BossBarManager : MonoSingleton<BossBarManager>
{
	private static readonly Logger Log = new Logger("BossBarManager");

	[SerializeField]
	private float overflowShrinkFactor = 0.14f;

	[SerializeField]
	private float minimumSize = 0.3f;

	[SerializeField]
	private float baseOverflowedSize = 0.82f;

	[Space]
	[SerializeField]
	private RectTransform containerRect;

	[SerializeField]
	private BossHealthBarTemplate template;

	[SerializeField]
	private SliderLayer[] layers;

	private readonly Dictionary<int, BossHealthBarTemplate> bossBars = new Dictionary<int, BossHealthBarTemplate>();

	private readonly Dictionary<int, TimeSince> bossBarLastUpdated = new Dictionary<int, TimeSince>();

	private readonly Queue<int> bossBarsToRemove = new Queue<int>();

	private bool bossBarsVisible = true;

	private const float BossBarTimeToExpire = 3f;

	public void UpdateBossBar(BossHealthBar bossBar)
	{
		if (bossBar.source == null || !bossBar.source.Dead)
		{
			int bossBarId = bossBar.bossBarId;
			IEnemyHealthDetails source = bossBar.source;
			bool flag = false;
			if (!bossBars.ContainsKey(bossBarId))
			{
				flag = true;
				CreateBossBar(bossBar);
			}
			BossHealthBarTemplate bossHealthBarTemplate = bossBars[bossBarId];
			bossHealthBarTemplate.UpdateState(source);
			if (!flag && bossHealthBarTemplate.visibilityDeferred)
			{
				bossHealthBarTemplate.visibilityDeferred = false;
				bossHealthBarTemplate.SetVisible(isVisible: true);
				RecalculateStretch();
			}
			if (bossBar.secondaryBar)
			{
				bossHealthBarTemplate.UpdateSecondaryBar(bossBar);
			}
			else
			{
				bossHealthBarTemplate.ResetSecondaryBar();
			}
			if (bossHealthBarTemplate.bossNameText.text != bossBar.bossName.ToUpper())
			{
				bossHealthBarTemplate.ChangeName(bossBar.bossName.ToUpper());
			}
			if (!source.Dead)
			{
				bossBarLastUpdated[bossBarId] = 0f;
			}
		}
	}

	public void ExpireImmediately(int bossBarId)
	{
		if (bossBars.TryGetValue(bossBarId, out var value))
		{
			Log.Info($"Immediately removing boss bar {value.bossNameText.text} ({bossBarId})", (IEnumerable<Tag>)null, (string)null, (object)null);
			bossBarLastUpdated[bossBarId] = 3f;
		}
	}

	private void CreateBossBar(BossHealthBar bossBar)
	{
		Log.Info($"Creating Boss Bar for {bossBar.bossName} ({bossBar.bossBarId})", (IEnumerable<Tag>)null, (string)null, (object)null);
		BossHealthBarTemplate bossHealthBarTemplate = Object.Instantiate(template, containerRect);
		bossHealthBarTemplate.Initialize(bossBar, layers);
		bossHealthBarTemplate.UpdateState(bossBar.source);
		bossHealthBarTemplate.SetVisible(isVisible: false);
		bossBars.Add(bossBar.bossBarId, bossHealthBarTemplate);
	}

	private void Update()
	{
		while (bossBarsToRemove.Count > 0)
		{
			int num = bossBarsToRemove.Dequeue();
			Log.Info($"Removing Expired Boss Bar for {bossBars[num].bossNameText.text} ({num})", (IEnumerable<Tag>)null, (string)null, (object)null);
			Object.Destroy(bossBars[num].gameObject);
			bossBars.Remove(num);
			bossBarLastUpdated.Remove(num);
			RecalculateStretch();
		}
		foreach (KeyValuePair<int, BossHealthBarTemplate> bossBar in bossBars)
		{
			int key = bossBar.Key;
			if ((float)bossBarLastUpdated[key] > 3f && !bossBarsToRemove.Contains(key))
			{
				bossBarsToRemove.Enqueue(key);
			}
		}
		if (bossBarsVisible == HideUI.Active)
		{
			bossBarsVisible = !HideUI.Active;
			RefreshVisibility();
		}
	}

	private void RecalculateStretch()
	{
		float b = 1f;
		if (bossBars.Count > 2)
		{
			b = baseOverflowedSize - (float)(bossBars.Count - 2) * overflowShrinkFactor;
		}
		b = Mathf.Max(minimumSize, b);
		containerRect.localScale = new Vector3(1f, b, 1f);
		foreach (BossHealthBarTemplate value in bossBars.Values)
		{
			value.ScaleChanged(b);
		}
	}

	private void RefreshVisibility()
	{
		foreach (BossHealthBarTemplate value in bossBars.Values)
		{
			if (!(value == null))
			{
				value.SetVisible(bossBarsVisible);
			}
		}
		if (bossBarsVisible)
		{
			RecalculateStretch();
		}
	}

	public void ForceLayoutRebuild()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
	}
}
