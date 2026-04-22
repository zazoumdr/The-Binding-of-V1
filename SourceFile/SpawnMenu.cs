using System;
using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Cheats.UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class SpawnMenu : MonoSingleton<SpawnMenu>
{
	[SerializeField]
	private SpawnMenuSectionReference sectionReference;

	[SerializeField]
	private SpawnableObjectsDatabase objects;

	[HideInInspector]
	public SummonSandboxArm armManager;

	[SerializeField]
	private Sprite lockedIcon;

	private Dictionary<string, Sprite> spriteIcons;

	private void Awake()
	{
		RebuildIcons();
		RebuildMenu();
		if (MonoSingleton<UnlockablesData>.Instance != null)
		{
			UnlockablesData? unlockablesData = MonoSingleton<UnlockablesData>.Instance;
			unlockablesData.unlockableFound = (UnityAction)Delegate.Combine(unlockablesData.unlockableFound, new UnityAction(RebuildMenu));
		}
	}

	private void OnDestroy()
	{
		if (MonoSingleton<UnlockablesData>.Instance != null)
		{
			UnlockablesData? unlockablesData = MonoSingleton<UnlockablesData>.Instance;
			unlockablesData.unlockableFound = (UnityAction)Delegate.Remove(unlockablesData.unlockableFound, new UnityAction(RebuildMenu));
		}
	}

	public void RebuildIcons()
	{
		spriteIcons = new Dictionary<string, Sprite>();
		CheatAssetObject.KeyIcon[] sandboxMenuIcons = MonoSingleton<IconManager>.Instance.CurrentIcons.sandboxMenuIcons;
		for (int i = 0; i < sandboxMenuIcons.Length; i++)
		{
			CheatAssetObject.KeyIcon keyIcon = sandboxMenuIcons[i];
			spriteIcons.Add(keyIcon.key, keyIcon.sprite);
		}
	}

	public void ResetMenu()
	{
		if (!(sectionReference == null) && !(sectionReference.transform.parent == null))
		{
			for (int i = 1; i < sectionReference.transform.parent.childCount; i++)
			{
				UnityEngine.Object.Destroy(sectionReference.transform.parent.GetChild(i).gameObject);
			}
		}
	}

	public void RebuildMenu()
	{
		ResetMenu();
		CreateButtons(objects.sandboxTools, "SANDBOX TOOLS :^)");
		if (MapInfoBase.Instance.sandboxTools)
		{
			CreateButtons(objects.sandboxObjects, "SANDBOX");
			CreateButtons(objects.specialSandbox, "SPECIAL");
		}
		CreateButtons(objects.enemies, "ENEMIES");
		CreateButtons(objects.objects, "ITEMS");
		CreateButtons(objects.unlockables, "UNLOCKABLES");
		sectionReference.gameObject.SetActive(value: false);
	}

	private Sprite ResolveSprite(SpawnableObject target)
	{
		if (!string.IsNullOrEmpty(target.iconKey) && spriteIcons.ContainsKey(target.iconKey))
		{
			return spriteIcons[target.iconKey];
		}
		if (target.gridIcon != null)
		{
			return target.gridIcon;
		}
		return MonoSingleton<IconManager>.Instance.CurrentIcons.genericSandboxToolIcon;
	}

	private void CreateButtons(SpawnableObject[] list, string sectionName)
	{
		SpawnMenuSectionReference spawnMenuSectionReference = UnityEngine.Object.Instantiate(sectionReference, sectionReference.transform.parent);
		spawnMenuSectionReference.gameObject.SetActive(value: true);
		spawnMenuSectionReference.sectionName.text = sectionName;
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] == null || (list[i].sandboxOnly && !MapInfoBase.Instance.sandboxTools))
			{
				continue;
			}
			bool flag = list != objects.enemies || MonoSingleton<BestiaryData>.Instance.GetEnemy(list[i].enemyType) >= 1;
			if (list[i].spawnableObjectType == SpawnableObject.SpawnableObjectDataType.Unlockable)
			{
				UnlockableType unlockableType = list[i].unlockableType;
				if (!MonoSingleton<UnlockablesData>.Instance.IsUnlocked(unlockableType))
				{
					flag = false;
				}
			}
			if (OverwriteUnlocks.Enabled)
			{
				flag = true;
			}
			if (flag)
			{
				((Graphic)spawnMenuSectionReference.buttonBackgroundImage).color = list[i].backgroundColor;
				spawnMenuSectionReference.buttonForegroundImage.sprite = ResolveSprite(list[i]);
			}
			else
			{
				((Graphic)spawnMenuSectionReference.buttonBackgroundImage).color = Color.gray;
				spawnMenuSectionReference.buttonForegroundImage.sprite = lockedIcon;
			}
			Button val = UnityEngine.Object.Instantiate<Button>(spawnMenuSectionReference.button, spawnMenuSectionReference.grid.transform, false);
			SpawnableObject spawnableObj = list[i];
			if (flag)
			{
				((UnityEvent)(object)val.onClick).AddListener((UnityAction)delegate
				{
					SelectObject(spawnableObj);
				});
			}
		}
		((Component)(object)spawnMenuSectionReference.button).gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			MonoSingleton<OptionsManager>.Instance.UnFreeze();
		}
	}

	private void OnEnable()
	{
		GameStateManager.Instance.RegisterState(new GameState("sandbox-spawn-menu", base.gameObject)
		{
			cursorLock = LockMode.Unlock,
			playerInputLock = LockMode.Lock,
			cameraInputLock = LockMode.Lock
		});
	}

	private void SelectObject(SpawnableObject obj)
	{
		armManager.SelectArm(obj);
	}
}
