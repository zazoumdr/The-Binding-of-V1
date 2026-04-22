using System;
using System.Collections.Generic;
using System.Linq;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class GunControl : MonoSingleton<GunControl>
{
	private InputManager inman;

	public bool activated = true;

	private int rememberedSlot;

	public int currentVariationIndex;

	public int currentSlotIndex;

	public GameObject currentWeapon;

	public List<List<GameObject>> slots = new List<List<GameObject>>();

	public List<GameObject> slot1 = new List<GameObject>();

	public List<GameObject> slot2 = new List<GameObject>();

	public List<GameObject> slot3 = new List<GameObject>();

	public List<GameObject> slot4 = new List<GameObject>();

	public List<GameObject> slot5 = new List<GameObject>();

	public List<GameObject> slot6 = new List<GameObject>();

	public List<GameObject> allWeapons = new List<GameObject>();

	public Dictionary<GameObject, int> slotDict = new Dictionary<GameObject, int>();

	public List<WeaponIcon> currentWeaponIcons = new List<WeaponIcon>();

	private AudioSource aud;

	public float killCharge;

	public Slider killMeter;

	public bool noWeapons = true;

	public int lastSlotIndex = 69;

	public int lastVariationIndex = 69;

	private Dictionary<int, int> retainedVariations = new Dictionary<int, int>();

	public float headShotComboTime;

	public int headshots;

	private bool hookCombo;

	private StyleHUD shud;

	public GameObject[] gunPanel;

	private float scrollCooldown;

	private const float WeaponWheelTime = 0.25f;

	[HideInInspector]
	public int dualWieldCount;

	[HideInInspector]
	public bool stayUnarmed;

	[HideInInspector]
	public bool variationMemory;

	public event Action<GameObject> OnWeaponChange;

	private void Start()
	{
		inman = MonoSingleton<InputManager>.Instance;
		currentVariationIndex = PlayerPrefs.GetInt("CurVar", 0);
		currentSlotIndex = PlayerPrefs.GetInt("CurSlo", 1);
		lastVariationIndex = PlayerPrefs.GetInt("LasVar", 69);
		lastSlotIndex = PlayerPrefs.GetInt("LasSlo", 69);
		Debug.Log($"Last Slot is {lastSlotIndex}");
		aud = GetComponent<AudioSource>();
		variationMemory = MonoSingleton<PrefsManager>.Instance.GetBool("variationMemory");
		slots.Add(slot1);
		slots.Add(slot2);
		slots.Add(slot3);
		slots.Add(slot4);
		slots.Add(slot5);
		slots.Add(slot6);
		if (currentSlotIndex > slots.Count)
		{
			currentSlotIndex = 1;
		}
		int num = 0;
		foreach (List<GameObject> slot in slots)
		{
			foreach (GameObject item in slot)
			{
				if (item != null)
				{
					allWeapons.Add(item);
					slotDict.Add(item, num);
				}
			}
			if (slot.Count != 0)
			{
				noWeapons = false;
			}
			num++;
		}
		if (currentWeapon == null && slots[currentSlotIndex - 1].Count > currentVariationIndex)
		{
			currentWeapon = slots[currentSlotIndex - 1][currentVariationIndex];
		}
		else if (currentWeapon == null && slot1.Count != 0)
		{
			currentSlotIndex = 1;
			currentVariationIndex = 0;
			currentWeapon = slot1[0];
		}
		shud = MonoSingleton<StyleHUD>.Instance;
		UpdateWeaponList(firstTime: true);
		for (int i = 0; i < slots.Count; i++)
		{
			int num2 = PlayerPrefs.GetInt("Slot" + i + "Var", -1);
			if (num2 >= 0 && num2 < slots[i].Count)
			{
				retainedVariations.Add(i, num2);
			}
		}
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<int, int> retainedVariation in retainedVariations)
		{
			PlayerPrefs.SetInt("Slot" + retainedVariation.Key + "Var", retainedVariation.Value);
		}
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string id, object value)
	{
		if (id == "variationMemory" && value is bool flag)
		{
			variationMemory = flag;
		}
	}

	private void CalculateSlotCount()
	{
		List<WeaponDescriptor> list = new List<WeaponDescriptor>();
		List<int> list2 = new List<int>();
		foreach (List<GameObject> slot in slots)
		{
			GameObject gameObject = slot.FirstOrDefault();
			if (!(gameObject == null))
			{
				WeaponIcon component = gameObject.GetComponent<WeaponIcon>();
				if (component != null)
				{
					list.Add(component.weaponDescriptor);
				}
				list2.Add(slots.IndexOf(slot));
			}
		}
		MonoSingleton<WeaponWheel>.Instance.SetSegments(list.ToArray(), list2.ToArray());
	}

	private void Update()
	{
		if (activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			PlayerInput inputSource = inman.InputSource;
			if (headShotComboTime > 0f)
			{
				headShotComboTime = Mathf.MoveTowards(headShotComboTime, 0f, Time.deltaTime);
			}
			else
			{
				headshots = 0;
			}
			if (lastSlotIndex == 0)
			{
				lastSlotIndex = 69;
			}
			if (!MonoSingleton<OptionsManager>.Instance.inIntro && !MonoSingleton<OptionsManager>.Instance.paused && !MonoSingleton<NewMovement>.Instance.dead)
			{
				if (inputSource.NextWeapon.IsPressed && inputSource.PrevWeapon.IsPressed)
				{
					hookCombo = true;
					if (MonoSingleton<WeaponWheel>.Instance.gameObject.activeSelf)
					{
						MonoSingleton<WeaponWheel>.Instance.gameObject.SetActive(value: false);
					}
				}
				if (((inputSource.NextWeapon.IsPressed && inputSource.NextWeapon.HoldTime >= 0.25f && !inputSource.PrevWeapon.IsPressed) || (inputSource.PrevWeapon.IsPressed && inputSource.PrevWeapon.HoldTime >= 0.25f && !inputSource.NextWeapon.IsPressed) || (inputSource.LastWeapon.IsPressed && inputSource.LastWeapon.HoldTime >= 0.25f) || (inputSource.PreviousVariation.IsPressed && inputSource.PreviousVariation.HoldTime >= 0.25f)) && !hookCombo)
				{
					MonoSingleton<WeaponWheel>.Instance.Show();
				}
			}
			if (inman.InputSource.Slot1.WasPerformedThisFrame)
			{
				if (slot1.Count > 0 && slot1[0] != null && (slot1.Count > 1 || currentSlotIndex != 1))
				{
					SwitchWeapon(1);
				}
			}
			else if (inman.InputSource.Slot2.WasPerformedThisFrame)
			{
				if (slot2.Count > 0 && slot2[0] != null && (slot2.Count > 1 || currentSlotIndex != 2))
				{
					SwitchWeapon(2);
				}
			}
			else if (inman.InputSource.Slot3.WasPerformedThisFrame && (slot3.Count > 1 || currentSlotIndex != 3))
			{
				if (slot3.Count > 0 && slot3[0] != null)
				{
					SwitchWeapon(3);
				}
			}
			else if (inman.InputSource.Slot4.WasPerformedThisFrame && (slot4.Count > 1 || currentSlotIndex != 4))
			{
				if (slot4.Count > 0 && slot4[0] != null)
				{
					SwitchWeapon(4);
				}
			}
			else if (inman.InputSource.Slot5.WasPerformedThisFrame && (slot5.Count > 1 || currentSlotIndex != 5))
			{
				if (slot5.Count > 0 && slot5[0] != null)
				{
					SwitchWeapon(5);
				}
			}
			else if (inman.InputSource.Slot6.WasPerformedThisFrame && (slot6.Count > 1 || currentSlotIndex != 6))
			{
				if (slot6.Count > 0 && slot6[0] != null)
				{
					SwitchWeapon(6, null, useRetainedVariation: true);
				}
			}
			else if (inman.InputSource.LastWeapon.WasCanceledThisFrame && inman.InputSource.LastWeapon.HoldTime < 0.25f && lastSlotIndex != 69)
			{
				if (slots[lastSlotIndex - 1] != null)
				{
					SwitchWeapon(lastSlotIndex, null, useRetainedVariation: true);
				}
			}
			else if (inman.InputSource.NextVariation.WasPerformedThisFrame && slots[currentSlotIndex - 1].Count > 1)
			{
				SwitchWeapon(currentSlotIndex, currentVariationIndex + 1, useRetainedVariation: false, cycleSlot: false, cycleVariation: true);
			}
			else if (inman.InputSource.PreviousVariation.WasCanceledThisFrame && inputSource.PreviousVariation.HoldTime < 0.25f && slots[currentSlotIndex - 1].Count > 1)
			{
				SwitchWeapon(currentSlotIndex, currentVariationIndex - 1, useRetainedVariation: false, cycleSlot: false, cycleVariation: true);
			}
			else if (inman.InputSource.SelectVariant1.WasPerformedThisFrame)
			{
				SwitchWeapon(currentSlotIndex, 0);
			}
			else if (inman.InputSource.SelectVariant2.WasPerformedThisFrame)
			{
				SwitchWeapon(currentSlotIndex, 1);
			}
			else if (inman.InputSource.SelectVariant3.WasPerformedThisFrame)
			{
				SwitchWeapon(currentSlotIndex, 2);
			}
			else if (!noWeapons)
			{
				float num = ((InputControl<Vector2>)(object)Mouse.current.scroll).ReadValue().y;
				if (inman.ScrRev)
				{
					num *= -1f;
				}
				if (inputSource.NextWeapon.HoldTime < 0.25f && !hookCombo && ((num > 0f && inman.ScrOn) || inputSource.NextWeapon.WasCanceledThisFrame) && scrollCooldown <= 0f)
				{
					int num2 = 0;
					if (inman.ScrWep && inman.ScrVar)
					{
						foreach (List<GameObject> slot in slots)
						{
							if (slot.Count > 0)
							{
								num2++;
							}
						}
					}
					bool flag = false;
					if (inman.ScrVar)
					{
						if (slots[currentSlotIndex - 1].Count > currentVariationIndex + 1 || ((!inman.ScrWep || num2 <= 1) && slots[currentSlotIndex - 1].Count > 1))
						{
							SwitchWeapon(currentSlotIndex, currentVariationIndex + 1, useRetainedVariation: false, cycleSlot: false, cycleVariation: true);
							scrollCooldown = 0.5f;
							flag = true;
						}
						else if (!inman.ScrWep)
						{
							flag = true;
						}
					}
					if (!flag && inman.ScrWep)
					{
						if (!flag && currentSlotIndex < slots.Count)
						{
							for (int i = currentSlotIndex; i < slots.Count; i++)
							{
								if (slots[i].Count > 0)
								{
									flag = true;
									SwitchWeapon(i + 1, null, useRetainedVariation: false, cycleSlot: true);
									scrollCooldown = 0.5f;
									break;
								}
							}
						}
						if (!flag)
						{
							for (int j = 0; j < currentSlotIndex; j++)
							{
								if (slots[j].Count > 0)
								{
									if (j != currentSlotIndex - 1)
									{
										SwitchWeapon(j + 1, null, useRetainedVariation: false, cycleSlot: true);
										scrollCooldown = 0.5f;
									}
									break;
								}
							}
						}
					}
				}
				else if (inputSource.PrevWeapon.HoldTime < 0.25f && !hookCombo && ((num < 0f && inman.ScrOn) || inputSource.PrevWeapon.WasCanceledThisFrame) && scrollCooldown <= 0f)
				{
					int num3 = 0;
					if (inman.ScrWep && inman.ScrVar)
					{
						foreach (List<GameObject> slot2 in slots)
						{
							if (slot2.Count > 0)
							{
								num3++;
							}
						}
					}
					if ((inman.ScrWep && !inman.ScrVar) || (inman.ScrWep && num3 > 1))
					{
						if (inman.ScrVar)
						{
							if (currentVariationIndex != 0)
							{
								GameObject weapon = slots[currentSlotIndex - 1][currentVariationIndex - 1];
								ForceWeapon(weapon);
								scrollCooldown = 0.5f;
							}
							else if (currentSlotIndex == 1)
							{
								for (int num4 = slots.Count - 1; num4 >= 0; num4--)
								{
									if (slots[num4].Count > 0)
									{
										if (num4 != currentSlotIndex - 1)
										{
											GameObject weapon2 = slots[num4][slots[num4].Count - 1];
											ForceWeapon(weapon2);
											scrollCooldown = 0.5f;
										}
										break;
									}
								}
							}
							else
							{
								bool flag2 = false;
								for (int num5 = currentSlotIndex - 2; num5 >= 0; num5--)
								{
									if (slots[num5].Count > 0)
									{
										GameObject weapon3 = slots[num5][slots[num5].Count - 1];
										ForceWeapon(weapon3);
										scrollCooldown = 0.5f;
										flag2 = true;
										break;
									}
								}
								if (!flag2)
								{
									for (int num6 = slots.Count - 1; num6 >= 0; num6--)
									{
										if (slots[num6].Count > 0)
										{
											if (num6 != currentSlotIndex - 1)
											{
												GameObject weapon4 = slots[num6][slots[num6].Count - 1];
												ForceWeapon(weapon4);
												scrollCooldown = 0.5f;
											}
											break;
										}
									}
								}
							}
						}
						else if (currentSlotIndex == 1)
						{
							for (int num7 = slots.Count - 1; num7 >= 0; num7--)
							{
								if (slots[num7].Count > 0)
								{
									if (num7 != currentSlotIndex - 1)
									{
										SwitchWeapon(num7 + 1, null, useRetainedVariation: false, cycleSlot: true);
										scrollCooldown = 0.5f;
									}
									break;
								}
							}
						}
						else
						{
							bool flag3 = false;
							for (int num8 = currentSlotIndex - 2; num8 >= 0; num8--)
							{
								if (slots[num8].Count > 0)
								{
									SwitchWeapon(num8 + 1, null, useRetainedVariation: false, cycleSlot: true);
									scrollCooldown = 0.5f;
									flag3 = true;
									break;
								}
							}
							if (!flag3)
							{
								for (int num9 = slots.Count - 1; num9 >= 0; num9--)
								{
									if (slots[num9].Count > 0)
									{
										if (num9 != currentSlotIndex - 1)
										{
											SwitchWeapon(num9 + 1, null, useRetainedVariation: false, cycleSlot: true);
											scrollCooldown = 0.5f;
										}
										break;
									}
								}
							}
						}
					}
					else if (slots[currentSlotIndex - 1].Count > 1)
					{
						SwitchWeapon(currentSlotIndex, currentVariationIndex - 1, useRetainedVariation: false, cycleSlot: false, cycleVariation: true);
						scrollCooldown = 0.5f;
					}
				}
			}
			if (hookCombo && !inputSource.NextWeapon.IsPressed && !inputSource.PrevWeapon.IsPressed)
			{
				hookCombo = false;
			}
		}
		if (scrollCooldown > 0f)
		{
			scrollCooldown = Mathf.MoveTowards(scrollCooldown, 0f, Time.deltaTime * 5f);
		}
	}

	private void OnGUI()
	{
		if (!GunControlDebug.GunControlActivated)
		{
			return;
		}
		GUILayout.Label("Gun Control", Array.Empty<GUILayoutOption>());
		GUILayout.Label("Last Used Slot: " + lastSlotIndex, Array.Empty<GUILayoutOption>());
		GUILayout.Label("Current Slot: " + currentSlotIndex, Array.Empty<GUILayoutOption>());
		GUILayout.Label("Current Variation: " + currentVariationIndex, Array.Empty<GUILayoutOption>());
		GUILayout.Space(12f);
		GUILayout.Label("Retained Variations:", Array.Empty<GUILayoutOption>());
		foreach (KeyValuePair<int, int> retainedVariation in retainedVariations)
		{
			GUILayout.Label(retainedVariation.Key + ": " + retainedVariation.Value, Array.Empty<GUILayoutOption>());
		}
	}

	private void RetainVariation(int slot, int variationIndex)
	{
		if (retainedVariations.ContainsKey(slot))
		{
			retainedVariations[slot] = variationIndex;
		}
		else
		{
			retainedVariations.Add(slot, currentVariationIndex);
		}
	}

	private int loop(int x, int m)
	{
		int num = x % m;
		if (num >= 0)
		{
			return num;
		}
		return num + m;
	}

	public void SwitchWeapon(int targetSlotIndex, int? targetVariationIndex = null, bool useRetainedVariation = false, bool cycleSlot = false, bool cycleVariation = false)
	{
		if (slots.Count == 0)
		{
			Debug.LogWarning("Tried to switch weapon with no slots");
			return;
		}
		targetSlotIndex = Mathf.Clamp(targetSlotIndex, 1, slots.Count);
		if (cycleSlot)
		{
			targetSlotIndex = loop(targetSlotIndex - 1, slots.Count) + 1;
		}
		List<GameObject> list = slots[targetSlotIndex - 1];
		if (list.Count == 0)
		{
			Debug.LogWarning("Tried to switch weapon to slot with no variations");
			return;
		}
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
		}
		if (cycleVariation)
		{
			targetVariationIndex = loop(targetVariationIndex.GetValueOrDefault(), list.Count);
		}
		if (targetSlotIndex != currentSlotIndex)
		{
			lastSlotIndex = currentSlotIndex;
		}
		int? num = targetVariationIndex;
		int num2 = currentVariationIndex;
		if (num != num2)
		{
			lastVariationIndex = currentVariationIndex;
		}
		if (targetVariationIndex.HasValue)
		{
			int valueOrDefault = targetVariationIndex.GetValueOrDefault();
			currentSlotIndex = targetSlotIndex;
			currentVariationIndex = loop(valueOrDefault, list.Count);
		}
		else if (currentSlotIndex == targetSlotIndex)
		{
			int num3 = MonoSingleton<PrefsManager>.Instance.GetInt("WeaponRedrawBehaviour");
			switch (num3)
			{
			case 0:
				num2 = loop(currentVariationIndex + 1, slots[targetSlotIndex - 1].Count);
				break;
			case 1:
				num2 = 0;
				break;
			case 2:
				num2 = currentVariationIndex;
				break;
			default:
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(num3);
				break;
			}
			currentVariationIndex = num2;
		}
		else
		{
			if ((useRetainedVariation || variationMemory) && retainedVariations.TryGetValue(targetSlotIndex - 1, out var value) && value >= 0 && value < list.Count)
			{
				targetVariationIndex = value;
			}
			currentSlotIndex = targetSlotIndex;
			currentVariationIndex = targetVariationIndex.GetValueOrDefault();
		}
		RetainVariation(currentSlotIndex - 1, currentVariationIndex);
		if (!noWeapons && currentVariationIndex < slots[currentSlotIndex - 1].Count)
		{
			currentWeapon = slots[currentSlotIndex - 1][currentVariationIndex];
			currentWeapon.SetActive(value: true);
			aud.Play(tracked: true);
			PlayerPrefs.SetInt("CurVar", currentVariationIndex);
			PlayerPrefs.SetInt("CurSlo", currentSlotIndex);
			PlayerPrefs.SetInt("LasVar", lastVariationIndex);
			PlayerPrefs.SetInt("LasSlo", lastSlotIndex);
		}
		this.OnWeaponChange?.Invoke(currentWeapon);
		shud.SnapFreshnessSlider();
	}

	public void ForceWeapon(GameObject weapon, bool setActive = true)
	{
		new List<GameObject>();
		foreach (List<GameObject> slot in slots)
		{
			for (int i = 0; i < slot.Count; i++)
			{
				if (slot[i].name == weapon.name + "(Clone)" || slot[i].name == weapon.name)
				{
					if (currentWeapon != null)
					{
						currentWeapon.SetActive(value: false);
					}
					currentSlotIndex = slots.IndexOf(slot) + 1;
					currentVariationIndex = i;
					RetainVariation(currentSlotIndex - 1, currentVariationIndex);
					currentWeapon = slot[currentVariationIndex];
					if (setActive)
					{
						currentWeapon.SetActive(value: true);
					}
					aud.Play(tracked: true);
					break;
				}
			}
		}
		this.OnWeaponChange?.Invoke(currentWeapon);
	}

	public void NoWeapon()
	{
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
			rememberedSlot = currentSlotIndex;
			activated = false;
		}
	}

	public void YesWeapon()
	{
		if (slots[currentSlotIndex - 1].Count > currentVariationIndex && slots[currentSlotIndex - 1][currentVariationIndex] != null)
		{
			currentWeapon = slots[currentSlotIndex - 1][currentVariationIndex];
			currentWeapon.SetActive(value: true);
		}
		else if (slots[currentSlotIndex - 1].Count > 0)
		{
			currentWeapon = slots[currentSlotIndex - 1][0];
			currentVariationIndex = 0;
			RetainVariation(currentSlotIndex - 1, currentVariationIndex);
			currentWeapon.SetActive(value: true);
		}
		else
		{
			int num = -1;
			for (int i = 0; i < currentSlotIndex; i++)
			{
				if (slots[i].Count > 0)
				{
					num = i;
				}
			}
			if (num == -1)
			{
				num = 99;
				for (int j = currentSlotIndex; j < slots.Count; j++)
				{
					if (slots[j].Count > 0 && j < num)
					{
						num = j;
					}
				}
			}
			if (num != 99)
			{
				currentWeapon = slots[num][0];
				currentSlotIndex = num + 1;
				currentVariationIndex = 0;
			}
			else
			{
				noWeapons = true;
			}
		}
		if (currentWeapon != null)
		{
			currentWeapon.SetActive(value: false);
			activated = true;
			currentWeapon.SetActive(value: true);
		}
	}

	public void AddKill()
	{
		if (killCharge < killMeter.maxValue)
		{
			killCharge += 1f;
			if (killCharge > killMeter.maxValue)
			{
				killCharge = killMeter.maxValue;
			}
			killMeter.value = killCharge;
		}
	}

	public void ClearKills()
	{
		killCharge = 0f;
		killMeter.value = killCharge;
	}

	public void UpdateWeaponList(bool firstTime = false)
	{
		allWeapons.Clear();
		noWeapons = true;
		slotDict.Clear();
		int num = 0;
		foreach (List<GameObject> slot in slots)
		{
			foreach (GameObject item in slot)
			{
				if (item != null)
				{
					allWeapons.Add(item);
					slotDict.Add(item, num);
					if (noWeapons)
					{
						noWeapons = false;
					}
				}
			}
			num++;
		}
		UpdateWeaponIcon(firstTime);
		if (MonoSingleton<RailcannonMeter>.Instance != null)
		{
			MonoSingleton<RailcannonMeter>.Instance.CheckStatus();
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		shud.ResetFreshness();
		CalculateSlotCount();
	}

	public void UpdateWeaponIcon(bool firstTime = false)
	{
		if (gunPanel == null || gunPanel.Length == 0)
		{
			return;
		}
		GameObject[] array;
		if (noWeapons || !MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons") || MapInfoBase.Instance.hideStockHUD)
		{
			array = gunPanel;
			foreach (GameObject gameObject in array)
			{
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
			}
			return;
		}
		array = gunPanel;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2 != null && (!firstTime || gameObject2 != gunPanel[0]))
			{
				gameObject2.SetActive(value: true);
			}
		}
	}
}
