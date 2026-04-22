using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WeaponHUD : MonoSingleton<WeaponHUD>
{
	private Image img;

	private Image glowImg;

	private void Awake()
	{
		WeaponIcon weaponIcon = Object.FindObjectOfType<WeaponIcon>();
		if ((bool)weaponIcon)
		{
			weaponIcon.UpdateIcon();
		}
	}

	public void UpdateImage(Sprite icon, Sprite glowIcon, int variation)
	{
		if ((Object)(object)img == null)
		{
			img = GetComponent<Image>();
		}
		if ((Object)(object)glowImg == null)
		{
			glowImg = base.transform.GetChild(0).GetComponent<Image>();
		}
		img.sprite = icon;
		((Graphic)img).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		glowImg.sprite = glowIcon;
		((Graphic)glowImg).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
	}
}
