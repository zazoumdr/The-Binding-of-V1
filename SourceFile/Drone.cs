using System;
using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Drone : EnemyScript
{
	private Enemy enemy;

	private EnemyIdentifier eid;

	private Rigidbody rb;

	private Animator anim;

	private AudioSource aud;

	private BloodsplatterManager bsm;

	private GoreZone gz;

	private KeepInBounds kib;

	[Header("Movement")]
	public bool stationary;

	public bool lockRotation;

	public bool lockPosition;

	public float preferredDistanceToTarget = 15f;

	[Header("Combat")]
	public AssetReference projectile;

	public Material shootMaterial;

	public AssetReference explosion;

	public AssetReference gib;

	public ParticleSystem chargeParticle;

	public AudioClip windUpSound;

	public AudioClip spotSound;

	public AudioClip loseSound;

	public AudioClip hurtSound;

	public AudioClip deathSound;

	public AudioClip dodgeSound;

	public AudioClip[] windUpSounds;

	public bool cantInstaExplode;

	public bool dontStartAware;

	public bool fleshDrone;

	public GameObject ghost;

	public GameObject enrageEffect;

	[HideInInspector]
	public GameObject currentEnrageEffect;

	[Header("Providence")]
	public bool instaready;

	public bool semiStationary;

	public float secondaryChance;

	public AssetReference secondaryProjectile;

	public AudioClip secondaryWindupSound;

	public AudioClip dodgeLaughSound;

	public AssetReference[] spawnOnDeath;

	[SerializeField]
	private Transform[] rotatorWings;

	[HideInInspector]
	public List<VirtueInsignia> childVi = new List<VirtueInsignia>();

	[HideInInspector]
	public float currentSecondaryChance;

	private Vector3 crashTarget;

	private bool canInterruptCrash;

	private Transform modelTransform;

	private bool toLastKnownPos;

	private Vector3 lastKnownPos;

	private float checkCooldown;

	private float blockCooldown;

	private float dodgeCooldown;

	private float attackCooldown;

	private bool killedByPlayer;

	private bool parried;

	private bool exploded;

	private Vector3 viewTarget;

	private int usedAttacks;

	private EnemyCooldowns vc;

	private bool checkingForCrash;

	private bool canHurtOtherDrones;

	private bool hooked;

	private bool homeRunnable;

	private bool returnToStartPosition;

	private Vector3 startPosition;

	private float droneWingRotationSpeed;

	private float droneCurrentRotation;

	private Quaternion[] rotatorWingsDefaultRotation;

	private Material origMaterial;

	private int difficulty = -1;

	private TimeSince sinceParryable;

	private TargetHandle droneTargetHandle;

	private VisionQuery droneTargetQuery;

	private TargetData droneTargetData;

	private int droneVisionMask;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	public Enemy Enemy => enemy;

	private Vision vision => enemy.vision;

	public bool crashing { get; private set; }

	public bool targetSpotted { get; private set; }

	public bool isEnraged { get; private set; }

	private bool hasDroneVision => droneTargetHandle != null;

	public override Vector3 VisionSourcePosition => base.transform.position;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private bool hasAnyTarget
	{
		get
		{
			if (!hasDroneVision)
			{
				return hasDimensionalTarget;
			}
			return true;
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return default(EnemyMovementData);
	}

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		if (eid.enemyType == EnemyType.Virtue || eid.enemyType == EnemyType.Providence)
		{
			vc = MonoSingleton<EnemyCooldowns>.Instance;
		}
		rb.solverIterations *= 3;
		rb.solverVelocityIterations *= 3;
		if (rotatorWings != null && rotatorWings.Length != 0)
		{
			rotatorWingsDefaultRotation = new Quaternion[rotatorWings.Length];
			for (int i = 0; i < rotatorWings.Length; i++)
			{
				rotatorWingsDefaultRotation[i] = rotatorWings[i].localRotation;
			}
		}
	}

	private void Start()
	{
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		difficulty = Enemy.InitializeDifficulty(eid);
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		gz = GoreZone.ResolveGoreZone(base.transform);
		kib = GetComponent<KeepInBounds>();
		if (!(UnityEngine.Object)(object)chargeParticle)
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!((Component)(object)componentsInChildren[i]).TryGetComponent(out SpawnEffect _))
				{
					chargeParticle = componentsInChildren[i];
					break;
				}
			}
		}
		if (eid.enemyType == EnemyType.Virtue)
		{
			anim = GetComponent<Animator>();
		}
		dodgeCooldown = UnityEngine.Random.Range(0.5f, 3f);
		if (instaready)
		{
			attackCooldown = 0f;
		}
		else if (eid.enemyType == EnemyType.Drone)
		{
			attackCooldown = UnityEngine.Random.Range(1f, 3f);
		}
		else if (eid.enemyType == EnemyType.Virtue)
		{
			attackCooldown = 1.5f;
		}
		else
		{
			attackCooldown = 2f;
		}
		if (!dontStartAware)
		{
			targetSpotted = true;
		}
		if (eid.enemyType == EnemyType.Drone)
		{
			modelTransform = base.transform.Find("drone");
			if ((bool)modelTransform)
			{
				EnemySimplifier[] componentsInChildren2 = modelTransform.GetComponentsInChildren<EnemySimplifier>();
				if (componentsInChildren2.Length != 0)
				{
					origMaterial = componentsInChildren2[0].GetComponent<Renderer>().material;
				}
			}
		}
		droneVisionMask = GetSightBlockMask(isPlayer: true);
		droneTargetQuery = new VisionQuery("DroneSight", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && ObstructionCheck(t));
		Invoke("SightCheck", 0.1f);
		if (isEnraged)
		{
			Enrage();
		}
		startPosition = base.transform.position;
		SightCheck();
	}

	private void OnEnable()
	{
		if (eid.enemyType == EnemyType.Virtue && (bool)vc)
		{
			vc.AddVirtue(enemy);
		}
		if (!enemy.musicRequested && !eid.dead)
		{
			MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
			enemy.musicRequested = true;
		}
		if (eid.enemyType == EnemyType.Drone && !MonoSingleton<EnemyTracker>.Instance.drones.Contains(this))
		{
			MonoSingleton<EnemyTracker>.Instance.drones.Add(this);
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTeleport));
		}
	}

	private void OnDisable()
	{
		if (eid.enemyType == EnemyType.Virtue && (bool)vc)
		{
			vc.RemoveVirtue(enemy);
		}
		if (enemy.musicRequested)
		{
			enemy.musicRequested = false;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayCleanMusic();
			}
		}
		if (MonoSingleton<EnemyTracker>.Instance != null && eid.enemyType == EnemyType.Drone && MonoSingleton<EnemyTracker>.Instance.drones.Contains(this))
		{
			MonoSingleton<EnemyTracker>.Instance.drones.Remove(this);
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance2))
		{
			PortalManagerV2 portalManagerV = instance2;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTeleport));
		}
	}

	private void OnTargetTeleport(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (droneTargetHandle != null && traveller.id == droneTargetHandle.id)
		{
			droneTargetHandle = droneTargetHandle.Then(details.portalSequence);
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		PortalHandle handle = details.exitHandle;
		NativePortal nativePortal = MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene.LookupPortal(in handle);
		if (nativePortal.valid)
		{
			Vector3 vector = float3.op_Implicit(nativePortal.transform.back);
			rb.velocity += 10f * Mathf.Max(eid.totalSpeedModifier, 1f) * vector;
			ApproveNewPosition();
			targetSpotted = false;
			TargetLost();
			toLastKnownPos = false;
			if (droneTargetHandle != null)
			{
				droneTargetHandle.From(details.portalSequence);
			}
		}
	}

	private void Update()
	{
		if (crashing)
		{
			return;
		}
		UpdateRigidbodySettings();
		bool flag = hasAnyTarget || (eid.enemyType == EnemyType.Virtue && eid.target != null);
		if ((targetSpotted || eid.enemyType == EnemyType.Virtue) && flag)
		{
			if (hasDroneVision)
			{
				viewTarget = vision.CalculateData(droneTargetHandle).headPosition;
			}
			else if (hasDimensionalTarget)
			{
				viewTarget = lastDimensionalTarget;
			}
			else if (eid.enemyType == EnemyType.Virtue && eid.target != null)
			{
				viewTarget = eid.target.position;
			}
			float num = GetCooldownSpeed();
			if (eid.enemyType == EnemyType.Providence && difficulty <= 3)
			{
				num /= 2f;
			}
			if (dodgeCooldown > 0f)
			{
				dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 0f, Time.deltaTime * num);
			}
			else if (!stationary && !lockPosition)
			{
				dodgeCooldown = UnityEngine.Random.Range(1f, 3f);
				RandomDodge();
			}
		}
		if (ShouldProcessAttack())
		{
			float cooldownSpeed = GetCooldownSpeed();
			if (attackCooldown > 0f)
			{
				attackCooldown = Mathf.MoveTowards(attackCooldown, 0f, Time.deltaTime * cooldownSpeed);
			}
			else if (projectile != null && (!vc || vc.virtueCooldown == 0f))
			{
				ProcessAttack();
			}
		}
		if ((bool)eid && eid.hooked && !hooked)
		{
			Hooked();
		}
		else if ((bool)eid && !eid.hooked && hooked)
		{
			Unhooked();
		}
		if (enemy.parryable)
		{
			sinceParryable = 0f;
		}
		if (eid.enemyType == EnemyType.Providence && hooked && !CanBeHooked())
		{
			MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
			Unhooked();
			DodgeLaugh();
			RandomDodge(force: true);
		}
		if (droneWingRotationSpeed != 0f)
		{
			for (int i = 0; i < rotatorWings.Length; i++)
			{
				rotatorWings[i].Rotate(Vector3.right, 360f * Time.deltaTime * droneWingRotationSpeed * (float)((i % 2 != 0) ? 1 : (-1)));
			}
		}
	}

	public bool CanBeHooked()
	{
		if (enemy.parryable)
		{
			return true;
		}
		if (difficulty >= 2)
		{
			return false;
		}
		return (float)sinceParryable <= ((difficulty == 1) ? 0.5f : 1f);
	}

	private void FixedUpdate()
	{
		if (enemy.parryFramesLeft > 0)
		{
			enemy.parryFramesLeft--;
		}
		if (eid.enemyType != EnemyType.Providence && rb.velocity.magnitude < 1f && rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
		if (crashing)
		{
			ProcessCrashing();
		}
		else if (targetSpotted || eid.enemyType == EnemyType.Virtue)
		{
			ProcessTargeting();
		}
		else if (toLastKnownPos && !semiStationary && !stationary && !lockPosition && eid.target != null)
		{
			ProcessSearching();
		}
		if (semiStationary)
		{
			if (Vector3.Distance(base.transform.position, startPosition) > 15f)
			{
				returnToStartPosition = true;
			}
			else if (Vector3.Distance(base.transform.position, startPosition) < 5f)
			{
				returnToStartPosition = false;
			}
			if (returnToStartPosition)
			{
				rb.AddForce((startPosition - base.transform.position) * 10f * eid.totalSpeedModifier, ForceMode.Acceleration);
			}
		}
		if (!crashing)
		{
			if (!lockRotation && eid.target != null)
			{
				Quaternion b = Quaternion.LookRotation(viewTarget - base.transform.position);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, 0.075f + 0.00025f * Quaternion.Angle(base.transform.rotation, b) * eid.totalSpeedModifier);
			}
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f * eid.totalSpeedModifier);
			if (kib != null)
			{
				kib.ValidateMove();
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (crashing && (collision.gameObject.layer == 0 || LayerMaskDefaults.IsMatchingLayer(collision.gameObject.layer, LMD.Environment) || collision.gameObject.CompareTag("Player") || collision.gameObject.layer == 10 || collision.gameObject.layer == 11 || collision.gameObject.layer == 12 || collision.gameObject.layer == 26))
		{
			Explode();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (crashing)
		{
			if ((eid.enemyType == EnemyType.Drone && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 12)) || (!other.isTrigger && (other.gameObject.layer == 0 || LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) || other.gameObject.layer == 26 || other.gameObject.CompareTag("Player"))))
			{
				Explode();
			}
			else if (eid.enemyType != EnemyType.Drone && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 12))
			{
				HandleCollisionWithEnemy(other);
			}
		}
	}

	public override void OnDamage(ref DamageData data)
	{
		GetHurt(data.force, data.damage, data.sourceWeapon, data.fromExplosion);
		data.cancel = true;
	}

	public void GetHurt(Vector3 force, float multiplier, GameObject sourceWeapon = null, bool fromExplosion = false)
	{
		bool flag = false;
		if (!crashing)
		{
			if ((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && !enemy.parryable && enemy.health - multiplier > 0f)
			{
				return;
			}
			if (((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && enemy.parryable) || eid.hitter == "punch")
			{
				if (enemy.parryable)
				{
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						multiplier = ((enemy.parryFramesLeft > 0) ? 3 : 4);
					}
					MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
					enemy.parryable = false;
					if ((bool)enemy.parryChallenge)
					{
						enemy.parryChallenge.Done();
					}
				}
				else
				{
					enemy.parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
				}
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				enemy.health -= 1f * multiplier;
			}
			else
			{
				multiplier = 0f;
			}
			enemy.health = (float)Math.Round(enemy.health, 4);
			if ((double)enemy.health <= 0.001)
			{
				enemy.health = 0f;
			}
			if (enemy.health <= 0f)
			{
				flag = true;
			}
			if (homeRunnable && !fleshDrone && !eid.puppet && flag && (eid.hitter == "punch" || eid.hitter == "heavypunch" || eid.hitter == "hammer"))
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.homerun", sourceWeapon, eid);
				MonoSingleton<StyleCalculator>.Instance.AddToMultiKill();
			}
			else if (eid.hitter != "enemy" && !eid.puppet && multiplier != 0f)
			{
				if (enemy.scalc == null)
				{
					enemy.scalc = MonoSingleton<StyleCalculator>.Instance;
				}
				if ((bool)enemy.scalc)
				{
					enemy.scalc.HitCalculator(eid.hitter, "drone", "", flag, eid, sourceWeapon);
				}
			}
			if (enemy.health <= 0f && !crashing)
			{
				enemy.parryable = false;
				Death(fromExplosion);
				if (eid.hitter != "punch" && eid.hitter != "heavypunch" && eid.hitter != "hammer")
				{
					if (eid.target != null)
					{
						crashTarget = (hasDroneVision ? droneTargetData.position : eid.target.position);
					}
				}
				else
				{
					canHurtOtherDrones = true;
					base.transform.position += force.normalized;
					crashTarget = base.transform.position + force;
					rb.velocity = force.normalized * 40f;
				}
				base.transform.LookAt(crashTarget);
				if (eid.enemyType == EnemyType.Drone)
				{
					aud.clip = deathSound;
					aud.volume = 0.75f;
					aud.SetPitch(UnityEngine.Random.Range(0.85f, 1.35f));
					aud.priority = 11;
					aud.Play(tracked: true);
				}
				else
				{
					PlaySound(deathSound);
				}
			}
			else if (eid.hitter != "fire")
			{
				GameObject gameObject = null;
				Bloodsplatter bloodsplatter = null;
				if (multiplier != 0f)
				{
					PlaySound(hurtSound);
					gameObject = bsm.GetGore(GoreType.Body, eid, fromExplosion);
					gameObject.transform.position = base.transform.position;
					gameObject.SetActive(value: true);
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
					if (eid.hitter == "drill")
					{
						gameObject.transform.localScale *= 2f;
					}
					bloodsplatter = gameObject.GetComponent<Bloodsplatter>();
				}
				if (enemy.health > 0f)
				{
					if ((bool)bloodsplatter)
					{
						bloodsplatter.GetReady();
					}
					if (!eid.blessed)
					{
						rb.velocity /= 10f;
						rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
						if (eid.enemyType != EnemyType.Providence)
						{
							rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
						}
						if (rb.velocity.magnitude > 50f)
						{
							rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f);
						}
					}
				}
				if (multiplier >= 1f)
				{
					if ((bool)bloodsplatter)
					{
						bloodsplatter.hpAmount = 30;
					}
					if (gib != null)
					{
						for (int i = 0; (float)i <= multiplier; i++)
						{
							UnityEngine.Object.Instantiate(gib.ToAsset(), base.transform.position, UnityEngine.Random.rotation).transform.SetParent(gz.gibZone, worldPositionStays: true);
						}
					}
				}
				if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && (bool)gameObject && gameObject.TryGetComponent<ParticleSystem>(out var component))
				{
					component.Play();
				}
			}
			else
			{
				PlaySound(hurtSound);
			}
		}
		else if ((eid.hitter == "punch" || eid.hitter == "hammer") && !parried)
		{
			parried = true;
			rb.velocity = Vector3.zero;
			base.transform.rotation = MonoSingleton<CameraController>.Instance.transform.rotation;
			Punch currentPunch = MonoSingleton<FistControl>.Instance.currentPunch;
			if (eid.hitter == "punch")
			{
				currentPunch.GetComponent<Animator>().Play("Hook", -1, 0.065f);
				currentPunch.Parry(hook: false, eid);
				if ((bool)enemy.parryChallenge)
				{
					enemy.parryChallenge.Done();
				}
			}
			if (eid.enemyType == EnemyType.Virtue && TryGetComponent<Collider>(out var component2))
			{
				component2.isTrigger = true;
			}
		}
		else if (multiplier >= 1f || canInterruptCrash)
		{
			Explode();
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
	}

	private void UpdateRigidbodySettings()
	{
		if (!hasDroneVision && !crashing)
		{
			rb.drag = 3f;
			rb.angularDrag = 3f;
		}
		else
		{
			rb.drag = 0f;
			rb.angularDrag = 0f;
		}
	}

	private int GetSightBlockMask(bool isPlayer)
	{
		return LayerMaskDefaults.Get((!isPlayer) ? LMD.Environment : LMD.EnvironmentAndBigEnemies);
	}

	private float GetCooldownSpeed()
	{
		float num = (float)difficulty / 2f;
		if (eid.enemyType == EnemyType.Providence && difficulty >= 3)
		{
			num -= (num - 1f) / 2f;
		}
		else if (eid.enemyType == EnemyType.Virtue && difficulty >= 4)
		{
			num = 1.2f;
		}
		else if (difficulty == 1)
		{
			num = 0.75f;
		}
		else if (difficulty == 0)
		{
			num = 0.5f;
		}
		if (num == 0f)
		{
			num = 0.25f;
		}
		return num * eid.totalSpeedModifier;
	}

	private bool ShouldProcessAttack()
	{
		if (eid.enemyType == EnemyType.Virtue)
		{
			if (eid.target == null)
			{
				return false;
			}
			if (eid.target.isPlayer && MonoSingleton<NewMovement>.Instance.levelOver)
			{
				return false;
			}
			if (hasDroneVision)
			{
				droneTargetData = vision.CalculateData(droneTargetHandle);
				if (!(droneTargetData.DistanceTo(base.transform.position) < 150f))
				{
					return stationary;
				}
				return true;
			}
			return true;
		}
		if (!hasDroneVision)
		{
			return false;
		}
		droneTargetData = vision.CalculateData(droneTargetHandle);
		if (!ObstructionCheck(droneTargetData))
		{
			return false;
		}
		return targetSpotted;
	}

	private bool ObstructionCheck(TargetDataRef data)
	{
		return !data.IsObstructed(base.transform.position, droneVisionMask);
	}

	private bool ObstructionCheck(TargetData data)
	{
		return !data.IsObstructed(base.transform.position, droneVisionMask);
	}

	private void ProcessAttack()
	{
		if ((bool)vc && eid.enemyType == EnemyType.Virtue)
		{
			vc.virtueCooldown = 1f / eid.totalSpeedModifier;
		}
		enemy.parryable = true;
		if ((UnityEngine.Object)(object)chargeParticle != null)
		{
			chargeParticle.Play();
		}
		if (shootMaterial != null && !eid.puppet && eid.enemyType == EnemyType.Drone)
		{
			EnemySimplifier[] componentsInChildren = modelTransform.GetComponentsInChildren<EnemySimplifier>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				EnemySimplifier[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ChangeMaterialNew(EnemySimplifier.MaterialState.normal, shootMaterial);
				}
			}
		}
		if (eid.enemyType == EnemyType.Drone || eid.enemyType == EnemyType.Providence)
		{
			attackCooldown = UnityEngine.Random.Range(2f, 4f);
			if (secondaryChance > 0f)
			{
				if (UnityEngine.Random.Range(0f, 1f) < currentSecondaryChance)
				{
					currentSecondaryChance = 0f;
					PlaySound(secondaryWindupSound);
					ShootSecondary();
				}
				else
				{
					currentSecondaryChance += secondaryChance;
					PrepShoot();
				}
			}
			else
			{
				PrepShoot();
			}
		}
		else
		{
			attackCooldown = UnityEngine.Random.Range(4f, 6f);
			if ((UnityEngine.Object)(object)anim != null)
			{
				anim.SetTrigger("Attack");
			}
		}
		if (enemy.parryFramesLeft > 0)
		{
			eid.hitter = "punch";
			eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
			enemy.parryFramesLeft = 0;
		}
	}

	private void ProcessCrashing()
	{
		if (eid.enemyType == EnemyType.Virtue)
		{
			if (parried)
			{
				rb.useGravity = false;
				rb.velocity = base.transform.forward * 120f * eid.totalSpeedModifier;
			}
		}
		else if (!parried)
		{
			float num = 50f;
			if (difficulty == 1)
			{
				num = 40f;
			}
			else if (difficulty == 0)
			{
				num = 25f;
			}
			num *= eid.totalSpeedModifier;
			rb.AddForce(base.transform.forward * num, ForceMode.Acceleration);
			if ((bool)modelTransform)
			{
				modelTransform.Rotate(0f, 0f, 10f, Space.Self);
			}
		}
		else
		{
			rb.velocity = base.transform.forward * 50f;
			if ((bool)modelTransform)
			{
				modelTransform.Rotate(0f, 0f, 50f, Space.Self);
			}
		}
	}

	private void ProcessTargeting()
	{
		if (!hasDroneVision)
		{
			return;
		}
		droneTargetData = vision.CalculateData(droneTargetHandle);
		float num = droneTargetData.DistanceTo(base.transform.position);
		if (eid.enemyType == EnemyType.Drone || eid.enemyType == EnemyType.Providence || eid.enemyType == EnemyType.Mandalore)
		{
			rb.velocity *= 0.95f;
			if (stationary || semiStationary || lockPosition)
			{
				return;
			}
			float num2 = 50f;
			if (difficulty >= 4 || eid.enemyType == EnemyType.Providence)
			{
				num2 = 250f;
			}
			if (num > preferredDistanceToTarget)
			{
				rb.AddForce(base.transform.forward * num2 * eid.totalSpeedModifier, ForceMode.Acceleration);
			}
			else
			{
				if (!(num < 5f))
				{
					return;
				}
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
				{
					rb.AddForce(base.transform.forward * -0.1f * eid.totalSpeedModifier, ForceMode.Impulse);
					return;
				}
				Vector3 vector = -base.transform.forward;
				if (eid.enemyType == EnemyType.Providence)
				{
					vector.y = 0f;
				}
				if (!PortalPhysicsV2.Raycast(base.transform.position, vector.normalized, 7f, LayerMaskDefaults.Get(LMD.Environment), out var _, out var portalTraversals, out var _) && portalTraversals.AllHasFlag(PortalTravellerFlags.Enemy))
				{
					rb.AddForce(vector.normalized * num2 * eid.totalSpeedModifier, ForceMode.Impulse);
				}
			}
		}
		else
		{
			rb.velocity *= 0.975f;
			if (!stationary && !semiStationary && num > 15f)
			{
				rb.AddForce(base.transform.forward * 10f * eid.totalSpeedModifier, ForceMode.Acceleration);
			}
		}
	}

	private void ProcessSearching()
	{
		if (blockCooldown == 0f)
		{
			viewTarget = lastKnownPos;
		}
		else
		{
			blockCooldown = Mathf.MoveTowards(blockCooldown, 0f, 0.01f);
		}
		if (!PortalPhysicsV2.Raycast(base.transform.position, base.transform.forward, 7f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			rb.AddForce(base.transform.forward * 10f * eid.totalSpeedModifier, ForceMode.Acceleration);
		}
		if (checkCooldown == 0f && Vector3.Distance(base.transform.position, lastKnownPos) > 5f)
		{
			checkCooldown = 0.1f;
			if (Physics.BoxCast(base.transform.position - (viewTarget - base.transform.position).normalized, Vector3.one, viewTarget - base.transform.position, base.transform.rotation, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				blockCooldown = UnityEngine.Random.Range(1.5f, 3f);
				Vector3 vector = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
				viewTarget = base.transform.position + vector * 100f;
			}
		}
		else if (Vector3.Distance(base.transform.position, lastKnownPos) <= 3f)
		{
			Physics.Raycast(base.transform.position, UnityEngine.Random.onUnitSphere, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies));
			lastKnownPos = hitInfo.point;
		}
		if (checkCooldown != 0f)
		{
			checkCooldown = Mathf.MoveTowards(checkCooldown, 0f, 0.01f);
		}
	}

	private void SightCheck()
	{
		if (crashing)
		{
			return;
		}
		Invoke("SightCheck", 0.25f);
		bool flag = false;
		TargetDataRef data = default(TargetDataRef);
		if (eid.target != null)
		{
			flag = vision.TrySee(droneTargetQuery, out data);
			if (flag && droneTargetHandle != null && data.CreateHandle() != droneTargetHandle)
			{
				flag = false;
			}
			bool flag2 = hasDimensionalTarget;
			lastDimensionalTarget = Vector3.zero;
			if (!flag)
			{
				enemy.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
				if (hasDimensionalTarget && !flag2)
				{
					droneTargetHandle = null;
				}
				else if (hasDimensionalTarget)
				{
					flag = true;
				}
			}
		}
		else
		{
			droneTargetHandle = null;
			lastDimensionalTarget = Vector3.zero;
		}
		if (targetSpotted && !flag)
		{
			TargetLost();
		}
		else if ((!targetSpotted || (!hasDroneVision && !hasDimensionalTarget)) && flag)
		{
			PlaySound(spotSound);
			targetSpotted = true;
			if (!hasDimensionalTarget)
			{
				Debug.Log("Setting handle");
				droneTargetHandle = data.CreateHandle();
			}
		}
		if (difficulty < 4 || MonoSingleton<EnemyTracker>.Instance.drones.Count <= 1)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		foreach (Drone drone in MonoSingleton<EnemyTracker>.Instance.drones)
		{
			if (!(drone == this) && Vector3.Distance(drone.transform.position, base.transform.position) < 10f)
			{
				zero += base.transform.position - drone.transform.position;
			}
		}
		if (zero.magnitude > 0f)
		{
			Dodge(zero);
		}
	}

	private void TargetLost()
	{
		if (targetSpotted)
		{
			targetSpotted = false;
			PlaySound(loseSound);
		}
		if (hasDroneVision)
		{
			lastKnownPos = vision.CalculateData(droneTargetHandle).position;
		}
		else if (hasDimensionalTarget)
		{
			lastKnownPos = lastDimensionalTarget;
		}
		else
		{
			if (eid.target == null)
			{
				return;
			}
			lastKnownPos = eid.target.position;
		}
		blockCooldown = 0f;
		checkCooldown = 0f;
		toLastKnownPos = true;
		if (eid.enemyType != EnemyType.Virtue)
		{
			droneTargetHandle = null;
		}
	}

	public void RandomDodge(bool force = false)
	{
		if (force || (difficulty != 0 && (difficulty != 1 || (eid.enemyType != EnemyType.Providence && !(UnityEngine.Random.Range(0f, 1f) > 0.75f)))))
		{
			Dodge();
			if (difficulty >= 2 && (difficulty >= 4 || force) && eid.enemyType == EnemyType.Providence)
			{
				Invoke("Dodge", 0.1f);
			}
		}
	}

	public void Dodge()
	{
		Vector3 direction = base.transform.up * UnityEngine.Random.Range(-5f, 5f) + base.transform.right * UnityEngine.Random.Range(-5f, 5f);
		if (PortalPhysicsV2.Raycast(base.transform.position, direction, 7f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			direction *= -1f;
		}
		Dodge(direction);
	}

	public void Dodge(Vector3 direction)
	{
		float num = 50f;
		if (eid.enemyType == EnemyType.Providence)
		{
			num = 750f;
		}
		else if (eid.enemyType == EnemyType.Virtue)
		{
			num = 150f;
		}
		num *= eid.totalSpeedModifier;
		if (PortalPhysicsV2.Raycast(base.transform.position, direction.normalized, 7f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			direction *= -1f;
		}
		if ((bool)(UnityEngine.Object)(object)dodgeSound && !hasDimensionalTarget)
		{
			dodgeSound.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.allGroup, base.transform.position, 128, 0f, 0.15f, UnityEngine.Random.Range(0.75f, 1.25f), (AudioRolloffMode)1);
		}
		rb.AddForce(direction.normalized * num, ForceMode.Impulse);
	}

	public void PlaySound(AudioClip clip)
	{
		if ((bool)(UnityEngine.Object)(object)clip)
		{
			aud.clip = clip;
			if (eid.enemyType == EnemyType.Drone)
			{
				aud.volume = 0.5f;
				aud.SetPitch(UnityEngine.Random.Range(0.85f, 1.35f));
			}
			aud.priority = 12;
			aud.Play(tracked: true);
		}
	}

	private void PrepShoot()
	{
		if (windUpSounds != null && windUpSounds.Length != 0)
		{
			PlaySound(windUpSounds[UnityEngine.Random.Range(0, windUpSounds.Length)]);
		}
		if (eid.enemyType == EnemyType.Providence)
		{
			anim.Play("Shoot");
		}
		Invoke("Shoot", 0.75f / eid.totalSpeedModifier);
	}

	public void Shoot()
	{
		enemy.parryable = false;
		if (crashing || !projectile.RuntimeKeyIsValid())
		{
			return;
		}
		if (eid.enemyType == EnemyType.Drone && !eid.puppet)
		{
			EnemySimplifier[] componentsInChildren = modelTransform.GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ChangeMaterialNew(EnemySimplifier.MaterialState.normal, origMaterial);
			}
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		Vector3 position2 = position + forward;
		Quaternion quaternion = base.transform.rotation;
		PortalPhysicsV2.ProjectThroughPortals(position, forward, default(LayerMask), out var _, out var endPoint, out var traversals);
		bool flag = false;
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
				position2 = endPoint;
				quaternion = travelMatrix.rotation * quaternion;
			}
			else
			{
				position2 = portalObject.GetTransform(portalHandle.side).GetPositionInFront(traversals[0].entrancePoint, 0.05f);
				flag = !portalObject.passThroughNonTraversals;
			}
		}
		List<Projectile> list = new List<Projectile>();
		GameObject gameObject = UnityEngine.Object.Instantiate(projectile.ToAsset(), position2, quaternion);
		if (eid.enemyType == EnemyType.Drone)
		{
			Transform obj = gameObject.transform;
			Vector3 position3 = obj.position;
			Vector3 up = obj.up;
			Quaternion rotation = obj.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			gameObject.transform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, UnityEngine.Random.Range(0, 360));
			gameObject.transform.localScale *= 0.5f;
			list.Add(gameObject.GetComponent<Projectile>());
			SetProjectileSettings(list[list.Count - 1]);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(projectile.ToAsset(), position3 + up, rotation);
			if (difficulty > 2)
			{
				gameObject2.transform.rotation = Quaternion.Euler(eulerAngles.x + 10f, eulerAngles.y, eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			list.Add(gameObject2.GetComponent<Projectile>());
			SetProjectileSettings(list[list.Count - 1]);
			gameObject2 = UnityEngine.Object.Instantiate(projectile.ToAsset(), position3 - up, rotation);
			if (difficulty > 2)
			{
				gameObject2.transform.rotation = Quaternion.Euler(eulerAngles.x - 10f, eulerAngles.y, eulerAngles.z);
			}
			gameObject2.transform.localScale *= 0.5f;
			list.Add(gameObject2.GetComponent<Projectile>());
			SetProjectileSettings(list[list.Count - 1]);
		}
		else if (eid.enemyType == EnemyType.Providence)
		{
			SetProjectileSettings(gameObject.GetComponent<Projectile>());
			rb.AddForce(base.transform.forward * -1500f, ForceMode.Impulse);
		}
		else
		{
			SetProjectileSettings(gameObject.GetComponent<Projectile>());
		}
		if (flag)
		{
			for (int j = 1; j < list.Count; j++)
			{
				list[j].Explode();
			}
		}
	}

	public void ShootSecondary()
	{
		if (!crashing && secondaryProjectile.RuntimeKeyIsValid())
		{
			anim.Play("BeamPrep");
			GameObject obj = UnityEngine.Object.Instantiate(secondaryProjectile.ToAsset(), base.transform.position + base.transform.forward, base.transform.rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: true);
			if (obj.TryGetComponent<Pincer>(out var component))
			{
				component.difficulty = difficulty;
				droneCurrentRotation = 0f;
				droneWingRotationSpeed = ((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? 1 : (-1));
				component.direction *= droneWingRotationSpeed;
				component.firedMessageReceiver = base.gameObject;
			}
		}
	}

	public void PincerFired()
	{
		enemy.parryable = false;
		anim.Play("BeamShoot");
		rb.AddForce(base.transform.forward * -1500f, ForceMode.Impulse);
		droneWingRotationSpeed = 0f;
		for (int i = 0; i < rotatorWings.Length; i++)
		{
			rotatorWings[i].localRotation = rotatorWingsDefaultRotation[i];
		}
	}

	private void SetProjectileSettings(Projectile proj)
	{
		float num = 35f;
		if (difficulty >= 3)
		{
			num = 45f;
		}
		else if (difficulty == 1)
		{
			num = 25f;
		}
		else if (difficulty == 0)
		{
			num = 15f;
		}
		if (eid.enemyType == EnemyType.Providence)
		{
			num *= 0.85f;
		}
		proj.damage *= eid.totalDamageModifier;
		proj.targetHandle = droneTargetHandle;
		proj.safeEnemyType = eid.enemyType;
		proj.speed = num;
		if ((bool)enemy.parryChallenge)
		{
			proj.parryChallenge = enemy.parryChallenge;
		}
	}

	public void SpawnDroneInsignia()
	{
		if (eid.target != null && !crashing)
		{
			enemy.parryable = false;
			GameObject gameObject = UnityEngine.Object.Instantiate(projectile.ToAsset(), eid.target.position, Quaternion.identity);
			gameObject.SetActive(value: false);
			VirtueInsignia component = gameObject.GetComponent<VirtueInsignia>();
			component.target = eid.target;
			component.parentEnemy = enemy;
			component.hadParent = true;
			if (isEnraged)
			{
				component.predictive = true;
			}
			if (difficulty == 1)
			{
				component.windUpSpeedMultiplier = 0.875f;
			}
			else if (difficulty == 0)
			{
				component.windUpSpeedMultiplier = 0.75f;
			}
			if (difficulty >= 4)
			{
				component.explosionLength = ((difficulty == 5) ? 5f : 3.5f);
			}
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
			{
				gameObject.transform.localScale *= 0.75f;
				component.windUpSpeedMultiplier *= 0.875f;
			}
			component.windUpSpeedMultiplier *= eid.totalSpeedModifier;
			component.damage = Mathf.RoundToInt((float)component.damage * eid.totalDamageModifier);
			gameObject.SetActive(value: true);
			chargeParticle.Stop(false, (ParticleSystemStopBehavior)0);
			usedAttacks++;
			if (((difficulty > 2 && usedAttacks > 2) || (difficulty == 2 && usedAttacks > 4 && !eid.blessed)) && !isEnraged && !eid.puppet && vc.currentVirtues.Count < 3)
			{
				Invoke("Enrage", 3f / eid.totalSpeedModifier);
			}
		}
	}

	private void HandleCollisionWithEnemy(Collider other)
	{
		if (checkingForCrash)
		{
			return;
		}
		checkingForCrash = true;
		EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
		EnemyIdentifier enemyIdentifier = ((!component || !component.eid) ? other.gameObject.GetComponent<EnemyIdentifier>() : component.eid);
		if ((bool)enemyIdentifier)
		{
			bool flag = true;
			if (!enemyIdentifier.dead)
			{
				flag = false;
			}
			enemyIdentifier.hitter = "cannonball";
			enemyIdentifier.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 100f, base.transform.position, 5f * eid.totalDamageModifier, tryForExplode: true);
			if (!enemyIdentifier || enemyIdentifier.dead)
			{
				if (!flag)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.cannonballed", null, enemyIdentifier);
				}
				if ((bool)enemyIdentifier)
				{
					enemyIdentifier.Explode();
				}
				checkingForCrash = false;
			}
			else
			{
				Explode();
			}
		}
		else
		{
			checkingForCrash = false;
		}
	}

	public void Explode()
	{
		if (exploded || !base.gameObject.activeInHierarchy || (cantInstaExplode && !canInterruptCrash))
		{
			return;
		}
		exploded = true;
		GameObject obj = UnityEngine.Object.Instantiate(this.explosion.ToAsset(), base.transform.position, Quaternion.identity);
		obj.transform.SetParent(gz.transform, worldPositionStays: true);
		Explosion[] componentsInChildren = obj.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (eid.totalDamageModifier != 1f)
			{
				explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
				explosion.maxSize *= eid.totalDamageModifier;
				explosion.speed *= eid.totalDamageModifier;
			}
			if (difficulty >= 4 && eid.enemyType == EnemyType.Drone && !parried && !canHurtOtherDrones)
			{
				explosion.toIgnore.Add(EnemyType.Drone);
			}
			if (killedByPlayer)
			{
				explosion.friendlyFire = true;
			}
		}
		DoubleRender componentInChildren = GetComponentInChildren<DoubleRender>();
		if ((bool)componentInChildren)
		{
			componentInChildren.RemoveEffect();
		}
		if (!crashing)
		{
			Death(fromExplosion: true);
		}
		else if (eid.drillers.Count > 0)
		{
			for (int num = eid.drillers.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(eid.drillers[num].gameObject);
			}
		}
		if (GhostDroneMode.Enabled && ghost != null)
		{
			UnityEngine.Object.Instantiate(ghost, base.transform.position, base.transform.rotation);
		}
		if (spawnOnDeath != null && spawnOnDeath.Length != 0 && !eid.puppet)
		{
			AssetReference[] array = spawnOnDeath;
			foreach (AssetReference val in array)
			{
				if (val.RuntimeKeyIsValid())
				{
					UnityEngine.Object.Instantiate(val.ToAsset(), base.transform.position, base.transform.rotation, gz.transform);
				}
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
		if (enemy.musicRequested)
		{
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			enemy.musicRequested = false;
			if ((bool)instance)
			{
				instance.PlayCleanMusic();
			}
		}
	}

	public void Death(bool fromExplosion = false)
	{
		if (crashing)
		{
			return;
		}
		if (TryGetComponent<Mandalore>(out var component))
		{
			component.DisableShammy();
		}
		crashing = true;
		UpdateRigidbodySettings();
		if (rb.isKinematic)
		{
			rb.isKinematic = false;
		}
		if (eid.enemyType == EnemyType.Providence)
		{
			Explode();
			return;
		}
		Invoke("CanInterruptCrash", 0.5f);
		Invoke("Explode", 5f);
		if (eid.enemyType == EnemyType.Virtue)
		{
			rb.velocity = Vector3.zero;
			rb.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
			rb.useGravity = true;
			if (childVi.Count > 0)
			{
				for (int num = childVi.Count - 1; num >= 0; num--)
				{
					if (childVi[num] != null && (bool)childVi[num].gameObject)
					{
						UnityEngine.Object.Destroy(childVi[num].gameObject);
					}
				}
				childVi.Clear();
			}
		}
		if (eid.hitter != "enemy")
		{
			killedByPlayer = true;
		}
		if (!(eid.hitter != "fire"))
		{
			return;
		}
		GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
		if ((bool)gore)
		{
			gore.transform.position = base.transform.position;
			if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && gore.TryGetComponent<ParticleSystem>(out var component2))
			{
				component2.Play();
			}
			gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
			if (eid.hitter == "drill")
			{
				gore.transform.localScale *= 2f;
			}
			if (gore.TryGetComponent<Bloodsplatter>(out var component3))
			{
				component3.GetReady();
			}
		}
	}

	private void CanInterruptCrash()
	{
		canInterruptCrash = true;
	}

	public void Hooked()
	{
		hooked = true;
		lockPosition = true;
		homeRunnable = true;
		CancelInvoke("DelayedUnhooked");
	}

	public void Unhooked()
	{
		hooked = false;
		Invoke("DelayedUnhooked", 0.25f);
	}

	private void DelayedUnhooked()
	{
		if (!crashing)
		{
			Invoke("NoMoreHomeRun", 0.5f);
		}
		lockPosition = false;
	}

	private void NoMoreHomeRun()
	{
		if (!crashing)
		{
			homeRunnable = false;
		}
	}

	public void Enrage()
	{
		if (!isEnraged && eid.enemyType != EnemyType.Drone && eid.enemyType != EnemyType.Mandalore)
		{
			isEnraged = true;
			currentEnrageEffect = UnityEngine.Object.Instantiate(enrageEffect, base.transform.position, base.transform.rotation);
			currentEnrageEffect.transform.SetParent(base.transform, worldPositionStays: true);
			EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enraged = true;
			}
		}
	}

	public void UnEnrage()
	{
		if (isEnraged)
		{
			isEnraged = false;
			EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
			}
			UnityEngine.Object.Destroy(currentEnrageEffect);
			EnemySimplifier[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
		}
	}

	private void ApproveNewPosition()
	{
		KeepInBounds component = GetComponent<KeepInBounds>();
		if ((bool)component)
		{
			component.ForceApproveNewPosition();
		}
	}

	public void DodgeLaugh()
	{
		if (!((UnityEngine.Object)(object)dodgeLaughSound == null))
		{
			PlaySound(dodgeLaughSound);
		}
	}
}
