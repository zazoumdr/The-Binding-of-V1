using UnityEngine;

public class PunchZone : MonoBehaviour
{
	public bool active;

	private AudioSource aud;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void OnTriggerStay(Collider other)
	{
		if (active)
		{
			if (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				aud.Play(tracked: true);
				active = false;
			}
			else if (other.gameObject.CompareTag("Enemy"))
			{
				active = false;
				other.gameObject.GetComponent<EnemyIdentifierIdentifier>().eid.DeliverDamage(other.gameObject, (base.transform.position - other.transform.position).normalized * 10000f, other.transform.position, 1f, tryForExplode: false, 1f);
			}
		}
	}
}
