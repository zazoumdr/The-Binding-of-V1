using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using UnityEngine;

public class Cannonball : MonoBehaviour, ITarget
{
	public bool launchable = true;

	[SerializeField]
	public bool launched;

	private Rigidbody rb;

	private Collider col;

	[SerializeField]
	private GameObject breakEffect;

	private bool checkingForBreak;

	private bool broken;

	public float damage;

	public float speed;

	public bool parry;

	[HideInInspector]
	public Sisyphus sisy;

	public bool ghostCollider;

	public bool canBreakBeforeLaunched;

	[Header("Physics Cannonball Settings")]
	public bool physicsCannonball;

	public AudioSource bounceSound;

	[HideInInspector]
	public List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public int maxBounces;

	private int currentBounces;

	[HideInInspector]
	public bool hasBounced;

	[HideInInspector]
	public bool forceMaxSpeed;

	public int durability = 99;

	[SerializeField]
	private GameObject interruptionExplosion;

	[SerializeField]
	private GameObject groundHitShockwave;

	[HideInInspector]
	public GameObject sourceWeapon;

	private TimeSince instaBreakDefence;

	private Vector3 cachedPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

	private SimplePortalTraveler portalTraveler;

	public int Id => GetInstanceID();

	public TargetType Type => TargetType.EXPLOSIVE;

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

	public Rigidbody Rigidbody => rb;

	public Transform Transform => base.transform;

	public Vector3 Position => cachedPos;

	public Vector3 HeadPosition => cachedPos;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
		instaBreakDefence = 1f;
		if (physicsCannonball)
		{
			MonoSingleton<ObjectTracker>.Instance.AddCannonball(this);
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			if (!TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType(PortalTravellerType.PLAYER_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravelBlocked = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravelBlocked, new PortalManagerV2.TravelCallback(OnPortalBlocked));
		}
		if (!sisy)
		{
			MonoSingleton<CoinTracker>.Instance.RegisterTarget(this, base.destroyCancellationToken);
		}
	}

	private void OnDestroy()
	{
		if (physicsCannonball && (bool)MonoSingleton<ObjectTracker>.Instance)
		{
			MonoSingleton<ObjectTracker>.Instance.RemoveCannonball(this);
		}
	}

	private void FixedUpdate()
	{
		if (launched)
		{
			rb.velocity = base.transform.forward * speed;
		}
		if (physicsCannonball && (bool)groundHitShockwave && rb.velocity.magnitude > 0f && rb.SweepTest(rb.velocity.normalized, out var hitInfo, rb.velocity.magnitude * Time.fixedDeltaTime) && LayerMaskDefaults.IsMatchingLayer(hitInfo.transform.gameObject.layer, LMD.Environment) && Vector3.Angle(hitInfo.normal, Vector3.up) < 45f)
		{
			GameObject obj = UnityEngine.Object.Instantiate(groundHitShockwave, hitInfo.point + hitInfo.normal * 0.1f, Quaternion.identity);
			obj.transform.up = hitInfo.normal;
			if (obj.TryGetComponent<PhysicalShockwave>(out var component))
			{
				component.force = 10000f + rb.velocity.magnitude * 80f;
			}
			Break();
		}
		if (hitEnemies.Count <= 0)
		{
			return;
		}
		for (int num = hitEnemies.Count - 1; num >= 0; num--)
		{
			if (hitEnemies[num] == null || Vector3.Distance(base.transform.position, hitEnemies[num].transform.position) > 20f)
			{
				hitEnemies.RemoveAt(num);
			}
		}
	}

