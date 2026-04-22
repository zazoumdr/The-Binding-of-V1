using UnityEngine;

public class TrailRendererAutoScaler : MonoBehaviour
{
	private TrailRenderer tr;

	[SerializeField]
	private bool setDefaultSizeOnAwake = true;

	[SerializeField]
	private Vector3 defaultSize = Vector3.one;

	private void Awake()
	{
		tr = GetComponent<TrailRenderer>();
		if (setDefaultSizeOnAwake)
		{
			defaultSize = base.transform.localScale;
		}
	}

	private void Update()
	{
		tr.widthMultiplier = SizeAverage(base.transform.localScale) / SizeAverage(defaultSize);
	}

	private float SizeAverage(Vector3 size)
	{
		return (size.x + size.y + size.z) / 3f;
	}
}
