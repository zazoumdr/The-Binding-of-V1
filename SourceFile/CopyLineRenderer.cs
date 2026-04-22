using UnityEngine;

public class CopyLineRenderer : MonoBehaviour
{
	[HideInInspector]
	public LineRenderer toCopy;

	[HideInInspector]
	public LineRenderer lr;

	private float origWidth;

	private void Awake()
	{
		lr = GetComponent<LineRenderer>();
		toCopy = base.transform.parent.GetComponentInParent<LineRenderer>();
		origWidth = lr.widthMultiplier;
	}

	private void Update()
	{
		for (int i = 0; i < toCopy.positionCount; i++)
		{
			lr.SetPosition(i, toCopy.GetPosition(i));
		}
		lr.widthMultiplier = toCopy.widthMultiplier * origWidth;
	}
}
