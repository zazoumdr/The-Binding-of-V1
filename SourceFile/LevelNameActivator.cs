using UnityEngine;

public class LevelNameActivator : MonoBehaviour
{
	private bool activateOnCollision;

	public float delay;

	[HideInInspector]
	public bool activated;

	public bool force;

	public bool customName;

	public string layerName;

	public string levelName;

	private void Start()
	{
		if ((!TryGetComponent<Collider>(out var component) || !component.isTrigger) && !TryGetComponent<Rigidbody>(out var _))
		{
			Invoke("GoTime", delay);
		}
		else
		{
			activateOnCollision = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (activateOnCollision && other.gameObject.CompareTag("Player"))
		{
			Invoke("GoTime", delay);
		}
	}

	private void GoTime()
	{
		if (!activated)
		{
			activated = true;
			if (customName)
			{
				MonoSingleton<LevelNamePopup>.Instance.CustomNameAppear(layerName, levelName);
			}
			else if (force)
			{
				MonoSingleton<LevelNamePopup>.Instance.NameAppearForce();
			}
			else
			{
				MonoSingleton<LevelNamePopup>.Instance.NameAppear();
			}
			Object.Destroy(this);
		}
	}
}
