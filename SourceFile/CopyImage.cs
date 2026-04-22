using UnityEngine;
using UnityEngine.UI;

public class CopyImage : MonoBehaviour
{
	private Image img;

	public Image imgToCopy;

	public CopyType copyType;

	public bool copyColor;

	private void Update()
	{
		if ((Object)(object)img == null)
		{
			img = GetComponent<Image>();
		}
		if ((Object)(object)imgToCopy == null && copyType != CopyType.None && MonoSingleton<WeaponHUD>.Instance != null)
		{
			if (copyType == CopyType.WeaponIcon)
			{
				imgToCopy = MonoSingleton<WeaponHUD>.Instance.GetComponent<Image>();
			}
			else if (copyType == CopyType.WeaponShadow)
			{
				imgToCopy = MonoSingleton<WeaponHUD>.Instance.transform.GetChild(0).GetComponent<Image>();
			}
		}
		if ((Object)(object)imgToCopy != null)
		{
			img.sprite = imgToCopy.sprite;
			if (copyColor)
			{
				((Graphic)img).color = ((Graphic)imgToCopy).color;
			}
		}
	}
}
