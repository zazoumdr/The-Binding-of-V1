using System.Collections.Generic;
using DebugOverlays;
using plog;
using plog.Models;
using ULTRAKILL.Cheats;
using UnityEngine;

public class SwingCheck2 : MonoBehaviour
{
	private static readonly Logger Log = new Logger("SwingCheck2");

	[HideInInspector]
	public EnemyIdentifier eid;

	public EnemyType type;

	public List<Collider> hitColliders = new List<Collider>();

	private NewMovement nmov;

	public int damage;

	public int enemyDamage;

	public float knockBackForce;

	public bool knockBackDirectionOverride;

	public Vector3 knockBackDirection;

	private LayerMask lmask;

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public bool strong;

	[HideInInspector]
	public Collider col;

	public Collider[] additionalColliders;

	public bool useRaycastCheck;

	private AudioSource aud;

	private bool physicalCollider;

	[HideInInspector]
	public bool damaging;

	public bool ignoreSlidingPlayer;

	public bool canHitPlayerMultipleTimes;

	public bool startActive;

	public bool interpolateBetweenFrames;

	public Transform checkForCollisionsBetween;

	private Vector3 previousPosition;

	private SwingCheckDebugOverlay debugOverlay;

	private bool hasPrinted;

	public GameObject hitEffect;

	private bool playerOnly
	{
		get
		{
			if (!(eid == null) && eid.target != null)
			{
				return eid.target.isPlayer;
			}
			return true;
		}
	}

	private LayerMask relevantLMask => lmask;

