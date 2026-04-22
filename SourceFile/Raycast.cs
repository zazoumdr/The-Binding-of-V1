using System;
using System.Collections.Generic;
using plog;
using plog.Models;
using UnityEngine;

public class Raycast : ICastable
{
	private static readonly Logger Log = new Logger("Raycast");

	public bool Cast(PortalCastStateV2 state, out PhysicsCastResult result)
	{
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(state.origin, state.direction, out hitInfo, state.maxDistance, state.layerMask, state.queryTriggerInteraction);
		result = new PhysicsCastResult
		{
			distance = (flag ? hitInfo.distance : state.maxDistance),
			point = (flag ? hitInfo.point : default(Vector3)),
			normal = (flag ? hitInfo.normal : default(Vector3))
		};
		if (flag)
		{
			Collider collider = hitInfo.collider;
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			Transform transform = (attachedRigidbody ? attachedRigidbody.transform : collider.transform);
			result.collider = collider;
			result.rigidbody = attachedRigidbody;
			result.transform = transform;
		}
		return flag;
	}

	public PhysicsCastResult[] CastAll(PortalCastStateV2 state)
	{
		RaycastHit[] array = Physics.RaycastAll(state.origin, state.direction, state.maxDistance, state.layerMask, state.queryTriggerInteraction);
		if (array.Length != 0)
		{
			Log.Info($"Normal raycast hit {array.Length} objects.", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		PhysicsCastResult[] array2 = new PhysicsCastResult[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			array2[i] = new PhysicsCastResult
			{
				distance = raycastHit.distance,
				point = raycastHit.point,
				normal = raycastHit.normal,
				transform = raycastHit.transform,
				collider = raycastHit.collider,
				rigidbody = raycastHit.rigidbody
			};
		}
		Array.Sort(array2);
		return array2;
	}
}
