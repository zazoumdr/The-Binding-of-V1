using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SelectionRedirector : Selectable
{
	public Selectable[] Selectables;

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		if (Selectables == null)
		{
			return;
		}
		Selectable[] selectables = Selectables;
		foreach (Selectable val in selectables)
		{
			if ((Object)(object)val != null && ((Behaviour)(object)val).isActiveAndEnabled)
			{
				((MonoBehaviour)this).StartCoroutine(SelectAtEndOfFrame(val));
				break;
			}
		}
	}

	private IEnumerator SelectAtEndOfFrame(Selectable selectable)
	{
		yield return new WaitForEndOfFrame();
		selectable.Select();
	}
}
