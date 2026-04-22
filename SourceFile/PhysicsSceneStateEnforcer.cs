using UnityEngine;

public class PhysicsSceneStateEnforcer : MonoBehaviour
{
	private GameObject matchingObject;

	public void SetMatchingObject(GameObject matchingObject)
	{
		this.matchingObject = matchingObject;
		SetMatchingObjectActive(base.gameObject.activeInHierarchy);
	}

	private void OnEnable()
	{
		SetMatchingObjectActive(active: true);
	}

	private void OnDisable()
	{
		SetMatchingObjectActive(active: false);
	}

	private void OnDestroy()
	{
		DestroyMatchingObject();
	}

	private void SetMatchingObjectActive(bool active)
	{
		if ((bool)matchingObject && matchingObject.activeSelf != active)
		{
			matchingObject.SetActive(active);
		}
	}

	private void DestroyMatchingObject()
	{
		if (matchingObject != null)
		{
			Object.Destroy(matchingObject);
		}
	}

	public void ForceUpdate()
	{
		if (!(matchingObject == null) && base.gameObject.activeInHierarchy)
		{
			matchingObject.SetActive(value: false);
			matchingObject.SetActive(value: true);
		}
	}
}
