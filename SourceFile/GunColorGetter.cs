using UnityEngine;

public class GunColorGetter : MonoBehaviour
{
	private Renderer rend;

	public Material[] defaultMaterials;

	public Material[] coloredMaterials;

	private MaterialPropertyBlock customColors;

	public int weaponNumber;

	public bool altVersion;

	private GunColorPreset currentColors = new GunColorPreset(Color.white, Color.white, Color.white);

	private bool hasCustomColors;

	private void Awake()
	{
		for (int i = 0; i < defaultMaterials.Length; i++)
		{
			defaultMaterials[i] = new Material(defaultMaterials[i]);
		}
		for (int j = 0; j < coloredMaterials.Length; j++)
		{
			coloredMaterials[j] = new Material(coloredMaterials[j]);
		}
	}

	private void OnEnable()
	{
		UpdateColor();
	}

	public void UpdateColor()
	{
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
		}
		if (customColors == null)
		{
			customColors = new MaterialPropertyBlock();
		}
		hasCustomColors = weaponNumber > 0 && weaponNumber <= MonoSingleton<GunColorController>.Instance.hasUnlockedColors.Length && MonoSingleton<PrefsManager>.Instance.GetBool("gunColorType." + weaponNumber + (altVersion ? ".a" : "")) && MonoSingleton<GunColorController>.Instance.hasUnlockedColors[weaponNumber - 1];
		GunColorPreset colors = GetColors();
		if (currentColors != colors)
		{
			if (GetPreset() != 0 || hasCustomColors)
			{
				rend.materials = coloredMaterials;
				rend.GetPropertyBlock(customColors);
				customColors.SetColor("_CustomColor1", colors.color1);
				customColors.SetColor("_CustomColor2", colors.color2);
				customColors.SetColor("_CustomColor3", colors.color3);
				rend.SetPropertyBlock(customColors);
			}
			else
			{
				rend.materials = defaultMaterials;
			}
		}
		currentColors = colors;
	}

	private int GetPreset()
	{
		GunColorController instance = MonoSingleton<GunColorController>.Instance;
		int num = ((!altVersion) ? instance.presets.Length : instance.altPresets.Length);
		if (weaponNumber > 0 && weaponNumber <= num)
		{
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.altPresets[weaponNumber - 1];
			}
			return MonoSingleton<GunColorController>.Instance.presets[weaponNumber - 1];
		}
		return 0;
	}

	private GunColorPreset GetColors()
	{
		GunColorController instance = MonoSingleton<GunColorController>.Instance;
		int num = ((!altVersion) ? instance.currentColors.Length : instance.currentAltColors.Length);
		if (weaponNumber > 0 && weaponNumber <= num)
		{
			if (altVersion)
			{
				return MonoSingleton<GunColorController>.Instance.currentAltColors[weaponNumber - 1];
			}
			return MonoSingleton<GunColorController>.Instance.currentColors[weaponNumber - 1];
		}
		return new GunColorPreset(Color.white, Color.white, Color.white);
	}
}
