using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameConsole;
using plog;
using plog.Models;
using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CheatsManager : MonoSingleton<CheatsManager>
{
	private static readonly Logger Log = new Logger("CheatsManager");

	[SerializeField]
	private GameObject cheatsManagerMenu;

	[Space]
	[SerializeField]
	private GameObject itemContainer;

	[SerializeField]
	private CheatMenuItem template;

	[Space]
	[SerializeField]
	private CheatMenuItem categoryTemplate;

	[Space]
	[SerializeField]
	private Color enabledColor = Color.green;

	[SerializeField]
	private Color disabledColor = Color.red;

	private Dictionary<ICheat, CheatMenuItem> menuItems;

	private Dictionary<string, ICheat> idToCheat;

	private readonly Dictionary<string, List<ICheat>> allRegisteredCheats = new Dictionary<string, List<ICheat>>();

	private Dictionary<string, Sprite> spriteIcons;

	public static bool KeepCheatsEnabled
	{
		get
		{
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("cheat.ultrakill.keep-enabled"))
			{
				if ((bool)MapInfoBase.Instance)
				{
					return MapInfoBase.Instance.sandboxTools;
				}
				return false;
			}
			return true;
		}
	}

	public void ShowMenu()
	{
		cheatsManagerMenu.SetActive(value: true);
		GameStateManager.Instance.RegisterState(new GameState("cheats-menu", cheatsManagerMenu)
		{
			cursorLock = LockMode.Unlock,
			playerInputLock = LockMode.Lock,
			cameraInputLock = LockMode.Lock
		});
	}

	public void HideMenu()
	{
		cheatsManagerMenu.SetActive(value: false);
	}

	public bool IsMenuOpen()
	{
		return cheatsManagerMenu.activeSelf;
	}

	public void RegisterCheat(ICheat cheat, string category = null)
	{
		if (string.IsNullOrEmpty(category))
		{
			category = "misc";
		}
		category = category.ToUpper();
		if (GetStartCheatState(cheat))
		{
			SetCheatActive(cheat, isActive: true, saveState: false);
		}
		if (allRegisteredCheats.TryGetValue(category, out var value))
		{
			value.Add(cheat);
			return;
		}
		allRegisteredCheats.Add(category, new List<ICheat> { cheat });
	}

	public void RegisterCheats(ICheat[] cheats, string category = null)
	{
		if (string.IsNullOrEmpty(category))
		{
			category = "misc";
		}
		category = category.ToUpper();
		foreach (ICheat cheat in cheats)
		{
			if (GetStartCheatState(cheat))
			{
				SetCheatActive(cheat, isActive: true, saveState: false);
			}
		}
		if (allRegisteredCheats.TryGetValue(category, out var value))
		{
			value.AddRange(cheats);
		}
		else
		{
			allRegisteredCheats.Add(category, cheats.ToList());
		}
	}

	public void RegisterExternalCheat(ICheat cheat)
	{
		RegisterCheat(cheat, "external");
	}

	public void ToggleCheat(ICheat targetCheat)
	{
		if (menuItems.ContainsKey(targetCheat))
		{
			MonoSingleton<CheatsController>.Instance.PlayToggleSound(!targetCheat.IsActive);
			SetCheatActive(targetCheat, !targetCheat.IsActive);
			UpdateCheatState(menuItems[targetCheat], targetCheat);
		}
	}

	public void RefreshCheatStates()
	{
		foreach (ICheat key in menuItems.Keys)
		{
			UpdateCheatState(menuItems[key], key);
		}
	}

	public void SetCheatActive(ICheat targetCheat, bool isActive, bool saveState = true)
	{
		if (targetCheat.IsActive)
		{
			if (!isActive)
			{
				targetCheat.Disable();
				if (saveState)
				{
					SaveCheatState(targetCheat);
				}
			}
		}
		else if (isActive)
		{
			targetCheat.Enable(this);
			StartCoroutine(targetCheat.Coroutine(this));
			if (saveState)
			{
				SaveCheatState(targetCheat);
			}
		}
	}

	public bool GetCheatState(string id)
	{
		if (idToCheat == null || !idToCheat.TryGetValue(id, out var value))
		{
			return false;
		}
		if (idToCheat.ContainsKey(id))
		{
			return value.IsActive;
		}
		return false;
	}

	public void DisableCheat(string id)
	{
		if (idToCheat.ContainsKey(id))
		{
			DisableCheat(idToCheat[id]);
		}
	}

	public void DisableCheat(ICheat targetCheat)
	{
		if (menuItems.ContainsKey(targetCheat) && targetCheat.IsActive)
		{
			MonoSingleton<CheatsController>.Instance.PlayToggleSound(newState: false);
			SetCheatActive(targetCheat, isActive: false);
			UpdateCheatState(menuItems[targetCheat], targetCheat);
		}
	}

	public T GetCheatInstance<T>() where T : ICheat
	{
		return allRegisteredCheats.Values.SelectMany((List<ICheat> cheats) => cheats).OfType<T>().FirstOrDefault();
	}

	public void RebuildIcons()
	{
		spriteIcons = new Dictionary<string, Sprite>();
		if (!(MonoSingleton<IconManager>.Instance == null) && !(MonoSingleton<IconManager>.Instance.CurrentIcons == null))
		{
			CheatAssetObject.KeyIcon[] cheatIcons = MonoSingleton<IconManager>.Instance.CurrentIcons.cheatIcons;
			for (int i = 0; i < cheatIcons.Length; i++)
			{
				CheatAssetObject.KeyIcon keyIcon = cheatIcons[i];
				spriteIcons.Add(keyIcon.key, keyIcon.sprite);
			}
		}
	}

	private void Start()
	{
		RebuildIcons();
		RegisterCheat(new KeepEnabled(), "meta");
		if (MapInfoBase.Instance != null && MapInfoBase.Instance.sandboxTools)
		{
			if (Debug.isDebugBuild)
			{
				RegisterCheat(new ExperimentalArmRotation(), "sandbox");
			}
			RegisterCheat(new QuickSave(), "sandbox");
			RegisterCheat(new QuickLoad(), "sandbox");
			RegisterCheat(new ManageSaves(), "sandbox");
			RegisterCheat(new ClearMap(), "sandbox");
			if ((bool)MonoSingleton<SandboxNavmesh>.Instance)
			{
				RegisterCheat(new RebuildNavmesh(), "sandbox");
			}
			RegisterCheats(new ICheat[2]
			{
				new ULTRAKILL.Cheats.Snapping(),
				new SpawnPhysics()
			}, "sandbox");
		}
		RegisterCheats(new ICheat[4]
		{
			new SummonSandboxArm(),
			new TeleportMenu(),
			new FullBright(),
			new Invincibility()
		}, "general");
		RegisterCheats(new ICheat[3]
		{
			new Noclip(),
			new Flight(),
			new InfiniteWallJumps()
		}, "movement");
		RegisterCheats(new ICheat[2]
		{
			new NoWeaponCooldown(),
			new InfinitePowerUps()
		}, "weapons");
		RegisterCheats(new ICheat[6]
		{
			new BlindEnemies(),
			new EnemiesHateEnemies(),
			new EnemyIgnorePlayer(),
			new DisableEnemySpawns(),
			new InvincibleEnemies(),
			new KillAllEnemies()
		}, "enemies");
		RegisterCheats(new ICheat[2]
		{
			new HideWeapons(),
			new HideUI()
		}, "visual");
		bool flag = UnityEngine.Object.FindAnyObjectByType<Portal>(FindObjectsInactive.Include) != null;
		if (GameProgressSaver.GetClashModeUnlocked() && !flag)
		{
			RegisterCheat(new CrashMode(), "special");
		}
		if (GameProgressSaver.GetGhostDroneModeUnlocked())
		{
			RegisterCheat(new GhostDroneMode(), "special");
		}
		if (Debug.isDebugBuild)
		{
			RegisterCheats(new ICheat[11]
			{
				new NonConvexJumpDebug(),
				new HideCheatsStatus(),
				new PlayerParentingDebug(),
				new StateDebug(),
				new GunControlDebug(),
				new SandboxArmDebug(),
				new EnemyIdentifierDebug(),
				new ForceBossBars(),
				new SpreadGasoline(),
				new DeathCatcherDebug(),
				new PreventTimerStart()
			}, "debug");
			RegisterCheat(new PauseTimedBombs(), "debug");
		}
		MonoSingleton<CheatBinds>.Instance.RestoreBinds(allRegisteredCheats);
		RebuildMenu();
	}

	public void CancelRebindIfNecessary()
	{
		if (MonoSingleton<CheatBinds>.Instance.isRebinding)
		{
			MonoSingleton<CheatBinds>.Instance.CancelRebind();
		}
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<string, List<ICheat>> allRegisteredCheat in allRegisteredCheats)
		{
			foreach (ICheat item in allRegisteredCheat.Value)
			{
				if (item.IsActive)
				{
					try
					{
						item.Disable();
					}
					catch (Exception ex)
					{
						Log.Error(ex.ToString(), (IEnumerable<Tag>)null, (string)null, (object)null);
					}
				}
			}
		}
		allRegisteredCheats.Clear();
	}

	public void HandleCheatBind(string identifier)
	{
		if (MonoSingleton<CheatsController>.Instance.cheatsEnabled && !MonoSingleton<CheatBinds>.Instance.isRebinding && !SandboxHud.SavesMenuOpen && !GameConsole.Console.IsOpen && !GameStateManager.Instance.IsStateActive("alter-menu") && !GameStateManager.Instance.IsStateActive("agony-menu"))
		{
			MonoSingleton<CheatsController>.Instance.PlayToggleSound(!idToCheat[identifier].IsActive);
			ToggleCheat(idToCheat[identifier]);
			UpdateCheatState(idToCheat[identifier]);
		}
	}

	private void OnGUI()
	{
		if (MonoSingleton<CheatsController>.Instance == null || !MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			return;
		}
		foreach (KeyValuePair<string, List<ICheat>> allRegisteredCheat in allRegisteredCheats)
		{
			foreach (ICheat item in allRegisteredCheat.Value)
			{
				if (item.IsActive && item is ICheatGUI cheatGUI)
				{
					cheatGUI.OnGUI();
				}
			}
		}
	}

	private bool GetStartCheatState(ICheat cheat)
	{
		if (cheat.Identifier == "ultrakill.spawner-arm" && (bool)MapInfoBase.Instance && MapInfoBase.Instance.sandboxTools)
		{
			return true;
		}
		if (cheat.PersistenceMode == StatePersistenceMode.NotPersistent)
		{
			return cheat.DefaultState;
		}
		if (!KeepCheatsEnabled)
		{
			MonoSingleton<PrefsManager>.Instance.DeleteKey("cheat." + cheat.Identifier);
			return cheat.DefaultState;
		}
		return MonoSingleton<PrefsManager>.Instance.GetBool("cheat." + cheat.Identifier, cheat.DefaultState);
	}

	private void SaveCheatState(ICheat cheat)
	{
		if (cheat.PersistenceMode != StatePersistenceMode.NotPersistent)
		{
			MonoSingleton<PrefsManager>.Instance.SetBool("cheat." + cheat.Identifier, cheat.IsActive);
		}
	}

	public void RenderCheatsInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((bool)MonoSingleton<SandboxNavmesh>.Instance && MonoSingleton<SandboxNavmesh>.Instance.isDirty)
		{
			stringBuilder.AppendLine("<color=red>NAVMESH OUT OF DATE<size=12>\n(Rebuild navigation in cheats menu)</size>\n</color>");
		}
		if (GetCheatState("ultrakill.spawner-arm"))
		{
			stringBuilder.AppendLine("<color=#C2D7FF>Spawner Arm in slot 6\n</color>");
		}
		foreach (KeyValuePair<string, List<ICheat>> allRegisteredCheat in allRegisteredCheats)
		{
			foreach (ICheat item in allRegisteredCheat.Value)
			{
				if (item.IsActive)
				{
					string text = MonoSingleton<CheatBinds>.Instance.ResolveCheatKey(item.Identifier);
					if (!string.IsNullOrEmpty(text))
					{
						stringBuilder.Append("[<color=orange>" + text.ToUpper() + "</color>] ");
					}
					else
					{
						stringBuilder.Append("[ ] ");
					}
					stringBuilder.Append("<color=white>" + item.LongName + "</color>\n");
				}
			}
		}
		MonoSingleton<CheatsController>.Instance.cheatsInfo.text = stringBuilder.ToString();
	}

	public void UpdateCheatState(ICheat cheat)
	{
		if (menuItems.ContainsKey(cheat))
		{
			UpdateCheatState(menuItems[cheat], cheat);
		}
	}

	private void UpdateCheatState(CheatMenuItem item, ICheat cheat)
	{
		item.longName.text = cheat.LongName;
		((Graphic)item.stateBackground).color = (cheat.IsActive ? enabledColor : disabledColor);
		item.stateText.text = (cheat.IsActive ? (cheat.ButtonEnabledOverride ?? "ENABLED") : (cheat.ButtonDisabledOverride ?? "DISABLED"));
		((Component)(object)item.bindButtonBack).gameObject.SetActive(value: false);
		string text = MonoSingleton<CheatBinds>.Instance.ResolveCheatKey(cheat.Identifier);
		if (string.IsNullOrEmpty(text))
		{
			item.bindButtonText.text = "Press to Bind";
		}
		else
		{
			item.bindButtonText.text = text.ToUpper();
		}
		RenderCheatsInfo();
	}

	private void ResetMenu()
	{
		menuItems = new Dictionary<ICheat, CheatMenuItem>();
		idToCheat = new Dictionary<string, ICheat>();
		for (int i = 2; i < itemContainer.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(itemContainer.transform.GetChild(i).gameObject);
		}
	}

	private void StartRebind(ICheat cheat)
	{
		if (MonoSingleton<CheatBinds>.Instance.isRebinding)
		{
			MonoSingleton<CheatBinds>.Instance.CancelRebind();
			return;
		}
		((Component)(object)menuItems[cheat].bindButtonBack).gameObject.SetActive(value: true);
		menuItems[cheat].bindButtonText.text = "Press any key";
		MonoSingleton<CheatBinds>.Instance.SetupRebind(cheat);
	}

	public void RebuildMenu()
	{
		ResetMenu();
		template.gameObject.SetActive(value: false);
		categoryTemplate.gameObject.SetActive(value: false);
		foreach (KeyValuePair<string, List<ICheat>> allRegisteredCheat in allRegisteredCheats)
		{
			CheatMenuItem cheatMenuItem = UnityEngine.Object.Instantiate(categoryTemplate, itemContainer.transform, worldPositionStays: false);
			cheatMenuItem.gameObject.SetActive(value: true);
			cheatMenuItem.longName.text = allRegisteredCheat.Key;
			foreach (ICheat cheat in allRegisteredCheat.Value)
			{
				CheatMenuItem item = UnityEngine.Object.Instantiate(template, itemContainer.transform, worldPositionStays: false);
				item.gameObject.SetActive(value: true);
				if (!string.IsNullOrEmpty(cheat.Icon))
				{
					if (spriteIcons.TryGetValue(cheat.Icon, out var value))
					{
						item.iconTarget.sprite = value;
					}
					else if ((bool)MonoSingleton<IconManager>.Instance && (bool)MonoSingleton<IconManager>.Instance.CurrentIcons)
					{
						item.iconTarget.sprite = MonoSingleton<IconManager>.Instance.CurrentIcons.genericCheatIcon;
					}
				}
				((UnityEvent)(object)item.stateButton.onClick).AddListener((UnityAction)delegate
				{
					ToggleCheat(cheat);
				});
				((UnityEvent)(object)item.bindButton.onClick).AddListener((UnityAction)delegate
				{
					StartRebind(cheat);
				});
				((UnityEvent)(object)item.resetBindButton.onClick).AddListener((UnityAction)delegate
				{
					MonoSingleton<CheatBinds>.Instance.ResetCheatBind(cheat.Identifier);
					UpdateCheatState(item, cheat);
				});
				UpdateCheatState(item, cheat);
				menuItems[cheat] = item;
				idToCheat[cheat.Identifier] = cheat;
			}
		}
	}
}
