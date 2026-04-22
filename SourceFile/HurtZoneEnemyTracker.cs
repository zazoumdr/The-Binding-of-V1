using System.Collections.Generic;
using UnityEngine;

public class HurtZoneEnemyTracker
{
	public EnemyIdentifier target;

	public List<Collider> limbs = new List<Collider>();

	public float timer;

	public HurtZoneEnemyTracker(EnemyIdentifier eid, Collider limb, float hurtCooldown)
	{
		target = eid;
		limbs.Add(limb);
		timer = hurtCooldown;
	}

	public bool HasLimbs(Collider colliderToCheck)
	{
		if (limbs.Count == 0)
		{
			return false;
		}
		int num = limbs.Count - 1;
		while (num >= 0)
		{
			if (limbs[num] == null || !limbs[num].enabled || limbs[num].transform.localScale == Vector3.zero || !limbs[num].gameObject.activeInHierarchy || ((bool)colliderToCheck && !Physics.ComputePenetration(colliderToCheck, colliderToCheck.transform.position, colliderToCheck.transform.rotation, limbs[num], limbs[num].transform.position, limbs[num].transform.rotation, out var _, out var _)))
			{
				limbs.RemoveAt(num);
				num--;
				continue;
			}
			return true;
		}
		return false;
	}
}