	public void Launch()
	{
		if (launchable)
		{
			launched = true;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: false);
			col.isTrigger = true;
			hitEnemies.Clear();
			InstaBreakDefenceCancel();
			if (currentBounces == 1 && hasBounced)
			{
				damage += 2f;
			}
			currentBounces++;
			if ((bool)sisy)
			{
				sisy.GotParried();
			}
		}
	}

	public void Unlaunch(bool relaunchable = true)
	{
		launchable = relaunchable;
		launched = false;
		if ((bool)rb)
		{
			rb.isKinematic = !physicsCannonball;
			rb.SetGravityMode(physicsCannonball);
			rb.velocity = Vector3.zero;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!ghostCollider || (launched && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 12)))
		{
			Collide(other);
		}
	}

	public void Collide(Collider other)
	{
		if (launched || canBreakBeforeLaunched)
		{
			if (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				Glass component2;
				if (other.gameObject.TryGetComponent<Breakable>(out var component) && !component.unbreakable && !component.precisionOnly && !component.specialCaseOnly)
				{
					component.Break();
				}
				else if (other.gameObject.TryGetComponent<Glass>(out component2) && !component2.broken)
				{
					component2.Shatter();
				}
				else if (!other.isTrigger)
				{
					Break();
					return;
				}
			}
			if (launched && !other.isTrigger && other.gameObject.layer == 0 && (!other.gameObject.CompareTag("Player") || !col.isTrigger))
			{
				Break();
				return;
			}
		}
		if ((!launched && !physicsCannonball) || (other.gameObject.layer != 10 && other.gameObject.layer != 11 && other.gameObject.layer != 12) || checkingForBreak)
		{
			return;
		}
		checkingForBreak = true;
		EnemyIdentifierIdentifier component3 = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
		EnemyIdentifier enemyIdentifier = ((!component3 || !component3.eid) ? other.gameObject.GetComponent<EnemyIdentifier>() : component3.eid);
		if ((bool)enemyIdentifier && !hitEnemies.Contains(enemyIdentifier))
		{
			if (physicsCannonball && (float)instaBreakDefence < 1f)
			{
				hitEnemies.Add(enemyIdentifier);
				return;
			}
			bool flag = true;
			if (!enemyIdentifier.dead)
			{
				flag = false;
			}
			enemyIdentifier.hitter = "cannonball";
			if (!physicsCannonball)
			{
				enemyIdentifier.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 100f, base.transform.position, damage, tryForExplode: true);
			}
			else if (forceMaxSpeed)
			{
				enemyIdentifier.DeliverDamage(other.gameObject, base.transform.forward.normalized * 1000f, base.transform.position, damage, tryForExplode: true);
			}
			else if (rb.velocity.magnitude > 10f)
			{
				enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * rb.velocity.magnitude * 1000f, base.transform.position, Mathf.Min(damage, rb.velocity.magnitude * 0.15f), tryForExplode: true);
			}
			hitEnemies.Add(enemyIdentifier);
			if (physicsCannonball && launched && !flag)
			{
				if (hasBounced)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(150, "ultrakill.cannonballedfrombounce", sourceWeapon, enemyIdentifier);
				}
				else
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.cannonboost", sourceWeapon, enemyIdentifier);
				}
			}
			if (!enemyIdentifier || enemyIdentifier.dead)
			{
				if (!flag)
				{
					durability--;
					if (durability <= 0)
					{
						Break();
					}
				}
				if (physicsCannonball && !launched && (!flag || other.gameObject.layer == 11))
				{
					Bounce();
				}
				if ((bool)enemyIdentifier)
				{
					enemyIdentifier.Explode();
				}
				checkingForBreak = false;
			}
			else
			{
				if (!physicsCannonball || rb.velocity.magnitude < 15f)
				{
					Break();
				}
				else
				{
					Bounce();
				}
				if (enemyIdentifier.enemyType == EnemyType.Sisyphus && enemyIdentifier.TryGetComponent<Sisyphus>(out var component4))
				{
					component4.Knockdown(base.transform.position);
				}
			}
		}
		else
		{
			checkingForBreak = false;
		}
	}

	public void Break()
	{
		if ((bool)sisy)
		{
			checkingForBreak = false;
			launched = false;
			launchable = false;
			rb.SetGravityMode(useGravity: true);
			rb.velocity = Vector3.up * 25f;
			MonoSingleton<CameraController>.Instance.CameraShake(1f);
			if ((bool)breakEffect)
			{
				UnityEngine.Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			sisy.SwingStop();
		}
		else if (!broken)
		{
			broken = true;
			if ((bool)breakEffect)
			{
				UnityEngine.Object.Instantiate(breakEffect, base.transform.position, base.transform.rotation);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Bounce()
	{
		if (currentBounces >= maxBounces)
		{
			Break();
			return;
		}
		instaBreakDefence = 0f;
		currentBounces++;
		durability = 99;
		hasBounced = true;
		launched = false;
		launchable = true;
		checkingForBreak = false;
		rb.SetGravityMode(useGravity: true);
		rb.velocity = Vector3.up * rb.velocity.magnitude * 0.15f + rb.velocity.normalized * -5f;
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
		if ((bool)(UnityEngine.Object)(object)bounceSound)
		{
			UnityEngine.Object.Instantiate<AudioSource>(bounceSound, base.transform.position, Quaternion.identity);
		}
	}

	public void Explode()
	{
		if ((bool)interruptionExplosion)
		{
			UnityEngine.Object.Instantiate(interruptionExplosion, base.transform.position, Quaternion.identity);
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions"))
		{
			breakEffect = null;
		}
		Break();
	}

	public void InstaBreakDefenceCancel()
	{
		instaBreakDefence = 1f;
	}

	public void OnPortalBlocked(in PortalTravelDetails details)
	{
		PortalHandle handle = details.portalSequence[0];
		NativePortalTransform nativePortalTransform = PortalUtils.GetPortalObject(handle).GetTransform(handle.side);
		base.transform.position = nativePortalTransform.GetPositionInFront(details.intersection, 0.05f);
		Break();
	}

	public void SetData(ref TargetData data)
	{
		data.position = cachedPos;
		data.realPosition = cachedPos;
		data.rotation = cachedRot;
		data.velocity = cachedVel;
	}

	public void UpdateCachedTransformData()
	{
		cachedPos = (rb ? rb.position : base.transform.position);
		cachedRot = (rb ? rb.rotation : base.transform.rotation);
		cachedVel = (rb ? rb.velocity : Vector3.zero);
	}
}
