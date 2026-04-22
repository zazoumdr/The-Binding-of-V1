using System;
using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using UnityEngine;

public class Grenade : MonoBehaviour, ITarget
{
	public string hitterWeapon;

	public GameObject sourceWeapon;

	public GameObject explosion;

	public GameObject harmlessExplosion;

	public GameObject superExplosion;

	public bool ignoreExplosions;

	[SerializeField]
	private RevolverBeam grenadeBeam;

	private bool exploded;

	public bool enemy;

	[HideInInspector]
	public EnemyIdentifier originEnemy;

	public float totalDamageMultiplier = 1f;

	public bool rocket;

	[HideInInspector]
	public Rigidbody rb;

	[HideInInspector]
	public List<MagnetInfo> magnets = new List<MagnetInfo>();

	[HideInInspector]
	public MagnetInfo? latestEnemyMagnet;

	public float rocketSpeed;

	[SerializeField]
	private GameObject freezeEffect;

	private CapsuleCollider col;

	[SerializeField]
	private GameObject interruptSphere;

	public bool playerRiding;

	private bool playerInRidingRange = true;

	private float downpull = -0.5f;

	public GameObject playerRideSound;

	[HideInInspector]
	public bool rideable;

	[HideInInspector]
	public bool hooked;

	private bool hasBeenRidden;

	private LayerMask rocketRideMask;

	public TargetHandle proximityTargetHandle;

	public GameObject proximityWindup;

	private bool selfExploding;

	[HideInInspector]
	public bool levelledUp;

	[HideInInspector]
	public float timeFrozen;

	[SerializeField]
	private GameObject levelUpEffect;

	public List<EnemyType> ignoreEnemyType = new List<EnemyType>();

	private SimplePortalTraveler portalTraveler;

	private Vector3 cachedPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

