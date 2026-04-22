using UnityEngine;

public class DisableAtDistance : MonoBehaviour
{
	public float distance;

	public GameObject toDisable;

	private void Update()
	{
		if ((bool)MonoSingleton<CameraController>.Instance)
		{
			if (toDisable.activeSelf && Vector3.Distance(base.transform.position, MonoSingleton<CameraController>.Instance.transform.position) > distance)
			{
				toDisable.SetActive(value: false);
			}
			else if (!toDisable.activeSelf && Vector3.Distance(base.transform.position, MonoSingleton<CameraController>.Instance.transform.position) < distance)
			{
				toDisable.SetActive(value: true);
			}
		}
	}
}
