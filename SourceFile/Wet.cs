using UnityEngine;

public class Wet : MonoBehaviour
{
	private ParticleSystem wetParticle;

	public float wetness;

	private bool drying;

	private void Start()
	{
		wetness = 5f;
	}

	private void Update()
	{
		if (!drying)
		{
			return;
		}
		if (wetness > 0f)
		{
			wetness = Mathf.MoveTowards(wetness, 0f, Time.deltaTime);
			return;
		}
		Flammable[] componentsInChildren = GetComponentsInChildren<Flammable>();
		if (componentsInChildren != null)
		{
			Flammable[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].wet = false;
			}
		}
		ReturnSoon();
	}

	public void Dry(Vector3 position)
	{
		drying = true;
		if (!(Object)(object)wetParticle)
		{
			wetParticle = MonoSingleton<PooledWaterStore>.Instance.GetFromQueue(Water.WaterGOType.wetparticle).GetComponent<ParticleSystem>();
			((Component)(object)wetParticle).transform.SetParent(base.transform, worldPositionStays: true);
			((Component)(object)wetParticle).transform.localPosition = position;
		}
		wetParticle.Play();
	}

	public void Refill()
	{
		drying = false;
		wetness = 5f;
		if ((bool)(Object)(object)wetParticle)
		{
			wetParticle.Stop();
		}
	}

	private void ReturnSoon()
	{
		if ((bool)(Object)(object)wetParticle)
		{
			wetParticle.Stop();
		}
		Invoke("ReturnWetParticle", 1f);
	}

	private void ReturnWetParticle()
	{
		if ((bool)(Object)(object)wetParticle)
		{
			MonoSingleton<PooledWaterStore>.Instance.ReturnToQueue(((Component)(object)wetParticle).gameObject, Water.WaterGOType.wetparticle);
			wetParticle = null;
		}
	}
}
