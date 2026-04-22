using System;
using System.Collections.Generic;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public GameObject sourceWeapon;

	private Rigidbody rb;

	public float speed;

	public float turnSpeed;

	public float speedRandomizer;

	private AudioSource aud;

	public GameObject explosionEffect;

	public float damage;

	public float enemyDamageMultiplier = 1f;

	public bool friendly;

	public bool playerBullet;

	public string bulletType;

	public string weaponType;

	public bool decorative;

	private Vector3 origScale;

	private bool active = true;

	public EnemyType safeEnemyType;

	public bool explosive;

	public bool bigExplosion;

	public List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();

	public HomingType homingType;

	public float turningSpeedMultiplier = 1f;

	[Obsolete("This field is obsolete. Use targetHandle instead.")]
	public EnemyTarget target;

	public TargetHandle targetHandle;

	public bool isTargetPlayer;

	private float maxSpeed;

	private Quaternion targetRotation;

	public float predictiveHomingMultiplier;

	public float stopTrackingAfterSeconds;

	[HideInInspector]
	public float secondsSinceSpawn;

	public bool hittingPlayer;

	private NewMovement nmov;

	public bool boosted;

	[HideInInspector]
	public bool parried;

	private Collider col;

	private float radius;

	public bool undeflectable;

	public bool unparryable;

	public bool breakable;

	public bool keepTrail;

	public bool strong;

	public bool spreaded;

	private int difficulty;

	public bool precheckForCollisions;

	public bool canHitCoin;

	public bool ignoreExplosions;

	public bool ignoreEnvironment;

	private List<Collider> alreadyDeflectedBy = new List<Collider>();

	private List<float> alreadyDeflectedCooldown = new List<float>();

	public List<ContinuousBeam> connectedBeams = new List<ContinuousBeam>();

	public SimplePortalTraveler portalTraveler;

	public PortalHandleSequence traversals = PortalHandleSequence.Empty;

	[HideInInspector]
	public ParryChallenge parryChallenge;

	private void Start()
	{
		if ((bool)(UnityEngine.Object)(object)aud)
		{
			aud.SetPitch(UnityEngine.Random.Range(1.8f, 2f));
			if (((Behaviour)(object)aud).enabled)
			{
				aud.Play(tracked: true);
			}
		}
		if (decorative)
		{
			origScale = base.transform.localScale;
			base.transform.localScale = Vector3.zero;
		}
		if (speed != 0f)
		{
			speed += UnityEngine.Random.Range(0f - speedRandomizer, speedRandomizer);
		}
		if (col != null && !decorative)
		{
			col.enabled = false;
			col.enabled = true;
		}
		maxSpeed = speed;
		if (target == null && targetHandle == null)
		{
			target = EnemyTarget.TrackPlayerIfAllowed();
		}
	}

	private void OnEnable()
	{
		if (!MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			return;
		}
		if (!decorative)
		{
			if (!TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType((friendly || playerBullet) ? PortalTravellerType.PLAYER_PROJECTILE : PortalTravellerType.ENEMY_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
			SimplePortalTraveler simplePortalTraveler2 = portalTraveler;
			simplePortalTraveler2.onTravelBlocked = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler2.onTravelBlocked, new PortalManagerV2.TravelCallback(OnPortalBlocked));
		}
		PortalManagerV2 portalManagerV = instance;
		portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
	}

	private void OnDisable()
	{
		if ((bool)portalTraveler)
		{
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
			SimplePortalTraveler simplePortalTraveler2 = portalTraveler;
			simplePortalTraveler2.onTravelBlocked = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler2.onTravelBlocked, new PortalManagerV2.TravelCallback(OnPortalBlocked));
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			PortalManagerV2? instance2 = MonoSingleton<PortalManagerV2>.Instance;
			instance2.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(instance2.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnDestroy()
	{
		if (portalTraveler != null)
		{
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTraversal));
			SimplePortalTraveler simplePortalTraveler2 = portalTraveler;
			simplePortalTraveler2.onTravelBlocked = (PortalManagerV2.TravelCallback)Delegate.Remove(simplePortalTraveler2.onTravelBlocked, new PortalManagerV2.TravelCallback(OnPortalBlocked));
		}
		if (!base.gameObject.scene.isLoaded || connectedBeams == null || connectedBeams.Count == 0)
		{
			return;
		}
		foreach (ContinuousBeam connectedBeam in connectedBeams)
		{
			if (!(connectedBeam == null))
			{
				if (connectedBeam.transform.parent == base.transform)
				{
					connectedBeam.DetachAndTurnOff();
				}
				else
				{
					connectedBeam.TurnOff();
				}
			}
		}
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		if (col == null)
		{
			col = GetComponentInChildren<Collider>();
		}
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	public static float GetProjectileSpeedMulti(int difficulty)
	{
		if (difficulty > 2)
		{
			return 1.35f;
		}
		return difficulty switch
		{
			1 => 0.75f, 
			0 => 0.5f, 
			_ => 1f, 
		};
	}

	private bool GetTargetData(out Vector3 targetPosition, out Vector3 targetVelocity)
	{
		targetPosition = default(Vector3);
		targetVelocity = default(Vector3);
		if (targetHandle != null)
		{
			TargetData targetData = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(targetHandle);
			targetPosition = (targetData.target.isPlayer ? targetData.headPosition : targetData.position);
			targetVelocity = targetData.velocity;
			return true;
		}
		if (target != null)
		{
			targetPosition = (target.isPlayer ? target.headPosition : target.position);
			targetVelocity = target.GetVelocity();
			return true;
		}
		return false;
	}

	private bool GetTargetPosition(out Vector3 targetPosition)
	{
		Vector3 targetVelocity;
		return GetTargetData(out targetPosition, out targetVelocity);
	}

	private bool IsTargetPlayer()
	{
		if (targetHandle != null)
		{
			return targetHandle.target.isPlayer;
		}
		if (target != null)
		{
			return target.isPlayer;
		}
		return false;
	}

	public void SetPortalTravellerType(PortalTravellerType type)
	{
		portalTraveler.SetType(type);
	}

	public void OnPortalTraversal(in PortalTravelDetails details)
	{
		for (int i = 0; i < connectedBeams.Count; i++)
		{
			if ((bool)connectedBeams[i])
			{
				connectedBeams[i].OnProjectileTraversal(in details);
			}
		}
		for (int j = 0; j < details.portalSequence.Count; j++)
		{
			traversals = traversals.Then(details.portalSequence[j]);
		}
		targetHandle = targetHandle?.From(details.portalSequence);
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence.Reversed());
		}
	}

	public void OnPortalBlocked(in PortalTravelDetails portalTravelDetails)
	{
		CollidedWithPortal();
	}

	private void Update()
	{
		if (alreadyDeflectedBy != null && alreadyDeflectedBy.Count > 0)
		{
			for (int num = alreadyDeflectedBy.Count - 1; num >= 0; num--)
			{
				alreadyDeflectedCooldown[num] = Mathf.MoveTowards(alreadyDeflectedCooldown[num], 0f, Time.deltaTime);
				if (alreadyDeflectedCooldown[num] == 0f)
				{
					alreadyDeflectedCooldown.RemoveAt(num);
					alreadyDeflectedBy.RemoveAt(num);
				}
			}
		}
		if (homingType == HomingType.None || hittingPlayer || !GetTargetData(out var targetPosition, out var targetVelocity))
		{
			return;
		}
		float num2 = predictiveHomingMultiplier;
		if (Vector3.Distance(base.transform.position, targetPosition) < 15f)
		{
			num2 = 0f;
		}
		bool flag = true;
		if (stopTrackingAfterSeconds > 0f)
		{
			if (secondsSinceSpawn > stopTrackingAfterSeconds)
			{
				flag = false;
			}
			secondsSinceSpawn += Time.deltaTime;
		}
		switch (homingType)
		{
		case HomingType.Gradual:
			if (difficulty == 1)
			{
				maxSpeed += Time.deltaTime * 17.5f;
			}
			else if (difficulty == 0)
			{
				maxSpeed += Time.deltaTime * 10f;
			}
			else
			{
				maxSpeed += Time.deltaTime * 25f;
			}
			if (flag)
			{
				Quaternion to2 = Quaternion.LookRotation(targetPosition + targetVelocity * num2 - base.transform.position);
				if (difficulty == 0)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to2, Time.deltaTime * 100f * turningSpeedMultiplier);
				}
				else if (difficulty == 1)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to2, Time.deltaTime * 135f * turningSpeedMultiplier);
				}
				else if (difficulty == 2)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to2, Time.deltaTime * 185f * turningSpeedMultiplier);
				}
				else
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to2, Time.deltaTime * 200f * turningSpeedMultiplier);
				}
			}
			rb.velocity = base.transform.forward * maxSpeed;
			break;
		case HomingType.Instant:
			if (flag)
			{
				Quaternion to = Quaternion.LookRotation(targetPosition + targetVelocity * num2 - base.transform.position);
				if (difficulty == 0)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 100f * turningSpeedMultiplier);
				}
				else if (difficulty == 1)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 135f * turningSpeedMultiplier);
				}
				else if (difficulty == 2)
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 185f * turningSpeedMultiplier);
				}
				else
				{
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * 200f * turningSpeedMultiplier);
				}
			}
			rb.velocity = base.transform.forward * speed;
			break;
		case HomingType.Loose:
		{
			maxSpeed += Time.deltaTime * 10f;
			base.transform.LookAt(base.transform.position + rb.velocity);
			Vector3 vector = ((!flag) ? base.transform.forward : (targetPosition + targetVelocity * num2 - base.transform.position).normalized);
			rb.AddForce(vector * speed * Time.deltaTime * 200f, ForceMode.Acceleration);
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
			break;
		}
		case HomingType.HorizontalOnly:
			base.transform.LookAt(targetPosition + rb.velocity);
			if (flag)
			{
				Vector3 vector2 = targetPosition + targetVelocity * num2;
				vector2.y = base.transform.position.y;
				float num3 = Mathf.Clamp(vector2.x - base.transform.position.x, 0f - turnSpeed, turnSpeed);
				float num4 = Mathf.Clamp(vector2.z - base.transform.position.z, 0f - turnSpeed, turnSpeed);
				if (Vector3.Distance(base.transform.position, vector2) < turnSpeed / 20f)
				{
					num3 = (vector2 - base.transform.position).x;
					num4 = (vector2 - base.transform.position).z;
				}
				float num5 = 15f;
				if (difficulty == 1)
				{
					num5 = 10f;
				}
				else if (difficulty == 0)
				{
					num5 = 5f;
				}
				else if (difficulty >= 3)
				{
					num5 = 25f;
				}
				float x = Mathf.MoveTowards(rb.velocity.x, num3, Time.deltaTime * num5 * turningSpeedMultiplier);
				float z = Mathf.MoveTowards(rb.velocity.z, num4, Time.deltaTime * num5 * turningSpeedMultiplier);
				rb.velocity = new Vector3(x, rb.velocity.y, z);
			}
			break;
		default:
			maxSpeed += Time.deltaTime * 10f;
			if (flag)
			{
				targetRotation = Quaternion.LookRotation(targetPosition + targetVelocity * num2 - base.transform.position);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
			}
			rb.velocity = base.transform.forward * maxSpeed;
			break;
		}
	}

	private void FixedUpdate()
	{
		if (!hittingPlayer && !undeflectable && !decorative && speed != 0f && homingType == HomingType.None)
		{
			rb.velocity = base.transform.forward * speed;
		}
		if (decorative && base.transform.localScale.x < origScale.x)
		{
			aud.SetPitch(base.transform.localScale.x / origScale.x * 2.8f);
			base.transform.localScale = Vector3.Slerp(base.transform.localScale, origScale, Time.deltaTime * speed);
		}
		if (!precheckForCollisions)
		{
			return;
		}
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);
		layerMask = (int)layerMask | 4;
		if ((radius > float.Epsilon) ? PortalPhysicsV2.SphereCast(base.transform.position, rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime, radius, layerMask, out var hitInfo, out var portalTraversals, out var endPoint) : PortalPhysicsV2.Raycast(base.transform.position, rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime, layerMask, out hitInfo, out portalTraversals, out endPoint))
		{
			if (portalTraversals.Length == 0)
			{
				base.transform.position = base.transform.position + rb.velocity.normalized * (hitInfo.distance - 0.1f);
				Collided(hitInfo.collider);
			}
			else if (!portalTraversals[0].portalObject.passThroughNonTraversals)
			{
				CollidedWithPortal();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Collided(other);
	}

	private void CollidedWithPortal()
	{
		if (explosive)
		{
			Explode();
			return;
		}
		active = false;
		if (keepTrail)
		{
			KeepTrail();
		}
		CreateExplosionEffect();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Collided(Collider other)
	{
		if (other.CompareTag("PlayerTrigger") || !active)
		{
			return;
		}
		EnemyIdentifierIdentifier component3;
		EnemyIdentifierIdentifier component4;
		if (!friendly && !hittingPlayer && other.gameObject.CompareTag("Player"))
		{
			if (IsTargetPlayer() && MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
				if (explosive)
				{
					Explode();
					return;
				}
				if (keepTrail)
				{
					KeepTrail();
				}
				CreateExplosionEffect();
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (spreaded)
			{
				ProjectileSpread componentInParent = GetComponentInParent<ProjectileSpread>();
				if (componentInParent != null && componentInParent.parried)
				{
					return;
				}
			}
			hittingPlayer = true;
			rb.velocity = Vector3.zero;
			if (keepTrail)
			{
				KeepTrail();
			}
			base.transform.position = new Vector3(other.transform.position.x, base.transform.position.y, other.transform.position.z);
			nmov = other.gameObject.GetComponentInParent<NewMovement>();
			Invoke("RecheckPlayerHit", 0.05f);
		}
		else if (canHitCoin && other.gameObject.CompareTag("Coin"))
		{
			Coin component = other.gameObject.GetComponent<Coin>();
			if ((bool)component && !component.shot)
			{
				if (!friendly)
				{
					if (target != null)
					{
						component.customTarget = target;
					}
					component.DelayedEnemyReflect();
				}
				else
				{
					component.DelayedReflectRevolver(component.transform.position);
				}
			}
			if (explosive)
			{
				Explode();
				return;
			}
			if (keepTrail)
			{
				KeepTrail();
			}
			active = false;
			CreateExplosionEffect();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (other.gameObject.layer == 14 && breakable)
		{
			if (!other.TryGetComponent<Projectile>(out var component2) || !component2.breakable)
			{
				Break();
			}
		}
		else if ((other.gameObject.CompareTag("Armor") && (friendly || !other.TryGetComponent<EnemyIdentifierIdentifier>(out component3) || !component3.eid || component3.eid.enemyType != safeEnemyType)) || (boosted && other.gameObject.layer == 11 && other.gameObject.CompareTag("Body") && other.TryGetComponent<EnemyIdentifierIdentifier>(out component4) && (bool)component4.eid && component4.eid.enemyType == EnemyType.MaliciousFace && !component4.eid.isGasolined))
		{
			Vector3 vector = rb.velocity * Time.fixedDeltaTime * 2f;
			if (!alreadyDeflectedBy.Contains(other) && Physics.Raycast(base.transform.position - vector, vector, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
			{
				base.transform.forward = Vector3.Reflect(base.transform.forward, hitInfo.normal).normalized;
				base.transform.position = hitInfo.point;
				UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, base.transform.position, Quaternion.identity);
				alreadyDeflectedBy.Add(other);
				alreadyDeflectedCooldown.Add(0.025f);
			}
		}
		else if (active && (other.gameObject.CompareTag("Head") || other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Limb") || other.gameObject.CompareTag("EndLimb")) && !other.gameObject.CompareTag("Armor"))
		{
			EnemyIdentifierIdentifier componentInParent2 = other.gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
			EnemyIdentifier enemyIdentifier = null;
			if (componentInParent2 != null && componentInParent2.eid != null)
			{
				enemyIdentifier = componentInParent2.eid;
			}
			if (!(enemyIdentifier != null) || (alreadyHitEnemies.Count != 0 && alreadyHitEnemies.Contains(enemyIdentifier)) || ((enemyIdentifier.enemyType == safeEnemyType || EnemyIdentifier.CheckHurtException(safeEnemyType, enemyIdentifier.enemyType, targetHandle)) && (!friendly || enemyIdentifier.immuneToFriendlyFire) && !playerBullet && !parried))
			{
				return;
			}
			if (explosive)
			{
				Explode();
			}
			active = false;
			bool tryForExplode = false;
			bool dead = enemyIdentifier.dead;
			if (playerBullet)
			{
				enemyIdentifier.hitter = bulletType;
				if (!enemyIdentifier.hitterWeapons.Contains(weaponType))
				{
					enemyIdentifier.hitterWeapons.Add(weaponType);
				}
			}
			else if (!friendly)
			{
				enemyIdentifier.hitter = "enemy";
			}
			else
			{
				enemyIdentifier.hitter = "projectile";
				tryForExplode = true;
			}
			if (boosted && !enemyIdentifier.blessed && !enemyIdentifier.dead)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(90, "ultrakill.projectileboost", sourceWeapon, enemyIdentifier);
			}
			bool flag = true;
			if (spreaded)
			{
				ProjectileSpread componentInParent3 = GetComponentInParent<ProjectileSpread>();
				if (componentInParent3 != null)
				{
					if (componentInParent3.hitEnemies.Contains(enemyIdentifier))
					{
						flag = false;
					}
					else
					{
						componentInParent3.hitEnemies.Add(enemyIdentifier);
					}
				}
			}
			if (!explosive)
			{
				if (flag)
				{
					if (playerBullet)
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 2500f, base.transform.position, damage / 4f * enemyDamageMultiplier, tryForExplode, 0f, sourceWeapon);
					}
					else if (friendly)
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 10000f, base.transform.position, damage / 4f * enemyDamageMultiplier, tryForExplode, 0f, sourceWeapon);
					}
					else
					{
						enemyIdentifier.DeliverDamage(other.gameObject, rb.velocity.normalized * 100f, base.transform.position, damage / 10f * enemyDamageMultiplier, tryForExplode, 0f, sourceWeapon);
					}
				}
				CreateExplosionEffect();
			}
			if (keepTrail)
			{
				KeepTrail();
			}
			if (!dead)
			{
				MonoSingleton<TimeController>.Instance.HitStop(0.005f);
			}
			if (!dead && !enemyIdentifier.dead && safeEnemyType == EnemyType.Power && enemyIdentifier.enemyType == EnemyType.Power && enemyIdentifier.TryGetComponent<Power>(out var component5) && !component5.enraged)
			{
				component5.JuggleStart();
			}
			if (!dead || other.gameObject.layer == 11 || boosted)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			alreadyHitEnemies.Add(enemyIdentifier);
			active = true;
		}
		else if (!hittingPlayer && LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) && !ignoreEnvironment && active)
		{
			Breakable component6 = other.gameObject.GetComponent<Breakable>();
			if (component6 != null && !component6.precisionOnly && !component6.specialCaseOnly && (component6.weak || strong))
			{
				component6.Break(damage / (float)((playerBullet || friendly) ? 4 : 10));
			}
			if (other.gameObject.TryGetComponent<Bleeder>(out var component7))
			{
				bool flag2 = false;
				if (!friendly && !playerBullet && component7.ignoreTypes.Length != 0)
				{
					EnemyType[] ignoreTypes = component7.ignoreTypes;
					for (int i = 0; i < ignoreTypes.Length; i++)
					{
						if (ignoreTypes[i] == safeEnemyType)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					if (damage <= 10f)
					{
						component7.GetHit(base.transform.position, GoreType.Body);
					}
					else if (damage <= 30f)
					{
						component7.GetHit(base.transform.position, GoreType.Limb);
					}
					else
					{
						component7.GetHit(base.transform.position, GoreType.Head);
					}
				}
			}
			if (SceneHelper.IsStaticEnvironment(other))
			{
				MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(base.transform.position - base.transform.forward, base.transform.forward, 5f, Mathf.Max(2, Mathf.RoundToInt(damage / (float)((playerBullet || friendly) ? 4 : 10))), Mathf.Min(1f, Mathf.Max(0.5f, damage / (float)((playerBullet || friendly) ? 4 : 10))));
			}
			if (explosive)
			{
				Explode();
			}
			else
			{
				if (keepTrail)
				{
					KeepTrail();
				}
				CreateExplosionEffect();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			active = false;
		}
		else if (other.gameObject.layer == 0)
		{
			Rigidbody componentInParent4 = other.GetComponentInParent<Rigidbody>();
			if (componentInParent4 != null)
			{
				componentInParent4.AddForce(base.transform.forward * 1000f);
			}
		}
	}

	public void Break()
	{
		if (explosive)
		{
			Explode();
			return;
		}
		if (keepTrail)
		{
			KeepTrail();
		}
		active = false;
		CreateExplosionEffect();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CreateExplosionEffect()
	{
		if (explosionEffect == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(explosionEffect, base.transform.position, base.transform.rotation);
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			explosion.sourceWeapon = sourceWeapon ?? explosion.sourceWeapon;
			if (explosion.damage != 0 && ((!friendly && !playerBullet) || (float)explosion.damage < damage))
			{
				explosion.damage = (int)damage;
			}
			if (!friendly && !playerBullet)
			{
				explosion.enemy = true;
			}
			explosion.boosted = boosted;
		}
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
		gameObject.transform.SetParent(base.transform.parent, worldPositionStays: true);
		if (boosted || parried)
		{
			MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(base.transform.position);
		}
	}

	public void Explode()
	{
		if (!active)
		{
			return;
		}
		active = false;
		if (keepTrail)
		{
			KeepTrail();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(explosionEffect, base.transform.position - rb.velocity * 0.02f, base.transform.rotation);
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			explosion.sourceWeapon = sourceWeapon ?? explosion.sourceWeapon;
			if (bigExplosion)
			{
				explosion.maxSize *= 1.5f;
			}
			if (explosion.damage != 0)
			{
				explosion.damage = Mathf.RoundToInt(damage);
			}
			explosion.enemy = true;
		}
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
		gameObject.transform.SetParent(base.transform.parent, worldPositionStays: true);
		MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(base.transform.position);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void RecheckPlayerHit()
	{
		if (hittingPlayer)
		{
			hittingPlayer = false;
			if ((bool)col)
			{
				col.enabled = false;
			}
			undeflectable = true;
			Invoke("TimeToDie", 0.01f);
		}
	}

	private void TimeToDie()
	{
		bool flag = false;
		if (spreaded)
		{
			ProjectileSpread componentInParent = GetComponentInParent<ProjectileSpread>();
			if (componentInParent != null && componentInParent.parried)
			{
				flag = true;
			}
		}
		if (!explosive)
		{
			CreateExplosionEffect();
		}
		if (!flag)
		{
			if (explosive)
			{
				base.transform.position = base.transform.position - base.transform.forward;
				Explode();
			}
			else
			{
				nmov.GetHurt(Mathf.RoundToInt(damage), invincible: true);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void KeepTrail()
	{
		TrailRenderer componentInChildren = GetComponentInChildren<TrailRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.transform.parent = null;
			componentInChildren.gameObject.AddComponent<RemoveOnTime>().time = 3f;
		}
	}
}
