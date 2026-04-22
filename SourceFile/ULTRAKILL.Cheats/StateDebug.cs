using System;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class StateDebug : ICheat, ICheatGUI
{
	private bool active;

	public string LongName => "Game State Debug";

	public string Identifier => "ultrakill.debug.game-state";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		active = true;
	}

	public void Disable()
	{
		active = false;
	}

	public void OnGUI()
	{
		GUILayout.Label("Game State:", Array.Empty<GUILayoutOption>());
		GUILayout.Label("opman paused: " + MonoSingleton<OptionsManager>.Instance.paused, Array.Empty<GUILayoutOption>());
		GUILayout.Label("opman frozen: " + MonoSingleton<OptionsManager>.Instance.frozen, Array.Empty<GUILayoutOption>());
		GUILayout.Label("fc shopping: " + MonoSingleton<FistControl>.Instance.shopping, Array.Empty<GUILayoutOption>());
		GUILayout.Label("gc activated: " + MonoSingleton<GunControl>.Instance.activated, Array.Empty<GUILayoutOption>());
		if ((bool)MonoSingleton<WeaponCharges>.Instance)
		{
			GUILayout.Label("rc: " + MonoSingleton<WeaponCharges>.Instance.rocketCount, Array.Empty<GUILayoutOption>());
			if (MonoSingleton<WeaponCharges>.Instance.rocketCount != 0 && MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
			{
				GUILayout.Label("ts: " + MonoSingleton<WeaponCharges>.Instance.timeSinceIdleFrozen.ToString(), Array.Empty<GUILayoutOption>());
			}
		}
	}
}
