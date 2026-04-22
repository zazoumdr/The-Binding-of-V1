using System;
using System.Collections.Generic;
using BeamHitInterpolation;
using ULTRAKILL.Portal;
using UnityEngine;

public class ContinuousBeam : MonoBehaviour
{
	public EnemyTarget target;

	private LineRenderer lr;

	private LayerMask environmentMask;

	private LayerMask hitMask;

	public bool canHitPlayer = true;

	public bool canHitEnemy = true;

	public bool ignoreInvincibility;

	public float beamWidth = 0.35f;

	public bool enemy;

	public EnemyType safeEnemyType;

	public float damage;

	public float parryMultiplier = 1f;

	private float playerCooldown;

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	private List<float> enemyCooldowns = new List<float>();

	public GameObject impactEffect;

	public GameObject trackOnBeamToPlayer;

	[HideInInspector]
	public float originalWidth;

	[HideInInspector]
	public bool dying;

	[HideInInspector]
	public bool off;

	[Header("Startup")]
	public bool startup;

	public float startUpSpeed = 100f;

	[HideInInspector]
	public float maxDistance;

	[Header("End Point")]
	public bool useProjectileRef;

	public Projectile projectile;

	public Projectile endProjectile;

	public Transform endPoint;

	private bool hasHadEndPoint;

	public bool cancelIfEndPointBlocked;

	public bool destroyIfEndPointDestroyed;

	private Vector3 lastEndPointPosition;

	public PortalTravellerFlags portalTravelFlags = PortalTravellerFlags.EnemyProjectile;

	[Header("Hit Interpolation")]
	public bool interpolateHits = true;

	private (Ray, float)[] lastRays = Array.Empty<(Ray, float)>();

	private PortalTraversalV2[] lastTraversals = Array.Empty<PortalTraversalV2>();

	private LineRendererPortalHelper lineRendererPortalHelper;

	public void OnProjectileTraversal(in PortalTravelDetails details)
	{
		lastRays = Array.Empty<(Ray, float)>();
		lastTraversals = Array.Empty<PortalTraversalV2>();
		FixedUpdate();
	}

	private void Awake()
	{
		if ((bool)impactEffect)
		{
			impactEffect.SetActive(value: false);
		}
		lr = GetComponent<LineRenderer>();
		if (originalWidth == 0f)
		{
			originalWidth = lr.widthMultiplier;
		}
	}

	private void Start()
	{
		environmentMask = LayerMaskDefaults.Get(LMD.Environment);
		hitMask = (int)hitMask | (int)environmentMask;
		hitMask = (int)hitMask | 0x400;
		hitMask = (int)hitMask | 4;
		if (ignoreInvincibility)
		{
			hitMask = (int)hitMask | 0x8000;
		}
		if (!startup)
		{
			maxDistance = float.PositiveInfinity;
		}
		hasHadEndPoint = (useProjectileRef ? ((bool)projectile) : ((bool)endPoint));
	}

	private void OnDestroy()
	{
		if ((bool)projectile)
		{
			projectile.connectedBeams.Remove(this);
		}
	}

	public void SetPlayerCooldown(float cooldown)
	{
		playerCooldown = cooldown;
	}

