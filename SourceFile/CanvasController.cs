using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class CanvasController : MonoSingleton<CanvasController>
{
	public Crosshair crosshair { get; private set; }

	private void Awake()
	{
		if ((bool)MonoSingleton<CanvasController>.Instance && MonoSingleton<CanvasController>.Instance != this)
		{
			Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			crosshair = GetComponentInChildren<Crosshair>(includeInactive: true);
		}
	}

	private void OnEnable()
	{
		base.transform.SetParent(null);
	}
}
