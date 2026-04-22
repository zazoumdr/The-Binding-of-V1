using System;
using System.Collections.Generic;
using System.Text;
using Gravity;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class NewMovement : MonoSingleton<NewMovement>, ITarget, IPortalTraveller
{
	public enum PreserveLengthMode
	{
		DontPreserve,
		PreserveHorizontal,
		PreserveAll
	}

	public float walkSpeed;

	public float airAcceleration;

	public float jumpPower;

	public float wallJumpPower;

	public Vector3 pushForce;

	private LayerMask frictionlessSurfaceMask;

	public CameraController cc;

	private InputManager inman;

	private AssistController asscon;

	private StyleHUD shud;

	private StyleCalculator scalc;

	private GunControl gunc;

	private FistControl punch;

	private int difficulty;

	[HideInInspector]
	public bool levelOver;

	[HideInInspector]
	public float longestSlide;

	[HideInInspector]
	public int sameCheckpointRestarts;

	public bool endlessMode;

	public bool quakeJump;

	public int gamepadFreezeCount;

	public bool activated;

	public int hp = 100;

	public float antiHp;

	[HideInInspector]
	public float hurtInvincibility;

	public bool dead;

	private Vector3 inputDir;

	private Vector3 targetVel;

	private Vector3 airDirection;

	private Vector3 groundCheckPos;

	private bool onGasoline;

	public bool walking;

	private bool enemyStepping;

	[HideInInspector]
	public bool falling;

	private bool jumpCooldown;

	public int currentWallJumps;

	private Vector3 wallJumpPos;

	private float clingFade;

	private float antiHpCooldown;

	private bool cantInstaHeal;

	public Action<PortalTravelDetails> onPortalTraversed;

	[NonSerialized]
	public int lastTraversalFrame;

	public bool boost;

	public float boostCharge = 300f;

	private float boostLeft;

	private bool dashedFromGround;

	private float dashStorage;

	public Vector3 dodgeDirection;

	public bool jumping;

	private float fallSpeed;

	private float fallTime;

	public bool stillHolding;

	public float slamForce;

	private bool slamStorage;

	[HideInInspector]
	public float slamCooldown;

	[HideInInspector]
	public bool exploded;

	[HideInInspector]
	public float safeExplosionLaunchCooldown;

	public bool sliding;

	private float slideSafety;

	private bool slideEnding;

	private float slideLength;

	private bool crouching;

	public bool standing;

	[HideInInspector]
	public int rocketJumps;

	[HideInInspector]
	public int hammerJumps;

	[HideInInspector]
	public float explosionLaunchResistance;

	[HideInInspector]
	public Grenade ridingRocket;

	[HideInInspector]
	public int rocketRides;

	public float preSlideSpeed;

	public float preSlideDelay;

	public bool slowMode;

	public CustomGroundProperties groundProperties;

	private float friction;

	public Image hurtScreen;

	public DeathSequence deathSequence;

	public FlashImage hpFlash;

	public FlashImage antiHpFlash;

	public GameObject screenHud;

	private Canvas fullHud;

	private Vector3 hudOriginalPos;

	public GameObject scrnBlood;

	public GameObject hudCam;

	private Vector3 camOriginalPos;

	private Color hurtColor;

	private Color currentColor;

	[HideInInspector]
	public Rigidbody rb;

	[HideInInspector]
	public CapsuleCollider playerCollider;

	public GroundCheckGroup gc;

	public GroundCheckGroup slopeCheck;

	private WallCheckGroup wcGroup;

	private VerticalClippingBlocker vcb;

	private AudioSource aud;

	private AudioSource audGround;

	private AudioSource audWoosh;

	private AudioSource hurtAud;

	private AudioSource greenHpAud;

	public AudioSource oilSlideEffect;

	public AudioClip jumpSound;

	public AudioClip landingSound;

	public AudioClip finalWallJump;

	public AudioClip dodgeSound;

	public GameObject staminaFailSound;

	public GameObject slideStopSound;

	private AudioSource[] fricSlideAuds;

	private float[] fricSlideAudVols;

	private float[] fricSlideAudPitches;

	public GameObject dashJumpSound;

	public GameObject quakeJumpSound;

	public GameObject dodgeParticle;

	public GameObject impactDust;

	public GameObject fallParticle;

	private GameObject currentFallParticle;

	public GameObject slideParticle;

	private GameObject currentSlideParticle;

	private SurfaceType currentSlideSurfaceType;

	private GameObject slideScrape;

	private TrailModule slideTrail;

	private MinMaxGradient normalSlideGradient;

	public MinMaxGradient invincibleSlideGradient;

	private GameObject wallScrape;

	private SurfaceType currentScrapeSurfaceType;

	private GameObject currentFrictionlessSlideParticle;

	private SurfaceType currentFricSlideSurfaceType;

	[HideInInspector]
	public bool modNoDashSlide;

	[HideInInspector]
	public bool modNoJump;

	[HideInInspector]
	public float modForcedFrictionMultip = 1f;

	private PortalAwarePlayerCollider portalAwareCollider;

	private RigidbodyConstraints defaultRBConstraints;

	private float ssjMaxFrames = 4f;

	[HideInInspector]
	public HashSet<Water> touchingWaters = new HashSet<Water>();

	[HideInInspector]
	public int fakeFallRequests;

	[HideInInspector]
	public List<FakeFallZone> fakeFallZones = new List<FakeFallZone>();

	[HideInInspector]
	public List<GravityVolume> gravityVolumes = new List<GravityVolume>();

	public double slideTimestamp;

	public double jumpTimestamp;

	public ParticleSystem windStateParticle;

	public float windState;

	private TimeSince lastJump;

	public const float ONE_PHYSICS_FRAME = 0.008f;

	public Vector3 preDashSpeed;

	private Vector3Int? lastCheckedGasolineVoxel;

	private int framesSinceSlide;

	private Vector3 velocityAfterSlide;

	private Vector3? postTeleportForward;

	private Vector3? postTeleportRight;

	private Vector3 cachedPos;

	private Vector3 cachedHeadPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

	private int remainingTeleportFrames;

	public Vector3 portalTravelPoint => cc.cam.transform.position;

	public int Id => GetInstanceID();

	public TargetType Type => TargetType.PLAYER;

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

	public Transform Transform => rb.transform;

	public Vector3 Position => cachedPos;

	public Vector3 HeadPosition => cachedHeadPos;

	public bool TeleportFixedFramesPending => remainingTeleportFrames > 0;

	public int id => GetInstanceID();

	public int? targetId => MonoSingleton<PlayerTracker>.Instance.GetInstanceID();

	public Vector3 travellerPosition
	{
		get
		{
			if (!wcGroup.OnWall())
			{
				return cc.transform.parent.localToWorldMatrix.MultiplyPoint3x4(cc.defaultTarget);
			}
			return cc.cam.transform.position;
		}
	}

	public Vector3 travellerVelocity => rb.velocity;

	public PortalTravellerType travellerType => PortalTravellerType.PLAYER;

	protected void Awake()
	{
		cc = GetComponentInChildren<CameraController>();
		rb = GetComponent<Rigidbody>();
		vcb = MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
		wcGroup = GetComponentInChildren<WallCheckGroup>();
		aud = GetComponent<AudioSource>();
		audGround = gc.GetComponent<AudioSource>();
		WallCheck componentInChildren = GetComponentInChildren<WallCheck>();
		audWoosh = componentInChildren.GetComponent<AudioSource>();
		portalAwareCollider = GetComponent<PortalAwarePlayerCollider>();
		playerCollider = GetComponent<CapsuleCollider>();
		frictionlessSurfaceMask = LayerMaskDefaults.Get(LMD.Environment);
		frictionlessSurfaceMask = (int)frictionlessSurfaceMask | 1;
	}

	private void OnEnable()
	{
		if (MonoSingleton<InputManager>.TryGetInstance(out InputManager inputManager))
		{
			inputManager.InputSource.Jump.Action.performed += JumpPerformed;
			inputManager.InputSource.Slide.Action.canceled += SlideCancelled;
		}
	}

	private void SlideCancelled(CallbackContext ctx)
	{
		if (sliding)
		{
			slideTimestamp = ((CallbackContext)(ref ctx)).time;
		}
	}

	private void JumpPerformed(CallbackContext ctx)
	{
		jumpTimestamp = ((CallbackContext)(ref ctx)).time;
	}

	private void OnDisable()
	{
		if (MonoSingleton<InputManager>.TryGetInstance(out InputManager inputManager))
		{
			inputManager.InputSource.Jump.Action.performed -= JumpPerformed;
			inputManager.InputSource.Slide.Action.canceled -= SlideCancelled;
		}
		if (sliding)
		{
			StopSlide();
		}
		if ((bool)currentFallParticle)
		{
			UnityEngine.Object.Destroy(currentFallParticle);
		}
		if ((bool)wallScrape)
		{
			UnityEngine.Object.Destroy(wallScrape);
		}
		Physics.IgnoreLayerCollision(2, 12, ignore: false);
	}

	private void OnPrefChanged(string key, object value)
	{
		if (key == "weaponHoldPosition" || key == "hudType")
		{
			int num = MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition");
			int num2 = MonoSingleton<PrefsManager>.Instance.GetInt("hudType");
			quakeJump = num == 1 && num2 >= 2;
		}
	}

	private void Start()
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Combine(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
		inman = MonoSingleton<InputManager>.Instance;
		asscon = MonoSingleton<AssistController>.Instance;
		if ((bool)(UnityEngine.Object)(object)hurtScreen)
		{
			hurtColor = ((Graphic)hurtScreen).color;
			currentColor = hurtColor;
			currentColor.a = 0f;
			((Graphic)hurtScreen).color = currentColor;
			((Behaviour)(object)hurtScreen).enabled = false;
			Shader.SetGlobalColor("_HurtScreenColor", currentColor);
			hurtAud = ((Component)(object)hurtScreen).GetComponent<AudioSource>();
			fullHud = ((Component)(object)hurtScreen).GetComponentInParent<Canvas>();
		}
		hudOriginalPos = screenHud.transform.localPosition;
		camOriginalPos = hudCam.transform.localPosition;
		MonoSingleton<TimeController>.Instance.SetAllPitch(1f);
		defaultRBConstraints = rb.constraints;
		rb.solverIterations *= 5;
		rb.solverVelocityIterations *= 5;
		groundCheckPos = gc.transform.localPosition;
		scalc = MonoSingleton<StyleCalculator>.Instance;
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		TrailModule trails = slideParticle.GetComponent<ParticleSystem>().trails;
		normalSlideGradient = ((TrailModule)(ref trails)).colorOverLifetime;
		if (difficulty == 0 && hp == 100)
		{
			hp = 200;
		}
		playerCollider.providesContacts = true;
		playerCollider.hasModifiableContacts = true;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 portalManagerV))
		{
			portalManagerV.AddPlayer(this);
		}
	}

	private void OnDestroy()
	{
		PrefsManager.onPrefChanged = (Action<string, object>)Delegate.Remove(PrefsManager.onPrefChanged, new Action<string, object>(OnPrefChanged));
	}

	public AudioSource DuplicateDetachWhoosh()
	{
		if (!(UnityEngine.Object)(object)audWoosh)
		{
			return null;
		}
		float time = audWoosh.time;
		((Behaviour)(object)audWoosh).enabled = false;
		GameObject obj = UnityEngine.Object.Instantiate(((Component)(object)audWoosh).gameObject, ((Component)(object)audWoosh).transform.parent, worldPositionStays: true);
		UnityEngine.Object.Destroy(obj.GetComponent<WallCheck>());
		AudioSource component = obj.GetComponent<AudioSource>();
		component.time = time;
		component.Play(tracked: true);
		return component;
	}

	public AudioSource RestoreWhoosh()
	{
		((Behaviour)(object)audWoosh).enabled = true;
		return audWoosh;
	}

	private void FrictionlessSlideParticle()
	{
		CreateSlideScrape(currentFrictionlessSlideParticle == null, frictionlessVersion: true);
		Vector3 vector = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		float num = Mathf.Min(35f, vector.magnitude) / 35f;
		currentFrictionlessSlideParticle.transform.localScale = Vector3.one * 0.25f * num;
		currentFrictionlessSlideParticle.transform.position = base.transform.position;
		currentFrictionlessSlideParticle.transform.rotation = Quaternion.LookRotation(vector.normalized);
		for (int i = 0; i < fricSlideAuds.Length; i++)
		{
			fricSlideAuds[i].volume = Mathf.Lerp(0f, fricSlideAudVols[i], num);
			fricSlideAuds[i].SetPitch(Mathf.Lerp(fricSlideAudPitches[i] / 2f, fricSlideAudPitches[i], num));
		}
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0884: Unknown result type (might be due to invalid IL or missing references)
		//IL_088c: Unknown result type (might be due to invalid IL or missing references)
		EmissionModule emission = windStateParticle.emission;
		((Component)(object)windStateParticle).transform.position = cc.GetDefaultPos() + rb.velocity.normalized;
		((Component)(object)windStateParticle).transform.LookAt(cc.GetDefaultPos());
		((EmissionModule)(ref emission)).rateOverDistanceMultiplier = Mathf.Min(windState, 0.5f) * 10f;
		if (gc.onGround)
		{
			CheckForGasoline();
			if (!onGasoline && groundProperties != null && groundProperties.friction == 0f)
			{
				FrictionlessSlideParticle();
			}
			else if (currentFrictionlessSlideParticle != null)
			{
				UnityEngine.Object.Destroy(currentFrictionlessSlideParticle);
			}
		}
		else
		{
			if (((Component)(object)oilSlideEffect).gameObject.activeSelf)
			{
				((Component)(object)oilSlideEffect).gameObject.SetActive(value: false);
			}
			if (!slopeCheck.onGround && Physics.Raycast(gc.transform.position + base.transform.up * 0.1f, base.transform.up * -1f, out var hitInfo, 0.5f, frictionlessSurfaceMask, QueryTriggerInteraction.Ignore))
			{
				if (hitInfo.transform.gameObject.layer == 0 || hitInfo.transform.gameObject.tag == "Slippery")
				{
					FrictionlessSlideParticle();
				}
			}
			else if (currentFrictionlessSlideParticle != null)
			{
				UnityEngine.Object.Destroy(currentFrictionlessSlideParticle);
			}
		}
		Vector2 vector = Vector2.zero;
		if (activated)
		{
			vector = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
			cc.movementHor = vector.x;
			cc.movementVer = vector.y;
			Vector3 normalized = Vector3.ProjectOnPlane(postTeleportRight ?? cc.cam.transform.right, rb.GetGravityDirection()).normalized;
			postTeleportRight = null;
			Vector3 vector2 = postTeleportForward ?? Vector3.ProjectOnPlane(base.transform.forward, rb.GetGravityDirection()).normalized;
			postTeleportForward = null;
			inputDir = Vector3.ClampMagnitude(normalized * vector.x + vector2 * vector.y, 1f);
			if (punch == null)
			{
				punch = GetComponentInChildren<FistControl>();
			}
			else if (!punch.enabled)
			{
				punch.YesFist();
			}
		}
		else
		{
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
			if (currentSlideParticle != null)
			{
				UnityEngine.Object.Destroy(currentSlideParticle);
			}
			else if (slideScrape != null)
			{
				DetachSlideScrape();
			}
			if (punch == null)
			{
				punch = GetComponentInChildren<FistControl>();
			}
			else
			{
				punch.NoFist();
			}
		}
		if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad && gamepadFreezeCount > 0)
		{
			vector = Vector2.zero;
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			cc.movementHor = 0f;
			cc.movementVer = 0f;
			inputDir = Vector3.zero;
			return;
		}
		if (gc.onGround)
		{
			fallTime = 0f;
			clingFade = 0f;
		}
		else
		{
			if (fallTime < 1f)
			{
				fallTime += Time.deltaTime * 5f;
			}
			else
			{
				falling = true;
			}
			float num = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
			if (num < -2f)
			{
				fallSpeed = num;
			}
			if (num < -20f)
			{
				audWoosh.SetPitch(Mathf.Abs(fallSpeed) / 120f);
				audWoosh.volume = Mathf.Abs(fallSpeed) / (float)(activated ? 80 : 240);
			}
			else
			{
				audWoosh.SetPitch(0f);
				audWoosh.volume = 0f;
			}
			if (windState > 0f)
			{
				audWoosh.volume = Mathf.Min(windState, 1f);
				audWoosh.SetPitch(1f);
			}
			if (num < -100f)
			{
				rb.velocity = Vector3.ProjectOnPlane(rb.velocity, rb.GetGravityDirection());
				rb.velocity += rb.GetGravityDirection() * 100f;
			}
		}
		if (falling)
		{
			CheckLanding();
		}
		else
		{
			audWoosh.SetPitch(0f);
			audWoosh.volume = 0f;
		}
		if (!GameStateManager.Instance.PlayerInputLocked && activated)
		{
			HandleInputs();
		}
		if (gc.onGround)
		{
			if ((bool)wallScrape)
			{
				DetachWallScrape();
			}
		}
		else
		{
			Cling();
		}
		Physics.IgnoreLayerCollision(2, 12, gc.heavyFall || sliding);
		TryFloorSnap();
		if (gc.heavyFall)
		{
			if (!slamStorage)
			{
				rb.velocity = rb.GetGravityDirection() * 100f;
			}
			slamForce += Time.deltaTime * 5f;
			if (Physics.Raycast(gc.transform.position + base.transform.up, base.transform.up * -1f, out var hitInfo2, 5f, LayerMaskDefaults.Get(LMD.Environment)) || Physics.SphereCast(gc.transform.position + base.transform.up, 1f, base.transform.up * -1f, out hitInfo2, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Breakable component = hitInfo2.collider.GetComponent<Breakable>();
				if (component != null && ((component.weak && !component.precisionOnly && !component.specialCaseOnly) || component.forceGroundSlammable) && !component.unbreakable)
				{
					UnityEngine.Object.Instantiate(impactDust, hitInfo2.point, Quaternion.identity);
					component.Break(2f);
				}
				if (hitInfo2.collider.gameObject.TryGetComponent<Bleeder>(out var component2))
				{
					component2.GetHit(hitInfo2.point, GoreType.Head);
				}
				if (hitInfo2.transform.TryGetComponent<Idol>(out var component3))
				{
					component3.Death();
				}
			}
		}
		if (stillHolding && MonoSingleton<InputManager>.Instance.InputSource.Slide.WasCanceledThisFrame)
		{
			stillHolding = false;
		}
		if (windState > 0f)
		{
			Vector3 normalized2 = Vector3.ProjectOnPlane(rb.velocity.normalized, base.transform.up).normalized;
			if (currentSlideParticle != null)
			{
				currentSlideParticle.transform.position = base.transform.position + normalized2 * 10f;
				currentSlideParticle.transform.forward = -dodgeDirection;
				((TrailModule)(ref slideTrail)).colorOverLifetime = ((boostLeft > 0f && base.gameObject.layer == 15) ? invincibleSlideGradient : normalSlideGradient);
			}
		}
		HandleSlideState();
		walking = vector.sqrMagnitude > Mathf.Epsilon && !sliding && gc.onGround;
		if (hurtInvincibility >= 0f && hp > 0)
		{
			hurtInvincibility = Mathf.MoveTowards(hurtInvincibility, 0f, Time.deltaTime);
		}
		if (currentColor.a > 0f)
		{
			currentColor.a -= Time.deltaTime;
			Shader.SetGlobalColor("_HurtScreenColor", currentColor);
		}
		if (safeExplosionLaunchCooldown > 0f)
		{
			safeExplosionLaunchCooldown = Mathf.MoveTowards(safeExplosionLaunchCooldown, 0f, Time.deltaTime);
		}
		if (boostCharge != 300f && !sliding && !slowMode)
		{
			float num2 = 1f;
			if (difficulty == 1)
			{
				num2 = 1.5f;
			}
			else if (difficulty == 0)
			{
				num2 = 2f;
			}
			boostCharge = Mathf.MoveTowards(boostCharge, 300f, 70f * Time.deltaTime * num2);
		}
		if (slamCooldown > 0f)
		{
			slamCooldown = Mathf.MoveTowards(slamCooldown, 0f, Time.deltaTime);
		}
		if (explosionLaunchResistance > 0f)
		{
			explosionLaunchResistance = Mathf.MoveTowards(explosionLaunchResistance, 0f, Time.deltaTime);
		}
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("reduceHudMotion"))
		{
			Vector3 vector3 = hudOriginalPos - cc.transform.InverseTransformDirection(rb.velocity) / 1000f;
			float num3 = Vector3.Distance(vector3, screenHud.transform.localPosition);
			screenHud.transform.localPosition = Vector3.MoveTowards(screenHud.transform.localPosition, vector3, Time.deltaTime * 15f * num3);
			Vector3 vector4 = Vector3.ClampMagnitude(camOriginalPos - cc.transform.InverseTransformDirection(rb.velocity) / 350f * -1f, 0.2f);
			float num4 = Vector3.Distance(vector4, hudCam.transform.localPosition);
			hudCam.transform.localPosition = Vector3.MoveTowards(hudCam.transform.localPosition, vector4, Time.deltaTime * 25f * num4);
		}
		int rankIndex = MonoSingleton<StyleHUD>.Instance.rankIndex;
		if ((rankIndex == 7 || difficulty <= 1) && !cantInstaHeal)
		{
			antiHp = 0f;
			antiHpCooldown = 0f;
		}
		else if (antiHpCooldown > 0f)
		{
			if (rankIndex >= 4)
			{
				antiHpCooldown = Mathf.MoveTowards(antiHpCooldown, 0f, Time.deltaTime * (float)(rankIndex / 2));
			}
			else
			{
				antiHpCooldown = Mathf.MoveTowards(antiHpCooldown, 0f, Time.deltaTime);
			}
		}
		else if (antiHp > 0f)
		{
			cantInstaHeal = false;
			if (rankIndex >= 4)
			{
				antiHp = Mathf.MoveTowards(antiHp, 0f, Time.deltaTime * (float)rankIndex * 10f);
			}
			else
			{
				antiHp = Mathf.MoveTowards(antiHp, 0f, Time.deltaTime * 15f);
			}
		}
		if (!gc.heavyFall && currentFallParticle != null)
		{
			UnityEngine.Object.Destroy(currentFallParticle);
		}
	}

	private void Cling()
	{
		if (wcGroup.OnWall())
		{
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
			PhysicsCastResult hitInfo;
			PortalTraversalV2[] portalTraversals;
			Vector3 endPoint;
			bool flag = PortalPhysicsV2.Raycast(base.transform.position, inputDir, 1f, layerMask, out hitInfo, out portalTraversals, out endPoint);
			if (!flag && portalTraversals.Length != 0)
			{
				for (int i = 0; i < portalTraversals.Length; i++)
				{
					Vector3 start = portalTraversals[i].exitPoint - portalTraversals[i].exitDirection * 0.0001f;
					Vector3 end = ((i == portalTraversals.Length - 1) ? endPoint : (portalTraversals[i + 1].entrancePoint + portalTraversals[i + 1].entranceDirection * 0.0001f));
					if (Physics.Linecast(start, end, layerMask))
					{
						flag = true;
						break;
					}
				}
			}
			float num = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
			if (!sliding && flag && !gc.heavyFall && num < -1f)
			{
				float num2 = Mathf.Clamp(-1f, 1f, Vector3.Dot(base.transform.right, rb.velocity));
				float num3 = Mathf.Clamp(-1f, 1f, Vector3.Dot(base.transform.forward, rb.velocity));
				rb.velocity = num2 * base.transform.right + num3 * base.transform.forward + rb.GetGravityDirection() * 2f * clingFade;
				CreateWallScrape(hitInfo.point + base.transform.up, wallScrape == null);
				wallScrape.transform.forward = hitInfo.normal;
				clingFade = Mathf.MoveTowards(clingFade, 50f, Time.deltaTime * 4f);
			}
			else if ((bool)wallScrape)
			{
				DetachWallScrape();
			}
		}
		else if ((bool)wallScrape)
		{
			DetachWallScrape();
		}
	}

	private void HandleSlideState()
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		if (sliding)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasCanceledThisFrame || (slowMode && !crouching))
			{
				StopSlide();
			}
			standing = false;
			slideLength += Time.deltaTime;
			cc.defaultTarget = cc.originalPos - Vector3.up * 0.625f;
			Vector3 normalized = Vector3.ProjectOnPlane(rb.velocity.normalized, base.transform.up).normalized;
			if (currentSlideParticle != null)
			{
				currentSlideParticle.transform.position = base.transform.position + normalized * 10f;
				currentSlideParticle.transform.forward = -dodgeDirection;
				((TrailModule)(ref slideTrail)).colorOverLifetime = ((boostLeft > 0f && base.gameObject.layer == 15) ? invincibleSlideGradient : normalSlideGradient);
			}
			if (slideSafety > 0f)
			{
				slideSafety -= Time.deltaTime * 5f;
			}
			if ((bool)slideScrape)
			{
				if (gc.onGround || wcGroup.OnWall())
				{
					slideScrape.transform.position = base.transform.position + normalized;
					slideScrape.transform.forward = -normalized;
					cc.CameraShake(0.1f);
				}
				else
				{
					slideScrape.transform.position = Vector3.one * 5000f;
				}
			}
			return;
		}
		if ((bool)groundProperties && groundProperties.forceCrouch)
		{
			playerCollider.height = 1.25f;
			crouching = true;
			if (standing)
			{
				standing = false;
				base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 1.125f, base.transform.position.z);
				gc.SetLocalPosition(groundCheckPos + Vector3.up * 1.125f);
			}
			cc.defaultTarget = cc.originalPos - base.transform.up * 0.625f;
			return;
		}
		if (currentSlideParticle != null)
		{
			UnityEngine.Object.Destroy(currentSlideParticle);
		}
		if (slideScrape != null)
		{
			DetachSlideScrape();
		}
		if ((bool)playerCollider && playerCollider.height != 3.5f)
		{
			Vector3 vector = new Vector3(playerCollider.bounds.center.x, playerCollider.bounds.min.y, playerCollider.bounds.center.z);
			if (Physics.Raycast(vector, base.transform.up, 3.5f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore) || Physics.SphereCast(new Ray(vector + base.transform.up * 0.25f, base.transform.up), 0.5f, 2f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				crouching = true;
				slowMode = true;
				return;
			}
			crouching = false;
			slowMode = false;
			Vector3 vector2 = travellerPosition;
			playerCollider.height = 3.5f;
			gc.SetLocalPosition(groundCheckPos);
			if (Physics.Raycast(base.transform.position, -base.transform.up, 2.25f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				base.transform.position += base.transform.up * 1.125f;
			}
			else
			{
				base.transform.position += base.transform.up * -0.625f;
				cc.defaultTarget = cc.originalPos;
				standing = true;
			}
			if (!portalAwareCollider)
			{
				return;
			}
			cc.defaultTarget = cc.originalPos;
			Vector3 to = base.transform.localToWorldMatrix.MultiplyPoint3x4(cc.originalPos);
			if (portalAwareCollider.TryGetCrossingPortal(vector2, to, out var handle, out var intersection, out var portalNormal))
			{
				Vector3 intersection2 = intersection;
				intersection += portalNormal * 0.01f;
				Vector3 vector3 = base.transform.localToWorldMatrix.MultiplyVector(cc.originalPos);
				Vector3 position = intersection - vector3;
				base.transform.position = position;
				rb.position = position;
				cc.transform.localPosition = cc.originalPos;
				cc.defaultPos = cc.originalPos;
				PortalManagerV2 portalManagerV = MonoSingleton<PortalManagerV2>.Instance;
				Matrix4x4 travelMatrix = portalManagerV.Scene.GetTravelMatrix(handle);
				PortalTravelDetails details = PortalTravelDetails.WithInteresction(new PortalHandleSequence(handle), Array.Empty<PortalTraversalV2>(), travelMatrix, intersection2);
				bool? flag = OnTravel(details);
				if (flag.HasValue && flag.Value)
				{
					IPortalTraveller traveller = this;
					portalManagerV.TravellerCallback(in traveller, in details);
					portalManagerV.UpdateTraveller(this);
				}
				else if (flag.HasValue && !flag.Value)
				{
					OnTeleportBlocked(details);
				}
			}
		}
		else if (cc.defaultTarget != cc.originalPos)
		{
			cc.defaultTarget = cc.originalPos;
		}
		else
		{
			standing = true;
		}
	}

	private void TryFloorSnap()
	{
		if (slopeCheck.onGround || slopeCheck.forcedOff > 0 || modForcedFrictionMultip == 0f || jumping || boost)
		{
			return;
		}
		float num = playerCollider.height / 2f - playerCollider.center.y;
		if (!(rb.velocity == Vector3.zero) && Physics.Raycast(base.transform.position, base.transform.up * -1f, out var hitInfo, num + 1f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			Vector3 target = base.transform.position - base.transform.up * hitInfo.distance + base.transform.up * num;
			base.transform.position = Vector3.MoveTowards(base.transform.position, target, hitInfo.distance * Time.deltaTime * 10f);
			if (Vector3.Dot(rb.velocity.normalized, base.transform.up) > 0f)
			{
				rb.velocity = Vector3.ProjectOnPlane(rb.velocity, base.transform.up);
			}
		}
	}

	private void HandleInputs()
	{
		bool flag = !falling;
		bool flag2 = !gc.onGround && (gc.canJump || wcGroup.CheckForEnemyCols());
		if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && !jumpCooldown)
		{
			if (flag2)
			{
				enemyStepping = true;
				EnemyStepResets();
				if (sliding || InputState.currentTime - slideTimestamp < 0.10000000149011612)
				{
					windState = 0.5f;
				}
				Jump();
			}
			else if (flag)
			{
				Jump();
			}
		}
		bool flag3 = GameStateManager.Instance.IsStateActive("alter-menu");
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame)
		{
			RaycastHit hitInfo;
			bool flag4 = Physics.Raycast(gc.transform.position + base.transform.up, base.transform.up * -1f, out hitInfo, 2f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore);
			if (!flag3 && (gc.onGround || gc.sinceLastGrounded < 0.06f || (float)lastJump < 0.06f || flag4) && (!slowMode || crouching) && !sliding)
			{
				StartSlide();
			}
		}
		if (!slowMode && MonoSingleton<InputManager>.Instance.InputSource.Dodge.WasPerformedThisFrame)
		{
			TryDash();
		}
		if (!gc.onGround && fakeFallRequests <= 0)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && currentWallJumps < 3 && !jumpCooldown && (bool)wcGroup && wcGroup.TryGetActiveInstance(out var wcInstance))
			{
				WallJump(wcInstance);
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame)
			{
				TryStartSlam();
			}
		}
	}

	private void TryDash()
	{
		bool num = ((bool)groundProperties && !groundProperties.canDash) || modNoDashSlide || boostCharge < 100f;
		dashedFromGround = gc.onGround;
		bool flag = modNoDashSlide || ((bool)groundProperties && groundProperties.silentDashFail);
		if (num)
		{
			if (!flag)
			{
				UnityEngine.Object.Instantiate(staminaFailSound);
			}
			return;
		}
		if (sliding)
		{
			StopSlide();
		}
		boostLeft = 100f;
		dashStorage = 1f;
		boost = true;
		dodgeDirection = ((inputDir == Vector3.zero) ? base.transform.forward : inputDir);
		Quaternion rotation = Quaternion.LookRotation(dodgeDirection * -1f);
		UnityEngine.Object.Instantiate(dodgeParticle, base.transform.position + dodgeDirection * 10f, rotation);
		if (!asscon.majorEnabled || !asscon.infiniteStamina)
		{
			boostCharge -= 100f;
		}
		if (dodgeDirection == base.transform.forward)
		{
			cc.dodgeDirection = 0;
		}
		else if (dodgeDirection == base.transform.forward * -1f)
		{
			cc.dodgeDirection = 1;
		}
		else
		{
			cc.dodgeDirection = 2;
		}
		aud.clip = dodgeSound;
		aud.volume = 1f;
		aud.SetPitch(1f);
		aud.Play(tracked: true);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.Dash);
		if (gc.heavyFall)
		{
			fallSpeed = 0f;
			gc.heavyFall = false;
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
		}
	}

	private void TryStartSlam()
	{
		if (dashedFromGround && boost && !sliding)
		{
			dashedFromGround = false;
			windState = 0.5f;
			boost = false;
		}
		if (!Physics.Raycast(gc.transform.position + base.transform.up, base.transform.up * -1f, out var _, 3f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore) && fallTime > 0.5f && slamCooldown == 0f && !gc.heavyFall)
		{
			if (boost)
			{
				boostLeft = 0f;
				boost = false;
			}
			if (sliding)
			{
				StopSlide();
			}
			falling = true;
			fallSpeed = -100f;
			rb.velocity = new Vector3(0f, -100f, 0f);
			stillHolding = true;
			gc.heavyFall = true;
			slamForce = 1f;
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
			currentFallParticle = UnityEngine.Object.Instantiate(fallParticle, base.transform);
			if ((bool)MonoSingleton<HookArm>.Instance && MonoSingleton<HookArm>.Instance.beingPulled)
			{
				slamStorage = true;
			}
		}
	}

	private void CheckLanding()
	{
		if (gc.onGround)
		{
			if (fallSpeed > -50f)
			{
				audGround.clip = landingSound;
				audGround.volume = 0.5f + fallSpeed * -0.01f;
				audGround.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
				audGround.Play(tracked: true);
				MonoSingleton<PlayerFootsteps>.Instance.Footstep(0.5f, force: true);
				MonoSingleton<PlayerFootsteps>.Instance.Footstep(0.5f, force: true, 0.05f);
			}
			else
			{
				gc.hasImpacted = true;
				LandingImpact();
			}
			MonoSingleton<PlayerAnimations>.Instance?.Land(Mathf.Abs(fallSpeed / 100f));
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
			if (!jumpCooldown)
			{
				falling = false;
			}
			fallSpeed = 0f;
			slamStorage = false;
			gc.heavyFall = false;
		}
	}

	private void FixedUpdate()
	{
		if (windState > 0f)
		{
			RaycastHit[] array = rb.SweepTestAll(rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime);
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].transform.gameObject.layer == 8 || array[i].transform.gameObject.layer == 24)
					{
						Glass component2;
						if (array[i].transform.TryGetComponent<Breakable>(out var component) && !component.unbreakable && !component.precisionOnly && !component.specialCaseOnly)
						{
							component.Break();
						}
						else if (array[i].transform.TryGetComponent<Glass>(out component2) && !component2.broken)
						{
							component2.Shatter();
						}
					}
				}
			}
		}
		rb.WakeUp();
		if (remainingTeleportFrames > 0)
		{
			remainingTeleportFrames--;
		}
		friction = modForcedFrictionMultip * (groundProperties ? groundProperties.friction : 1f);
		if (sliding)
		{
			if (slideSafety <= 0f)
			{
				Vector3 vector = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
				float num = 10f;
				if ((bool)groundProperties && groundProperties.speedMultiplier < 1f)
				{
					num *= groundProperties.speedMultiplier;
				}
				if (vector.magnitude < num)
				{
					slideSafety = Mathf.MoveTowards(slideSafety, -0.1f, Time.deltaTime);
					if (slideSafety <= -0.1f)
					{
						StopSlide();
					}
				}
				else
				{
					slideSafety = 0f;
				}
			}
			float num2 = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
			if (wcGroup.OnWall() && num2 < 0f)
			{
				rb.AddForce(-rb.GetGravityVector() * 0.4f, ForceMode.Acceleration);
			}
		}
		if (!sliding && activated)
		{
			framesSinceSlide++;
			if (gc.heavyFall)
			{
				preSlideDelay = 0.2f;
				preSlideSpeed = slamForce;
				if (Physics.SphereCast(base.transform.position + base.transform.up * 1.5f, 0.35f, -base.transform.up, out var hitInfo, 3f + Time.fixedDeltaTime * Mathf.Abs(rb.velocity.y), LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					base.transform.position = hitInfo.point + base.transform.up * 1.5f;
					rb.velocity = Vector3.zero;
				}
			}
			else if (!boost && falling && rb.velocity.magnitude / 24f > preSlideSpeed)
			{
				preSlideSpeed = rb.velocity.magnitude / 24f;
				preSlideDelay = 0.2f;
			}
			else
			{
				preSlideDelay = Mathf.MoveTowards(preSlideDelay, 0f, Time.fixedDeltaTime);
				if (preSlideDelay <= 0f)
				{
					preSlideDelay = 0.2f;
					preSlideSpeed = rb.velocity.magnitude / 24f;
				}
			}
		}
		windState -= Time.fixedDeltaTime;
		if (boost)
		{
			rb.SetGravityMode(useGravity: true);
			Dodge();
		}
		else
		{
			Move();
		}
		if (!boost || boostLeft <= 0f)
		{
			preDashSpeed = rb.velocity;
		}
	}

	private void Move()
	{
		slideEnding = false;
		if (hurtInvincibility <= 0f && !levelOver)
		{
			base.gameObject.layer = 2;
			exploded = false;
		}
		if (gc.onGround && !jumping)
		{
			currentWallJumps = 0;
			rocketJumps = 0;
			hammerJumps = 0;
			rocketRides = 0;
		}
		if (gc.onGround && friction > 0f && !jumping)
		{
			float num = Vector3.Dot(rb.GetGravityDirection().normalized, rb.velocity);
			if (slopeCheck.onGround && inputDir.x == 0f && inputDir.z == 0f)
			{
				num = 0f;
				rb.SetGravityMode(useGravity: false);
			}
			else
			{
				rb.SetGravityMode(useGravity: true);
			}
			float num2 = 2.75f;
			if (slowMode)
			{
				num2 = 1.25f;
			}
			if ((bool)groundProperties)
			{
				num2 *= groundProperties.speedMultiplier;
			}
			targetVel = inputDir * (walkSpeed * Time.deltaTime * num2) + rb.GetGravityDirection() * num;
			Vector3 vector = pushForce;
			if ((bool)groundProperties && groundProperties.push)
			{
				Vector3 vector2 = groundProperties.pushForce;
				if (groundProperties.pushDirectionRelative)
				{
					vector2 = groundProperties.transform.rotation * vector2;
				}
				vector += vector2;
			}
			rb.velocity = Vector3.Lerp(rb.velocity, targetVel + vector, 0.25f * friction);
			return;
		}
		rb.SetGravityMode(useGravity: true);
		Vector3 vector3 = (slowMode ? 1.25f : 2.75f) * Time.deltaTime * walkSpeed * inputDir;
		Vector3 gravityDirection = rb.GetGravityDirection();
		float num3 = Vector3.Dot(gravityDirection, rb.velocity);
		targetVel = vector3 + gravityDirection * num3;
		Vector3 vector4 = Vector3.ProjectOnPlane(rb.velocity, gravityDirection);
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, gravityDirection).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(base.transform.right, gravityDirection).normalized;
		float num4 = Vector3.Dot(vector3, normalized);
		float num5 = Vector3.Dot(vector3, normalized2);
		float num6 = Vector3.Dot(vector4, normalized);
		float num7 = Vector3.Dot(vector4, normalized2);
		if ((num4 > 0f && num6 < num4) || num4 < 0f)
		{
		}
		if ((num5 > 0f && num7 < num5) || num5 < 0f)
		{
		}
		airDirection = Vector3.zero;
		float num8 = 0f;
		num8 = ((windState > 0f) ? 1f : ((!(windState > -0.25f)) ? 0f : Mathf.InverseLerp(0f, -0.25f, windState)));
		float num9 = Mathf.Lerp(1f, 2f, num8);
		if (inputDir.magnitude > 0f)
		{
			float num10 = airAcceleration * Mathf.Lerp(1f, 4f, num8) * Time.fixedDeltaTime * (1f / rb.mass);
			Vector3 vector5 = Vector3.Project(inputDir, base.transform.right);
			float num11 = Vector3.Dot(vector4, vector5.normalized);
			float num12 = num10;
			if (num11 + num12 > 16.5f)
			{
				num12 = Mathf.Max(16.5f - num11, 0f);
			}
			Vector3 vector6 = Vector3.Project(inputDir, base.transform.forward);
			float num13 = Vector3.Dot(vector4, vector6.normalized);
			float num14 = num10;
			if (num13 + num14 > 16.5f)
			{
				num14 = Mathf.Max(16.5f - num13, 0f);
			}
			Vector3 vector7 = vector5 * num12 + vector6 * num14;
			Vector3 vector8 = ((vector4.magnitude > 0.001f) ? vector4.normalized : inputDir);
			Vector3 vector9 = Vector3.ProjectOnPlane(vector7, vector8);
			Vector3 vector10 = Vector3.Project(vector7, vector8);
			float num15 = Mathf.Lerp(0.1f, 0.4f, 1f - num8);
			bool flag = Vector3.Dot(vector10, vector8) > 0f;
			bool flag2 = vector4.magnitude > 16.5f;
			airDirection += vector9 + vector10 * ((flag && flag2) ? num15 : 1f);
			float f = 1f - airDirection.magnitude / (num10 + 0.0001f);
			f = Mathf.Pow(f, 3f);
			float maxLength = airAcceleration * (1f / rb.mass) * Time.fixedDeltaTime * num9 * f;
			Vector3 vector11 = inputDir * vector4.magnitude - vector4;
			Vector3 vector12 = Vector3.Project(vector11, vector8);
			vector11 = Vector3.ProjectOnPlane(vector11, vector8);
			if (Vector3.Dot(vector12, vector4) < 0f)
			{
				vector11 += vector12 * Mathf.Lerp(0.4f, 1f, 1f - num8);
			}
			else
			{
				vector11 += vector12;
			}
			Vector3 vector13 = Vector3.ClampMagnitude(vector11, maxLength);
			airDirection += vector13;
		}
		if (fakeFallRequests > 0 && targetVel.x != 0f && airDirection.x == 0f && targetVel.z != 0f && airDirection.z == 0f)
		{
			Vector3 vector14 = new Vector3(targetVel.x, 0f, targetVel.z);
			Vector3 vector15 = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
			rb.AddForce(vector14.normalized * airAcceleration * 3f);
			rb.AddForce(-vector15.normalized * airAcceleration * 3f);
		}
		else
		{
			rb.AddForce(airDirection, ForceMode.VelocityChange);
		}
		if (fakeFallRequests > 0 && inputDir.magnitude < 0.01f)
		{
			rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.01f - inputDir.magnitude);
		}
	}

	public Vector3 MakeHorizontal(Vector3 direction, PreserveLengthMode preserveLength = PreserveLengthMode.PreserveHorizontal, bool relativeToCamera = true)
	{
		Vector3 planeNormal = (relativeToCamera ? cc.cam.transform.up : Vector3.up);
		Vector3 result = Vector3.ProjectOnPlane(direction, planeNormal);
		switch (preserveLength)
		{
		case PreserveLengthMode.DontPreserve:
			return result;
		case PreserveLengthMode.PreserveHorizontal:
			return result;
		case PreserveLengthMode.PreserveAll:
			return result.normalized * direction.magnitude;
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(preserveLength);
			Vector3 result2 = default(Vector3);
			return result2;
		}
		}
	}

	private void Dodge()
	{
		if (sliding)
		{
			if (hurtInvincibility <= 0f && !levelOver && boostLeft <= 0f)
			{
				base.gameObject.layer = 2;
				exploded = false;
			}
			float num = 1f;
			if (preSlideSpeed > 1f)
			{
				if (preSlideSpeed > 3f)
				{
					preSlideSpeed = 3f;
				}
				num = preSlideSpeed;
				if (gc.onGround && friction != 0f)
				{
					preSlideSpeed -= Time.fixedDeltaTime * preSlideSpeed * friction;
				}
				preSlideDelay = 0f;
			}
			if (modNoDashSlide)
			{
				StopSlide();
				return;
			}
			if ((bool)groundProperties)
			{
				if (!groundProperties.canSlide)
				{
					StopSlide();
					return;
				}
				num *= groundProperties.speedMultiplier;
			}
			float num2 = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
			Vector3 vector = Vector3.ProjectOnPlane(dodgeDirection, rb.GetGravityDirection()) * walkSpeed * Time.deltaTime * 4f * num;
			vector += -rb.GetGravityDirection() * num2;
			if ((bool)groundProperties && groundProperties.push)
			{
				Vector3 vector2 = groundProperties.pushForce;
				if (groundProperties.pushDirectionRelative)
				{
					vector2 = groundProperties.transform.rotation * vector2;
				}
				vector += vector2;
			}
			if (gc.onGround || wcGroup.OnWall())
			{
				CreateSlideScrape();
			}
			if (boostLeft > 0f)
			{
				dashStorage = Mathf.MoveTowards(dashStorage, 0f, Time.fixedDeltaTime);
				if (dashStorage <= 0f)
				{
					boostLeft = 0f;
				}
			}
			inputDir = Vector3.ClampMagnitude(MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>().x * Vector3.ProjectOnPlane(base.transform.right, rb.GetGravityDirection()).normalized, 1f) * 5f;
			if (!MonoSingleton<HookArm>.Instance || !MonoSingleton<HookArm>.Instance.beingPulled)
			{
				rb.velocity = vector + pushForce + inputDir;
			}
			else
			{
				StopSlide();
			}
		}
		else
		{
			if (framesSinceSlide <= 3)
			{
				return;
			}
			float num3 = 0f;
			if (slideEnding)
			{
				num3 = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
			}
			float num4 = 2.75f;
			float num5 = Vector3.Angle(base.transform.up, dodgeDirection);
			if (num5 < 85f || num5 > 95f)
			{
				boost = false;
				return;
			}
			if (gc.onGround)
			{
				dashedFromGround = true;
			}
			targetVel = dodgeDirection * walkSpeed * Time.deltaTime * num4 + -rb.GetGravityDirection() * num3;
			base.gameObject.layer = 15;
			if (slideEnding)
			{
				slideEnding = false;
				if (!gc.onGround || friction == 0f)
				{
					boost = false;
					return;
				}
			}
			if (boostLeft > 0f)
			{
				rb.velocity = targetVel * 3f;
				boostLeft -= 4f;
				return;
			}
			if (!gc.onGround || friction != 0f)
			{
				rb.velocity = targetVel;
			}
			boost = false;
		}
	}

	public void Jump()
	{
		fallTime = 0f;
		lastJump = 0f;
		float num = 1500f;
		if (modNoJump || (bool)groundProperties)
		{
			if (modNoJump || !groundProperties.canJump)
			{
				if (modNoJump || !groundProperties.silentJumpFail)
				{
					aud.clip = jumpSound;
					aud.volume = 0.75f;
					aud.SetPitch(0.25f);
					aud.Play(tracked: true);
				}
				return;
			}
			num *= groundProperties.jumpForceMultiplier;
		}
		jumping = true;
		CancelInvoke("NotJumping");
		Invoke("NotJumping", 0.25f);
		MonoSingleton<PlayerAnimations>.Instance?.Jump();
		falling = true;
		if (quakeJump)
		{
			UnityEngine.Object.Instantiate(quakeJumpSound).GetComponent<AudioSource>().SetPitch(1f + UnityEngine.Random.Range(0f, 0.1f));
		}
		aud.clip = jumpSound;
		if (gc.superJumpChance > 0f || gc.bounceChance > 0f)
		{
			aud.volume = 0.85f;
			aud.SetPitch(2f);
		}
		else
		{
			aud.volume = 0.75f;
			aud.SetPitch(1f);
		}
		aud.Play(tracked: true);
		rb.velocity = Vector3.ProjectOnPlane(rb.velocity, base.transform.up);
		if (sliding)
		{
			if (slowMode)
			{
				rb.AddForce(base.transform.up * jumpPower * num);
			}
			else
			{
				rb.AddForce(base.transform.up * jumpPower * num * 2f);
			}
			StopSlide();
		}
		else if (boost && jumpTimestamp - slideTimestamp > (double)(0.008f * ssjMaxFrames))
		{
			if (enemyStepping)
			{
				UnityEngine.Object.Instantiate(dashJumpSound);
				windState = 0.5f;
			}
			else if (boostCharge >= 100f)
			{
				if (!asscon.majorEnabled || !asscon.infiniteStamina)
				{
					boostCharge -= 100f;
				}
				UnityEngine.Object.Instantiate(dashJumpSound);
			}
			else
			{
				rb.velocity = new Vector3(inputDir.x * walkSpeed * Time.deltaTime * 2.75f, 0f, inputDir.z * walkSpeed * Time.deltaTime * 2.75f);
				UnityEngine.Object.Instantiate(staminaFailSound);
			}
			if (slowMode)
			{
				rb.AddForce(base.transform.up * jumpPower * num * 0.75f);
			}
			else
			{
				rb.AddForce(base.transform.up * jumpPower * num * 1.5f);
			}
		}
		else if (slowMode)
		{
			rb.AddForce(base.transform.up * jumpPower * num * 1.25f);
		}
		else if (gc.superJumpChance > 0f || gc.bounceChance > 0f || gc.extraJumpChance > 0f)
		{
			if (slamForce < 5.5f)
			{
				rb.AddForce(base.transform.up * jumpPower * num * (3f + (slamForce - 1f)));
			}
			else
			{
				rb.AddForce(base.transform.up * jumpPower * num * 12.5f);
			}
		}
		else
		{
			rb.AddForce(base.transform.up * jumpPower * num * 2.6f);
		}
		TrySSJ(dodgeDirection.normalized, 0.5f, (int frame) => 1f / Mathf.Pow(2f, frame - 1));
		jumpCooldown = true;
		CancelInvoke("JumpReady");
		Invoke("JumpReady", 0.2f);
		boost = false;
		gc.bounceChance = 0f;
		gc.heavyFall = false;
		enemyStepping = false;
	}

	private void TrySSJ(Vector3 direction, float speedMultiplier, Func<int, float> speedLossFormula)
	{
		double num = jumpTimestamp - slideTimestamp;
		if (!(num > 0.0))
		{
			return;
		}
		int num2 = (int)(num / 0.00800000037997961);
		if (num2 == 0 || (float)num2 >= ssjMaxFrames)
		{
			return;
		}
		float num3 = speedLossFormula(num2);
		float num4 = speedMultiplier * walkSpeed * 2.75f * 3f * Time.fixedDeltaTime;
		float y = rb.velocity.y;
		float num5 = num3 * num4;
		rb.velocity = velocityAfterSlide + direction * num5;
		rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
		rb.velocity = Mathf.Min(rb.velocity.magnitude, 100f) * rb.velocity.normalized;
		if (!MonoSingleton<PrefsManager>.Instance.GetBool("ssjIndicator"))
		{
			return;
		}
		double num6 = 0.00800000037997961;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		do
		{
			num6 += 0.002;
			string text = (int)(num6 / 0.00800000037997961) switch
			{
				0 => "\"black\"", 
				1 => "#00fff2", 
				2 => "\"yellow\"", 
				3 => "\"red\"", 
				_ => "\"black\"", 
			};
			if (!flag && num6 > num)
			{
				stringBuilder.Append("<color=" + text + ">|</color>");
				flag = true;
			}
			else
			{
				stringBuilder.Append("<color=" + text + ">■</color>");
			}
		}
		while (num6 < (double)ssjMaxFrames * 0.008);
		MonoSingleton<SubtitleController>.Instance.DisplaySubtitle($"<mspace=.65em><size=200%>{stringBuilder}</size></mspace> <voffset=0.35em>+{num5:f0}u/s</voffset>");
	}

	private void WallJump(WallCheck wcInstance)
	{
		jumping = true;
		Invoke("NotJumping", 0.25f);
		MonoSingleton<PlayerAnimations>.Instance?.Jump();
		currentWallJumps++;
		if (gc.heavyFall)
		{
			slamStorage = true;
		}
		if (quakeJump)
		{
			UnityEngine.Object.Instantiate(quakeJumpSound).GetComponent<AudioSource>().SetPitch(1.1f + (float)currentWallJumps * 0.05f);
		}
		aud.clip = jumpSound;
		aud.SetPitch(1f + 0.25f * (float)currentWallJumps);
		aud.volume = 0.75f;
		aud.Play(tracked: true);
		if (currentWallJumps == 3)
		{
			audGround.clip = finalWallJump;
			audGround.volume = 0.75f;
			audGround.Play(tracked: true);
		}
		Vector3 pointOfCollision = wcInstance.GetPointOfCollision();
		wallJumpPos = base.transform.position - pointOfCollision;
		if ((bool)wcInstance.currentCollider && (wcInstance.currentCollider.TryGetComponent<CustomGroundProperties>(out var component) || ((bool)wcInstance.currentCollider.attachedRigidbody && wcInstance.currentCollider.attachedRigidbody.TryGetComponent<CustomGroundProperties>(out component))) && (component.overrideFootsteps || component.overrideSurfaceType))
		{
			MonoSingleton<PlayerFootsteps>.Instance.WallJump(component);
		}
		else
		{
			MonoSingleton<PlayerFootsteps>.Instance.WallJump(pointOfCollision);
		}
		if (NonConvexJumpDebug.Active)
		{
			for (int i = 0; i < 4; i++)
			{
				NonConvexJumpDebug.CreateBall(Color.white, Vector3.Lerp(base.transform.position, pointOfCollision, (float)i / 4f), 0.4f);
			}
		}
		Vector3 gravityDirection = rb.GetGravityDirection();
		if (sliding || InputState.currentTime - slideTimestamp < (double)(ssjMaxFrames * 0.008f))
		{
			Vector3 vector = Vector3.Reflect(dodgeDirection.normalized, wallJumpPos.normalized);
			vector = Vector3.ProjectOnPlane(vector, -gravityDirection.normalized).normalized;
			vector = (dodgeDirection = (vector + wallJumpPos.normalized * 0.35f).normalized);
			rb.velocity = vector.normalized * rb.velocity.magnitude;
			TrySSJ(vector, 0.75f, (int frame) => (ssjMaxFrames - (float)frame + 1f) / ssjMaxFrames);
			float a = Vector3.Dot(rb.velocity, -gravityDirection.normalized);
			rb.velocity = Vector3.ProjectOnPlane(rb.velocity, gravityDirection);
			rb.velocity += -gravityDirection.normalized * Mathf.Max(a, 15f);
			windState = 0.5f;
		}
		else
		{
			boost = false;
			rb.velocity = Vector3.zero;
			Vector3 vector2 = Vector3.ProjectOnPlane(wallJumpPos.normalized, -gravityDirection.normalized);
			vector2 += gravityDirection.normalized * -1f;
			rb.AddForce(vector2 * 2000f * wallJumpPower);
		}
		jumpCooldown = true;
		Invoke("JumpReady", 0.1f);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (sliding)
		{
			ContactPoint[] contacts = other.contacts;
			foreach (ContactPoint contactPoint in contacts)
			{
				Vector3.Angle(Vector3.ProjectOnPlane(dodgeDirection, contactPoint.normal).normalized, dodgeDirection);
				_ = 60f;
			}
		}
	}

	public void LaunchUp(float multiplier)
	{
		Launch(-rb.GetGravityDirection(), multiplier, ignoreMass: true);
	}

	public void Launch(Vector3 direction, float multiplier = 8f, bool ignoreMass = false)
	{
		if (((bool)groundProperties && !groundProperties.launchable) || (direction == Vector3.down && gc.onGround))
		{
			return;
		}
		jumping = true;
		Invoke("NotJumping", 0.5f);
		jumpCooldown = true;
		Invoke("JumpReady", 0.2f);
		boost = false;
		if (gc.heavyFall)
		{
			fallSpeed = 0f;
			gc.heavyFall = false;
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
		}
		if (direction.magnitude > 0f)
		{
			rb.velocity = Vector3.zero;
		}
		rb.AddForce(Vector3.ClampMagnitude(direction, 1000f) * multiplier, (!ignoreMass) ? ForceMode.Impulse : ForceMode.VelocityChange);
	}

	public void LaunchFromPoint(Vector3 position, float strength, float maxDistance = 1f)
	{
		if (!groundProperties || groundProperties.launchable)
		{
			Vector3 vector = (base.transform.position - position).normalized;
			if (position == base.transform.position)
			{
				vector = Vector3.up;
			}
			Vector3 vector2;
			if (jumping)
			{
				vector2 = maxDistance * strength * vector;
				Vector3 vector3 = Vector3.ProjectOnPlane(vector2, rb.GetGravityDirection());
				float a = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
				vector2 = vector3 - rb.GetGravityDirection() * Mathf.Max(a, 0.5f * maxDistance * strength);
			}
			else
			{
				float num = Mathf.Max(0f, maxDistance - Vector3.Distance(base.transform.position, position));
				vector2 = num * strength * vector;
				Vector3 vector4 = Vector3.ProjectOnPlane(vector2, rb.GetGravityDirection());
				float a2 = Vector3.Dot(-rb.GetGravityDirection(), rb.velocity);
				vector2 = vector4 - rb.GetGravityDirection() * Mathf.Max(a2, 0.5f * num * strength);
			}
			Launch(vector2);
		}
	}

	public void LaunchFromPointAtSpeed(Vector3 position, float speed)
	{
		if (!groundProperties || groundProperties.launchable)
		{
			Vector3 vector = (base.transform.position - position).normalized;
			if (position == base.transform.position)
			{
				vector = -rb.GetGravityDirection();
			}
			Vector3 vector2 = vector * speed;
			float num = Vector3.Dot(-rb.GetGravityDirection(), vector2);
			float num2 = 0.5f * speed;
			num2 = Mathf.Max(0f, num2 - num);
			vector2 += -rb.GetGravityDirection() * num2;
			Launch(vector2, 1f, ignoreMass: true);
		}
	}

	public void Slamdown(float strength)
	{
		boost = false;
		if (gc.heavyFall)
		{
			fallSpeed = 0f;
			gc.heavyFall = false;
			if (currentFallParticle != null)
			{
				UnityEngine.Object.Destroy(currentFallParticle);
			}
		}
		rb.velocity = Vector3.zero;
		rb.velocity = new Vector3(0f, 0f - strength, 0f);
	}

	private void JumpReady()
	{
		jumpCooldown = false;
	}

	public void FakeHurt(bool silent = false)
	{
		currentColor.a = 0.25f;
		cc.CameraShake(0.1f);
		if (!silent)
		{
			hurtAud.SetPitch(UnityEngine.Random.Range(0.8f, 1f));
			hurtAud.PlayOneShot(hurtAud.clip, tracked: true);
		}
	}

	public void GetHurt(int damage, bool invincible, float scoreLossMultiplier = 1f, bool explosion = false, bool instablack = false, float hardDamageMultiplier = 0.35f, bool ignoreInvincibility = false)
	{
		if (dead || levelOver || !(!invincible || base.gameObject.layer != 15 || ignoreInvincibility) || damage <= 0)
		{
			return;
		}
		if (explosion)
		{
			exploded = true;
		}
		if (asscon.majorEnabled)
		{
			damage = Mathf.RoundToInt((float)damage * asscon.damageTaken);
		}
		if (Invincibility.Enabled)
		{
			damage = 0;
		}
		if (damage >= 50)
		{
			currentColor.a = 0.8f;
		}
		else
		{
			currentColor.a = 0.5f;
		}
		if (invincible)
		{
			hurtInvincibility = currentColor.a;
			base.gameObject.layer = 15;
		}
		cc.CameraShake(damage / 20);
		hurtAud.SetPitch(UnityEngine.Random.Range(0.8f, 1f));
		hurtAud.PlayOneShot(hurtAud.clip, tracked: true);
		if (hp - damage > 0)
		{
			hp -= damage;
		}
		else
		{
			hp = 0;
		}
		if (invincible && scoreLossMultiplier != 0f && difficulty >= 2 && (!asscon.majorEnabled || !asscon.disableHardDamage) && hp <= 100)
		{
			if (antiHp + (float)damage * hardDamageMultiplier < 99f)
			{
				antiHp += (float)damage * hardDamageMultiplier;
			}
			else
			{
				antiHp = 99f;
			}
			if (antiHpCooldown == 0f)
			{
				antiHpCooldown += 1f;
			}
			if (difficulty >= 3)
			{
				antiHpCooldown += 1f;
			}
			antiHpFlash.Flash(1f);
			antiHpCooldown += damage / 20;
		}
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		if (scoreLossMultiplier > 0.5f)
		{
			shud.RemovePoints(0);
			shud.DescendRank();
		}
		else if (scoreLossMultiplier > 0f)
		{
			shud.RemovePoints(Mathf.RoundToInt(damage));
		}
		StatsManager statsManager = MonoSingleton<StatsManager>.Instance;
		if (damage <= 200)
		{
			statsManager.stylePoints -= Mathf.RoundToInt((float)(damage * 5) * scoreLossMultiplier);
		}
		else
		{
			statsManager.stylePoints -= Mathf.RoundToInt(1000f * scoreLossMultiplier);
		}
		statsManager.tookDamage = true;
		if (hp != 0)
		{
			return;
		}
		if (!endlessMode)
		{
			deathSequence.gameObject.SetActive(value: true);
			if (instablack)
			{
				deathSequence.EndSequence();
			}
			MonoSingleton<TimeController>.Instance.controlPitch = false;
			screenHud.SetActive(value: false);
		}
		else
		{
			GetComponentInChildren<FinalCyberRank>().GameOver();
			CrowdReactions crowdReactions = MonoSingleton<CrowdReactions>.Instance;
			if (crowdReactions != null)
			{
				crowdReactions.React(crowdReactions.aww);
			}
		}
		rb.constraints = RigidbodyConstraints.None;
		rb.AddTorque(Vector3.right * -1f, ForceMode.VelocityChange);
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.juice = 0f;
		}
		cc.enabled = false;
		if (gunc == null)
		{
			gunc = GetComponentInChildren<GunControl>();
		}
		gunc.NoWeapon();
		rb.constraints = RigidbodyConstraints.None;
		dead = true;
		activated = false;
		if (punch == null)
		{
			punch = GetComponentInChildren<FistControl>();
		}
		punch.NoFist();
	}

	public void ForceAntiHP(float amount, bool silent = false, bool dontOverwriteHp = false, bool addToCooldown = true, bool stopInstaHeal = false)
	{
		if ((asscon.majorEnabled && asscon.disableHardDamage) || hp > 100)
		{
			return;
		}
		amount = Mathf.Clamp(amount, 0f, 99f);
		float num = antiHp;
		if ((float)hp > 100f - amount)
		{
			if (dontOverwriteHp)
			{
				amount = 100 - hp;
			}
			else
			{
				hp = Mathf.RoundToInt(100f - amount);
			}
		}
		if (MonoSingleton<StyleHUD>.Instance.rankIndex < 7)
		{
			antiHpFlash.Flash(1f);
			if (amount > antiHp)
			{
				FakeHurt(silent);
			}
		}
		antiHp = amount;
		cantInstaHeal = stopInstaHeal;
		if (addToCooldown)
		{
			if (antiHpCooldown < 1f || (difficulty >= 3 && antiHpCooldown < 2f))
			{
				antiHpCooldown = ((difficulty < 3) ? 1 : 2);
			}
			if (amount - num < 50f)
			{
				antiHpCooldown += (amount - num) / 20f;
			}
			else
			{
				antiHpCooldown += 2.5f;
			}
		}
		else if (antiHpCooldown <= 1f)
		{
			antiHpCooldown = 1f;
		}
	}

	public void ForceAddAntiHP(float amount, bool silent = false, bool dontOverwriteHp = false, bool addToCooldown = true, bool stopInstaHeal = false)
	{
		ForceAntiHP(antiHp + amount, silent, dontOverwriteHp, addToCooldown, stopInstaHeal);
	}

	public void GetHealth(int health, bool silent, bool fromExplosion = false, bool bloodsplatter = true)
	{
		if (dead || (exploded && fromExplosion))
		{
			return;
		}
		float num = health;
		float num2 = 100f;
		if (difficulty == 0 || (difficulty == 1 && sameCheckpointRestarts > 2))
		{
			num2 = 200f;
		}
		if (num < 1f)
		{
			num = 1f;
		}
		if ((float)hp <= num2)
		{
			if ((float)hp + num < num2 - (float)Mathf.RoundToInt(antiHp))
			{
				hp += Mathf.RoundToInt(num);
			}
			else if ((float)hp != num2 - (float)Mathf.RoundToInt(antiHp))
			{
				hp = Mathf.RoundToInt(num2) - Mathf.RoundToInt(antiHp);
			}
			hpFlash.Flash(1f);
			if (!silent && health > 5)
			{
				if ((UnityEngine.Object)(object)greenHpAud == null)
				{
					greenHpAud = hpFlash.GetComponent<AudioSource>();
				}
				greenHpAud.Play(tracked: true);
			}
		}
		if (!silent && health > 5 && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled"))
		{
			UnityEngine.Object.Instantiate(scrnBlood, ((Component)(object)fullHud).transform);
		}
	}

	public void FullHeal(bool silent = false)
	{
		GetHealth(200, silent, fromExplosion: false, bloodsplatter: false);
	}

	public void Parry(EnemyIdentifier eid = null, string customParryText = "")
	{
		MonoSingleton<TimeController>.Instance.ParryFlash();
		exploded = false;
		GetHealth(999, silent: false);
		FullStamina();
		if (shud == null)
		{
			shud = MonoSingleton<StyleHUD>.Instance;
		}
		if (!eid || !eid.blessed)
		{
			shud.AddPoints(100, (customParryText != "") ? ("<color=green>" + customParryText + "</color>") : "ultrakill.parry");
		}
	}

	public void SuperCharge()
	{
		GetHealth(100, silent: true);
		hp = 200;
	}

	public void Respawn()
	{
		MonoSingleton<CameraController>.Instance.cam.useOcclusionCulling = true;
		if (sliding)
		{
			StopSlide();
		}
		sameCheckpointRestarts++;
		if (difficulty == 0)
		{
			hp = 200;
		}
		else
		{
			hp = 100;
		}
		boostCharge = 299f;
		antiHp = 0f;
		antiHpCooldown = 0f;
		rb.constraints = defaultRBConstraints;
		activated = true;
		deathSequence.gameObject.SetActive(value: false);
		cc.enabled = true;
		if ((bool)MonoSingleton<PowerUpMeter>.Instance)
		{
			MonoSingleton<PowerUpMeter>.Instance.juice = 0f;
		}
		StatsManager? statsManager = MonoSingleton<StatsManager>.Instance;
		statsManager.stylePoints = statsManager.stylePoints / 3 * 2;
		if (gunc == null)
		{
			gunc = GetComponentInChildren<GunControl>();
		}
		gunc.YesWeapon();
		screenHud.SetActive(value: true);
		dead = false;
		MonoSingleton<TimeController>.Instance.controlPitch = true;
		MonoSingleton<HookArm>.Instance?.Cancel();
		if (punch == null)
		{
			punch = GetComponentInChildren<FistControl>();
		}
		punch.activated = true;
		punch.YesFist();
		slowMode = false;
		MonoSingleton<WeaponCharges>.Instance.MaxCharges();
		if (MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketLauncher.UnfreezeRockets();
		}
	}

	public void ResetHardDamage()
	{
		antiHp = 0f;
		antiHpCooldown = 0f;
	}

	private void NotJumping()
	{
		jumping = false;
	}

	public void EnemyStepResets()
	{
		currentWallJumps = 0;
		rocketJumps = 0;
		hammerJumps = 0;
		clingFade = 0f;
		rocketRides = 0;
	}

	public void LandingImpact()
	{
		UnityEngine.Object.Instantiate(impactDust, gc.transform.position, Quaternion.identity).transform.forward = base.transform.up;
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.FallImpact);
		MonoSingleton<PlayerFootsteps>.Instance.Footstep(1f, force: true);
		MonoSingleton<PlayerFootsteps>.Instance.Footstep(1f, force: true, 0.05f);
		MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(base.transform.position, base.transform.up * -1f, 5f, Mathf.RoundToInt(Mathf.Lerp(3f, 5f, (Mathf.Abs(fallSpeed) - 50f) / 50f)));
	}

	private void StartSlide()
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (currentSlideParticle != null)
		{
			UnityEngine.Object.Destroy(currentSlideParticle);
		}
		if (slideScrape != null)
		{
			DetachSlideScrape();
		}
		if (modNoDashSlide)
		{
			StopSlide();
		}
		else
		{
			if ((bool)MonoSingleton<HookArm>.Instance && MonoSingleton<HookArm>.Instance.beingPulled)
			{
				return;
			}
			if ((bool)groundProperties && !groundProperties.canSlide)
			{
				if (!groundProperties.silentSlideFail)
				{
					StopSlide();
				}
				return;
			}
			if (!crouching)
			{
				float height = playerCollider.height;
				playerCollider.height = 1.25f;
				float num = height - 1.25f;
				base.transform.position += base.transform.up * -0.5f * num;
				gc.SetLocalPosition(groundCheckPos + Vector3.up * 1.125f);
			}
			slideSafety = 1f;
			sliding = true;
			boost = true;
			dodgeDirection = inputDir;
			if (dodgeDirection == Vector3.zero)
			{
				dodgeDirection = base.transform.forward;
			}
			currentSlideParticle = UnityEngine.Object.Instantiate(slideParticle, base.transform.position + dodgeDirection * 10f, Quaternion.LookRotation(-dodgeDirection));
			slideTrail = currentSlideParticle.GetComponent<ParticleSystem>().trails;
			((TrailModule)(ref slideTrail)).colorOverLifetime = ((boostLeft > 0f) ? invincibleSlideGradient : normalSlideGradient);
			CreateSlideScrape(ignorePrevious: true);
			if (dodgeDirection == base.transform.forward)
			{
				cc.dodgeDirection = 0;
			}
			else if (dodgeDirection == base.transform.forward * -1f)
			{
				cc.dodgeDirection = 1;
			}
			else
			{
				cc.dodgeDirection = 2;
			}
			MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.Slide);
		}
	}

	private void CreateSlideScrape(bool ignorePrevious = false, bool frictionlessVersion = false)
	{
		bool flag = (bool)groundProperties && groundProperties.overrideSurfaceType;
		SceneHelper.HitSurfaceData hitSurfaceData = default(SceneHelper.HitSurfaceData);
		WallCheck wallCheck;
		if (flag)
		{
			hitSurfaceData.surfaceType = groundProperties.surfaceType;
			hitSurfaceData.particleColor = groundProperties.particleColor;
		}
		else if (!gc.onGround && wcGroup.TryGetActiveInstance(out wallCheck))
		{
			Vector3 normalized = (wallCheck.poc - wallCheck.transform.position).normalized;
			if (MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(wallCheck.transform.position, normalized, 3f, out hitSurfaceData))
			{
				flag = true;
			}
		}
		else if (MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position, rb.GetGravityDirection(), 3f, out hitSurfaceData))
		{
			flag = true;
		}
		if (flag)
		{
			if (!MonoSingleton<DefaultReferenceManager>.Instance.footstepSet.TryGetSlideParticle(hitSurfaceData.surfaceType, out var particle))
			{
				hitSurfaceData.surfaceType = SurfaceType.Generic;
			}
			if (!ignorePrevious && hitSurfaceData.surfaceType == (frictionlessVersion ? currentFricSlideSurfaceType : currentSlideSurfaceType))
			{
				return;
			}
			if (frictionlessVersion)
			{
				if (currentFrictionlessSlideParticle != null)
				{
					UnityEngine.Object.Destroy(currentFrictionlessSlideParticle);
				}
				currentFrictionlessSlideParticle = UnityEngine.Object.Instantiate(particle, base.transform.position, Quaternion.identity);
				SetFrictionlessSlideValues();
				currentFricSlideSurfaceType = hitSurfaceData.surfaceType;
			}
			else
			{
				DetachScrape(slideScrape);
				slideScrape = UnityEngine.Object.Instantiate(particle, base.transform.position + dodgeDirection * 2f, Quaternion.LookRotation(-dodgeDirection));
				MonoSingleton<SceneHelper>.Instance.SetParticlesColors(slideScrape, ref hitSurfaceData);
				currentSlideSurfaceType = hitSurfaceData.surfaceType;
			}
		}
		else
		{
			if (!((frictionlessVersion ? currentFricSlideSurfaceType : currentSlideSurfaceType) != SurfaceType.Generic || ignorePrevious))
			{
				return;
			}
			MonoSingleton<DefaultReferenceManager>.Instance.footstepSet.TryGetSlideParticle(SurfaceType.Generic, out var particle2);
			if (frictionlessVersion)
			{
				if (currentFrictionlessSlideParticle != null)
				{
					UnityEngine.Object.Destroy(currentFrictionlessSlideParticle);
				}
				currentFrictionlessSlideParticle = UnityEngine.Object.Instantiate(particle2, base.transform.position, Quaternion.identity);
				SetFrictionlessSlideValues();
				currentFricSlideSurfaceType = SurfaceType.Generic;
			}
			else
			{
				DetachScrape(slideScrape);
				slideScrape = UnityEngine.Object.Instantiate(particle2, base.transform.position + dodgeDirection * 2f, Quaternion.LookRotation(-dodgeDirection));
				currentSlideSurfaceType = SurfaceType.Generic;
			}
		}
	}

	private void SetFrictionlessSlideValues()
	{
		fricSlideAuds = currentFrictionlessSlideParticle.GetComponentsInChildren<AudioSource>(includeInactive: true);
		fricSlideAudVols = new float[fricSlideAuds.Length];
		fricSlideAudPitches = new float[fricSlideAuds.Length];
		for (int i = 0; i < fricSlideAuds.Length; i++)
		{
			fricSlideAudVols[i] = fricSlideAuds[i].volume;
			fricSlideAudPitches[i] = fricSlideAuds[i].GetPitch();
		}
	}

	private void CreateWallScrape(Vector3 position, bool ignorePrevious = false)
	{
		if (MonoSingleton<SceneHelper>.Instance.TryGetSurfaceData(base.transform.position, position - base.transform.position, 5f, out var hitSurfaceData))
		{
			SurfaceType surfaceType = hitSurfaceData.surfaceType;
			if (!MonoSingleton<DefaultReferenceManager>.Instance.footstepSet.TryGetWallScrapeParticle(surfaceType, out var particle))
			{
				surfaceType = SurfaceType.Generic;
			}
			if (ignorePrevious || currentScrapeSurfaceType != surfaceType)
			{
				DetachScrape(wallScrape);
				wallScrape = UnityEngine.Object.Instantiate(particle, position, Quaternion.identity);
				MonoSingleton<SceneHelper>.Instance.SetParticlesColors(wallScrape, ref hitSurfaceData);
				currentScrapeSurfaceType = surfaceType;
			}
			else
			{
				wallScrape.transform.position = position;
			}
		}
		else if (currentScrapeSurfaceType != SurfaceType.Generic || ignorePrevious)
		{
			DetachScrape(wallScrape);
			MonoSingleton<DefaultReferenceManager>.Instance.footstepSet.TryGetWallScrapeParticle(SurfaceType.Generic, out var particle2);
			wallScrape = UnityEngine.Object.Instantiate(particle2, position, Quaternion.identity);
			currentScrapeSurfaceType = SurfaceType.Generic;
		}
		else
		{
			wallScrape.transform.position = position;
		}
	}

	private void CheckForGasoline()
	{
		Vector3Int vector3Int = StainVoxelManager.WorldToVoxelPosition(base.transform.position + rb.GetGravityDirection() * 1.8333334f);
		if (!lastCheckedGasolineVoxel.HasValue || lastCheckedGasolineVoxel.Value != vector3Int)
		{
			lastCheckedGasolineVoxel = vector3Int;
			modForcedFrictionMultip = ((!MonoSingleton<StainVoxelManager>.Instance.HasProxiesAt(vector3Int, 3, VoxelCheckingShape.VerticalBox, ProxySearchMode.AnyFloor)) ? 1 : 0);
			onGasoline = modForcedFrictionMultip == 0f;
		}
		if (((Component)(object)oilSlideEffect).gameObject.activeSelf != (modForcedFrictionMultip == 0f))
		{
			((Component)(object)oilSlideEffect).gameObject.SetActive(modForcedFrictionMultip == 0f);
		}
		if (modForcedFrictionMultip == 0f)
		{
			float num = Mathf.Min(35f, rb.velocity.magnitude) / 35f;
			oilSlideEffect.volume = Mathf.Lerp(0f, 0.85f, num);
			((Component)(object)oilSlideEffect).transform.localScale = Vector3.one * num;
			oilSlideEffect.SetPitch(Mathf.Lerp(1.75f, 2.75f, num));
		}
	}

	public void StopSlide()
	{
		if (currentSlideParticle != null)
		{
			UnityEngine.Object.Destroy(currentSlideParticle);
		}
		if (slideScrape != null)
		{
			DetachSlideScrape();
		}
		UnityEngine.Object.Instantiate(slideStopSound);
		cc.ResetToDefaultPos();
		sliding = false;
		slideEnding = true;
		if (slideLength > longestSlide)
		{
			longestSlide = slideLength;
		}
		slideLength = 0f;
		if (!gc.heavyFall)
		{
			Physics.IgnoreLayerCollision(2, 12, ignore: false);
		}
		framesSinceSlide = 0;
		velocityAfterSlide = dodgeDirection.normalized * Mathf.Max(24f, preDashSpeed.magnitude);
		MonoSingleton<RumbleManager>.Instance.StopVibration(RumbleProperties.Slide);
	}

	private void DetachSlideScrape()
	{
		DetachScrape(slideScrape);
		slideScrape = null;
	}

	private void DetachWallScrape()
	{
		DetachScrape(wallScrape);
		wallScrape = null;
	}

	private void DetachScrape(GameObject scrape)
	{
		if (!(scrape == null))
		{
			ParticleSystem[] componentsInChildren = scrape.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop();
			}
			AudioSource[] componentsInChildren2 = scrape.GetComponentsInChildren<AudioSource>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Stop();
			}
			scrape.AddComponent<RemoveOnTime>().time = 10f;
		}
	}

	public void EmptyStamina()
	{
		boostCharge = 0f;
	}

	public void FullStamina()
	{
		boostCharge = 300f;
	}

	public void DeactivatePlayer()
	{
		activated = false;
		MonoSingleton<CameraController>.Instance.activated = false;
		MonoSingleton<GunControl>.Instance.NoWeapon();
		MonoSingleton<FistControl>.Instance.NoFist();
		if (sliding)
		{
			StopSlide();
		}
	}

	public void ActivatePlayer()
	{
		activated = true;
		MonoSingleton<CameraController>.Instance.activated = true;
		MonoSingleton<GunControl>.Instance.YesWeapon();
		MonoSingleton<FistControl>.Instance.YesFist();
	}

	public void StopMovement()
	{
		if (sliding)
		{
			StopSlide();
		}
		if (boost)
		{
			boostLeft = 0f;
			boost = false;
		}
		inputDir = Vector3.zero;
		rb.velocity = Vector3.zero;
	}

	public void DeactivateMovement()
	{
		activated = false;
		inputDir = Vector3.zero;
	}

	public void ReactivateMovement()
	{
		activated = true;
		punch.YesFist();
	}

	public void LockMovementAxes()
	{
		rb.constraints = (RigidbodyConstraints)122;
	}

	public void UnlockMovementAxes()
	{
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}

	public void SwitchGravity(Vector3 direction, bool instant = false, bool transformCamera = true)
	{
		if (!slamStorage)
		{
			gc.heavyFall = false;
		}
		rb.SetCustomGravity(direction);
		rb.SetCustomGravityMode(useCustomGravity: true);
		if (instant)
		{
			cc.gravityRotation = Quaternion.identity;
			cc.gravityVec = direction.normalized;
			cc.ApplyRotations();
		}
		if (transformCamera)
		{
			cc.Transform(Matrix4x4.identity, direction);
		}
	}

	public void ResetGravity(bool instant = false)
	{
		if (!slamStorage)
		{
			gc.heavyFall = false;
		}
		rb.SetCustomGravityMode(useCustomGravity: false);
		cc.Transform(Matrix4x4.identity, Physics.gravity);
		if (instant)
		{
			cc.gravityVec = rb.GetGravityDirection();
		}
	}

	public void CheckGravityVolumes(bool resetIfNone = false)
	{
		if (gravityVolumes.Count <= 0)
		{
			return;
		}
		int num = gravityVolumes.Count - 1;
		while (num >= 0)
		{
			if (gravityVolumes[num] == null || !gravityVolumes[num].enabled)
			{
				gravityVolumes.RemoveAt(num);
				num--;
				continue;
			}
			SwitchGravity(gravityVolumes[num].GravityVector);
			break;
		}
	}

	private static Quaternion GetSimulatedCameraWorldRotation(Quaternion inputRotation, Transform playerTransform, Vector3 gravityDir)
	{
		Vector3 eulerAngles = inputRotation.eulerAngles;
		Quaternion.FromToRotation(Vector3.down, gravityDir);
		float num = 0f - eulerAngles.x;
		float y = eulerAngles.y;
		float z = eulerAngles.z;
		float f = Mathf.DeltaAngle(0f, num);
		if (Mathf.Abs(f) > 90f)
		{
			num = 90f * Mathf.Sign(f);
		}
		Quaternion quaternion = Quaternion.AngleAxis(y, -gravityDir);
		Quaternion quaternion2 = ((playerTransform.parent != null) ? playerTransform.parent.rotation : Quaternion.identity) * quaternion;
		Quaternion quaternion3 = Quaternion.AngleAxis(0f - num, Vector3.right) * Quaternion.AngleAxis(z, Vector3.forward);
		return quaternion2 * quaternion3;
	}

	public bool? OnTravel(PortalTravelDetails details)
	{
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		lastTraversalFrame = Time.frameCount;
		Vector3 up = base.transform.up;
		PortalScene scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		Vector3 position = cc.cam.transform.position;
		Matrix4x4 enterToExit = details.enterToExit;
		PortalHandle enterHandle = details.enterHandle;
		PortalHandle exitHandle = details.exitHandle;
		Portal portalObject = scene.GetPortalObject(details.enterHandle);
		Portal portalObject2 = scene.GetPortalObject(details.exitHandle);
		float minimumPassThroughSpeed = scene.GetMinimumPassThroughSpeed(portalObject2, exitHandle.side);
		if (minimumPassThroughSpeed != 0f)
		{
			Vector3 vector = ((minimumPassThroughSpeed > 0f) ? portalObject.GetTransform(enterHandle.side).forwardManaged : portalObject2.GetTransform(exitHandle.side).forwardManaged);
			minimumPassThroughSpeed = Mathf.Abs(minimumPassThroughSpeed);
			float num = Vector3.Dot(rb.velocity, vector);
			if (num < minimumPassThroughSpeed)
			{
				float num2 = minimumPassThroughSpeed - num;
				Vector3 vector2 = vector * num2;
				rb.velocity += vector2;
			}
		}
		if ((enterHandle.side == PortalSide.Enter) ? portalObject.usePerceivedGravityOnEnter : portalObject.usePerceivedGravityOnExit)
		{
			bool num3 = ((enterHandle.side == PortalSide.Enter) ? portalObject.forceOrthogonalGravityOnEnter : portalObject.forceOrthogonalGravityOnExit);
			Vector3 vector3 = enterToExit.MultiplyVector(rb.GetGravityVector());
			float item = Vector3.Dot(vector3, Vector3.up);
			float num4 = Vector3.Dot(vector3, Vector3.forward);
			float num5 = Vector3.Dot(vector3, Vector3.right);
			if (num3)
			{
				(float, Vector3) tuple = (item, Vector3.up);
				if (math.abs(tuple.Item1) < math.abs(num4))
				{
					tuple = (num4, Vector3.forward);
				}
				if (math.abs(tuple.Item1) < math.abs(num5))
				{
					tuple = (num5, Vector3.right);
				}
				vector3 = tuple.Item2 * Physics.gravity.magnitude * Mathf.Sign(tuple.Item1);
			}
			SwitchGravity(vector3, instant: false, transformCamera: false);
		}
		else
		{
			GravityVolume gravityVolume = ((exitHandle.side == PortalSide.Enter) ? portalObject2.enterGravityVolume : portalObject2.exitGravityVolume);
			if ((bool)gravityVolume)
			{
				SwitchGravity(gravityVolume.GravityVector, instant: false, transformCamera: false);
			}
		}
		Quaternion? proposedRotation = null;
		bool num6 = cc.transform.localRotation.x < 0f;
		bool flag = cc.transform.localRotation.w < 0f;
		if (num6 == flag)
		{
			Quaternion localRotation = cc.transform.localRotation;
			localRotation.x *= -1f;
			localRotation.Normalize();
			Quaternion quaternion = (cc.transform.parent ? cc.transform.parent.rotation : Quaternion.identity) * localRotation;
			Vector3 normalized = enterToExit.MultiplyVector(quaternion * Vector3.forward).normalized;
			Vector3 normalized2 = enterToExit.MultiplyVector(quaternion * Vector3.up).normalized;
			proposedRotation = Quaternion.LookRotation(normalized, normalized2);
		}
		Vector3 vector4 = enterToExit.MultiplyVector(rb.velocity).normalized * rb.velocity.magnitude;
		Vector3 vector5 = Vector3.ProjectOnPlane(rb.velocity, base.transform.up);
		Vector3 vector6 = Vector3.ProjectOnPlane(dodgeDirection, base.transform.up);
		inputDir = enterToExit.MultiplyVector(inputDir).normalized * inputDir.magnitude;
		dodgeDirection = enterToExit.MultiplyVector(dodgeDirection).normalized;
		postTeleportRight = enterToExit.MultiplyVector(cc.cam.transform.right);
		postTeleportForward = enterToExit.MultiplyVector(base.transform.forward);
		remainingTeleportFrames = 2;
		cc.Transform(enterToExit, rb.GetGravityDirection(), proposedRotation);
		Vector3 vector7 = enterToExit.MultiplyPoint3x4(position);
		Vector3 vector8 = enterToExit.MultiplyPoint3x4(details.intersection);
		Vector3 normalized3 = (vector7 - vector8).normalized;
		float num7 = 0.1f;
		if (TryGetComponent<Collider>(out var component))
		{
			num7 = component.bounds.extents.x;
		}
		Vector3 end = vector7 + normalized3 * num7;
		if (Physics.Linecast(vector8, end, out var hitInfo, LayerMaskDefaults.Get(LMD.Environment)))
		{
			float num8 = hitInfo.distance - num7;
			if (!(num8 > 0f))
			{
				return false;
			}
			vector7 = vector8 + normalized3 * num8;
		}
		Vector3 vector9 = vector7 - base.transform.localToWorldMatrix.MultiplyVector(cc.cam.transform.localPosition);
		rb.velocity = vector4;
		rb.position = vector9;
		base.transform.position = vector9;
		vcb.PerformClippingCheck(heavyFall: false);
		base.transform.position = rb.position;
		_ = enterToExit.rotation * cc.transform.rotation;
		gc.Update();
		if (gc.onGround)
		{
			Vector3 vector10 = -rb.GetGravityDirection();
			float num9 = playerCollider.height / 2f - playerCollider.center.y;
			if (Physics.Raycast(vector9, -vector10, out var hitInfo2, num9 + 1f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				NativePortalTransform nativePortalTransform = portalObject.GetTransform(enterHandle.side);
				NativePortalTransform nativePortalTransform2 = portalObject2.GetTransform(exitHandle.side);
				Vector3 forward = Vector3.ProjectOnPlane(float3.op_Implicit(nativePortalTransform.forward), up);
				Vector3 forward2 = Vector3.ProjectOnPlane(float3.op_Implicit(nativePortalTransform2.back), hitInfo2.normal);
				if (forward.sqrMagnitude > 0.01f && forward2.sqrMagnitude > 0.01f)
				{
					forward.Normalize();
					forward2.Normalize();
					Quaternion quaternion2 = Quaternion.LookRotation(forward2, hitInfo2.normal) * Quaternion.Inverse(Quaternion.LookRotation(forward, up));
					Vector3 vector11 = quaternion2 * vector5;
					Vector3 vector12 = Vector3.Project(vector4, vector10);
					rb.velocity = vector11 + vector12;
					dodgeDirection = quaternion2 * vector6;
				}
			}
		}
		if (!slamStorage)
		{
			gc.heavyFall = false;
		}
		portalObject2.SetUpdatedSkyFog(enterHandle.side);
		portalAwareCollider.IgnorePortalHandle(exitHandle);
		onPortalTraversed?.Invoke(details);
		if ((bool)ridingRocket)
		{
			ridingRocket.OnRiderTraversal(details);
		}
		return true;
	}

	public void OnTeleportBlocked(PortalTravelDetails details)
	{
		PortalHandle enterHandle = details.enterHandle;
		Vector3 positionInFront = PortalUtils.GetPortalObject(enterHandle).GetTransform(enterHandle.side).GetPositionInFront(details.intersection, 0.05f);
		positionInFront -= cc.cam.transform.localPosition;
		base.transform.position = positionInFront;
		rb.position = positionInFront;
		rb.velocity = Vector3.zero;
		if (sliding)
		{
			StopSlide();
		}
	}

	public void SetData(ref TargetData data)
	{
		data.position = cachedPos;
		data.headPosition = cachedHeadPos;
		data.realPosition = cachedPos;
		data.realHeadPosition = cachedHeadPos;
		data.rotation = cachedRot;
		data.velocity = cachedVel;
	}

	public void UpdateCachedTransformData()
	{
		cachedPos = rb.position;
		cachedHeadPos = MonoSingleton<CameraController>.Instance.transform.position;
		cachedRot = rb.rotation;
		cachedVel = rb.velocity;
	}
}
