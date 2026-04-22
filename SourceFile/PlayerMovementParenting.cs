using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using plog;
using plog.Models;
using ULTRAKILL.Cheats;
using UnityEngine;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class PlayerMovementParenting : MonoSingleton<PlayerMovementParenting>
{
	private static readonly Logger Log = new Logger("PlayerMovementParenting");

	public Transform deltaReceiver;

	private Vector3 lastTrackedPos;

	private float lastAngle;

	private Transform playerTracker;

	[HideInInspector]
	public bool lockParent;

	private Vector3 teleportLockDelta;

	private Rigidbody rb;

	private List<Transform> trackedObjects = new List<Transform>();

	public Vector3 currentDelta { get; private set; }

	public List<Transform> TrackedObjects => trackedObjects;

	private void Awake()
	{
		if (deltaReceiver == null)
		{
			deltaReceiver = base.transform;
		}
		if (!rb)
		{
			rb = GetComponent<Rigidbody>();
		}
	}

	private void FixedUpdate()
	{
		currentDelta = Vector3.zero;
		if (playerTracker == null)
		{
			return;
		}
		if (!MonoSingleton<NewMovement>.Instance.enabled)
		{
			DetachPlayer();
			return;
		}
		Vector3 position = playerTracker.transform.position;
		float y = playerTracker.transform.eulerAngles.y;
		Vector3 vector = position - lastTrackedPos;
		lastTrackedPos = position;
		bool flag = true;
		if ((bool)MonoSingleton<NewMovement>.Instance && (bool)MonoSingleton<NewMovement>.Instance.groundProperties && MonoSingleton<NewMovement>.Instance.groundProperties.dontRotateCamera)
		{
			flag = false;
		}
		float num = y - lastAngle;
		lastAngle = y;
		float num2 = Mathf.Abs(num);
		if (num2 > 180f)
		{
			num2 = 360f - num2;
		}
		if (num2 > 5f)
		{
			if (PlayerParentingDebug.Active)
			{
				Log.Fine($"Angle delta too high: {num2} degrees", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			DetachPlayer();
			return;
		}
		if (vector.magnitude > 2f)
		{
			if (PlayerParentingDebug.Active)
			{
				Log.Fine($"Position delta too high: {vector.magnitude} units", (IEnumerable<Tag>)null, (string)null, (object)null);
			}
			DetachPlayer();
			return;
		}
		if ((bool)rb)
		{
			rb.MovePosition(rb.position + vector);
		}
		else
		{
			deltaReceiver.position += vector;
		}
		playerTracker.transform.position = deltaReceiver.position;
		lastTrackedPos = playerTracker.transform.position;
		currentDelta = vector;
		if (flag)
		{
			MonoSingleton<CameraController>.Instance.rotationY += num;
		}
	}

	public bool IsPlayerTracking()
	{
		return playerTracker != null;
	}

	public bool IsObjectTracked(Transform other)
	{
		return trackedObjects.Contains(other);
	}

	public void AttachPlayer(Transform other)
	{
		if (!lockParent)
		{
			trackedObjects.Add(other);
			GameObject obj = new GameObject("Player Position Proxy");
			obj.transform.parent = other;
			obj.transform.position = deltaReceiver.position;
			obj.transform.rotation = deltaReceiver.rotation;
			GameObject gameObject = obj;
			lastTrackedPos = gameObject.transform.position;
			lastAngle = gameObject.transform.eulerAngles.y;
			if (playerTracker != null)
			{
				Object.Destroy(playerTracker.gameObject);
			}
			playerTracker = gameObject.transform;
			ClearTrackedNulls();
		}
	}

	public void DetachPlayer([CanBeNull] Transform other = null)
	{
		if (lockParent)
		{
			return;
		}
		if (other == null)
		{
			trackedObjects.Clear();
		}
		else
		{
			trackedObjects.Remove(other);
		}
		if (trackedObjects.Count == 0)
		{
			Object.Destroy(playerTracker.gameObject);
			playerTracker = null;
			return;
		}
		ClearTrackedNulls();
		if (playerTracker != null && trackedObjects.Count > 0)
		{
			playerTracker.SetParent(trackedObjects.First());
		}
	}

	private void ClearTrackedNulls()
	{
		for (int num = trackedObjects.Count - 1; num >= 0; num--)
		{
			if (trackedObjects[num] == null)
			{
				trackedObjects.RemoveAt(num);
			}
		}
	}

	public void LockMovementParent(bool fuck)
	{
		lockParent = fuck;
	}

	public void LockMovementParentTeleport(bool fuck)
	{
		if ((bool)playerTracker)
		{
			if (fuck)
			{
				teleportLockDelta = lastTrackedPos - playerTracker.position;
			}
			if (lockParent && !fuck)
			{
				lastTrackedPos = playerTracker.position - teleportLockDelta;
			}
		}
		else
		{
			teleportLockDelta = lastTrackedPos;
		}
		lockParent = fuck;
	}
}
