using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MannequinString : MonoBehaviour
{
	public float width = 0.25f;

	public float lengthScale = 2f;

	public float wobbleStrength = 0.5f;

	public float wobbleSpeed = 2f;

	private LineRenderer lRend;

	private float randomOffset;

	private void Start()
	{
		lRend = GetComponent<LineRenderer>();
		lRend.positionCount = 10;
		randomOffset = Random.Range(0f, 100f);
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		lRend.SetPosition(0, position);
		for (int i = 0; i < lRend.positionCount; i++)
		{
			float num = 1f - Mathf.Abs(((float)i - (float)lRend.positionCount / 2f) / ((float)lRend.positionCount / 2f));
			num *= wobbleStrength;
			lRend.SetPosition(i, position + Vector3.up * ((float)i * lengthScale) + Vector3.right * Mathf.Sin(Time.time * wobbleSpeed + (float)i + randomOffset) * num);
		}
		lRend.startWidth = width;
		lRend.endWidth = width;
	}
}
