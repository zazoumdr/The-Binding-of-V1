using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuNavigationBuilder : MonoBehaviour
{
	[SerializeField]
	private Selectable topSelectable;

	[SerializeField]
	private Selectable[] selectables;

	[SerializeField]
	private Selectable bottomSelectable;

	[SerializeField]
	private bool loopAround = true;

	private GamepadObjectSelector gamepadObjectSelector;

	private void Start()
	{
		gamepadObjectSelector = GetComponent<GamepadObjectSelector>();
		StartCoroutine(BuildNavigationDelayed());
	}

	private IEnumerator BuildNavigationDelayed()
	{
		yield return null;
		BuildNavigation();
	}

	private void BuildNavigation()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		List<Selectable> list = new List<Selectable>();
		if ((Object)(object)topSelectable != null)
		{
			list.Add(topSelectable);
		}
		Selectable[] array = selectables;
		foreach (Selectable val in array)
		{
			if (!((Object)(object)val == null) && ((Component)(object)val).gameObject.activeInHierarchy)
			{
				list.Add(val);
			}
		}
		if ((Object)(object)bottomSelectable != null)
		{
			list.Add(bottomSelectable);
		}
		if (list.Count == 0)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			Navigation navigation = list[j].navigation;
			((Navigation)(ref navigation)).mode = (Mode)4;
			if (j > 0)
			{
				((Navigation)(ref navigation)).selectOnUp = list[j - 1];
			}
			else if (loopAround)
			{
				((Navigation)(ref navigation)).selectOnUp = list[list.Count - 1];
			}
			if (j < list.Count - 1)
			{
				((Navigation)(ref navigation)).selectOnDown = list[j + 1];
			}
			else if (loopAround)
			{
				((Navigation)(ref navigation)).selectOnDown = list[0];
			}
			list[j].navigation = navigation;
		}
		foreach (Selectable item in list)
		{
			Transform parent = ((Component)(object)item).transform.parent;
			if (!(parent == null))
			{
				SettingsRestoreDefaultButton componentInChildren = parent.GetComponentInChildren<SettingsRestoreDefaultButton>(includeInactive: true);
				if (!(componentInChildren == null) && !(componentInChildren.transform.parent != parent))
				{
					componentInChildren.SetNavigation(item);
				}
			}
		}
		if (gamepadObjectSelector != null)
		{
			gamepadObjectSelector.SetMainTarget(list[0]);
		}
	}
}
