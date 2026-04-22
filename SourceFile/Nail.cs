using System;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;

public class Nail : MonoBehaviour
{
	public GameObject sourceWeapon;

	[HideInInspector]
	public bool hit;

	public float damage;

	private AudioSource aud;

	[HideInInspector]
	public Rigidbody rb;

	public AudioClip environmentHitSound;

	public AudioClip enemyHitSound;

	public Material zapMaterial;

	public GameObject zapParticle;

	private bool zapped;

	public bool fodderDamageBoost;

	public string weaponType;

	public bool heated;

	[HideInInspector]
	public List<MagnetInfo> magnets = new List<MagnetInfo>();

	private bool launched;

	[HideInInspector]
	public NailBurstController nbc;

	public bool enemy;

	public EnemyType safeEnemyType;

	private Vector3 startPosition;

	[Header("Sawblades")]
	public bool sawblade;

	public bool chainsaw;

	public float hitAmount = 3.9f;

	private EnemyIdentifier currentHitEnemy;

	private EnemyIdentifier previouslyHitEnemy;

	private float sameEnemyHitCooldown;

	[SerializeField]
	private GameObject sawBreakEffect;

	[SerializeField]
	private GameObject sawBounceEffect;

	[SerializeField]
	private GameObject sawHitEffect;

	[HideInInspector]
	public int magnetRotationDirection;

	private List<Transform> hitLimbs = new List<Transform>();

	private float removeTimeMultiplier = 1f;

	public bool bounceToSurfaceNormal;

	[HideInInspector]
	public bool stopped;

	public int multiHitAmount = 1;

	private int currentMultiHitAmount;

	private float multiHitCooldown;

	private Transform hitTarget;

	[HideInInspector]
	public Vector3 originalVelocity;

	public AudioSource stoppedAud;

	[HideInInspector]
	public bool punchable;

	[HideInInspector]
	public bool punched;

	[HideInInspector]
	public float punchDistance;

