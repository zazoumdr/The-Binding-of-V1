using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Water : MonoBehaviour
{
	public struct WaterColData
	{
		public float maxHeight;

		public float minHeight;

		public Vector3 position;

		public Quaternion rotation;
	}

	public enum WaterGOType
	{
		none,
		small,
		big,
		continuous,
		bubble,
		wetparticle
	}

	private Dictionary<Collider, WaterObject> tracked = new Dictionary<Collider, WaterObject>();

	[Header("Visual/FX")]
	public Color clr = new Color(0f, 0.5f, 1f);

	public bool notWet;

	public bool simplifyWaterProcessing;

	public bool visualsOnly;

	[Header("References (Optional)")]
	public FishDB fishDB;

	public Transform overrideFishingPoint;

	public FishObject[] attractFish;

	private Collider[] waterColliders;

	private bool isPlayerUnderWater;

	[HideInInspector]
	public bool isPlayerTouchingWater;

	private DryZoneController dzc;

	private HashSet<Collider> toRemove = new HashSet<Collider>();

	private List<WaterColData> waterColData = new List<WaterColData>();

	private int waterCount;

	private Vector3 gravity;

	private float scaledGravityY;

	private Quaternion lookUp;

	private bool doneThisFrame;

	private PooledWaterStore waterStore;

	private void Start()
	{
		waterStore = MonoSingleton<PooledWaterStore>.Instance;
		lookUp = Quaternion.LookRotation(Vector3.up);
		waterColliders = GetComponentsInChildren<Collider>();
		if ((bool)fishDB)
		{
			fishDB.SetupWater(this);
		}
	}

	private void OnEnable()
	{
		dzc = MonoSingleton<DryZoneController>.Instance;
		dzc.waters.Add(this);
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (!base.gameObject.scene.isLoaded)
		{
			return;
		}
		toRemove.Clear();
		toRemove.UnionWith(tracked.Keys);
		foreach (Collider item in toRemove)
		{
			RemoveFromWater(item, wasOnTriggerExit: false);
		}
		tracked.Clear();
		dzc = MonoSingleton<DryZoneController>.Instance;
		dzc.waters.Remove(this);
	}

	private void FixedUpdate()
	{
		if (doneThisFrame)
		{
			foreach (WaterObject value in tracked.Values)
			{
				if (value.IsPlayer && !IsCollidingWithWater(value.Col))
				{
					toRemove.Add(value.Col);
				}
				ApplyWaterForces(value, doBackupCheck: true);
			}
			return;
		}
		gravity = Physics.gravity;
		scaledGravityY = gravity.y * 0.2f;
		waterColData.Clear();
		waterCount = waterColliders.Length;
		for (int i = 0; i < waterCount; i++)
		{
			Collider collider = waterColliders[i];
			if ((bool)collider)
			{
				Bounds bounds = collider.bounds;
				WaterColData item = new WaterColData
				{
					maxHeight = bounds.max.y,
					minHeight = bounds.min.y,
					position = collider.transform.position,
					rotation = collider.transform.rotation
				};
				waterColData.Add(item);
			}
		}
		foreach (WaterObject value2 in tracked.Values)
		{
			if (MarkIfStaleObject(value2))
			{
				continue;
			}
			if ((value2.IsPlayer || value2.IsEnemy) && TryGetSurfaceAndIsSubmerged(value2.Col, out var surfacePoint, out var isSubmerged))
			{
				if (value2.IsEnemy)
				{
					if (isSubmerged != value2.EID.underwater)
					{
						value2.EID.underwater = isSubmerged;
						value2.BubbleEffect.SetActive(isSubmerged);
						for (int j = 0; j < value2.LowPassFilters.Count; j++)
						{
							((Behaviour)(object)value2.LowPassFilters[j]).enabled = isSubmerged;
						}
					}
					if (isSubmerged)
					{
						KillStreetCleaner(value2);
					}
				}
				UpdateContinuousSplash(value2, surfacePoint, isSubmerged);
			}
			if (value2.IsUWC && !IsCollidingWithWater(value2.Col))
			{
				toRemove.Add(value2.Col);
			}
			if (value2.IsEnemy && value2.EID.dead)
			{
				waterStore.ReturnToQueue(value2.BubbleEffect, WaterGOType.bubble);
			}
			ApplyWaterForces(value2, doBackupCheck: false);
		}
		foreach (Collider item2 in toRemove)
		{
			RemoveFromWater(item2, wasOnTriggerExit: false);
		}
		toRemove.Clear();
		doneThisFrame = true;
	}

	private bool MarkIfStaleObject(WaterObject wObj)
	{
		if (!wObj.Col || !wObj.Col.enabled || !wObj.Rb || !wObj.rbGO.activeInHierarchy)
		{
			toRemove.Add(wObj.Col);
			return true;
		}
		return false;
	}

	private void Update()
	{
		doneThisFrame = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody attachedRigidbody = other.attachedRigidbody;
		if (!other || !attachedRigidbody || ((bool)dzc && dzc.colliderCalls.ContainsKey(other)) || tracked.ContainsKey(other))
		{
			return;
		}
		bool isUWC = false;
		if (other.isTrigger)
		{
			if (!attachedRigidbody.CompareTag("Player") || !other.TryGetComponent<UnderwaterController>(out var _))
			{
				return;
			}
			isUWC = true;
		}
		GameObject rbGO = attachedRigidbody.gameObject;
		int layer = other.gameObject.layer;
		WaterObject waterObject = new WaterObject
		{
			Rb = attachedRigidbody,
			rbGO = rbGO,
			Col = other,
			EnterVelocity = attachedRigidbody.velocity,
			IsUWC = isUWC,
			IsPlayer = other.CompareTag("Player"),
			IsEnemy = (layer == 12),
			Layer = layer,
			EID = attachedRigidbody.GetComponent<EnemyIdentifier>()
		};
		tracked.Add(other, waterObject);
		if (waterObject.IsPlayer)
		{
			isPlayerTouchingWater = true;
			MonoSingleton<NewMovement>.Instance.touchingWaters.Add(this);
		}
		else if (waterObject.IsUWC)
		{
			isPlayerUnderWater = true;
			UnderwaterController instance = MonoSingleton<UnderwaterController>.Instance;
			if ((bool)instance)
			{
				instance.EnterWater(this);
			}
			SpawnBubbles(waterObject);
		}
		else
		{
			if (waterObject.IsEnemy && (bool)waterObject.EID)
			{
				waterObject.EID.touchingWaters.Add(this);
				if (!simplifyWaterProcessing && !notWet)
				{
					SpawnBubbles(waterObject);
				}
			}
			if (!notWet)
			{
				AddLowPassFilters(waterObject);
				ExtinguishFires(other);
				MarkObjectWet(waterObject);
			}
		}
		TrySpawnSplash(waterObject);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!dzc || !dzc.colliderCalls.ContainsKey(other))
		{
			RemoveFromWater(other, wasOnTriggerExit: true);
		}
	}

	public void EnterDryZone(Collider other)
	{
		RemoveFromWater(other, wasOnTriggerExit: false);
	}

	public void ExitDryZone(Collider other)
	{
		if (base.isActiveAndEnabled && IsCollidingWithWater(other))
		{
			OnTriggerEnter(other);
		}
	}

	public bool IsCollidingWithWater(Collider other)
	{
		Transform obj = other.transform;
		Vector3 position = obj.position;
		Quaternion rotation = obj.rotation;
		for (int i = 0; i < waterCount; i++)
		{
			Collider colliderB = waterColliders[i];
			if (Physics.ComputePenetration(other, position, rotation, colliderB, waterColData[i].position, waterColData[i].rotation, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveFromWater(Collider col, bool wasOnTriggerExit)
	{
		if ((wasOnTriggerExit && waterCount > 1 && IsCollidingWithWater(col)) || !tracked.TryGetValue(col, out var value))
		{
			return;
		}
		if (wasOnTriggerExit && (bool)col && (bool)value.Rb)
		{
			TrySpawnSplash(value);
		}
		CleanupWaterEffects(value);
		if (value.IsPlayer)
		{
			if (isPlayerTouchingWater)
			{
				isPlayerTouchingWater = false;
				MonoSingleton<NewMovement>.Instance.touchingWaters.Remove(this);
			}
		}
		else if (value.IsUWC)
		{
			isPlayerUnderWater = false;
			UnderwaterController instance = MonoSingleton<UnderwaterController>.Instance;
			if ((bool)instance)
			{
				instance.OutWater(this);
			}
		}
		else if (value.IsEnemy && (bool)value.EID)
		{
			value.EID.underwater = false;
			value.EID.touchingWaters.Remove(this);
		}
		tracked.Remove(col);
	}

	private void ApplyWaterForces(WaterObject wObj, bool doBackupCheck)
	{
		if (wObj.IsUWC || visualsOnly)
		{
			return;
		}
		Rigidbody rb = wObj.Rb;
		if (doBackupCheck && (!rb || !wObj.rbGO.activeInHierarchy))
		{
			toRemove.Add(wObj.Col);
		}
		else if (rb.GetGravityMode() && !rb.isKinematic)
		{
			Vector3 velocity = rb.velocity;
			if (velocity.y < scaledGravityY)
			{
				rb.velocity = Vector3.MoveTowards(target: new Vector3(velocity.x, scaledGravityY, velocity.z), current: velocity, maxDistanceDelta: Time.fixedDeltaTime * 10f * Mathf.Abs(velocity.y - scaledGravityY + 0.5f));
			}
			else if (wObj.Layer == 10 || wObj.Layer == 9)
			{
				rb.AddForce(gravity * (rb.mass * -0.45f));
			}
			else
			{
				rb.AddForce(gravity * (rb.mass * -0.75f));
			}
		}
	}

	private void UpdateContinuousSplash(WaterObject wObj, Vector3 surfacePoint, bool isSubmerged)
	{
		if ((bool)wObj.ContinuousSplashEffect)
		{
			wObj.ContinuousSplashEffect.position = surfacePoint;
			if (isSubmerged)
			{
				wObj.ContinuousSplashEffect.GetComponent<SplashContinuous>().ReturnSoon();
				wObj.ContinuousSplashEffect = null;
			}
		}
		else if (!isSubmerged)
		{
			GameObject fromQueue = waterStore.GetFromQueue(WaterGOType.continuous);
			fromQueue.transform.SetPositionAndRotation(surfacePoint, lookUp);
			fromQueue.transform.localScale = 3f * wObj.Col.bounds.size.magnitude * Vector3.one;
			SplashContinuous component = fromQueue.GetComponent<SplashContinuous>();
			if (wObj.IsEnemy && wObj.Col.TryGetComponent<NavMeshAgent>(out var component2))
			{
				component.nma = component2;
			}
			wObj.ContinuousSplashEffect = fromQueue.transform;
			wObj.ContinuousSplashEffect.position = surfacePoint;
		}
	}

	private bool TryGetSurfaceAndIsSubmerged(Collider col, out Vector3 surfacePoint, out bool isSubmerged)
	{
		surfacePoint = Vector3.zero;
		isSubmerged = false;
		Vector3 position = col.transform.position;
		for (int i = 0; i < waterCount; i++)
		{
			Collider collider = waterColliders[i];
			if (collider == null || !(Vector3.Distance(collider.ClosestPoint(position), position) < 1f))
			{
				continue;
			}
			WaterColData waterColData = this.waterColData[i];
			if (Physics.Raycast(new Vector3(position.x, waterColData.maxHeight + 0.1f, position.z), maxDistance: waterColData.maxHeight - waterColData.minHeight, direction: Vector3.down, hitInfo: out var hitInfo, layerMask: 16, queryTriggerInteraction: QueryTriggerInteraction.Collide))
			{
				surfacePoint = hitInfo.point;
				Bounds bounds = col.bounds;
				float y = bounds.center.y;
				float y2 = bounds.extents.y;
				float num = y + y2;
				float num2 = y - y2;
				if (num - (num - num2) / 3f < surfacePoint.y)
				{
					isSubmerged = true;
				}
				return true;
			}
		}
		return false;
	}

	private void TrySpawnSplash(WaterObject wObj)
	{
		Rigidbody rb = wObj.Rb;
		int layer = wObj.Layer;
		WaterGOType waterGOType = WaterGOType.none;
		if ((rb.velocity.y < -25f || layer == 11) && rb.mass >= 1f && layer != 10 && layer != 9)
		{
			waterGOType = WaterGOType.big;
		}
		else if (!rb.isKinematic)
		{
			waterGOType = WaterGOType.small;
		}
		if (waterGOType == WaterGOType.none)
		{
			return;
		}
		Vector3 position = rb.transform.position;
		Vector3 vector = Vector3.positiveInfinity;
		float num = float.PositiveInfinity;
		for (int i = 0; i < waterCount; i++)
		{
			Collider obj = waterColliders[i];
			Vector3 position2 = new Vector3(position.x, waterColData[i].maxHeight, position.z);
			Vector3 vector2 = obj.ClosestPointOnBounds(position2);
			float num2 = Vector3.Distance(vector2, position);
			if (num2 < num)
			{
				vector = vector2;
				num = num2;
			}
		}
		if (Vector3.Distance(vector, wObj.Col.ClosestPoint(vector)) < 1f)
		{
			GameObject fromQueue = waterStore.GetFromQueue(waterGOType);
			if (wObj.IsPlayer && fromQueue.TryGetComponent<PooledSplash>(out var component) && wObj.IsPlayer)
			{
				component.defaultPitch = 0.45f;
			}
			Transform obj2 = fromQueue.transform;
			obj2.SetPositionAndRotation(vector, lookUp);
			obj2.localScale = 3f * wObj.Col.bounds.size.magnitude * Vector3.one;
		}
	}

	public GameObject SpawnBasicSplash(WaterGOType type)
	{
		return waterStore.GetFromQueue(type);
	}

	private void AddLowPassFilters(WaterObject wObj)
	{
		if (wObj.Layer == 11 || wObj.Layer == 12)
		{
			return;
		}
		GameObject rbGO = wObj.rbGO;
		if (wObj.Layer == 0 && rbGO.TryGetComponent<BloodUnderwaterChecker>(out var _))
		{
			wObj.AudioSources = rbGO.transform.parent.GetComponentsInChildren<AudioSource>();
		}
		else
		{
			wObj.AudioSources = rbGO.GetComponentsInChildren<AudioSource>();
		}
		AudioSource[] audioSources = wObj.AudioSources;
		foreach (AudioSource val in audioSources)
		{
			if ((bool)(Object)(object)val)
			{
				if (!((Component)(object)val).TryGetComponent(out AudioLowPassFilter component2))
				{
					component2 = ((Component)(object)val).gameObject.AddComponent<AudioLowPassFilter>();
				}
				component2.cutoffFrequency = 1000f;
				component2.lowpassResonanceQ = 1f;
				if (wObj.IsEnemy)
				{
					((Behaviour)(object)component2).enabled = false;
				}
				wObj.LowPassFilters.Add(component2);
			}
		}
	}

	private void RemoveLowPassFilters(WaterObject wObj)
	{
		for (int i = 0; i < wObj.LowPassFilters.Count; i++)
		{
			AudioLowPassFilter val = wObj.LowPassFilters[i];
			if ((bool)(Object)(object)val)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		wObj.LowPassFilters.Clear();
	}

	private void SpawnBubbles(WaterObject wObj)
	{
		GameObject fromQueue = waterStore.GetFromQueue(WaterGOType.bubble);
		if (wObj.IsUWC)
		{
			fromQueue.transform.SetPositionAndRotation(wObj.Rb.transform.position, lookUp);
		}
		else
		{
			fromQueue.transform.SetPositionAndRotation(wObj.Col.bounds.center, lookUp);
			fromQueue.SetActive(value: false);
		}
		fromQueue.transform.SetParent(wObj.Rb.transform, worldPositionStays: true);
		wObj.BubbleEffect = fromQueue;
	}

	private void ExtinguishFires(Collider col)
	{
		Flammable[] componentsInChildren = col.GetComponentsInChildren<Flammable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].PutOut();
		}
	}

	private void MarkObjectWet(WaterObject wObj)
	{
		if (wObj.Layer != 9 && wObj.Layer != 10)
		{
			if (wObj.Col.TryGetComponent<Wet>(out var component))
			{
				component.Refill();
			}
			else
			{
				wObj.Col.gameObject.AddComponent<Wet>();
			}
		}
	}

	private void KillStreetCleaner(WaterObject wObj)
	{
		EnemyIdentifier eID = wObj.EID;
		if (eID.enemyType == EnemyType.Streetcleaner && !eID.dead)
		{
			eID.InstaKill();
		}
	}

	private void CleanupWaterEffects(WaterObject wObj)
	{
		if ((bool)wObj.BubbleEffect)
		{
			waterStore.ReturnToQueue(wObj.BubbleEffect, WaterGOType.bubble);
		}
		if ((bool)wObj.ContinuousSplashEffect)
		{
			if (wObj.ContinuousSplashEffect.TryGetComponent<SplashContinuous>(out var component))
			{
				component.ReturnSoon();
			}
			wObj.ContinuousSplashEffect = null;
		}
		RemoveLowPassFilters(wObj);
		if (!notWet && !wObj.IsPlayer && !wObj.IsUWC && (bool)wObj.Rb && wObj.Layer != 10 && wObj.Layer != 9)
		{
			wObj.rbGO.GetOrAddComponent<Wet>().Dry(wObj.Rb.centerOfMass);
		}
	}

	public void UpdateColor(Color newColor)
	{
		clr = newColor;
		if (isPlayerUnderWater)
		{
			UnderwaterController instance = MonoSingleton<UnderwaterController>.Instance;
			if ((bool)instance)
			{
				instance.UpdateColor(newColor);
			}
		}
	}
}
