using UnityEngine;

public class ObjectBounds : MonoBehaviour
{
	public Transform target;

	public bool cancelMomentum;

	private Rigidbody rb;

	private Collider[] cols;

	private void Start()
	{
		cols = GetComponents<Collider>();
	}

	private void Update()
	{
		bool flag = false;
		for (int num = cols.Length - 1; num >= 0; num--)
		{
			if (!(cols[num] == null) && Vector3.Distance(cols[num].ClosestPoint(target.position), target.position) <= 0.1f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			target.position = base.transform.position;
			if (cancelMomentum && ((bool)rb || target.TryGetComponent<Rigidbody>(out rb)))
			{
				rb.velocity = Vector3.zero;
			}
		}
	}
}
