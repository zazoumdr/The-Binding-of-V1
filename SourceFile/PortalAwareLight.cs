using ULTRAKILL.Portal;
using UnityEngine;

public class PortalAwareLight : MonoBehaviour
{
	public PortalRenderV2.PortalLightType portalLightType;

	private PortalRenderV2 pRend;

	private Light thisLight;

	private void Start()
	{
		thisLight = GetComponent<Light>();
		PortalManagerV2 instance;
		if (thisLight.type == LightType.Directional)
		{
			Object.Destroy(this);
		}
		else if (!MonoSingleton<PortalManagerV2>.TryGetInstance(out instance))
		{
			Object.Destroy(this);
		}
		else
		{
			pRend = instance.render;
		}
	}

	private void LateUpdate()
	{
		if (MonoSingleton<PortalManagerV2>.Instance.portalCount == 0 || !thisLight || !thisLight.isActiveAndEnabled || (bool)thisLight.GetComponentInParent<PlayerMovementParenting>())
		{
			return;
		}
		if (!pRend)
		{
			PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
			if (instance == null)
			{
				return;
			}
			pRend = instance.render;
			if (!pRend)
			{
				return;
			}
		}
		pRend.AddPortalAwareLight(thisLight, portalLightType);
	}
}
