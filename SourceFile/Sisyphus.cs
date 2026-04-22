using System;
using System.Collections;
using System.Collections.Generic;
using NewBlood.IK;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Solver3D))]
public class Sisyphus : EnemyScript
{
	private enum AttackType
	{
		OverheadSlam,
		HorizontalSwing,
		Stab,
		AirStab
	}

	private static readonly int s_SwingAnimSpeed = Animator.StringToHash("SwingSpeed");

	private float swingArmSpeed;

	private static readonly int s_OverheadSlam = Animator.StringToHash("OverheadSlam");

	private static readonly int s_HorizontalSwing = Animator.StringToHash("HorizontalSwing");

	private static readonly int s_Stab = Animator.StringToHash("Stab");

	private static readonly int s_AirStab = Animator.StringToHash("AirStab");

	private static readonly int s_AirStabCancel = Animator.StringToHash("AirStabCancel");

	private static readonly int s_Stomp = Animator.StringToHash("Stomp");

	[SerializeField]
	private Solver3D m_Solver;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private Transform m_Boulder;

	[SerializeField]
	private Collider boulderCol;

	[SerializeField]
	private PhysicalShockwave m_ShockwavePrefab;

	[SerializeField]
	private GameObject explosion;

	private Pose m_StartPose;

	private AttackType m_AttackType;

	private float[] m_NormalizedDistances;

	private Transform[] m_Transforms;

	private bool didCollide;

	private bool airStabCancelled;

	private bool pullSelfRetract;

	private bool swinging;

	private bool inAction;

	private float stuckInActionTimer;

	private int attacksPerformed;

	private int previousAttack = -1;

	private bool previouslyJumped;

	private float cooldown;

	private NavMeshAgent nma;

	private SwingCheck2 sc;

	private float airStabOvershoot = 2f;

	private float stabOvershoot = 1.1f;

	private GroundCheckEnemy gce;

	private Rigidbody rb;

	private bool jumping;

	private Vector3 jumpTarget;

	private bool superJumping;

	private float trackingX;

	private float trackingY;

	private bool forceCorrectOrientation;

	private Collider col;

	[SerializeField]
	private GameObject rubble;

	[SerializeField]
	private TrailRenderer trail;

	[SerializeField]
	private ParticleSystem swingParticle;

	[SerializeField]
	private AudioSource swingAudio;

	public bool stationary;

	private AudioSource aud;

	[SerializeField]
	private AudioClip[] attackVoices;

	[SerializeField]
	private AudioClip stompVoice;

	[SerializeField]
	private AudioClip deathVoice;

	[SerializeField]
	private GameObject[] hurtSounds;

	private GameObject currentHurtSound;

	[SerializeField]
	private Transform[] legs;

	[SerializeField]
	private Transform armature;

	private int difficulty = -1;

	[SerializeField]
	private GameObject attackFlash;

	private float stuckChecker;

	private EnemyIdentifier eid;

	private GoreZone gz;

	private Enemy mach;

	private Coroutine co;

	[SerializeField]
	private Cannonball boulderCb;

	private bool isParried;

	[SerializeField]
	private Transform originalBoulder;

	[HideInInspector]
	public bool knockedDownByCannonball;

	[SerializeField]
	private GameObject fallSound;

	private List<EnemyIdentifier> fallEnemiesHit = new List<EnemyIdentifier>();

	[Header("Animations")]
	[SerializeField]
	private SisyAttackAnimationDetails overheadSlamAnim;

	[SerializeField]
	private SisyAttackAnimationDetails horizontalSwingAnim;

	[SerializeField]
	private SisyAttackAnimationDetails groundStabAnim;

	[SerializeField]
	private SisyAttackAnimationDetails airStabAnim;

	[HideInInspector]
	public bool downed;

	public bool jumpOnSpawn;

	private bool dontFacePlayer;

	private float superKnockdownWindow;

	private LayerMask lmask;

	private VisionQuery visionQuery;

	private TargetData lastTargetData = TargetData.None;

	private TargetHandle targetHandle;

	private Vector3 lastDimensionalTarget = Vector3.zero;

	public Vector3 cachedVisionPos;

	private Vision vision => mach.vision;

	private bool hasVision => targetHandle != null;

	private bool isVisionThroughPortal
	{
		get
		{
			if (hasVision)
			{
				return targetHandle.portals.Count > 0;
			}
			return false;
		}
	}

	private bool hasDimensionalTarget => lastDimensionalTarget != Vector3.zero;

	private Vector3 CurrentTargetPosition
	{
		get
		{
			if (!hasDimensionalTarget)
			{
				if (!hasVision)
				{
					if (eid.target == null)
					{
						return base.transform.position;
					}
					return eid.target.position;
				}
				return lastTargetData.position;
			}
			return lastDimensionalTarget;
		}
	}

	public override Vector3 VisionSourcePosition => cachedVisionPos;

	private SisyAttackAnimationDetails GetAnimationDetails(AttackType type)
	{
		return type switch
		{
			AttackType.OverheadSlam => overheadSlamAnim, 
			AttackType.HorizontalSwing => horizontalSwingAnim, 
			AttackType.Stab => groundStabAnim, 
			AttackType.AirStab => airStabAnim, 
			_ => null, 
		};
	}

