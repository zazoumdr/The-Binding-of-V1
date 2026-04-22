using UnityEngine;
using UnityEngine.UI;

public class ShopButtonListController : MonoBehaviour
{
	[SerializeField]
	private bool resetOnEnable = true;

	[SerializeField]
	private Button[] buttons;

	private void Start()
	{
		Button[] array = buttons;
		foreach (Button button in array)
		{
			((Component)(object)button).GetComponent<ShopButton>().PointerClickSuccess += delegate
			{
				SetActiveButton(button);
			};
		}
	}

	private void SetActiveButton(Button specButton)
	{
		Button[] array = buttons;
		foreach (Button val in array)
		{
			if ((Object)(object)val == (Object)(object)specButton)
			{
				((Selectable)val).interactable = false;
				if (((Component)(object)val).TryGetComponent(out ShopButton component))
				{
					component.deactivated = true;
				}
			}
			else
			{
				((Selectable)val).interactable = true;
				if (((Component)(object)val).TryGetComponent(out ShopButton component2))
				{
					component2.deactivated = false;
				}
			}
		}
	}

	public void ResetButtons()
	{
		SetActiveButton(null);
	}
}
