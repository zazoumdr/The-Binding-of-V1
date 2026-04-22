using UnityEngine;

namespace Train;

public class ConnectedTram : MonoBehaviour
{
	public float offset = 10f;

	private Tram thisTram;

	private void Awake()
	{
		thisTram = GetComponent<Tram>();
	}

	public void UpdateTram(TramPath parentPath)
	{
		if (parentPath == null)
		{
			return;
		}
		thisTram.currentPath = new TramPath(parentPath.start, parentPath.end);
		float num = offset / parentPath.DistanceTotal;
		float num2 = parentPath.Progress - num;
		if (num2 < 0f)
		{
			float num3 = (0f - num2) * parentPath.DistanceTotal;
			TramPath tramPath = new TramPath(parentPath.start, parentPath.end);
			while (num3 > 0f && tramPath.start.GetDestination(forward: false) != null)
			{
				tramPath = new TramPath(tramPath.start.GetDestination(forward: false), tramPath.start);
				if (num3 <= tramPath.DistanceTotal)
				{
					thisTram.currentPath = tramPath;
					thisTram.currentPath.distanceTravelled = tramPath.DistanceTotal - num3;
					num3 = 0f;
				}
				else
				{
					num3 -= tramPath.DistanceTotal;
				}
			}
			thisTram.UpdateWorldRotation();
			thisTram.UpdateWorldPosition();
		}
		else
		{
			if (!thisTram.currentPath.Equals(parentPath))
			{
				thisTram.currentPath = new TramPath(parentPath.start, parentPath.end);
			}
			thisTram.currentPath.distanceTravelled = num2 * thisTram.currentPath.DistanceTotal;
			thisTram.UpdateWorldRotation();
			thisTram.UpdateWorldPosition();
		}
	}
}
