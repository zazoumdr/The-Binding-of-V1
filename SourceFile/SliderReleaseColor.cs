using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderReleaseColor : MonoBehaviour, IPointerUpHandler, IEventSystemHandler
{
	[SerializeField]
	private Color releaseColor;

	private Color defaultColor;

	private Selectable slider;

	private float fade;

	private void Awake()
	{
		slider = GetComponent<Selectable>();
		defaultColor = slider.targetGraphic.color;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		ColorBlock colors = slider.colors;
		fade = ((ColorBlock)(ref colors)).fadeDuration;
	}

	private void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (fade != 0f)
		{
			fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime);
			Graphic targetGraphic = slider.targetGraphic;
			Color a = defaultColor;
			Color b = releaseColor;
			float num = fade;
			ColorBlock colors = slider.colors;
			targetGraphic.color = Color.Lerp(a, b, num / ((ColorBlock)(ref colors)).fadeDuration);
		}
	}
}
