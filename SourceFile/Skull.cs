using UnityEngine;

public class Skull : MonoBehaviour
{
	private Light lit;

	private float origRange;

	private float litTime;

	private AudioSource aud;

	private ModifyMaterial mod;

	private void Awake()
	{
		lit = GetComponent<Light>();
		origRange = lit.range;
		aud = GetComponent<AudioSource>();
		mod = GetComponentInChildren<ModifyMaterial>();
	}

	private void Update()
	{
		if (litTime > 0f)
		{
			litTime = Mathf.MoveTowards(litTime, 0f, Time.deltaTime);
		}
		else if (lit.range > origRange)
		{
			lit.range = Mathf.MoveTowards(lit.range, origRange, Time.deltaTime * 5f);
		}
	}

	public void PunchWith()
	{
		if (lit.range == origRange)
		{
			litTime = 1f;
			lit.range = origRange * 2.5f;
			aud.Play(tracked: true);
		}
	}

	public void HitWith(GameObject target)
	{
		Flammable component = target.gameObject.GetComponent<Flammable>();
		if (component != null && !component.enemyOnly)
		{
			component.Burn(4f);
		}
	}

	public void HitSurface(RaycastHit hit)
	{
		MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(hit.point);
	}

	public void PutDown()
	{
		if (lit.enabled)
		{
			aud.Stop();
		}
	}

	public void OnCorrectUse()
	{
		lit.enabled = false;
		mod.ChangeEmissionIntensity(1f);
	}

	public void OffCorrectUse()
	{
		lit.enabled = true;
		mod.ChangeEmissionIntensity(0f);
	}
}
