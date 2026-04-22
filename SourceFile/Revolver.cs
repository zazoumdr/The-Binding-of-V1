using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

public class Revolver : MonoBehaviour
{
	private InputManager inman;

	private WeaponIdentifier wid;

	public int gunVariation;

	public bool altVersion;

	private AudioSource gunAud;

	public AudioClip[] gunShots;

	private int currentGunShot;

	public GameObject gunBarrel;

	private bool gunReady;

	private bool shootReady = true;

	private bool pierceReady = true;

	public float shootCharge;

	public float pierceCharge;

	private bool chargingPierce;

	public float pierceShotCharge;

	public Vector3 shotHitPoint;

	public GameObject revolverBeam;

	public GameObject revolverBeamSuper;

	public AudioSource superGunSound;

	public RaycastHit hit;

	public RaycastHit[] allHits;

	private int currentHit;

	private int currentHitMultiplier;

	public float recoilFOV;

	public GameObject chargeEffect;

	private AudioSource ceaud;

	private Light celight;

	private GameObject camObj;

	private Camera cam;

	private CameraController cc;

	private Vector3 tempCamPos;

	public Vector3 beamReflectPos;

	private GameObject beamDirectionSetter;

	public MeshRenderer screenMR;

	private MaterialPropertyBlock screenProps;

	public Material batteryMat;

	public Texture2D batteryFull;

	public Texture2D batteryMid;

	public Texture2D batteryLow;

	public Texture2D[] batteryCharges;

	private AudioSource screenAud;

	public AudioClip chargedSound;

	public AudioClip chargingSound;

	public Transform twirlBone;

	private float latestTwirlRotation;

	private float twirlLevel;

	public bool twirlRecovery;

	public SpriteRenderer twirlSprite;

	public GameObject twirlShotSound;

	private GameObject currentDrip;

	public GameObject coin;

	public GameObject quickTwirlEffect;

	[HideInInspector]
	public RevolverCylinder cylinder;

	private SwitchMaterial rimLight;

	public GunControl gc;

	private Animator anim;

	private Punch punch;

	private NewMovement nmov;

	private WeaponPos wpos;

	public Image[] coinPanels;

	public bool[] coinPanelsCharged;

	private WeaponCharges wc;

	private CameraFrustumTargeter targeter;

	private float coinCharge = 400f;

	private void Start()
	{
		targeter = MonoSingleton<CameraFrustumTargeter>.Instance;
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		gunReady = false;
		cam = MonoSingleton<CameraController>.Instance.GetComponent<Camera>();
		camObj = cam.gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		nmov = MonoSingleton<NewMovement>.Instance;
		shootCharge = 0f;
		pierceShotCharge = 0f;
		pierceCharge = 100f;
		pierceReady = false;
		gunAud = GetComponent<AudioSource>();
		if (gunVariation == 0)
		{
			screenAud = screenMR.gameObject.GetComponent<AudioSource>();
		}
		else
		{
			screenAud = ((Component)(object)GetComponentInChildren<Canvas>()).GetComponent<AudioSource>();
		}
		if ((bool)chargeEffect)
		{
			ceaud = chargeEffect.GetComponent<AudioSource>();
			celight = chargeEffect.GetComponent<Light>();
		}
		if (gunVariation == 0)
		{
			screenAud.clip = chargingSound;
			screenAud.loop = true;
			screenAud.SetPitch(1f);
			screenAud.volume = 0.25f;
			screenAud.Play(tracked: true);
		}
		cylinder = GetComponentInChildren<RevolverCylinder>();
		gc = GetComponentInParent<GunControl>();
		beamDirectionSetter = new GameObject();
		anim = GetComponentInChildren<Animator>();
		wc = MonoSingleton<WeaponCharges>.Instance;
		wpos = GetComponent<WeaponPos>();
		if (wid.delay != 0f && gunVariation == 0)
		{
			pierceCharge = wc.rev0charge;
		}
	}

	private void OnDisable()
	{
		if (base.gameObject.scene.isLoaded)
		{
			if (wc == null)
			{
				wc = MonoSingleton<WeaponCharges>.Instance;
			}
			if (gunVariation == 0)
			{
				wc.rev0alt = altVersion;
				wc.rev0charge = pierceCharge;
			}
			pierceShotCharge = 0f;
			gunReady = false;
		}
	}

