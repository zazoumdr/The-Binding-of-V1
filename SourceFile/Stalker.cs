using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class Stalker : EnemyScript, IEnemyRelationshipLogic
{
	private EnemyIdentifier eid;

	private Enemy mach;

	private int difficulty = -1;

	private NavMeshAgent nma;

	[HideInInspector]
	public float defaultMovementSpeed;

	private Animator anim;

	private bool inAction;

	private float explosionCharge;

	private float countDownAmount;

	private bool exploding;

	private bool exploded;

	public AssetReference explosion;

	private float maxHp;

	private Light lit;

	private Color currentColor;

	public Color[] lightColors;

	private bool blinking;

	private float blinkTimer;

	private AudioSource lightAud;

	public AudioClip[] lightSounds;

	public SkinnedMeshRenderer canRenderer;

	public GameObject stepSound;

	public GameObject screamSound;

	private float explodeSpeed = 1f;

	public float prepareTime = 5f;

	public float prepareWarningTime = 3f;

	private void Awake()
	{
		mach = GetComponent<Enemy>();
		lit = GetComponentInChildren<Light>();
		lightAud = lit.GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		eid = GetComponent<EnemyIdentifier>();
		nma = GetComponent<NavMeshAgent>();
		defaultMovementSpeed = nma.speed;
	}

	private void Start()
	{
		maxHp = mach.health;
		currentColor = lightColors[0];
		lightAud.clip = lightSounds[0];
		lightAud.loop = false;
		lightAud.SetPitch(1f);
		lightAud.volume = 0.35f;
		SetSpeed();
		NavigationUpdate();
		SlowUpdate();
	}

	public override EnemyMovementData GetSpeed(int difficulty)
	{
		return new EnemyMovementData
		{
			speed = defaultMovementSpeed,
			angularSpeed = 1600f,
			acceleration = 64f
		};
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(Object)(object)anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!(Object)(object)nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (defaultMovementSpeed == 0f)
		{
			defaultMovementSpeed = nma.speed;
		}
		UpdateSpeed();
		anim.speed = eid.totalSpeedModifier;
		anim.SetFloat("ExplodeSpeed", explodeSpeed);
	}

	private void UpdateSpeed()
	{
		float num = Mathf.Pow(0.8f, eid.stuckMagnets.Count);
		nma.speed = defaultMovementSpeed * eid.totalSpeedModifier * num;
	}

	private void OnDisable()
	{
		if (exploding)
		{
			exploding = false;
			explosionCharge = prepareTime;
			inAction = false;
			blinking = false;
		}
	}

	private void NavigationUpdate()
	{
		Invoke("NavigationUpdate", 0.1f);
		if (!((Object)(object)nma == null) && ((Behaviour)(object)nma).enabled && nma.isOnNavMesh)
		{
			if (eid.target == null)
			{
				mach.SetDestination(base.transform.position);
			}
			else if ((bool)mach && mach.grounded)
			{
				mach.SetDestination(inAction ? base.transform.position : eid.target.position);
			}
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", GetUpdateRate(nma, 0.5f));
		if (inAction || !mach || !mach.grounded || !(Object)(object)nma || !nma.isOnNavMesh || !eid.attackEnemies)
		{
			return;
		}
		List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies == null || currentEnemies.Count == 0)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		float num = float.PositiveInfinity;
		EnemyIdentifier targetEid = null;
		for (int num2 = 7; num2 >= 0; num2--)
		{
			for (int i = 0; i < currentEnemies.Count; i++)
			{
				if (currentEnemies[i].flying || currentEnemies[i].puppet || currentEnemies[i].sandified || currentEnemies[i].enemyType == EnemyType.Deathcatcher || MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[i]) < num2)
				{
					continue;
				}
				float num3 = Vector3.Distance(base.transform.position, GetEnemyTargetPosition(currentEnemies[i]));
				if (num3 >= num || !CheckForPath(currentEnemies[i]))
				{
					continue;
				}
				bool flag3 = MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(currentEnemies[i].transform) && (eid.target == null || currentEnemies[i].transform != eid.target.targetTransform);
				if (!(flag3 && flag))
				{
					if (flag3 || num3 >= 100f)
					{
						flag2 = true;
					}
					else
					{
						flag = true;
					}
					targetEid = currentEnemies[i];
					num = num3;
				}
			}
			if (flag)
			{
				ChangeTarget(targetEid);
				return;
			}
		}
		if (flag2)
		{
			ChangeTarget(targetEid);
			return;
		}
		if (eid.target != null)
		{
			RemoveTarget();
		}
		eid.enemyScanner?.Reset();
		eid.target = EnemyTarget.TrackPlayerIfAllowed();
	}

	private void ChangeTarget(EnemyIdentifier targetEid)
	{
		if (eid.target != null && MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(eid.target.targetTransform))
		{
			MonoSingleton<StalkerController>.Instance.targets.Remove(eid.target.targetTransform);
		}
		MonoSingleton<StalkerController>.Instance.targets.Add(targetEid.transform);
		eid.target = new EnemyTarget(targetEid.transform);
		targetEid.buffTargeter = eid;
	}

	private void RemoveTarget()
	{
		if (MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(eid.target.targetTransform))
		{
			MonoSingleton<StalkerController>.Instance.targets.Remove(eid.target.targetTransform);
		}
		if (eid.target.targetTransform.TryGetComponent<EnemyIdentifier>(out var component) && component.buffTargeter == eid)
		{
			component.buffTargeter = null;
		}
		eid.target = EnemyTarget.TrackPlayerIfAllowed();
	}

	private void Update()
	{
		UpdateSpeed();
		UpdateLightColor();
		UpdateAnimations();
		if (exploding)
		{
			countDownAmount = Mathf.MoveTowards(countDownAmount, 2f, Time.deltaTime * explodeSpeed * eid.totalSpeedModifier);
			if (countDownAmount >= 2f)
			{
				exploding = false;
				SandExplode(0);
			}
			return;
		}
		if (explosionCharge < prepareTime)
		{
			explosionCharge = Mathf.MoveTowards(explosionCharge, prepareTime, Time.deltaTime * eid.totalSpeedModifier);
			if (explosionCharge > prepareWarningTime)
			{
				blinking = true;
			}
			return;
		}
		if (lit.color != lightColors[1] * (mach.health / maxHp))
		{
			blinking = false;
			currentColor = lightColors[1];
			lightAud.clip = lightSounds[1];
			lightAud.loop = true;
			lightAud.SetPitch(0.5f);
			lightAud.volume = 0.65f;
			lightAud.Play(tracked: true);
		}
		if (explosionCharge < prepareTime + 1f)
		{
			explosionCharge = Mathf.MoveTowards(explosionCharge, prepareTime + 1f, Time.deltaTime * eid.totalSpeedModifier);
		}
		else if (eid.target != null)
		{
			Vector3 b = (eid.target.isPlayer ? GetPlayerTargetPosition(eid.target) : GetEnemyTargetPosition(eid.target.enemyIdentifier));
			if (Vector3.Distance(base.transform.position, b) < 8f && !exploding)
			{
				exploding = true;
				Countdown();
			}
		}
	}

	private void UpdateAnimations()
	{
		if (!inAction)
		{
			anim.SetBool("Running", nma.velocity.magnitude > 5f);
			anim.SetBool("Walking", nma.velocity.magnitude > 0f);
		}
	}

	private void UpdateLightColor()
	{
		Color color = currentColor * ((mach.health + 0.2f) / (maxHp + 0.2f));
		if (blinking)
		{
			blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, Time.deltaTime);
			if (blinkTimer <= 0f)
			{
				lit.color = ((lit.color != Color.black) ? Color.black : color);
				if (lit.color != Color.black)
				{
					AudioSource obj = lightAud;
					if (obj != null)
					{
						obj.Stop();
					}
				}
				else
				{
					lightAud?.Play(tracked: true);
				}
				blinkTimer = 0.1f;
			}
		}
		else
		{
			lit.color = color;
			blinkTimer = 0f;
		}
		if ((bool)canRenderer)
		{
			canRenderer.material.SetColor("_EmissiveColor", lit.color);
		}
	}

	private Vector3 GetPlayerTargetPosition(EnemyTarget target)
	{
		VisionQuery query = new VisionQuery("Player", (TargetDataRef t) => t.target.isPlayer);
		if (!mach.vision.TrySee(query, out var data))
		{
			return target.targetTransform.position;
		}
		return data.position;
	}

	private Vector3 GetEnemyTargetPosition(EnemyIdentifier targetEID)
	{
		VisionQuery query = new VisionQuery("Enemies", (TargetDataRef t) => CheckSameTarget(t, targetEID));
		if (!mach.vision.TrySee(query, out var data))
		{
			return targetEID.transform.position;
		}
		return data.position;
	}

	private bool CheckSameTarget(TargetDataRef data, EnemyIdentifier eid)
	{
		ITarget target = data.target;
		if (!target.isEnemy)
		{
			return false;
		}
		return (object)target.EID == eid;
	}

	public void Countdown()
	{
		inAction = true;
		blinking = true;
		currentColor = lightColors[2];
		lightAud.clip = lightSounds[2];
		lightAud.loop = false;
		lightAud.SetPitch(1f);
		lightAud.volume = 0.65f;
		explosionCharge = 0f;
		countDownAmount = 0f;
		Object.Instantiate(screamSound, base.transform);
		anim.SetTrigger("Explode");
	}

	public void SandExplode(int onDeath = 1)
	{
		if (exploded)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(explosion.ToAsset(), base.transform.position + Vector3.up * 2.5f, Quaternion.identity);
		if (onDeath != 1)
		{
			gameObject.transform.localScale *= 1.5f;
		}
		if (eid.stuckMagnets.Count > 0)
		{
			float num = 0.75f;
			if (eid.stuckMagnets.Count > 1)
			{
				num -= 0.125f * (float)(eid.stuckMagnets.Count - 1);
			}
			gameObject.transform.localScale *= num;
		}
		if (eid.target != null && (bool)eid.target.enemyIdentifier && eid.target.enemyIdentifier.sandified)
		{
			RemoveTarget();
		}
		if (onDeath != 1 && (difficulty > 3 || eid.blessed || InvincibleEnemies.Enabled))
		{
			exploding = false;
			countDownAmount = 0f;
			explosionCharge = 0f;
			currentColor = lightColors[0];
			lightAud.clip = lightSounds[0];
			blinking = false;
			return;
		}
		exploded = true;
		if (!mach.limp && onDeath != 1)
		{
			mach.GoLimp();
			eid.Death();
		}
		if (eid.drillers.Count != 0)
		{
			for (int num2 = eid.drillers.Count - 1; num2 >= 0; num2--)
			{
				Object.Destroy(eid.drillers[num2].gameObject);
			}
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	public bool CheckForPath(EnemyIdentifier ed)
	{
		if (ed == null)
		{
			return false;
		}
		if (ed.enemyType == EnemyType.MaliciousFace)
		{
			return CheckForOffsetPath(ed);
		}
		return CheckForPath(ed.transform.position);
	}

	public bool CheckForOffsetPath(EnemyIdentifier ed)
	{
		if (ed.TryGetComponent<NavMeshAgent>(out var component))
		{
			return CheckForPath(ed.transform.position + Vector3.down * component.height * component.baseOffset * ed.transform.localScale.y);
		}
		return false;
	}

	public bool CheckForPath(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		NavMeshPath val = new NavMeshPath();
		nma.CalculatePath(position, val);
		if (val != null)
		{
			return (int)val.status == 0;
		}
		return false;
	}

	public void StopAction()
	{
		inAction = false;
	}

	public void Step()
	{
		Object.Instantiate(stepSound, base.transform.position, Quaternion.identity);
	}

	public bool ShouldAttackEnemies()
	{
		return true;
	}

	public bool ShouldIgnorePlayer()
	{
		return false;
	}
}
