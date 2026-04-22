using System.Collections.Generic;
using Sandbox;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class SpiderBody : MonoBehaviour, IEnrage, IAlter, IAlterOptions<bool>
{
	private NavMeshAgent nma;

	private Quaternion targetRotation;

	public GameObject proj;

	private RaycastHit hit2;

	private bool readyToShoot = true;

	private float projectileBurstCooldown = 1f;

	private int maxBurst;

	private int currentBurst;

	public float health;

	public bool stationary;

	private Rigidbody rb;

	private bool falling;

	private Enemy enemy;

	private Transform firstChild;

	private CharacterJoint[] cjs;

	private CharacterJoint cj;

	public GameObject impactParticle;

	public GameObject impactSprite;

	private Quaternion spriteRot;

	private Vector3 spritePos;

	public Transform mouth;

	private GameObject currentProj;

	private bool chargingBeam;

	public GameObject chargeEffect;

	[HideInInspector]
	public GameObject currentChargeEffect;

	private float beamCharge;

	private AudioSource chargeEffectAudio;

	private Light chargeEffectLight;

	private Vector3 predictedPlayerPos;

	public GameObject spiderBeam;

	private GameObject currentBeam;

	public AssetReference beamExplosion;

	private GameObject currentExplosion;

	private float beamProbability;

	private Quaternion predictedRotation;

	private bool rotating;

	public GameObject dripBlood;

	private GameObject currentDrip;

	public AudioClip hurtSound;

	private StyleCalculator scalc;

	private EnemyIdentifier eid;

	public GameObject spark;

	private int difficulty;

	private float coolDownMultiplier = 1f;

	private int beamsAmount = 1;

	private float maxHealth;

	public GameObject enrageEffect;

	[HideInInspector]
	public GameObject currentEnrageEffect;

	private Material origMaterial;

	public Material woundedMaterial;

	public Material woundedEnrageMaterial;

	public GameObject woundedParticle;

	private bool parryable;

	private MusicManager muman;

	private bool requestedMusic;

	private GoreZone gz;

	[SerializeField]
	private Transform headModel;

	public GameObject breakParticle;

	private bool corpseBroken;

	public AssetReference shockwave;

	private EnemySimplifier[] ensims;

	public Renderer mainMesh;

	public float targetHeight = 1f;

	private float defaultHeight;

	[SerializeField]
	private Collider headCollider;

	private List<EnemyIdentifier> fallEnemiesHit = new List<EnemyIdentifier>();

	private int parryFramesLeft;

	private EnemyTarget target => eid.target;

	public string alterKey => "spider";

	public string alterCategoryName => "malicious face";

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

	public bool isEnraged { get; private set; }

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		nma = GetComponent<NavMeshAgent>();
		eid = GetComponent<EnemyIdentifier>();
	}

	private void Start()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		maxHealth = health;
		if (difficulty >= 3)
		{
			coolDownMultiplier = 1.25f;
		}
		else if (difficulty == 1)
		{
			coolDownMultiplier = 0.75f;
		}
		else if (difficulty == 0)
		{
			coolDownMultiplier = 0.5f;
		}
		if (difficulty >= 4)
		{
			maxBurst = 10;
		}
		else if (difficulty >= 2)
		{
			maxBurst = 5;
		}
		else
		{
			maxBurst = 2;
		}
		if (!mainMesh)
		{
			mainMesh = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		origMaterial = mainMesh.material;
		gz = GoreZone.ResolveGoreZone(base.transform.parent ? base.transform.parent : base.transform);
		if ((bool)(Object)(object)nma)
		{
			nma.updateRotation = false;
			if (stationary)
			{
				nma.speed = 0f;
			}
		}
		if ((bool)currentChargeEffect)
		{
			Object.Destroy(currentChargeEffect);
		}
		defaultHeight = targetHeight;
	}

	private void OnDisable()
	{
		if (!eid.dead)
		{
			requestedMusic = false;
			enemy.musicRequested = false;
			if (muman == null)
			{
				muman = MonoSingleton<MusicManager>.Instance;
			}
			if ((bool)muman)
			{
				muman.PlayCleanMusic();
			}
		}
	}

	private void Update()
	{
		if (enemy == null || target == null || eid.dead)
		{
			return;
		}
		if (!enemy.musicRequested)
		{
			enemy.musicRequested = true;
			muman = MonoSingleton<MusicManager>.Instance;
			muman.PlayBattleMusic();
		}
		SetHeadRotation();
		if (!isEnraged && difficulty > 2 && health < maxHealth / 2f)
		{
			Enrage();
		}
		if (chargingBeam)
		{
			BeamChargeUpdate();
		}
		else if (beamCharge == 0f)
		{
			if ((Object)(object)nma != null && !stationary)
			{
				MovementUpdate();
			}
			if (projectileBurstCooldown != 0f)
			{
				projectileBurstCooldown = Mathf.MoveTowards(projectileBurstCooldown, 0f, Time.deltaTime * coolDownMultiplier * eid.totalSpeedModifier);
			}
			if (currentBurst > maxBurst && projectileBurstCooldown == 0f)
			{
				currentBurst = 0;
				projectileBurstCooldown = ((difficulty != 0) ? 1 : 2);
			}
			if (readyToShoot && projectileBurstCooldown == 0f)
			{
				AttackCheck();
			}
		}
	}

	private void SetHeadRotation()
	{
		if (beamCharge >= 1f)
		{
			if (rotating)
			{
				headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRotation, Quaternion.Angle(headModel.transform.rotation, predictedRotation) * Time.deltaTime * 20f * eid.totalSpeedModifier);
				return;
			}
			predictedRotation = Quaternion.LookRotation(target.position - base.transform.position);
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, predictedRotation, (Quaternion.Angle(headModel.transform.rotation, predictedRotation) + 10f) * Time.deltaTime * 10f * eid.totalSpeedModifier);
		}
		else
		{
			targetRotation = Quaternion.LookRotation((target.headPosition - base.transform.position).normalized);
			headModel.transform.rotation = Quaternion.RotateTowards(headModel.transform.rotation, targetRotation, (Quaternion.Angle(headModel.transform.rotation, targetRotation) + 10f) * Time.deltaTime * 15f * eid.totalSpeedModifier);
		}
	}

	private void MovementUpdate()
	{
		if (!((Behaviour)(object)nma).enabled)
		{
			((Behaviour)(object)nma).enabled = true;
			if (nma.isOnNavMesh)
			{
				nma.isStopped = false;
			}
			nma.speed = 3.5f * eid.totalSpeedModifier;
		}
		if (!nma.isOnNavMesh)
		{
			return;
		}
		targetHeight = defaultHeight;
		if ((bool)eid.buffTargeter)
		{
			nma.SetDestination(target.position);
			if (Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) < 15f)
			{
				targetHeight = 0.35f;
			}
		}
		else
		{
			nma.SetDestination(target.position);
		}
		nma.baseOffset = Mathf.MoveTowards(nma.baseOffset, targetHeight, Time.deltaTime * defaultHeight / 2f * eid.totalSpeedModifier);
	}

	private void AttackCheck()
	{
		if (currentBurst != 0)
		{
			ShootProj();
		}
		else
		{
			if ((!(Quaternion.Angle(headModel.rotation, targetRotation) < 1f) && !(Vector3.Distance(base.transform.position, target.position) < 10f)) || Physics.Raycast(base.transform.position, target.position - base.transform.position, out var _, Vector3.Distance(base.transform.position, target.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				return;
			}
			bool num = beamProbability > 5f || Random.Range(0f, health * 0.4f) < beamProbability;
			bool flag = Vector3.Distance(base.transform.position, target.position) <= 50f || (bool)MonoSingleton<NewMovement>.Instance.ridingRocket;
			bool flag2 = (bool)eid.buffTargeter && Vector3.Distance(base.transform.position, eid.buffTargeter.transform.position) <= 15f;
			if (num && flag && !flag2)
			{
				ChargeBeam();
				if (difficulty > 2 && isEnraged)
				{
					beamsAmount = 2;
				}
				beamProbability = ((!(health > 10f)) ? 1 : 0);
			}
			else
			{
				ShootProj();
				beamProbability += 1f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (parryFramesLeft > 0)
		{
			parryFramesLeft--;
		}
	}

	public void GetHurt(GameObject target, Vector3 force, Vector3 hitPoint, float multiplier, GameObject sourceWeapon = null)
	{
		bool dead = false;
		float num = health;
		bool flag = true;
		if (hitPoint == Vector3.zero)
		{
			hitPoint = target.transform.position;
		}
		flag = MonoSingleton<BloodsplatterManager>.Instance.goreOn;
		if (eid == null)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (eid.hitter != "fire")
		{
			if (!eid.sandified && !eid.blessed)
			{
				GameObject gameObject = Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid), hitPoint, Quaternion.identity);
				if ((bool)gameObject)
				{
					Bloodsplatter component = gameObject.GetComponent<Bloodsplatter>();
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
					if (eid.hitter == "drill")
					{
						gameObject.transform.localScale *= 2f;
					}
					if (health > 0f)
					{
						component.GetReady();
					}
					if (eid.hitter == "nail")
					{
						component.hpAmount = 3;
						AudioSource component2 = component.GetComponent<AudioSource>();
						component2.volume *= 0.8f;
					}
					else if (multiplier >= 1f)
					{
						component.hpAmount = 30;
					}
					if (flag)
					{
						gameObject.GetComponent<ParticleSystem>().Play();
					}
				}
				if (eid.hitter != "shotgun" && eid.hitter != "drill" && base.gameObject.activeInHierarchy)
				{
					if (dripBlood != null)
					{
						currentDrip = Object.Instantiate(dripBlood, hitPoint, Quaternion.identity);
					}
					if ((bool)currentDrip)
					{
						currentDrip.transform.parent = base.transform;
						currentDrip.transform.LookAt(base.transform);
						currentDrip.transform.Rotate(180f, 180f, 180f);
						if (flag)
						{
							currentDrip.GetComponent<ParticleSystem>().Play();
						}
					}
				}
			}
			else
			{
				Object.Instantiate(MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, eid), hitPoint, Quaternion.identity);
			}
		}
		if (!eid.dead)
		{
			if (!eid.blessed && !InvincibleEnemies.Enabled)
			{
				health -= 1f * multiplier;
			}
			if (scalc == null)
			{
				scalc = MonoSingleton<StyleCalculator>.Instance;
			}
			if (health <= 0f)
			{
				dead = true;
			}
			if (((eid.hitter == "shotgunzone" || eid.hitter == "hammerzone") && parryable) || eid.hitter == "punch")
			{
				if (parryable)
				{
					parryable = false;
					MonoSingleton<FistControl>.Instance.currentPunch.Parry(hook: false, eid);
					currentExplosion = Object.Instantiate(beamExplosion.ToAsset(), base.transform.position, Quaternion.identity);
					if (!InvincibleEnemies.Enabled && !eid.blessed)
					{
						health -= (float)((parryFramesLeft > 0) ? 4 : 5) / eid.totalHealthModifier;
					}
					Explosion[] componentsInChildren = currentExplosion.GetComponentsInChildren<Explosion>();
					foreach (Explosion obj in componentsInChildren)
					{
						obj.speed *= eid.totalDamageModifier;
						obj.maxSize *= 1.75f * eid.totalDamageModifier;
						obj.damage = Mathf.RoundToInt(50f * eid.totalDamageModifier);
						obj.canHit = AffectedSubjects.EnemiesOnly;
						obj.friendlyFire = true;
					}
					if (currentEnrageEffect == null)
					{
						CancelInvoke("BeamFire");
						Invoke("StopWaiting", 1f);
						Object.Destroy(currentChargeEffect);
					}
					parryFramesLeft = 0;
				}
				else
				{
					parryFramesLeft = MonoSingleton<FistControl>.Instance.currentPunch.activeFrames;
				}
			}
			if (multiplier != 0f)
			{
				scalc.HitCalculator(eid.hitter, "spider", "", dead, eid, sourceWeapon);
			}
			if (num >= maxHealth / 2f && health < maxHealth / 2f)
			{
				if (ensims == null || ensims.Length == 0)
				{
					ensims = GetComponentsInChildren<EnemySimplifier>();
				}
				Object.Instantiate(woundedParticle, base.transform.position, Quaternion.identity);
				if (!eid.puppet)
				{
					EnemySimplifier[] array = ensims;
					foreach (EnemySimplifier enemySimplifier in array)
					{
						if (!enemySimplifier.ignoreCustomColor)
						{
							enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, woundedMaterial);
							enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, woundedEnrageMaterial);
						}
					}
				}
			}
			if ((bool)(Object)(object)hurtSound && num > 0f)
			{
				hurtSound.PlayClipAtPoint(MonoSingleton<AudioMixerController>.Instance.goreGroup, base.transform.position, 12, 1f, 0.75f, Random.Range(0.85f, 1.35f), (AudioRolloffMode)1);
			}
			if (health <= 0f && !eid.dead)
			{
				Die();
			}
		}
		else if (eid.hitter == "ground slam")
		{
			BreakCorpse();
		}
	}

	public void Die()
	{
		rb = GetComponentInChildren<Rigidbody>();
		DoubleRender[] componentsInChildren = GetComponentsInChildren<DoubleRender>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RemoveEffect();
		}
		falling = true;
		parryable = false;
		rb.isKinematic = false;
		rb.SetGravityMode(useGravity: true);
		if (health > 0f)
		{
			health = 0f;
		}
		base.gameObject.layer = 11;
		ResolveStuckness();
		for (int j = 1; j < base.transform.parent.childCount - 1; j++)
		{
			Object.Destroy(base.transform.parent.GetChild(j).gameObject);
		}
		if (currentChargeEffect != null)
		{
			Object.Destroy(currentChargeEffect);
		}
		Object.Destroy((Object)(object)nma);
		if (muman == null)
		{
			muman = MonoSingleton<MusicManager>.Instance;
		}
		muman.PlayCleanMusic();
		enemy.musicRequested = false;
		EnemySimplifier[] array;
		if (currentEnrageEffect != null)
		{
			mainMesh.material = origMaterial;
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].material = origMaterial;
			}
			array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			Object.Destroy(currentEnrageEffect);
		}
		if (ensims == null)
		{
			ensims = GetComponentsInChildren<EnemySimplifier>();
		}
		array = ensims;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Begone();
		}
		if (eid.hitter == "ground slam" || eid.hitter == "breaker")
		{
			BreakCorpse();
		}
	}

	private void ShootProj()
	{
		if (target == null)
		{
			return;
		}
		currentProj = Object.Instantiate(proj, mouth.position, headModel.transform.rotation);
		currentProj.transform.rotation = Quaternion.LookRotation(target.headPosition - mouth.position);
		if (difficulty >= 4)
		{
			switch (currentBurst % 5)
			{
			case 1:
				currentProj.transform.LookAt(target.headPosition + base.transform.right * (1 + currentBurst / 5 * 2));
				break;
			case 2:
				currentProj.transform.LookAt(target.headPosition + base.transform.up * (1 + currentBurst / 5 * 2));
				break;
			case 3:
				currentProj.transform.LookAt(target.headPosition - base.transform.right * (1 + currentBurst / 5 * 2));
				break;
			case 4:
				currentProj.transform.LookAt(target.headPosition - base.transform.up * (1 + currentBurst / 5 * 2));
				break;
			}
		}
		currentBurst++;
		Projectile component = currentProj.GetComponent<Projectile>();
		component.safeEnemyType = EnemyType.MaliciousFace;
		component.target = eid.target;
		switch (difficulty)
		{
		case 3:
		case 4:
		case 5:
			component.speed *= 1.25f;
			break;
		case 1:
			component.speed *= 0.75f;
			break;
		case 0:
			component.speed *= 0.5f;
			break;
		}
		component.damage *= eid.totalDamageModifier;
		readyToShoot = false;
		float num = 0.1f;
		if (difficulty >= 4)
		{
			num = 0.05f;
		}
		else if (difficulty == 0)
		{
			num = 0.2f;
		}
		Invoke("ReadyToShoot", num / eid.totalSpeedModifier);
	}

	private void ChargeBeam()
	{
		chargingBeam = true;
		currentChargeEffect = Object.Instantiate(chargeEffect, mouth);
		currentChargeEffect.transform.localScale = Vector3.zero;
		chargeEffectAudio = currentChargeEffect.GetComponent<AudioSource>();
		chargeEffectLight = currentChargeEffect.GetComponent<Light>();
	}

	private void BeamChargeUpdate()
	{
		nma.speed = 0f;
		if (nma.isOnNavMesh)
		{
			nma.SetDestination(base.transform.position);
			nma.isStopped = true;
		}
		float num = ((difficulty >= 4) ? 1.5f : 1f);
		beamCharge = Mathf.MoveTowards(beamCharge, 1f, 0.5f * coolDownMultiplier * num * Time.deltaTime * eid.totalSpeedModifier);
		currentChargeEffect.transform.localScale = Vector3.one * beamCharge * 2.5f;
		chargeEffectAudio.SetPitch(beamCharge * 2f);
		chargeEffectLight.intensity = beamCharge * 30f;
		if (beamCharge == 1f)
		{
			chargingBeam = false;
			BeamChargeEnd();
		}
	}

	private void BeamChargeEnd()
	{
		if (beamsAmount <= 1 && (bool)(Object)(object)chargeEffectAudio)
		{
			chargeEffectAudio.Stop();
		}
		if (target != null)
		{
			if ((bool)(Object)(object)nma)
			{
				((Behaviour)(object)nma).enabled = false;
			}
			PredictPlayerPosition();
			predictedRotation = Quaternion.LookRotation(predictedPlayerPos - base.transform.position);
			rotating = true;
			Object.Instantiate(spark, mouth.position, mouth.rotation).transform.LookAt(predictedPlayerPos);
			if (difficulty > 1)
			{
				Invoke("BeamFire", 0.5f / eid.totalSpeedModifier);
			}
			else if (difficulty == 1)
			{
				Invoke("BeamFire", 0.75f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("BeamFire", 1f / eid.totalSpeedModifier);
			}
			parryable = true;
			if (parryFramesLeft > 0)
			{
				eid.hitter = "punch";
				eid.DeliverDamage(base.gameObject, MonoSingleton<CameraController>.Instance.transform.forward * 25000f, base.transform.position, 1f, tryForExplode: false);
			}
		}
	}

	private void PredictPlayerPosition()
	{
		Vector3 velocity = target.GetVelocity();
		Vector3 vector = velocity;
		Vector3 vector2 = ((eid.target.isPlayer && (bool)MonoSingleton<NewMovement>.Instance.ridingRocket) ? MonoSingleton<NewMovement>.Instance.ridingRocket.transform.position : target.position);
		if (!eid.target.isPlayer || (bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			vector.y /= 2f;
		}
		predictedPlayerPos = vector2 + vector * 0.5f / eid.totalSpeedModifier;
		RaycastHit hitInfo2;
		if (velocity.magnitude > 1f && headCollider.Raycast(new Ray(target.position, velocity.normalized), out var _, velocity.magnitude * 0.5f / eid.totalSpeedModifier))
		{
			predictedPlayerPos = target.position;
		}
		else if (Physics.Raycast(target.position, predictedPlayerPos - target.position, out hitInfo2, Vector3.Distance(predictedPlayerPos, target.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
		{
			predictedPlayerPos = hitInfo2.point;
		}
	}

	private void BeamFire()
	{
		parryable = false;
		if (!eid.dead)
		{
			currentBeam = Object.Instantiate(spiderBeam, mouth.position, mouth.rotation);
			rotating = false;
			if (eid.totalDamageModifier != 1f && currentBeam.TryGetComponent<RevolverBeam>(out var component))
			{
				component.damage *= eid.totalDamageModifier;
			}
			if (beamsAmount > 1)
			{
				beamsAmount--;
				chargeEffectAudio.SetPitch(4f);
				chargeEffectAudio.volume = 1f;
				Invoke("BeamChargeEnd", 0.5f / eid.totalSpeedModifier);
			}
			else
			{
				Object.Destroy(currentChargeEffect);
				Invoke("StopWaiting", 1f / eid.totalSpeedModifier);
			}
		}
	}

	private void StopWaiting()
	{
		if (!eid.dead)
		{
			beamCharge = 0f;
		}
	}

	private void ReadyToShoot()
	{
		readyToShoot = true;
	}

	public void TriggerHit(Collider other)
	{
		if (!falling)
		{
			return;
		}
		EnemyIdentifier enemyIdentifier = other.gameObject.GetComponent<EnemyIdentifier>();
		if (enemyIdentifier == null)
		{
			EnemyIdentifierIdentifier component = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if (component != null && component.eid != null)
			{
				enemyIdentifier = component.eid;
			}
		}
		if (enemyIdentifier == null && other.gameObject.TryGetComponent<IdolMauricer>(out var _))
		{
			enemyIdentifier = other.gameObject.GetComponentInParent<EnemyIdentifier>();
		}
		if ((bool)enemyIdentifier && enemyIdentifier != eid && !fallEnemiesHit.Contains(enemyIdentifier))
		{
			FallKillEnemy(enemyIdentifier);
		}
	}

	private void FallKillEnemy(EnemyIdentifier targetEid)
	{
		if ((bool)MonoSingleton<StyleHUD>.Instance && !targetEid.dead)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.mauriced", null, eid);
		}
		targetEid.hitter = "maurice";
		fallEnemiesHit.Add(targetEid);
		if (targetEid.TryGetComponent<Collider>(out var component))
		{
			Physics.IgnoreCollision(headCollider, component, ignore: true);
		}
		EnemyIdentifier.FallOnEnemy(targetEid);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!falling)
		{
			return;
		}
		if (other.gameObject.CompareTag("Moving"))
		{
			BreakCorpse();
			MonoSingleton<CameraController>.Instance.CameraShake(2f);
		}
		else
		{
			if (!LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				return;
			}
			Breakable component4;
			if (other.gameObject.CompareTag("Floor"))
			{
				rb.isKinematic = true;
				rb.SetGravityMode(useGravity: false);
				Transform transform = base.transform;
				Object.Instantiate(impactParticle, transform.position, transform.rotation);
				spriteRot.eulerAngles = new Vector3(other.contacts[0].normal.x + 90f, other.contacts[0].normal.y, other.contacts[0].normal.z);
				spritePos = new Vector3(other.contacts[0].point.x, other.contacts[0].point.y + 0.1f, other.contacts[0].point.z);
				AudioSource componentInChildren = Object.Instantiate(shockwave.ToAsset(), spritePos, Quaternion.identity).GetComponentInChildren<AudioSource>();
				if ((bool)(Object)(object)componentInChildren)
				{
					Object.Destroy((Object)(object)componentInChildren);
				}
				Transform transform2 = base.transform;
				transform2.position -= transform2.up * 1.5f;
				falling = false;
				if (!other.gameObject.TryGetComponent<MaliciousFaceCatcher>(out var _))
				{
					Object.Instantiate(impactSprite, spritePos, spriteRot).transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				if (TryGetComponent<SphereCollider>(out var component2))
				{
					Object.Destroy(component2);
				}
				SpiderBodyTrigger componentInChildren2 = base.transform.parent.GetComponentInChildren<SpiderBodyTrigger>(includeInactive: true);
				if ((bool)componentInChildren2)
				{
					Object.Destroy(componentInChildren2.gameObject);
				}
				((Behaviour)(object)rb.GetComponent<NavMeshObstacle>()).enabled = true;
				MonoSingleton<CameraController>.Instance.CameraShake(2f);
				if (fallEnemiesHit.Count <= 0)
				{
					return;
				}
				foreach (EnemyIdentifier item in fallEnemiesHit)
				{
					if (item != null && !item.dead && item.TryGetComponent<Collider>(out var component3))
					{
						Physics.IgnoreCollision(headCollider, component3, ignore: false);
					}
				}
				fallEnemiesHit.Clear();
			}
			else if (other.gameObject.TryGetComponent<Breakable>(out component4) && !component4.playerOnly && !component4.specialCaseOnly)
			{
				component4.Break();
			}
		}
	}

	public void BreakCorpse()
	{
		if (!corpseBroken)
		{
			corpseBroken = true;
			if (breakParticle != null)
			{
				Transform transform = base.transform;
				Object.Instantiate(breakParticle, transform.position, transform.rotation).transform.SetParent(gz.gibZone);
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void ResolveStuckness()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			SphereCollider component = GetComponent<SphereCollider>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Physics.ComputePenetration(component, base.transform.position, base.transform.rotation, collider, collider.transform.position, collider.transform.rotation, out var direction, out var distance);
				base.transform.position = base.transform.position + direction * (distance + 0.5f);
			}
		}
		array = Physics.OverlapSphere(base.transform.position, 2f, LayerMaskDefaults.Get(LMD.Environment));
		if (array != null && array.Length != 0)
		{
			BreakCorpse();
		}
	}

	public void Enrage()
	{
		if (!eid.dead && !isEnraged)
		{
			isEnraged = true;
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = true;
			}
			currentEnrageEffect = Object.Instantiate(enrageEffect, base.transform);
			currentEnrageEffect.transform.localScale = Vector3.one * 0.2f;
		}
	}

	public void UnEnrage()
	{
		if (!eid.dead && isEnraged)
		{
			isEnraged = false;
			if (ensims == null || ensims.Length == 0)
			{
				ensims = GetComponentsInChildren<EnemySimplifier>();
			}
			EnemySimplifier[] array = ensims;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enraged = false;
			}
			if (currentEnrageEffect != null)
			{
				Object.Destroy(currentEnrageEffect);
			}
		}
	}
}
