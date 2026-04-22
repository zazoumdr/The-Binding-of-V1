using System.Collections.Generic;
using UnityEngine;

public class ObjectTracker : MonoSingleton<ObjectTracker>
{
	public List<Grenade> grenadeList = new List<Grenade>();

	public List<Cannonball> cannonballList = new List<Cannonball>();

	public List<Landmine> landmineList = new List<Landmine>();

	public List<Magnet> magnetList = new List<Magnet>();

	public List<Zappable> zappablesList = new List<Zappable>();

	public List<HookPoint> providenceHookPointsList = new List<HookPoint>();

	public void AddGrenade(Grenade gren)
	{
		if (!grenadeList.Contains(gren))
		{
			grenadeList.Add(gren);
		}
	}

	public void AddCannonball(Cannonball cb)
	{
		if (!cannonballList.Contains(cb))
		{
			cannonballList.Add(cb);
		}
	}

	public void AddLandmine(Landmine lm)
	{
		if (!landmineList.Contains(lm))
		{
			landmineList.Add(lm);
		}
	}

	public void AddMagnet(Magnet mag)
	{
		if (!magnetList.Contains(mag))
		{
			magnetList.Add(mag);
		}
	}

	public void AddZappable(Zappable zap)
	{
		if (!zappablesList.Contains(zap))
		{
			zappablesList.Add(zap);
		}
	}

	public void AddProvidenceHookPoint(HookPoint hp)
	{
		if (!providenceHookPointsList.Contains(hp))
		{
			providenceHookPointsList.Add(hp);
		}
	}

	public void RemoveGrenade(Grenade gren)
	{
		if (grenadeList.Contains(gren))
		{
			grenadeList.Remove(gren);
		}
	}

	public void RemoveCannonball(Cannonball cb)
	{
		if (cannonballList.Contains(cb))
		{
			cannonballList.Remove(cb);
		}
	}

	public void RemoveLandmine(Landmine lm)
	{
		if (landmineList.Contains(lm))
		{
			landmineList.Remove(lm);
		}
	}

	public void RemoveMagnet(Magnet mag)
	{
		if (magnetList.Contains(mag))
		{
			magnetList.Remove(mag);
		}
	}

	public void RemoveZappable(Zappable zap)
	{
		if (zappablesList.Contains(zap))
		{
			zappablesList.Remove(zap);
		}
	}

	public void RemoveProvidenceHookPoint(HookPoint hp)
	{
		if (providenceHookPointsList.Contains(hp))
		{
			providenceHookPointsList.Remove(hp);
		}
	}

	public Grenade GetGrenade(Transform tf)
	{
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] != null && grenadeList[num].transform == tf)
			{
				return grenadeList[num];
			}
		}
		return null;
	}

	public Cannonball GetCannonball(Transform tf)
	{
		for (int num = cannonballList.Count - 1; num >= 0; num--)
		{
			if (cannonballList[num] != null && cannonballList[num].transform == tf)
			{
				return cannonballList[num];
			}
		}
		return null;
	}

	public Landmine GetLandmine(Transform tf)
	{
		for (int num = landmineList.Count - 1; num >= 0; num--)
		{
			if (landmineList[num] != null && landmineList[num].transform == tf)
			{
				return landmineList[num];
			}
		}
		return null;
	}

	public bool HasTransform(Transform tf)
	{
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] != null && grenadeList[num].transform == tf)
			{
				return true;
			}
		}
		for (int num2 = cannonballList.Count - 1; num2 >= 0; num2--)
		{
			if (cannonballList[num2] != null && cannonballList[num2].transform == tf)
			{
				return true;
			}
		}
		for (int num3 = landmineList.Count - 1; num3 >= 0; num3--)
		{
			if (landmineList[num3] != null && landmineList[num3].transform == tf)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		Invoke("SlowUpdate", 30f);
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 30f);
		for (int num = grenadeList.Count - 1; num >= 0; num--)
		{
			if (grenadeList[num] == null)
			{
				grenadeList.RemoveAt(num);
			}
		}
		for (int num2 = cannonballList.Count - 1; num2 >= 0; num2--)
		{
			if (cannonballList[num2] == null)
			{
				cannonballList.RemoveAt(num2);
			}
		}
		for (int num3 = landmineList.Count - 1; num3 >= 0; num3--)
		{
			if (landmineList[num3] == null)
			{
				landmineList.RemoveAt(num3);
			}
		}
		for (int num4 = magnetList.Count - 1; num4 >= 0; num4--)
		{
			if (magnetList[num4] == null)
			{
				magnetList.RemoveAt(num4);
			}
		}
		for (int num5 = zappablesList.Count - 1; num5 >= 0; num5--)
		{
			if (zappablesList[num5] == null)
			{
				zappablesList.RemoveAt(num5);
			}
		}
		for (int num6 = providenceHookPointsList.Count - 1; num6 >= 0; num6--)
		{
			if (providenceHookPointsList[num6] == null)
			{
				providenceHookPointsList.RemoveAt(num6);
			}
		}
	}
}
