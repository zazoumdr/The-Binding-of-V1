using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponWheel : MonoSingleton<WeaponWheel>
{
	private List<WheelSegment> segments;

	public int segmentCount;

	public GameObject clickSound;

	public GameObject background;

	private int selectedSegment;

	private int lastSelectedSegment;

	private Vector2 direction;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		background.SetActive(value: true);
	}

	private void OnEnable()
	{
		if (!(MonoSingleton<InputManager>.Instance == null))
		{
			Time.timeScale = 0.25f;
			MonoSingleton<TimeController>.Instance.timeScaleModifier = 0.25f;
			selectedSegment = -1;
			direction = Vector2.zero;
			GameStateManager.Instance.RegisterState(new GameState("weapon-wheel", base.gameObject)
			{
				timerModifier = 4f,
				cameraInputLock = LockMode.Lock
			});
		}
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<TimeController>.Instance)
		{
			MonoSingleton<TimeController>.Instance.timeScaleModifier = 1f;
			MonoSingleton<TimeController>.Instance.RestoreTime();
		}
		if ((bool)MonoSingleton<FistControl>.Instance)
		{
			MonoSingleton<FistControl>.Instance.RefreshArm();
		}
	}

	private void Update()
	{
		if (!MonoSingleton<GunControl>.Instance || !MonoSingleton<GunControl>.Instance.activated || MonoSingleton<OptionsManager>.Instance.paused || MonoSingleton<NewMovement>.Instance.dead || GameStateManager.Instance.PlayerInputLocked)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (MonoSingleton<InputManager>.Instance.InputSource.NextWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.PrevWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.LastWeapon.WasCanceledThisFrame || MonoSingleton<InputManager>.Instance.InputSource.PreviousVariation.WasCanceledThisFrame)
		{
			if (selectedSegment != -1)
			{
				int targetSlotIndex = segments[selectedSegment].slotIndex + 1;
				MonoSingleton<GunControl>.Instance.SwitchWeapon(targetSlotIndex);
			}
			base.gameObject.SetActive(value: false);
		}
		else
		{
			if (segments == null || segments.Count == 0)
			{
				return;
			}
			direction = Vector2.ClampMagnitude(direction + MonoSingleton<InputManager>.Instance.InputSource.WheelLook.ReadValue<Vector2>(), 1f);
			float num = Mathf.Repeat(Mathf.Atan2(direction.x, direction.y) * 57.29578f + 90f, 360f);
			if (Mathf.Approximately(num, 360f))
			{
				num = 0f;
			}
			selectedSegment = ((direction.sqrMagnitude > 0f) ? ((int)(num / (360f / (float)segmentCount))) : selectedSegment);
			for (int i = 0; i < segments.Count; i++)
			{
				if (i == selectedSegment)
				{
					segments[i].SetActive(active: true);
				}
				else
				{
					segments[i].SetActive(active: false);
				}
			}
			if (selectedSegment != lastSelectedSegment)
			{
				UnityEngine.Object.Instantiate(clickSound);
				lastSelectedSegment = selectedSegment;
				if ((bool)MonoSingleton<RumbleManager>.Instance)
				{
					MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.WeaponWheelTick);
				}
			}
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			lastSelectedSegment = -1;
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetSegments(WeaponDescriptor[] weaponDescriptors, int[] slotIndexes)
	{
		int num = weaponDescriptors.Length;
		if (num == segmentCount)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (!(segments[i].descriptor == weaponDescriptors[i]) || segments[i].slotIndex != slotIndexes[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		segmentCount = num;
		lastSelectedSegment = -1;
		if (segments == null)
		{
			segments = new List<WheelSegment>(segmentCount);
		}
		foreach (WheelSegment segment in segments)
		{
			segment.DestroySegment();
		}
		segments.Clear();
		for (int j = 0; j < segmentCount; j++)
		{
			UICircle val = new GameObject().AddComponent<UICircle>();
			((UnityEngine.Object)(object)val).name = "Segment " + j;
			val.Arc = 1f / (float)segmentCount - 0.005f;
			val.ArcRotation = (int)(360f * ((float)j / (float)segmentCount) + 1.8f);
			val.Fill = false;
			((Component)(object)val).transform.SetParent(base.transform, worldPositionStays: false);
			((Graphic)val).rectTransform.anchorMin = Vector2.zero;
			((Graphic)val).rectTransform.anchorMax = Vector2.one;
			((Graphic)val).rectTransform.anchoredPosition = Vector2.zero;
			((Graphic)val).rectTransform.sizeDelta = Vector2.zero;
			Outline obj = ((Component)(object)val).gameObject.AddComponent<Outline>();
			((Shadow)obj).effectDistance = new Vector2(2f, -2f);
			((Shadow)obj).effectColor = Color.white;
			UICircle val2 = new GameObject().AddComponent<UICircle>();
			((UnityEngine.Object)(object)val2).name = "Segment Divider " + j;
			val2.Arc = 0.005f;
			val2.ArcRotation = (int)(360f * ((float)j / (float)segmentCount) + 1.8f - 0.9f);
			val2.Fill = false;
			((Component)(object)val2).transform.SetParent(base.transform, worldPositionStays: false);
			((Graphic)val2).rectTransform.anchorMin = Vector2.zero;
			((Graphic)val2).rectTransform.anchorMax = Vector2.one;
			((Graphic)val2).rectTransform.sizeDelta = new Vector2(256f, 256f);
			val2.Thickness = 128f;
			Image val3 = new GameObject().AddComponent<Image>();
			((UnityEngine.Object)(object)val3).name = "Icon " + j;
			val3.sprite = weaponDescriptors[j].icon;
			((Component)(object)val3).transform.SetParent(((Component)(object)val).transform, worldPositionStays: false);
			float num2 = (float)j * 360f / (float)segmentCount;
			float num3 = val.Arc * 360f / 2f;
			float num4 = num2 + num3;
			float f = num4 * (MathF.PI / 180f);
			float num5 = 112f;
			Vector2 vector = new Vector2(0f - Mathf.Cos(f), Mathf.Sin(f)) * num5;
			((Component)(object)val3).transform.localPosition = vector;
			float num6 = num4 + 180f;
			((Component)(object)val3).transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num6);
			Vector2 size = val3.sprite.rect.size;
			((Graphic)val3).rectTransform.sizeDelta = new Vector2(size.x, size.y) * 0.12f;
			Image val4 = new GameObject().AddComponent<Image>();
			((UnityEngine.Object)(object)val4).name = "Icon Outline " + j;
			val4.sprite = weaponDescriptors[j].glowIcon;
			((Component)(object)val4).transform.SetParent(((Component)(object)val).transform, worldPositionStays: false);
			((Component)(object)val4).transform.localPosition = ((Component)(object)val3).transform.localPosition;
			((Component)(object)val4).transform.localRotation = ((Component)(object)val3).transform.localRotation;
			((Graphic)val4).rectTransform.sizeDelta = ((Graphic)val3).rectTransform.sizeDelta;
			((Component)(object)val4).transform.SetAsFirstSibling();
			WheelSegment wheelSegment = new WheelSegment
			{
				segment = val,
				icon = val3,
				iconGlow = val4,
				descriptor = weaponDescriptors[j],
				divider = val2,
				slotIndex = slotIndexes[j]
			};
			segments.Add(wheelSegment);
			wheelSegment.SetActive(active: false);
		}
	}
}
