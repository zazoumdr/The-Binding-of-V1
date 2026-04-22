using Unity.Burst;
using UnityEngine;

public class Bloodsplatter : MonoBehaviour
{
	public BSType bloodSplatterType;

	[HideInInspector]
	public ParticleSystem part;

	private int i;

	private AudioSource aud;

	private int eidID;

	private SpriteRenderer sr;

	private MeshRenderer mr;

	private NewMovement nmov;

	public int hpAmount;

	private SphereCollider col;

	public bool hpOnParticleCollision;

	[HideInInspector]
	public bool beenPlayed;

	public bool halfChance;

	public bool ready;

	public GoreZone gz;

	public bool underwater;

	private MaterialPropertyBlock propertyBlock;

	private bool canCollide = true;

	public BloodsplatterManager bsm;

	[HideInInspector]
	public bool fromExplosion;

	private ComponentsDatabase cdatabase;

	[HideInInspector]
	public EnemyIdentifier eid
	{
		set
		{
			if (value != null)
			{
				eidID = value.GetInstanceID();
			}
		}
	}

	private void Awake()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		if (!(Object)(object)part)
		{
			part = GetComponent<ParticleSystem>();
		}
		if ((Object)(object)part == null)
		{
			part = GetComponentInChildren<ParticleSystem>();
		}
		if (!(Object)(object)aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (!col)
		{
			col = GetComponent<SphereCollider>();
		}
		cdatabase = MonoSingleton<ComponentsDatabase>.Instance;
		MainModule main = part.main;
		((MainModule)(ref main)).stopAction = (ParticleSystemStopAction)3;
		((Component)(object)part).AddListener<ParticleSystemStoppedMessage>(Repool);
	}

	private void OnEnable()
	{
		if (!gz)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		bsm.splatters[GetInstanceID()] = this;
		if (beenPlayed)
		{
			if ((bool)col && col.enabled)
			{
				if (underwater)
				{
					Invoke("DisableCollider", 2.5f);
				}
				else
				{
					Invoke("DisableCollider", 0.25f);
				}
			}
			return;
		}
		beenPlayed = true;
		if (bsm == null)
		{
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
		}
		bool flag = bsm.forceOn || MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled");
		if ((bool)(Object)(object)part)
		{
			part.Clear();
			if (flag)
			{
				part.Play();
			}
		}
		canCollide = true;
		if ((Object)(object)aud != null)
		{
			aud.SetPitch(Random.Range(0.75f, 1.5f));
			aud.Play(tracked: true);
		}
		if ((bool)col)
		{
			col.enabled = true;
		}
		if (underwater)
		{
			Invoke("DisableCollider", 2.5f);
		}
		else
		{
			Invoke("DisableCollider", 0.25f);
		}
	}

	private void OnDisable()
	{
		if ((bool)col)
		{
			col.enabled = false;
		}
		CancelInvoke("DisableCollider");
		ready = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		Collide(other);
	}

	private void Collide(Collider other)
	{
		if (ready && !(bsm == null))
		{
			if (bsm.hasBloodFillers && ((bsm.bloodFillers.Contains(other.gameObject) && other.gameObject.TryGetComponent<BloodFiller>(out var component)) || ((bool)other.attachedRigidbody && bsm.bloodFillers.Contains(other.attachedRigidbody.gameObject) && other.attachedRigidbody.TryGetComponent<BloodFiller>(out component))))
			{
				component.FillBloodSlider(hpAmount, base.transform.position, eidID);
			}
			else if (canCollide && other.gameObject.CompareTag("Player"))
			{
				MonoSingleton<NewMovement>.Instance.GetHealth(hpAmount, silent: false, fromExplosion);
				DisableCollider();
			}
		}
	}

	public void Repool()
	{
		if (bloodSplatterType == BSType.dontpool)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (bloodSplatterType == BSType.unknown)
		{
			Debug.LogWarning(base.gameObject?.ToString() + "has an unknown BSType, this shouldn't happen!");
			Object.Destroy(base.gameObject);
			return;
		}
		CancelInvoke("DisableCollider");
		gz = null;
		eid = null;
		fromExplosion = false;
		ready = false;
		beenPlayed = false;
		base.transform.localScale = Vector3.one;
		if ((bool)bsm)
		{
			bsm.RepoolGore(this, bloodSplatterType);
		}
	}

	[BurstCompile]
	public void CreateBloodstain(in RaycastHit hit, BloodsplatterManager bsman)
	{
		bsm = bsman;
		Collider collider = hit.collider;
		if (!collider)
		{
			return;
		}
		Rigidbody rigidbody = hit.rigidbody;
		GameObject gameObject = (rigidbody ? rigidbody.gameObject : collider.gameObject);
		Debug.Log("Bloodstain collision with: " + gameObject.name, gameObject);
		if (StockMapInfo.Instance.continuousGibCollisions)
		{
			Debug.Log("Hit 7-S continuous gib collision object: " + gameObject.name, gameObject);
			if (gameObject.TryGetComponent<IBloodstainReceiver>(out var component) && component.HandleBloodstainHit(in hit))
			{
				Debug.Log("Bloodstain receiver handled the hit, playing blood sound and returning");
				return;
			}
		}
		if (ready && hpOnParticleCollision && gameObject.CompareTag("Player"))
		{
			MonoSingleton<NewMovement>.Instance.GetHealth(3, silent: false, fromExplosion);
			return;
		}
		Transform item = gameObject.transform;
		float bloodstainChance = bsm.GetBloodstainChance();
		bloodstainChance = (halfChance ? (bloodstainChance / 2f) : bloodstainChance);
		if (!((float)Random.Range(0, 100) < bloodstainChance))
		{
			return;
		}
		bool flag = gameObject.CompareTag("Wall");
		bool flag2 = !flag && gameObject.CompareTag("Floor");
		bool flag3 = !flag && gameObject.CompareTag("Moving");
		bool flag4 = !flag && gameObject.CompareTag("Glass");
		bool flag5 = !flag && gameObject.CompareTag("GlassFloor");
		if (!(flag || flag2 || flag3 || flag4 || flag5))
		{
			return;
		}
		bool flag6 = false;
		bool clipToSurface = true;
		if (flag || flag2)
		{
			gameObject.TryGetComponent<MeshRenderer>(out var _);
		}
		bool flag7 = flag3 || flag4 || flag5;
		if (!flag7)
		{
			flag7 |= (bool)cdatabase && cdatabase.scrollers.Contains(item);
		}
		if (flag7)
		{
			ScrollingTexture component3;
			BloodstainParent bloodstainParent = (gameObject.TryGetComponent<ScrollingTexture>(out component3) ? component3.parent : gameObject.GetOrAddComponent<BloodstainParent>());
			if (MonoSingleton<BloodsplatterManager>.Instance.usedComputeShadersAtStart)
			{
				bloodstainParent.CreateChild(flag6 ? (hit.point + hit.normal * 0.2f) : hit.point, hit.normal, clipToSurface, fromStep: false);
			}
		}
		else
		{
			gz.stains.CreateChild(flag6 ? (hit.point + hit.normal * 0.2f) : hit.point, hit.normal, clipToSurface, fromStep: true);
		}
	}

	private void DisableCollider()
	{
		canCollide = false;
		if (part.isStopped)
		{
			Repool();
		}
	}

	public void GetReady()
	{
		ready = true;
	}
}
