using System;
using Sandbox;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

public class Gutterman : EnemyScript, IEnrage, IAlter, IAlterOptions<bool>
{
	private bool gotValues;

	private EnemyIdentifier eid;

	private NavMeshAgent nma;

	private Enemy mach;

	private Rigidbody rb;

	private Animator anim;

	private int difficulty = -1;

	private float defaultMovementSpeed;

	[HideInInspector]
	public bool dead;

	[HideInInspector]
	public bool fallen;

	[HideInInspector]
	public bool exploded;

	public bool hasShield = true;

	public bool stationary;

	[SerializeField]
	private GameObject[] shield;

	public Transform torsoAimBone;

	public Transform gunAimBone;

	private Quaternion torsoDefaultRotation;

	[SerializeField]
	private SwingCheck2 sc;

	[SerializeField]
	private SwingCheck2 shieldlessSwingcheck;

	private bool inAction;

	private bool attacking;

	private bool moveForward;

	private bool trackInAction;

	public Transform shootPoint;

	public GameObject beam;

	private float windup;

	private float windupSpeed;

	[SerializeField]
	private AudioSource windupAud;

	[SerializeField]
	private Transform windupBarrel;

	private Quaternion barrelRotation;

	private bool slowMode;

	private float slowModeLerp;

	private bool firing;

	private float bulletCooldown;

	private float lineOfSightTimer;

	private float trackingSpeed;

	private float trackingSpeedMultiplier;

	private float defaultTrackingSpeed = 1f;

	private Vector3 trackingPosition;

	private Vector3 lastKnownPosition;

	private TimeSince lastParried;

	[SerializeField]
	private GameObject playerUnstucker;

	[SerializeField]
	private GameObject fallingKillTrigger;

	[SerializeField]
	private GameObject fallEffect;

	[SerializeField]
	private GameObject corpseExplosion;

	[SerializeField]
	private GameObject shieldBreakEffect;

	[SerializeField]
	private AudioSource bonkSound;

	[SerializeField]
	private AudioSource releaseSound;

	[SerializeField]
	private AudioSource deathSound;

	private bool enraged;

	public bool eternalRage;

	[SerializeField]
	private AudioSource enrageEffect;

	private AudioSource currentEnrageEffect;

	private float rageLeft;

	private EnemySimplifier[] ensims;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	private TargetHandle lastTargetHandle;

	private bool fallbackVision;

	private VisionQuery sightQuery;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => mach.chest.transform.position;

	private bool hasVision
	{
		get
		{
			if (!(targetHandle != null))
			{
				return fallbackVision;
			}
			return true;
		}
	}

	private float targetDistance => lastTargetData.DistanceTo(base.transform.position);

	public bool isEnraged => enraged;

	public string alterKey => "Gutterman";

	public string alterCategoryName => "Gutterman";

