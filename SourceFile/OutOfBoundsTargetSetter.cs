using UnityEngine;

public class OutOfBoundsTargetSetter : MonoBehaviour
{
	public DeathZone[] deathZones;

	public OutOfBounds[] oobs;

	private void Start()
	{
		if (!TryGetComponent<Collider>(out var _) && !TryGetComponent<Rigidbody>(out var _))
		{
			Activate();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Activate();
		}
	}

	public void Activate()
	{
		bool flag = false;
		if (deathZones == null || deathZones.Length == 0)
		{
			deathZones = Object.FindObjectsOfType<DeathZone>();
		}
		else
		{
			flag = true;
		}
		DeathZone[] array = deathZones;
		foreach (DeathZone deathZone in array)
		{
			if ((bool)deathZone && (!deathZone.dontChangeRespawnTarget || flag))
			{
				deathZone.respawnTarget = base.transform.position;
			}
		}
		if (oobs == null || oobs.Length == 0)
		{
			oobs = Object.FindObjectsOfType<OutOfBounds>();
		}
		OutOfBounds[] array2 = oobs;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].overrideResetPosition = base.transform.position;
		}
	}
}
