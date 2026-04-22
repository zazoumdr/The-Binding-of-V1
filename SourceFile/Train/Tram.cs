using System.Linq;
using UnityEngine;

namespace Train;

public class Tram : MonoBehaviour
{
	public bool poweredOn = true;

	private AudioSource aud;

	public GameObject bonkSound;

	public GameObject deathZones;

	[HideInInspector]
	public float zapAmount;

	public float speed;

	public ConnectedTram[] connectedTrams;

	[Space]
	public TrainTrackPoint currentPoint;

	public TramPath currentPath;

	private ScreenZone[] screenActivators;

	[HideInInspector]
	public TramControl controller;

	private Rigidbody rb;

	public bool canGoForward { get; private set; }

	public bool canGoBackward { get; private set; }

	public TramMovementDirection movementDirection
	{
		get
		{
			if (!(speed > 0f))
			{
				if (!(speed < 0f))
				{
					return TramMovementDirection.None;
				}
				return TramMovementDirection.Backward;
			}
			return TramMovementDirection.Forward;
		}
	}

	public float directionMod => (speed > 0f) ? 1 : (-1);

	public float computedSpeed => speed * inheritedSpeedMultiplier;

	public float inheritedSpeedMultiplier
	{
		get
		{
			if (!(zapAmount > 0f))
			{
				return currentPath?.MaxSpeedMultiplier(movementDirection, speed) ?? 1f;
			}
			return Mathf.Lerp(currentPath?.MaxSpeedMultiplier(movementDirection, speed) ?? 1f, 2f, zapAmount);
		}
	}

	public float backwardOffset
	{
		get
		{
			if (connectedTrams != null && connectedTrams.Length != 0)
			{
				return connectedTrams.Sum((ConnectedTram tram) => tram.offset);
			}
			return 0f;
		}
	}

