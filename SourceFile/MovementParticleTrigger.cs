using System.Collections.Generic;
using UnityEngine;

public class MovementParticleTrigger : MonoBehaviour
{
	public GameObject particle;

	public float distancePerParticle = 3f;

	private Collider[] colliders;

	[HideInInspector]
	public List<EntererTracker> enterers = new List<EntererTracker>();

	private void Awake()
	{
		colliders = GetComponentsInChildren<Collider>(includeInactive: true);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Enter(collision.collider);
	}

	private void OnTriggerEnter(Collider other)
	{
		Enter(other);
	}

	private void Enter(Collider other)
	{
		if (other.gameObject == MonoSingleton<NewMovement>.Instance.gameObject || other.gameObject.layer == 12)
		{
			EntererTracker entererTracker = IsTracked(other.gameObject);
			if (entererTracker == null)
			{
				entererTracker = new EntererTracker(other.gameObject, other.gameObject.transform.position);
				enterers.Add(entererTracker);
				Object.Instantiate(particle, GetClosestPointOnTrigger(other.gameObject.transform.position), Quaternion.identity);
			}
			entererTracker.amount++;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		Exit(collision.collider);
	}

	private void OnTriggerExit(Collider other)
	{
		Exit(other);
	}

	private void Exit(Collider other)
	{
		EntererTracker entererTracker = IsTracked(other.gameObject);
		if (entererTracker != null)
		{
			entererTracker.amount--;
			if (entererTracker.amount == 0)
			{
				enterers.Remove(entererTracker);
			}
		}
	}

	private void OnDisable()
	{
		enterers.Clear();
	}

	private void Update()
	{
		if (enterers == null || enterers.Count == 0)
		{
			return;
		}
		for (int num = enterers.Count - 1; num >= 0; num--)
		{
			if (enterers[num] == null || enterers[num].target == null || !enterers[num].target.activeInHierarchy)
			{
				enterers.RemoveAt(num);
			}
			else
			{
				Transform transform = enterers[num].target.transform;
				float num2 = Vector3.Distance(enterers[num].position, transform.position);
				if (num2 > distancePerParticle)
				{
					Object.Instantiate(particle, GetClosestPointOnTrigger(transform.position), Quaternion.identity);
					if (num2 > distancePerParticle * 2f)
					{
						Vector3 normalized = (enterers[num].position - transform.position).normalized;
						while (num2 > distancePerParticle)
						{
							num2 -= distancePerParticle;
							_ = transform.position + normalized * num2;
						}
					}
					enterers[num].position = transform.position;
				}
			}
		}
	}

	private EntererTracker IsTracked(GameObject gob)
	{
		EntererTracker result = null;
		for (int i = 0; i < enterers.Count; i++)
		{
			if (enterers[i].target == gob)
			{
				result = enterers[i];
			}
		}
		return result;
	}

	private Vector3 GetClosestPointOnTrigger(Vector3 position)
	{
		if (colliders.Length == 0)
		{
			return Vector3.zero;
		}
		if (colliders.Length == 1)
		{
			if (!(colliders[0] == null))
			{
				return colliders[0].ClosestPoint(position);
			}
			return Vector3.zero;
		}
		Vector3 vector = position + Vector3.one * 100f;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (!(colliders[i] == null) && colliders[i].enabled && colliders[i].gameObject.activeInHierarchy)
			{
				Vector3 vector2 = colliders[i].ClosestPoint(position);
				if (vector2 == position)
				{
					return vector2;
				}
				if (Vector3.Distance(position, vector) > Vector3.Distance(position, vector2))
				{
					vector = vector2;
				}
			}
		}
		if (vector == position + Vector3.one * 100f)
		{
			return position;
		}
		return vector;
	}
}
