using System.Collections.Generic;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Shotgun : MonoBehaviour
{
	private InputManager inman;

	private WeaponIdentifier wid;

	private AudioSource gunAud;

	public AudioClip shootSound;

	public AudioClip shootSound2;

	public AudioClip clickSound;

	public AudioClip clickChargeSound;

	public AudioClip smackSound;

	public AudioClip pump1sound;

	public AudioClip pump2sound;

	public int variation;

	public GameObject bullet;

	public GameObject grenade;

	public float spread;

	private bool smallSpread;

	private Animator anim;

	private GameObject cam;

	private CameraController cc;

	private GunControl gc;

	private bool gunReady;

	public Transform[] shootPoints;

	public GameObject muzzleFlash;

	public SkinnedMeshRenderer heatSinkSMR;

	private Color tempColor;

	private bool releasingHeat;

	[SerializeField]
	private ParticleSystem[] heatReleaseParticles;

	private AudioSource heatSinkAud;

	private PhysicsCastResult[] rhits;

	private bool charging;

	private float grenadeForce;

	private Vector3 grenadeVector;

	private Slider chargeSlider;

	public Image sliderFill;

	public GameObject grenadeSoundBubble;

	public GameObject chargeSoundBubble;

	private AudioSource tempChargeSound;

	[HideInInspector]
	public int primaryCharge;

	private bool cockedBack;

	public GameObject explosion;

	public GameObject pumpChargeSound;

	public GameObject warningBeep;

	private float timeToBeep;

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

	private WeaponPos wpos;

	private CameraFrustumTargeter targeter;

	private bool meterOverride;

	private bool resettingCores;

	private TimeSince sinceEquipped;

	private TimeSince sinceLastCore;

	private void Awake()
	{
		chargeSlider = GetComponentInChildren<Slider>();
		sliderFill = ((Component)(object)chargeSlider).GetComponentInChildren<Image>();
	}

	private void Start()
	{
		targeter = MonoSingleton<CameraController>.Instance.GetComponent<CameraFrustumTargeter>();
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		gunAud = GetComponent<AudioSource>();
		anim = GetComponentInChildren<Animator>();
		cam = MonoSingleton<CameraController>.Instance.gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		gc = GetComponentInParent<GunControl>();
		sinceEquipped = 0f;
		tempColor = heatSinkSMR.materials[3].GetColor("_TintColor");
		heatSinkAud = heatSinkSMR.GetComponent<AudioSource>();
		if (variation == 0)
		{
			chargeSlider.value = chargeSlider.maxValue;
		}
		else if (variation == 1)
		{
			chargeSlider.value = 0f;
		}
		wpos = GetComponent<WeaponPos>();
		if ((bool)chainsawBladeScroll)
		{
			chainsawBladeRenderer = chainsawBladeScroll.GetComponent<MeshRenderer>();
			chainsawBladeMaterial = chainsawBladeRenderer.sharedMaterial;
		}
		if ((bool)chainsawRenderer)
		{
			chainsawMaterial = chainsawRenderer.sharedMaterial;
			chainsawBrokenVibrate = chainsawRenderer.GetComponent<Vibrate>();
		}
		if ((bool)sawZone)
		{
			sawZone.sourceWeapon = base.gameObject;
		}
	}

	private void OnEnable()
	{
		resettingCores = false;
		sinceEquipped = 0f;
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
		if (!base.gameObject.scene.isLoaded)
		{
			return;
		}
		if ((Object)(object)anim == null)
		{
			anim = GetComponentInChildren<Animator>();
		}
		anim.StopPlayback();
		gunReady = false;
		if ((Object)(object)sliderFill != null && (bool)MonoSingleton<ColorBlindSettings>.Instance)
		{
			((Graphic)sliderFill).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		}
		if ((Object)(object)chargeSlider == null)
		{
			chargeSlider = GetComponentInChildren<Slider>();
		}
		if (variation == 0)
		{
			chargeSlider.value = chargeSlider.maxValue;
		}
		else if (variation == 1)
		{
			chargeSlider.value = 0f;
		}
		if ((Object)(object)sliderFill == null)
		{
			sliderFill = ((Component)(object)chargeSlider).GetComponentInChildren<Image>();
		}
		primaryCharge = 0;
		charging = false;
		grenadeForce = 0f;
		meterOverride = false;
		sinceEquipped = 0f;
		if ((Object)(object)tempChargeSound != null)
		{
			Object.Destroy(((Component)(object)tempChargeSound).gameObject);
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

	private void Update()
	{
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && gunReady && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			if (!wid || wid.delay == 0f)
			{
				Shoot();
			}
			else
			{
				gunReady = false;
				Invoke("Shoot", wid.delay);
			}
		}
		bool flag = (float)sinceEquipped < 0.25f && (float)MonoSingleton<WeaponCharges>.Instance.sinceAltFirePressed > 0.025f;
		bool flag2 = (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && !flag) || MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame;
		if (flag2 && (float)sinceEquipped < 0.25f)
		{
			sinceEquipped = 0.25f;
		}
		if (flag2 && variation == 1 && gunReady && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			gunReady = false;
			if (!wid || wid.delay == 0f)
			{
				Pump();
			}
			else
			{
				Invoke("Pump", wid.delay);
			}
		}
		if ((float)sinceLastCore > 0.5f && flag2 && variation != 1 && gc.activated && !GameStateManager.Instance.PlayerInputLocked && ((variation == 0 && !resettingCores) || (variation == 2 && MonoSingleton<WeaponCharges>.Instance.shoSawCharge >= 1f && gunReady)))
		{
			charging = true;
			if (grenadeForce < 60f)
			{
				grenadeForce = Mathf.MoveTowards(grenadeForce, 60f, Time.deltaTime * 60f);
			}
			grenadeVector = new Vector3(cam.transform.forward.x, cam.transform.forward.y, cam.transform.forward.z);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				grenadeVector = Vector3.Normalize(targeter.CurrentTarget.bounds.center - cam.transform.position);
			}
			grenadeVector += new Vector3(0f, grenadeForce * 0.002f, 0f);
			float num = 3000f;
			if (variation == 2)
			{
				num = 12000f;
			}
			base.transform.localPosition = new Vector3(wpos.currentDefault.x + Random.Range(grenadeForce / num * -1f, grenadeForce / num), wpos.currentDefault.y + Random.Range(grenadeForce / num * -1f, grenadeForce / num), wpos.currentDefault.z + Random.Range(grenadeForce / num * -1f, grenadeForce / num));
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
			MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.ShotgunCharge, ((Component)(object)tempChargeSound).gameObject).intensityMultiplier = grenadeForce / 60f;
			if (variation == 0)
			{
				tempChargeSound.SetPitch(grenadeForce / 60f);
			}
			else if (chainsawBroken)
			{
				tempChargeSound.SetPitch(Mathf.MoveTowards(tempChargeSound.pitch, chainSawBrokenPitchTarget, Time.deltaTime * 2f));
				if (tempChargeSound.GetPitch() == chainSawBrokenPitchTarget)
				{
					chainSawBrokenPitchTarget = 1f + Random.Range(-0.33f, 0.33f);
				}
			}
			else
			{
				tempChargeSound.SetPitch((grenadeForce / 2f + 30f) / 60f);
			}
		}
		if (!flag2 && !flag && variation != 1 && gc.activated && charging)
		{
			charging = false;
			if (variation == 2)
			{
				MonoSingleton<WeaponCharges>.Instance.shoSawCharge = 0f;
			}
			if (!wid || wid.delay == 0f)
			{
				sinceLastCore = 0f;
				if (variation == 0)
				{
					ShootSinks();
				}
				else
				{
					ShootSaw();
				}
			}
			else
			{
				gunReady = false;
				Invoke((variation == 0) ? "ShootSinks" : "ShootSaw", wid.delay);
			}
			Object.Destroy(((Component)(object)tempChargeSound).gameObject);
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
			chainsawBladeScroll.scrollSpeedX = grenadeForce / 6f;
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
		if (releasingHeat)
		{
			tempColor.a -= Time.deltaTime * 2.5f;
			heatSinkSMR.sharedMaterials[3].SetColor("_TintColor", tempColor);
		}
		UpdateMeter();
	}

	private void UpdateMeter()
	{
		if (variation == 1)
		{
			if (timeToBeep != 0f)
			{
				timeToBeep = Mathf.MoveTowards(timeToBeep, 0f, Time.deltaTime * 5f);
			}
			if (primaryCharge == 3)
			{
				chargeSlider.value = chargeSlider.maxValue;
				if (timeToBeep == 0f)
				{
					timeToBeep = 1f;
					Object.Instantiate(warningBeep);
					((Graphic)sliderFill).color = Color.red;
				}
				else if (timeToBeep < 0.5f)
				{
					((Graphic)sliderFill).color = Color.black;
				}
			}
			else
			{
				chargeSlider.value = primaryCharge * 20;
				((Graphic)sliderFill).color = Color.Lerp(MonoSingleton<ColorBlindSettings>.Instance.variationColors[1], new Color(1f, 0.25f, 0.25f), (float)primaryCharge / 2f);
			}
		}
		else if (!meterOverride)
		{
			if (variation == 2 && MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f && !chainsawAttachPoint.gameObject.activeSelf)
			{
				chainsawAttachPoint.gameObject.SetActive(value: true);
			}
			if (grenadeForce > 0f)
			{
				chargeSlider.value = grenadeForce;
				((Graphic)sliderFill).color = Color.Lerp(MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation], new Color(1f, 0.25f, 0.25f), grenadeForce / 60f);
			}
			else if (variation == 0)
			{
				chargeSlider.value = chargeSlider.maxValue;
				((Graphic)sliderFill).color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[0];
			}
			else
			{
				chargeSlider.value = MonoSingleton<WeaponCharges>.Instance.shoSawCharge * chargeSlider.maxValue;
				((Graphic)sliderFill).color = ((MonoSingleton<WeaponCharges>.Instance.shoSawCharge == 1f) ? MonoSingleton<ColorBlindSettings>.Instance.variationColors[2] : Color.gray);
			}
		}
	}

	private void Shoot()
	{
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody rb = MonoSingleton<NewMovement>.Instance.rb;
		Vector3 velocity = rb.velocity;
		Vector3 endPoint = Vector3.ProjectOnPlane(MonoSingleton<CameraController>.Instance.cam.transform.forward, MonoSingleton<NewMovement>.Instance.transform.up);
		rb.velocity = velocity + endPoint.normalized;
		Vector3 position = cam.transform.position;
		Vector3 forward = cam.transform.forward;
		gunReady = false;
		int num = 12;
		MonoSingleton<PlayerAnimations>.Instance?.Shoot(0.5f);
		if (variation == 1)
		{
			switch (primaryCharge)
			{
			case 0:
				num = 10;
				gunAud.SetPitch(Random.Range(1.15f, 1.25f));
				break;
			case 1:
				num = 16;
				gunAud.SetPitch(Random.Range(0.95f, 1.05f));
				break;
			case 2:
				num = 24;
				gunAud.SetPitch(Random.Range(0.75f, 0.85f));
				break;
			case 3:
				num = 0;
				gunAud.SetPitch(Random.Range(0.75f, 0.85f));
				break;
			}
		}
		MonoSingleton<CameraController>.Instance.StopShake();
		Vector3 direction = cam.transform.forward;
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			endPoint = targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos();
			direction = endPoint.normalized;
		}
		rhits = PortalPhysicsV2.RaycastAll(position, direction, 4f, LayerMaskDefaults.Get(LMD.Enemies), out var _);
		if (rhits.Length != 0)
		{
			PhysicsCastResult[] array = rhits;
			for (int i = 0; i < array.Length; i++)
			{
				PhysicsCastResult physicsCastResult = array[i];
				if (!physicsCastResult.collider.gameObject.CompareTag("Body"))
				{
					continue;
				}
				EnemyIdentifierIdentifier componentInParent = physicsCastResult.collider.GetComponentInParent<EnemyIdentifierIdentifier>();
				if (!componentInParent || !componentInParent.eid)
				{
					continue;
				}
				EnemyIdentifier eid = componentInParent.eid;
				if (!eid.dead && !eid.blessed)
				{
					AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
					if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Equip"))
					{
						MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.quickdraw", gc.currentWeapon, eid);
					}
				}
				eid.hitter = "shotgunzone";
				if (!eid.hitterWeapons.Contains("shotgun" + variation))
				{
					eid.hitterWeapons.Add("shotgun" + variation);
				}
				GameObject target = physicsCastResult.collider.gameObject;
				endPoint = eid.transform.position - base.transform.position;
				eid.DeliverDamage(target, endPoint.normalized * 10000f, physicsCastResult.point, 4f, tryForExplode: false, 0f, base.gameObject);
			}
		}
		MonoSingleton<RumbleManager>.Instance.SetVibrationTracked(RumbleProperties.GunFireProjectiles, base.gameObject);
		if (variation != 1 || primaryCharge != 3)
		{
			for (int j = 0; j < num; j++)
			{
				GameObject gameObject = Object.Instantiate(bullet, position, cam.transform.rotation);
				Projectile component = gameObject.GetComponent<Projectile>();
				component.weaponType = "shotgun" + variation;
				component.sourceWeapon = gc.currentWeapon;
				if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
				{
					gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
				}
				if (variation == 1)
				{
					switch (primaryCharge)
					{
					case 0:
						gameObject.transform.Rotate(Random.Range((0f - spread) / 1.5f, spread / 1.5f), Random.Range((0f - spread) / 1.5f, spread / 1.5f), Random.Range((0f - spread) / 1.5f, spread / 1.5f));
						break;
					case 1:
						gameObject.transform.Rotate(Random.Range(0f - spread, spread), Random.Range(0f - spread, spread), Random.Range(0f - spread, spread));
						break;
					case 2:
						gameObject.transform.Rotate(Random.Range((0f - spread) * 2f, spread * 2f), Random.Range((0f - spread) * 2f, spread * 2f), Random.Range((0f - spread) * 2f, spread * 2f));
						break;
					}
				}
				else
				{
					gameObject.transform.Rotate(Random.Range(0f - spread, spread), Random.Range(0f - spread, spread), Random.Range(0f - spread, spread));
				}
			}
		}
		else
		{
			Vector3 vector = position + forward;
			PhysicsCastResult hitInfo;
			PortalTraversalV2[] portalTraversals2;
			bool flag = PortalPhysicsV2.Raycast(position, forward, 1f, LayerMaskDefaults.Get(LMD.Environment), out hitInfo, out portalTraversals2, out endPoint);
			Quaternion quaternion = cam.transform.rotation;
			Matrix4x4 matrix4x = Matrix4x4.identity;
			if (portalTraversals2.Length != 0)
			{
				matrix4x = PortalUtils.GetTravelMatrix(portalTraversals2);
				quaternion = matrix4x.rotation * quaternion;
				if (portalTraversals2.AllHasFlag(PortalTravellerFlags.PlayerProjectile, out var blocked))
				{
					Vector3 exitPoint = portalTraversals2[^1].exitPoint;
					matrix4x = PortalUtils.GetTravelMatrix(portalTraversals2);
					quaternion = matrix4x.rotation * quaternion;
					if (flag)
					{
						float num2 = Vector3.Distance(exitPoint, hitInfo.point) - 0.1f;
						if (num2 < 0f)
						{
							num2 = 0f;
						}
						vector = exitPoint + forward * num2;
					}
					else
					{
						vector = matrix4x.MultiplyPoint3x4(vector);
					}
				}
				else if (blocked)
				{
					PortalTraversalV2 portalTraversalV = portalTraversals2[0];
					vector = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side).GetPositionInFront(portalTraversalV.entrancePoint, 0.01f);
				}
			}
			else if (flag)
			{
				float num3 = Vector3.Distance(position, hitInfo.point) - 0.1f;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				vector = position + forward * num3;
			}
			GameObject gameObject2 = Object.Instantiate(explosion, vector, quaternion);
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				Vector3 center = targeter.CurrentTarget.bounds.center;
				if (portalTraversals2.Length != 0)
				{
					matrix4x.MultiplyPoint3x4(center);
				}
				gameObject2.transform.LookAt(center);
			}
			Explosion[] componentsInChildren = gameObject2.GetComponentsInChildren<Explosion>();
			foreach (Explosion obj in componentsInChildren)
			{
				obj.sourceWeapon = gc.currentWeapon;
				obj.enemyDamageMultiplier = 1f;
				obj.maxSize *= 1.5f;
				obj.damage = 50;
			}
		}
		if (variation != 1)
		{
			gunAud.SetPitch(Random.Range(0.95f, 1.05f));
		}
		gunAud.clip = shootSound;
		gunAud.volume = 0.45f;
		gunAud.panStereo = 0f;
		gunAud.Play(tracked: true);
		cc.CameraShake(1f);
		if (variation == 1)
		{
			anim.SetTrigger("PumpFire");
		}
		else
		{
			anim.SetTrigger("Fire");
		}
		Transform[] array2 = shootPoints;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].transform.GetPositionAndRotation(out var position2, out var rotation);
			Vector3 vector2 = position2 - position;
			PortalPhysicsV2.Raycast(position, vector2.normalized, vector2.magnitude, default(LayerMask), out var _, out var portalTraversals3, out endPoint);
			if (portalTraversals3.Length != 0)
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals3);
				position2 = travelMatrix.MultiplyPoint3x4(position2);
				rotation = travelMatrix.rotation * rotation;
			}
			Object.Instantiate(muzzleFlash, position2, rotation);
		}
		releasingHeat = false;
		tempColor.a = 1f;
		heatSinkSMR.sharedMaterials[3].SetColor("_TintColor", tempColor);
		if (variation == 1)
		{
			primaryCharge = 0;
		}
	}

	private void ShootSinks()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = cam.transform.position;
		Vector3 forward = cam.transform.forward;
		gunReady = false;
		base.transform.localPosition = wpos.currentDefault;
		MonoSingleton<PlayerAnimations>.Instance?.Shoot();
		Vector3 vector = position + forward * 0.5f;
		PortalPhysicsV2.Raycast(position, forward, 0.5f, default(LayerMask), out var hitInfo, out var portalTraversals, out var endPoint);
		Vector3 point = MonoSingleton<NewMovement>.Instance.rb.GetGravityDirection();
		if (portalTraversals.Length != 0)
		{
			if (portalTraversals.AllHasFlag(PortalTravellerFlags.PlayerProjectile, out var blocked))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
				vector = travelMatrix.MultiplyPoint3x4(vector);
				point = travelMatrix.MultiplyPoint3x4(point);
				grenadeVector = travelMatrix.MultiplyVector(grenadeVector);
			}
			else if (blocked)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[0];
				NativePortalTransform nativePortalTransform = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side);
				vector = nativePortalTransform.GetPositionInFront(portalTraversalV.entrancePoint, 0.01f);
				endPoint = Vector3.Reflect(forward, float3.op_Implicit(nativePortalTransform.back));
				grenadeVector = endPoint.normalized;
			}
		}
		Transform[] array = shootPoints;
		for (int i = 0; i < array.Length; i++)
		{
			_ = array[i];
			GameObject obj = Object.Instantiate(grenade, vector, Random.rotation);
			obj.GetComponentInChildren<Grenade>().sourceWeapon = gc.currentWeapon;
			Rigidbody component = obj.GetComponent<Rigidbody>();
			if (MonoSingleton<NewMovement>.Instance.rb.GetCustomGravityMode())
			{
				Vector3 normalized = point.normalized;
				endPoint = Physics.gravity;
				component.SetCustomGravity(normalized * endPoint.magnitude);
				component.SetCustomGravityMode(useCustomGravity: true);
			}
			component.AddForce(grenadeVector * (grenadeForce + 10f), ForceMode.VelocityChange);
			component.AddTorque(Random.insideUnitSphere * grenadeForce * 3f, ForceMode.VelocityChange);
			Debug.DrawLine(Vector3.zero, vector, Color.magenta, 5f);
			Debug.DrawLine(vector, vector + grenadeVector, Color.cyan, 5f);
		}
		Object.Instantiate(grenadeSoundBubble).GetComponent<AudioSource>().volume = 0.45f * Mathf.Sqrt(Mathf.Pow(1f, 2f) - Mathf.Pow(grenadeForce, 2f) / Mathf.Pow(60f, 2f));
		anim.SetTrigger("Secondary Fire");
		gunAud.clip = shootSound;
		gunAud.volume = 0.45f * (grenadeForce / 60f);
		gunAud.panStereo = 0f;
		gunAud.SetPitch(Random.Range(0.75f, 0.85f));
		gunAud.Play(tracked: true);
		cc.CameraShake(1f);
		meterOverride = true;
		resettingCores = true;
		chargeSlider.value = 0f;
		((Graphic)sliderFill).color = Color.black;
		array = shootPoints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.GetPositionAndRotation(out var position2, out var rotation);
			Vector3 vector2 = position2 - position;
			PortalPhysicsV2.Raycast(position, vector2.normalized, vector2.magnitude, default(LayerMask), out hitInfo, out var portalTraversals2, out endPoint);
			if (portalTraversals.Length != 0)
			{
				Matrix4x4 travelMatrix2 = PortalUtils.GetTravelMatrix(portalTraversals2);
				position2 = travelMatrix2.MultiplyPoint3x4(position2);
				rotation = travelMatrix2.rotation * rotation;
			}
			Object.Instantiate(muzzleFlash, position2, rotation);
		}
		releasingHeat = false;
		tempColor.a = 0f;
		heatSinkSMR.sharedMaterials[3].SetColor("_TintColor", tempColor);
		grenadeForce = 0f;
	}

	private void ShootSaw()
	{
		ShootSaw(noSaw: false);
	}

	private void ShootSaw(bool noSaw)
	{
		Vector3 position = cam.transform.position;
		gunReady = true;
		base.transform.localPosition = wpos.currentDefault;
		if (!noSaw)
		{
			MonoSingleton<PlayerAnimations>.Instance?.Shoot();
			Vector3 vector = cam.transform.forward;
			Vector3 endPoint;
			if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
			{
				endPoint = targeter.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.GetDefaultPos();
				vector = endPoint.normalized;
			}
			bool blocked = false;
			Transform[] array = shootPoints;
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i];
				Vector3 vector2 = position + vector * 0.5f;
				Vector3 vector3 = vector;
				PhysicsCastResult hitInfo;
				PortalTraversalV2[] portalTraversals;
				bool flag = PortalPhysicsV2.Raycast(position, vector, 0.5f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), out hitInfo, out portalTraversals, out endPoint);
				if (portalTraversals.Length != 0)
				{
					if (portalTraversals.AllHasFlag(PortalTravellerFlags.PlayerProjectile, out blocked))
					{
						Vector3 exitPoint = portalTraversals[^1].exitPoint;
						Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
						if (flag)
						{
							float num = Vector3.Distance(exitPoint, hitInfo.point) - 0.1f;
							if (num < 0f)
							{
								num = 0f;
							}
							vector2 = exitPoint + vector3 * num;
							vector3 = portalTraversals[^1].exitDirection;
						}
						else
						{
							vector2 = travelMatrix.MultiplyPoint3x4(vector2);
							vector3 = travelMatrix.MultiplyVector(vector3);
						}
					}
					else if (blocked)
					{
						PortalTraversalV2 portalTraversalV = portalTraversals[0];
						vector2 = portalTraversalV.portalObject.GetTransform(portalTraversalV.portalHandle.side).GetPositionInFront(portalTraversalV.entrancePoint, 0.1f);
					}
				}
				else if (flag)
				{
					float num2 = Vector3.Distance(position, hitInfo.point) - 0.5f;
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					vector2 = position + vector3 * num2;
				}
				Chainsaw chainsaw = Object.Instantiate(this.chainsaw, vector2, Random.rotation);
				chainsaw.weaponType = "shotgun" + variation;
				chainsaw.CheckMultipleRicochets(onStart: true);
				chainsaw.sourceWeapon = gc.currentWeapon;
				chainsaw.attachedTransform = MonoSingleton<PlayerTracker>.Instance.GetTarget();
				chainsaw.lineStartTransform = chainsawAttachPoint;
				if (!blocked)
				{
					chainsaw.traversals = new List<PortalTraversalV2>(portalTraversals);
				}
				chainsaw.GetComponent<Rigidbody>().AddForce(vector3 * (grenadeForce + 10f) * 1.5f, ForceMode.VelocityChange);
				currentChainsaws.Add(chainsaw);
			}
			if (chainsawBroken)
			{
				chainsawBroken = false;
				chainsawRenderer.material = chainsawMaterial;
				chainsawBrokenVibrate.enabled = false;
			}
			Object.Instantiate(grenadeSoundBubble).GetComponent<AudioSource>().volume = 0.45f * Mathf.Sqrt(Mathf.Pow(1f, 2f) - Mathf.Pow(grenadeForce, 2f) / Mathf.Pow(60f, 2f));
			gunAud.clip = shootSound;
			gunAud.volume = 0.45f * Mathf.Max(0.5f, grenadeForce / 60f);
			gunAud.panStereo = 0f;
			gunAud.SetPitch(Random.Range(0.75f, 0.85f));
			gunAud.Play(tracked: true);
		}
		chainsawBladeRenderer.material = chainsawBladeMaterial;
		chainsawBladeScroll.scrollSpeedX = 0f;
		chainsawAttachPoint.gameObject.SetActive(value: false);
		anim.Play("FireNoReload");
		cc.CameraShake(1f);
		releasingHeat = false;
		grenadeForce = 0f;
	}

	private void Pump()
	{
		anim.SetTrigger("Pump");
		if (primaryCharge < 3)
		{
			primaryCharge++;
		}
	}

	public void ReleaseHeat()
	{
		releasingHeat = true;
		ParticleSystem[] array = heatReleaseParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
		heatSinkAud.Play(tracked: true);
	}

	public void ClickSound()
	{
		if (((Graphic)sliderFill).color != MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation])
		{
			gunAud.clip = clickChargeSound;
		}
		else
		{
			gunAud.clip = clickSound;
		}
		gunAud.volume = 0.5f;
		gunAud.SetPitch(Random.Range(0.95f, 1.05f));
		gunAud.panStereo = 0.1f;
		gunAud.Play(tracked: true);
	}

	public void ReadyGun()
	{
		gunReady = true;
		meterOverride = false;
		resettingCores = false;
	}

	public void Smack()
	{
		gunAud.clip = smackSound;
		gunAud.volume = 0.75f;
		gunAud.SetPitch(Random.Range(2f, 2.2f));
		gunAud.panStereo = 0.1f;
		gunAud.Play(tracked: true);
	}

	public void SkipShoot()
	{
		anim.ResetTrigger("Fire");
		anim.Play("FireWithReload", -1, 0.05f);
	}

	public void Pump1Sound()
	{
		AudioSource component = Object.Instantiate(grenadeSoundBubble).GetComponent<AudioSource>();
		component.SetPitch(Random.Range(0.95f, 1.05f));
		component.clip = pump1sound;
		component.volume = 1f;
		component.panStereo = 0.1f;
		component.Play(tracked: true);
		AudioSource component2 = Object.Instantiate(pumpChargeSound).GetComponent<AudioSource>();
		float num = primaryCharge;
		component2.SetPitch(1f + num / 5f);
		component2.Play(tracked: true);
	}

	public void Pump2Sound()
	{
		AudioSource component = Object.Instantiate(grenadeSoundBubble).GetComponent<AudioSource>();
		component.SetPitch(Random.Range(0.95f, 1.05f));
		component.clip = pump2sound;
		component.volume = 1f;
		component.panStereo = 0.1f;
		component.Play(tracked: true);
	}
}