	private void Awake()
	{
		nma = GetComponent<NavMeshAgent>();
		sc = GetComponentInChildren<SwingCheck2>();
		rb = GetComponent<Rigidbody>();
		gce = GetComponentInChildren<GroundCheckEnemy>();
		col = GetComponent<Collider>();
		aud = GetComponent<AudioSource>();
		mach = GetComponent<Enemy>();
		lmask = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void Start()
	{
		eid = GetComponent<EnemyIdentifier>();
		if (stationary)
		{
			eid.stationary = true;
		}
		else if (eid.stationary)
		{
			stationary = true;
		}
		visionQuery = new VisionQuery("SisyphusVision", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, lmask));
		m_Solver.Initialize();
		IKChain3D chain = m_Solver.GetChain(0);
		m_Transforms = new Transform[chain.transformCount];
		m_Transforms[m_Transforms.Length - 1] = chain.effector;
		for (int num = m_Transforms.Length - 2; num >= 0; num--)
		{
			m_Transforms[num] = m_Transforms[num + 1].parent;
		}
		float num2 = 0f;
		m_NormalizedDistances = new float[m_Transforms.Length - 1];
		for (int num3 = 0; num3 < m_NormalizedDistances.Length; num3++)
		{
			m_NormalizedDistances[num3] = Vector3.Distance(m_Transforms[num3].position, m_Transforms[num3 + 1].position);
			num2 += m_NormalizedDistances[num3];
		}
		for (int num4 = 0; num4 < m_NormalizedDistances.Length; num4++)
		{
			m_NormalizedDistances[num4] /= num2;
		}
		m_StartPose = new Pose(m_Boulder.localPosition, m_Boulder.localRotation);
		if (difficulty < 4)
		{
			cooldown = 3f;
		}
		else
		{
			cooldown = 1f;
		}
		if ((bool)(UnityEngine.Object)(object)nma)
		{
			((Behaviour)(object)nma).enabled = true;
		}
		gz = GoreZone.ResolveGoreZone(base.transform);
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
		Physics.IgnoreCollision(col, boulderCol);
		SetSpeed();
		boulderCb.sisy = this;
		if (jumpOnSpawn)
		{
			if (eid.target == null)
			{
				eid.UpdateTarget();
			}
			if (eid.target != null)
			{
				Jump(eid.target.position);
			}
		}
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			if (eid.difficultyOverride >= 0)
			{
				difficulty = eid.difficultyOverride;
			}
			else
			{
				difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
		}
		anim.SetFloat("DownedSpeed", (float)((difficulty < 4) ? 1 : 2));
		if (difficulty <= 1)
		{
			anim.SetFloat("StompSpeed", 0.75f * eid.totalSpeedModifier);
		}
		else if (difficulty == 2)
		{
			anim.SetFloat("StompSpeed", 0.875f * eid.totalSpeedModifier);
		}
		else
		{
			anim.SetFloat("StompSpeed", 1f * eid.totalSpeedModifier);
		}
	}

	private void OnEnable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Combine(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
		SetSpeed();
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
	}

