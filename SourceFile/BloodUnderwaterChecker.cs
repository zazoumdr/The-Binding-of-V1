using UnityEngine;

public class BloodUnderwaterChecker : MonoBehaviour
{
	private bool cancelled;

	private DryZoneController dzc;

	private void OnEnable()
	{
		dzc = MonoSingleton<DryZoneController>.Instance;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (cancelled || other.gameObject.layer != 4)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 vector = new Vector3(position.x, position.y + 1.5f, position.z);
		if (!(Vector3.Distance(other.ClosestPointOnBounds(vector), vector) < 0.5f))
		{
			return;
		}
		if (dzc.dryZones != null && dzc.dryZones.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(position, 0.01f, 65536, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].TryGetComponent<DryZone>(out var _))
				{
					base.gameObject.SetActive(value: false);
					cancelled = true;
					return;
				}
			}
		}
		GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Body, isUnderwater: true);
		if (!gore)
		{
			return;
		}
		Bloodsplatter component2 = base.transform.parent.GetComponent<Bloodsplatter>();
		Bloodsplatter component3 = gore.GetComponent<Bloodsplatter>();
		if ((bool)component3 && (bool)component2)
		{
			component3.hpAmount = component2.hpAmount;
			component3.fromExplosion = component2.fromExplosion;
			if (component2.ready)
			{
				component3.GetReady();
			}
		}
		gore.transform.position = base.transform.position;
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		if (componentInParent != null && componentInParent.goreZone != null)
		{
			gore.transform.SetParent(componentInParent.goreZone, worldPositionStays: true);
		}
		gore.SetActive(value: true);
		base.transform.parent.gameObject.SetActive(value: false);
	}
}
