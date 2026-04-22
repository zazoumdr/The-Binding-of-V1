using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ULTRAKILL.Portal.Native;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace ULTRAKILL.Portal;

[DefaultExecutionOrder(1000)]
public class PortalAwarePlayerCollider : MonoBehaviour
{
	private class Clone
	{
		public GameObject GameObject;

		public Transform Transform;

		public CapsuleCollider Collider;

		public Rigidbody Rigidbody;

		public PortalAwarePlayerColliderClone ColliderClone;

		public GroundCheck GroundCheck;

		public GroundCheck SlopeCheck;

		public WallCheck WallCheck;

		public ClimbStep ClimbStep;

		public bool WasActive;
	}

	private static readonly List<PortalHandle> reusablePortalHandles = new List<PortalHandle>();

	private readonly ConcurrentDictionary<int, Matrix4x4> colliderTravelMatrices = new ConcurrentDictionary<int, Matrix4x4>();

	private readonly ConcurrentDictionary<int, Plane> colliderPortalPlanes = new ConcurrentDictionary<int, Plane>();

	private readonly List<Clone> clones = new List<Clone>();

	private readonly RaycastHit[] reusableHits = new RaycastHit[32];

	private readonly HashSet<PortalHandle> portalsToIgnore = new HashSet<PortalHandle>();

	private readonly HashSet<long> portalHashCodes = new HashSet<long>();

	private readonly List<(PortalHandle handle, Plane plane)> activePortals = new List<(PortalHandle, Plane)>();

	private static Comparison<PortalHandle> portalSortComparison;

	private static Vector3 portalSortCenter;

	[SerializeField]
	private CapsuleCollider targetCollider;

	[SerializeField]
	private ClimbStep climbStep;

	[SerializeField]
	private GroundCheckGroup scGroup;

	[SerializeField]
	private GroundCheckGroup gcGroup;

	[SerializeField]
	private WallCheckGroup wcGroup;

	[SerializeField]
	private WallCheck wcTemplate;

	[SerializeField]
	private GroundCheck gcTemplate;

	[SerializeField]
	private GroundCheck scTemplate;

	[SerializeField]
	private LayerMask blockerLayerMask;

	private Rigidbody rb;

	private NativePortalScene nativeScene;

	public void IgnorePortalHandle(PortalHandle handle)
	{
		portalsToIgnore.Add(handle);
		portalsToIgnore.Add(handle.Reverse());
	}

	public bool TryGetCrossingPortal(Vector3 from, Vector3 to, out PortalHandle handle, out Vector3 intersection, out Vector3 portalNormal)
	{
		foreach (var (portalHandle, plane) in activePortals)
		{
			if (plane.GetSide(from) != plane.GetSide(to))
			{
				handle = portalHandle;
				Ray ray = new Ray(from, to - from);
				if (plane.Raycast(ray, out var enter))
				{
					intersection = ray.GetPoint(enter);
				}
				else
				{
					intersection = Vector3.Lerp(from, to, 0.5f);
				}
				portalNormal = (plane.GetSide(to) ? plane.normal : (-plane.normal));
				return true;
			}
		}
		handle = default(PortalHandle);
		intersection = default(Vector3);
		portalNormal = default(Vector3);
		return false;
	}

