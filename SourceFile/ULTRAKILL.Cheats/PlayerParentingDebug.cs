using System;
using System.Collections;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class PlayerParentingDebug : ICheat, ICheatGUI
{
	private static PlayerParentingDebug _lastInstance;

	private bool active;

	private PlayerMovementParenting[] pmp;

	public static bool Active
	{
		get
		{
			if (_lastInstance != null)
			{
				return _lastInstance.active;
			}
			return false;
		}
	}

	public string LongName => "Player Parenting Debug";

	public string Identifier => "ultrakill.debug.player-parent-debug";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => null;

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		active = true;
		_lastInstance = this;
	}

	public void Disable()
	{
		active = false;
	}

	public IEnumerator Coroutine(CheatsManager manager)
	{
		while (active)
		{
			Update();
			yield return null;
		}
	}

	public void Update()
	{
		pmp = UnityEngine.Object.FindObjectsOfType<PlayerMovementParenting>();
		if (pmp != null)
		{
			_ = pmp.LongLength;
		}
	}

	public void OnGUI()
	{
		GUILayout.Label("Player Parenting Debug", Array.Empty<GUILayoutOption>());
		if (pmp == null)
		{
			return;
		}
		PlayerMovementParenting[] array = pmp;
		foreach (PlayerMovementParenting playerMovementParenting in array)
		{
			if (playerMovementParenting == null)
			{
				continue;
			}
			GUILayout.Label(playerMovementParenting.gameObject.name, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Attached to:", Array.Empty<GUILayoutOption>());
			foreach (Transform trackedObject in playerMovementParenting.TrackedObjects)
			{
				if (trackedObject == null)
				{
					GUILayout.Label("null", Array.Empty<GUILayoutOption>());
				}
				else
				{
					GUILayout.Label("- " + trackedObject.name, Array.Empty<GUILayoutOption>());
				}
			}
			GUILayout.Label("------------------------------", Array.Empty<GUILayoutOption>());
		}
	}
}
