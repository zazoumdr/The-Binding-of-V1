using UnityEngine;

public class BattleDimmingLights : MonoBehaviour
{
	private Light[] lights;

	private float[] intensities;

	private float lerp = 1f;

	public float speedMultiplier = 1f;

	public bool disabledUnlessAlwaysDark;

	[Header("Ambient Color")]
	public bool dimAmbientLight;

	private Color originalAmbientLightColor;

	public Color dimmedAmbientLightColor;

	private void Start()
	{
		lights = GetComponentsInChildren<Light>();
		intensities = new float[lights.Length];
		for (int i = 0; i < lights.Length; i++)
		{
			intensities[i] = lights[i].intensity;
		}
		originalAmbientLightColor = RenderSettings.ambientLight;
	}

	private void Update()
	{
		if (MonoSingleton<PrefsManager>.Instance.GetBool("level_7-1.alwaysDark"))
		{
			for (int i = 0; i < lights.Length; i++)
			{
				lights[i].intensity = 0f;
			}
			if (dimAmbientLight)
			{
				RenderSettings.ambientLight = dimmedAmbientLightColor;
			}
		}
		else if (!disabledUnlessAlwaysDark && (MonoSingleton<MusicManager>.Instance.IsInBattle() || lerp < 1f))
		{
			lerp = Mathf.MoveTowards(lerp, (!MonoSingleton<MusicManager>.Instance.IsInBattle()) ? 1 : 0, Time.deltaTime * speedMultiplier);
			for (int j = 0; j < lights.Length; j++)
			{
				lights[j].intensity = Mathf.Lerp(0f, intensities[j], lerp);
			}
			if (dimAmbientLight)
			{
				RenderSettings.ambientLight = Color.Lerp(dimmedAmbientLightColor, originalAmbientLightColor, lerp);
			}
		}
	}

	public void Active(bool stuff)
	{
		disabledUnlessAlwaysDark = !stuff;
	}
}
