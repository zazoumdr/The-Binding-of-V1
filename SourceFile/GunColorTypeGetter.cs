using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunColorTypeGetter : MonoBehaviour
{
	public int weaponNumber;

	public bool altVersion;

	public GameObject template;

	public GameObject custom;

	public Button altButton;

	public Button presetsButton;

	public Button customButton;

	public GameObject previewModelStandard;

	public GunColorGetter[] previewColorGetterStandard;

	public GameObject previewModelAlt;

	public GunColorGetter[] previewColorGetterAlt;

	public List<Button> templateButtons;

	private Image[] templateButtonsImages;

	private TMP_Text[] templateTexts;

	private string[] originalTemplateTexts;

	public GunColorSetter[] gunColorSetters;

	private void Awake()
	{
		templateButtonsImages = (Image[])(object)new Image[templateButtons.Count];
		for (int i = 0; i < templateButtons.Count; i++)
		{
			templateButtonsImages[i] = ((Component)(object)templateButtons[i]).GetComponent<Image>();
		}
		templateTexts = (TMP_Text[])(object)new TMP_Text[templateButtons.Count];
		for (int j = 0; j < templateButtons.Count; j++)
		{
			templateTexts[j] = ((Component)(object)templateButtons[j]).GetComponentInChildren<TMP_Text>();
		}
		for (int k = 0; k < templateButtons.Count; k++)
		{
			int index = k;
			((Component)(object)templateButtons[k]).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetPreset(index);
			};
		}
		originalTemplateTexts = new string[templateTexts.Length];
		for (int num = 0; num < templateTexts.Length; num++)
		{
			originalTemplateTexts[num] = templateTexts[num].text;
		}
		((Component)(object)presetsButton).GetComponent<ShopButton>().PointerClickSuccess += delegate
		{
			SetType(isCustom: false);
		};
		((Component)(object)customButton).GetComponent<ShopButton>().PointerClickSuccess += delegate
		{
			SetType(isCustom: true);
		};
	}

	private void OnEnable()
	{
		SetType(MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)));
		if ((bool)(Object)(object)altButton)
		{
			string gear = "";
			switch (weaponNumber)
			{
			case 1:
				gear = "revalt";
				break;
			case 2:
				gear = "shoalt";
				break;
			case 3:
				gear = "naialt";
				break;
			}
			if (GameProgressSaver.CheckGear(gear) >= 1)
			{
				((Component)(object)altButton).gameObject.SetActive(value: true);
			}
			else
			{
				((Component)(object)altButton).gameObject.SetActive(value: false);
			}
		}
		SetPreset(MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : "")));
	}

	public void SetType(bool isCustom)
	{
		bool flag = GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1));
		MonoSingleton<PrefsManager>.Instance.SetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : ""), isCustom && flag);
		MonoSingleton<GunColorController>.Instance.UpdateGunColors();
		template.SetActive(!isCustom);
		((Selectable)presetsButton).interactable = isCustom;
		((Component)(object)presetsButton).GetComponent<ShopButton>().deactivated = !isCustom;
		custom.SetActive(isCustom);
		((Selectable)customButton).interactable = !isCustom;
		((Component)(object)customButton).GetComponent<ShopButton>().deactivated = isCustom;
		UpdatePreview();
	}

	public void SetPreset(int index)
	{
		int totalSecretsFound = GameProgressSaver.GetTotalSecretsFound();
		MonoSingleton<PrefsManager>.Instance.SetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : ""), index);
		MonoSingleton<GunColorController>.Instance.UpdateGunColors();
		for (int i = 0; i < templateButtons.Count; i++)
		{
			int num = GunColorController.requiredSecrets[i];
			bool num2 = totalSecretsFound >= num;
			ShopButton component = ((Component)(object)templateButtons[i]).GetComponent<ShopButton>();
			if (num2)
			{
				templateTexts[i].SetText(originalTemplateTexts[i], true);
				((Graphic)templateTexts[i]).color = Color.white;
				((Graphic)templateButtonsImages[i]).color = Color.white;
				component.failure = false;
				if (i == index)
				{
					((Selectable)templateButtons[i]).interactable = false;
					component.deactivated = true;
				}
				else
				{
					((Selectable)templateButtons[i]).interactable = true;
					component.deactivated = false;
				}
			}
			else
			{
				templateTexts[i].SetText("SOUL ORBS: " + totalSecretsFound + " / " + num, true);
				((Graphic)templateTexts[i]).color = Color.red;
				((Graphic)templateButtonsImages[i]).color = Color.red;
				((Selectable)templateButtons[i]).interactable = false;
				component.failure = true;
				component.deactivated = false;
			}
		}
		UpdatePreview();
	}

	public void UpdatePreview()
	{
		if (!altVersion)
		{
			GunColorGetter[] array = previewColorGetterStandard;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateColor();
			}
		}
		else
		{
			GunColorGetter[] array = previewColorGetterAlt;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateColor();
			}
		}
	}

	public void ToggleAlternate()
	{
		altVersion = !altVersion;
		((Component)(object)altButton).GetComponentInChildren<TMP_Text>().SetText(altVersion ? "Standard" : "Alternate", true);
		previewModelStandard.SetActive(!altVersion);
		previewModelAlt.SetActive(altVersion);
		GunColorSetter[] array = gunColorSetters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateSliders();
		}
		SetType(MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")) && GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)));
		SetPreset(MonoSingleton<PrefsManager>.Instance.GetInt("gunColorPreset." + weaponNumber + (altVersion ? ".a" : "")));
	}
}
