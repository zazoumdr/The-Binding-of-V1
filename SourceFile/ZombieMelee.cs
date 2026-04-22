using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMelee : EnemyScript, IHitTargetCallback
{
	public bool harmless;

	public bool damaging;

	public TrailRenderer biteTrail;

	public TrailRenderer diveTrail;

	public bool track;

	public float coolDown;

	private NavMeshAgent nma;

	private Animator anim;

	private EnemyIdentifier eid;

	private LayerMask envMask;

	private Enemy mach;

	private VisionQuery nearestQuery;

	private TargetData targetData;

	private TargetHandle targetHandle;

	private float swingDistance;

	private Vector3 diveTargetPos;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	private bool musicRequested;

	private int difficulty = -1;

	private float defaultCoolDown = 0.5f;

	public GameObject swingSound;

	private Rigidbody rb;

	[HideInInspector]
	public SwingCheck2 swingCheck;

	[HideInInspector]
	public SwingCheck2 diveSwingCheck;

	[HideInInspector]
	public bool diving;

	private bool inAction;

	[SerializeField]
	private Transform modelTransform;

	private TimeSince randomJumpChanceCooldown;

	private bool aboutToDive;

	[SerializeField]
	private GameObject hitGroundParticle;

	[SerializeField]
	private GameObject pullOutParticle;

	private EnemySimplifier ensim;

	public Material originalMaterial;

	public Material biteMaterial;

	private EnemyTarget target => eid.target;

	private Vision vision => mach.vision;

	private bool hasVision => targetHandle != null;

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private void Awake()
	{
		mach = GetComponent<Enemy>();
		eid = GetComponent<EnemyIdentifier>();
		rb = GetComponent<Rigidbody>();
		nma = GetComponent<NavMeshAgent>();
		envMask = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void Start()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			defaultCoolDown = 0.25f;
			break;
		case 2:
			defaultCoolDown = 0.5f;
			break;
		case 1:
			defaultCoolDown = 0.75f;
			break;
		case 0:
			defaultCoolDown = 1f;
			break;
		}
		swingDistance = ((target != null && !target.isPlayer) ? 4f : 3f);
		if (!mach.musicRequested && !eid.IgnorePlayer)
		{
			musicRequested = true;
			mach.musicRequested = true;
			MonoSingleton<MusicManager>.Instance?.PlayBattleMusic();
		}
		ensim = GetComponentInChildren<EnemySimplifier>();
		anim = mach.anim;
		nearestQuery = new VisionQuery("Nearest", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, envMask));
		TrackTick();
	}

	private void Update()
	{
		if (diving)
		{
			base.transform.localRotation = Quaternion.identity;
			modelTransform.LookAt(base.transform.position + base.transform.forward + rb.velocity.normalized * 5f);
			modelTransform.Rotate(Vector3.right * 90f, Space.Self);
		}
		else
		{
			modelTransform.localRotation = Quaternion.identity;
			if (damaging)
			{
				rb.isKinematic = false;
				float num = 1f;
				if (difficulty >= 4)
				{
					num = 1.25f;
				}
				rb.velocity = base.transform.forward * 40f * num * anim.speed;
			}
		}
		if (coolDown >= 0f)
		{
			coolDown = Mathf.MoveTowards(coolDown, 0f, 0.4f * Time.deltaTime * eid.totalSpeedModifier);
		}
		if (target == null)
		{
			return;
		}
		UpdateTargetVision();
		if (track && hasVision)
		{
			if (difficulty > 1)
			{
				base.transform.LookAt(ToPlanePos(targetData.position));
			}
			else
			{
				float num2 = ((difficulty == 0) ? 360 : 720);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(ToPlanePos(targetData.position) - base.transform.position), Time.deltaTime * num2 * eid.totalSpeedModifier);
			}
		}
		if (!hasDimensionalTarget && coolDown <= 0f && mach.grounded && !nma.isOnOffMeshLink && !mach.isTraversingPortalLink && !aboutToDive && !inAction && !damaging)
		{
			if (Vector3.Distance(hasVision ? targetData.position : target.position, base.transform.position) < swingDistance)
			{
				Swing();
			}
			else if (difficulty >= 4)
			{
				DiveCheck();
			}
		}
	}

	private void UpdateTargetVision()
	{
		if (vision.TrySee(nearestQuery, out var data))
		{
			targetData = data.ToData();
			targetHandle = targetData.handle;
		}
	}

	private void DiveCheck()
	{
		if (!hasVision || targetData.DistanceTo(base.transform.position) > 20f)
		{
			return;
		}
		diveTargetPos = targetData.position;
		if (diveTargetPos.y > base.transform.position.y + 5f)
		{
			aboutToDive = true;
			Invoke("JumpAttack", Random.Range(0f, 0.5f));
		}
		else if (targetData.DistanceTo(base.transform.position) > 10f && (float)randomJumpChanceCooldown > 1f)
		{
			randomJumpChanceCooldown = 0f;
			if (Random.Range(0f, 1f) > 0.8f)
			{
				JumpAttack();
			}
		}
	}

	private void OnEnable()
	{
		if (!mach.musicRequested && !eid.IgnorePlayer)
		{
			musicRequested = true;
			mach.musicRequested = true;
			if (MonoSingleton<MusicManager>.TryGetInstance(out MusicManager instance))
			{
				instance.PlayBattleMusic();
			}
		}
		CancelAttack();
		if (mach.grounded && (bool)rb)
		{
			rb.isKinematic = true;
			rb.velocity = Vector3.zero;
		}
	}

	private void OnDisable()
	{
		if (musicRequested && !eid.IgnorePlayer && !mach.limp)
		{
			musicRequested = false;
			mach.musicRequested = false;
			if (MonoSingleton<MusicManager>.TryGetInstance(out MusicManager instance))
			{
				instance.PlayCleanMusic();
			}
		}
	}

	private void FixedUpdate()
	{
		if (mach.grounded && !damaging && !rb.isKinematic && !mach.knockedBack)
		{
			rb.velocity = Vector3.zero;
			rb.isKinematic = true;
		}
		if (mach.grounded && !((Object)(object)nma == null) && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			bool flag = !nma.isStopped && nma.velocity.magnitude > 0f;
			anim.SetBool("Running", flag || mach.isTraversingPortalLink);
		}
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return difficulty switch
		{
			0 => new EnemyMovementData
			{
				acceleration = 15f,
				angularSpeed = 400f,
				speed = 10f
			}, 
			1 => new EnemyMovementData
			{
				acceleration = 30f,
				angularSpeed = 400f,
				speed = 15f
			}, 
			2 => new EnemyMovementData
			{
				acceleration = 30f,
				angularSpeed = 800f,
				speed = 20f
			}, 
			3 => new EnemyMovementData
			{
				acceleration = 60f,
				angularSpeed = 2600f,
				speed = 20f
			}, 
			_ => new EnemyMovementData
			{
				acceleration = 30f,
				angularSpeed = 400f,
				speed = 20f
			}, 
		};
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		track = false;
		if (!mach.chestExploding)
		{
			mach.anim.StopPlayback();
		}
		if (biteTrail != null)
		{
			biteTrail.enabled = false;
		}
		if (diveTrail != null)
		{
			diveTrail.enabled = false;
		}
		Object.Destroy(swingCheck.gameObject);
		Object.Destroy(diveSwingCheck.gameObject);
		Object.Destroy(this);
	}

	public override void OnLand()
	{
		if (diving)
		{
			JumpEnd();
		}
	}

	public override void OnFall()
	{
		if (!diving)
		{
			CancelAttack();
		}
	}

	public override void OnDamage(ref DamageData data)
	{
		if (diving)
		{
			CancelAttack();
		}
	}

	public void JumpAttack()
	{
		aboutToDive = false;
		if (!nma.isOnOffMeshLink)
		{
			anim.Play("JumpStart");
			coolDown = defaultCoolDown;
			inAction = true;
			mach.stopped = true;
			((Behaviour)(object)nma).enabled = false;
		}
	}

	public void JumpStart()
	{
		base.transform.LookAt(ToPlanePos(diveTargetPos));
		mach.Jump(Vector3.up * 25f + Vector3.ClampMagnitude(new Vector3((diveTargetPos.x - base.transform.position.x) * 2f, 0f, (diveTargetPos.z - base.transform.position.z) * 2f), 25f));
		Object.Instantiate(swingSound, base.transform);
		diving = true;
		DamageStart();
		mach.ParryableCheck();
		Invoke("CheckThatJumpStarted", 1f);
	}

	private void CheckThatJumpStarted()
	{
		if (diving && !mach.falling)
		{
			JumpEnd();
		}
	}

	public void JumpEnd()
	{
		CancelInvoke("CheckThatJumpStarted");
		anim.Play("JumpEnd");
		diving = false;
		DamageEnd();
		Object.Instantiate(hitGroundParticle, base.transform.position, Quaternion.identity);
	}

	public void PullOut()
	{
		Object.Instantiate(pullOutParticle, base.transform.position, Quaternion.identity);
	}

	public void JumpEndEnd()
	{
		inAction = false;
	}

	public void Swing()
	{
		if (!damaging && !harmless && target != null)
		{
			GetComponentInChildren<SwingCheck2>().OverrideEnemyIdentifier(eid);
			mach.stopped = true;
			track = true;
			coolDown = defaultCoolDown;
			((Behaviour)(object)nma).enabled = false;
			inAction = true;
			anim.SetTrigger("Swing");
			Object.Instantiate(swingSound, base.transform);
		}
	}

	public void SwingEnd()
	{
		inAction = false;
		if (mach.grounded)
		{
			((Behaviour)(object)nma).enabled = true;
		}
		mach.stopped = false;
	}

	public void DamageStart()
	{
		if (!harmless)
		{
			damaging = true;
			if (diving)
			{
				diveTrail.emitting = true;
				diveSwingCheck.DamageStart();
			}
			else
			{
				biteTrail.enabled = true;
				swingCheck.DamageStart();
				MouthClose();
			}
		}
	}

	public void TargetBeenHit()
	{
		MouthClose();
	}

	public void DamageEnd()
	{
		damaging = false;
		mach.parryable = false;
		biteTrail.enabled = false;
		diveTrail.emitting = false;
		swingCheck.DamageStop();
		diveSwingCheck.DamageStop();
	}

	public void StopTracking()
	{
		track = false;
		if (difficulty >= 4 && target != null && target.isPlayer)
		{
			Vector3 pos = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.2f);
			base.transform.LookAt(ToPlanePos(pos));
		}
		mach.ParryableCheck();
	}

	public void CancelAttack()
	{
		damaging = false;
		mach.parryable = false;
		inAction = false;
		biteTrail.enabled = false;
		diveTrail.emitting = false;
		diving = false;
		mach.stopped = false;
		track = false;
		coolDown = defaultCoolDown;
		swingCheck.DamageStop();
	}

	public void TrackTick()
	{
		Invoke("TrackTick", GetUpdateRate(nma));
		if (base.gameObject.activeInHierarchy && target != null && !((Object)(object)nma == null) && eid.enabled && mach.grounded && !mach.falling && !mach.knockedBack && !diving && !inAction && !mach.isOnOffNavmeshLink && !mach.isTraversingPortalLink)
		{
			if (!mach.TryGetDimensionalTarget(target.position, out lastDimensionalTarget))
			{
				lastDimensionalTarget = Vector3.zero;
			}
			((Behaviour)(object)nma).enabled = true;
			Vector3 destination = (hasDimensionalTarget ? EnemyTarget.GetNavPoint(lastDimensionalTarget) : target.GetNavPoint());
			mach.SetDestination(destination);
		}
	}

	public void MouthClose()
	{
		if (!eid.puppet)
		{
			if ((bool)ensim)
			{
				ensim.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, biteMaterial);
			}
			CancelInvoke("MouthOpen");
			Invoke("MouthOpen", 0.75f);
		}
	}

	private void MouthOpen()
	{
		if (!eid.puppet && (bool)ensim)
		{
			ensim.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalMaterial);
		}
	}
}
