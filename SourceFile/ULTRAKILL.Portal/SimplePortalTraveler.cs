using System;
using ULTRAKILL.Portal.Native;
using Unity.Mathematics;
using UnityEngine;

namespace ULTRAKILL.Portal;

public class SimplePortalTraveler : MonoBehaviour, IPortalTraveller
{
	[Header("Transforms")]
	public bool transformPosition = true;

	public bool transformRotation = true;

	public bool transformVelocity = true;

	[Header("Extras")]
	public bool clearTrail = true;

	[HideInInspector]
	public Rigidbody rb;

	[HideInInspector]
	public TrailRenderer trail;

	private PortalManagerV2 pm;

	public PortalManagerV2.TravelCallback onTravel;

	public Func<PortalTravelDetails, bool?> onTravelOverride;

	public PortalManagerV2.TravelCallback onTravelBlocked;

	private Vector3 cachedRbPos;

	private Vector3 cachedRbVel;

	public PortalTravellerType travellerType { get; private set; }

	public int id => GetInstanceID();

	public int? targetId { get; set; }

	public Vector3 travellerPosition => cachedRbPos;

	public Vector3 travellerVelocity => cachedRbVel;

	public void SetType(PortalTravellerType type)
	{
		travellerType = type;
	}

	private void OnEnable()
	{
		CacheTravelerValues();
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			instance.UpdateTraveller(this);
		}
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		trail = GetComponent<TrailRenderer>();
		if (clearTrail && !trail)
		{
			trail = GetComponentInChildren<TrailRenderer>(includeInactive: true);
		}
	}

	private void Start()
	{
		pm = MonoSingleton<PortalManagerV2>.Instance;
		pm.AddTraveller(this, base.destroyCancellationToken);
	}

	private void OnDestroy()
	{
		_ = base.gameObject.scene.isLoaded;
	}

	private void FixedUpdate()
	{
		CacheTravelerValues();
	}

	private void CacheTravelerValues()
	{
		if ((bool)rb)
		{
			cachedRbPos = rb.position;
			cachedRbVel = rb.velocity;
		}
	}

	public bool? OnTravel(PortalTravelDetails details)
	{
		if (onTravelOverride != null)
		{
			onTravelOverride(details);
			return null;
		}
		Travel(details);
		onTravel?.Invoke(in details);
		return true;
	}

	public void Travel(PortalTravelDetails details)
	{
		Matrix4x4 enterToExit = details.enterToExit;
		if (transformPosition)
		{
			Vector3 position = base.transform.position;
			position = enterToExit.MultiplyPoint3x4(position);
			rb.position = position;
			base.transform.position = position;
			cachedRbPos = position;
		}
		if (transformRotation)
		{
			Quaternion rotation = base.transform.rotation;
			rotation = enterToExit.rotation * rotation;
			rb.rotation = rotation;
			base.transform.rotation = rotation;
		}
		if (transformVelocity && (bool)rb)
		{
			Vector3 velocity = rb.velocity;
			velocity = enterToExit.MultiplyVector(velocity);
			rb.velocity = velocity;
			cachedRbVel = velocity;
		}
		if (clearTrail && (bool)trail)
		{
			trail.Clear();
		}
	}

	public void OnTeleportBlocked(PortalTravelDetails details)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (onTravelBlocked != null)
		{
			onTravelBlocked(in details);
			return;
		}
		PortalHandle enterHandle = details.enterHandle;
		NativePortalTransform nativePortalTransform = PortalUtils.GetPortalObject(enterHandle).GetTransform(enterHandle.side);
		Vector3 vector = details.intersection + nativePortalTransform.backManaged * 0.01f;
		Vector3 normalized = Vector3.Reflect(rb.velocity, float3.op_Implicit(nativePortalTransform.back)).normalized;
		base.transform.position = vector;
		base.transform.LookAt(vector + normalized);
		rb.velocity = normalized * rb.velocity.magnitude;
	}
}
