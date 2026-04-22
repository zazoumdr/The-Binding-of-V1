using System;
using System.Collections.Generic;
using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public static float globalSizeMulti = 1f;

	private static readonly Collider[] PortalOverlapBuffer = new Collider[32];

	public HurtCooldownCollection HurtCooldownCollection;

	public GameObject sourceWeapon;

	public Vector3? playerProjectileForceDirection;

	public bool enemy;

	public bool harmless;

	public bool lowQuality;

	private CameraController cc;

	private Light light;

	private MeshRenderer mr;

	private Color materialColor;

	private Material originalMaterial;

	private TimeSince explosionTime;

	private bool whiteExplosion;

	private bool fading;

	public float speed;

	public float maxSize;

	public float pushForceMultiplier = 1f;

	private LayerMask lmask;

	public int damage;

	public float enemyDamageMultiplier;

	[HideInInspector]
	public int playerDamageOverride = -1;

	public GameObject explosionChunk;

	public bool ignite;

	public bool friendlyFire;

	public bool isFup;

	public bool boosted;

	private HashSet<int> hitColliders = new HashSet<int>();

	public string hitterWeapon;

	public bool halved;

	private SphereCollider scol;

	public AffectedSubjects canHit;

	private bool hasHitPlayer;

	[HideInInspector]
	public EnemyIdentifier originEnemy;

	public bool rocketExplosion;

	public List<EnemyType> toIgnore;

	[HideInInspector]
	public EnemyIdentifier interruptedEnemy;

	[HideInInspector]
	public bool ultrabooster;

	public bool unblockable;

	public bool electric;

	private int enviroGibs;

	private void Start()
	{
		explosionTime = 0f;
		mr = GetComponent<MeshRenderer>();
		materialColor = mr.material.GetColor("_Color");
		originalMaterial = mr.sharedMaterial;
		mr.material = new Material(MonoSingleton<DefaultReferenceManager>.Instance.blankMaterial);
		whiteExplosion = true;
		cc = MonoSingleton<CameraController>.Instance;
		float num = Vector3.Distance(base.transform.position, cc.transform.position);
		float num2 = ((damage == 0) ? 0.25f : 1f);
		if (num < 3f * maxSize)
		{
			cc.CameraShake(1.5f * num2);
		}
		else if (num < 85f)
		{
			cc.CameraShake((1.5f - (num - 20f) / 65f * 1.5f) / 6f * maxSize * num2);
		}
		scol = GetComponent<SphereCollider>();
		if ((bool)scol)
		{
			scol.enabled = true;
		}
		if (speed == 0f)
		{
			speed = 1f;
		}
		if (!lowQuality)
		{
			lowQuality = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions");
		}
		ComponentsDatabase instance = MonoSingleton<ComponentsDatabase>.Instance;
		if ((bool)instance && instance.scrollers.Count > 0)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 1f, LayerMaskDefaults.Get(LMD.Environment));
			foreach (Collider collider in array)
			{
				if (instance.scrollers.Contains(collider.transform) && collider.transform.TryGetComponent<ScrollingTexture>(out var component))
				{
					component.attachedObjects.Add(base.transform);
				}
			}
		}
		if (!lowQuality)
		{
			light = GetComponentInChildren<Light>();
			light.enabled = true;
			if (explosionChunk != null)
			{
				for (int j = 0; j < UnityEngine.Random.Range(24, 30); j++)
				{
					GameObject obj = UnityEngine.Object.Instantiate(explosionChunk, base.transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)), UnityEngine.Random.rotation);
					Vector3 vector = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 2f), UnityEngine.Random.Range(-1f, 1f));
					obj.GetComponent<Rigidbody>().AddForce(vector * 250f, ForceMode.VelocityChange);
					Physics.IgnoreCollision(obj.GetComponent<Collider>(), scol);
				}
			}
		}
		lmask = LayerMaskDefaults.Get(LMD.Environment);
		lmask = (int)lmask | 0x4000000;
		speed *= globalSizeMulti;
		maxSize *= globalSizeMulti;
	}

	private void Update()
	{
		if (light != null)
		{
			light.range += 5f * Time.deltaTime * speed;
		}
		if (whiteExplosion && (float)explosionTime > 0.1f)
		{
			whiteExplosion = false;
			mr.material = new Material(originalMaterial);
		}
		if (fading)
		{
			materialColor.a -= 2f * Time.deltaTime;
			if (light != null)
			{
				light.intensity -= 65f * Time.deltaTime;
			}
			mr.material.SetColor("_Color", materialColor);
			if (materialColor.a <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void FixedUpdate()
	{
		base.transform.localScale += Vector3.one * 0.05f * speed;
		float num = base.transform.lossyScale.x * scol.radius;
		if (!fading && num > maxSize)
		{
			harmless = true;
			scol.enabled = false;
			fading = true;
			speed /= 4f;
		}
		if (!halved && num > maxSize / 2f)
		{
			halved = true;
			damage = Mathf.RoundToInt((float)damage / 1.5f);
		}
		if (!harmless && MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			PortalScene scene = instance.Scene;
			if (scene != null)
			{
				CheckPortalOverlaps(scene, num);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 9 && !harmless)
		{
			Collide(other, base.transform.position);
		}
	}

	private void Collide(Collider other, Vector3 origin)
	{
		Vector3 position = other.transform.position;
		Vector3 normalized = (position - origin).normalized;
		float num = Vector3.Distance(position, origin);
		Vector3 vector = origin - normalized * 0.01f;
		float maxDistance = Vector3.Distance(vector, position);
		int instanceID = other.GetInstanceID();
		EnemyIdentifierIdentifier componentInParent;
		EnemyIdentifier eid;
		if (!hitColliders.Contains(instanceID))
		{
			if (!hasHitPlayer && other.gameObject.CompareTag("Player"))
			{
				if (Physics.Raycast(vector, normalized, out var _, maxDistance, 2048, QueryTriggerInteraction.Ignore) || (enemy && Physics.Raycast(position, -normalized, num - 0.1f, lmask, QueryTriggerInteraction.Ignore)))
				{
					return;
				}
				hasHitPlayer = true;
				hitColliders.Add(instanceID);
				if (canHit != AffectedSubjects.EnemiesOnly)
				{
					if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && damage > 0)
					{
						MonoSingleton<PlatformerMovement>.Instance.Burn();
						return;
					}
					if (!MonoSingleton<NewMovement>.Instance.exploded && MonoSingleton<NewMovement>.Instance.explosionLaunchResistance <= 0f && (MonoSingleton<NewMovement>.Instance.safeExplosionLaunchCooldown <= 0f || damage > 0))
					{
						int num2 = 200;
						if (rocketExplosion && damage == 0)
						{
							num2 = Mathf.RoundToInt(100f / ((float)(MonoSingleton<NewMovement>.Instance.rocketJumps + 3) / 3f));
							MonoSingleton<NewMovement>.Instance.rocketJumps++;
						}
						Vector3 vector2 = origin - position;
						Vector3 position2 = ((new Vector3(vector2.x, 0f, vector2.z).sqrMagnitude < 0.0625f) ? position : origin);
						if (boosted)
						{
							Debug.Log($"YAY!! {Mathf.Max(60f, MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude)}");
							MonoSingleton<NewMovement>.Instance.LaunchFromPointAtSpeed(position2, Mathf.Max(60f, MonoSingleton<NewMovement>.Instance.rb.velocity.magnitude) * pushForceMultiplier);
						}
						else if (isFup)
						{
							MonoSingleton<NewMovement>.Instance.LaunchFromPointAtSpeed(position2, 60f * pushForceMultiplier);
						}
						else if (rocketExplosion && damage <= 0)
						{
							MonoSingleton<NewMovement>.Instance.windState = 0.75f;
							MonoSingleton<NewMovement>.Instance.LaunchFromPoint(position2, (float)num2 * pushForceMultiplier, maxSize);
						}
						else
						{
							MonoSingleton<NewMovement>.Instance.LaunchFromPoint(position2, (float)num2 * pushForceMultiplier, maxSize);
							if (ultrabooster && num < 12f)
							{
								MonoSingleton<NewMovement>.Instance.LaunchFromPoint(position2, (float)num2 * pushForceMultiplier, maxSize);
							}
						}
						if (damage <= 0)
						{
							MonoSingleton<NewMovement>.Instance.safeExplosionLaunchCooldown = 0.5f;
						}
					}
					if (damage > 0)
					{
						int num3 = damage;
						if (ultrabooster)
						{
							num3 = ((num < 3f) ? 35 : 50);
						}
						num3 = ((playerDamageOverride >= 0) ? playerDamageOverride : num3);
						MonoSingleton<NewMovement>.Instance.GetHurt(num3, invincible: true, enemy ? 1 : 0, explosion: true);
					}
				}
			}
			else if ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && canHit != AffectedSubjects.PlayerOnly)
			{
				componentInParent = other.GetComponentInParent<EnemyIdentifierIdentifier>();
				if ((bool)componentInParent && (bool)componentInParent.eid)
				{
					eid = componentInParent.eid;
					if (!eid.dead && eid.TryGetComponent<Collider>(out var component))
					{
						int instanceID2 = component.GetInstanceID();
						if (hitColliders.Add(instanceID2) && (HurtCooldownCollection == null || HurtCooldownCollection.TryHurtCheckEnemy(eid)))
						{
							EnemyType enemyType = eid.enemyType;
							if (enemyType != EnemyType.MaliciousFace)
							{
								if (enemyType != EnemyType.Idol && enemyType != EnemyType.Deathcatcher)
								{
									goto IL_04a8;
								}
								if (!Physics.Linecast(origin, component.bounds.center, LayerMaskDefaults.Get(LMD.Environment)))
								{
									eid.hitter = hitterWeapon;
									eid.DeliverDamage(other.gameObject, Vector3.zero, position, 1f, tryForExplode: false, 0f, sourceWeapon, ignoreTotalDamageTakenMultiplier: false, fromExplosion: true);
								}
							}
							else
							{
								if (eid.isGasolined)
								{
									goto IL_04a8;
								}
								UnityEngine.Object.Instantiate(MonoSingleton<DefaultReferenceManager>.Instance.ineffectiveSound, position, Quaternion.identity);
							}
						}
					}
					else if (eid.dead)
					{
						hitColliders.Add(instanceID);
						eid.hitter = (enemy ? "enemy" : "explosion");
						eid.DeliverDamage(other.gameObject, (playerProjectileForceDirection ?? normalized) * 5000f, position, (float)damage / 10f * enemyDamageMultiplier, tryForExplode: false, 0f, sourceWeapon, ignoreTotalDamageTakenMultiplier: false, fromExplosion: true);
						if (ignite && componentInParent.TryGetComponent<Flammable>(out var _))
						{
							Flammable componentInChildren = eid.GetComponentInChildren<Flammable>();
							if (componentInChildren != null)
							{
								componentInChildren.Burn(damage / 10);
							}
						}
					}
				}
			}
			else
			{
				if (SceneHelper.IsStaticEnvironment(other))
				{
					enviroGibs++;
					Vector3 vector3 = other.ClosestPoint(origin);
					Vector3 normalized2 = (vector3 - origin).normalized;
					float num4 = Vector3.Distance(origin, vector3);
					float num5 = 1f;
					if (enemyDamageMultiplier > 1f)
					{
						num5 = Mathf.Min(enemyDamageMultiplier, 2f);
					}
					MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(vector3 - normalized2, normalized2, 5f, Mathf.RoundToInt(Mathf.Lerp(10f, 2f, num4 / maxSize) * num5) / enviroGibs, Mathf.Lerp(2f, 0.5f, num4 / maxSize) * num5 / (float)enviroGibs);
				}
				Bleeder component4;
				Glass component5;
				Flammable component6;
				if ((other.TryGetComponent<Breakable>(out var component3) || ((bool)other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<Breakable>(out component3))) && !component3.unbreakable && !component3.precisionOnly && (!component3.playerOnly || !enemy) && (!component3.ignoreExplosions || (component3.forceKnuckleblasterable && hitterWeapon == "heavypunch")) && !component3.specialCaseOnly)
				{
					if (!component3.accurateExplosionsOnly)
					{
						component3.Break(damage);
					}
					else
					{
						Vector3 vector4 = other.ClosestPoint(origin);
						if (!Physics.Raycast(vector4 + (vector4 - origin).normalized * 0.001f, origin - vector4, Vector3.Distance(origin, vector4), lmask, QueryTriggerInteraction.Ignore))
						{
							component3.Break(damage);
						}
					}
				}
				else if (other.TryGetComponent<Bleeder>(out component4))
				{
					bool flag = false;
					if (toIgnore.Count > 0 && component4.ignoreTypes.Length != 0)
					{
						EnemyType[] ignoreTypes = component4.ignoreTypes;
						foreach (EnemyType enemyType2 in ignoreTypes)
						{
							for (int j = 0; j < toIgnore.Count; j++)
							{
								if (enemyType2 == toIgnore[j])
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
					if (!flag)
					{
						component4.GetHit(position, GoreType.Head, fromExplosion: true);
					}
				}
				else if (other.TryGetComponent<Glass>(out component5))
				{
					component5.Shatter();
				}
				else if (ignite && other.TryGetComponent<Flammable>(out component6) && (!enemy || !component6.playerOnly) && (enemy || !component6.enemyOnly))
				{
					component6.Burn(4f);
				}
			}
		}
		goto IL_0b5a;
		IL_04a8:
		if ((!enemy || (eid.enemyType != EnemyType.HideousMass && eid.enemyType != EnemyType.Sisyphus)) && !toIgnore.Contains(eid.enemyType))
		{
			if (eid.enemyType == EnemyType.Gutterman && hitterWeapon == "heavypunch")
			{
				eid.hitter = "heavypunch";
			}
			else if (hitterWeapon == "lightningbolt")
			{
				eid.hitter = "lightningbolt";
			}
			else
			{
				eid.hitter = (friendlyFire ? "ffexplosion" : (enemy ? "enemy" : "explosion"));
			}
			if (!eid.hitterWeapons.Contains(hitterWeapon))
			{
				eid.hitterWeapons.Add(hitterWeapon);
			}
			Vector3 vector5 = playerProjectileForceDirection ?? normalized;
			if (eid.enemyType == EnemyType.Drone && damage == 0)
			{
				vector5 = Vector3.zero;
			}
			else if (vector5.y <= 0.5f)
			{
				vector5 = new Vector3(vector5.x, vector5.y + 0.5f, vector5.z);
			}
			else if (vector5.y < 1f)
			{
				vector5 = new Vector3(vector5.x, 1f, vector5.z);
			}
			float num6 = (float)damage / 10f * enemyDamageMultiplier;
			if (rocketExplosion)
			{
				if (eid.enemyType == EnemyType.Cerberus)
				{
					num6 *= 1.5f;
				}
				else if (eid.enemyType == EnemyType.Providence)
				{
					num6 *= 2f;
				}
			}
			Enemy component7 = eid.GetComponent<Enemy>();
			ZombieProjectiles component8 = eid.GetComponent<ZombieProjectiles>();
			if (eid.enemyType != EnemyType.Soldier || eid.isGasolined || unblockable || BlindEnemies.Blind || component7 == null || !component7.grounded || component8 == null || component8.difficulty < 2)
			{
				if (electric)
				{
					eid.hitterAttributes.Add(HitterAttribute.Electricity);
				}
				eid.DeliverDamage(componentInParent.gameObject, vector5 * 50000f * pushForceMultiplier, position, num6, tryForExplode: false, 0f, sourceWeapon, ignoreTotalDamageTakenMultiplier: false, fromExplosion: true);
				if (ignite)
				{
					if (eid.flammables != null && eid.flammables.Count > 0)
					{
						eid.StartBurning(damage / 10);
					}
					else
					{
						Flammable componentInChildren2 = eid.GetComponentInChildren<Flammable>();
						if (componentInChildren2 != null)
						{
							componentInChildren2.Burn(damage / 10);
						}
					}
				}
			}
			else
			{
				eid.hitter = "blocked";
				if (component8.difficulty <= 3 || electric)
				{
					if (electric)
					{
						eid.hitterAttributes.Add(HitterAttribute.Electricity);
					}
					eid.DeliverDamage(other.gameObject, Vector3.zero, position, num6 * 0.25f, tryForExplode: false, 0f, sourceWeapon, ignoreTotalDamageTakenMultiplier: false, fromExplosion: true);
				}
				component8.Block(origin);
			}
		}
		goto IL_0b5a;
		IL_0b5a:
		if (other.gameObject.CompareTag("Player") && MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer)
		{
			return;
		}
		Rigidbody component9 = other.GetComponent<Rigidbody>();
		bool flag2 = other.gameObject.layer == 14;
		if ((!((bool)component9 && flag2) || !other.gameObject.CompareTag("Metal") || !other.TryGetComponent<Nail>(out var _)) && (bool)component9 && (!flag2 || component9.GetGravityMode()) && !other.gameObject.CompareTag("IgnorePushes"))
		{
			hitColliders.Add(instanceID);
			Grenade component11;
			if (playerProjectileForceDirection.HasValue)
			{
				component9.velocity = playerProjectileForceDirection.Value * 100f;
			}
			else if (!component9.TryGetComponent<Grenade>(out component11) || !component11.ignoreExplosions)
			{
				Vector3 a = normalized * Mathf.Max(5f - num, 0f);
				a = Vector3.Scale(a, new Vector3(7500f, 1f, 7500f));
				if (component9.GetGravityMode())
				{
					a = new Vector3(a.x, 18750f, a.z);
				}
				if (other.gameObject.layer == 27 || other.gameObject.layer == 9)
				{
					a = Vector3.ClampMagnitude(a, 5000f);
				}
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && other.gameObject == MonoSingleton<PlatformerMovement>.Instance.gameObject)
				{
					a *= 30f;
				}
				component9.AddForce(a);
			}
		}
		if (!flag2)
		{
			return;
		}
		ThrownSword component12 = other.GetComponent<ThrownSword>();
		Projectile component13 = other.GetComponent<Projectile>();
		if (component12 != null)
		{
			component12.deflected = true;
		}
		if (!(component13 != null))
		{
			return;
		}
		if (component13.breakable && hitterWeapon == "heavypunch")
		{
			component13.Break();
		}
		if (component13.ignoreExplosions)
		{
			return;
		}
		component13.homingType = HomingType.None;
		Vector3 vector6 = normalized;
		if (component13.playerBullet)
		{
			vector6 = playerProjectileForceDirection ?? normalized;
		}
		other.transform.LookAt(position + vector6);
		component13.friendly = true;
		component13.target = null;
		component13.turnSpeed = 0f;
		component13.speed = Mathf.Max(component13.speed, 65f);
		if (component13.connectedBeams.Count <= 0)
		{
			return;
		}
		foreach (ContinuousBeam connectedBeam in component13.connectedBeams)
		{
			if ((bool)connectedBeam)
			{
				connectedBeam.enemy = false;
				connectedBeam.canHitEnemy = true;
			}
		}
	}

	private void CheckPortalOverlaps(PortalScene scene, float radius)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = base.transform.position;
		if (!scene.nativeScene.valid)
		{
			return;
		}
		Span<NativePortal> span = scene.nativeScene.portals.AsArray().AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref NativePortal reference = ref span[i];
			float num = ((Plane)(ref reference.plane)).SignedDistanceToPoint(float3.op_Implicit(position));
			if (num < 0f || num >= radius)
			{
				continue;
			}
			Vector3 vector = reference.travelMatrixManaged.MultiplyPoint3x4(position);
			int num2 = Physics.OverlapSphereNonAlloc(vector, radius, PortalOverlapBuffer, LayerMaskDefaults.Get(LMD.EnemiesAndPlayer));
			for (int j = 0; j < num2; j++)
			{
				Collider collider = PortalOverlapBuffer[j];
				Vector3 closestPoint = collider.ClosestPoint(vector);
				if (PrecisePortalCheck(scene, reference.handle, position, closestPoint))
				{
					Collide(collider, vector);
				}
			}
		}
	}

	private static bool PrecisePortalCheck(PortalScene scene, PortalHandle portalHandle, Vector3 startPosition, Vector3 closestPoint)
	{
		Vector3 end = scene.GetTravelMatrix(portalHandle.Reverse()).MultiplyPoint3x4(closestPoint);
		if (scene.FindPortalBetween(startPosition, end, out var hitPortal, out var _, out var _))
		{
			return hitPortal == portalHandle;
		}
		return false;
	}
}
