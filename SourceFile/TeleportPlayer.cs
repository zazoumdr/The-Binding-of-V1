using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
	public bool affectPosition = true;

	public Vector3 relativePosition;

	public bool notRelative;

	public bool relativeToCollider;

	public Vector3 objectivePosition;

	public bool includeOffsetFromCollider;

	public bool affectRotation;

	public bool notRelativeRotation;

	public Vector2 rotationDelta;

	public Vector2 objectiveRotation;

	public bool resetPlayerSpeed;

	public bool cancelGroundSlam;

	public bool dontDetachPlayerFromMovementParent;

	public Transform[] teleportWithPlayer;

	public GameObject teleportEffect;

	public UltrakillEvent onTeleportPlayer;

	public bool OOBTeleport;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			PerformTheTeleport(other.transform);
		}
	}

	public void PerformTheTeleport()
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			PerformTheTeleport(MonoSingleton<NewMovement>.Instance.transform);
		}
		else
		{
			PerformTheTeleport(MonoSingleton<PlatformerMovement>.Instance.transform);
		}
	}

	private void PerformTheTeleport(Transform target)
	{
		if ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && MonoSingleton<NewMovement>.Instance.dead) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && MonoSingleton<PlatformerMovement>.Instance.dead))
		{
			return;
		}
		if ((bool)MonoSingleton<NewMovement>.Instance && (bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			MonoSingleton<NewMovement>.Instance.ridingRocket.PlayerRideEnd();
		}
		if (affectPosition)
		{
			Vector3 position = target.position;
			Vector3 position2 = target.position;
			if (dontDetachPlayerFromMovementParent)
			{
				MonoSingleton<PlayerMovementParenting>.Instance.LockMovementParentTeleport(fuck: true);
			}
			position2 = (target.position = (notRelative ? ((!includeOffsetFromCollider) ? objectivePosition : (objectivePosition - (base.transform.position - target.position))) : ((!relativeToCollider) ? (target.position + relativePosition) : (base.transform.position + relativePosition))));
			if (teleportWithPlayer != null && teleportWithPlayer.Length != 0)
			{
				for (int i = 0; i < teleportWithPlayer.Length; i++)
				{
					if (teleportWithPlayer[i] != null)
					{
						teleportWithPlayer[i].position += position2 - position;
					}
				}
			}
			if (dontDetachPlayerFromMovementParent)
			{
				MonoSingleton<PlayerMovementParenting>.Instance.LockMovementParentTeleport(fuck: false);
			}
		}
		if (affectRotation)
		{
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				if (notRelativeRotation)
				{
					MonoSingleton<CameraController>.Instance.gravityRotation = Quaternion.identity;
					MonoSingleton<CameraController>.Instance.rotationY = objectiveRotation.y;
					MonoSingleton<CameraController>.Instance.rotationX = objectiveRotation.x;
					MonoSingleton<NewMovement>.Instance.dodgeDirection = Quaternion.AngleAxis(rotationDelta.y, Vector3.up) * Vector3.forward;
				}
				else
				{
					MonoSingleton<CameraController>.Instance.rotationY += rotationDelta.y;
					MonoSingleton<CameraController>.Instance.rotationX += rotationDelta.x;
					MonoSingleton<NewMovement>.Instance.dodgeDirection = Quaternion.AngleAxis(rotationDelta.y, Vector3.up) * MonoSingleton<NewMovement>.Instance.dodgeDirection;
				}
				MonoSingleton<CameraController>.Instance.ApplyRotations();
			}
			else
			{
				if (notRelativeRotation)
				{
					MonoSingleton<PlatformerMovement>.Instance.rotationY = objectiveRotation.y;
					MonoSingleton<PlatformerMovement>.Instance.rotationX = objectiveRotation.x;
				}
				else
				{
					MonoSingleton<PlatformerMovement>.Instance.rotationY += rotationDelta.y;
					MonoSingleton<PlatformerMovement>.Instance.rotationX += rotationDelta.x;
				}
				MonoSingleton<PlatformerMovement>.Instance.transform.rotation = Quaternion.Euler(0f, MonoSingleton<CameraController>.Instance.rotationY, 0f);
				MonoSingleton<CameraController>.Instance.transform.localRotation = Quaternion.Euler(MonoSingleton<CameraController>.Instance.rotationX, 0f, 0f);
			}
		}
		if (resetPlayerSpeed && MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			MonoSingleton<NewMovement>.Instance.StopMovement();
		}
		if (cancelGroundSlam && MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			MonoSingleton<NewMovement>.Instance.gc.heavyFall = false;
		}
		if ((bool)teleportEffect)
		{
			Object.Instantiate(teleportEffect, target.position, Quaternion.identity);
		}
		if (OOBTeleport)
		{
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Whoops, sorry about that.");
		}
		onTeleportPlayer.Invoke();
	}
}
