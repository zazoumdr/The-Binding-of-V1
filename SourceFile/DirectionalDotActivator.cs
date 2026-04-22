using UnityEngine;

public class DirectionalDotActivator : MonoBehaviour
{
	public Vector3 dotVector;

	public UltrakillEvent onPositive;

	public bool invertOnNegative;

	[HideInInspector]
	public bool currentStatus;

	private void Start()
	{
		Check(force: true);
	}

	private void Update()
	{
		Check();
	}

	private void Check(bool force = false)
	{
		if (Vector3.Dot(base.transform.forward, dotVector) >= 0f && !currentStatus)
		{
			currentStatus = true;
			onPositive.Invoke();
		}
		else if (currentStatus && invertOnNegative)
		{
			currentStatus = false;
			onPositive.Revert();
		}
	}
}
