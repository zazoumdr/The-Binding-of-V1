using System;
using Sandbox;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Mindflayer : EnemyScript, IEnrage, IAlter, IAlterOptions<bool>
{
	private Enemy enemy;

	private EnemyIdentifier eid;

	private Rigidbody rb;

	private Animator anim;

	private float defaultAnimSpeed = 1f;

	[HideInInspector]
	public bool active = true;

	public Transform model;

	public GameObject homingProjectile;

	public GameObject decorativeProjectile;

	public GameObject warningFlash;

	public GameObject warningFlashUnparriable;

	public GameObject decoy;

	public Transform[] tentacles;

	private SwingCheck2 sc;

	public float cooldown;

	private bool inAction;

	private bool overrideRotation;

	private Vector3 overrideTarget;

	private bool dontTeleport;

	private LayerMask environmentMask;

	private float decoyThreshold;

	private int teleportAttempts;

	private int teleportInterval = 6;

	public GameObject bigHurt;

	public GameObject windUp;

	public GameObject windUpSmall;

	public GameObject teleportSound;

	private bool goingLeft;

	private bool goForward;

	private VisionQuery sightQuery;

	private TargetHandle targetHandle;

	private TargetHandle lastTargetHandle;

	private TargetData lastTargetData;

	private bool beaming;

	private bool beamCooldown = true;

	private bool beamNext;

	public ContinuousBeam beam;

	[HideInInspector]
	public ContinuousBeam tempBeam;

	public Transform rightHand;

	private float beamDistance;

	public LineRenderer lr;

	private PortalLineRenderer plr;

	[SerializeField]
	private LineRenderer[] sweepLineRenderers;

	private PortalLineRenderer[] sweepLines;

	private float outOfSightTime;

	public AssetReference deathExplosion;

	public ParticleSystem chargeParticle;

	private bool vibrate;

	private Vector3 origPos;

	private float timeSinceMelee;

	private float spawnAttackDelay = 1f;

	private int difficulty = -1;

	private float cooldownMultiplier;

	private bool enraged;

	public GameObject enrageEffect;

	private GameObject currentEnrageEffect;

	private EnemySimplifier[] ensims;

	public GameObject originalGlow;

	public GameObject enrageGlow;

	public Gradient originalTentacleGradient;

	public SkinnedMeshRenderer smr;

	public EnemySimplifier ensim;

	public Mesh maleMesh;

	public Material maleMaterial;

	public Material maleMaterialEnraged;

	[HideInInspector]
	public bool dying;

	private bool launched;

	private Collider[] ownColliders;

	[HideInInspector]
	public Vector3 originalPosition;

	public override Vector3 VisionSourcePosition => enemy.chest.transform.position;

	private bool hasVision => targetHandle != null;

	private bool hadVision => lastTargetHandle != null;

	private bool hasTarget => eid.target != null;

	private Vision vision => enemy.vision;

	public bool isEnraged => enraged;

	public string alterKey => "mindflayer";

	public string alterCategoryName => "mindflayer";

	public AlterOption<bool>[] options => new AlterOption<bool>[1]
	{
		new AlterOption<bool>
		{
			value = isEnraged,
			callback = delegate(bool value)
			{
				if (value)
				{
					Enrage();
				}
				else
				{
					UnEnrage();
				}
			},
			key = "enraged",
			name = "Enraged"
		}
	};

	public override void OnDamage(ref DamageData data)
	{
		if (dying && data.hitter == "heavypunch")
		{
			DeadLaunch(data.force);
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		active = false;
	}

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		if (originalPosition == Vector3.zero)
		{
			originalPosition = base.transform.position;
		}
		if (UnityEngine.Random.Range(0, 50) == 8 && smr != null && ensim != null)
		{
			smr.sharedMesh = maleMesh;
			smr.material = maleMaterial;
			ensim.enragedMaterial = maleMaterialEnraged;
		}
	}

	private void Start()
	{
		cooldown = 2f;
		decoyThreshold = enemy.health - (float)teleportInterval;
		environmentMask = LayerMaskDefaults.Get(LMD.Environment);
		sc = GetComponentInChildren<SwingCheck2>();
		plr = new PortalLineRenderer(lr);
		plr.SetEnabled(value: false);
		sweepLines = PortalLineRenderer.MakeArray(sweepLineRenderers);
		sightQuery = new VisionQuery("MindflayerSight", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, environmentMask, toHead: true));
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
		}
		RandomizeDirection();
		SetSpeed();
		if (dying)
		{
			Death();
		}
		if (eid.target != null)
		{
			UpdateVision();
		}
	}

	private void UpdateVision()
	{
		if (vision.TrySee(sightQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
			lastTargetHandle = targetHandle;
		}
		else
		{
			targetHandle = null;
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(UnityEngine.Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		eid.immuneToFriendlyFire = difficulty >= 4;
		if (difficulty >= 4)
		{
			cooldownMultiplier = 2.5f;
			anim.speed = 1.5f;
		}
		else if (difficulty == 3)
		{
			cooldownMultiplier = 1.5f;
			anim.speed = 1.35f;
		}
		else if (difficulty < 2)
		{
			cooldownMultiplier = 0.75f;
			if (difficulty == 1)
			{
				anim.speed = 0.75f;
			}
			else if (difficulty == 0)
			{
				anim.speed = 0.5f;
			}
		}
		else
		{
			cooldownMultiplier = 1f;
			anim.speed = 1f;
		}
		cooldownMultiplier *= eid.totalSpeedModifier;
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		defaultAnimSpeed = anim.speed;
	}

	private void OnEnable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
	}

	private void OnDisable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
		StopAction();
		if ((bool)sc)
		{
			DamageEnd();
		}
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
		}
		chargeParticle.Stop(false, (ParticleSystemStopBehavior)1);
		overrideRotation = false;
	}

	private void OnDestroy()
	{
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		if (targetHandle != null)
		{
			targetHandle.From(details.portalSequence);
			lastTargetData = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(targetHandle);
		}
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
			lastTargetData = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(targetHandle);
		}
	}

	private void UpdateRigidbodySettings()
	{
		rb.drag = ((!hasTarget) ? 3 : 0);
		rb.angularDrag = ((!hasTarget) ? 3 : 0);
	}

	private void Update()
	{
		UpdateRigidbodySettings();
		if (vibrate)
		{
			model.localPosition = new Vector3(origPos.x + UnityEngine.Random.Range(-0.2f, 0.2f), origPos.y + UnityEngine.Random.Range(-0.2f, 0.2f), origPos.z + UnityEngine.Random.Range(-0.2f, 0.2f));
		}
		if (launched)
		{
			model.Rotate(Vector3.right, -1200f * Time.deltaTime, Space.Self);
		}
		if (!hasTarget)
		{
			if (beaming)
			{
				StopBeam();
			}
			if (inAction)
			{
				StopAction();
			}
		}
		else
		{
			if (!active)
			{
				return;
			}
			UpdateVision();
			if (!hasVision && eid.target == null)
			{
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 vector = (hasVision ? lastTargetData.realPosition : eid.target.position);
			float num = (hasVision ? lastTargetData.DistanceTo(position) : Vector3.Distance(eid.target.position, position));
			if (spawnAttackDelay > 0f)
			{
				spawnAttackDelay = Mathf.MoveTowards(spawnAttackDelay, 0f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if (num < 5f && !inAction)
			{
				MeleeAttack();
			}
			timeSinceMelee += Time.deltaTime * eid.totalSpeedModifier;
			if (((difficulty > 2 && timeSinceMelee > 10f) || (difficulty == 2 && timeSinceMelee > 15f)) && !inAction)
			{
				Teleport(closeRange: true);
				timeSinceMelee = 5f;
				if (num < 8f)
				{
					MeleeAttack();
				}
			}
			bool flag = !hasVision || num > 25f || position.y > vector.y + 15f;
			if (cooldown > 0f)
			{
				cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * cooldownMultiplier);
			}
			else if (!inAction && !flag)
			{
				if (beamCooldown || (UnityEngine.Random.Range(0f, 1f) < 0.25f && !beamNext))
				{
					if (!beamCooldown)
					{
						beamNext = true;
					}
					beamCooldown = false;
					HomingAttack();
				}
				else
				{
					BeamAttack();
				}
			}
			if (flag)
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 3f, Time.deltaTime * eid.totalSpeedModifier);
				if (outOfSightTime >= 3f && !inAction)
				{
					Teleport();
				}
			}
			else
			{
				outOfSightTime = Mathf.MoveTowards(outOfSightTime, 0f, Time.deltaTime * 2f * eid.totalSpeedModifier);
			}
			Quaternion rotation = base.transform.rotation;
			if (!overrideRotation)
			{
				Quaternion quaternion = Quaternion.LookRotation(vector - position, Vector3.up);
				base.transform.rotation = Quaternion.RotateTowards(rotation, quaternion, Time.deltaTime * (10f * Quaternion.Angle(quaternion, rotation) + 2f) * eid.totalSpeedModifier);
			}
			else
			{
				Quaternion quaternion2 = Quaternion.LookRotation(overrideTarget - position, Vector3.up);
				if (!beaming)
				{
					base.transform.rotation = Quaternion.RotateTowards(rotation, quaternion2, Time.deltaTime * (100f * Quaternion.Angle(quaternion2, rotation) + 10f) * eid.totalSpeedModifier);
				}
				else
				{
					float num2 = 1f;
					if (difficulty == 1)
					{
						num2 = 0.85f;
					}
					else if (difficulty == 0)
					{
						num2 = 0.65f;
					}
					base.transform.rotation = Quaternion.RotateTowards(rotation, quaternion2, Time.deltaTime * beamDistance * num2 * eid.totalSpeedModifier);
					UpdateBeamVisuals();
					if (Quaternion.Angle(rotation, quaternion2) < 1f)
					{
						StopBeam();
					}
				}
			}
			if (decoyThreshold > enemy.health && decoyThreshold > 0f && !dontTeleport)
			{
				UnityEngine.Object.Instantiate(bigHurt, position, Quaternion.identity);
				while (decoyThreshold > enemy.health)
				{
					decoyThreshold -= teleportInterval;
				}
				Teleport();
			}
			if (difficulty > 2 && enemy.health < 15f && !enraged)
			{
				Enrage();
			}
		}
	}

	private void UpdateBeamVisuals()
	{
		Vector3 vector = rightHand.position;
		Quaternion quaternion = base.transform.rotation;
		Vector3 direction = vector - VisionSourcePosition;
		bool flag = false;
		PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, direction, default(LayerMask), out var _, out var endPoint, out var traversals);
		if (traversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = traversals[0];
			PortalHandle portalHandle = portalTraversalV.portalHandle;
			Portal portalObject = portalTraversalV.portalObject;
			if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
			{
				Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(traversals);
				vector = endPoint;
				quaternion = travelMatrix.rotation * quaternion;
			}
			else if (!portalObject.passThroughNonTraversals)
			{
				flag = true;
			}
		}
		tempBeam.transform.SetPositionAndRotation(vector, quaternion);
		tempBeam.gameObject.SetActive(!flag);
	}

	private void FixedUpdate()
	{
		if (launched)
		{
			LaunchedUpdate();
		}
		else
		{
			if (!hasTarget)
			{
				return;
			}
			PhysicsCastResult hitInfo;
			Vector3 endPoint;
			PortalTraversalV2[] portalTraversals3;
			if (!inAction)
			{
				PortalTraversalV2[] portalTraversals2;
				if (goingLeft)
				{
					if (!PortalPhysicsV2.Raycast(base.transform.position, -base.transform.right, 1f, environmentMask, out hitInfo, out var portalTraversals, out endPoint) && portalTraversals.AllHasFlag(PortalTravellerFlags.Enemy))
					{
						rb.MovePosition(base.transform.position + base.transform.right * -5f * Time.fixedDeltaTime * anim.speed);
					}
					else
					{
						goingLeft = false;
					}
				}
				else if (!PortalPhysicsV2.Raycast(base.transform.position, base.transform.right, 1f, environmentMask, out hitInfo, out portalTraversals2, out endPoint) && portalTraversals2.AllHasFlag(PortalTravellerFlags.Enemy))
				{
					rb.MovePosition(base.transform.position + base.transform.right * 5f * Time.fixedDeltaTime * anim.speed);
				}
				else
				{
					goingLeft = true;
				}
			}
			else if (goForward && !PortalPhysicsV2.Raycast(base.transform.position, base.transform.forward, 1f, environmentMask, out hitInfo, out portalTraversals3, out endPoint) && portalTraversals3.AllHasFlag(PortalTravellerFlags.Enemy))
			{
				rb.MovePosition(base.transform.position + base.transform.forward * 75f * Time.fixedDeltaTime * anim.speed);
			}
		}
	}

	private void LaunchedUpdate()
	{
		RaycastHit[] array = Physics.SphereCastAll(base.transform.position, 1f, base.transform.forward * -1f, 50f * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment));
		bool flag = false;
		bool flag2 = false;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (LayerMaskDefaults.IsMatchingLayer(raycastHit.collider.gameObject.layer, LMD.Environment))
			{
				flag = true;
				break;
			}
			Collider[] array3 = ownColliders;
			foreach (Collider collider in array3)
			{
				if (raycastHit.collider == collider)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = true;
				if (raycastHit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && !component.eid.dead)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.strike", null, eid);
					if (component.eid.enemyType == EnemyType.Gutterman && component.eid.TryGetComponent<Gutterman>(out var component2) && component2.hasShield)
					{
						component2.ShieldBreak(player: true, flash: false);
					}
				}
				break;
			}
			flag2 = false;
		}
		if (flag)
		{
			CancelInvoke("DeathExplosion");
			DeathExplosion();
		}
		else
		{
			rb.MovePosition(base.transform.position - base.transform.forward * 50f * Time.fixedDeltaTime);
		}
	}

	private void RandomizeDirection()
	{
		if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
		{
			goingLeft = true;
		}
		else
		{
			goingLeft = false;
		}
	}

	public void Teleport(bool closeRange = false)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		outOfSightTime = 0f;
		if (((bool)eid && eid.drillers.Count > 0) || (!hasVision && eid.target == null))
		{
			return;
		}
		if (teleportAttempts == 0)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(decoy, base.transform.GetChild(0).position, base.transform.GetChild(0).rotation);
			Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
			AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			componentInChildren.Play(((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash, 0, ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
			componentInChildren.speed = 0f;
			if (enraged)
			{
				gameObject.GetComponent<MindflayerDecoy>().enraged = true;
			}
		}
		Vector3 origin = ((targetHandle != null) ? lastTargetData.realPosition : eid.target.position);
		origin += Vector3.up;
		Vector3 endDirection = UnityEngine.Random.onUnitSphere;
		Vector3 normalized = endDirection.normalized;
		if (normalized.y < 0f)
		{
			normalized.y *= -1f;
		}
		float num = (closeRange ? UnityEngine.Random.Range(5, 8) : UnityEngine.Random.Range(8, 15));
		Vector3? vector = null;
		if (!PortalEnemyUtils.IsRayObstructedByHitOrPortal(origin, normalized, num, environmentMask, out var endPos, out endDirection, out var portalTraversals, QueryTriggerInteraction.Ignore))
		{
			vector = endPos;
		}
		else
		{
			num -= 3f;
			if (!PortalEnemyUtils.IsRayObstructedByHitOrPortal(origin, normalized, num, environmentMask, out endPos, out endDirection, out portalTraversals, QueryTriggerInteraction.Ignore))
			{
				vector = endPos;
			}
		}
		if (!vector.HasValue)
		{
			teleportAttempts++;
			if (teleportAttempts <= 10)
			{
				Teleport(closeRange);
			}
			return;
		}
		Vector3 value = vector.Value;
		float distance = 5f;
		Vector3 endPos2;
		bool flag = PortalEnemyUtils.IsRayObstructedByHitOrPortal(value, Vector3.up, distance, environmentMask, out endPos2, out endDirection, out portalTraversals, QueryTriggerInteraction.Ignore);
		Vector3 endPos3;
		bool flag2 = PortalEnemyUtils.IsRayObstructedByHitOrPortal(value, Vector3.down, distance, environmentMask, out endPos3, out endDirection, out portalTraversals, QueryTriggerInteraction.Ignore);
		Vector3 vector2;
		if (!(flag && flag2))
		{
			vector2 = (flag ? (endPos2 + Vector3.down * UnityEngine.Random.Range(5, 10)) : ((!flag2) ? value : (endPos3 + Vector3.up * UnityEngine.Random.Range(5, 10))));
		}
		else
		{
			if (!(Vector3.Distance(endPos2, endPos3) > 7f))
			{
				teleportAttempts++;
				if (teleportAttempts <= 10)
				{
					Teleport(closeRange);
				}
				return;
			}
			vector2 = new Vector3(value.x, (endPos3.y + endPos2.y) / 2f, value.z);
		}
		if (Physics.CheckSphere(vector2, 0.1f, environmentMask, QueryTriggerInteraction.Ignore))
		{
			teleportAttempts++;
			if (teleportAttempts <= 10)
			{
				Teleport(closeRange);
			}
		}
		else
		{
			TeleportTo(vector2);
		}
	}

	public void TeleportTo(Vector3 targetPosition)
	{
		if (eid.hooked)
		{
			MonoSingleton<HookArm>.Instance.StopThrow(1f, sparks: true);
		}
		base.transform.position = targetPosition;
		MonoSingleton<PortalManagerV2>.Instance.UpdateTraveller(enemy);
		UnityEngine.Object.Instantiate(teleportSound, targetPosition, Quaternion.identity);
		teleportAttempts = 0;
		goingLeft = !goingLeft;
	}

	public void Death()
	{
		active = false;
		inAction = true;
		chargeParticle.Play();
		anim.SetTrigger("Death");
		Invoke("DeathExplosion", 2f);
		origPos = model.localPosition;
		vibrate = true;
		dying = true;
		if ((bool)currentEnrageEffect)
		{
			UnityEngine.Object.Destroy(currentEnrageEffect);
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			TrailRenderer component = tentacles[i].GetComponent<TrailRenderer>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
		}
		plr.SetEnabled(value: false);
	}

	private void DeathExplosion()
	{
		UnityEngine.Object.Instantiate(deathExplosion.ToAsset(), base.transform.position, Quaternion.identity);
		if (eid.drillers.Count > 0)
		{
			for (int num = eid.drillers.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(eid.drillers[num].gameObject);
			}
		}
		if ((bool)tempBeam)
		{
			tempBeam.DetachAndTurnOff();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void DeadLaunch(Vector3 direction)
	{
		launched = true;
		base.transform.LookAt(base.transform.position - direction);
		anim.Play("BeamHold", 0, 0f);
		ownColliders = GetComponentsInChildren<Collider>();
		CancelInvoke("DeathExplosion");
		Invoke("DeathExplosion", 2f / eid.totalSpeedModifier);
	}

	private void HomingAttack()
	{
		inAction = true;
		dontTeleport = true;
		chargeParticle.Play();
		anim.SetTrigger("HomingAttack");
		UnityEngine.Object.Instantiate(windUp, base.transform);
	}

	private void BeamAttack()
	{
		inAction = true;
		chargeParticle.Play();
		dontTeleport = true;
		beamCooldown = true;
		beamNext = false;
		anim.SetTrigger("BeamAttack");
		UnityEngine.Object.Instantiate(windUp, base.transform).GetComponent<AudioSource>().SetPitch(1.5f);
	}

	private void MeleeAttack()
	{
		timeSinceMelee = 0f;
		inAction = true;
		anim.SetTrigger("MeleeAttack");
		UnityEngine.Object.Instantiate(windUpSmall, base.transform);
	}

	public void SwingStart()
	{
		UnityEngine.Object.Instantiate(warningFlash, eid.weakPoint.transform).transform.localScale *= 8f;
		enemy.ParryableCheck();
	}

	public void DamageStart()
	{
		sc.DamageStart();
		goForward = true;
	}

	public void DamageEnd()
	{
		sc.DamageStop();
		enemy.parryable = false;
		goForward = false;
	}

	public void LockTarget()
	{
		if (!hadVision)
		{
			return;
		}
		if (difficulty > 2 && enraged && UnityEngine.Random.Range(0f, 1f) > 0.5f)
		{
			Teleport();
		}
		if (lastTargetData.target.isPlayer)
		{
			Vector3 velocity = lastTargetData.velocity;
			if (difficulty < 4)
			{
				float y = -1.5f;
				if (velocity.y < 0f)
				{
					y = velocity.y * 3f - 1.5f;
				}
				Vector3 vector = new Vector3(velocity.x * 2.5f, y, velocity.z * 2.5f);
				overrideTarget = lastTargetData.position + vector;
				if (velocity.y < 0f && Physics.Raycast(maxDistance: Vector3.Distance(new Vector3(lastTargetData.position.x + vector.x, lastTargetData.position.y, lastTargetData.position.z + vector.z), overrideTarget), origin: overrideTarget, direction: Vector3.down, hitInfo: out var hitInfo, layerMask: environmentMask))
				{
					overrideTarget = hitInfo.point + Vector3.up;
				}
			}
			else
			{
				overrideTarget = lastTargetData.PredictTargetPosition(0.5f);
			}
		}
		else
		{
			overrideTarget = lastTargetData.position;
		}
		UnityEngine.Object.Instantiate(warningFlashUnparriable, eid.weakPoint.transform).transform.localScale *= 8f;
		PhysicsCastResult hitInfo2;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		bool flag = PortalPhysicsV2.Raycast(base.transform.position, overrideTarget - base.transform.position, 999f, environmentMask, out hitInfo2, out portalTraversals, out endPoint);
		plr.SetLines(base.transform.position, flag ? hitInfo2.point : overrideTarget, portalTraversals);
		plr.SetEnabled(value: true);
		overrideRotation = true;
	}

	public void StartBeam()
	{
		if (!launched && hadVision && !beaming)
		{
			plr.SetEnabled(value: false);
			beaming = true;
			tempBeam = UnityEngine.Object.Instantiate(beam, rightHand.transform.position, base.transform.rotation);
			tempBeam.transform.SetParent(launched ? model : rightHand, worldPositionStays: true);
			tempBeam.damage *= eid.totalDamageModifier;
			tempBeam.target = eid.target;
			if (launched)
			{
				tempBeam.canHitPlayer = false;
			}
			Vector3 vector = ((difficulty >= 4 && lastTargetData.target.isPlayer) ? lastTargetData.PredictTargetPosition(0.5f) : lastTargetData.position);
			overrideTarget += (vector - overrideTarget) * 2f;
			Quaternion b = Quaternion.LookRotation(overrideTarget - base.transform.position, Vector3.up);
			beamDistance = Quaternion.Angle(base.transform.rotation, b);
		}
	}

	private void StopBeam()
	{
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
		}
		chargeParticle.Stop(false, (ParticleSystemStopBehavior)1);
		overrideRotation = false;
		plr.SetEnabled(value: false);
		anim.SetTrigger("StopBeam");
		PortalLineRenderer[] array = sweepLines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEnabled(value: false);
		}
	}

	public void ShootProjectiles()
	{
		if (!hadVision)
		{
			return;
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.position = base.transform.position;
		ProjectileSpread projectileSpread = gameObject.AddComponent<ProjectileSpread>();
		projectileSpread.dontSpawn = true;
		projectileSpread.timeUntilDestroy = 10f;
		Vector3 position = base.transform.position;
		for (int i = 0; i < tentacles.Length; i++)
		{
			Vector3 vector = tentacles[i].position;
			Quaternion quaternion = Quaternion.LookRotation(lastTargetData.position - tentacles[i].position);
			TargetHandle targetHandle = this.targetHandle;
			PhysicsCastResult hitInfo;
			PortalTraversalV2[] portalTraversals;
			Vector3 endPoint;
			bool flag = PortalPhysicsV2.Raycast(position, vector - position, Vector3.Distance(vector, position), environmentMask, out hitInfo, out portalTraversals, out endPoint, QueryTriggerInteraction.Ignore);
			if (portalTraversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[0];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				Portal portalObject = portalTraversalV.portalObject;
				if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
				{
					Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(portalTraversals);
					vector = (flag ? position : endPoint);
					quaternion = travelMatrix.rotation * quaternion;
					for (int j = 0; j < portalTraversals.Length; j++)
					{
						PortalTraversalV2 portalTraversalV2 = portalTraversals[j];
						targetHandle = targetHandle.Then(portalTraversalV2.portalHandle);
					}
				}
				else if (!portalObject.passThroughNonTraversals)
				{
					vector = position;
				}
			}
			else if (flag)
			{
				vector = position;
			}
			GameObject obj = UnityEngine.Object.Instantiate(homingProjectile, vector, quaternion);
			obj.transform.position = vector;
			obj.transform.rotation = quaternion;
			obj.transform.SetParent(gameObject.transform, worldPositionStays: true);
			if (obj.TryGetComponent<Projectile>(out var component))
			{
				component.targetHandle = targetHandle;
				component.speed = 10f;
				component.damage *= eid.totalDamageModifier;
			}
		}
		chargeParticle.Stop(false, (ParticleSystemStopBehavior)0);
		cooldown = UnityEngine.Random.Range(4, 5);
	}

	public void HighDifficultyTeleport()
	{
		if (enraged && !dontTeleport)
		{
			Teleport();
			anim.speed = 0f;
			Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
			if (UnityEngine.Random.Range(0f, 1f) < 0.1f || (difficulty > 3 && UnityEngine.Random.Range(0f, 1f) < 0.33f))
			{
				Invoke("Teleport", 0.2f / eid.totalSpeedModifier);
			}
		}
	}

	public void MeleeTeleport()
	{
		if (enraged)
		{
			Teleport(closeRange: true);
			anim.speed = 0f;
			CancelInvoke("ResetAnimSpeed");
			Invoke("ResetAnimSpeed", 0.25f / eid.totalSpeedModifier);
		}
	}

	public void ResetAnimSpeed()
	{
		anim.speed = defaultAnimSpeed;
	}

	public void StopAction()
	{
		if (tempBeam != null)
		{
			tempBeam.DetachAndTurnOff();
			plr.SetEnabled(value: false);
			PortalLineRenderer[] array = sweepLines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetEnabled(value: false);
			}
			chargeParticle.Stop(false, (ParticleSystemStopBehavior)1);
		}
		beaming = false;
		inAction = false;
		dontTeleport = false;
		overrideRotation = false;
		RandomizeDirection();
	}

	public void Enrage()
	{
		if (enraged)
		{
			return;
		}
		enraged = true;
		if (ensims == null || ensims.Length == 0)
		{
			ensims = GetComponentsInChildren<EnemySimplifier>();
		}
		EnemySimplifier[] array = ensims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enraged = true;
		}
		if (tentacles.Length != 0)
		{
			Gradient gradient = new Gradient();
			GradientColorKey[] array2 = new GradientColorKey[2];
			array2[0].color = Color.red;
			array2[0].time = 0f;
			array2[1].color = Color.red;
			array2[1].time = 1f;
			GradientAlphaKey[] array3 = new GradientAlphaKey[2];
			array3[0].alpha = 1f;
			array3[0].time = 0f;
			array3[1].alpha = 0f;
			array3[1].time = 1f;
			gradient.SetKeys(array2, array3);
			for (int j = 0; j < tentacles.Length; j++)
			{
				TrailRenderer component = tentacles[j].GetComponent<TrailRenderer>();
				if ((bool)component)
				{
					component.colorGradient = gradient;
				}
			}
		}
		currentEnrageEffect = UnityEngine.Object.Instantiate(enrageEffect, base.transform.position, base.transform.rotation);
		currentEnrageEffect.transform.SetParent(base.transform, worldPositionStays: true);
		originalGlow.SetActive(value: false);
		enrageGlow.SetActive(value: true);
	}

	public void UnEnrage()
	{
		if (!enraged)
		{
			return;
		}
		enraged = false;
		if (ensims == null || ensims.Length == 0)
		{
			ensims = GetComponentsInChildren<EnemySimplifier>();
		}
		EnemySimplifier[] array = ensims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enraged = false;
		}
		for (int j = 0; j < tentacles.Length; j++)
		{
			TrailRenderer component = tentacles[j].GetComponent<TrailRenderer>();
			if ((bool)component)
			{
				component.colorGradient = originalTentacleGradient;
			}
		}
		UnityEngine.Object.Destroy(currentEnrageEffect);
		originalGlow.SetActive(value: true);
		enrageGlow.SetActive(value: false);
	}
}
