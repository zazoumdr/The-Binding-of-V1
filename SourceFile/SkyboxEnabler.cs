using UnityEngine;

public class SkyboxEnabler : MonoBehaviour
{
	public bool disable;

	public bool oneTime;

	public bool dontActivateOnEnable;

	private bool activated;

	public Material changeSkybox;

	private void OnEnable()
	{
		if (!dontActivateOnEnable)
		{
			Activate();
		}
	}

	public void Activate()
	{
		if (!oneTime || !activated)
		{
			activated = true;
			if (MonoSingleton<CameraController>.Instance.TryGetComponent<Camera>(out var component))
			{
				component.clearFlags = ((!disable) ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			}
			if ((bool)changeSkybox)
			{
				RenderSettings.skybox = new Material(changeSkybox);
			}
		}
	}
}
