using UnityEngine;

public class ScreenDistortionField : MonoBehaviour
{
	private Collider col;

	public float distance;

	public float strength = 1f;

	public float currentStrength;

	private void Start()
	{
		col = GetComponent<Collider>();
	}

	private void OnEnable()
	{
		if (!MonoSingleton<ScreenDistortionController>.Instance.fields.Contains(this))
		{
			MonoSingleton<ScreenDistortionController>.Instance.fields.Add(this);
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<ScreenDistortionController>.Instance.fields.Contains(this))
		{
			MonoSingleton<ScreenDistortionController>.Instance.fields.Remove(this);
		}
	}

	private void Update()
	{
		Vector3 position = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
		Vector3 b = (col ? col.ClosestPoint(position) : base.transform.position);
		float num = Vector3.Distance(position, b);
		if (num < distance)
		{
			float num2 = Mathf.Pow((distance - num) / distance, 2f);
			currentStrength = num2 * strength;
		}
		else if (currentStrength != 0f)
		{
			currentStrength = 0f;
		}
	}
}
