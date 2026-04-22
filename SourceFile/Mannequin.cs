using ULTRAKILL.Cheats.UnityEditor;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class Mannequin : EnemyScript
{
	private bool gotValues;

	private Animator anim;

	private NavMeshAgent nma;

	private NavMeshPath nmp;

	private Enemy enemy;

	private EnemyIdentifier eid;

	private Rigidbody rb;

	private SwingCheck2 sc;

	public GameObject bloodSpray;

	private bool skitterMode;

	private float walkSpeed = 22f;

	private float skitterSpeed = 64f;

	private int difficulty = -1;

	public bool inAction;

	public MannequinBehavior behavior;

	public bool dontChangeBehavior;

	public bool dontAutoDrop;

	public bool dontMeleeBehavior;

	public bool stationary;

	private Vector3 randomMovementTarget;

	private bool trackTarget;

	private bool moveForward;

	[SerializeField]
	private TrailRenderer[] trails;

	[SerializeField]
	private Transform shootPoint;

	private bool aiming;

	[SerializeField]
	private Transform aimBone;

	private Vector3 aimPoint;

	public Projectile projectile;

	public GameObject chargeProjectile;

	[HideInInspector]
	public GameObject currentChargeProjectile;

	private bool chargingProjectile;

	private float meleeCooldown = 0.5f;

	private float projectileCooldown = 1f;

	private float jumpCooldown = 2f;

	private float meleeBehaviorCancel = 3.5f;

	public bool inControl;

	private bool canCling = true;

	[HideInInspector]
	public bool clinging;

	private Collider clungSurfaceCollider;

	private int attacksWhileClinging;

	private Vector3 clingNormal;

	private Vector3? clungMovementTarget;

	[SerializeField]
	private float clungMovementTolerance = 1.25f;

	private bool firstClingCheck = true;

	public AudioSource clingSound;

	private Collider col;

	[SerializeField]
	private AudioSource skitterSound;

	public string mostRecentAction;

	[HideInInspector]
	public bool jumping;

	public bool hasShootTarget;

	public TargetData shootTarget;

	public VisionQuery shootQuery;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private static bool debug => MannequinDebugGizmos.Enabled;

	public override Vector3 VisionSourcePosition => eid.overrideCenter.position;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private void Awake()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		anim = GetComponent<Animator>();
		nma = GetComponent<NavMeshAgent>();
		enemy = GetComponent<Enemy>();
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		sc = GetComponentInChildren<SwingCheck2>();
		col = GetComponent<Collider>();
		nmp = new NavMeshPath();
	}

	public override void OnLand()
	{
		if (enemy.fallTime > 0.2f)
		{
			Landing();
		}
		else
		{
			inControl = true;
		}
		ResetMovementTarget();
	}

	private void Start()
	{
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		shootQuery = new VisionQuery("MannequinShoot", (TargetDataRef target) => EnemyScript.CheckTarget(target, eid) && !target.IsObstructed(VisionSourcePosition, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)));
		GetValues();
		SlowUpdate();
	}

	private void OnEnable()
	{
		CancelActions(changeBehavior: false);
	}

	private void GetValues()
	{
		if (!gotValues)
		{
			gotValues = true;
			if (difficulty < 0)
			{
				difficulty = Enemy.InitializeDifficulty(eid);
			}
			skitterSound.priority = Random.Range(100, 200);
			SetSpeed();
			if (behavior == MannequinBehavior.Random)
			{
				ChangeBehavior();
			}
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = (skitterMode ? skitterSpeed : walkSpeed),
			angularSpeed = 36000f,
			acceleration = 1000f
		};
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
		case 4:
		case 5:
			anim.speed = 1.25f;
			walkSpeed = 20f;
			skitterSpeed = 64f;
			break;
		case 2:
		case 3:
			anim.speed = 1f;
			walkSpeed = 16f;
			skitterSpeed = 64f;
			break;
		case 1:
			anim.speed = 0.85f;
			walkSpeed = 12f;
			skitterSpeed = 48f;
			break;
		case 0:
			anim.speed = 0.75f;
			walkSpeed = 10f;
			skitterSpeed = 32f;
			break;
		}
		walkSpeed *= eid.totalSpeedModifier;
		skitterSpeed *= eid.totalSpeedModifier;
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
		anim.SetFloat("DifficultyDependentSpeed", (difficulty <= 2) ? 0.66f : 1f);
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", GetUpdateRate(nma));
		if (inAction || eid.target == null)
		{
			((Behaviour)(object)nma).enabled = false;
			return;
		}
		if (enemy.gc.onGround)
		{
			((Behaviour)(object)nma).enabled = true;
		}
		bool num = ((Behaviour)(object)nma).enabled && enemy.gc.onGround && nma.isOnNavMesh;
		hasShootTarget = enemy.vision.TrySee(shootQuery, out var data);
		shootTarget = (hasShootTarget ? data.ToData() : default(TargetData));
		if (!num)
		{
			if (!clinging)
			{
				return;
			}
			if (hasShootTarget)
			{
				if (projectileCooldown <= 0f && !clungMovementTarget.HasValue)
				{
					ProjectileAttack();
				}
			}
			else if (!stationary)
			{
				inControl = true;
				anim.SetBool("InControl", true);
				Uncling();
			}
			return;
		}
		canCling = true;
		if (meleeCooldown <= 0f && Vector3.Distance(eid.target.position, base.transform.position) < 5f)
		{
			MeleeAttack();
			return;
		}
		if (!hasShootTarget)
		{
			enemy.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
		}
		if (behavior == MannequinBehavior.Melee || (!hasShootTarget && !hasDimensionalTarget))
		{
			if (!stationary)
			{
				randomMovementTarget = base.transform.position;
				MoveToTarget(GetTargetPosition(), forceSkitter: true);
			}
		}
		else if (projectileCooldown <= 0f && !hasDimensionalTarget)
		{
			ProjectileAttack();
		}
		else
		{
			if (stationary)
			{
				return;
			}
			Vector3 vector = (hasDimensionalTarget ? lastDimensionalTarget : eid.target.position);
			if (Vector3.Distance(vector, base.transform.position) > 50f)
			{
				SetMovementTarget(vector - base.transform.position, Vector3.Distance(vector, base.transform.position) - 40f);
				return;
			}
			if (behavior == MannequinBehavior.RunAway && Vector3.Distance(vector, base.transform.position) < 15f)
			{
				SetMovementTarget(base.transform.position - vector, 20f - Vector3.Distance(vector, base.transform.position));
				return;
			}
			if (canCling && behavior == MannequinBehavior.Jump && jumpCooldown <= 0f && Physics.Raycast(base.transform.position + Vector3.up, Vector3.up, out var hitInfo, 40f, LayerMaskDefaults.Get(LMD.Environment)) && !Physics.Raycast(hitInfo.point - Vector3.up * 3f, vector - (hitInfo.point - Vector3.up * 3f), Vector3.Distance(vector, hitInfo.point - Vector3.up * 3f), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Ignore))
			{
				Jump();
				return;
			}
			enemy.SetDestination(randomMovementTarget);
			if (Vector3.Distance(base.transform.position, randomMovementTarget) < 5f)
			{
				SetMovementTarget(Random.onUnitSphere);
			}
			else
			{
				enemy.SetDestination(randomMovementTarget);
			}
		}
	}

	private void Update()
	{
		anim.SetBool("Walking", enemy.gc.onGround && nma.velocity.magnitude > 3f);
		anim.SetBool("Skittering", skitterMode);
		anim.SetBool("InControl", inControl);
		nma.speed = (skitterMode ? skitterSpeed : walkSpeed);
		if (skitterMode && nma.velocity.magnitude > 3f)
		{
			if (!skitterSound.isPlaying)
			{
				skitterSound.SetPitch(Random.Range(0.9f, 1.1f));
				skitterSound.Play(tracked: true);
				skitterSound.time = Random.Range(0f, skitterSound.clip.length);
			}
		}
		else
		{
			skitterSound.Stop();
		}
		if (trackTarget && eid.target != null && (inAction || clinging))
		{
			float num = Vector3.Dot(base.transform.up, eid.target.position - base.transform.position);
			Quaternion quaternion = Quaternion.LookRotation(eid.target.position - base.transform.up * num - base.transform.position, base.transform.up);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, Mathf.Max(Quaternion.Angle(base.transform.rotation, quaternion), 10f) * 10f * Time.deltaTime);
		}
		if (meleeCooldown > 0f)
		{
			meleeCooldown = Mathf.MoveTowards(meleeCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (projectileCooldown > 0f)
		{
			projectileCooldown = Mathf.MoveTowards(projectileCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (jumpCooldown > 0f)
		{
			jumpCooldown = Mathf.MoveTowards(jumpCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (behavior == MannequinBehavior.Melee && !inAction && meleeBehaviorCancel > 0f)
		{
			meleeBehaviorCancel = Mathf.MoveTowards(meleeBehaviorCancel, 0f, Time.deltaTime * eid.totalSpeedModifier);
			if (meleeBehaviorCancel <= 0f)
			{
				ChangeBehavior(noMelee: true);
			}
		}
		if (((Behaviour)(object)nma).enabled && !inAction && (behavior == MannequinBehavior.RunAway || behavior == MannequinBehavior.Wander) && nma.velocity.magnitude > 2f)
		{
			Vector3 origin = eid.overrideCenter.position + Vector3.up * 0.5f;
			Vector3 normalized = nma.velocity.normalized;
			normalized.y = 0f;
			if (Physics.Raycast(new Ray(origin, normalized), out var hitInfo, 6f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				Ray ray = new Ray(origin, Quaternion.Euler(0f, -90f, 0f) * normalized);
				Ray ray2 = new Ray(origin, Quaternion.Euler(0f, 90f, 0f) * normalized);
				float maxDistance = 2f;
				if (Physics.Raycast(ray, out var hitInfo2, maxDistance, LayerMaskDefaults.Get(LMD.Environment)) || Physics.Raycast(ray2, out hitInfo2, maxDistance, LayerMaskDefaults.Get(LMD.Environment)))
				{
					if (debug)
					{
						Debug.Log("Space too tight, ignoring cling attempt", base.gameObject);
					}
					return;
				}
				clungMovementTarget = null;
				ClingToSurface(hitInfo);
				RelocateWhileClinging(ClungMannequinMovementDirection.Vertical);
			}
		}
		if (clungMovementTarget.HasValue && clinging && !inAction)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, clungMovementTarget.Value, 30f * Time.deltaTime * eid.totalSpeedModifier);
			if (Vector3.Distance(base.transform.position, clungMovementTarget.Value) < 0.1f)
			{
				if (debug)
				{
					Debug.Log("Reached clung movement target", base.gameObject);
				}
				clungMovementTarget = null;
				skitterMode = false;
				if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out var _, 3f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					if (debug)
					{
						Debug.Log("We've hit the floor while cling walking. Let's jump off", base.gameObject);
					}
					Uncling();
				}
			}
		}
		if (clinging && (clungSurfaceCollider == null || !clungSurfaceCollider.enabled || !clungSurfaceCollider.gameObject.activeInHierarchy))
		{
			Uncling();
		}
	}

	private void FixedUpdate()
	{
		if (canCling && !enemy.gc.onGround)
		{
			CheckClings();
		}
		anim.SetFloat("ySpeed", rb.isKinematic ? 0f : rb.velocity.y);
		if (inAction && moveForward && !Physics.Raycast(base.transform.position + Vector3.up * 3f, base.transform.forward, 55f * Time.fixedDeltaTime * eid.totalSpeedModifier, LayerMaskDefaults.Get(LMD.Player), QueryTriggerInteraction.Ignore))
		{
			if (enemy.IsLedgeSafe())
			{
				rb.velocity = base.transform.forward * 55f * anim.speed * eid.totalSpeedModifier;
			}
			else
			{
				rb.velocity = Vector3.zero;
			}
		}
	}

	private void LateUpdate()
	{
		if (aiming)
		{
			if (trackTarget && hasShootTarget)
			{
				aimPoint = aimBone.position - shootTarget.position;
			}
			aimBone.LookAt(aimBone.position + aimPoint, base.transform.up);
		}
	}

	private float EvaluateMaxClingWalkDistance(Vector3 origin, Vector3 movementDirection, Vector3 backToWallDirection, float maxDistance = 20f, float incrementLength = 1.5f)
	{
		float num = 0f;
		Vector3 vector = origin;
		Vector3 vector2 = clingNormal * clungMovementTolerance;
		while (num < maxDistance)
		{
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(new Ray(vector + vector2, backToWallDirection), out hitInfo, clungMovementTolerance * 2f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore);
			if (!(Vector3.Angle(hitInfo.normal, clingNormal) < 5f))
			{
				flag = false;
			}
			if (flag)
			{
				if (Physics.Raycast(new Ray(vector + vector2 - movementDirection.normalized * 0.1f, movementDirection), out hitInfo, incrementLength * 1.25f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					return num - incrementLength * 1.5f;
				}
				num += incrementLength;
				vector += movementDirection * incrementLength;
				continue;
			}
			return num - incrementLength * 1.5f;
		}
		if (num == 0f)
		{
			return 0f;
		}
		return num - incrementLength * 1.5f;
	}

	private void RelocateWhileClinging(ClungMannequinMovementDirection direction)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = ((!(Mathf.Abs(Vector3.Dot(clingNormal, Vector3.up)) < 0.99f)) ? Vector3.Cross(clingNormal, Vector3.right).normalized : Vector3.Cross(clingNormal, Vector3.up).normalized);
		Vector3 normalized = Vector3.Cross(clingNormal, vector).normalized;
		Vector3 vector2 = ((direction != ClungMannequinMovementDirection.Horizontal) ? normalized : vector);
		float maxInclusive = EvaluateMaxClingWalkDistance(position, vector2, -clingNormal);
		float num = Random.Range(0f - EvaluateMaxClingWalkDistance(position, -vector2, -clingNormal), maxInclusive);
		if (Mathf.Abs(num) <= 2f)
		{
			return;
		}
		Vector3 vector3 = position + vector2 * num;
		if (Physics.Raycast(new Ray(vector3 + clingNormal * clungMovementTolerance, -clingNormal), out var hitInfo, clungMovementTolerance * 2f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			if (debug)
			{
				Debug.Log($"Accounting for bump at target. Distance: {Vector3.Distance(vector3, hitInfo.point)}", base.gameObject);
			}
			vector3 = hitInfo.point;
		}
		MoveToTarget(vector3, forceSkitter: true, clungMode: true);
	}

	private void CheckClings()
	{
		bool flag = firstClingCheck;
		firstClingCheck = false;
		if (Physics.Raycast(base.transform.position, Vector3.up, out var hitInfo, flag ? 9.5f : 7f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore) && hitInfo.normal.y <= 0f)
		{
			ClingToSurface(hitInfo);
		}
		else if (flag || new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude > 3f)
		{
			Collider[] array = Physics.OverlapSphere(col.bounds.center, 2f, LayerMaskDefaults.Get(LMD.Environment));
			if (array != null && array.Length != 0 && Physics.Raycast(col.bounds.center, array[0].ClosestPoint(col.bounds.center) - col.bounds.center, out hitInfo, flag ? 3.5f : 2f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				ClingToSurface(hitInfo);
			}
		}
	}

	private void ClingToSurface(RaycastHit hit)
	{
		CancelActions();
		Vector3 point = hit.point;
		Vector3 normal = hit.normal;
		canCling = false;
		clinging = true;
		clungSurfaceCollider = hit.collider;
		skitterMode = false;
		enemy.gc.ForceOff();
		base.transform.position = point;
		base.transform.up = normal;
		trackTarget = true;
		clingNormal = normal.normalized;
		((Behaviour)(object)nma).enabled = false;
		enemy.overrideFalling = true;
		rb.isKinematic = true;
		rb.SetGravityMode(useGravity: false);
		anim.SetBool("Clinging", true);
		anim.Play("WallCling");
		if (!firstClingCheck)
		{
			Object.Instantiate<AudioSource>(clingSound, base.transform.position, Quaternion.identity);
		}
		projectileCooldown = Random.Range(0f, 0.5f);
	}

	public void Uncling()
	{
		clinging = false;
		clungSurfaceCollider = null;
		CancelActions();
		Vector3 vector = new Vector3(clingNormal.x * 2f, clingNormal.y * 6f, clingNormal.z * 2f);
		if (Mathf.Abs(vector.y) < 6f && Physics.Raycast(new Ray(col.bounds.center, Vector3.up), out var _, 4f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			vector.y = -6f;
		}
		base.transform.LookAt(((bool)eid && eid.target != null) ? new Vector3(eid.target.position.x, base.transform.position.y, eid.target.position.z) : (base.transform.position + clingNormal));
		base.transform.position += vector;
		trackTarget = false;
		Invoke("DelayedGroundCheckReenable", 0.1f);
		jumpCooldown = 2f;
		skitterMode = false;
		attacksWhileClinging = 0;
		enemy.overrideFalling = false;
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		if (inControl)
		{
			rb.AddForce(Vector3.down * 50f, ForceMode.VelocityChange);
		}
		anim.SetBool("Clinging", false);
	}

	private void MeleeAttack()
	{
		if (!inAction)
		{
			inAction = true;
			mostRecentAction = "Melee Attack";
			meleeCooldown = 2f / eid.totalSpeedModifier;
			((Behaviour)(object)nma).enabled = false;
			anim.Play("MeleeAttack");
			trackTarget = true;
		}
	}

	private void ProjectileAttack()
	{
		if (!inAction)
		{
			inAction = true;
			mostRecentAction = "Projectile Attack";
			projectileCooldown = Random.Range(6f - (float)difficulty, 8f - (float)difficulty) / eid.totalSpeedModifier;
			((Behaviour)(object)nma).enabled = false;
			anim.Play(clinging ? "WallClingProjectile" : "ProjectileAttack");
			trackTarget = true;
			aiming = true;
			chargingProjectile = true;
			if (clinging)
			{
				attacksWhileClinging++;
			}
		}
	}

	private void Jump()
	{
		if (!inAction)
		{
			inAction = true;
			mostRecentAction = "Jump";
			jumping = true;
			enemy.overrideFalling = true;
			skitterMode = false;
			((Behaviour)(object)nma).enabled = false;
			jumpCooldown = 2f;
			anim.SetBool("Jump", true);
		}
	}

	private void JumpNow()
	{
		enemy.gc.ForceOff();
		Invoke("DelayedGroundCheckReenable", 0.1f);
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
		inControl = true;
		skitterMode = false;
		anim.SetBool("Jump", false);
		anim.SetBool("InControl", inControl);
	}

	private void MoveToTarget(Vector3 target, bool forceSkitter = false, bool clungMode = false)
	{
		if (clungMode)
		{
			if (debug)
			{
				Debug.Log("Starting clung movement");
			}
			clungMovementTarget = target;
			skitterMode = true;
		}
		else if (!inAction)
		{
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(target, ref val, 15f, nma.areaMask))
			{
				target = ((NavMeshHit)(ref val)).position;
			}
			nma.CalculatePath(target, nmp);
			skitterMode = forceSkitter || ((difficulty >= 3 || Random.Range(0f, 1f) > 0.5f) && Vector3.Distance(base.transform.position, target) > 15f);
			nma.path = nmp;
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		OnDeath();
		GameObject gore = enemy.bsm.GetGore(GoreType.Head, eid, fromExplosion);
		gore.transform.position = enemy.chest.transform.position;
		enemy.GetGoreZone().SetGoreZone(gore);
		gore.SetActive(value: true);
	}

	public override void OnFall()
	{
		if (inAction && !jumping && !inControl)
		{
			CancelActions();
		}
	}

	public void OnDeath()
	{
		if ((bool)currentChargeProjectile)
		{
			Object.Destroy(currentChargeProjectile);
		}
		if (TryGetComponent<KeepInBounds>(out var component))
		{
			Object.Destroy(component);
		}
		skitterSound.Stop();
		sc.DamageStop();
		TrailRenderer[] array = trails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].emitting = false;
		}
		enemy.parryable = false;
		Object.Destroy(this);
	}

	private void StopTracking(int parryable = 0)
	{
		if (eid.target != null)
		{
			base.transform.LookAt(base.transform.position + (new Vector3(eid.target.position.x, base.transform.position.y, eid.target.position.z) - base.transform.position));
		}
		trackTarget = false;
		if (parryable > 0)
		{
			enemy.parryable = true;
			GameObject obj = Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.parryableFlash, eid.weakPoint.transform.position + eid.weakPoint.transform.forward * -0.35f, Quaternion.identity);
			obj.transform.LookAt(MonoSingleton<CameraController>.Instance.GetDefaultPos());
			obj.transform.localScale *= 3f;
			obj.transform.SetParent(eid.weakPoint.transform, worldPositionStays: true);
		}
	}

	private void SwingStart(int limb = 0)
	{
		moveForward = true;
		rb.isKinematic = false;
		sc.DamageStart();
		if (limb < trails.Length)
		{
			trails[limb].emitting = true;
		}
	}

	private void SwingEnd(int parryEnd = 0)
	{
		moveForward = false;
		if (eid.gce.onGround)
		{
			rb.isKinematic = true;
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}
		sc.DamageStop();
		TrailRenderer[] array = trails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].emitting = false;
		}
		if (parryEnd > 0)
		{
			enemy.parryable = false;
		}
	}

	private void ChargeProjectile()
	{
		if ((bool)currentChargeProjectile)
		{
			Object.Destroy(currentChargeProjectile);
		}
		if (chargingProjectile)
		{
			currentChargeProjectile = Object.Instantiate(chargeProjectile, shootPoint.position, shootPoint.rotation);
			currentChargeProjectile.transform.SetParent(shootPoint, worldPositionStays: true);
		}
	}

	private void ShootProjectile()
	{
		if ((bool)currentChargeProjectile)
		{
			Object.Destroy(currentChargeProjectile);
		}
		if (this.projectile == null || this.projectile.Equals(null))
		{
			trackTarget = false;
			chargingProjectile = false;
			return;
		}
		Projectile projectile = Object.Instantiate(this.projectile, shootPoint.position, (eid.target != null) ? Quaternion.LookRotation(shootTarget.position - shootPoint.position) : shootPoint.rotation);
		projectile.targetHandle = shootTarget.handle;
		projectile.safeEnemyType = EnemyType.Mannequin;
		if (difficulty <= 2)
		{
			projectile.turningSpeedMultiplier = 0.75f;
		}
		trackTarget = false;
		chargingProjectile = false;
	}

	public void ChangeBehavior(bool noMelee = false)
	{
		randomMovementTarget = base.transform.position;
		if (!dontChangeBehavior)
		{
			if (!noMelee && !stationary && !dontMeleeBehavior && Random.Range(0f, 1f) < 0.35f)
			{
				meleeBehaviorCancel = 3.5f;
				behavior = MannequinBehavior.Melee;
			}
			else
			{
				behavior = (MannequinBehavior)Random.Range(2, 5);
			}
		}
	}

	public void ResetMovementTarget()
	{
		randomMovementTarget = base.transform.position;
	}

	private void StopAiming()
	{
		aiming = false;
	}

	public void Landing()
	{
		enemy.parryable = false;
		if (difficulty >= 4)
		{
			inControl = true;
		}
		if (!inControl)
		{
			anim.Play("Landing");
			inAction = true;
			mostRecentAction = "Landing";
			inControl = true;
			((Behaviour)(object)nma).enabled = false;
			randomMovementTarget = base.transform.position;
		}
	}

	public void StopAction()
	{
		StopAction(true);
	}

	public void StopAction(bool changeBehavior = true)
	{
		if (clinging)
		{
			if (!stationary && !dontAutoDrop && attacksWhileClinging >= ((Random.Range(0f, 1f) > 0.5f) ? 2 : 4))
			{
				attacksWhileClinging = 0;
				inControl = true;
				anim.SetBool("InControl", true);
				Uncling();
			}
			if (inAction && !jumping)
			{
				bool flag = Random.Range(0f, 1f) > 0.5f;
				RelocateWhileClinging((!flag) ? ClungMannequinMovementDirection.Vertical : ClungMannequinMovementDirection.Horizontal);
			}
		}
		else
		{
			clungMovementTarget = null;
			jumping = false;
			enemy.overrideFalling = false;
		}
		trackTarget = clinging;
		aiming = false;
		inAction = false;
		enemy.parryable = false;
		moveForward = false;
		chargingProjectile = false;
		if (changeBehavior)
		{
			ChangeBehavior();
		}
	}

	public void CancelActions(bool changeBehavior = true)
	{
		if (moveForward)
		{
			SwingEnd();
		}
		StopAction(changeBehavior);
		if ((bool)currentChargeProjectile)
		{
			Object.Destroy(currentChargeProjectile);
		}
	}

	public void SetMovementTarget(Vector3 direction, float distance = -1f)
	{
		direction.y = 0f;
		if (distance == -1f)
		{
			distance = Random.Range(5f, 25f);
		}
		if (Physics.Raycast(eid.overrideCenter.position, direction, out var hitInfo, distance, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			NavMeshHit val = default(NavMeshHit);
			if (NavMesh.SamplePosition(hitInfo.point, ref val, 5f, nma.areaMask))
			{
				randomMovementTarget = ((NavMeshHit)(ref val)).position;
			}
			else if (Physics.SphereCast(hitInfo.point, 1f, Vector3.down, out hitInfo, distance, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
			{
				randomMovementTarget = hitInfo.point;
			}
		}
		else if (Physics.Raycast(eid.overrideCenter.position + direction.normalized * distance, Vector3.down, out hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			randomMovementTarget = hitInfo.point;
		}
		if ((bool)(Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh && enemy.gc.onGround)
		{
			MoveToTarget(randomMovementTarget);
		}
	}

	private void DelayedGroundCheckReenable()
	{
		enemy.gc.StopForceOff();
		if (jumping)
		{
			jumping = false;
			enemy.overrideFalling = false;
			inAction = false;
		}
	}

	private float GetRealDistance(NavMeshPath path)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)path.status == 2 || path.corners.Length <= 1)
		{
			return Vector3.Distance(base.transform.position, eid.target.GetNavPoint());
		}
		float num = 0f;
		if (path.corners.Length > 1)
		{
			for (int i = 1; i < path.corners.Length; i++)
			{
				num += Vector3.Distance(path.corners[i - 1], path.corners[i]);
			}
		}
		return num;
	}

	private Vector3 GetTargetPosition()
	{
		if (enemy.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget))
		{
			return lastDimensionalTarget;
		}
		if (((eid.target.isPlayer && !MonoSingleton<NewMovement>.Instance.gc.onGround) || (eid.target.isEnemy && (bool)eid.target.enemyIdentifier && (!eid.target.enemyIdentifier.gce || !eid.target.enemyIdentifier.gce.onGround))) && Physics.Raycast(eid.target.position, Vector3.down, out var hitInfo, 200f, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return eid.target.position;
	}
}
