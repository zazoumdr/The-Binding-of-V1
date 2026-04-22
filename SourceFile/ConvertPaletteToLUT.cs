using SettingsMenu.Components.Pages;
using UnityEngine;
using UnityEngine.Rendering;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class ConvertPaletteToLUT : MonoSingleton<ConvertPaletteToLUT>
{
	public RenderTexture processedLUT;

	public RenderTexture lastLUT;

	public Texture2D lastPalette;

	private Material fallbackMaterial;

	public void ApplyLastPalette()
	{
		Shader.EnableKeyword("PALETTIZE");
		Shader.SetGlobalInt("_ColorPrecision", 2048);
		Shader.SetGlobalTexture("_LUT", processedLUT);
	}

	public void ConvertPalette(Texture2D inputPalette, ComputeShader paletteCompute, Shader paletteCalc)
	{
		if (!processedLUT)
		{
			processedLUT = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, 0);
			processedLUT.dimension = TextureDimension.Tex3D;
			processedLUT.volumeDepth = 256;
			processedLUT.enableRandomWrite = true;
		}
		Shader.SetGlobalTexture("_LUT", processedLUT);
		bool flag = true;
		if ((bool)lastPalette)
		{
			flag = lastPalette.name != inputPalette.name;
		}
		if (!flag)
		{
			return;
		}
		lastPalette = inputPalette;
		if (SettingsMenu.Components.Pages.GraphicsSettings.disabledComputeShaders)
		{
			if (!fallbackMaterial)
			{
				fallbackMaterial = new Material(paletteCalc);
			}
			fallbackMaterial.SetTexture("_PaletteTex", inputPalette);
			for (int i = 0; i < processedLUT.volumeDepth; i++)
			{
				float value = (float)i / 255f;
				fallbackMaterial.SetFloat("_Slice", value);
				Graphics.SetRenderTarget(processedLUT, 0, CubemapFace.PositiveX, i);
				Graphics.Blit(null, fallbackMaterial);
			}
		}
		else
		{
			paletteCompute.SetTexture(0, "_PaletteTex", inputPalette);
			paletteCompute.SetTexture(0, "Result", processedLUT);
			paletteCompute.Dispatch(0, 32, 32, 32);
		}
		lastLUT = new RenderTexture(processedLUT);
		processedLUT.antiAliasing = 1;
		processedLUT.filterMode = FilterMode.Point;
		lastLUT.antiAliasing = 1;
		lastLUT.filterMode = FilterMode.Point;
	}
}
