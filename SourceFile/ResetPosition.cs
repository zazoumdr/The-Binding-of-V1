using UnityEngine;

public class ResetPosition : MonoBehaviour
{
	[HideInInspector]
	public bool valueSet;

	[HideInInspector]
	public Vector3 originalPosition;

	[HideInInspector]
	public Quaternion originalRotation;

	private void Awake()
	{
		if (!valueSet)
		{
			valueSet = true;
			ChangeOriginalPositionAndRotation();
		}
	}

	public void Activate()
	{
		if (valueSet)
		{
			base.transform.localPosition = originalPosition;
			base.transform.localRotation = originalRotation;
		}
	}

	public void ChangeOriginalPositionAndRotation()
	{
		ChangeOriginalPosition(base.transform.localPosition);
		ChangeOriginalRotation(base.transform.localRotation);
	}

	public void ChangeOriginalPosition(Vector3 target)
	{
		originalPosition = target;
	}

	public void ChangeOriginalRotation(Quaternion target)
	{
		originalRotation = target;
	}

	public void ChangeOriginalRotation(Vector3 target)
	{
		ChangeOriginalRotation(Quaternion.Euler(target));
	}
}
