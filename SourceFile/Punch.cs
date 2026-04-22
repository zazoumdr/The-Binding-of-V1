using System.Collections;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.InputSystem;

public class Punch : MonoBehaviour
{
	private InputManager inman;

	public FistType type;

	private string hitter;

	private float damage;

	private float screenShakeMultiplier;

	private float force;

	private bool tryForExplode;

	private float cooldownCost;

	public bool ready = true;

	[HideInInspector]
	public Animator anim;

	private SkinnedMeshRenderer smr;

	private Revolver rev;

	[SerializeField]
	private AudioSource aud;

	private GameObject camObj;

	private CameraController cc;

	private RaycastHit hit;

	private LayerMask environmentMask;

	private NewMovement nmov;

	private TrailRenderer tr;

	private Light parryLight;

	private GameObject currentDustParticle;

	public GameObject dustParticle;

	public AudioSource normalHit;

	public AudioSource heavyHit;

	public AudioSource specialHit;

	private StyleHUD shud;

	private StatsManager sman;

	public bool holding;

	public Transform holder;

	public ItemIdentifier heldItem;

	private bool hasHeldItem;

	private FistControl fc;

	private bool shopping;

	private int shopRequests;

	public GameObject parriedProjectileHitObject;

	private ProjectileParryZone ppz;

	private bool returnToOrigRot;

	public GameObject blastWave;

	private bool holdingInput;

	public GameObject shell;

	public Transform shellEjector;

	private AudioSource ejectorAud;

	private bool alreadyBoostedProjectile;

	private bool ignoreDoublePunch;

	public bool hitSomething;

	public bool parriedSomething;

	public bool alreadyHitCoin;

	public int activeFrames;

	public InputAction heldAction;

	private List<Chainsaw> punchedChainsaws = new List<Chainsaw>();

	private Coroutine punchChainsawsRoutine;

	private void Awake()
	{
		inman = MonoSingleton<InputManager>.Instance;
		anim = GetComponent<Animator>();
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		rev = base.transform.parent.parent.GetComponentInChildren<Revolver>();
		camObj = MonoSingleton<CameraController>.Instance.gameObject;
		cc = MonoSingleton<CameraController>.Instance;
		aud = GetComponent<AudioSource>();
		parryLight = base.transform.Find("PunchZone").GetComponent<Light>();
		nmov = GetComponentInParent<NewMovement>();
		tr = GetComponentInChildren<TrailRenderer>();
		shud = MonoSingleton<StyleHUD>.Instance;
		sman = MonoSingleton<StatsManager>.Instance;
		environmentMask = LayerMaskDefaults.Get(LMD.Environment);
		environmentMask = (int)environmentMask | 0x400000;
	}

	private void Start()
	{
		holdingInput = false;
		if (fc == null)
		{
			fc = MonoSingleton<FistControl>.Instance;
		}
		switch (type)
		{
		case FistType.Standard:
			damage = 1f;
			screenShakeMultiplier = 1f;
			force = 25f;
			tryForExplode = false;
			cooldownCost = 2f;
			hitter = "punch";
			break;
		case FistType.Heavy:
			damage = 2.5f;
			screenShakeMultiplier = 2f;
			force = 100f;
			tryForExplode = true;
			cooldownCost = 3f;
			hitter = "heavypunch";
			break;
		}
	}

	private void OnEnable()
	{
		holdingInput = false;
		ReadyToPunch();
		ignoreDoublePunch = false;
		if (fc == null)
		{
			fc = GetComponentInParent<FistControl>();
			anim = GetComponent<Animator>();
		}
		if (fc.heldObject != null)
		{
			heldItem = fc.heldObject;
			heldItem.transform.SetParent(holder, worldPositionStays: true);
			holding = true;
			if (!heldItem.noHoldingAnimation && fc.forceNoHold <= 0)
			{
				anim.SetBool("SemiHolding", false);
				anim.SetBool("Holding", true);
				anim.Play("Holding", -1, 0f);
			}
			else
			{
				anim.SetBool("SemiHolding", true);
			}
			ResetHeldItemPosition();
		}
	}

	public void ResetHeldState()
	{
		holding = false;
		anim.SetBool("Holding", false);
		anim.SetBool("SemiHolding", false);
	}

