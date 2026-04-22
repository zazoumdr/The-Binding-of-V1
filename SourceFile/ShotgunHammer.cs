using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotgunHammer : MonoBehaviour
{
	private WeaponIdentifier wid;

	public int variation;

	private GunControl gc;

	private bool gunReady;

	private WeaponPos wpos;

	private CameraFrustumTargeter targeter;

	private Animator anim;

	[HideInInspector]
	public int primaryCharge;

	public GameObject pumpChargeSound;

	public GameObject warningBeep;

	private float timeToBeep;

	private bool chargingSwing;

	private float swingCharge;

	[SerializeField]
	private Transform modelTransform;

	[HideInInspector]
	public Vector3 defaultModelPosition;

	[SerializeField]
	private Transform hammerPullable;

	[SerializeField]
	private AudioSource hammerPullSound;

	[HideInInspector]
	public Vector3 hammerDefaultPosition;

	private TimeSince pulledOut;

	private bool fireHeldOnPullOut;

	private float hammerCooldown;

	[SerializeField]
	private Transform rotatingMotor;

	private Quaternion motorPreviousRotation;

	[SerializeField]
	private SpriteRenderer motorSprite;

	[SerializeField]
	private AudioSource motorSound;

	private bool overheated;

	[SerializeField]
	private ParticleSystem overheatParticle;

	[SerializeField]
	private AudioSource overheatAud;

	private float currentSpeed;

	[SerializeField]
	private Renderer meter;

	[SerializeField]
	private Texture[] meterEmissives;

	private int tier;

	private MaterialPropertyBlock block;

	[SerializeField]
	private Transform meterHand;

	private TimeSince tierDownTimer;

	[SerializeField]
	private Image secondaryMeter;

	private float secondaryMeterFill;

	[SerializeField]
	private AudioSource hitSound;

	[SerializeField]
	private GameObject[] hitImpactParticle;

	private Coroutine impactRoutine;

	private float storedSpeed;

	private TimeSince speedStorageTimer;

	[Header("Core Eject")]
	[SerializeField]
	private GameObject grenade;

	[SerializeField]
	private AudioSource nadeSpawnSound;

	[SerializeField]
	private AudioSource nadeReadySound;

	private bool nadeCharging;

	[Header("Pump Charge")]
	[SerializeField]
	private AudioSource pump1Sound;

	[SerializeField]
	private AudioSource pump2Sound;

	[SerializeField]
	private GameObject pumpExplosion;

	[SerializeField]
	private GameObject overPumpExplosion;

	private bool aboutToSecondary;

	[Header("Chainsaw")]
	public GameObject chargeSoundBubble;

	private AudioSource tempChargeSound;

	private bool charging;

	private float chargeForce;

	[SerializeField]
	private Chainsaw chainsaw;

	private List<Chainsaw> currentChainsaws = new List<Chainsaw>();

	[SerializeField]
	private Transform chainsawAttachPoint;

	[SerializeField]
	private ScrollingTexture chainsawBladeScroll;

	private MeshRenderer chainsawBladeRenderer;

	private Material chainsawBladeMaterial;

	[SerializeField]
	private Material chainsawBladeMotionMaterial;

	[HideInInspector]
	public bool chainsawBroken;

	private Vibrate chainsawBrokenVibrate;

	[SerializeField]
	private MeshRenderer chainsawRenderer;

	private Material chainsawMaterial;

	[SerializeField]
	private Material chainsawBrokenMaterial;

	[SerializeField]
	private GameObject chainsawBreakEffect;

	private float chainSawBrokenPitchTarget;

	[SerializeField]
	private HurtZone sawZone;

	[SerializeField]
	private ParticleSystem environmentalSawSpark;

	[SerializeField]
	private AudioSource environmentalSawSound;

	private TimeSince enviroGibSpawnCooldown;

	private bool launchPlayer;

	private EnemyIdentifier hitEnemy;

	private Vector3 direction;

	private Transform target;

	private Vector3 hitPosition;

	private float damage;

	private bool forceWeakHit;

	private Grenade hitGrenade;

	private void Awake()
	{
		if (defaultModelPosition == Vector3.zero)
		{
			defaultModelPosition = modelTransform.localPosition;
		}
		if (hammerDefaultPosition == Vector3.zero)
		{
			hammerDefaultPosition = hammerPullable.localPosition;
		}
		targeter = Camera.main.GetComponent<CameraFrustumTargeter>();
		wid = GetComponent<WeaponIdentifier>();
		gc = GetComponentInParent<GunControl>();
		wpos = GetComponent<WeaponPos>();
		anim = GetComponent<Animator>();
		block = new MaterialPropertyBlock();
		if ((bool)chainsawBladeScroll)
		{
			chainsawBladeRenderer = chainsawBladeScroll.GetComponent<MeshRenderer>();
			chainsawBladeMaterial = chainsawBladeRenderer.sharedMaterial;
		}
		if ((bool)sawZone)
		{
			sawZone.sourceWeapon = base.gameObject;
		}
		if ((bool)chainsawRenderer)
		{
			chainsawMaterial = chainsawRenderer.sharedMaterial;
			chainsawBrokenVibrate = chainsawRenderer.GetComponent<Vibrate>();
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
		{
			fireHeldOnPullOut = true;
		}
		impactRoutine = null;
		pulledOut = 0f;
		UpdateMeter(forceUpdateTexture: true);
		if (variation == 2)
		{
			foreach (Chainsaw currentChainsaw in currentChainsaws)
			{
				currentChainsaw.lineStartTransform = chainsawAttachPoint;
			}
			chainsawAttachPoint.gameObject.SetActive(MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f);
		}
		if (chainsawBroken && MonoSingleton<WeaponCharges>.Instance.shoSawResetTimer <= 0f)
		{
			chainsawBroken = false;
			chainsawRenderer.material = chainsawMaterial;
			chainsawBrokenVibrate.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (impactRoutine != null)
		{
			if (hitGrenade != null)
			{
				HitNade();
			}
			if (hitEnemy != null)
			{
				DeliverDamage();
			}
			ImpactEffects();
		}
		impactRoutine = null;
		gunReady = false;
		primaryCharge = 0;
		chargingSwing = false;
		charging = false;
		swingCharge = 0f;
		hammerPullable.localPosition = hammerDefaultPosition;
		tier = 0;
		hammerCooldown = 0f;
		chargeForce = 0f;
		if ((Object)(object)tempChargeSound != null)
		{
			Object.Destroy((Object)(object)tempChargeSound);
		}
		foreach (Chainsaw currentChainsaw in currentChainsaws)
		{
			currentChainsaw.lineStartTransform = MonoSingleton<NewMovement>.Instance.transform;
		}
		if ((bool)sawZone)
		{
			sawZone.enabled = false;
		}
		if ((bool)(Object)(object)environmentalSawSound)
		{
			environmentalSawSound.Stop();
		}
		if ((bool)(Object)(object)environmentalSawSpark)
		{
			environmentalSawSpark.Stop();
		}
	}

	private void UpdateMeter(bool forceUpdateTexture = false)
	{
		int num = 0;
		if (currentSpeed > 0.66f)
		{
			num = 2;
		}
		else if (currentSpeed > 0.33f)
		{
			num = 1;
		}
		if (MonoSingleton<HookArm>.Instance.beingPulled && tier == 2)
		{
			num = 1;
		}
		else if (num < tier)
		{
			if ((float)tierDownTimer <= 0.5f)
			{
				num = tier;
			}
		}
		else
		{
			tierDownTimer = 0f;
		}
		meter.GetPropertyBlock(block, 1);
		if (tier != num || forceUpdateTexture)
		{
			block.SetTexture("_EmissiveTex", meterEmissives[num]);
			tier = num;
		}
		if (overheated)
		{
			block.SetFloat("_EmissiveIntensity", 0f);
		}
		else if ((float)tierDownTimer > 0f)
		{
			block.SetFloat("_EmissiveIntensity", ((float)tierDownTimer % 0.1f >= 0.05f) ? 1 : 0);
		}
		else
		{
			block.SetFloat("_EmissiveIntensity", 1f);
		}
		meter.SetPropertyBlock(block, 1);
		if (variation == 0)
		{
			secondaryMeterFill = MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge;
			if (secondaryMeterFill >= 1f)
			{
				((Graphic)secondaryMeter).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[0];
			}
			else
			{
				((Graphic)secondaryMeter).color = Color.white;
			}
		}
		else if (variation == 1)
		{
			if (timeToBeep != 0f)
			{
				timeToBeep = Mathf.MoveTowards(timeToBeep, 0f, Time.deltaTime * 5f);
			}
			secondaryMeterFill = (float)primaryCharge / 3f;
			if (primaryCharge == 3)
			{
				secondaryMeterFill = 1f;
				if (timeToBeep == 0f)
				{
					timeToBeep = 1f;
					Object.Instantiate(warningBeep);
					((Graphic)secondaryMeter).color = Color.red;
				}
				else if (timeToBeep < 0.5f)
				{
					((Graphic)secondaryMeter).color = Color.black;
				}
			}
			else if (primaryCharge == 1)
			{
				((Graphic)secondaryMeter).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[1];
			}
			else if (primaryCharge == 2)
			{
				((Graphic)secondaryMeter).color = Color.yellow;
			}
		}
		else if (variation == 2)
		{
			if (MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f && !chainsawAttachPoint.gameObject.activeSelf)
			{
				chainsawAttachPoint.gameObject.SetActive(value: true);
			}
			if (charging)
			{
				secondaryMeterFill = chargeForce / 60f;
				((Graphic)secondaryMeter).color = Color.Lerp(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation], new Color(1f, 0.25f, 0.25f), chargeForce / 60f);
			}
			else
			{
				secondaryMeterFill = MonoSingleton<WeaponCharges>.Instance.shoSawCharge;
				((Graphic)secondaryMeter).color = ((MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f) ? MonoSingleton<ColorBlindSettings>.Instance.variationColors[2] : Color.white);
			}
		}
	}

	private void Update()
	{
		overheated = MonoSingleton<WeaponCharges>.Instance.shoaltcooldowns[variation] > 0f;
		if (overheated && !overheatAud.isPlaying)
		{
			overheatAud.Play(tracked: true);
			overheatParticle.Play();
			anim.SetBool("Cooldown", true);
		}
		else if (!overheated && overheatAud.isPlaying)
		{
			overheatAud.Stop();
			overheatParticle.Stop();
			anim.SetBool("Cooldown", false);
		}
		float num = Mathf.Min(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude / 60f, 1f);
		currentSpeed = (overheated ? 0f : Mathf.MoveTowards(currentSpeed, num, Time.deltaTime * 2f));
		if (MonoSingleton<HookArm>.Instance.beingPulled)
		{
			currentSpeed = Mathf.Min(currentSpeed, 0.5f);
		}
		UpdateMeter();
		if ((float)pulledOut >= 0.5f)
		{
			gunReady = true;
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && !chargingSwing && hammerCooldown <= 0f && !overheated && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && (!fireHeldOnPullOut || (float)pulledOut >= 0.25f) && gc.activated)
		{
			fireHeldOnPullOut = false;
			chargingSwing = true;
		}
		else if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && !MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && swingCharge == 1f && gunReady && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			chargingSwing = false;
			swingCharge = 0f;
			if (!wid || wid.delay == 0f)
			{
				Impact();
			}
			else
			{
				gunReady = false;
				Invoke("Impact", wid.delay);
			}
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && variation == 2 && gunReady && gc.activated && !GameStateManager.Instance.PlayerInputLocked && MonoSingleton<WeaponCharges>.Instance.shoSawCharge >= 1f)
		{
			charging = true;
			if (chargeForce < 60f)
			{
				chargeForce = Mathf.MoveTowards(chargeForce, 60f, Time.deltaTime * 60f);
			}
			float num2 = 12000f;
			base.transform.localPosition = new Vector3(wpos.currentDefault.x + Random.Range(chargeForce / num2 * -1f, chargeForce / num2), wpos.currentDefault.y + Random.Range(chargeForce / num2 * -1f, chargeForce / num2), wpos.currentDefault.z + Random.Range(chargeForce / num2 * -1f, chargeForce / num2));
			if ((Object)(object)tempChargeSound == null)
			{
				GameObject gameObject = Object.Instantiate(chargeSoundBubble);
				tempChargeSound = gameObject.GetComponent<AudioSource>();
				if ((bool)wid && wid.delay > 0f)
				{
					AudioSource obj = tempChargeSound;
					obj.volume -= wid.delay * 2f;
					if (tempChargeSound.volume < 0f)
					{
						tempChargeSound.volume = 0f;
					}
				}
			}
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.ShotgunCharge, ((Component)(object)tempChargeSound).gameObject).intensityMultiplier = chargeForce / 60f;
			if (chainsawBroken)
			{
				tempChargeSound.SetPitch(Mathf.MoveTowards(tempChargeSound.pitch, chainSawBrokenPitchTarget, Time.deltaTime * 2f));
				if (tempChargeSound.GetPitch() == chainSawBrokenPitchTarget)
				{
					chainSawBrokenPitchTarget = 1f + Random.Range(-0.33f, 0.33f);
				}
			}
			else
			{
				tempChargeSound.SetPitch((chargeForce / 2f + 30f) / 60f);
			}
		}
		if (!MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && variation == 2 && gunReady && gc.activated && charging)
		{
			charging = false;
			MonoSingleton<WeaponCharges>.Instance.shoSawCharge = 0f;
			if (!wid || wid.delay == 0f)
			{
				ShootSaw();
			}
			else
			{
				gunReady = false;
				Invoke("ShootSaw", wid.delay);
			}
			if ((bool)(Object)(object)tempChargeSound)
			{
				Object.Destroy(((Component)(object)tempChargeSound).gameObject);
			}
		}
		if (variation == 2)
		{
			if (charging && chainsawBladeScroll.scrollSpeedX == 0f)
			{
				chainsawBladeRenderer.material = chainsawBladeMotionMaterial;
			}
			else if (!charging && chainsawBladeScroll.scrollSpeedX > 0f)
			{
				chainsawBladeRenderer.material = chainsawBladeMaterial;
			}
			chainsawBladeScroll.scrollSpeedX = chargeForce / 6f;
			anim.SetBool("Sawing", charging);
			sawZone.enabled = charging;
			if (charging && Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), MonoSingleton<CameraController>.Instance.transform.forward, out var hitInfo, 3f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				((Component)(object)environmentalSawSpark).transform.position = hitInfo.point;
				if (!environmentalSawSpark.isEmitting)
				{
					environmentalSawSpark.Play();
				}
				if (!environmentalSawSound.isPlaying)
				{
					environmentalSawSound.Play(tracked: true);
				}
				MonoSingleton<CameraController>.Instance.CameraShake(0.1f);
				if ((float)enviroGibSpawnCooldown > 0.1f)
				{
					if (SceneHelper.IsStaticEnvironment(hitInfo))
					{
						MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(hitInfo);
					}
					enviroGibSpawnCooldown = 0f;
				}
			}
			else
			{
				if (environmentalSawSpark.isEmitting)
				{
					environmentalSawSpark.Stop();
				}
				if (environmentalSawSound.isPlaying)
				{
					environmentalSawSound.Stop();
				}
			}
			if (charging)
			{
				if (sawZone.chainsawNonlethalHits > 60)
				{
					charging = false;
					chainsawBrokenVibrate.enabled = false;
					MonoSingleton<WeaponCharges>.Instance.shoSawCharge = 0f;
					ShootSaw(noSaw: true);
					if ((bool)(Object)(object)tempChargeSound)
					{
						Object.Destroy(((Component)(object)tempChargeSound).gameObject);
					}
					chainsawBroken = false;
					chainsawRenderer.material = chainsawMaterial;
					Object.Instantiate(chainsawBreakEffect, chainsawBladeRenderer.transform.position, Quaternion.identity).SetActive(value: true);
				}
				else if (sawZone.chainsawNonlethalHits > 40 && !chainsawBroken)
				{
					chainsawBroken = true;
					chainsawBrokenVibrate.enabled = true;
					chainsawRenderer.material = chainsawBrokenMaterial;
					GameObject obj2 = Object.Instantiate(chainsawBreakEffect, chainsawBladeRenderer.transform.position, Quaternion.identity);
					MonoSingleton<CameraController>.Instance.CameraShake(1f);
					obj2.SetActive(value: true);
					chainSawBrokenPitchTarget = 1f;
				}
			}
		}
		if (chargingSwing)
		{
			swingCharge = Mathf.MoveTowards(swingCharge, 1f, Time.deltaTime * 2f);
		}
		modelTransform.localPosition = new Vector3(defaultModelPosition.x + Random.Range((0f - swingCharge) / 30f, swingCharge / 30f), defaultModelPosition.y + Random.Range((0f - swingCharge) / 30f, swingCharge / 30f), defaultModelPosition.z + Random.Range((0f - swingCharge) / 30f, swingCharge / 30f));
		if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && variation != 2 && (variation == 1 || MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge >= 1f) && !aboutToSecondary && gunReady && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			gunReady = false;
			if (!wid || wid.delay == 0f)
			{
				if (variation == 0)
				{
					ThrowNade();
				}
				else
				{
					Pump();
				}
			}
			else
			{
				aboutToSecondary = true;
				Invoke((variation == 0) ? "ThrowNade" : "Pump", wid.delay);
			}
		}
		if (secondaryMeterFill >= 1f)
		{
			secondaryMeter.fillAmount = 1f;
		}
		else if (secondaryMeterFill <= 0f)
		{
			secondaryMeter.fillAmount = 0f;
		}
		else
		{
			secondaryMeter.fillAmount = Mathf.Lerp(0.275f, 0.625f, secondaryMeterFill);
		}
		if (hammerCooldown > 0f)
		{
			hammerCooldown = Mathf.MoveTowards(hammerCooldown, 0f, Time.deltaTime);
		}
		if (MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge < 1f)
		{
			nadeCharging = true;
		}
		else if (nadeCharging)
		{
			nadeCharging = false;
			Object.Instantiate<AudioSource>(nadeReadySound);
		}
	}

	private void FixedUpdate()
	{
		float magnitude = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude;
		if (magnitude >= storedSpeed - 5f || (float)speedStorageTimer > 0.5f)
		{
			storedSpeed = magnitude;
			speedStorageTimer = 0f;
		}
	}

	private void LateUpdate()
	{
		float num = Time.timeScale;
		if (impactRoutine != null)
		{
			num = 1f;
		}
		hammerPullable.localPosition = Vector3.Lerp(hammerDefaultPosition, hammerDefaultPosition + Vector3.up * -0.65f, swingCharge);
		hammerPullSound.volume = swingCharge * 0.6f;
		hammerPullSound.SetPitch(swingCharge + currentSpeed * 0.2f * num * Time.unscaledDeltaTime * 150f);
		if (overheated)
		{
			meterHand.localRotation = Quaternion.Euler(0f, 170f - MonoSingleton<WeaponCharges>.Instance.shoaltcooldowns[variation] / 7f * 200f, -90f);
		}
		else
		{
			meterHand.localRotation = Quaternion.Euler(0f, 170f - currentSpeed * 200f, -90f);
		}
		rotatingMotor.localRotation = motorPreviousRotation;
		rotatingMotor.Rotate(Vector3.up * currentSpeed * 10f * num * Time.unscaledDeltaTime * 150f, Space.Self);
		motorPreviousRotation = rotatingMotor.localRotation;
		motorSprite.color = new Color(1f, 1f, 1f, currentSpeed / 3f);
		motorSound.volume = currentSpeed / 2f;
		if (impactRoutine == null)
		{
			motorSound.SetPitch(currentSpeed * num);
		}
		else
		{
			motorSound.SetPitch(tier + 1);
		}
	}

	private void Impact()
	{
		impactRoutine = StartCoroutine(ImpactRoutine());
	}

	private IEnumerator ImpactRoutine()
	{
		hitEnemy = null;
		hitGrenade = null;
		target = null;
		hitPosition = Vector3.zero;
		MonoSingleton<PlayerAnimations>.Instance?.Shoot();
		hammerCooldown = 0.5f;
		Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos();
		direction = MonoSingleton<CameraController>.Instance.transform.forward;
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			direction = (targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
		}
		if (MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0 || MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
		{
			Collider[] cols = Physics.OverlapSphere(position, 0.01f);
			if (cols.Length != 0)
			{
				for (int i = 0; i < cols.Length; i++)
				{
					Transform transform = cols[i].transform;
					if (transform.TryGetComponent<ParryHelper>(out var component))
					{
						transform = component.target;
					}
					if (MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0)
					{
						Grenade componentInParent = transform.GetComponentInParent<Grenade>();
						if ((bool)componentInParent && hitGrenade != componentInParent)
						{
							hitGrenade = componentInParent;
							cols[i].enabled = false;
							Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
							MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
							yield return new WaitForSeconds(0.01f);
							HitNade();
						}
					}
					else if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
					{
						Landmine component3;
						if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 && transform.TryGetComponent<Chainsaw>(out var component2))
						{
							Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
							component2.GetPunched();
							component2.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + direction;
							component2.rb.velocity = (Punch.GetParryLookTarget() - component2.transform.position).normalized * 105f;
						}
						else if (MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0 && transform.TryGetComponent<Landmine>(out component3))
						{
							component3.transform.LookAt(Punch.GetParryLookTarget());
							component3.Parry();
							Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
							anim.Play("Fire", -1, 0f);
							MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
							yield return new WaitForSeconds(0.01f);
						}
					}
				}
			}
		}
		if (MonoSingleton<WeaponCharges>.Instance.shoSawAmount > 0 || MonoSingleton<ObjectTracker>.Instance.landmineList.Count > 0)
		{
			RaycastHit[] rhits = Physics.RaycastAll(position, direction, 8f, 16384, QueryTriggerInteraction.Collide);
			for (int i = 0; i < rhits.Length; i++)
			{
				Transform transform2 = rhits[i].transform;
				if (transform2.TryGetComponent<ParryHelper>(out var component4))
				{
					transform2 = component4.target;
				}
				Landmine component6;
				if (transform2.TryGetComponent<Chainsaw>(out var component5))
				{
					Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
					component5.GetPunched();
					component5.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + direction;
					component5.rb.velocity = (Punch.GetParryLookTarget() - component5.transform.position).normalized * 105f;
				}
				else if (transform2.TryGetComponent<Landmine>(out component6))
				{
					component6.transform.LookAt(Punch.GetParryLookTarget());
					component6.Parry();
					Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
					anim.Play("Fire", -1, 0f);
					MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
					yield return new WaitForSeconds(0.01f);
				}
			}
		}
		if (Physics.Raycast(position, direction, out var rhit, 8f, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment), QueryTriggerInteraction.Collide))
		{
			if (rhit.transform.gameObject.layer == 11 || rhit.transform.gameObject.layer == 10)
			{
				GameObject gameObject = rhit.transform.gameObject;
				EnemyIdentifierIdentifier component11;
				if (rhit.transform.gameObject.TryGetComponent<ParryHelper>(out var component7))
				{
					EnemyIdentifier component9;
					Grenade component10;
					if (component7.target.TryGetComponent<EnemyIdentifierIdentifier>(out var component8) && (bool)component8.eid && !component8.eid.dead)
					{
						hitEnemy = component8.eid;
					}
					else if (component7.target.TryGetComponent<EnemyIdentifier>(out component9) && !component9.dead)
					{
						hitEnemy = component9;
					}
					else if (component7.target.TryGetComponent<Grenade>(out component10))
					{
						gameObject = component7.target.gameObject;
					}
				}
				else if (rhit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component11) && (bool)component11.eid && !component11.eid.dead)
				{
					hitEnemy = component11.eid;
				}
				if (hitEnemy == null && MonoSingleton<ObjectTracker>.Instance.grenadeList.Count > 0)
				{
					Grenade componentInParent2 = gameObject.GetComponentInParent<Grenade>();
					if ((bool)componentInParent2 && hitGrenade != componentInParent2)
					{
						hitGrenade = componentInParent2;
						rhit.collider.enabled = false;
						Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
						anim.Play("Fire", -1, 0f);
						MonoSingleton<TimeController>.Instance.TrueStop(0.25f);
						yield return new WaitForSeconds(0.01f);
						HitNade();
					}
				}
			}
			target = rhit.transform;
			hitPosition = rhit.point;
		}
		if (hitEnemy == null)
		{
			Vector3 vector = position + direction * 2.5f;
			Collider[] array = Physics.OverlapSphere(vector, 2.5f);
			if (array.Length != 0)
			{
				float num = 2.5f;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].TryGetComponent<ParryHelper>(out var component12) && component12.target.TryGetComponent<Collider>(out var component13))
					{
						array[j] = component13;
					}
					if (array[j].gameObject.layer != 10 && array[j].gameObject.layer != 11)
					{
						continue;
					}
					Vector3 vector2 = array[j].ClosestPoint(vector);
					if (Physics.Raycast(position, vector2 - position, out rhit, Vector3.Distance(vector2, position), LayerMaskDefaults.Get(LMD.Environment)))
					{
						continue;
					}
					float num2 = Vector3.Distance(vector, vector2);
					if (num2 < num)
					{
						Transform transform3 = ((array[j].attachedRigidbody != null) ? array[j].attachedRigidbody.transform : array[j].transform);
						EnemyIdentifier component14 = null;
						if (transform3.TryGetComponent<EnemyIdentifierIdentifier>(out var component15))
						{
							component14 = component15.eid;
						}
						else
						{
							transform3.TryGetComponent<EnemyIdentifier>(out component14);
						}
						if ((bool)component14 && (!component14.dead || hitEnemy == null))
						{
							hitEnemy = component15.eid;
							num = num2;
							target = transform3;
							hitPosition = vector2;
						}
					}
				}
			}
			RaycastHit[] array2 = Physics.SphereCastAll(position + direction * 2.5f, 2.5f, direction, 3f, LayerMaskDefaults.Get(LMD.Enemies));
			if (array2.Length != 0)
			{
				float num3 = -1f;
				if (hitEnemy != null)
				{
					num3 = Vector3.Dot(direction, hitPosition - position);
				}
				for (int k = 0; k < array2.Length; k++)
				{
					if (Physics.Raycast(position, array2[k].point - position, out rhit, Vector3.Distance(array2[k].point, position), LayerMaskDefaults.Get(LMD.Environment)))
					{
						continue;
					}
					float num4 = Vector3.Dot(direction, array2[k].point - position);
					if (num4 > num3)
					{
						Transform transform4 = array2[k].transform;
						Vector3 point = array2[k].point;
						if (transform4.TryGetComponent<ParryHelper>(out var component16))
						{
							transform4 = component16.target.transform;
						}
						EnemyIdentifier component17 = null;
						if (transform4.TryGetComponent<EnemyIdentifierIdentifier>(out var component18))
						{
							component17 = component18.eid;
						}
						else
						{
							transform4.TryGetComponent<EnemyIdentifier>(out component17);
						}
						if ((bool)component17 && (!component17.dead || hitEnemy == null))
						{
							hitEnemy = component17;
							num3 = num4;
							target = transform4;
							hitPosition = point;
						}
					}
				}
			}
		}
		forceWeakHit = true;
		if (target != null)
		{
			Breakable component19;
			Glass component20;
			if (hitEnemy != null)
			{
				float num5 = 0.05f;
				damage = 3f;
				if (tier == 2)
				{
					num5 = 0.5f;
					damage = 10f;
				}
				else if (tier == 1)
				{
					num5 = 0.25f;
					damage = 6f;
				}
				if (hitEnemy.dead)
				{
					num5 = 0f;
				}
				if (num5 > 0f)
				{
					forceWeakHit = false;
					launchPlayer = true;
					Object.Instantiate<AudioSource>(hitSound, base.transform.position, Quaternion.identity);
					MonoSingleton<TimeController>.Instance.TrueStop(num5);
					yield return new WaitForSeconds(0.01f);
				}
				else
				{
					launchPlayer = false;
				}
				DeliverDamage();
			}
			else if (target.TryGetComponent<Breakable>(out component19) && !component19.precisionOnly && !component19.specialCaseOnly && !component19.unbreakable)
			{
				component19.Break(damage);
			}
			else if (target.TryGetComponent<Glass>(out component20) && !component20.broken)
			{
				component20.Shatter();
			}
		}
		if (!hitGrenade && ((hitEnemy == null && target != null && LayerMaskDefaults.IsMatchingLayer(target.gameObject.layer, LMD.Environment)) || ((bool)hitEnemy && hitEnemy.dead)))
		{
			MonoSingleton<NewMovement>.Instance.Launch(-direction * ((float)(100 * tier + 300) / ((float)(MonoSingleton<NewMovement>.Instance.hammerJumps + 3) / 3f)));
			MonoSingleton<NewMovement>.Instance.hammerJumps++;
			MonoSingleton<NewMovement>.Instance.explosionLaunchResistance = 0.5f;
			MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(position, direction, 8f, 10, 2f);
		}
		ImpactEffects();
		hitEnemy = null;
		hitGrenade = null;
		impactRoutine = null;
	}

	private void DeliverDamage()
	{
		direction = MonoSingleton<CameraController>.Instance.transform.forward;
		if (launchPlayer)
		{
			MonoSingleton<NewMovement>.Instance.Launch(-direction * (300 * tier + 100));
			if (MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude < storedSpeed)
			{
				MonoSingleton<NewMovement>.Instance.rb.velocity = -direction * storedSpeed;
			}
		}
		if (!hitEnemy.hitterWeapons.Contains("hammer" + variation))
		{
			hitEnemy.hitterWeapons.Add("hammer" + variation);
		}
		bool dead = hitEnemy.dead;
		if (target.gameObject.CompareTag("Body"))
		{
			hitEnemy.hitter = "hammerzone";
			hitEnemy.DeliverDamage(target.gameObject, direction * (50000 * tier + 50000), hitPosition, 4f, tryForExplode: true, 0f, base.gameObject);
		}
		hitEnemy.hitter = "hammer";
		hitEnemy.DeliverDamage(target.gameObject, direction * (50000 * tier + 50000), hitPosition, damage, tryForExplode: true, 0f, base.gameObject);
		if (!dead)
		{
			if (hitEnemy.dead)
			{
				if (tier == 2)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(240, "ultrakill.hammerhitred", base.gameObject, hitEnemy);
				}
				else if (tier == 1)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(120, "ultrakill.hammerhityellow", base.gameObject, hitEnemy);
				}
				else
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.hammerhitgreen", base.gameObject, hitEnemy);
				}
			}
			else if (tier == 2)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(120, "ultrakill.hammerhitheavy", base.gameObject, hitEnemy);
			}
			else
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(40 * (tier + 1), "ultrakill.hammerhit", base.gameObject, hitEnemy);
			}
		}
		if (dead || hitEnemy.enemyType == EnemyType.Idol || hitEnemy.enemyType == EnemyType.Deathcatcher || wid.delay != 0f)
		{
			return;
		}
		if (tier == 2)
		{
			MonoSingleton<WeaponCharges>.Instance.shoaltcooldowns[variation] = 7f;
		}
		else if (tier == 1)
		{
			if (MonoSingleton<WeaponCharges>.Instance.shoAltYellows >= 2)
			{
				MonoSingleton<WeaponCharges>.Instance.shoaltcooldowns[variation] = 7f;
			}
			MonoSingleton<WeaponCharges>.Instance.shoAltYellowsTimer = 3f;
			MonoSingleton<WeaponCharges>.Instance.shoAltYellows++;
		}
	}

	private void HitNade()
	{
		direction = MonoSingleton<CameraController>.Instance.transform.forward;
		if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), direction, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment), QueryTriggerInteraction.Ignore))
		{
			hitGrenade.GrenadeBeam(hitInfo.point, base.gameObject);
		}
		else
		{
			hitGrenade.GrenadeBeam(MonoSingleton<CameraController>.Instance.GetDefaultPos() + direction * 1000f, base.gameObject);
		}
	}

	private void ImpactEffects()
	{
		Vector3 position = ((hitPosition != Vector3.zero) ? (hitPosition - (hitPosition - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized) : (MonoSingleton<CameraController>.Instance.GetDefaultPos() + direction * 2.5f));
		if (primaryCharge > 0)
		{
			GameObject gameObject = Object.Instantiate((primaryCharge == 3) ? overPumpExplosion : pumpExplosion, position, Quaternion.LookRotation(direction));
			Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
			foreach (Explosion explosion in componentsInChildren)
			{
				explosion.sourceWeapon = base.gameObject;
				explosion.hitterWeapon = "hammer";
				if (primaryCharge == 2)
				{
					explosion.maxSize *= 2f;
				}
			}
			if (primaryCharge == 2 && gameObject.TryGetComponent<AudioSource>(out var component))
			{
				component.volume = 1f;
				component.SetPitch(component.GetPitch() - 0.4f);
			}
			primaryCharge = 0;
		}
		if (forceWeakHit || tier == 0)
		{
			anim.Play("Fire", -1, 0f);
		}
		else if (tier == 1)
		{
			anim.Play("FireStrong", -1, 0f);
		}
		else
		{
			anim.Play("FireStrongest", -1, 0f);
		}
		Object.Instantiate(hitImpactParticle[(!forceWeakHit) ? tier : 0], position, MonoSingleton<CameraController>.Instance.transform.rotation);
	}

	private void ThrowNade()
	{
		MonoSingleton<WeaponCharges>.Instance.shoAltNadeCharge = 0f;
		pulledOut = 0.3f;
		gunReady = false;
		aboutToSecondary = false;
		MonoSingleton<PlayerAnimations>.Instance?.Shoot();
		Vector3 vector = MonoSingleton<CameraController>.Instance.transform.forward;
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			vector = (targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
		}
		GameObject obj = Object.Instantiate(grenade, MonoSingleton<CameraController>.Instance.GetDefaultPos() + vector * 2f - MonoSingleton<CameraController>.Instance.transform.up * 0.5f, Random.rotation);
		if (obj.TryGetComponent<Rigidbody>(out var component))
		{
			component.AddForce(MonoSingleton<CameraController>.Instance.transform.forward * 3f + Vector3.up * 7.5f + (MonoSingleton<NewMovement>.Instance.ridingRocket ? MonoSingleton<NewMovement>.Instance.ridingRocket.rb.velocity : MonoSingleton<NewMovement>.Instance.rb.velocity), ForceMode.VelocityChange);
		}
		if (obj.TryGetComponent<Grenade>(out var component2))
		{
			component2.sourceWeapon = base.gameObject;
		}
		anim.Play("NadeSpawn", -1, 0f);
		Object.Instantiate<AudioSource>(nadeSpawnSound);
	}

	private void ShootSaw()
	{
		ShootSaw(noSaw: false);
	}

	private void ShootSaw(bool noSaw)
	{
		gunReady = true;
		base.transform.localPosition = wpos.currentDefault;
		if (!noSaw)
		{
			MonoSingleton<PlayerAnimations>.Instance?.Shoot();
			Vector3 vector = MonoSingleton<CameraController>.Instance.transform.forward;
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				vector = (targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos()).normalized;
			}
			Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + vector * 0.5f;
			if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), vector, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				position = hitInfo.point - vector * 5f;
			}
			Chainsaw chainsaw = Object.Instantiate(this.chainsaw, position, Random.rotation);
			chainsaw.weaponType = "hammer" + variation;
			chainsaw.CheckMultipleRicochets(onStart: true);
			chainsaw.sourceWeapon = gc.currentWeapon;
			chainsaw.attachedTransform = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			chainsaw.lineStartTransform = chainsawAttachPoint;
			chainsaw.GetComponent<Rigidbody>().AddForce(vector * (chargeForce + 10f) * 1.5f, ForceMode.VelocityChange);
			currentChainsaws.Add(chainsaw);
			Object.Instantiate<AudioSource>(nadeSpawnSound);
			if (chainsawBroken)
			{
				chainsawBroken = false;
				chainsawRenderer.material = chainsawMaterial;
				chainsawBrokenVibrate.enabled = false;
			}
		}
		chainsawBladeRenderer.material = chainsawBladeMaterial;
		chainsawBladeScroll.scrollSpeedX = 0f;
		chainsawAttachPoint.gameObject.SetActive(value: false);
		anim.Play("SawingShot");
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
		chargeForce = 0f;
	}

	private void Pump()
	{
		if (primaryCharge < 3)
		{
			primaryCharge++;
		}
		pulledOut = 0f;
		gunReady = false;
		aboutToSecondary = false;
		AudioSource component = Object.Instantiate(pumpChargeSound, base.transform.position, Quaternion.identity).GetComponent<AudioSource>();
		float num = primaryCharge;
		component.SetPitch(1f + num / 5f);
		component.Play(tracked: true);
		Object.Instantiate<AudioSource>(pump1Sound);
		anim.Play("Pump", -1, 0f);
	}

	private void Pump2Sound()
	{
		Object.Instantiate<AudioSource>(pump2Sound);
	}
}
