using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
	[SerializeField]
	private Magnet magnet;

	public bool drill;

	private bool drilling;

	private float drillCooldown;

	private bool hit;

	private bool stopped;

	private bool punched;

	public float damage;

	private float damageLeft;

	private AudioSource aud;

	public AudioClip environmentHitSound;

	public AudioClip enemyHitSound;

	private Rigidbody rb;

	private EnemyIdentifierIdentifier target;

	public AudioSource drillSound;

	private AudioSource currentDrillSound;

	public int drillHits;

	private int drillHitsLeft;

	private Vector3 startPosition;

	[SerializeField]
	private GameObject breakEffect;

	private FixedJoint fj;

	private TrailRenderer tr;

	[HideInInspector]
	public GameObject sourceWeapon;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		tr = GetComponent<TrailRenderer>();
		damageLeft = damage;
		if (drill)
		{
			drillHitsLeft = drillHits;
		}
		Invoke("DestroyIfNotHit", 5f);
		Invoke("MasterDestroy", 30f);
		Invoke("SlowUpdate", 2f);
		startPosition = base.transform.position;
		if (!TryGetComponent<SimplePortalTraveler>(out var _))
		{
			base.gameObject.AddComponent<SimplePortalTraveler>().SetType(PortalTravellerType.PLAYER_PROJECTILE);
		}
	}

	private void SlowUpdate()
	{
		if (Vector3.Distance(startPosition, base.transform.position) > 999f)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Invoke("SlowUpdate", 2f);
		}
	}

	private void Update()
	{
		if (!stopped && !punched && rb.velocity.magnitude > 1f)
		{
			base.transform.LookAt(base.transform.position + rb.velocity);
		}
		else if (drilling)
		{
			base.transform.Rotate(Vector3.forward, 14400f * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (stopped && drilling && (bool)target)
		{
			if (drillCooldown != 0f)
			{
				drillCooldown = Mathf.MoveTowards(drillCooldown, 0f, Time.deltaTime);
				return;
			}
			drillCooldown = 0.05f;
			if ((bool)target.eid)
			{
				target.eid.hitter = "drill";
				target.eid.DeliverDamage(target.gameObject, Vector3.zero, base.transform.position, 0.0625f, tryForExplode: false, 0f, sourceWeapon);
			}
			if ((bool)(Object)(object)currentDrillSound)
			{
				currentDrillSound.SetPitch(1.5f - (float)drillHitsLeft / (float)drillHits / 2f);
			}
			drillHitsLeft--;
			if (drillHitsLeft <= 0 && !PauseTimedBombs.Paused)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else if (drilling && target == null)
		{
			drilling = false;
			DelayedDestroyIfOnCorpse();
		}
	}

	private void OnDestroy()
	{
		if ((bool)target && (bool)target.eid && (bool)magnet && target.eid.stuckMagnets.Contains(magnet))
		{
			target.eid.stuckMagnets.Remove(magnet);
		}
		if (drill)
		{
			Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
		}
	}

	private void OnEnable()
	{
		if (stopped && (bool)target && (bool)target.eid && drill)
		{
			target.eid.drillers.Add(this);
		}
	}

	private void OnDisable()
	{
		if (stopped && (bool)target && (bool)target.eid && drill && target.eid.drillers.Contains(this))
		{
			target.eid.drillers.Remove(this);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!hit && other.gameObject.layer == 16 && other.TryGetComponent<HarpoonHelper>(out var component))
		{
			other = component.target;
			base.transform.position = other.transform.position;
		}
		GoreZone componentInParent = other.GetComponentInParent<GoreZone>();
		if (!hit && (other.gameObject.layer == 10 || other.gameObject.layer == 11) && (other.gameObject.CompareTag("Armor") || other.gameObject.CompareTag("Head") || other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Limb") || other.gameObject.CompareTag("EndLimb")))
		{
			if (!other.TryGetComponent<EnemyIdentifierIdentifier>(out var component2) || !component2.eid || ((bool)target && (bool)target.eid && component2.eid == target.eid) || (drill && component2.eid.harpooned) || ((bool)magnet && component2.eid.dead && component2.eid.enemyType != EnemyType.MaliciousFace && component2.eid.enemyType != EnemyType.Gutterman) || ((bool)target && component2.eid == target.eid))
			{
				return;
			}
			target = component2;
			hit = true;
			EnemyIdentifier eid = target.eid;
			eid.hitter = "harpoon";
			float health = eid.health;
			eid.DeliverDamage(other.gameObject, Vector3.zero, base.transform.position, damageLeft, tryForExplode: false, 0f, sourceWeapon);
			if (eid.mirrorOnly)
			{
				EnemyIdentifier.SendToPortalLayer(base.gameObject);
			}
			if (drill)
			{
				eid.drillers.Add(this);
			}
			if (health < damageLeft)
			{
				damageLeft -= health;
			}
			if (other.gameObject.layer == 10)
			{
				Debug.Log(other.gameObject.name, other.gameObject);
				if ((bool)other.gameObject.GetComponentInParent<Rigidbody>())
				{
					fj = base.gameObject.AddComponent<FixedJoint>();
					fj.connectedBody = other.gameObject.GetComponentInParent<Rigidbody>();
				}
				if (componentInParent != null)
				{
					base.transform.SetParent(componentInParent.transform, worldPositionStays: true);
				}
			}
			else
			{
				rb.velocity = Vector3.zero;
				rb.SetGravityMode(useGravity: false);
				rb.constraints = RigidbodyConstraints.FreezeAll;
				base.transform.SetParent(other.transform, worldPositionStays: true);
			}
			if (!magnet && eid.dead && !eid.harpooned && other.gameObject.layer == 10 && (!eid.machine || !eid.machine.specialDeath))
			{
				eid.harpooned = true;
				other.gameObject.transform.position = base.transform.position;
				rb?.AddForce(base.transform.forward, ForceMode.VelocityChange);
				if (drill)
				{
					hit = false;
				}
			}
			else
			{
				stopped = true;
				if (drill)
				{
					drilling = true;
					currentDrillSound = Object.Instantiate<AudioSource>(drillSound, base.transform.position, base.transform.rotation);
					((Component)(object)currentDrillSound).transform.SetParent(base.transform, worldPositionStays: true);
				}
				rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				tr.emitting = false;
				TimeBomb component3 = GetComponent<TimeBomb>();
				if (component3 != null)
				{
					component3.StartCountdown();
				}
				if (magnet != null)
				{
					magnet.onEnemy = eid;
					magnet.ignoredEids.Add(eid);
					magnet.ExitEnemy(eid);
					if (eid.enemyType != EnemyType.FleshPrison && eid.enemyType != EnemyType.FleshPanopticon)
					{
						magnet.transform.position = other.bounds.center;
					}
					if (!eid.stuckMagnets.Contains(magnet))
					{
						eid.stuckMagnets.Add(magnet);
					}
					if (!component2.eid.dead)
					{
						Breakable[] componentsInChildren = GetComponentsInChildren<Breakable>();
						if (componentsInChildren.Length != 0)
						{
							Breakable[] array = componentsInChildren;
							for (int i = 0; i < array.Length; i++)
							{
								Object.Destroy(array[i].gameObject);
							}
						}
					}
				}
			}
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = enemyHitSound;
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.volume = 0.4f;
			aud.Play(tracked: true);
			Object.Destroy(GetComponent<SimplePortalTraveler>());
		}
		else
		{
			if (stopped || !LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				return;
			}
			if (drill && !hit)
			{
				Glass component5;
				if (other.gameObject.TryGetComponent<Breakable>(out var component4) && !component4.unbreakable && !component4.precisionOnly && !component4.specialCaseOnly)
				{
					component4.Break();
				}
				else if (other.gameObject.TryGetComponent<Glass>(out component5) && !component5.broken)
				{
					component5.Shatter();
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
				return;
			}
			stopped = true;
			hit = true;
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			rb.isKinematic = true;
			if (other.gameObject.CompareTag("Armor") || other.gameObject.CompareTag("Door") || other.gameObject.CompareTag("Moving") || ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform)))
			{
				Rigidbody component6 = other.gameObject.GetComponent<Rigidbody>();
				if ((bool)component6)
				{
					Debug.Log(other.gameObject.name, other.gameObject);
					base.gameObject.AddComponent<FixedJoint>().connectedBody = component6;
					rb.isKinematic = false;
				}
				else
				{
					GameObject gameObject = new GameObject("ScaleFixer");
					gameObject.transform.position = base.transform.position;
					gameObject.transform.rotation = other.transform.rotation;
					gameObject.transform.SetParent(other.transform, worldPositionStays: true);
					base.transform.SetParent(gameObject.transform, worldPositionStays: true);
				}
				hit = true;
				if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Contains(other.transform) && other.transform.TryGetComponent<ScrollingTexture>(out var component7))
				{
					component7.attachedObjects.Add(base.transform);
					if (TryGetComponent<BoxCollider>(out var component8))
					{
						component7.specialScrollers.Add(new WaterDryTracker(base.transform, component8.ClosestPoint(other.ClosestPoint(base.transform.position + base.transform.forward * component8.size.z * base.transform.lossyScale.z)) - base.transform.position));
					}
				}
			}
			else if ((bool)componentInParent)
			{
				base.transform.SetParent(componentInParent.transform, worldPositionStays: true);
			}
			else
			{
				GoreZone[] array2 = Object.FindObjectsOfType<GoreZone>();
				if (array2 != null && array2.Length != 0)
				{
					GoreZone goreZone = array2[0];
					if (array2.Length > 1)
					{
						for (int j = 1; j < array2.Length; j++)
						{
							if (array2[j].gameObject.activeInHierarchy && Vector3.Distance(goreZone.transform.position, base.transform.position) > Vector3.Distance(array2[j].transform.position, base.transform.position))
							{
								goreZone = array2[j];
							}
						}
					}
					base.transform.SetParent(goreZone.transform, worldPositionStays: true);
				}
			}
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = environmentHitSound;
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.volume = 0.4f;
			aud.Play(tracked: true);
			tr.emitting = false;
			TimeBomb component9 = GetComponent<TimeBomb>();
			if (component9 != null)
			{
				component9.StartCountdown();
			}
			Object.Destroy(GetComponent<SimplePortalTraveler>());
		}
	}

	public void Punched()
	{
		hit = false;
		stopped = false;
		drilling = false;
		punched = true;
		damageLeft = damage;
		CancelInvoke("DestroyIfNotHit");
		Invoke("DestroyIfNotHit", 5f);
		CancelInvoke("MasterDestroy");
		Invoke("MasterDestroy", 30f);
		CancelInvoke("DestroyIfOnCorpse");
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: false);
		rb.AddForce(base.transform.forward * 150f, ForceMode.VelocityChange);
		aud.Stop();
		rb.constraints = RigidbodyConstraints.None;
		base.transform.SetParent(null, worldPositionStays: true);
		if ((bool)tr)
		{
			tr.emitting = true;
		}
		if (base.gameObject.layer == 30)
		{
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.gameObject.layer = ((renderer.gameObject == base.gameObject) ? 14 : 0);
			}
		}
		if ((bool)target && (bool)target.eid)
		{
			target.eid.drillers.Remove(this);
			target.eid.hitter = "drillpunch";
			target.eid.DeliverDamage(target.gameObject, base.transform.forward * 150f, base.transform.position, 4f + (float)drillHitsLeft * 0.0625f, tryForExplode: true);
			if ((bool)fj)
			{
				Object.Destroy(fj);
			}
			if ((bool)(Object)(object)currentDrillSound)
			{
				Object.Destroy((Object)(object)currentDrillSound);
			}
		}
		drillHitsLeft = drillHits;
	}

	private void DestroyIfNotHit()
	{
		if (!hit && !PauseTimedBombs.Paused)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void MasterDestroy()
	{
		if (!PauseTimedBombs.Paused && !NoWeaponCooldown.NoCooldown)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void DelayedDestroyIfOnCorpse(float delay = 1f)
	{
		Invoke("DestroyIfOnCorpse", delay);
	}

	private void DestroyIfOnCorpse()
	{
		if ((bool)target && (!target.eid || target.eid.dead))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
