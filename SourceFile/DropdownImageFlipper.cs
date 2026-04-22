using UnityEngine;
using UnityEngine.UI;

public class DropdownImageFlipper : MonoBehaviour
{
	[SerializeField]
	private Image targetImage;

	[SerializeField]
	private Sprite flippedSprite;

	private RectTransform rect;

	private void Awake()
	{
		if (!(Object)(object)targetImage)
		{
			targetImage = GetComponent<Image>();
		}
		rect = GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (rect.pivot.y < 0.5f)
		{
			targetImage.sprite = flippedSprite;
			float y = ((Graphic)targetImage).rectTransform.offsetMin.y;
			((Graphic)targetImage).rectTransform.offsetMin = new Vector2(((Graphic)targetImage).rectTransform.offsetMin.x, 0f - ((Graphic)targetImage).rectTransform.offsetMax.y);
			((Graphic)targetImage).rectTransform.offsetMax = new Vector2(((Graphic)targetImage).rectTransform.offsetMax.x, y);
			base.enabled = false;
		}
	}
}