	public AlterOption<bool>[] options => new AlterOption<bool>[2]
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
		},
		new AlterOption<bool>
		{
			value = eternalRage,
			callback = delegate(bool value)
			{
				eternalRage = value;
			},
			key = "eternal-rage",
			name = "Eternal Rage"
		}
	};

	public override void OnGoLimp(bool fromExplosion)
	{
		base.OnGoLimp(fromExplosion);
		if (mach != null && mach.musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		Matrix4x4 enterToExit = details.enterToExit;
		trackingPosition = enterToExit.MultiplyPoint3x4(trackingPosition);
		lastKnownPosition = enterToExit.MultiplyPoint3x4(lastKnownPosition);
		if (targetHandle != null)
		{
			targetHandle = targetHandle.From(details.portalSequence);
		}
		if (lastTargetHandle != null)
		{
			lastTargetHandle = lastTargetHandle.From(details.portalSequence);
		}
	}

	private void OnTargetTeleport(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
		}
		if (lastTargetHandle != null && traveller.id == lastTargetHandle.id)
		{
			lastTargetHandle = lastTargetHandle.Then(details.portalSequence);
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		float speed;
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			speed = 10f;
			break;
		case 1:
			speed = 9f;
			break;
		default:
			speed = 8f;
			break;
		}
		float angularSpeed = 120f;
		float acceleration = 8f;
		return new EnemyMovementData
		{
			speed = speed,
			angularSpeed = angularSpeed,
			acceleration = acceleration
		};
	}

	private void Start()
	{
		GetValues();
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
	}

	public override void OnDamage(ref DamageData data)
	{
		if (hasShield)
		{
			if ((bool)eid && !eid.dead)
			{
				string hitter = data.hitter;
				if (hitter == "heavypunch" || hitter == "hammer")
				{
					ShieldBreak();
					goto IL_0061;
				}
			}
			data.damage /= 1.5f;
		}
		goto IL_0061;
		IL_0061:
		if ((bool)eid && eid.dead && fallen && !exploded && data.hitter == "ground slam")
		{
			Explode();
			MonoSingleton<NewMovement>.Instance.Launch(Vector3.up * 750f);
		}
	}

	private void GetValues()
	{
		if (gotValues)
		{
			return;
		}
		gotValues = true;
		eid = GetComponent<EnemyIdentifier>();
		nma = GetComponent<NavMeshAgent>();
		mach = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		ensims = GetComponentsInChildren<EnemySimplifier>();
		if (dead)
		{
			CheckIfInstaCorpse();
			return;
		}
		sightQuery = new VisionQuery("GuttermanSight", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.Environment)));
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (!hasShield)
		{
			GameObject[] array = shield;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].SetActive(value: false);
			}
		}
		anim.SetBool("Shield", hasShield);
		torsoDefaultRotation = Quaternion.Inverse(base.transform.rotation) * torsoAimBone.rotation;
		lastParried = 5f;
		barrelRotation = windupBarrel.localRotation;
		if (windupAud.GetPitch() != 0f)
		{
			windupAud.Play(tracked: true);
		}
		SetSpeed();
		SlowUpdate();
	}

	private void OnEnable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTeleport));
		}
		GetValues();
		CheckIfInstaCorpse();
	}

	private void OnDisable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTeleport));
		}
		inAction = false;
		CancelInvoke("DoneDying");
	}

	private void CheckIfInstaCorpse()
	{
		if (dead)
		{
			if ((bool)(UnityEngine.Object)(object)anim)
			{
				anim.Play("Death", 0, 1f);
			}
			fallen = true;
			if (TryGetComponent<Collider>(out var component))
			{
				component.enabled = false;
			}
			if ((bool)fallEffect)
			{
				fallEffect.transform.position = new Vector3(mach.chest.transform.position.x, base.transform.position.y, mach.chest.transform.position.z);
				fallEffect.SetActive(value: true);
			}
			Invoke("DoneDying", 0.5f);
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		GetValues();
		switch (difficulty)
		{
		case 2:
		case 3:
		case 4:
		case 5:
			anim.speed = 1f;
			defaultMovementSpeed = 10f;
			windupSpeed = 1f;
			trackingSpeedMultiplier = ((difficulty == 2) ? 0.8f : 1f);
			break;
		case 1:
			anim.speed = 0.9f;
			defaultMovementSpeed = 9f;
			windupSpeed = 0.75f;
			trackingSpeedMultiplier = 0.5f;
			break;
		case 0:
			anim.speed = 0.8f;
			defaultMovementSpeed = 8f;
			windupSpeed = 0.5f;
			trackingSpeedMultiplier = 0.35f;
			break;
		}
		if (eid.puppet)
		{
			trackingSpeedMultiplier *= 0.75f;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		defaultMovementSpeed *= eid.totalSpeedModifier;
		nma.speed = (slowMode ? (defaultMovementSpeed / 2f) : defaultMovementSpeed);
		windupSpeed *= eid.totalSpeedModifier;
		defaultTrackingSpeed = 1f;
		if (trackingSpeed < defaultTrackingSpeed)
		{
			trackingSpeed = defaultTrackingSpeed;
		}
	}

	private void Update()
	{
		if (dead)
		{
			return;
		}
		if (eid.target != null)
		{
			VisionUpdate();
			AttackUpdate();
		}
		else
		{
			targetHandle = null;
			fallbackVision = false;
		}
		lineOfSightTimer = Mathf.MoveTowards(lineOfSightTimer, hasVision ? 1 : 0, Time.deltaTime * 2f);
		if (lineOfSightTimer >= 0.9f || (slowMode && lineOfSightTimer > 0f))
		{
			windup = Mathf.MoveTowards(windup, 1f, Time.deltaTime * windupSpeed);
		}
		else
		{
			windup = Mathf.MoveTowards(windup, 0f, Time.deltaTime * windupSpeed);
		}
		windupAud.SetPitch(windup * 3f);
		if (windupAud.GetPitch() == 0f)
		{
			windupAud.Stop();
		}
		else if (!windupAud.isPlaying)
		{
			windupAud.Play(tracked: true);
		}
		slowModeLerp = Mathf.MoveTowards(slowModeLerp, slowMode ? 1 : 0, Time.deltaTime * 2.5f);
		anim.SetFloat("WalkSpeed", slowMode ? 0.5f : 1f);
		anim.SetBool("Walking", nma.velocity.magnitude > 2.5f);
		anim.SetLayerWeight(1, (float)(firing ? 1 : 0));
		if (eternalRage || !(rageLeft > 0f))
		{
			return;
		}
		rageLeft = Mathf.MoveTowards(rageLeft, 0f, Time.deltaTime * eid.totalSpeedModifier);
		if ((UnityEngine.Object)(object)currentEnrageEffect != null && rageLeft < 3f)
		{
			currentEnrageEffect.SetPitch(rageLeft / 3f);
		}
		if (!(rageLeft <= 0f))
		{
			return;
		}
		enraged = false;
		EnemySimplifier[] array = ensims;
		foreach (EnemySimplifier enemySimplifier in array)
		{
			if ((bool)enemySimplifier)
			{
				enemySimplifier.enraged = false;
			}
		}
		if ((UnityEngine.Object)(object)currentEnrageEffect != null)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentEnrageEffect).gameObject);
		}
	}

	private void AttackUpdate()
	{
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		Vector3 pos;
		Vector3 a;
		if (targetHandle != null || fallbackVision)
		{
			pos = lastTargetData.position;
			a = lastTargetData.headPosition;
		}
		else
		{
			if (!(lastTargetHandle != null))
			{
				return;
			}
			Matrix4x4 travelMatrix = PortalUtils.GetTravelMatrix(lastTargetHandle.portals);
			pos = travelMatrix.MultiplyPoint3x4(lastTargetHandle.target.Position);
			a = travelMatrix.MultiplyPoint3x4(lastTargetHandle.target.HeadPosition);
		}
		Vector3 vector = ToPlanePos(pos);
		if (inAction)
		{
			firing = false;
			if (((Behaviour)(object)nma).enabled)
			{
				nma.updateRotation = false;
			}
			if (difficulty <= 1)
			{
				windup = 0f;
			}
			trackingPosition = position + forward * Mathf.Max(5f, Vector3.Distance(position, vector)) + Vector3.up * (pos.y - position.y);
			if (trackInAction || moveForward)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(vector - position), (float)(trackInAction ? 360 : 90) * Time.deltaTime);
			}
			return;
		}
		if (windup >= 0.5f)
		{
			if (!slowMode)
			{
				trackingPosition = position + forward * Mathf.Max(30f, Vector3.Distance(a, position));
			}
			slowMode = true;
		}
		else if (slowMode && windup <= 0f)
		{
			slowMode = false;
			UnityEngine.Object.Instantiate<AudioSource>(releaseSound, position, Quaternion.identity);
		}
		if (firing && !mach.gc.onGround)
		{
			firing = false;
		}
		if (slowMode)
		{
			if (firing)
			{
				trackingSpeed += Time.deltaTime * (float)(hasShield ? 2 : 5) * trackingSpeedMultiplier * eid.totalSpeedModifier;
			}
			if (((Behaviour)(object)nma).enabled)
			{
				nma.updateRotation = false;
			}
			if (lineOfSightTimer > 0f)
			{
				lastKnownPosition = a;
			}
			trackingPosition = Vector3.MoveTowards(trackingPosition, lastKnownPosition, (Vector3.Distance(a, trackingPosition) + trackingSpeed) * Time.deltaTime);
			base.transform.rotation = Quaternion.LookRotation(new Vector3(trackingPosition.x, position.y, trackingPosition.z) - position);
		}
		else
		{
			trackingSpeed = defaultTrackingSpeed;
			if (((Behaviour)(object)nma).enabled)
			{
				nma.updateRotation = true;
			}
		}
		nma.speed = (slowMode ? (defaultMovementSpeed / 2f) : defaultMovementSpeed);
		if (hasVision && lineOfSightTimer >= 0.5f && (float)lastParried > 5f && mach.gc.onGround && Vector3.Distance(a, position) < 12f)
		{
			ShieldBash();
		}
	}

	private void LateUpdate()
	{
		if (!dead && eid.target != null)
		{
			if (inAction)
			{
				Quaternion quaternion = Quaternion.RotateTowards(torsoAimBone.rotation, Quaternion.LookRotation(torsoAimBone.position - trackingPosition, Vector3.up), 60f);
				Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation * torsoDefaultRotation) * torsoAimBone.rotation;
				torsoAimBone.rotation = quaternion * quaternion2;
				sc.knockBackDirection = trackingPosition - torsoAimBone.position;
			}
			else if (slowModeLerp > 0f)
			{
				torsoAimBone.rotation = Quaternion.Lerp(torsoAimBone.rotation, Quaternion.LookRotation(torsoAimBone.position - trackingPosition), slowModeLerp);
				Quaternion rotation = gunAimBone.rotation;
				gunAimBone.rotation = Quaternion.LookRotation(gunAimBone.position - trackingPosition);
				gunAimBone.Rotate(Vector3.left, 90f, Space.Self);
				gunAimBone.Rotate(Vector3.up, 180f, Space.Self);
				gunAimBone.rotation = Quaternion.Lerp(rotation, gunAimBone.rotation, slowModeLerp);
			}
			windupBarrel.localRotation = barrelRotation;
			if (windup > 0f)
			{
				windupBarrel.Rotate(Vector3.up * -3600f * windup * Time.deltaTime);
				barrelRotation = windupBarrel.localRotation;
			}
		}
	}

	private void FixedUpdate()
	{
		if (dead || eid.target == null)
		{
			return;
		}
		if (inAction && !stationary)
		{
			rb.isKinematic = !moveForward;
			if (moveForward)
			{
				float maxDistance = ((eid.target == null) ? 22f : Mathf.Max(22f, base.transform.position.y - eid.target.position.y + 2.5f));
				if (Physics.Raycast(base.transform.position + Vector3.up + base.transform.forward, Vector3.down, maxDistance, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					rb.velocity = base.transform.forward * (hasShield ? 25 : 45) * anim.speed * eid.totalSpeedModifier;
				}
				else
				{
					rb.velocity = Vector3.zero;
				}
			}
		}
		if (!firing)
		{
			return;
		}
		if (bulletCooldown == 0f)
		{
			Vector3 vector = shootPoint.position - shootPoint.forward * 4f;
			Quaternion quaternion = shootPoint.rotation;
			if (!PortalPhysicsV2.Raycast(vector, shootPoint.forward, 4f, LayerMaskDefaults.Get(LMD.EnvironmentAndPlayer)))
			{
				vector = shootPoint.position + shootPoint.right * UnityEngine.Random.Range(-0.2f, 0.2f) + shootPoint.up * UnityEngine.Random.Range(-0.2f, 0.2f);
			}
			PortalPhysicsV2.ProjectThroughPortals(VisionSourcePosition, vector - VisionSourcePosition, default(LayerMask), out var _, out var endPoint, out var traversals);
			bool flag = false;
			if (traversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV = traversals[0];
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				Portal portalObject = portalTraversalV.portalObject;
				if (portalObject.GetTravelFlags(portalHandle.side).HasFlag(PortalTravellerFlags.EnemyProjectile))
				{
					vector = endPoint;
					quaternion = PortalUtils.GetTravelMatrix(traversals).rotation * quaternion;
				}
				else
				{
					flag = !portalObject.passThroughNonTraversals;
				}
			}
			if (!flag)
			{
				UnityEngine.Object.Instantiate(beam, vector, quaternion).transform.Rotate(new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
			}
			bulletCooldown = 0.05f / windup;
		}
		else
		{
			bulletCooldown = Mathf.MoveTowards(bulletCooldown, 0f, Time.fixedDeltaTime);
		}
	}

	private void VisionUpdate()
	{
		fallbackVision = false;
		if (vision.TrySee(sightQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
			lastTargetHandle = targetHandle;
			return;
		}
		targetHandle = null;
		if (!Physics.Raycast(base.transform.position + Vector3.up * 3.5f, eid.target.headPosition - (base.transform.position + Vector3.up), Vector3.Distance(eid.target.position, base.transform.position + Vector3.up), LayerMaskDefaults.Get(LMD.Environment)))
		{
			lastTargetData = new TargetData
			{
				position = eid.target.position,
				headPosition = eid.target.headPosition
			};
			fallbackVision = true;
		}
	}

	private void SlowUpdate()
	{
		if (!dead)
		{
			Invoke("SlowUpdate", GetUpdateRate(nma));
			if (!inAction)
			{
				SetFiring();
				NavigationUpdate();
			}
		}
	}

	private void NavigationUpdate()
	{
		if (eid.target == null)
		{
			mach.SetDestination(base.transform.position);
		}
		else if ((bool)(UnityEngine.Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			if (!stationary)
			{
				mach.SetDestination(eid.target.position);
			}
			else
			{
				mach.SetDestination(base.transform.position);
			}
		}
	}

	private void SetFiring()
	{
		if (eid.target != null && slowMode && windup >= 0.5f && (firing || windup >= 1f) && mach.gc.onGround && ((UnityEngine.Object)(object)nma == null || !nma.isOnOffMeshLink))
		{
			firing = true;
			Vector3 vector = base.transform.position + Vector3.up + base.transform.forward * 3f;
			Vector3 vector2 = lastTargetData.headPosition - vector;
			PortalTraversalV2[] portalTraversals;
			PhysicsCastResult[] array = PortalPhysicsV2.RaycastAll(vector, vector2.normalized, vector2.magnitude, LayerMaskDefaults.Get(LMD.Enemies), out portalTraversals, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < array.Length; i++)
			{
				if (!FiringEnemyCheck(array[i]))
				{
					firing = false;
					break;
				}
			}
		}
		else
		{
			firing = false;
		}
	}

	private bool FiringEnemyCheck(PhysicsCastResult hit)
	{
		if (hit.transform == eid.target.targetTransform)
		{
			return true;
		}
		GameObject[] array = shield;
		if (array != null && array.Length > 0 && hit.transform == shield[0].transform)
		{
			return true;
		}
		if (hit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component) && (bool)component.eid && (component.eid == eid || component.eid.dead || component.eid == eid.target.enemyIdentifier))
		{
			return true;
		}
		return false;
	}

	private void Death()
	{
		UnityEngine.Object.Instantiate<AudioSource>(deathSound, base.transform);
		ShieldBashStop();
		dead = true;
		windupAud.Stop();
		anim.SetBool("Dead", true);
		anim.SetLayerWeight(1, 0f);
		anim.Play("Death", 0, 0f);
		if (TryGetComponent<Collider>(out var component))
		{
			component.enabled = false;
		}
		if (mach.gc.onGround)
		{
			rb.isKinematic = true;
		}
		else
		{
			rb.constraints = (RigidbodyConstraints)122;
		}
		if ((UnityEngine.Object)(object)currentEnrageEffect != null)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentEnrageEffect).gameObject);
		}
	}

	public void ShieldBreak(bool player = true, bool flash = true)
	{
		anim.Play("ShieldBreak", 0, 0f);
		anim.SetBool("Shield", false);
		if (player)
		{
			if (flash)
			{
				MonoSingleton<NewMovement>.Instance.Parry(null, "GUARD BREAK");
			}
			else
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(100, "<color=green>GUARD BREAK</color>");
			}
			if (difficulty >= 4)
			{
				Enrage();
			}
		}
		GameObject[] array = shield;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		hasShield = false;
		UnityEngine.Object.Instantiate<AudioSource>(bonkSound, base.transform.position, Quaternion.identity);
		UnityEngine.Object.Instantiate(shieldBreakEffect, shield[0].transform.position, Quaternion.identity);
		if (inAction)
		{
			ShieldBashStop();
			StopAction();
		}
		sc = shieldlessSwingcheck;
		inAction = true;
		attacking = false;
		trackInAction = false;
		((Behaviour)(object)nma).enabled = false;
		moveForward = false;
		firing = false;
		slowMode = false;
		windup = 0f;
	}

	private void ShieldBash()
	{
		if (difficulty <= 2 && hasShield)
		{
			lastParried = 3f;
		}
		anim.Play(hasShield ? "ShieldBash" : "Smack", 0, 0f);
		UnityEngine.Object.Instantiate((hasShield || enraged) ? MonoSingleton<DefaultReferenceManager>.Instance.unparryableFlash : MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, shield[0].transform.position + base.transform.forward, base.transform.rotation).transform.localScale *= 15f;
		inAction = true;
		((Behaviour)(object)nma).enabled = false;
		firing = false;
		attacking = true;
		trackInAction = true;
		if (!hasShield && !enraged)
		{
			mach.parryable = true;
		}
	}

	private void ShieldBashActive()
	{
		if (attacking)
		{
			sc.DamageStart();
			sc.knockBackDirectionOverride = true;
			sc.knockBackDirection = base.transform.forward;
			moveForward = true;
			trackInAction = false;
		}
	}

	private void ShieldBashStop()
	{
		sc.DamageStop();
		moveForward = false;
		mach.parryable = false;
		attacking = false;
	}

	private void StopAction()
	{
		if (!dead)
		{
			inAction = false;
			if (mach.gc.onGround)
			{
				rb.isKinematic = true;
				((Behaviour)(object)nma).enabled = true;
			}
			else
			{
				rb.isKinematic = false;
			}
		}
	}

	public void GotParried()
	{
		anim.Play("ShieldBreak", 0, 0f);
		ShieldBashStop();
		StopAction();
		inAction = true;
		trackInAction = false;
		attacking = false;
		((Behaviour)(object)nma).enabled = false;
		moveForward = false;
		if (difficulty >= 4)
		{
			Enrage();
		}
		else
		{
			lastParried = 0f;
		}
		windup = 0f;
		trackingSpeed = defaultTrackingSpeed;
		UnityEngine.Object.Instantiate<AudioSource>(bonkSound, base.transform.position, Quaternion.identity);
	}

	private void FallStart()
	{
		fallingKillTrigger.SetActive(value: true);
	}

	private void FallOver()
	{
		if (!fallEffect)
		{
			return;
		}
		if ((bool)MonoSingleton<EndlessGrid>.Instance)
		{
			Explode();
			return;
		}
		if (mach.gc.onGround)
		{
			for (int i = 0; i < mach.gc.cols.Count; i++)
			{
				if (mach.gc.cols[i].gameObject.CompareTag("Moving"))
				{
					Explode();
					return;
				}
			}
		}
		fallEffect.transform.position = new Vector3(mach.chest.transform.position.x, base.transform.position.y, mach.chest.transform.position.z);
		fallEffect.SetActive(value: true);
		fallingKillTrigger.SetActive(value: false);
		playerUnstucker.SetActive(value: true);
		fallen = true;
		Invoke("DoneDying", 1f);
	}

	public void Explode()
	{
		if (exploded)
		{
			return;
		}
		exploded = true;
		UnityEngine.Object.Instantiate(corpseExplosion, torsoAimBone.position, Quaternion.identity);
		if ((bool)mach)
		{
			EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
			{
				if (!(enemyIdentifierIdentifier == null))
				{
					mach.GetHurt(enemyIdentifierIdentifier.gameObject, (base.transform.position - enemyIdentifierIdentifier.transform.position).normalized * 1000f, 999f, 1f);
				}
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void DoneDying()
	{
		playerUnstucker.SetActive(value: false);
		((Behaviour)(object)anim).enabled = false;
	}

	public void SetStationary(bool status)
	{
		stationary = status;
		eid.stationary = status;
	}

	public void Enrage()
	{
		if (enraged)
		{
			return;
		}
		enraged = true;
		rageLeft = 10f;
		EnemySimplifier[] array = ensims;
		foreach (EnemySimplifier enemySimplifier in array)
		{
			if ((bool)enemySimplifier)
			{
				enemySimplifier.enraged = true;
			}
		}
		if ((UnityEngine.Object)(object)currentEnrageEffect == null)
		{
			currentEnrageEffect = UnityEngine.Object.Instantiate<AudioSource>(enrageEffect, mach.chest.transform);
			currentEnrageEffect.SetPitch(1f);
			((Component)(object)currentEnrageEffect).transform.localScale *= 0.01f;
		}
	}

	public void UnEnrage()
	{
		enraged = false;
		rageLeft = 0f;
		EnemySimplifier[] array = ensims;
		foreach (EnemySimplifier enemySimplifier in array)
		{
			if ((bool)enemySimplifier)
			{
				enemySimplifier.enraged = false;
			}
		}
		if ((UnityEngine.Object)(object)currentEnrageEffect != null)
		{
			UnityEngine.Object.Destroy(((Component)(object)currentEnrageEffect).gameObject);
		}
	}
}
