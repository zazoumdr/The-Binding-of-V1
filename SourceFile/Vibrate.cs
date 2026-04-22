using UnityEngine;

public class Vibrate : MonoBehaviour
{
	public float intensity;

	private Vector3 origPos;

	public float speed;

	private Vector3 targetPos;

	public bool returnToOriginalPositionOnDisable;

	private void Start()
	{
		origPos = base.transform.localPosition;
		targetPos = origPos;
	}

	private void OnDisable()
	{
		if (returnToOriginalPositionOnDisable)
		{
			base.transform.localPosition = origPos;
		}
	}

	private void Update()
	{
		Vector3 vector = Vector3.zero;
		if (speed != 0f)
		{
			vector = base.transform.localPosition;
		}
		if (speed == 0f)
		{
			base.transform.localPosition = origPos + Random.insideUnitSphere * intensity;
		}
		else if (vector == targetPos)
		{
			targetPos = origPos + Random.insideUnitSphere * intensity;
		}
		else
		{
			base.transform.localPosition = Vector3.MoveTowards(vector, targetPos, Time.deltaTime * speed);
		}
	}
}
