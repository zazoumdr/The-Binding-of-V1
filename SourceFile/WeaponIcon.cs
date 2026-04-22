using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WeaponIcon : MonoBehaviour
{
	[FormerlySerializedAs("descriptor")]
	public WeaponDescriptor weaponDescriptor;

	[SerializeField]
	private Renderer[] variationColoredRenderers;

	[SerializeField]
	private Image[] variationColoredImages;

	private int variationColor
	{
		get
		{
			if (!(weaponDescriptor == null))
			{
				return (int)weaponDescriptor.variationColor;
			}
			return -1;
		}
	}

	private void Start()
	{
		UpdateIcon();
	}

	private void OnEnable()
	{
		MonoSingleton<GunControl>.Instance.currentWeaponIcons.Add(this);
		UpdateIcon();
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			MonoSingleton<GunControl>.Instance.currentWeaponIcons.Remove(this);
		}
	}

	private void OnDestroy()
	{
		if (base.gameObject.scene.isLoaded)
		{
			MonoSingleton<GunControl>.Instance.currentWeaponIcons.Remove(this);
		}
	}

	public void UpdateIcon()
	{
		if ((bool)MonoSingleton<WeaponHUD>.Instance)
		{
			MonoSingleton<WeaponHUD>.Instance.UpdateImage(weaponDescriptor.icon, weaponDescriptor.glowIcon, variationColor);
		}
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer[] array = variationColoredRenderers;
		foreach (Renderer obj in array)
		{
			obj.GetPropertyBlock(materialPropertyBlock);
			if (obj.sharedMaterial.HasProperty("_EmissiveColor"))
			{
				materialPropertyBlock.SetColor("_EmissiveColor", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
			}
			else
			{
				materialPropertyBlock.SetColor("_Color", MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor]);
			}
			obj.SetPropertyBlock(materialPropertyBlock);
		}
		Image[] array2 = variationColoredImages;
		foreach (Image val in array2)
		{
			((Graphic)val).color = new Color(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].r, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].g, MonoSingleton<ColorBlindSettings>.Instance.variationColors[variationColor].b, ((Graphic)val).color.a);
		}
	}
}
