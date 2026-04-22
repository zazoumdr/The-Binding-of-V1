using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class WheelSegment
{
	public WeaponDescriptor descriptor;

	public int slotIndex;

	public UICircle segment;

	public UICircle divider;

	public Image icon;

	public Image iconGlow;

	public void SetActive(bool active)
	{
		((Graphic)segment).color = (active ? Color.red : Color.black);
		((Graphic)icon).color = (active ? Color.red : Color.white);
		Color color = (active ? Color.red : Color.black);
		color.a = 0.7f;
		((Graphic)iconGlow).color = color;
	}

	public void DestroySegment()
	{
		Object.Destroy(((Component)(object)segment).gameObject);
		Object.Destroy(((Component)(object)divider).gameObject);
	}
}