	public void ForceThrow()
	{
		if (!heldItem)
		{
			ResetHeldState();
			return;
		}
		ItemIdentifier itemIdentifier = heldItem;
		Rigidbody[] componentsInChildren = itemIdentifier.GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			ForceDrop();
			Rigidbody[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AddForce((base.transform.parent.forward + Vector3.up * 0.1f) * 5000f);
			}
			itemIdentifier.transform.position = base.transform.parent.position + base.transform.parent.forward;
		}
	}

	public void ForceDrop()
	{
		if (!heldItem)
		{
			ResetHeldState();
			return;
		}
		Rigidbody[] componentsInChildren = heldItem.GetComponentsInChildren<Rigidbody>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		heldItem.transform.SetParent(null, worldPositionStays: true);
		heldItem.pickedUp = false;
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localScale = Vector3.one;
		}
		else
		{
			heldItem.transform.localScale = heldItem.putDownScale;
		}
		Transform[] componentsInChildren2 = heldItem.GetComponentsInChildren<Transform>();
		foreach (Transform obj in componentsInChildren2)
		{
			obj.gameObject.layer = 22;
			if (obj.TryGetComponent<OutdoorsChecker>(out var component) && component.enabled)
			{
				component.CancelInvoke("SlowUpdate");
				component.SlowUpdate();
			}
		}
		Rigidbody[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].isKinematic = false;
		}
		Collider[] componentsInChildren3 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = true;
		}
		if (!heldItem.hooked)
		{
			heldItem.transform.position = base.transform.parent.position + base.transform.parent.forward;
		}
		heldItem.SendMessage("PutDown", SendMessageOptions.DontRequireReceiver);
		anim.SetBool("Holding", false);
		anim.SetBool("SemiHolding", false);
		holding = false;
		fc.heldObject = null;
		heldItem = null;
	}

	public void PlaceHeldObject(ItemPlaceZone[] placeZones, Transform target)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!heldItem)
		{
			ResetHeldState();
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("JabHolding"))
		{
			ignoreDoublePunch = true;
			anim.Play("Jab", 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
		}
		holding = false;
		anim.SetBool("Holding", false);
		anim.SetBool("SemiHolding", false);
		heldItem.transform.SetParent(target);
		heldItem.pickedUp = false;
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localPosition = Vector3.zero;
			heldItem.transform.localScale = Vector3.one;
			heldItem.transform.localRotation = Quaternion.identity;
		}
		else
		{
			heldItem.transform.localPosition = heldItem.putDownPosition;
			heldItem.transform.localScale = heldItem.putDownScale;
			heldItem.transform.localRotation = Quaternion.Euler(heldItem.putDownRotation);
		}
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		foreach (Transform obj in componentsInChildren)
		{
			obj.gameObject.layer = 22;
			if (obj.TryGetComponent<OutdoorsChecker>(out var component) && component.enabled)
			{
				component.CancelInvoke("SlowUpdate");
				component.SlowUpdate();
			}
		}
		Collider[] componentsInChildren2 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = true;
		}
		heldItem.SendMessage("PutDown", SendMessageOptions.DontRequireReceiver);
		Object.Instantiate(heldItem.pickUpSound);
		heldItem = null;
		fc.heldObject = null;
		for (int i = 0; i < placeZones.Length; i++)
		{
			placeZones[i].CheckItem();
		}
		ResetHeldState();
	}

	public void ResetHeldItemPosition()
	{
		if (heldItem.reverseTransformSettings)
		{
			heldItem.transform.localPosition = heldItem.putDownPosition;
			heldItem.transform.localScale = heldItem.putDownScale;
			heldItem.transform.localRotation = Quaternion.Euler(heldItem.putDownRotation);
		}
		else
		{
			heldItem.transform.localPosition = Vector3.zero;
			heldItem.transform.localScale = Vector3.one;
			heldItem.transform.localRotation = Quaternion.identity;
		}
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 13;
		}
	}

	public void ForceHold(ItemIdentifier itid)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		holding = true;
		if (itid.TryGetComponent<FishObjectReference>(out var component) && (bool)MonoSingleton<FishManager>.Instance && MonoSingleton<FishManager>.Instance.recognizedFishes.ContainsKey(component.fishObject) && !MonoSingleton<FishManager>.Instance.recognizedFishes[component.fishObject])
		{
			MonoSingleton<FishManager>.Instance.UnlockFish(component.fishObject);
			MonoSingleton<FishingHUD>.Instance.ShowFishCaught(show: true, component.fishObject);
		}
		if (!itid.noHoldingAnimation && fc.forceNoHold <= 0)
		{
			anim.SetBool("SemiHolding", false);
			anim.SetBool("Holding", true);
		}
		else
		{
			anim.SetBool("SemiHolding", true);
		}
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Jab") || ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Jab2"))
		{
			ignoreDoublePunch = true;
			anim.Play("JabHolding", 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
		}
		ItemPlaceZone[] componentsInParent = itid.GetComponentsInParent<ItemPlaceZone>();
		itid.ipz = null;
		heldItem = itid;
		itid.transform.SetParent(holder);
		fc.heldObject = itid;
		itid.pickedUp = true;
		itid.beenPickedUp = true;
		itid.SendMessage("OffCorrectUse", SendMessageOptions.DontRequireReceiver);
		ResetHeldItemPosition();
		Transform[] componentsInChildren = heldItem.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 13;
		}
		Rigidbody[] componentsInChildren2 = heldItem.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].isKinematic = true;
		}
		Collider[] componentsInChildren3 = heldItem.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = false;
		}
		Object.Instantiate(itid.pickUpSound);
		heldItem.SendMessage("PickUp", SendMessageOptions.DontRequireReceiver);
		if (componentsInParent != null && componentsInParent.Length != 0)
		{
			ItemPlaceZone[] array = componentsInParent;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CheckItem();
			}
		}
	}

	private void OnDisable()
	{
		holding = false;
		anim.SetBool("Holding", false);
		anim.SetBool("SemiHolding", false);
		ignoreDoublePunch = false;
		if (punchChainsawsRoutine == null)
		{
			return;
		}
		punchChainsawsRoutine = null;
		foreach (Chainsaw punchedChainsaw in punchedChainsaws)
		{
			if (!(punchedChainsaw == null))
			{
				punchedChainsaw.beingPunched = false;
			}
		}
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Punch.WasPerformedThisFrame && ready && !shopping && fc.fistCooldown <= 0f && MonoSingleton<WeaponCharges>.Instance.punchStamina >= 1f && fc.activated && !GameStateManager.Instance.PlayerInputLocked)
		{
			heldAction = MonoSingleton<InputManager>.Instance.InputSource.Punch.Action;
			PunchStart();
		}
		if (holdingInput && heldAction.WasReleasedThisFrame())
		{
			holdingInput = false;
		}
		float layerWeight = anim.GetLayerWeight(1);
		if (shopping && layerWeight < 1f)
		{
			anim.SetLayerWeight(1, Mathf.MoveTowards(layerWeight, 1f, Time.deltaTime / 10f + 5f * Time.deltaTime * (1f - layerWeight)));
		}
		else if (!shopping && layerWeight > 0f)
		{
			anim.SetLayerWeight(1, Mathf.MoveTowards(layerWeight, 0f, Time.deltaTime / 10f + 5f * Time.deltaTime * layerWeight));
		}
		if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && shopping)
		{
			anim.SetTrigger("ShopTap");
		}
		if (returnToOrigRot)
		{
			base.transform.parent.localRotation = Quaternion.RotateTowards(base.transform.parent.localRotation, Quaternion.identity, (Quaternion.Angle(base.transform.parent.localRotation, Quaternion.identity) * 5f + 5f) * Time.deltaTime * 5f);
			if (base.transform.parent.localRotation == Quaternion.identity)
			{
				returnToOrigRot = false;
			}
		}
		if (fc.shopping && !shopping)
		{
			ShopMode();
		}
		else if (!fc.shopping && shopping)
		{
			StopShop();
		}
		if (holding)
		{
			if (heldItem.Equals(null))
			{
				MonoSingleton<FistControl>.Instance.currentPunch.ResetHeldState();
			}
			else if (!heldItem.noHoldingAnimation && fc.forceNoHold <= 0)
			{
				anim.SetBool("SemiHolding", false);
				anim.SetBool("Holding", true);
			}
			else
			{
				anim.SetBool("SemiHolding", true);
			}
		}
	}

	public void PunchStart()
	{
		holdingInput = true;
		MonoSingleton<PlayerAnimations>.Instance?.Punch((type == FistType.Heavy) ? 0.5f : 1f);
		if (ready)
		{
			ready = false;
			anim.SetFloat("PunchRandomizer", Random.Range(0f, 1f));
			anim.SetTrigger("Punch");
			fc.fistCooldown = cooldownCost * 0.25f;
			MonoSingleton<WeaponCharges>.Instance.punchStamina -= cooldownCost / 2f;
			hitSomething = false;
			parriedSomething = false;
			alreadyHitCoin = false;
			aud.SetPitch(Random.Range(0.9f, 1.1f));
			aud.Play(tracked: true);
			tr.widthMultiplier = 0.5f;
			MonoSingleton<HookArm>.Instance.Cancel();
			if (holding && (bool)heldItem)
			{
				heldItem.SendMessage("PunchWith", SendMessageOptions.DontRequireReceiver);
			}
			MonoSingleton<RumbleManager>.Instance.SetVibration(RumbleProperties.Punch);
		}
	}

	private void ActiveStart()
	{
		if (ignoreDoublePunch)
		{
			ignoreDoublePunch = false;
			return;
		}
		returnToOrigRot = false;
		hitSomething = false;
		parriedSomething = false;
		activeFrames = (MonoSingleton<AssistController>.Instance ? MonoSingleton<AssistController>.Instance.punchAssistFrames : 6);
		bool num = holding;
		hasHeldItem = holding;
		ActiveFrame(firstFrame: true);
		if (num && holding && heldItem != null)
		{
			ForceThrow();
		}
	}

	private void FixedUpdate()
	{
		if (activeFrames > 0)
		{
			activeFrames--;
			ActiveFrame();
			if (activeFrames == 0)
			{
				hasHeldItem = false;
			}
		}
	}

	private void ActiveFrame(bool firstFrame = false)
	{
		if (type == FistType.Standard && !parriedSomething)
		{
			Collider[] array = Physics.OverlapSphere(cc.GetDefaultPos(), 0.01f, 16384, QueryTriggerInteraction.Collide);
			List<Transform> list = new List<Transform>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				list.Add(collider.transform);
				if (TryParryProjectile((collider.attachedRigidbody != null) ? collider.attachedRigidbody.transform : collider.transform, firstFrame))
				{
					break;
				}
			}
			bool flag = Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, 16384);
			if (!flag)
			{
				flag = Physics.BoxCast(cc.GetDefaultPos(), Vector3.one * 0.3f, camObj.transform.forward, out hit, camObj.transform.rotation, 4f, 16384);
			}
			if (!flag || list.Contains(hit.transform) || !TryParryProjectile(hit.transform, firstFrame))
			{
				if (ppz == null)
				{
					ppz = base.transform.parent.GetComponentInChildren<ProjectileParryZone>();
				}
				if (ppz != null)
				{
					Projectile projectile = ppz.CheckParryZone();
					if (projectile != null)
					{
						bool flag2 = !alreadyBoostedProjectile && firstFrame;
						if (!list.Contains(projectile.transform) && !projectile.unparryable && !projectile.undeflectable && (flag2 || !projectile.playerBullet))
						{
							ParryProjectile(projectile);
							parriedSomething = true;
							hitSomething = true;
						}
					}
				}
			}
		}
		else if (type == FistType.Heavy && !hitSomething)
		{
			Transform transform = null;
			Collider[] array3 = Physics.OverlapSphere(cc.GetDefaultPos(), 0.1f, 16384);
			if (array3.Length != 0)
			{
				transform = array3[0].transform;
			}
			else if (Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, 16384) || Physics.BoxCast(cc.GetDefaultPos(), Vector3.one * 0.3f, camObj.transform.forward, out hit, camObj.transform.rotation, 4f, 16384))
			{
				transform = hit.transform;
			}
			_ = (bool)transform;
			if ((bool)transform)
			{
				if (transform.TryGetComponent<ParryHelper>(out var component) && component.target.TryGetComponent<Cannonball>(out var component2))
				{
					MonoSingleton<TimeController>.Instance.HitStop(0.1f);
					cc.CameraShake(0.5f * screenShakeMultiplier);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					component2.Break();
					RaycastHit[] array4 = Physics.BoxCastAll(MonoSingleton<CameraController>.Instance.transform.position, new Vector3(5f, 1f, 1f), cc.transform.forward, cc.transform.rotation, 20f, 4096, QueryTriggerInteraction.Collide);
					for (int i = 0; i < array4.Length; i++)
					{
						RaycastHit raycastHit = array4[i];
						Debug.Log(raycastHit.transform.gameObject.name, raycastHit.transform.gameObject);
						EnemyIdentifier component3 = raycastHit.transform.GetComponent<EnemyIdentifier>();
						if ((bool)component3)
						{
							component3.DeliverDamage(raycastHit.transform.gameObject, cc.transform.forward * 50000f, raycastHit.point, 0f, tryForExplode: false);
							component3.canWallsplat = true;
							component3.useBrakes = false;
						}
					}
					Rigidbody rb = MonoSingleton<NewMovement>.Instance.rb;
					rb.velocity = new Vector3(rb.velocity.x, Vector3.Dot(MonoSingleton<CameraController>.Instance.transform.forward, Vector3.down) * 42f, rb.velocity.z);
					hitSomething = true;
				}
				if (transform.TryGetComponent<MassSpear>(out var component4) && component4.hitPlayer)
				{
					Object.Instantiate<AudioSource>(specialHit, base.transform.position, Quaternion.identity);
					MonoSingleton<TimeController>.Instance.HitStop(0.1f);
					cc.CameraShake(0.5f * screenShakeMultiplier);
					component4.GetHurt(10f);
					hitSomething = true;
				}
				if (transform.TryGetComponent<Chainsaw>(out var component5))
				{
					MonoSingleton<WeaponCharges>.Instance.punchStamina = 2f;
					component5.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos();
					component5.transform.rotation = Quaternion.LookRotation(component5.transform.position - GetParryLookTarget());
					component5.transform.position -= component5.transform.forward;
					component5.rb.velocity = component5.transform.forward * -105f;
					component5.stopped = false;
					MonoSingleton<TimeController>.Instance.ParryFlash();
					component5.TurnIntoSawblade();
					hitSomething = true;
				}
			}
		}
		bool flag3 = Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);
		if (!flag3)
		{
			flag3 = Physics.SphereCast(cc.GetDefaultPos(), 1f, camObj.transform.forward, out hit, 4f, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);
		}
		if (flag3)
		{
			if (!alreadyHitCoin && type == FistType.Standard && hit.collider.CompareTag("Coin"))
			{
				Coin component6 = hit.collider.GetComponent<Coin>();
				if ((bool)component6 && component6.doubled)
				{
					anim.Play("Hook", 0, 0.065f);
					component6.DelayedPunchflection();
					alreadyHitCoin = true;
				}
			}
			if (hitSomething)
			{
				return;
			}
			bool flag4 = false;
			if (Physics.Raycast(cc.GetDefaultPos(), hit.point - cc.GetDefaultPos(), out var hitInfo, 5f, environmentMask) && Vector3.Distance(cc.GetDefaultPos(), hit.point) > Vector3.Distance(cc.GetDefaultPos(), hitInfo.point))
			{
				flag4 = true;
			}
			if (!flag4)
			{
				PunchSuccess(hit.point, hit.transform);
				hitSomething = true;
			}
		}
		if (hitSomething)
		{
			return;
		}
		Collider[] array5 = Physics.OverlapSphere(cc.GetDefaultPos(), 0.1f, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);
		if (array5 != null && array5.Length != 0)
		{
			Collider[] array2 = array5;
			foreach (Collider collider2 in array2)
			{
				PunchSuccess(cc.GetDefaultPos(), collider2.transform);
			}
			hitSomething = true;
		}
		if (type == FistType.Standard && !hitSomething && !parriedSomething)
		{
			Collider[] array6 = Physics.OverlapSphere(cc.GetDefaultPos() + camObj.transform.forward * 3f, 3f, 16384, QueryTriggerInteraction.Collide);
			bool flag5 = false;
			Collider[] array2 = array6;
			foreach (Collider collider3 in array2)
			{
				Nail nail = ((!collider3.attachedRigidbody) ? collider3.GetComponent<Nail>() : collider3.attachedRigidbody.GetComponent<Nail>());
				if (!(nail == null) && nail.sawblade && nail.punchable)
				{
					flag5 = true;
					if (nail.stopped)
					{
						nail.stopped = false;
						nail.rb.velocity = (GetParryLookTarget() - nail.transform.position).normalized * nail.originalVelocity.magnitude;
					}
					else
					{
						nail.rb.velocity = (GetParryLookTarget() - nail.transform.position).normalized * nail.rb.velocity.magnitude;
					}
					nail.punched = true;
					if (nail.magnets.Count > 0)
					{
						nail.punchDistance = Vector3.Distance(nail.transform.position, nail.GetTargetMagnet().GetWorldPosition());
					}
				}
			}
			if (!flag5)
			{
				array2 = Physics.OverlapSphere(cc.GetDefaultPos() + camObj.transform.forward, 1f, 1, QueryTriggerInteraction.Collide);
				foreach (Collider collider4 in array2)
				{
					float num = Vector3.Distance(cc.GetDefaultPos() + camObj.transform.forward, collider4.transform.position);
					if (num < 6f || num > 12f || Mathf.Abs((cc.GetDefaultPos() + camObj.transform.forward).y - collider4.transform.position.y) > 3f || !collider4.TryGetComponent<Magnet>(out var component7) || component7.sawblades.Count <= 0)
					{
						continue;
					}
					float num2 = float.PositiveInfinity;
					float num3 = 0f;
					int num4 = -1;
					for (int num5 = component7.sawblades.Count - 1; num5 >= 0; num5--)
					{
						if (component7.sawblades[num5] == null)
						{
							component7.sawblades.RemoveAt(num5);
							if (flag5)
							{
								num4--;
							}
						}
						else
						{
							num3 = Vector3.Distance(component7.sawblades[num5].transform.position, cc.GetDefaultPos());
							if (component7.sawblades[num5] != null && (num4 < 0 || num2 < num3))
							{
								num4 = num5;
								num2 = num3;
								flag5 = true;
							}
						}
					}
					if (!flag5 || !component7.sawblades[num4].TryGetComponent<Nail>(out var component8))
					{
						continue;
					}
					component8.transform.position = cc.GetDefaultPos() + cc.transform.forward;
					if (component8.stopped)
					{
						component8.stopped = false;
						component8.rb.velocity = (GetParryLookTarget() - component8.transform.position).normalized * component8.originalVelocity.magnitude;
					}
					else
					{
						component8.rb.velocity = (GetParryLookTarget() - component8.transform.position).normalized * component8.rb.velocity.magnitude;
					}
					component8.punched = true;
					if (component8.magnets.Count > 0)
					{
						MagnetInfo targetMagnet = component8.GetTargetMagnet();
						Vector3 worldPosition = targetMagnet.GetWorldPosition();
						if (Vector3.Distance(component8.transform.position + component8.rb.velocity.normalized, worldPosition) > Vector3.Distance(component8.transform.position, worldPosition))
						{
							component8.MagnetRelease(targetMagnet);
						}
						else
						{
							component8.punchDistance = Vector3.Distance(component8.transform.position, worldPosition);
						}
					}
					break;
				}
			}
			if (flag5)
			{
				Object.Instantiate<AudioSource>(specialHit, base.transform.position, Quaternion.identity);
				MonoSingleton<TimeController>.Instance.HitStop(0.1f);
				anim.Play("Hook", -1, 0.065f);
				parriedSomething = true;
				hitSomething = true;
			}
		}
		if (Physics.CheckSphere(cc.GetDefaultPos(), 0.01f, environmentMask, QueryTriggerInteraction.Collide))
		{
			Collider[] array2 = Physics.OverlapSphere(cc.GetDefaultPos(), 0.01f, environmentMask);
			foreach (Collider collider5 in array2)
			{
				AltHit(collider5.transform);
			}
		}
		else
		{
			if (!Physics.Raycast(cc.GetDefaultPos(), camObj.transform.forward, out hit, 4f, environmentMask))
			{
				return;
			}
			AltHit(hit.transform);
			if (!LayerMaskDefaults.IsMatchingLayer(hit.transform.gameObject.layer, LMD.Environment))
			{
				return;
			}
			hitSomething = true;
			base.transform.parent.localRotation = Quaternion.identity;
			cc.CameraShake(0.2f * screenShakeMultiplier);
			Object.Instantiate<AudioSource>(normalHit, base.transform.position, Quaternion.identity);
			currentDustParticle = Object.Instantiate(dustParticle, hit.point, base.transform.rotation);
			currentDustParticle.transform.forward = hit.normal;
			Breakable component9 = hit.transform.gameObject.GetComponent<Breakable>();
			if (component9 != null && !component9.precisionOnly && !component9.specialCaseOnly && (component9.weak || type == FistType.Heavy))
			{
				component9.Break((component9.knuckleBlasterIgnoreDurability && type == FistType.Heavy) ? 999999f : damage);
			}
			if (hit.collider.gameObject.TryGetComponent<Bleeder>(out var component10))
			{
				if (type == FistType.Standard)
				{
					component10.GetHit(hit.point, GoreType.Body);
				}
				else
				{
					component10.GetHit(hit.point, GoreType.Head);
				}
			}
			if (type == FistType.Heavy)
			{
				Glass component11 = hit.collider.gameObject.GetComponent<Glass>();
				if (component11 != null && !component11.broken)
				{
					component11.Shatter();
				}
			}
			HitSurface(hit);
		}
	}

	private void HitSurface(RaycastHit hit)
	{
		if (holding)
		{
			heldItem.SendMessage("HitSurface", hit, SendMessageOptions.DontRequireReceiver);
		}
	}

	private bool TryParryProjectile(Transform target, bool canProjectileBoost = false)
	{
		if (target.TryGetComponent<ParryHelper>(out var component))
		{
			target = component.target;
		}
		if (target.TryGetComponent<Projectile>(out var component2) && !component2.unparryable && !component2.undeflectable && ((!alreadyBoostedProjectile && canProjectileBoost) || !component2.playerBullet))
		{
			ParryProjectile(component2);
			hitSomething = true;
			parriedSomething = true;
			return true;
		}
		if (target.TryGetComponent<Cannonball>(out var component3) && component3.launchable)
		{
			anim.Play("Hook", 0, 0.065f);
			if (!component3.parry)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			else
			{
				Parry();
			}
			Vector3 parryLookTarget = GetParryLookTarget();
			if (Vector3.Distance(component3.transform.position, parryLookTarget) < 10f)
			{
				if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), MonoSingleton<CameraController>.Instance.transform.forward, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
				{
					component3.transform.position = hitInfo.point;
				}
				else
				{
					component3.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + MonoSingleton<CameraController>.Instance.transform.forward * 5f;
				}
				component3.transform.forward = MonoSingleton<CameraController>.Instance.transform.forward;
			}
			else
			{
				component3.transform.LookAt(parryLookTarget);
			}
			component3.Launch();
			hitSomething = true;
			parriedSomething = true;
			return true;
		}
		if (target.TryGetComponent<ParryReceiver>(out var component4))
		{
			if (!component4.enabled)
			{
				return false;
			}
			anim.Play("Hook", 0, 0.065f);
			if (component4.parryHeal)
			{
				Parry();
			}
			else
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			component4.Parry();
			hitSomething = true;
			parriedSomething = true;
			return true;
		}
		if (target.TryGetComponent<ThrownSword>(out var component5) && !component5.friendly && component5.active)
		{
			component5.GetParried();
			anim.Play("Hook", -1, 0.065f);
			Parry(hook: false, component5.returnTransform.GetComponentInParent<EnemyIdentifier>());
			hitSomething = true;
			parriedSomething = true;
			return true;
		}
		if (target.TryGetComponent<MassSpear>(out var component6))
		{
			if (!component6.beenStopped || component6.hittingPlayer)
			{
				component6.Deflected();
				anim.Play("Hook", -1, 0.065f);
				Parry();
				hitSomething = true;
				parriedSomething = true;
			}
			else
			{
				if (!component6.hitPlayer || hitSomething)
				{
					return false;
				}
				Object.Instantiate<AudioSource>(specialHit, base.transform.position, Quaternion.identity);
				MonoSingleton<TimeController>.Instance.HitStop(0.1f);
				cc.CameraShake(0.5f * screenShakeMultiplier);
				component6.GetHurt(5f);
				hitSomething = true;
			}
			return true;
		}
		if (target.TryGetComponent<Landmine>(out var component7))
		{
			anim.Play("Hook", 0, 0.065f);
			Parry();
			component7.transform.LookAt(GetParryLookTarget());
			component7.Parry();
			hitSomething = true;
			parriedSomething = true;
			return true;
		}
		GroundWave component9;
		if (target.TryGetComponent<Chainsaw>(out var component8))
		{
			anim.Play("Hook", 0, 0.065f);
			component8.beingPunched = true;
			MonoSingleton<WeaponCharges>.Instance.punchStamina = 2f;
			if (!punchedChainsaws.Contains(component8))
			{
				punchedChainsaws.Add(component8);
			}
			if (punchChainsawsRoutine == null)
			{
				punchChainsawsRoutine = StartCoroutine(ChainsawPunchRoutine());
			}
		}
		else if (target.TryGetComponent<GroundWave>(out component9))
		{
			anim.Play("Hook", 0, 0.065f);
			Parry();
			component9.ChangeVelocity((GetParryLookTarget() - component9.transform.position).normalized * 100f);
		}
		return false;
	}

	private IEnumerator ChainsawPunchRoutine()
	{
		while (punchedChainsaws.Count > 0)
		{
			punchedChainsaws.RemoveAll((Chainsaw x) => x == null);
			if (punchedChainsaws.Count == 0)
			{
				break;
			}
			Chainsaw chainsaw = punchedChainsaws[punchedChainsaws.Count - 1];
			chainsaw.GetPunched();
			if (chainsaw.stopped)
			{
				chainsaw.stopped = false;
			}
			chainsaw.transform.position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + MonoSingleton<CameraController>.Instance.transform.forward;
			chainsaw.rb.velocity = (GetParryLookTarget() - chainsaw.transform.position).normalized * 105f;
			Object.Instantiate<AudioSource>(specialHit, base.transform.position, Quaternion.identity);
			MonoSingleton<TimeController>.Instance.HitStop(0.1f);
			parriedSomething = true;
			hitSomething = true;
			punchedChainsaws.RemoveAt(punchedChainsaws.Count - 1);
			yield return new WaitForSeconds(0.05f);
		}
		punchChainsawsRoutine = null;
	}

	public void CoinFlip()
	{
		if (ready && MonoSingleton<FistControl>.Instance.forceNoHold <= 0)
		{
			anim.SetTrigger("CoinFlip");
		}
	}

	private void ActiveEnd()
	{
		tr.widthMultiplier = 0f;
		ignoreDoublePunch = false;
		if (type == FistType.Standard)
		{
			ResetFistRotation();
		}
	}

	public void ResetFistRotation()
	{
		returnToOrigRot = true;
	}

	private void PunchEnd()
	{
	}

	private void ReadyToPunch()
	{
		returnToOrigRot = true;
		holdingInput = false;
		ready = true;
		alreadyBoostedProjectile = false;
		ignoreDoublePunch = false;
	}

	private void PunchSuccess(Vector3 point, Transform target)
	{
		base.transform.parent.LookAt(point);
		if (Quaternion.Angle(base.transform.parent.localRotation, Quaternion.identity) > 45f)
		{
			Quaternion localRotation = base.transform.parent.localRotation;
			float num = localRotation.eulerAngles.x;
			float num2 = localRotation.eulerAngles.y;
			float num3 = localRotation.eulerAngles.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			localRotation.eulerAngles = new Vector3(Mathf.Clamp(num, -45f, 45f), Mathf.Clamp(num2, -45f, 45f), Mathf.Clamp(num3, -45f, 45f));
			base.transform.parent.localRotation = localRotation;
		}
		if (target.TryGetComponent<ParryHelper>(out var component))
		{
			target = component.target;
		}
		EnemyIdentifier component3;
		if (target.gameObject.CompareTag("Enemy") || target.gameObject.CompareTag("Armor") || target.gameObject.CompareTag("Head") || target.gameObject.CompareTag("Body") || target.gameObject.CompareTag("Limb") || target.gameObject.CompareTag("EndLimb"))
		{
			if (anim.GetFloat("PunchRandomizer") < 0.5f)
			{
				anim.Play("Jab", 0, 0.075f);
			}
			else
			{
				anim.Play("Jab2", 0, 0.075f);
			}
			Object.Instantiate<AudioSource>(heavyHit, base.transform.position, Quaternion.identity);
			MonoSingleton<TimeController>.Instance.HitStop(0.1f);
			cc.CameraShake(0.5f * screenShakeMultiplier);
			EnemyIdentifier enemyIdentifier = null;
			if (target.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
			{
				enemyIdentifier = component2.eid;
			}
			if ((bool)enemyIdentifier)
			{
				if (enemyIdentifier.drillers.Count > 0 && type != FistType.Heavy)
				{
					anim.Play("Hook", 0, 0.065f);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					Harpoon harpoon = enemyIdentifier.drillers[enemyIdentifier.drillers.Count - 1];
					harpoon.transform.forward = cc.transform.forward;
					harpoon.transform.position = cc.GetDefaultPos();
					harpoon.Punched();
				}
				enemyIdentifier.hitter = hitter;
				enemyIdentifier.DeliverDamage(target.gameObject, camObj.transform.forward * force * 1000f, point, damage, tryForExplode);
			}
			if (holding)
			{
				heldItem.SendMessage("HitWith", target.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (target.TryGetComponent<EnemyIdentifier>(out component3) && (component3.enemyType == EnemyType.Idol || component3.enemyType == EnemyType.Deathcatcher))
		{
			component3.hitter = hitter;
			component3.DeliverDamage(target.gameObject, camObj.transform.forward * force * 1000f, point, damage, tryForExplode);
		}
	}

	public void Parry(bool hook = false, EnemyIdentifier eid = null, string customParryText = "")
	{
		parriedSomething = true;
		hitSomething = true;
		activeFrames = 0;
		if (hook)
		{
			anim.Play("Hook", 0, 0.065f);
		}
		aud.SetPitch(Random.Range(0.7f, 0.8f));
		MonoSingleton<NewMovement>.Instance.Parry(eid, customParryText);
	}

	private void ParryProjectile(Projectile proj)
	{
		proj.hittingPlayer = false;
		proj.friendly = true;
		proj.parried = true;
		proj.speed *= 2f;
		proj.homingType = HomingType.None;
		proj.explosionEffect = parriedProjectileHitObject;
		proj.precheckForCollisions = true;
		proj.SetPortalTravellerType(PortalTravellerType.PLAYER_PROJECTILE);
		if ((bool)proj.parryChallenge)
		{
			proj.parryChallenge.Done();
		}
		List<ContinuousBeam> list = new List<ContinuousBeam>();
		if (proj.connectedBeams.Count > 0)
		{
			list.AddRange(proj.connectedBeams);
		}
		ContinuousBeam[] componentsInChildren = proj.GetComponentsInChildren<ContinuousBeam>();
		if (componentsInChildren.Length != 0)
		{
			ContinuousBeam[] array = componentsInChildren;
			foreach (ContinuousBeam item in array)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		if (list.Count > 0)
		{
			foreach (ContinuousBeam item2 in list)
			{
				ParriedProjectileBeam(item2);
			}
		}
		Rigidbody component = proj.GetComponent<Rigidbody>();
		if (proj.playerBullet)
		{
			alreadyBoostedProjectile = true;
			proj.boosted = true;
			proj.GetComponent<SphereCollider>().radius *= 4f;
			proj.damage = 0f;
			if ((bool)component)
			{
				component.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
			Color color = new Color(1f, 0.35f, 0f);
			if (proj.TryGetComponent<MeshRenderer>(out var component2) && (bool)component2.material && component2.material.HasProperty("_Color"))
			{
				component2.material.SetColor("_Color", color);
			}
			if (proj.TryGetComponent<TrailRenderer>(out var component3))
			{
				Gradient gradient = new Gradient();
				gradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(color, 0f),
					new GradientColorKey(color, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(0f, 1f)
				});
				component3.colorGradient = gradient;
			}
			if (proj.TryGetComponent<Light>(out var component4))
			{
				component4.color = color;
			}
		}
		if ((bool)component)
		{
			component.constraints = RigidbodyConstraints.FreezeRotation;
		}
		anim.Play("Hook", 0, 0.065f);
		if (!proj.playerBullet)
		{
			Parry();
		}
		else
		{
			MonoSingleton<TimeController>.Instance.ParryFlash();
		}
		if (proj.explosive)
		{
			proj.explosive = false;
		}
		Rigidbody component5 = proj.GetComponent<Rigidbody>();
		if ((bool)component5 && component5.GetGravityMode())
		{
			component5.SetGravityMode(useGravity: false);
		}
		Vector3 parryLookTarget = GetParryLookTarget();
		proj.transform.LookAt(parryLookTarget);
		if (proj.speed == 0f)
		{
			component5.velocity = (parryLookTarget - base.transform.position).normalized * 250f;
		}
		else if (proj.speed < 100f)
		{
			proj.speed = 100f;
		}
		if (proj.spreaded)
		{
			ProjectileSpread componentInParent = proj.GetComponentInParent<ProjectileSpread>();
			if (componentInParent != null)
			{
				componentInParent.ParriedProjectile();
			}
		}
		proj.transform.SetParent(null, worldPositionStays: true);
	}

	private void ParriedProjectileBeam(ContinuousBeam beam)
	{
		if (!(beam == null))
		{
			if (beam.enemy)
			{
				beam.parryMultiplier = 2.5f;
			}
			beam.enemy = false;
			beam.canHitEnemy = true;
			beam.canHitPlayer = false;
		}
	}

	public void BlastCheck()
	{
		if (heldAction.IsPressed())
		{
			holdingInput = false;
			anim.SetTrigger("PunchBlast");
			MonoSingleton<PlayerAnimations>.Instance?.Punch();
			Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + MonoSingleton<CameraController>.Instance.transform.forward * 2f;
			if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), MonoSingleton<CameraController>.Instance.transform.forward, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
			{
				position = hitInfo.point - camObj.transform.forward * 0.1f;
			}
			Object.Instantiate(blastWave, position, MonoSingleton<CameraController>.Instance.transform.rotation).GetComponentInChildren<Explosion>().playerProjectileForceDirection = MonoSingleton<CameraController>.Instance.transform.forward;
		}
	}

	public void Eject()
	{
		if ((Object)(object)ejectorAud == null)
		{
			ejectorAud = shellEjector.GetComponent<AudioSource>();
		}
		ejectorAud.Play(tracked: true);
		for (int i = 0; i < 2; i++)
		{
			GameObject gameObject = Object.Instantiate(shell, shellEjector.position + shellEjector.right * 0.075f, shellEjector.rotation);
			if (i == 1)
			{
				gameObject.transform.position = gameObject.transform.position - shellEjector.right * 0.15f;
			}
			gameObject.transform.Rotate(Vector3.forward, Random.Range(-45, 45), Space.Self);
			gameObject.GetComponent<Rigidbody>().AddForce((shellEjector.forward / 1.75f + shellEjector.up / 2f + Vector3.up / 1.75f) * Random.Range(8, 12), ForceMode.VelocityChange);
		}
	}

	public void Hide()
	{
	}

	public void ShopMode()
	{
		shopping = true;
		holdingInput = false;
		shopRequests++;
	}

	public void StopShop()
	{
		shopRequests--;
		if (shopRequests <= 0)
		{
			shopping = false;
		}
	}

	public void EquipAnimation()
	{
		if ((Object)(object)anim == null)
		{
			anim = GetComponent<Animator>();
		}
		anim.SetTrigger("Equip");
	}

	private void AltHit(Transform target)
	{
		if (target.gameObject.layer == 22)
		{
			ItemIdentifier itemIdentifier = target.GetComponent<ItemIdentifier>();
			if ((bool)itemIdentifier && hasHeldItem)
			{
				return;
			}
			ItemPlaceZone[] components = target.GetComponents<ItemPlaceZone>();
			if ((bool)itemIdentifier && itemIdentifier.infiniteSource)
			{
				itemIdentifier = itemIdentifier.CreateCopy();
			}
			if (holding && components != null && components.Length != 0)
			{
				PlaceHeldObject(components, target);
				hitSomething = true;
			}
			else if (!holding && itemIdentifier != null)
			{
				ForceHold(itemIdentifier);
				hitSomething = true;
			}
		}
		if (holding)
		{
			heldItem.SendMessage("HitWith", target.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void CancelAttack()
	{
		anim.Rebind();
		anim.Update(0f);
		ActiveEnd();
		ReadyToPunch();
	}

	public static Vector3 GetParryLookTarget()
	{
		Vector3 vector = MonoSingleton<CameraController>.Instance.transform.forward;
		if ((bool)MonoSingleton<CameraFrustumTargeter>.Instance && (bool)MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget && MonoSingleton<CameraFrustumTargeter>.Instance.IsAutoAimed)
		{
			vector = MonoSingleton<CameraFrustumTargeter>.Instance.CurrentTarget.bounds.center - MonoSingleton<CameraController>.Instance.transform.position;
		}
		if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), vector, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return MonoSingleton<CameraController>.Instance.GetDefaultPos() + vector * 1000f;
	}
}
