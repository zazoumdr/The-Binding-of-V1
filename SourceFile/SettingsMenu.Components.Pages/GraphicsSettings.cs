using System;
using System.Collections.Generic;
using System.Text;
using SettingsMenu.Models;
using UnityEngine;
using UnityEngine.Events;

namespace SettingsMenu.Components.Pages;

public class GraphicsSettings : SettingsLogicBase
{
	[SerializeField]
	private SettingsItem resolutionItem;

	public static bool simpleNailPhysics = true;

	public static bool bloodEnabled = true;

	private SettingsMenu settingsMenu;

	private int currentResolutionIndex;

	private readonly List<(Resolution, string)> availableResolutions = new List<(Resolution, string)>();

	public static bool disabledComputeShaders;

	public override void Initialize(SettingsMenu settingsMenu)
	{
		this.settingsMenu = settingsMenu;
		int intLocal = MonoSingleton<PrefsManager>.Instance.GetIntLocal("frameRateLimit");
		SetFrameRateLimit(intLocal);
		Screen.fullScreen = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("fullscreen");
		bool boolLocal = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("vSync");
		SetVSync(boolLocal);
		bool boolLocal2 = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions");
		SetSimpleExplosions(boolLocal2);
		int simplifyEnemies = MonoSingleton<PrefsManager>.Instance.GetInt("simplifyEnemies");
		SetSimplifyEnemies(simplifyEnemies);
		float dithering = MonoSingleton<PrefsManager>.Instance.GetFloat("dithering");
		SetDithering(dithering);
		float textureWarping = MonoSingleton<PrefsManager>.Instance.GetFloat("textureWarping");
		SetTextureWarping(textureWarping);
		int vertexWarping = MonoSingleton<PrefsManager>.Instance.GetInt("vertexWarping");
		SetVertexWarping(vertexWarping);
		int colorCompression = MonoSingleton<PrefsManager>.Instance.GetInt("colorCompression");
		SetColorCompression(colorCompression);
		float gamma = MonoSingleton<PrefsManager>.Instance.GetFloat("gamma");
		SetGamma(gamma);
		bool boolLocal3 = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette");
		SetColorPalette(boolLocal3);
		bloodEnabled = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled");
		disabledComputeShaders = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("disabledComputeShaders");
	}

	private void Start()
	{
		InitializeResolutionDropdown();
	}

