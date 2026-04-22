using UnityEngine;

public class DetachFromParent : MonoBehaviour
{
	public bool detachOnStart;

	public Transform newParent;

	private void Start()
	{
		if (detachOnStart)
		{
			Detach();
		}
	}

	public void Detach()
	{
		base.transform.SetParent(newParent, worldPositionStays: true);
	}
}
