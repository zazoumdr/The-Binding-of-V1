using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilValuesByLayer : MonoBehaviour
{
	public bool applyStencilValue = true;

	public bool applyRainOutdoors;

	public Shader masterShader;

	private Renderer[] rends;

	private List<Material> reusableMaterials = new List<Material>();

	private void Start()
	{
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		Debug.LogWarning("RAN STENCIL SETTER");
		RenderSettings.skybox.SetFloat("_Stencil", 1f);
		StaticSceneOptimizer instance = MonoSingleton<StaticSceneOptimizer>.Instance;
		if (instance != null)
		{
			instance.UpdateRain(applyRainOutdoors);
		}
		rends = Object.FindObjectsOfType<Renderer>(includeInactive: true);
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			if (renderer == null || renderer.gameObject.layer != 24)
			{
				continue;
			}
			renderer.GetMaterials(reusableMaterials);
			bool flag = false;
			foreach (Material reusableMaterial in reusableMaterials)
			{
				if (reusableMaterial == null)
				{
					continue;
				}
				Shader shader = reusableMaterial.shader;
				if (shader == null)
				{
					continue;
				}
				bool flag2 = shader == masterShader;
				flag = flag || flag2;
				if (flag2)
				{
					if (applyStencilValue)
					{
						reusableMaterial.SetFloat("_Stencil", 1f);
					}
					if (applyRainOutdoors)
					{
						reusableMaterial.EnableKeyword("RAIN");
					}
				}
			}
			if (flag)
			{
				renderer.SetMaterials(reusableMaterials);
			}
		}
	}

	public void EnableRain(bool doEnable)
	{
		if (MonoSingleton<StaticSceneOptimizer>.Instance != null)
		{
			MonoSingleton<StaticSceneOptimizer>.Instance.UpdateRain(applyRainOutdoors);
		}
		Renderer[] array = rends;
		foreach (Renderer renderer in array)
		{
			if (renderer == null)
			{
				continue;
			}
			renderer.GetSharedMaterials(reusableMaterials);
			bool flag = false;
			foreach (Material reusableMaterial in reusableMaterials)
			{
				if (reusableMaterial == null)
				{
					continue;
				}
				Shader shader = reusableMaterial.shader;
				if (shader == null)
				{
					continue;
				}
				bool flag2 = shader == masterShader;
				flag = flag || flag2;
				if (flag2 && renderer.gameObject.layer == 24)
				{
					if (doEnable)
					{
						reusableMaterial.EnableKeyword("RAIN");
					}
					else
					{
						reusableMaterial.DisableKeyword("RAIN");
					}
				}
			}
			if (flag)
			{
				renderer.SetSharedMaterials(reusableMaterials);
			}
		}
	}
}
