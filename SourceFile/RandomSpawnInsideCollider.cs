using UnityEngine;

public class RandomSpawnInsideCollider : MonoBehaviour
{
	public GameObject spawnedObject;

	private BoxCollider boxCollider;

	public float delay;

	private float cooldown;

	public bool oneTime;

	private bool activated;

	private void Start()
	{
		boxCollider = GetComponent<BoxCollider>();
	}

	private void Update()
	{
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		if (cooldown <= 0f)
		{
			Spawn();
		}
	}

	private void Spawn()
	{
		if (activated)
		{
			if (oneTime)
			{
				base.enabled = false;
				return;
			}
		}
		else
		{
			activated = true;
		}
		Vector3 vector = boxCollider.size / 2f;
		Vector3 position = new Vector3(Random.Range(0f - vector.x, vector.x), Random.Range(0f - vector.y, vector.y), Random.Range(0f - vector.z, vector.z)) + boxCollider.center;
		Object.Instantiate(spawnedObject, boxCollider.transform.TransformPoint(position), Random.rotation);
		cooldown = delay;
		if (oneTime)
		{
			base.enabled = false;
		}
	}
}
