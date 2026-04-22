using UnityEngine;
using UnityEngine.AI;

public class SplashContinuous : MonoBehaviour
{
	private bool active = true;

	private float cooldown;

	[SerializeField]
	private Water.WaterGOType waterGOType;

	[SerializeField]
	private ParticleSystem particles;

	[SerializeField]
	private GameObject wadingSound;

	[SerializeField]
	private AudioClip[] wadingSounds;

	[SerializeField]
	private float wadingSoundPitch = 0.8f;

	private Vector3 previousPosition;

	[SerializeField]
	private float movingEmissionRate = 20f;

	[SerializeField]
	private float stillEmissionRate = 2f;

	[HideInInspector]
	public NavMeshAgent nma;

	private PooledWaterStore waterStore;

	private void Start()
	{
		waterStore = MonoSingleton<PooledWaterStore>.Instance;
	}

	private void OnEnable()
	{
		active = true;
	}

	private void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!active)
		{
			return;
		}
		EmissionModule emission = particles.emission;
		if (((bool)(Object)(object)nma && nma.velocity.magnitude > 4f) || Vector3.Distance(base.transform.position, previousPosition) > 0.05f)
		{
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(movingEmissionRate);
			if (cooldown == 0f)
			{
				if (Object.Instantiate(wadingSound, base.transform).TryGetComponent<AudioSource>(out var component))
				{
					component.clip = wadingSounds[Random.Range(0, wadingSounds.Length)];
					component.SetPitch(Random.Range(wadingSoundPitch - 0.05f, wadingSoundPitch + 0.05f));
					component.Play();
				}
				cooldown = 0.75f;
			}
		}
		else
		{
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(stillEmissionRate);
		}
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * (1f + Vector3.Distance(base.transform.position, previousPosition) * 5f));
		previousPosition = base.transform.position;
	}

	public void ReturnSoon()
	{
		particles.Stop();
		active = false;
		Invoke("ReturnToQueue", 2f);
	}

	private void ReturnToQueue()
	{
		if (waterGOType == Water.WaterGOType.continuous && (bool)waterStore)
		{
			waterStore.ReturnToQueue(base.gameObject, waterGOType);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
