using System.Collections.Generic;
using Sandbox.Arm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ULTRAKILL.Cheats;

public class SummonSandboxArm : ICheat
{
	private bool active;

	private readonly List<SandboxArm> createdArms = new List<SandboxArm>();

	private readonly Dictionary<SpawnableType, SandboxArm> spawnedArmMap = new Dictionary<SpawnableType, SandboxArm>();

	private readonly SpawnableType[][] mainArmTypes = new SpawnableType[4][]
	{
		new SpawnableType[1] { SpawnableType.MoveHand },
		new SpawnableType[1] { SpawnableType.AlterHand },
		new SpawnableType[1] { SpawnableType.DestroyHand },
		new SpawnableType[3]
		{
			SpawnableType.Prop,
			SpawnableType.SimpleSpawn,
			SpawnableType.BuildHand
		}
	};

	public static List<GameObject> armSlot => MonoSingleton<GunControl>.Instance.slot6;

	public string LongName => "Spawner Arm";

	public string Identifier => "ultrakill.spawner-arm";

	public string ButtonEnabledOverride => "REMOVE";

	public string ButtonDisabledOverride => "EQUIP";

	public string Icon => "spawner-arm";

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public static bool AnyArmActive
	{
		get
		{
			if ((bool)MonoSingleton<GunControl>.Instance && (bool)MonoSingleton<GunControl>.Instance.currentWeapon)
			{
				return MonoSingleton<GunControl>.Instance.currentWeapon.GetComponent<SandboxArm>();
			}
			return false;
		}
	}

	public void Enable(CheatsManager manager)
	{
		if (MonoSingleton<CheatsManager>.Instance.GetCheatState("ultrakill.clash-mode"))
		{
			return;
		}
		if (!active && createdArms.Count > 0)
		{
			DeleteAllArms();
		}
		active = true;
		SpawnMenu componentInChildren = MonoSingleton<CanvasController>.Instance.GetComponentInChildren<SpawnMenu>(includeInactive: true);
		SpawnableType[][] array = mainArmTypes;
		foreach (SpawnableType[] array2 in array)
		{
			SandboxArm sandboxArm = CreateArm(array2[0]);
			createdArms.Add(sandboxArm);
			sandboxArm.menu = componentInChildren;
			SpawnableType[] array3 = array2;
			foreach (SpawnableType key in array3)
			{
				spawnedArmMap[key] = sandboxArm;
			}
		}
		if ((bool)componentInChildren)
		{
			componentInChildren.armManager = this;
		}
		TryCreateArmType(SpawnableType.MoveHand);
	}

	public void TryCreateArmType(SpawnableType type)
	{
		SandboxArm sandboxArm;
		if (spawnedArmMap.TryGetValue(type, out var value))
		{
			sandboxArm = value;
		}
		else
		{
			sandboxArm = CreateArm(type);
			createdArms.Add(sandboxArm);
			spawnedArmMap[type] = sandboxArm;
		}
		if ((bool)MonoSingleton<GunControl>.Instance)
		{
			MonoSingleton<GunControl>.Instance.ForceWeapon(sandboxArm.gameObject, MonoSingleton<GunControl>.Instance.activated);
		}
	}

	private SandboxArm CreateArm(SpawnableType type)
	{
		GameObject gameObject = Object.Instantiate(MonoSingleton<CheatsController>.Instance.spawnerArm, MonoSingleton<GunControl>.Instance.transform);
		gameObject.name = gameObject.name + " - " + type;
		armSlot.Add(gameObject);
		MonoSingleton<GunControl>.Instance.UpdateWeaponList();
		SandboxArm component = gameObject.GetComponent<SandboxArm>();
		component.cameraCtrl = MonoSingleton<CameraController>.Instance;
		component.onEnableType = type;
		component.SetArmMode(type);
		return component;
	}

	public void SelectArm(SpawnableObject obj)
	{
		SandboxArm sandboxArm = null;
		if (spawnedArmMap.TryGetValue(obj.spawnableType, out var value))
		{
			sandboxArm = value;
			if ((bool)sandboxArm)
			{
				MonoSingleton<GunControl>.Instance.ForceWeapon(sandboxArm.gameObject);
			}
		}
		if (sandboxArm == null)
		{
			sandboxArm = MonoSingleton<GunControl>.Instance.currentWeapon.GetComponent<SandboxArm>();
		}
		if ((bool)sandboxArm)
		{
			sandboxArm.SelectObject(obj);
		}
	}

	public void Disable()
	{
		active = false;
		if (SceneManager.GetActiveScene().isLoaded)
		{
			DeleteAllArms();
		}
	}

	private void DeleteAllArms()
	{
		foreach (SandboxArm createdArm in createdArms)
		{
			if ((bool)MonoSingleton<GunControl>.Instance && armSlot.Contains(createdArm.gameObject))
			{
				armSlot.Remove(createdArm.gameObject);
			}
			Object.Destroy(createdArm.gameObject);
		}
		if ((bool)MonoSingleton<GunControl>.Instance)
		{
			MonoSingleton<GunControl>.Instance.UpdateWeaponList();
		}
		createdArms.Clear();
		spawnedArmMap.Clear();
	}
}