	private void SetColorPalette(bool value)
	{
		if (!value)
		{
			MonoSingleton<PostProcessV2_Handler>.Instance.ColorPalette(stuff: false);
			return;
		}
		try
		{
			Texture2D texture2D = CustomPaletteSelector.LoadSavedPalette();
			if ((bool)texture2D)
			{
				MonoSingleton<PostProcessV2_Handler>.Instance.ApplyUserColorPalette(texture2D);
				MonoSingleton<PostProcessV2_Handler>.Instance.ColorPalette(stuff: true);
				Shader.SetGlobalInt("_ColorPrecision", 2048);
			}
			else
			{
				MonoSingleton<PostProcessV2_Handler>.Instance.ColorPalette(stuff: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading color palette: " + ex);
			MonoSingleton<PostProcessV2_Handler>.Instance.ColorPalette(stuff: false);
		}
	}

	private void InitializeResolutionDropdown()
	{
		GetAvailableResolutions();
		if (!(resolutionItem == null) && settingsMenu.TryGetItemBuilderInstance<SettingsDropdown>(resolutionItem, out var builder))
		{
			builder.SetDropdownItems(availableResolutions.ConvertAll(((Resolution, string) x) => x.Item2));
			builder.SetDropdownValue(currentResolutionIndex);
			((UnityEvent<int>)(object)builder.onValueChanged).AddListener((UnityAction<int>)SetResolution);
		}
	}

	public override void OnPrefChanged(string key, object value)
	{
		switch (key)
		{
		case "frameRateLimit":
			if (value is int frameRateLimit)
			{
				SetFrameRateLimit(frameRateLimit);
			}
			break;
		case "fullscreen":
			if (value is bool fullScreen)
			{
				Screen.fullScreen = fullScreen;
			}
			break;
		case "vSync":
			if (value is bool vSync)
			{
				SetVSync(vSync);
			}
			break;
		case "simpleExplosions":
			if (value is bool simpleExplosions)
			{
				SetSimpleExplosions(simpleExplosions);
			}
			break;
		case "simplifyEnemies":
			if (value is int simplifyEnemies)
			{
				SetSimplifyEnemies(simplifyEnemies);
			}
			break;
		case "dithering":
			if (value is float dithering)
			{
				SetDithering(dithering);
			}
			break;
		case "textureWarping":
			if (value is float textureWarping)
			{
				SetTextureWarping(textureWarping);
			}
			break;
		case "vertexWarping":
			if (value is int vertexWarping)
			{
				SetVertexWarping(vertexWarping);
			}
			break;
		case "colorCompression":
			if (value is int colorCompression)
			{
				SetColorCompression(colorCompression);
			}
			break;
		case "gamma":
			if (value is float gamma)
			{
				SetGamma(gamma);
			}
			break;
		case "colorPalette":
			if (value is bool colorPalette)
			{
				SetColorPalette(colorPalette);
			}
			break;
		case "colorPaletteTexture":
			SetColorPalette(MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"));
			break;
		case "bloodEnabled":
			if (value is bool flag2)
			{
				bloodEnabled = flag2;
			}
			break;
		case "disabledComputeShaders":
			if (value is bool flag)
			{
				disabledComputeShaders = flag;
			}
			break;
		}
	}

	private void SetGamma(float value)
	{
		Shader.SetGlobalFloat("_Gamma", value);
	}

	private void SetColorCompression(int value)
	{
		if (!MonoSingleton<PrefsManager>.Instance.GetBoolLocal("colorPalette"))
		{
			Shader.SetGlobalFloat("_ColorPrecision", GetColorCompressionValue(value));
		}
	}

	public static float GetPixelizationValue(int option)
	{
		return option switch
		{
			0 => 0f, 
			1 => 720f, 
			2 => 480f, 
			3 => 360f, 
			4 => 240f, 
			5 => 144f, 
			6 => 36f, 
			_ => 0f, 
		};
	}

	public static int GetColorCompressionValue(int option)
	{
		return option switch
		{
			0 => 2048, 
			1 => 64, 
			2 => 32, 
			3 => 16, 
			4 => 8, 
			5 => 3, 
			_ => 2048, 
		};
	}

	public static float GetVertexWarpingValue(int option)
	{
		return option switch
		{
			0 => 0, 
			1 => 400, 
			2 => 160, 
			3 => 80, 
			4 => 40, 
			5 => 16, 
			_ => 0, 
		};
	}

	private void SetTextureWarping(float value)
	{
		Shader.SetGlobalFloat("_TextureWarping", Mathf.Clamp01(value) * 0.5f);
	}

	private void SetVertexWarping(int value)
	{
		Shader.SetGlobalFloat("_StainWarping", value);
		float vertexWarpingValue = GetVertexWarpingValue(value);
		if (vertexWarpingValue != 0f)
		{
			Shader.EnableKeyword("VERTEX_WARPING");
		}
		else
		{
			Shader.DisableKeyword("VERTEX_WARPING");
		}
		Shader.SetGlobalFloat("_VertexWarping", vertexWarpingValue);
	}

	private void SetDithering(float value)
	{
		Shader.SetGlobalFloat("_DitherStrength", value);
	}

	private void GetAvailableResolutions()
	{
		Resolution[] resolutions = Screen.resolutions;
		availableResolutions.Clear();
		currentResolutionIndex = 0;
		HashSet<(int, int)> hashSet = new HashSet<(int, int)>();
		StringBuilder stringBuilder = new StringBuilder(16);
		Resolution[] array = resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution item = array[i];
			if (hashSet.Add((item.width, item.height)))
			{
				stringBuilder.Clear();
				stringBuilder.Append(item.width).Append(" x ").Append(item.height);
				availableResolutions.Add((item, stringBuilder.ToString()));
				if (item.width == Screen.width && item.height == Screen.height)
				{
					currentResolutionIndex = availableResolutions.Count - 1;
				}
			}
		}
		availableResolutions.Sort(delegate((Resolution, string) a, (Resolution, string) b)
		{
			int num = a.Item1.width.CompareTo(b.Item1.width);
			return (num == 0) ? a.Item1.height.CompareTo(b.Item1.height) : num;
		});
	}

	public void SetResolution(int stuff)
	{
		Resolution item = availableResolutions[stuff].Item1;
		Screen.SetResolution(item.width, item.height, Screen.fullScreen);
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("resolutionWidth", item.width);
		MonoSingleton<PrefsManager>.Instance.SetIntLocal("resolutionHeight", item.height);
		MonoSingleton<PrefsManager>.Instance.SetBoolLocal("fullscreen", Screen.fullScreen);
	}

	private void SetFrameRateLimit(int stepValue)
	{
		Application.targetFrameRate = stepValue switch
		{
			0 => -1, 
			1 => (int)(Screen.currentResolution.refreshRateRatio.value * 2.0), 
			2 => 30, 
			3 => 60, 
			4 => 120, 
			5 => 144, 
			6 => 240, 
			7 => 288, 
			_ => Application.targetFrameRate, 
		};
	}

	private void SetVSync(bool value)
	{
		QualitySettings.vSyncCount = (value ? 1 : 0);
	}

	private void SetSimpleExplosions(bool value)
	{
		Physics.IgnoreLayerCollision(23, 9, value);
		Physics.IgnoreLayerCollision(23, 27, value);
	}

	private void SetSimplifyEnemies(int value)
	{
	}
}
