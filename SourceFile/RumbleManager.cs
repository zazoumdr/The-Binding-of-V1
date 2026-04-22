using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class RumbleManager : MonoSingleton<RumbleManager>
{
	public readonly Dictionary<RumbleKey, PendingVibration> pendingVibrations = new Dictionary<RumbleKey, PendingVibration>();

	private List<RumbleKey> discardedKeys = new List<RumbleKey>();

	private static readonly Dictionary<RumbleKey, float> rumbleDurations = new Dictionary<RumbleKey, float>
	{
		{
			RumbleProperties.Slide,
			float.PositiveInfinity
		},
		{
			RumbleProperties.WhiplashThrow,
			float.PositiveInfinity
		},
		{
			RumbleProperties.WhiplashPull,
			float.PositiveInfinity
		},
		{
			RumbleProperties.RailcannonIdle,
			float.PositiveInfinity
		},
		{
			RumbleProperties.ShotgunCharge,
			float.PositiveInfinity
		},
		{
			RumbleProperties.NailgunFire,
			float.PositiveInfinity
		},
		{
			RumbleProperties.RevolverCharge,
			float.PositiveInfinity
		},
		{
			RumbleProperties.FallImpact,
			0.5f
		},
		{
			RumbleProperties.FallImpactHeavy,
			0.5f
		},
		{
			RumbleProperties.Jump,
			0.2f
		},
		{
			RumbleProperties.Dash,
			0.2f
		},
		{
			RumbleProperties.Punch,
			0.2f
		},
		{
			RumbleProperties.Sawblade,
			0.2f
		},
		{
			RumbleProperties.GunFire,
			0.4f
		},
		{
			RumbleProperties.SuperSaw,
			0.7f
		},
		{
			RumbleProperties.GunFireStrong,
			0.7f
		},
		{
			RumbleProperties.GunFireProjectiles,
			0.8f
		},
		{
			RumbleProperties.ParryFlash,
			0.1f
		},
		{
			RumbleProperties.CoinToss,
			0.1f
		},
		{
			RumbleProperties.Magnet,
			0.1f
		},
		{
			RumbleProperties.WeaponWheelTick,
			0.025f
		}
	};

	public static readonly Dictionary<RumbleKey, float> rumbleIntensities = new Dictionary<RumbleKey, float>
	{
		{
			RumbleProperties.Slide,
			0.1f
		},
		{
			RumbleProperties.Dash,
			0.2f
		},
		{
			RumbleProperties.FallImpact,
			0.2f
		},
		{
			RumbleProperties.Jump,
			0.1f
		},
		{
			RumbleProperties.FallImpactHeavy,
			0.5f
		},
		{
			RumbleProperties.WhiplashThrow,
			0.2f
		},
		{
			RumbleProperties.WhiplashPull,
			0.35f
		},
		{
			RumbleProperties.GunFire,
			0.8f
		},
		{
			RumbleProperties.GunFireStrong,
			1f
		},
		{
			RumbleProperties.GunFireProjectiles,
			0.7f
		},
		{
			RumbleProperties.RailcannonIdle,
			0.2f
		},
		{
			RumbleProperties.NailgunFire,
			0.2f
		},
		{
			RumbleProperties.SuperSaw,
			0.7f
		},
		{
			RumbleProperties.ShotgunCharge,
			0.7f
		},
		{
			RumbleProperties.Sawblade,
			0.5f
		},
		{
			RumbleProperties.RevolverCharge,
			0.5f
		},
		{
			RumbleProperties.Punch,
			0.2f
		},
		{
			RumbleProperties.ParryFlash,
			0.2f
		},
		{
			RumbleProperties.Magnet,
			0.2f
		},
		{
			RumbleProperties.CoinToss,
			0.1f
		},
		{
			RumbleProperties.WeaponWheelTick,
			0.05f
		}
	};

	public static readonly Dictionary<RumbleKey, string> fullNames = new Dictionary<RumbleKey, string>
	{
		{
			RumbleProperties.Slide,
			"Sliding"
		},
		{
			RumbleProperties.Dash,
			"Dashing"
		},
		{
			RumbleProperties.FallImpact,
			"Fall Impact"
		},
		{
			RumbleProperties.Jump,
			"Jumping"
		},
		{
			RumbleProperties.FallImpactHeavy,
			"Heavy Fall Impact"
		},
		{
			RumbleProperties.WhiplashThrow,
			"Whiplash Throw"
		},
		{
			RumbleProperties.WhiplashPull,
			"Whiplash Pull"
		},
		{
			RumbleProperties.GunFire,
			"Gun Fire"
		},
		{
			RumbleProperties.GunFireStrong,
			"Stronger Gun Fire"
		},
		{
			RumbleProperties.GunFireProjectiles,
			"Gun Fire (projectiles)"
		},
		{
			RumbleProperties.RailcannonIdle,
			"Railcannon Idle"
		},
		{
			RumbleProperties.NailgunFire,
			"Nailgun Fire"
		},
		{
			RumbleProperties.Sawblade,
			"Sawblade"
		},
		{
			RumbleProperties.SuperSaw,
			"Super Saw"
		},
		{
			RumbleProperties.Magnet,
			"Magnet"
		},
		{
			RumbleProperties.ShotgunCharge,
			"Shotgun Charge"
		},
		{
			RumbleProperties.RevolverCharge,
			"Revolver Charge"
		},
		{
			RumbleProperties.ParryFlash,
			"Parry Flash"
		},
		{
			RumbleProperties.CoinToss,
			"Coin Toss"
		},
		{
			RumbleProperties.Punch,
			"Punching"
		},
		{
			RumbleProperties.WeaponWheelTick,
			"Weapon Wheel Tick"
		}
	};

	public float currentIntensity { get; private set; }

	public PendingVibration SetVibration(RumbleKey key)
	{
		if (pendingVibrations.TryGetValue(key, out var value))
		{
			value.timeSinceStart = 0f;
			if (value.isTracking)
			{
				value.isTracking = false;
			}
			return value;
		}
		PendingVibration pendingVibration = new PendingVibration
		{
			key = key,
			timeSinceStart = 0f,
			isTracking = false
		};
		pendingVibrations.Add(key, pendingVibration);
		return pendingVibration;
	}

	public PendingVibration SetVibrationTracked(RumbleKey key, GameObject tracked)
	{
		if (pendingVibrations.TryGetValue(key, out var value))
		{
			value.timeSinceStart = 0f;
			value.isTracking = true;
			value.trackedObject = tracked;
			return value;
		}
		PendingVibration pendingVibration = new PendingVibration
		{
			key = key,
			timeSinceStart = 0f,
			isTracking = true,
			trackedObject = tracked
		};
		pendingVibrations.Add(key, pendingVibration);
		return pendingVibration;
	}

	public void StopVibration(RumbleKey key)
	{
		if (pendingVibrations.ContainsKey(key))
		{
			pendingVibrations.Remove(key);
		}
	}

	public void StopAllVibrations()
	{
		pendingVibrations.Clear();
	}

	private void Update()
	{
		discardedKeys.Clear();
		foreach (KeyValuePair<RumbleKey, PendingVibration> pendingVibration in pendingVibrations)
		{
			if (pendingVibration.Value.isTracking && (pendingVibration.Value.trackedObject == null || !pendingVibration.Value.trackedObject.activeInHierarchy))
			{
				discardedKeys.Add(pendingVibration.Key);
			}
			else if (pendingVibration.Value.IsFinished)
			{
				discardedKeys.Add(pendingVibration.Key);
			}
		}
		foreach (RumbleKey discardedKey in discardedKeys)
		{
			pendingVibrations.Remove(discardedKey);
		}
		float num = 0f;
		foreach (KeyValuePair<RumbleKey, PendingVibration> pendingVibration2 in pendingVibrations)
		{
			if (pendingVibration2.Value.Intensity > num)
			{
				num = pendingVibration2.Value.Intensity;
			}
		}
		num *= MonoSingleton<PrefsManager>.Instance.GetFloat("totalRumbleIntensity");
		if ((bool)MonoSingleton<OptionsManager>.Instance && MonoSingleton<OptionsManager>.Instance.paused)
		{
			num = 0f;
		}
		currentIntensity = num;
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(num, num);
		}
	}

	private void OnDisable()
	{
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}

	public float ResolveDuration(RumbleKey key)
	{
		string key2 = key.name + ".duration";
		if (MonoSingleton<PrefsManager>.Instance.HasKey(key2))
		{
			return MonoSingleton<PrefsManager>.Instance.GetFloat(key2);
		}
		return ResolveDefaultDuration(key);
	}

	public float ResolveDefaultDuration(RumbleKey key)
	{
		if (rumbleDurations.TryGetValue(key, out var value))
		{
			return value;
		}
		Debug.LogError("No duration found for key: " + key);
		return 0.5f;
	}

	public float ResolveIntensity(RumbleKey key)
	{
		if (MonoSingleton<PrefsManager>.Instance.HasKey(key.name + ".intensity"))
		{
			return MonoSingleton<PrefsManager>.Instance.GetFloat(key.name + ".intensity");
		}
		return ResolveDefaultIntensity(key);
	}

	public float ResolveDefaultIntensity(RumbleKey key)
	{
		if (rumbleIntensities.TryGetValue(key, out var value))
		{
			return value;
		}
		Debug.LogError("No intensity found for key: " + key);
		return 0.5f;
	}

	public static string ResolveFullName(RumbleKey key)
	{
		if (fullNames.ContainsKey(key))
		{
			return fullNames[key];
		}
		return key.ToString();
	}
}
