using UnityEngine;

public class ActivateWhenActive : MonoBehaviour
{
	public GameObject[] toTrack;

	public int amountRequiredToActivate = 1;

	[HideInInspector]
	public bool valuesSet;

	private void Awake()
	{
		SetValues();
		Check();
	}

	private void SetValues()
	{
		if (valuesSet)
		{
			return;
		}
		valuesSet = true;
		for (int i = 0; i < toTrack.Length; i++)
		{
			if (!(toTrack[i] == null))
			{
				toTrack[i].AddComponent<ActivateWhenActiveTracker>().target = this;
			}
		}
	}

	public void Check()
	{
		int num = 0;
		for (int i = 0; i < toTrack.Length; i++)
		{
			if (!(toTrack[i] == null) && toTrack[i].activeInHierarchy)
			{
				num++;
			}
		}
		base.gameObject.SetActive(num >= amountRequiredToActivate);
	}
}
