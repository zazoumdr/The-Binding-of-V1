using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;
using UnityEngine.Serialization;

public class BossHealthBar : MonoBehaviour
{
	private static readonly Logger Log = new Logger("BossHealthBar");

	[HideInInspector]
	public int bossBarId;

	[HideInInspector]
	public IEnemyHealthDetails source;

	public HealthLayer[] healthLayers;

	public string bossName;

	public bool secondaryBar;

	[FormerlySerializedAs("secondaryColor")]
	[SerializeField]
	public Color secondaryBarColor = Color.white;

	public float secondaryBarValue;

	private void Awake()
	{
		source = GetComponent<IEnemyHealthDetails>();
		source.ForceGetHealth();
		if (healthLayers == null)
		{
			healthLayers = Array.Empty<HealthLayer>();
		}
		if (healthLayers.Length == 0)
		{
			healthLayers = new HealthLayer[1];
			healthLayers[0] = new HealthLayer
			{
				health = source.Health
			};
		}
		if (string.IsNullOrEmpty(bossName))
		{
			bossName = source.FullName;
		}
		if (bossBarId == 0)
		{
			bossBarId = GetInstanceID();
		}
		else
		{
			Log.Info($"Taking over boss bar {bossBarId}", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
	}

	private void UpdateBossBarTick()
	{
		if (MonoSingleton<BossBarManager>.TryGetInstance(out BossBarManager instance))
		{
			instance.UpdateBossBar(this);
		}
		SendMessage("HasBossBar", this, SendMessageOptions.DontRequireReceiver);
	}

	private void Start()
	{
		UpdateBossBarTick();
	}

	private void OnEnable()
	{
		UpdateBossBarTick();
		if (!source.Dead)
		{
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if (instance == null)
			{
				instance = MonoSingleton<MusicManager>.Instance;
			}
			if (instance.useBossTheme)
			{
				instance.PlayBossMusic();
			}
		}
	}

	public void UpdateSecondaryBar(float value)
	{
		secondaryBarValue = value;
	}

	public void SetSecondaryBarColor(Color clr)
	{
		secondaryBarColor = clr;
	}

	private void Update()
	{
		MonoSingleton<BossBarManager>.Instance.UpdateBossBar(this);
	}

	private void OnDisable()
	{
		DisappearBar();
	}

	public void DisappearBar()
	{
		if ((bool)MonoSingleton<BossBarManager>.Instance)
		{
			MonoSingleton<BossBarManager>.Instance.ExpireImmediately(bossBarId);
		}
	}

	public void ChangeName(string newName)
	{
		bossName = newName;
		MonoSingleton<BossBarManager>.Instance.UpdateBossBar(this);
	}
}
