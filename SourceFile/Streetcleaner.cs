using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class Streetcleaner : EnemyScript
{
	private Animator anim;

	private NavMeshAgent nma;

	private Rigidbody rb;

	public bool dead;

	private TrailRenderer handTrail;

	private LayerMask enviroMask;

	public bool dodging;

	private float dodgeSpeed;

	private float dodgeCooldown;

	public GameObject dodgeSound;

	public Transform hose;

	public Transform hoseTarget;

	public GameObject canister;

	public AssetReference explosion;

	public bool canisterHit;

	public GameObject firePoint;

	private Transform warningFlame;

	private ParticleSystem firePart;

	private Light fireLight;

	private AudioSource fireAud;

	public GameObject fireStopSound;

	public bool damaging;

	private bool attacking;

	public GameObject warningFlash;

	[SerializeField]
	private Transform aimBone;

	private Quaternion torsoDefaultRotation;

	[SerializeField]
	private Transform flameThrowerBone;

	private int difficulty = -1;

	private float cooldown;

	private RaycastHit rhit;

	[HideInInspector]
	public EnemyIdentifier eid;

	private GroundCheckEnemy gc;

	private Enemy mach;

	private VisionQuery targetQuery;

	private TargetHandle targetHandle;

	private TargetData targetData;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private EnemyTarget target => eid.target;

	private Vision vision => mach.vision;

	public override Vector3 VisionSourcePosition => mach.chest.transform.position;

	private bool hasVision => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	public override void OnTeleport(PortalTravelDetails details)
	{
		if (targetHandle != null)
		{
			targetHandle = targetHandle.From(details.portalSequence);
			targetData = MonoSingleton<PortalManagerV2>.Instance.TargetTracker.CalculateData(targetHandle);
		}
	}

	public override bool ShouldKnockback(ref DamageData data)
	{
		return !dodging;
	}

	public override void OnDamage(ref DamageData data)
	{
		if (data.hitTarget == canister && eid.hitter == "revolver" && !canisterHit)
		{
			HandleCanisterHit(data.sourceWeapon);
		}
	}

	private void HandleCanisterHit(GameObject sourceWeapon)
	{
		if (!InvincibleEnemies.Enabled && !eid.blessed)
		{
			canisterHit = true;
		}
		if (!eid.dead && !InvincibleEnemies.Enabled && !eid.blessed)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(200, "ultrakill.instakill", sourceWeapon, eid);
		}
		MonoSingleton<TimeController>.Instance.ParryFlash();
		Invoke("CanisterExplosion", 0.1f);
	}

	public virtual void CanisterExplosion()
	{
		if (InvincibleEnemies.Enabled || eid.blessed)
		{
			if (canisterHit)
			{
				canisterHit = false;
			}
			return;
		}
		if (explosion == null || canister == null)
		{
			Debug.LogWarning("Canister explosion failed - Streetcleaner, explosion, or canister reference is null");
			return;
		}
		eid.Explode(fromExplosion: true);
		Explosion[] componentsInChildren = Object.Instantiate(explosion.ToAsset(), canister.transform.position, Quaternion.identity).GetComponentsInChildren<Explosion>();
		foreach (Explosion obj in componentsInChildren)
		{
			obj.maxSize *= 1.75f;
			obj.damage = 50;
			obj.friendlyFire = true;
		}
		if ((bool)mach)
		{
			mach.deathTokenSource?.Cancel();
			if (mach.chest != null)
			{
				CharacterJoint[] componentsInChildren2 = mach.chest.GetComponentsInChildren<CharacterJoint>();
				if (componentsInChildren2.Length != 0)
				{
					CharacterJoint[] array = componentsInChildren2;
					foreach (CharacterJoint characterJoint in array)
					{
						if (characterJoint.transform.parent.parent == mach.chest.transform)
						{
							if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
							{
								component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
							}
							Object.Destroy(characterJoint);
							characterJoint.transform.parent = null;
						}
					}
				}
			}
			if (MonoSingleton<BloodsplatterManager>.Instance.goreOn)
			{
				for (int j = 0; j < 2; j++)
				{
					GameObject gib = mach.bsm.GetGib(BSType.gib);
					if ((bool)gib && (bool)mach.gz && (bool)mach.gz.gibZone)
					{
						mach.ReadyGib(gib, canister);
					}
				}
			}
			if (mach.gz != null)
			{
				GameObject gore = mach.bsm.GetGore(GoreType.Head, eid, fromExplosion: true);
				gore.transform.position = canister.transform.position;
				gore.transform.SetParent(mach.gz.goreZone, worldPositionStays: true);
			}
			if (mach.chest != null)
			{
				mach.chest.transform.localScale = Vector3.zero;
			}
			if (mach.gz != null)
			{
				canister.transform.parent = mach.gz.transform;
				canister.transform.position = Vector3.zero;
			}
		}
		if (canister.TryGetComponent<Collider>(out var component2))
		{
			Object.Destroy(component2);
		}
		canister.transform.localScale = Vector3.zero;
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		if ((Object)(object)mach.anim != null)
		{
			mach.anim.StopPlayback();
		}
		BulletCheck componentInChildren = mach.GetComponentInChildren<BulletCheck>();
		if (componentInChildren != null)
		{
			Object.Destroy(componentInChildren.gameObject);
		}
		hose.SetParent(hoseTarget, worldPositionStays: true);
		hose.transform.localPosition = Vector3.zero;
		hose.transform.localScale = Vector3.zero;
		StopFire();
		dead = true;
		damaging = false;
		FireZone componentInChildren2 = mach.GetComponentInChildren<FireZone>();
		if (componentInChildren2 != null)
		{
			Object.Destroy(componentInChildren2.gameObject);
		}
		if (!(canister != null))
		{
			return;
		}
		canister.GetComponentInChildren<ParticleSystem>().Stop();
		AudioSource componentInChildren3 = canister.GetComponentInChildren<AudioSource>();
		if ((Object)(object)componentInChildren3 != null)
		{
			if (((Component)(object)componentInChildren3).TryGetComponent(out AudioLowPassFilter component))
			{
				Object.Destroy((Object)(object)component);
			}
			Object.Destroy((Object)(object)componentInChildren3);
		}
	}

	public override void OnFall()
	{
		StopFire();
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		eid = GetComponent<EnemyIdentifier>();
		anim = GetComponentInChildren<Animator>();
		nma = GetComponent<NavMeshAgent>();
		enviroMask = LayerMaskDefaults.Get(LMD.Environment);
		enviroMask = (int)enviroMask | 0x800;
	}

	private void Start()
	{
		if (!dead)
		{
			handTrail = GetComponentInChildren<TrailRenderer>();
			handTrail.emitting = false;
			warningFlame = firePoint.GetComponentInChildren<SpriteRenderer>().transform;
			warningFlame.localScale = Vector3.zero;
			firePart = firePoint.GetComponentInChildren<ParticleSystem>();
			fireLight = firePoint.GetComponentInChildren<Light>();
			fireLight.enabled = false;
			fireAud = firePoint.GetComponent<AudioSource>();
			torsoDefaultRotation = Quaternion.Inverse(base.transform.rotation) * aimBone.rotation;
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			gc = GetComponentInChildren<GroundCheckEnemy>();
			mach = GetComponent<Enemy>();
			targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && t.SqrDist(VisionSourcePosition) <= 3600f && !t.IsObstructed(VisionSourcePosition, enviroMask));
			SlowUpdate();
		}
	}

	private void OnDisable()
	{
		if (dodging)
		{
			StopMoving();
		}
	}

	private void SlowUpdate()
	{
		if (dead)
		{
			return;
		}
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.25f));
		if ((Object)(object)nma == null || !((Behaviour)(object)nma).enabled || !nma.isOnNavMesh || mach.isTraversingPortalLink)
		{
			return;
		}
		lastDimensionalTarget = Vector3.zero;
		if (target != null)
		{
			if (mach.TryGetDimensionalTarget(target.position, out lastDimensionalTarget))
			{
				mach.SetDestination(EnemyTarget.GetNavPoint(enviroMask, lastDimensionalTarget));
			}
			else
			{
				mach.SetDestination(target.GetNavPoint(enviroMask));
			}
		}
	}

	private void Update()
	{
		if (dead)
		{
			return;
		}
		if (dodgeCooldown != 0f)
		{
			dodgeCooldown = Mathf.MoveTowards(dodgeCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (difficulty <= 2 && cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (target == null)
		{
			if ((bool)(Object)(object)nma && nma.isOnNavMesh)
			{
				nma.isStopped = true;
				nma.ResetPath();
			}
			if ((bool)(Object)(object)anim)
			{
				SetRunAnimation(running: false, walking: false);
			}
			targetHandle = null;
			return;
		}
		if (damaging)
		{
			TryIgniteStains();
		}
		UpdateVision();
		float num = targetData.DistanceTo(mach.chest.transform.position);
		if (target != null && target.isEnemy && target.enemyIdentifier != null)
		{
			num *= target.enemyIdentifier.GetReachDistanceMultiplier();
		}
		bool flag = false;
		flag = !hasDimensionalTarget && num <= (float)(attacking ? 16 : 6);
		if (flag != attacking)
		{
			if (attacking)
			{
				StopFire();
			}
			else if (difficulty > 2 || cooldown <= 0f)
			{
				attacking = true;
				GameObject obj = Object.Instantiate(warningFlash, firePoint.transform.position, firePoint.transform.rotation);
				obj.transform.localScale = Vector3.one * 8f;
				obj.transform.SetParent(firePoint.transform, worldPositionStays: true);
				Invoke("StartFire", ((difficulty >= 2) ? 0.5f : 1f) / eid.totalSpeedModifier);
			}
		}
		if ((Object)(object)nma != null && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			float num2 = 12f;
			if (difficulty <= 1)
			{
				num2 = ((difficulty == 0) ? 4 : 10);
			}
			bool running = !attacking && (nma.velocity.magnitude > num2 || mach.isTraversingPortalLink);
			bool flag2 = nma.velocity.magnitude > 2f || mach.isTraversingPortalLink;
			SetRunAnimation(running, flag2);
			nma.updateRotation = flag2 && !mach.isTraversingPortalLink;
			float num3 = ((difficulty < 4) ? 1f : (flag ? 1.25f : 1.5f));
			anim.SetFloat("RunSpeed", num3);
			switch (difficulty)
			{
			case 4:
			case 5:
				nma.speed = (flag ? 20 : 24);
				break;
			case 2:
			case 3:
				nma.speed = 16f;
				break;
			case 1:
				nma.speed = (flag ? 7 : 14);
				break;
			case 0:
				nma.speed = (flag ? 1 : 10);
				break;
			}
			NavMeshAgent obj2 = nma;
			obj2.speed *= eid.totalSpeedModifier;
		}
		else if ((Object)(object)nma != null)
		{
			nma.updateRotation = false;
		}
	}

	private void UpdateVision()
	{
		if (vision != null && vision.TrySee(targetQuery, out var data))
		{
			TargetData targetData = data.ToData();
			this.targetData = targetData;
			targetHandle = data.CreateHandle();
		}
		else
		{
			targetHandle = null;
		}
	}

	private void SetRunAnimation(bool running, bool walking)
	{
		anim.SetBool("Running", running);
		anim.SetBool("Walking", walking);
	}

	private void FixedUpdate()
	{
		if (!dead && dodging)
		{
			rb.velocity = -1f * dodgeSpeed * eid.totalSpeedModifier * base.transform.forward;
			dodgeSpeed = dodgeSpeed * 0.95f / Mathf.Max(1f, eid.totalSpeedModifier);
		}
	}

	private void LateUpdate()
	{
		if (difficulty >= 4 && attacking && target != null)
		{
			Vector3 vector = (hasVision ? targetData.headPosition : target.headPosition);
			float maxDegreesDelta = ((difficulty == 5) ? 90 : 35);
			Quaternion rotation = aimBone.rotation;
			Quaternion quaternion = Quaternion.RotateTowards(aimBone.rotation, Quaternion.LookRotation(vector - aimBone.position, Vector3.up), maxDegreesDelta);
			Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation * torsoDefaultRotation) * aimBone.rotation;
			if (Vector3.Dot(Vector3.up, quaternion * Vector3.forward) > 0f)
			{
				aimBone.rotation = quaternion * quaternion2;
			}
			Quaternion quaternion3 = Quaternion.Inverse(rotation) * aimBone.rotation;
			quaternion3 = Quaternion.Euler(0f - quaternion3.eulerAngles.y, quaternion3.eulerAngles.z, 0f - quaternion3.eulerAngles.x);
			flameThrowerBone.rotation *= quaternion3;
		}
	}

	public void StartFire()
	{
		fireAud.Play(tracked: true);
		firePart.Play();
		fireLight.enabled = true;
		Invoke("StartDamaging", 0.15f / eid.totalSpeedModifier);
		if (difficulty <= 2)
		{
			Invoke("StopFire", ((difficulty == 0) ? 0.5f : 1f) / eid.totalSpeedModifier);
		}
	}

	public void StartDamaging()
	{
		damaging = true;
	}

	public void StopFire()
	{
		if ((bool)(Object)(object)fireAud && fireAud.isPlaying)
		{
			fireAud.Stop();
			Object.Instantiate(fireStopSound, firePoint.transform.position, Quaternion.identity);
		}
		attacking = false;
		CancelInvoke("StartFire");
		CancelInvoke("StartDamaging");
		firePart.Stop();
		fireLight.enabled = false;
		warningFlame.localScale = Vector3.zero;
		damaging = false;
		if (difficulty < 3)
		{
			switch (difficulty)
			{
			case 2:
				cooldown = 1f;
				break;
			case 1:
				cooldown = 1.5f;
				break;
			case 0:
				cooldown = 2f;
				break;
			}
			CancelInvoke("StopFire");
		}
	}

	public void Dodge()
	{
		if (!dead && !(dodgeCooldown > 0f))
		{
			dodgeCooldown = Random.Range(2f, 4f);
			dodgeSpeed = 60f;
			((Behaviour)(object)nma).enabled = false;
			rb.isKinematic = false;
			eid.hookIgnore = true;
			StopFire();
			Object.Instantiate(dodgeSound, base.transform.position, Quaternion.identity);
			anim.SetTrigger("Dodge");
			dodging = true;
			bool num = Physics.Raycast(base.transform.position + Vector3.up, base.transform.right, 5f, enviroMask, QueryTriggerInteraction.Ignore);
			bool flag = Physics.Raycast(base.transform.position + Vector3.up, base.transform.right * -1f, 5f, enviroMask, QueryTriggerInteraction.Ignore);
			if (num && flag)
			{
				base.transform.LookAt(base.transform.position + base.transform.right * ((!(Random.Range(0f, 1f) > 0.5f)) ? 1 : (-1)));
			}
			else
			{
				base.transform.LookAt(base.transform.position + base.transform.right * ((!flag) ? 1 : (-1)));
			}
		}
	}

	public void StopMoving()
	{
		if (dead)
		{
			return;
		}
		dodging = false;
		((Behaviour)(object)nma).enabled = false;
		eid.hookIgnore = false;
		if (gc.onGround)
		{
			rb.isKinematic = true;
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(gc.transform.position, ref val, 4f, 1))
			{
				nma.Warp(((NavMeshHit)(ref val)).position);
				((Behaviour)(object)nma).enabled = true;
			}
		}
		rb.velocity = Vector3.zero;
	}

	public void DeflectShot()
	{
		if (!dead)
		{
			anim.SetLayerWeight(1, 1f);
			anim.SetTrigger("Deflect");
			handTrail.emitting = true;
		}
	}

	public void SlapOver()
	{
		if (!dead)
		{
			handTrail.emitting = false;
		}
	}

	public void OverrideOver()
	{
		if (!dead)
		{
			anim.SetLayerWeight(1, 0f);
		}
	}

	private void TryIgniteStains()
	{
		Vector3 position = firePoint.transform.position;
		Vector3 forward = firePoint.transform.forward;
		Vector3 worldPosition = position + forward * 3.75f;
		Vector3 worldPosition2 = position + forward * 7.5f;
		Vector3 worldPosition3 = position + forward * 15f;
		StainVoxelManager instance = MonoSingleton<StainVoxelManager>.Instance;
		if (!(instance == null))
		{
			instance.TryIgniteAt(position, 1);
			instance.TryIgniteAt(worldPosition, 1);
			instance.TryIgniteAt(worldPosition2, 1);
			instance.TryIgniteAt(worldPosition3, 1);
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		EnemyMovementData result = default(EnemyMovementData);
		switch (difficulty)
		{
		case 4:
		case 5:
			result.speed = 24f;
			break;
		case 2:
		case 3:
			result.speed = 16f;
			break;
		case 1:
			result.speed = 14f;
			break;
		case 0:
			result.speed = 10f;
			break;
		}
		result.acceleration = 64f;
		result.angularSpeed = 1600f;
		return result;
	}
}
