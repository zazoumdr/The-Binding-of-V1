using System.Collections.Generic;
using UnityEngine;

public class SetChildShaderPropertiesOnPref : MonoBehaviour
{
	private List<Renderer> renderers;

	public bool localPref;

	public string prefName;

	[Space]
	public ShaderProperty[] onTrue;

	public ShaderProperty[] onFalse;

	private void Awake()
	{
		renderers = new List<Renderer>();
		renderers.AddRange(GetComponentsInChildren<Renderer>(includeInactive: true));
	}

	private void OnEnable()
	{
		ShaderProperty[] array;
		if (localPref ? MonoSingleton<PrefsManager>.Instance.GetBoolLocal(prefName) : MonoSingleton<PrefsManager>.Instance.GetBool(prefName))
		{
			array = onTrue;
			foreach (ShaderProperty shaderProperty in array)
			{
				foreach (Renderer renderer in renderers)
				{
					shaderProperty.Set(renderer.material);
				}
			}
			return;
		}
		array = onFalse;
		foreach (ShaderProperty shaderProperty2 in array)
		{
			foreach (Renderer renderer2 in renderers)
			{
				shaderProperty2.Set(renderer2.material);
			}
		}
	}
}
