using System.Collections.Generic;
using UnityEngine;

public class DryZoneController : MonoSingleton<DryZoneController>
{
	public HashSet<Water> waters = new HashSet<Water>();

	public Dictionary<Collider, int> colliderCalls = new Dictionary<Collider, int>();

	public HashSet<DryZone> dryZones = new HashSet<DryZone>();

	public void AddCollider(Collider other)
	{
		if (!colliderCalls.TryGetValue(other, out var _))
		{
			colliderCalls.Add(other, 1);
			if (waters.Count <= 0)
			{
				return;
			}
			{
				foreach (Water item in waters)
				{
					item.EnterDryZone(other);
				}
				return;
			}
		}
		colliderCalls[other]++;
	}

	public void RemoveCollider(Collider other)
	{
		if (!colliderCalls.TryGetValue(other, out var value))
		{
			return;
		}
		if (value > 1)
		{
			colliderCalls[other]--;
			return;
		}
		colliderCalls.Remove(other);
		if (waters.Count <= 0)
		{
			return;
		}
		foreach (Water item in waters)
		{
			item.ExitDryZone(other);
		}
	}
}
