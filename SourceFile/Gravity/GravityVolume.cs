using UnityEngine;

namespace Gravity;

public class GravityVolume : MonoBehaviour
{
	public bool updateContinuously;

	public bool resetOnExit;

	protected int playerRequests;

	public Vector3 GravityVector => base.transform.up * (0f - Physics.gravity.magnitude);

	private void OnDisable()
	{
		playerRequests = 0;
		if (MonoSingleton<NewMovement>.TryGetInstance(out NewMovement instance) && instance.gravityVolumes.Contains(this))
		{
			instance.gravityVolumes.Remove(this);
			instance.CheckGravityVolumes(resetOnExit);
		}
	}

	private void Update()
	{
		if (updateContinuously && playerRequests > 0)
		{
			MonoSingleton<NewMovement>.Instance.SwitchGravity(CalculateGravityVector());
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			if (playerRequests <= 0)
			{
				MonoSingleton<NewMovement>.Instance.SwitchGravity(CalculateGravityVector());
				MonoSingleton<NewMovement>.Instance.gravityVolumes.Add(this);
			}
			playerRequests++;
		}
	}

	protected virtual Vector3 CalculateGravityVector()
	{
		return base.transform.up * (0f - Physics.gravity.magnitude);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			playerRequests--;
			if (playerRequests <= 0)
			{
				MonoSingleton<NewMovement>.Instance.gravityVolumes.Remove(this);
				MonoSingleton<NewMovement>.Instance.CheckGravityVolumes(resetOnExit);
			}
		}
	}

	protected virtual void OnDrawGizmos()
	{
	}
}
