using UnityEngine;

public class FloatOnWater : MonoBehaviour
{
	private Rigidbody rb;

	private Collider waterCol;

	private bool isInWater;

	public float floatiness = 50f;

	public float dampen = 0.9f;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (isInWater && Physics.Raycast(new Vector3(base.transform.position.x, waterCol.bounds.max.y + 0.1f, base.transform.position.z), Vector3.down, out var hitInfo, 900f, 16, QueryTriggerInteraction.Collide))
		{
			Vector3 vector = hitInfo.point - base.transform.position;
			rb.AddForce(vector * floatiness - rb.velocity * dampen);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent<Water>(out var component))
		{
			component = other.GetComponentInParent<Water>();
		}
		if (component != null)
		{
			isInWater = true;
			waterCol = other;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other == waterCol)
		{
			waterCol = null;
			isInWater = false;
		}
	}
}
