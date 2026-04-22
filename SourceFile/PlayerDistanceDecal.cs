using UnityEngine;

public class PlayerDistanceDecal : MonoBehaviour
{
	public GameObject decal;

	private GameObject currentDecal;

	private Collider col;

	private GameObject camObj;

	private void OnDisable()
	{
		if (currentDecal != null)
		{
			Object.Destroy(currentDecal);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			if (col == null)
			{
				col = GetComponent<Collider>();
			}
			if (camObj == null)
			{
				camObj = MonoSingleton<CameraController>.Instance.gameObject;
			}
			if (currentDecal == null)
			{
				currentDecal = Object.Instantiate(decal, base.transform.position, Quaternion.identity);
			}
			currentDecal.transform.position = col.ClosestPointOnBounds(camObj.transform.position);
			currentDecal.transform.LookAt(camObj.transform.position);
			currentDecal.transform.position += currentDecal.transform.forward * 0.1f;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (currentDecal != null)
		{
			Object.Destroy(currentDecal);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			if (col == null)
			{
				col = GetComponent<Collider>();
			}
			if (camObj == null)
			{
				camObj = MonoSingleton<CameraController>.Instance.gameObject;
			}
			if (currentDecal == null)
			{
				currentDecal = Object.Instantiate(decal, base.transform.position, Quaternion.identity);
			}
			currentDecal.transform.position = col.ClosestPointOnBounds(camObj.transform.position);
			currentDecal.transform.LookAt(camObj.transform.position);
			currentDecal.transform.position += currentDecal.transform.forward * 0.1f;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (currentDecal != null)
		{
			Object.Destroy(currentDecal);
		}
	}
}
