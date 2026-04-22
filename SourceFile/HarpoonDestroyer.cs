using UnityEngine;

public class HarpoonDestroyer : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		if (!(col == null) && !(col.attachedRigidbody == null) && col.attachedRigidbody.TryGetComponent<Harpoon>(out var component))
		{
			Object.Destroy(component.gameObject);
		}
	}
}
