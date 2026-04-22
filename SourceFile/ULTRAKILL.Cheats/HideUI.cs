using plog;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class HideUI : ICheat
{
	private static readonly Logger Log = new Logger("HideUI");

	private HudController[] hudControllers;

	public static bool Active
	{
		get
		{
			if (Instance != null)
			{
				return Instance.IsActive;
			}
			return false;
		}
	}

	private static HideUI Instance { get; set; }

	public string LongName => "Hide UI";

	public string Identifier => "ultrakill.hide-ui";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon { get; }

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable(CheatsManager manager)
	{
		Instance = this;
		IsActive = true;
		hudControllers = Object.FindObjectsOfType<HudController>();
		HudController[] array = hudControllers;
		foreach (HudController hudController in array)
		{
			if (hudController != null)
			{
				hudController.CheckSituation();
				hudController.SetStyleVisibleTemp();
			}
		}
		if ((bool)MonoSingleton<CanvasController>.Instance && (bool)MonoSingleton<CanvasController>.Instance.crosshair)
		{
			MonoSingleton<CanvasController>.Instance.crosshair.CheckCrossHair();
		}
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.UpdateMeter();
		}
	}

	public void Disable()
	{
		IsActive = false;
		if (hudControllers == null)
		{
			return;
		}
		HudController[] array = hudControllers;
		foreach (HudController hudController in array)
		{
			if (hudController != null)
			{
				hudController.CheckSituation();
				hudController.SetStyleVisibleTemp();
			}
		}
		if ((bool)MonoSingleton<CanvasController>.Instance && (bool)MonoSingleton<CanvasController>.Instance.crosshair)
		{
			MonoSingleton<CanvasController>.Instance.crosshair.CheckCrossHair();
		}
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.UpdateMeter();
		}
	}

	public void Update()
	{
	}
}
