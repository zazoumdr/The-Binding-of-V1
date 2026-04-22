using System.Collections.Generic;
using Sandbox;
using ULTRAKILL.Cheats;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class Idol : MonoBehaviour, IAlter, IAlterOptions<int>
{
	public EnemyIdentifier overrideTarget;

	public bool activeWhileWaitingForOverride;

	[HideInInspector]
	public EnemyIdentifier target;

	private int difficulty = -1;

	[SerializeField]
	private LineRenderer beam;

	[HideInInspector]
	public Material unradiantBeam;

	[SerializeField]
	private Material radiantBeam;

	[SerializeField]
	private SpriteRenderer halo;

	[HideInInspector]
	public Sprite unradiantHalo;

	[HideInInspector]
	public Color haloColor;

	[SerializeField]
	private Sprite radiantHalo;

	private Vector3 beamOffset;

	[SerializeField]
	private GameObject deathParticle;

	private bool dead;

	private EnemyIdentifier eid;

	public Vision vision;

	private VisionQuery beamQuery;

	private VisionQuery nearestQuery;

	private EnemyIdentifier tempTarget;

	private int restorableOverrideTargetID;

	public string alterKey => "idol";

	public string alterCategoryName => "Idol";

	public AlterOption<int>[] options => new AlterOption<int>[1]
	{
		new AlterOption<int>
		{
			name = "Override Target ID",
			key = "overrideTarget",
			hidden = true,
			callback = delegate(int value)
			{
				if (value == 0)
				{
					Debug.Log("Not restoring overrideTarget because ID is 0");
				}
				else
				{
					Debug.Log("Trying to restore overrideTarget with ID " + value);
					restorableOverrideTargetID = value;
				}
			},
			value = ((!(overrideTarget == null)) ? overrideTarget.GetInstanceID() : 0)
		}
	};

	private void Awake()
	{
		if ((bool)overrideTarget)
		{
			if (overrideTarget.gameObject.activeInHierarchy)
			{
				ChangeTarget(overrideTarget);
			}
			else
			{
				restorableOverrideTargetID = overrideTarget.GetInstanceID();
			}
		}
		eid = GetComponent<EnemyIdentifier>();
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		if (unradiantBeam == null)
		{
			unradiantBeam = beam.material;
		}
		if (unradiantHalo == null)
		{
			unradiantHalo = halo.sprite;
			haloColor = halo.color;
		}
		vision = new Vision(base.transform.position, new VisionTypeFilter(TargetType.ENEMY));
		LayerMask lm = LayerMaskDefaults.Get(LMD.Environment);
		beamQuery = new VisionQuery("Beam", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid) && !t.IsObstructed(beam.transform.position, lm));
		nearestQuery = new VisionQuery("Nearest", (TargetDataRef t) => EnemyScript.CheckTarget(t, eid));
		SlowUpdate();
	}

	private void FixedUpdate()
	{
		vision.UpdateSourcePos(base.transform.position);
	}

	private void OnDisable()
	{
		CancelInvoke("SlowUpdate");
		if ((bool)target)
		{
			ChangeTarget(null);
		}
	}

	private void OnEnable()
	{
		CancelInvoke("SlowUpdate");
		SlowUpdate();
	}

	private void UpdateBuff()
	{
		beam.material = ((eid.damageBuff || eid.healthBuff || eid.speedBuff) ? radiantBeam : unradiantBeam);
		halo.sprite = ((eid.damageBuff || eid.healthBuff || eid.speedBuff) ? radiantHalo : unradiantHalo);
		halo.color = ((eid.damageBuff || eid.healthBuff || eid.speedBuff) ? Color.white : haloColor);
	}

	private void Update()
	{
		if ((bool)overrideTarget && target != overrideTarget && !overrideTarget.dead && overrideTarget.gameObject.activeInHierarchy)
		{
			ChangeTarget(overrideTarget);
		}
		if (beam.enabled != (bool)target)
		{
			beam.enabled = target;
		}
		if ((bool)target)
		{
			Vector3 position = beam.transform.position;
			Vector3 position2 = target.transform.position + beamOffset;
			beam.SetPosition(0, position);
			beam.SetPosition(1, position2);
			PortalTraversalV2[] portalTraversals = null;
			if (vision.TrySee(beamQuery, out var data))
			{
				Matrix4x4 portalMatrix = data.portalMatrix;
				Vector3 direction = data.position + portalMatrix.MultiplyVector(beamOffset) - position;
				float magnitude = direction.magnitude;
				direction /= magnitude;
				LayerMask layerMask = LayerMaskDefaults.Get(LMD.Environment);
				PortalPhysicsV2.Raycast(position, direction, magnitude, layerMask, out var _, out portalTraversals, out var _);
			}
			this.GenerateLineRendererSegments(beam, portalTraversals);
		}
	}

	private void SlowUpdate()
	{
		Invoke("SlowUpdate", 0.2f);
		if (BlindEnemies.Blind)
		{
			if ((bool)target && (!overrideTarget || target != overrideTarget || overrideTarget.dead))
			{
				ChangeTarget(null);
			}
			return;
		}
		if (overrideTarget == null && restorableOverrideTargetID != 0 && !MonoSingleton<EnemyTracker>.Instance.TryGetEnemy(restorableOverrideTargetID, out overrideTarget, includePuppetRespawned: true) && !MonoSingleton<EnemyTracker>.Instance.IsEnemySpawnRecorded(restorableOverrideTargetID))
		{
			restorableOverrideTargetID = 0;
			Debug.Log("Removing restorableOverrideTargetID because EnemyTracker does not remember this enemy");
		}
		if ((bool)overrideTarget)
		{
			if ((bool)overrideTarget && !overrideTarget.dead && (overrideTarget.gameObject.activeInHierarchy || !activeWhileWaitingForOverride))
			{
				if (target != overrideTarget && overrideTarget.gameObject.activeInHierarchy)
				{
					ChangeTarget(overrideTarget);
				}
				return;
			}
			overrideTarget = null;
			ChangeTarget(null);
		}
		PickNewTarget(ignoreIfAlreadyTargeting: false);
	}

	public void PickNewTarget(bool ignoreIfAlreadyTargeting = true)
	{
		if (ignoreIfAlreadyTargeting && (target != null || (overrideTarget != null && !overrideTarget.Equals(null))))
		{
			return;
		}
		List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies == null || currentEnemies.Count <= 0)
		{
			return;
		}
		bool flag = false;
		float num = float.PositiveInfinity;
		tempTarget = null;
		int num2 = ((!target || target.dead) ? 1 : Mathf.Max(MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(target), 2));
		for (int num3 = 7; num3 > num2; num3--)
		{
			for (int i = 0; i < currentEnemies.Count; i++)
			{
				if (currentEnemies[i] != target && (currentEnemies[i].blessed || currentEnemies[i].enemyType == EnemyType.Idol))
				{
					continue;
				}
				int enemyRank = MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[i]);
				if (enemyRank == num3 || (enemyRank <= 2 && num3 == 2))
				{
					TargetDataRef data;
					float num4 = (vision.TrySee(nearestQuery, out data) ? data.DistanceTo(base.transform.position) : Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, currentEnemies[i].transform.position));
					if (num4 < num)
					{
						tempTarget = currentEnemies[i];
						flag = true;
						num = num4;
					}
				}
			}
			if (flag)
			{
				ChangeTarget(tempTarget);
				break;
			}
		}
	}

	public void Death()
	{
		if (dead)
		{
			return;
		}
		dead = true;
		if ((bool)target && (eid.damageBuff || eid.speedBuff || eid.healthBuff))
		{
			if (eid.damageBuff)
			{
				target.DamageBuff();
			}
			if (eid.speedBuff)
			{
				target.SpeedBuff();
			}
			if (eid.healthBuff)
			{
				target.HealthBuff();
			}
		}
		GoreZone goreZone = GoreZone.ResolveGoreZone(base.transform);
		if ((bool)goreZone && (bool)eid)
		{
			goreZone.EnemyDeath(eid);
		}
		if ((bool)eid)
		{
			eid.Death();
		}
		if ((bool)deathParticle)
		{
			Object.Instantiate(deathParticle, beam.transform.position, Quaternion.identity, goreZone.gibZone);
		}
		GameObject gameObject = null;
		for (int i = 0; i < 3; i++)
		{
			gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, eid);
			if (!gameObject)
			{
				break;
			}
			gameObject.transform.position = beam.transform.position;
			gameObject.transform.SetParent(goreZone.goreZone, worldPositionStays: true);
			gameObject.SetActive(value: true);
			if (gameObject.TryGetComponent<Bloodsplatter>(out var component))
			{
				component.GetReady();
			}
		}
		if (!eid.dontCountAsKills)
		{
			ActivateNextWave componentInParent = GetComponentInParent<ActivateNextWave>();
			if (componentInParent != null)
			{
				componentInParent.AddDeadEnemy();
			}
		}
		MonoSingleton<StyleHUD>.Instance.AddPoints(80, "ultrakill.iconoclasm", null, eid);
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	private void ChangeTarget(EnemyIdentifier newTarget)
	{
		if ((bool)target)
		{
			target.Unbless();
		}
		if (!newTarget)
		{
			target = null;
			return;
		}
		target = newTarget;
		target.Bless();
		if (target.TryGetComponent<Collider>(out var component))
		{
			beamOffset = component.bounds.center - target.transform.position;
		}
	}

	public void ChangeOverrideTarget(EnemyIdentifier eid)
	{
		overrideTarget = eid;
		ChangeTarget(eid);
	}
}
