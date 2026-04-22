using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class MassSpear : MonoBehaviour
{
	[Obsolete]
	public EnemyTarget target;

	public TargetHandle targetHandle;

	private LineRenderer lr;

	private Rigidbody rb;

	public bool hittingPlayer;

	public bool hitPlayer;

	public bool beenStopped;

	private bool returning;

	private bool deflected;

	public Transform originPoint;

	private NewMovement nmov;

	public float spearHealth;

	[HideInInspector]
	public int difficulty;

	public GameObject breakMetalSmall;

	private AudioSource aud;

	public AudioClip hit;

	public AudioClip stop;

	private Mass mass;

	public float speedMultiplier = 1f;

	public float damageMultiplier = 1f;

	private Stack<PortalHandle> traversedPortals = new Stack<PortalHandle>();

	private Vector3 lastPosition;

	private float distanceTravelled;

	private SimplePortalTraveler portalTraveler;

	private void Start()
	{
		lr = GetComponentInChildren<LineRenderer>();
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		mass = originPoint.GetComponentInParent<Mass>();
		Invoke("CheckForDistance", 3f / speedMultiplier);
		float num = 75f;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			num = 250f;
			break;
		case 2:
			num = 200f;
			break;
		case 1:
			num = 75f;
			break;
		}
		rb.AddForce(num * speedMultiplier * base.transform.forward, ForceMode.VelocityChange);
		lastPosition = lr.transform.position;
		distanceTravelled = 0f;
	}

	private void OnEnable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			if (portalTraveler == null && !TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType(PortalTravellerType.ENEMY_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
		}
	}

	private void OnDisable()
	{
		if (!returning)
		{
			Return();
		}
		traversedPortals.Clear();
		lastPosition = lr.transform.position;
		distanceTravelled = 0f;
		if (portalTraveler != null)
		{
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
		}
	}

	private void Update()
	{
		if (originPoint != null && targetHandle != null && !originPoint.gameObject.activeInHierarchy)
		{
			Vector3 position = originPoint.position;
			Vector3 position2 = lr.transform.position;
			lr.SetPosition(0, position);
			lr.SetPosition(1, position2);
			distanceTravelled += Vector3.Distance(lastPosition, position2);
			lastPosition = position2;
			Vector3 direction = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(targetHandle).position - position;
			direction.Normalize();
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
			PortalPhysicsV2.Raycast(position, direction, distanceTravelled, layerMask, out var _, out var portalTraversals, out var _);
			this.GenerateLineRendererSegments(lr, portalTraversals);
			if (returning)
			{
				if (!originPoint || !originPoint.parent || !originPoint.parent.gameObject.activeInHierarchy)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				Vector3 vector = originPoint.position;
				if (traversedPortals.TryPeek(out var result))
				{
					vector = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(result).GetTravelMatrix(result.side).MultiplyPoint3x4(vector);
				}
				base.transform.rotation = Quaternion.LookRotation(base.transform.position - vector, Vector3.up);
				rb.velocity = -100f * speedMultiplier * base.transform.forward;
				if (Vector3.Distance(base.transform.position, originPoint.position) < 1f)
				{
					if (mass != null)
					{
						mass.SpearReturned();
					}
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			else if (deflected)
			{
				base.transform.LookAt(originPoint.position);
				rb.velocity = 300f * speedMultiplier * base.transform.forward;
				if (!(Vector3.Distance(base.transform.position, originPoint.position) < 1f) || !(mass != null))
				{
					return;
				}
				mass.SpearReturned();
				BloodsplatterManager instance = MonoSingleton<BloodsplatterManager>.Instance;
				EnemyIdentifier component = mass.GetComponent<EnemyIdentifier>();
				Transform child = mass.tailEnd.GetChild(0);
				HurtEnemy(child.gameObject, component);
				for (int i = 0; i < 3; i++)
				{
					GameObject gore = instance.GetGore(GoreType.Head, component);
					gore.transform.position = child.position;
					GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
					if ((bool)goreZone)
					{
						gore.transform.SetParent(goreZone.goreZone);
					}
				}
				mass.SpearParried();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (hitPlayer && !returning)
			{
				if (nmov.hp <= 0)
				{
					Return();
					UnityEngine.Object.Destroy(base.gameObject);
				}
				if (spearHealth > 0f)
				{
					spearHealth = Mathf.MoveTowards(spearHealth, 0f, Time.deltaTime);
				}
				else if (spearHealth <= 0f)
				{
					Return();
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void OnPortalTraversal(in PortalTravelDetails details)
	{
		lastPosition = base.transform.position;
		for (int i = 0; i < details.portalSequence.Count; i++)
		{
			PortalHandle item = details.portalSequence[i];
			if (traversedPortals.TryPeek(out var result))
			{
				if (result.instanceId == item.instanceId && result.side.Reverse() == item.side)
				{
					traversedPortals.Pop();
				}
			}
			else
			{
				traversedPortals.Push(item);
			}
		}
	}

	private void HurtEnemy(GameObject target, EnemyIdentifier eid = null)
	{
		if (eid == null)
		{
			eid = target.GetComponent<EnemyIdentifier>();
			if (!eid)
			{
				EnemyIdentifierIdentifier component = target.GetComponent<EnemyIdentifierIdentifier>();
				if ((bool)component)
				{
					eid = component.eid;
				}
			}
		}
		if (eid != null && target == null)
		{
			target = eid.gameObject;
		}
		if ((bool)eid)
		{
			eid.DeliverDamage(target, Vector3.zero, originPoint.position, 30f * damageMultiplier, tryForExplode: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (beenStopped)
		{
			return;
		}
		if (!hitPlayer && !hittingPlayer && other.gameObject.CompareTag("Player"))
		{
			hittingPlayer = true;
			beenStopped = true;
			rb.isKinematic = true;
			rb.SetGravityMode(useGravity: false);
			rb.velocity = Vector3.zero;
			base.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos();
			Invoke("DelayedPlayerCheck", 0.05f);
		}
		else if (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
		{
			beenStopped = true;
			rb.velocity = Vector3.zero;
			rb.SetGravityMode(useGravity: false);
			base.transform.position += base.transform.forward * 2f;
			Invoke("Return", 2f / speedMultiplier);
			aud.SetPitch(1f);
			aud.clip = stop;
			aud.Play(tracked: true);
		}
		else if (target != null && target.isEnemy && (other.gameObject.CompareTag("Head") || other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Limb") || other.gameObject.CompareTag("EndLimb")) && !other.gameObject.CompareTag("Armor"))
		{
			EnemyIdentifierIdentifier componentInParent = other.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
			EnemyIdentifier enemyIdentifier = null;
			if (componentInParent != null && componentInParent.eid != null)
			{
				enemyIdentifier = componentInParent.eid;
			}
			if (!(enemyIdentifier == null) && !(enemyIdentifier != target.enemyIdentifier) && enemyIdentifier != null)
			{
				HurtEnemy(other.gameObject, enemyIdentifier);
				Return();
			}
		}
	}

	private void DelayedPlayerCheck()
	{
		if (!deflected)
		{
			hittingPlayer = false;
			hitPlayer = true;
			nmov = MonoSingleton<NewMovement>.Instance;
			nmov.GetHurt(Mathf.RoundToInt(25f * damageMultiplier), invincible: true);
			nmov.slowMode = true;
			base.transform.position = nmov.transform.position;
			base.transform.SetParent(nmov.transform, worldPositionStays: true);
			rb.velocity = Vector3.zero;
			rb.SetGravityMode(useGravity: false);
			rb.isKinematic = true;
			beenStopped = true;
			GetComponent<CapsuleCollider>().radius *= 0.5f;
			aud.SetPitch(1f);
			aud.clip = hit;
			aud.Play(tracked: true);
		}
	}

	public void GetHurt(float damage)
	{
		UnityEngine.Object.Instantiate(breakMetalSmall, base.transform.position, Quaternion.identity);
		spearHealth -= ((difficulty >= 4) ? (damage / 1.5f) : damage);
	}

	public void Deflected()
	{
		deflected = true;
		rb.isKinematic = false;
		GetComponent<Collider>().enabled = false;
	}

	private void Return()
	{
		if (hitPlayer)
		{
			nmov.slowMode = false;
			base.transform.SetParent(null, worldPositionStays: true);
			rb.isKinematic = false;
		}
		if (base.gameObject.activeInHierarchy)
		{
			aud.clip = stop;
			aud.SetPitch(1f);
			aud.Play(tracked: true);
		}
		returning = true;
		beenStopped = true;
	}

	private void CheckForDistance()
	{
		if (!returning && !beenStopped && !hitPlayer && !deflected)
		{
			returning = true;
			beenStopped = true;
			base.transform.position = originPoint.position;
		}
	}
}