	private void OnDisable()
	{
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalManagerV2 portalManagerV = instance;
			portalManagerV.OnTargetTravelled = (Action<IPortalTraveller, PortalTravelDetails>)Delegate.Remove(portalManagerV.OnTargetTravelled, new Action<IPortalTraveller, PortalTravelDetails>(OnTargetTravelled));
		}
		if (co != null)
		{
			StopCoroutine(co);
		}
		StopAction();
		ResetBoulderPose();
		SwingStop();
		if (eid.target != null)
		{
			swingArmSpeed = Mathf.Max(0.01f, Vector3.Distance(base.transform.position, eid.target.position) / 100f) * eid.totalSpeedModifier;
		}
		if ((bool)gce && !gce.onGround)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
		}
	}

	public override void OnTeleport(PortalTravelDetails details)
	{
		targetHandle?.From(details.portalSequence.Reversed());
	}

	private void OnTargetTravelled(IPortalTraveller traveller, PortalTravelDetails details)
	{
		if (targetHandle != null && traveller.id == targetHandle.id)
		{
			targetHandle = targetHandle.Then(details.portalSequence);
		}
	}

	private void VisionUpdate()
	{
		if (vision != null)
		{
			_ = base.transform.position;
			vision.sourcePos = VisionSourcePosition;
			if (vision.TrySee(visionQuery, out var data))
			{
				lastTargetData = data.ToData();
				targetHandle = lastTargetData.handle;
			}
			else
			{
				targetHandle = null;
			}
		}
	}

	private void LateUpdate()
	{
		ChangeArmLength(Vector3.Distance(m_Transforms[0].position, m_Boulder.position));
		m_Solver.UpdateIK(1f);
		m_Transforms[m_Transforms.Length - 1].position = m_Boulder.position;
		if (!isParried)
		{
			m_Boulder.rotation = originalBoulder.rotation;
			m_Boulder.Rotate(Vector3.right * -90f, Space.Self);
			m_Boulder.Rotate(Vector3.up * -5f, Space.Self);
		}
		else
		{
			originalBoulder.transform.up = m_Boulder.transform.forward;
		}
	}

	private void ChangeArmLength(float targetLength)
	{
		for (int i = 0; i < m_NormalizedDistances.Length; i++)
		{
			Vector3 vector = Vector3.Normalize(m_Transforms[i + 1].position - m_Transforms[i].position);
			float num = targetLength * m_NormalizedDistances[i];
			m_Transforms[i + 1].position = m_Transforms[i].position + vector * num;
		}
	}

	private void FixedUpdate()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (eid.target != null)
		{
			lastDimensionalTarget = Vector3.zero;
			if (!hasVision)
			{
				mach.TryGetDimensionalTarget(eid.target.position, out lastDimensionalTarget);
			}
		}
		if (eid.target != null)
		{
			if (!inAction)
			{
				goto IL_00be;
			}
			AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Walking"))
			{
				currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
				if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Idle"))
				{
					goto IL_00be;
				}
			}
			stuckInActionTimer = Mathf.MoveTowards(stuckInActionTimer, 2f, Time.fixedDeltaTime);
			if (stuckInActionTimer == 2f)
			{
				inAction = false;
			}
		}
		else
		{
			anim.SetBool("Walking", false);
			if (((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
			{
				nma.SetDestination(base.transform.position);
			}
		}
		goto IL_010d;
		IL_010d:
		if (gce.onGround && !nma.isOnNavMesh && !nma.isOnOffMeshLink && eid.target != null)
		{
			if (gce.onGround && !nma.isOnNavMesh && !nma.isOnOffMeshLink && !inAction)
			{
				stuckChecker = Mathf.MoveTowards(stuckChecker, 3f, Time.fixedDeltaTime);
				if (stuckChecker >= 3f && !jumping)
				{
					stuckChecker = 2f;
					superJumping = true;
					Jump(CurrentTargetPosition);
				}
			}
			else
			{
				stuckChecker = 0f;
			}
		}
		if (gce.onGround && !superJumping && !inAction && rb.useGravity && !rb.isKinematic)
		{
			((Behaviour)(object)nma).enabled = true;
			rb.isKinematic = true;
			rb.useGravity = false;
			jumping = false;
			inAction = true;
			if (superKnockdownWindow > 0f)
			{
				downed = true;
				Knockdown(base.transform.position + base.transform.forward);
				MonoSingleton<StyleHUD>.Instance.AddPoints(60, "ultrakill.insurrknockdown", null, eid);
				Invoke("Undown", 4f);
			}
			else
			{
				anim.Play("Landing");
				if (difficulty >= 1)
				{
					RaycastHit[] array = Physics.RaycastAll(base.transform.position + Vector3.up * 4f, Vector3.down, 6f, LayerMaskDefaults.Get(LMD.Environment));
					PhysicalShockwave physicalShockwave = null;
					if (array.Length != 0)
					{
						bool flag = false;
						RaycastHit[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							RaycastHit raycastHit = array2[i];
							if (raycastHit.collider != boulderCol)
							{
								physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, raycastHit.point, Quaternion.identity);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, base.transform.position, Quaternion.identity);
						}
					}
					else
					{
						physicalShockwave = UnityEngine.Object.Instantiate(m_ShockwavePrefab, base.transform.position, Quaternion.identity);
					}
					if ((bool)physicalShockwave)
					{
						physicalShockwave.transform.SetParent(gz.transform);
						physicalShockwave.speed *= eid.totalSpeedModifier;
						physicalShockwave.damage = Mathf.RoundToInt((float)physicalShockwave.damage * eid.totalDamageModifier);
					}
				}
			}
			if (fallEnemiesHit.Count > 0)
			{
				foreach (EnemyIdentifier item in fallEnemiesHit)
				{
					if (item != null && !item.dead && item.TryGetComponent<Collider>(out var component))
					{
						Physics.IgnoreCollision(col, component, ignore: false);
					}
				}
				fallEnemiesHit.Clear();
			}
		}
		else if (!gce.onGround && rb.useGravity && !rb.isKinematic)
		{
			RaycastHit[] array2 = Physics.SphereCastAll(col.bounds.center, 2.5f, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime + 6f, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment));
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit2 = array2[i];
				EnemyIdentifierIdentifier component4;
				RaycastHit hitInfo;
				if (LayerMaskDefaults.IsMatchingLayer(raycastHit2.transform.gameObject.layer, LMD.Environment))
				{
					Glass component3;
					if (raycastHit2.transform.TryGetComponent<Breakable>(out var component2) && !component2.playerOnly && !component2.specialCaseOnly && !component2.precisionOnly)
					{
						component2.Break();
					}
					else if (raycastHit2.transform.TryGetComponent<Glass>(out component3))
					{
						component3.Shatter();
					}
				}
				else if (raycastHit2.transform.TryGetComponent<EnemyIdentifierIdentifier>(out component4) && (bool)component4.eid && component4.eid != eid && !fallEnemiesHit.Contains(component4.eid) && !Physics.Linecast(eid.GetCenter().position, component4.eid.GetCenter().position, out hitInfo, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					FallKillEnemy(component4.eid);
				}
			}
			array2 = Physics.SphereCastAll(col.bounds.center, 2.5f, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime + 6f, 4096);
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit3 = array2[i];
				if (raycastHit3.transform != base.transform && raycastHit3.transform.TryGetComponent<EnemyIdentifier>(out var component5) && !fallEnemiesHit.Contains(component5) && !Physics.Linecast(eid.GetCenter().position, component5.GetCenter().position, out var _, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
				{
					FallKillEnemy(component5);
				}
			}
		}
		if (!inAction && gce.onGround && !jumping && eid.target != null)
		{
			if (cooldown > 0f)
			{
				forceCorrectOrientation = false;
				if (GetDistanceToTarget() < 10f)
				{
					cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * 3f * eid.totalSpeedModifier);
				}
				else
				{
					cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
				}
				if (!stationary)
				{
					if (((Behaviour)(object)nma).enabled && nma.isOnNavMesh && Physics.Raycast(CurrentTargetPosition, Vector3.down, out var hitInfo3, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
					{
						NavMeshHit val = default(NavMeshHit);
						if (NavMesh.SamplePosition(CurrentTargetPosition, ref val, 1f, nma.areaMask))
						{
							nma.SetDestination(((NavMeshHit)(ref val)).position);
						}
						else
						{
							nma.SetDestination(hitInfo3.point);
						}
					}
					else if (((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
					{
						nma.SetDestination(CurrentTargetPosition);
					}
					if (nma.velocity.magnitude < 1f)
					{
						anim.SetBool("Walking", false);
					}
					else
					{
						anim.SetBool("Walking", true);
					}
				}
			}
			else if (GetDistanceToTarget() < 8f && difficulty != 0)
			{
				inAction = true;
				aud.SetPitch(UnityEngine.Random.Range(1.4f, 1.6f));
				aud.PlayOneShot(stompVoice, tracked: true);
				anim.SetTrigger(s_Stomp);
			}
			else
			{
				if ((attacksPerformed >= UnityEngine.Random.Range(2, 4) || GetDistanceToTarget() > 100f) && Physics.Raycast(CurrentTargetPosition, Vector3.down, 50f, LayerMaskDefaults.Get(LMD.Environment)))
				{
					Jump(CurrentTargetPosition);
					attacksPerformed = 0;
					return;
				}
				int num = UnityEngine.Random.Range(0, 4);
				bool flag2 = false;
				int num2 = 0;
				while ((num == previousAttack || (num == 3 && previouslyJumped)) && num2 < 10)
				{
					num2++;
					num = UnityEngine.Random.Range(0, 4);
					if (num2 == 10)
					{
						Debug.LogError("While method in Sisyphus' attack choosing function hit the failsafe", this);
					}
				}
				if (TestAttack(num))
				{
					flag2 = true;
				}
				else
				{
					int[] array3 = new int[4] { 0, 1, 2, 3 };
					int num3 = 4;
					if (previouslyJumped)
					{
						num3 = 3;
					}
					for (int j = 0; j < num3; j++)
					{
						int num4 = array3[j];
						int num5 = UnityEngine.Random.Range(j, num3);
						array3[j] = array3[num5];
						array3[num5] = num4;
					}
					for (int k = 0; k < 4; k++)
					{
						if (array3[k] != num && TestAttack(array3[k]))
						{
							flag2 = true;
							num = array3[k];
							break;
						}
					}
				}
				forceCorrectOrientation = false;
				if (flag2)
				{
					if (!stationary && nma.isOnNavMesh)
					{
						nma.SetDestination(base.transform.position);
					}
					inAction = true;
					cooldown = 3 - difficulty / 2;
					previousAttack = num;
					previouslyJumped = false;
					switch (num)
					{
					case 0:
						m_AttackType = AttackType.OverheadSlam;
						base.transform.LookAt(new Vector3(CurrentTargetPosition.x, base.transform.position.y, CurrentTargetPosition.z));
						anim.SetTrigger(s_OverheadSlam);
						trackingX = 1f;
						trackingY = 0.15f;
						break;
					case 1:
						m_AttackType = AttackType.HorizontalSwing;
						anim.SetTrigger(s_HorizontalSwing);
						trackingX = 0f;
						trackingY = 1f;
						break;
					case 2:
						m_AttackType = AttackType.Stab;
						anim.SetTrigger(s_Stab);
						trackingX = 0.9f;
						trackingY = 0.5f;
						break;
					case 3:
						m_AttackType = AttackType.AirStab;
						StartCoroutine(AirStab());
						Jump(noEnd: true);
						trackingX = 0f;
						trackingY = 0.9f;
						break;
					}
					if (num < attackVoices.Length && num != 3)
					{
						if (num == 1)
						{
							aud.SetPitch(UnityEngine.Random.Range(1.4f, 1.6f));
						}
						else
						{
							aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
						}
						aud.PlayOneShot(attackVoices[num], tracked: true);
					}
					if (num != 3)
					{
						UnityEngine.Object.Instantiate(attackFlash, m_Boulder);
					}
					attacksPerformed++;
				}
				else
				{
					Jump(CurrentTargetPosition);
				}
			}
		}
		else if (inAction)
		{
			if (!gce.onGround)
			{
				rb.useGravity = false;
			}
			if (!dontFacePlayer)
			{
				RotateTowardsTarget();
			}
		}
		else if (jumping)
		{
			RotateTowardsTarget();
		}
		if (jumping)
		{
			Vector3 vector = new Vector3(jumpTarget.x, base.transform.position.y, jumpTarget.z);
			if (!Physics.Raycast(col.bounds.center, vector - base.transform.position, Vector3.Distance(base.transform.position, vector) * Time.fixedDeltaTime * 2f + 2f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				base.transform.position = Vector3.MoveTowards(base.transform.position, vector, Vector3.Distance(base.transform.position, vector) * Time.fixedDeltaTime * 2f);
			}
			if (superJumping)
			{
				RaycastHit hitInfo4;
				bool num6 = Physics.SphereCast(base.transform.position, 3f, rb.velocity.normalized, out hitInfo4, rb.velocity.magnitude * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment));
				if (!num6 && didCollide && Physics.SphereCast(base.transform.position, 3f, -rb.velocity.normalized, out hitInfo4, rb.velocity.magnitude * Time.fixedDeltaTime, LayerMaskDefaults.Get(LMD.Environment)))
				{
					UnityEngine.Object.Instantiate(rubble, hitInfo4.point, Quaternion.LookRotation(hitInfo4.normal));
					didCollide = false;
				}
				if (num6 && !didCollide)
				{
					didCollide = true;
					if (Vector3.Distance(base.transform.position + rb.velocity * Time.fixedDeltaTime, jumpTarget) > 3f)
					{
						UnityEngine.Object.Instantiate(rubble, hitInfo4.point, Quaternion.LookRotation(hitInfo4.normal));
					}
				}
				if (rb.velocity.y >= 0f)
				{
					col.isTrigger = Vector3.Distance(base.transform.position, jumpTarget) > 1f;
				}
				else if (jumpTarget.y + 8f > base.transform.position.y + Mathf.Abs(rb.velocity.y) * Time.fixedDeltaTime)
				{
					col.isTrigger = false;
					superJumping = false;
					base.transform.position = vector;
				}
			}
		}
		if (!inAction && !gce.onGround)
		{
			rb.useGravity = true;
		}
		if (!rb.isKinematic && rb.useGravity)
		{
			rb.velocity -= Vector3.up * 200f * Time.fixedDeltaTime;
		}
		if (!jumping && !rb.isKinematic && !inAction)
		{
			anim.Play("Jump", -1, 0.95f);
		}
		else if (gce.onGround && !inAction && !superJumping)
		{
			superJumping = false;
			jumping = false;
		}
		return;
		IL_00be:
		stuckInActionTimer = 0f;
		goto IL_010d;
	}

	private void Update()
	{
		if (superKnockdownWindow > 0f)
		{
			superKnockdownWindow = Mathf.MoveTowards(superKnockdownWindow, 0f, Time.deltaTime);
		}
		cachedVisionPos = (col ? col.bounds.center : (base.transform.position + Vector3.up * 3f));
		if (eid.target != null)
		{
			VisionUpdate();
		}
	}

	private float GetDistanceToTarget()
	{
		return Vector3.Distance(base.transform.position, CurrentTargetPosition);
	}

	private bool TestAttack(int attack)
	{
		float distanceToTarget = GetDistanceToTarget();
		Vector3 currentTargetPosition = CurrentTargetPosition;
		switch (attack)
		{
		case 0:
			if (!Physics.Raycast(base.transform.position, Vector3.up, distanceToTarget, lmask) && !Physics.Raycast(base.transform.position + Vector3.up * distanceToTarget, currentTargetPosition - base.transform.position, distanceToTarget, lmask) && !Physics.Raycast(currentTargetPosition, Vector3.up, distanceToTarget, lmask))
			{
				return true;
			}
			return false;
		case 1:
		{
			Vector3 position = base.transform.position;
			float num = Vector3.Distance(currentTargetPosition, position);
			float num2 = currentTargetPosition.y - position.y;
			Vector3 vector2 = position + base.transform.up * 5f + base.transform.right * (0f - num);
			Vector3 vector3 = position + base.transform.up * 5f + Vector3.up * num2 * 2f + base.transform.right * num;
			if (!Physics.Raycast(base.transform.position + Vector3.up * 3f, -base.transform.right, distanceToTarget, lmask) && !Physics.Raycast(vector2, currentTargetPosition - vector2, Vector3.Distance(vector2, currentTargetPosition), lmask) && !Physics.Raycast(base.transform.position + Vector3.up * 3f, base.transform.right, distanceToTarget, lmask) && !Physics.Raycast(vector3, currentTargetPosition - vector3, Vector3.Distance(vector3, currentTargetPosition), lmask))
			{
				return true;
			}
			return false;
		}
		case 2:
		{
			Vector3 vector4 = currentTargetPosition + Vector3.up * 3f;
			RaycastHit hitInfo;
			return !Physics.SphereCast(base.transform.position + Vector3.up * 3f, 1.75f, Quaternion.LookRotation(vector4 - base.transform.position, Vector3.up).eulerAngles, out hitInfo, Vector3.Distance(base.transform.position, currentTargetPosition), lmask);
		}
		case 3:
		{
			Vector3 vector = base.transform.position + Vector3.up * 73f;
			Vector3 direction = Vector3.Normalize(currentTargetPosition - vector);
			if (!Physics.Raycast(base.transform.position + Vector3.up * 3f, base.transform.up, 70f, lmask))
			{
				return !Physics.Raycast(vector, direction, Vector3.Distance(vector, currentTargetPosition), lmask);
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool CanFit(Vector3 point)
	{
		if (Physics.Raycast(point, Vector3.up, 11f, LayerMaskDefaults.Get(LMD.Environment)))
		{
			return false;
		}
		return true;
	}

	private IEnumerator AirStab()
	{
		superJumping = false;
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Instantiate(attackFlash, m_Boulder).transform.localScale *= 5f;
		aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
		aud.PlayOneShot(attackVoices[3], tracked: true);
		trackingX = 0.9f;
		trackingY = 0.9f;
		rb.isKinematic = true;
		anim.SetTrigger(s_AirStab);
	}

	private IEnumerator AirStabAttack(float time)
	{
		if (eid.target == null)
		{
			yield break;
		}
		airStabCancelled = true;
		rb.isKinematic = true;
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.AirStab).finalDurationMulti;
		Vector3 currentTargetPosition = CurrentTargetPosition;
		Vector3 attackTarget = base.transform.position + (base.transform.forward * Vector3.Distance(base.transform.position, currentTargetPosition) + base.transform.right * 3f) * airStabOvershoot;
		sc.DamageStart();
		while (swinging)
		{
			Vector3 vector = Vector3.LerpUnclamped(start, attackTarget, t / time);
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (Physics.OverlapSphere(m_Boulder.position, 3.75f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				SlamShockwave();
				SwingStop();
				swinging = false;
				trackingX = 0f;
				trackingY = 0f;
			}
		}
		trackingX = 0.75f;
		trackingY = 0f;
		SwingStop();
	}

	public void ExtendArm(float time)
	{
		SisyAttackAnimationDetails animationDetails = GetAnimationDetails(m_AttackType);
		boulderCol.enabled = false;
		trail.emitting = true;
		swingParticle.Play();
		swinging = true;
		swingAudio.Play(tracked: true);
		boulderCb.launchable = true;
		float num = Vector3.Distance(base.transform.position, GetActualTargetPos());
		if (num < 10f)
		{
			num = 10f;
		}
		num -= 10f;
		swingArmSpeed = Mathf.Clamp(num / animationDetails.boulderDistanceDivide, animationDetails.minBoulderSpeed, animationDetails.maxBoulderSpeed);
		if (m_AttackType == AttackType.AirStab)
		{
			num *= 0.35f;
			swingArmSpeed /= airStabOvershoot;
		}
		float num2 = 1f - num / animationDetails.boulderDistanceDivide;
		num2 *= animationDetails.speedDistanceMulti;
		num2 = Mathf.Clamp(num2, animationDetails.minAnimSpeedCap, animationDetails.maxAnimSpeedCap);
		_ = m_AttackType;
		_ = 2;
		float num3 = 1f;
		if (difficulty >= 4)
		{
			num3 = 1.5f;
		}
		else if (difficulty == 3)
		{
			num3 = 1.25f;
		}
		else if (difficulty == 1)
		{
			num3 = 0.75f;
		}
		else if (difficulty == 0)
		{
			num3 = 0.5f;
		}
		num3 *= eid.totalSpeedModifier;
		num2 *= num3;
		swingArmSpeed /= num3;
		anim.SetFloat(s_SwingAnimSpeed, num2);
		if (m_AttackType == AttackType.OverheadSlam)
		{
			co = StartCoroutine(OverheadSlamAttack(time));
		}
		else if (m_AttackType == AttackType.HorizontalSwing)
		{
			co = StartCoroutine(HorizontalSwingAttack(time));
		}
		else if (m_AttackType == AttackType.Stab)
		{
			co = StartCoroutine(StabAttack(time));
		}
		else if (m_AttackType == AttackType.AirStab)
		{
			co = StartCoroutine(AirStabAttack(time));
		}
	}

	public void RetractArm(float time)
	{
		inAction = false;
		anim.SetFloat(s_SwingAnimSpeed, 1f * eid.totalSpeedModifier);
		if (eid.target != null)
		{
			swingArmSpeed = Mathf.Max(0.01f, GetDistanceToTarget() / 100f);
		}
		TryToRetractArm(time);
	}

	private Vector3 GetActualTargetPos()
	{
		if (eid.target == null)
		{
			return base.transform.position;
		}
		Vector3 currentTargetPosition = CurrentTargetPosition;
		switch (m_AttackType)
		{
		case AttackType.OverheadSlam:
		{
			Vector3 position = base.transform.position;
			position.y = currentTargetPosition.y;
			return position + base.transform.forward * (Vector3.Distance(position, currentTargetPosition) - 0.5f) - base.transform.forward;
		}
		case AttackType.HorizontalSwing:
		{
			Vector3 result = currentTargetPosition - base.transform.forward * 3f;
			result.y = currentTargetPosition.y;
			return result;
		}
		case AttackType.AirStab:
			return currentTargetPosition + base.transform.right * 10f;
		default:
			return currentTargetPosition;
		}
	}

	private bool SwingCheck(bool noExplosion = false)
	{
		if (Physics.OverlapSphere(m_Boulder.position, 0.75f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
		{
			if (!noExplosion)
			{
				GameObject temp = UnityEngine.Object.Instantiate(explosion, m_Boulder.position + m_Boulder.forward, Quaternion.identity);
				SetupExplosion(temp);
			}
			SwingStop();
			return true;
		}
		return false;
	}

	private void SetupExplosion(GameObject temp)
	{
		if (temp.TryGetComponent<PhysicalShockwave>(out var component))
		{
			component.target = eid.target;
		}
		if (difficulty > 2 && eid.totalDamageModifier == 1f && eid.totalSpeedModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = temp.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty <= 2)
			{
				explosion.maxSize *= 0.66f;
				explosion.speed /= 0.66f;
			}
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.speed *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	private IEnumerator HorizontalSwingAttack(float time)
	{
		ResetBoulderPose();
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.HorizontalSwing).finalDurationMulti;
		Vector3 actualTarget = GetActualTargetPos();
		sc.DamageStart();
		while (t < time / 3f && swinging)
		{
			float num = Vector3.Distance(actualTarget, base.transform.position);
			Vector3 vector = base.transform.position + base.transform.up * 5f + base.transform.right * (0f - num);
			Vector3 a = m_Boulder.parent.TransformPoint(m_StartPose.position);
			Debug.DrawLine(base.transform.position, vector, Color.red, 8f);
			Vector3 vector2 = Vector3.Lerp(a, vector, t / (time / 2f));
			trail.transform.forward = vector2 - m_Boulder.position;
			m_Boulder.transform.position = vector2;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (SwingCheck(noExplosion: true))
			{
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
				yield break;
			}
		}
		t = 0f;
		float progressEnd = time / 1.5f;
		float yPos = actualTarget.y;
		while (t < progressEnd && swinging)
		{
			float num2 = t / progressEnd;
			if (num2 <= 0.5f)
			{
				actualTarget = GetActualTargetPos();
				_ = 0.12f;
				actualTarget.y = yPos + 2f;
			}
			Vector3 position = base.transform.position;
			float num3 = Vector3.Distance(actualTarget, position);
			float num4 = actualTarget.y - position.y;
			Vector3 vector3 = position + base.transform.up * 5f + base.transform.right * (0f - num3);
			Vector3 vector4 = position + base.transform.up * 5f + Vector3.up * num4 * 2f + base.transform.right * num3;
			trackingY = 1f;
			Quaternion a2 = Quaternion.LookRotation(vector3 - position, Vector3.up);
			Quaternion b = Quaternion.LookRotation(vector4 - position, Vector3.up);
			Quaternion quaternion = Quaternion.LookRotation(actualTarget - position, Vector3.up);
			Quaternion quaternion2 = ((num2 > 0.5f) ? Quaternion.Lerp(quaternion, b, (num2 - 0.5f) * 2f) : Quaternion.Lerp(a2, quaternion, num2 * 2f));
			Vector3 vector5 = position + quaternion2 * Vector3.forward * num3;
			trail.transform.forward = vector5 - m_Boulder.position;
			m_Boulder.position = vector5;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (SwingCheck())
			{
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
				yield break;
			}
		}
		SwingStop();
		TryToRetractArm(2f);
	}

	private IEnumerator OverheadSlamAttack(float time)
	{
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.OverheadSlam).finalDurationMulti;
		sc.DamageStart();
		Vector3 actualTargetPos = GetActualTargetPos();
		while (t < time)
		{
			Vector3 vector = Vector3.Lerp(start, actualTargetPos, t / time);
			vector.y += Vector3.Distance(start, actualTargetPos) * Mathf.Sin(Mathf.Clamp01(t / time) * MathF.PI);
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			actualTargetPos = GetActualTargetPos();
		}
		if (swinging)
		{
			if (Physics.OverlapSphere(m_Boulder.position, 5f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				SlamShockwave();
				SwingStop();
			}
			else
			{
				bool hit = false;
				t = 0f;
				while (!hit)
				{
					Vector3 position = m_Boulder.position;
					position.y -= Time.deltaTime * swingArmSpeed * 400f;
					trail.transform.forward = position - m_Boulder.position;
					m_Boulder.position = position;
					if (Physics.OverlapSphere(m_Boulder.position, 5f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
					{
						SlamShockwave();
						SwingStop();
						hit = true;
					}
					yield return new WaitForEndOfFrame();
					if (t > 1.5f)
					{
						hit = true;
					}
					t += Time.deltaTime;
				}
			}
		}
		trackingY = 0f;
		sc.DamageStop();
		yield return new WaitForSeconds(1f);
		TryToRetractArm(2f);
	}

	private void SlamShockwave()
	{
		Collider[] array = Physics.OverlapSphere(m_Boulder.position, 3.5f, LayerMaskDefaults.Get(LMD.Environment));
		if (array.Length != 0)
		{
			float num = 5f;
			Vector3 vector = m_Boulder.position;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector2 = array[i].ClosestPoint(m_Boulder.position);
				if (Vector3.Distance(m_Boulder.position, vector2) < num)
				{
					vector = vector2;
					num = Vector3.Distance(m_Boulder.position, vector2);
				}
			}
			GameObject temp = UnityEngine.Object.Instantiate(explosion, vector + Vector3.up * 0.1f, Quaternion.identity);
			m_Boulder.position = vector;
			SetupExplosion(temp);
		}
		else
		{
			GameObject temp2 = UnityEngine.Object.Instantiate(explosion, m_Boulder.position, Quaternion.identity);
			m_Boulder.position -= Vector3.up * 2f;
			SetupExplosion(temp2);
		}
	}

	private IEnumerator StabAttack(float time)
	{
		if (eid.target == null)
		{
			yield break;
		}
		ResetBoulderPose();
		Vector3 start = m_Boulder.position;
		float t = 0f;
		time *= swingArmSpeed * GetAnimationDetails(AttackType.Stab).finalDurationMulti;
		Vector3 b = CurrentTargetPosition + Vector3.up * 3f;
		Vector3 attackTarget = base.transform.position + base.transform.forward * Vector3.Distance(base.transform.position, b);
		attackTarget.y = b.y;
		trackingX = 0f;
		trackingY = 0f;
		sc.DamageStart();
		bool canCancel = false;
		while (swinging)
		{
			Vector3 vector = Vector3.LerpUnclamped(start, attackTarget, t / time);
			if (!canCancel && Vector3.Distance(start, vector) >= 20f)
			{
				canCancel = true;
			}
			trail.transform.forward = vector - m_Boulder.position;
			m_Boulder.position = vector;
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			if (canCancel && Physics.OverlapSphere(m_Boulder.position, 2f, LayerMaskDefaults.Get(LMD.Environment)).Length != 0)
			{
				GameObject temp = UnityEngine.Object.Instantiate(explosion, m_Boulder.position + m_Boulder.forward, Quaternion.identity);
				SetupExplosion(temp);
				anim.Play(s_Stab, -1, 0.73f);
				SwingStop();
				yield return new WaitForSeconds(0.5f);
				RetractArm(0.5f);
			}
		}
		sc.DamageStop();
	}

	public void TryToRetractArm(float time)
	{
		if (swinging)
		{
			swinging = false;
			boulderCol.enabled = true;
			boulderCb.Unlaunch(relaunchable: false);
			SwingStop();
			co = StartCoroutine(RetractArmAsync(time));
			isParried = false;
		}
	}

	public void SwingStop()
	{
		trail.emitting = false;
		ParticleSystem obj = swingParticle;
		if (obj != null)
		{
			obj.Stop();
		}
		sc?.DamageStop();
		AudioSource obj2 = swingAudio;
		if (obj2 != null)
		{
			obj2.Stop();
		}
		boulderCb?.Unlaunch(relaunchable: false);
		isParried = false;
	}

	private IEnumerator RetractArmAsync(float time)
	{
		float t = 0f;
		Vector3 boulderStart = m_Boulder.position;
		Transform oldBoulderParent = m_Boulder.parent;
		Vector3 bossStart = base.transform.position;
		if (pullSelfRetract)
		{
			m_Boulder.SetParent(base.transform.parent ? base.transform.parent : null);
		}
		for (; t < time; t += Time.deltaTime)
		{
			Vector3 b = ((!pullSelfRetract) ? m_Boulder.parent.TransformPoint(m_StartPose.position) : m_Boulder.transform.position);
			(pullSelfRetract ? base.transform : m_Boulder).position = Vector3.Lerp(pullSelfRetract ? bossStart : boulderStart, b, t / time);
			_ = pullSelfRetract;
			yield return new WaitForEndOfFrame();
		}
		if (pullSelfRetract)
		{
			m_Boulder.SetParent(oldBoulderParent);
			rb.isKinematic = false;
			rb.useGravity = true;
			base.transform.rotation = Quaternion.identity;
			rb.AddForce(Vector3.down * 300f, ForceMode.VelocityChange);
			pullSelfRetract = false;
			ResetBoulderPose();
			StopAction();
		}
	}

	private void Jump(bool noEnd = false)
	{
		Jump(base.transform.position, noEnd);
	}

	private void Jump(Vector3 target, bool noEnd = false)
	{
		if (jumping || stationary)
		{
			return;
		}
		previouslyJumped = true;
		if (RaycastHelper.RaycastAndDebugDraw(target, Vector3.up, 50f, LayerMaskDefaults.Get(LMD.Environment)) || RaycastHelper.RaycastAndDebugDraw(col.bounds.center, Vector3.up, 25f, LayerMaskDefaults.Get(LMD.Environment)) || RaycastHelper.RaycastAndDebugDraw(col.bounds.center, target - col.bounds.center, Vector3.Distance(col.bounds.center, target), LayerMaskDefaults.Get(LMD.Environment)))
		{
			superJumping = true;
		}
		didCollide = false;
		jumpTarget = target;
		if (superJumping)
		{
			NavMeshHit val = default(NavMeshHit);
			RaycastHit hitInfo;
			if (NavMesh.SamplePosition(target, ref val, 2f, nma.areaMask))
			{
				jumpTarget = ((NavMeshHit)(ref val)).position;
			}
			else if (Physics.Raycast(target, -Vector3.up, out hitInfo, 3f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				jumpTarget = hitInfo.point;
			}
			if (!CanFit(jumpTarget))
			{
				int num = 60;
				float num2 = UnityEngine.Random.Range(0f, 360f);
				int num3 = 0;
				while (num3 < 360)
				{
					Vector3 vector = jumpTarget + Quaternion.Euler(0f, (float)num3 + num2, 0f) * Vector3.forward * 12f;
					vector += Vector3.up * 2f;
					if (!Physics.Linecast(jumpTarget + Vector3.up * 2f, vector, LayerMaskDefaults.Get(LMD.Environment)))
					{
						Debug.DrawRay(vector, Vector3.down * 50f, Color.yellow, 50f);
						if (Physics.Raycast(vector, Vector3.down, out var hitInfo2, 50f, LayerMaskDefaults.Get(LMD.Environment)))
						{
							if (!CanFit(hitInfo2.point))
							{
								num3 += num;
								continue;
							}
							jumpTarget = hitInfo2.point;
							break;
						}
					}
					num3 += num;
				}
			}
		}
		jumping = true;
		anim.Play("Jump");
		rb.isKinematic = false;
		rb.useGravity = true;
		if (superJumping)
		{
			col.isTrigger = true;
		}
		((Behaviour)(object)nma).enabled = false;
		UnityEngine.Object.Instantiate(rubble, base.transform.position, base.transform.rotation);
		rb.velocity = Vector3.zero;
		rb.AddForce(Vector3.up * Mathf.Max(50f, 100f + Vector3.Distance(base.transform.position, target)), ForceMode.VelocityChange);
		trackingX = 0f;
		trackingY = 1f;
		inAction = true;
		if (!noEnd)
		{
			Invoke("StopAction", 0.5f);
		}
	}

	private void FlyToArm()
	{
		if (!airStabCancelled)
		{
			inAction = false;
			pullSelfRetract = true;
			forceCorrectOrientation = true;
			trackingX = 0.3f;
			aud.SetPitch(UnityEngine.Random.Range(1.4f, 1.6f));
			aud.PlayOneShot(attackVoices[3], tracked: true);
			anim.SetFloat(s_SwingAnimSpeed, 1f);
			swinging = true;
			TryToRetractArm(0.4f);
		}
	}

	private void CancelAirStab()
	{
		Vector3 position = base.transform.position;
		Vector3 currentTargetPosition = CurrentTargetPosition;
		if (eid.target != null)
		{
			position.y = currentTargetPosition.y;
		}
		if (eid.target != null && Vector3.Distance(position, currentTargetPosition) > Vector3.Distance(m_Boulder.position, currentTargetPosition) && !swinging)
		{
			airStabCancelled = false;
			return;
		}
		inAction = false;
		airStabCancelled = true;
		pullSelfRetract = false;
		swinging = true;
		RetractArm(1f);
		anim.SetTrigger(s_AirStabCancel);
		rb.isKinematic = false;
		rb.useGravity = true;
		((Behaviour)(object)nma).enabled = false;
		rb.velocity = Vector3.zero;
		forceCorrectOrientation = true;
		trackingX = 0.3f;
		trackingY = 1f;
	}

	public void Death()
	{
		aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
		aud.PlayOneShot(deathVoice, tracked: true);
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		Transform[] array = legs;
		foreach (Transform obj in array)
		{
			obj.parent = componentInParent.gibZone;
			Rigidbody[] componentsInChildren = obj.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody obj2 in componentsInChildren)
			{
				obj2.isKinematic = false;
				obj2.useGravity = true;
			}
		}
		EnemyIdentifierIdentifier[] componentsInChildren2 = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		if (GraphicsSettings.bloodEnabled)
		{
			EnemyIdentifierIdentifier[] array2 = componentsInChildren2;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array2)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, enemyIdentifierIdentifier.eid);
				if ((bool)gore)
				{
					gore.transform.position = enemyIdentifierIdentifier.transform.position;
					gore.transform.SetParent(componentInParent.goreZone, worldPositionStays: true);
					gore.SetActive(value: true);
				}
				for (int k = 0; k < 3; k++)
				{
					GameObject gib = MonoSingleton<BloodsplatterManager>.Instance.GetGib(BSType.gib);
					if ((bool)gib)
					{
						gib.transform.SetPositionAndRotation(enemyIdentifierIdentifier.transform.position, UnityEngine.Random.rotation);
						gib.transform.SetParent(componentInParent.gibZone, worldPositionStays: true);
						gib.transform.localScale *= 4f;
					}
				}
			}
		}
		if (mach.musicRequested)
		{
			MonoSingleton<MusicManager>.Instance.PlayCleanMusic();
		}
		armature.localScale = Vector3.zero;
		Collider[] componentsInChildren3 = GetComponentsInChildren<Collider>();
		for (int num = componentsInChildren3.Length - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[num]);
		}
		UnityEngine.Object.Destroy(m_Boulder.gameObject);
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)anim);
		UnityEngine.Object.Destroy(this);
	}

	private void StopAction()
	{
		inAction = false;
		dontFacePlayer = false;
		knockedDownByCannonball = false;
	}

	private void ResetBoulderPose()
	{
		m_Boulder.localPosition = m_StartPose.position;
		m_Boulder.localRotation = m_StartPose.rotation;
		boulderCb.Unlaunch();
		isParried = false;
	}

	private void RotateTowardsTarget()
	{
		if (eid.target != null)
		{
			Vector3 vector = CurrentTargetPosition;
			if (gce.onGround || forceCorrectOrientation)
			{
				vector = new Vector3(CurrentTargetPosition.x, base.transform.position.y, CurrentTargetPosition.z);
			}
			Quaternion b = Quaternion.LookRotation(vector - base.transform.position);
			float num = (Quaternion.Angle(base.transform.rotation, b) * 10f + 30f) * Time.fixedDeltaTime;
			float num2 = base.transform.rotation.eulerAngles.x;
			float num3 = base.transform.rotation.eulerAngles.y;
			while (num2 - b.eulerAngles.x > 180f)
			{
				num2 -= 360f;
			}
			for (; num2 - b.eulerAngles.x < -180f; num2 += 360f)
			{
			}
			while (num3 - b.eulerAngles.y > 180f)
			{
				num3 -= 360f;
			}
			for (; num3 - b.eulerAngles.y < -180f; num3 += 360f)
			{
			}
			float num4 = 1f;
			if (difficulty == 1)
			{
				num4 = 0.75f;
			}
			else if (difficulty == 0)
			{
				num4 = 0.5f;
			}
			base.transform.rotation = Quaternion.Euler(Mathf.MoveTowards(num2, b.eulerAngles.x, num * trackingX * num4), Mathf.MoveTowards(num3, b.eulerAngles.y, num * trackingY * num4), Mathf.MoveTowards(base.transform.rotation.eulerAngles.z, b.eulerAngles.z, num));
		}
	}

	public void StompExplosion()
	{
		Vector3 vector = base.transform.position + Vector3.up;
		Vector3 currentTargetPosition = CurrentTargetPosition;
		if (Physics.Raycast(vector, currentTargetPosition - vector, Vector3.Distance(currentTargetPosition, vector), LayerMaskDefaults.Get(LMD.Environment)))
		{
			vector = base.transform.position + Vector3.up * 5f;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(this.explosion, vector, Quaternion.identity);
		if (difficulty > 2 && eid.totalDamageModifier == 1f && eid.totalSpeedModifier == 1f)
		{
			return;
		}
		Explosion[] componentsInChildren = gameObject.GetComponentsInChildren<Explosion>();
		foreach (Explosion explosion in componentsInChildren)
		{
			if (difficulty >= 3)
			{
				explosion.maxSize *= 1.5f;
				explosion.speed *= 1.5f;
			}
			explosion.maxSize *= eid.totalDamageModifier;
			explosion.speed *= eid.totalDamageModifier;
			explosion.damage = Mathf.RoundToInt((float)explosion.damage * eid.totalDamageModifier);
		}
	}

	public void PlayHurtSound(int type = 0)
	{
		if ((bool)currentHurtSound)
		{
			if (type == 0)
			{
				return;
			}
			UnityEngine.Object.Destroy(currentHurtSound);
		}
		currentHurtSound = UnityEngine.Object.Instantiate(hurtSounds[type], base.transform.position, Quaternion.identity);
	}

	public void GotParried()
	{
		isParried = true;
		if (co != null)
		{
			StopCoroutine(co);
		}
	}

	public void Knockdown(Vector3 boulderPos)
	{
		if (!pullSelfRetract)
		{
			if (co != null)
			{
				StopCoroutine(co);
			}
			if (!knockedDownByCannonball)
			{
				base.transform.LookAt(new Vector3(boulderPos.x, base.transform.position.y, boulderPos.z));
			}
			if (!inAction && gce.onGround)
			{
				inAction = true;
				if (!stationary && nma.isOnNavMesh)
				{
					nma.SetDestination(base.transform.position);
				}
			}
		}
		if (!gce.onGround)
		{
			superKnockdownWindow = 0.25f;
		}
		dontFacePlayer = true;
		if (gce.onGround && !knockedDownByCannonball)
		{
			knockedDownByCannonball = true;
			anim.Play("Knockdown");
		}
		PlayHurtSound(2);
		trackingX = 0f;
		trackingY = 0f;
		if (knockedDownByCannonball)
		{
			Invoke("CheckLoop", 0.85f);
		}
		GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Splatter, eid);
		if ((bool)gore)
		{
			gore.transform.position = boulderPos;
			gore.transform.up = base.transform.forward;
			gore.transform.SetParent(GetComponentInParent<GoreZone>().goreZone, worldPositionStays: true);
			gore.SetActive(value: true);
			if (gore.TryGetComponent<Bloodsplatter>(out var component))
			{
				component.GetReady();
			}
		}
		if (!pullSelfRetract)
		{
			ResetBoulderPose();
			SwingStop();
		}
	}

	public void FallSound()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
		UnityEngine.Object.Instantiate(fallSound, base.transform.position, Quaternion.identity);
	}

	private void FallKillEnemy(EnemyIdentifier eid)
	{
		if ((bool)MonoSingleton<StyleHUD>.Instance && !eid.dead)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.insurrstomp", null, eid);
		}
		eid.hitter = "maurice";
		fallEnemiesHit.Add(eid);
		if (eid.TryGetComponent<Collider>(out var component))
		{
			Physics.IgnoreCollision(col, component, ignore: true);
		}
		EnemyIdentifier.FallOnEnemy(eid);
	}

	public void CheckLoop()
	{
		if (downed)
		{
			anim.SetFloat("DownedSpeed", 0f);
			Invoke("CheckLoop", 0.1f);
		}
		else
		{
			anim.SetFloat("DownedSpeed", (float)((difficulty < 4) ? 1 : 2));
		}
	}

	private void Undown()
	{
		downed = false;
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = 10f,
			angularSpeed = 999f,
			acceleration = 666f
		};
	}
}
