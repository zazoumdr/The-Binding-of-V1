using UnityEngine;

public class EnemyIdentifierIdentifier : MonoBehaviour
{
	[HideInInspector]
	public EnemyIdentifier eid;

	private bool deactivated;

	private Vector3 startPos;

	public int bloodAbsorberCount;

	private void Awake()
	{
		if (!eid)
		{
			eid = GetComponentInParent<EnemyIdentifier>();
		}
	}

	private void Start()
	{
		startPos = base.transform.position;
		SlowCheck();
	}

	public void Detach(Transform zone)
	{
		if (TryGetComponent<CharacterJoint>(out var component))
		{
			Object.Destroy(component);
		}
		base.transform.SetParent(zone, worldPositionStays: true);
	}

	public void GoLimp()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.isKinematic = false;
			component.SetGravityMode(useGravity: true);
		}
	}

	public void DetachChildren(Transform parent, bool recursive, bool setParent = true)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
		{
			Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
		}
		CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint characterJoint in componentsInChildren)
		{
			if (recursive || characterJoint.connectedBody == component)
			{
				characterJoint.transform.parent = parent;
				Object.Destroy(characterJoint);
				Rigidbody[] componentsInChildren2 = characterJoint.GetComponentsInChildren<Rigidbody>();
				foreach (Rigidbody obj in componentsInChildren2)
				{
					obj.isKinematic = false;
					obj.SetGravityMode(useGravity: true);
				}
			}
		}
	}

	public void SetupForHellBath()
	{
		if (TryGetComponent<Rigidbody>(out var component))
		{
			component.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
		Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
	}

	public void Break()
	{
		Break(true, false);
	}

	public void Break(bool reparentToChild = true, bool destroy = false)
	{
		if (base.transform.childCount == 0)
		{
			Debug.LogWarning("Tried to break an enemy limb with no children", this);
			return;
		}
		if (reparentToChild)
		{
			Transform child = base.transform.GetChild(0);
			base.transform.SetParent(child);
			base.transform.localPosition = Vector3.zero;
		}
		base.transform.localScale = Vector3.zero;
		CharacterJoint component = GetComponent<CharacterJoint>();
		if (component != null)
		{
			component.connectedBody = null;
			Object.Destroy(component);
		}
		Rigidbody component2 = GetComponent<Rigidbody>();
		if (component2 != null)
		{
			Object.Destroy(component2);
		}
		Collider component3 = GetComponent<Collider>();
		if (component3 != null)
		{
			Object.Destroy(component3);
		}
		if (destroy)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void DestroyLimbIfNotTouchedBloodAbsorber()
	{
		if (eid == null || !eid.dead)
		{
			return;
		}
		int num = bloodAbsorberCount;
		if (eid == GetComponentInParent<EnemyIdentifier>())
		{
			num = 0;
			EnemyIdentifierIdentifier[] componentsInChildren = eid.GetComponentsInChildren<EnemyIdentifierIdentifier>();
			foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in componentsInChildren)
			{
				num += enemyIdentifierIdentifier.bloodAbsorberCount;
			}
		}
		if (num <= 0 && TryGetComponent<Collider>(out var component))
		{
			GibDestroyer.LimbBegone(component);
		}
		else if (StockMapInfo.Instance.removeGibsWithoutAbsorbers)
		{
			Invoke("DestroyLimbIfNotTouchedBloodAbsorber", StockMapInfo.Instance.gibRemoveTime);
		}
	}

	private void SlowCheck()
	{
		if (eid == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (base.gameObject.activeInHierarchy)
		{
			Vector3 position = base.transform.position;
			if (position.y > 0f)
			{
				position.y = startPos.y;
			}
			if (eid == null || Vector3.Distance(position, startPos) > 9999f || (Vector3.Distance(position, startPos) > 999f && eid.dead))
			{
				deactivated = true;
				MonoSingleton<FireObjectPool>.Instance.RemoveAllFiresFromObject(base.gameObject);
				base.gameObject.SetActive(value: false);
				base.transform.position = new Vector3(-100f, -100f, -100f);
				base.transform.localScale = Vector3.zero;
				if (eid != null && !eid.dead)
				{
					eid.InstaKill();
				}
			}
		}
		if (!deactivated)
		{
			Invoke("SlowCheck", 3f);
		}
	}
}
