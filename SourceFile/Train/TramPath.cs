using UnityEngine;

namespace Train;

public class TramPath
{
	private const int CurveDistanceCalculationSteps = 16;

	private const float TramDirectionCalcStepLength = 0.05f;

	private const float MinSpeedMultiplier = 0.0125f;

	public readonly TrainTrackPoint start;

	public readonly TrainTrackPoint end;

	public float distanceTravelled;

	public float DistanceTotal { get; private set; }

	public float Progress
	{
		get
		{
			if (distanceTravelled != 0f || DistanceTotal != 0f)
			{
				return distanceTravelled / DistanceTotal;
			}
			return 0f;
		}
	}

	public TramPath(TrainTrackPoint start, bool forward)
	{
		this.start = start;
		end = start.GetDestination(forward);
		DistanceTotal = CalculateFullDistance();
	}

	public TramPath(TrainTrackPoint start, TrainTrackPoint end)
	{
		this.start = start;
		this.end = end;
		DistanceTotal = CalculateFullDistance();
	}

	private float CalculateFullDistance()
	{
		return CalculateFullDistance(start, end);
	}

	private float CalculateFullDistance(TrainTrackPoint startPoint, TrainTrackPoint endPoint)
	{
		switch (startPoint.forwardCurveSettings.curve)
		{
		case PathInterpolation.Linear:
			return Vector3.Distance(startPoint.transform.position, endPoint.transform.position);
		case PathInterpolation.SphericalManual:
		case PathInterpolation.SphericalAutomatic:
		{
			float num = 0f;
			Vector3 a = startPoint.transform.position;
			for (int i = 0; i < 16; i++)
			{
				Vector3 pointOnSimulatedPath = GetPointOnSimulatedPath((float)i / 16f, startPoint, endPoint);
				float num2 = Vector3.Distance(a, pointOnSimulatedPath);
				a = pointOnSimulatedPath;
				if (i > 0)
				{
					num += num2;
				}
			}
			return num;
		}
		default:
			return 0f;
		}
	}

	public Vector3 GetPointOnPath(float progress)
	{
		return GetPointOnSimulatedPath(progress, start, end);
	}

	public static Vector3 GetPointOnSimulatedPath(float progress, TrainTrackPoint startPoint, TrainTrackPoint endPoint)
	{
		Vector3 position = startPoint.transform.position;
		Vector3 position2 = endPoint.transform.position;
		TrackCurveSettings forwardCurveSettings = startPoint.forwardCurveSettings;
		switch (forwardCurveSettings.curve)
		{
		case PathInterpolation.SphericalAutomatic:
		{
			float angle = forwardCurveSettings.angle;
			bool flipCurve = forwardCurveSettings.flipCurve;
			float angle2 = angle;
			Vector3 center = PathTools.ComputeSphericalCurveCenter(position, position2, flipCurve, angle2);
			return PathTools.InterpolateAlongCircle(position, position2, center, progress);
		}
		case PathInterpolation.SphericalManual:
		{
			Transform handle = forwardCurveSettings.handle;
			return PathTools.InterpolateAlongCircle(position, position2, handle.position, progress);
		}
		default:
			return Vector3.Lerp(position, position2, progress);
		}
	}

	public float MaxSpeedMultiplier(TramMovementDirection direction, float speed)
	{
		if (IsDeadEnd(direction))
		{
			if (GetNextPoint(direction).stopBehaviour == StopBehaviour.EaseOut)
			{
				float num = 1.5f;
				float p = 0.85f;
				float f = ((direction == TramMovementDirection.Forward) ? (DistanceTotal - distanceTravelled) : distanceTravelled);
				f = Mathf.Abs(f);
				if (f < speed * num)
				{
					return Mathf.Clamp(Mathf.Pow(f / (speed * num), p), 0.0125f, 1f);
				}
				return 1f;
			}
			return 1f;
		}
		return 1f;
	}

	private Vector3 CalculateCurrentMovementDirection()
	{
		float num = Progress + 0.05f / DistanceTotal;
		Vector3 vector;
		if (num > 1f)
		{
			TrainTrackPoint destination = end.GetDestination();
			if (destination != null)
			{
				float num2 = num - 1f;
				float num3 = CalculateFullDistance(end, destination);
				vector = GetPointOnSimulatedPath(num2 * DistanceTotal / num3, end, destination);
			}
			else
			{
				vector = GetPointOnPath(Mathf.Clamp01(num));
			}
		}
		else
		{
			vector = GetPointOnPath(num);
		}
		float num4 = Progress - 0.05f / DistanceTotal;
		Vector3 vector2;
		if (num4 < 0f)
		{
			TrainTrackPoint destination2 = start.GetDestination(forward: false);
			if (destination2 != null)
			{
				float num5 = 0f - num4;
				float num6 = CalculateFullDistance(destination2, start);
				float num7 = num5 * DistanceTotal / num6;
				vector2 = GetPointOnSimulatedPath(1f - num7, destination2, start);
			}
			else
			{
				vector2 = GetPointOnPath(Mathf.Clamp01(num4));
			}
		}
		else
		{
			vector2 = GetPointOnPath(num4);
		}
		return (vector - vector2).normalized;
	}

	public string PrintPathDirectional(TramMovementDirection direction)
	{
		return direction switch
		{
			TramMovementDirection.None => "(" + start.gameObject.name + ") --- (" + end.gameObject.name + ")", 
			TramMovementDirection.Forward => "(" + start.gameObject.name + ") --> (" + end.gameObject.name + ")", 
			_ => "(" + start.gameObject.name + ") <-- (" + end.gameObject.name + ")", 
		};
	}

	public bool IsDeadEnd(TramMovementDirection direction)
	{
		if (direction == TramMovementDirection.None)
		{
			return false;
		}
		return GetNextPoint(direction).GetDestination(direction == TramMovementDirection.Forward) == null;
	}

	public TrainTrackPoint GetNextPoint(TramMovementDirection direction)
	{
		return direction switch
		{
			TramMovementDirection.None => null, 
			TramMovementDirection.Forward => end, 
			_ => start, 
		};
	}

	public Vector3 MovementDirection()
	{
		return CalculateCurrentMovementDirection();
	}

	public override bool Equals(object obj)
	{
		if (obj is TramPath tramPath)
		{
			if (start == tramPath.start)
			{
				return end == tramPath.end;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (start, end).GetHashCode();
	}
}