	private void Update()
	{
		lr.widthMultiplier = Mathf.MoveTowards(lr.widthMultiplier, (off || dying) ? 0f : originalWidth, Time.deltaTime * 4f * ((originalWidth == 0f) ? 1f : originalWidth));
		if (dying)
		{
			if (lr.widthMultiplier == 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			return;
		}
		if (hasHadEndPoint && destroyIfEndPointDestroyed && !endPoint)
		{
			if (useProjectileRef)
			{
				if (!projectile)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			return;
		}
		if (startup)
		{
			maxDistance += startUpSpeed * Time.deltaTime;
		}
		if (playerCooldown > 0f)
		{
			playerCooldown = Mathf.MoveTowards(playerCooldown, 0f, Time.deltaTime);
		}
		if (enemyCooldowns.Count > 0)
		{
			for (int i = 0; i < enemyCooldowns.Count; i++)
			{
				enemyCooldowns[i] = Mathf.MoveTowards(enemyCooldowns[i], 0f, Time.deltaTime);
			}
		}
	}

	private void FixedUpdate()
	{
		Vector3 position = base.transform.position;
		PortalTraversalV2[] portalTraversals;
		Vector3 vector3;
		if (useProjectileRef)
		{
			if (!projectile || !endProjectile)
			{
				DisableBeam();
				return;
			}
			Vector3 position2 = endProjectile.transform.position;
			Vector3 vector = position2;
			PortalHandleSequence portalHandleSequence = endProjectile.traversals.Reversed();
			for (int i = 0; i < projectile.traversals.Count; i++)
			{
				portalHandleSequence = portalHandleSequence.Then(projectile.traversals[i]);
			}
			if (portalHandleSequence.Count > 0)
			{
				vector = PortalUtils.GetTravelMatrix(portalHandleSequence).MultiplyPoint3x4(vector);
			}
			Vector3 vector2 = vector - position;
			PhysicsCastResult hitInfo;
			Vector3 a;
			bool flag = PortalPhysicsV2.Raycast(position, vector2.normalized, vector2.magnitude, environmentMask, out hitInfo, out portalTraversals, out a);
			if (Vector3.Distance(a, position2) >= 0.1f)
			{
				DisableBeam();
				SetImpactEffect(value: false);
				return;
			}
			if (flag)
			{
				if (cancelIfEndPointBlocked)
				{
					off = true;
					DisableBeam();
					SetImpactEffect(value: false);
					return;
				}
				off = false;
				vector3 = hitInfo.point;
				SetImpactEffect(value: true);
			}
			else
			{
				vector3 = position2;
				SetImpactEffect(value: false);
			}
		}
		else
		{
			float magnitude;
			Vector3 vector4;
			if ((bool)endPoint)
			{
				vector4 = endPoint.position - position;
				magnitude = vector4.magnitude;
				vector4 = vector4.normalized;
			}
			else
			{
				vector4 = base.transform.forward;
				magnitude = maxDistance;
			}
			if (PortalPhysicsV2.Raycast(position, vector4, magnitude, environmentMask, out var hitInfo2, out portalTraversals, out var vector5))
			{
				if (cancelIfEndPointBlocked)
				{
					off = true;
					vector3 = endPoint.position;
					DisableBeam();
					SetImpactEffect(value: false);
					return;
				}
				off = false;
				vector3 = hitInfo2.point;
				SetImpactEffect(value: true);
			}
			else
			{
				off = false;
				vector3 = vector5;
				SetImpactEffect(!endPoint && maxDistance < 999f);
			}
		}
		for (int j = 0; j < portalTraversals.Length; j++)
		{
			PortalHandle portalHandle = portalTraversals[j].portalHandle;
			if (!PortalUtils.GetPortalObject(portalHandle).GetTravelFlags(portalHandle.side).HasAllFlags(portalTravelFlags))
			{
				if (cancelIfEndPointBlocked)
				{
					off = true;
					DisableBeam();
					SetImpactEffect(value: false);
					return;
				}
				vector3 = portalTraversals[j].entrancePoint;
				Array.Resize(ref portalTraversals, j);
				SetImpactEffect(value: true);
				break;
			}
		}
		lr.SetPosition(0, position);
		lr.SetPosition(1, vector3);
		if (portalTraversals != null && portalTraversals.Length > 0 && lineRendererPortalHelper == null)
		{
			lineRendererPortalHelper = LineRendererPortalHelper.GetOrCreateHelper(lr);
		}
		if (lineRendererPortalHelper != null)
		{
			lineRendererPortalHelper.DisableSegments();
			lineRendererPortalHelper.enabled = true;
			lineRendererPortalHelper.UpdateTraversals(portalTraversals);
		}
		else
		{
			lr.enabled = true;
		}
		if ((bool)impactEffect)
		{
			impactEffect.transform.position = vector3;
		}
		if (off || dying)
		{
			return;
		}
		if ((bool)trackOnBeamToPlayer)
		{
			Vector3 vector6 = vector3 - position;
			float num = Mathf.Clamp(Vector3.Dot(MonoSingleton<CameraController>.Instance.transform.position - position, vector6.normalized), 0f, vector6.magnitude);
			trackOnBeamToPlayer.transform.position = position + vector6.normalized * num;
		}
		List<InterpolatedHit> list = ListPool<InterpolatedHit>.Get();
		(Ray, float)[] array = new(Ray, float)[portalTraversals.Length + 1];
		int num2 = Math.Min(portalTraversals.Length, lastTraversals.Length);
		int num3 = num2 - 1;
		for (int k = 0; k < num2; k++)
		{
			if (portalTraversals[k].portalHandle != lastTraversals[k].portalHandle)
			{
				num3 = k - 1;
				break;
			}
		}
		Vector3 vector7 = base.transform.position;
		Vector3 vector8 = base.transform.forward;
		Vector3 vector9 = ((portalTraversals.Length != 0) ? portalTraversals[0].entrancePoint : vector3);
		for (int l = 0; l <= portalTraversals.Length; l++)
		{
			if (l > 0)
			{
				int num4 = l - 1;
				vector7 = portalTraversals[num4].exitPoint;
				vector8 = portalTraversals[num4].exitDirection;
				vector9 = ((l == portalTraversals.Length) ? vector3 : portalTraversals[num4 + 1].entrancePoint);
			}
			Ray ray = new Ray(vector7 + vector8 * beamWidth, (vector9 - vector7).normalized);
			float num5 = Vector3.Distance(vector7, vector9) - beamWidth;
			RaycastHit[] raycastHits = ArrayPool.GetRaycastHits();
			int num6 = Physics.SphereCastNonAlloc(ray, beamWidth, raycastHits, num5, hitMask);
			for (int m = 0; m < num6; m++)
			{
				list.Add(InterpolatedHit.FromRaycastHit(raycastHits[m]));
			}
			if (interpolateHits && ((l == 0 && lastRays.Length != 0) || (l > 0 && l <= num3 + 1)))
			{
				List<InterpolatedHit> list2 = ListPool<InterpolatedHit>.Get();
				BeamHitInterpolator.HitInterpolated(ray, num5, lastRays[l].Item1, lastRays[l].Item2, beamWidth, hitMask, list2);
				if (list2.Count > 0)
				{
					foreach (InterpolatedHit item in list2)
					{
						bool flag2 = false;
						for (int n = 0; n < num6; n++)
						{
							if (!(raycastHits[n].collider != item.collider))
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							list.Add(item);
						}
					}
				}
				ListPool<InterpolatedHit>.Release(list2);
			}
			array[l] = (ray, num5);
			ArrayPool.ReturnRaycastHits(raycastHits);
		}
		InterpolatedHit[] array2 = list.ToArray();
		ListPool<InterpolatedHit>.Release(list);
		lastTraversals = portalTraversals;
		lastRays = array;
		if (array2 == null || array2.Length <= 0)
		{
			return;
		}
		for (int num7 = 0; num7 < array2.Length; num7++)
		{
			if (canHitPlayer && MonoSingleton<NewMovement>.Instance.hurtInvincibility <= 0f && playerCooldown <= 0f && array2[num7].collider.gameObject.CompareTag("Player"))
			{
				playerCooldown = 0.5f;
				if (!Physics.Raycast(base.transform.position, array2[num7].point - base.transform.position, array2[num7].distance, environmentMask))
				{
					MonoSingleton<NewMovement>.Instance.GetHurt(Mathf.RoundToInt(damage), invincible: true, 1f, explosion: false, instablack: false, 0.35f, ignoreInvincibility: true);
				}
			}
			else if ((array2[num7].transform.gameObject.layer == 10 || array2[num7].transform.gameObject.layer == 11) && canHitEnemy)
			{
				EnemyIdentifierIdentifier component = array2[num7].transform.GetComponent<EnemyIdentifierIdentifier>();
				if (!component || !component.eid || (enemy && (component.eid.enemyType == safeEnemyType || component.eid.immuneToFriendlyFire || EnemyIdentifier.CheckHurtException(safeEnemyType, component.eid.enemyType, target))))
				{
					continue;
				}
				EnemyIdentifier eid = component.eid;
				bool flag3 = hitEnemies.Contains(eid);
				if (!flag3 || enemyCooldowns[hitEnemies.IndexOf(eid)] <= 0f)
				{
					if (!flag3)
					{
						hitEnemies.Add(eid);
						enemyCooldowns.Add(0.5f);
					}
					else
					{
						enemyCooldowns[hitEnemies.IndexOf(eid)] = 0.5f;
					}
					if (enemy)
					{
						eid.hitter = "enemy";
					}
					eid.DeliverDamage(array2[num7].transform.gameObject, (vector3 - base.transform.position).normalized * 1000f, array2[num7].point, damage * parryMultiplier / 10f, tryForExplode: true);
				}
			}
			else if (LayerMaskDefaults.IsMatchingLayer(array2[num7].transform.gameObject.layer, LMD.Environment))
			{
				Breakable component2 = array2[num7].transform.GetComponent<Breakable>();
				if ((bool)component2 && !component2.playerOnly && !component2.precisionOnly && !component2.specialCaseOnly)
				{
					component2.Break(damage);
				}
				if (array2[num7].transform.gameObject.TryGetComponent<Bleeder>(out var component3))
				{
					component3.GetHit(array2[num7].point, GoreType.Small);
				}
			}
		}
		void DisableBeam()
		{
			lr.enabled = false;
			if (lineRendererPortalHelper != null)
			{
				lineRendererPortalHelper.DisableSegments();
				lineRendererPortalHelper.enabled = false;
			}
			lastTraversals = Array.Empty<PortalTraversalV2>();
			lastRays = Array.Empty<(Ray, float)>();
		}
		void SetImpactEffect(bool value)
		{
			if ((bool)impactEffect)
			{
				impactEffect.SetActive(value);
			}
		}
	}

	public void TurnOff()
	{
		dying = true;
		base.enabled = true;
	}

	public void DetachAndTurnOff()
	{
		dying = true;
		base.enabled = true;
		base.transform.SetParent(null, worldPositionStays: true);
	}
}
