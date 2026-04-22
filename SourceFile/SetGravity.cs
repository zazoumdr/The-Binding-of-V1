using UnityEngine;

public class SetGravity : MonoBehaviour
{
	public Vector3 target = Vector3.down * 40f;

	public bool oneTime;

	[HideInInspector]
	public bool activated;

	public bool activateOnEnable;

	public bool deactivateOnDisable;

	private void OnEnable()
	{
		if (activateOnEnable)
		{
			Activate();
		}
	}

	private void OnDisable()
	{
		if (deactivateOnDisable)
		{
			Revert();
		}
	}

	public void Activate()
	{
		if (!oneTime || !activated)
		{
			activated = true;
			Physics.gravity = target;
			MonoSingleton<NewMovement>.Instance.SwitchGravity(Physics.gravity);
		}
	}

	public void Revert()
	{
		Physics.gravity = Vector3.down * 40f;
		if (MonoSingleton<NewMovement>.TryGetInstance(out NewMovement instance) && instance.rb != null)
		{
			instance.SwitchGravity(Physics.gravity);
		}
	}
}
