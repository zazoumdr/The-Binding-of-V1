using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunColorLock : MonoBehaviour
{
	private int weaponNumber;

	public bool alreadyUnlocked;

	public UltrakillEvent onUnlock;

	public GameObject buySound;

	public Button button;

	public TMP_Text buttonText;

	private void OnEnable()
	{
		if (weaponNumber == 0)
		{
			weaponNumber = GetComponentInParent<GunColorTypeGetter>().weaponNumber;
		}
		if (GameProgressSaver.HasWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1)))
		{
			onUnlock?.Invoke();
		}
		else if (GameProgressSaver.GetMoney() < 1000000)
		{
			((Selectable)button).interactable = false;
			buttonText.text = "<color=red>1,000,000 P</color>";
			((Graphic)((Component)(object)button).GetComponent<Image>()).color = Color.red;
			((Component)(object)button).GetComponent<ShopButton>().failure = true;
		}
		else
		{
			((Selectable)button).interactable = true;
			buttonText.text = "1,000,000 <color=#FF4343>P</color>";
			((Graphic)((Component)(object)button).GetComponent<Image>()).color = Color.white;
			((Component)(object)button).GetComponent<ShopButton>().failure = false;
		}
	}

	public void Unlock()
	{
		GameProgressSaver.AddMoney(-1000000);
		GameProgressSaver.UnlockWeaponCustomization((GameProgressSaver.WeaponCustomizationType)(weaponNumber - 1));
		onUnlock?.Invoke();
		((Graphic)((Component)(object)button).GetComponent<Image>()).color = Color.white;
		GetComponentInParent<GunColorTypeGetter>().SetType(isCustom: true);
		Object.Instantiate(buySound);
		MoneyText[] array = Object.FindObjectsOfType<MoneyText>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateMoney();
		}
		VariationInfo[] array2 = Object.FindObjectsOfType<VariationInfo>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].UpdateMoney();
		}
	}
}
