using UnityEngine;

public class HideBone : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.localScale = Vector3.zero;
	}
}