	public bool frozen
	{
		get
		{
			if (!MonoSingleton<WeaponCharges>.Instance)
			{
				return false;
			}
			return MonoSingleton<WeaponCharges>.Instance.rocketFrozen;
		}
	}

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

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<CapsuleCollider>();
		if (!enemy)
		{
			CanCollideWithPlayer(can: false);
		}
		MonoSingleton<ObjectTracker>.Instance.AddGrenade(this);
		rocketRideMask = LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies);
		rocketRideMask = (int)rocketRideMask | 0x40000;
	}

	private void Start()
	{
		if (rocket)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketCount++;
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			if (!TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType(enemy ? PortalTravellerType.ENEMY_PROJECTILE : PortalTravellerType.PLAYER_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravelOverride = (Func<PortalTravelDetails, bool?>)Delegate.Combine(simplePortalTraveler.onTravelOverride, new Func<PortalTravelDetails, bool?>(OnPortalTraversal));
			SimplePortalTraveler simplePortalTraveler2 = portalTraveler;
			simplePortalTraveler2.onTravelBlocked = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler2.onTravelBlocked, new PortalManagerV2.TravelCallback(OnPortalBlocked));
		}
		MonoSingleton<CoinTracker>.Instance.RegisterTarget(this, base.destroyCancellationToken);
	}

	private void OnDestroy()
	{
		if (base.gameObject.scene.isLoaded)
		{
			MonoSingleton<ObjectTracker>.Instance.RemoveGrenade(this);
			if (playerRiding)
			{
				PlayerRideEnd();
			}
		}
		if (portalTraveler != null)
		{
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravelOverride = (Func<PortalTravelDetails, bool?>)Delegate.Remove(simplePortalTraveler.onTravelOverride, new Func<PortalTravelDetails, bool?>(OnPortalTraversal));
		}
		if (rocket && (bool)MonoSingleton<WeaponCharges>.Instance)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketCount--;
			MonoSingleton<WeaponCharges>.Instance.timeSinceIdleFrozen = 0f;
		}
	}

	private void Update()
	{
		if (rocket && rocketSpeed != 0f && (bool)rb && !MonoSingleton<OptionsManager>.Instance.paused && playerRiding)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
			{
				PlayerRideEnd();
				MonoSingleton<NewMovement>.Instance.Jump();
				MonoSingleton<NewMovement>.Instance.preSlideSpeed = rb.velocity.magnitude / 24f;
				MonoSingleton<NewMovement>.Instance.preSlideDelay = 0.2f;
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame)
			{
				PlayerRideEnd();
			}
		}
	}

	public void CheckPlayerCollision(NewMovement nm, out bool collided, out Vector3 newPosition, out Collider targetCollider)
	{
		Vector3 vector = nm.transform.position + nm.playerCollider.center;
		collided = false;
		newPosition = Vector3.positiveInfinity;
		targetCollider = null;
		Vector3 vector2 = -nm.rb.GetGravityDirection();
		Vector3 position = nm.transform.position;
		CapsuleCollider playerCollider = nm.playerCollider;
		if (!Physics.CheckCapsule(vector + vector2 * (playerCollider.height / 2f), vector - vector2 * (playerCollider.height / 2f), 0.5f, rocketRideMask, QueryTriggerInteraction.Ignore))
		{
			RaycastHit[] array = Physics.CapsuleCastAll(vector + vector2 * (playerCollider.height / 2f), vector - vector2 * (playerCollider.height / 2f), 0.499f, rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime, rocketRideMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].collider.isTrigger && array[i].collider.gameObject.layer != 12 && array[i].collider.gameObject.layer != 14 && (!array[i].collider.attachedRigidbody || array[i].collider.attachedRigidbody != rb))
				{
					Vector3 vector3 = playerCollider.ClosestPoint(array[i].point);
					Vector3 vector4 = array[i].point - (array[i].point - vector3).normalized * Vector3.Distance(position, vector3);
					if (Vector3.Distance(position, vector4) < Vector3.Distance(position, newPosition))
					{
						newPosition = vector4;
						targetCollider = array[i].collider;
					}
					collided = true;
					continue;
				}
				_ = array[i].collider.isTrigger;
				_ = array[i].collider.gameObject.layer;
				_ = 12;
				_ = array[i].collider.gameObject.layer;
				_ = 14;
				if ((bool)array[i].collider.attachedRigidbody)
				{
					_ = array[i].collider.attachedRigidbody == rb;
				}
			}
		}
		else
		{
			newPosition = position;
			targetCollider = Physics.OverlapCapsule(vector + vector2 * (playerCollider.height / 2f), vector - vector2 * (playerCollider.height / 2f), 0.5f, rocketRideMask, QueryTriggerInteraction.Ignore)[0];
			collided = true;
		}
	}

	private void FixedUpdate()
	{
		if (!rocket || rocketSpeed == 0f || !rb)
		{
			return;
		}
		if (frozen)
		{
			if (magnets.Count > 0)
			{
				ignoreEnemyType.Clear();
			}
			rideable = true;
			if (!rb.isKinematic)
			{
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}
			timeFrozen += Time.fixedDeltaTime;
			if (timeFrozen >= 1f && (!enemy || hasBeenRidden) && !levelledUp)
			{
				levelledUp = true;
				if ((bool)levelUpEffect)
				{
					levelUpEffect.SetActive(value: true);
				}
			}
		}
		else if (playerRiding)
		{
			if (NoWeaponCooldown.NoCooldown || MonoSingleton<UnderwaterController>.Instance.inWater || MonoSingleton<WeaponCharges>.Instance.infiniteRocketRide || MonoSingleton<NewMovement>.Instance.fakeFallRequests > 0 || MonoSingleton<NewMovement>.Instance.rb.GetGravityVector().magnitude < 30f)
			{
				if (MonoSingleton<UnderwaterController>.Instance.inWater && downpull > 0f)
				{
					downpull = 0f;
				}
				rb.velocity = base.transform.forward * rocketSpeed * 0.65f;
			}
			else
			{
				rb.velocity = Vector3.Lerp(base.transform.forward * (rocketSpeed * 0.65f), Vector3.down * 100f, Mathf.Max(0f, downpull));
				downpull += Time.fixedDeltaTime / 4.5f * Mathf.Max(1f, 1f + rb.velocity.normalized.y);
			}
		}
		else if (!rb.isKinematic)
		{
			rb.velocity = base.transform.forward * rocketSpeed;
		}
		NewMovement instance = MonoSingleton<NewMovement>.Instance;
		if (playerRiding)
		{
			instance.rb.velocity = Vector3.zero;
			CheckPlayerCollision(instance, out var collided, out var newPosition, out var targetCollider);
			if (collided)
			{
				PlayerRideEnd();
				MonoSingleton<NewMovement>.Instance.transform.position = newPosition;
				base.transform.position = newPosition;
				Collision(targetCollider);
			}
		}
		else
		{
			float num = Vector3.Distance(instance.gc.transform.position, base.transform.position + base.transform.forward);
			float num2 = Vector3.Dot(instance.rb.GetGravityDirection(), instance.rb.velocity);
			if (num < 2.25f && (num2 > 0f || hooked) && !instance.gc.onGround && !instance.dead && rideable && (!enemy || instance.gc.heavyFall))
			{
				if (!instance.ridingRocket && !playerInRidingRange)
				{
					PlayerRideStart();
				}
			}
			else if (playerInRidingRange && (num > 3f || MonoSingleton<NewMovement>.Instance.gc.onGround || (num2 < 0f && !hooked)))
			{
				playerInRidingRange = false;
			}
		}
		if (freezeEffect.activeSelf != frozen)
		{
			freezeEffect.SetActive(frozen);
		}
		PortalScene portalScene = null;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance2))
		{
			portalScene = instance2.Scene;
		}
		if (magnets.Count > 0)
		{
			int num3 = magnets.Count - 1;
			while (num3 >= 0)
			{
				if (magnets[num3].magnet == null)
				{
					magnets.RemoveAt(num3);
					num3--;
					continue;
				}
				if (frozen)
				{
					if (latestEnemyMagnet.HasValue && (bool)latestEnemyMagnet.Value.magnet && latestEnemyMagnet.Value.magnet.gameObject.activeInHierarchy && !Physics.Raycast(base.transform.position, latestEnemyMagnet.Value.GetWorldPosition(portalScene) - base.transform.position, Vector3.Distance(latestEnemyMagnet.Value.GetWorldPosition(portalScene), base.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
					{
						base.transform.LookAt(latestEnemyMagnet.Value.GetWorldPosition(portalScene));
					}
					else
					{
						base.transform.LookAt(magnets[num3].GetWorldPosition(portalScene));
					}
				}
				else
				{
					Vector3 worldPosition = magnets[num3].GetWorldPosition(portalScene);
					base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(worldPosition - base.transform.position), Time.fixedDeltaTime * 180f);
				}
				break;
			}
		}
		else if (latestEnemyMagnet.HasValue && (bool)latestEnemyMagnet.Value.magnet && latestEnemyMagnet.Value.magnet.gameObject.activeInHierarchy && !Physics.Raycast(base.transform.position, latestEnemyMagnet.Value.GetWorldPosition(portalScene) - base.transform.position, Vector3.Distance(latestEnemyMagnet.Value.GetWorldPosition(portalScene), base.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
		{
			base.transform.LookAt(latestEnemyMagnet.Value.GetWorldPosition(portalScene));
		}
		if (proximityTargetHandle != null && magnets.Count == 0 && !frozen && !playerRiding && !selfExploding)
		{
			TargetData data = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(proximityTargetHandle);
			if (data.DistanceTo(base.transform.position) < Vector3.Distance(data.PredictTargetPosition(Time.fixedDeltaTime), base.transform.position + rb.velocity * Time.fixedDeltaTime))
			{
				selfExploding = true;
				rideable = true;
				UnityEngine.Object.Instantiate(proximityWindup, col.bounds.center, Quaternion.identity);
				rb.isKinematic = true;
				Invoke("ProximityExplosion", 0.5f);
			}
		}
	}

	private void LateUpdate()
	{
		if (playerRiding)
		{
			NewMovement instance = MonoSingleton<NewMovement>.Instance;
			if (Vector3.Distance(base.transform.position, instance.transform.position) > 5f + rb.velocity.magnitude * Time.deltaTime)
			{
				PlayerRideEnd();
				return;
			}
			Vector2 vector = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
			Vector3 gravityDirection = instance.rb.GetGravityDirection();
			float num = 0f - Vector3.Dot(gravityDirection, instance.transform.up);
			Vector3 normalized = Vector3.Cross(gravityDirection, instance.transform.forward).normalized;
			float num2 = 0f - Vector3.Dot(gravityDirection, normalized);
			float num3 = vector.x * num + vector.y * num2;
			float num4 = vector.y * num + vector.x * num2;
			num4 = (MonoSingleton<PrefsManager>.Instance.GetBool("InvertRocketRide") ? (0f - num4) : num4);
			base.transform.Rotate(num4 * Time.deltaTime * 165f, num3 * Time.deltaTime * 165f, 0f, Space.Self);
			RaycastHit hitInfo;
			Vector3 position = ((!Physics.Raycast(base.transform.position + base.transform.forward, base.transform.up, 4f, LayerMaskDefaults.Get(LMD.Environment))) ? (base.transform.position + base.transform.up * 2f + base.transform.forward) : ((!Physics.Raycast(base.transform.position + base.transform.forward, -gravityDirection, out hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment))) ? (base.transform.position + base.transform.forward) : (base.transform.position + base.transform.forward + gravityDirection * hitInfo.distance)));
			instance.transform.position = position;
			instance.rb.position = position;
			MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collision(collision.collider, collision.relativeVelocity * -1f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (rocket && frozen && (other.gameObject.layer == 10 || other.gameObject.layer == 11) && !other.isTrigger)
		{
			Collision(other);
		}
	}

	public void Collision(Collider other)
	{
		Collision(other, rb.velocity);
	}

	public void Collision(Collider other, Vector3 velocity)
	{
		if (other.TryGetComponent<PortalAwarePlayerColliderClone>(out var _) || exploded || (!enemy && other.CompareTag("Player")) || other.gameObject.layer == 14 || other.gameObject.layer == 20)
		{
			return;
		}
		bool flag = false;
		if ((other.gameObject.layer == 11 || other.gameObject.layer == 10) && (other.attachedRigidbody ? other.attachedRigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var component2) : other.TryGetComponent<EnemyIdentifierIdentifier>(out component2)) && (bool)component2.eid)
		{
			if (component2.eid.enemyType == EnemyType.MaliciousFace && !component2.eid.isGasolined)
			{
				flag = true;
			}
			else
			{
				if (ignoreEnemyType.Count > 0 && ignoreEnemyType.Contains(component2.eid.enemyType))
				{
					return;
				}
				if (component2.eid.dead)
				{
					Physics.IgnoreCollision(col, other, ignore: true);
					return;
				}
			}
		}
		if (!flag && other.gameObject.CompareTag("Armor"))
		{
			flag = true;
		}
		if (flag)
		{
			rb.constraints = RigidbodyConstraints.None;
			Vector3 vector = velocity * Time.fixedDeltaTime * 2f;
			if (Physics.Raycast(base.transform.position - vector, vector + velocity.normalized * col.bounds.size.magnitude, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment), QueryTriggerInteraction.Ignore))
			{
				rb.velocity = Vector3.zero;
				rb.AddForce(Vector3.Reflect(velocity.normalized, hitInfo.normal).normalized * velocity.magnitude, ForceMode.VelocityChange);
				base.transform.forward = Vector3.Reflect(velocity.normalized, hitInfo.normal).normalized;
				rb.AddTorque(UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(0, 250));
			}
			UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>().volume = 0.75f;
			return;
		}
		bool harmless = false;
		bool big = false;
		bool flag2 = false;
		if (rocket)
		{
			if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
			{
				EnemyIdentifierIdentifier component3 = other.GetComponent<EnemyIdentifierIdentifier>();
				if ((bool)component3 && (bool)component3.eid)
				{
					if (levelledUp || playerRiding)
					{
						flag2 = true;
					}
					else if (!component3.eid.dead && !component3.eid.flying && (((bool)component3.eid.gce && !component3.eid.gce.onGround) || (float)component3.eid.timeSinceSpawned <= 0.15f))
					{
						flag2 = true;
					}
					if (component3.eid.stuckMagnets.Count > 0)
					{
						foreach (Magnet stuckMagnet in component3.eid.stuckMagnets)
						{
							if (!(stuckMagnet == null))
							{
								stuckMagnet.DamageMagnet((!flag2) ? 1 : 2);
							}
						}
					}
					if (component3.eid == originEnemy && !component3.eid.blessed)
					{
						if (hasBeenRidden && !frozen && originEnemy.enemyType == EnemyType.Guttertank)
						{
							originEnemy.Explode(fromExplosion: true);
							MonoSingleton<StyleHUD>.Instance.AddPoints(300, "ultrakill.roundtrip", null, component3.eid);
						}
						else
						{
							MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.rocketreturn", null, component3.eid);
						}
					}
				}
				MonoSingleton<TimeController>.Instance.HitStop(0.05f);
			}
			else if (!enemy || !other.gameObject.CompareTag("Player"))
			{
				harmless = true;
			}
		}
		else if (!LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
		{
			MonoSingleton<TimeController>.Instance.HitStop(0.05f);
		}
		Explode(big, harmless, flag2);
	}

	private void ProximityExplosion()
	{
		Explode(big: true);
	}

	public void Explode(bool big = false, bool harmless = false, bool super = false, float sizeMultiplier = 1f, bool ultrabooster = false, GameObject exploderWeapon = null, bool fup = false)
	{
		if (exploded)
		{
			return;
		}
		exploded = true;
		int checkSize = (super ? 7 : 3);
		if (MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(base.transform.position, checkSize))
		{
			harmless = false;
		}
		GameObject gameObject = (harmless ? UnityEngine.Object.Instantiate(harmlessExplosion, base.transform.position, Quaternion.identity) : ((!super) ? UnityEngine.Object.Instantiate(this.explosion, base.transform.position, Quaternion.identity) : UnityEngine.Object.Instantiate(superExplosion, base.transform.position, Quaternion.identity)));
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			explosion.sourceWeapon = exploderWeapon ?? sourceWeapon;
			explosion.hitterWeapon = hitterWeapon;
			explosion.isFup = fup;
			if (enemy)
			{
				explosion.enemy = true;
			}
			if (ignoreEnemyType.Count > 0)
			{
				explosion.toIgnore = ignoreEnemyType;
			}
			if (rocket && super && big)
			{
				explosion.maxSize *= 2.5f;
				explosion.speed *= 2.5f;
			}
			else if (big || (rocket && frozen))
			{
				explosion.maxSize *= 1.5f;
				explosion.speed *= 1.5f;
			}
			if (totalDamageMultiplier != 1f)
			{
				explosion.damage = (int)((float)explosion.damage * totalDamageMultiplier);
			}
			explosion.maxSize *= sizeMultiplier;
			explosion.speed *= sizeMultiplier;
			if ((bool)originEnemy)
			{
				explosion.originEnemy = originEnemy;
			}
			if (ultrabooster)
			{
				explosion.ultrabooster = true;
			}
			if (rocket && explosion.damage != 0)
			{
				explosion.rocketExplosion = true;
			}
		}
		gameObject.transform.localScale *= sizeMultiplier;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void PlayerRideStart()
	{
		playerInRidingRange = true;
		CanCollideWithPlayer(can: false);
		if (enemy && proximityTargetHandle != null)
		{
			CancelInvoke("ProximityExplosion");
			proximityTargetHandle = null;
			rb.isKinematic = false;
		}
		ignoreEnemyType.Clear();
		playerRiding = true;
		MonoSingleton<NewMovement>.Instance.ridingRocket = this;
		MonoSingleton<NewMovement>.Instance.gc.heavyFall = false;
		MonoSingleton<NewMovement>.Instance.gc.ForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.ForceOff();
		UnityEngine.Object.Instantiate(playerRideSound);
		if (!hasBeenRidden && !enemy)
		{
			MonoSingleton<NewMovement>.Instance.rocketRides++;
			hasBeenRidden = true;
			if (MonoSingleton<NewMovement>.Instance.rocketRides > 3)
			{
				downpull += 0.25f * (float)(MonoSingleton<NewMovement>.Instance.rocketRides - 3);
			}
		}
		else if (!hasBeenRidden)
		{
			hasBeenRidden = true;
		}
	}

	public void PlayerRideEnd()
	{
		playerRiding = false;
		MonoSingleton<NewMovement>.Instance.ridingRocket = null;
		MonoSingleton<NewMovement>.Instance.gc.StopForceOff();
		MonoSingleton<NewMovement>.Instance.slopeCheck.StopForceOff();
	}

	public void CanCollideWithPlayer(bool can = true)
	{
		Physics.IgnoreCollision(col, MonoSingleton<NewMovement>.Instance.playerCollider, !can);
	}

	public bool? OnPortalTraversal(PortalTravelDetails details)
	{
		if (!playerRiding)
		{
			portalTraveler.Travel(details);
			proximityTargetHandle = proximityTargetHandle?.Then(details.portalSequence);
			return true;
		}
		return null;
	}

	public void OnRiderTraversal(PortalTravelDetails riderTravelDetails)
	{
		portalTraveler.Travel(riderTravelDetails);
		proximityTargetHandle = proximityTargetHandle?.Then(riderTravelDetails.portalSequence);
	}

	public void OnPortalBlocked(in PortalTravelDetails details)
	{
		PortalHandle handle = details.portalSequence[0];
		NativePortalTransform nativePortalTransform = PortalUtils.GetPortalObject(handle).GetTransform(handle.side);
		base.transform.position = nativePortalTransform.GetPositionInFront(details.intersection, 0.05f);
		Explode();
	}

	public void MagnetCaught(MagnetInfo mag)
	{
		for (int i = 0; i < magnets.Count; i++)
		{
			if (!(magnets[i].magnet != mag.magnet))
			{
				magnets[i] = mag;
				return;
			}
		}
		latestEnemyMagnet = mag;
		magnets.Add(mag);
	}

	public void MagnetRelease(MagnetInfo mag)
	{
		for (int num = magnets.Count - 1; num >= 0; num--)
		{
			if (!(magnets[num].magnet != mag.magnet))
			{
				magnets.RemoveAt(num);
				break;
			}
		}
		if (latestEnemyMagnet.HasValue && !(latestEnemyMagnet.Value.magnet != mag.magnet))
		{
			if (magnets.Count > 0)
			{
				List<MagnetInfo> list = magnets;
				latestEnemyMagnet = list[list.Count - 1];
			}
			else
			{
				latestEnemyMagnet = null;
			}
		}
	}

	public void UpdateMagnetPath(MagnetInfo mag)
	{
		for (int i = 0; i < magnets.Count; i++)
		{
			if (!(magnets[i].magnet != mag.magnet))
			{
				magnets[i] = mag;
				break;
			}
		}
		if (latestEnemyMagnet.HasValue && latestEnemyMagnet.Value.magnet == mag.magnet)
		{
			latestEnemyMagnet = mag;
		}
	}

	public void GrenadeBeam(Vector3 targetPoint, GameObject newSourceWeapon = null)
	{
		if (!exploded)
		{
			RevolverBeam revolverBeam = UnityEngine.Object.Instantiate(grenadeBeam, base.transform.position, Quaternion.LookRotation(targetPoint - base.transform.position));
			revolverBeam.sourceWeapon = ((newSourceWeapon != null) ? newSourceWeapon : sourceWeapon);
			revolverBeam.hitterOverride = hitterWeapon;
			revolverBeam.isRocketBeam = rocket;
			if (levelledUp)
			{
				revolverBeam.hitParticle = superExplosion;
			}
			exploded = true;
			MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(targetPoint);
			UnityEngine.Object.Destroy(base.gameObject);
		}
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
