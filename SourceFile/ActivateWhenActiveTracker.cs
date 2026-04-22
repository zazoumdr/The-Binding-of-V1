using UnityEngine;

public class ActivateWhenActiveTracker : MonoBehaviour
{
	public ActivateWhenActive target;

	private void OnEnable()
	{
		if ((bool)target)
		{
			target.Check();
		}
	}

	private void OnDisable()
	{
		if ((bool)target)
		{
			target.Check();
		}
	}
}
