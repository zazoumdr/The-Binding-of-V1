using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionSpawn : MonoBehaviour
{
	private ParticleSystem part;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	public GameObject toSpawn;

	private void OnParticleCollision(GameObject other)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)part == null)
		{
			part = GetComponent<ParticleSystem>();
		}
		ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);
		if (collisionEvents.Count > 0)
		{
			GameObject original = toSpawn;
			ParticleCollisionEvent val = collisionEvents[0];
			Vector3 intersection = ((ParticleCollisionEvent)(ref val)).intersection;
			val = collisionEvents[0];
			Object.Instantiate(original, intersection, Quaternion.LookRotation(((ParticleCollisionEvent)(ref val)).normal)).SetActive(value: true);
		}
	}
}
