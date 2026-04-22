using UnityEngine;

public class ShotgunShell : MonoBehaviour
{
	private bool hitGround;

	private AudioSource aud;

	private Collider col;

	private void Start()
	{
		Invoke("TurnGib", 0.2f);
		Invoke("Remove", 2f);
	}

	private void TurnGib()
	{
		col = GetComponent<Collider>();
		col.enabled = true;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 9;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!hitGround && LayerMaskDefaults.IsMatchingLayer(collision.gameObject.layer, LMD.Environment))
		{
			hitGround = true;
			if (!(Object)(object)aud)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.SetPitch(Random.Range(0.85f, 1.15f));
			aud.Play(tracked: true);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (LayerMaskDefaults.IsMatchingLayer(collision.gameObject.layer, LMD.Environment))
		{
			hitGround = false;
		}
	}

	private void Remove()
	{
		if (!hitGround || base.transform.position.magnitude > 1000f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		base.transform.SetParent(GoreZone.ResolveGoreZone(base.transform).gibZone, worldPositionStays: true);
		Object.Destroy(GetComponent<Rigidbody>());
		Object.Destroy(col);
	}
}
