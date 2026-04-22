using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VariationInfo : MonoBehaviour
{
	public GameObject varPage;

	private int money;

	public int cost;

	public TMP_Text costText;

	public ShopButton buyButton;

	private TMP_Text buttonText;

	public GameObject buySound;

	public Image icon;

	public TMP_Text equipText;

	public GameObject equipButtons;

	private int equipStatus;

	public Button equipStatusButton;

	public bool alreadyOwned;

	public string weaponName;

	private GunSetter gs;

	private FistControl fc;

	private GameObject player;

	public GameObject orderButtons;

	[SerializeField]
	private Animator drawer;

	private void Start()
	{
		player = MonoSingleton<NewMovement>.Instance.gameObject;
		buttonText = buyButton.GetComponentInChildren<TMP_Text>();
		buyButton.variationInfo = this;
		if (GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		UpdateMoney();
	}

	private void OnEnable()
	{
		UpdateMoney();
	}

	public void UpdateMoney()
	{
		money = GameProgressSaver.GetMoney();
		MoneyText[] array = Object.FindObjectsOfType<MoneyText>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateMoney();
		}
		if (!alreadyOwned && cost < 0 && GameProgressSaver.CheckGear(weaponName) > 0)
		{
			alreadyOwned = true;
		}
		if (!alreadyOwned)
		{
			if (cost < 0)
			{
				costText.text = "<color=red>Unavailable</color>";
				if ((Object)(object)buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<TMP_Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				((Selectable)buyButton.GetComponent<Button>()).interactable = false;
				((Graphic)buyButton.GetComponent<Image>()).color = Color.red;
				if (TryGetComponent<ShopButton>(out var component))
				{
					component.failure = true;
				}
			}
			else if (cost > money)
			{
				costText.text = "<color=red>" + MoneyText.DivideMoney(cost) + " P</color>";
				if ((Object)(object)buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<TMP_Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = true;
				((Selectable)buyButton.GetComponent<Button>()).interactable = false;
				((Graphic)buyButton.GetComponent<Image>()).color = Color.red;
			}
			else
			{
				costText.text = MoneyText.DivideMoney(cost) + " <color=#FF4343>P</color>";
				if ((Object)(object)buttonText == null)
				{
					buttonText = buyButton.GetComponentInChildren<TMP_Text>();
				}
				buttonText.text = costText.text;
				buyButton.failure = false;
				((Selectable)buyButton.GetComponent<Button>()).interactable = true;
				((Graphic)buyButton.GetComponent<Image>()).color = Color.white;
			}
			equipButtons.SetActive(value: false);
			return;
		}
		costText.text = "Already Owned";
		if ((Object)(object)buttonText == null)
		{
			buttonText = buyButton.GetComponentInChildren<TMP_Text>();
		}
		buttonText.text = costText.text;
		buyButton.failure = true;
		((Selectable)buyButton.GetComponent<Button>()).interactable = false;
		((Graphic)buyButton.GetComponent<Image>()).color = Color.white;
		equipButtons.SetActive(value: true);
		((Selectable)equipStatusButton).interactable = true;
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("weapon." + weaponName, 1);
		if (num == 2 && GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0)
		{
			equipStatus = 2;
		}
		else if (num > 0)
		{
			equipStatus = 1;
		}
		else
		{
			equipStatus = 0;
		}
		if ((bool)orderButtons)
		{
			if (equipStatus != 0)
			{
				orderButtons.SetActive(value: true);
				((Graphic)icon).rectTransform.anchoredPosition = new Vector2(25f, 0f);
				((Graphic)icon).rectTransform.sizeDelta = new Vector2(75f, 75f);
			}
			else
			{
				orderButtons.SetActive(value: false);
				((Graphic)icon).rectTransform.anchoredPosition = new Vector2(0f, 0f);
				((Graphic)icon).rectTransform.sizeDelta = new Vector2(100f, 100f);
			}
		}
		SetEquipStatusText(equipStatus);
		if (cost < 0 && TryGetComponent<ShopButton>(out var component2))
		{
			component2.failure = false;
		}
	}

	public void WeaponBought()
	{
		alreadyOwned = true;
		Object.Instantiate(buySound);
		GameProgressSaver.AddMoney(cost * -1);
		GameProgressSaver.AddGear(weaponName);
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, 1);
		UpdateMoney();
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
		if (PlayerPrefs.GetInt("FirVar", 1) == 1)
		{
			GetComponentInParent<ShopZone>().firstVariationBuy = true;
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		gs.ForceWeapon(weaponName);
		gs.gunc.NoWeapon();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
		drawer.Play("Open", 0, 0f);
	}

	public void ChangeEquipment(int value)
	{
		int num = equipStatus;
		num = ((value <= 0) ? (num - 1) : (num + 1));
		int content = num;
		if (num < 0)
		{
			content = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") <= 0) ? 1 : 2);
		}
		else if (num == 2)
		{
			content = ((GameProgressSaver.CheckGear(weaponName.Substring(0, weaponName.Length - 1) + "alt") > 0) ? 2 : 0);
		}
		else if (num > 2)
		{
			content = 0;
		}
		equipStatus = content;
		SetEquipStatusText(equipStatus);
		MonoSingleton<PrefsManager>.Instance.SetInt("weapon." + weaponName, content);
		if ((bool)orderButtons)
		{
			if (equipStatus != 0)
			{
				orderButtons.SetActive(value: true);
				((Graphic)icon).rectTransform.anchoredPosition = new Vector2(25f, 0f);
				((Graphic)icon).rectTransform.sizeDelta = new Vector2(75f, 75f);
			}
			else
			{
				orderButtons.SetActive(value: false);
				((Graphic)icon).rectTransform.anchoredPosition = new Vector2(0f, 0f);
				((Graphic)icon).rectTransform.sizeDelta = new Vector2(100f, 100f);
			}
		}
		if (gs == null)
		{
			gs = player.GetComponentInChildren<GunSetter>();
		}
		gs.ResetWeapons();
		if (fc == null)
		{
			fc = player.GetComponentInChildren<FistControl>();
		}
		fc.ResetFists();
		drawer.Play("Open", 0, 0f);
	}

	private void SetEquipStatusText(int equipStatus)
	{
		switch (equipStatus)
		{
		case 0:
			equipText.SetText("Unequipped", true);
			((Graphic)equipText).color = Color.gray;
			break;
		case 1:
			equipText.SetText("Equipped", true);
			((Graphic)equipText).color = Color.white;
			break;
		case 2:
			equipText.SetText("Alternate", true);
			((Graphic)equipText).color = new Color(1f, 0.3f, 0.3f);
			break;
		}
	}
}
