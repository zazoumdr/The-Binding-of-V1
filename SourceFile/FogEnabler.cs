using UnityEngine;

public class FogEnabler : MonoBehaviour
{
	public bool disable;

	public bool oneTime;

	private bool activated;

	private bool colliderless;

	public bool gradual;

	public float gradualFadeSpeed = 10f;

	[Space]
	public bool changeFogSettings;

	public Color fogColor;

	public float fogMinimum;

	public float fogMaximum;

	[Space]
	public bool changeLimboSkyboxFogSettings;

	public Color limboSkyboxFogColor;

	public float limboSkyboxFogMinimum;

	public float limboSkyboxFogMaximum;

	private void Awake()
	{
		if (!TryGetComponent<Collider>(out var _) && !TryGetComponent<Rigidbody>(out var _))
		{
			colliderless = true;
		}
	}

	private void OnEnable()
	{
		if (colliderless)
		{
			Activate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform == MonoSingleton<NewMovement>.Instance.transform)
		{
			Activate();
		}
	}

	public void Activate()
	{
		if (oneTime && activated)
		{
			return;
		}
		activated = true;
		if (changeFogSettings)
		{
			RenderSettings.fogColor = fogColor;
			if (!gradual)
			{
				RenderSettings.fogStartDistance = fogMinimum;
				RenderSettings.fogEndDistance = fogMaximum;
			}
		}
		if (gradual)
		{
			if (disable)
			{
				MonoSingleton<FogFadeController>.Instance.FadeOut(autoDetectFogChange: true, gradualFadeSpeed);
			}
			else if (changeFogSettings)
			{
				MonoSingleton<FogFadeController>.Instance.FadeIn(fogMinimum, fogMaximum, autoDetectFogChange: true, gradualFadeSpeed);
			}
			else
			{
				MonoSingleton<FogFadeController>.Instance.FadeIn(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance, autoDetectFogChange: true, gradualFadeSpeed);
			}
		}
		else
		{
			RenderSettings.fog = !disable;
		}
		if (!changeLimboSkyboxFogSettings)
		{
			return;
		}
		LimboSkybox[] array = Object.FindObjectsOfType<LimboSkybox>(includeInactive: true);
		if (array != null && array.Length != 0)
		{
			LimboSkybox[] array2 = array;
			foreach (LimboSkybox obj in array2)
			{
				obj.fogColor = limboSkyboxFogColor;
				obj.fogStart = limboSkyboxFogMinimum;
				obj.fogEnd = limboSkyboxFogMaximum;
			}
		}
	}
}