	private void OnEnable()
	{
		if (wc == null)
		{
			wc = MonoSingleton<WeaponCharges>.Instance;
		}
		shootCharge = 100f;
		if (gunVariation == 0)
		{
			pierceCharge = wc.rev0charge;
		}
		else
		{
			pierceCharge = 100f;
			pierceReady = true;
			CheckCoinCharges();
		}
		if (gunVariation == 2)
		{
			wc.rev2alt = altVersion;
		}
		if (altVersion)
		{
			if (!(Object)(object)anim)
			{
				anim = GetComponentInChildren<Animator>();
			}
			if (wc.revaltpickupcharges[gunVariation] > 0f)
			{
				anim.SetBool("SlowPickup", true);
			}
			else
			{
				anim.SetBool("SlowPickup", false);
			}
		}
		if (screenProps == null)
		{
			screenProps = new MaterialPropertyBlock();
		}
		if ((bool)screenMR)
		{
			screenMR.GetPropertyBlock(screenProps);
		}
		gunReady = false;
	}

	private void Update()
	{
		if (!shootReady)
		{
			if (shootCharge + 200f * Time.deltaTime < 100f)
			{
				shootCharge += 200f * Time.deltaTime;
			}
			else
			{
				shootCharge = 100f;
				shootReady = true;
			}
		}
		if (!pierceReady)
		{
			if (gunVariation == 0)
			{
				if (NoWeaponCooldown.NoCooldown)
				{
					pierceCharge = 100f;
				}
				float num = 1f;
				if (altVersion)
				{
					num = 0.5f;
				}
				if (pierceCharge + 40f * Time.deltaTime < 100f)
				{
					pierceCharge += 40f * Time.deltaTime * num;
				}
				else
				{
					pierceCharge = 100f;
					pierceReady = true;
					screenAud.clip = chargedSound;
					screenAud.loop = false;
					screenAud.volume = 0.35f;
					screenAud.SetPitch(Random.Range(1f, 1.1f));
					screenAud.Play(tracked: true);
				}
				if (cylinder.spinSpeed > 0f)
				{
					cylinder.spinSpeed = Mathf.MoveTowards(cylinder.spinSpeed, 0f, Time.deltaTime * 50f);
				}
				if (pierceCharge < 50f)
				{
					screenProps.SetTexture("_MainTex", batteryLow);
					screenProps.SetColor("_Color", Color.red);
				}
				else if (pierceCharge < 100f)
				{
					screenProps.SetTexture("_MainTex", batteryMid);
					screenProps.SetColor("_Color", Color.yellow);
				}
				else
				{
					screenProps.SetTexture("_MainTex", batteryFull);
				}
			}
			else if (pierceCharge + 480f * Time.deltaTime < 100f)
			{
				pierceCharge += 480f * Time.deltaTime;
			}
			else
			{
				pierceCharge = 100f;
				pierceReady = true;
			}
		}
		else if (gunVariation == 0)
		{
			if (pierceShotCharge != 0f)
			{
				if (pierceShotCharge < 50f)
				{
					screenProps.SetTexture("_MainTex", batteryCharges[0]);
				}
				else if (pierceShotCharge < 100f)
				{
					screenProps.SetTexture("_MainTex", batteryCharges[1]);
				}
				else
				{
					screenProps.SetTexture("_MainTex", batteryCharges[2]);
				}
				base.transform.localPosition = new Vector3(wpos.currentDefault.x + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f), wpos.currentDefault.y + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f), wpos.currentDefault.z + pierceShotCharge / 250f * Random.Range(-0.05f, 0.05f));
				cylinder.spinSpeed = pierceShotCharge;
			}
			else
			{
				screenProps.SetTexture("_MainTex", batteryFull);
				if (cylinder.spinSpeed != 0f)
				{
					cylinder.spinSpeed = 0f;
				}
			}
		}
		if (gc.activated)
		{
			if (gunVariation != 1 && gunReady)
			{
				float num2 = ((gunVariation == 0) ? 175 : (altVersion ? 750 : 75));
				if ((inman.InputSource.Fire2.WasCanceledThisFrame || (!inman.PerformingCheatMenuCombo() && !GameStateManager.Instance.PlayerInputLocked && inman.InputSource.Fire1.IsPressed)) && shootReady && ((gunVariation == 0) ? (pierceShotCharge == 100f) : (pierceShotCharge >= 25f)))
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot(2);
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot2", wid.delay);
					}
				}
				else if (!inman.PerformingCheatMenuCombo() && inman.InputSource.Fire1.IsPressed && shootReady && !chargingPierce)
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot();
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot", wid.delay);
					}
				}
				else if (inman.InputSource.Fire2.IsPressed && (gunVariation == 2 || shootReady) && ((gunVariation == 0) ? pierceReady : (coinCharge >= (float)(altVersion ? 300 : 100))))
				{
					if (!chargingPierce && !twirlRecovery)
					{
						latestTwirlRotation = 0f;
					}
					chargingPierce = true;
					if (pierceShotCharge + num2 * Time.deltaTime < 100f)
					{
						pierceShotCharge += num2 * Time.deltaTime;
					}
					else
					{
						pierceShotCharge = 100f;
					}
				}
				else
				{
					if (chargingPierce)
					{
						twirlRecovery = true;
					}
					chargingPierce = false;
					if (pierceShotCharge - num2 * Time.deltaTime > 0f)
					{
						pierceShotCharge -= num2 * Time.deltaTime;
					}
					else
					{
						pierceShotCharge = 0f;
					}
				}
			}
			else if (gunVariation == 1)
			{
				if (inman.InputSource.Fire2.WasPerformedThisFrame && pierceReady && coinCharge >= 100f)
				{
					cc.StopShake();
					if (!wid || wid.delay == 0f)
					{
						wc.rev1charge -= 100f;
					}
					if (!wid || wid.delay == 0f)
					{
						ThrowCoin();
					}
					else
					{
						Invoke("ThrowCoin", wid.delay);
						pierceReady = false;
						pierceCharge = 0f;
					}
				}
				else if (gunReady && !inman.PerformingCheatMenuCombo() && inman.InputSource.Fire1.IsPressed && shootReady)
				{
					if (!wid || wid.delay == 0f)
					{
						Shoot();
					}
					else
					{
						shootReady = false;
						shootCharge = 0f;
						Invoke("DelayedShoot", wid.delay);
					}
					if ((bool)(Object)(object)ceaud && ceaud.volume != 0f)
					{
						ceaud.volume = 0f;
					}
				}
			}
		}
		if ((bool)celight)
		{
			if (pierceShotCharge == 0f && celight.enabled)
			{
				celight.enabled = false;
			}
			else if (pierceShotCharge != 0f)
			{
				celight.enabled = true;
				celight.range = pierceShotCharge * 0.01f;
			}
		}
		if (gunVariation != 1)
		{
			if (gunVariation == 0)
			{
				chargeEffect.transform.localScale = Vector3.one * pierceShotCharge * 0.02f;
				ceaud.SetPitch(pierceShotCharge * 0.005f);
			}
			ceaud.volume = 0.25f + pierceShotCharge * 0.005f;
			if ((bool)MonoSingleton<RumbleManager>.Instance)
			{
				MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.RevolverCharge, ((Component)(object)ceaud).gameObject).intensityMultiplier = pierceShotCharge / 250f;
			}
		}
		if (gunVariation != 0)
		{
			CheckCoinCharges();
		}
		else if (pierceCharge == 100f && (bool)MonoSingleton<ColorBlindSettings>.Instance)
		{
			screenProps.SetColor("_Color", MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation]);
		}
		if (gunVariation == 0)
		{
			screenMR.SetPropertyBlock(screenProps);
		}
	}

	private void LateUpdate()
	{
		if (gunVariation != 2)
		{
			return;
		}
		if (chargingPierce || twirlRecovery)
		{
			anim.SetBool("Spinning", true);
			bool flag = latestTwirlRotation < 0f;
			if (chargingPierce)
			{
				twirlLevel = Mathf.Min(3f, Mathf.Floor(pierceShotCharge / 25f)) + 1f;
			}
			else
			{
				twirlLevel = Mathf.MoveTowards(twirlLevel, 0.1f, Time.deltaTime * 100f * twirlLevel);
			}
			latestTwirlRotation += 1200f * (twirlLevel / 3f + 0.5f) * Time.deltaTime;
			if ((bool)twirlSprite)
			{
				twirlSprite.color = new Color(1f, 1f, 1f, Mathf.Min(2f, Mathf.Floor(pierceShotCharge / 25f)) / 3f);
			}
			if (!ceaud.isPlaying)
			{
				ceaud.Play(tracked: true);
			}
			ceaud.SetPitch(0.5f + twirlLevel / 2f);
			if (twirlRecovery && flag && latestTwirlRotation >= 0f)
			{
				latestTwirlRotation = 0f;
				twirlRecovery = false;
				if ((bool)twirlSprite)
				{
					twirlSprite.color = new Color(1f, 1f, 1f, 0f);
				}
			}
			else
			{
				while (latestTwirlRotation > 180f)
				{
					latestTwirlRotation -= 360f;
				}
				twirlBone.localRotation = Quaternion.Euler(twirlBone.localRotation.eulerAngles + (altVersion ? Vector3.left : Vector3.forward) * latestTwirlRotation);
			}
			anim.SetFloat("TwirlSpeed", twirlLevel / 3f);
			if ((bool)wid && wid.delay != 0f && !MonoSingleton<NewMovement>.Instance.gc.onGround)
			{
				MonoSingleton<NewMovement>.Instance.rb.AddForce(MonoSingleton<CameraController>.Instance.transform.up * 400f * twirlLevel * Time.deltaTime, ForceMode.Acceleration);
			}
		}
		else
		{
			anim.SetBool("Spinning", false);
			if ((bool)twirlSprite)
			{
				twirlSprite.color = new Color(1f, 1f, 1f, 0f);
			}
			ceaud.Stop();
		}
	}

	private void Shoot(int shotType = 1)
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		cc.StopShake();
		shootReady = false;
		shootCharge = 0f;
		MonoSingleton<PlayerAnimations>.Instance?.Shoot(altVersion ? 0.5f : 1f);
		if (altVersion)
		{
			MonoSingleton<WeaponCharges>.Instance.revaltpickupcharges[gunVariation] = 2f;
		}
		AnimatorStateInfo currentAnimatorStateInfo;
		switch (shotType)
		{
		case 1:
		{
			GameObject gameObject2 = Object.Instantiate(revolverBeam, cc.transform.position, cc.transform.rotation);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				gameObject2.transform.LookAt(targeter.CurrentTarget.bounds.center);
			}
			RevolverBeam component2 = gameObject2.GetComponent<RevolverBeam>();
			component2.sourceWeapon = gc.currentWeapon;
			component2.alternateStartPoint = gunBarrel.transform.position;
			component2.gunVariation = gunVariation;
			currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("PickUp"))
			{
				component2.quickDraw = true;
			}
			currentGunShot = Random.Range(0, gunShots.Length);
			gunAud.clip = gunShots[currentGunShot];
			gunAud.volume = 0.55f;
			gunAud.SetPitch(Random.Range(0.9f, 1.1f));
			gunAud.Play(tracked: true);
			cam.fieldOfView += cc.defaultFov / 40f;
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFire, base.gameObject);
			break;
		}
		case 2:
		{
			GameObject gameObject = Object.Instantiate(revolverBeamSuper, cc.transform.position, cc.transform.rotation);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
			}
			RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
			component.sourceWeapon = gc.currentWeapon;
			component.alternateStartPoint = gunBarrel.transform.position;
			component.gunVariation = gunVariation;
			if (gunVariation == 2)
			{
				component.ricochetAmount = Mathf.Min(3, Mathf.FloorToInt(pierceShotCharge / 25f));
			}
			pierceShotCharge = 0f;
			currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("PickUp"))
			{
				component.quickDraw = true;
			}
			pierceReady = false;
			pierceCharge = 0f;
			if (gunVariation == 0)
			{
				screenAud.clip = chargingSound;
				screenAud.loop = true;
				if (altVersion)
				{
					screenAud.SetPitch(0.5f);
				}
				else
				{
					screenAud.SetPitch(1f);
				}
				screenAud.volume = 0.55f;
				screenAud.Play(tracked: true);
			}
			else if (!wid || wid.delay == 0f)
			{
				wc.rev2charge -= (altVersion ? 300 : 100);
			}
			if ((bool)(Object)(object)superGunSound)
			{
				Object.Instantiate<AudioSource>(superGunSound);
			}
			if (gunVariation == 2 && (bool)twirlShotSound)
			{
				Object.Instantiate(twirlShotSound, base.transform.position, Quaternion.identity);
			}
			cam.fieldOfView += cc.defaultFov / 20f;
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFireStrong, base.gameObject);
			break;
		}
		}
		if (!altVersion)
		{
			cylinder.DoTurn();
		}
		anim.SetFloat("RandomChance", Random.Range(0f, 1f));
		if (shotType == 1)
		{
			anim.SetTrigger("Shoot");
		}
		else
		{
			anim.SetTrigger("ChargeShoot");
		}
		gunReady = false;
	}

	private void ThrowCoin()
	{
		if (punch == null || !punch.gameObject.activeInHierarchy)
		{
			punch = MonoSingleton<FistControl>.Instance.currentPunch;
		}
		if ((bool)punch)
		{
			punch.CoinFlip();
		}
		MonoSingleton<PlayerAnimations>.Instance?.CoinToss();
		Coin component = Object.Instantiate(coin, camObj.transform.position + camObj.transform.up * -0.5f, camObj.transform.rotation).GetComponent<Coin>();
		component.sourceWeapon = gc.currentWeapon;
		Vector3 vector = Vector3.down;
		if (nmov.rb.TryGetCustomGravity(out var gravity) && nmov.rb.GetCustomGravityMode())
		{
			vector = gravity;
		}
		component.rb.SetCustomGravity(vector.normalized * Physics.gravity.magnitude);
		component.rb.SetCustomGravityMode(useCustomGravity: true);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.CoinToss);
		component.rb.AddForce(camObj.transform.forward * 20f + -vector.normalized * 15f + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(trueVelocity: true), ForceMode.VelocityChange);
		pierceCharge = 0f;
		pierceReady = false;
	}

	private void ReadyToShoot()
	{
		shootReady = true;
	}

	public void Punch()
	{
		gunReady = false;
		anim.SetTrigger("ChargeShoot");
	}

	public void ReadyGun()
	{
		gunReady = true;
	}

	public void Click()
	{
		if (altVersion)
		{
			MonoSingleton<WeaponCharges>.Instance.revaltpickupcharges[gunVariation] = 0f;
		}
		if (gunVariation == 2)
		{
			chargingPierce = false;
			twirlRecovery = false;
		}
	}

	public void InstaClick()
	{
		if (altVersion)
		{
			for (int i = 0; i < MonoSingleton<WeaponCharges>.Instance.revaltpickupcharges.Length; i++)
			{
				MonoSingleton<WeaponCharges>.Instance.revaltpickupcharges[i] = 0f;
			}
			Object.Instantiate(quickTwirlEffect, cylinder.transform.position, base.transform.rotation).transform.SetParent(cylinder.transform);
			anim.Rebind();
			anim.Play("ShootTwirl", -1, 0f);
		}
	}

	public void MaxCharge()
	{
		if (gunVariation == 0)
		{
			pierceCharge = 100f;
		}
		else
		{
			CheckCoinCharges();
		}
	}

	private void DelayedShoot()
	{
		Shoot();
	}

	private void DelayedShoot2()
	{
		Shoot(2);
	}

	private void CheckCoinCharges()
	{
		if (coinPanelsCharged == null || coinPanelsCharged.Length == 0)
		{
			coinPanelsCharged = new bool[coinPanels.Length];
		}
		coinCharge = ((gunVariation == 1) ? wc.rev1charge : wc.rev2charge);
		for (int i = 0; i < coinPanels.Length; i++)
		{
			if (altVersion && gunVariation == 2)
			{
				coinPanels[i].fillAmount = coinCharge / 300f;
			}
			else
			{
				coinPanels[i].fillAmount = coinCharge / 100f - (float)i;
			}
			if (coinPanels[i].fillAmount < 1f)
			{
				((Graphic)coinPanels[i]).color = ((gunVariation == 1) ? Color.red : Color.gray);
				coinPanelsCharged[i] = false;
				continue;
			}
			if ((bool)MonoSingleton<ColorBlindSettings>.Instance && ((Graphic)coinPanels[i]).color != MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation])
			{
				((Graphic)coinPanels[i]).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[gunVariation];
			}
			if (!coinPanelsCharged[i] && (!wid || wid.delay == 0f))
			{
				if (!(Object)(object)screenAud)
				{
					screenAud = ((Component)(object)GetComponentInChildren<Canvas>()).GetComponent<AudioSource>();
				}
				screenAud.SetPitch(1f + (float)i / 2f);
				screenAud.Play(tracked: true);
				coinPanelsCharged[i] = true;
			}
		}
	}
}
