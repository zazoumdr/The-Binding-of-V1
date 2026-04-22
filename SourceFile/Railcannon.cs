using ULTRAKILL.Cheats;
using UnityEngine;

public class Railcannon : MonoBehaviour
{
	public int variation;

	public GameObject beam;

	public Transform shootPoint;

	public GameObject fullCharge;

	public GameObject fireSound;

	private AudioSource fullAud;

	private bool pitchRise;

	private InputManager inman;

	public WeaponIdentifier wid;

	private float gotWidDelay;

	private AudioSource aud;

	private CameraController cc;

	private Camera cam;

	private GunControl gc;

	private Animator anim;

	public SkinnedMeshRenderer body;

	public SkinnedMeshRenderer[] pips;

	private WeaponCharges wc;

	private WeaponPos wpos;

	private bool zooming;

	private bool gotStuff;

	private MaterialPropertyBlock propBlock;

	private static readonly int EmissiveIntensityID = Shader.PropertyToID("_EmissiveIntensity");

	private CameraFrustumTargeter targeter;

	private float altCharge;

	[SerializeField]
	private Light fullChargeLight;

	[SerializeField]
	private ParticleSystem fullChargeParticles;

	private void Awake()
	{
		if (!gotStuff)
		{
			wid = GetComponent<WeaponIdentifier>();
		}
	}

	private void Start()
	{
		if (!gotStuff)
		{
			gotStuff = true;
			GetStuff();
		}
	}

	private void OnEnable()
	{
		if (!gotStuff)
		{
			gotStuff = true;
			GetStuff();
		}
		if (wc.raicharge != 5f)
		{
			fullCharge.SetActive(value: false);
			base.transform.localPosition = wpos.currentDefault;
		}
		else if (variation == 2)
		{
			if ((Object)(object)fullAud == null)
			{
				fullAud = fullCharge.GetComponent<AudioSource>();
			}
			pitchRise = true;
			fullAud.SetPitch(0f);
		}
	}

	private void OnDisable()
	{
		if (wc == null)
		{
			wc = GetComponentInParent<WeaponCharges>();
		}
		if (wpos != null)
		{
			base.transform.localPosition = wpos.currentDefault;
		}
		if (zooming)
		{
			zooming = false;
			cc.StopZoom();
		}
	}

