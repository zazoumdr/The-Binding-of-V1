using UnityEngine;

public class Screenshaker : MonoBehaviour
{
	public float amount;

	public bool oneTime;

	public bool continuous;

	private bool alreadyShaken;

	private bool colliderless;

	public float minDistance;

	public float maxDistance;

	private void Awake()
	{
		colliderless = GetComponent<Collider>() == null && GetComponent<Rigidbody>() == null;
	}

	private void OnEnable()
	{
		if (colliderless)
		{
			Shake();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
		{
			Shake();
		}
	}

	private void Update()
	{
		if (continuous && base.gameObject.activeInHierarchy)
		{
			MonoSingleton<CameraController>.Instance.CameraShake((maxDistance == 0f) ? amount : GetDistanceValue());
		}
	}

	public void Shake()
	{
		if (!oneTime || !alreadyShaken)
		{
			float distanceValue = amount;
			if (maxDistance != 0f)
			{
				distanceValue = GetDistanceValue();
			}
			alreadyShaken = true;
			MonoSingleton<CameraController>.Instance.CameraShake(distanceValue);
			if (oneTime && !continuous)
			{
				Object.Destroy(this);
			}
		}
	}

	private float GetDistanceValue()
	{
		return Mathf.Lerp(amount, 0f, (Vector3.Distance(MonoSingleton<CameraController>.Instance.transform.position, base.transform.position) - minDistance) / (maxDistance - minDistance));
	}
}
