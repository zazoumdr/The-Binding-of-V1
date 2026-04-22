using UnityEngine;

public class OutdoorsZone : MonoBehaviour
{
	private int hasRequested;

	public bool ignoreCheckers;

	private void Start()
	{
		if (!MonoSingleton<OutdoorLightMaster>.Instance || ignoreCheckers)
		{
			return;
		}
		Collider component2;
		if (TryGetComponent<Rigidbody>(out var component))
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if ((bool)collider.attachedRigidbody && collider.attachedRigidbody == component)
				{
					MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZonesCheckerable.Add(collider);
				}
			}
		}
		else if (TryGetComponent<Collider>(out component2) && (bool)MonoSingleton<OutdoorLightMaster>.Instance && !MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZonesCheckerable.Contains(component2))
		{
			MonoSingleton<OutdoorLightMaster>.Instance.outdoorsZonesCheckerable.Add(component2);
		}
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<OutdoorLightMaster>.Instance && hasRequested > 0)
		{
			for (int num = hasRequested; num > 0; num--)
			{
				MonoSingleton<OutdoorLightMaster>.Instance.RemoveRequest();
			}
			hasRequested = 0;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)MonoSingleton<OutdoorLightMaster>.Instance && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			if (hasRequested == 0)
			{
				MonoSingleton<OutdoorLightMaster>.Instance.AddRequest();
			}
			hasRequested++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((bool)MonoSingleton<OutdoorLightMaster>.Instance && other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			if (hasRequested == 1)
			{
				MonoSingleton<OutdoorLightMaster>.Instance.RemoveRequest();
			}
			hasRequested--;
		}
	}
}
