using System;
using System.Collections.Generic;
using Sandbox;
using ULTRAKILL.Portal;
using UnityEngine;

public class RevolverBeam : MonoBehaviour
{
	private const float ForceBulletPropMulti = 0.005f;

	public EnemyTarget target;

	public BeamType beamType;

	public HitterAttribute[] attributes;

	private LineRenderer lr;

	private AudioSource aud;

	private Light muzzleLight;

	public Vector3 alternateStartPoint;

	public GameObject sourceWeapon;

	[HideInInspector]
	public int bodiesPierced;

	private int enemiesPierced;

	[HideInInspector]
	public List<PhysicsCastResult> hitList = new List<PhysicsCastResult>();

	private GunControl gc;

	private Vector3 shotHitPoint;

	public CameraController cc;

	private Vector3? lastForward;

	private bool maliciousIgnorePlayer;

	public GameObject hitParticle;

	public int bulletForce;

	public bool quickDraw;

	public int gunVariation;

	public float damage;

	[HideInInspector]
	public float addedDamage;

	public float enemyDamageOverride;

	public float critDamageOverride;

	public float screenshakeMultiplier = 1f;

	public float coinDamageBonusMultiplier = 1f;

	public int hitAmount;

	public int maxHitsPerTarget;

	private int currentHits;

	public bool noMuzzleflash;

	private bool fadeOut;

	private LayerMask ignoreEnemyTrigger;

	private LayerMask enemyLayerMask;

	private LayerMask pierceLayerMask;

	public int ricochetAmount;

	[HideInInspector]
	public bool hasBeenRicocheter;

	public GameObject ricochetSound;

	public GameObject enemyHitSound;

	public bool fake;

	public EnemyType ignoreEnemyType;

	public bool deflected;

	private bool chargeBacked;

	public bool strongAlt;

	public bool ultraRicocheter = true;

	public bool splitcoinable;

	public bool canHitProjectiles;

	private bool hasHitProjectile;

	public bool knocksDownInsurrectionists;

	public string hitterOverride;

	public bool isRocketBeam;

	[HideInInspector]
	public List<EnemyIdentifier> hitEids = new List<EnemyIdentifier>();

	[HideInInspector]
	public Transform previouslyHitTransform;

	[HideInInspector]
	public bool aimAssist;

	[HideInInspector]
	public bool intentionalRicochet;

	private Vector3 actualForward => lastForward ?? base.transform.forward;

	private void Start()
	{
		if (aimAssist)
		{
			RicochetAimAssist(base.gameObject, intentionalRicochet);
		}
		if (ricochetAmount > 0)
		{
			hasBeenRicocheter = true;
		}
		muzzleLight = GetComponent<Light>();
		lr = GetComponent<LineRenderer>();
		cc = MonoSingleton<CameraController>.Instance;
		gc = cc.GetComponentInChildren<GunControl>();
		if (beamType == BeamType.Enemy)
		{
			enemyLayerMask = (int)enemyLayerMask | 4;
		}
		enemyLayerMask = (int)enemyLayerMask | 0x400;
		enemyLayerMask = (int)enemyLayerMask | 0x800;
		if (canHitProjectiles)
		{
			enemyLayerMask = (int)enemyLayerMask | 0x4000;
		}
		pierceLayerMask = (int)pierceLayerMask | 0x40;
		pierceLayerMask = (int)pierceLayerMask | 0x80;
		pierceLayerMask = (int)pierceLayerMask | 0x100;
		pierceLayerMask = (int)pierceLayerMask | 0x1000000;
		pierceLayerMask = (int)pierceLayerMask | 0x4000000;
		ignoreEnemyTrigger = (int)enemyLayerMask | (int)pierceLayerMask;
		if (!fake)
		{
			Shoot();
		}
		else
		{
			fadeOut = true;
		}
		if (maxHitsPerTarget == 0)
		{
			maxHitsPerTarget = 99;
		}
	}

