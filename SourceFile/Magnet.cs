using System.Collections;
using System.Collections.Generic;
using SettingsMenu.Components.Pages;
using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Native;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Magnet : MonoBehaviour
{
	private enum TrackingMode
	{
		Direct,
		Portal,
		Both
	}

	private struct PortalTrackingEntry
	{
		public Rigidbody rb;

		public TrackingMode mode;

		public PortalHandleSequence sequence;
	}

	private static readonly Collider[] PortalOverlapBuffer = new Collider[64];

	private const int PortalOverlapMask = 22528;

	private readonly Dictionary<int, PortalTrackingEntry> portalTracking = new Dictionary<int, PortalTrackingEntry>();

	private readonly HashSet<int> portalSeenThisFrame = new HashSet<int>();

	private readonly List<int> portalExitBuffer = new List<int>();

	private readonly List<KeyValuePair<int, PortalTrackingEntry>> portalDowngradeBuffer = new List<KeyValuePair<int, PortalTrackingEntry>>();

	private List<Rigidbody> affectedRbs = new List<Rigidbody>();

	private List<Rigidbody> removeRbs = new List<Rigidbody>();

	private List<EnemyIdentifier> eids = new List<EnemyIdentifier>();

	private List<Rigidbody> eidRbs = new List<Rigidbody>();

	public List<EnemyIdentifier> ignoredEids = new List<EnemyIdentifier>();

	public EnemyIdentifier onEnemy;

	public List<Magnet> connectedMagnets = new List<Magnet>();

	public List<Rigidbody> sawblades = new List<Rigidbody>();

	public List<Rigidbody> rockets = new List<Rigidbody>();

	public List<Rigidbody> chainsaws = new List<Rigidbody>();

	private SphereCollider col;

	private PortalManagerV2 portalManager;

	public float strength;

	private LayerMask lmask;

	private RaycastHit rhit;

	private bool beenZapped;

	[SerializeField]
	private float maxWeight = 10f;

	private TimeBomb tb;

	[HideInInspector]
	public float health = 3f;

	private float maxWeightFinal => maxWeight;

	private void Start()
	{
		col = GetComponent<SphereCollider>();
		MonoSingleton<PortalManagerV2>.TryGetInstance(out portalManager);
		lmask = (int)lmask | 0x400;
		lmask = (int)lmask | 0x800;
		tb = GetComponentInParent<TimeBomb>();
		col.enabled = false;
		col.enabled = true;
	}

	private void OnEnable()
	{
		MonoSingleton<ObjectTracker>.Instance.AddMagnet(this);
	}

	private void OnDisable()
	{
		if ((bool)MonoSingleton<ObjectTracker>.Instance)
		{
			MonoSingleton<ObjectTracker>.Instance.RemoveMagnet(this);
		}
	}

	private void OnDestroy()
	{
		Launch();
		if (connectedMagnets.Count > 0)
		{
			for (int num = connectedMagnets.Count - 1; num >= 0; num--)
			{
				if (connectedMagnets[num] != null)
				{
					DisconnectMagnets(connectedMagnets[num]);
				}
			}
		}
		if ((bool)tb && tb.gameObject.activeInHierarchy)
		{
			Object.Destroy(tb.gameObject);
		}
	}

	public void Launch()
	{
		if (eids.Count > 0)
		{
			for (int num = eids.Count - 1; num >= 0; num--)
			{
				if ((bool)eids[num])
				{
					ExitEnemy(eids[num]);
				}
			}
		}
		if (affectedRbs.Count == 0 && sawblades.Count == 0)
		{
			portalTracking.Clear();
			return;
		}
		List<Nail> list = new List<Nail>();
		foreach (Rigidbody sawblade in sawblades)
		{
			if (!(sawblade != null))
			{
				continue;
			}
			sawblade.velocity = (base.transform.position - sawblade.transform.position).normalized * sawblade.velocity.magnitude;
			if (sawblade.TryGetComponent<Nail>(out var component))
			{
				component.MagnetRelease(MakeInfo());
				if (component.magnets.Count == 0)
				{
					list.Add(component);
				}
			}
		}
		foreach (Rigidbody affectedRb in affectedRbs)
		{
			if (!(affectedRb != null))
			{
				continue;
			}
			affectedRb.velocity = Vector3.zero;
			if (Physics.SphereCast(new Ray(affectedRb.transform.position, affectedRb.transform.position - base.transform.position), 5f, out rhit, 100f, lmask))
			{
				affectedRb.AddForce((rhit.point - affectedRb.transform.position).normalized * strength * 10f);
			}
			else
			{
				affectedRb.AddForce((base.transform.position - affectedRb.transform.position).normalized * strength * -10f);
			}
			if (affectedRb.TryGetComponent<Nail>(out var component2))
			{
				component2.MagnetRelease(MakeInfo());
				if (component2.magnets.Count == 0)
				{
					affectedRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
					list.Add(component2);
				}
			}
		}
		if (list.Count > 0)
		{
			GameObject obj = new GameObject("NailBurstController");
			NailBurstController nailBurstController = obj.AddComponent<NailBurstController>();
			nailBurstController.nails = new List<Nail>(list);
			nailBurstController.originalNailCount = list.Count;
			obj.AddComponent<RemoveOnTime>().time = 5f;
			foreach (Nail item in list)
			{
				item.nbc = nailBurstController;
			}
		}
		portalTracking.Clear();
	}

	private void FixedUpdate()
	{
		if (!(portalManager == null))
		{
			PortalScene scene = portalManager.Scene;
			if (scene != null)
			{
				portalSeenThisFrame.Clear();
				CheckPortalOverlaps(scene, col.radius);
				ProcessPortalExits();
			}
		}
	}

	private void CheckPortalOverlaps(PortalScene scene, float radius)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!scene.nativeScene.valid)
		{
			return;
		}
		NativeList<NativePortal> portals = scene.nativeScene.portals;
		Vector3 position = base.transform.position;
		foreach (NativePortal item in portals)
		{
			NativePortalTransform nativePortalTransform = item.transform;
			if (!nativePortalTransform.IsPointInFront(position) || Vector3.Dot(float3.op_Implicit(nativePortalTransform.back), position - nativePortalTransform.centerManaged) >= radius)
			{
				continue;
			}
			PortalHandleSequence travelHandles = new PortalHandleSequence(item.handle);
			Vector3 position2 = scene.GetTravelMatrix(in travelHandles).MultiplyPoint3x4(position);
			int num = Physics.OverlapSphereNonAlloc(position2, radius, PortalOverlapBuffer, 22528);
			for (int i = 0; i < num; i++)
			{
				Collider collider = PortalOverlapBuffer[i];
				Vector3 closestPoint = collider.ClosestPoint(position2);
				if (PrecisePortalCheck(scene, item.handle, position, closestPoint))
				{
					HandlePortalScanHit(collider, travelHandles);
				}
			}
		}
	}

	private static bool PrecisePortalCheck(PortalScene scene, PortalHandle portalHandle, Vector3 startPosition, Vector3 closestPoint)
	{
		Vector3 end = scene.GetTravelMatrix(portalHandle.Reverse()).MultiplyPoint3x4(closestPoint);
		if (scene.FindPortalBetween(startPosition, end, out var hitPortal, out var _, out var _))
		{
			return hitPortal == portalHandle;
		}
		return false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 14 && other.gameObject.CompareTag("Metal"))
		{
			ProcessMetalDetection(other, isDirect: true, default(PortalHandleSequence));
			return;
		}
		int layer = other.gameObject.layer;
		Magnet component;
		if (layer == 12 || layer == 11)
		{
			ProcessEnemyDetection(other, isDirect: true, default(PortalHandleSequence));
		}
		else if (other.TryGetComponent<Magnet>(out component) && component != this && !connectedMagnets.Contains(component))
		{
			ConnectMagnets(component);
		}
	}

	private void HandlePortalScanHit(Collider other, PortalHandleSequence sequence)
	{
		if (other.gameObject.layer == 14 && other.gameObject.CompareTag("Metal"))
		{
			ProcessMetalDetection(other, isDirect: false, sequence);
			return;
		}
		int layer = other.gameObject.layer;
		if (layer == 12 || layer == 11)
		{
			ProcessEnemyDetection(other, isDirect: false, sequence);
		}
	}

	private void ProcessMetalDetection(Collider other, bool isDirect, PortalHandleSequence sequence)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (attachedRigidbody == null)
		{
			return;
		}
		int instanceID = attachedRigidbody.GetInstanceID();
		if (!isDirect)
		{
			portalSeenThisFrame.Add(instanceID);
		}
		if (portalTracking.TryGetValue(instanceID, out var value))
		{
			if (isDirect)
			{
				if (value.mode == TrackingMode.Portal)
				{
					value.mode = TrackingMode.Both;
					portalTracking[instanceID] = value;
					NotifyProjectilePathChanged(attachedRigidbody, MakeInfo());
				}
			}
			else if (value.mode == TrackingMode.Direct)
			{
				value.mode = TrackingMode.Both;
				value.sequence = sequence;
				portalTracking[instanceID] = value;
			}
			else if (!value.sequence.Equals(sequence))
			{
				bool num = value.mode == TrackingMode.Portal;
				value.sequence = sequence;
				portalTracking[instanceID] = value;
				if (num)
				{
					NotifyProjectilePathChanged(attachedRigidbody, MakeInfo(sequence));
				}
			}
		}
		else
		{
			portalTracking[instanceID] = new PortalTrackingEntry
			{
				rb = attachedRigidbody,
				mode = ((!isDirect) ? TrackingMode.Portal : TrackingMode.Direct),
				sequence = (isDirect ? default(PortalHandleSequence) : sequence)
			};
			HandleEnter(attachedRigidbody, MakeInfo(isDirect ? default(PortalHandleSequence) : sequence));
		}
	}

	private void ProcessEnemyDetection(Collider other, bool isDirect, PortalHandleSequence sequence)
	{
		EnemyIdentifier component = other.gameObject.GetComponent<EnemyIdentifier>();
		if (component == null || component.bigEnemy || eids.Contains(component) || ignoredEids.Contains(component))
		{
			return;
		}
		Rigidbody component2 = component.GetComponent<Rigidbody>();
		if (component2 == null)
		{
			return;
		}
		int instanceID = component2.GetInstanceID();
		if (!isDirect)
		{
			portalSeenThisFrame.Add(instanceID);
		}
		if (!portalTracking.ContainsKey(instanceID))
		{
			portalTracking[instanceID] = new PortalTrackingEntry
			{
				rb = component2,
				mode = ((!isDirect) ? TrackingMode.Portal : TrackingMode.Direct),
				sequence = (isDirect ? default(PortalHandleSequence) : sequence)
			};
		}
		else
		{
			PortalTrackingEntry value = portalTracking[instanceID];
			if ((isDirect && value.mode == TrackingMode.Portal) || (!isDirect && value.mode == TrackingMode.Direct))
			{
				value.mode = TrackingMode.Both;
				if (!isDirect)
				{
					value.sequence = sequence;
				}
				portalTracking[instanceID] = value;
			}
		}
		component2.mass /= 2f;
		eids.Add(component);
		eidRbs.Add(component2);
	}

	private void HandleEnter(Rigidbody rb, MagnetInfo info)
	{
		if (affectedRbs.Contains(rb))
		{
			return;
		}
		Grenade component2;
		Chainsaw component3;
		if (rb.TryGetComponent<Nail>(out var component))
		{
			component.MagnetCaught(info);
			if (!component.sawblade)
			{
				affectedRbs.Add(rb);
				if (GraphicsSettings.simpleNailPhysics)
				{
					rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
			}
			else if (!sawblades.Contains(rb))
			{
				sawblades.Add(rb);
			}
			if (component.chainsaw && Vector3.Distance(base.transform.position, component.transform.position) > 20f)
			{
				component.transform.position = Vector3.MoveTowards(component.transform.position, base.transform.position, Vector3.Distance(base.transform.position, component.transform.position) - 20f);
			}
		}
		else if (rb.TryGetComponent<Grenade>(out component2))
		{
			if ((!(onEnemy == null) && !onEnemy.dead) || component2.enemy)
			{
				component2.MagnetCaught(info);
				if (!rockets.Contains(rb))
				{
					rockets.Add(rb);
				}
			}
		}
		else if (rb.TryGetComponent<Chainsaw>(out component3))
		{
			if (!chainsaws.Contains(rb))
			{
				chainsaws.Add(rb);
			}
		}
		else
		{
			affectedRbs.Add(rb);
		}
	}

	private void ProcessPortalExits()
	{
		foreach (KeyValuePair<int, PortalTrackingEntry> item in portalTracking)
		{
			PortalTrackingEntry value = item.Value;
			if (value.rb == null || (value.mode == TrackingMode.Portal && !portalSeenThisFrame.Contains(item.Key)))
			{
				portalExitBuffer.Add(item.Key);
			}
			else if (value.mode == TrackingMode.Both && !portalSeenThisFrame.Contains(item.Key))
			{
				value.mode = TrackingMode.Direct;
				value.sequence = default(PortalHandleSequence);
				portalDowngradeBuffer.Add(new KeyValuePair<int, PortalTrackingEntry>(item.Key, value));
			}
		}
		foreach (KeyValuePair<int, PortalTrackingEntry> item2 in portalDowngradeBuffer)
		{
			portalTracking[item2.Key] = item2.Value;
			NotifyProjectilePathChanged(item2.Value.rb, MakeInfo());
		}
		portalDowngradeBuffer.Clear();
		foreach (int item3 in portalExitBuffer)
		{
			if (!portalTracking.Remove(item3, out var value2) || value2.rb == null)
			{
				continue;
			}
			MagnetInfo mag = MakeInfo(value2.sequence);
			Grenade component2;
			if (value2.rb.TryGetComponent<Nail>(out var component))
			{
				component.MagnetRelease(mag);
				if (component.magnets.Count == 0 && !component.sawblade)
				{
					value2.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				}
			}
			else if (value2.rb.TryGetComponent<Grenade>(out component2))
			{
				component2.MagnetRelease(mag);
			}
			affectedRbs.Remove(value2.rb);
			sawblades.Remove(value2.rb);
			rockets.Remove(value2.rb);
			chainsaws.Remove(value2.rb);
		}
		portalExitBuffer.Clear();
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 14 && other.gameObject.CompareTag("Metal"))
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			if (attachedRigidbody == null)
			{
				return;
			}
			int instanceID = attachedRigidbody.GetInstanceID();
			if (!portalTracking.TryGetValue(instanceID, out var value))
			{
				return;
			}
			switch (value.mode)
			{
			case TrackingMode.Direct:
				portalTracking.Remove(instanceID);
				if (affectedRbs.Contains(attachedRigidbody))
				{
					affectedRbs.Remove(attachedRigidbody);
					if (other.TryGetComponent<Nail>(out var component))
					{
						component.MagnetRelease(MakeInfo());
						if (component.magnets.Count == 0)
						{
							attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
						}
					}
				}
				else if (sawblades.Contains(attachedRigidbody))
				{
					if (other.TryGetComponent<Nail>(out var component2))
					{
						component2.MagnetRelease(MakeInfo());
					}
					sawblades.Remove(attachedRigidbody);
				}
				else if (rockets.Contains(attachedRigidbody))
				{
					if (other.TryGetComponent<Grenade>(out var component3))
					{
						component3.MagnetRelease(MakeInfo());
					}
					rockets.Remove(attachedRigidbody);
				}
				else if (chainsaws.Contains(attachedRigidbody))
				{
					chainsaws.Remove(attachedRigidbody);
				}
				break;
			case TrackingMode.Both:
				value.mode = TrackingMode.Portal;
				portalTracking[instanceID] = value;
				NotifyProjectilePathChanged(attachedRigidbody, MakeInfo(value.sequence));
				break;
			}
		}
		else
		{
			if (other.gameObject.layer != 12)
			{
				return;
			}
			EnemyIdentifier component4 = other.gameObject.GetComponent<EnemyIdentifier>();
			if (component4 != null)
			{
				Rigidbody component5 = component4.GetComponent<Rigidbody>();
				if (component5 != null)
				{
					int instanceID2 = component5.GetInstanceID();
					if (portalTracking.TryGetValue(instanceID2, out var value2))
					{
						switch (value2.mode)
						{
						case TrackingMode.Direct:
							portalTracking.Remove(instanceID2);
							break;
						case TrackingMode.Both:
							value2.mode = TrackingMode.Portal;
							portalTracking[instanceID2] = value2;
							break;
						}
					}
				}
			}
			ExitEnemy(component4);
		}
	}

	public void ConnectMagnets(Magnet target)
	{
		if (!target.connectedMagnets.Contains(this))
		{
			target.connectedMagnets.Add(this);
		}
		if (!connectedMagnets.Contains(target))
		{
			connectedMagnets.Add(target);
		}
	}

	public void DisconnectMagnets(Magnet target)
	{
		if (target.connectedMagnets.Contains(this))
		{
			target.connectedMagnets.Remove(this);
		}
		if (connectedMagnets.Contains(target))
		{
			connectedMagnets.Remove(target);
		}
	}

	public void ExitEnemy(EnemyIdentifier eid)
	{
		if (eid != null && eids.Contains(eid))
		{
			int index = eids.IndexOf(eid);
			eids.RemoveAt(index);
			if (eidRbs[index] != null)
			{
				eidRbs[index].mass *= 2f;
			}
			eidRbs.RemoveAt(index);
		}
	}

	private void Update()
	{
		float num = 0f;
		float num2 = strength * Time.deltaTime;
		Vector3 position = base.transform.position;
		PortalScene portalScene = null;
		if (portalTracking.Count > 0 && portalManager != null)
		{
			portalScene = portalManager.Scene;
		}
		foreach (Rigidbody affectedRb in affectedRbs)
		{
			if (affectedRb != null)
			{
				Vector3 virtualPosition = GetVirtualPosition(affectedRb, position, portalScene);
				Vector3 position2 = affectedRb.transform.position;
				if (Mathf.Abs(Vector3.Dot(affectedRb.velocity, virtualPosition - position2)) < 1000f)
				{
					affectedRb.AddForce((virtualPosition - position2) * ((col.radius - Vector3.Distance(position2, virtualPosition)) / col.radius * 50f * num2));
					num += affectedRb.mass;
				}
			}
			else
			{
				removeRbs.Add(affectedRb);
			}
		}
		if (chainsaws.Count > 0)
		{
			for (int num3 = chainsaws.Count - 1; num3 >= 0; num3--)
			{
				if (chainsaws[num3] == null)
				{
					chainsaws.RemoveAt(num3);
				}
				else
				{
					Vector3 virtualPosition2 = GetVirtualPosition(chainsaws[num3], position, portalScene);
					if (Vector3.Distance(virtualPosition2, chainsaws[num3].position) < 15f && Vector3.Dot(chainsaws[num3].position - virtualPosition2, chainsaws[num3].velocity.normalized) < 0f)
					{
						Vector3 position3 = chainsaws[num3].transform.position;
						if (Mathf.Abs(Vector3.Dot(chainsaws[num3].velocity, virtualPosition2 - position3)) < 1000f)
						{
							chainsaws[num3].AddForce((virtualPosition2 - position3) * ((col.radius - Vector3.Distance(position3, virtualPosition2)) / col.radius * 50f * num2));
							num += chainsaws[num3].mass;
						}
					}
				}
			}
		}
		foreach (Rigidbody sawblade in sawblades)
		{
			if (sawblade != null)
			{
				num += sawblade.mass;
			}
			else
			{
				removeRbs.Add(sawblade);
			}
		}
		if (removeRbs.Count > 0)
		{
			foreach (Rigidbody removeRb in removeRbs)
			{
				affectedRbs.Remove(removeRb);
			}
			removeRbs.Clear();
		}
		for (int num4 = eids.Count - 1; num4 >= 0; num4--)
		{
			EnemyIdentifier enemyIdentifier = eids[num4];
			Rigidbody rigidbody = eidRbs[num4];
			if (enemyIdentifier != null && rigidbody != null && !ignoredEids.Contains(enemyIdentifier))
			{
				Vector3 virtualPosition3 = GetVirtualPosition(rigidbody, position, portalScene);
				Vector3 position4 = rigidbody.transform.position;
				if (enemyIdentifier.nailsAmount > 0 && !eidRbs[num4].isKinematic)
				{
					enemyIdentifier.useBrakes = false;
					enemyIdentifier.pulledByMagnet = true;
					rigidbody.AddForce((virtualPosition3 - position4).normalized * ((col.radius - Vector3.Distance(position4, virtualPosition3)) / col.radius * (float)enemyIdentifier.nailsAmount * 5f * num2));
					num += rigidbody.mass;
				}
			}
			else
			{
				eids.RemoveAt(num4);
				eidRbs.RemoveAt(num4);
			}
		}
		float num5 = maxWeightFinal * (float)(connectedMagnets.Count + 1);
		if (num > num5 && !PauseTimedBombs.Paused)
		{
			Object.Destroy(tb.gameObject);
			return;
		}
		tb.beeperColor = Color.Lerp(Color.green, Color.red, num / num5);
		tb.beeperPitch = num / num5 / 2f + 0.25f;
		tb.beeperSizeMultiplier = num / num5 + 1f;
	}

	private MagnetInfo MakeInfo(PortalHandleSequence sequence = default(PortalHandleSequence))
	{
		return new MagnetInfo
		{
			magnet = this,
			sequence = sequence
		};
	}

	private static void NotifyProjectilePathChanged(Rigidbody rb, MagnetInfo info)
	{
		Grenade component2;
		if (rb.TryGetComponent<Nail>(out var component))
		{
			component.UpdateMagnetPath(info);
		}
		else if (rb.TryGetComponent<Grenade>(out component2))
		{
			component2.UpdateMagnetPath(info);
		}
	}

	private Vector3 GetVirtualPosition(Rigidbody rb, Vector3 magnetPos, PortalScene portalScene)
	{
		if (portalScene != null && portalTracking.TryGetValue(rb.GetInstanceID(), out var value) && value.mode == TrackingMode.Portal)
		{
			return portalScene.GetTravelMatrix(in value.sequence).MultiplyPoint3x4(magnetPos);
		}
		return magnetPos;
	}

	public IEnumerator Zap(List<GameObject> alreadyHitObjects, float damage = 1f, GameObject sourceWeapon = null)
	{
		if (!beenZapped)
		{
			beenZapped = true;
			alreadyHitObjects.Add(base.gameObject);
			yield return new WaitForSeconds(0.25f);
			EnemyIdentifier.Zap(base.transform.position, damage / 2f, alreadyHitObjects, sourceWeapon);
			DamageMagnet(1f);
			yield return new WaitForSeconds(1f);
			beenZapped = false;
		}
	}

	public void DamageMagnet(float damage)
	{
		health -= damage;
		if (health <= 0f)
		{
			if ((bool)base.transform.parent && base.transform.parent.TryGetComponent<Harpoon>(out var component))
			{
				Object.Destroy(component.gameObject);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