	private void FixedUpdate()
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		int intersectingPortalHandles = GetIntersectingPortalHandles(reusablePortalHandles);
		activePortals.Clear();
		if (rb.isKinematic || intersectingPortalHandles == 0)
		{
			DeactivateClones();
			portalsToIgnore.Clear();
			return;
		}
		for (int i = 0; i < clones.Count; i++)
		{
			Clone clone = clones[i];
			clone.WasActive = clone.GameObject.activeSelf;
		}
		EnsureClones(intersectingPortalHandles);
		NativePortalScene nativePortalScene = MonoSingleton<PortalManagerV2>.Instance.Scene.nativeScene;
		int count = clones.Count;
		float fixedDeltaTime = Time.fixedDeltaTime;
		NewMovement instance = MonoSingleton<NewMovement>.Instance;
		Transform obj = instance.transform;
		Vector3 up = obj.up;
		_ = obj.localToWorldMatrix;
		obj.GetPositionAndRotation(out var position, out var rotation);
		for (int j = 0; j < count; j++)
		{
			Clone clone2 = clones[j];
			if (clone2 == null || !clone2.GameObject.activeSelf)
			{
				continue;
			}
			PortalHandle handle = reusablePortalHandles[j];
			if (portalsToIgnore.Contains(handle))
			{
				DeactivateClone(clone2);
				continue;
			}
			NativePortal nativePortal = nativePortalScene.LookupPortal(in handle);
			ref Plane plane = ref nativePortal.plane;
			Plane val = plane;
			float num = ((Plane)(ref val)).SignedDistanceToPoint(float3.op_Implicit(position));
			if (num > 0f)
			{
				Vector3 velocity = rb.velocity;
				val = plane;
				float num2 = 0f - Vector3.Dot(velocity, float3.op_Implicit(((Plane)(ref val)).Normal));
				float num3 = targetCollider.radius + Mathf.Max(num2 * fixedDeltaTime * 2f, 0f);
				if (num > num3)
				{
					DeactivateClone(clone2);
					continue;
				}
			}
			NativePortal nativePortal2 = nativePortalScene.LookupPortal(handle.Reverse());
			Matrix4x4 portalTravelMatrix = nativePortal.travelMatrixManaged;
			Matrix4x4 travelMatrixManaged = nativePortal2.travelMatrixManaged;
			List<(PortalHandle handle, Plane plane)> list = activePortals;
			PortalHandle item = handle;
			val = plane;
			Vector3 inNormal = float3.op_Implicit(((Plane)(ref val)).Normal);
			val = plane;
			list.Add((item, new Plane(inNormal, ((Plane)(ref val)).Distance)));
			Vector3 normalized = portalTravelMatrix.MultiplyVector(rb.velocity).normalized;
			float magnitude = rb.velocity.magnitude;
			Quaternion quaternion = portalTravelMatrix.rotation * rotation;
			Vector3 normalized2 = portalTravelMatrix.MultiplyVector(up).normalized;
			Vector3 vector = portalTravelMatrix.MultiplyPoint3x4(instance.cc.cam.transform.position) - portalTravelMatrix.MultiplyVector(instance.cc.cam.transform.position - position);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, quaternion, Vector3.one);
			float num4 = targetCollider.height * 0.45f - targetCollider.radius;
			Vector3 vector2 = vector - normalized2 * num4;
			Vector3 point = vector + normalized2 * num4;
			Vector3 point2 = vector2 + matrix4x.MultiplyVector(targetCollider.center);
			point += matrix4x.MultiplyVector(targetCollider.center);
			int num5 = Physics.CapsuleCastNonAlloc(point2, point, targetCollider.radius, normalized, reusableHits, magnitude * fixedDeltaTime, blockerLayerMask, QueryTriggerInteraction.Ignore);
			ref Plane plane2 = ref nativePortal2.plane;
			for (int k = 0; k < num5; k++)
			{
				ref RaycastHit reference = ref reusableHits[k];
				if (climbStep.TryClimb(vector, reference, ref portalTravelMatrix, allowCollisionResolution: false))
				{
					continue;
				}
				if (reference.distance == 0f)
				{
					Vector3 vector3 = rb.velocity * fixedDeltaTime;
					Vector3 vector4 = rb.position + vector3;
					Vector3 velocity2 = rb.velocity;
					val = plane;
					if (Vector3.Dot(velocity2, float3.op_Implicit(((Plane)(ref val)).Normal)) > 0f)
					{
						val = plane;
						if (((Plane)(ref val)).SignedDistanceToPoint(float3.op_Implicit(vector4)) + targetCollider.radius > 0f)
						{
							Rigidbody rigidbody = rb;
							Vector3 velocity3 = rb.velocity;
							val = plane;
							rigidbody.velocity = Vector3.ProjectOnPlane(velocity3, float3.op_Implicit(((Plane)(ref val)).Normal));
						}
					}
				}
				else
				{
					val = plane2;
					if (!(((Plane)(ref val)).SignedDistanceToPoint(float3.op_Implicit(reference.point)) > 0f) && !(Vector3.Dot(reference.normal, normalized2) >= 0.8f))
					{
						Vector3 planeNormal = travelMatrixManaged.MultiplyVector(reference.normal);
						rb.velocity = Vector3.ProjectOnPlane(rb.velocity, planeNormal);
					}
				}
			}
			if (!clone2.WasActive)
			{
				clone2.Transform.SetPositionAndRotation(vector, quaternion);
			}
			else
			{
				Vector3 velocity4 = portalTravelMatrix.MultiplyVector(rb.velocity);
				clone2.Rigidbody.Move(vector, quaternion);
				clone2.Rigidbody.velocity = velocity4;
			}
			ApplyCapsuleColliderProperties(clone2.Collider);
			int instanceID = clone2.Collider.GetInstanceID();
			colliderTravelMatrices[instanceID] = portalTravelMatrix;
			ConcurrentDictionary<int, Plane> concurrentDictionary = colliderPortalPlanes;
			val = plane;
			Vector3 inNormal2 = float3.op_Implicit(((Plane)(ref val)).Normal);
			val = plane;
			concurrentDictionary[instanceID] = new Plane(inNormal2, ((Plane)(ref val)).Distance);
			clone2.ColliderClone.TravelMatrix = travelMatrixManaged;
			PortalAwarePlayerColliderClone colliderClone = clone2.ColliderClone;
			val = plane2;
			Vector3 inNormal3 = float3.op_Implicit(((Plane)(ref val)).Normal);
			val = plane2;
			colliderClone.PortalPlane = new Plane(inNormal3, ((Plane)(ref val)).Distance);
			clone2.WallCheck.portalTravelMatrix = travelMatrixManaged;
			clone2.ClimbStep.portalTravelMatrix = travelMatrixManaged;
			GroundCheck groundCheck = clone2.GroundCheck;
			val = plane2;
			Vector3 inNormal4 = float3.op_Implicit(((Plane)(ref val)).Normal);
			val = plane2;
			groundCheck.PortalPlane = new Plane(inNormal4, ((Plane)(ref val)).Distance);
		}
		portalsToIgnore.Clear();
	}

	private void EnsureClones(int desiredCloneCount)
	{
		int num = desiredCloneCount - clones.Count;
		for (int i = 0; i < num; i++)
		{
			CreateClone();
		}
		int count = clones.Count;
		for (int j = 0; j < count; j++)
		{
			Clone clone = clones[j];
			if (j < desiredCloneCount)
			{
				ActivateClone(clone);
			}
			else
			{
				DeactivateClone(clone);
			}
		}
	}

	private int GetIntersectingPortalHandles(List<PortalHandle> handles)
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		if (handles == null)
		{
			throw new ArgumentNullException("handles");
		}
		portalHashCodes.Clear();
		handles.Clear();
		if (!MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance))
		{
			return 0;
		}
		PortalScene scene = instance.Scene;
		Bounds bounds = targetCollider.bounds;
		Vector3 vector = rb.velocity * Time.deltaTime;
		Bounds bounds2 = bounds;
		bounds2.center += vector;
		bounds.Encapsulate(bounds2);
		bounds.min -= Vector3.one * 0.5f;
		bounds.max += Vector3.one * 0.5f;
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		float num = 1.4142135f * Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
		float num2 = num * num;
		Vector3 vector2 = default(Vector3);
		nativeScene = scene.nativeScene;
		if (!nativeScene.valid)
		{
			return 0;
		}
		foreach (NativePortal portal2 in nativeScene.portals)
		{
			NativePortal portal = portal2;
			if (!portal.travellerFlags.HasAllFlags(PortalTravellerFlags.Player))
			{
				continue;
			}
			PortalHandle handle = portal.handle;
			float3 center2 = portal.transform.center;
			float num3 = math.max(portal.dimensions.x, portal.dimensions.y);
			float num4 = num3 * num3;
			vector2.x = center2.x - center.x;
			vector2.y = center2.y - center.y;
			vector2.z = center2.z - center.z;
			if (!(vector2.sqrMagnitude - num2 > num4))
			{
				PortalAwareRenderer.CacheBoundsVertices(ref center, ref extents);
				PortalAwareRenderer.CachePortalVertices(in portal);
				if (PortalAwareRenderer.IntersectCachedShapes())
				{
					handles.Add(handle);
					portalHashCodes.Add(handle.PackedKey);
				}
			}
		}
		nativeScene = scene.nativeScene;
		handles.Sort(portalSortComparison);
		for (int num5 = handles.Count - 1; num5 >= 0; num5--)
		{
			PortalHandle portalHandle = handles[num5];
			if (portalHashCodes.Contains(portalHandle.Reverse().PackedKey))
			{
				handles.RemoveAt(num5);
				portalHashCodes.Remove(portalHandle.PackedKey);
			}
		}
		return handles.Count;
	}

	private void DeactivateClones()
	{
		foreach (Clone clone in clones)
		{
			DeactivateClone(clone);
		}
	}

	private void ActivateClone(Clone clone)
	{
		if (clone != null && !(clone.GameObject == null))
		{
			clone.GameObject.SetActive(value: true);
			clone.GroundCheck.ForceGroundCheck();
			clone.SlopeCheck.ForceGroundCheck();
			clone.WallCheck.UpdateOnWall();
		}
	}

	private void DeactivateClone(Clone clone)
	{
		if (clone != null && !(clone.GameObject == null))
		{
			clone.GameObject.SetActive(value: false);
		}
	}

	private void CreateClone()
	{
		GameObject gameObject = new GameObject("Player Collider Clone")
		{
			layer = targetCollider.gameObject.layer
		};
		CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.hasModifiableContacts = true;
		capsuleCollider.sharedMaterial = targetCollider.sharedMaterial;
		ApplyCapsuleColliderProperties(capsuleCollider);
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		rigidbody.interpolation = RigidbodyInterpolation.None;
		rigidbody.collisionDetectionMode = rb.collisionDetectionMode;
		rigidbody.SetGravityMode(useGravity: false);
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		rigidbody.mass = rb.mass;
		PortalAwarePlayerColliderClone portalAwarePlayerColliderClone = gameObject.AddComponent<PortalAwarePlayerColliderClone>();
		portalAwarePlayerColliderClone.TargetRigidbody = rb;
		WallCheck wallCheck = UnityEngine.Object.Instantiate(wcTemplate, gameObject.transform);
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)wallCheck.GetComponent<AudioSource>());
		wcTemplate.transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
		wallCheck.transform.SetLocalPositionAndRotation(localPosition, localRotation);
		wcGroup.AddInstance(wallCheck);
		GroundCheck groundCheck = UnityEngine.Object.Instantiate(scTemplate, gameObject.transform);
		UnityEngine.Object.Destroy(groundCheck.GetComponent<GroundCheckGroup>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)groundCheck.GetComponent<AudioSource>());
		scTemplate.transform.GetLocalPositionAndRotation(out var localPosition2, out var localRotation2);
		groundCheck.transform.SetLocalPositionAndRotation(localPosition2, localRotation2);
		groundCheck.ForceGroundCheck();
		scGroup.AddInstance(groundCheck);
		GroundCheck groundCheck2 = UnityEngine.Object.Instantiate(gcTemplate, gameObject.transform);
		UnityEngine.Object.Destroy(groundCheck2.GetComponent<GroundCheckGroup>());
		UnityEngine.Object.Destroy((UnityEngine.Object)(object)groundCheck2.GetComponent<AudioSource>());
		foreach (Transform item2 in groundCheck2.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		gcTemplate.transform.GetLocalPositionAndRotation(out var localPosition3, out var localRotation3);
		groundCheck2.transform.SetLocalPositionAndRotation(localPosition3, localRotation3);
		groundCheck2.ForceGroundCheck();
		gcGroup.AddInstance(groundCheck2);
		ClimbStep climbStep = gameObject.AddComponent<ClimbStep>();
		climbStep.SetTarget(rb.transform, rb);
		Clone item = new Clone
		{
			GameObject = gameObject,
			Transform = gameObject.transform,
			Collider = capsuleCollider,
			Rigidbody = rigidbody,
			ColliderClone = portalAwarePlayerColliderClone,
			GroundCheck = groundCheck2,
			SlopeCheck = groundCheck,
			WallCheck = wallCheck,
			ClimbStep = climbStep
		};
		Physics.IgnoreCollision(capsuleCollider, targetCollider);
		for (int i = 0; i < clones.Count; i++)
		{
			Physics.IgnoreCollision(capsuleCollider, clones[i].Collider);
		}
		clones.Add(item);
		gameObject.SetActive(value: false);
	}

	private void ApplyCapsuleColliderProperties(CapsuleCollider cc)
	{
		cc.height = targetCollider.height;
		cc.radius = targetCollider.radius;
		cc.center = targetCollider.center;
		cc.direction = targetCollider.direction;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		targetCollider.hasModifiableContacts = true;
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 _))
		{
			portalSortComparison = delegate(PortalHandle a, PortalHandle b)
			{
				NativePortal nativePortal = nativeScene.LookupPortal(in a);
				NativePortal nativePortal2 = nativeScene.LookupPortal(in b);
				Vector3 centerManaged = nativePortal.transform.centerManaged;
				Vector3 centerManaged2 = nativePortal2.transform.centerManaged;
				float num = Vector3.SqrMagnitude(centerManaged - portalSortCenter);
				float value = Vector3.SqrMagnitude(centerManaged2 - portalSortCenter);
				return num.CompareTo(value);
			};
		}
	}

	private void OnEnable()
	{
		Physics.ContactModifyEvent += ModifyEvent;
		Physics.ContactModifyEventCCD += ModifyEvent;
	}

	private void OnDisable()
	{
		Physics.ContactModifyEvent -= ModifyEvent;
		Physics.ContactModifyEventCCD -= ModifyEvent;
	}

	private void OnDestroy()
	{
		if (wcGroup != null)
		{
			foreach (Clone clone in clones)
			{
				wcGroup.RemoveInstance(clone.WallCheck);
			}
		}
		foreach (Clone clone2 in clones)
		{
			if (clone2 != null && !(clone2.GameObject == null))
			{
				UnityEngine.Object.Destroy(clone2.GameObject);
			}
		}
	}

	private void ModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
	{
		foreach (ModifiableContactPair item in pairs)
		{
			for (int i = 0; i < item.contactCount; i++)
			{
				if (colliderPortalPlanes.TryGetValue(item.colliderInstanceID, out var value))
				{
					Vector3 point = item.GetPoint(i);
					if (value.GetSide(point))
					{
						item.IgnoreContact(i);
						continue;
					}
				}
				if (colliderTravelMatrices.TryGetValue(item.colliderInstanceID, out var value2))
				{
					item.IgnoreContact(i);
					float maxImpulse = item.GetMaxImpulse(i);
					maxImpulse = Mathf.Max(maxImpulse, item.bodyVelocity.magnitude);
					item.SetMaxImpulse(i, maxImpulse);
					Vector3 normal = item.GetNormal(i);
					item.SetNormal(i, value2.MultiplyVector(normal).normalized);
				}
			}
		}
	}
}