	private void Awake()
	{
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		lmask = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void Start()
	{
		if (!eid)
		{
			eid = GetComponentInParent<EnemyIdentifier>();
		}
		if ((bool)eid)
		{
			type = eid.enemyType;
		}
		if (!col.isTrigger)
		{
			physicalCollider = true;
		}
		else if (!startActive)
		{
			col.enabled = false;
		}
		else
		{
			DamageStart();
		}
		if (interpolateBetweenFrames)
		{
			previousPosition = base.transform.position;
		}
	}

	private void OnEnable()
	{
		if (startActive && (bool)col)
		{
			col.enabled = true;
			DamageStart();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (damaging)
		{
			CheckCollision(other);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (damaging)
		{
			CheckCollision(collision.collider);
		}
	}

	private void Update()
	{
		if ((interpolateBetweenFrames || (bool)checkForCollisionsBetween) && damaging && (bool)col.attachedRigidbody)
		{
			if (interpolateBetweenFrames)
			{
				if (Input.GetKey(KeyCode.Alpha7) && !hasPrinted)
				{
					hasPrinted = true;
					Object.Instantiate(new GameObject("previousPosition"), previousPosition, Quaternion.identity);
					Object.Instantiate(new GameObject("position"), base.transform.position, Quaternion.identity);
					Object.Instantiate(new GameObject("rigidbodyPosition"), col.attachedRigidbody.position, Quaternion.identity);
					Object.Instantiate(new GameObject("checkForCollisionBetween"), checkForCollisionsBetween.position, Quaternion.identity);
				}
				RaycastHit[] array = col.attachedRigidbody.SweepTestAll(previousPosition - base.transform.position, Vector3.Distance(previousPosition, base.transform.position), QueryTriggerInteraction.Collide);
				foreach (RaycastHit raycastHit in array)
				{
					CheckCollision(raycastHit.collider);
				}
			}
			if ((bool)checkForCollisionsBetween)
			{
				RaycastHit[] array = col.attachedRigidbody.SweepTestAll(checkForCollisionsBetween.position - base.transform.position, Vector3.Distance(checkForCollisionsBetween.position, base.transform.position), QueryTriggerInteraction.Collide);
				foreach (RaycastHit raycastHit2 in array)
				{
					CheckCollision(raycastHit2.collider);
				}
			}
			previousPosition = base.transform.position;
		}
		UpdateDebugOverlay();
	}

	private void CheckCollision(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			if (hitColliders.Contains(other) || other.gameObject.layer == 15)
			{
				return;
			}
			bool flag = false;
			if (useRaycastCheck && (bool)eid)
			{
				Vector3 vector = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
				if (Physics.Raycast(vector, other.bounds.center - vector, Vector3.Distance(vector, other.bounds.center), relevantLMask))
				{
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				if (!ignoreSlidingPlayer || !MonoSingleton<PlatformerMovement>.Instance.sliding)
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode();
				}
				return;
			}
			if (nmov == null)
			{
				nmov = other.GetComponent<NewMovement>();
			}
			if (ignoreSlidingPlayer && nmov.sliding)
			{
				return;
			}
			nmov.GetHurt(Mathf.RoundToInt((float)damage * eid.totalDamageModifier), invincible: true);
			if (!canHitPlayerMultipleTimes)
			{
				hitColliders.Add(other);
			}
			if (knockBackForce > 0f)
			{
				Vector3 forward = base.transform.forward;
				if (knockBackDirectionOverride)
				{
					forward = knockBackDirection;
				}
				if (knockBackDirection == Vector3.down)
				{
					nmov.Slamdown(knockBackForce);
				}
				else
				{
					nmov.LaunchFromPoint(nmov.transform.position + forward * -1f, knockBackForce, knockBackForce);
				}
			}
			NotifyTargetBeenHit();
			if ((bool)hitEffect)
			{
				Object.Instantiate(hitEffect, base.transform.position, Quaternion.LookRotation(other.transform.position - base.transform.position));
			}
		}
		else if (other.gameObject.layer == 10 && !playerOnly && !hitColliders.Contains(other))
		{
			EnemyIdentifierIdentifier component = other.GetComponent<EnemyIdentifierIdentifier>();
			if (component != null && component.eid != null)
			{
				EnemyIdentifier enid = component.eid;
				CheckEidCollision(enid, other);
			}
		}
		else if (other.gameObject.layer == 12 && !playerOnly && !hitColliders.Contains(other))
		{
			EnemyIdentifier component2 = other.GetComponent<EnemyIdentifier>();
			if (component2 != null)
			{
				CheckEidCollision(component2, other);
			}
		}
		else
		{
			if (!other.gameObject.CompareTag("Breakable"))
			{
				return;
			}
			Breakable component3 = other.gameObject.GetComponent<Breakable>();
			if (component3 != null && (strong || component3.weak) && !component3.playerOnly && !component3.precisionOnly && !component3.specialCaseOnly)
			{
				component3.Break(damage);
				if ((bool)hitEffect)
				{
					Object.Instantiate(hitEffect, base.transform.position, Quaternion.LookRotation(other.transform.position - base.transform.position));
				}
			}
		}
	}

	private void CheckEidCollision(EnemyIdentifier enid, Collider other)
	{
		if (hitEnemies.Contains(enid) || enid.enemyType == type || eid.immuneToFriendlyFire || EnemyIdentifier.CheckHurtException(type, enid.enemyType, (eid != null) ? eid.target : null))
		{
			return;
		}
		if (EnemyIdentifierDebug.Active)
		{
			Log.Fine("We're in, no hurt exception", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (hitEnemies.Contains(enid) && !(hitEnemies[hitEnemies.IndexOf(enid)] != null) && (!enid.dead || !other.gameObject.CompareTag("Head")))
		{
			return;
		}
		if (EnemyIdentifierDebug.Active)
		{
			Log.Fine("hit enemies doesn't contain " + enid.gameObject.name, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (enid.dead && (!enid.dead || other.gameObject.CompareTag("Body")))
		{
			return;
		}
		if (EnemyIdentifierDebug.Active)
		{
			Log.Fine("enid not dead or enid dead and not body", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		bool flag = false;
		if (useRaycastCheck && (bool)eid)
		{
			Vector3 vector = new Vector3(eid.transform.position.x, base.transform.position.y, eid.transform.position.z);
			if (Physics.Raycast(vector, other.transform.position - vector, out var hitInfo, Vector3.Distance(vector, other.transform.position), relevantLMask))
			{
				flag = true;
				if (EnemyIdentifierDebug.Active)
				{
					Log.Fine("block hit by " + hitInfo.collider.gameObject.name, (IEnumerable<Tag>)null, (string)null, (object)null);
				}
			}
			else if (EnemyIdentifierDebug.Active)
			{
				Log.Fine("no block hit", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
		}
		if (flag)
		{
			return;
		}
		enid.hitter = "enemy";
		if (enemyDamage == 0)
		{
			enemyDamage = damage / 10;
		}
		float num = (float)enemyDamage * eid.totalDamageModifier;
		if (type == EnemyType.Guttertank && enid.enemyType == EnemyType.Gutterman && !enid.dead && enid.TryGetComponent<Gutterman>(out var component))
		{
			if (component.hasShield)
			{
				component.ShieldBreak(player: false, flash: false);
			}
			else
			{
				component.GotParried();
			}
			num *= 4f;
		}
		enid.DeliverDamage(other.gameObject, ((base.transform.position - other.transform.position).normalized + Vector3.up) * 10000f, other.transform.position, num, tryForExplode: false);
		hitEnemies.Add(enid);
		hitColliders.Add(other);
		NotifyTargetBeenHit();
		if ((bool)hitEffect)
		{
			Object.Instantiate(hitEffect, base.transform.position, Quaternion.LookRotation(other.transform.position - base.transform.position));
		}
	}

	private void NotifyTargetBeenHit()
	{
		if ((bool)eid)
		{
			IHitTargetCallback[] components = eid.GetComponents<IHitTargetCallback>();
			if (EnemyIdentifierDebug.Active)
			{
				Log.Info(string.Format("We've hit <b>{0}</b>. Broadcasting to <b>{1}</b> receiver{2}.", eid.target, components.Length, (components.Length == 1) ? string.Empty : "s"), (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			IHitTargetCallback[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].TargetBeenHit();
			}
		}
	}

	public void DamageStart()
	{
		if (damaging)
		{
			DamageStop();
		}
		previousPosition = base.transform.position;
		damaging = true;
		if (!physicalCollider)
		{
			if ((bool)col)
			{
				col.enabled = true;
			}
			if (additionalColliders != null)
			{
				Collider[] array = additionalColliders;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = true;
				}
			}
		}
		if ((Object)(object)aud != null)
		{
			aud.Play(tracked: true);
		}
	}

	public void DamageStop()
	{
		damaging = false;
		if (hitColliders.Count > 0)
		{
			hitColliders.Clear();
		}
		if (hitEnemies.Count > 0)
		{
			hitEnemies.Clear();
		}
		if (physicalCollider)
		{
			return;
		}
		if ((bool)col)
		{
			col.enabled = false;
		}
		if (additionalColliders != null)
		{
			Collider[] array = additionalColliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	public void OverrideEnemyIdentifier(EnemyIdentifier newEid)
	{
		eid = newEid;
	}

	private void UpdateDebugOverlay()
	{
		if (EnemyIdentifierDebug.Active)
		{
			if (debugOverlay == null)
			{
				debugOverlay = base.gameObject.AddComponent<SwingCheckDebugOverlay>();
			}
			debugOverlay.ConsumeData(damaging, eid);
		}
		else if (debugOverlay != null)
		{
			Object.Destroy(debugOverlay);
		}
	}

	public void CanHitPlayerMultipleTimes(bool yes)
	{
		canHitPlayerMultipleTimes = yes;
		if (yes && hitColliders.Contains(MonoSingleton<NewMovement>.Instance.playerCollider))
		{
			hitColliders.Remove(MonoSingleton<NewMovement>.Instance.playerCollider);
		}
	}
}
