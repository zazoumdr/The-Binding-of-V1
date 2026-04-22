using System;
using System.Collections.Generic;
using System.Threading;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

public class Coin : MonoBehaviour, ITarget
{
	private struct PendingBeamHit
	{
		public Vector3 hitPoint;

		public GameObject altBeam;
	}

	private Vision vision;

	private VisionQuery playerQuery;

	private VisionQuery enemyQuery;

	private VisionQuery coinQuery;

	private VisionQuery punchEnemyQuery;

	private Vector3 cachedPlayerPos;

	private VisionQuery explosiveQuery;

	private VisionQuery glassQuery;

	private LayerMask env_lm;

	private LayerMask empty_lm;

	public EnemyTarget customTarget;

	public GameObject sourceWeapon;

	public Rigidbody rb;

	private bool checkingSpeed;

	private float timeToDelete = 1f;

	public GameObject refBeam;

	public Vector3 hitPoint = Vector3.zero;

	private Collider[] cols;

	private SphereCollider scol;

	private CancellationTokenSource untrackCoinTokenSource;

	public bool shot;

	[HideInInspector]
	public bool shotByEnemy;

	private bool wasShotByEnemy;

	public GameObject coinBreak;

	public float power;

	private EnemyIdentifier eid;

	public bool quickDraw;

	public Material uselessMaterial;

	private GameObject altBeam;

	public GameObject coinHitSound;

	private Queue<PendingBeamHit> pendingBeamHits = new Queue<PendingBeamHit>();

	[HideInInspector]
	public int hitTimes = 1;

	public bool doubled;

	public GameObject flash;

	public GameObject enemyFlash;

	public GameObject chargeEffect;

	private GameObject currentCharge;

	private StyleHUD shud;

	public CoinChainCache ccc;

	public int ricochets;

	[HideInInspector]
	public int difficulty = -1;

	public bool dontDestroyOnPlayerRespawn;

	public bool ignoreBlessedEnemies;

	private Vector3 cachedPos;

	private Quaternion cachedRot;

	private Vector3 cachedVel;

	private SimplePortalTraveler portalTraveler;

	public int Id => GetInstanceID();

	public TargetType Type => TargetType.COIN;

	public EnemyIdentifier EID => null;

	public GameObject GameObject
	{
		get
		{
			if (!(this == null))
			{
				return base.gameObject;
			}
			return null;
		}
	}

	public Rigidbody Rigidbody => rb;

	public Transform Transform => base.transform;

	public Vector3 Position => cachedPos;

