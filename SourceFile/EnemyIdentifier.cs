using System;
using System.Collections;
using System.Collections.Generic;
using DebugOverlays;
using plog;
using plog.Models;
using Sandbox;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-500)]
public class EnemyIdentifier : MonoBehaviour, IAlter, IAlterOptions<bool>, IEnemyHealthDetails
{
	private static readonly Logger Log = new Logger("EnemyIdentifier");

	[HideInInspector]
	public Enemy zombie;

	[HideInInspector]
	public MaliciousFace spider;

	[HideInInspector]
	public Enemy machine;

	[HideInInspector]
	public Enemy statue;

	[HideInInspector]
	public Wicked wicked;

	[HideInInspector]
	public Drone drone;

	[HideInInspector]
	public Idol idol;

	[HideInInspector]
	public Deathcatcher dcatch;

	public EnemyClass enemyClass;

	public EnemyType enemyType;

	public bool spawnIn;

	public GameObject spawnEffect;

	public float health;

	[HideInInspector]
	public string hitter;

	[HideInInspector]
	public List<HitterAttribute> hitterAttributes = new List<HitterAttribute>();

	[HideInInspector]
	public List<string> hitterWeapons = new List<string>();

	public string[] weaknesses;

	public float[] weaknessMultipliers;

	public float totalDamageTakenMultiplier = 1f;

	public GameObject weakPoint;

	public Transform overrideCenter;

	[HideInInspector]
	public bool exploded;

	public bool stationary;

	public bool dead;

	[HideInInspector]
	public DoorController usingDoor;

	public bool ignoredByEnemies;

	private EnemyIdentifierIdentifier[] limbs;

	[HideInInspector]
	public int nailsAmount;

	[HideInInspector]
	public List<Nail> nails = new List<Nail>();

	public bool useBrakes;

	public bool canWallsplat;

	public bool bigEnemy;

	public bool unbounceable;

	public bool poise;

	public bool immuneToFriendlyFire;

	[HideInInspector]
	public bool beingZapped;

	[HideInInspector]
	public bool hasBeenZapped;

	[HideInInspector]
	public TimeSince lastZapped;

	[HideInInspector]
	public bool pulledByMagnet;

	[HideInInspector]
	public List<Magnet> stuckMagnets = new List<Magnet>();

	[HideInInspector]
	public List<Harpoon> drillers = new List<Harpoon>();

	[HideInInspector]
	public bool underwater;

	[HideInInspector]
	public HashSet<Water> touchingWaters = new HashSet<Water>();

	[HideInInspector]
	public bool checkingSpawnStatus = true;

	public bool flying;

	public bool dontCountAsKills;

	public bool dontUnlockBestiary;

	public bool specialOob;

	public GameObject[] activateOnDeath;

	public UnityEvent onDeath;

	public UltrakillEvent onEnable;

	private BloodsplatterManager bsm;

	[HideInInspector]
	public GroundCheckEnemy gce;

	[HideInInspector]
	public GoreZone gz;

	private Rigidbody rb;

	private RigidbodyConstraints rbc;

	private List<GameObject> sandifiedParticles = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> blessingGlows = new List<GameObject>();

	[HideInInspector]
	public EnemyIdentifier buffTargeter;

	public int difficultyOverride = -1;

	private int difficulty = -1;

	[HideInInspector]
	public bool hooked;

	public List<Flammable> burners;

	[HideInInspector]
	public List<Flammable> flammables;

	private bool getFireDamageMultiplier;

	[HideInInspector]
	public bool beenGasolined;

	public bool overrideFlamableSize;

	public Vector3 flamableSize = Vector3.zero;

	[HideInInspector]
	public bool harpooned;

	[HideInInspector]
	public Zapper zapperer;

	private GameObject afterShockSourceWeapon;

	private bool waterOnlyAftershock;

	private bool afterShockFromZap;

	[Header("Modifiers")]
	public bool hookIgnore;

	public bool sandified;

	public bool blessed;

	public bool puppet;

	private bool permaPuppet;

	public bool mirrorOnly;

	private int blessings;

	private float puppetSpawnTimer;

	[HideInInspector]
	public Vector3 squishedScale;

	[HideInInspector]
	public Vector3 originalScale;

	private List<Renderer> puppetRenderers = new List<Renderer>();

	private bool puppetSpawnIgnoringPlayer;

	private Collider[] puppetSpawnColliders;

	public float radianceTier = 1f;

	public bool healthBuff;

	public float healthBuffModifier = 1.5f;

	[HideInInspector]
	public int healthBuffRequests;

	public bool speedBuff;

	public float speedBuffModifier = 1.5f;

	[HideInInspector]
	public int speedBuffRequests;

	public bool damageBuff;

	public float damageBuffModifier = 1.5f;

	[HideInInspector]
	public int damageBuffRequests;

	[HideInInspector]
	public bool hasRadianceEffected;

	[HideInInspector]
	public float totalSpeedModifier = 1f;

	[HideInInspector]
	public float totalDamageModifier = 1f;

	[HideInInspector]
	public float totalHealthModifier = 1f;

	[HideInInspector]
	public bool isBoss;

	[Space(10f)]
	public List<Renderer> buffUnaffectedRenderers = new List<Renderer>();

	[SerializeField]
	private string overrideFullName;

	[Header("Relationships")]
	public bool ignorePlayer;

	public bool attackEnemies;

	public EnemyTarget target;

	public bool prioritizePlayerOverFallback = true;

	public bool prioritizeEnemiesUnlessAttacked;

	public Transform fallbackTarget;

	[HideInInspector]
	public bool madness;

	[HideInInspector]
	public TimeSince timeSinceSpawned;

	private TimeSince? timeSinceNoTarget;

	[HideInInspector]
	public EnemyScanner enemyScanner;

	private IEnemyRelationshipLogic[] relationshipLogic;

	private EnemyIdentifierDebugOverlay debugOverlay;

	private BossHealthBar cheatCreatedBossBar;

	[HideInInspector]
	public List<GameObject> destroyOnDeath = new List<GameObject>();

	private static readonly int HasSandBuff = Shader.PropertyToID("_HasSandBuff");

	private static readonly int NewSanded = Shader.PropertyToID("_Sanded");

	public Coroutine AfterShockRoutine;

	public Transform bodyTransform
	{
		get
		{
			if (!overrideCenter)
			{
				return base.transform;
			}
			return overrideCenter;
		}
	}

	[HideInInspector]
	public bool isGasolined
	{
		get
		{
			foreach (Flammable flammable in flammables)
			{
				if (flammable.fuel > 0f)
				{
					return true;
				}
			}
			return false;
		}
	}

	private bool IsSandboxEnemy
	{
		get
		{
			if (TryGetComponent<EnemySpawnableInstance>(out var component))
			{
				return component.sourceObject != null;
			}
			return false;
		}
	}

	public bool IsCurrentTargetFallback
	{
		get
		{
			if (target != null && fallbackTarget != null)
			{
				return target.trackedTransform == fallbackTarget;
			}
			return false;
		}
	}

