using UnityEngine;

public class IgnoreCollisionWith : MonoBehaviour
{
	[HideInInspector]
	public bool gotValues;

	[HideInInspector]
	public Collider[] cols;

	[HideInInspector]
	private bool ignoring;

	public Collider[] targets;

	public bool includeOwnChildren;

	private void Awake()
	{
		GetValues();
		Ignore(ignore: true);
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			if (includeOwnChildren)
			{
				cols = GetComponentsInChildren<Collider>();
			}
			else
			{
				cols = GetComponents<Collider>();
			}
		}
	}

	private void OnEnable()
	{
		Ignore(ignore: true);
	}

	private void OnDisable()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Ignore(ignore: false);
		}
	}

	public void Ignore(bool ignore)
	{
		if (!gotValues)
		{
			GetValues();
		}
		if (ignoring == ignore)
		{
			return;
		}
		ignoring = ignore;
		Collider[] array = cols;
		foreach (Collider collider in array)
		{
			if (collider == null)
			{
				continue;
			}
			Collider[] array2 = targets;
			foreach (Collider collider2 in array2)
			{
				if (!(collider2 == null))
				{
					Physics.IgnoreCollision(collider, collider2, ignore);
				}
			}
		}
	}
}
