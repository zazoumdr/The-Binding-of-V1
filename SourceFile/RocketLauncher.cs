using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class RocketLauncher : MonoBehaviour
{
	public int variation;

	public GameObject rocket;

	public GameObject clunkSound;

	public float rateOfFire;

	private float cooldown = 0.25f;

	private bool lookingForValue;

	private AudioSource aud;

	private Animator anim;

	private WeaponIdentifier wid;

	public Transform shootPoint;

	public GameObject muzzleFlash;

	[SerializeField]
	private Image timerMeter;

	[SerializeField]
	private RectTransform timerArm;

	[SerializeField]
	private Image[] variationColorables;

	private float[] colorablesTransparencies;

	private WeaponPos wpos;

	[Header("Freeze variation")]
	[SerializeField]
	private AudioSource timerFreezeSound;

	[SerializeField]
	private AudioSource timerUnfreezeSound;

	[SerializeField]
	private AudioSource timerTickSound;

	[HideInInspector]
	public AudioSource currentTimerTickSound;

	[SerializeField]
	private AudioSource timerWindupSound;

	private float lastKnownTimerAmount;

	[Header("Cannonball variation")]
	public Rigidbody cannonBall;

	[SerializeField]
	private AudioSource chargeSound;

	private float cbCharge;

	private bool firingCannonball;

	[Header("Napalm variation")]
	[SerializeField]
	private Rigidbody napalmProjectile;

	private float napalmProjectileCooldown;

	[SerializeField]
	private Transform napalmMuzzleFlashTransform;

	[SerializeField]
	private ParticleSystem napalmMuzzleFlashParticles;

	[SerializeField]
	private AudioSource[] napalmMuzzleFlashSounds;

	[SerializeField]
	private AudioSource napalmStopSound;

	[SerializeField]
	private AudioSource napalmNoAmmoSound;

	private bool firingNapalm;

	private TimeSince sinceEquipped;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		wid = GetComponent<WeaponIdentifier>();
		anim = GetComponent<Animator>();
		wpos = GetComponent<WeaponPos>();
		sinceEquipped = 0f;
		colorablesTransparencies = new float[variationColorables.Length];
		for (int i = 0; i < variationColorables.Length; i++)
		{
			colorablesTransparencies[i] = ((Graphic)variationColorables[i]).color.a;
		}
		if (variation == 0 && (!wid || wid.delay == 0f))
		{
			MonoSingleton<WeaponCharges>.Instance.rocketLauncher = this;
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<WeaponCharges>.Instance.rocketset)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketset = false;
			if (MonoSingleton<WeaponCharges>.Instance.rocketcharge < 0.25f)
			{
				cooldown = 0.25f;
			}
			else
			{
				cooldown = MonoSingleton<WeaponCharges>.Instance.rocketcharge;
			}
		}
		else
		{
			lookingForValue = true;
		}
		sinceEquipped = 0f;
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			if (!NoWeaponCooldown.NoCooldown)
			{
				MonoSingleton<WeaponCharges>.Instance.rocketcharge = cooldown;
			}
			MonoSingleton<WeaponCharges>.Instance.rocketset = true;
			cbCharge = 0f;
			firingCannonball = false;
			sinceEquipped = 0f;
		}
	}

	private void OnDestroy()
	{
		if ((bool)(Object)(object)currentTimerTickSound)
		{
			Object.Destroy(((Component)(object)currentTimerTickSound).gameObject);
		}
	}

	private void Update()
	{
		if (!MonoSingleton<ColorBlindSettings>.Instance)
		{
			return;
		}
		Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		float num = 1f;
		if (MonoSingleton<WeaponCharges>.Instance.rocketset && lookingForValue)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketset = false;
			lookingForValue = false;
			if (MonoSingleton<WeaponCharges>.Instance.rocketcharge < 0.25f)
			{
				cooldown = 0.25f;
			}
			else
			{
				cooldown = MonoSingleton<WeaponCharges>.Instance.rocketcharge;
			}
		}
		bool flag = (float)sinceEquipped < 0.25f && (float)MonoSingleton<WeaponCharges>.Instance.sinceAltFirePressed > 0.025f;
		bool flag2 = (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !flag) || MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame;
		if (flag2 && (float)sinceEquipped < 0.25f)
		{
			sinceEquipped = 0.25f;
		}
		if (variation == 1)
		{
			if (MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				if ((bool)timerArm)
				{
					timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge));
				}
				if ((bool)(Object)(object)timerMeter)
				{
					timerMeter.fillAmount = MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge;
				}
				if (lastKnownTimerAmount != MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge && (!wid || wid.delay == 0f))
				{
					for (float num2 = 4f; num2 > 0f; num2 -= 1f)
					{
						if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge >= num2 / 4f && lastKnownTimerAmount < num2 / 4f)
						{
							AudioSource val = Object.Instantiate<AudioSource>(timerWindupSound);
							val.SetPitch(1.6f + num2 * 0.1f);
							if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge < 1f)
							{
								val.volume /= 2f;
							}
							break;
						}
					}
					lastKnownTimerAmount = MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge;
				}
				if (flag2 && !MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge >= 1f)
				{
					if (!wid || wid.delay == 0f)
					{
						if (!chargeSound.isPlaying)
						{
							chargeSound.Play(tracked: true);
						}
						chargeSound.SetPitch(cbCharge + 0.5f);
					}
					cbCharge = Mathf.MoveTowards(cbCharge, 1f, Time.deltaTime);
					base.transform.localPosition = new Vector3(wpos.currentDefault.x + Random.Range(cbCharge / 100f * -1f, cbCharge / 100f), wpos.currentDefault.y + Random.Range(cbCharge / 100f * -1f, cbCharge / 100f), wpos.currentDefault.z + Random.Range(cbCharge / 100f * -1f, cbCharge / 100f));
					if ((bool)timerArm)
					{
						timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, cbCharge));
					}
					if ((bool)(Object)(object)timerMeter)
					{
						timerMeter.fillAmount = cbCharge;
					}
				}
				else if (!flag && cbCharge > 0f && !firingCannonball)
				{
					chargeSound.Stop();
					if (!wid || wid.delay == 0f)
					{
						ShootCannonball();
					}
					else
					{
						Invoke("ShootCannonball", wid.delay);
						firingCannonball = true;
					}
					MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge = 0f;
				}
			}
			if (cbCharge > 0f)
			{
				color = Color.Lerp(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation], Color.red, cbCharge);
			}
			else if (MonoSingleton<WeaponCharges>.Instance.rocketCannonballCharge < 1f)
			{
				num = 0.5f;
			}
		}
		else if (variation == 0)
		{
			if (MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime > 0f && MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && !GameStateManager.Instance.PlayerInputLocked && (!wid || !wid.duplicate))
			{
				if (MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
				{
					UnfreezeRockets();
				}
				else
				{
					FreezeRockets();
				}
			}
			if ((bool)timerArm)
			{
				timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f));
			}
			if ((bool)(Object)(object)timerMeter)
			{
				timerMeter.fillAmount = MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f;
			}
			if (lastKnownTimerAmount != MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime && (!wid || wid.delay == 0f))
			{
				for (float num3 = 4f; num3 > 0f; num3 -= 1f)
				{
					if (MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime / 5f >= num3 / 4f && lastKnownTimerAmount / 5f < num3 / 4f)
					{
						Object.Instantiate<AudioSource>(timerWindupSound).SetPitch(0.6f + num3 * 0.1f);
						break;
					}
				}
				lastKnownTimerAmount = MonoSingleton<WeaponCharges>.Instance.rocketFreezeTime;
			}
		}
		else if (variation == 2)
		{
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && !GameStateManager.Instance.PlayerInputLocked && MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel > 0f && (firingNapalm || MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel >= 0.25f))
			{
				if (cooldown < 0.5f)
				{
					if (!firingNapalm)
					{
						napalmMuzzleFlashTransform.localScale = Vector3.one * 3f;
						napalmMuzzleFlashParticles.Play();
						AudioSource[] array = napalmMuzzleFlashSounds;
						foreach (AudioSource obj in array)
						{
							obj.SetPitch(Random.Range(0.9f, 1.1f));
							obj.Play(tracked: true);
						}
					}
					firingNapalm = true;
				}
			}
			else if (firingNapalm)
			{
				firingNapalm = false;
				napalmMuzzleFlashParticles.Stop();
				AudioSource[] array = napalmMuzzleFlashSounds;
				foreach (AudioSource val2 in array)
				{
					if (val2.loop)
					{
						val2.Stop();
					}
				}
				napalmStopSound.SetPitch(Random.Range(0.9f, 1.1f));
				napalmStopSound.Play(tracked: true);
			}
			else if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel < 0.25f)
			{
				napalmNoAmmoSound.Play(tracked: true);
			}
			if (!firingNapalm && napalmMuzzleFlashTransform.localScale != Vector3.zero)
			{
				napalmMuzzleFlashTransform.localScale = Vector3.MoveTowards(napalmMuzzleFlashTransform.localScale, Vector3.zero, Time.deltaTime * 9f);
			}
			if ((bool)timerArm)
			{
				timerArm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(360f, 0f, MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel));
			}
			if ((bool)(Object)(object)timerMeter)
			{
				timerMeter.fillAmount = MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel;
			}
			if (lastKnownTimerAmount != MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel && (!wid || wid.delay == 0f))
			{
				if (MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel >= 0.25f && lastKnownTimerAmount < 0.25f)
				{
					Object.Instantiate<AudioSource>(timerWindupSound);
				}
				lastKnownTimerAmount = MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel;
			}
			if (!firingNapalm && MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel < 0.25f)
			{
				color = Color.grey;
			}
		}
		if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
		}
		else if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && MonoSingleton<GunControl>.Instance.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (!wid || wid.delay == 0f)
			{
				Shoot();
			}
			else
			{
				Invoke("Shoot", wid.delay);
				cooldown = 1f;
			}
		}
		for (int j = 0; j < variationColorables.Length; j++)
		{
			((Graphic)variationColorables[j]).color = new Color(color.r, color.g, color.b, colorablesTransparencies[j] * num);
		}
	}

	private void FixedUpdate()
	{
		if (napalmProjectileCooldown > 0f)
		{
			napalmProjectileCooldown = Mathf.MoveTowards(napalmProjectileCooldown, 0f, Time.fixedDeltaTime);
		}
		if (firingNapalm && napalmProjectileCooldown == 0f)
		{
			ShootNapalm();
		}
	}

	public void Shoot()
	{
		CameraController? instance = MonoSingleton<CameraController>.Instance;
		Vector3 position = instance.transform.position;
		if ((bool)(Object)(object)aud)
		{
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.Play(tracked: true);
		}
		if (variation == 1 && cbCharge > 0f)
		{
			chargeSound.Stop();
			cbCharge = 0f;
		}
		shootPoint.transform.GetPositionAndRotation(out var position2, out var rotation);
		Vector3 vector = position2 - position;
		PortalPhysicsV2.Raycast(position, vector.normalized, vector.magnitude, default(LayerMask), out var _, out var portalTraversals, out var _);
		if (portalTraversals.Length != 0)
		{
			Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
			position2 = travelMatrix.MultiplyPoint3x4(position2);
			rotation = travelMatrix.rotation * rotation;
		}
		Object.Instantiate(muzzleFlash, position2, rotation);
		MonoSingleton<PlayerAnimations>.Instance?.Shoot(0.5f);
		if (!firingNapalm)
		{
			anim.SetTrigger("Fire");
		}
		cooldown = rateOfFire;
		instance.transform.GetPositionAndRotation(out var position3, out var rotation2);
		GameObject gameObject = Object.Instantiate(rocket, position3, rotation2);
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			gameObject.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		Grenade component = gameObject.GetComponent<Grenade>();
		if ((bool)component)
		{
			component.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
		}
		instance.CameraShake(0.75f);
		MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFire, base.gameObject);
	}

	public void ShootCannonball()
	{
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		CameraController? instance = MonoSingleton<CameraController>.Instance;
		Vector3 position = instance.transform.position;
		Vector3 forward = instance.transform.forward;
		if ((bool)(Object)(object)aud)
		{
			aud.SetPitch(Random.Range(0.6f, 0.8f));
			aud.Play(tracked: true);
		}
		base.transform.localPosition = wpos.currentDefault;
		shootPoint.transform.GetPositionAndRotation(out var position2, out var rotation);
		Vector3 vector = position2 - position;
		PortalPhysicsV2.Raycast(position, vector.normalized, vector.magnitude, default(LayerMask), out var hitInfo, out var portalTraversals, out var endPoint);
		if (portalTraversals.Length != 0)
		{
			Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
			position2 = travelMatrix.MultiplyPoint3x4(position2);
			rotation = travelMatrix.rotation * rotation;
		}
		Object.Instantiate(muzzleFlash, position2, rotation);
		MonoSingleton<PlayerAnimations>.Instance?.Shoot(0.5f);
		anim.SetTrigger("Fire");
		cooldown = rateOfFire;
		Vector3 vector2 = position + forward;
		Vector3 vector3 = forward;
		Quaternion quaternion = base.transform.rotation;
		PortalPhysicsV2.Raycast(position, forward, 1f, LayerMaskDefaults.Get(LMD.Environment), out hitInfo, out var portalTraversals2, out endPoint);
		if (portalTraversals2.Length != 0)
		{
			if (portalTraversals2.AllHasFlag(PortalTravellerFlags.PlayerProjectile, out var blocked))
			{
				Matrix4x4 travelMatrix2 = PortalUtils.GetTravelMatrix(portalTraversals2);
				quaternion = travelMatrix2.rotation * quaternion;
				vector2 = travelMatrix2.MultiplyPoint3x4(vector2);
			}
			else if (blocked)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals2[0];
				NativePortalTransform nativePortalTransform = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side);
				vector2 = nativePortalTransform.GetPositionInFront(portalTraversalV.entrancePoint, 0.01f);
				quaternion = Quaternion.LookRotation(Vector3.Reflect(forward, float3.op_Implicit(nativePortalTransform.back)).normalized);
			}
		}
		Rigidbody rigidbody = Object.Instantiate(cannonBall, vector2, quaternion);
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			rigidbody.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		rigidbody.velocity = (1f - cbCharge) * MonoSingleton<NewMovement>.Instance.rb.velocity + vector3 * Mathf.Max(15f, cbCharge * 150f);
		if (rigidbody.TryGetComponent<Cannonball>(out var component))
		{
			component.sourceWeapon = MonoSingleton<GunControl>.Instance.currentWeapon;
		}
		instance.CameraShake(0.75f);
		cbCharge = 0f;
		firingCannonball = false;
	}

	public void ShootNapalm()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		CameraController? instance = MonoSingleton<CameraController>.Instance;
		Vector3 position = instance.transform.position;
		Vector3 forward = instance.transform.forward;
		Quaternion rotation = instance.transform.rotation;
		anim.SetTrigger("Spray");
		napalmProjectileCooldown = 0.02f;
		if (wid.delay == 0f)
		{
			MonoSingleton<WeaponCharges>.Instance.rocketNapalmFuel -= 0.015f;
		}
		Vector3 vector = position + forward;
		Quaternion quaternion = rotation;
		PortalPhysicsV2.Raycast(position, forward, 1f, LayerMaskDefaults.Get(LMD.Environment), out var _, out var portalTraversals, out var _);
		if (portalTraversals.Length != 0)
		{
			if (portalTraversals.AllHasFlag(PortalTravellerFlags.PlayerProjectile, out var blocked))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
				quaternion = travelMatrix.rotation * quaternion;
				vector = travelMatrix.MultiplyPoint3x4(vector);
			}
			else if (blocked)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[0];
				NativePortalTransform nativePortalTransform = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side);
				vector = nativePortalTransform.GetPositionInFront(portalTraversalV.entrancePoint, 0.01f);
				quaternion = Quaternion.LookRotation(Vector3.Reflect(forward, float3.op_Implicit(nativePortalTransform.back)).normalized);
			}
		}
		Rigidbody rigidbody = Object.Instantiate(napalmProjectile, vector, quaternion);
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			rigidbody.transform.LookAt(MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center);
		}
		rigidbody.transform.Rotate(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
		rigidbody.velocity = rigidbody.transform.forward * 150f;
		instance.CameraShake(0.15f);
	}

	public void FreezeRockets()
	{
		MonoSingleton<WeaponCharges>.Instance.rocketFrozen = true;
		if (!wid || wid.delay == 0f)
		{
			MonoSingleton<WeaponCharges>.Instance.timeSinceIdleFrozen = 0f;
			Object.Instantiate<AudioSource>(timerFreezeSound);
			if (!(Object)(object)currentTimerTickSound)
			{
				currentTimerTickSound = Object.Instantiate<AudioSource>(timerTickSound);
			}
		}
	}

	public void UnfreezeRockets()
	{
		MonoSingleton<WeaponCharges>.Instance.rocketFrozen = false;
		if (!wid || wid.delay == 0f)
		{
			MonoSingleton<WeaponCharges>.Instance.canAutoUnfreeze = false;
			Object.Instantiate<AudioSource>(timerUnfreezeSound);
			if ((bool)(Object)(object)currentTimerTickSound)
			{
				Object.Destroy(((Component)(object)currentTimerTickSound).gameObject);
			}
		}
	}

	public void Clunk(float pitch)
	{
		GameObject obj = Object.Instantiate(clunkSound, base.transform.position, Quaternion.identity);
		MonoSingleton<CameraController>.Instance.CameraShake(0.25f);
		if (obj.TryGetComponent<AudioSource>(out var component))
		{
			component.SetPitch(Random.Range(pitch - 0.1f, pitch + 0.1f));
		}
	}
}
