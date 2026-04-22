using System.Collections.Generic;
using System.Threading;
using Sandbox;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour, IAlter, IAlterOptions<float>, ITarget, IPortalTraveller
{
	public struct EnemyPortalLinkTraversalData
	{
		public OffMeshLinkData data;

		public PortalHandle portalHandle;

		public bool wasOnLink;

		public float waitTimer;

		public bool hasCrossed;

		public bool startUpdateRotation;

		public bool startUpdatePosition;

		public bool startAutoTraverseOffMeshLink;

		public bool startUseGravity;

		public Vector3 startNavDestination;

		public Vector3 endSpiderOffset;

		public Vector3 portalPos;

		public Vector3 portalTargetPos;

		public Matrix4x4 startEnterToExitMatrix;

		public Vector3 startPortalTargetExitPos;

		public Vector3 startPortalTargetEnterLocalPos;

		public bool hasWarped;

		public Vector3 linkEndPos;

		public readonly bool isWaitExceeded => waitTimer > 0.4f;

		public readonly bool isTraversing
		{
			get
			{
				if (!portalHandle.IsValid())
				{
					return hasCrossed;
				}
				return true;
			}
		}

		public void Reset()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			data = default(OffMeshLinkData);
			portalHandle = PortalHandle.None;
			hasCrossed = false;
			wasOnLink = false;
			portalTargetPos = default(Vector3);
			waitTimer = 0f;
			hasWarped = false;
			linkEndPos = default(Vector3);
		}

		public readonly float GetExitPosDisplacement(PortalTransform currentTransform, Matrix4x4 currentEnterToExit)
		{
			Vector3 local = startPortalTargetEnterLocalPos;
			Vector3 point = currentTransform.LocalToWorld(local);
			return Vector3.Distance(currentEnterToExit.MultiplyPoint3x4(point), startPortalTargetExitPos);
		}

		public readonly float GetEntryPosDisplacement(PortalTransform currentTransform)
		{
			Vector3 local = startPortalTargetEnterLocalPos;
			return Vector3.Distance(currentTransform.LocalToWorld(local), portalTargetPos);
		}
	}

	private static readonly int RunSpeed = Animator.StringToHash("RunSpeed");

	private static readonly RaycastHit[] ReusableRaycastHits = new RaycastHit[8];

	[Header("Enemy Type")]
	public bool isZombie;

	public bool isStatue;

	public bool isMachine;

	public bool isSpider;

	public bool isDrone;

	[Header("Combat")]
	public float health;

	public bool noFallDamage;

	public bool dontDie;

	public bool specialDeath;

	public bool simpleDeath;

	public bool dismemberment;

	[HideInInspector]
	public float originalHealth;

	public bool limp;

	public bool grounded;

	public bool knockedBack;

	public bool falling;

	public float fallTime;

	public float brakes = 1f;

	public float juggleWeight;

	public int parryFramesLeft;

	public bool parryable;

	public bool partiallyParryable;

	[HideInInspector]
	public List<Transform> parryables = new List<Transform>();

	protected bool parryFramesOnPartial;

	public bool isMassDeath;

	public bool isMassDieing;

	[Header("Audio")]
	public AudioClip hurtSound;

	public AudioClip[] hurtSounds;

	public float hurtSoundVol;

	public AudioClip deathSound;

	public float deathSoundVol;

	public AudioClip scream;

	protected AudioSource aud;

	[Header("References")]
	public GameObject chest;

	public Transform bodyCenter;

	public Transform rotationTransform;

	public SkinnedMeshRenderer smr;

	public GoreZone gz;

	public Transform hitJiggleRoot;

	public Enemy symbiote;

	protected EnemyScript script;

	protected EnemyIdentifier eid;

	public BloodsplatterManager bsm;

	[HideInInspector]
	public NavMeshAgent nma;

	protected Rigidbody rb;

	protected Rigidbody[] rbs;

	[HideInInspector]
	public Animator anim;

	[HideInInspector]
	public GroundCheckEnemy gc;

	[HideInInspector]
	public StyleCalculator scalc;

	private ActivateNextWave anw;

	[Header("Materials")]
	public Material deadMaterial;

	public Material woundedMaterial;

	public Material woundedEnrageMaterial;

	protected Material originalMaterial;

	[Header("Effects")]
	public GameObject woundedParticle;

	public GameObject woundedModel;

	public GameObject enrageEffect;

	[HideInInspector]
	public GameObject currentEnrageEffect;

	public GameObject[] destroyOnDeath = new GameObject[0];

	[Header("Enemy Settings")]
	public bool bigKill;

	public bool thickLimbs;

	public bool overrideFalling;

	protected bool symbiotic;

	protected bool healing;

	protected Vector3 jiggleRootPosition;

	public List<GameObject> extraDamageZones = new List<GameObject>();

	public float extraDamageMultiplier;

	public bool bigBlood;

	public readonly List<Transform> transforms = new List<Transform>();

	protected bool affectedByGravity = true;

	public bool variableSpeed;

	public bool stopped;

	public bool isOnOffNavmeshLink;

	public bool chestExploding;

	protected bool chestExploded;

	protected bool attacking;

	protected float defaultSpeed;

	protected float speedMultiplier = 1f;

	private float maxFallSpeed = -500f;

	private float terminalVelocityTimer;

	[HideInInspector]
	public GameObject currentTerminalVelocityEffect;

	protected float chestHP = 3f;

	protected bool noheal;

	protected float fallSpeed;

	protected float knockBackCharge;

	protected int difficulty = -1;

	public LayerMask lmask;

	public LayerMask lmaskEnv;

	public LayerMask lmaskWater;

	protected float reduceFallTime;

	[Header("Events")]
	public UnityEvent onDeath;

	[HideInInspector]
	public ParryChallenge parryChallenge;

	public Vision vision;

	public TimeSince lastTargetTick;

	[HideInInspector]
	public bool musicRequested;

	public CancellationTokenSource deathTokenSource;

	private Vector3 origPos;

	public float startSpeed;

	public float startAngularSpeed;

	public float startAcceleration;

	private Vector3 cachedPos;

	private Vector3 cachedHeadPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

	private EnemyPortalLinkTraversalData portalLinkData;

	public Quaternion postPortalOffsetRot;

	private float postPortalDuration;

	private float postPortalTimer;

	private bool postPortalRotationByCenter;

	protected SwordsMachine sm => script as SwordsMachine;

	public Streetcleaner sc => script as Streetcleaner;

	protected V2 v2 => script as V2;

	protected Mindflayer mf => script as Mindflayer;

	protected Sisyphus sisy => script as Sisyphus;

	protected Turret tur => script as Turret;

	protected Mannequin man => script as Mannequin;

	protected Minotaur min => script as Minotaur;

	protected Mass mass => script as Mass;

	protected Drone drone => script as Drone;

	protected Ferryman fm => script as Ferryman;

	protected Power pwr => script as Power;

	public bool isEnraged { get; private set; }

	private PortalManagerV2 portalManager => MonoSingleton<PortalManagerV2>.Instance;

	private TargetTracker targetTracker => portalManager.TargetTracker;

	public string alterKey => "enemy";

	public string alterCategoryName => "Enemy";

	public AlterOption<float>[] options => new AlterOption<float>[1]
	{
		new AlterOption<float>
		{
			key = "health",
			name = "Health",
			value = health,
			callback = delegate(float value)
			{
				health = value;
			}
		}
	};

	public int Id => GetInstanceID();

	public TargetType Type => TargetType.ENEMY;

	public EnemyIdentifier EID => eid;

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

	public Vector3 HeadPosition => cachedHeadPos;

	public int id => GetInstanceID();

	public int? targetId => GetInstanceID();

	public Vector3 lastPos { get; set; }

	public Vector3 travellerPosition
	{
		get
		{
			if (IsDrone())
			{
				if (!rb)
				{
					return base.transform.position;
				}
				return rb.position;
			}
			if (IsStatue() && eid.enemyType != EnemyType.FleshPrison && eid.enemyType != EnemyType.FleshPanopticon)
			{
				return chest.transform.position;
			}
			if ((bool)bodyCenter)
			{
				return bodyCenter.position;
			}
			if ((bool)chest)
			{
				Vector3 position = base.transform.position;
				return new Vector3(position.x, chest.transform.position.y, position.z);
			}
			return base.transform.position;
		}
	}

	public Vector3 travellerVelocity
	{
		get
		{
			if (!(rb != null))
			{
				return Vector3.zero;
			}
			return rb.velocity;
		}
	}

	public PortalTravellerType travellerType => PortalTravellerType.ENEMY;

	public bool isTraversingPortalLink => portalLinkData.isTraversing;

	public bool IsZombie()
	{
		return isZombie;
	}

	public bool IsStatue()
	{
		return isStatue;
	}

	public bool IsMachine()
	{
		return isMachine;
	}

	public bool IsSpider()
	{
		return isSpider;
	}

	public bool IsDrone()
	{
		return isDrone;
	}

	public float GetSplatVelocity()
	{
		if (IsStatue())
		{
			return -50f;
		}
		if (IsZombie())
		{
			return -50f;
		}
		if (IsMachine())
		{
			return -60f;
		}
		if (IsSpider())
		{
			return -60f;
		}
		if (IsDrone())
		{
			return -50f;
		}
		return 0f;
	}

	protected void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		script = GetComponent<EnemyScript>();
		if (!IsStatue() || !limp)
		{
			if ((Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			if ((IsZombie() || IsSpider()) && smr == null)
			{
				smr = GetComponentInChildren<SkinnedMeshRenderer>();
			}
			nma = GetComponent<NavMeshAgent>();
			if ((bool)(Object)(object)nma)
			{
				startSpeed = nma.speed;
				startAngularSpeed = nma.angularSpeed;
				startAcceleration = nma.acceleration;
			}
			anim = GetComponentInChildren<Animator>();
			bsm = MonoSingleton<BloodsplatterManager>.Instance;
			rb = GetComponent<Rigidbody>();
			rbs = GetComponentsInChildren<Rigidbody>();
			gc = GetComponentInChildren<GroundCheckEnemy>();
			deathTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { base.destroyCancellationToken });
			vision = new Vision(base.transform.position, GetVisionFilter());
			portalLinkData.portalHandle = PortalHandle.None;
		}
	}

	protected void Start()
	{
		if (eid == null)
		{
			return;
		}
		GetGoreZone();
		if (!limp)
		{
			targetTracker.RegisterTarget(this, deathTokenSource.Token);
			targetTracker.RegisterVision(vision, deathTokenSource.Token);
			portalManager.AddTraveller(this, deathTokenSource.Token);
		}
		if (IsStatue() && limp)
		{
			noheal = true;
			return;
		}
		if (smr != null)
		{
			originalMaterial = smr.material;
		}
		if (symbiote != null)
		{
			symbiotic = true;
		}
		if (hitJiggleRoot != null)
		{
			jiggleRootPosition = hitJiggleRoot.localPosition;
		}
		if (!musicRequested && (bool)eid && !eid.dead && (sm == null || !eid.IgnorePlayer))
		{
			MonoSingleton<MusicManager>.Instance.PlayBattleMusic();
			musicRequested = true;
		}
		if (IsStatue() && gc == null)
		{
			affectedByGravity = false;
		}
		if (limp)
		{
			noheal = true;
		}
		if (originalHealth == 0f)
		{
			originalHealth = health;
		}
		if (IsZombie())
		{
			SetSpeed();
		}
		SetupLayerMasks();
	}

	protected void OnEnable()
	{
		parryable = false;
		partiallyParryable = false;
	}

	protected virtual void Update()
	{
		if ((bool)script && (float)lastTargetTick > 0.5f)
		{
			lastTargetTick = 0f;
			script.OnTargetTick();
		}
		if (knockBackCharge > 0f)
		{
			knockBackCharge = Mathf.MoveTowards(knockBackCharge, 0f, Time.deltaTime);
		}
		if (IsMachine())
		{
			UpdateMachineHealing();
		}
		if (isMassDieing)
		{
			base.transform.position = new Vector3(origPos.x + Random.Range(-0.5f, 0.5f), origPos.y + Random.Range(-0.5f, 0.5f), origPos.z + Random.Range(-0.5f, 0.5f));
			if (transforms.Count != 0 && Random.Range(0f, 1f) < Time.deltaTime * 5f)
			{
				int index = Random.Range(0, transforms.Count);
				if ((bool)transforms[index])
				{
					GameObject gore = bsm.GetGore(GoreType.Head, eid);
					if ((bool)gore)
					{
						gore.transform.position = transforms[index].position;
						if ((bool)gz && (bool)gz.goreZone)
						{
							gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
						}
						if (gore.TryGetComponent<Bloodsplatter>(out var component))
						{
							component.GetReady();
						}
					}
				}
				else
				{
					transforms.RemoveAt(index);
				}
			}
		}
		if ((bool)eid && !eid.dead)
		{
			UpdateFalling();
			ValidateNavState();
			UpdatePostPortalRotation();
		}
	}

	protected virtual void FixedUpdate()
	{
		bool flag = gc;
		bool hasNma = (Object)(object)nma;
		bool hasAnim = (Object)(object)anim;
		bool hasRb = rb;
		if (base.transform.position.magnitude > 100000f)
		{
			GoLimp();
		}
		if (parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
		if (IsMachine())
		{
			UpdateJiggleBone();
		}
		if (!eid || eid.dead)
		{
			return;
		}
		SyncVision();
		if (!IsDrone())
		{
			HandlePortalLinkTraversal(hasNma);
			UpdateKnockback();
			if (flag)
			{
				UpdateGroundedState(flag, hasNma, hasAnim);
				UpdateFallingState(flag, hasNma, hasRb, hasAnim);
			}
		}
		if (IsSpider() && parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
	}

	protected void OnDestroy()
	{
		deathTokenSource?.Dispose();
		if (isMassDieing)
		{
			DeathEnd();
		}
	}

	public void SyncVision()
	{
		vision.UpdateFilter(GetVisionFilter());
		vision.UpdateSourcePos(GetVisionPos());
	}

	public Vector3 GetVisionPos()
	{
		if (IsDrone())
		{
			return base.transform.position;
		}
		if ((bool)script)
		{
			return script.VisionSourcePosition;
		}
		return base.transform.position;
	}

	public VisionTypeFilter GetVisionFilter()
	{
		if (!eid)
		{
			return default(VisionTypeFilter);
		}
		if (EnemiesHateEnemies.Active)
		{
			return new VisionTypeFilter(TargetType.PLAYER, TargetType.ENEMY);
		}
		if (eid.enemyType == EnemyType.Swordsmachine || eid.enemyType == EnemyType.Stalker)
		{
			return new VisionTypeFilter(TargetType.PLAYER, TargetType.ENEMY);
		}
		if (eid.target != null && eid.target.isEnemy)
		{
			return new VisionTypeFilter(TargetType.ENEMY);
		}
		if (eid.target != null && eid.target.isPlayer)
		{
			return new VisionTypeFilter(TargetType.PLAYER);
		}
		if (eid.attackEnemies)
		{
			return new VisionTypeFilter(TargetType.PLAYER, TargetType.ENEMY);
		}
		return new VisionTypeFilter(TargetType.PLAYER);
	}

	private void ValidateNavState()
	{
		if (!limp && (bool)eid && !eid.dead && !((Object)(object)nma == null) && !isSpider && !isDrone && !isOnOffNavmeshLink)
		{
			bool flag = rb.isKinematic && (bool)gc && gc.onGround;
			if (flag && !((Behaviour)(object)nma).enabled)
			{
				nma.updatePosition = true;
				nma.updateRotation = true;
				((Behaviour)(object)nma).enabled = true;
			}
			else if (!flag && ((Behaviour)(object)nma).enabled)
			{
				((Behaviour)(object)nma).enabled = false;
			}
		}
	}

	public bool TryGetDimensionalTarget(Vector3 targetPosition, out Vector3 dimensionalTargetPosition)
	{
		dimensionalTargetPosition = default(Vector3);
		if (gz == null || !gz.TryGetComponent<DimensionalArena>(out var component))
		{
			return false;
		}
		if (component.TryGetPortalSide(base.transform.position, out var side))
		{
			if (component.TryGetPortalSide(targetPosition, out var side2))
			{
				if (side == side2)
				{
					return false;
				}
				Vector3 vector = component.TransformPoint(targetPosition, side2);
				dimensionalTargetPosition = vector;
				return true;
			}
			Debug.LogWarning("Target out of bounds of the dimensional arena?");
			return false;
		}
		Debug.LogWarning("Enemy out of bounds of the dimensional arena?");
		return false;
	}

	public bool DefaultVisionCheck(TargetData data)
	{
		ITarget target = data.target;
		if (target.Type != TargetType.PLAYER && target.Type != TargetType.ENEMY)
		{
			return false;
		}
		if (!Vision.ValidateEIDTarget(data, eid.target))
		{
			return false;
		}
		return true;
	}

	private void UpdateGroundedState(bool hasGc, bool hasNma, bool hasAnim)
	{
		if (!hasGc)
		{
			return;
		}
		if (!knockedBack)
		{
			if (!grounded && gc.onGround && (Object)(object)nma != null && IsZombie())
			{
				nma.speed = defaultSpeed;
			}
			grounded = gc.onGround;
		}
		if (!(isZombie && hasNma))
		{
			return;
		}
		isOnOffNavmeshLink = nma.isOnOffMeshLink;
		if (hasAnim && grounded && ((Behaviour)(object)nma).enabled && variableSpeed && nma.isOnNavMesh)
		{
			Vector3 velocity = nma.velocity;
			if (nma.isStopped || velocity == Vector3.zero || stopped)
			{
				anim.SetFloat(RunSpeed, 1f);
			}
			else
			{
				anim.SetFloat(RunSpeed, velocity.magnitude / nma.speed);
			}
		}
	}

	private bool CanFall()
	{
		if (IsZombie())
		{
			return !limp;
		}
		if (IsStatue())
		{
			if (!limp)
			{
				return affectedByGravity;
			}
			return false;
		}
		if (IsMachine())
		{
			if (!limp && !overrideFalling)
			{
				return gc != null;
			}
			return false;
		}
		return false;
	}

	private void StartFalling()
	{
		if (rb == null || (Object)(object)nma == null || (Object)(object)anim == null)
		{
			return;
		}
		if (isTraversingPortalLink)
		{
			AbortLinkTraversal(warpPosition: false);
		}
		((Behaviour)(object)nma).enabled = false;
		if (IsZombie())
		{
			grounded = false;
		}
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		falling = true;
		anim.SetBool("Falling", true);
		if (IsZombie())
		{
			anim.SetTrigger("StartFalling");
		}
		if (IsMachine())
		{
			if (tur != null)
			{
				tur.CancelAim(instant: true);
			}
			if (man != null && man.inAction && !man.jumping && !man.inControl)
			{
				man.CancelActions();
			}
		}
		if (script != null)
		{
			script.OnFall();
		}
	}

	private void UpdateFalling()
	{
		bool flag = false;
		if (IsMachine())
		{
			flag = falling && rb != null && !overrideFalling && (!(Object)(object)nma || !nma.isOnOffMeshLink);
		}
		if (IsZombie())
		{
			flag = falling && !limp;
		}
		if (IsSpider())
		{
			flag = falling && rb != null;
		}
		if (IsStatue())
		{
			flag = falling && rb != null && !overrideFalling && (!(Object)(object)nma || !nma.isOnOffMeshLink);
		}
		if (eid.enemyType == EnemyType.Power && pwr.juggled)
		{
			flag = true;
		}
		if (flag || overrideFalling)
		{
			if (!overrideFalling)
			{
				fallTime += Time.deltaTime;
			}
			else
			{
				fallTime = 0f;
			}
			if ((bool)rb && rb.velocity.y < maxFallSpeed && !InvincibleEnemies.Enabled)
			{
				terminalVelocityTimer += Time.deltaTime;
				if (terminalVelocityTimer > 1f && currentTerminalVelocityEffect == null)
				{
					currentTerminalVelocityEffect = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.terminalVelocityEffect, base.transform.position, Quaternion.LookRotation(rb.velocity.normalized));
					currentTerminalVelocityEffect.transform.SetParent(base.transform, worldPositionStays: true);
				}
				if (terminalVelocityTimer >= 4f)
				{
					if ((bool)currentTerminalVelocityEffect)
					{
						Object.Destroy(currentTerminalVelocityEffect);
					}
					Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.superExplosion, base.transform.position, Quaternion.identity);
					eid.hitter = "terminalvelocity";
					MonoSingleton<StyleHUD>.Instance.AddPoints(350, "ultrakill.terminalvelocity", null, eid);
					eid.Explode();
					for (int i = 0; i < 5; i++)
					{
						GameObject gore = bsm.GetGore(GoreType.Head, this);
						gore.transform.position = base.transform.position;
						GoreZone goreZone = GetGoreZone();
						if (goreZone != null && goreZone.goreZone != null)
						{
							gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
						}
						gore.transform.localScale *= 2f;
						gore.GetComponent<Bloodsplatter>()?.GetReady();
					}
					return;
				}
			}
			else
			{
				if (currentTerminalVelocityEffect != null)
				{
					Object.Destroy(currentTerminalVelocityEffect);
					Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.terminalVelocityExtinguish, base.transform.position, Quaternion.identity).transform.SetParent(base.transform, worldPositionStays: true);
				}
				terminalVelocityTimer = 0f;
			}
			if (overrideFalling)
			{
				return;
			}
			if ((bool)gc)
			{
				bool flag2 = false;
				if (IsMachine())
				{
					flag2 = gc.onGround && (Object)(object)nma != null;
				}
				if (IsZombie())
				{
					flag2 = gc.onGround;
				}
				if (IsSpider())
				{
					flag2 = (bool)gc && gc.onGround && (Object)(object)nma != null;
				}
				if (flag2)
				{
					HandleLanding();
					return;
				}
			}
			float speedDiff = HandleFallingValues();
			if ((bool)man)
			{
				noFallDamage = man.inControl;
				if (fallTime > 0.2f && !man.inControl)
				{
					parryable = true;
				}
			}
			HandleFallingSound(speedDiff);
		}
		else if (fallTime > 0f)
		{
			fallTime = 0f;
			if ((bool)currentTerminalVelocityEffect)
			{
				Object.Destroy(currentTerminalVelocityEffect);
			}
		}
	}

	private void UpdateFallingState(bool hasGc, bool hasNma, bool hasRb, bool hasAnim)
	{
		if (!hasGc || !hasNma || !hasRb || !hasAnim || (IsMachine() && (limp || overrideFalling)))
		{
			return;
		}
		if (!falling && !gc.onGround && (!((Behaviour)(object)nma).enabled || !nma.isOnOffMeshLink))
		{
			StartFalling();
		}
		else if (IsStatue() && gc.onGround && falling)
		{
			bool flag = !InvincibleEnemies.Enabled && !eid.blessed && !noFallDamage;
			if (fallSpeed <= GetSplatVelocity() && flag)
			{
				eid.Splatter();
				return;
			}
			fallSpeed = 0f;
			nma.updatePosition = true;
			nma.updateRotation = true;
			rb.isKinematic = true;
			rb.SetGravityMode(useGravity: false);
			((Behaviour)(object)nma).enabled = true;
			nma.Warp(base.transform.position);
			falling = false;
			anim.SetBool("Falling", false);
		}
	}

	private void ResetFallingState()
	{
		fallSpeed = 0f;
		if ((Object)(object)aud != null && (Object)(object)aud.clip == (Object)(object)scream && aud.isPlaying)
		{
			aud.Stop();
		}
		if (IsZombie() && !Physics.CheckSphere(base.transform.position + Vector3.up * 1.5f, 0.1f, lmaskEnv, QueryTriggerInteraction.Ignore))
		{
			rb.isKinematic = true;
			rb.SetGravityMode(useGravity: false);
		}
		if (!IsMachine() && !IsSpider())
		{
			return;
		}
		if (sm == null || !sm.moveAtTarget)
		{
			rb.isKinematic = true;
		}
		rb.SetGravityMode(useGravity: false);
		if ((bool)man)
		{
			if (fallTime > 0.2f)
			{
				man.Landing();
			}
			else
			{
				man.inControl = true;
			}
			man.ResetMovementTarget();
		}
	}

	private float HandleFallingValues()
	{
		float num = rb.velocity.y - fallSpeed;
		if (fallTime > 0.05f)
		{
			if (num < 0f)
			{
				fallSpeed = rb.velocity.y;
			}
			if (num > 0f)
			{
				reduceFallTime = Mathf.MoveTowards(reduceFallTime, 0f, Time.deltaTime);
				if (reduceFallTime <= 0f)
				{
					fallSpeed = rb.velocity.y;
				}
			}
		}
		else
		{
			fallSpeed = 0f;
		}
		return num;
	}

	private void HandleFallingSound(float speedDiff)
	{
		if (!(Object)(object)aud)
		{
			return;
		}
		if (eid.underwater)
		{
			if ((Object)(object)aud.clip == (Object)(object)scream && aud.isPlaying)
			{
				aud.Stop();
			}
		}
		else if (fallTime > 0.05f && speedDiff < 0f)
		{
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(base.transform.position, Vector3.down, out hitInfo, 42f, lmaskEnv, QueryTriggerInteraction.Ignore);
			bool flag2 = flag && hitInfo.transform.gameObject.layer == 4;
			bool flag3 = false;
			int layerMask = (int)lmaskEnv | 1;
			if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo2, 42f, layerMask, QueryTriggerInteraction.Collide) && hitInfo2.transform.TryGetComponent<DeathZone>(out var _) && (!flag || hitInfo2.distance < hitInfo.distance))
			{
				flag3 = true;
				flag2 = false;
			}
			if ((flag3 || !(fallSpeed > GetSplatVelocity() || flag2)) && !aud.isPlaying && !limp)
			{
				aud.clip = scream;
				aud.volume = 1f;
				aud.priority = 78;
				aud.SetPitch(Random.Range(0.8f, 1.2f));
				aud.Play(tracked: true);
			}
		}
	}

	private void HandleLanding()
	{
		bool num = ShouldSplat();
		Debug.Log(num);
		if (num)
		{
			if (eid == null)
			{
				eid = GetComponent<EnemyIdentifier>();
			}
			eid.Splatter();
			return;
		}
		ResetFallingState();
		Vector3 position = base.transform.position;
		bool flag = true;
		if (IsZombie())
		{
			NavMeshHit val = default(NavMeshHit);
			flag = NavMesh.SamplePosition(base.transform.position, ref val, 4f, nma.areaMask);
			if (flag)
			{
				position = ((NavMeshHit)(ref val)).position;
			}
		}
		if (flag)
		{
			nma.updatePosition = true;
			nma.updateRotation = true;
			((Behaviour)(object)nma).enabled = true;
			nma.Warp(position);
		}
		falling = false;
		if (script != null)
		{
			script.OnLand();
		}
		anim.SetBool("Falling", false);
	}

	private bool ShouldSplat()
	{
		Debug.Log($"fallSpeed: {fallSpeed}, other thing: {gc.fallSuppressed && eid.unbounceable}");
		if (IsMachine())
		{
			if (fallSpeed <= -60f && !noFallDamage && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				if (gc.fallSuppressed)
				{
					return eid.unbounceable;
				}
				return true;
			}
			return false;
		}
		if (IsZombie())
		{
			if (gc.fallSuppressed && !eid.unbounceable)
			{
				return false;
			}
			if (fallSpeed <= -50f && !noFallDamage && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				return !gc.fallSuppressed;
			}
			return false;
		}
		if (IsStatue())
		{
			if (fallSpeed <= -50f && !InvincibleEnemies.Enabled)
			{
				return !eid.blessed;
			}
			return false;
		}
		if (IsSpider())
		{
			if (fallSpeed <= -60f && !noFallDamage && !InvincibleEnemies.Enabled && !eid.blessed)
			{
				if (gc.fallSuppressed)
				{
					return eid.unbounceable;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private void HandleMassDeath()
	{
		origPos = base.transform.position;
		transforms.AddRange(GetComponentsInChildren<Transform>());
		Debug.Log(transforms.Count);
		isMassDieing = true;
		Invoke("BloodExplosion", 3f);
	}

	public void BloodExplosion()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform transform in transforms)
		{
			if (transform != null && Random.Range(0f, 1f) < 0.33f)
			{
				GameObject gore = bsm.GetGore(GoreType.Head, eid);
				if ((bool)gore)
				{
					gore.transform.position = transform.position;
					if (gz != null && gz.goreZone != null)
					{
						gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					gore.GetComponent<Bloodsplatter>()?.GetReady();
				}
			}
			else if (transform == null)
			{
				list.Add(transform);
			}
		}
		if (list.Count > 0)
		{
			foreach (Transform item in list)
			{
				transforms.Remove(item);
			}
			list.Clear();
		}
		if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < 40; i++)
			{
				GameObject gib;
				if (i < 30)
				{
					gib = bsm.GetGib(BSType.gib);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(4f, 7f);
					}
					else
					{
						i = 30;
					}
					continue;
				}
				if (i < 35)
				{
					gib = bsm.GetGib(BSType.eyeball);
					if ((bool)gib)
					{
						if ((bool)gz && (bool)gz.gibZone)
						{
							ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
						}
						gib.transform.localScale *= Random.Range(3f, 6f);
					}
					else
					{
						i = 35;
					}
					continue;
				}
				gib = bsm.GetGib(BSType.brainChunk);
				if (!gib)
				{
					break;
				}
				if ((bool)gz && (bool)gz.gibZone)
				{
					ReadyGib(gib, transforms[Random.Range(0, transforms.Count)].gameObject);
				}
				gib.transform.localScale *= Random.Range(3f, 4f);
			}
		}
		base.enabled = false;
		isMassDieing = false;
		DeathEnd();
	}

	protected void DeathEnd()
	{
		if (!eid.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		if (musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		if (base.gameObject != null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetDestination(Vector3 position)
	{
		if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(position, ref val, 1f, nma.areaMask))
			{
				position = ((NavMeshHit)(ref val)).position;
			}
			if ((position - nma.destination).sqrMagnitude > 0.25f)
			{
				nma.SetDestination(position);
			}
		}
	}

	protected virtual void SetSpeed()
	{
		if (!limp)
		{
			if (difficulty < 0)
			{
				difficulty = InitializeDifficulty(eid);
			}
			if (difficulty >= 4)
			{
				speedMultiplier = 1.5f;
			}
			else if (difficulty == 3)
			{
				speedMultiplier = 1.25f;
			}
			else if (difficulty == 2)
			{
				speedMultiplier = 1f;
			}
			else if (difficulty == 1)
			{
				speedMultiplier = 0.75f;
			}
			else if (difficulty == 0)
			{
				speedMultiplier = 0.5f;
			}
			ConfigureSpeed();
			ApplySpeedModifiers();
		}
	}

	private void ConfigureSpeed()
	{
		EnemyMovementData speed = script.GetSpeed(difficulty);
		if (speed.speed < 0f)
		{
			speed.speed = startSpeed;
			speed.acceleration = startAcceleration;
			speed.angularSpeed = startAngularSpeed;
		}
		Debug.Log(speed.speed);
		if ((bool)(Object)(object)nma)
		{
			nma.acceleration = speed.acceleration;
			nma.angularSpeed = speed.angularSpeed;
			nma.speed = speed.speed;
		}
		if (eid.enemyType == EnemyType.Soldier)
		{
			float num = 1f;
			if (difficulty == 5)
			{
				num = 1.75f;
			}
			anim.SetFloat("RunSpeed", num * speedMultiplier);
		}
	}

	private void ApplySpeedModifiers()
	{
		if ((bool)(Object)(object)nma)
		{
			NavMeshAgent obj = nma;
			obj.acceleration *= eid.totalSpeedModifier;
			NavMeshAgent obj2 = nma;
			obj2.angularSpeed *= eid.totalSpeedModifier;
			NavMeshAgent obj3 = nma;
			obj3.speed *= eid.totalSpeedModifier;
			defaultSpeed = nma.speed;
		}
		if ((bool)(Object)(object)anim)
		{
			if (variableSpeed)
			{
				anim.speed = 1f * speedMultiplier;
			}
			else if (difficulty >= 2)
			{
				anim.speed = 1f * eid.totalSpeedModifier;
			}
			else if (difficulty == 1)
			{
				anim.speed = 0.875f * eid.totalSpeedModifier;
			}
			else if (difficulty == 0)
			{
				anim.speed = 0.75f * eid.totalSpeedModifier;
			}
		}
	}

	protected void UpdateBuff()
	{
		if (eid.enemyType != EnemyType.Puppet)
		{
			SetSpeed();
		}
	}

	public void Jump(Vector3 vector)
	{
		gc.ForceOff();
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		eid.useBrakes = false;
		rb.AddForce(vector, ForceMode.VelocityChange);
		Invoke("Jumped", 0.5f);
	}

	public void Jumped()
	{
		gc.StopForceOff();
	}

	private bool CanBeKnockedBack()
	{
		if (eid.poise)
		{
			return false;
		}
		if (IsStatue())
		{
			return false;
		}
		if (IsMachine() && tur != null && tur.lodged)
		{
			return false;
		}
		return true;
	}

	public void KnockBack(Vector3 force)
	{
		if (!CanBeKnockedBack())
		{
			return;
		}
		if ((Object)(object)nma != null)
		{
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
		}
		if (IsMachine() && man != null)
		{
			man.inControl = false;
			if (man.clinging)
			{
				man.Uncling();
			}
		}
		if ((bool)gc && !overrideFalling)
		{
			if (!knockedBack || (!gc.onGround && rb.velocity.y < 0f))
			{
				rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			}
			if (!gc.onGround)
			{
				rb.AddForce(Vector3.up, ForceMode.VelocityChange);
			}
		}
		if (IsMachine() && hitJiggleRoot != null)
		{
			Vector3 vector = new Vector3(force.x, 0f, force.z);
			hitJiggleRoot.localPosition = jiggleRootPosition + vector.normalized * -0.01f;
			if (Vector3.Distance(hitJiggleRoot.localPosition, jiggleRootPosition) > 0.1f)
			{
				hitJiggleRoot.localPosition = jiggleRootPosition + (hitJiggleRoot.localPosition - jiggleRootPosition).normalized * 0.1f;
			}
		}
		rb.AddForce(force / 10f, ForceMode.VelocityChange);
		knockedBack = true;
		knockBackCharge = Mathf.Min(knockBackCharge + force.magnitude / 1500f, 0.35f);
		brakes = 1f;
	}

	private void UpdateKnockback()
	{
		if (!CanFall() || !knockedBack)
		{
			return;
		}
		bool flag = knockBackCharge <= 0f && rb.velocity.magnitude < 1f && gc.onGround;
		if (v2 != null && knockBackCharge <= 0f)
		{
			flag = true;
		}
		if (flag)
		{
			StopKnockBack();
			return;
		}
		HandleBraking();
		if ((Object)(object)nma != null)
		{
			nma.updatePosition = false;
			nma.updateRotation = false;
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			rb.SetGravityMode(useGravity: true);
		}
	}

	public void StopKnockBack()
	{
		knockBackCharge = 0f;
		if (v2 != null)
		{
			knockedBack = false;
			juggleWeight = 0f;
		}
		else
		{
			if ((Object)(object)nma == null)
			{
				return;
			}
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out hitInfo, float.PositiveInfinity, lmask);
			if (IsMachine() || IsSpider())
			{
				flag = flag && gc.onGround;
			}
			NavMeshHit val = default(NavMeshHit);
			if (!flag)
			{
				knockBackCharge = 0.5f;
			}
			else if (NavMesh.SamplePosition(hitInfo.point, ref val, (float)(IsZombie() ? 8 : 4), nma.areaMask))
			{
				knockedBack = false;
				nma.updatePosition = true;
				nma.updateRotation = true;
				((Behaviour)(object)nma).enabled = true;
				if ((sm == null || !sm.moveAtTarget) && (man == null || !man.jumping))
				{
					rb.isKinematic = true;
				}
				if ((bool)man)
				{
					man.inControl = true;
				}
				juggleWeight = 0f;
				nma.Warp(((NavMeshHit)(ref val)).position);
			}
			else if (IsZombie() || IsSpider() || IsMachine())
			{
				if (gc.onGround)
				{
					rb.isKinematic = true;
					knockedBack = false;
					juggleWeight = 0f;
					eid.pulledByMagnet = false;
					if (IsMachine())
					{
						nma.updatePosition = true;
						nma.updateRotation = true;
						((Behaviour)(object)nma).enabled = true;
					}
				}
			}
			else
			{
				knockBackCharge = 0.5f;
			}
		}
	}

	private void HandleBraking()
	{
		if (gc.onGround)
		{
			if (knockBackCharge <= 0f)
			{
				brakes = Mathf.MoveTowards(brakes, 0f, 0.0005f * brakes);
			}
		}
		else if (!eid.useBrakes && !IsStatue())
		{
			brakes = 1f;
		}
		bool flag = eid.useBrakes || gc.onGround;
		if (IsStatue())
		{
			flag = true;
		}
		if (flag)
		{
			float num = rb.velocity.y - juggleWeight;
			if (IsStatue() && rb.velocity.y > 0f)
			{
				num *= brakes;
			}
			rb.velocity = new Vector3(rb.velocity.x * 0.95f * brakes, num, rb.velocity.z * 0.95f * brakes);
			if (IsMachine() || IsSpider())
			{
				juggleWeight += 0.00025f;
			}
		}
	}

	public virtual void GetHurt(GameObject target, Vector3 force, float multiplier, float critMultiplier, Vector3 hurtPos = default(Vector3), GameObject sourceWeapon = null, bool fromExplosion = false)
	{
		if (isMassDieing)
		{
			return;
		}
		if (IsZombie() && !gc.onGround && eid.hitter != "fire")
		{
			multiplier *= 1.5f;
		}
		DamageData data = new DamageData
		{
			hitTarget = target,
			hitter = eid.hitter,
			damage = multiplier,
			sourceWeapon = sourceWeapon,
			force = force,
			fromExplosion = fromExplosion
		};
		string hitLimb = "";
		bool killed = false;
		bool flag = false;
		float healthBeforeDamage = health;
		bool extraDamageZone = false;
		if (IsMachine())
		{
			HandleMachineSpecialCases(target, force, fromExplosion, ref data);
		}
		if (!limp && force != Vector3.zero && (script == null || script.ShouldKnockback(ref data)))
		{
			KnockBack(force / 100f);
			bool flag2 = eid.hitter == "heavypunch" || (eid.hitter == "cannonball" && (!gc || !gc.onGround));
			eid.useBrakes = !flag2;
		}
		if ((bool)script)
		{
			script.OnDamage(ref data);
		}
		if (!data.cancel)
		{
			HandleParrying(ref data);
		}
		target = data.hitTarget;
		eid.hitter = data.hitter;
		multiplier = data.damage;
		sourceWeapon = data.sourceWeapon;
		force = data.force;
		if (!data.cancel)
		{
			float limbMultiplier = CalculateLimbMultiplier(target);
			float num = CalculateDamage(multiplier, limbMultiplier, critMultiplier, target, ref extraDamageZone);
			if (IsZombie() && chestExploding && !limp && (bool)eid)
			{
				ChestExplodeEnd();
			}
			if ((bool)sisy && !limp && eid.hitter == "fire" && health > 0f && health - num < 0.01f && !eid.isGasolined)
			{
				num = health - 0.01f;
			}
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= num;
			}
			GameObject gameObject = null;
			gameObject = HandleBloodSelection(target, num, fromExplosion, extraDamageZone);
			if (!limp)
			{
				flag = true;
				hitLimb = DetermineLimbType(target);
			}
			if (health <= 0f)
			{
				HandleDeath(target, force, fromExplosion, num);
			}
			ProcessBloodEffects(gameObject, target, hurtPos);
			if (limp)
			{
				PortalUtils.AddForcePortalAware(target, force * (IsZombie() ? 10f : 1f), ForceMode.Force, searchParent: true);
			}
			if (IsStatue() && mass != null)
			{
				HandleMassSpear(target, num, fromExplosion);
			}
			PlayHurtSound();
			if (IsStatue())
			{
				ApplyWoundedEffect(healthBeforeDamage);
			}
			if (num == 0f || eid.puppet)
			{
				flag = false;
			}
			if (flag && eid.hitter != "enemy")
			{
				SendStyleInformation(hitLimb, killed, sourceWeapon);
			}
		}
	}

	private float CalculateDamage(float multiplier, float limbMultiplier, float critMultiplier, GameObject target, ref bool extraDamageZone)
	{
		float num = multiplier + multiplier * limbMultiplier * critMultiplier;
		if (IsStatue() && extraDamageZones.Count > 0 && extraDamageZones.Contains(target))
		{
			num *= extraDamageMultiplier;
			extraDamageZone = true;
		}
		return num;
	}

	private float CalculateLimbMultiplier(GameObject target)
	{
		if (target.gameObject.CompareTag("Head"))
		{
			return 1f;
		}
		if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			return 0.5f;
		}
		return 0f;
	}

	private string DetermineLimbType(GameObject target)
	{
		if (target.gameObject.CompareTag("Head"))
		{
			return "head";
		}
		if (target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			return "limb";
		}
		return "body";
	}

	private void PlayHurtSound()
	{
		if ((Object)(object)aud == null || hurtSounds.Length == 0)
		{
			return;
		}
		bool flag = false;
		if (IsMachine())
		{
			flag = (health > 0f || symbiotic) && !eid.blessed;
		}
		if (IsZombie())
		{
			flag = health > 0f && !limp && !eid.blessed && eid.hitter != "blocked";
		}
		if (IsStatue())
		{
			flag = health > 0f && !eid.blessed;
		}
		if (IsSpider())
		{
			flag = health > 0f && !eid.blessed;
		}
		if (flag)
		{
			aud.clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
			if ((bool)tur)
			{
				aud.volume = 0.85f;
			}
			else if ((bool)min)
			{
				aud.volume = 1f;
			}
			else if (IsStatue())
			{
				aud.volume = 0.75f;
			}
			else
			{
				aud.volume = 0.5f;
			}
			if (sm != null)
			{
				aud.SetPitch(Random.Range(0.85f, 1.35f));
			}
			else
			{
				aud.SetPitch(Random.Range(0.9f, 1.1f));
			}
			aud.priority = 12;
			aud.Play(tracked: true);
		}
	}

	public void ReadyGib(GameObject tempGib, GameObject target)
	{
		tempGib.transform.SetPositionAndRotation(target.transform.position, Random.rotation);
		GetGoreZone().SetGoreZone(tempGib);
		if (IsStatue())
		{
			tempGib.SetActive(bsm.goreOn);
		}
		else if (IsMachine() || IsSpider())
		{
			tempGib.SetActive(GraphicsSettings.bloodEnabled);
		}
	}

	private GameObject HandleBloodSelection(GameObject target, float damage, bool fromExplosion, bool extraDamageZone)
	{
		GameObject gameObject = null;
		if ((bool)sisy && damage > 0f)
		{
			if (eid.burners.Count > 0)
			{
				if (eid.hitter == "fire")
				{
					sisy.PlayHurtSound();
				}
				else if (damage > 0.5f)
				{
					gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
					sisy.PlayHurtSound(2);
				}
				else
				{
					gameObject = bsm.GetGore(GoreType.Limb, eid, fromExplosion);
					sisy.PlayHurtSound(1);
				}
			}
			else if (eid.hitter != "fire")
			{
				gameObject = bsm.GetGore(GoreType.Smallest, eid, fromExplosion);
			}
		}
		if (IsStatue() && extraDamageZone && (damage >= 1f || (eid.hitter == "shotgun" && Random.Range(0f, 1f) > 0.5f) || (eid.hitter == "nail" && Random.Range(0f, 1f) > 0.85f)))
		{
			gameObject = ((!(extraDamageMultiplier >= 2f)) ? bsm.GetGore(GoreType.Limb, eid, fromExplosion) : bsm.GetGore(GoreType.Head, eid, fromExplosion));
		}
		if (gameObject == null && eid.hitter != "fire" && damage > 0f)
		{
			bool flag = damage >= 1f || health <= 0f;
			bool num = (target.CompareTag("Head") && flag) || eid.hitter == "hammer" || eid.hitter == "heavypunch";
			bool flag2 = (eid.hitter == "explosion" && target.CompareTag("EndLimb")) || (flag && eid.hitter != "explosion");
			if (num)
			{
				gameObject = bsm.GetGore(GoreType.Head, eid, fromExplosion);
			}
			else if (flag2)
			{
				gameObject = bsm.GetGore((!target.CompareTag("Body")) ? GoreType.Limb : GoreType.Body, eid, fromExplosion);
			}
			else if (eid.hitter != "explosion")
			{
				gameObject = bsm.GetGore(GoreType.Small, eid, fromExplosion);
			}
		}
		return gameObject;
	}

	private void ProcessBloodEffects(GameObject blood, GameObject target, Vector3 hurtPos)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (!(blood != null))
		{
			return;
		}
		if (!gz)
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		Vector3 vector = Vector3.zero;
		if (IsMachine() || IsSpider())
		{
			if (thickLimbs && target.TryGetComponent<Collider>(out var component))
			{
				vector = component.ClosestPoint(MonoSingleton<NewMovement>.Instance.transform.position);
			}
		}
		else if (IsStatue() && hurtPos != default(Vector3) && hurtPos != Vector3.zero)
		{
			vector = hurtPos;
		}
		if (vector == Vector3.zero)
		{
			vector = target.transform.position;
		}
		blood.transform.position = vector;
		if (eid.hitter == "drill")
		{
			blood.transform.localScale *= 2f;
		}
		if (IsStatue() && bigBlood)
		{
			blood.transform.localScale *= 2f;
		}
		if (gz != null && gz.goreZone != null)
		{
			blood.transform.SetParent(gz.goreZone, worldPositionStays: true);
		}
		Bloodsplatter component2 = blood.GetComponent<Bloodsplatter>();
		if (!component2)
		{
			return;
		}
		CollisionModule collision = component2.GetComponent<ParticleSystem>().collision;
		if (eid.hitter == "shotgun" || eid.hitter == "shotgunzone" || eid.hitter == "explosion")
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				((CollisionModule)(ref collision)).enabled = false;
			}
			component2.hpAmount = 3;
		}
		else if (eid.hitter == "nail")
		{
			component2.hpAmount = 1;
			AudioSource component3 = component2.GetComponent<AudioSource>();
			component3.volume *= 0.8f;
		}
		if (!noheal)
		{
			component2.GetReady();
		}
	}

	private void ApplyWoundedEffect(float healthBeforeDamage)
	{
		if (!(healthBeforeDamage >= originalHealth / 2f) || !(health < originalHealth / 2f))
		{
			return;
		}
		if ((bool)woundedParticle)
		{
			Object.Instantiate(woundedParticle, chest.transform.position, Quaternion.identity);
		}
		if (eid.puppet)
		{
			return;
		}
		if ((bool)woundedModel)
		{
			woundedModel.SetActive(value: true);
			smr.gameObject.SetActive(value: false);
		}
		else if ((bool)woundedMaterial)
		{
			smr.material = woundedMaterial;
			if (smr.TryGetComponent<EnemySimplifier>(out var component))
			{
				component.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
				component.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
			}
		}
	}

	private void DismemberLimb(GameObject target)
	{
		Transform child = target.transform.GetChild(0);
		CharacterJoint[] componentsInChildren = target.GetComponentsInChildren<CharacterJoint>();
		if (componentsInChildren.Length != 0)
		{
			CharacterJoint[] array = componentsInChildren;
			foreach (CharacterJoint characterJoint in array)
			{
				if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
				{
					component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
				}
				Object.Destroy(characterJoint);
			}
		}
		CharacterJoint component2 = target.GetComponent<CharacterJoint>();
		if (component2 != null)
		{
			component2.connectedBody = null;
			Object.Destroy(component2);
		}
		target.transform.position = child.position;
		target.transform.SetParent(child);
		child.SetParent(gz.gibZone);
		Object.Destroy(target.GetComponent<Rigidbody>());
	}

	private void HandleDismemberment(GameObject target, float damage, bool fromExplosion)
	{
		if (target.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			if (eid.hitter == "sawblade")
			{
				if (target.transform.position.y > chest.transform.position.y - 1f)
				{
					if (IsZombie() && !chestExploded)
					{
						ChestExplosion(cut: true);
					}
					component.Detach(gz.transform);
					component.GoLimp();
					Rigidbody[] componentsInChildren = component.GetComponentsInChildren<Rigidbody>();
					foreach (Rigidbody obj in componentsInChildren)
					{
						obj.isKinematic = false;
						obj.SetGravityMode(useGravity: true);
						obj.angularDrag = 0.001f;
						obj.maxAngularVelocity = float.PositiveInfinity;
						obj.velocity = Vector3.zero;
						obj.AddForce(Vector3.up * (target.CompareTag("Head") ? 250 : 25), ForceMode.VelocityChange);
						obj.AddTorque(target.transform.right * 1f, ForceMode.VelocityChange);
					}
				}
			}
			else if (target == chest && v2 == null && sc == null && IsZombie())
			{
				HandleChestExplosionDismemberment(component, damage, fromExplosion);
			}
			switch (target.tag)
			{
			case "Limb":
			case "EndLimb":
			case "Head":
				component.DetachChildren(gz.gibZone, recursive: false);
				component.Break();
				break;
			}
		}
		if (IsZombie() && eid.hitter == "sawblade")
		{
			Cut(target);
		}
		else if (target.gameObject.CompareTag("Limb") && IsZombie())
		{
			if (target.transform.childCount > 0)
			{
				Transform child = target.transform.GetChild(0);
				CharacterJoint[] componentsInChildren2 = target.GetComponentsInChildren<CharacterJoint>();
				if (componentsInChildren2.Length != 0)
				{
					CharacterJoint[] array = componentsInChildren2;
					foreach (CharacterJoint characterJoint in array)
					{
						if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
						{
							component2.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
						}
						characterJoint.transform.SetParent(gz.transform);
						Object.Destroy(characterJoint);
					}
				}
				CharacterJoint component3 = target.GetComponent<CharacterJoint>();
				if (component3 != null)
				{
					component3.connectedBody = null;
					Object.Destroy(component3);
				}
				target.transform.position = child.position;
				target.transform.SetParent(child);
				child.SetParent(gz.transform, worldPositionStays: true);
				Object.Destroy(target.GetComponent<Rigidbody>());
			}
			if (target.TryGetComponent<Collider>(out var component4))
			{
				Object.Destroy(component4);
			}
			target.transform.localScale = Vector3.zero;
			target.gameObject.SetActive(value: false);
		}
		if (IsZombie() && (target.gameObject.CompareTag("EndLimb") || target.gameObject.CompareTag("Head")))
		{
			target.transform.localScale = Vector3.zero;
			target.gameObject.SetActive(value: false);
		}
		if ((IsMachine() || IsSpider()) && !target.gameObject.CompareTag("Body"))
		{
			if (target.TryGetComponent<Collider>(out var component5))
			{
				Object.Destroy(component5);
			}
			target.transform.localScale = Vector3.zero;
		}
	}

	private void HandleChestExplosionDismemberment(EnemyIdentifierIdentifier eii, float damage, bool fromExplosion)
	{
		string hitter = eid.hitter;
		if (hitter == "shotgunzone" || hitter == "hammmerzone")
		{
			chestHP = 0f;
		}
		else
		{
			chestHP -= damage;
		}
		if (!(chestHP <= 0f))
		{
			return;
		}
		if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
		{
			for (int i = 0; i < 2; i++)
			{
				GameObject gib = bsm.GetGib(BSType.gib);
				if ((bool)gib && (bool)gz && (bool)gz.gibZone)
				{
					ReadyGib(gib, chest);
				}
			}
		}
		if (IsZombie() && !eid.sandified)
		{
			GameObject fromQueue = bsm.GetFromQueue(BSType.chestExplosion);
			gz.SetGoreZone(fromQueue);
			fromQueue.transform.SetPositionAndRotation(chest.transform.parent.position, chest.transform.parent.rotation);
			fromQueue.transform.SetParent(chest.transform.parent, worldPositionStays: true);
		}
		EnemyIdentifierIdentifier[] componentsInChildren = eii.GetComponentsInChildren<EnemyIdentifierIdentifier>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			GoreType goreType;
			switch (componentsInChildren[j].tag)
			{
			case "Head":
				goreType = GoreType.Head;
				break;
			case "Limb":
			case "EndLimb":
				goreType = GoreType.Limb;
				break;
			default:
				goreType = GoreType.Body;
				break;
			}
			GoreType got = goreType;
			GameObject gore = bsm.GetGore(got, eid, fromExplosion);
			gore.transform.position = chest.transform.position;
			gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
			if (!noheal && gore.TryGetComponent<Bloodsplatter>(out var component))
			{
				component.GetReady();
			}
		}
		eii.DetachChildren(gz.gibZone, recursive: false);
		if (!limp)
		{
			if ((bool)(Object)(object)anim)
			{
				anim.Rebind();
				anim.SetTrigger("ChestExplosion");
			}
			chestExploding = true;
		}
		eii.Break(reparentToChild: false);
		for (int k = 0; k < 6; k++)
		{
			GameObject gib2 = bsm.GetGib((k < 2) ? BSType.jawChunk : BSType.gib);
			ReadyGib(gib2, chest);
		}
	}

	public void GoLimp()
	{
		GoLimp(fromExplosion: false);
	}

	public void GoLimp(bool fromExplosion)
	{
		if (limp)
		{
			return;
		}
		gz = GetGoreZone();
		anw = GetComponentInParent<ActivateNextWave>();
		onDeath?.Invoke();
		if (health > 0f)
		{
			health = 0f;
		}
		if (smr != null)
		{
			smr.updateWhenOffscreen = true;
		}
		if (mf == null)
		{
			Invoke("StopHealing", 1f);
		}
		if (script != null)
		{
			script.OnGoLimp(fromExplosion);
		}
		if (limp)
		{
			return;
		}
		if (specialDeath && TryGetComponent<LeviathanController>(out var component))
		{
			component.SpecialDeath();
		}
		HideDestroyOnDeathObjects();
		SimplifyEnemyModels();
		ApplyDeadMaterial();
		if ((Object)(object)nma != null)
		{
			Object.Destroy((Object)(object)nma);
		}
		NotifyDeathToComponents();
		deathTokenSource.Cancel();
		if (simpleDeath && eid.hitter != "spin")
		{
			CreateDeathExplosion();
			return;
		}
		if (isMassDeath && !isMassDieing)
		{
			HandleMassDeath();
		}
		if (!specialDeath && !isMassDieing && (!IsMachine() || eid.hitter != "spin"))
		{
			HandleStandardDeath();
		}
		PlayDeathSound();
		parryable = false;
		partiallyParryable = false;
		limp = true;
	}

	private void HideDestroyOnDeathObjects()
	{
		GameObject[] array = destroyOnDeath;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeInHierarchy)
			{
				Transform transform = gameObject.GetComponentInParent<Rigidbody>().transform;
				if (transform != null)
				{
					gameObject.transform.SetParent(transform);
					gameObject.transform.position = transform.position;
					gameObject.transform.localScale = Vector3.zero;
				}
			}
		}
	}

	public void CountDeath()
	{
		if (!dontDie && !eid.dontCountAsKills)
		{
			if (!anw)
			{
				anw = GetComponentInParent<ActivateNextWave>();
			}
			MonoSingleton<StatsManager>.Instance.kills++;
			if ((bool)gz)
			{
				gz.AddDeath();
				gz.EnemyDeath(eid);
			}
			if (!isMassDieing && anw != null)
			{
				anw.AddDeadEnemy();
			}
		}
	}

	private void SimplifyEnemyModels()
	{
		EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Begone();
		}
	}

	private void ApplyDeadMaterial()
	{
		if (smr != null)
		{
			if (deadMaterial != null)
			{
				smr.sharedMaterial = deadMaterial;
			}
			else if (woundedMaterial != null)
			{
				smr.sharedMaterial = woundedMaterial;
			}
			else if (originalMaterial != null)
			{
				smr.sharedMaterial = originalMaterial;
			}
		}
	}

	private void NotifyDeathToComponents()
	{
		if (IsMachine())
		{
			SendMessage("Death", SendMessageOptions.DontRequireReceiver);
		}
		if (specialDeath && IsStatue())
		{
			SendMessage("SpecialDeath", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void CreateDeathExplosion()
	{
		Explosion[] componentsInChildren = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.explosion, base.transform.position, base.transform.rotation).GetComponentsInChildren<Explosion>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].canHit = AffectedSubjects.EnemiesOnly;
		}
		Object.Destroy(base.gameObject);
	}

	private void HandleDeath(GameObject target, Vector3 force, bool fromExplosion, float damage)
	{
		if ((bool)currentTerminalVelocityEffect)
		{
			Object.Destroy(currentTerminalVelocityEffect);
		}
		if (symbiotic)
		{
			if (symbiote != null && symbiote.health > 0f)
			{
				HandleSymbioteDeath(fromExplosion);
				return;
			}
			symbiotic = false;
		}
		deathTokenSource?.Cancel();
		if (IsSpider())
		{
			if (!limp)
			{
				GoLimp(fromExplosion);
			}
			return;
		}
		if (MonoSingleton<BloodsplatterManager>.Instance.goreOn && !target.gameObject.CompareTag("EndLimb"))
		{
			SpawnGibsOnDeath(target, fromExplosion);
		}
		if (dismemberment)
		{
			HandleDismemberment(target, damage, fromExplosion);
		}
		if (!limp)
		{
			GoLimp(fromExplosion);
		}
	}

	private void HandleStandardDeath()
	{
		if (v2 == null)
		{
			if (!chestExploding)
			{
				Object.Destroy((Object)(object)anim);
			}
			Object.Destroy(GetComponent<Collider>());
			if (rb == null)
			{
				rb = GetComponent<Rigidbody>();
			}
			Object.Destroy(rb);
		}
		if (v2 == null && mf == null && !chestExploding)
		{
			Rigidbody[] array = rbs;
			foreach (Rigidbody rigidbody in array)
			{
				if ((bool)rigidbody)
				{
					rigidbody.isKinematic = false;
					rigidbody.SetGravityMode(useGravity: true);
					if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
					{
						component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
					}
					if (IsStatue())
					{
						gz.SetGoreZone(rigidbody.gameObject);
						rigidbody.AddForce(Random.onUnitSphere * 2.5f, ForceMode.VelocityChange);
					}
					else if (man != null)
					{
						rigidbody.AddForce((rigidbody.position - eid.overrideCenter.transform.position).normalized * Random.Range(20f, 30f), ForceMode.VelocityChange);
						rigidbody.AddTorque(Random.onUnitSphere * 360f, ForceMode.VelocityChange);
						Object.Instantiate(man.bloodSpray, rigidbody.transform.position, Quaternion.LookRotation(rigidbody.transform.parent.position - rigidbody.transform.position)).transform.SetParent(rigidbody.transform, worldPositionStays: true);
						rigidbody.transform.SetParent(gz.goreZone, worldPositionStays: true);
					}
					if ((bool)MonoSingleton<ComponentsDatabase>.Instance && MonoSingleton<ComponentsDatabase>.Instance.scrollers.Count > 0)
					{
						CheckForScroller checkForScroller = rigidbody.gameObject.AddComponent<CheckForScroller>();
						checkForScroller.checkOnStart = false;
						checkForScroller.checkOnCollision = true;
						checkForScroller.asRigidbody = true;
					}
				}
			}
		}
		if (IsZombie() && gz != null)
		{
			gz.SetGoreZone(base.gameObject);
		}
		if (musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		if (specialDeath && TryGetComponent<MinosPrime>(out var component2))
		{
			component2.Death();
		}
	}

	private void HandleSymbioteDeath(bool fromExplosion)
	{
		if (symbiote.health > 0f)
		{
			if (sm != null && !sm.downed)
			{
				sm.downed = true;
				sm.Down(fromExplosion);
				Invoke("StartHealing", 3f);
			}
			else if (sisy != null && !sisy.downed)
			{
				sisy.downed = true;
				sisy.Knockdown(base.transform.position + base.transform.forward);
				Invoke("StartHealing", 3f);
			}
			else if (fm != null && !fm.downed)
			{
				fm.downed = true;
				fm.Knockdown();
				Invoke("StartHealing", 3f);
			}
		}
		else if (symbiote.health <= 0f)
		{
			symbiotic = false;
			if (!limp)
			{
				GoLimp(fromExplosion);
			}
		}
	}

	private void SpawnGibsOnDeath(GameObject target, bool fromExplosion)
	{
		float num = eid.hitter switch
		{
			"shotgun" => 0.5f, 
			"shotgunzone" => 0.5f, 
			"explosion" => IsMachine() ? 0.5f : 0.25f, 
			_ => 1f, 
		};
		string text = target.gameObject.tag;
		if (!(text == "Head"))
		{
			if (!(text == "Limb"))
			{
				return;
			}
			for (int i = 0; (float)i < 4f * num; i++)
			{
				GameObject gib = bsm.GetGib(BSType.gib);
				if ((bool)gib && (bool)gz && (bool)gz.gibZone)
				{
					ReadyGib(gib, target);
				}
			}
			if ((IsMachine() || IsSpider()) && target.transform.childCount > 0 && dismemberment)
			{
				DismemberLimb(target);
			}
			return;
		}
		for (int j = 0; (float)j < 6f * num; j++)
		{
			GameObject gib = bsm.GetGib(BSType.skullChunk);
			if ((bool)gib && (bool)gz && (bool)gz.gibZone)
			{
				ReadyGib(gib, target);
			}
		}
		for (int k = 0; (float)k < 4f * num; k++)
		{
			GameObject gib = bsm.GetGib(BSType.brainChunk);
			if ((bool)gib && (bool)gz && (bool)gz.gibZone)
			{
				ReadyGib(gib, target);
			}
		}
		for (int l = 0; (float)l < 2f * num; l++)
		{
			GameObject gib = bsm.GetGib(BSType.eyeball);
			if ((bool)gib && (bool)gz && (bool)gz.gibZone)
			{
				ReadyGib(gib, target);
			}
			gib = bsm.GetGib(BSType.jawChunk);
			if ((bool)gib && (bool)gz && (bool)gz.gibZone)
			{
				ReadyGib(gib, target);
			}
		}
	}

	private void PlayDeathSound()
	{
		if (!((Object)(object)deathSound != null) || !((Object)(object)aud != null))
		{
			return;
		}
		aud.clip = deathSound;
		if (IsStatue() || tur != null)
		{
			aud.volume = 1f;
		}
		else if (IsZombie())
		{
			if (eid.hitter != "fire")
			{
				aud.volume = deathSoundVol;
			}
			else
			{
				aud.volume = 0.5f;
			}
		}
		aud.SetPitch(Random.Range(0.85f, 1.35f));
		aud.priority = 11;
		aud.Play(tracked: true);
	}

	protected void StartHealing()
	{
		if (symbiotic && symbiote != null)
		{
			healing = true;
		}
	}

	protected void StopHealing()
	{
		noheal = true;
	}

	private void SendStyleInformation(string hitLimb, bool killed, GameObject sourceWeapon)
	{
		if (scalc == null)
		{
			scalc = MonoSingleton<StyleCalculator>.Instance;
		}
		if (health <= 0f && !symbiotic && (v2 == null || !v2.dontDie) && (!eid.flying || (bool)mf))
		{
			killed = true;
			if ((bool)gc && !gc.onGround)
			{
				if (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon")
				{
					scalc.shud.AddPoints(120, "ultrakill.fireworks", sourceWeapon, eid);
				}
				else if (eid.hitter == "ground slam")
				{
					scalc.shud.AddPoints(160, "ultrakill.airslam", sourceWeapon, eid);
				}
				else if (eid.hitter != "deathzone" && eid.hitter != "secret" && eid.hitter != "terminalvelocity")
				{
					scalc.shud.AddPoints(50, "ultrakill.airshot", sourceWeapon, eid);
				}
			}
		}
		else if (health > 0f && (bool)gc && !gc.onGround && (eid.hitter == "explosion" || eid.hitter == "ffexplosion" || eid.hitter == "railcannon"))
		{
			scalc.shud.AddPoints(20, "ultrakill.fireworksweak", sourceWeapon, eid);
		}
		if (IsZombie() && killed && eid.hitter != "fire")
		{
			Flammable componentInChildren = GetComponentInChildren<Flammable>();
			if ((bool)componentInChildren && componentInChildren.burning)
			{
				scalc.shud.AddPoints(50, "ultrakill.finishedoff", sourceWeapon, eid);
			}
		}
		if (eid.hitter != "secret" && eid.hitter != "terminalvelocity")
		{
			string enemyType = DetermineEnemyTypeForStyleCalculation();
			scalc.HitCalculator(eid.hitter, enemyType, hitLimb, killed, eid, sourceWeapon);
		}
	}

	private string DetermineEnemyTypeForStyleCalculation()
	{
		if (IsZombie())
		{
			return "zombie";
		}
		if (IsMachine())
		{
			if (!bigKill)
			{
				return "machine";
			}
			return "spider";
		}
		if (IsStatue() || IsSpider())
		{
			return "spider";
		}
		return "machine";
	}

	private void HandleParrying(ref DamageData data)
	{
		GameObject hitTarget = data.hitTarget;
		bool flag = false;
		if (eid.hitter == "punch")
		{
			if (IsZombie())
			{
				if (parryable)
				{
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						health -= ((parryFramesLeft > 0) ? 4 : 5);
					}
					parryable = false;
					MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
					flag = true;
				}
				else
				{
					parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
				}
			}
			else if (IsSpider())
			{
				script.OnParry(ref data, isShotgun: false);
				flag = true;
			}
			else
			{
				bool flag2 = false;
				flag2 = (parryables != null && parryables.Count > 0 && parryables.Contains(hitTarget.transform)) || (parryFramesLeft > 0 && parryFramesOnPartial);
				if (parryable || (partiallyParryable && flag2))
				{
					parryable = false;
					partiallyParryable = false;
					parryables.Clear();
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						health -= ((parryFramesLeft > 0) ? 4 : 5);
					}
					script.OnParry(ref data, isShotgun: false);
					MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
					flag = true;
					if (IsMachine() && sm != null && health > 0f)
					{
						if (!sm.enraged)
						{
							sm.Knockdown(data.fromExplosion);
						}
						else
						{
							sm.Enrage();
						}
					}
					else
					{
						SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
					parryFramesOnPartial = flag2;
				}
			}
		}
		if (IsMachine() || IsStatue())
		{
			if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
			{
				bool flag3 = hitTarget == chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f;
				bool flag4 = health - data.damage <= 0f;
				bool flag5 = partiallyParryable && parryables != null && parryables.Contains(hitTarget.transform);
				if (parryable || (!flag3 && flag4))
				{
					if (flag3 || flag5)
					{
						data.damage *= 1.5f;
						parryable = false;
						partiallyParryable = false;
						parryables.Clear();
						script.OnParry(ref data, isShotgun: true);
						MonoSingleton<NewMovement>.Instance.Parry(eid);
						flag = true;
						if (IsMachine() && sm != null && health - data.damage > 0f)
						{
							if (!sm.enraged)
							{
								sm.Knockdown(data.fromExplosion);
							}
							else
							{
								sm.Enrage();
							}
						}
						else
						{
							SendMessage("GotParried", SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				else if (flag3 || !flag4)
				{
					data.damage = 0f;
				}
			}
		}
		else if (IsZombie())
		{
			if (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone")
			{
				if (!parryable && (hitTarget != chest || health - data.damage > 0f))
				{
					data.damage = 0f;
				}
				else if (parryable && (hitTarget == chest || MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 18f))
				{
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						data.damage *= 2f;
					}
					MonoSingleton<NewMovement>.Instance.Parry(eid);
					flag = true;
				}
			}
		}
		else if (IsSpider() && (eid.hitter == "shotgunzone" || eid.hitter == "hammerzone"))
		{
			script.OnParry(ref data, isShotgun: true);
			flag = true;
		}
		if (flag && (bool)parryChallenge)
		{
			parryChallenge.Done();
		}
	}

	public virtual void ParryableCheck(bool partial = false)
	{
		if (IsZombie())
		{
			parryable = true;
			if (parryFramesLeft > 0)
			{
				eid.hitter = "punch";
				eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
				parryFramesLeft = 0;
			}
			return;
		}
		if (partial)
		{
			partiallyParryable = true;
		}
		else
		{
			parryable = true;
		}
		if (parryFramesLeft > 0 && (!partial || parryFramesOnPartial))
		{
			eid.hitter = "punch";
			eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
			parryFramesLeft = 0;
		}
	}

	private void HandleMachineSpecialCases(GameObject target, Vector3 force, bool fromExplosion, ref DamageData data)
	{
		if (tur != null && tur.aiming && (eid.hitter == "revolver" || eid.hitter == "coin") && tur.interruptables.Contains(target.transform))
		{
			tur.Interrupt();
		}
		else if (v2 != null && v2.secondEncounter && eid.hitter == "heavypunch")
		{
			v2.InstaEnrage();
		}
		else if (eid.enemyType == EnemyType.Minotaur)
		{
			Minotaur component = GetComponent<Minotaur>();
			if (component != null && component.ramTimer > 0f && eid.hitter == "ground slam")
			{
				component.GotSlammed();
			}
		}
	}

	private void UpdateMachineHealing()
	{
		if (!healing || limp || !symbiote)
		{
			return;
		}
		health = Mathf.MoveTowards(health, symbiote.health, Time.deltaTime * 10f);
		eid.health = health;
		if (health >= symbiote.health)
		{
			healing = false;
			if ((bool)sm)
			{
				sm.downed = false;
			}
			if ((bool)sisy)
			{
				sisy.downed = false;
			}
			if ((bool)fm)
			{
				fm.downed = false;
			}
		}
	}

	private void UpdateJiggleBone()
	{
		if (hitJiggleRoot != null)
		{
			Vector3 localPosition = hitJiggleRoot.localPosition;
			Vector3 vector = jiggleRootPosition;
			if (localPosition != vector)
			{
				hitJiggleRoot.localPosition = Vector3.MoveTowards(localPosition, vector, (Vector3.Distance(localPosition, vector) + 1f) * 100f * Time.fixedDeltaTime);
			}
		}
	}

	public virtual void Cut(GameObject target)
	{
		if (IsZombie() && target.TryGetComponent<CharacterJoint>(out var component))
		{
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && component.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
			{
				component2.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
			}
			Object.Destroy(component);
			target.transform.SetParent(gz.transform, worldPositionStays: true);
			Rigidbody[] componentsInChildren = target.transform.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody obj in componentsInChildren)
			{
				obj.isKinematic = false;
				obj.SetGravityMode(useGravity: true);
				obj.angularDrag = 0.001f;
				obj.maxAngularVelocity = float.PositiveInfinity;
				obj.velocity = Vector3.zero;
				obj.AddForce(Vector3.up * (target.CompareTag("Head") ? 250 : 25), ForceMode.VelocityChange);
				obj.AddTorque(target.transform.right * 1f, ForceMode.VelocityChange);
			}
		}
	}

	public virtual void ChestExplosion(bool cut = false, bool fromExplosion = false)
	{
		if (!IsZombie() || chestExploded)
		{
			return;
		}
		GetGoreZone();
		if (!cut)
		{
			chest.GetComponent<EnemyIdentifierIdentifier>().Break();
			if (chest.TryGetComponent<Rigidbody>(out var component))
			{
				Object.Destroy(component);
			}
			if (!limp && !eid.exploded && !eid.dead)
			{
				if (gc.onGround)
				{
					rb.isKinematic = true;
					knockedBack = false;
				}
				anim.Rebind();
				anim.SetTrigger("ChestExplosion");
				chestExploding = true;
			}
		}
		GetGoreZone();
		if (MonoSingleton<BloodsplatterManager>.Instance.forceOn || MonoSingleton<BloodsplatterManager>.Instance.forceGibs || MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			GetGoreZone();
			for (int i = 0; i < 6; i++)
			{
				GameObject gib = bsm.GetGib((i < 2) ? BSType.jawChunk : BSType.gib);
				ReadyGib(gib, chest);
			}
			if (!eid.sandified)
			{
				GameObject fromQueue = bsm.GetFromQueue(BSType.chestExplosion);
				gz.SetGoreZone(fromQueue);
				fromQueue.transform.SetPositionAndRotation(chest.transform.parent.position, chest.transform.parent.rotation);
				fromQueue.transform.SetParent(chest.transform.parent, worldPositionStays: true);
			}
		}
		EnemyIdentifierIdentifier[] componentsInChildren = chest.GetComponentsInChildren<EnemyIdentifierIdentifier>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (!componentsInChildren[j])
			{
				continue;
			}
			GoreType got;
			switch (componentsInChildren[j].gameObject.tag)
			{
			case "Head":
				got = GoreType.Head;
				break;
			case "EndLimb":
			case "Limb":
				got = GoreType.Limb;
				break;
			default:
				got = GoreType.Body;
				break;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(got, eid, fromExplosion);
			if ((bool)gore)
			{
				gore.transform.position = chest.transform.position;
				Bloodsplatter component2 = gore.GetComponent<Bloodsplatter>();
				if ((bool)component2)
				{
					component2.hpAmount = 10;
				}
				if (gz != null && gz.goreZone != null)
				{
					gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				if (!noheal && (bool)component2)
				{
					component2.GetReady();
				}
			}
		}
		if (!cut)
		{
			chest.transform.localScale = Vector3.zero;
		}
		else
		{
			if (!limp)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.halfoff", null, eid);
			}
			Cut(chest);
		}
		chestExploded = true;
	}

	public virtual void ChestExplodeEnd()
	{
		if (!IsZombie())
		{
			return;
		}
		((Behaviour)(object)anim).enabled = false;
		anim.StopPlayback();
		Object.Destroy((Object)(object)anim);
		rbs = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = rbs;
		foreach (Rigidbody rigidbody in array)
		{
			if (rigidbody != null)
			{
				rigidbody.isKinematic = false;
				rigidbody.SetGravityMode(useGravity: true);
				if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && rigidbody.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
				{
					component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
				}
			}
		}
		chestExploding = false;
	}

	private void HandleMassSpear(GameObject target, float damage, bool fromExplosion)
	{
		if (mass.spearShot && (bool)mass.tempSpear && mass.tailHitboxes.Contains(target))
		{
			MassSpear component = mass.tempSpear.GetComponent<MassSpear>();
			if (component != null && component.hitPlayer)
			{
				if (damage >= 1f || component.spearHealth - damage <= 0f)
				{
					GameObject gore = bsm.GetGore(GoreType.Head, eid, fromExplosion);
					ReadyGib(gore, mass.tailEnd.GetChild(0).gameObject);
				}
				component.spearHealth -= damage;
			}
		}
		else if (mass.spearShot && !mass.tempSpear)
		{
			mass.spearShot = false;
		}
	}

	public static int InitializeDifficulty(EnemyIdentifier eid)
	{
		if (eid != null && eid.difficultyOverride >= 0)
		{
			return eid.difficultyOverride;
		}
		return MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}

	private void SetupLayerMasks()
	{
		lmaskEnv = LayerMaskDefaults.Get(LMD.Environment);
		if (IsMachine() || IsZombie() || IsSpider() || IsStatue())
		{
			lmask = lmaskEnv;
		}
		lmaskWater = lmask;
		lmaskWater = (int)lmaskWater | 0x10;
	}

	public GoreZone GetGoreZone()
	{
		if (gz != null)
		{
			return gz;
		}
		if (IsStatue() && base.transform.parent != null)
		{
			gz = GoreZone.ResolveGoreZone(base.transform.parent);
		}
		else
		{
			gz = GoreZone.ResolveGoreZone(base.transform);
		}
		return gz;
	}

	public bool IsLedgeSafe()
	{
		RaycastHit hitInfo;
		return Physics.Raycast(base.transform.position + Vector3.up + base.transform.forward, Vector3.down, out hitInfo, (eid.target != null) ? Mathf.Max(22f, base.transform.position.y - eid.target.position.y + 2.5f) : 22f, lmaskEnv, QueryTriggerInteraction.Ignore);
	}

	public void SetData(ref TargetData data)
	{
		data.position = cachedPos;
		data.realPosition = cachedPos;
		data.headPosition = cachedHeadPos;
		data.realHeadPosition = cachedHeadPos;
		data.rotation = cachedRot;
		data.velocity = cachedVel;
	}

	public void UpdateCachedTransformData()
	{
		if (eid.enemyType == EnemyType.FleshPanopticon || eid.enemyType == EnemyType.FleshPrison)
		{
			cachedPos = chest.transform.position;
			cachedRot = base.transform.rotation;
			cachedHeadPos = chest.transform.position;
			return;
		}
		bool flag = rb;
		cachedPos = eid.bodyTransform.position;
		cachedRot = (flag ? rb.rotation : base.transform.rotation);
		cachedVel = (flag ? rb.velocity : Vector3.zero);
		if ((bool)eid.weakPoint && eid.weakPoint.activeInHierarchy)
		{
			cachedHeadPos = eid.weakPoint.transform.position;
		}
		else
		{
			cachedHeadPos = cachedPos;
		}
	}

	public bool? OnTravel(PortalTravelDetails details)
	{
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Expected O, but got Unknown
		if (eid.hooked)
		{
			return null;
		}
		PortalHandle enterHandle = details.enterHandle;
		PortalHandle exitHandle = details.exitHandle;
		Portal portalObject = portalManager.Scene.GetPortalObject(enterHandle);
		Portal exitPortal = portalManager.Scene.GetPortalObject(exitHandle);
		if (portalLinkData.hasCrossed)
		{
			if (enterHandle == portalLinkData.portalHandle.Reverse())
			{
				return null;
			}
			if (portalObject.isMultiPanel)
			{
				return null;
			}
		}
		Matrix4x4 enterToExit = details.enterToExit;
		Vector3 vector = enterToExit.MultiplyVector(base.transform.forward).normalized;
		Vector3 normalized = enterToExit.MultiplyVector(base.transform.up).normalized;
		Quaternion quaternion = Quaternion.LookRotation(vector, normalized);
		portalObject.GetTransform(enterHandle.side);
		NativePortalTransform exitTrans = exitPortal.GetTransform(exitHandle.side);
		Vector3 tpNavFwd = vector;
		bool flag = false;
		bool flag2 = false;
		Vector3 vector2;
		if (IsDrone() || (IsSpider() && details.isIntersectTraversal))
		{
			vector2 = enterToExit.MultiplyPoint3x4(base.transform.position);
			Vector3 vector3 = enterToExit.MultiplyPoint3x4(details.intersection);
			Vector3 normalized2 = (vector2 - vector3).normalized;
			float num = 0.1f;
			if (TryGetComponent<Collider>(out var component))
			{
				num = component.bounds.extents.x;
			}
			Vector3 end = vector2 + normalized2 * num;
			if (Physics.Linecast(vector3, end, out var hitInfo, lmaskEnv))
			{
				float num2 = hitInfo.distance - num;
				if (!(num2 > 0f))
				{
					return false;
				}
				vector2 = vector3 + normalized2 * num2;
			}
			if (IsSpider())
			{
				AbortLinkTraversal();
			}
		}
		else
		{
			Vector3 position = base.transform.position;
			Vector3 vector4 = travellerPosition;
			Vector3 vector5 = position - vector4;
			Vector3 vector6 = enterToExit.MultiplyPoint3x4(position);
			vector2 = vector6;
			if (exitTrans.IsFloor())
			{
				tpNavFwd = (vector6 - exitTrans.backManaged).normalized;
			}
			else
			{
				tpNavFwd = exitTrans.backManaged;
				if (exitTrans.IsTilted())
				{
					tpNavFwd = new Vector3(tpNavFwd.x, 0f, tpNavFwd.z).normalized;
				}
			}
			if (IsSpider() && details.isLinkTraversal)
			{
				vector2 = enterToExit.MultiplyPoint3x4(base.transform.position);
				Vector3 vector7;
				if (!details.calculateLinkEndPos)
				{
					vector7 = portalLinkData.linkEndPos;
				}
				else
				{
					Vector3 tpPos = enterToExit.MultiplyPoint3x4(portalLinkData.portalPos);
					if (exitTrans.IsFloor() && SampleEndPos(tpPos, out var endPos))
					{
						vector7 = endPos;
					}
					else
					{
						vector7 = new Vector3(vector2.x, tpPos.y, vector2.z);
						float num3 = exitPortal.LinkOffset(exitHandle.side) + 3f;
						vector7 += exitTrans.backManaged * num3;
					}
				}
				vector7 += portalLinkData.endSpiderOffset;
				if (TryGetComponent<Collider>(out var component2))
				{
					_ = component2.bounds.extents;
				}
				if (Physics.Linecast(vector2, vector7, out var _, lmaskEnv))
				{
					return false;
				}
				portalLinkData.linkEndPos = vector7;
			}
			else if (details.isLinkTraversal)
			{
				vector2 = vector6 + tpNavFwd * details.navDistance * Vector3.Dot(tpNavFwd, vector);
				NavMeshHit val = default(NavMeshHit);
				if (NavMesh.SamplePosition(vector2, ref val, 0.5f, nma.areaMask))
				{
					vector2 = ((NavMeshHit)(ref val)).position;
					flag = true;
				}
				if (details.calculateLinkEndPos)
				{
					Vector3 endPos2;
					bool flag3 = SampleEndPos(vector2, out endPos2);
					if (flag3)
					{
						portalLinkData.linkEndPos = endPos2;
					}
					if (!flag && !flag3)
					{
						return null;
					}
				}
				Quaternion quaternion2 = quaternion;
				quaternion = Quaternion.LookRotation(tpNavFwd, Vector3.up);
				postPortalOffsetRot = Quaternion.Inverse(quaternion) * quaternion2;
			}
			else if (details.isIntersectTraversal)
			{
				Vector3 vector8 = enterToExit.MultiplyPoint3x4(vector4);
				Vector3 vector9 = enterToExit.MultiplyPoint3x4(details.intersection);
				float num4 = 0.1f;
				if (TryGetComponent<Collider>(out var component3))
				{
					num4 = component3.bounds.extents.x;
				}
				Vector3 end2 = vector8 + vector * num4;
				if (Physics.Linecast(vector9, end2, out var hitInfo3, lmaskEnv))
				{
					float num5 = hitInfo3.distance - num4;
					if (!(num5 > 0f))
					{
						return false;
					}
					vector8 = vector9 + (vector8 - vector9).normalized * num5;
				}
				vector2 = ((!Physics.Raycast(vector8, vector5.normalized, out var hitInfo4, vector5.magnitude, lmaskEnv, QueryTriggerInteraction.Ignore)) ? (vector8 + vector5) : hitInfo4.point);
				if ((bool)(Object)(object)nma)
				{
					NavMeshHit val2 = default(NavMeshHit);
					if (NavMesh.SamplePosition(vector2, ref val2, 1f, nma.areaMask))
					{
						vector2 = ((NavMeshHit)(ref val2)).position;
						flag = true;
					}
					else
					{
						flag2 = true;
					}
				}
				vector = Quaternion.FromToRotation(enterToExit.MultiplyVector(vector5), vector5) * vector;
				Quaternion quaternion3 = quaternion;
				quaternion = Quaternion.LookRotation(vector, Vector3.up);
				postPortalOffsetRot = Quaternion.Inverse(quaternion) * quaternion3;
			}
		}
		if ((bool)rb)
		{
			Vector3 velocity = (flag ? tpNavFwd : enterToExit.MultiplyVector(rb.velocity).normalized) * rb.velocity.magnitude;
			rb.velocity = velocity;
			rb.position = vector2;
		}
		base.transform.SetPositionAndRotation(vector2, quaternion);
		ApplyOffsetRotation();
		if (!IsDrone())
		{
			ResetPortalOffsetRotation();
			postPortalRotationByCenter = details.isIntersectTraversal;
			postPortalDuration = 0.2f;
			postPortalTimer = postPortalDuration;
		}
		if (flag)
		{
			portalLinkData.hasWarped = true;
			nma.Warp(vector2);
		}
		else if (flag2)
		{
			StartFalling();
			if ((bool)gc)
			{
				gc.ForceOff();
				Invoke("DelayedGroundCheckReenable", 0.15f);
			}
			if (details.isLinkTraversal && (bool)rb)
			{
				rb.velocity += 5f * Mathf.Max(eid.totalSpeedModifier, 1f) * vector;
			}
			AbortLinkTraversal(warpPosition: false);
		}
		if (isTraversingPortalLink)
		{
			nma.velocity = tpNavFwd * nma.speed;
			NavMeshPath val3 = new NavMeshPath();
			if (nma.isOnNavMesh && nma.CalculatePath(portalLinkData.startNavDestination, val3))
			{
				nma.path = val3;
			}
			portalLinkData.hasCrossed = true;
		}
		script.OnTeleport(details);
		return true;
		bool SampleEndPos(Vector3 vector10, out Vector3 reference)
		{
			float num6 = exitPortal.LinkOffset(exitHandle.side);
			if (exitTrans.IsFloor())
			{
				reference = vector10 + exitTrans.backManaged * num6 + tpNavFwd * 0.5f;
			}
			else
			{
				reference = vector10 + tpNavFwd * num6;
			}
			NavMeshHit val4 = default(NavMeshHit);
			if (NavMesh.SamplePosition(reference, ref val4, 2f, nma.areaMask))
			{
				reference = ((NavMeshHit)(ref val4)).position;
				return true;
			}
			return false;
		}
	}

	public void OnTeleportBlocked(PortalTravelDetails details)
	{
		if (details.isIntersectTraversal)
		{
			PortalHandle handle = details.portalSequence[0];
			Vector3 positionInFront = PortalUtils.GetPortalObject(handle).GetTransform(handle.side).GetPositionInFront(details.intersection, 0.05f);
			if (isTraversingPortalLink)
			{
				positionInFront -= travellerPosition - base.transform.position;
			}
			base.transform.position = positionInFront;
			rb.position = positionInFront;
		}
		AbortLinkTraversal();
	}

	private void HandlePortalLinkTraversal(bool hasNma)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		if (!hasNma)
		{
			return;
		}
		if (!isTraversingPortalLink)
		{
			OffMeshLinkData currentOffMeshLinkData = nma.currentOffMeshLinkData;
			if (((OffMeshLinkData)(ref currentOffMeshLinkData)).valid)
			{
				portalLinkData.data = nma.currentOffMeshLinkData;
			}
			Object navMeshOwner = nma.navMeshOwner;
			if ((bool)navMeshOwner)
			{
				if (navMeshOwner is PortalIdentifier portalIdentifier)
				{
					portalLinkData.portalHandle = portalIdentifier.Handle;
				}
			}
			else
			{
				currentOffMeshLinkData = nma.currentOffMeshLinkData;
				if (!((OffMeshLinkData)(ref currentOffMeshLinkData)).valid && nma.isOnOffMeshLink)
				{
					nma.ResetPath();
					nma.Warp(base.transform.position);
				}
			}
		}
		if (!isTraversingPortalLink)
		{
			return;
		}
		PortalScene scene = portalManager.Scene;
		PortalHandle portalHandle = portalLinkData.portalHandle;
		Portal portalObject = scene.GetPortalObject(portalHandle);
		NativePortalTransform nativePortalTransform = portalObject.GetTransform(portalHandle.side);
		if (!portalLinkData.hasCrossed && (!portalObject.isActiveAndEnabled || !portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.Enemy)))
		{
			AbortLinkTraversal();
			return;
		}
		if (!portalLinkData.wasOnLink)
		{
			Vector3 startPos = ((OffMeshLinkData)(ref portalLinkData.data)).startPos;
			if (scene.GetPortalIdentifier(portalHandle) == null || !scene.GetPortalIdentifier(portalHandle).GetClosestLink(startPos, out var portalLink))
			{
				AbortLinkTraversal();
				nma.SetDestination(base.transform.position);
				return;
			}
			portalLinkData.startUpdateRotation = nma.updateRotation;
			portalLinkData.startUpdatePosition = nma.updatePosition;
			portalLinkData.startAutoTraverseOffMeshLink = nma.autoTraverseOffMeshLink;
			portalLinkData.startUseGravity = rb.useGravity;
			nma.autoTraverseOffMeshLink = false;
			nma.updatePosition = false;
			nma.updateRotation = false;
			rb.useGravity = false;
			Vector3 portalPos = portalLink.portalPos;
			portalLinkData.portalPos = portalPos;
			if (!IsSpider())
			{
				if (nativePortalTransform.IsFloor())
				{
					portalLinkData.portalTargetPos = portalPos;
				}
				else
				{
					portalLinkData.portalTargetPos = new Vector3(portalPos.x, base.transform.position.y, portalPos.z);
				}
			}
			else
			{
				portalLinkData.portalTargetPos = portalPos;
			}
			portalLinkData.linkEndPos = ((OffMeshLinkData)(ref portalLinkData.data)).endPos;
			portalLinkData.startEnterToExitMatrix = portalObject.GetTravelMatrix(portalHandle.side);
			portalLinkData.startNavDestination = nma.destination;
			if (IsSpider())
			{
				portalLinkData.endSpiderOffset = new Vector3(0f, (base.transform.position - startPos).y, 0f);
				Vector3 vector = portalLinkData.portalTargetPos;
				if (!nativePortalTransform.IsFloor())
				{
					vector += portalLinkData.endSpiderOffset;
				}
				if (!portalObject.isMultiPanel)
				{
					float num = 2f;
					if (TryGetComponent<Collider>(out var component))
					{
						num = component.bounds.extents.y;
					}
					if (!portalObject.IsAnyTransformTilted() || nativePortalTransform.IsFloor())
					{
						vector = portalObject.AdjustIntercept(portalHandle.side, vector, num * 1.5f);
					}
				}
				portalLinkData.portalTargetPos = vector;
			}
			portalLinkData.startPortalTargetExitPos = portalLinkData.startEnterToExitMatrix.MultiplyPoint3x4(portalLinkData.portalTargetPos);
			portalLinkData.startPortalTargetEnterLocalPos = nativePortalTransform.WorldToLocal(float3.op_Implicit(portalLinkData.portalTargetPos));
			portalLinkData.wasOnLink = true;
		}
		Vector3 position = base.transform.position;
		Vector3 endPos = ((!portalLinkData.hasCrossed) ? portalLinkData.portalTargetPos : portalLinkData.linkEndPos);
		Vector3 vector2 = Vector3.MoveTowards(position, endPos, nma.speed * Time.fixedDeltaTime);
		if (!portalLinkData.hasCrossed)
		{
			float num2 = nma.speed * Time.fixedDeltaTime;
			float num3 = Vector3.Distance(position, endPos);
			if (num3 < num2)
			{
				portalLinkData.waitTimer += Time.fixedDeltaTime;
				num2 -= num3;
				if (portalLinkData.isWaitExceeded)
				{
					AbortLinkTraversal();
				}
				else if ((IsSpider() && portalObject.isMultiPanel) || CheckLinkTraversalEntryPos(portalObject, portalHandle.side, position, 0.5f, 0.5f))
				{
					bool calculateLinkEndPos = true;
					if (IsSpider() && portalObject.isMultiPanel)
					{
						calculateLinkEndPos = false;
					}
					PortalTravelDetails details = PortalTravelDetails.WithNavOffset(portalHandle, portalObject.GetTravelMatrix(portalHandle.side), num2, calculateLinkEndPos);
					bool? flag = OnTravel(details);
					if (flag.HasValue && flag.Value && portalLinkData.hasCrossed)
					{
						PortalManagerV2 portalManagerV = portalManager;
						IPortalTraveller traveller = this;
						portalManagerV.TravellerCallback(in traveller, in details);
						portalManager.UpdateTraveller(this);
					}
				}
			}
			else
			{
				MoveTo(vector2);
			}
		}
		else
		{
			MoveTo(vector2);
			float num4 = Vector3.Distance(endPos, vector2);
			if (portalLinkData.hasCrossed && num4 < 0.01f)
			{
				CompleteLinkTraversal(!portalLinkData.hasWarped);
			}
		}
		void MoveTo(Vector3 pos)
		{
			Quaternion rotation = Quaternion.LookRotation(new Vector3(endPos.x, pos.y, endPos.z) - base.transform.position, Vector3.up);
			base.transform.SetPositionAndRotation(pos, rotation);
			rb.rotation = base.transform.rotation;
		}
	}

	private bool CheckLinkTraversalEntryPos(Portal portal, PortalSide side, Vector3 worldIntersect, float margin, float distToPlane)
	{
		Vector3 vector = portal.GetTransform(side).toLocalManaged.MultiplyPoint3x4(worldIntersect);
		if (Mathf.Abs(vector.z) > distToPlane)
		{
			return false;
		}
		PlaneShape shape = portal.GetShape();
		if (Mathf.Abs(vector.x) > shape.width / 2f + margin)
		{
			return false;
		}
		if (Mathf.Abs(vector.y) > shape.height / 2f + margin)
		{
			return false;
		}
		return true;
	}

	private void AbortLinkTraversal(bool warpPosition = true)
	{
		CompleteLinkTraversal(warpPosition, resetPath: true);
	}

	private void CompleteLinkTraversal(bool warpPosition = false, bool resetPath = false)
	{
		if (!isTraversingPortalLink)
		{
			return;
		}
		if ((bool)(Object)(object)nma)
		{
			if (nma.isOnNavMesh)
			{
				if (resetPath)
				{
					nma.ResetPath();
				}
				if (warpPosition)
				{
					nma.Warp(base.transform.position);
				}
			}
			nma.updateRotation = portalLinkData.startUpdateRotation;
			nma.updatePosition = portalLinkData.startUpdatePosition;
			nma.autoTraverseOffMeshLink = portalLinkData.startAutoTraverseOffMeshLink;
		}
		if ((bool)rb)
		{
			rb.useGravity = portalLinkData.startUseGravity;
		}
		portalLinkData.Reset();
	}

	private void DelayedGroundCheckReenable()
	{
		if ((bool)gc)
		{
			gc.StopForceOff();
		}
	}

	private void ApplyOffsetRotation()
	{
		if (postPortalRotationByCenter && (bool)bodyCenter)
		{
			bodyCenter.localRotation = postPortalOffsetRot;
		}
		if (!postPortalRotationByCenter && (bool)rotationTransform)
		{
			rotationTransform.localRotation = postPortalOffsetRot;
		}
	}

	private void UpdatePostPortalRotation()
	{
		if (!IsDrone() && !(postPortalTimer <= 0f))
		{
			postPortalTimer = Mathf.MoveTowards(postPortalTimer, 0f, Time.deltaTime);
			if (postPortalRotationByCenter && (bool)bodyCenter)
			{
				bodyCenter.localRotation = Quaternion.Lerp(Quaternion.identity, postPortalOffsetRot, postPortalTimer / postPortalDuration);
			}
			if (!postPortalRotationByCenter && (bool)rotationTransform)
			{
				rotationTransform.localRotation = Quaternion.Lerp(Quaternion.identity, postPortalOffsetRot, postPortalTimer / postPortalDuration);
			}
		}
	}

	private void ResetPortalOffsetRotation()
	{
		if ((bool)bodyCenter)
		{
			bodyCenter.localRotation = Quaternion.identity;
		}
		if ((bool)rotationTransform)
		{
			rotationTransform.localRotation = Quaternion.identity;
		}
	}
}
