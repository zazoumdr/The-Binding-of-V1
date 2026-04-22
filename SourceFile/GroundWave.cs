using System;
using ULTRAKILL.Portal;
using UnityEngine;
using UnityEngine.AI;

public class GroundWave : MonoBehaviour
{
	private NavMeshAgent nma;

	private AudioSource aud;

	private float originalPitch;

	public EnemyTarget target;

	public float lifetime;

	public float fadeOutSpeed = 1f;

	public float speedMultiplier = 1f;

	public bool startAtFullSpeed;

	public float fadeOutTime = 0.5f;

	private float currentFadeOutTime;

	private Vector3 originalScale;

	[SerializeField]
	private HurtZone hurtzone;

	private AffectedSubjects hurtZoneAffecteds;

	[SerializeField]
	private Transform faceDirection;

	[HideInInspector]
	public EnemyIdentifier eid;

	[HideInInspector]
	public int difficulty = -1;

	private Animator anim;

	private bool inGrabRange;

	private SimplePortalTraveler portalTraveler;

	private Rigidbody rb;

	private bool isTraversingLink;

	private bool hasCrossed;

	private Vector3 traversalVelocity;

	private float traversalSpeed;

	private float postTeleportDistance;

	private TimeSince sinceParried;

	private Vector3 parriedVelocity;

	private bool isParried;

