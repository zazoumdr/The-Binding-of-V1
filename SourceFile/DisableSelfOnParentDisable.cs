using UnityEngine;

public class DisableSelfOnParentDisable : MonoBehaviour
{
	private void OnDisable()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
