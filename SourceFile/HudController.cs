using System;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
	public static HudController Instance;

	public bool altHud;

	public bool colorless;

	private GameObject altHudObj;

	private HUDPos hudpos;

	public GameObject gunCanvas;

	public GameObject weaponIcon;

	public GameObject armIcon;

	public Sprite[] fistIcons;

	public Image fistFill;

	public Image fistBackground;

	public GameObject styleMeter;

	public GameObject styleInfo;

	public Speedometer speedometer;

	[Space]
	public Image[] hudBackgrounds;

	public TMP_Text[] textElements;

	[Space]
	public Material normalTextMaterial;

	public Material overlayTextMaterial;

	private void Awake()
	{
		if (!altHud && !Instance)
		{
			Instance = this;
		}
		if (altHud && altHudObj == null)
		{
			altHudObj = base.transform.GetChild(0).gameObject;
		}
		if (!altHud && hudpos == null)
		{
			hudpos = gunCanvas.GetComponent<HUDPos>();
		}
		MonoSingleton<FistControl>.Instance.FistIconUpdated += UpdateFistIcon;
	}

	private void OnDestroy()
	{
		if ((bool)MonoSingleton<FistControl>.Instance)
		{
			MonoSingleton<FistControl>.Instance.FistIconUpdated -= UpdateFistIcon;
		}
	}

	private void UpdateFistIcon(int current)
	{
		fistFill.sprite = fistIcons[current];
		fistBackground.sprite = fistIcons[current];
		MonoSingleton<FistControl>.Instance.fistIconColor = MonoSingleton<ColorBlindSettings>.Instance.variationColors[current switch
		{
			1 => 2, 
			2 => 1, 
			_ => current, 
		}];
	}

	private void OnEnable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnDisable()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	private void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "hudType":
			CheckSituation();
			break;
		case "hudBackgroundOpacity":
			if (value is float opacity)
			{
				SetOpacity(opacity);
			}
			break;
		case "hudAlwaysOnTop":
			if (value is bool alwaysOnTop)
			{
				SetAlwaysOnTop(alwaysOnTop);
			}
			break;
		case "weaponIcons":
			if (value is bool weaponIcons)
			{
				SetWeaponIcons(weaponIcons);
			}
			break;
		case "armIcons":
			if (value is bool armIcons)
			{
				SetArmIcons(armIcons);
			}
			break;
		case "styleMeter":
			if (value is bool value3)
			{
				SetStyleVisibleTemp(value3);
			}
			break;
		case "styleInfo":
			if (value is bool value2)
			{
				bool? infoVisible = value2;
				SetStyleVisibleTemp(null, infoVisible);
			}
			break;
		}
	}

	private void SetWeaponIcons(bool showIcons)
	{
		if (!altHud)
		{
			weaponIcon.SetActive(showIcons);
			Vector3 localPosition = weaponIcon.transform.localPosition;
			weaponIcon.transform.localPosition = new Vector3(localPosition.x, localPosition.y, showIcons ? 45 : (-9999));
			Vector2 anchoredPosition = (showIcons ? new Vector2(-79f, 590f) : new Vector2(-79f, 190f));
			speedometer.rect.anchoredPosition = anchoredPosition;
		}
		else
		{
			weaponIcon.SetActive(showIcons);
		}
	}

	private void SetArmIcons(bool showIcons)
	{
		if (!altHud)
		{
			Vector3 localPosition = armIcon.transform.localPosition;
			armIcon.transform.localPosition = new Vector3(localPosition.x, localPosition.y, (!showIcons) ? (-9999) : 0);
		}
		else
		{
			armIcon.SetActive(showIcons);
		}
	}

	public void SetStyleVisibleTemp(bool? meterVisible = null, bool? infoVisible = null)
	{
		if (altHud)
		{
			return;
		}
		if (HideUI.Active)
		{
			meterVisible = false;
			infoVisible = false;
		}
		else
		{
			bool valueOrDefault = meterVisible == true;
			if (!meterVisible.HasValue)
			{
				valueOrDefault = MonoSingleton<PrefsManager>.Instance.GetBool("styleMeter");
				meterVisible = valueOrDefault;
			}
			valueOrDefault = infoVisible == true;
			if (!infoVisible.HasValue)
			{
				valueOrDefault = MonoSingleton<PrefsManager>.Instance.GetBool("styleInfo");
				infoVisible = valueOrDefault;
			}
		}
		styleMeter.transform.localPosition = new Vector3(styleMeter.transform.localPosition.x, styleMeter.transform.localPosition.y, (!meterVisible.Value) ? (-9999) : 0);
		styleInfo.transform.localPosition = new Vector3(styleInfo.transform.localPosition.x, styleInfo.transform.localPosition.y, (!infoVisible.Value) ? (-9999) : 0);
		((Behaviour)(object)MonoSingleton<StyleHUD>.Instance.GetComponent<AudioSource>()).enabled = infoVisible.Value;
	}

	private void Update()
	{
		float punchStamina = MonoSingleton<WeaponCharges>.Instance.punchStamina;
		Color fistIconColor = MonoSingleton<FistControl>.Instance.fistIconColor;
		fistFill.fillAmount = punchStamina / 2f;
		((Graphic)fistFill).color = ((punchStamina >= 1f) ? fistIconColor : (fistIconColor * new Color(0.6f, 0.6f, 0.6f, 1f)));
		((Graphic)fistBackground).color = fistIconColor * new Color(0.2f, 0.2f, 0.2f, 1f);
	}

	private void Start()
	{
		if (MapInfoBase.Instance.hideStockHUD)
		{
			weaponIcon.SetActive(value: false);
			armIcon.SetActive(value: false);
			return;
		}
		CheckSituation();
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("weaponIcons"))
		{
			if (!altHud)
			{
				speedometer.rect.anchoredPosition = new Vector2(-79f, 190f);
				weaponIcon.transform.localPosition = new Vector3(weaponIcon.transform.localPosition.x, weaponIcon.transform.localPosition.y, 45f);
			}
			else
			{
				weaponIcon.SetActive(value: false);
			}
		}
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("armIcons"))
		{
			if (!altHud)
			{
				armIcon.transform.localPosition = new Vector3(armIcon.transform.localPosition.x, armIcon.transform.localPosition.y, 0f);
			}
			else
			{
				armIcon.SetActive(value: false);
			}
		}
		if (!altHud)
		{
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("styleMeter"))
			{
				styleMeter.transform.localPosition = new Vector3(styleMeter.transform.localPosition.x, styleMeter.transform.localPosition.y, -9999f);
			}
			if (!MonoSingleton<PrefsManager>.Instance.GetBool("styleInfo"))
			{
				styleInfo.transform.localPosition = new Vector3(styleInfo.transform.localPosition.x, styleInfo.transform.localPosition.y, -9999f);
				((Behaviour)(object)MonoSingleton<StyleHUD>.Instance.GetComponent<AudioSource>()).enabled = false;
			}
		}
		float num = MonoSingleton<PrefsManager>.Instance.GetFloat("hudBackgroundOpacity");
		if (num != 50f)
		{
			SetOpacity(num);
		}
		SetAlwaysOnTop(MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop"));
	}

	public void CheckSituation()
	{
		if (HideUI.Active)
		{
			if ((bool)gunCanvas)
			{
				((Behaviour)(object)gunCanvas.GetComponent<Canvas>()).enabled = false;
			}
			if ((bool)altHudObj)
			{
				altHudObj.SetActive(value: false);
			}
			return;
		}
		if (altHud)
		{
			if ((bool)altHudObj)
			{
				if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") == 2 && !colorless)
				{
					altHudObj.SetActive(value: true);
				}
				else if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") == 3 && colorless)
				{
					altHudObj.SetActive(value: true);
				}
				else
				{
					altHudObj.SetActive(value: false);
				}
			}
			MonoSingleton<PrefsManager>.Instance.GetBool("speedometer");
			return;
		}
		if (MonoSingleton<PrefsManager>.Instance.GetInt("hudType") != 1)
		{
			if (gunCanvas == null)
			{
				gunCanvas = base.transform.Find("GunCanvas").gameObject;
			}
			if (hudpos == null)
			{
				hudpos = gunCanvas.GetComponent<HUDPos>();
			}
			gunCanvas.transform.localPosition = new Vector3(gunCanvas.transform.localPosition.x, gunCanvas.transform.localPosition.y, -100f);
			((Behaviour)(object)gunCanvas.GetComponent<Canvas>()).enabled = false;
			if ((bool)hudpos)
			{
				hudpos.active = false;
			}
			return;
		}
		if (gunCanvas == null)
		{
			gunCanvas = base.transform.Find("GunCanvas").gameObject;
		}
		if (hudpos == null)
		{
			hudpos = gunCanvas.GetComponent<HUDPos>();
		}
		((Behaviour)(object)gunCanvas.GetComponent<Canvas>()).enabled = true;
		gunCanvas.transform.localPosition = new Vector3(gunCanvas.transform.localPosition.x, gunCanvas.transform.localPosition.y, 1f);
		if ((bool)hudpos)
		{
			hudpos.active = true;
			hudpos.CheckPos();
		}
	}

	public void SetOpacity(float amount)
	{
		Image[] array = hudBackgrounds;
		foreach (Image val in array)
		{
			if ((bool)(UnityEngine.Object)(object)val)
			{
				Color color = ((Graphic)val).color;
				color.a = amount / 100f;
				((Graphic)val).color = color;
			}
		}
	}

	public void SetAlwaysOnTop(bool onTop)
	{
		if (textElements != null)
		{
			TMP_Text[] array = textElements;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].fontSharedMaterial = (onTop ? overlayTextMaterial : normalTextMaterial);
			}
		}
	}
}