	public void TurnOn()
	{
		poweredOn = true;
		if (screenActivators != null && screenActivators.Length != 0)
		{
			ScreenZone[] array = screenActivators;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: true);
			}
		}
	}

	public void ShutDown()
	{
		poweredOn = false;
		if (screenActivators == null || screenActivators.Length == 0)
		{
			return;
		}
		ScreenZone[] array = screenActivators;
		foreach (ScreenZone screenZone in array)
		{
			ObjectActivator[] components = screenZone.GetComponents<ObjectActivator>();
			if (components != null && components.Length != 0)
			{
				ObjectActivator[] array2 = components;
				foreach (ObjectActivator objectActivator in array2)
				{
					if (objectActivator.events.toActivateObjects != null && objectActivator.events.toActivateObjects.Length != 0)
					{
						GameObject[] toActivateObjects = objectActivator.events.toActivateObjects;
						for (int k = 0; k < toActivateObjects.Length; k++)
						{
							toActivateObjects[k].SetActive(value: false);
						}
					}
				}
			}
			screenZone.gameObject.SetActive(value: false);
		}
	}

	public void StopAndTeleport(TrainTrackPoint point)
	{
		currentPoint = point;
		currentPath = null;
		speed = 0f;
		TrainTrackPoint destination = currentPoint.GetDestination();
		TrainTrackPoint destination2 = currentPoint.GetDestination(forward: false);
		TramPath tramPath = null;
		if ((bool)destination)
		{
			tramPath = new TramPath(currentPoint, destination);
		}
		else if ((bool)destination2)
		{
			tramPath = new TramPath(destination2, currentPoint);
			tramPath.distanceTravelled = tramPath.DistanceTotal;
		}
		if (tramPath != null)
		{
			currentPath = tramPath;
			UpdateWorldRotation();
			ConnectedTram[] array = connectedTrams;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateTram(currentPath);
			}
			currentPath = null;
		}
	}

	private void Awake()
	{
		aud = GetComponent<AudioSource>();
		screenActivators = GetComponentsInChildren<ScreenZone>();
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		UpdateAudio();
	}

	private void FixedUpdate()
	{
		if (currentPath == null && currentPoint != null)
		{
			rb.MovePosition(currentPoint.transform.position);
			canGoForward = currentPoint.GetDestination() != null;
			canGoBackward = currentPoint.GetDestination(forward: false) != null;
		}
		if (speed != 0f)
		{
			if (currentPath == null && currentPoint != null)
			{
				ReceiveNewPath();
			}
			if (currentPath != null)
			{
				TraversePath();
			}
		}
		if ((bool)deathZones)
		{
			deathZones.SetActive(speed != 0f);
		}
		if (currentPath != null)
		{
			UpdateWorldPosition();
			if (movementDirection != TramMovementDirection.None)
			{
				UpdateWorldRotation();
			}
			DrawPathPreview();
			ConnectedTram[] array = connectedTrams;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateTram(currentPath);
			}
		}
	}

	private void DrawPathPreview()
	{
		if (Debug.isDebugBuild)
		{
			int num = 16;
			_ = rb.position;
			for (int i = 0; i < num; i++)
			{
				float progress = currentPath.Progress + (float)i / (float)num * directionMod;
				currentPath.GetPointOnPath(progress);
				_ = Vector3.up * 1f + new Vector3(0f, 0.125f, 0f);
			}
		}
	}

	private void TraversePath()
	{
		currentPath.distanceTravelled += computedSpeed * Time.deltaTime;
		if (!IsAtEndOfPath())
		{
			canGoForward = true;
			canGoBackward = true;
			return;
		}
		float num = currentPath.distanceTravelled;
		if (movementDirection == TramMovementDirection.Forward)
		{
			num -= currentPath.DistanceTotal;
		}
		if (movementDirection == TramMovementDirection.Backward && backwardOffset != 0f && currentPath.IsDeadEnd(movementDirection))
		{
			speed = 0f;
			canGoBackward = false;
			Object.Instantiate(bonkSound, rb.position, Quaternion.identity);
			return;
		}
		currentPoint = ((movementDirection == TramMovementDirection.Forward) ? currentPath.end : currentPath.start);
		_ = currentPoint != null;
		currentPath = null;
		ReceiveNewPath();
		if (currentPath != null)
		{
			currentPath.distanceTravelled += num;
			if (movementDirection == TramMovementDirection.Forward)
			{
				canGoForward = true;
			}
			else
			{
				canGoBackward = true;
			}
			return;
		}
		if (currentPoint.stopBehaviour == StopBehaviour.InstantClank || zapAmount > 0f)
		{
			Object.Instantiate(bonkSound, rb.position, Quaternion.identity);
		}
		if (movementDirection == TramMovementDirection.Forward)
		{
			canGoForward = false;
		}
		else
		{
			canGoBackward = false;
		}
		speed = 0f;
	}

	private bool IsAtEndOfPath()
	{
		if (currentPath == null)
		{
			return false;
		}
		float distanceTotal = currentPath.DistanceTotal;
		float num = 0f;
		if (movementDirection == TramMovementDirection.Backward && backwardOffset != 0f && currentPath.start.GetDestination(forward: false) == null)
		{
			num += backwardOffset;
		}
		if (movementDirection != TramMovementDirection.Forward)
		{
			return currentPath.distanceTravelled <= num;
		}
		return currentPath.distanceTravelled >= distanceTotal;
	}

	public void UpdateWorldPosition()
	{
		if (currentPath != null)
		{
			Vector3 pointOnPath = currentPath.GetPointOnPath(currentPath.Progress);
			rb.MovePosition(pointOnPath);
		}
	}

	public void UpdateWorldRotation()
	{
		if (currentPath != null)
		{
			Quaternion rotation = Quaternion.LookRotation(currentPath.MovementDirection(), Vector3.up);
			rb.rotation = rotation;
		}
	}

	private void ReceiveNewPath()
	{
		if (currentPoint == null)
		{
			return;
		}
		bool flag = movementDirection == TramMovementDirection.Forward;
		TrainTrackPoint destination = currentPoint.GetDestination(flag);
		if (!(destination == null))
		{
			TrainTrackPoint start = (flag ? currentPoint : destination);
			TrainTrackPoint end = (flag ? destination : currentPoint);
			TramPath tramPath = new TramPath(start, end);
			if (!flag)
			{
				tramPath.distanceTravelled = tramPath.DistanceTotal;
			}
			currentPath = tramPath;
			currentPoint = null;
		}
	}

	private void UpdateAudio()
	{
		if (computedSpeed != 0f && !aud.isPlaying)
		{
			aud.Play(tracked: true);
		}
		else if (computedSpeed == 0f && aud.isPlaying)
		{
			aud.Stop();
		}
		float num = 0f;
		num = ((!(Mathf.Abs(computedSpeed) >= 50f)) ? (Mathf.Abs(computedSpeed) * 0.02f) : ((zapAmount > 0f) ? Mathf.Lerp(1f, 1.5f, zapAmount) : 1f));
		aud.volume = num;
		aud.SetPitch(num * 2f);
	}
}