	private TimeSince sinceLastEnviroParticle;

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
	}

	private void Start()
	{
		if (sawblade)
		{
			removeTimeMultiplier = 3f;
		}
		if (magnets.Count == 0)
		{
			Invoke("RemoveTime", 5f * removeTimeMultiplier);
		}
		sinceLastEnviroParticle = 1f;
		Invoke("MasterRemoveTime", 60f);
		startPosition = base.transform.position;
		Invoke("SlowUpdate", 2f);
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _) && !TryGetComponent<SimplePortalTraveler>(out var _))
		{
			base.gameObject.AddComponent<SimplePortalTraveler>().SetType(PortalTravellerType.PLAYER_PROJECTILE);
		}
	}

	private void OnDestroy()
	{
		if (base.gameObject.scene.isLoaded && zapped)
		{
			UnityEngine.Object.Instantiate(zapParticle, base.transform.position, base.transform.rotation);
		}
	}

	private void SlowUpdate()
	{
		if (Vector3.Distance(base.transform.position, startPosition) > 1000f)
		{
			RemoveTime();
		}
		else
		{
			Invoke("SlowUpdate", 2f);
		}
	}

	private void Update()
	{
		if (!hit)
		{
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			if ((bool)rb)
			{
				base.transform.LookAt(base.transform.position + rb.velocity * -1f);
			}
		}
		if (sameEnemyHitCooldown > 0f && !stopped)
		{
			sameEnemyHitCooldown = Mathf.MoveTowards(sameEnemyHitCooldown, 0f, Time.deltaTime);
			if (sameEnemyHitCooldown <= 0f)
			{
				currentHitEnemy = null;
			}
		}
		if (multiHitAmount <= 1)
		{
			return;
		}
		if (multiHitCooldown > 0f)
		{
			multiHitCooldown = Mathf.MoveTowards(multiHitCooldown, 0f, Time.deltaTime);
		}
		else if (stopped)
		{
			if (currentHitEnemy != null && !currentHitEnemy.dead && currentMultiHitAmount > 0)
			{
				currentMultiHitAmount--;
				hitAmount -= 1f;
				DamageEnemy(hitTarget, currentHitEnemy);
			}
			if (currentHitEnemy == null || currentHitEnemy.dead || currentMultiHitAmount <= 0)
			{
				stopped = false;
				rb.velocity = originalVelocity;
				if (hitAmount <= 0f)
				{
					SawBreak();
				}
				return;
			}
			multiHitCooldown = 0.15f;
		}
		if ((bool)(UnityEngine.Object)(object)stoppedAud)
		{
			if (stopped)
			{
				stoppedAud.SetPitch(2f);
				stoppedAud.SetPitch(0.5f);
			}
			else
			{
				stoppedAud.SetPitch(1f);
				stoppedAud.volume = 0.25f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!sawblade || !rb || hit)
		{
			return;
		}
		if (stopped)
		{
			rb.velocity = Vector3.zero;
			return;
		}
		if (magnets.Count > 0)
		{
			magnets.RemoveAll((MagnetInfo magInfo) => magInfo.magnet == null);
			if (magnets.Count == 0)
			{
				return;
			}
			MagnetInfo targetMagnet = GetTargetMagnet();
			if (!targetMagnet.magnet)
			{
				return;
			}
			Vector3 worldPosition = targetMagnet.GetWorldPosition();
			if (punched)
			{
				if (Vector3.Distance(base.transform.position, worldPosition) > punchDistance)
				{
					punchDistance = 0f;
					rb.velocity = Vector3.RotateTowards(rb.velocity, Quaternion.Euler(0f, 85 * magnetRotationDirection, 0f) * (worldPosition - base.transform.position).normalized * rb.velocity.magnitude, float.PositiveInfinity, rb.velocity.magnitude);
				}
			}
			else
			{
				rb.velocity = Vector3.RotateTowards(rb.velocity, Quaternion.Euler(0f, 85 * magnetRotationDirection, 0f) * (worldPosition - base.transform.position).normalized * rb.velocity.magnitude, float.PositiveInfinity, rb.velocity.magnitude);
			}
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.5f, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Ignore);
		if (array != null && array.Length != 0)
		{
			for (int num = 0; num < array.Length; num++)
			{
				GameObject gameObject = ((array[num].attachedRigidbody != null) ? array[num].attachedRigidbody.gameObject : array[num].gameObject);
				if (!hit && (gameObject.layer == 10 || gameObject.layer == 11) && (gameObject.gameObject.CompareTag("Head") || gameObject.gameObject.CompareTag("Body") || gameObject.gameObject.CompareTag("Limb") || gameObject.gameObject.CompareTag("EndLimb") || gameObject.gameObject.CompareTag("Enemy")))
				{
					TouchEnemy(gameObject.transform);
				}
			}
		}
		RaycastHit[] array2 = rb.SweepTestAll(rb.velocity.normalized, rb.velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Ignore);
		if (array2 == null || array2.Length == 0)
		{
			return;
		}
		Array.Sort(array2, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		bool flag = false;
		bool flag2 = false;
		for (int num2 = 0; num2 < array2.Length; num2++)
		{
			GameObject gameObject2 = array2[num2].transform.gameObject;
			if (!hit && (gameObject2.layer == 10 || gameObject2.layer == 11) && (gameObject2.gameObject.CompareTag("Head") || gameObject2.gameObject.CompareTag("Body") || gameObject2.gameObject.CompareTag("Limb") || gameObject2.gameObject.CompareTag("EndLimb") || gameObject2.gameObject.CompareTag("Enemy")))
			{
				TouchEnemy(gameObject2.transform);
			}
			else
			{
				if (!LayerMaskDefaults.IsMatchingLayer(gameObject2.layer, LMD.Environment) && gameObject2.layer != 26 && !gameObject2.CompareTag("Armor"))
				{
					continue;
				}
				if (gameObject2.TryGetComponent<Breakable>(out var component) && ((component.weak && !component.specialCaseOnly) || (component.forceSawbladeable && !chainsaw)))
				{
					if (component.forceSawbladeable)
					{
						component.ForceBreak();
					}
					else
					{
						component.Break(damage);
					}
					return;
				}
				if (hitAmount <= 0f)
				{
					SawBreak();
					return;
				}
				base.transform.position = array2[num2].point;
				if (bounceToSurfaceNormal)
				{
					rb.velocity = array2[num2].normal * rb.velocity.magnitude;
				}
				else
				{
					rb.velocity = Vector3.Reflect(rb.velocity.normalized, array2[num2].normal) * rb.velocity.magnitude;
				}
				flag = true;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(sawBounceEffect, array2[num2].point, Quaternion.LookRotation(array2[num2].normal));
				if (flag2 && gameObject3.TryGetComponent<AudioSource>(out var component2))
				{
					((Behaviour)(object)component2).enabled = false;
				}
				else
				{
					flag2 = true;
				}
				if (SceneHelper.IsStaticEnvironment(array2[num2]) && (float)sinceLastEnviroParticle > 0.25f)
				{
					MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(array2[num2], 3, 0.5f);
					sinceLastEnviroParticle = 0f;
				}
				punchable = true;
				sameEnemyHitCooldown = 0f;
				currentHitEnemy = null;
				if (magnets.Count > 0)
				{
					magnetRotationDirection *= -1;
					hitAmount -= 0.1f;
				}
				else
				{
					hitAmount -= 0.25f;
				}
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int num3 = 0; num3 < 3; num3++)
		{
			if (!Physics.Raycast(base.transform.position, rb.velocity.normalized, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				break;
			}
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component3) && ((component3.weak && !component3.specialCaseOnly) || (component3.forceSawbladeable && !chainsaw)))
			{
				if (component3.forceSawbladeable)
				{
					component3.ForceBreak();
				}
				else
				{
					component3.Break(damage);
				}
				break;
			}
			base.transform.position = hitInfo.point;
			if (bounceToSurfaceNormal)
			{
				rb.velocity = hitInfo.normal * rb.velocity.magnitude;
			}
			else
			{
				rb.velocity = Vector3.Reflect(rb.velocity.normalized, hitInfo.normal) * rb.velocity.magnitude;
			}
			hitAmount -= 0.125f;
			GameObject gameObject4 = UnityEngine.Object.Instantiate(sawBounceEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			if (flag2 && gameObject4.TryGetComponent<AudioSource>(out var component4))
			{
				((Behaviour)(object)component4).enabled = false;
			}
			else
			{
				flag2 = true;
			}
			if (SceneHelper.IsStaticEnvironment(hitInfo) && (float)sinceLastEnviroParticle > 0.25f)
			{
				MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(hitInfo, 3, 0.5f);
				sinceLastEnviroParticle = 0f;
			}
			punchable = true;
			sameEnemyHitCooldown = 0f;
			currentHitEnemy = null;
		}
	}

	public MagnetInfo GetTargetMagnet()
	{
		MagnetInfo result = default(MagnetInfo);
		float num = float.PositiveInfinity;
		PortalScene portalScene = null;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			portalScene = instance.Scene;
		}
		for (int i = 0; i < magnets.Count; i++)
		{
			Vector3 lhs = magnets[i].GetWorldPosition(portalScene) - base.transform.position;
			float sqrMagnitude = lhs.sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = magnets[i];
				Vector3 normalized = new Vector3(rb.velocity.z, rb.velocity.y, 0f - rb.velocity.x).normalized;
				if (Vector3.Dot(lhs, normalized) > 0f)
				{
					magnetRotationDirection = -1;
				}
				else
				{
					magnetRotationDirection = 1;
				}
			}
		}
		return result;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (hit)
		{
			return;
		}
		GameObject gameObject = other.gameObject;
		if ((gameObject.layer == 10 || gameObject.layer == 11) && (gameObject.CompareTag("Head") || gameObject.CompareTag("Body") || gameObject.CompareTag("Limb") || gameObject.CompareTag("EndLimb") || gameObject.CompareTag("Enemy")))
		{
			TouchEnemy(other.transform);
		}
		else if (enemy && gameObject.layer == 2)
		{
			MonoSingleton<NewMovement>.Instance.GetHurt(8, invincible: true);
			hit = true;
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			if (magnets.Count != 0 || !LayerMaskDefaults.IsMatchingLayer(gameObject.layer, LMD.Environment))
			{
				return;
			}
			hit = true;
			CancelInvoke("RemoveTime");
			Invoke("RemoveTime", 1f);
			if (SceneHelper.IsStaticEnvironment(other.collider) && (float)sinceLastEnviroParticle > 0.25f)
			{
				MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(other.GetContact(0), 1, 0.5f);
				sinceLastEnviroParticle = 0f;
			}
			if ((UnityEngine.Object)(object)aud == null)
			{
				aud = GetComponent<AudioSource>();
			}
			aud.clip = environmentHitSound;
			aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
			aud.volume = 0.2f;
			aud.Play(tracked: true);
			Breakable component = gameObject.GetComponent<Breakable>();
			if (component != null && (((component.weak || heated) && !component.precisionOnly && !component.specialCaseOnly && !component.forceSawbladeable) || (sawblade && !chainsaw && component.forceSawbladeable)))
			{
				if (component.forceSawbladeable)
				{
					component.ForceBreak();
				}
				else
				{
					component.Break(damage);
				}
			}
			if (gameObject.TryGetComponent<Bleeder>(out var component2))
			{
				component2.GetHit(base.transform.position, GoreType.Small);
			}
			if (heated)
			{
				Flammable componentInChildren = gameObject.GetComponentInChildren<Flammable>();
				if (componentInChildren != null && (enemy || !componentInChildren.enemyOnly) && (!enemy || !componentInChildren.playerOnly))
				{
					componentInChildren.Burn(2f);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!sawblade && !hit && (other.gameObject.layer == 10 || other.gameObject.layer == 11) && (other.gameObject.CompareTag("Head") || other.gameObject.CompareTag("Body") || other.gameObject.CompareTag("Limb") || other.gameObject.CompareTag("EndLimb") || other.gameObject.CompareTag("Enemy")))
		{
			hit = true;
			TouchEnemy(other.transform);
		}
	}

	private void TouchEnemy(Transform other)
	{
		if (sawblade && multiHitAmount > 1)
		{
			if (stopped || !other.TryGetComponent<EnemyIdentifierIdentifier>(out var component) || !component.eid)
			{
				return;
			}
			if (component.eid.dead)
			{
				HitEnemy(other, component);
			}
			else
			{
				if (sameEnemyHitCooldown > 0f && currentHitEnemy != null && currentHitEnemy == component.eid)
				{
					return;
				}
				stopped = true;
				currentMultiHitAmount = multiHitAmount;
				hitTarget = other;
				currentHitEnemy = component.eid;
				if (previouslyHitEnemy != currentHitEnemy)
				{
					if (previouslyHitEnemy != null)
					{
						punched = false;
					}
					previouslyHitEnemy = currentHitEnemy;
				}
				originalVelocity = rb.velocity;
				sameEnemyHitCooldown = 0.15f;
			}
		}
		else
		{
			HitEnemy(other);
		}
	}

	private void HitEnemy(Transform other, EnemyIdentifierIdentifier eidid = null)
	{
		if ((!eidid && !other.TryGetComponent<EnemyIdentifierIdentifier>(out eidid)) || !eidid.eid || (enemy && (bool)eidid && (bool)eidid.eid && eidid.eid.enemyType == safeEnemyType) || (sawblade && ((sameEnemyHitCooldown > 0f && currentHitEnemy != null && currentHitEnemy == eidid.eid) || hitLimbs.Contains(other))))
		{
			return;
		}
		if (!sawblade)
		{
			hit = true;
		}
		else if (!eidid.eid.dead)
		{
			sameEnemyHitCooldown = 0.15f;
			currentHitEnemy = eidid.eid;
			hitAmount -= 1f;
		}
		if ((UnityEngine.Object)(object)aud == null)
		{
			aud = GetComponent<AudioSource>();
		}
		aud.clip = enemyHitSound;
		aud.SetPitch(UnityEngine.Random.Range(0.9f, 1.1f));
		aud.volume = 0.2f;
		aud.Play(tracked: true);
		if ((bool)eidid && (bool)eidid.eid)
		{
			if (sawblade && eidid.eid.zapperer != null)
			{
				eidid.eid.zapperer.damage += 0.5f;
				eidid.eid.zapperer.ChargeBoost(0.5f);
			}
			DamageEnemy(other, eidid.eid);
		}
		if (sawblade)
		{
			if (hitAmount < 1f)
			{
				SawBreak();
			}
			return;
		}
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		rb.isKinematic = true;
		UnityEngine.Object.Destroy(rb);
		base.transform.position += base.transform.forward * -0.5f;
		base.transform.SetParent(other.transform, worldPositionStays: true);
		UnityEngine.Object.Destroy(GetComponent<SimplePortalTraveler>());
		if (TryGetComponent<TrailRenderer>(out var component))
		{
			component.enabled = false;
		}
		CancelInvoke("RemoveTime");
	}

	private void DamageEnemy(Transform other, EnemyIdentifier eid)
	{
		if (!sawblade)
		{
			eid.hitter = "nail";
		}
		else if (chainsaw)
		{
			eid.hitter = "chainsawprojectile";
		}
		else
		{
			eid.hitter = "sawblade";
		}
		if (!eid.hitterWeapons.Contains(weaponType))
		{
			eid.hitterWeapons.Add(weaponType);
		}
		if (sawHitEffect != null)
		{
			UnityEngine.Object.Instantiate(sawHitEffect, other.transform.position, Quaternion.identity).transform.localScale *= 3f;
		}
		bool flag = false;
		if (magnets.Count > 0)
		{
			foreach (MagnetInfo magnet in magnets)
			{
				if (magnet.magnet.ignoredEids.Contains(eid))
				{
					flag = true;
					break;
				}
			}
		}
		bool dead = eid.dead;
		if (fodderDamageBoost && !eid.dead)
		{
			damage *= GetFodderDamageMultiplier(eid.enemyType);
		}
		if ((bool)nbc && !sawblade)
		{
			if (!nbc.damagedEnemies.Contains(eid))
			{
				eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 3000f, base.transform.position, damage * (float)(nbc.originalNailCount / 2) * (float)((!punched) ? 1 : 2), tryForExplode: true, 0f, sourceWeapon);
				nbc.damagedEnemies.Add(eid);
			}
		}
		else
		{
			eid.DeliverDamage(other.gameObject, (other.transform.position - base.transform.position).normalized * 3000f, base.transform.position, damage * (float)((!punched) ? 1 : 2), tryForExplode: false, 0f, sourceWeapon);
		}
		if (!dead && eid.dead && !flag && magnets.Count > 0)
		{
			if (magnets.Count > 1)
			{
				StyleHUD? instance = MonoSingleton<StyleHUD>.Instance;
				int points = Mathf.RoundToInt(120f);
				EnemyIdentifier eid2 = eid;
				instance.AddPoints(points, "ultrakill.bipolar", sourceWeapon, eid2);
			}
			else
			{
				StyleHUD? instance2 = MonoSingleton<StyleHUD>.Instance;
				int points2 = Mathf.RoundToInt(60f);
				EnemyIdentifier eid2 = eid;
				instance2.AddPoints(points2, "ultrakill.attripator", sourceWeapon, eid2);
			}
		}
		else if (launched && !sawblade)
		{
			if (!dead && eid.dead)
			{
				StyleHUD? instance3 = MonoSingleton<StyleHUD>.Instance;
				int points3 = Mathf.RoundToInt(120f);
				EnemyIdentifier eid2 = eid;
				instance3.AddPoints(points3, "ultrakill.nailbombed", sourceWeapon, eid2);
			}
			else if (!eid.dead)
			{
				StyleHUD? instance4 = MonoSingleton<StyleHUD>.Instance;
				int points4 = Mathf.RoundToInt(10f);
				EnemyIdentifier eid2 = eid;
				instance4.AddPoints(points4, "ultrakill.nailbombedalive", sourceWeapon, eid2);
			}
		}
		if (!dead && !sawblade)
		{
			eid.nailsAmount++;
			eid.nails.Add(this);
			if (eid.mirrorOnly)
			{
				EnemyIdentifier.SendToPortalLayer(base.gameObject);
			}
		}
		else if (dead && sawblade)
		{
			hitLimbs.Add(other);
		}
		if (heated)
		{
			Flammable componentInChildren = eid.GetComponentInChildren<Flammable>();
			if (componentInChildren != null && (enemy || !componentInChildren.enemyOnly) && (!enemy || !componentInChildren.playerOnly))
			{
				componentInChildren.Burn(2f, componentInChildren.burning);
			}
		}
		if (dead)
		{
			_ = magnets.Count;
		}
	}

	public void MagnetCaught(MagnetInfo mag)
	{
		for (int i = 0; i < magnets.Count; i++)
		{
			if (!(magnets[i].magnet != mag.magnet))
			{
				magnets[i] = mag;
				return;
			}
		}
		CancelInvoke("RemoveTime");
		launched = false;
		enemy = false;
		if (sawblade)
		{
			punchable = true;
		}
		magnets.Add(mag);
		if ((bool)nbc)
		{
			nbc.nails.Remove(this);
			nbc = null;
		}
	}

	public void MagnetRelease(MagnetInfo mag)
	{
		CancelInvoke("RemoveTime");
		for (int num = magnets.Count - 1; num >= 0; num--)
		{
			if (!(magnets[num].magnet != mag.magnet))
			{
				magnets.RemoveAt(num);
				break;
			}
		}
		if (magnets.Count == 0)
		{
			if (TryGetComponent<SphereCollider>(out var component))
			{
				component.enabled = true;
			}
			launched = true;
			Invoke("RemoveTime", 5f * removeTimeMultiplier);
		}
	}

	public void UpdateMagnetPath(MagnetInfo mag)
	{
		for (int i = 0; i < magnets.Count; i++)
		{
			if (!(magnets[i].magnet != mag.magnet))
			{
				magnets[i] = mag;
				break;
			}
		}
	}

	public void Zap()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.material = zapMaterial;
		}
		zapped = true;
	}

	private void RemoveTime()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void MasterRemoveTime()
	{
		RemoveTime();
	}

	public void SawBreak()
	{
		hit = true;
		UnityEngine.Object.Instantiate(sawBreakEffect, base.transform.position, Quaternion.identity);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private float GetFodderDamageMultiplier(EnemyType et)
	{
		return et switch
		{
			EnemyType.Filth => 2f, 
			EnemyType.Schism => 1.5f, 
			EnemyType.Soldier => 1.5f, 
			EnemyType.Stalker => 1.5f, 
			EnemyType.Stray => 2f, 
			EnemyType.Virtue => 1.25f, 
			EnemyType.Providence => 1.25f, 
			EnemyType.Power => 1.25f, 
			_ => 1f, 
		};
	}

	public void ForceCheckSawbladeRicochet()
	{
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (!Physics.Raycast(base.transform.position, rb.velocity.normalized, out var hitInfo, 5f, LayerMaskDefaults.Get(LMD.Environment)))
			{
				break;
			}
			if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && ((component.weak && !component.specialCaseOnly) || (component.forceSawbladeable && !chainsaw)))
			{
				if (component.forceSawbladeable)
				{
					component.ForceBreak();
				}
				else
				{
					component.Break(damage);
				}
				return;
			}
			base.transform.position = hitInfo.point;
			if (bounceToSurfaceNormal)
			{
				rb.velocity = hitInfo.normal * rb.velocity.magnitude;
			}
			else
			{
				rb.velocity = Vector3.Reflect(rb.velocity.normalized, hitInfo.normal) * rb.velocity.magnitude;
			}
			hitAmount -= 0.125f;
			GameObject gameObject = UnityEngine.Object.Instantiate(sawBounceEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			if (flag && gameObject.TryGetComponent<AudioSource>(out var component2))
			{
				((Behaviour)(object)component2).enabled = false;
			}
			else
			{
				flag = true;
			}
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 1.5f, LayerMaskDefaults.Get(LMD.Enemies));
		if (array.Length != 0)
		{
			TouchEnemy(array[0].transform);
		}
	}
}
