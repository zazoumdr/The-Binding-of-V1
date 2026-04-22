using System.Collections.Generic;
using UnityEngine;

public class LightPillar : MonoBehaviour
{
	[HideInInspector]
	public bool gotValues;

	[HideInInspector]
	public bool activated;

	[HideInInspector]
	public bool completed;

	[HideInInspector]
	public Light[] lights;

	[HideInInspector]
	public AudioSource aud;

	[HideInInspector]
	public Vector3 origScale;

	[HideInInspector]
	public float lightRange;

	[HideInInspector]
	public float origPitch;

	public float speed;

	public GameObject tipGlow;

	public Renderer[] emissivesToLightUp;

	[HideInInspector]
	public List<float> emissiveStrengths = new List<float>();

	private MaterialPropertyBlock block;

	private bool heightDone;

	public UltrakillEvent onHeightDone;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		if (activated || completed)
		{
			return;
		}
		if (!gotValues)
		{
			gotValues = true;
			lights = GetComponentsInChildren<Light>();
			aud = GetComponentInChildren<AudioSource>();
			origScale = base.transform.localScale;
			origPitch = aud.GetPitch() + Random.Range(-0.1f, 0.1f);
			if (lights.Length != 0)
			{
				lightRange = lights[0].range;
				Light[] array = lights;
				foreach (Light light in array)
				{
					if (light.gameObject != tipGlow)
					{
						light.range = 0f;
					}
				}
			}
			for (int j = 0; j < emissivesToLightUp.Length; j++)
			{
				for (int k = 0; k < emissivesToLightUp[j].sharedMaterials.Length; k++)
				{
					if (emissivesToLightUp[j].sharedMaterials[k].HasFloat(UKShaderProperties.EmissiveIntensity))
					{
						emissiveStrengths.Add(emissivesToLightUp[j].sharedMaterials[k].GetFloat(UKShaderProperties.EmissiveIntensity));
						emissivesToLightUp[j].GetPropertyBlock(block, k);
						block.SetFloat(UKShaderProperties.EmissiveIntensity, 0f);
						emissivesToLightUp[j].SetPropertyBlock(block, k);
					}
				}
			}
		}
		else if (emissivesToLightUp.Length != 0)
		{
			SetEmissives(0f);
		}
		aud.SetPitch(0f);
		base.transform.localScale = Vector3.zero;
	}

	private void Update()
	{
		if (!activated)
		{
			return;
		}
		if (!heightDone)
		{
			base.transform.localScale = new Vector3(origScale.x / 10f, base.transform.localScale.y + Mathf.Min(speed * 5f, speed * (origScale.y - base.transform.localScale.y) + 0.1f) * Time.deltaTime, origScale.z / 10f);
			if (!(base.transform.localScale.y > origScale.y - 0.1f))
			{
				return;
			}
			base.transform.localScale = new Vector3(origScale.x / 10f, origScale.y, origScale.z / 10f);
			heightDone = true;
			tipGlow.SetActive(value: false);
			onHeightDone?.Invoke();
		}
		if (base.transform.localScale != origScale)
		{
			base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, origScale, Mathf.Min(speed, speed * Vector3.Distance(base.transform.localScale, origScale) + 0.01f) * Time.deltaTime);
		}
		if (lights != null && lights.Length != 0 && lights[0].range != lightRange)
		{
			Light[] array = lights;
			foreach (Light obj in array)
			{
				obj.range = Mathf.MoveTowards(obj.range, lightRange, speed * 4f * Time.deltaTime);
			}
			if (emissivesToLightUp.Length != 0)
			{
				float emissives = lights[0].range / lightRange;
				SetEmissives(emissives);
			}
		}
		if (aud.GetPitch() != origPitch)
		{
			aud.SetPitch(Mathf.MoveTowards(aud.GetPitch(), origPitch, speed / 3f * origPitch * Time.deltaTime));
		}
		else if (base.transform.localScale == origScale && lights[0].range == lightRange)
		{
			activated = false;
			completed = true;
		}
	}

	private void SetEmissives(float lerpAmount)
	{
		int num = 0;
		for (int i = 0; i < emissivesToLightUp.Length; i++)
		{
			for (int j = 0; j < emissivesToLightUp[i].materials.Length; j++)
			{
				emissivesToLightUp[i].GetPropertyBlock(block, j);
				if (block.HasFloat(UKShaderProperties.EmissiveIntensity))
				{
					block.SetFloat(UKShaderProperties.EmissiveIntensity, Mathf.Lerp(0f, emissiveStrengths[num], lerpAmount));
					emissivesToLightUp[i].SetPropertyBlock(block, j);
					num++;
				}
			}
		}
	}

	public void ActivatePillar()
	{
		activated = true;
		tipGlow.SetActive(value: true);
	}
}