	public string FullName
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideFullName))
			{
				return overrideFullName;
			}
			return EnemyTypes.GetEnemyName(enemyType);
		}
	}

	public float Health => health;

	public bool Dead => dead;

	public bool Blessed => blessed;

	public bool AttackEnemies
	{
		get
		{
			if (BlindEnemies.Blind)
			{
				return false;
			}
			if (EnemiesHateEnemies.Active)
			{
				return true;
			}
			if (madness)
			{
				return true;
			}
			IEnemyRelationshipLogic[] array = relationshipLogic;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ShouldAttackEnemies())
				{
					return true;
				}
			}
			return attackEnemies;
		}
	}

	public bool IgnorePlayer
	{
		get
		{
			if (BlindEnemies.Blind)
			{
				return true;
			}
			if (EnemyIgnorePlayer.Active)
			{
				return true;
			}
			if (madness)
			{
				return true;
			}
			IEnemyRelationshipLogic[] array = relationshipLogic;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ShouldIgnorePlayer())
				{
					return true;
				}
			}
			return ignorePlayer;
		}
	}

	public string alterKey => "enemy-identifier";

	public string alterCategoryName => "enemy";

	public AlterOption<bool>[] options => new AlterOption<bool>[5]
	{
		new AlterOption<bool>
		{
			name = "Boss Health Bar",
			key = "health-bar",
			callback = delegate(bool value)
			{
				BossBar(value);
			},
			value = (GetComponent<BossHealthBar>() != null)
		},
		new AlterOption<bool>
		{
			name = "Sandified",
			key = "sandified",
			callback = ((!puppet) ? ((Action<bool>)delegate(bool value)
			{
				if (value)
				{
					Sandify();
				}
				else
				{
					Desandify();
				}
			}) : null),
			value = sandified
		},
		new AlterOption<bool>
		{
			name = "Puppeted",
			key = "puppeted",
			callback = ((!puppet || (IsSandboxEnemy && !permaPuppet)) ? ((Action<bool>)delegate(bool value)
			{
				if (value && !puppet)
				{
					PuppetSpawn();
				}
				if (!value && puppet && IsSandboxEnemy)
				{
					TryUnPuppet();
				}
			}) : null),
			tooltip = (permaPuppet ? "This enemy cannot be un-puppeteered " : ((puppet && !IsSandboxEnemy) ? "Un-puppeteering is not supported for non-sandbox enemies" : null)),
			value = puppet
		},
		new AlterOption<bool>
		{
			name = "Ignore Player",
			key = "ignorePlayer",
			callback = delegate(bool value)
			{
				ignorePlayer = value;
			},
			value = ignorePlayer
		},
		new AlterOption<bool>
		{
			name = "Attack Enemies",
			key = "attackEnemies",
			callback = delegate(bool value)
			{
				attackEnemies = value;
			},
			value = attackEnemies
		}
	};

	private void Awake()
	{
		if (puppet)
		{
			permaPuppet = true;
		}
		health = 999f;
		InitializeReferences();
		ForceGetHealth();
		UpdateModifiers();
		if (StockMapInfo.Instance != null && StockMapInfo.Instance.forceUpdateEnemyRenderers)
		{
			SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].updateWhenOffscreen = true;
			}
		}
		bsm = MonoSingleton<BloodsplatterManager>.Instance;
	}

	private void OnEnable()
	{
		onEnable?.Invoke();
		hasBeenZapped = false;
		lastZapped = 0f;
	}

	private void OnDisable()
	{
		onEnable?.Revert();
	}

	private void InitializeReferences()
	{
		if (enemyType == EnemyType.Idol)
		{
			if (!idol)
			{
				idol = (idol ? idol : GetComponent<Idol>());
			}
			ignoredByEnemies = true;
		}
		if (enemyType == EnemyType.Deathcatcher)
		{
			if (!dcatch)
			{
				dcatch = (dcatch ? dcatch : GetComponent<Deathcatcher>());
			}
			ignoredByEnemies = true;
		}
		relationshipLogic = GetComponents<IEnemyRelationshipLogic>();
		rb = GetComponent<Rigidbody>();
		gce = GetComponentInChildren<GroundCheckEnemy>(includeInactive: true);
		Flammable[] componentsInChildren = GetComponentsInChildren<Flammable>();
		foreach (Flammable flammable in componentsInChildren)
		{
			if (!flammables.Contains(flammable))
			{
				flammables.Add(flammable);
			}
			if (!flammable.fuelOnly)
			{
				getFireDamageMultiplier = true;
			}
		}
	}

	public bool DestroyLimb(Transform limb, LimbDestroyType type = LimbDestroyType.Destroy)
	{
		if (puppet)
		{
			type = LimbDestroyType.Destroy;
		}
		if (!limb.TryGetComponent<CharacterJoint>(out var component))
		{
			return false;
		}
		UnityEngine.Object.Destroy(component);
		GoreZone goreZone = GetGoreZone();
		if (type == LimbDestroyType.Detach)
		{
			if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && limb.TryGetComponent<EnemyIdentifierIdentifier>(out var component2))
			{
				component2.SetupForHellBath();
			}
			limb.transform.parent = goreZone.transform;
		}
		if (type == LimbDestroyType.LimbGibs)
		{
			CharacterJoint[] componentsInChildren = limb.GetComponentsInChildren<CharacterJoint>();
			foreach (CharacterJoint characterJoint in componentsInChildren)
			{
				if (StockMapInfo.Instance.removeGibsWithoutAbsorbers && characterJoint.TryGetComponent<EnemyIdentifierIdentifier>(out var component3))
				{
					component3.SetupForHellBath();
				}
				UnityEngine.Object.Destroy(characterJoint);
				characterJoint.transform.parent = goreZone.transform;
			}
		}
		if (type == LimbDestroyType.Destroy)
		{
			CharacterJoint[] componentsInChildren = limb.GetComponentsInChildren<CharacterJoint>();
			foreach (CharacterJoint characterJoint2 in componentsInChildren)
			{
				if (characterJoint2.TryGetComponent<Collider>(out var component4))
				{
					UnityEngine.Object.Destroy(characterJoint2);
					if ((bool)component4.attachedRigidbody)
					{
						UnityEngine.Object.Destroy(component4.attachedRigidbody);
					}
					UnityEngine.Object.Destroy(component4);
				}
				else
				{
					UnityEngine.Object.Destroy(characterJoint2);
				}
				characterJoint2.transform.localScale = Vector3.zero;
				characterJoint2.gameObject.SetActive(value: false);
			}
		}
		if (type != LimbDestroyType.Detach)
		{
			limb.localScale = Vector3.zero;
			limb.gameObject.SetActive(value: false);
		}
		return true;
	}

	public bool IsTypeFriendly(EnemyIdentifier owner)
	{
		if (enemyType == owner.enemyType)
		{
			return true;
		}
		if (enemyClass == EnemyClass.Husk && owner.enemyClass == EnemyClass.Husk)
		{
			return true;
		}
		return false;
	}

	private GoreZone GetGoreZone()
	{
		if ((bool)gz)
		{
			return gz;
		}
		Transform parent = base.transform;
		if (((bool)spider || (bool)statue) && base.transform.parent != null)
		{
			parent = base.transform.parent;
		}
		gz = GoreZone.ResolveGoreZone(parent);
		return gz;
	}

	public void ForceGetHealth()
	{
		if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue || enemyType == EnemyType.Providence)
		{
			if (!drone)
			{
				drone = GetComponent<Drone>();
			}
			if ((bool)drone && (bool)drone.Enemy)
			{
				health = drone.Enemy.health;
			}
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (!spider)
			{
				spider = GetComponent<MaliciousFace>();
			}
			if ((bool)spider)
			{
				health = spider.spider.health;
			}
			return;
		}
		switch (enemyClass)
		{
		case EnemyClass.Husk:
			if (!zombie)
			{
				zombie = GetComponent<Enemy>();
			}
			if ((bool)zombie)
			{
				health = zombie.health;
			}
			break;
		case EnemyClass.Demon:
			if (!statue)
			{
				statue = GetComponent<Enemy>();
			}
			if ((bool)statue)
			{
				health = statue.health;
			}
			break;
		case EnemyClass.Machine:
			if (!machine)
			{
				machine = GetComponent<Enemy>();
			}
			if ((bool)machine)
			{
				health = machine.health;
			}
			break;
		}
	}

	private void Start()
	{
		if (OptionsManager.forcePuppet)
		{
			puppet = true;
		}
		if (!dontUnlockBestiary)
		{
			MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType, 1);
		}
		if (checkingSpawnStatus)
		{
			if (!dead)
			{
				if (OptionsManager.forceBossBars)
				{
					BossBar(enable: true);
				}
				if (puppet)
				{
					PuppetSpawn();
				}
				if ((sandified || OptionsManager.forceSand) && enemyType != EnemyType.Stalker)
				{
					Sandify(ignorePrevious: true);
				}
				if (blessed)
				{
					Bless(ignorePrevious: true);
				}
				if (speedBuff || damageBuff || healthBuff || OptionsManager.forceRadiance)
				{
					if (speedBuff)
					{
						speedBuffRequests++;
					}
					if (damageBuff)
					{
						damageBuffRequests++;
					}
					if (healthBuff)
					{
						healthBuffRequests++;
					}
					UpdateBuffs(visualsOnly: false, spawnIn);
				}
			}
			checkingSpawnStatus = false;
		}
		if (spawnIn && (bool)spawnEffect && !puppet)
		{
			spawnEffect.SetActive(value: true);
			spawnIn = false;
			timeSinceSpawned = 0f;
		}
		if (!MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies().Contains(this))
		{
			MonoSingleton<EnemyTracker>.Instance.AddEnemy(this);
		}
		gz = GetGoreZone();
		if ((bool)MonoSingleton<MarkedForDeath>.Instance && MonoSingleton<MarkedForDeath>.Instance.gameObject.activeInHierarchy)
		{
			PlayerMarkedForDeath();
		}
		isBoss = GetComponentInChildren<BossIdentifier>() != null;
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(this);
		}
		UpdateTarget();
		SlowUpdate();
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 1f);
		if (drillers.Count <= 0)
		{
			return;
		}
		for (int num = drillers.Count - 1; num >= 0; num--)
		{
			if (drillers[num] == null || !drillers[num].gameObject.activeInHierarchy)
			{
				drillers.RemoveAt(num);
			}
		}
	}

	private void Update()
	{
		UpdateTarget();
		UpdateEnemyScanner();
		ForceGetHealth();
		UpdateModifiers();
		UpdateDebugStuff();
		if (!puppet)
		{
			return;
		}
		if (puppetSpawnTimer < 1f)
		{
			puppetSpawnTimer = Mathf.MoveTowards(puppetSpawnTimer, 1f, Time.deltaTime * 2f * Mathf.Max(1f - puppetSpawnTimer, 0.001f));
			base.transform.localScale = Vector3.Lerp(squishedScale, originalScale, puppetSpawnTimer);
			squishedScale = new Vector3(Mathf.MoveTowards(squishedScale.x, originalScale.x * puppetSpawnTimer, Time.deltaTime * 4f), squishedScale.y, Mathf.MoveTowards(squishedScale.z, originalScale.z * puppetSpawnTimer, Time.deltaTime * 4f));
			if (puppetSpawnIgnoringPlayer && puppetSpawnTimer > 0.75f)
			{
				puppetSpawnIgnoringPlayer = false;
				Collider[] array = puppetSpawnColliders;
				for (int i = 0; i < array.Length; i++)
				{
					Physics.IgnoreCollision(array[i], MonoSingleton<NewMovement>.Instance.playerCollider, ignore: false);
				}
				if ((bool)rb)
				{
					rb.constraints = rbc;
				}
			}
			foreach (GameObject blessingGlow in blessingGlows)
			{
				Collider componentInParent = blessingGlow.GetComponentInParent<Collider>();
				if ((bool)componentInParent)
				{
					blessingGlow.transform.localScale = componentInParent.bounds.size;
				}
			}
			UpdateBuffs();
		}
		foreach (Renderer puppetRenderer in puppetRenderers)
		{
			if (puppetRenderer != null)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				puppetRenderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat("_VertexNoiseAmplitude", Mathf.Lerp(10f, 1f, Mathf.Max(0f, puppetSpawnTimer - 0.5f) * 2f));
				materialPropertyBlock.SetColor("_FlowDirection", new Color(UnityEngine.Random.Range(0f, 1f), 0.2f, UnityEngine.Random.Range(0f, 1f)));
				puppetRenderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	private void UpdateEnemyScanner()
	{
		if (!AttackEnemies)
		{
			enemyScanner?.Reset();
			return;
		}
		if (enemyScanner == null)
		{
			enemyScanner = new EnemyScanner(this);
		}
		enemyScanner.Update();
	}

	private void UpdateDebugStuff()
	{
		if (EnemyIdentifierDebug.Active)
		{
			if (debugOverlay == null)
			{
				debugOverlay = base.gameObject.AddComponent<EnemyIdentifierDebugOverlay>();
			}
			debugOverlay.ConsumeData(enemyType, enemyClass, dead, IgnorePlayer, AttackEnemies, target);
		}
		else if (debugOverlay != null)
		{
			UnityEngine.Object.Destroy(debugOverlay);
		}
		if (ForceBossBars.Active)
		{
			if (!GetComponent<BossHealthBar>())
			{
				cheatCreatedBossBar = base.gameObject.AddComponent<BossHealthBar>();
			}
		}
		else if (cheatCreatedBossBar != null)
		{
			UnityEngine.Object.Destroy(cheatCreatedBossBar);
		}
	}

	private bool HandleTargetCheats()
	{
		if (BlindEnemies.Blind)
		{
			target = null;
			return true;
		}
		if (EnemyIgnorePlayer.Active && target != null && target.isPlayer)
		{
			target = null;
		}
		return false;
	}

	public void UpdateTarget()
	{
		if (target != null && !target.isValid)
		{
			if (target.trackedTransform == fallbackTarget)
			{
				fallbackTarget = null;
			}
			target = null;
		}
		if (timeSinceNoTarget.HasValue && target != null)
		{
			timeSinceNoTarget = null;
		}
		if (!timeSinceNoTarget.HasValue && target == null)
		{
			timeSinceNoTarget = 0f;
		}
		if (HandleTargetCheats())
		{
			return;
		}
		if (!IgnorePlayer)
		{
			bool flag = fallbackTarget == null;
			flag |= prioritizePlayerOverFallback && (target == null || target.trackedTransform == fallbackTarget);
			if (!timeSinceNoTarget.HasValue || (float?)timeSinceNoTarget < 1.5f)
			{
				if (AttackEnemies && prioritizeEnemiesUnlessAttacked)
				{
					flag = false;
				}
				if (enemyType == EnemyType.Stalker)
				{
					flag = false;
				}
			}
			if (flag)
			{
				target = EnemyTarget.TrackPlayer();
			}
		}
		else if (target != null && target.isPlayer)
		{
			target = null;
		}
		if (target == null && fallbackTarget != null)
		{
			EnemyTarget enemyTarget = new EnemyTarget(fallbackTarget);
			if (enemyTarget.isValid)
			{
				target = enemyTarget;
			}
			else
			{
				fallbackTarget = null;
			}
		}
	}

	public void SetFallbackTarget(GameObject target)
	{
		fallbackTarget = target.transform;
	}

	public void SetOverrideCenter(Transform center)
	{
		overrideCenter = center;
	}

	public void ResetTarget()
	{
		target = null;
	}

	private void UpdateModifiers()
	{
		totalSpeedModifier = 1f;
		totalHealthModifier = 1f;
		totalDamageModifier = 1f;
		float num = Mathf.Max(OptionsManager.radianceTier, radianceTier);
		if (speedBuff || OptionsManager.forceRadiance)
		{
			totalSpeedModifier *= speedBuffModifier * ((num > 1f) ? (0.75f + num / 4f) : num);
		}
		if (healthBuff || OptionsManager.forceRadiance)
		{
			totalHealthModifier *= healthBuffModifier * ((num > 1f) ? (0.75f + num / 4f) : num);
		}
		if (damageBuff || OptionsManager.forceRadiance)
		{
			totalDamageModifier *= damageBuffModifier;
		}
		if (puppet)
		{
			totalHealthModifier /= 2f;
			totalSpeedModifier *= Mathf.Lerp(0.01f, Mathf.Max(0.01f, puppetSpawnTimer - 0.75f) * 3f, puppetSpawnTimer);
		}
	}

	public void StartBurning(float heat)
	{
		foreach (Flammable flammable in flammables)
		{
			flammable.Burn(heat);
		}
	}

	public void Burn()
	{
		CancelInvoke("Burn");
		if (burners.Count == 0)
		{
			return;
		}
		for (int num = burners.Count - 1; num >= 0; num--)
		{
			if (burners[num] == null || !burners[num].burning)
			{
				burners.RemoveAt(num);
			}
			else
			{
				burners[num].Pulse();
				if (difficulty >= 2 && mirrorOnly && !(burners[num].currentFire == null) && burners[num].currentFire.layer != 30)
				{
					SendToPortalLayer(burners[num].currentFire);
				}
			}
		}
		if (burners.Count == 0)
		{
			return;
		}
		Invoke("Burn", 0.5f);
		if (!dead)
		{
			TryIgniteGasoline();
		}
		float num2 = 0f;
		foreach (Flammable flammable in flammables)
		{
			num2 = Mathf.Max(num2, flammable.fuel);
		}
		hitter = "fire";
		DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, (num2 > 0f) ? 0.5f : 0.2f, tryForExplode: false);
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!buffUnaffectedRenderers.Contains(renderer) && !(renderer is ParticleSystemRenderer))
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat("_OiledAmount", num2 / 5f);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	private void TryIgniteGasoline()
	{
		MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(base.transform.position);
	}

	public void DeliverDamage(GameObject target, Vector3 force, Vector3 hitPoint, float multiplier, bool tryForExplode, float critMultiplier = 0f, GameObject sourceWeapon = null, bool ignoreTotalDamageTakenMultiplier = false, bool fromExplosion = false)
	{
		if (EnemyIdentifierDebug.Active)
		{
			Log.Fine("Delivering damage to: " + base.gameObject.name + ", Damage:" + multiplier, (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (target == base.gameObject)
		{
			EnemyIdentifierIdentifier componentInChildren = GetComponentInChildren<EnemyIdentifierIdentifier>();
			if (componentInChildren != null)
			{
				target = componentInChildren.gameObject;
			}
		}
		if ((bool)sourceWeapon)
		{
			if (prioritizeEnemiesUnlessAttacked)
			{
				prioritizeEnemiesUnlessAttacked = false;
			}
			if (!IgnorePlayer && ((this.target != null && !this.target.isPlayer) || this.target == null))
			{
				this.target = EnemyTarget.TrackPlayer();
				HandleTargetCheats();
			}
		}
		if (!ignoreTotalDamageTakenMultiplier)
		{
			multiplier *= totalDamageTakenMultiplier;
		}
		multiplier /= totalHealthModifier;
		if (isBoss && difficulty >= 4)
		{
			multiplier = ((difficulty != 5) ? (multiplier / 1.5f) : (multiplier / 2f));
		}
		if (weaknesses.Length != 0)
		{
			for (int i = 0; i < weaknesses.Length; i++)
			{
				if (hitter == weaknesses[i] || (hitterAttributes.Contains(HitterAttribute.Electricity) && weaknesses[i] == "electricity"))
				{
					multiplier *= weaknessMultipliers[i];
				}
			}
		}
		if (getFireDamageMultiplier && burners.Count > 0 && hitter != "fire" && hitter != "explosion" && hitter != "ffexplosion")
		{
			multiplier *= 1.5f;
		}
		if (nails.Count > 10)
		{
			for (int j = 0; j < nails.Count - 10; j++)
			{
				if (nails[j] != null)
				{
					UnityEngine.Object.Destroy(nails[j].gameObject);
				}
				nails.RemoveAt(j);
			}
		}
		if (!beingZapped && hitterAttributes.Contains(HitterAttribute.Electricity) && hitter != "aftershock" && (nailsAmount > 0 || stuckMagnets.Count > 0 || touchingWaters.Count > 0))
		{
			beingZapped = true;
			foreach (Nail nail in nails)
			{
				if (nail != null)
				{
					nail.Zap();
				}
			}
			if (hitter == "zapper" && multiplier > health)
			{
				multiplier = health - 0.001f;
			}
			afterShockSourceWeapon = sourceWeapon;
			waterOnlyAftershock = nailsAmount == 0 && stuckMagnets.Count == 0;
			if (hitter == "zap")
			{
				afterShockFromZap = true;
			}
			AfterShockRoutine = StartCoroutine(DelayedAfterShock(0.5f, overrideCenter ? overrideCenter.position : base.transform.position));
		}
		if (pulledByMagnet && hitter != "deathzone")
		{
			pulledByMagnet = false;
		}
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.MaliciousFace:
		{
			if (spider == null)
			{
				spider = GetComponent<MaliciousFace>();
			}
			if (spider == null)
			{
				return;
			}
			bool flag2 = dead;
			if ((hitter != "explosion" && hitter != "ffexplosion") || isGasolined)
			{
				spider.spider.GetHurt(target, force, multiplier, critMultiplier, hitPoint, sourceWeapon);
			}
			if (spider.spider.health <= 0f && !flag2)
			{
				ProcessDeath(fromExplosion: false);
			}
			health = spider.spider.health;
			flag = true;
			break;
		}
		case EnemyType.Wicked:
			if (wicked == null)
			{
				wicked = GetComponent<Wicked>();
			}
			if (wicked == null)
			{
				return;
			}
			wicked.GetHit();
			flag = true;
			break;
		case EnemyType.Drone:
		case EnemyType.Virtue:
		case EnemyType.Providence:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			if (drone == null)
			{
				return;
			}
			drone.GetHurt(force, multiplier, sourceWeapon, fromExplosion);
			health = drone.Enemy.health;
			if (health <= 0f)
			{
				Death();
			}
			flag = true;
			break;
		case EnemyType.Idol:
			idol = (idol ? idol : GetComponent<Idol>());
			if (HitterCanKillIdol(hitter))
			{
				idol?.Death();
			}
			break;
		case EnemyType.Deathcatcher:
			dcatch = (dcatch ? dcatch : GetComponent<Deathcatcher>());
			if (HitterCanKillIdol(hitter) && (bool)dcatch)
			{
				if (dcatch.active)
				{
					dcatch.Death();
				}
				else
				{
					UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, base.transform.position, Quaternion.identity);
				}
			}
			break;
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
			{
				if (!zombie)
				{
					zombie = GetComponent<Enemy>();
				}
				if (!zombie)
				{
					return;
				}
				Enemy enemy2 = zombie;
				GameObject obj2 = target;
				float multiplier3 = multiplier;
				GameObject sourceWeapon2 = sourceWeapon;
				bool fromExplosion2 = fromExplosion;
				enemy2.GetHurt(obj2, force, multiplier3, critMultiplier, default(Vector3), sourceWeapon2, fromExplosion2);
				if (tryForExplode && zombie.health <= 0f && !exploded)
				{
					Explode(fromExplosion);
				}
				if (zombie.health <= 0f)
				{
					Death();
				}
				health = zombie.health;
				break;
			}
			case EnemyClass.Machine:
			{
				if (machine == null)
				{
					machine = GetComponent<Enemy>();
				}
				if (machine == null)
				{
					return;
				}
				Enemy enemy = machine;
				GameObject obj = target;
				float multiplier2 = multiplier;
				GameObject sourceWeapon2 = sourceWeapon;
				bool fromExplosion2 = fromExplosion;
				enemy.GetHurt(obj, force, multiplier2, critMultiplier, default(Vector3), sourceWeapon2, fromExplosion2);
				if (tryForExplode && machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f) && !machine.dontDie && !exploded)
				{
					Explode(fromExplosion);
				}
				if (machine.health <= 0f && (machine.symbiote == null || machine.symbiote.health <= 0f))
				{
					Death();
				}
				health = machine.health;
				break;
			}
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Enemy>();
				}
				if (statue == null)
				{
					return;
				}
				statue.GetHurt(target, force, multiplier, critMultiplier, hitPoint, sourceWeapon, fromExplosion);
				if (tryForExplode && statue.health <= 0f && !exploded)
				{
					Explode(fromExplosion);
				}
				if (statue.health <= 0f)
				{
					Death();
				}
				health = statue.health;
				break;
			}
		}
		hitterAttributes.Clear();
	}

	public IEnumerator DelayedAfterShock(float delay, Vector3 pos)
	{
		float extraDamage = Mathf.Min(6f, (float)nailsAmount / 15f);
		afterShockFromZap = false;
		if (stuckMagnets.Count > 0)
		{
			extraDamage += (float)stuckMagnets.Count;
			foreach (Magnet stuckMagnet in stuckMagnets)
			{
				if (stuckMagnet != null)
				{
					stuckMagnet.health -= 1f;
				}
			}
		}
		if (extraDamage < 1f && touchingWaters.Count > 0)
		{
			extraDamage = 1f;
		}
		yield return new WaitForSeconds(delay);
		AfterShock(pos, extraDamage);
	}

	private void AfterShock(Vector3 pos, float extraDamage)
	{
		GoreZone goreZone = GetGoreZone();
		foreach (Nail nail in nails)
		{
			if (!nail)
			{
				continue;
			}
			GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Small, this);
			if ((bool)gore && (bool)goreZone)
			{
				gore.transform.position = nail.transform.position;
				gore.SetActive(value: true);
				Bloodsplatter component = gore.GetComponent<Bloodsplatter>();
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
				if ((bool)component && !dead)
				{
					component.GetReady();
				}
			}
			UnityEngine.Object.Destroy(nail.gameObject);
		}
		List<GameObject> list = new List<GameObject>();
		list.Add(base.gameObject);
		Zap(pos, extraDamage, list, afterShockSourceWeapon, this, null, waterOnlyAftershock);
		nails.Clear();
		nailsAmount = 0;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		if (!dead && !puppet)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(Mathf.Max(1, Mathf.RoundToInt(extraDamage * 15f)), "<color=#00ffffff>CONDUCTOR</color>", afterShockSourceWeapon, this);
		}
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i] == null) && componentsInChildren[i].gameObject != base.gameObject)
				{
					hitter = "aftershock";
					hitterAttributes.Add(HitterAttribute.Electricity);
					DeliverDamage(componentsInChildren[i].gameObject, Vector3.zero, base.transform.position, extraDamage, tryForExplode: true, 0f, afterShockSourceWeapon);
					break;
				}
			}
		}
		beingZapped = false;
		MonoSingleton<CameraController>.Instance.CameraShake(1f);
	}

	public static void Zap(Vector3 position, float damage = 2f, List<GameObject> alreadyHitObjects = null, GameObject sourceWeapon = null, EnemyIdentifier sourceEid = null, Water sourceWater = null, bool waterOnly = false)
	{
		bool flag = false;
		if ((bool)sourceWater && sourceWater.isPlayerTouchingWater)
		{
			flag = true;
		}
		else if ((bool)sourceEid && sourceEid.touchingWaters.Count > 0)
		{
			foreach (Water touchingWater in sourceEid.touchingWaters)
			{
				if ((bool)touchingWater && touchingWater.isPlayerTouchingWater)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			MonoSingleton<NewMovement>.Instance.GetHurt(50, invincible: true, 1f, explosion: false, instablack: false, 1f);
			LineRenderer lineRenderer = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, MonoSingleton<NewMovement>.Instance.transform.position, 0.5f), Quaternion.identity);
			lineRenderer.SetPosition(0, position);
			lineRenderer.SetPosition(1, MonoSingleton<NewMovement>.Instance.transform.position);
			UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.zapImpactParticle, MonoSingleton<NewMovement>.Instance.transform.position, Quaternion.identity);
		}
		foreach (EnemyIdentifier currentEnemy in MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies())
		{
			if (alreadyHitObjects != null && alreadyHitObjects.Contains(currentEnemy.gameObject))
			{
				continue;
			}
			bool flag2 = false;
			if (currentEnemy.flying && ((!sourceWater && !sourceEid) || currentEnemy.touchingWaters.Count == 0))
			{
				continue;
			}
			if (currentEnemy.touchingWaters.Count > 0)
			{
				if (sourceWater != null)
				{
					flag2 = currentEnemy.touchingWaters.Contains(sourceWater);
				}
				if (!flag2 && sourceEid != null && sourceEid.touchingWaters.Count > 0)
				{
					foreach (Water touchingWater2 in currentEnemy.touchingWaters)
					{
						if (!(touchingWater2 == null) && sourceEid.touchingWaters.Contains(touchingWater2))
						{
							flag2 = true;
							break;
						}
					}
				}
				if (currentEnemy.flying && !flag2)
				{
					continue;
				}
			}
			Vector3 vector = (currentEnemy.overrideCenter ? currentEnemy.overrideCenter.position : currentEnemy.transform.position);
			if ((flag2 && (!waterOnly || !currentEnemy.hasBeenZapped || (float)currentEnemy.lastZapped > 1f)) || (!waterOnly && (Vector3.Distance(position, vector) < 30f || (position.y > vector.y && position.y - vector.y < 60f && Vector3.Distance(position, new Vector3(vector.x, position.y, vector.z)) < 30f)) && !Physics.Raycast(position, vector - position, Vector3.Distance(position, vector), LayerMaskDefaults.Get(LMD.Environment))))
			{
				currentEnemy.hitter = "zap";
				currentEnemy.hitterAttributes.Add(HitterAttribute.Electricity);
				float multiplier = Mathf.Max((currentEnemy.hasBeenZapped && (float)currentEnemy.lastZapped < 5f) ? 0.5f : 2f, damage);
				currentEnemy.DeliverDamage(currentEnemy.gameObject, Vector3.zero, vector, multiplier, tryForExplode: true, 0f, sourceWeapon);
				currentEnemy.lastZapped = 0f;
				currentEnemy.hasBeenZapped = true;
				LineRenderer lineRenderer2 = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, vector, 0.5f), Quaternion.identity);
				lineRenderer2.SetPosition(0, position);
				lineRenderer2.SetPosition(1, vector);
				UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.zapImpactParticle, vector, Quaternion.identity);
			}
		}
		if (waterOnly)
		{
			return;
		}
		foreach (Magnet magnet in MonoSingleton<ObjectTracker>.Instance.magnetList)
		{
			if (!(magnet == null) && !alreadyHitObjects.Contains(magnet.gameObject) && (!(magnet.onEnemy != null) || !alreadyHitObjects.Contains(magnet.onEnemy.gameObject)) && Vector3.Distance(position, magnet.transform.position) < 30f && !Physics.Raycast(position, magnet.transform.position - position, Vector3.Distance(position, magnet.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				magnet.StartCoroutine(magnet.Zap(alreadyHitObjects, Mathf.Max(0.5f, damage), sourceWeapon));
				LineRenderer lineRenderer3 = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, magnet.transform.position, 0.5f), Quaternion.identity);
				lineRenderer3.SetPosition(0, position);
				lineRenderer3.SetPosition(1, magnet.transform.position);
			}
		}
		foreach (Zappable zappables in MonoSingleton<ObjectTracker>.Instance.zappablesList)
		{
			if ((bool)zappables && !alreadyHitObjects.Contains(zappables.gameObject) && Vector3.Distance(position, zappables.transform.position) < 30f && (!Physics.Raycast(position, zappables.transform.position - position, out var hitInfo, Vector3.Distance(position, zappables.transform.position), LayerMaskDefaults.Get(LMD.Environment)) || hitInfo.transform.gameObject == zappables.gameObject))
			{
				zappables.StartCoroutine(zappables.Zap(alreadyHitObjects, Mathf.Max(0.5f, damage), sourceWeapon));
				LineRenderer lineRenderer4 = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.electricLine, Vector3.Lerp(position, zappables.transform.position, 0.5f), Quaternion.identity);
				lineRenderer4.SetPosition(0, position);
				lineRenderer4.SetPosition(1, zappables.transform.position);
			}
		}
	}

	public void Death()
	{
		Death(fromExplosion: false);
	}

	public void Death(bool fromExplosion)
	{
		if (!dead)
		{
			ProcessDeath(fromExplosion);
		}
	}

	public void ProcessDeath(bool fromExplosion)
	{
		dead = true;
		GameObject[] array = activateOnDeath;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
		foreach (GameObject item in destroyOnDeath)
		{
			if ((bool)item)
			{
				UnityEngine.Object.Destroy(item);
			}
		}
		onDeath?.Invoke();
		if ((bool)MonoSingleton<EnemyTracker>.Instance)
		{
			MonoSingleton<EnemyTracker>.Instance.RemoveEnemy(this);
		}
		if (TryGetComponent<Enemy>(out var component))
		{
			component.CountDeath();
		}
		else if (!dontCountAsKills)
		{
			if (enemyType == EnemyType.Deathcatcher || enemyType == EnemyType.Idol)
			{
				MonoSingleton<StatsManager>.Instance.kills++;
			}
			GetGoreZone().AddDeath();
		}
		if (enemyType == EnemyType.Deathcatcher && TryGetComponent<Deathcatcher>(out var component2))
		{
			component2.Death();
		}
		if (!puppet)
		{
			if (hitterWeapons.Count > 1)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.arsenal", null, this);
			}
			if (!dontUnlockBestiary)
			{
				MonoSingleton<BestiaryData>.Instance.SetEnemy(enemyType);
				if (TryGetComponent<UnlockBestiary>(out var component3))
				{
					MonoSingleton<BestiaryData>.Instance.SetEnemy(component3.enemy);
				}
			}
		}
		if (health > 0f)
		{
			health = 0f;
		}
		DestroyMagnets();
		if (drillers.Count > 0 && enemyType != EnemyType.MaliciousFace && enemyType != EnemyType.Gutterman && enemyType != EnemyType.Mindflayer)
		{
			foreach (Harpoon driller in drillers)
			{
				driller.DelayedDestroyIfOnCorpse();
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
		Desandify(visualOnly: true);
		Unbless();
		if (puppet)
		{
			EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			GameObject gameObject2 = null;
			EnemyIdentifierIdentifier[] array2 = componentsInChildren;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array2)
			{
				GoreType got = GoreType.Body;
				if (enemyType == EnemyType.HideousMass || enemyIdentifierIdentifier.CompareTag("Head") || enemyIdentifierIdentifier.CompareTag("EndLimb"))
				{
					got = GoreType.Head;
				}
				else if (enemyIdentifierIdentifier.CompareTag("Limb"))
				{
					got = GoreType.Limb;
				}
				gameObject2 = bsm.GetGore(got, this, fromExplosion);
				gameObject2.transform.position = enemyIdentifierIdentifier.transform.position;
				if (gz != null && gz.goreZone != null)
				{
					gameObject2.transform.SetParent(gz.goreZone, worldPositionStays: true);
				}
				gameObject2.GetComponent<Bloodsplatter>()?.GetReady();
				UnityEngine.Object.Destroy(enemyIdentifierIdentifier.gameObject);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (beingZapped)
		{
			StopCoroutine(AfterShockRoutine);
			AfterShockRoutine = StartCoroutine(DelayedAfterShock(0.1f, overrideCenter ? overrideCenter.position : base.transform.position));
		}
	}

	public void DestroyMagnets()
	{
		if (stuckMagnets.Count <= 0)
		{
			return;
		}
		for (int num = stuckMagnets.Count - 1; num >= 0; num--)
		{
			if (stuckMagnets[num] != null)
			{
				UnityEngine.Object.Destroy(stuckMagnets[num].gameObject);
			}
		}
	}

	public void InstaKill()
	{
		if (dead)
		{
			return;
		}
		Death();
		if (pulledByMagnet && !puppet)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(240, "ultrakill.catapulted", null, this);
		}
		dead = true;
		bool flag = false;
		switch (enemyType)
		{
		case EnemyType.Mandalore:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.Death();
			flag = true;
			break;
		case EnemyType.Drone:
		case EnemyType.Virtue:
		case EnemyType.Providence:
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.GetHurt(Vector3.zero, 999f);
			drone.Explode();
			flag = true;
			break;
		case EnemyType.MaliciousFace:
			if (spider == null)
			{
				spider = GetComponent<MaliciousFace>();
			}
			spider.ProcessDeath();
			flag = true;
			break;
		case EnemyType.Idol:
		{
			if (TryGetComponent<Idol>(out var component2))
			{
				component2.Death();
			}
			break;
		}
		case EnemyType.Deathcatcher:
		{
			if (TryGetComponent<Deathcatcher>(out var component))
			{
				component.Death();
			}
			break;
		}
		}
		if (!flag)
		{
			switch (enemyClass)
			{
			case EnemyClass.Husk:
				if (zombie == null)
				{
					zombie = GetComponent<Enemy>();
				}
				if (!zombie.limp)
				{
					zombie.GoLimp();
				}
				break;
			case EnemyClass.Machine:
				if (machine == null)
				{
					machine = GetComponent<Enemy>();
				}
				if (!machine.limp)
				{
					machine.GoLimp();
				}
				break;
			case EnemyClass.Demon:
				if (statue == null)
				{
					statue = GetComponent<Enemy>();
				}
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				break;
			}
		}
		if (usingDoor != null)
		{
			usingDoor.Close();
			usingDoor = null;
		}
	}

	public void Explode(bool fromExplosion = false)
	{
		bool flag = dead;
		if (!dead)
		{
			Death();
		}
		if (!flag && enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<MaliciousFace>();
			}
			hitter = "breaker";
			spider.ProcessDeath();
		}
		else if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue || enemyType == EnemyType.Providence)
		{
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.Explode();
		}
		else if (enemyClass == EnemyClass.Husk)
		{
			if (zombie == null)
			{
				zombie = GetComponent<Enemy>();
			}
			if (exploded || !zombie || zombie.chestExploding)
			{
				return;
			}
			exploded = true;
			if (zombie.chestExploding)
			{
				zombie.ChestExplodeEnd();
			}
			if (!flag && pulledByMagnet && !puppet)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(240, "ultrakill.catapulted", null, this);
			}
			EnemyIdentifierIdentifier[] componentsInChildren = zombie.GetComponentsInChildren<EnemyIdentifierIdentifier>(includeInactive: true);
			GetGoreZone();
			bool flag2 = false;
			EnemyIdentifierIdentifier[] array = componentsInChildren;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
			{
				if (enemyIdentifierIdentifier.gameObject.CompareTag("Limb"))
				{
					DestroyLimb(enemyIdentifierIdentifier.transform, LimbDestroyType.Detach);
					if (!flag2)
					{
						zombie.GetHurt(enemyIdentifierIdentifier.gameObject, (base.transform.position - enemyIdentifierIdentifier.transform.position).normalized * 1000f, 1E+09f, 1f);
					}
				}
				else if (enemyIdentifierIdentifier.gameObject.CompareTag("Head") || enemyIdentifierIdentifier.gameObject.CompareTag("EndLimb"))
				{
					flag2 = true;
					zombie.GetHurt(enemyIdentifierIdentifier.gameObject, (base.transform.position - enemyIdentifierIdentifier.transform.position).normalized * 1000f, 1E+09f, 1f);
				}
			}
			if (!flag2)
			{
				zombie.GoLimp();
			}
			health = zombie.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else if (enemyClass == EnemyClass.Machine && enemyType != EnemyType.Drone)
		{
			if (machine == null)
			{
				machine = GetComponent<Enemy>();
			}
			if (exploded || !machine)
			{
				return;
			}
			exploded = true;
			bool flag3 = false;
			if (machine.dismemberment)
			{
				Collider[] componentsInChildren2 = machine.GetComponentsInChildren<Collider>();
				List<EnemyIdentifierIdentifier> list = new List<EnemyIdentifierIdentifier>();
				Collider[] array2 = componentsInChildren2;
				for (int i = 0; i < array2.Length; i++)
				{
					EnemyIdentifierIdentifier component = array2[i].GetComponent<EnemyIdentifierIdentifier>();
					if (component != null)
					{
						list.Add(component);
					}
				}
				GetGoreZone();
				foreach (EnemyIdentifierIdentifier item in list)
				{
					if (item.gameObject.CompareTag("Limb"))
					{
						DestroyLimb(item.transform, LimbDestroyType.Detach);
					}
					else if (item.gameObject.CompareTag("Head") || item.gameObject.CompareTag("EndLimb"))
					{
						flag3 = true;
						machine.GetHurt(item.gameObject, (base.transform.position - item.transform.position).normalized * 1000f, 999f, 1f);
					}
				}
			}
			if (!flag3)
			{
				machine.GoLimp(fromExplosion);
			}
			health = machine.health;
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		else
		{
			if (enemyClass != EnemyClass.Demon)
			{
				return;
			}
			if (statue == null)
			{
				statue = GetComponent<Enemy>();
			}
			if (!exploded)
			{
				exploded = true;
				if (!statue.limp)
				{
					statue.GoLimp();
				}
				health = statue.health;
			}
		}
	}

	public void Splatter(bool styleBonus = true)
	{
		if (InvincibleEnemies.Enabled || blessed)
		{
			return;
		}
		if (enemyType == EnemyType.MaliciousFace)
		{
			if (spider == null)
			{
				spider = GetComponent<MaliciousFace>();
			}
			if (!dead)
			{
				bool num = dead;
				Death();
				if (!num)
				{
					hitter = "breaker";
					spider.ProcessDeath();
				}
			}
			else
			{
				spider.BreakCorpse();
			}
			return;
		}
		if (enemyType == EnemyType.Drone || enemyType == EnemyType.Virtue || enemyType == EnemyType.Providence)
		{
			if (drone == null)
			{
				drone = GetComponent<Drone>();
			}
			drone.GetHurt(Vector3.zero, 999f);
			if (enemyType == EnemyType.Virtue)
			{
				drone.Explode();
			}
			Death();
			return;
		}
		switch (enemyClass)
		{
		case EnemyClass.Husk:
			if (zombie == null)
			{
				zombie = GetComponent<Enemy>();
			}
			break;
		case EnemyClass.Demon:
			if (statue == null)
			{
				statue = GetComponent<Enemy>();
			}
			break;
		case EnemyClass.Machine:
			if (machine == null)
			{
				machine = GetComponent<Enemy>();
			}
			break;
		}
		bool flag = dead;
		if (enemyClass == EnemyClass.Machine && (bool)machine && (!machine.dismemberment || enemyType == EnemyType.Power))
		{
			InstaKill();
		}
		else if (enemyClass == EnemyClass.Demon && (bool)statue && (statue.isMassDeath || statue.specialDeath))
		{
			InstaKill();
		}
		else if (!exploded && (enemyClass != EnemyClass.Husk || !zombie.chestExploding))
		{
			exploded = true;
			limbs = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			if (!flag)
			{
				SendMessage("GoLimp", SendMessageOptions.DontRequireReceiver);
				StyleHUD instance = MonoSingleton<StyleHUD>.Instance;
				if (!puppet)
				{
					if (pulledByMagnet)
					{
						instance.AddPoints(120, "ultrakill.catapulted", null, this);
					}
					if (styleBonus)
					{
						instance.AddPoints(100, "ultrakill.splattered", null, this);
					}
				}
				base.transform.Rotate(new Vector3(90f, 0f, 0f));
			}
			GameObject gore = bsm.GetGore(GoreType.Splatter, this);
			gore.transform.position = base.transform.position + Vector3.up;
			GoreZone goreZone = GetGoreZone();
			if (goreZone != null && goreZone.goreZone != null)
			{
				gore.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
			}
			gore.GetComponent<Bloodsplatter>()?.GetReady();
			EnemyIdentifierIdentifier[] array = limbs;
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
			{
				if (enemyIdentifierIdentifier.gameObject.CompareTag("Body") || enemyIdentifierIdentifier.gameObject.CompareTag("Limb") || enemyIdentifierIdentifier.gameObject.CompareTag("Head") || enemyIdentifierIdentifier.gameObject.CompareTag("EndLimb"))
				{
					UnityEngine.Object.Destroy(enemyIdentifierIdentifier.GetComponent<CharacterJoint>());
					enemyIdentifierIdentifier.transform.SetParent(GetGoreZone().gibZone, worldPositionStays: true);
					Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
					if (component != null)
					{
						component.velocity = Vector3.zero;
						enemyIdentifierIdentifier.transform.position = new Vector3(enemyIdentifierIdentifier.transform.position.x, base.transform.position.y + 0.1f, enemyIdentifierIdentifier.transform.position.z);
						Vector3 vector = new Vector3(base.transform.position.x - enemyIdentifierIdentifier.transform.position.x, 0f, base.transform.position.z - enemyIdentifierIdentifier.transform.position.z);
						component.AddForce(vector * 15f, ForceMode.VelocityChange);
						component.constraints = RigidbodyConstraints.FreezePositionY;
					}
				}
			}
			if ((bool)machine && enemyType == EnemyType.Streetcleaner)
			{
				machine.sc.CanisterExplosion();
			}
			Invoke("StopSplatter", 1f);
			if (usingDoor != null)
			{
				usingDoor.Close();
				usingDoor = null;
			}
		}
		Death();
	}

	public void StopSplatter()
	{
		EnemyIdentifierIdentifier[] array = limbs;
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in array)
		{
			if (enemyIdentifierIdentifier != null)
			{
				Rigidbody component = enemyIdentifierIdentifier.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.constraints = RigidbodyConstraints.None;
				}
			}
		}
	}

	public void Sandify(bool ignorePrevious = false)
	{
		if (dead || (!ignorePrevious && sandified))
		{
			return;
		}
		sandified = true;
		if (puppet)
		{
			InstaKill();
			return;
		}
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandDrip, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			sandifiedParticles.Add(gameObject);
		}
		Collider component2 = GetComponent<Collider>();
		if ((bool)component2)
		{
			UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, component2.bounds.center, Quaternion.identity);
		}
		else
		{
			UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.sandificationEffect, base.transform.position, Quaternion.identity);
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren2)
		{
			if (!buffUnaffectedRenderers.Contains(renderer))
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat(HasSandBuff, 1f);
				materialPropertyBlock.SetFloat(NewSanded, 1f);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	public void Desandify(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			sandified = false;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (!buffUnaffectedRenderers.Contains(renderer))
			{
				renderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat(HasSandBuff, 0f);
				materialPropertyBlock.SetFloat(NewSanded, 0f);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
		foreach (GameObject sandifiedParticle in sandifiedParticles)
		{
			UnityEngine.Object.Destroy(sandifiedParticle);
		}
		sandifiedParticles.Clear();
	}

	public void Bless(bool ignorePrevious = false)
	{
		if (!ignorePrevious)
		{
			blessings++;
			if (blessings > 1)
			{
				return;
			}
		}
		if (!ignorePrevious && blessed)
		{
			return;
		}
		blessed = true;
		EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
		foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.blessingGlow, enemyIdentifierIdentifier.transform.position, enemyIdentifierIdentifier.transform.rotation);
			Collider component = enemyIdentifierIdentifier.GetComponent<Collider>();
			if ((bool)component)
			{
				gameObject.transform.localScale = component.bounds.size;
			}
			gameObject.transform.SetParent(enemyIdentifierIdentifier.transform, worldPositionStays: true);
			blessingGlows.Add(gameObject);
		}
		if (burners == null || burners.Count <= 0)
		{
			return;
		}
		foreach (Flammable burner in burners)
		{
			burner.PutOut(getWet: false);
		}
		burners.Clear();
	}

	public void Unbless(bool visualOnly = false)
	{
		if (!visualOnly)
		{
			if (blessings <= 0)
			{
				return;
			}
			blessings--;
			if (blessings < 0)
			{
				blessings = 0;
			}
			if (blessings > 0)
			{
				return;
			}
			blessed = false;
		}
		foreach (GameObject blessingGlow in blessingGlows)
		{
			UnityEngine.Object.Destroy(blessingGlow);
		}
		blessingGlows.Clear();
		if (!visualOnly)
		{
			MonoSingleton<EnemyTracker>.Instance.UpdateIdolsNow();
		}
	}

	public void AddFlammable(float amount)
	{
		if (!beenGasolined)
		{
			beenGasolined = true;
			EnemyIdentifierIdentifier[] componentsInChildren = GetComponentsInChildren<EnemyIdentifierIdentifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i].gameObject == base.gameObject) && !(componentsInChildren[i].eid != this))
				{
					if (!componentsInChildren[i].TryGetComponent<Flammable>(out var component))
					{
						component = componentsInChildren[i].gameObject.AddComponent<Flammable>();
						component.fuelOnly = true;
						component.useOverrideSize = overrideFlamableSize;
						component.overrideSize = flamableSize;
					}
					if (!flammables.Contains(component))
					{
						flammables.Add(component);
					}
				}
			}
		}
		float num = 0f;
		foreach (Flammable flammable in flammables)
		{
			if (flammable.fuel < 5f)
			{
				flammable.fuel += Mathf.Min(amount, 5f - flammable.fuel);
			}
			num = Mathf.Max(num, flammable.fuel);
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren2)
		{
			if (!buffUnaffectedRenderers.Contains(renderer) && !(renderer is ParticleSystemRenderer))
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetFloat("_OiledAmount", num / 5f);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	public void PuppetSpawn()
	{
		if (dead)
		{
			return;
		}
		dontCountAsKills = true;
		puppet = true;
		if (sandified && enemyType != EnemyType.Stalker)
		{
			InstaKill();
			return;
		}
		puppetSpawnTimer = 0f;
		SpawnEffect componentInChildren = GetComponentInChildren<SpawnEffect>();
		if ((bool)componentInChildren)
		{
			componentInChildren.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(componentInChildren.gameObject);
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Material material = new Material(MonoSingleton<DefaultReferenceManager>.Instance.puppetMaterial);
		EnemySimplifier[] componentsInChildren2 = GetComponentsInChildren<EnemySimplifier>();
		foreach (EnemySimplifier enemySimplifier in componentsInChildren2)
		{
			enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, material);
			enemySimplifier.ChangeMaterialNew(EnemySimplifier.MaterialState.enraged, material);
			enemySimplifier.ignoreCustomColor = true;
			if ((bool)enemySimplifier.simplifiedMaterial)
			{
				Material material2 = new Material(enemySimplifier.simplifiedMaterial);
				material2.color = Color.red;
				enemySimplifier.simplifiedMaterial = material2;
				if ((bool)enemySimplifier.simplifiedMaterial2)
				{
					enemySimplifier.simplifiedMaterial2 = material2;
				}
				if ((bool)enemySimplifier.simplifiedMaterial3)
				{
					enemySimplifier.simplifiedMaterial3 = material2;
				}
			}
		}
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (!buffUnaffectedRenderers.Contains(renderer) && !(renderer is ParticleSystemRenderer))
			{
				Material[] array2 = new Material[renderer.sharedMaterials.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = material;
				}
				renderer.sharedMaterials = array2;
				puppetRenderers.Add(renderer);
			}
		}
		if (originalScale == Vector3.zero)
		{
			if ((bool)rb)
			{
				rbc = rb.constraints;
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}
			originalScale = base.transform.localScale;
			squishedScale = new Vector3(originalScale.x * 5f, 0.001f, originalScale.z * 5f);
			base.transform.localScale = squishedScale;
		}
		puppetSpawnColliders = GetComponentsInChildren<Collider>();
		Collider[] array3 = puppetSpawnColliders;
		for (int i = 0; i < array3.Length; i++)
		{
			Physics.IgnoreCollision(array3[i], MonoSingleton<NewMovement>.Instance.playerCollider, ignore: true);
		}
		puppetSpawnIgnoringPlayer = true;
		UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.puppetSpawn, base.transform.position + Vector3.up * 0.25f, Quaternion.identity).transform.SetParent(GetGoreZone().transform, worldPositionStays: true);
	}

	public void BuffAll()
	{
		damageBuffRequests++;
		speedBuffRequests++;
		healthBuffRequests++;
		UpdateBuffs();
	}

	public void UnbuffAll()
	{
		speedBuffRequests--;
		healthBuffRequests--;
		damageBuffRequests--;
		UpdateBuffs();
	}

	public void DamageBuff()
	{
		DamageBuff(damageBuffModifier);
	}

	public void DamageBuff(float modifier)
	{
		damageBuffRequests++;
		damageBuffModifier = modifier;
		UpdateBuffs();
	}

	public void DamageUnbuff()
	{
		damageBuffRequests--;
		UpdateBuffs();
	}

	public void SpeedBuff()
	{
		SpeedBuff(speedBuffModifier);
	}

	public void SpeedBuff(float modifier)
	{
		speedBuffRequests++;
		speedBuffModifier = modifier;
		UpdateBuffs();
	}

	public void SpeedUnbuff()
	{
		speedBuffRequests--;
		UpdateBuffs();
	}

	public void HealthBuff()
	{
		HealthBuff(healthBuffModifier);
	}

	public void HealthBuff(float modifier)
	{
		healthBuffRequests++;
		healthBuffModifier = modifier;
		UpdateBuffs();
	}

	public void HealthUnbuff()
	{
		healthBuffRequests--;
		UpdateBuffs();
	}

	public void UpdateBuffs(bool visualsOnly = false, bool particle = true)
	{
		speedBuff = speedBuffRequests > 0;
		healthBuff = healthBuffRequests > 0;
		damageBuff = damageBuffRequests > 0;
		if (healthBuff || speedBuff || damageBuff || OptionsManager.forceRadiance)
		{
			if (!hasRadianceEffected)
			{
				hasRadianceEffected = true;
				if (particle)
				{
					Collider component = GetComponent<Collider>();
					if ((bool)component)
					{
						UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.radianceEffect, component.bounds.center, Quaternion.identity);
					}
					else
					{
						UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.radianceEffect, base.transform.position, Quaternion.identity);
					}
				}
			}
		}
		else
		{
			hasRadianceEffected = false;
			speedBuffRequests = 0;
			healthBuffRequests = 0;
			damageBuffRequests = 0;
		}
		if (!visualsOnly)
		{
			UpdateModifiers();
			SendMessage("UpdateBuff", SendMessageOptions.DontRequireReceiver);
		}
	}

	[Obsolete("Use version with TargetHandle")]
	public static bool CheckHurtException(EnemyType attacker, EnemyType receiver, EnemyTarget attackTarget = null)
	{
		if (EnemyIdentifierDebug.Active)
		{
			Log.Fine($"Checking hurt exception between <b>{attacker}</b> and <b>{receiver}</b> with attack target <b>{attackTarget}</b>", (IEnumerable<Tag>)null, (string)null, (object)null);
		}
		if (attacker == receiver)
		{
			return true;
		}
		if (attackTarget != null && attackTarget.isEnemy && attackTarget.enemyIdentifier.enemyType == receiver)
		{
			return false;
		}
		if ((attacker == EnemyType.Stalker && receiver != EnemyType.Swordsmachine) || (receiver == EnemyType.Stalker && attacker != EnemyType.Swordsmachine))
		{
			return true;
		}
		if ((attacker == EnemyType.Filth || attacker == EnemyType.Stray || attacker == EnemyType.Schism || attacker == EnemyType.Soldier) && (receiver == EnemyType.Filth || receiver == EnemyType.Stray || receiver == EnemyType.Schism || receiver == EnemyType.Soldier))
		{
			return true;
		}
		switch (receiver)
		{
		case EnemyType.Sisyphus:
			return true;
		case EnemyType.Ferryman:
			return true;
		case EnemyType.Power:
			return true;
		default:
			if (((attacker == EnemyType.Drone || attacker == EnemyType.Virtue || attacker == EnemyType.Providence) && receiver == EnemyType.FleshPrison) || ((receiver == EnemyType.Drone || receiver == EnemyType.Virtue || receiver == EnemyType.Providence) && attacker == EnemyType.FleshPrison))
			{
				return true;
			}
			if (((attacker == EnemyType.Drone || attacker == EnemyType.Virtue || attacker == EnemyType.Providence) && receiver == EnemyType.FleshPanopticon) || ((receiver == EnemyType.Drone || receiver == EnemyType.Virtue || receiver == EnemyType.Providence) && attacker == EnemyType.FleshPanopticon))
			{
				return true;
			}
			if ((attacker == EnemyType.Gabriel || attacker == EnemyType.GabrielSecond) && (receiver == EnemyType.Gabriel || receiver == EnemyType.GabrielSecond))
			{
				return true;
			}
			return false;
		}
	}

	public static bool CheckHurtException(EnemyType attacker, EnemyType receiver, TargetHandle targetHandle)
	{
		if (attacker == receiver)
		{
			return true;
		}
		if (targetHandle != null && targetHandle.target.Type == TargetType.ENEMY && targetHandle.target.EID.enemyType == receiver)
		{
			return false;
		}
		if ((attacker == EnemyType.Stalker && receiver != EnemyType.Swordsmachine) || (receiver == EnemyType.Stalker && attacker != EnemyType.Swordsmachine))
		{
			return true;
		}
		if ((attacker == EnemyType.Filth || attacker == EnemyType.Stray || attacker == EnemyType.Schism || attacker == EnemyType.Soldier) && (receiver == EnemyType.Filth || receiver == EnemyType.Stray || receiver == EnemyType.Schism || receiver == EnemyType.Soldier))
		{
			return true;
		}
		switch (receiver)
		{
		case EnemyType.Sisyphus:
			return true;
		case EnemyType.Ferryman:
			return true;
		case EnemyType.Power:
			return true;
		default:
			if (((attacker == EnemyType.Drone || attacker == EnemyType.Virtue || attacker == EnemyType.Providence) && receiver == EnemyType.FleshPrison) || ((receiver == EnemyType.Drone || receiver == EnemyType.Virtue || receiver == EnemyType.Providence) && attacker == EnemyType.FleshPrison))
			{
				return true;
			}
			if (((attacker == EnemyType.Drone || attacker == EnemyType.Virtue || attacker == EnemyType.Providence) && receiver == EnemyType.FleshPanopticon) || ((receiver == EnemyType.Drone || receiver == EnemyType.Virtue || receiver == EnemyType.Providence) && attacker == EnemyType.FleshPanopticon))
			{
				return true;
			}
			if ((attacker == EnemyType.Gabriel || attacker == EnemyType.GabrielSecond) && (receiver == EnemyType.Gabriel || receiver == EnemyType.GabrielSecond))
			{
				return true;
			}
			return false;
		}
	}

	public static void FallOnEnemy(EnemyIdentifier eid, bool returnToOrigin = false)
	{
		if (eid.dead)
		{
			eid.Explode();
			return;
		}
		switch (eid.enemyType)
		{
		case EnemyType.Idol:
		case EnemyType.Deathcatcher:
			eid.InstaKill();
			break;
		case EnemyType.Sisyphus:
		{
			if (eid.TryGetComponent<Sisyphus>(out var component3))
			{
				eid.DeliverDamage(eid.gameObject, Vector3.zero, eid.transform.position, 22f, tryForExplode: true);
				component3.Knockdown(component3.transform.position + component3.transform.forward);
			}
			break;
		}
		case EnemyType.Mindflayer:
		{
			if (eid.TryGetComponent<Mindflayer>(out var component5))
			{
				if (returnToOrigin)
				{
					component5.TeleportTo(component5.originalPosition);
				}
				else
				{
					component5.Teleport();
				}
			}
			break;
		}
		case EnemyType.Gabriel:
		case EnemyType.GabrielSecond:
		{
			if (eid.TryGetComponent<GabrielBase>(out var component4))
			{
				if (returnToOrigin)
				{
					component4.TeleportTo(component4.originalPosition);
				}
				else
				{
					component4.Teleport();
				}
			}
			break;
		}
		case EnemyType.Power:
		{
			if (!eid.TryGetComponent<Power>(out var component2))
			{
				break;
			}
			if (component2.juggled)
			{
				eid.Splatter(styleBonus: false);
				break;
			}
			eid.DeliverDamage(eid.gameObject, Vector3.zero, eid.transform.position, 22f, tryForExplode: true);
			component2.PlaySound(MonoSingleton<PowerVoiceController>.Instance.HurtBig());
			component2.StopAction();
			if (returnToOrigin)
			{
				component2.TeleportTo(component2.originalPosition);
			}
			else
			{
				component2.Teleport();
			}
			break;
		}
		case EnemyType.Ferryman:
		{
			if (eid.TryGetComponent<Ferryman>(out var component))
			{
				component.Roll();
			}
			break;
		}
		default:
			eid.Splatter(styleBonus: false);
			break;
		}
	}

	public static void SendToPortalLayer(GameObject gob)
	{
		Renderer[] componentsInChildren = gob.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 30;
		}
	}

	public void SendAttachedProjectilesToMirrorLayer()
	{
		foreach (Nail nail in nails)
		{
			SendToPortalLayer(nail.gameObject);
		}
		if (stuckMagnets.Count > 0)
		{
			foreach (Magnet stuckMagnet in stuckMagnets)
			{
				Harpoon componentInParent = stuckMagnet.GetComponentInParent<Harpoon>();
				SendToPortalLayer(componentInParent ? componentInParent.gameObject : stuckMagnet.gameObject);
			}
		}
		if (drillers.Count <= 0)
		{
			return;
		}
		foreach (Harpoon driller in drillers)
		{
			SendToPortalLayer(driller.gameObject);
		}
	}

	public void PlayerMarkedForDeath()
	{
		attackEnemies = false;
		prioritizeEnemiesUnlessAttacked = false;
		prioritizePlayerOverFallback = true;
	}

	public bool HitterCanKillIdol(string hitter)
	{
		switch (hitter)
		{
		default:
			return hitter == "hammer";
		case "punch":
		case "heavypunch":
		case "ground slam":
			return true;
		}
	}

	public void BossBar(bool enable)
	{
		BossHealthBar bossHealthBar = GetComponent<BossHealthBar>();
		if (enable)
		{
			if (bossHealthBar == null)
			{
				bossHealthBar = base.gameObject.AddComponent<BossHealthBar>();
			}
			switch (enemyType)
			{
			case EnemyType.FleshPrison:
				bossHealthBar.SetSecondaryBarColor(Color.green);
				bossHealthBar.secondaryBar = true;
				break;
			case EnemyType.FleshPanopticon:
				bossHealthBar.SetSecondaryBarColor(new Color(1f, 64f / 85f, 0f));
				bossHealthBar.secondaryBar = true;
				break;
			}
		}
		else if (bossHealthBar != null)
		{
			UnityEngine.Object.Destroy(bossHealthBar);
		}
	}

	public void ChangeDamageTakenMultiplier(float newMultiplier)
	{
		totalDamageTakenMultiplier = newMultiplier;
	}

	public void SimpleDamage(float amount)
	{
		DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount, tryForExplode: false);
	}

	public void SimpleDamageIgnoreMultiplier(float amount)
	{
		if (totalDamageTakenMultiplier != 0f)
		{
			DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount / totalDamageTakenMultiplier, tryForExplode: false);
		}
		else
		{
			DeliverDamage(base.gameObject, Vector3.zero, base.transform.position, amount, tryForExplode: false, 0f, null, ignoreTotalDamageTakenMultiplier: true);
		}
	}

	private void TryUnPuppet()
	{
		if (!permaPuppet && TryGetComponent<EnemySpawnableInstance>(out var component) && !(component.sourceObject.gameObject == null))
		{
			puppet = false;
			SavedEnemy savedEnemy = component.SaveEnemy();
			savedEnemy.Scale = SavedVector3.One;
			MonoSingleton<SandboxSaver>.Instance.RebuildObjectList();
			MonoSingleton<SandboxSaver>.Instance.RecreateEnemy(savedEnemy, newSizing: true);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public float GetReachDistanceMultiplier()
	{
		switch (enemyType)
		{
		case EnemyType.HideousMass:
		case EnemyType.Sisyphus:
		case EnemyType.SisyphusPrime:
		case EnemyType.Gutterman:
		case EnemyType.Guttertank:
			return 0.5f;
		default:
			return 1f;
		}
	}

	public Transform GetCenter()
	{
		if (!(overrideCenter != null))
		{
			return base.transform;
		}
		return overrideCenter;
	}
}
