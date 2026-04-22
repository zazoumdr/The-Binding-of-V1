using UnityEngine;

public class ChangeLayer : MonoBehaviour
{
	public GameObject target;

	public int layer;

	public float delay;

	public bool includeChildren;

	public bool oneTime = true;

	[HideInInspector]
	public bool activated;

	public bool activateOnEnable = true;

	private void Start()
	{
		if (activateOnEnable)
		{
			Invoke("Change", delay);
		}
	}

	public void Change()
	{
		Change(layer);
	}

	public void Change(int targetLayer)
	{
		if (oneTime && activated)
		{
			return;
		}
		activated = true;
		if (target == null)
		{
			target = base.gameObject;
		}
		if (!includeChildren)
		{
			base.gameObject.layer = targetLayer;
			return;
		}
		Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = targetLayer;
		}
	}
}
