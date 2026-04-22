using System.Collections.Generic;
using ULTRAKILL.Enemy;
using UnityEngine;

public class Mass : EnemyScript
{
	private Animator anim;

	private bool battleMode;

	private Vector3 targetPos;

	private Quaternion targetRot;

	private float transformCooldown;

	private bool walking;

	private float walkWeight;

	public bool inAction;

	private bool inSemiAction;

	public Transform[] shootPoints;

	public GameObject homingProjectile;

	private float homingAttackChance = 50f;

	public float explosiveCooldown = 2f;

	public GameObject explosiveProjectile;

	public float explosiveProjectileLaunchVelocity = 50f;

	public GameObject slamExplosion;

	private SwingCheck2[] swingChecks;

	private float swingCooldown = 2f;

	private bool attackedOnce;

	private float playerDistanceCooldown = 1.5f;

	public Transform tailEnd;

	private GameObject tailSpear;

	private float spearCooldown = 5f;

	public GameObject spear;

	public bool spearShot;

	public GameObject spearFlash;

	public GameObject tempSpear;

	public List<GameObject> tailHitboxes = new List<GameObject>();

	public GameObject regurgitateSound;

	public GameObject bigPainSound;

	public GameObject windupSound;

	public bool dead;

	public bool crazyMode;

	public float crazyModeHealth;

	private Enemy stat;

	public EnemyIdentifier eid;

	private int crazyPoint;

	public GameObject enrageEffect;

	public GameObject currentEnrageEffect;

	public Material enrageMaterial;

	public GameObject[] activateOnEnrage;

	private int difficulty = -1;

	private VisionQuery targetQuery;

	private TargetHandle targetHandle;

	private TargetData lastTargetData;

	public bool isDead;

	private bool hasVision => targetHandle != null;

	private List<Transform> transforms => stat.transforms;

	private GoreZone gz => stat.gz;

	private BloodsplatterManager bsm => stat.bsm;

