using System.Collections;
using UnityEngine;

namespace ULTRAKILL.Cheats;

public class FullBright : ICheat
{
	private static FullBright _instance;

	private bool lastFogEnabled;

	private Color lastAmbientColor;

	private GameObject lightObject;

	private static Color brightAmbientColor = new Color(0.2f, 0.2f, 0.2f);

	public static bool Enabled => _instance?.IsActive ?? false;

	public string LongName => "Fullbright";

	public string Identifier => "ultrakill.full-bright";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "light";

	public bool IsActive { get; private set; }

	public bool DefaultState => false;

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

	public void Enable(CheatsManager manager)
	{
		_instance = this;
		IsActive = true;
		lightObject = Object.Instantiate(MonoSingleton<CheatsController>.Instance.fullBrightLight);
		lastFogEnabled = RenderSettings.fog;
		RenderSettings.fog = false;
		lastAmbientColor = RenderSettings.ambientLight;
		RenderSettings.ambientLight = brightAmbientColor;
	}

	public void Disable()
	{
		IsActive = false;
		Object.Destroy(lightObject);
		RenderSettings.fog = lastFogEnabled;
		RenderSettings.ambientLight = lastAmbientColor;
	}

	public IEnumerator Coroutine(CheatsManager manager)
	{
		while (IsActive)
		{
			Update();
			yield return null;
		}
	}

	private void Update()
	{
		if (IsActive)
		{
			if (RenderSettings.fog)
			{
				lastFogEnabled = true;
				RenderSettings.fog = false;
			}
			if (RenderSettings.ambientLight != brightAmbientColor)
			{
				lastAmbientColor = RenderSettings.ambientLight;
				RenderSettings.ambientLight = brightAmbientColor;
			}
		}
	}
}
