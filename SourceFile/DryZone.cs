using System.Collections.Generic;
using UnityEngine;

public class DryZone : MonoBehaviour
{
	private HashSet<Collider> cols = new HashSet<Collider>();

	private DryZoneController dzc;

	private void Awake()
	{
		dzc = MonoSingleton<DryZoneController>.Instance;
		dzc.dryZones.Add(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			cols.Add(other);
			dzc.AddCollider(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (cols.Remove(other))
		{
			dzc.RemoveCollider(other);
		}
	}

	private void OnDisable()
	{
		if (!base.gameObject.scene.isLoaded)
		{
			return;
		}
		foreach (Collider col in cols)
		{
			dzc.RemoveCollider(col);
		}
		dzc.dryZones.Remove(this);
	}

	private void OnEnable()
	{
		dzc = MonoSingleton<DryZoneController>.Instance;
		foreach (Collider col in cols)
		{
			dzc.AddCollider(col);
		}
		dzc.dryZones.Add(this);
	}
}