	public override bool ShouldKnockback(ref DamageData data)
	{
		return false;
	}

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		swingChecks = GetComponentsInChildren<SwingCheck2>();
		stat = GetComponent<Enemy>();
	}

	private void Start()
	{
		transformCooldown = 10f;
		LayerMask lm = LayerMaskDefaults.Get(LMD.Environment);
		targetQuery = new VisionQuery("Target", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(VisionSourcePosition, lm));
		SetSpeed();
		stat.isMassDeath = true;
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!(Object)(object)anim)
		{
			anim = GetComponentInChildren<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		switch (difficulty)
		{
		case 4:
		case 5:
			anim.speed = 1.25f;
			break;
		case 2:
		case 3:
			anim.speed = 1f;
			break;
		case 1:
			anim.speed = 0.85f;
			break;
		case 0:
			anim.speed = 0.65f;
			break;
		}
		Animator obj = anim;
		obj.speed *= eid.totalSpeedModifier;
	}

	private void OnDisable()
	{
		StopAction();
		inSemiAction = false;
		if (spearShot)
		{
			SpearReturned();
		}
		if (swingChecks != null)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
		}
	}

	private void OnEnable()
	{
		if (battleMode)
		{
			anim.Play("BattlePose");
		}
	}

	private void Update()
	{
		if (isDead)
		{
			return;
		}
		if (eid.target != null)
		{
			UpdateVision();
			if (!hasVision)
			{
				if (eid.target == null)
				{
					return;
				}
				targetPos = eid.target.position;
			}
			else
			{
				targetPos = lastTargetData.position;
			}
			Vector3 vector = ToPlanePos(targetPos);
			targetRot = Quaternion.LookRotation(vector - base.transform.position, base.transform.up);
			RotateToTarget();
			UpdateCooldowns();
			ChooseAction();
		}
		else
		{
			targetHandle = null;
			if (inAction)
			{
				StopAction();
			}
			if (battleMode)
			{
				ToScout();
			}
		}
	}

	private void UpdateVision()
	{
		if (stat.vision.TrySee(targetQuery, out var data))
		{
			lastTargetData = data.ToData();
			targetHandle = lastTargetData.handle;
		}
		else
		{
			targetHandle = null;
		}
	}

	private void RotateToTarget()
	{
		if (!inAction && base.transform.rotation != targetRot)
		{
			if (battleMode || crazyMode)
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * Quaternion.Angle(base.transform.rotation, targetRot) + Time.deltaTime * 50f * eid.totalSpeedModifier);
			}
			else
			{
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRot, Time.deltaTime * Quaternion.Angle(base.transform.rotation, targetRot) + Time.deltaTime * 120f * eid.totalSpeedModifier);
			}
			walking = stat.health >= 35f;
		}
		else
		{
			walking = false;
		}
		if ((walking && walkWeight != 1f) || (!walking && walkWeight != 0f))
		{
			walkWeight = Mathf.MoveTowards(walkWeight, walking ? 1 : 0, Time.deltaTime * 4f);
			anim.SetLayerWeight(1, walkWeight);
		}
	}

	private void UpdateCooldowns()
	{
		if (spearCooldown != 0f && !spearShot)
		{
			spearCooldown = Mathf.MoveTowards(spearCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if (swingCooldown != 0f)
		{
			swingCooldown = Mathf.MoveTowards(swingCooldown, 0f, Time.deltaTime * ((difficulty >= 4) ? 1.5f : 1f) * eid.totalSpeedModifier);
		}
		if (explosiveCooldown != 0f)
		{
			explosiveCooldown = Mathf.MoveTowards(explosiveCooldown, 0f, Time.deltaTime * ((difficulty >= 4) ? 1.5f : 1f) * eid.totalSpeedModifier);
		}
		playerDistanceCooldown = Mathf.MoveTowards(playerDistanceCooldown, (!(Vector3.Distance(targetPos, base.transform.position) < 7f)) ? 3 : 0, Time.deltaTime * eid.totalSpeedModifier);
		if (transformCooldown != 0f)
		{
			transformCooldown = Mathf.MoveTowards(transformCooldown, 0f, Time.deltaTime * (battleMode ? 1f : 1.5f) * eid.totalSpeedModifier);
		}
	}

	private void ChooseAction()
	{
		if ((!hasVision && eid.target == null) || inAction || inSemiAction)
		{
			return;
		}
		if (hasVision && SpearCanBeActive() && spearCooldown == 0f && (transformCooldown > 1f || crazyMode))
		{
			Vector3 position = lastTargetData.position;
			if (!PortalPhysicsV2.Raycast(tailEnd.position, position - tailEnd.position, Vector3.Distance(position, tailEnd.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				spearCooldown = Random.Range(2, 4);
				ReadySpear();
			}
			else
			{
				spearCooldown = 0.1f;
			}
		}
		else
		{
			if (crazyMode)
			{
				return;
			}
			if (stat.health < crazyModeHealth)
			{
				if (battleMode)
				{
					ToScout();
				}
				else
				{
					anim.SetBool("Crazy", true);
				}
				return;
			}
			if (transformCooldown <= 0f)
			{
				if (battleMode)
				{
					ToScout();
				}
				else
				{
					ToBattle();
				}
				return;
			}
			if (!battleMode && playerDistanceCooldown == 0f)
			{
				ToBattle();
			}
			if (transformCooldown <= 0f)
			{
				return;
			}
			if (battleMode)
			{
				if (!(swingCooldown > 0f))
				{
					Vector3 worldPosition = ToPlanePos(targetPos);
					base.transform.LookAt(worldPosition, base.transform.up);
					if (ShouldBattleSlam())
					{
						BattleSlam();
					}
					else
					{
						SwingAttack();
					}
				}
			}
			else if (explosiveCooldown <= 0f)
			{
				ExplosiveAttack();
			}
		}
	}

	private bool SpearCanBeActive()
	{
		if (eid.target == null || dead || inAction || inSemiAction)
		{
			return false;
		}
		if (!battleMode && (!crazyMode || difficulty < 4))
		{
			return false;
		}
		return true;
	}

	private bool ShouldBattleSlam()
	{
		if (!attackedOnce)
		{
			return false;
		}
		Vector3 vector = (hasVision ? lastTargetData.position : targetPos);
		if (vector.y - base.transform.position.y > 15f)
		{
			return false;
		}
		if (vector.y - base.transform.position.y < -5f)
		{
			return false;
		}
		if (Vector3.Distance(targetPos, base.transform.position) > 15f)
		{
			return true;
		}
		if (Vector3.Distance(targetPos, base.transform.position) > 7f && Random.Range(0f, 1f) < 0.5f)
		{
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (SpearCanBeActive() && hasVision)
		{
			Vector3 position = lastTargetData.position;
			tailEnd.LookAt(position);
		}
	}

	public void HomingAttack()
	{
		inAction = true;
		anim.SetTrigger("HomingAttack");
		explosiveCooldown = Random.Range(3, 5);
	}

	public void ExplosiveAttack()
	{
		inAction = true;
		anim.SetTrigger("ExplosiveAttack");
		explosiveCooldown = Random.Range(3, 5);
	}

	public void SwingAttack()
	{
		inAction = true;
		anim.SetTrigger("Swing");
		swingCooldown = Random.Range(3, 5);
		Object.Instantiate(windupSound, shootPoints[2].position, Quaternion.identity);
		attackedOnce = true;
	}

	public void ToScout()
	{
		if (battleMode)
		{
			battleMode = false;
			transformCooldown = Random.Range(8, 12);
			inAction = true;
			anim.SetBool("Transform", true);
			eid.weakPoint = stat.extraDamageZones[0];
		}
	}

	public void ToBattle()
	{
		if (!battleMode)
		{
			battleMode = true;
			Vector3 worldPosition = ToPlanePos(targetPos);
			base.transform.LookAt(worldPosition, base.transform.up);
			transformCooldown = Random.Range(8, 12);
			inAction = true;
			anim.SetBool("Transform", false);
			anim.SetTrigger("Slam");
			spearCooldown = 3f;
			SlamWindup();
			eid.weakPoint = stat.extraDamageZones[1];
			attackedOnce = false;
		}
	}

	private void SlamWindup()
	{
		AudioSource component = Object.Instantiate(windupSound, shootPoints[2].position, Quaternion.identity).GetComponent<AudioSource>();
		component.SetPitch(1f);
		component.volume = 0.75f;
	}

	public void SlamImpact()
	{
		if (!dead)
		{
			GameObject gameObject = Object.Instantiate(slamExplosion, new Vector3(shootPoints[2].position.x, base.transform.position.y, shootPoints[2].position.z), Quaternion.identity);
			PhysicalShockwave component = gameObject.GetComponent<PhysicalShockwave>();
			float num = 1.5f;
			switch (difficulty)
			{
			case 4:
			case 5:
				component.speed = 35f;
				num = 2.5f;
				break;
			case 2:
			case 3:
				component.speed = 25f;
				num = 2.5f;
				break;
			case 1:
				component.speed = 20f;
				num = 2f;
				break;
			case 0:
				component.speed = 15f;
				num = 1.5f;
				break;
			}
			gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y * num, gameObject.transform.localScale.z);
			component.damage = Mathf.RoundToInt(30f * eid.totalDamageModifier);
			component.maxSize = 100f;
			component.enemy = true;
			component.enemyType = EnemyType.HideousMass;
			gameObject.transform.SetParent(stat.GetGoreZone().transform, worldPositionStays: true);
		}
	}

	public void ShootHoming(int arm)
	{
		ShootProjectile(arm, homingProjectile, 5f);
	}

	public void ShootExplosive(int arm)
	{
		ShootProjectile(arm, explosiveProjectile, explosiveProjectileLaunchVelocity);
	}

	private void ShootProjectile(int arm, GameObject projectile, float velocity)
	{
		if (!dead && eid.target != null)
		{
			Transform transform = shootPoints[arm];
			GameObject obj = Object.Instantiate(projectile, transform.position, transform.rotation);
			if (obj.TryGetComponent<Rigidbody>(out var component))
			{
				component.AddForce(transform.up * velocity, ForceMode.VelocityChange);
			}
			if (obj.TryGetComponent<Projectile>(out var component2))
			{
				component2.target = eid.target;
				component2.safeEnemyType = EnemyType.HideousMass;
				component2.transform.SetParent(stat.GetGoreZone().transform, worldPositionStays: true);
				component2.damage *= eid.totalDamageModifier;
			}
		}
	}

	private void ReadySpear()
	{
		if (eid.target != null && !dead && difficulty != 0 && (difficulty > 3 || !crazyMode))
		{
			inSemiAction = true;
			if (tailSpear == null)
			{
				tailSpear = tailEnd.GetChild(1).gameObject;
			}
			Object.Instantiate(spearFlash, tailSpear.transform.position, Quaternion.identity).transform.SetParent(tailSpear.transform, worldPositionStays: true);
			Object.Instantiate(regurgitateSound, tailSpear.transform.position, Quaternion.identity);
			anim.SetBool("ShootSpear", true);
			if (crazyMode)
			{
				anim.SetLayerWeight(2, 1f);
			}
		}
	}

	public void ShootSpear()
	{
		if (eid.target == null || dead || difficulty == 0)
		{
			return;
		}
		inSemiAction = false;
		tailEnd.LookAt(targetPos);
		tempSpear = Object.Instantiate(spear, tailSpear.transform.position, tailEnd.rotation);
		tempSpear.transform.LookAt(targetPos, base.transform.up);
		if (tempSpear.TryGetComponent<MassSpear>(out var component))
		{
			component.target = eid.target;
			component.targetHandle = lastTargetData.handle;
			component.originPoint = tailSpear.transform;
			component.damageMultiplier = eid.totalDamageModifier;
			component.difficulty = difficulty;
			if (difficulty >= 4)
			{
				component.spearHealth *= 2f;
			}
		}
		tailSpear.SetActive(value: false);
		spearShot = true;
		anim.SetBool("ShootSpear", false);
		anim.SetLayerWeight(2, 0f);
	}

	public void SpearParried()
	{
		if (!dead)
		{
			inAction = true;
			anim.SetTrigger("SpearParried");
			Object.Instantiate(bigPainSound, tailSpear.transform);
		}
	}

	public void SpearReturned()
	{
		tailSpear.SetActive(value: true);
		spearShot = false;
	}

	public void StopAction()
	{
		inAction = false;
	}

	public void BattleSlam()
	{
		inAction = true;
		anim.SetTrigger("BattleSlam");
		swingCooldown = Random.Range(3, 5);
		SlamWindup();
	}

	public void SwingStart()
	{
		if (!dead)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStart();
			}
		}
	}

	public void SwingEnd()
	{
		if (!dead)
		{
			SwingCheck2[] array = swingChecks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			GameObject obj = Object.Instantiate(slamExplosion, (shootPoints[0].position + shootPoints[1].position) / 2f, Quaternion.identity);
			obj.transform.up = base.transform.right;
			PhysicalShockwave component = obj.GetComponent<PhysicalShockwave>();
			component.damage = Mathf.RoundToInt(20f * eid.totalDamageModifier);
			component.speed = 100f;
			component.maxSize = ((difficulty < 2) ? 10 : 100);
			component.enemy = true;
			component.enemyType = EnemyType.HideousMass;
			AudioSource component2 = obj.GetComponent<AudioSource>();
			component2.SetPitch(1.5f);
			component2.volume = 0.5f;
			obj.transform.SetParent(stat.GetGoreZone().transform, worldPositionStays: true);
		}
	}

	public void Enrage()
	{
		currentEnrageEffect = Object.Instantiate(enrageEffect, stat.chest.transform);
		currentEnrageEffect.transform.localScale = Vector3.one * 2f;
		currentEnrageEffect.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
		EnemySimplifier[] componentsInChildren = GetComponentsInChildren<EnemySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enraged = true;
		}
		eid.UpdateBuffs(visualsOnly: true);
		GameObject[] array = activateOnEnrage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		GetComponent<AudioSource>().Play(tracked: true);
	}

	public void CrazyReady()
	{
		inAction = false;
		inSemiAction = false;
		crazyMode = true;
		Invoke("CrazyShoot", 0.5f / eid.totalSpeedModifier);
		Invoke("CrazyShoot", 1.5f / eid.totalSpeedModifier);
	}

	public void CrazyShoot()
	{
		if (!dead)
		{
			ShootExplosive(crazyPoint);
			crazyPoint = ((crazyPoint == 0) ? 1 : 0);
			Invoke("CrazyShoot", Random.Range(2f, 3f) / (float)((difficulty < 4) ? 1 : 2) / eid.totalSpeedModifier);
		}
	}

	public override void OnGoLimp(bool fromExplosion)
	{
		dead = true;
		anim.speed = 0f;
		SwingCheck2[] componentsInChildren = stat.GetComponentsInChildren<SwingCheck2>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		if (currentEnrageEffect != null)
		{
			Object.Destroy(currentEnrageEffect);
		}
		isDead = true;
		stat.isMassDeath = true;
	}
}
