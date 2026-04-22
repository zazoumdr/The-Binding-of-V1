using UnityEngine;

public class GibDestroyer : MonoBehaviour
{
	[SerializeField]
	private AudioSource soundEffect;

	[SerializeField]
	private AudioSource rareSoundEffect;

	private TimeSince lastSound;

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == null)
		{
			return;
		}
		if (col.transform.TryGetComponent<GoreSplatter>(out var component))
		{
			component.Repool();
		}
		else
		{
			if (!col.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component2) || !component2.eid.dead)
			{
				return;
			}
			if ((float)lastSound > 0.1f)
			{
				lastSound = 0f;
				if ((bool)(Object)(object)rareSoundEffect && Random.Range(0f, 1f) < 0.025f)
				{
					Object.Instantiate<AudioSource>(rareSoundEffect, col.transform.position, Quaternion.identity);
				}
				else if ((bool)(Object)(object)soundEffect)
				{
					Object.Instantiate<AudioSource>(soundEffect, col.transform.position, Quaternion.identity);
				}
			}
			LimbBegone(col);
		}
	}

	public static void LimbBegone(Collider col)
	{
		col.transform.localScale = Vector3.zero;
		if (col.transform.parent != null)
		{
			col.transform.position = col.transform.parent.position;
		}
		else
		{
			col.transform.position = Vector3.zero;
		}
		if ((bool)col.attachedRigidbody)
		{
			Joint[] componentsInChildren = col.attachedRigidbody.GetComponentsInChildren<Joint>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i]);
			}
			Object.Destroy(col.attachedRigidbody);
		}
		col.gameObject.SetActive(value: false);
		Object.Destroy(col);
	}
}
