using UnityEngine;

public class ExplosionChunk : MonoBehaviour
{
	private bool done;

	private Rigidbody rb;

	private void Start()
	{
		Invoke("Gone", 3f);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!done && LayerMaskDefaults.IsMatchingLayer(collision.gameObject.layer, LMD.Environment))
		{
			done = true;
			rb = GetComponent<Rigidbody>();
			GetComponent<TrailRenderer>().emitting = false;
			Invoke("Gone", 1f);
		}
	}

	private void Gone()
	{
		Object.Destroy(base.gameObject);
	}
}