	private void Update()
	{
		if (fadeOut)
		{
			lr.widthMultiplier -= Time.deltaTime * 1.5f;
			if (muzzleLight != null)
			{
				muzzleLight.intensity -= Time.deltaTime * 100f;
			}
			if (lr.widthMultiplier <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void FakeShoot(Vector3 target)
	{
		Vector3 position = base.transform.position;
		if (alternateStartPoint != Vector3.zero)
		{
			position = alternateStartPoint;
		}
		lr.SetPosition(0, position);
		lr.SetPosition(1, target);
		Transform child = base.transform.GetChild(0);
		if (!noMuzzleflash)
		{
			child.SetPositionAndRotation(position, base.transform.rotation);
		}
		else
		{
			child.gameObject.SetActive(value: false);
		}
	}

	private void Shoot()
	{
		bool flag = hitAmount != 1;
		float radius;
		if (!flag)
		{
			float num = ((beamType != BeamType.Enemy) ? 0.4f : 0.1f);
			radius = num;
		}
		else
		{
			radius = beamType switch
			{
				BeamType.Enemy => 0.3f, 
				BeamType.Railgun => 1.2f, 
				_ => 0.6f, 
			};
		}
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		float num2 = 1000f;
		LayerMask layerMask = (flag ? pierceLayerMask : ignoreEnemyTrigger);
		bool flag2 = true;
		bool flag3 = false;
		PhysicsCastResult hitInfo = default(PhysicsCastResult);
		PortalTraversalV2[] portalTraversals = Array.Empty<PortalTraversalV2>();
		bool flag4 = false;
		Vector3 a = position;
		Vector3 vector = forward;
		float num3 = 0f;
		Vector3 endPoint;
		while (flag2)
		{
			flag2 = false;
			flag3 = PortalPhysicsV2.Raycast(position, forward, num2, layerMask, out hitInfo, out portalTraversals, out endPoint);
			for (int i = 0; i < portalTraversals.Length; i++)
			{
				PortalTraversalV2 portalTraversalV = portalTraversals[i];
				Vector3 entrancePoint = portalTraversalV.entrancePoint;
				num3 += Vector3.Distance(a, entrancePoint);
				PortalHandle portalHandle = portalTraversalV.portalHandle;
				if (!portalTraversalV.portalObject.GetTravelFlags(portalHandle.side).HasAllFlags(PortalTravellerFlags.PlayerProjectile))
				{
					shotHitPoint = entrancePoint;
					Array.Resize(ref portalTraversals, i);
					flag4 = true;
					break;
				}
				a = portalTraversalV.exitPoint;
				vector = portalTraversalV.exitDirection;
			}
			if (flag4)
			{
				flag3 = false;
				num2 = num3 - 0.01f;
				lastForward = vector.normalized;
			}
			else if (flag3)
			{
				if (flag)
				{
					Glass component2;
					if (hitInfo.transform.TryGetComponent<Breakable>(out var component) && !component.specialCaseOnly && !component.unbreakable && !component.broken && (strongAlt || component.weak || beamType == BeamType.Railgun))
					{
						Break(component);
						if (component.broken)
						{
							flag2 = true;
						}
					}
					else if (hitInfo.transform.TryGetComponent<Glass>(out component2) && !component2.broken)
					{
						component2.Shatter();
						flag2 = true;
					}
				}
				if (!flag2)
				{
					num2 = hitInfo.distance;
					shotHitPoint = hitInfo.point;
					UpdateForward(portalTraversals, hitInfo);
				}
			}
			else
			{
				Vector3 vector2 = position;
				Vector3 vector3 = forward;
				if (portalTraversals.Length != 0)
				{
					PortalTraversalV2 portalTraversalV2 = portalTraversals[^1];
					vector2 = portalTraversalV2.exitPoint;
					vector3 = portalTraversalV2.exitDirection;
				}
				shotHitPoint = vector2 + vector3 * num2;
			}
		}
		CheckWater(position, forward, num2);
		PortalTraversalV2[] portalTraversals2;
		if (!flag)
		{
			fadeOut = true;
			if (beamType != BeamType.Enemy)
			{
				if (beamType == BeamType.Railgun)
				{
					cc.CameraShake(2f * screenshakeMultiplier);
				}
				else if (strongAlt)
				{
					cc.CameraShake(0.25f * screenshakeMultiplier);
				}
			}
			PhysicsCastResult hitInfo2;
			bool num4 = PortalPhysicsV2.SphereCast(position, forward, num2, radius, enemyLayerMask, out hitInfo2, out portalTraversals2, out endPoint);
			if (num4)
			{
				shotHitPoint = hitInfo2.point;
			}
			if (num4 && (!hitInfo2.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component3) || !hitEids.Contains(component3.eid)))
			{
				shotHitPoint = hitInfo2.point;
				HitSomething(hitInfo2);
			}
			else if (flag3)
			{
				shotHitPoint = hitInfo.point;
				HitSomething(hitInfo);
			}
		}
		else
		{
			PhysicsCastResult[] allHits = PortalPhysicsV2.SphereCastAll(position, forward, num2, radius, enemyLayerMask, out portalTraversals2, out endPoint, ignorePortalMargin: true, QueryTriggerInteraction.Collide);
			PiercingShotOrder(flag3, allHits, hitInfo);
		}
		Vector3 vector4 = position;
		bool flag5 = false;
		Vector3 vector5 = PortalUtils.GetTravelMatrix(portalTraversals).inverse.MultiplyPoint3x4(shotHitPoint);
		Vector3 vector6 = vector5 - vector4;
		PhysicsCastResult hit;
		if (alternateStartPoint != Vector3.zero)
		{
			PortalPhysicsV2.ProjectThroughPortals(base.transform.position, alternateStartPoint - base.transform.position, default(LayerMask), out hit, out var endPoint2, out var traversals);
			if (traversals.Length != 0)
			{
				PortalTraversalV2 portalTraversalV3 = traversals[0];
				PortalHandle portalHandle2 = portalTraversalV3.portalHandle;
				if (!portalTraversalV3.portalObject.GetTravelFlags(portalHandle2.side).HasAllFlags(PortalTravellerFlags.PlayerProjectile))
				{
					flag5 = true;
				}
				else if (traversals.AllHasFlag(PortalTravellerFlags.PlayerProjectile))
				{
					vector4 = endPoint2;
					vector6 = PortalUtils.GetTravelMatrix(traversals).MultiplyPoint3x4(vector5) - vector4;
				}
				else
				{
					vector4 = alternateStartPoint;
					vector6 = vector5 - vector4;
				}
			}
			else
			{
				vector4 = alternateStartPoint;
				vector6 = vector5 - vector4;
			}
		}
		else
		{
			vector6 = vector5 - vector4;
		}
		if (!flag5)
		{
			PortalPhysicsV2.Raycast(vector4, vector6.normalized, vector6.magnitude - 0.01f, default(LayerMask), out hit, out var portalTraversals3, out endPoint);
			lr.SetPosition(0, vector4);
			lr.SetPosition(1, shotHitPoint);
			if (portalTraversals3 != null && portalTraversals3.Length > 0)
			{
				this.GenerateLineRendererSegments(lr, portalTraversals3);
			}
			Transform child = base.transform.GetChild(0);
			if (!noMuzzleflash)
			{
				child.SetPositionAndRotation(vector4, base.transform.rotation);
			}
			else
			{
				child.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateForward(PortalTraversalV2[] portalTraversals, PhysicsCastResult latestHit)
	{
		UpdateForward(portalTraversals, latestHit.point);
	}

	private void UpdateForward(PortalTraversalV2[] portalTraversals, Vector3 hitPos)
	{
		if (portalTraversals.Length != 0)
		{
			PortalTraversalV2 portalTraversalV = portalTraversals[^1];
			lastForward = hitPos - portalTraversalV.exitPoint;
			lastForward = lastForward.Value.normalized;
		}
	}

	private void CheckWater(Vector3 origin, Vector3 direction, float distance)
	{
		if (attributes.Length == 0)
		{
			return;
		}
		bool flag = false;
		HitterAttribute[] array = attributes;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == HitterAttribute.Electricity)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		Water component = null;
		List<Water> list = new List<Water>();
		List<GameObject> alreadyHitObjects = new List<GameObject>();
		Collider[] array2 = Physics.OverlapSphere(origin, 0.01f, 16, QueryTriggerInteraction.Collide);
		if (array2.Length != 0)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				if ((!array2[j].attachedRigidbody && !array2[j].TryGetComponent<Water>(out component)) || ((bool)array2[j].attachedRigidbody && !array2[j].attachedRigidbody.TryGetComponent<Water>(out component)) || list.Contains(component))
				{
					return;
				}
				list.Add(component);
				EnemyIdentifier.Zap(base.transform.position, 2f, alreadyHitObjects, sourceWeapon, null, component, waterOnly: true);
			}
		}
		PortalTraversalV2[] portalTraversals;
		PhysicsCastResult[] array3 = PortalPhysicsV2.RaycastAll(origin, direction, distance, 16, out portalTraversals, QueryTriggerInteraction.Collide);
		if (array3.Length == 0)
		{
			return;
		}
		for (int k = 0; k < array3.Length && array3[k].transform.TryGetComponent<Water>(out component); k++)
		{
			if (list.Contains(component))
			{
				break;
			}
			list.Add(component);
			EnemyIdentifier.Zap(array3[k].point, 2f, alreadyHitObjects, sourceWeapon, null, component, waterOnly: true);
		}
	}

	private void HitSomething(PhysicsCastResult hit)
	{
		bool flag = false;
		if (LayerMaskDefaults.IsMatchingLayer(hit.transform.gameObject.layer, LMD.Environment))
		{
			ExecuteHits(hit);
		}
		else if (beamType != BeamType.Revolver && hit.transform.gameObject.CompareTag("Coin"))
		{
			flag = true;
			lr.SetPosition(1, hit.transform.position);
			GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject, hit.point, base.transform.rotation);
			gameObject.SetActive(value: false);
			RevolverBeam component = gameObject.GetComponent<RevolverBeam>();
			component.bodiesPierced = 0;
			component.noMuzzleflash = true;
			component.alternateStartPoint = Vector3.zero;
			if (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy)
			{
				component.deflected = true;
			}
			Coin component2 = hit.transform.gameObject.GetComponent<Coin>();
			if (component2 != null)
			{
				if (component.deflected)
				{
					component2.ignoreBlessedEnemies = true;
				}
				sourceWeapon = component2.sourceWeapon ?? sourceWeapon;
				component2.DelayedReflectRevolver(hit.point, gameObject);
			}
			fadeOut = true;
		}
		else
		{
			ExecuteHits(hit);
		}
		if (hit.transform.gameObject.CompareTag("Armor") || flag || !(hitParticle != null))
		{
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(hitParticle, shotHitPoint, base.transform.rotation);
		obj.transform.forward = hit.normal;
		Explosion[] componentsInChildren = obj.GetComponentsInChildren<Explosion>();
		Explosion[] array = componentsInChildren;
		foreach (Explosion explosion in array)
		{
			explosion.sourceWeapon = sourceWeapon ?? explosion.sourceWeapon;
			explosion.hitterWeapon = GetHitterName(includeVariation: true);
			if (isRocketBeam)
			{
				explosion.rocketExplosion = true;
			}
			if (explosion.damage > 0 && addedDamage > 0f)
			{
				explosion.playerDamageOverride = explosion.damage;
				explosion.damage += Mathf.RoundToInt(addedDamage * 20f);
			}
		}
		if ((beamType != BeamType.MaliciousFace && (beamType != BeamType.Railgun || !maliciousIgnorePlayer)) || componentsInChildren.Length == 0)
		{
			return;
		}
		int num = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		if (beamType == BeamType.MaliciousFace)
		{
			array = componentsInChildren;
			foreach (Explosion explosion2 in array)
			{
				if (deflected || maliciousIgnorePlayer)
				{
					explosion2.unblockable = true;
					explosion2.canHit = AffectedSubjects.EnemiesOnly;
				}
				else
				{
					explosion2.enemy = true;
				}
				if (num < 2)
				{
					explosion2.maxSize *= 0.65f;
					explosion2.speed *= 0.65f;
				}
			}
		}
		else
		{
			array = componentsInChildren;
			foreach (Explosion explosion3 in array)
			{
				explosion3.sourceWeapon = sourceWeapon ?? explosion3.sourceWeapon;
				explosion3.canHit = AffectedSubjects.EnemiesOnly;
			}
		}
	}

	private void PiercingShotOrder(bool isPierceHit, PhysicsCastResult[] allHits, PhysicsCastResult rayHit)
	{
		hitList.Clear();
		for (int i = 0; i < allHits.Length; i++)
		{
			PhysicsCastResult item = allHits[i];
			if (item.transform != previouslyHitTransform)
			{
				hitList.Add(item);
			}
		}
		bool flag = true;
		if (isPierceHit)
		{
			Transform transform = rayHit.transform;
			GameObject gameObject = transform.gameObject;
			if (LayerMaskDefaults.IsMatchingLayer(gameObject.layer, LMD.Environment))
			{
				if (gameObject.TryGetComponent<SandboxProp>(out var _))
				{
					PortalUtils.AddForceAtPositionPortalAware(rayHit, actualForward * bulletForce * 0.005f, rayHit.point, ForceMode.VelocityChange);
				}
				AttributeChecker component4;
				if (transform.TryGetComponent<Breakable>(out var _) || gameObject.TryGetComponent<Bleeder>(out var _))
				{
					flag = true;
				}
				else if (transform.TryGetComponent<AttributeChecker>(out component4))
				{
					flag = true;
				}
			}
			if (flag || gameObject.CompareTag("Glass") || gameObject.CompareTag("GlassFloor") || gameObject.CompareTag("Armor"))
			{
				hitList.Add(rayHit);
			}
		}
		hitList.Sort();
		for (int num = hitList.Count - 1; num >= 0; num--)
		{
			if (!(hitList[num].transform.GetComponentInParent<PortalAwareRendererClone>() == null))
			{
				EnemyIdentifierIdentifier componentInParent = hitList[num].transform.GetComponentInParent<EnemyIdentifierIdentifier>();
				if (!(componentInParent == null) && !(componentInParent.eid == null))
				{
					for (int j = 0; j < hitList.Count; j++)
					{
						if (num != j && !(hitList[j].transform.GetComponentInParent<PortalAwareRendererClone>() != null))
						{
							EnemyIdentifierIdentifier componentInParent2 = hitList[j].transform.GetComponentInParent<EnemyIdentifierIdentifier>();
							if (componentInParent2 != null && componentInParent2.eid == componentInParent.eid)
							{
								hitList.RemoveAt(num);
								break;
							}
						}
					}
				}
			}
		}
		PiercingShotCheck();
	}

	private void PiercingShotCheck()
	{
		if (enemiesPierced < hitList.Count)
		{
			PhysicsCastResult physicsCastResult2;
			PhysicsCastResult physicsCastResult = (physicsCastResult2 = hitList[enemiesPierced]);
			Transform transform = physicsCastResult.transform;
			if (transform == null)
			{
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			GameObject gameObject = transform.gameObject;
			if (gameObject.CompareTag("Armor") || (ricochetAmount > 0 && (LayerMaskDefaults.IsMatchingLayer(gameObject.layer, LMD.Environment) || gameObject.layer == 0)))
			{
				bool flag = !gameObject.CompareTag("Armor");
				GameObject gameObject2 = UnityEngine.Object.Instantiate(base.gameObject, physicsCastResult2.point, base.transform.rotation);
				gameObject2.transform.forward = Vector3.Reflect(actualForward, physicsCastResult2.normal);
				lr.SetPosition(1, physicsCastResult2.point);
				RevolverBeam component = gameObject2.GetComponent<RevolverBeam>();
				component.noMuzzleflash = true;
				component.alternateStartPoint = Vector3.zero;
				component.bodiesPierced = bodiesPierced;
				component.previouslyHitTransform = transform;
				component.aimAssist = true;
				component.intentionalRicochet = flag;
				if (flag)
				{
					ricochetAmount--;
					if (beamType != BeamType.Revolver || component.maxHitsPerTarget < 3 || (strongAlt && component.maxHitsPerTarget < 4))
					{
						component.maxHitsPerTarget++;
					}
					if (SceneHelper.IsStaticEnvironment(physicsCastResult2))
					{
						MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(physicsCastResult2);
					}
					component.hitEids.Clear();
				}
				component.ricochetAmount = ricochetAmount;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(ricochetSound, physicsCastResult2.point, Quaternion.identity);
				gameObject3.SetActive(value: false);
				gameObject2.SetActive(value: false);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject2, 0.1f);
				MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject3, 0.1f);
				if (gameObject.TryGetComponent<Glass>(out var component2) && !component2.broken)
				{
					component2.Shatter();
				}
				if (gameObject.TryGetComponent<Breakable>(out var component3) && !component3.specialCaseOnly && (strongAlt || component3.weak || beamType == BeamType.Railgun))
				{
					component3.Break(damage * (float)hitAmount);
				}
				fadeOut = true;
				enemiesPierced = hitList.Count;
				return;
			}
			if (gameObject.CompareTag("Coin") && bodiesPierced < hitAmount)
			{
				if (!gameObject.TryGetComponent<Coin>(out var component4))
				{
					enemiesPierced++;
					PiercingShotCheck();
					return;
				}
				shotHitPoint = transform.position;
				lr.SetPosition(1, transform.position);
				GameObject gameObject4 = UnityEngine.Object.Instantiate(base.gameObject, physicsCastResult2.point, base.transform.rotation);
				gameObject4.SetActive(value: false);
				RevolverBeam component5 = gameObject4.GetComponent<RevolverBeam>();
				component5.bodiesPierced = 0;
				component5.noMuzzleflash = true;
				component5.alternateStartPoint = Vector3.zero;
				component5.hitEids.Clear();
				component5.previouslyHitTransform = transform;
				Revolver component6;
				if (beamType == BeamType.Enemy)
				{
					component4.ignoreBlessedEnemies = true;
					component5.deflected = true;
				}
				else if (beamType == BeamType.Revolver && strongAlt && component4.hitTimes > 1 && (bool)sourceWeapon && sourceWeapon.TryGetComponent<Revolver>(out component6) && component6.altVersion)
				{
					component6.InstaClick();
				}
				component4.DelayedReflectRevolver(physicsCastResult2.point, gameObject4);
				fadeOut = true;
				enemiesPierced = hitList.Count;
				return;
			}
			if ((gameObject.layer == 10 || gameObject.layer == 11) && bodiesPierced < hitAmount && !gameObject.CompareTag("Breakable"))
			{
				EnemyIdentifierIdentifier componentInParent = gameObject.GetComponentInParent<EnemyIdentifierIdentifier>();
				if (!componentInParent)
				{
					if (attributes.Length != 0 && transform.TryGetComponent<AttributeChecker>(out var component7))
					{
						HitterAttribute[] array = attributes;
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] == component7.targetAttribute)
							{
								component7.DelayedActivate();
								break;
							}
						}
					}
					enemiesPierced++;
					currentHits = 0;
					PiercingShotCheck();
					return;
				}
				EnemyIdentifier eid = componentInParent.eid;
				if (eid != null)
				{
					if ((!hitEids.Contains(eid) || (eid.dead && beamType == BeamType.Revolver && enemiesPierced == hitList.Count - 1)) && ((beamType != BeamType.Enemy && beamType != BeamType.MaliciousFace) || deflected || (eid.enemyType != ignoreEnemyType && !eid.immuneToFriendlyFire && !EnemyIdentifier.CheckHurtException(ignoreEnemyType, eid.enemyType, target))))
					{
						bool dead = eid.dead;
						ExecuteHits(physicsCastResult2);
						if (!dead || gameObject.layer == 11 || (beamType == BeamType.Revolver && enemiesPierced == hitList.Count - 1))
						{
							currentHits++;
							bodiesPierced++;
							UnityEngine.Object.Instantiate(hitParticle, physicsCastResult2.point, base.transform.rotation);
							MonoSingleton<TimeController>.Instance.HitStop(0.05f);
						}
						else
						{
							if (beamType == BeamType.Revolver)
							{
								hitEids.Add(eid);
							}
							enemiesPierced++;
							currentHits = 0;
						}
						if (currentHits >= maxHitsPerTarget)
						{
							hitEids.Add(eid);
							currentHits = 0;
							enemiesPierced++;
						}
						if (beamType == BeamType.Revolver && !dead)
						{
							Invoke("PiercingShotCheck", 0.05f);
						}
						else if (beamType == BeamType.Revolver)
						{
							PiercingShotCheck();
						}
						else if (!dead)
						{
							Invoke("PiercingShotCheck", 0.025f);
						}
						else
						{
							Invoke("PiercingShotCheck", 0.01f);
						}
					}
					else
					{
						enemiesPierced++;
						currentHits = 0;
						PiercingShotCheck();
					}
				}
				else
				{
					ExecuteHits(physicsCastResult2);
					enemiesPierced++;
					PiercingShotCheck();
				}
				return;
			}
			if (canHitProjectiles && gameObject.layer == 14)
			{
				if (!hasHitProjectile)
				{
					Invoke("PiercingShotCheck", 0.01f);
				}
				else
				{
					MonoSingleton<TimeController>.Instance.HitStop(0.05f);
					Invoke("PiercingShotCheck", 0.05f);
				}
				ExecuteHits(physicsCastResult2);
				enemiesPierced++;
				return;
			}
			if (gameObject.CompareTag("Glass") || gameObject.CompareTag("GlassFloor"))
			{
				gameObject.TryGetComponent<Glass>(out var component8);
				if (!component8.broken)
				{
					component8.Shatter();
				}
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			if (beamType == BeamType.Enemy && bodiesPierced < hitAmount && !physicsCastResult2.collider.isTrigger && gameObject.CompareTag("Player"))
			{
				ExecuteHits(physicsCastResult2);
				bodiesPierced++;
				enemiesPierced++;
				PiercingShotCheck();
				return;
			}
			if (transform.TryGetComponent<Breakable>(out var component9) && !component9.specialCaseOnly && (beamType == BeamType.Railgun || component9.weak))
			{
				if (component9.interrupt)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", sourceWeapon);
					MonoSingleton<TimeController>.Instance.ParryFlash();
					if (canHitProjectiles)
					{
						component9.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
					}
					if ((bool)component9.interruptEnemy && !component9.interruptEnemy.blessed)
					{
						component9.interruptEnemy.Explode(fromExplosion: true);
					}
				}
				component9.Break(damage * (float)hitAmount);
			}
			else if (bodiesPierced < hitAmount)
			{
				ExecuteHits(physicsCastResult2);
			}
			UnityEngine.Object.Instantiate(hitParticle, physicsCastResult2.point, Quaternion.LookRotation(physicsCastResult2.normal));
			enemiesPierced++;
			PiercingShotCheck();
		}
		else
		{
			enemiesPierced = 0;
			fadeOut = true;
		}
	}

	public void ExecuteHits(PhysicsCastResult currentHit)
	{
		Transform transform = currentHit.transform;
		if (transform == null)
		{
			return;
		}
		GameObject gameObject = transform.gameObject;
		if (transform.TryGetComponent<Breakable>(out var component) && !component.specialCaseOnly && (strongAlt || beamType == BeamType.Railgun || component.weak))
		{
			Break(component);
		}
		if (SceneHelper.IsStaticEnvironment(currentHit))
		{
			MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(currentHit, Mathf.RoundToInt(3f * damage), damage);
		}
		if (gameObject.TryGetComponent<Glass>(out var component2) && !component2.broken && beamType == BeamType.Enemy)
		{
			component2.Shatter();
		}
		if (canHitProjectiles && gameObject.layer == 14 && gameObject.TryGetComponent<Projectile>(out var component3) && (component3.speed != 0f || component3.turnSpeed != 0f || component3.decorative))
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate((!hasHitProjectile) ? MonoSingleton<DefaultReferenceManager>.Instance.superExplosion : component3.explosionEffect, component3.transform.position, Quaternion.identity);
			if (!gameObject2.activeSelf)
			{
				gameObject2.SetActive(value: true);
			}
			gameObject2.transform.SetParent(base.transform.parent, worldPositionStays: true);
			UnityEngine.Object.Destroy(component3.gameObject);
			if (!hasHitProjectile)
			{
				MonoSingleton<TimeController>.Instance.ParryFlash();
			}
			hasHitProjectile = true;
		}
		if (gameObject.TryGetComponent<Bleeder>(out var component4))
		{
			if (beamType == BeamType.Railgun || strongAlt)
			{
				component4.GetHit(currentHit.point, GoreType.Head);
			}
			else
			{
				component4.GetHit(currentHit.point, GoreType.Body);
			}
		}
		if (gameObject.TryGetComponent<SandboxProp>(out var _) && currentHit.rigidbody != null)
		{
			PortalUtils.AddForceAtPositionPortalAware(currentHit, actualForward * bulletForce * 0.005f, currentHit.point, ForceMode.VelocityChange);
		}
		if (transform.TryGetComponent<Coin>(out var component6) && beamType == BeamType.Revolver)
		{
			if (quickDraw)
			{
				component6.quickDraw = true;
			}
			component6.DelayedReflectRevolver(currentHit.point);
		}
		if (gameObject.CompareTag("Enemy") || gameObject.CompareTag("Body") || gameObject.CompareTag("Limb") || gameObject.CompareTag("EndLimb") || gameObject.CompareTag("Head"))
		{
			EnemyIdentifierIdentifier componentInParent = transform.GetComponentInParent<EnemyIdentifierIdentifier>();
			if (componentInParent == null)
			{
				return;
			}
			EnemyIdentifier eid = componentInParent.eid;
			if ((bool)eid && !deflected && (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy) && (eid.enemyType == ignoreEnemyType || eid.immuneToFriendlyFire || EnemyIdentifier.CheckHurtException(ignoreEnemyType, eid.enemyType, target)))
			{
				enemiesPierced++;
				return;
			}
			if (beamType != BeamType.Enemy)
			{
				if (hitAmount > 1)
				{
					cc.CameraShake(1f * screenshakeMultiplier);
				}
				else
				{
					cc.CameraShake(0.5f * screenshakeMultiplier);
				}
			}
			if ((bool)eid && !eid.dead && quickDraw && !eid.blessed && !eid.puppet)
			{
				MonoSingleton<StyleHUD>.Instance.AddPoints(50, "ultrakill.quickdraw", sourceWeapon, eid);
				quickDraw = false;
			}
			if ((bool)eid)
			{
				eid.hitter = GetHitterName();
				if (attributes != null && attributes.Length != 0)
				{
					HitterAttribute[] array = attributes;
					foreach (HitterAttribute item in array)
					{
						eid.hitterAttributes.Add(item);
					}
				}
				if (!eid.hitterWeapons.Contains(GetHitterName(includeVariation: true)))
				{
					eid.hitterWeapons.Add(GetHitterName(includeVariation: true));
				}
			}
			float critMultiplier = 1f;
			if (beamType != BeamType.Revolver)
			{
				critMultiplier = 0f;
			}
			if (critDamageOverride != 0f || strongAlt)
			{
				critMultiplier = critDamageOverride;
			}
			float num = ((enemyDamageOverride != 0f) ? enemyDamageOverride : damage);
			if ((bool)eid && deflected)
			{
				if (beamType == BeamType.MaliciousFace && eid.enemyType == EnemyType.MaliciousFace)
				{
					num = 999f;
				}
				else if (beamType == BeamType.Enemy)
				{
					num *= 2.5f;
				}
				if (!chargeBacked)
				{
					chargeBacked = true;
					if (!eid.blessed)
					{
						MonoSingleton<StyleHUD>.Instance.AddPoints(400, "ultrakill.chargeback", sourceWeapon, eid);
					}
				}
			}
			bool tryForExplode = false;
			if (strongAlt)
			{
				tryForExplode = true;
			}
			if ((bool)eid)
			{
				Vector3 vector = currentHit.direction.normalized * bulletForce;
				Vector3 vector2 = currentHit.point;
				PortalAwareRendererClone componentInParent2 = gameObject.GetComponentInParent<PortalAwareRendererClone>();
				if (componentInParent2 != null && componentInParent2.Owner.TryGetPortalHandle(out var result))
				{
					Matrix4x4 travelMatrix = MonoSingleton<PortalManagerV2>.Instance.Scene.GetPortalObject(result).GetTravelMatrix((result.side != PortalSide.Enter) ? PortalSide.Enter : PortalSide.Exit);
					vector = travelMatrix.MultiplyVector(vector).normalized * vector.magnitude;
					vector2 = travelMatrix.MultiplyPoint3x4(vector2);
				}
				eid.DeliverDamage(gameObject, vector, vector2, num, tryForExplode, critMultiplier, sourceWeapon);
			}
			if (beamType != BeamType.MaliciousFace && beamType != BeamType.Enemy)
			{
				if ((bool)eid && !eid.dead && beamType == BeamType.Revolver && !eid.blessed && gameObject.CompareTag("Head"))
				{
					gc.headshots++;
					gc.headShotComboTime = 3f;
				}
				else if (beamType == BeamType.Railgun || !gameObject.CompareTag("Head"))
				{
					gc.headshots = 0;
					gc.headShotComboTime = 0f;
				}
				if (gc.headshots > 1 && (bool)eid && !eid.blessed)
				{
					MonoSingleton<StyleHUD>.Instance.AddPoints(gc.headshots * 20, "ultrakill.headshotcombo", count: gc.headshots, sourceWeapon: sourceWeapon, eid: eid);
				}
			}
			if (knocksDownInsurrectionists && eid.enemyType == EnemyType.Sisyphus && !eid.dead && eid.TryGetComponent<Sisyphus>(out var component7))
			{
				component7.Knockdown(currentHit.point + (base.transform.position - currentHit.point).normalized);
			}
			if ((bool)enemyHitSound)
			{
				UnityEngine.Object.Instantiate(enemyHitSound, currentHit.point, Quaternion.identity);
			}
			return;
		}
		if (gameObject.layer == 10)
		{
			if (TryGetComponent<ParryHelper>(out var component8))
			{
				transform = component8.target;
				gameObject = transform.gameObject;
			}
			Debug.Log(gameObject.name, gameObject);
			Grenade componentInParent3 = transform.GetComponentInParent<Grenade>();
			if (componentInParent3 != null)
			{
				if (beamType != BeamType.Enemy || !componentInParent3.enemy || componentInParent3.playerRiding)
				{
					MonoSingleton<TimeController>.Instance.ParryFlash();
				}
				if ((beamType == BeamType.Railgun && hitAmount == 1) || beamType == BeamType.MaliciousFace)
				{
					maliciousIgnorePlayer = true;
					componentInParent3.Explode(componentInParent3.rocket, harmless: false, !componentInParent3.rocket, 2f, ultrabooster: true, sourceWeapon);
				}
				else
				{
					componentInParent3.Explode(componentInParent3.rocket, harmless: false, !componentInParent3.rocket, 1f, ultrabooster: false, sourceWeapon);
				}
			}
			else
			{
				Cannonball componentInParent4 = transform.GetComponentInParent<Cannonball>();
				if ((bool)componentInParent4)
				{
					MonoSingleton<TimeController>.Instance.ParryFlash();
					componentInParent4.Explode();
				}
			}
			return;
		}
		if (beamType == BeamType.Enemy && !currentHit.collider.isTrigger && gameObject.CompareTag("Player"))
		{
			if ((bool)enemyHitSound)
			{
				UnityEngine.Object.Instantiate(enemyHitSound, currentHit.point, Quaternion.identity);
			}
			if (beamType == BeamType.Enemy)
			{
				if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
				{
					MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(damage * 10f), invincible: true);
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.Explode();
				}
			}
			return;
		}
		if ((bool)gc)
		{
			gc.headshots = 0;
			gc.headShotComboTime = 0f;
		}
		if (!gameObject.CompareTag("Armor"))
		{
			return;
		}
		EnemyIdentifier enemyIdentifier = null;
		bool flag = true;
		if (gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component9))
		{
			enemyIdentifier = component9.eid;
			if (hitEids.Contains(component9.eid))
			{
				flag = false;
			}
		}
		if (flag)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(base.gameObject, currentHit.point, base.transform.rotation);
			gameObject3.transform.forward = Vector3.Reflect(actualForward, currentHit.normal);
			RevolverBeam component10 = gameObject3.GetComponent<RevolverBeam>();
			component10.noMuzzleflash = true;
			component10.alternateStartPoint = Vector3.zero;
			component10.aimAssist = true;
			if (enemyIdentifier != null)
			{
				component10.hitEids.Add(enemyIdentifier);
			}
			GameObject gameObject4 = UnityEngine.Object.Instantiate(ricochetSound, currentHit.point, Quaternion.identity);
			gameObject4.SetActive(value: false);
			gameObject3.SetActive(value: false);
			MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject3, 0.1f);
			MonoSingleton<DelayedActivationManager>.Instance.Add(gameObject4, 0.1f);
		}
	}

	private void Break(Breakable brk)
	{
		if (brk.interrupt)
		{
			MonoSingleton<StyleHUD>.Instance.AddPoints(100, "ultrakill.interruption", sourceWeapon);
			MonoSingleton<TimeController>.Instance.ParryFlash();
			if (canHitProjectiles)
			{
				brk.breakParticle = MonoSingleton<DefaultReferenceManager>.Instance.superExplosion;
			}
			if ((bool)brk.interruptEnemy && !brk.interruptEnemy.blessed)
			{
				brk.interruptEnemy.Explode(fromExplosion: true);
			}
		}
		brk.Break(damage * (float)hitAmount);
	}

	private void RicochetAimAssist(GameObject beam, bool aimAtHead = false)
	{
		RaycastHit[] array = Physics.SphereCastAll(beam.transform.position, 5f, beam.transform.forward, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Enemies));
		if (array == null || array.Length == 0)
		{
			return;
		}
		Vector3 worldPosition = beam.transform.forward * 1000f;
		float num = float.PositiveInfinity;
		GameObject gameObject = null;
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			Coin component;
			bool flag2 = MonoSingleton<CoinTracker>.Instance.revolverCoinsList.Count > 0 && array[i].transform.TryGetComponent<Coin>(out component) && (!component.shot || component.shotByEnemy);
			if ((!flag || flag2) && (!(array[i].distance > num) || (!flag && flag2)) && (!(array[i].distance < 0.1f) || flag2) && !PortalPhysicsV2.Raycast(beam.transform.position, array[i].point - beam.transform.position, array[i].distance, LayerMaskDefaults.Get(LMD.Environment), out var _, out var _, out var _) && (flag2 || (array[i].transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component2) && (bool)component2.eid && !component2.eid.dead)))
			{
				if (flag2)
				{
					flag = true;
				}
				worldPosition = (flag2 ? array[i].transform.position : array[i].point);
				num = array[i].distance;
				gameObject = array[i].transform.gameObject;
			}
		}
		if ((bool)gameObject)
		{
			if (aimAtHead && !flag && (critDamageOverride != 0f || (beamType == BeamType.Revolver && !strongAlt)) && gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component3) && (bool)component3.eid && (bool)component3.eid.weakPoint && !PortalPhysicsV2.Raycast(beam.transform.position, component3.eid.weakPoint.transform.position - beam.transform.position, Vector3.Distance(component3.eid.weakPoint.transform.position, beam.transform.position), LayerMaskDefaults.Get(LMD.Environment)))
			{
				worldPosition = component3.eid.weakPoint.transform.position;
			}
			beam.transform.LookAt(worldPosition);
		}
	}

	private string GetHitterName(bool includeVariation = false)
	{
		if (!string.IsNullOrEmpty(hitterOverride))
		{
			return hitterOverride;
		}
		string text = "";
		if (beamType == BeamType.Revolver)
		{
			text = "revolver";
		}
		else if (beamType == BeamType.Railgun)
		{
			text = "railcannon";
		}
		else if (beamType == BeamType.MaliciousFace || beamType == BeamType.Enemy)
		{
			text = "enemy";
		}
		if (includeVariation)
		{
			text += gunVariation;
		}
		return text;
	}
}
