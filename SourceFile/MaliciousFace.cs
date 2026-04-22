using System;
using System.Collections.Generic;
using plog;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class MaliciousFace : EnemyScript
{
	private static readonly Logger Log = new Logger("MaliciousFace");

	[Header("Enemy Reference")]
	public Enemy spider;

	private EnemyIdentifier eid;

	private NavMeshAgent nma;

	private Animator anim;

	private Rigidbody rb;

	private AudioSource aud;

	private BloodsplatterManager bsm;

	private GoreZone gz;

	[Header("Combat")]
	public GameObject proj;

	public GameObject spark;

	public GameObject spiderBeam;

	public AssetReference beamExplosion;

	public AssetReference shockwave;

	[Header("Effects")]
	public GameObject breakParticle;

	public GameObject impactParticle;

	public GameObject impactSprite;

	public GameObject dripBlood;

	public GameObject chargeEffect;

	public GameObject enrageEffect;

	public AudioClip hurtSound;

	[Header("Configuration")]
	public bool spiderStationary;

	public float spiderTargetHeight = 1f;

	public Transform mouth;

	public Vector3 sparkRotationOffset = new Vector3(0f, 90f, 0f);

	[SerializeField]
	private Transform headModel;

	[SerializeField]
	private Collider headCollider;

	public Renderer mainMesh;

	public GameObject legController;

	public GameObject[] legs;

	[Header("Materials")]
	private Material origMaterial;

	public Material woundedMaterial;

	public Material woundedEnrageMaterial;

	public GameObject woundedParticle;

	private bool readyToShoot = true;

	private float burstCharge = 5f;

	private int maxBurst;

	private int currentBurst;

	private float maxHealth;

	private float beamCharge;

	private bool chargeEnded = true;

	private float beamProbability;

	private int beamsAmount = 1;

	private float coolDownMultiplier = 1f;

	private NavMeshPath tempPath;

	private bool spiderFalling;

	private bool spiderCorpseBroken;

	private float defaultSpiderHeight;

	private bool beamFiring;

	private bool isBeamPortalBlocked;

	private Quaternion predictedRot;

	private Quaternion followPlayerRot;

	private GameObject currentProj;

	private GameObject currentBeam;

	private GameObject currentExplosion;

	private GameObject currentCE;

	private GameObject currentDrip;

	private GameObject currentEnrageEffect;

	private AlwaysLookAtCamera currentEnrageAlac;

	private AudioSource ceAud;

	private Light ceLight;

	private Vector3 spritePos;

	private Quaternion spriteRot;

	private List<EnemyIdentifier> fallEnemiesHit = new List<EnemyIdentifier>();

	private TargetHandle targetHandle;

	private TargetHandle beamHandle;

	private TargetData lastTargetData;

	private VisionQuery visionQuery;

	private TargetData beamTargetData;

	private Vector3 beamMouthPos;

	private Quaternion beamMouthRot;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private MusicManager muman;

	private bool spiderRequestedMusic;

	private EnemySimplifier[] spiderEnsims;

	private int difficulty = -1;

	private bool spiderParryable;

	private Vector3 predictedPlayerPos;

	private Vision vision => spider.vision;

	private bool charging => beamHandle != null;

	private bool hasVision => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	public bool isEnraged { get; private set; }

	private void Awake()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		spider = GetComponent<Enemy>();
		spider.isSpider = true;
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		nma = GetComponent<NavMeshAgent>();
		anim = GetComponentInChildren<Animator>();
		aud = GetComponent<AudioSource>();
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
		tempPath = new NavMeshPath();
	}

	private void Start()
	{
		if (spiderStationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			spiderStationary = true;
		}
		difficulty = Enemy.InitializeDifficulty(eid);
		maxHealth = spider.health;
		Initialize();
		if (gz == null)
		{
			gz = GoreZone.ResolveGoreZone(base.transform.parent ? base.transform.parent : base.transform);
			spider.gz = gz;
		}
		if (!mainMesh)
		{
			mainMesh = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		origMaterial = mainMesh.material;
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			nma.updateRotation = false;
			if (spiderStationary)
			{
				nma.speed = 0f;
			}
		}
		if ((bool)currentCE)
		{
			UnityEngine.Object.Destroy(currentCE);
		}
		defaultSpiderHeight = spiderTargetHeight;
		readyToShoot = true;
		burstCharge = 1f;
		currentBurst = 0;
		beamCharge = 0f;
		visionQuery = new VisionQuery("MaliciousFaceSight", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.Environment)));
	}

	private void Initialize()
	{
		if (difficulty >= 3)
		{
			coolDownMultiplier = 1.25f;
		}
		else if (difficulty == 1)
		{
			coolDownMultiplier = 0.75f;
		}
		else if (difficulty == 0)
		{
			coolDownMultiplier = 0.5f;
		}
		if (difficulty >= 4)
		{
			maxBurst = 10;
		}
		else if (difficulty >= 2)
		{
			maxBurst = 5;
		}
		else
		{
			maxBurst = 2;
		}
	}

	private void Update()
	{
		if (eid.dead)
		{
			return;
		}
		if (!spider.musicRequested)
		{
			muman = MonoSingleton<MusicManager>.Instance;
			muman.PlayBattleMusic();
			spider.musicRequested = true;
		}
		if (eid.target != null)
		{
			VisionUpdate();
		}
		else
		{
			targetHandle = null;
			lastDimensionalTarget = Vector3.zero;
			if (charging)
			{
				CancelInvoke("BeamFire");
				StopWaiting();
				ceAud.Stop();
				UnityEngine.Object.Destroy(currentCE);
			}
		}
		if (!isEnraged && difficulty > 2 && spider.health < maxHealth / 2f)
		{
			Enrage();
		}
		if (charging)
		{
			if (hasVision && targetHandle != null)
			{
				beamHandle = targetHandle;
			}
			beamTargetData = vision.CalculateData(beamHandle);
			SetBeamHeadRotation(beamTargetData);
			BeamChargeUpdate();
			if (currentEnrageAlac != null && beamTargetData.isAcrossPortals)
			{
				currentEnrageAlac.overrideTargetData = beamTargetData;
			}
		}
		else
		{
			if (beamCharge != 0f)
			{
				return;
			}
			if ((UnityEngine.Object)(object)nma != null && !spiderStationary)
			{
				MovementUpdate();
			}
			if (burstCharge != 0f)
			{
				burstCharge = Mathf.MoveTowards(burstCharge, 0f, Time.deltaTime * coolDownMultiplier * eid.totalSpeedModifier);
			}
			if (currentBurst > maxBurst && burstCharge == 0f)
			{
				currentBurst = 0;
				burstCharge = ((difficulty != 0) ? 1 : 2);
			}
			if (hasVision)
			{
				SetFollowHeadRotation(lastTargetData.headPosition);
				if (readyToShoot && burstCharge == 0f)
				{
					AttackCheck(lastTargetData);
				}
			}
			else if (hasDimensionalTarget)
			{
				SetFollowHeadRotation(lastDimensionalTarget);
			}
		}
	}

	private void OnEnable()
	{
		if (!eid.dead)
		{
			spiderRequestedMusic = false;
			if (muman == null)
			{
				muman = MonoSingleton<MusicManager>.Instance;
			}
			if ((bool)muman)
			{
				muman.PlayCleanMusic();
			}
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnDisable()
	{
		if (!eid.dead)
		{
			spiderRequestedMusic = false;
			spider.musicRequested = false;
			if (muman == null)
			{
				muman = MonoSingleton<MusicManager>.Instance;
			}
			if ((bool)muman)
			{
				muman.PlayCleanMusic();
			}
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (spiderFalling || spider.falling)
		{
			HandleCollision(other);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (spiderFalling || spider.falling)
		{
			TriggerHit(other);
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = 3.5f,
			angularSpeed = 0f,
			acceleration = 10f
		};
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		Die();
		spider.limp = true;
	}

	public override void OnDamage(ref DamageData data)
	{
		HandleSpiderDamage(ref data);
	}

	public override void OnParry(ref DamageData data, bool isShotgun)
	{
		if (spiderParryable)
		{
			spiderParryable = false;
			MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
			currentExplosion = UnityEngine.Object.Instantiate(beamExplosion.ToAsset(), base.transform.position, Quaternion.identity);
			if (!InvincibleEnemies.Enabled && !eid.blessed)
			{
				spider.health -= (float)((spider.parryFramesLeft > 0) ? 4 : 5) / eid.totalHealthModifier;
			}
			Explosion[] componentsInChildren = currentExplosion.GetComponentsInChildren<Explosion>();
			foreach (Explosion obj in componentsInChildren)
			{
				obj.speed *= eid.totalDamageModifier;
				obj.maxSize *= 1.75f * eid.totalDamageModifier;
				obj.damage = Mathf.RoundToInt(50f * eid.totalDamageModifier);
				obj.canHit = AffectedSubjects.EnemiesOnly;
				obj.friendlyFire = true;
			}
			if (currentEnrageEffect == null)
			{
				CancelInvoke("BeamFire");
				Invoke("StopWaiting", 1f);
				UnityEngine.Object.Destroy(currentCE);
			}
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		PortalHandleSequence sequence = details.portalSequence.Reversed();
		beamHandle?.From(sequence);
		targetHandle?.From(sequence);
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (beamHandle != null && traveller.id == beamHandle.id)
		{
			beamHandle = beamHandle.Then(details.portalSequence);
		}
		if (this.targetHandle != null && traveller.id == this.targetHandle.id)
		{
			this.targetHandle = this.targetHandle.Then(details.portalSequence);
		}
		if (currentEnrageAlac != null)
		{
			TargetHandle targetHandle = beamHandle ?? this.targetHandle;
			currentEnrageAlac.overrideTargetData = ((targetHandle != null && targetHandle.portals.Count > 0) ? new TargetData?(vision.CalculateData(targetHandle)) : ((TargetData?)null));
		}
	}

	private void VisionUpdate()
	{
		if (visionQuery != null)
		{
			if (vision.TrySee(visionQuery, out var data))
			{
				targetHandle = data.CreateHandle();
				lastDimensionalTarget = Vector3.zero;
				lastTargetData = data.ToData();
			}
			else
			{
				targetHandle = null;
				spider.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
			}
			if (!charging && currentEnrageAlac != null)
			{
				currentEnrageAlac.overrideTargetData = ((hasVision && data.isAcrossPortals) ? new TargetData?(data.ToData()) : ((TargetData?)null));
			}
		}
	}

	private void SetBeamHeadRotation(TargetData targetData)
	{
		if (beamFiring)
		{
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRot, Quaternion.Angle(headModel.transform.rotation, predictedRot) * Time.deltaTime * 20f * eid.totalSpeedModifier);
			return;
		}
		Vector3 position = targetData.position;
		predictedRot = Quaternion.LookRotation(position - base.transform.position, base.transform.up);
		headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRot, (Quaternion.Angle(headModel.transform.rotation, predictedRot) + 10f) * Time.deltaTime * 10f * eid.totalSpeedModifier);
	}

	private void SetFollowHeadRotation(Vector3 targetPosition)
	{
		followPlayerRot = Quaternion.LookRotation((targetPosition - base.transform.position).normalized, base.transform.up);
		headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, followPlayerRot, (Quaternion.Angle(headModel.transform.rotation, followPlayerRot) + 10f) * Time.deltaTime * 15f * eid.totalSpeedModifier);
	}

	private void MovementUpdate()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Invalid comparison between Unknown and I4
		if (spider.isTraversingPortalLink)
		{
			return;
		}
		if (!((Behaviour)(object)nma).enabled)
		{
			((Behaviour)(object)nma).enabled = true;
			if (nma.isOnNavMesh)
			{
				nma.isStopped = false;
			}
			nma.speed = 3.5f * eid.totalSpeedModifier;
		}
		if (!nma.isOnNavMesh)
		{
			return;
		}
		spiderTargetHeight = defaultSpiderHeight;
		if ((bool)eid.buffTargeter)
		{
			spider.SetDestination(eid.buffTargeter.transform.position);
			if (Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) < 15f)
			{
				spiderTargetHeight = 0.35f;
			}
		}
		else if (hasDimensionalTarget)
		{
			spider.SetDestination(lastDimensionalTarget);
		}
		else if (eid.target != null)
		{
			Vector3 position = eid.target.position;
			if (nma.CalculatePath(position, tempPath) && (int)tempPath.status == 0)
			{
				nma.SetDestination(position);
			}
			else if (hasVision)
			{
				bool flag = false;
				if (lastTargetData.isValid() && lastTargetData.isAcrossPortals && lastTargetData.IsTargetInPortalOrth())
				{
					PortalHandleSequence portals = lastTargetData.handle.portals;
					PortalHandle handle = portals[portals.Count - 1];
					flag = PortalUtils.GetPortalObject(handle).GetTransform(handle.side).IsFacingDown();
				}
				if (flag && PortalEnemyUtils.CalcNavPosToPortalProjection_TargetWithPortalNormal(base.transform.position, lastTargetData, nma.areaMask, out var navPos))
				{
					nma.SetDestination(navPos);
				}
				else
				{
					nma.SetDestination(base.transform.position);
				}
			}
			else
			{
				nma.SetDestination(base.transform.position);
			}
		}
		nma.baseOffset = Mathf.MoveTowards(nma.baseOffset, spiderTargetHeight, Time.deltaTime * defaultSpiderHeight / 2f * eid.totalSpeedModifier);
	}

	private void AttackCheck(TargetData targetData)
	{
		if (currentBurst != 0)
		{
			ShootProj(targetData);
		}
		else
		{
			if (!hasVision)
			{
				return;
			}
			float num = targetData.DistanceTo(base.transform.position);
			if (!(Quaternion.Angle(headModel.rotation, followPlayerRot) < 1f) && !(num < 10f))
			{
				return;
			}
			bool num2 = beamProbability > 5f || UnityEngine.Random.Range(0f, spider.health * 0.4f) < beamProbability;
			bool flag = num <= 50f || (eid.target.isPlayer && (bool)MonoSingleton<NewMovement>.Instance.ridingRocket);
			bool flag2 = (bool)eid.buffTargeter && Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) <= 15f;
			if (num2 && flag && !flag2 && !spider.isTraversingPortalLink)
			{
				ChargeBeam(targetData);
				if (difficulty > 2 && isEnraged)
				{
					beamsAmount = 2;
				}
				beamProbability = ((!(spider.health > 10f)) ? 1 : 0);
			}
			else
			{
				ShootProj(targetData);
				beamProbability += 1f;
			}
		}
	}

	private void BeamChargeUpdate()
	{
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			nma.speed = 0f;
			if (nma.isOnNavMesh)
			{
				spider.SetDestination(base.transform.position);
				nma.isStopped = true;
			}
		}
		if (!chargeEnded)
		{
			float num = ((difficulty >= 4) ? 1.5f : 1f);
			beamCharge = Mathf.MoveTowards(beamCharge, 1f, 0.5f * coolDownMultiplier * num * Time.deltaTime * eid.totalSpeedModifier);
			currentCE.transform.localScale = Vector3.one * beamCharge * 2.5f;
			ceAud.SetPitch(beamCharge * 2f);
			ceLight.intensity = beamCharge * 30f;
			if (beamCharge >= 1f)
			{
				chargeEnded = true;
				BeamChargeEnd();
			}
		}
	}

	private void ShootProj(TargetData targetData)
	{
		Vector3 vector = targetData.headPosition;
		Vector3 vector2 = mouth.position;
		Vector3 direction = vector2 - base.transform.position;
		PortalPhysicsV2.ProjectThroughPortals(base.transform.position, direction, default(LayerMask), out var _, out var endPoint, out var traversals);
		bool flag = false;
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				vector2 = endPoint;
				vector = PortalUtils.GetTravelMatrix(traversals).MultiplyPoint3x4(vector);
			}
			else
			{
				flag = !portalObject.passThroughNonTraversals;
			}
		}
		currentProj = UnityEngine.Object.Instantiate(proj, vector2, Quaternion.LookRotation(vector - vector2));
		if (difficulty >= 4)
		{
			switch (currentBurst % 5)
			{
			case 1:
				currentProj.transform.LookAt(vector + base.transform.right * (1 + currentBurst / 5 * 2));
				break;
			case 2:
				currentProj.transform.LookAt(vector + base.transform.up * (1 + currentBurst / 5 * 2));
				break;
			case 3:
				currentProj.transform.LookAt(vector - base.transform.right * (1 + currentBurst / 5 * 2));
				break;
			case 4:
				currentProj.transform.LookAt(vector - base.transform.up * (1 + currentBurst / 5 * 2));
				break;
			}
		}
		Projectile component = currentProj.GetComponent<Projectile>();
		if (!flag)
		{
			component.safeEnemyType = EnemyType.MaliciousFace;
			component.targetHandle = targetData.handle;
			if (difficulty > 2)
			{
				component.speed *= 1.25f;
			}
			else if (difficulty == 1)
			{
				component.speed *= 0.75f;
			}
			else if (difficulty == 0)
			{
				component.speed *= 0.5f;
			}
			component.damage *= eid.totalDamageModifier;
		}
		else
		{
			component.Explode();
		}
		currentBurst++;
		readyToShoot = false;
		if (difficulty >= 4)
		{
			Invoke("ReadyToShoot", 0.05f / eid.totalSpeedModifier);
		}
		else if (difficulty > 0)
		{
			Invoke("ReadyToShoot", 0.1f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("ReadyToShoot", 0.2f / eid.totalSpeedModifier);
		}
	}

	private void ChargeBeam(TargetData targetData)
	{
		chargeEnded = false;
		beamHandle = targetData.handle;
		currentCE = UnityEngine.Object.Instantiate(chargeEffect, mouth);
		currentCE.transform.localScale = Vector3.zero;
		ceAud = currentCE.GetComponent<AudioSource>();
		ceLight = currentCE.GetComponent<Light>();
	}

	private void BeamChargeEnd()
	{
		if (beamsAmount <= 1 && (bool)(UnityEngine.Object)(object)ceAud)
		{
			ceAud.Stop();
		}
		if (eid.target != null)
		{
			Vector3 velocity = eid.target.GetVelocity();
			Vector3 vector = beamTargetData.realPosition;
			Vector3 vector2 = eid.target.rigidbody.GetGravityDirection() * 1.67f;
			if (eid.target.isPlayer)
			{
				vector = beamTargetData.realPosition;
				vector += vector2;
				Grenade ridingRocket = MonoSingleton<NewMovement>.Instance.ridingRocket;
				if ((bool)ridingRocket)
				{
					vector = ridingRocket.transform.position;
					velocity = ridingRocket.rb.velocity;
				}
			}
			Vector3 vector3 = new Vector3(velocity.x, velocity.y / (float)((eid.target.isPlayer && (bool)MonoSingleton<NewMovement>.Instance.ridingRocket) ? 1 : 2), velocity.z);
			Vector3 vector4 = vector + vector3 / 2f / eid.totalSpeedModifier;
			PhysicsCastResult hitInfo2;
			PortalTraversalV2[] portalTraversals;
			Vector3 endPoint;
			if (velocity.magnitude > 1f && headCollider.Raycast(new Ray(beamTargetData.realPosition, vector3.normalized), out var _, vector3.magnitude / 2f / eid.totalSpeedModifier))
			{
				vector4 = vector;
			}
			else if (PortalPhysicsV2.Raycast(beamTargetData.realPosition, vector4 - vector, Vector3.Distance(vector4, vector), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), out hitInfo2, out portalTraversals, out endPoint, QueryTriggerInteraction.Collide))
			{
				vector4 = ((portalTraversals.Length != 0) ? PortalUtils.GetTravelMatrix(portalTraversals).inverse.MultiplyPoint3x4(hitInfo2.point) : hitInfo2.point);
			}
			predictedPlayerPos = beamTargetData.portalMatrix.MultiplyPoint3x4(vector4);
			predictedRot = Quaternion.LookRotation(predictedPlayerPos - base.transform.position, base.transform.up);
			Vector3 vector5 = mouth.position;
			Vector3 direction = vector5 - base.transform.position;
			PortalPhysicsV2.ProjectThroughPortals(base.transform.position, direction, default(LayerMask), out var _, out var endPoint2, out var traversals);
			isBeamPortalBlocked = false;
			if (traversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV = traversals[0];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				if (portalTraversalV.portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
				{
					vector5 = endPoint2;
					predictedPlayerPos = PortalUtils.GetTravelMatrix(traversals).MultiplyPoint3x4(predictedPlayerPos);
				}
				else
				{
					isBeamPortalBlocked = true;
				}
			}
			Quaternion quaternion = Quaternion.LookRotation(predictedPlayerPos - vector5);
			beamMouthPos = vector5;
			beamMouthRot = quaternion;
			GameObject obj = UnityEngine.Object.Instantiate(spark, beamMouthPos, beamMouthRot);
			obj.transform.LookAt(predictedPlayerPos);
			obj.transform.Rotate(sparkRotationOffset);
		}
		beamFiring = true;
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			((Behaviour)(object)nma).enabled = false;
		}
		if (difficulty > 1)
		{
			Invoke("BeamFire", 0.5f / eid.totalSpeedModifier);
		}
		else if (difficulty == 1)
		{
			Invoke("BeamFire", 0.75f / eid.totalSpeedModifier);
		}
		else
		{
			Invoke("BeamFire", 1f / eid.totalSpeedModifier);
		}
		spiderParryable = true;
		if (spider.parryFramesLeft > 0)
		{
			eid.hitter = "punch";
			eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
		}
	}

	private void UpdateBeamMouth()
	{
		Vector3 vector = mouth.position;
		Vector3 direction = vector - base.transform.position;
		Quaternion quaternion = mouth.rotation;
		PortalPhysicsV2.ProjectThroughPortals(base.transform.position, direction, default(LayerMask), out var _, out var endPoint, out var traversals);
		isBeamPortalBlocked = false;
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			if (portalTraversalV.portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				vector = endPoint;
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
				quaternion = Quaternion.LookRotation(travelMatrix.MultiplyVector(quaternion * Vector3.forward), travelMatrix.MultiplyVector(quaternion * Vector3.up));
			}
			else
			{
				isBeamPortalBlocked = true;
			}
			beamMouthPos = vector;
			beamMouthRot = quaternion;
		}
	}

	private void BeamFire()
	{
		if (eid.dead)
		{
			return;
		}
		spiderParryable = false;
		beamFiring = false;
		if (!isBeamPortalBlocked)
		{
			UpdateBeamMouth();
			Vector3 vector = predictedPlayerPos - mouth.transform.position;
			if (Vector3.Angle(vector, mouth.transform.forward) > 20f)
			{
				vector = mouth.transform.forward;
			}
			if (!isBeamPortalBlocked)
			{
				currentBeam = UnityEngine.Object.Instantiate(spiderBeam, mouth.transform.position, Quaternion.LookRotation(vector));
				if (eid.totalDamageModifier != 1f && currentBeam.TryGetComponent<RevolverBeam>(out var component))
				{
					component.damage *= eid.totalDamageModifier;
				}
			}
		}
		if (beamsAmount > 1)
		{
			beamsAmount--;
			ceAud.SetPitch(4f);
			ceAud.volume = 1f;
			Invoke("BeamChargeEnd", 0.5f / eid.totalSpeedModifier);
		}
		else
		{
			UnityEngine.Object.Destroy(currentCE);
			Invoke("StopWaiting", 1f / eid.totalSpeedModifier);
		}
	}

	private void StopWaiting()
	{
		if (!eid.dead)
		{
			beamCharge = 0f;
			beamHandle = null;
			beamFiring = false;
			chargeEnded = false;
		}
	}

	private void ReadyToShoot()
	{
		readyToShoot = true;
	}

	public void Die(bool ignoreAlreadyDead = false)
	{
		if (ignoreAlreadyDead || !eid.dead)
		{
			ProcessDeath();
		}
	}

	public void ProcessDeath()
	{
		eid.dead = true;
		rb = GetComponentInChildren<Rigidbody>();
		DoubleRender[] componentsInChildren = GetComponentsInChildren<DoubleRender>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RemoveEffect();
		}
		spiderFalling = true;
		rb.excludeLayers = 1;
		spider.falling = true;
		spiderParryable = false;
		if (rb != null)
		{
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
			rb.velocity = Vector3.zero;
		}
		if (spider.health > 0f)
		{
			spider.health = 0f;
		}
		base.gameObject.layer = 11;
		ResolveStuckness();
		if (gz != null)
		{
			base.transform.parent.SetParent(gz.transform, worldPositionStays: true);
		}
		UnityEngine.Object.Destroy(legController);
		for (int j = 0; j < legs.Length; j++)
		{
			UnityEngine.Object.Destroy(legs[j]);
		}
		if (currentCE != null)
		{
			UnityEngine.Object.Destroy(currentCE);
		}
		if ((UnityEngine.Object)(object)nma != null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)nma);
		}
		if (muman == null)
		{
			muman = MonoSingleton<MusicManager>.Instance;
		}
		muman.PlayCleanMusic();
		spider.musicRequested = false;
		EnemySimplifier[] array;
		if (currentEnrageEffect != null)
		{
			mainMesh.material = origMaterial;
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].material = origMaterial;
			}
			array = spiderEnsims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			UnityEngine.Object.Destroy(currentEnrageEffect);
		}
		if (spiderEnsims == null)
		{
			spiderEnsims = GetComponentsInChildren<EnemySimplifier>();
		}
		array = spiderEnsims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Begone();
		}
		switch (eid.hitter)
		{
		case "ground slam":
		case "breaker":
		case "cannonball":
			BreakCorpse();
			break;
		}
	}

	public void BreakCorpse()
	{
		if (!spiderCorpseBroken)
		{
			spiderCorpseBroken = true;
			if (breakParticle != null)
			{
				Transform transform = base.transform;
				UnityEngine.Object.Instantiate(breakParticle, transform.position, transform.rotation).transform.SetParent(gz.gibZone);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void ResolveStuckness()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			SphereCollider component = GetComponent<SphereCollider>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Physics.ComputePenetration(component, base.transform.position, base.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance);
				base.transform.position = base.transform.position + direction * (distance + 0.5f);
			}
		}
		array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			BreakCorpse();
		}
	}

	public void TriggerHit(Collider other)
	{
		if (!spiderFalling)
		{
			return;
		}
		EnemyIdentifier enemyIdentifier = other.gameObject.GetComponent<EnemyIdentifier>();
		if (enemyIdentifier == null)
		{
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if (component != null && component.eid != null)
			{
				enemyIdentifier = component.eid;
			}
		}
		if (enemyIdentifier == null && other.gameObject.TryGetComponent<IdolMauricer>(out var _))
		{
			enemyIdentifier = other.gameObject.GetComponentInParent<EnemyIdentifier>();
		}
		if ((bool)enemyIdentifier && enemyIdentifier != eid && !fallEnemiesHit.Contains(enemyIdentifier))
		{
			FallKillEnemy(enemyIdentifier);
		}
	}

	private void FallKillEnemy(EnemyIdentifier targetEid)
	{
		if (!Physics.Linecast(eid.GetCenter().position, targetEid.GetCenter().position, out var _, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			if ((bool)MonoSingleton<StyleHUD>.Instance && !targetEid.dead)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.mauriced", null, eid);
			}
			targetEid.hitter = "maurice";
			fallEnemiesHit.Add(targetEid);
			if (targetEid.TryGetComponent<Collider>(out var component))
			{
				Physics.IgnoreCollision(headCollider, component, ignore: true);
			}
			EnemyIdentifier.FallOnEnemy(targetEid);
		}
	}

	private void HandleCollision(Collision other)
	{
		if (other.gameObject.CompareTag("Moving"))
		{
			BreakCorpse();
			MonoSingleton<CameraController>.Instance.CameraShake(2f);
		}
		else
		{
			if (!LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				return;
			}
			Breakable component4;
			if (other.gameObject.CompareTag("Floor"))
			{
				rb.isKinematic = true;
				rb.SetGravityMode(useGravity: false);
				Transform transform = base.transform;
				UnityEngine.Object.Instantiate(impactParticle, transform.position, transform.rotation);
				spriteRot.eulerAngles = new Vector3(other.contacts[0].normal.x + 90f, other.contacts[0].normal.y, other.contacts[0].normal.z);
				spritePos = new Vector3(other.contacts[0].point.x, other.contacts[0].point.y + 0.1f, other.contacts[0].point.z);
				AudioSource componentInChildren = UnityEngine.Object.Instantiate(shockwave.ToAsset(), spritePos, Quaternion.identity).GetComponentInChildren<AudioSource>();
				if ((bool)(UnityEngine.Object)(object)componentInChildren)
				{
					UnityEngine.Object.Destroy((UnityEngine.Object)(object)componentInChildren);
				}
				Transform transform2 = base.transform;
				transform2.position -= transform2.up * 1.5f;
				spiderFalling = false;
				rb.excludeLayers = default(LayerMask);
				if (!other.gameObject.TryGetComponent<MaliciousFaceCatcher>(out var _))
				{
					UnityEngine.Object.Instantiate(impactSprite, spritePos, spriteRot).transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				if (TryGetComponent<SphereCollider>(out var component2))
				{
					UnityEngine.Object.Destroy(component2);
				}
				SpiderBodyTrigger componentInChildren2 = base.transform.parent.GetComponentInChildren<SpiderBodyTrigger>(includeInactive: true);
				if ((bool)componentInChildren2)
				{
					UnityEngine.Object.Destroy(componentInChildren2.gameObject);
				}
				((Behaviour)(object)rb.GetComponent<NavMeshObstacle>()).enabled = true;
				MonoSingleton<CameraController>.Instance.CameraShake(2f);
				if (fallEnemiesHit.Count <= 0)
				{
					return;
				}
				foreach (EnemyIdentifier item in fallEnemiesHit)
				{
					if (item != null && !item.dead && item.TryGetComponent<Collider>(out var component3))
					{
						Physics.IgnoreCollision(headCollider, component3, ignore: false);
					}
				}
				fallEnemiesHit.Clear();
			}
			else if (other.gameObject.TryGetComponent<Breakable>(out component4) && !component4.playerOnly && !component4.specialCaseOnly)
			{
				component4.Break();
			}
		}
	}

	private void HandleSpiderDamage(ref DamageData data)
	{
		if ((bool)eid && eid.dead)
		{
			if (eid.hitter != "fire" && !eid.sandified && !eid.blessed)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(bsm.GetGore(GoreType.Small, eid, data.fromExplosion));
				gameObject.transform.position = data.hitTarget.transform.position;
				if ((bool)gameObject)
				{
					gameObject.transform.position = data.hitTarget.transform.position;
					if (gz == null)
					{
						gz = GoreZone.ResolveGoreZone(base.transform.parent ? base.transform.parent : base.transform);
					}
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
					if (eid.hitter == "drill")
					{
						gameObject.transform.localScale *= 2f;
					}
					Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
					if ((bool)component)
					{
						if (data.damage >= 1f)
						{
							component.hpAmount = 30;
						}
						if (spider.health > 0f)
						{
							component.GetReady();
						}
					}
				}
			}
			if (eid.hitter == "ground slam" || eid.hitter == "breaker")
			{
				BreakCorpse();
			}
			data.cancel = true;
			return;
		}
		if (eid.hitter != "fire" && !eid.sandified && !eid.blessed)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(bsm.GetGore(GoreType.Small, eid, data.fromExplosion));
			gameObject2.transform.position = data.hitTarget.transform.position;
			if ((bool)gameObject2)
			{
				gameObject2.transform.position = data.hitTarget.transform.position;
				if (gz == null)
				{
					gz = GoreZone.ResolveGoreZone(base.transform.parent ? base.transform.parent : base.transform);
				}
				gameObject2.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (eid.hitter == "drill")
				{
					gameObject2.transform.localScale *= 2f;
				}
				Bloodsplatter component2 = gameObject2.GetComponent<Bloodsplatter>();
				if ((bool)component2)
				{
					if (spider.health > 0f)
					{
						component2.GetReady();
					}
					if (eid.hitter == "nail")
					{
						component2.hpAmount = 3;
						AudioSource component3 = component2.GetComponent<AudioSource>();
						component3.volume *= 0.8f;
					}
					else if (data.damage >= 1f)
					{
						component2.hpAmount = 30;
					}
				}
				if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
				{
					ParticleSystem component4 = gameObject2.GetComponent<ParticleSystem>();
					if ((bool)(UnityEngine.Object)(object)component4)
					{
						component4.Play();
					}
				}
			}
			if (eid.hitter != "shotgun" && eid.hitter != "drill" && base.gameObject.activeInHierarchy && dripBlood != null)
			{
				currentDrip = UnityEngine.Object.Instantiate(dripBlood, data.hitTarget.transform.position, Quaternion.identity);
				if ((bool)currentDrip)
				{
					currentDrip.transform.parent = base.transform;
					currentDrip.transform.LookAt(base.transform);
					currentDrip.transform.Rotate(180f, 180f, 180f);
					if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
					{
						ParticleSystem component5 = currentDrip.GetComponent<ParticleSystem>();
						if ((bool)(UnityEngine.Object)(object)component5)
						{
							component5.Play();
						}
					}
				}
			}
		}
		if (spider.health >= maxHealth / 2f && spider.health - data.damage < maxHealth / 2f)
		{
			if (spiderEnsims == null || spiderEnsims.Length == 0)
			{
				spiderEnsims = GetComponentsInChildren<EnemySimplifier>();
			}
			UnityEngine.Object.Instantiate(woundedParticle, base.transform.position, Quaternion.identity);
			if (!eid.puppet)
			{
				EnemySimplifier[] array = spiderEnsims;
				foreach (EnemySimplifier enemySimplifier in array)
				{
					if (!enemySimplifier.ignoreCustomColor)
					{
						enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
						enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
					}
				}
			}
		}
		if ((bool)(UnityEngine.Object)(object)hurtSound && spider.health > 0f)
		{
			hurtSound.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.goreGroup, base.transform.position, 12, 1f, 0.75f, UnityEngine.Random.Range(0.85f, 1.35f), (AudioRolloffMode)1);
		}
	}

	public void Enrage()
	{
		if (eid.dead || isEnraged)
		{
			return;
		}
		isEnraged = true;
		if (spiderEnsims == null || spiderEnsims.Length == 0)
		{
			spiderEnsims = GetComponentsInChildren<EnemySimplifier>();
		}
		EnemySimplifier[] array = spiderEnsims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enraged = true;
		}
		currentEnrageEffect = UnityEngine.Object.Instantiate(enrageEffect, base.transform);
		currentEnrageEffect.transform.localScale = Vector3.one * 0.2f;
		currentEnrageAlac = currentEnrageEffect.GetComponentInChildren<AlwaysLookAtCamera>();
		if ((bool)currentEnrageAlac)
		{
			if (beamHandle != null)
			{
				currentEnrageAlac.overrideTargetData = vision.CalculateData(beamHandle);
			}
			else if (targetHandle != null)
			{
				currentEnrageAlac.overrideTargetData = vision.CalculateData(targetHandle);
			}
		}
	}

	public void UnEnrage()
	{
		if (!eid.dead && isEnraged)
		{
			isEnraged = false;
			if (spiderEnsims == null || spiderEnsims.Length == 0)
			{
				spiderEnsims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = spiderEnsims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			if (currentEnrageEffect != null)
			{
				UnityEngine.Object.Destroy(currentEnrageEffect);
			}
		}
	}

	public override bool ShouldKnockback(ref DamageData data)
	{
		return false;
	}
}
