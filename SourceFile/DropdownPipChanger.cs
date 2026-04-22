using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownPipChanger : MonoBehaviour
{
	private TMP_Dropdown dropdown;

	[SerializeField]
	private Image pip;

	private Sprite defaultSprite;

	[SerializeField]
	private Sprite openedSprite;

	private void Awake()
	{
		dropdown = GetComponent<TMP_Dropdown>();
		defaultSprite = pip.sprite;
	}

	private void Update()
	{
		pip.sprite = (dropdown.IsExpanded ? openedSprite : defaultSprite);
	}
}
