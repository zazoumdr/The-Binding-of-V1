using System.Collections.Generic;
using System.Threading;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class Glass : MonoBehaviour, ITarget
{
	public bool broken;

	public bool wall;

	private Transform[] glasses;

	public GameObject shatterParticle;

	private int kills;

	private Collider[] cols;

	private List<GameObject> enemies = new List<GameObject>();

	public UltrakillEvent onShatter;

	private CancellationTokenSource becomeObsticleTokenSource;

	private Vector3 cachedPos;

	private Quaternion cachedRot;

	public int Id => GetInstanceID();

	public TargetType Type => TargetType.GLASS;

	public EnemyIdentifier EID => null;

	public GameObject GameObject
	{
		get
		{
			if (!(this == null))
			{
				return base.gameObject;
			}
			return null;
		}
	}

	public Rigidbody Rigidbody => null;

	public Transform Transform => base.transform;

	public Vector3 Position => cachedPos;

	public Vector3 HeadPosition => cachedPos;

	private void OnEnable()
	{
		becomeObsticleTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { base.destroyCancellationToken });
		MonoSingleton<CoinTracker>.Instance.RegisterTarget(this, becomeObsticleTokenSource.Token);
	}

	private void OnDisable()
	{
		if (becomeObsticleTokenSource != null)
		{
			becomeObsticleTokenSource.Cancel();
			becomeObsticleTokenSource.Dispose();
		}
	}

	public void Shatter()
	{
		if (broken)
		{
			return;
		}
		cols = GetComponentsInChildren<Collider>();
		broken = true;
		becomeObsticleTokenSource.Cancel();
		onShatter?.Invoke();
		glasses = base.transform.GetComponentsInChildren<Transform>();
		Transform[] array = glasses;
		foreach (Transform transform in array)
		{
			if (transform.gameObject != base.gameObject)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		Collider[] array2 = cols;
		foreach (Collider collider in array2)
		{
			if (!collider.isTrigger)
			{
				collider.enabled = false;
			}
		}
		foreach (GameObject enemy in enemies)
		{
			if (enemy != null && enemy.TryGetComponent<GroundCheckEnemy>(out var _))
			{
				kills++;
			}
		}
		if (TryGetComponent<BloodstainParent>(out var component2))
		{
			Object.Destroy(component2);
		}
		Invoke("BecomeObstacle", 0.5f);
		GameObject gameObject = Object.Instantiate(shatterParticle, base.transform);
		if (base.gameObject.layer == 24)
		{
			array = gameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.layer = 24;
			}
		}
		base.gameObject.layer = 17;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!broken && other.gameObject.layer == 20 && !enemies.Contains(other.gameObject))
		{
			enemies.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!broken && other.gameObject.layer == 20 && enemies.Contains(other.gameObject))
		{
			enemies.Remove(other.gameObject);
		}
	}

	private void BecomeObstacle()
	{
		NavMeshObstacle component = GetComponent<NavMeshObstacle>();
		if (wall)
		{
			component.carving = false;
			((Behaviour)(object)component).enabled = false;
		}
		else
		{
			((Behaviour)(object)component).enabled = true;
			Collider[] array = cols;
			foreach (Collider collider in array)
			{
				if (collider != null && !collider.isTrigger)
				{
					collider.enabled = false;
				}
			}
		}
		if (kills >= 3)
		{
			StatsManager instance = MonoSingleton<StatsManager>.Instance;
			if (instance.maxGlassKills < kills)
			{
				instance.maxGlassKills = kills;
			}
		}
		base.enabled = false;
		becomeObsticleTokenSource.Dispose();
	}

	public void SetData(ref TargetData data)
	{
		data.position = cachedPos;
		data.realPosition = cachedPos;
		data.rotation = cachedRot;
		data.velocity = default(Vector3);
	}

	public void UpdateCachedTransformData()
	{
		base.transform.GetPositionAndRotation(out cachedPos, out cachedRot);
	}
}
