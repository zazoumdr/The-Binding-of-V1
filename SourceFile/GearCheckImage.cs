using UnityEngine;
using UnityEngine.UI;

public class GearCheckImage : MonoBehaviour
{
	public string gearName;

	private Image image;

	private Sprite originalSprite;

	[SerializeField]
	private Sprite lockedSprite;

	private void Awake()
	{
		if (!(Object)(object)image)
		{
			image = GetComponent<Image>();
		}
		if (!originalSprite)
		{
			originalSprite = image.sprite;
		}
	}

	private void OnEnable()
	{
		if (GameProgressSaver.CheckGear(gearName) == 0)
		{
			image.sprite = lockedSprite;
		}
		else
		{
			image.sprite = originalSprite;
		}
	}
}
