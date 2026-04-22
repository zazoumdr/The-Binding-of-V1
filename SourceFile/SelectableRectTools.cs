using UnityEngine;
using UnityEngine.UI;

public class SelectableRectTools : MonoBehaviour
{
	[SerializeField]
	private Selectable target;

	[SerializeField]
	private bool autoSwitchForDown;

	[SerializeField]
	private bool autoSwitchForUp;

	[SerializeField]
	private Selectable[] prioritySwitch;

	private void Awake()
	{
		if ((Object)(object)target == null)
		{
			target = GetComponent<Selectable>();
		}
	}

	private void OnEnable()
	{
		Selectable[] array;
		if (autoSwitchForDown)
		{
			array = prioritySwitch;
			foreach (Selectable val in array)
			{
				if (((Component)(object)val).gameObject.activeSelf && val.IsInteractable() && ((Behaviour)(object)val).enabled)
				{
					ChangeSelectOnDown(val);
					break;
				}
			}
		}
		if (!autoSwitchForUp)
		{
			return;
		}
		array = prioritySwitch;
		foreach (Selectable val2 in array)
		{
			if (((Component)(object)val2).gameObject.activeSelf && val2.IsInteractable() && ((Behaviour)(object)val2).enabled)
			{
				ChangeSelectOnUp(val2);
				break;
			}
		}
	}

	public void ChangeSelectOnUp(Selectable newElement)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = target.navigation;
		((Navigation)(ref navigation)).selectOnUp = newElement;
		target.navigation = navigation;
	}

	public void ChangeSelectOnDown(Selectable newElement)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = target.navigation;
		((Navigation)(ref navigation)).selectOnDown = newElement;
		target.navigation = navigation;
	}
}
