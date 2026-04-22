using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostMode : MonoBehaviour
{
	public GameObject ghostGroup;

	public GameObject ghostLights;

	private GameObject duplicateGhosts;

	private List<GhostDrone> ghostDrones;

	private bool isInGhostMode;

	public GameObject insideLightsGroup;

	public Light[] otherLights;

	private List<Light> lightsToDim = new List<Light>();

	private List<float> defaultIntensities = new List<float>();

	private Coroutine crt;

	private Color defaultAmbientColor;

	public Color dimmedAmbientColor;

	public Renderer godRays;

	private Color defaultRayTint;

	public UltrakillEvent onFinish;

	private void Start()
	{
		lightsToDim.AddRange(insideLightsGroup.GetComponentsInChildren<Light>());
		lightsToDim.AddRange(otherLights);
		defaultRayTint = godRays.sharedMaterial.GetColor("_TintColor");
		defaultAmbientColor = RenderSettings.ambientLight;
		foreach (Light item in lightsToDim)
		{
			defaultIntensities.Add(item.intensity);
		}
	}

	public void StartGhostMode()
	{
		if (crt == null)
		{
			crt = StartCoroutine(RunGhostMode());
		}
	}

	private IEnumerator RunGhostMode()
	{
		ghostLights.SetActive(value: true);
		duplicateGhosts = Object.Instantiate(ghostGroup, ghostGroup.transform.position, ghostGroup.transform.rotation);
		ghostDrones = duplicateGhosts.GetComponentsInChildren<GhostDrone>().ToList();
		duplicateGhosts.SetActive(value: true);
		isInGhostMode = true;
		float time = 0f;
		MaterialPropertyBlock props = new MaterialPropertyBlock();
		while (time < 1f)
		{
			time += Time.deltaTime;
			for (int i = 0; i < lightsToDim.Count; i++)
			{
				lightsToDim[i].intensity = Mathf.Lerp(defaultIntensities[i], 0f, time);
			}
			RenderSettings.ambientLight = Color.Lerp(defaultAmbientColor, dimmedAmbientColor, time);
			Color value = Color.Lerp(defaultRayTint, defaultRayTint * 0.25f, time);
			props.SetColor("_TintColor", value);
			godRays.SetPropertyBlock(props);
			yield return null;
		}
		while (ghostDrones.Count > 0)
		{
			for (int num = ghostDrones.Count - 1; num >= 0; num--)
			{
				GhostDrone ghostDrone = ghostDrones[num];
				if (ghostDrone == null)
				{
					ghostDrones.Remove(ghostDrone);
				}
			}
			yield return null;
		}
		StartCoroutine(EndGhostMode());
	}

	private IEnumerator EndGhostMode()
	{
		float time = 0f;
		MaterialPropertyBlock props = new MaterialPropertyBlock();
		while (time < 1f)
		{
			time += Time.deltaTime;
			for (int i = 0; i < lightsToDim.Count; i++)
			{
				lightsToDim[i].intensity = Mathf.Lerp(0f, defaultIntensities[i], time);
			}
			RenderSettings.ambientLight = Color.Lerp(dimmedAmbientColor, defaultAmbientColor, time);
			Color value = Color.Lerp(defaultRayTint * 0.25f, defaultRayTint, time);
			props.SetColor("_TintColor", value);
			godRays.SetPropertyBlock(props);
			yield return null;
		}
		GameProgressSaver.SetGhostDroneModeUnlocked(unlocked: true);
		MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>DRONE HAUNTING</color> CHEAT UNLOCKED");
		onFinish.Invoke();
	}

	public void ResetOnRespawn()
	{
		if (isInGhostMode)
		{
			ghostLights.SetActive(value: false);
			if (crt != null)
			{
				StopCoroutine(crt);
			}
			crt = null;
			for (int i = 0; i < lightsToDim.Count; i++)
			{
				lightsToDim[i].intensity = defaultIntensities[i];
			}
			RenderSettings.ambientLight = defaultAmbientColor;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetColor("_TintColor", defaultRayTint);
			godRays.SetPropertyBlock(materialPropertyBlock);
			foreach (GhostDrone ghostDrone in ghostDrones)
			{
				if (ghostDrone != null)
				{
					Object.Destroy(ghostDrone.gameObject);
				}
			}
		}
		isInGhostMode = false;
	}
}
