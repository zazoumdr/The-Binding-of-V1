using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
	public Plane? PortalPlane;

	public bool slopeCheck;

	public bool onGround;

	public bool touchingGround;

	public bool canJump;

	public bool heavyFall;

	public bool instakillStomp;

	public GameObject shockwave;

	public float superJumpChance;

	public float extraJumpChance;

	public float bounceChance;

	private Vector3 bouncePosition;

	[HideInInspector]
	public bool hasImpacted;

	public TimeSince sinceLastGrounded;

	private NewMovement nmov;

	private PlayerMovementParenting pmov;

	private Collider currentEnemyCol;

	public int forcedOff;

	private LayerMask waterMask;

	public List<Collider> cols = new List<Collider>();

	private List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

	public CapsuleCollider capsule;

	private int id;

	private void Start()
	{
		nmov = MonoSingleton<NewMovement>.Instance;
		pmov = base.transform.parent.GetComponent<PlayerMovementParenting>();
		if (pmov == null)
		{
			pmov = nmov.GetComponent<PlayerMovementParenting>();
		}
		waterMask = LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies);
		waterMask = (int)waterMask | 4;
		id = capsule.GetInstanceID();
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		base.transform.parent.parent = null;
	}

	private void OnDisable()
	{
		touchingGround = false;
		if ((bool)MonoSingleton<NewMovement>.Instance)
		{
			MonoSingleton<NewMovement>.Instance.groundProperties = null;
		}
		cols.Clear();
		slopeCheck = false;
		onGround = false;
		touchingGround = false;
		canJump = false;
		heavyFall = false;
		instakillStomp = false;
		superJumpChance = 0f;
		extraJumpChance = 0f;
		bounceChance = 0f;
		hasImpacted = false;
		sinceLastGrounded = default(TimeSince);
		forcedOff = 0;
	}

	public void ForceGroundCheck()
	{
		cols.Clear();
		onGround = false;
		touchingGround = false;
		if (!nmov)
		{
			nmov = MonoSingleton<NewMovement>.Instance;
		}
		Vector3 vector = -nmov.rb.GetGravityDirection();
		Vector3 point = capsule.transform.TransformPoint(capsule.center) + vector * (capsule.height * 0.5f - capsule.radius);
		Vector3 point2 = capsule.transform.TransformPoint(capsule.center) - vector * (capsule.height * 0.5f - capsule.radius);
		Collider[] array = Physics.OverlapCapsule(point, point2, capsule.radius);
		foreach (Collider other in array)
		{
			OnTriggerEnter(other);
		}
		UpdateState();
	}

	public void UpdateState()
	{
		if (forcedOff > 0)
		{
			onGround = false;
		}
		else if (onGround != touchingGround)
		{
			onGround = touchingGround;
		}
		if (!heavyFall)
		{
			hitEnemies.Clear();
		}
		if (onGround)
		{
			sinceLastGrounded = 0f;
		}
		if (superJumpChance > 0f)
		{
			superJumpChance = Mathf.MoveTowards(superJumpChance, 0f, Time.deltaTime);
			if (superJumpChance == 0f)
			{
				if (shockwave != null && nmov.stillHolding)
				{
					Object.Instantiate(shockwave, base.transform.position, Quaternion.LookRotation(base.transform.forward, base.transform.up)).GetComponent<PhysicalShockwave>().force *= nmov.slamForce * 2.25f;
					nmov.cc.CameraShake(0.75f);
					MonoSingleton<SceneHelper>.Instance.CreateEnviroGibs(base.transform.position, base.transform.up * -1f, 5f, 10);
				}
				extraJumpChance = 0.306f;
				nmov.stillHolding = false;
			}
		}
		if (extraJumpChance > 0f)
		{
			extraJumpChance = Mathf.MoveTowards(extraJumpChance, 0f, Time.deltaTime);
			if (extraJumpChance <= 0f && superJumpChance <= 0f && bounceChance <= 0f)
			{
				nmov.slamForce = 0f;
			}
		}
		if (bounceChance > 0f)
		{
			bounceChance = Mathf.MoveTowards(bounceChance, 0f, Time.deltaTime);
			if (!GameStateManager.Instance.PlayerInputLocked && MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
			{
				bounceChance = 0f;
				Bounce(bouncePosition);
			}
			if (!heavyFall && extraJumpChance <= 0f && superJumpChance <= 0f && bounceChance <= 0f)
			{
				nmov.slamForce = 0f;
			}
		}
		else
		{
			hasImpacted = false;
		}
		if (cols.Count > 0)
		{
			for (int num = cols.Count - 1; num >= 0; num--)
			{
				if (!ColliderIsStillUsable(cols[num]))
				{
					cols.RemoveAt(num);
				}
			}
		}
		if (touchingGround && cols.Count == 0)
		{
			touchingGround = false;
			MonoSingleton<NewMovement>.Instance.groundProperties = null;
		}
		if (canJump && (currentEnemyCol == null || !currentEnemyCol.gameObject.activeInHierarchy || Vector3.Distance(base.transform.position, currentEnemyCol.transform.position) > 40f))
		{
			canJump = false;
		}
	}

	private void FixedUpdate()
	{
		if (heavyFall)
		{
			Collider[] array = RaycastAssistant.TrueSphereCastAll(base.transform.position, 1.25f, nmov.rb.GetGravityDirection(), 3f, LayerMaskDefaults.Get(LMD.Enemies));
			if (array != null)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if ((collider.gameObject.layer != 10 && collider.gameObject.layer != 11) || Physics.Raycast(base.transform.position + base.transform.up, collider.bounds.center - base.transform.position + base.transform.up, Vector3.Distance(base.transform.position + base.transform.up, collider.bounds.center), LayerMaskDefaults.Get(LMD.Environment)))
					{
						continue;
					}
					EnemyIdentifierIdentifier component = collider.gameObject.GetComponent<EnemyIdentifierIdentifier>();
					if (!component || !component.eid || hitEnemies.Contains(component.eid))
					{
						continue;
					}
					bool dead = component.eid.dead;
					hitEnemies.Add(component.eid);
					component.eid.hitter = "ground slam";
					component.eid.DeliverDamage(collider.gameObject, base.transform.up * -50000f, collider.transform.position, instakillStomp ? 99999 : 2, tryForExplode: true);
					if (!dead)
					{
						if (!GameStateManager.Instance.PlayerInputLocked && MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
						{
							Bounce(base.transform.position);
						}
						else if (bounceChance <= 0f)
						{
							bouncePosition = base.transform.position;
							bounceChance = 0.15f;
						}
					}
				}
			}
		}
		if (!MonoSingleton<UnderwaterController>.Instance.inWater && !slopeCheck && !(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y >= 0f) && (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.FPS || MonoSingleton<NewMovement>.Instance.sliding) && (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer || MonoSingleton<PlatformerMovement>.Instance.sliding) && Physics.Raycast(base.transform.position, nmov.rb.GetGravityDirection(), out var hitInfo, Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y), waterMask, QueryTriggerInteraction.Collide) && hitInfo.transform.gameObject.layer == 4)
		{
			BounceOnWater(hitInfo.collider);
		}
	}

	private void Bounce(Vector3 position)
	{
		heavyFall = false;
		Vector3 position2 = MonoSingleton<CameraController>.Instance.transform.position;
		nmov.transform.position = position;
		MonoSingleton<CameraController>.Instance.transform.position = position2;
		MonoSingleton<CameraController>.Instance.defaultPos = MonoSingleton<CameraController>.Instance.transform.localPosition;
		nmov.Jump();
		nmov.EnemyStepResets();
		nmov.slamCooldown = 0.25f;
		if (!hasImpacted)
		{
			nmov.LandingImpact();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ColliderIsCheckable(other) && cols.Contains(other))
		{
			if (cols.IndexOf(other) == cols.Count - 1)
			{
				cols.Remove(other);
				if (cols.Count > 0)
				{
					for (int num = cols.Count - 1; num >= 0; num--)
					{
						if (ColliderIsStillUsable(cols[num]))
						{
							MonoSingleton<NewMovement>.Instance.groundProperties = (cols[num].attachedRigidbody ? cols[num].attachedRigidbody.GetComponent<CustomGroundProperties>() : cols[num].GetComponent<CustomGroundProperties>());
							break;
						}
						cols.RemoveAt(num);
					}
				}
			}
			else
			{
				cols.Remove(other);
			}
			if (cols.Count == 0)
			{
				touchingGround = false;
				MonoSingleton<NewMovement>.Instance.groundProperties = null;
			}
			if (!slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && pmov.IsObjectTracked(other.transform))
			{
				pmov.DetachPlayer(other.transform);
			}
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			canJump = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ColliderIsCheckable(other) && !cols.Contains(other))
		{
			cols.Add(other);
			touchingGround = true;
			Plane? portalPlane = PortalPlane;
			if (portalPlane.HasValue)
			{
				Plane valueOrDefault = portalPlane.GetValueOrDefault();
				Vector3 point = other.ClosestPoint(base.transform.position);
				if (valueOrDefault.GetSide(point))
				{
					touchingGround = false;
				}
			}
			if (touchingGround)
			{
				if ((!other.attachedRigidbody && other.TryGetComponent<CustomGroundProperties>(out var component)) || ((bool)other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<CustomGroundProperties>(out component)))
				{
					MonoSingleton<NewMovement>.Instance.groundProperties = component;
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.groundProperties = null;
				}
				if (!slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && other.attachedRigidbody != null && !pmov.IsObjectTracked(other.transform))
				{
					pmov.AttachPlayer(other.transform);
				}
			}
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			currentEnemyCol = other;
			canJump = true;
		}
		if (heavyFall)
		{
			if ((other.gameObject.layer == 10 || other.gameObject.layer == 11) && !Physics.Raycast(base.transform.position + base.transform.up, other.bounds.center - base.transform.position + base.transform.up, Vector3.Distance(base.transform.position + base.transform.up, other.bounds.center), LayerMaskDefaults.Get(LMD.Environment)))
			{
				EnemyIdentifierIdentifier component2 = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if ((bool)component2 && (bool)component2.eid && !hitEnemies.Contains(component2.eid))
				{
					bool dead = component2.eid.dead;
					hitEnemies.Add(component2.eid);
					component2.eid.hitter = "ground slam";
					component2.eid.DeliverDamage(other.gameObject, (base.transform.position - other.transform.position) * 5000f, other.transform.position, instakillStomp ? 99999 : 2, tryForExplode: true);
					if (!dead)
					{
						if (!GameStateManager.Instance.PlayerInputLocked && MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
						{
							Bounce(base.transform.position);
						}
						else if (bounceChance <= 0f)
						{
							bouncePosition = base.transform.position;
							bounceChance = 0.15f;
						}
					}
				}
			}
			else if (!other.gameObject.CompareTag("Slippery") && LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment))
			{
				Breakable component3 = other.gameObject.GetComponent<Breakable>();
				if (component3 != null && ((component3.weak && !component3.precisionOnly) || component3.forceGroundSlammable) && !component3.unbreakable && !component3.specialCaseOnly)
				{
					component3.Break(instakillStomp ? 99999 : 2);
				}
				else
				{
					heavyFall = false;
				}
				if (other.gameObject.TryGetComponent<Bleeder>(out var component4))
				{
					component4.GetHit(other.transform.position, GoreType.Body);
				}
				if (other.transform.TryGetComponent<Idol>(out var component5))
				{
					component5.Death();
				}
				superJumpChance = 0.1f;
			}
		}
		if (!slopeCheck && other.gameObject.layer == 4 && ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && nmov.sliding && nmov.rb.velocity.y < 0f) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && MonoSingleton<PlatformerMovement>.Instance.sliding && MonoSingleton<PlatformerMovement>.Instance.rb.velocity.y < 0f)))
		{
			Vector3 a = other.ClosestPoint(base.transform.position);
			if (!MonoSingleton<UnderwaterController>.Instance.inWater && ((Vector3.Distance(a, base.transform.position) < 0.1f && other.Raycast(new Ray(base.transform.position + base.transform.up * 1f, base.transform.up * -1f), out var _, 1.1f)) || !Physics.Raycast(base.transform.position, base.transform.up * -1f, Vector3.Distance(a, base.transform.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide)))
			{
				BounceOnWater(other);
			}
		}
	}

	private void BounceOnWater(Collider other)
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			nmov.rb.velocity = new Vector3(nmov.rb.velocity.x, 0f, nmov.rb.velocity.z);
			nmov.rb.AddForce(base.transform.up * 10f, ForceMode.VelocityChange);
		}
		else
		{
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = new Vector3(MonoSingleton<PlatformerMovement>.Instance.rb.velocity.x, 0f, MonoSingleton<PlatformerMovement>.Instance.rb.velocity.z);
			MonoSingleton<PlatformerMovement>.Instance.rb.AddForce(base.transform.up * 10f, ForceMode.VelocityChange);
		}
		Water componentInParent = other.GetComponentInParent<Water>();
		if ((bool)componentInParent)
		{
			GameObject obj = componentInParent.SpawnBasicSplash(Water.WaterGOType.small);
			obj.transform.SetPositionAndRotation(base.transform.position, Quaternion.LookRotation(base.transform.up));
			obj.GetComponent<AudioSource>().volume = 0.65f;
			ChallengeTrigger component = componentInParent.GetComponent<ChallengeTrigger>();
			if ((bool)component)
			{
				component.Entered();
			}
		}
	}

	public void ForceOff()
	{
		onGround = false;
		forcedOff++;
	}

	public void StopForceOff()
	{
		forcedOff--;
		if (forcedOff <= 0)
		{
			onGround = touchingGround;
		}
	}

	public bool ColliderIsCheckable(Collider col)
	{
		if (!col.isTrigger && !col.gameObject.CompareTag("Slippery"))
		{
			if (!LayerMaskDefaults.IsMatchingLayer(col.gameObject.layer, LMD.Environment) && col.gameObject.layer != 11 && col.gameObject.layer != 26)
			{
				if (col.gameObject.layer == 18)
				{
					return col.gameObject.CompareTag("Floor");
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool ColliderIsStillUsable(Collider col)
	{
		if (!(col == null) && col.enabled && !col.isTrigger && col.gameObject.activeInHierarchy && col.gameObject.layer != 17)
		{
			return col.gameObject.layer != 10;
		}
		return false;
	}
}