	private void Start()
	{
		if (difficulty < 0)
		{
			difficulty = Enemy.InitializeDifficulty(eid);
		}
		aud = GetComponent<AudioSource>();
		originalPitch = aud.GetPitch();
		nma = GetComponent<NavMeshAgent>();
		NavMeshAgent obj = nma;
		obj.speed *= speedMultiplier;
		nma.velocity = base.transform.forward * nma.speed;
		nma.autoTraverseOffMeshLink = false;
		originalScale = base.transform.localScale;
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		sinceParried = 1f;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			if (!TryGetComponent<SimplePortalTraveler>(out portalTraveler))
			{
				portalTraveler = base.gameObject.AddComponent<SimplePortalTraveler>();
				portalTraveler.SetType(PortalTravellerType.ENEMY_PROJECTILE);
			}
			SimplePortalTraveler simplePortalTraveler = portalTraveler;
			simplePortalTraveler.onTravel = (PortalManagerV2.TravelCallback)Delegate.Combine(simplePortalTraveler.onTravel, new PortalManagerV2.TravelCallback(OnPortalTravel));
		}
		if ((bool)hurtzone)
		{
			hurtZoneAffecteds = hurtzone.affected;
		}
		TrackTick();
	}

	private void Update()
	{
		if (isParried)
		{
			rb.isKinematic = false;
			nma.updatePosition = false;
			rb.velocity = new Vector3(parriedVelocity.x, rb.velocity.y, parriedVelocity.z);
			if ((float)sinceParried >= 1f)
			{
				isParried = false;
				rb.isKinematic = true;
				nma.Warp(base.transform.position);
				nma.updatePosition = true;
				hurtzone.affected = hurtZoneAffecteds;
			}
		}
		if (!isTraversingLink && (bool)(UnityEngine.Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.isOnOffMeshLink)
		{
			UnityEngine.Object navMeshOwner = nma.navMeshOwner;
			if ((bool)navMeshOwner && navMeshOwner is PortalIdentifier)
			{
				Vector3 vector = nma.velocity;
				if (vector.sqrMagnitude < 0.1f)
				{
					vector = base.transform.forward * nma.speed;
				}
				traversalSpeed = nma.speed;
				traversalVelocity = vector.normalized * traversalSpeed;
				postTeleportDistance = 0f;
				hasCrossed = false;
				isTraversingLink = true;
				((Behaviour)(object)nma).enabled = false;
			}
			else
			{
				nma.CompleteOffMeshLink();
			}
		}
		lifetime = Mathf.MoveTowards(lifetime, 0f, Time.deltaTime);
		if ((bool)faceDirection)
		{
			if (isTraversingLink && traversalVelocity.sqrMagnitude > 0.01f)
			{
				faceDirection.LookAt(faceDirection.position + traversalVelocity);
			}
			else if ((bool)(UnityEngine.Object)(object)nma && ((Behaviour)(object)nma).enabled && nma.desiredVelocity.sqrMagnitude > 0.01f)
			{
				faceDirection.LookAt(faceDirection.position + nma.desiredVelocity);
			}
		}
		if (lifetime <= 0f)
		{
			if ((bool)hurtzone)
			{
				hurtzone.enabled = false;
			}
			currentFadeOutTime = Mathf.MoveTowards(currentFadeOutTime, fadeOutTime, Time.deltaTime * fadeOutSpeed);
			base.transform.localScale = new Vector3(Mathf.Lerp(originalScale.x, 0f, currentFadeOutTime / fadeOutTime), originalScale.y, Mathf.Lerp(originalScale.z, 0f, currentFadeOutTime / fadeOutTime));
			aud.SetPitch(currentFadeOutTime / fadeOutTime * originalPitch);
			if (currentFadeOutTime == fadeOutTime)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		if (Vector3.Distance(base.transform.position, target.position) < 5f)
		{
			if ((bool)(UnityEngine.Object)(object)anim && !inGrabRange)
			{
				anim.Play("Grab", 0, 0f);
			}
			inGrabRange = true;
		}
		else
		{
			inGrabRange = false;
		}
	}

	private void FixedUpdate()
	{
		if (!isTraversingLink)
		{
			return;
		}
		Vector3 vector = traversalVelocity * Time.fixedDeltaTime;
		base.transform.position += vector;
		if (traversalVelocity.sqrMagnitude > 0.001f)
		{
			base.transform.rotation = Quaternion.LookRotation(traversalVelocity.normalized, Vector3.up);
		}
		if ((bool)rb)
		{
			rb.position = base.transform.position;
			rb.rotation = base.transform.rotation;
		}
		if (hasCrossed)
		{
			postTeleportDistance += vector.magnitude;
			if (postTeleportDistance >= 2f)
			{
				isTraversingLink = false;
				((Behaviour)(object)nma).enabled = true;
				nma.Warp(base.transform.position);
				nma.velocity = traversalVelocity;
				nma.SetDestination(target.GetNavPoint());
			}
		}
	}

	private void OnPortalTravel(in PortalTravelDetails details)
	{
		if (isTraversingLink)
		{
			if ((bool)rb && rb.velocity.sqrMagnitude > 0.01f)
			{
				traversalVelocity = rb.velocity.normalized * traversalSpeed;
			}
			else
			{
				traversalVelocity = base.transform.forward * traversalSpeed;
			}
			hasCrossed = true;
			postTeleportDistance = 0f;
		}
	}

	public void TrackTick()
	{
		Invoke("TrackTick", 0.1f);
		if (!isTraversingLink && (bool)(UnityEngine.Object)(object)nma && ((Behaviour)(object)nma).enabled)
		{
			if ((float)sinceParried < 1f)
			{
				nma.SetDestination(base.transform.position + parriedVelocity * 10f);
			}
			else
			{
				nma.SetDestination(target.GetNavPoint());
			}
		}
	}

	public void Disappear()
	{
		lifetime = 0f;
	}

	public void DisappearWithSpeed(float newFadeOutSpeed = 1f)
	{
		Disappear();
		fadeOutSpeed = newFadeOutSpeed;
	}

	public void DisappearOnAndBelowDifficulty(int diff)
	{
		if (difficulty <= diff)
		{
			Disappear();
		}
	}

	public void ChangeVelocity(Vector3 newVelocity)
	{
		parriedVelocity = newVelocity;
		sinceParried = 0f;
		isParried = true;
		nma.updatePosition = false;
		rb.isKinematic = false;
		hurtzone.affected = AffectedSubjects.EnemiesOnly;
	}
}
