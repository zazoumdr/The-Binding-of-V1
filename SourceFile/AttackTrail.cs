using UnityEngine;

public class AttackTrail : MonoBehaviour
{
	public Transform target;

	public Transform pivot;

	public int distance;

	private void Update()
	{
		if ((bool)target && (bool)pivot)
		{
			Vector3 position = target.position;
			Vector3 position2 = target.position + (target.position - pivot.position).normalized * distance;
			Quaternion rotation = Quaternion.LookRotation(base.transform.position - position);
			base.transform.SetPositionAndRotation(position2, rotation);
		}
	}

	public void DelayedDestroy(float time)
	{
		Invoke("DestroyNow", time);
	}

	private void DestroyNow()
	{
		Object.Destroy(base.gameObject);
	}
}
