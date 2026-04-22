using ULTRAKILL.Cheats;
using UnityEngine;

public class DisableLightsOnFullBright : MonoBehaviour
{
	private Light[] lights;

	private bool fullBrightActive;

	private void OnEnable()
	{
		fullBrightActive = FullBright.Enabled;
		if (fullBrightActive)
		{
			SetLightsEnabled(isEnabled: false);
		}
	}

	private void Update()
	{
		if (fullBrightActive && !FullBright.Enabled)
		{
			fullBrightActive = false;
			SetLightsEnabled(isEnabled: true);
		}
		else if (!fullBrightActive && FullBright.Enabled)
		{
			fullBrightActive = true;
			SetLightsEnabled(isEnabled: false);
		}
	}

	private void SetLightsEnabled(bool isEnabled)
	{
		if (lights == null)
		{
			lights = GetComponentsInChildren<Light>();
		}
		Light[] array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = isEnabled;
		}
	}
}
