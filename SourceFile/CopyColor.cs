using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopyColor : MonoBehaviour
{
	private Image img;

	public Image target;

	public TMP_Text textTarget;

	private void Start()
	{
		img = GetComponent<Image>();
	}

	private void Update()
	{
		if ((bool)(Object)(object)img)
		{
			if ((bool)(Object)(object)target)
			{
				((Graphic)img).color = ((Graphic)target).color;
			}
			else if ((bool)(Object)(object)textTarget)
			{
				((Graphic)img).color = ((Graphic)textTarget).color;
			}
		}
	}
}
