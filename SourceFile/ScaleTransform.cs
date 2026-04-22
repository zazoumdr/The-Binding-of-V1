using UnityEngine;
using UnityEngine.Events;

public class ScaleTransform : MonoBehaviour
{
	public bool scaleToOriginalScale;

	public Vector3 target;

	public bool setToZeroOnStart;

	[HideInInspector]
	public bool hasValue;

	public float speed;

	public UnityEvent onComplete;

	private void Start()
	{
		if (!hasValue)
		{
			hasValue = true;
			if (scaleToOriginalScale)
			{
				target = base.transform.localScale;
			}
			if (scaleToOriginalScale && setToZeroOnStart)
			{
				base.transform.localScale = Vector3.zero;
			}
		}
	}

	private void Update()
	{
		if (base.transform.localScale != target)
		{
			base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, target, speed * Time.deltaTime);
			if (base.transform.localScale == target)
			{
				onComplete?.Invoke();
			}
		}
	}

	public void SetTransformX(float target)
	{
		base.transform.localScale = new Vector3(target, base.transform.localScale.y, base.transform.localScale.z);
	}

	public void SetTransformY(float target)
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, target, base.transform.localScale.z);
	}

	public void SetTransformZ(float target)
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, target);
	}

	public void SetScaleToZero()
	{
		base.transform.localScale = Vector3.zero;
	}

	public void SetTargetToZero()
	{
		target = Vector3.zero;
	}
}
