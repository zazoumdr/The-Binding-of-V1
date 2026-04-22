using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTarget
{
	public bool isPlayer;

	public Transform targetTransform;

	public EnemyIdentifier enemyIdentifier;

	public Rigidbody rigidbody;

	public bool isEnemy
	{
		get
		{
			if (!isPlayer)
			{
				return enemyIdentifier != null;
			}
			return false;
		}
	}

	public Vector3 position
	{
		get
		{
			if (!(enemyIdentifier != null) || !enemyIdentifier.overrideCenter)
			{
				if (!(targetTransform != null))
				{
					return Vector3.zero;
				}
				return targetTransform.position;
			}
			return enemyIdentifier.overrideCenter.position;
		}
	}

	public Vector3 headPosition => headTransform.position;

	public Transform headTransform
	{
		get
		{
			if (!(enemyIdentifier == null) || !isPlayer || MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.FPS)
			{
				return trackedTransform;
			}
			return MonoSingleton<CameraController>.Instance.cam.transform;
		}
	}

	public Transform trackedTransform
	{
		get
		{
			if (!(enemyIdentifier != null) || !enemyIdentifier.overrideCenter)
			{
				return targetTransform;
			}
			return enemyIdentifier.overrideCenter;
		}
	}

	public Vector3 forward => targetTransform.forward;

	public Vector3 right => targetTransform.right;

	public bool isOnGround
	{
		get
		{
			if (isPlayer)
			{
				return MonoSingleton<PlayerTracker>.Instance.GetOnGround();
			}
			return true;
		}
	}

	public bool isValid
	{
		get
		{
			if (targetTransform != null && targetTransform.gameObject.activeInHierarchy)
			{
				if (!(enemyIdentifier == null))
				{
					return !enemyIdentifier.dead;
				}
				return true;
			}
			return false;
		}
	}

	public Vector3 GetNavPoint()
	{
		return GetNavPoint(LayerMaskDefaults.Get(LMD.Environment));
	}

	public static Vector3 GetNavPoint(Vector3 pos)
	{
		return GetNavPoint(LayerMaskDefaults.Get(LMD.Environment), pos);
	}

	public Vector3 GetNavPoint(LayerMask mask)
	{
		return GetNavPoint(mask, position);
	}

	public static Vector3 GetNavPoint(LayerMask mask, Vector3 pos)
	{
		Vector3 result = pos;
		if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, mask))
		{
			result = hitInfo.point;
		}
		return result;
	}

	public bool IsTargetTransform(Transform other)
	{
		if (isPlayer)
		{
			return other == MonoSingleton<PlayerTracker>.Instance.GetPlayer().parent;
		}
		return other == targetTransform;
	}

	public EnemyTarget(Transform targetTransform)
	{
		isPlayer = false;
		this.targetTransform = targetTransform;
		enemyIdentifier = this.targetTransform.GetComponent<EnemyIdentifier>();
		rigidbody = this.targetTransform.GetComponent<Rigidbody>();
	}

	public EnemyTarget(EnemyIdentifier otherEnemy)
	{
		isPlayer = false;
		targetTransform = otherEnemy.transform;
		enemyIdentifier = otherEnemy;
		rigidbody = targetTransform.GetComponent<Rigidbody>();
	}

	public Vector3 GetVelocity()
	{
		if (isPlayer)
		{
			return MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity();
		}
		if (targetTransform == null)
		{
			return Vector3.zero;
		}
		if (targetTransform.TryGetComponent<NavMeshAgent>(out var component) && ((Behaviour)(object)component).enabled)
		{
			return component.velocity;
		}
		if (rigidbody != null)
		{
			return rigidbody.velocity;
		}
		return Vector3.zero;
	}

	public Vector3 PredictTargetPosition(float time, bool includeGravity = false, bool assumeGroundMovement = false)
	{
		Vector3 vector = GetVelocity() * time;
		if (rigidbody == null)
		{
			return targetTransform.position + vector;
		}
		if (includeGravity && ((isEnemy && !rigidbody.isKinematic) || (isPlayer && !MonoSingleton<NewMovement>.Instance.gc.onGround)))
		{
			vector += 0.5f * Physics.gravity * (time * time);
		}
		if (Physics.Raycast(rigidbody.position, vector, out var hitInfo, vector.magnitude, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			if (!assumeGroundMovement)
			{
				return hitInfo.point;
			}
			vector = new Vector3(vector.x, hitInfo.point.y, vector.z);
		}
		return rigidbody.position + vector;
	}

	private EnemyTarget()
	{
		isPlayer = false;
		targetTransform = null;
	}

	public static EnemyTarget TrackPlayer()
	{
		PlayerTracker instance = MonoSingleton<PlayerTracker>.Instance;
		return new EnemyTarget
		{
			isPlayer = true,
			targetTransform = instance.GetPlayer().transform,
			rigidbody = instance.GetRigidbody()
		};
	}

	public static EnemyTarget TrackPlayerIfAllowed()
	{
		if (EnemyIgnorePlayer.Active || BlindEnemies.Blind)
		{
			return null;
		}
		return TrackPlayer();
	}

	public override string ToString()
	{
		return (isPlayer ? "Player: " : (isEnemy ? "Enemy: " : "Custom Target: ")) + targetTransform.name + " (" + targetTransform.position.ToString() + ")";
	}
}
