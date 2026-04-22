using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
	public ItemType targetType;

	public bool oneTime;

	private bool activated;

	public bool disableOnExit;

	public bool disableActivator;

	public bool destroyActivator;

	public bool dontRequireItemLayer;

	public UltrakillEvent onEvent;

	private int requests;

	private void OnDisable()
	{
		requests = 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((oneTime && activated) || (!dontRequireItemLayer && other.gameObject.layer != 22) || !(other.attachedRigidbody ? other.attachedRigidbody.TryGetComponent<ItemIdentifier>(out var component) : other.TryGetComponent<ItemIdentifier>(out component)) || component.itemType != targetType)
		{
			return;
		}
		if (requests == 0)
		{
			activated = true;
			onEvent?.Invoke(base.gameObject.name);
			if (destroyActivator)
			{
				Object.Destroy(component.gameObject);
			}
			else if (disableActivator)
			{
				component.gameObject.SetActive(value: false);
			}
		}
		if (!destroyActivator && !disableActivator)
		{
			requests++;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (requests <= 0 || (!dontRequireItemLayer && other.gameObject.layer != 22) || !(other.attachedRigidbody ? other.attachedRigidbody.TryGetComponent<ItemIdentifier>(out var component) : other.TryGetComponent<ItemIdentifier>(out component)) || component.itemType != targetType)
		{
			return;
		}
		requests--;
		if (requests == 0 && disableOnExit)
		{
			onEvent?.Revert();
			if (destroyActivator)
			{
				Object.Destroy(component.gameObject);
			}
			else if (disableActivator)
			{
				component.gameObject.SetActive(value: false);
			}
		}
	}
}