	private void Update()
	{
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		if (wid.delay > 0f && altCharge < wc.raicharge)
		{
			altCharge = wc.raicharge;
		}
		float raicharge = wc.raicharge;
		if (wid.delay > 0f)
		{
			raicharge = altCharge;
		}
		if (raicharge < 5f && !NoWeaponCooldown.NoCooldown)
		{
			SetMaterialIntensity(raicharge, isRecharging: true);
		}
		else
		{
			MonoSingleton<RumbleManager>.Instance?.SetVibrationTracked(RumbleProperties.RailcannonIdle, fullCharge);
			if (!fullCharge.activeSelf)
			{
				fullCharge.SetActive(value: true);
				if (variation == 2)
				{
					pitchRise = true;
					fullAud.SetPitch(0f);
				}
			}
			if (!wc.railChargePlayed)
			{
				wc.PlayRailCharge();
			}
			base.transform.localPosition = new Vector3(wpos.currentDefault.x + Random.Range(-0.005f, 0.005f), wpos.currentDefault.y + Random.Range(-0.005f, 0.005f), wpos.currentDefault.z + Random.Range(-0.005f, 0.005f));
			SetMaterialIntensity(1f, isRecharging: false);
			Color color = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
			fullChargeLight.color = color;
			MainModule main = fullChargeParticles.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(color);
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				fullCharge.SetActive(value: false);
				base.transform.localPosition = wpos.currentDefault;
				wc.raicharge = 0f;
				wc.railChargePlayed = false;
				altCharge = 0f;
				if (!wid || wid.delay == 0f)
				{
					Shoot();
				}
				else
				{
					Invoke("Shoot", wid.delay);
				}
			}
		}
		if (!wid || wid.delay == 0f)
		{
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed && gc.activated && !GameStateManager.Instance.PlayerInputLocked)
			{
				zooming = true;
				cc.Zoom(cc.defaultFov / 2f);
			}
			else if (zooming)
			{
				zooming = false;
				cc.StopZoom();
			}
		}
		if (wid.delay != gotWidDelay)
		{
			gotWidDelay = wid.delay;
			if ((bool)wid && wid.delay != 0f)
			{
				AudioSource obj = fullAud;
				obj.volume -= wid.delay * 2f;
				if (fullAud.volume < 0f)
				{
					fullAud.volume = 0f;
				}
			}
		}
		if (pitchRise)
		{
			fullAud.SetPitch(Mathf.MoveTowards(fullAud.GetPitch(), 2f, Time.deltaTime * 4f));
			if (fullAud.GetPitch() == 2f)
			{
				pitchRise = false;
			}
		}
	}

	private void SetMaterialIntensity(float newIntensity, bool isRecharging)
	{
		Color value = MonoSingleton<ColorBlindSettings>.Instance.variationColors[variation];
		body.GetPropertyBlock(propBlock);
		propBlock.SetFloat(EmissiveIntensityID, isRecharging ? (newIntensity / 5f) : newIntensity);
		propBlock.SetColor("_EmissiveColor", value);
		body.SetPropertyBlock(propBlock);
		for (int i = 0; i < pips.Length; i++)
		{
			SkinnedMeshRenderer obj = pips[i];
			obj.GetPropertyBlock(propBlock);
			propBlock.SetColor("_EmissiveColor", value);
			if (isRecharging)
			{
				if (newIntensity > (float)i + 1f)
				{
					propBlock.SetFloat(EmissiveIntensityID, 1f);
				}
				else
				{
					float value2 = (newIntensity - (float)i) % 1f;
					propBlock.SetFloat(EmissiveIntensityID, value2);
				}
			}
			else
			{
				propBlock.SetFloat(EmissiveIntensityID, newIntensity);
			}
			obj.SetPropertyBlock(propBlock);
		}
	}

	private void Shoot()
	{
		MonoSingleton<PlayerAnimations>.Instance?.Shoot(0.25f);
		GameObject gameObject = Object.Instantiate(beam, cc.GetDefaultPos(), cc.transform.rotation);
		if ((bool)targeter.CurrentTarget && targeter.IsAutoAimed)
		{
			gameObject.transform.LookAt(targeter.CurrentTarget.bounds.center);
		}
		if (variation != 1)
		{
			if (gameObject.TryGetComponent<RevolverBeam>(out var component))
			{
				component.sourceWeapon = gc.currentWeapon;
				component.alternateStartPoint = shootPoint.position;
			}
		}
		else
		{
			gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * 250f, ForceMode.VelocityChange);
			if (gameObject.TryGetComponent<Harpoon>(out var component2))
			{
				component2.sourceWeapon = base.gameObject;
			}
		}
		Object.Instantiate(fireSound);
		anim.SetTrigger("Shoot");
		cc.CameraShake(2f);
		MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.GunFireStrong);
	}

	private void GetStuff()
	{
		targeter = Camera.main.GetComponent<CameraFrustumTargeter>();
		inman = MonoSingleton<InputManager>.Instance;
		wid = GetComponent<WeaponIdentifier>();
		aud = GetComponent<AudioSource>();
		cc = MonoSingleton<CameraController>.Instance;
		cam = cc.GetComponent<Camera>();
		gc = GetComponentInParent<GunControl>();
		anim = GetComponentInChildren<Animator>();
		wpos = GetComponent<WeaponPos>();
		fullAud = fullCharge.GetComponent<AudioSource>();
		wc = MonoSingleton<WeaponCharges>.Instance;
		propBlock = new MaterialPropertyBlock();
	}
}