	public Vector3 HeadPosition => cachedPos;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		MonoSingleton<CoinTracker>.Instance.AddCoin(this);
		shud = MonoSingleton<StyleHUD>.Instance;
		doubled = false;
		Invoke("GetDeleted", 5f);
		Invoke("StartCheckingSpeed", 0.1f);
		Invoke("TripleTime", 0.35f);
		Invoke("TripleTimeEnd", 0.417f);
		Invoke("DoubleTime", 1f);
		cols = GetComponents<Collider>();
		scol = GetComponent<SphereCollider>();
		Collider[] array = cols;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		env_lm = LayerMaskDefaults.Get(LMD.Environment);
		empty_lm = default(LayerMask);
		vision = new Vision(base.transform.position, new VisionTypeFilter(TargetType.PLAYER, TargetType.COIN, TargetType.ENEMY, TargetType.EXPLOSIVE, TargetType.GLASS));
		MonoSingleton<PortalManagerV2>.Instance.TargetTracker.RegisterVision(vision, base.destroyCancellationToken);
		playerQuery = new VisionQuery("CoinPlayer", (TargetDataRef t) => t.target.Type == TargetType.PLAYER && !t.IsObstructed(base.transform.position, env_lm));
		enemyQuery = new VisionQuery("CoinEnemy", (TargetDataRef t) => t.target.Type == TargetType.ENEMY && !t.target.EID.dead && (!ignoreBlessedEnemies || !t.target.EID.blessed) && CheckEnemyObstruction(t) && (ccc == null || !ccc.beenHit.Contains(t.target.EID.gameObject)));
		punchEnemyQuery = new VisionQuery("CoinEnemy", (TargetDataRef t) => t.target.Type == TargetType.ENEMY && !t.target.EID.dead && (!ignoreBlessedEnemies || !t.target.EID.blessed) && CheckEnemyObstruction(t));
		coinQuery = new VisionQuery("CoinCoin", (TargetDataRef t) => t.target.Type == TargetType.COIN && CoinReflectCheck(t) && !t.IsObstructed(base.transform.position, env_lm));
		explosiveQuery = new VisionQuery("CoinExplosive", (TargetDataRef t) => t.target.Type == TargetType.EXPLOSIVE && ExplosiveReflectCheck(t) && t.DistanceTo(cachedPlayerPos) < 100f && !t.IsObstructed(base.transform.position, env_lm));
		glassQuery = new VisionQuery("CoinGlass", (TargetDataRef t) => t.target.Type == TargetType.GLASS && (!t.IsObstructed(base.transform.position, env_lm, toHead: false, out var obstructionResult, out var _) || GlassHitCheck(t, obstructionResult)));
		if (!TryGetComponent<SimplePortalTraveler>(out var _))
		{
			portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
			portalTraveler.SetType(PortalTravellerType.PLAYER_PROJECTILE);
		}
		untrackCoinTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { base.destroyCancellationToken });
	}

	public bool CheckEnemyObstruction(TargetDataRef t)
	{
		if (t.target.EID.enemyType == EnemyType.Geryon)
		{
			return true;
		}
		bool toHead = t.target.EID.weakPoint != null && t.target.EID.weakPoint.activeInHierarchy;
		return !t.IsObstructed(base.transform.position, env_lm, toHead);
	}

	public bool IsOverrideTarget(TargetDataRef t)
	{
		if (t.target.EID != null)
		{
			_ = t.target.EID.enemyType;
			_ = 42;
			return false;
		}
		return false;
	}

	public void UntrackCoin()
	{
		if (untrackCoinTokenSource != null)
		{
			untrackCoinTokenSource.Cancel();
			untrackCoinTokenSource.Dispose();
			untrackCoinTokenSource = null;
		}
	}

	private bool CoinReflectCheck(TargetDataRef data)
	{
		GameObject gameObject = data.target.GameObject;
		if (gameObject == null)
		{
			return false;
		}
		GameObject gameObject2 = base.gameObject;
		if (gameObject2 != null && gameObject2 == gameObject)
		{
			return false;
		}
		if (gameObject.TryGetComponent<Coin>(out var component))
		{
			if (component.shot)
			{
				return component.shotByEnemy;
			}
			return true;
		}
		return false;
	}

	private bool ExplosiveReflectCheck(TargetDataRef data)
	{
		GameObject gameObject = data.target.GameObject;
		if (gameObject == null)
		{
			return false;
		}
		if (gameObject.TryGetComponent<Grenade>(out var component))
		{
			if (!component.playerRiding)
			{
				return !component.enemy;
			}
			return false;
		}
		if (gameObject.TryGetComponent<Cannonball>(out var _))
		{
			return true;
		}
		return false;
	}

	private bool GlassHitCheck(TargetDataRef data, RaycastHit hit)
	{
		GameObject gameObject = data.target.GameObject;
		if (gameObject == null)
		{
			return false;
		}
		return hit.transform == gameObject.transform;
	}

	private void Update()
	{
		vision.sourcePos = base.transform.position;
		if (!shot)
		{
			if (checkingSpeed && rb.velocity.magnitude < 1f)
			{
				timeToDelete -= Time.deltaTime * 10f;
			}
			else
			{
				timeToDelete = 1f;
			}
			if (timeToDelete <= 0f)
			{
				GetDeleted();
			}
		}
	}

	private void TripleTime()
	{
		if (!shot)
		{
			hitTimes = 2;
			doubled = true;
			if ((bool)currentCharge)
			{
				UnityEngine.Object.Destroy(currentCharge);
			}
			if ((bool)flash)
			{
				currentCharge = UnityEngine.Object.Instantiate(flash, base.transform.position, Quaternion.identity);
				currentCharge.transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
	}

	private void TripleTimeEnd()
	{
		if (!shot)
		{
			hitTimes = 1;
			doubled = true;
		}
	}

	private void DoubleTime()
	{
		if (!shot)
		{
			hitTimes = 2;
			doubled = true;
			if ((bool)currentCharge)
			{
				UnityEngine.Object.Destroy(currentCharge);
			}
			if ((bool)chargeEffect)
			{
				currentCharge = UnityEngine.Object.Instantiate(chargeEffect, base.transform.position, base.transform.rotation);
				currentCharge.transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
	}

	public void DelayedReflectRevolver(Vector3 hitp, GameObject beam = null)
	{
		if (checkingSpeed)
		{
			if (shotByEnemy)
			{
				CancelInvoke("EnemyReflect");
				CancelInvoke("ShootAtPlayer");
				shotByEnemy = false;
			}
			ricochets++;
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			if (!ccc && beam == null)
			{
				GameObject gameObject = new GameObject();
				ccc = gameObject.AddComponent<CoinChainCache>();
				gameObject.AddComponent<RemoveOnTime>().time = 5f;
			}
			rb.isKinematic = true;
			shot = true;
			pendingBeamHits.Enqueue(new PendingBeamHit
			{
				hitPoint = hitp,
				altBeam = beam
			});
			if (pendingBeamHits.Count == 1)
			{
				Invoke("ReflectRevolver", 0.1f);
			}
		}
	}

	public void ReflectRevolver()
	{
		if (pendingBeamHits.Count > 0)
		{
			PendingBeamHit pendingBeamHit = pendingBeamHits.Dequeue();
			hitPoint = pendingBeamHit.hitPoint;
			altBeam = pendingBeamHit.altBeam;
		}
		GameObject gameObject = null;
		TargetData? targetData = null;
		_ = base.transform.position;
		scol.enabled = false;
		bool flag = false;
		bool flag2 = false;
		vision.UpdateSourcePos(base.transform.position);
		RaycastHit obstructionResult;
		if (MonoSingleton<CoinTracker>.Instance.revolverCoinsList.Count > 1)
		{
			if (vision.TrySee(coinQuery, out var data))
			{
				gameObject = data.target.GameObject;
				targetData = data.ToData();
			}
			if (gameObject != null)
			{
				flag = true;
				Coin component = gameObject.GetComponent<Coin>();
				component.power = power + 1f;
				component.ricochets += ricochets;
				if (quickDraw)
				{
					component.quickDraw = true;
				}
				if (component.shotByEnemy)
				{
					component.CancelInvoke("EnemyReflect");
					component.CancelInvoke("ShootAtPlayer");
					component.shotByEnemy = false;
				}
				AudioSource[] array = null;
				if (!ccc)
				{
					GameObject gameObject2 = new GameObject();
					ccc = gameObject2.AddComponent<CoinChainCache>();
					gameObject2.AddComponent<RemoveOnTime>().time = 5f;
				}
				component.ccc = ccc;
				if (altBeam == null)
				{
					component.DelayedReflectRevolver(gameObject.transform.position);
					LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
					array = component2.GetComponents<AudioSource>();
					if (hitPoint == Vector3.zero)
					{
						component2.SetPosition(0, base.transform.position);
					}
					else
					{
						component2.SetPosition(0, hitPoint);
					}
					component2.SetPosition(1, gameObject.transform.position);
					data.IsObstructed(base.transform.position, empty_lm, toHead: false, out obstructionResult, out var traversals);
					this.GenerateLineRendererSegments(component2, traversals);
					if (power > 2f)
					{
						AudioSource[] array2 = array;
						foreach (AudioSource obj in array2)
						{
							obj.SetPitch(1f + (power - 2f) / 5f);
							obj.Play(tracked: true);
						}
					}
				}
			}
		}
		if (!flag)
		{
			gameObject = null;
			List<Transform> list = new List<Transform>();
			foreach (Grenade grenade in MonoSingleton<ObjectTracker>.Instance.grenadeList)
			{
				if (!grenade.playerRiding && !grenade.enemy)
				{
					list.Add(grenade.transform);
				}
			}
			foreach (Cannonball cannonball in MonoSingleton<ObjectTracker>.Instance.cannonballList)
			{
				list.Add(cannonball.transform);
			}
			cachedPlayerPos = MonoSingleton<NewMovement>.Instance.Position;
			if (vision.TrySee(explosiveQuery, out var data2))
			{
				gameObject = data2.target.GameObject;
				targetData = data2.ToData();
			}
			if (gameObject != null && !altBeam)
			{
				LineRenderer component3 = SpawnBeam().GetComponent<LineRenderer>();
				component3.GetComponents<AudioSource>();
				if (hitPoint == Vector3.zero)
				{
					component3.SetPosition(0, base.transform.position);
				}
				else
				{
					component3.SetPosition(0, hitPoint);
				}
				component3.SetPosition(1, gameObject.transform.position);
				data2.IsObstructed(base.transform.position, empty_lm, toHead: false, out obstructionResult, out var traversals2);
				this.GenerateLineRendererSegments(component3, traversals2);
				Cannonball component5;
				if (gameObject.TryGetComponent<Grenade>(out var component4))
				{
					component4.Explode(component4.rocket, harmless: false, !component4.rocket);
				}
				else if (gameObject.TryGetComponent<Cannonball>(out component5))
				{
					component5.Explode();
				}
			}
			if (gameObject == null)
			{
				TargetDataRef data3;
				bool flag3 = vision.TrySee(enemyQuery, out data3);
				Vector3 vector = default(Vector3);
				PortalTraversalV2[] traversals3 = Array.Empty<PortalTraversalV2>();
				bool flag4 = flag3 && data3.target.EID.enemyType == EnemyType.Geryon;
				if (flag4)
				{
					flag3 = GetGeryonTarget(data3, out var pos);
					vector = pos;
				}
				if (flag3)
				{
					EnemyIdentifier eID = data3.target.EID;
					if (eID.dead || (ccc != null && ccc.beenHit.Contains(eID.gameObject)))
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					gameObject = data3.target.EID.gameObject;
					targetData = data3.ToData();
					if (eid == null)
					{
						eid = data3.target.EID;
					}
					flag2 = true;
					if ((bool)ccc && (bool)eid && (bool)eid.gameObject)
					{
						ccc.beenHit.Add(eid.gameObject);
					}
					if (altBeam == null)
					{
						LineRenderer component6 = SpawnBeam().GetComponent<LineRenderer>();
						AudioSource[] components = component6.GetComponents<AudioSource>();
						if (hitPoint == Vector3.zero)
						{
							component6.SetPosition(0, base.transform.position);
						}
						else
						{
							component6.SetPosition(0, hitPoint);
						}
						if (flag4)
						{
							component6.SetPosition(1, vector);
						}
						else
						{
							bool flag5 = eid.weakPoint != null && eid.weakPoint.activeInHierarchy;
							vector = ((!flag5) ? targetData.Value.position : targetData.Value.headPosition);
							component6.SetPosition(1, vector);
							data3.IsObstructed(base.transform.position, empty_lm, flag5, out obstructionResult, out traversals3);
						}
						this.GenerateLineRendererSegments(component6, traversals3);
						if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponent<EnemyIdentifierIdentifier>() != null)
						{
							bool flag6 = false;
							if (eid.enemyType == EnemyType.Streetcleaner)
							{
								Vector3 vector2 = data3.portalMatrix.MultiplyPoint3x4(vector);
								Vector3 vector3 = vector2 - base.transform.position;
								float maxDistance = Vector3.Distance(base.transform.position, vector2);
								if (PortalPhysicsV2.Raycast(base.transform.position, vector3.normalized, out var hitInfo, maxDistance, LayerMaskDefaults.Get(LMD.Enemies)) && hitInfo.transform != eid.weakPoint.transform)
								{
									EnemyIdentifierIdentifier component7 = hitInfo.transform.GetComponent<EnemyIdentifierIdentifier>();
									if ((bool)component7 && (bool)component7.eid && component7.eid == eid)
									{
										eid.DeliverDamage(hitInfo.transform.gameObject, (hitInfo.transform.position - base.transform.position).normalized * 10000f, hitInfo.transform.position, power, tryForExplode: false, 1f, sourceWeapon);
									}
									flag6 = true;
								}
							}
							if (!eid.blessed && !eid.puppet)
							{
								RicoshotPointsCheck();
								if (quickDraw)
								{
									shud.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
								}
							}
							eid.hitter = "revolver";
							if (!eid.hitterWeapons.Contains("revolver1"))
							{
								eid.hitterWeapons.Add("revolver1");
							}
							if (!flag6)
							{
								eid.DeliverDamage(eid.weakPoint, (eid.weakPoint.transform.position - base.transform.position).normalized * 10000f, vector, power, tryForExplode: false, 1f, sourceWeapon);
							}
						}
						else if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponentInChildren<Breakable>() != null)
						{
							Breakable componentInChildren = eid.weakPoint.GetComponentInChildren<Breakable>();
							RicoshotPointsCheck();
							if (componentInChildren.precisionOnly)
							{
								shud.AddPoints(100, "ultrakill.interruption", sourceWeapon, eid);
								MonoSingleton<TimeController>.Instance.ParryFlash();
								if ((bool)componentInChildren.interruptEnemy && !componentInChildren.interruptEnemy.blessed)
								{
									componentInChildren.interruptEnemy.Explode(fromExplosion: true);
								}
							}
							componentInChildren.Break(power);
						}
						else
						{
							RicoshotPointsCheck();
							eid.hitter = "revolver";
							eid.DeliverDamage(eid.GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, (eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position - base.transform.position).normalized * 10000f, vector, power, tryForExplode: false, 1f, sourceWeapon);
						}
						if (power > 2f)
						{
							AudioSource[] array2 = components;
							foreach (AudioSource obj2 in array2)
							{
								obj2.SetPitch(1f + (power - 2f) / 5f);
								obj2.Play(tracked: true);
							}
						}
						eid = null;
					}
				}
			}
			if (gameObject == null)
			{
				if (vision.TrySee(glassQuery, out var data4))
				{
					gameObject = data4.target.GameObject;
					targetData = data4.ToData();
				}
				if (gameObject != null && altBeam == null)
				{
					gameObject.GetComponentInChildren<Glass>().Shatter();
					if ((bool)ccc)
					{
						ccc.beenHit.Add(gameObject);
					}
					LineRenderer component8 = SpawnBeam().GetComponent<LineRenderer>();
					if (power > 2f)
					{
						AudioSource[] array2 = component8.GetComponents<AudioSource>();
						foreach (AudioSource obj3 in array2)
						{
							obj3.SetPitch(1f + (power - 2f) / 5f);
							obj3.Play(tracked: true);
						}
					}
					if (hitPoint == Vector3.zero)
					{
						component8.SetPosition(0, base.transform.position);
					}
					else
					{
						component8.SetPosition(0, hitPoint);
					}
					component8.SetPosition(1, gameObject.transform.position);
					data4.IsObstructed(base.transform.position, empty_lm, toHead: false, out obstructionResult, out var traversals4);
					this.GenerateLineRendererSegments(component8, traversals4);
				}
				if (gameObject == null && altBeam == null)
				{
					Vector3 endPoint = UnityEngine.Random.insideUnitSphere;
					Vector3 normalized = endPoint.normalized;
					LineRenderer component9 = SpawnBeam().GetComponent<LineRenderer>();
					if (power > 2f)
					{
						AudioSource[] array2 = component9.GetComponents<AudioSource>();
						foreach (AudioSource obj4 in array2)
						{
							obj4.SetPitch(1f + (power - 2f) / 5f);
							obj4.Play(tracked: true);
						}
					}
					if (hitPoint == Vector3.zero)
					{
						component9.SetPosition(0, base.transform.position);
					}
					else
					{
						component9.SetPosition(0, hitPoint);
					}
					if (!PortalPhysicsV2.Raycast(base.transform.position, normalized, float.PositiveInfinity, env_lm, out var hitInfo2, out var portalTraversals, out endPoint))
					{
						component9.SetPosition(1, hitInfo2.point);
						if (SceneHelper.IsStaticEnvironment(hitInfo2))
						{
							MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(hitInfo2);
						}
					}
					else
					{
						component9.SetPosition(1, base.transform.position + normalized * 1000f);
					}
					this.GenerateLineRendererSegments(component9, portalTraversals);
				}
			}
		}
		bool flag7 = false;
		if (altBeam != null)
		{
			GameObject gameObject3 = altBeam;
			AudioSource[] components2 = UnityEngine.Object.Instantiate(coinHitSound, base.transform.position, Quaternion.identity).GetComponents<AudioSource>();
			RevolverBeam component10 = altBeam.GetComponent<RevolverBeam>();
			if (hitTimes > 1 && component10.splitcoinable)
			{
				if (!ccc)
				{
					GameObject gameObject4 = new GameObject();
					ccc = gameObject4.AddComponent<CoinChainCache>();
					gameObject4.AddComponent<RemoveOnTime>().time = 5f;
				}
				if ((bool)ccc && (bool)eid)
				{
					ccc.beenHit.Add(eid.gameObject);
				}
				flag7 = true;
				gameObject3 = UnityEngine.Object.Instantiate(altBeam, altBeam.transform.parent);
				component10 = gameObject3.GetComponent<RevolverBeam>();
				Collider[] array3 = cols;
				for (int i = 0; i < array3.Length; i++)
				{
					array3[i].enabled = false;
				}
			}
			gameObject3.transform.position = base.transform.position;
			if (component10.beamType == BeamType.Revolver && hitTimes > 1 && component10.strongAlt && component10.hitAmount < 99)
			{
				component10.hitAmount++;
				component10.maxHitsPerTarget = component10.hitAmount;
			}
			if (flag2)
			{
				if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
				{
					gameObject3.transform.LookAt(targetData.Value.headPosition);
				}
				else
				{
					gameObject3.transform.LookAt(targetData.Value.position);
				}
				if (!eid.blessed && !eid.puppet)
				{
					RicoshotPointsCheck();
					if (quickDraw)
					{
						shud.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
					}
				}
				if (component10.beamType == BeamType.Revolver)
				{
					eid.hitter = "revolver";
					if (!eid.hitterWeapons.Contains("revolver" + component10.gunVariation))
					{
						eid.hitterWeapons.Add("revolver" + component10.gunVariation);
					}
				}
				else
				{
					eid.hitter = "railcannon";
					if (!eid.hitterWeapons.Contains("railcannon0"))
					{
						eid.hitterWeapons.Add("railcannon0");
					}
				}
				if (flag7)
				{
					eid = null;
				}
			}
			else if (gameObject != null)
			{
				gameObject3.transform.LookAt(targetData.Value.position);
			}
			else
			{
				gameObject3.transform.forward = UnityEngine.Random.insideUnitSphere.normalized;
			}
			if (!flag)
			{
				if (component10.beamType == BeamType.Revolver && component10.hasBeenRicocheter)
				{
					if (component10.maxHitsPerTarget < (component10.strongAlt ? 4 : 3))
					{
						component10.maxHitsPerTarget = Mathf.Min(component10.maxHitsPerTarget + 2, component10.strongAlt ? 4 : 3);
					}
				}
				else
				{
					component10.addedDamage += power / 4f;
					component10.damage += power / 4f * component10.coinDamageBonusMultiplier;
				}
			}
			if (power > 2f)
			{
				AudioSource[] array2 = components2;
				foreach (AudioSource obj5 in array2)
				{
					obj5.SetPitch(1f + (power - 2f) / 5f);
					obj5.Play(tracked: true);
				}
			}
			gameObject3.SetActive(value: true);
		}
		hitTimes--;
		if (hitTimes > 0 && (altBeam == null || flag7))
		{
			Invoke("ReflectRevolver", 0.05f);
			return;
		}
		if (pendingBeamHits.Count > 0)
		{
			Invoke("ReflectRevolver", 0.05f);
			return;
		}
		UntrackCoin();
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
	}

	public bool GetGeryonTarget(TargetDataRef data, out Vector3 pos)
	{
		List<Vector3> list = new List<Vector3> { data.headPosition };
		if (data.target.GameObject.TryGetComponent<Geryon>(out var component))
		{
			list.Add(component.tailBase.bounds.center);
			List<Vector3> list2 = new List<Vector3>
			{
				component.tailMid.bounds.center,
				component.wingRight.bounds.center,
				component.wingLeft.bounds.center
			};
			while (list2.Count > 0)
			{
				Vector3 item = list2[UnityEngine.Random.Range(0, list2.Count)];
				list2.RemoveAt(UnityEngine.Random.Range(0, list2.Count));
				list.Add(item);
			}
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < list.Count; i++)
		{
			Vector3 vector = list[i];
			Vector3 direction = vector - position;
			if (!PortalPhysicsV2.ProjectThroughPortals(base.transform.position, direction, env_lm, out var _, out var _, out var _))
			{
				pos = vector;
				return true;
			}
		}
		pos = Vector3.zero;
		return false;
	}

	public void DelayedPunchflection()
	{
		if (checkingSpeed && (!shot || shotByEnemy))
		{
			if (shotByEnemy)
			{
				CancelInvoke("EnemyReflect");
				CancelInvoke("ShootAtPlayer");
				shotByEnemy = false;
			}
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			ricochets++;
			if ((bool)currentCharge)
			{
				UnityEngine.Object.Destroy(currentCharge);
			}
			rb.isKinematic = true;
			shot = true;
			Punchflection();
		}
	}

	public void Punchflection()
	{
		bool flag = false;
		bool flag2 = false;
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity);
		gameObject.SetActive(value: false);
		Vector3 position = base.transform.position;
		scol.enabled = false;
		UntrackCoin();
		vision.UpdateSourcePos(base.transform.position);
		TargetDataRef data;
		bool flag3 = vision.TrySee(punchEnemyQuery, out data);
		Vector3 vector = default(Vector3);
		PortalTraversalV2[] traversals = Array.Empty<PortalTraversalV2>();
		bool flag4 = flag3 && data.target.EID.enemyType == EnemyType.Geryon;
		if (flag4)
		{
			flag3 = GetGeryonTarget(data, out var pos);
			vector = pos;
		}
		if (flag3 && data.target.EID.dead)
		{
			flag3 = false;
		}
		if (flag3)
		{
			if (eid == null)
			{
				eid = data.target.EID;
			}
			LineRenderer component = SpawnBeam().GetComponent<LineRenderer>();
			AudioSource[] components = component.GetComponents<AudioSource>();
			if (hitPoint == Vector3.zero)
			{
				component.SetPosition(0, base.transform.position);
			}
			else
			{
				component.SetPosition(0, hitPoint);
			}
			if (flag4)
			{
				component.SetPosition(1, vector);
			}
			else
			{
				bool flag5 = eid.weakPoint != null && eid.weakPoint.activeInHierarchy;
				vector = ((!flag5) ? eid.transform.position : eid.weakPoint.transform.position);
				component.SetPosition(1, vector);
				data.IsObstructed(base.transform.position, empty_lm, flag5, out var _, out traversals);
			}
			this.GenerateLineRendererSegments(component, traversals);
			if (eid.blessed)
			{
				flag2 = true;
			}
			position = vector;
			if (!eid.puppet && !eid.blessed)
			{
				shud.AddPoints(50, "ultrakill.fistfullofdollar", sourceWeapon, eid);
			}
			if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy && eid.weakPoint.GetComponent<EnemyIdentifierIdentifier>() != null)
			{
				eid.hitter = "coin";
				if (!eid.hitterWeapons.Contains("coin"))
				{
					eid.hitterWeapons.Add("coin");
				}
				eid.DeliverDamage(eid.weakPoint, (eid.weakPoint.transform.position - base.transform.position).normalized * 10000f, vector, power, tryForExplode: false, 1f, sourceWeapon);
			}
			else if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
			{
				Breakable componentInChildren = eid.weakPoint.GetComponentInChildren<Breakable>();
				if (componentInChildren.precisionOnly)
				{
					shud.AddPoints(100, "ultrakill.interruption", sourceWeapon, eid);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					if ((bool)componentInChildren.interruptEnemy && !componentInChildren.interruptEnemy.blessed)
					{
						componentInChildren.interruptEnemy.Explode(fromExplosion: true);
					}
				}
				componentInChildren.Break(power);
			}
			else
			{
				eid.hitter = "coin";
				eid.DeliverDamage(eid.GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject, (eid.GetComponentInChildren<EnemyIdentifierIdentifier>().transform.position - base.transform.position).normalized * 10000f, hitPoint, power, tryForExplode: false, 1f, sourceWeapon);
			}
			if (power > 2f)
			{
				AudioSource[] array = components;
				foreach (AudioSource obj in array)
				{
					obj.SetPitch(1f + (power - 2f) / 5f);
					obj.Play(tracked: true);
				}
				eid = null;
			}
		}
		else
		{
			flag = true;
			Vector3 forward = MonoSingleton<CameraController>.Instance.transform.forward;
			LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
			if (power > 2f)
			{
				AudioSource[] array = component2.GetComponents<AudioSource>();
				foreach (AudioSource obj2 in array)
				{
					obj2.SetPitch(1f + (power - 2f) / 5f);
					obj2.Play(tracked: true);
				}
			}
			if (hitPoint == Vector3.zero)
			{
				component2.SetPosition(0, base.transform.position);
			}
			else
			{
				component2.SetPosition(0, hitPoint);
			}
			if (PortalPhysicsV2.Raycast(MonoSingleton<CameraController>.Instance.transform.position, forward, float.PositiveInfinity, env_lm, out var hitInfo, out var portalTraversals, out var endPoint))
			{
				component2.SetPosition(1, hitInfo.point);
				Vector3 vector2 = ((portalTraversals.Length != 0) ? portalTraversals[^1].exitDirection : forward);
				position = hitInfo.point - vector2;
				if (SceneHelper.IsStaticEnvironment(hitInfo))
				{
					MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(hitInfo);
				}
			}
			else
			{
				component2.SetPosition(1, endPoint);
				UnityEngine.Object.Destroy(gameObject);
			}
			this.GenerateLineRendererSegments(component2, portalTraversals);
		}
		if ((bool)gameObject)
		{
			gameObject.transform.position = position;
			gameObject.SetActive(value: true);
			Coin component3 = gameObject.GetComponent<Coin>();
			if ((bool)component3)
			{
				component3.shot = false;
				if (component3.power < 5f || (!flag && !flag2))
				{
					component3.power += 1f;
				}
				gameObject.name = "NewCoin+" + (component3.power - 2f);
			}
			Rigidbody component4 = gameObject.GetComponent<Rigidbody>();
			if ((bool)component4)
			{
				component4.isKinematic = false;
				component4.velocity = Vector3.zero;
				component4.AddForce(Vector3.up * 25f, ForceMode.VelocityChange);
				component4.SetGravityMode(useGravity: true);
			}
		}
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
	}

	public void Bounce()
	{
		if (!shot)
		{
			if ((bool)currentCharge)
			{
				UnityEngine.Object.Destroy(currentCharge);
			}
			GameObject obj = UnityEngine.Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity);
			obj.name = "NewCoin+" + (power - 2f);
			obj.SetActive(value: false);
			Vector3 position = base.transform.position;
			obj.transform.position = position;
			obj.SetActive(value: true);
			scol.enabled = false;
			UntrackCoin();
			shot = true;
			Coin component = obj.GetComponent<Coin>();
			if ((bool)component)
			{
				component.shot = false;
			}
			Rigidbody component2 = obj.GetComponent<Rigidbody>();
			if ((bool)component2)
			{
				component2.isKinematic = false;
				component2.velocity = Vector3.zero;
				component2.AddForce(Vector3.up * 25f, ForceMode.VelocityChange);
				component2.SetGravityMode(useGravity: true);
			}
			base.gameObject.SetActive(value: false);
			new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
			CancelInvoke("GetDeleted");
		}
	}

	public void DelayedEnemyReflect()
	{
		if (!shot)
		{
			shotByEnemy = true;
			wasShotByEnemy = true;
			CancelInvoke("TripleTime");
			CancelInvoke("TripleTimeEnd");
			CancelInvoke("DoubleTime");
			ricochets++;
			if (!ccc)
			{
				GameObject gameObject = new GameObject();
				ccc = gameObject.AddComponent<CoinChainCache>();
				ccc.target = customTarget;
				gameObject.AddComponent<RemoveOnTime>().time = 5f;
			}
			rb.isKinematic = true;
			shot = true;
			Invoke("EnemyReflect", 0.1f);
		}
	}

	public void EnemyReflect()
	{
		bool flag = false;
		if (MonoSingleton<CoinTracker>.Instance.revolverCoinsList.Count > 1)
		{
			vision.UpdateSourcePos(base.transform.position);
			if (vision.TrySee(coinQuery, out var data))
			{
				flag = true;
				Coin component = data.target.GameObject.GetComponent<Coin>();
				component.power = power + 1f;
				component.ricochets += ricochets;
				if (quickDraw)
				{
					component.quickDraw = true;
				}
				AudioSource[] array = null;
				if ((bool)ccc)
				{
					component.ccc = ccc;
				}
				else
				{
					GameObject gameObject = new GameObject();
					ccc = gameObject.AddComponent<CoinChainCache>();
					component.ccc = ccc;
					gameObject.AddComponent<RemoveOnTime>().time = 5f;
				}
				component.DelayedEnemyReflect();
				LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
				array = component2.GetComponents<AudioSource>();
				if (hitPoint == Vector3.zero)
				{
					component2.SetPosition(0, base.transform.position);
				}
				else
				{
					component2.SetPosition(0, hitPoint);
				}
				component2.SetPosition(1, data.target.Position);
				Gradient gradient = new Gradient();
				gradient.SetKeys(new GradientColorKey[2]
				{
					new GradientColorKey(Color.red, 0f),
					new GradientColorKey(Color.red, 1f)
				}, new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(1f, 1f)
				});
				component2.colorGradient = gradient;
				data.IsObstructed(base.transform.position, empty_lm, toHead: false, out var _, out var traversals);
				this.GenerateLineRendererSegments(component2, traversals);
				if (power > 2f)
				{
					AudioSource[] array2 = array;
					foreach (AudioSource obj in array2)
					{
						obj.SetPitch(1f + (power - 2f) / 5f);
						obj.Play(tracked: true);
					}
				}
			}
		}
		if (!flag)
		{
			Invoke("ShootAtPlayer", 0.5f);
			if ((bool)scol)
			{
				scol.radius = 20f;
			}
			if ((bool)enemyFlash)
			{
				UnityEngine.Object.Instantiate(enemyFlash, base.transform.position, Quaternion.identity).transform.SetParent(base.transform, worldPositionStays: true);
			}
		}
		else
		{
			shotByEnemy = false;
			base.gameObject.SetActive(value: false);
			new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
			CancelInvoke("GetDeleted");
			scol.enabled = false;
			UntrackCoin();
		}
	}

	private void ShootAtPlayer()
	{
		scol.enabled = false;
		UntrackCoin();
		TargetData? targetData = null;
		Vector3 vector;
		TargetDataRef data;
		if (customTarget == null && ccc != null)
		{
			customTarget = ccc.target;
			vector = customTarget.position;
		}
		else if (vision.TrySee(playerQuery, out data))
		{
			vector = data.position;
			targetData = data.ToData();
		}
		else
		{
			vector = MonoSingleton<CameraController>.Instance.GetDefaultPos();
		}
		if (difficulty < 0)
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		if (difficulty <= 2)
		{
			Vector3 vector2 = ((customTarget != null) ? customTarget.GetVelocity() : ((!targetData.HasValue) ? MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().normalized : targetData.Value.velocity));
			vector -= vector2 * ((float)(3 - difficulty) / 1.5f);
		}
		PhysicsCastResult hitInfo;
		PortalTraversalV2[] portalTraversals;
		Vector3 endPoint;
		Vector3 position = ((!PortalPhysicsV2.Raycast(base.transform.position, vector - base.transform.position, float.PositiveInfinity, env_lm, out hitInfo, out portalTraversals, out endPoint)) ? endPoint : hitInfo.point);
		PhysicsCastResult hitInfo2;
		if (customTarget == null || customTarget.isPlayer)
		{
			NewMovement instance = MonoSingleton<NewMovement>.Instance;
			if (instance.gameObject.layer != 15 && PortalPhysicsV2.Raycast(base.transform.position, vector - base.transform.position, hitInfo.distance, 4))
			{
				instance.GetHurt(Mathf.RoundToInt(7.5f * power), invincible: true);
			}
		}
		else if (PortalPhysicsV2.Raycast(base.transform.position, vector - base.transform.position, out hitInfo2, hitInfo.distance, 1024))
		{
			EnemyIdentifierIdentifier component = hitInfo2.collider.GetComponent<EnemyIdentifierIdentifier>();
			if (component != null && component.eid == customTarget.enemyIdentifier)
			{
				customTarget.enemyIdentifier.SimpleDamage(Mathf.RoundToInt(7.5f * power));
			}
		}
		LineRenderer component2 = SpawnBeam().GetComponent<LineRenderer>();
		AudioSource[] components = component2.GetComponents<AudioSource>();
		if (hitPoint == Vector3.zero)
		{
			component2.SetPosition(0, base.transform.position);
		}
		else
		{
			component2.SetPosition(0, hitPoint);
		}
		component2.SetPosition(1, position);
		Gradient gradient = new Gradient();
		gradient.SetKeys(new GradientColorKey[2]
		{
			new GradientColorKey(Color.red, 0f),
			new GradientColorKey(Color.red, 1f)
		}, new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		});
		component2.colorGradient = gradient;
		component2.widthMultiplier *= 2f;
		this.GenerateLineRendererSegments(component2, portalTraversals);
		if (power > 2f)
		{
			AudioSource[] array = components;
			foreach (AudioSource obj in array)
			{
				obj.SetPitch(1f + (power - 2f) / 5f);
				obj.Play(tracked: true);
			}
		}
		base.gameObject.SetActive(value: false);
		new GameObject().AddComponent<CoinCollector>().coin = base.gameObject;
		CancelInvoke("GetDeleted");
		shotByEnemy = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (LayerMaskDefaults.IsMatchingLayer(collision.gameObject.layer, LMD.Environment))
		{
			GoreZone componentInParent = collision.transform.GetComponentInParent<GoreZone>();
			if (componentInParent != null)
			{
				base.transform.SetParent(componentInParent.gibZone, worldPositionStays: true);
			}
			GetDeleted();
		}
	}

	public void GetDeleted()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UnityEngine.Object.Instantiate(coinBreak, base.transform.position, Quaternion.identity);
		}
		GetComponentInChildren<MeshRenderer>().material = uselessMaterial;
		AudioLowPassFilter[] componentsInChildren = GetComponentsInChildren<AudioLowPassFilter>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			AudioLowPassFilter[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)array[i]);
			}
		}
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)GetComponent<AudioSource>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)base.transform.GetChild(1).GetComponent<AudioSource>());
		UnityEngine.Object.Destroy(GetComponent<TrailRenderer>());
		UnityEngine.Object.Destroy(scol);
		if (TryGetComponent<Zappable>(out var component))
		{
			UnityEngine.Object.Destroy(component);
		}
		base.gameObject.AddComponent<RemoveOnTime>().time = 5f;
		if ((bool)currentCharge)
		{
			UnityEngine.Object.Destroy(currentCharge);
		}
		UnityEngine.Object.Destroy(this);
	}

	private void StartCheckingSpeed()
	{
		Collider[] array = cols;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		checkingSpeed = true;
	}

	private GameObject SpawnBeam()
	{
		GameObject obj = UnityEngine.Object.Instantiate(refBeam, base.transform.position, Quaternion.identity);
		obj.GetComponent<RevolverBeam>().sourceWeapon = sourceWeapon;
		return obj;
	}

	public void RicoshotPointsCheck()
	{
		string text = "";
		int num = 50;
		if (altBeam != null && altBeam.TryGetComponent<RevolverBeam>(out var component) && component.ultraRicocheter)
		{
			text = "<color=orange>ULTRA</color>";
			num += 50;
		}
		if (wasShotByEnemy)
		{
			text += "<color=red>COUNTER</color>";
			num += 50;
		}
		if (ricochets > 1)
		{
			num += ricochets * 15;
		}
		StyleHUD styleHUD = shud;
		int points = num;
		string prefix = text;
		styleHUD.AddPoints(points, "ultrakill.ricoshot", sourceWeapon, eid, ricochets, prefix);
	}

	public void SetData(ref TargetData data)
	{
		data.position = cachedPos;
		data.realPosition = cachedPos;
		data.rotation = cachedRot;
		data.velocity = cachedVel;
	}

	public void UpdateCachedTransformData()
	{
		cachedPos = (rb ? rb.position : base.transform.position);
		cachedRot = (rb ? rb.rotation : base.transform.rotation);
		cachedVel = (rb ? rb.velocity : Vector3.zero);
	}
}
