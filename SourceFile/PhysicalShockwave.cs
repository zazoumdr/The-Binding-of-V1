using System.Collections.Generic;
using System.Linq;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class PhysicalShockwave : MonoBehaviour
{
	private struct OriginDepthPair(Transform origin, int depth, PortalHandleSequence portalSequence = default(PortalHandleSequence))
	{
		public readonly Transform origin = origin;

		public readonly int depth = depth;

		public readonly PortalHandleSequence portalSequence = portalSequence;
	}

	private const int MaxReplicaCount = 12;

	public EnemyTarget target;

	public int damage;

	public float speed;

	public float maxSize;

	public float force;

	public bool hasHurtPlayer;

	public bool ignorePlayerDash;

	public bool enemy;

	public bool noDamageToEnemy;

	private List<Collider> hitColliders = new List<Collider>();

	public EnemyType enemyType;

	public GameObject soundEffect;

	[HideInInspector]
	public bool fading;

	private ScaleNFade[] faders;

	private GameObject[] portalReplicas;

	private HashSet<PortalHandle> replicaPortalHandles;

	private void Start()
	{
		if (soundEffect != null)
		{
			Object.Instantiate(soundEffect, base.transform.position, Quaternion.identity);
		}
		faders = GetComponentsInChildren<ScaleNFade>();
		if (!fading)
		{
			ScaleNFade[] array = faders;
			foreach (ScaleNFade obj in array)
			{
				obj.enabled = false;
				obj.fade = true;
				obj.fadeSpeed = speed / 10f;
			}
		}
		if (MonoSingleton<PortalManagerV2>.TryGetInstance(out PortalManagerV2 instance) && instance.Scene != null)
		{
			CreatePortalReplicas();
		}
	}

	private void CreatePortalReplicas()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		PortalScene scene = MonoSingleton<PortalManagerV2>.Instance.Scene;
		_ = scene.nativeScene;
		NativeList<NativePortal> portals = scene.nativeScene.portals;
		List<GameObject> list = new List<GameObject>();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Bounds boundsOverride = new Bounds(base.transform.position, Vector3.zero);
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			boundsOverride.Encapsulate(collider.bounds);
		}
		float y = boundsOverride.size.y;
		boundsOverride.Expand(maxSize * 2f);
		Vector3 size = boundsOverride.size;
		size.y = y;
		boundsOverride.size = size;
		Queue<OriginDepthPair> queue = new Queue<OriginDepthPair>(new OriginDepthPair[1]
		{
			new OriginDepthPair(base.transform, 1)
		});
		while (queue.Count > 0 && list.Count < 12)
		{
			OriginDepthPair originDepthPair = queue.Dequeue();
			int depth = originDepthPair.depth;
			Transform origin = originDepthPair.origin;
			foreach (NativePortal item in portals)
			{
				PortalHandle handle = item.handle;
				if (originDepthPair.portalSequence.Contains(handle))
				{
					continue;
				}
				NativePortalTransform nativePortalTransform = item.transform;
				PortalVertices vertices = item.vertices;
				float y2 = vertices[0].y;
				float y3 = vertices[0].y;
				for (int j = 1; j < vertices.Length; j++)
				{
					if (vertices[j].y < y2)
					{
						y2 = vertices[j].y;
					}
					if (vertices[j].y > y3)
					{
						y3 = vertices[j].y;
					}
				}
				float y4 = origin.position.y;
				float num = y * 0.5f;
				float num2 = y4 - num;
				if (y4 + num < y2 || num2 > y3)
				{
					continue;
				}
				Vector2 a = new Vector2(origin.position.x, origin.position.z);
				Vector2 b = new Vector2(nativePortalTransform.center.x, nativePortalTransform.center.z);
				if (!(Vector2.Distance(a, b) < maxSize * 2f))
				{
					continue;
				}
				Collider[] componentsInChildren2 = origin.GetComponentsInChildren<Collider>();
				if (componentsInChildren2.Length == 0)
				{
					continue;
				}
				Matrix4x4 travelMatrix = scene.GetTravelMatrix(new PortalHandleSequence(handle));
				Vector3 position = travelMatrix.MultiplyPoint3x4(origin.position);
				Vector3 normalized = travelMatrix.MultiplyVector(origin.forward).normalized;
				Vector3 normalized2 = travelMatrix.MultiplyVector(origin.up).normalized;
				GameObject gameObject = new GameObject($"PhysicalShockwave {GetInstanceID()} - Portal Replica {handle} Depth {depth}");
				gameObject.transform.SetParent(origin.parent);
				gameObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normalized, normalized2));
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.detectCollisions = true;
				NativePortalTransform nativePortalTransform2 = scene.GetPortalObject(handle).GetTransform(handle.side);
				int num3 = 0;
				array = componentsInChildren2;
				foreach (Collider collider2 in array)
				{
					if (collider2.transform == origin)
					{
						if (collider2 is MeshCollider meshCollider)
						{
							MeshCollider meshCollider2 = gameObject.AddComponent<MeshCollider>();
							meshCollider2.sharedMesh = meshCollider.sharedMesh;
							meshCollider2.convex = meshCollider.convex;
							meshCollider2.isTrigger = meshCollider.isTrigger;
							num3++;
							PhysicalShockwaveCollisionProxy physicalShockwaveCollisionProxy = gameObject.AddComponent<PhysicalShockwaveCollisionProxy>();
							physicalShockwaveCollisionProxy.receiver = this;
							physicalShockwaveCollisionProxy.portalHandle = handle;
							physicalShockwaveCollisionProxy.previousOrigin = origin;
						}
						else
						{
							Debug.LogError("Collider type not currently supported for portal replication.", base.gameObject);
						}
						continue;
					}
					Vector3 center = collider2.bounds.center;
					bool num4 = Vector3.Dot(center - nativePortalTransform2.centerManaged, float3.op_Implicit(nativePortalTransform2.forward)) < 0f;
					Vector3 normalized3 = (center - origin.position).normalized;
					Vector3 normalized4 = (nativePortalTransform2.centerManaged - center).normalized;
					bool flag = Vector3.Dot(normalized3, normalized4) > 0f;
					if (num4 && flag)
					{
						if (collider2 is MeshCollider meshCollider3)
						{
							GameObject obj = new GameObject(collider2.gameObject.name);
							obj.transform.SetParent(gameObject.transform);
							obj.transform.localPosition = collider2.transform.localPosition;
							obj.transform.localRotation = collider2.transform.localRotation;
							obj.transform.localScale = collider2.transform.localScale;
							MeshCollider meshCollider4 = obj.AddComponent<MeshCollider>();
							meshCollider4.sharedMesh = meshCollider3.sharedMesh;
							meshCollider4.convex = meshCollider3.convex;
							meshCollider4.isTrigger = meshCollider3.isTrigger;
							PhysicalShockwaveCollisionProxy physicalShockwaveCollisionProxy2 = obj.AddComponent<PhysicalShockwaveCollisionProxy>();
							physicalShockwaveCollisionProxy2.receiver = this;
							physicalShockwaveCollisionProxy2.portalHandle = handle;
							physicalShockwaveCollisionProxy2.previousOrigin = origin;
							num3++;
						}
						else
						{
							Debug.LogError("Collider type not currently supported for portal replication.", base.gameObject);
						}
					}
				}
				if (num3 == 0)
				{
					Object.Destroy(gameObject);
					continue;
				}
				if (replicaPortalHandles == null)
				{
					replicaPortalHandles = new HashSet<PortalHandle>();
				}
				replicaPortalHandles.Add(handle);
				list.Add(gameObject);
				if (depth < MonoSingleton<PortalManagerV2>.Instance.maxRecursions)
				{
					PortalHandleSequence portalSequence = originDepthPair.portalSequence;
					portalSequence = portalSequence.Then(scene, handle);
					queue.Enqueue(new OriginDepthPair(gameObject.transform, depth + 1, portalSequence));
				}
			}
		}
		if (list.Count >= 12)
		{
			Debug.LogWarning($"PhysicalShockwave portal replica count reached the maximum limit of {12}.", base.gameObject);
		}
		portalReplicas = list.ToArray();
		if (portalReplicas.Length == 0)
		{
			return;
		}
		Renderer[] componentsInChildren3 = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			PortalAwareRenderer portalAwareRenderer = componentsInChildren3[i].gameObject.AddComponent<PortalAwareRenderer>();
			HashSet<PortalHandle> hashSet = replicaPortalHandles;
			if (hashSet != null && hashSet.Count > 0)
			{
				HashSet<PortalHandle> handleSet = replicaPortalHandles;
				portalAwareRenderer.SetPortalHandleFilter((PortalAwareRenderer _, PortalHandle item) => handleSet.Contains(item) || handleSet.Contains(item.Reverse()));
				portalAwareRenderer.SetBoundsOverride(boundsOverride);
			}
		}
	}

	private void Update()
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x + Time.deltaTime * speed, base.transform.localScale.y, base.transform.localScale.z + Time.deltaTime * speed);
		if (portalReplicas != null)
		{
			GameObject[] array = portalReplicas;
			foreach (GameObject gameObject in array)
			{
				if (!(gameObject == null))
				{
					gameObject.transform.localScale = base.transform.localScale;
				}
			}
		}
		if (!fading && (base.transform.localScale.x > maxSize || base.transform.localScale.z > maxSize))
		{
			fading = true;
			ScaleNFade[] array2 = faders;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Invoke("GetDestroyed", speed / 10f);
			DestroyReplicas();
		}
	}

	private void DestroyReplicas()
	{
		if (portalReplicas == null)
		{
			return;
		}
		GameObject[] array = portalReplicas;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				Object.Destroy(gameObject);
			}
		}
		portalReplicas = null;
	}

	private void OnDestroy()
	{
		DestroyReplicas();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!fading)
		{
			CheckCollision(collision.collider);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (!fading)
		{
			CheckCollision(col);
		}
	}

	public void HandleReplicaCollision(Collider col, PortalHandle portalHandle, Vector3 previousOriginPosition, Vector3 closestPoint)
	{
		if (SimpleCollisionCheck(col) && PrecisePortalCheck(portalHandle, previousOriginPosition, closestPoint))
		{
			OnTriggerEnter(col);
		}
	}

	private bool SimpleCollisionCheck(Collider col)
	{
		bool num = !hasHurtPlayer && col.gameObject.layer != 15 && col.gameObject.CompareTag("Player");
		bool flag = col.gameObject.layer == 10;
		Landmine component;
		bool flag2 = !enemy && (bool)col.attachedRigidbody && col.attachedRigidbody.TryGetComponent<Landmine>(out component);
		return num || flag || flag2;
	}

	private bool PrecisePortalCheck(PortalHandle portalHandle, Vector3 startPosition, Vector3 closestPoint)
	{
		PortalManagerV2 instance = MonoSingleton<PortalManagerV2>.Instance;
		Vector3 end = instance.Scene.GetTravelMatrix(new PortalHandleSequence(portalHandle.Reverse())).MultiplyPoint3x4(closestPoint);
		if (instance.Scene.FindPortalBetween(startPosition, end, out var hitPortal, out var _, out var _))
		{
			return hitPortal == portalHandle;
		}
		return false;
	}

	private void CheckCollision(Collider col)
	{
		Landmine component4;
		if (!hasHurtPlayer && (col.gameObject.layer != 15 || (ignorePlayerDash && MonoSingleton<NewMovement>.Instance.hurtInvincibility <= 0f)) && col.gameObject.CompareTag("Player"))
		{
			hasHurtPlayer = true;
			if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
			{
				NewMovement? instance = MonoSingleton<NewMovement>.Instance;
				instance.GetHurt(damage, invincible: true);
				instance.LaunchFromPoint(instance.transform.position + Vector3.down, 30f, 30f);
			}
			else if (damage == 0)
			{
				MonoSingleton<PlatformerMovement>.Instance.Jump();
			}
			else
			{
				MonoSingleton<PlatformerMovement>.Instance.Explode();
			}
		}
		else if (col.gameObject.layer == 10)
		{
			EnemyIdentifierIdentifier component = col.gameObject.GetComponent<EnemyIdentifierIdentifier>();
			if (!(component != null) || !(component.eid != null) || (enemy && (component.eid.enemyType == enemyType || component.eid.immuneToFriendlyFire || EnemyIdentifier.CheckHurtException(enemyType, component.eid.enemyType, target))))
			{
				return;
			}
			Collider component2 = component.eid.GetComponent<Collider>();
			float multiplier = (float)damage / 10f;
			if (noDamageToEnemy || base.transform.localScale.x > 10f || base.transform.localScale.z > 10f)
			{
				multiplier = 0f;
			}
			if (component2 != null && !hitColliders.Contains(component2) && !component.eid.dead)
			{
				hitColliders.Add(component2);
				if (enemy)
				{
					component.eid.hitter = "enemy";
				}
				else
				{
					component.eid.hitter = "explosion";
				}
				if (component.eid.enemyType == EnemyType.Turret && component.eid.TryGetComponent<Turret>(out var component3) && component3.lodged)
				{
					component3.Unlodge();
				}
				component.eid.DeliverDamage(col.gameObject, Vector3.up * force * 2f, col.transform.position, multiplier, tryForExplode: false);
			}
			else if (component2 != null && component.eid.dead)
			{
				hitColliders.Add(component2);
				component.eid.hitter = "explosion";
				component.eid.DeliverDamage(col.gameObject, Vector3.up * 2000f, col.transform.position, multiplier, tryForExplode: false);
			}
		}
		else if (!enemy && (bool)col.attachedRigidbody && col.attachedRigidbody.TryGetComponent<Landmine>(out component4))
		{
			component4.Activate(1.5f);
		}
	}

	private void GetDestroyed()
	{
		Object.Destroy(base.gameObject);
	}
}
