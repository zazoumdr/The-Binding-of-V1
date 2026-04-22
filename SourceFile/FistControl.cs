using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class FistControl : MonoSingleton<FistControl>
{
	private InputManager inman;

	public ForcedLoadout forcedLoadout;

	public AssetReference blueArm;

	public AssetReference redArm;

	public AssetReference goldArm;

	private int currentOrderNum;

	private int currentVarNum;

	private List<GameObject> spawnedArms = new List<GameObject>();

	private List<int> spawnedArmNums = new List<int>();

	private AudioSource aud;

	public bool shopping;

	private int shopRequests;

	public GameObject[] fistPanels;

	public ItemIdentifier heldObject;

	public float fistCooldown;

	public Color fistIconColor;

	private bool _activated = true;

	public Punch currentPunch;

	[HideInInspector]
	public int forceNoHold;

	private bool zooming;

	public bool activated
	{
		get
		{
			if (_activated)
			{
				return !MonoSingleton<OptionsManager>.Instance.paused;
			}
			return false;
		}
		set
		{
			_activated = value;
		}
	}

	public GameObject currentArmObject => spawnedArms[currentOrderNum];

	public event Action<int> FistIconUpdated;

	private void Start()
	{
		inman = MonoSingleton<InputManager>.Instance;
		aud = GetComponent<AudioSource>();
		currentVarNum = PlayerPrefs.GetInt("CurArm", 0);
		ResetFists();
		fistCooldown = 0f;
	}

	private void Update()
	{
		if (fistCooldown > -1f)
		{
			fistCooldown = Mathf.MoveTowards(fistCooldown, 0f, Time.deltaTime * 2f);
		}
		float punchStamina = MonoSingleton<WeaponCharges>.Instance.punchStamina;
		if (!MonoSingleton<OptionsManager>.Instance.paused && fistCooldown <= 0f && punchStamina >= 1f)
		{
			if (inman.InputSource.Actions.Fist.PunchFeedbacker.WasPerformedThisFrame() && (currentVarNum == 0 || ForceArm(0)))
			{
				currentPunch.heldAction = MonoSingleton<InputManager>.Instance.InputSource.Actions.Fist.PunchFeedbacker;
				currentPunch.PunchStart();
			}
			if (inman.InputSource.Actions.Fist.PunchKnuckleblaster.WasPerformedThisFrame() && (currentVarNum == 1 || ForceArm(1)))
			{
				currentPunch.heldAction = MonoSingleton<InputManager>.Instance.InputSource.Actions.Fist.PunchKnuckleblaster;
				currentPunch.PunchStart();
			}
		}
		if (!MonoSingleton<OptionsManager>.Instance || MonoSingleton<OptionsManager>.Instance.mainMenu || MonoSingleton<OptionsManager>.Instance.inIntro || MonoSingleton<OptionsManager>.Instance.paused || ((bool)MonoSingleton<ScanningStuff>.Instance && MonoSingleton<ScanningStuff>.Instance.IsReading) || GameStateManager.Instance.PlayerInputLocked)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && shopping)
		{
			zooming = true;
			MonoSingleton<CameraController>.Instance.Zoom(MonoSingleton<CameraController>.Instance.defaultFov / 2f);
		}
		else if (zooming)
		{
			zooming = false;
			MonoSingleton<CameraController>.Instance.StopZoom();
		}
		if (spawnedArms.Count > 1 && !shopping && (MonoSingleton<SpawnMenu>.Instance == null || !MonoSingleton<SpawnMenu>.Instance.gameObject.activeInHierarchy))
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.ChangeFist.WasPerformedThisFrame)
			{
				ScrollArm();
			}
		}
		else if (spawnedArms.Count > 0 && currentPunch == null)
		{
			ArmChange(0);
		}
		if (spawnedArms.Count == 0 && MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame && (forcedLoadout == null || forcedLoadout.arm.blueVariant != VariantOption.ForceOff || forcedLoadout.arm.redVariant != VariantOption.ForceOff))
		{
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>CAN'T PUNCH IF YOU HAVE NO ARM EQUIPPED, DUMBASS</color>\nArms can be re-equipped at the shop");
		}
	}

	public void ScrollArm()
	{
		if (currentOrderNum < spawnedArms.Count - 1)
		{
			ArmChange(currentOrderNum + 1);
		}
		else
		{
			ArmChange(0);
		}
		aud.Play(tracked: true);
	}

	public void RefreshArm()
	{
		ArmChange(currentOrderNum);
	}

	public bool ForceArm(int varNum, bool animation = false)
	{
		bool result = false;
		if (spawnedArmNums.Contains(varNum))
		{
			ArmChange(spawnedArmNums.IndexOf(varNum));
			result = true;
		}
		if (animation)
		{
			Punch component;
			if (varNum == 2)
			{
				MonoSingleton<HookArm>.Instance.Inspect();
			}
			else if (spawnedArms[currentOrderNum].TryGetComponent<Punch>(out component))
			{
				component.EquipAnimation();
			}
			aud.Play(tracked: true);
		}
		return result;
	}

	public void ArmChange(int orderNum)
	{
		if (orderNum < spawnedArms.Count)
		{
			if (currentOrderNum < spawnedArms.Count)
			{
				spawnedArms[currentOrderNum].SetActive(value: false);
			}
			spawnedArms[orderNum].SetActive(value: true);
			currentOrderNum = orderNum;
			currentVarNum = spawnedArmNums[orderNum];
			PlayerPrefs.SetInt("CurArm", currentVarNum);
			UpdateFistIcon();
			currentPunch = currentArmObject.GetComponent<Punch>();
			if (MonoSingleton<FistControl>.Instance.fistCooldown > 0.1f)
			{
				MonoSingleton<FistControl>.Instance.fistCooldown = 0.1f;
			}
		}
	}

	public void UpdateFistIcon()
	{
		this.FistIconUpdated?.Invoke(currentVarNum);
	}

	public void NoFist()
	{
		if (spawnedArms.Count > 0)
		{
			spawnedArms[currentOrderNum].SetActive(value: false);
		}
		activated = false;
	}

	public void YesFist()
	{
		if (spawnedArms.Count > 0)
		{
			spawnedArms[currentOrderNum].SetActive(value: true);
		}
		activated = true;
		UpdateFistIcon();
	}

	public void ResetFists()
	{
		if (spawnedArms.Count > 0)
		{
			for (int i = 0; i < spawnedArms.Count; i++)
			{
				UnityEngine.Object.Destroy(spawnedArms[i]);
			}
			spawnedArms.Clear();
			spawnedArmNums.Clear();
		}
		MonoSingleton<HookArm>.Instance.equipped = false;
		if ((MonoSingleton<PrefsManager>.Instance.GetInt("weapon.arm0", 1) == 1 && (forcedLoadout == null || forcedLoadout.arm.blueVariant == VariantOption.IfEquipped)) || (forcedLoadout != null && forcedLoadout.arm.blueVariant == VariantOption.ForceOn))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(blueArm.ToAsset(), base.transform);
			gameObject.SetActive(value: false);
			spawnedArms.Add(gameObject);
			spawnedArmNums.Add(0);
		}
		CheckFist("arm1");
		CheckFist("arm2");
		CheckFist("arm3");
		if (spawnedArms.Count < 1 || !MonoSingleton<PrefsManager>.Instance.GetBool("armIcons"))
		{
			GameObject[] array = fistPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
		}
		else
		{
			GameObject[] array = fistPanels;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}
		ForceArm(currentVarNum);
		UpdateFistIcon();
	}

	private void CheckFist(string name)
	{
		if (forcedLoadout != null)
		{
			if (!(name == "arm1"))
			{
				if (name == "arm2")
				{
					if (forcedLoadout.arm.greenVariant == VariantOption.ForceOn)
					{
						MonoSingleton<HookArm>.Instance.equipped = true;
						return;
					}
					if (forcedLoadout.arm.greenVariant == VariantOption.ForceOff)
					{
						return;
					}
				}
			}
			else
			{
				if (forcedLoadout.arm.redVariant == VariantOption.ForceOn)
				{
					spawnedArmNums.Add(1);
					GameObject gameObject = UnityEngine.Object.Instantiate(redArm.ToAsset(), base.transform);
					gameObject.SetActive(value: false);
					spawnedArms.Add(gameObject);
					return;
				}
				if (forcedLoadout.arm.redVariant == VariantOption.ForceOff)
				{
					return;
				}
			}
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + name, 1) == 1 && GameProgressSaver.CheckGear(name) == 1)
		{
			GameObject gameObject2 = null;
			switch (name)
			{
			case "arm1":
				gameObject2 = UnityEngine.Object.Instantiate(redArm.ToAsset(), base.transform);
				spawnedArmNums.Add(1);
				break;
			case "arm2":
				MonoSingleton<HookArm>.Instance.equipped = true;
				break;
			case "arm3":
				gameObject2 = UnityEngine.Object.Instantiate(goldArm.ToAsset(), base.transform);
				spawnedArmNums.Add(3);
				break;
			}
			if (gameObject2 != null)
			{
				spawnedArms.Add(gameObject2);
			}
		}
	}

	public void ShopMode()
	{
		shopping = true;
		shopRequests++;
	}

	public void StopShop()
	{
		shopRequests--;
		if (shopRequests <= 0)
		{
			shopping = false;
		}
	}

	public void ResetHeldItemPosition()
	{
		if (heldObject.reverseTransformSettings)
		{
			heldObject.transform.localPosition = heldObject.putDownPosition;
			heldObject.transform.localScale = heldObject.putDownScale;
			heldObject.transform.localRotation = Quaternion.Euler(heldObject.putDownRotation);
		}
		else
		{
			heldObject.transform.localPosition = Vector3.zero;
			heldObject.transform.localScale = Vector3.one;
			heldObject.transform.localRotation = Quaternion.identity;
		}
		Transform[] componentsInChildren = heldObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 13;
		}
	}

	public void TutorialCheckForArmThatCanPunch()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetInt("weapon.arm0", 1) == 0 && MonoSingleton<PrefsManager>.Instance.GetInt("weapon.arm1", 1) == 0)
		{
			MonoSingleton<PrefsManager>.Instance.SetInt("weapon.arm0", 1);
			ResetFists();
		}
	}
}
