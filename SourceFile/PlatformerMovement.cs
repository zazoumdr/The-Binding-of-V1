using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.InputSystem;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlatformerMovement : MonoSingleton<PlatformerMovement>, ITarget
{
	public Transform platformerCamera;

	public Vector3 cameraTarget = new Vector3(0f, 7f, -5.5f);

	public Vector3 defaultCameraTarget = new Vector3(0f, 9f, -6.5f);

	public Vector3 defaultFreeCameraTarget = new Vector3(0f, 7f, -5.5f);

	public Vector3 cameraRotation = new Vector3(20f, 0f, 0f);

	private Vector3 defaultCameraRotation = new Vector3(20f, 0f, 0f);

	[HideInInspector]
	public List<CameraTargetInfo> cameraTargets = new List<CameraTargetInfo>();

	private bool cameraTrack = true;

	private bool cameraLowered;

	public bool freeCamera;

	[HideInInspector]
	public float rotationY;

	[HideInInspector]
	public float rotationX;

	private float lastYPos;

	private float currentYPos;

	private bool beenOverYPosMax;

	private float yPosDifferential;

	public GroundCheck groundCheck;

	[SerializeField]
	private GroundCheck slopeCheck;

	public Transform playerModel;

	[HideInInspector]
	public Rigidbody rb;

	private AudioSource aud;

	private CapsuleCollider playerCollider;

	private Animator anim;

	[SerializeField]
	private AudioClip jumpSound;

	[SerializeField]
	private AudioClip dodgeSound;

	[SerializeField]
	private AudioClip bounceSound;

	[HideInInspector]
	public bool activated = true;

	private Vector3 movementDirection;

	private Vector3 movementDirection2;

	private Vector3 airDirection;

	private Vector3 dodgeDirection;

	private float walkSpeed = 600f;

	private float jumpPower = 80f;

	private bool airFrictionless;

	private bool boost;

	private float boostCharge = 300f;

	private float boostLeft;

	[SerializeField]
	private GameObject staminaFailSound;

	[SerializeField]
	private GameObject dodgeParticle;

	[SerializeField]
	private GameObject dashJumpSound;

	private MaterialPropertyBlock block;

	private SkinnedMeshRenderer smr;

	[HideInInspector]
	public bool sliding;

	private bool crouching;

	private bool slideEnding;

	private float preSlideSpeed;

	private float preSlideDelay;

	private float slideSafety;

	private float slideLength;

	[SerializeField]
	private GameObject slideStopSound;

	[SerializeField]
	private GameObject slideEffect;

	[SerializeField]
	private GameObject slideScrape;

	private GameObject currentSlideEffect;

	private GameObject currentSlideScrape;

	[SerializeField]
	private GameObject fallParticle;

	private GameObject currentFallParticle;

	private bool jumping;

	private bool inSpecialJump;

	private bool jumpCooldown;

	[HideInInspector]
	public CustomGroundProperties groundProperties;

	public Transform jumpShadow;

	private bool falling;

	private float fallSpeed;

	private float fallTime;

	private bool aboutToSlam;

	private TimeSince slamWindUp;

	[SerializeField]
	private AudioSource slamReadySound;

	private bool slamming;

	public float slamForce;

	[SerializeField]
	private GameObject impactDust;

	private bool spinning;

	private float spinJuice;

	private Vector3 spinDirection;

	private float spinSpeed;

	private float spinCooldown;

	public Transform holder;

	private int difficulty;

	[SerializeField]
	private GameObject spinZone;

	[SerializeField]
	private GameObject coinGet;

	private float coinTimer;

	private float coinPitch;

	private int queuedCoins;

	private float coinEffectTimer;

	public int extraHits;

	private bool invincible;

	private float blinkTimer;

	public GameObject[] protectors;

	private float superTimer;

	public GameObject protectorGet;

	public GameObject protectorLose;

	public GameObject protectorOof;

	private InputBinding rbSlide;

	private InputBinding dpadMove;

	[Header("Death Stuff")]
	[SerializeField]
	private Material burnMaterial;

	[SerializeField]
	private GameObject defaultBurnEffect;

	[SerializeField]
	private GameObject ashParticle;

	[SerializeField]
	private GameObject ashSound;

	private GameObject currentCorpse;

	[SerializeField]
	private GameObject fallSound;

	[HideInInspector]
	public bool dead;

	private Vector3 cachedPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

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

	public Transform Transform => base.transform;

	public Vector3 Position => cachedPos;

	public Vector3 HeadPosition => cachedPos;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		rbSlide = new InputBinding("<Gamepad>/rightShoulder", (string)null, "Gamepad", (string)null, (string)null, "rbSlide");
		dpadMove = new InputBinding("<Gamepad>/dpad", (string)null, "Gamepad", (string)null, (string)null, "dpadMove");
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		aud = GetComponent<AudioSource>();
		playerCollider = GetComponent<CapsuleCollider>();
		anim = GetComponent<Animator>();
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		block = new MaterialPropertyBlock();
		UpdateWings();
		base.transform.position = MonoSingleton<NewMovement>.Instance.gc.transform.position;
		rb.velocity = MonoSingleton<NewMovement>.Instance.rb.velocity;
		currentYPos = base.transform.position.y;
		if (MonoSingleton<PlayerTracker>.Instance.pmov == null)
		{
			MonoSingleton<PlayerTracker>.Instance.currentPlatformerPlayerPrefab = base.transform.parent.gameObject;
			MonoSingleton<PlayerTracker>.Instance.pmov = this;
			MonoSingleton<PlayerTracker>.Instance.ChangeToPlatformer();
		}
	}

	public void CheckItem()
	{
		if ((bool)MonoSingleton<FistControl>.Instance && (bool)MonoSingleton<FistControl>.Instance.heldObject)
		{
			MonoSingleton<FistControl>.Instance.heldObject.transform.SetParent(holder, worldPositionStays: true);
			MonoSingleton<FistControl>.Instance.ResetHeldItemPosition();
			Transform[] componentsInChildren = MonoSingleton<FistControl>.Instance.heldObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = 22;
			}
		}
	}

	private void OnEnable()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		PlayerInput inputSource = MonoSingleton<InputManager>.Instance.InputSource;
		InputActionRebindingExtensions.ApplyBindingOverride(inputSource.Jump.Action, "<Gamepad>/buttonSouth", "Gamepad", (string)null);
		InputActionRebindingExtensions.ApplyBindingOverride(inputSource.Fire1.Action, "<Gamepad>/buttonWest", "Gamepad", (string)null);
		InputActionRebindingExtensions.ApplyBindingOverride(inputSource.Dodge.Action, "<Gamepad>/buttonNorth", "Gamepad", (string)null);
		InputActionRebindingExtensions.ApplyBindingOverride(inputSource.Slide.Action, "<Gamepad>/buttonEast", "Gamepad", (string)null);
		InputActionSetupExtensions.AddBinding(inputSource.Slide.Action, rbSlide);
		InputActionSetupExtensions.AddBinding(inputSource.Move.Action, dpadMove);
		slideLength = 0f;
	}

	private void OnDisable()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (base.gameObject.scene.isLoaded)
		{
			PlayerInput inputSource = MonoSingleton<InputManager>.Instance.InputSource;
			InputActionRebindingExtensions.RemoveAllBindingOverrides(inputSource.Jump.Action);
			InputActionRebindingExtensions.RemoveAllBindingOverrides(inputSource.Fire1.Action);
			InputActionRebindingExtensions.RemoveAllBindingOverrides(inputSource.Dodge.Action);
			InputActionRebindingExtensions.RemoveAllBindingOverrides(inputSource.Slide.Action);
			BindingSyntax val = InputActionSetupExtensions.ChangeBinding(inputSource.Slide.Action, rbSlide);
			((BindingSyntax)(ref val)).Erase();
			val = InputActionSetupExtensions.ChangeBinding(inputSource.Move.Action, dpadMove);
			((BindingSyntax)(ref val)).Erase();
			cameraTargets.Clear();
			if (sliding)
			{
				StopSlide();
			}
		}
	}

	private void UpdateWings()
	{
		float value = 0f;
		Color value2 = Color.red;
		if (boostCharge >= 300f)
		{
			value2 = new Color(1f, 0.66f, 0f);
		}
		else if (boostCharge >= 200f)
		{
			value2 = new Color(1f, 0.33f, 0f);
		}
		for (int i = 1; i < smr.materials.Length; i++)
		{
			switch (i)
			{
			case 1:
			case 2:
			case 3:
			case 6:
				value = ((boostCharge >= 300f) ? 1.2f : 0f);
				break;
			case 4:
			case 7:
				value = ((boostCharge >= 200f) ? 1.2f : 0f);
				break;
			case 5:
			case 8:
				value = ((boostCharge >= 100f) ? 1.2f : 0f);
				break;
			}
			smr.GetPropertyBlock(block, i);
			block.SetFloat(UKShaderProperties.EmissiveIntensity, value);
			block.SetColor(UKShaderProperties.EmissiveColor, value2);
			smr.SetPropertyBlock(block, i);
		}
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		UpdateWings();
		if (aboutToSlam)
		{
			rb.velocity = Vector3.up * 5f / ((float)slamWindUp * 10f);
			if ((float)slamWindUp >= 0.5f)
			{
				Slam();
			}
			return;
		}
		if (groundCheck.heavyFall)
		{
			rb.velocity = Vector3.down * 100f;
		}
		Vector2 zero = Vector2.zero;
		airFrictionless = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude > 25f;
		if (activated)
		{
			zero = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
			movementDirection = Vector3.ClampMagnitude(zero.x * Vector3.right + zero.y * Vector3.forward, 1f);
			movementDirection = Quaternion.Euler(0f, platformerCamera.rotation.eulerAngles.y, 0f) * movementDirection;
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			movementDirection = Vector3.zero;
		}
		if (movementDirection.magnitude > 0f)
		{
			anim.SetBool("Running", true);
		}
		else
		{
			anim.SetBool("Running", false);
		}
		if (rb.velocity.y < -100f)
		{
			rb.velocity = new Vector3(rb.velocity.x, -100f, rb.velocity.z);
		}
		if (activated && MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame && !falling && !jumpCooldown)
		{
			Jump();
		}
		groundCheck.UpdateState();
		if (!groundCheck.onGround)
		{
			if (fallTime < 1f)
			{
				fallTime += Time.deltaTime * 5f;
				if (fallTime > 1f)
				{
					falling = true;
				}
			}
			else if (rb.velocity.y < -2f)
			{
				fallSpeed = rb.velocity.y;
			}
		}
		else
		{
			fallTime = 0f;
		}
		if (groundCheck.onGround && falling && !jumpCooldown)
		{
			falling = false;
			SlamEnd(!slamming);
			if (aboutToSlam)
			{
				anim.Play("Landing", -1, 0f);
			}
		}
		if (!groundCheck.onGround && !aboutToSlam && activated && MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame && !GameStateManager.Instance.PlayerInputLocked)
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
			if (fallTime > 0.5f && !Physics.Raycast(groundCheck.transform.position + base.transform.up, base.transform.up * -1f, out var _, 3f, LayerMaskDefaults.Get(LMD.Environment)) && !groundCheck.heavyFall)
			{
				aboutToSlam = true;
				if (spinning)
				{
					StopSpin();
				}
				Object.Instantiate<AudioSource>(slamReadySound, base.transform.position, Quaternion.identity);
				anim.Play("SlamStart", -1, 0f);
				slamWindUp = 0f;
			}
		}
		if (groundCheck.heavyFall)
		{
			slamForce += Time.deltaTime * 5f;
			if (Physics.Raycast(groundCheck.transform.position + base.transform.up, base.transform.up * -1f, out var hitInfo2, 5f, LayerMaskDefaults.Get(LMD.Environment)) || Physics.SphereCast(groundCheck.transform.position + base.transform.up, 1f, base.transform.up * -1f, out hitInfo2, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Breakable component = hitInfo2.collider.GetComponent<Breakable>();
				if (component != null && ((component.weak && !component.precisionOnly && !component.specialCaseOnly) || component.forceGroundSlammable) && !component.unbreakable)
				{
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
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame && groundCheck.onGround && activated && !sliding)
		{
			StartSlide();
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame && !groundCheck.onGround && !sliding && !jumping && activated && Physics.Raycast(groundCheck.transform.position + base.transform.up, base.transform.up * -1f, out var _, 2f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			StartSlide();
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasCanceledThisFrame && sliding)
		{
			StopSlide();
		}
		if (sliding && activated)
		{
			slideLength += Time.deltaTime;
			if (currentSlideEffect != null)
			{
				currentSlideEffect.transform.position = base.transform.position + dodgeDirection * 10f;
			}
			if (slideSafety > 0f)
			{
				slideSafety -= Time.deltaTime * 5f;
			}
			if (groundCheck.onGround)
			{
				currentSlideScrape.transform.position = base.transform.position + dodgeDirection;
			}
			else
			{
				currentSlideScrape.transform.position = Vector3.one * 5000f;
			}
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.WasPerformedThisFrame && activated)
		{
			if ((bool)groundProperties && !groundProperties.canDash)
			{
				if (!groundProperties.silentDashFail)
				{
					Object.Instantiate(staminaFailSound);
				}
			}
			else if (boostCharge >= 100f)
			{
				if (sliding)
				{
					StopSlide();
				}
				boostLeft = 100f;
				boost = true;
				anim.Play("Dash", -1, 0f);
				dodgeDirection = movementDirection;
				if (dodgeDirection == Vector3.zero)
				{
					dodgeDirection = playerModel.forward;
				}
				Quaternion identity = Quaternion.identity;
				identity.SetLookRotation(dodgeDirection * -1f);
				Object.Instantiate(dodgeParticle, base.transform.position + Vector3.up * 2f + dodgeDirection * 10f, identity).transform.localScale *= 2f;
				if (!MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.infiniteStamina)
				{
					boostCharge -= 100f;
				}
				aud.clip = dodgeSound;
				aud.volume = 1f;
				aud.SetPitch(1f);
				aud.Play(tracked: true);
			}
			else
			{
				Object.Instantiate(staminaFailSound);
			}
		}
		if (boostCharge != 300f && !sliding && !spinning)
		{
			float num = 1f;
			if (difficulty == 1)
			{
				num = 1.5f;
			}
			else if (difficulty == 0)
			{
				num = 2f;
			}
			boostCharge = Mathf.MoveTowards(boostCharge, 300f, 70f * Time.deltaTime * num);
		}
		if (spinCooldown > 0f)
		{
			spinCooldown = Mathf.MoveTowards(spinCooldown, 0f, Time.deltaTime);
		}
		if (activated && !spinning && spinCooldown <= 0f && !MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame || MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame) && !MonoSingleton<OptionsManager>.Instance.paused)
		{
			Spin();
		}
		if (spinning)
		{
			playerModel.Rotate(Vector3.up, Time.deltaTime * 3600f, Space.Self);
		}
		else if (movementDirection.magnitude != 0f || boost || sliding)
		{
			Quaternion quaternion = Quaternion.LookRotation(movementDirection);
			if (sliding)
			{
				quaternion = Quaternion.LookRotation(rb.velocity);
			}
			else if (boost)
			{
				quaternion = Quaternion.LookRotation(dodgeDirection);
			}
			playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, quaternion, (Quaternion.Angle(playerModel.rotation, quaternion) + 20f) * 35f * movementDirection.magnitude * Time.deltaTime);
		}
		if ((groundCheck.onGround && !jumping) || base.transform.position.y < lastYPos)
		{
			beenOverYPosMax = false;
			lastYPos = base.transform.position.y;
		}
		if (cameraTrack)
		{
			float num2 = lastYPos;
			if (base.transform.position.y > lastYPos + 10f || beenOverYPosMax)
			{
				if (!beenOverYPosMax)
				{
					beenOverYPosMax = true;
					yPosDifferential = 10f;
				}
				if (rb.velocity.y < 0f && Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo4, yPosDifferential, LayerMaskDefaults.Get(LMD.Environment)) && !hitInfo4.transform.gameObject.CompareTag("Breakable") && hitInfo4.distance <= Mathf.Max(5f, yPosDifferential))
				{
					yPosDifferential = Mathf.MoveTowards(yPosDifferential, hitInfo4.distance, Time.deltaTime * (0.1f + Mathf.Abs(rb.velocity.y)));
				}
				else if (base.transform.position.y - lastYPos <= Mathf.Max(5f, yPosDifferential))
				{
					yPosDifferential = base.transform.position.y - lastYPos;
				}
				else
				{
					yPosDifferential = Mathf.MoveTowards(yPosDifferential, Mathf.Min(5f, yPosDifferential), Time.deltaTime * (0.1f + Mathf.Abs(rb.velocity.y)));
				}
				currentYPos = base.transform.position.y - yPosDifferential;
			}
			else
			{
				currentYPos = Mathf.MoveTowards(currentYPos, num2, Time.deltaTime * 15f * (0.1f + Mathf.Abs(Mathf.Abs(num2) - Mathf.Abs(currentYPos))));
			}
			if (!freeCamera)
			{
				CheckCameraTarget();
				Vector3 vector = new Vector3(base.transform.position.x, currentYPos, base.transform.position.z) + cameraTarget;
				if ((Physics.CheckSphere(vector, 0.5f, LayerMaskDefaults.Get(LMD.Environment)) && (!Physics.CheckSphere(vector - Vector3.up * 2f, 0.5f, LayerMaskDefaults.Get(LMD.Environment)) || cameraLowered)) || (Physics.SphereCast(new Ray(vector, base.transform.forward), 0.5f, 2f, LayerMaskDefaults.Get(LMD.Environment)) && (!Physics.SphereCast(new Ray(vector - Vector3.up * 2f, base.transform.forward), 0.5f, 2f, LayerMaskDefaults.Get(LMD.Environment)) || cameraLowered)))
				{
					cameraLowered = true;
					vector -= Vector3.up * 2f;
				}
				else
				{
					cameraLowered = false;
				}
				Vector3 position = Vector3.MoveTowards(platformerCamera.position, vector, Time.deltaTime * 15f * (0.1f + Vector3.Distance(platformerCamera.position, vector)));
				Quaternion rotation = Quaternion.RotateTowards(platformerCamera.transform.rotation, Quaternion.Euler(cameraRotation), Time.deltaTime * 15f * (0.1f + Vector3.Distance(platformerCamera.rotation.eulerAngles, cameraRotation)));
				platformerCamera.transform.SetPositionAndRotation(position, rotation);
			}
			else if (!MonoSingleton<OptionsManager>.Instance.paused)
			{
				platformerCamera.SetPositionAndRotation(base.transform.position + (freeCamera ? defaultFreeCameraTarget : defaultCameraTarget), Quaternion.Euler(defaultCameraRotation));
				Vector2 vector2 = MonoSingleton<InputManager>.Instance.InputSource.Look.ReadValue<Vector2>();
				if (!MonoSingleton<CameraController>.Instance.reverseY)
				{
					rotationX += vector2.y * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				else
				{
					rotationX -= vector2.y * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				if (!MonoSingleton<CameraController>.Instance.reverseX)
				{
					rotationY += vector2.x * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				else
				{
					rotationY -= vector2.x * (MonoSingleton<OptionsManager>.Instance.mouseSensitivity / 10f);
				}
				if (rotationY > 180f)
				{
					rotationY -= 360f;
				}
				else if (rotationY < -180f)
				{
					rotationY += 360f;
				}
				rotationX = Mathf.Clamp(rotationX, -69f, 109f);
				float num3 = 2.5f;
				if (sliding || Physics.Raycast(base.transform.position + Vector3.up * 0.625f, Vector3.up, 2.5f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					num3 = 0.625f;
				}
				Vector3 vector3 = base.transform.position + Vector3.up * num3;
				platformerCamera.RotateAround(vector3, Vector3.left, rotationX);
				platformerCamera.RotateAround(vector3, Vector3.up, rotationY);
				if (Physics.SphereCast(vector3, 0.25f, platformerCamera.position - vector3, out var hitInfo5, Vector3.Distance(vector3, platformerCamera.position), LayerMaskDefaults.Get(LMD.Environment)))
				{
					platformerCamera.position = hitInfo5.point + 0.5f * hitInfo5.normal;
				}
			}
		}
		if (Physics.SphereCast(base.transform.position + Vector3.up, 0.5f, Vector3.down, out var hitInfo6, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			jumpShadow.position = hitInfo6.point + Vector3.up * 0.05f;
			jumpShadow.forward = hitInfo6.normal;
		}
		else
		{
			jumpShadow.position = base.transform.position - Vector3.up * 1000f;
			jumpShadow.forward = Vector3.up;
		}
		if (coinTimer > 0f)
		{
			coinTimer = Mathf.MoveTowards(coinTimer, 0f, Time.deltaTime);
		}
		if (coinEffectTimer > 0f)
		{
			coinEffectTimer = Mathf.MoveTowards(coinEffectTimer, 0f, Time.deltaTime);
		}
		else if (queuedCoins > 0)
		{
			CoinGetEffect();
		}
		if (invincible && extraHits < 3)
		{
			if (blinkTimer > 0f)
			{
				blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, Time.deltaTime);
			}
			else
			{
				blinkTimer = 0.05f;
				if (playerModel.gameObject.activeSelf)
				{
					playerModel.gameObject.SetActive(value: false);
				}
				else
				{
					playerModel.gameObject.SetActive(value: true);
				}
			}
		}
		if (superTimer > 0f)
		{
			if (!NoWeaponCooldown.NoCooldown)
			{
				superTimer = Mathf.MoveTowards(superTimer, 0f, Time.deltaTime);
			}
			if (superTimer == 0f)
			{
				GetHit();
			}
		}
	}

	private void CheckCameraTarget(bool instant = false)
	{
		Vector3 vector = (freeCamera ? defaultFreeCameraTarget : defaultCameraTarget);
		Vector3 rotation = defaultCameraRotation;
		if (cameraTargets.Count > 0)
		{
			for (int num = cameraTargets.Count - 1; num >= 0; num--)
			{
				if ((bool)cameraTargets[num].caller && cameraTargets[num].caller.activeInHierarchy)
				{
					vector = cameraTargets[num].position;
					rotation = cameraTargets[num].rotation;
					break;
				}
				cameraTargets.RemoveAt(num);
			}
		}
		if (instant)
		{
			cameraTarget = vector;
			cameraRotation = rotation;
		}
		else
		{
			cameraTarget = Vector3.MoveTowards(cameraTarget, vector, Time.deltaTime * 2f * (0.1f + Vector3.Distance(cameraTarget, vector)));
			cameraRotation = Vector3.MoveTowards(cameraRotation, rotation, Time.deltaTime * 2f * (0.1f + Vector3.Distance(cameraRotation, rotation)));
		}
	}

	private void FixedUpdate()
	{
		groundCheck.UpdateState();
		slopeCheck.UpdateState();
		SlideValues();
		if (boost || spinning)
		{
			rb.SetGravityMode(useGravity: true);
			Dodge();
			return;
		}
		base.gameObject.layer = 2;
		if (groundCheck.onGround && !jumping)
		{
			anim.SetBool("InAir", false);
			inSpecialJump = false;
			float y = rb.velocity.y;
			if (slopeCheck.onGround && movementDirection.x == 0f && movementDirection.z == 0f)
			{
				y = 0f;
				rb.SetGravityMode(useGravity: false);
			}
			else
			{
				rb.SetGravityMode(useGravity: true);
			}
			float num = 2.75f;
			if ((bool)groundProperties)
			{
				num *= groundProperties.speedMultiplier;
			}
			movementDirection2 = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * num, y, movementDirection.z * walkSpeed * Time.deltaTime * num);
			float num2 = 2.5f;
			Vector3 zero = Vector3.zero;
			if ((bool)groundProperties)
			{
				num2 *= groundProperties.friction;
				if (groundProperties.push)
				{
					Vector3 vector = groundProperties.pushForce;
					if (groundProperties.pushDirectionRelative)
					{
						vector = groundProperties.transform.rotation * vector;
					}
					zero += vector;
				}
			}
			rb.velocity = Vector3.MoveTowards(rb.velocity, movementDirection2 + zero, num2);
			return;
		}
		anim.SetBool("InAir", true);
		rb.SetGravityMode(useGravity: true);
		movementDirection2 = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * 2.75f, rb.velocity.y, movementDirection.z * walkSpeed * Time.deltaTime * 2.75f);
		airDirection.y = 0f;
		if ((movementDirection2.x > 0f && rb.velocity.x < movementDirection2.x) || (movementDirection2.x < 0f && rb.velocity.x > movementDirection2.x))
		{
			airDirection.x = movementDirection2.x;
		}
		else
		{
			airDirection.x = 0f;
			if (!inSpecialJump && !airFrictionless)
			{
				rb.velocity = new Vector3(Mathf.MoveTowards(rb.velocity.x, movementDirection2.x, Time.fixedDeltaTime * 25f), rb.velocity.y, rb.velocity.z);
			}
		}
		if ((movementDirection2.z > 0f && rb.velocity.z < movementDirection2.z) || (movementDirection2.z < 0f && rb.velocity.z > movementDirection2.z))
		{
			airDirection.z = movementDirection2.z;
		}
		else
		{
			airDirection.z = 0f;
			if (!inSpecialJump && !airFrictionless)
			{
				rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, Mathf.MoveTowards(rb.velocity.z, movementDirection2.z, Time.fixedDeltaTime * 25f));
			}
		}
		rb.AddForce(airDirection.normalized * 6000f);
		LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
		if (rb.velocity.y < 0f)
		{
			layerMask = (int)layerMask | 0x1000;
		}
		if (!Physics.SphereCast(base.transform.position + Vector3.up * 2.5f * base.transform.localScale.y, (base.transform.localScale.x + base.transform.localScale.z) / 2f * 0.75f - 0.1f, Vector3.up * rb.velocity.y, out var hitInfo, 2.51f + rb.velocity.y * Time.fixedDeltaTime, layerMask))
		{
			return;
		}
		if (LayerMaskDefaults.IsMatchingLayer(hitInfo.transform.gameObject.layer, LMD.Environment))
		{
			EnemyIdentifier component2;
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && component.crate)
			{
				if (groundCheck.heavyFall && !component.precisionOnly && !component.specialCaseOnly)
				{
					component.Break(2f);
					return;
				}
				if (groundCheck.heavyFall)
				{
					SlamEnd();
				}
				if (component.bounceHealth > 1)
				{
					aud.clip = bounceSound;
					aud.SetPitch(Mathf.Lerp(1f, 2f, (float)(component.originalBounceHealth - component.bounceHealth) / (float)component.originalBounceHealth));
					aud.volume = 0.75f;
					aud.Play(tracked: true);
				}
				component.Bounce();
				if (base.transform.position.y < hitInfo.transform.position.y)
				{
					rb.velocity = new Vector3(MonoSingleton<PlatformerMovement>.Instance.rb.velocity.x, -10f, MonoSingleton<PlatformerMovement>.Instance.rb.velocity.z);
				}
				else if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
				{
					Jump(silent: true, 1.35f);
				}
				else
				{
					Jump(silent: true, 0.75f);
				}
			}
			else if (hitInfo.transform.gameObject.CompareTag("Armor") && hitInfo.transform.TryGetComponent<EnemyIdentifier>(out component2))
			{
				component2.InstaKill();
			}
		}
		else
		{
			if (!hitInfo.transform.TryGetComponent<EnemyIdentifier>(out var component3) || component3.dead)
			{
				return;
			}
			if (!component3.blessed)
			{
				component3.Splatter();
			}
			if (!groundCheck.heavyFall)
			{
				if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
				{
					Jump(silent: true, 1.25f);
				}
				else
				{
					Jump(silent: true, 0.75f);
				}
			}
		}
	}

	public void Jump(bool silent = false, float multiplier = 1f)
	{
		float num = 1500f * multiplier;
		if (groundCheck.heavyFall)
		{
			SlamEnd(cancel: true);
		}
		if (multiplier > 1f || base.transform.position.y > lastYPos + 1f)
		{
			if (multiplier <= 1f)
			{
				beenOverYPosMax = false;
			}
			else
			{
				beenOverYPosMax = true;
				yPosDifferential = 5f;
			}
			lastYPos = base.transform.position.y;
		}
		if ((bool)groundProperties)
		{
			if (!groundProperties.canJump)
			{
				if (!groundProperties.silentJumpFail)
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
		anim.SetBool("InAir", true);
		anim.Play("Jump");
		falling = true;
		jumping = true;
		Invoke("NotJumping", 0.25f);
		if (!silent)
		{
			aud.clip = jumpSound;
			if (groundCheck.superJumpChance > 0f)
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
		}
		rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
		if (sliding)
		{
			inSpecialJump = true;
			rb.AddForce(Vector3.up * jumpPower * num * 2f);
			StopSlide();
		}
		else if (boost)
		{
			if (boostCharge >= 100f)
			{
				if (!MonoSingleton<AssistController>.Instance.majorEnabled || !MonoSingleton<AssistController>.Instance.infiniteStamina)
				{
					boostCharge -= 100f;
				}
				Object.Instantiate(dashJumpSound);
				inSpecialJump = true;
			}
			else
			{
				rb.velocity = new Vector3(movementDirection.x * walkSpeed * Time.deltaTime * 2.75f, 0f, movementDirection.z * walkSpeed * Time.deltaTime * 2.75f);
				Object.Instantiate(staminaFailSound);
				inSpecialJump = false;
			}
			rb.AddForce(Vector3.up * jumpPower * num * 1.5f);
		}
		else
		{
			inSpecialJump = false;
			rb.AddForce(Vector3.up * jumpPower * num * 2.6f);
		}
		jumpCooldown = true;
		Invoke("JumpReady", 0.2f);
		boost = false;
	}

	private void Dodge()
	{
		aboutToSlam = false;
		if (spinning)
		{
			movementDirection2 = new Vector3(movementDirection.x * spinSpeed, rb.velocity.y, movementDirection.z * spinSpeed);
			if (movementDirection.magnitude == 0f && !falling)
			{
				rb.velocity = new Vector3(Mathf.MoveTowards(rb.velocity.x, 0f, Time.fixedDeltaTime * 150f), rb.velocity.y, Mathf.MoveTowards(rb.velocity.z, 0f, Time.fixedDeltaTime * 150f));
			}
			else
			{
				airDirection.y = 0f;
				if ((movementDirection2.x > 0f && rb.velocity.x < movementDirection2.x) || (movementDirection2.x < 0f && rb.velocity.x > movementDirection2.x))
				{
					airDirection.x = movementDirection2.x;
				}
				else
				{
					airDirection.x = 0f;
				}
				if ((movementDirection2.z > 0f && rb.velocity.z < movementDirection2.z) || (movementDirection2.z < 0f && rb.velocity.z > movementDirection2.z))
				{
					airDirection.z = movementDirection2.z;
				}
				else
				{
					airDirection.z = 0f;
				}
				if (falling)
				{
					rb.AddForce(airDirection.normalized * 4000f);
				}
				else
				{
					rb.AddForce(airDirection.normalized * 24000f);
				}
			}
			spinJuice = Mathf.MoveTowards(spinJuice, 0f, Time.fixedDeltaTime * 3f);
			if (spinJuice <= 0f)
			{
				StopSpin();
			}
			return;
		}
		if (sliding)
		{
			float num = 1f;
			if (preSlideSpeed > 1f)
			{
				if (preSlideSpeed > 3f)
				{
					preSlideSpeed = 3f;
				}
				num = preSlideSpeed;
				preSlideSpeed -= Time.fixedDeltaTime * preSlideSpeed;
				preSlideDelay = 0f;
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
			Vector3 velocity = new Vector3(dodgeDirection.x * walkSpeed * Time.fixedDeltaTime * 5f * num, rb.velocity.y, dodgeDirection.z * walkSpeed * Time.fixedDeltaTime * 5f * num);
			if ((bool)groundProperties && groundProperties.push)
			{
				Vector3 vector = groundProperties.pushForce;
				if (groundProperties.pushDirectionRelative)
				{
					vector = groundProperties.transform.rotation * vector;
				}
				velocity += vector;
			}
			rb.velocity = velocity;
			return;
		}
		float y = 0f;
		if (slideEnding)
		{
			y = rb.velocity.y;
		}
		float num2 = 2.25f;
		movementDirection2 = new Vector3(dodgeDirection.x * walkSpeed * Time.fixedDeltaTime * num2, y, dodgeDirection.z * walkSpeed * Time.fixedDeltaTime * num2);
		if (!slideEnding || groundCheck.onGround)
		{
			rb.velocity = movementDirection2 * 3f;
		}
		base.gameObject.layer = 15;
		boostLeft -= 4f;
		if (boostLeft <= 0f)
		{
			boost = false;
			if (!groundCheck.onGround && !slideEnding)
			{
				rb.velocity = movementDirection2;
			}
		}
		slideEnding = false;
	}

	public void Slam()
	{
		aboutToSlam = false;
		rb.velocity = new Vector3(0f, -100f, 0f);
		falling = true;
		fallSpeed = -100f;
		slamming = true;
		groundCheck.heavyFall = true;
		slamForce = 1f;
		if (currentFallParticle != null)
		{
			Object.Destroy(currentFallParticle);
		}
		currentFallParticle = Object.Instantiate(fallParticle, base.transform);
	}

	public void SlamEnd(bool cancel = false)
	{
		fallSpeed = 0f;
		groundCheck.heavyFall = false;
		slamming = false;
		if (currentFallParticle != null)
		{
			Object.Destroy(currentFallParticle);
		}
		if (!cancel)
		{
			anim.Play("SlamEnd", -1, 0f);
			MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
			Object.Instantiate(impactDust, base.transform.position, Quaternion.identity);
			MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(base.transform.position + Vector3.up * 0.1f, Vector3.down, 3f, 5);
		}
	}

	private void Spin()
	{
		anim.Play("Spin", -1, 0f);
		anim.SetBool("Spinning", true);
		spinning = true;
		spinJuice = 1f;
		spinZone.SetActive(value: true);
		if (sliding)
		{
			float num = 1f;
			if (preSlideSpeed > 1f)
			{
				if (preSlideSpeed > 3f)
				{
					preSlideSpeed = 3f;
				}
				num = preSlideSpeed;
			}
			if ((bool)groundProperties)
			{
				num *= groundProperties.speedMultiplier;
			}
			spinDirection = dodgeDirection;
			spinSpeed = walkSpeed * 5f * num * Time.fixedDeltaTime;
			StopSlide();
			boostLeft = 0f;
			boost = false;
			playerModel.rotation = Quaternion.LookRotation(movementDirection);
		}
		else if (boost)
		{
			spinDirection = dodgeDirection;
			spinSpeed = walkSpeed * 8.25f * Time.fixedDeltaTime;
			boostLeft = 0f;
			boost = false;
		}
		else
		{
			Vector3 velocity = rb.velocity;
			velocity += movementDirection * walkSpeed * Time.fixedDeltaTime;
			velocity.y = 0f;
			if (velocity.magnitude <= 0.25f)
			{
				spinDirection = playerModel.forward;
			}
			else
			{
				spinDirection = velocity;
			}
			spinSpeed = velocity.magnitude;
		}
		rb.velocity = new Vector3(spinDirection.normalized.x * spinSpeed, rb.velocity.y, spinDirection.normalized.z * spinSpeed);
	}

	private void StopSpin()
	{
		spinning = false;
		anim.SetBool("Spinning", false);
		spinJuice = 0f;
		playerModel.forward = spinDirection;
		spinCooldown = 0.2f;
		spinZone.SetActive(value: false);
	}

	private void StartSlide()
	{
		slideLength = 0f;
		anim.SetBool("Sliding", true);
		if (currentSlideEffect != null)
		{
			Object.Destroy(currentSlideEffect);
		}
		if (currentSlideScrape != null)
		{
			Object.Destroy(currentSlideScrape);
		}
		if ((bool)groundProperties && !groundProperties.canSlide)
		{
			if (!groundProperties.silentSlideFail)
			{
				StopSlide();
			}
			return;
		}
		playerCollider.height = 1.25f;
		playerCollider.center = Vector3.up * 0.625f;
		slideSafety = 1f;
		sliding = true;
		boost = true;
		dodgeDirection = movementDirection;
		if (dodgeDirection == Vector3.zero)
		{
			dodgeDirection = playerModel.forward;
		}
		Quaternion identity = Quaternion.identity;
		identity.SetLookRotation(dodgeDirection * -1f);
		currentSlideEffect = Object.Instantiate(slideEffect, base.transform.position + dodgeDirection * 10f, identity);
		currentSlideScrape = Object.Instantiate(slideScrape, base.transform.position + dodgeDirection * 2f, identity);
	}

	public void StopSlide()
	{
		anim.SetBool("Sliding", false);
		if (currentSlideEffect != null)
		{
			Object.Destroy(currentSlideEffect);
		}
		if (currentSlideScrape != null)
		{
			Object.Destroy(currentSlideScrape);
		}
		if (sliding)
		{
			Object.Instantiate(slideStopSound);
		}
		sliding = false;
		slideEnding = true;
		if (slideLength > MonoSingleton<NewMovement>.Instance.longestSlide)
		{
			MonoSingleton<NewMovement>.Instance.longestSlide = slideLength;
		}
		slideLength = 0f;
		if (!crouching)
		{
			playerCollider.height = 5f;
			playerCollider.center = Vector3.up * 2.5f;
		}
	}

	private void SlideValues()
	{
		if (sliding && slideSafety <= 0f)
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
		if (sliding || !activated || groundCheck.heavyFall)
		{
			return;
		}
		if (!boost && !falling && rb.velocity.magnitude / 24f > preSlideSpeed)
		{
			preSlideSpeed = rb.velocity.magnitude / 24f;
			preSlideDelay = 0.2f;
			return;
		}
		preSlideDelay = Mathf.MoveTowards(preSlideDelay, 0f, Time.fixedDeltaTime);
		if (preSlideDelay <= 0f)
		{
			preSlideDelay = 0.2f;
			preSlideSpeed = rb.velocity.magnitude / 24f;
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

	private void JumpReady()
	{
		jumpCooldown = false;
	}

	private void NotJumping()
	{
		jumping = false;
	}

	public void AddExtraHit(int amount = 1)
	{
		extraHits = Mathf.Clamp(extraHits + amount, 0, 3);
		CheckProtector();
		Object.Instantiate(protectorGet, playerCollider.bounds.center, Quaternion.identity, base.transform);
		if (extraHits >= 3)
		{
			invincible = true;
			playerModel.gameObject.SetActive(value: true);
			superTimer = 20f;
		}
	}

	private void CheckProtector()
	{
		extraHits = Mathf.Clamp(extraHits, 0, 3);
		for (int i = 0; i <= 2; i++)
		{
			if (i == extraHits - 1)
			{
				protectors[i].SetActive(value: true);
			}
			else
			{
				protectors[i].SetActive(value: false);
			}
		}
	}

	private void GetHit()
	{
		MonoSingleton<StatsManager>.Instance.tookDamage = true;
		extraHits--;
		CheckProtector();
		Object.Instantiate(protectorLose, playerCollider.bounds.center, Quaternion.identity, base.transform);
		invincible = true;
		Invoke("StopInvincibility", 3f);
	}

	private void StopInvincibility()
	{
		playerModel.gameObject.SetActive(value: true);
		invincible = false;
	}

	private void Death()
	{
		cameraTrack = false;
		dead = true;
		MonoSingleton<StatsManager>.Instance.tookDamage = true;
		if (extraHits > 0)
		{
			extraHits = 0;
			CheckProtector();
		}
		if (!freeCamera)
		{
			platformerCamera.transform.position = base.transform.position + cameraTarget;
		}
		if (boost || spinning)
		{
			StopSpin();
			boost = false;
		}
	}

	public void Fall()
	{
		if (!dead)
		{
			Death();
			Object.Instantiate(fallSound, base.transform.position, Quaternion.identity);
			Invoke("DeathOver", 2f);
		}
	}

	public void Explode(bool ignoreInvincible = false)
	{
		if (dead || (!ignoreInvincible && (invincible || Invincibility.Enabled)))
		{
			if (!dead && (extraHits == 3 || Invincibility.Enabled))
			{
				Object.Instantiate(protectorOof, playerCollider.bounds.center, Quaternion.identity, base.transform);
				Jump(silent: true);
			}
			return;
		}
		if (!ignoreInvincible && extraHits > 0)
		{
			GetHit();
			return;
		}
		Death();
		GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
		GameObject gameObject = Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.head, playerCollider.bounds.center, Quaternion.identity, goreZone.goreZone);
		Transform[] componentsInChildren = playerModel.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.childCount <= 0 && !(Random.Range(0f, 1f) > 0.5f))
			{
				gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Body);
				if (!gameObject)
				{
					break;
				}
				gameObject.transform.parent = goreZone.goreZone;
				gameObject.transform.position = transform.position;
				gameObject.SetActive(value: true);
				gameObject = null;
			}
		}
		base.gameObject.SetActive(value: false);
		Invoke("DeathOver", 2f);
	}

	public void Burn(bool ignoreInvincible = false)
	{
		if (dead || (!ignoreInvincible && (invincible || Invincibility.Enabled)))
		{
			if (!dead && (extraHits == 3 || Invincibility.Enabled))
			{
				Object.Instantiate(protectorOof, playerCollider.bounds.center, Quaternion.identity, base.transform);
				Jump(silent: true);
			}
			return;
		}
		if (!ignoreInvincible && extraHits > 0)
		{
			GetHit();
			return;
		}
		if (currentCorpse != null)
		{
			CancelInvoke();
			Object.Destroy(currentCorpse);
		}
		Death();
		if ((bool)defaultBurnEffect)
		{
			Object.Instantiate(defaultBurnEffect, base.transform.position, Quaternion.identity);
		}
		currentCorpse = Object.Instantiate(playerModel.gameObject, playerModel.position, playerModel.rotation);
		base.gameObject.SetActive(value: false);
		SandboxUtils.StripForPreview(currentCorpse.transform, burnMaterial);
		Invoke("BecomeAsh", 1f);
	}

	private void BecomeAsh()
	{
		if ((bool)currentCorpse)
		{
			Object.Instantiate(ashSound, base.transform.position, Quaternion.identity);
			Transform[] componentsInChildren = currentCorpse.transform.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				Object.Instantiate(ashParticle, transform.position, Quaternion.identity);
			}
			Object.Destroy(currentCorpse);
			Invoke("DeathOver", 1f);
		}
	}

	private void DeathOver()
	{
		Respawn();
		MonoSingleton<StatsManager>.Instance.Restart();
	}

	public void Respawn()
	{
		cameraTrack = true;
		dead = false;
		jumping = false;
		jumpCooldown = false;
		falling = false;
		fallTime = 0f;
		fallSpeed = 0f;
		aboutToSlam = false;
		slamming = false;
		groundCheck.heavyFall = false;
		if (currentFallParticle != null)
		{
			Object.Destroy(currentFallParticle);
		}
		extraHits = 0;
		boostCharge = 300f;
		rb.velocity = Vector3.zero;
		CancelInvoke();
		if ((bool)currentCorpse)
		{
			Object.Destroy(currentCorpse);
		}
		CheckProtector();
		StopInvincibility();
	}

	public void CoinGet()
	{
		queuedCoins++;
	}

	public void CoinGetEffect()
	{
		AudioSource component = Object.Instantiate(coinGet, playerCollider.bounds.center, Quaternion.identity).GetComponent<AudioSource>();
		if (coinTimer > 0f)
		{
			if (coinPitch < 1.35f)
			{
				coinPitch += 0.025f;
			}
			component.SetPitch(coinPitch);
		}
		else
		{
			coinPitch = 1f;
		}
		coinTimer = 1.5f;
		coinEffectTimer = 0.05f;
		queuedCoins--;
	}

	public void SnapCamera()
	{
		CheckCameraTarget(instant: true);
		platformerCamera.SetPositionAndRotation(base.transform.position + cameraTarget, Quaternion.Euler(cameraRotation));
	}

	public void SnapCamera(Vector3 targetPos, Vector3 targetRot)
	{
		cameraTarget = targetPos;
		cameraRotation = targetRot;
		platformerCamera.SetPositionAndRotation(targetPos, Quaternion.Euler(targetRot));
	}

	public void ResetCamera(float degreesY, float degreesX = 0f)
	{
		rotationY = degreesY;
		rotationX = degreesX;
	}

	public void SetData(ref TargetData data)
	{
		Vector3 headPosition = (data.position = cachedPos);
		data.realPosition = cachedPos;
		data.headPosition = headPosition;
		data.rotation = cachedRot;
		data.velocity = cachedVel;
	}

	public void UpdateCachedTransformData()
	{
		cachedPos = base.transform.position;
		cachedRot = base.transform.rotation;
		cachedVel = (rb ? rb.velocity : Vector3.zero);
	}
}
