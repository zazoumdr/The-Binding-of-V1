using System;
using System.Text;
using UnityEngine;

public class SpeedWeed : MonoBehaviour
{
	private Vector3 lastPosition;

	private float lastSpeed;

	private TimeSince timeSinceLastSpeedChange;

	private void FixedUpdate()
	{
		float num = (base.transform.position - lastPosition).magnitude / Time.deltaTime;
		lastPosition = base.transform.position;
		if (!(Math.Abs(num - lastSpeed) < 0.1f))
		{
			TimeSince timeSince = timeSinceLastSpeedChange;
			timeSinceLastSpeedChange = 0f;
			lastSpeed = num;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Speed: ");
			stringBuilder.Append(num);
			if ((float)timeSince > 1f)
			{
				stringBuilder.Append(" (changed after ");
				stringBuilder.Append(timeSince);
				stringBuilder.Append("s)");
			}
		}
	}
}
