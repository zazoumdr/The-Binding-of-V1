using ULTRAKILL.Cheats;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HideUICheatGroup : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private void Awake()
	{
		if (TryGetComponent<CanvasGroup>(out canvasGroup))
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
			((Behaviour)(object)canvasGroup).enabled = false;
		}
	}

	private void Update()
	{
		if (!((Object)(object)canvasGroup == null))
		{
			((Behaviour)(object)canvasGroup).enabled = HideUI.Active;
		}
	}
}
