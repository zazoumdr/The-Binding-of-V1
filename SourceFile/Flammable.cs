using UnityEngine;
using UnityEngine.Events;

public class Flammable : MonoBehaviour
{
	public float heat;

	public float fuel;

	[HideInInspector]
	public GameObject currentFire;

	private AudioSource currentFireAud;

	private Light currentFireLight;

	public bool burning;

	private bool fading;

	public bool secondary;

	public bool fuelOnly;

	private bool enemy;

	private EnemyIdentifierIdentifier eidid;

	private Flammable[] flammables;

	public bool wet;

	public Vector3 overrideSize = Vector3.zero;

	public bool useOverrideSize;

	private Breakable breakable;

	public bool playerOnly;

	public bool enemyOnly;

	public bool specialFlammable;

	public UnityEvent onSpecialActivate;

	private Collider col;

	private bool alwaysSimpleFire;

	private bool markedForDestroy;

	private Vector3 relativeOffset = Vector3.zero;

	private void Start()
	{
		if (base.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
		{
			enemy = true;
			eidid = component;
		}
		if (base.gameObject.TryGetComponent<Breakable>(out var component2))
		{
			breakable = component2;
		}
		alwaysSimpleFire = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleFire");
	}

	private void OnEnable()
	{
		if (burning)
		{
			Pulse();
		}
	}

	private bool IsLossyScaleInvalid()
	{
		if (base.transform == null)
		{
			return true;
		}
		Vector3 lossyScale = base.transform.lossyScale;
		if (!float.IsNaN(lossyScale.x) && !float.IsNaN(lossyScale.y) && !float.IsNaN(lossyScale.z) && !float.IsInfinity(lossyScale.x) && !float.IsInfinity(lossyScale.y) && !float.IsInfinity(lossyScale.z) && lossyScale.x != 0f && lossyScale.y != 0f)
		{
			return lossyScale.z == 0f;
		}
		return true;
	}

	private void Update()
	{
		if (!(currentFire == null))
		{
			currentFire.transform.position = base.transform.position + relativeOffset;
		}
	}

	public void Burn(float newHeat, bool noInstaDamage = false)
	{
		if (markedForDestroy || (fuelOnly && fuel <= 0f))
		{
			return;
		}
		if (IsLossyScaleInvalid())
		{
			MarkForDestroy();
		}
		else if (specialFlammable)
		{
			onSpecialActivate?.Invoke();
		}
		else
		{
			if (wet || (enemy && (bool)eidid && (bool)eidid.eid && eidid.eid.blessed))
			{
				return;
			}
			if (col == null)
			{
				col = GetComponent<Collider>();
			}
			if (col == null)
			{
				MarkForDestroy();
				return;
			}
			if (fuelOnly)
			{
				heat = 0.1f;
			}
			else if (newHeat > heat)
			{
				heat = newHeat;
			}
			if (currentFire == null)
			{
				Bounds bounds = col.bounds;
				currentFire = MonoSingleton<FireObjectPool>.Instance.GetFire(secondary || alwaysSimpleFire);
				currentFire.transform.SetParent(null);
				relativeOffset = base.transform.position - bounds.center;
				Debug.Log(relativeOffset);
				currentFire.transform.position = base.transform.position + relativeOffset;
				_ = base.transform.lossyScale;
				float num = Mathf.Min(((!useOverrideSize || !(overrideSize != Vector3.zero)) ? bounds.size : overrideSize).magnitude, 25f);
				currentFire.transform.localScale = Vector3.one * num;
				currentFireAud = currentFire.GetComponentInChildren<AudioSource>();
				if (!secondary && !alwaysSimpleFire)
				{
					currentFireLight = currentFire.GetComponent<Light>();
					currentFireLight.enabled = true;
				}
			}
			if ((bool)eidid && (bool)eidid.eid && !eidid.eid.burners.Contains(this))
			{
				eidid.eid.burners.Add(this);
			}
			if (enemy)
			{
				burning = true;
				if (eidid.eid.burners.Count == 1)
				{
					eidid.eid.Burn();
				}
			}
			if (breakable != null)
			{
				breakable.Burn();
			}
			if (!secondary)
			{
				flammables = GetComponentsInChildren<Flammable>();
				Flammable[] array = flammables;
				foreach (Flammable flammable in array)
				{
					if (flammable != this)
					{
						flammable.secondary = true;
						flammable.Burn(heat);
						flammable.Pulse();
					}
				}
			}
			burning = true;
		}
	}

	public void Pulse()
	{
		if (markedForDestroy)
		{
			return;
		}
		if (IsLossyScaleInvalid())
		{
			MarkForDestroy();
		}
		else if (burning)
		{
			if (fuel >= 0.175f)
			{
				fuel -= 0.175f;
			}
			else if (fuel > 0f)
			{
				heat = 0.175f - fuel;
				fuel = 0f;
			}
			else
			{
				heat -= 0.25f;
			}
			if (heat <= 0f)
			{
				burning = false;
				fading = true;
				Invoke("Pulse", Random.Range(0.25f, 0.5f));
			}
			else if (!enemy)
			{
				Invoke("Pulse", 0.5f);
				TryIgniteGasoline();
			}
		}
		else
		{
			if (!fading || !(currentFire != null))
			{
				return;
			}
			if (fuel > 0f)
			{
				Burn(0.1f);
				CancelInvoke("Pulse");
				currentFire.transform.localScale = new Vector3(col.bounds.size.x / base.transform.lossyScale.x, col.bounds.size.y / base.transform.lossyScale.y, col.bounds.size.z / base.transform.lossyScale.z);
				return;
			}
			if (currentFire != null)
			{
				currentFire.transform.localScale *= 0.75f;
				if ((Object)(object)currentFireAud == null)
				{
					currentFireAud = currentFire.GetComponentInChildren<AudioSource>();
				}
				AudioSource obj = currentFireAud;
				obj.volume *= 0.75f;
				if (!secondary && currentFireLight != null)
				{
					currentFireLight.range *= 0.75f;
				}
			}
			if (currentFire.transform.localScale.x < 0.1f)
			{
				fading = false;
				ReturnToQueue();
			}
			else
			{
				Invoke("Pulse", Random.Range(0.25f, 0.5f));
			}
		}
	}

	private void TryIgniteGasoline()
	{
		MonoSingleton<StainVoxelManager>.Instance.TryIgniteAt(base.transform.position);
	}

	public void PutOut(bool getWet = true)
	{
		wet = getWet;
		if ((bool)currentFire)
		{
			heat = 0f;
			burning = false;
			fading = false;
			ReturnToQueue();
		}
		if (secondary || flammables == null)
		{
			return;
		}
		Flammable[] array = flammables;
		foreach (Flammable flammable in array)
		{
			if (flammable != this)
			{
				flammable.PutOut();
			}
		}
	}

	public void MarkForDestroy()
	{
		markedForDestroy = true;
		ReturnToQueue();
		EnemyIdentifierIdentifier component;
		if (fuelOnly)
		{
			Object.Destroy(this, Random.Range(0.001f, 0.01f));
		}
		else if (TryGetComponent<EnemyIdentifierIdentifier>(out component))
		{
			component.Invoke("DestroyLimbIfNotTouchedBloodAbsorber", Random.Range(0.001f, 0.01f));
		}
		else
		{
			Object.Destroy(base.gameObject, Random.Range(0.001f, 0.01f));
		}
	}

	public void ReturnToQueue()
	{
		if (currentFire != null)
		{
			MonoSingleton<FireObjectPool>.Instance.ReturnFire(currentFire, secondary || alwaysSimpleFire);
			currentFire = null;
		}
	}

	private void OnDestroy()
	{
		ReturnToQueue();
	}
}
