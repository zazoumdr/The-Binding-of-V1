using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Train;

public class TrainTrackPoint : MonoBehaviour
{
	[HideInInspector]
	public int instanceId;

	public List<TrainTrackPoint> forwardPoints;

	public List<TrainTrackPoint> backwardPoints;

	public StopBehaviour stopBehaviour;

	[HideInInspector]
	public int forwardPath;

	[HideInInspector]
	public int backwardPath;

	[HideInInspector]
	public TrackCurveSettings forwardCurveSettings;

	private static readonly Color ForwardActive = Color.green;

	private static readonly Color BackwardActive = new Color(0.4f, 0.6f, 0.5f);

	public TrainTrackPoint GetDestination(bool forward = true)
	{
		TrainTrackPoint trainTrackPoint;
		if (forward)
		{
			if (forwardPoints == null || forwardPoints.Count == 0 || forwardPoints.All((TrainTrackPoint point) => point == null))
			{
				return null;
			}
			trainTrackPoint = forwardPoints[forwardPath];
		}
		else
		{
			if (backwardPoints == null || backwardPoints.Count == 0 || backwardPoints.All((TrainTrackPoint point) => point == null))
			{
				return null;
			}
			trainTrackPoint = backwardPoints[backwardPath];
		}
		if (trainTrackPoint == null || !trainTrackPoint.gameObject.activeSelf)
		{
			return null;
		}
		return trainTrackPoint;
	}

	private void OnDrawGizmos()
	{
		_ = base.transform.position;
		DrawPaths(forwardPoints, forwardPath, backward: false);
		DrawPaths(backwardPoints, backwardPath, backward: true);
	}

	private void Update()
	{
		DrawPaths(forwardPoints, forwardPath, backward: false);
		DrawPaths(backwardPoints, backwardPath, backward: true);
	}

	private void DrawPaths(IReadOnlyList<TrainTrackPoint> points, int path, bool backward)
	{
		Vector3 position = base.transform.position;
		if (points == null)
		{
			return;
		}
		for (int i = 0; i < points.Count; i++)
		{
			TrainTrackPoint trainTrackPoint = points[i];
			if (trainTrackPoint == null)
			{
				continue;
			}
			Vector3 position2 = trainTrackPoint.transform.position;
			bool flag = !trainTrackPoint.gameObject.activeSelf;
			if (path != i || flag)
			{
				_ = Color.red;
			}
			else if (!backward)
			{
				_ = ForwardActive;
			}
			else
			{
				_ = BackwardActive;
			}
			TrackCurveSettings trackCurveSettings = (backward ? trainTrackPoint.forwardCurveSettings : forwardCurveSettings);
			if (trackCurveSettings.curve == PathInterpolation.Linear)
			{
				Vector3.Lerp(position, position2, flag ? 1f : 0.5f);
			}
			else
			{
				if (trackCurveSettings.curve == PathInterpolation.SphericalManual && trackCurveSettings.handle == null)
				{
					continue;
				}
				int num = 16;
				TrainTrackPoint startPoint = (backward ? trainTrackPoint : this);
				TrainTrackPoint endPoint = (backward ? this : trainTrackPoint);
				for (int j = 0; j <= num; j++)
				{
					float num2 = (float)j / (float)num;
					if (!flag)
					{
						num2 *= 0.5f;
					}
					if (backward)
					{
						num2 = 1f - num2;
					}
					TramPath.GetPointOnSimulatedPath(num2, startPoint, endPoint);
				}
			}
		}
	}
}
