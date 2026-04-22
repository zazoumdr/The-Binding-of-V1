using UnityEngine;

public class ClimbStep : MonoBehaviour
{
	private InputManager inman;

	private Rigidbody rb;

	private int layerMask;

	private NewMovement newMovement;

	private float step = 2.1f;

	private float allowedAngle = 0.1f;

	private float allowedSpeed = 0.1f;

	private float allowedInput = 0.5f;

	private float cooldown;

	private float cooldownMax = 0.1f;

	private float deltaVertical;

	private float deltaHorizontal = 0.6f;

	private Vector3 gizmoPosition1;

	private Vector3 gizmoPosition2;

	private Vector3 movementDirection;

	public Matrix4x4? portalTravelMatrix;

	private Transform targetTransform;

	private Rigidbody targetRb;

	public void SetTarget(Transform targetTransform, Rigidbody targetRb)
	{
		this.targetTransform = targetTransform;
		this.targetRb = targetRb;
	}

	public bool TryClimb(Vector3 position, RaycastHit hit, ref Matrix4x4 portalTravelMatrix, bool allowCollisionResolution = true)
	{
		if (MonoSingleton<NewMovement>.Instance.gc.forcedOff > 0)
		{
			return false;
		}
		Vector3 vector = -rb.GetGravityDirection();
		Vector3 verticalVelocity = Vector3.Project(rb.velocity, vector);
		float magnitude = verticalVelocity.magnitude;
		vector = portalTravelMatrix.MultiplyVector(vector).normalized;
		Vector3 relativeVelocity = portalTravelMatrix.MultiplyVector(rb.velocity).normalized * rb.velocity.magnitude;
		Vector3 vector2 = portalTravelMatrix.MultiplyVector(movementDirection).normalized * movementDirection.magnitude;
		return HandleCollision(position, vector, verticalVelocity, magnitude, hit.normal, relativeVelocity, vector2, allowCollisionResolution);
	}

	private void Awake()
	{
		targetTransform = base.transform;
		rb = GetComponent<Rigidbody>();
		layerMask = LayerMaskDefaults.Get(LMD.Environment);
	}

	private void Start()
	{
		newMovement = MonoSingleton<NewMovement>.Instance;
		inman = MonoSingleton<InputManager>.Instance;
	}

	private void FixedUpdate()
	{
		if (cooldown <= 0f)
		{
			cooldown = 0f;
		}
		else
		{
			cooldown -= Time.deltaTime;
		}
		Vector2 vector = MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>();
		movementDirection = Vector3.ClampMagnitude(vector.x * base.transform.right + vector.y * base.transform.forward, 1f);
		if (portalTravelMatrix.HasValue)
		{
			movementDirection = portalTravelMatrix.Value.MultiplyVector(movementDirection).normalized * movementDirection.magnitude;
		}
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (MonoSingleton<NewMovement>.Instance.gc.forcedOff <= 0 && layerMask == (layerMask | (1 << collisionInfo.collider.gameObject.layer)) && cooldown == 0f)
		{
			Vector3 vector = -rb.GetGravityDirection();
			Vector3 verticalVelocity = Vector3.Project(rb.velocity, vector);
			float magnitude = verticalVelocity.magnitude;
			Vector3 position = base.transform.position;
			ContactPoint[] contacts = collisionInfo.contacts;
			foreach (ContactPoint contactPoint in contacts)
			{
				HandleCollision(position, vector, verticalVelocity, magnitude, contactPoint.normal, collisionInfo.relativeVelocity, movementDirection);
			}
		}
	}

	private bool HandleCollision(Vector3 position, Vector3 up, Vector3 verticalVelocity, float verticalSpeed, Vector3 normal, Vector3 relativeVelocity, Vector3 movementDirection, bool allowCollisionResolution = true)
	{
		bool result = false;
		if ((verticalSpeed < allowedSpeed || allowedSpeed == 0f) && cooldown == 0f && ((Vector3.Dot(movementDirection, -Vector3.ProjectOnPlane(normal, up).normalized) > allowedInput && !newMovement.boost) || (Vector3.Dot(newMovement.dodgeDirection, -Vector3.ProjectOnPlane(normal, up).normalized) > allowedInput && newMovement.boost)) && Mathf.Abs(Vector3.Dot(up, normal)) < allowedAngle)
		{
			position += up * step + up * 0.25f;
			if (newMovement.sliding)
			{
				position += up * 1.125f;
			}
			Collider[] array = Physics.OverlapCapsule(position - up * step, position + up * 1.25f, 0.499999f, layerMask, QueryTriggerInteraction.Ignore);
			Collider[] array2 = Physics.OverlapCapsule(position - up * 1.25f - Vector3.ProjectOnPlane(normal, up) * 0.5f, position + up * 1.25f - Vector3.ProjectOnPlane(normal, up) * 0.5f, 0.5f, layerMask, QueryTriggerInteraction.Ignore);
			if (array.Length == 0 && array2.Length == 0)
			{
				cooldown = cooldownMax;
				Vector3 position2 = MonoSingleton<CameraController>.Instance.transform.position;
				float num = 1.75f;
				if (!Physics.Raycast(position - up * num - Vector3.ProjectOnPlane(normal, up).normalized * deltaHorizontal, -up, out var hitInfo, step, layerMask, QueryTriggerInteraction.Ignore))
				{
					if (allowCollisionResolution)
					{
						Vector3 vector = up * (step + deltaVertical) - Vector3.ProjectOnPlane(normal, up).normalized * deltaHorizontal;
						base.transform.position += vector;
						rb.velocity = -relativeVelocity;
						if (portalTravelMatrix.HasValue)
						{
							targetTransform.position += portalTravelMatrix.Value.inverse.MultiplyVector(vector);
							targetRb.velocity = -portalTravelMatrix.Value.MultiplyVector(relativeVelocity);
						}
					}
					return true;
				}
				if (hitInfo.collider.TryGetComponent<CustomGroundProperties>(out var component) && !component.canClimbStep)
				{
					return false;
				}
				if (allowCollisionResolution)
				{
					rb.velocity -= verticalVelocity;
					Vector3 vector2 = up * (step + deltaVertical - hitInfo.distance) - Vector3.ProjectOnPlane(normal, up).normalized * deltaHorizontal;
					base.transform.position += vector2;
					if (portalTravelMatrix.HasValue)
					{
						targetTransform.position += portalTravelMatrix.Value.inverse.MultiplyVector(vector2);
						targetRb.velocity = -portalTravelMatrix.Value.MultiplyVector(relativeVelocity);
					}
					rb.velocity = -relativeVelocity;
				}
				result = true;
				if (allowCollisionResolution)
				{
					MonoSingleton<CameraController>.Instance.transform.position = position2;
					MonoSingleton<CameraController>.Instance.defaultPos = MonoSingleton<CameraController>.Instance.transform.localPosition;
				}
			}
		}
		return result;
	}
}
