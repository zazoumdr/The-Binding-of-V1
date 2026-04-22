using UnityEngine;

[ExecuteInEditMode]
public class CausticVolume : MonoBehaviour
{
	public Color color = Color.white;

	public float intensity = 1f;

	public float nearRadius = 1f;

	public float farRadius = 2f;

	[HideInInspector]
	public CausticVolumeManager manager;

	private void OnValidate()
	{
		manager = MonoSingleton<CausticVolumeManager>.Instance;
		if (manager == null)
		{
			manager = Object.FindAnyObjectByType<CausticVolumeManager>(FindObjectsInactive.Include);
		}
		if (manager == null)
		{
			GameObject gameObject = new GameObject("CausticVolumeManager");
			manager = gameObject.AddComponent<CausticVolumeManager>();
		}
		manager.AddVolume(this);
	}

	private void OnEnable()
	{
		manager.AddVolume(this);
	}

	private void OnDisable()
	{
		manager.RemoveVolume(this);
	}

	private void OnDestroy()
	{
		if (!(manager == null))
		{
			manager.RemoveVolume(this);
		}
	}
}
