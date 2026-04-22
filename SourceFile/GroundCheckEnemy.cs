using System.Collections.Generic;
using UnityEngine;

public class GroundCheckEnemy : MonoBehaviour
{
	public bool onGround;

	public bool fallSuppressed;

	public bool touchingGround;

	public List<Collider> cols = new List<Collider>();

	private List<Collider> toRemove = new List<Collider>();

	public bool dontCheckTags;

	[HideInInspector]
	public int forcedOff;

	public List<Collider> toIgnore = new List<Collider>();

	private Collider col;

	private bool waitForPhysicsTick;

	private void Start()
	{
		col = GetComponent<Collider>();
		if ((bool)col)
		{
			col.enabled = false;
			col.enabled = true;
		}
		CheckCols();
	}

	private void OnEnable()
	{
		cols.Clear();
		toRemove.Clear();
	}

	public static bool TaggedAsStandable(GameObject obj)
	{
		if (!obj.CompareTag("Floor") && !obj.CompareTag("Wall") && !obj.CompareTag("GlassFloor") && !obj.CompareTag("Moving") && !obj.CompareTag("Breakable"))
		{
			return obj.CompareTag("SoftFloor");
		}
		return true;
	}

	private void UpdateGrounds()
	{
		bool flag = true;
		for (int i = 0; i < cols.Count; i++)
		{
			if (!(cols[i] != null) || !cols[i].CompareTag("SoftFloor"))
			{
				flag = false;
				break;
			}
		}
		fallSuppressed = flag;
	}

	private void FixedUpdate()
	{
		waitForPhysicsTick = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Slippery") && (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) || other.gameObject.layer == 16) && ((other.gameObject.layer != 16 && dontCheckTags) || TaggedAsStandable(other.gameObject)) && !toIgnore.Contains(other))
		{
			touchingGround = true;
			cols.Add(other);
			UpdateGrounds();
			if (forcedOff <= 0)
			{
				onGround = touchingGround;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.gameObject.CompareTag("Slippery") && (LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) || other.gameObject.layer == 16) && ((other.gameObject.layer != 16 && dontCheckTags) || TaggedAsStandable(other.gameObject)) && cols.Contains(other))
		{
			cols.Remove(other);
			UpdateGrounds();
		}
	}

	private void CheckCols()
	{
		Invoke("CheckCols", 0.1f);
		if (base.transform.up.y < 0.25f)
		{
			touchingGround = false;
			onGround = false;
			waitForPhysicsTick = true;
		}
		else if (!waitForPhysicsTick)
		{
			CheckColsOnce();
			if (forcedOff > 0)
			{
				onGround = false;
			}
			else
			{
				onGround = touchingGround;
			}
		}
	}

	private void CheckColsOnce()
	{
		bool flag = false;
		for (int num = cols.Count - 1; num >= 0; num--)
		{
			Collider collider = cols[num];
			if (collider == null || toIgnore.Contains(collider) || !collider.enabled)
			{
				cols.RemoveAt(num);
			}
			else
			{
				GameObject gameObject = collider.gameObject;
				if (!gameObject.activeInHierarchy)
				{
					cols.RemoveAt(num);
				}
				else if (!gameObject.CompareTag("Slippery") && (LayerMaskDefaults.IsMatchingLayer(gameObject.layer, LMD.Environment) || gameObject.layer == 16) && ((gameObject.layer != 16 && dontCheckTags) || TaggedAsStandable(gameObject)))
				{
					flag = true;
				}
			}
		}
		touchingGround = flag;
	}

	public void ForceOff()
	{
		forcedOff++;
		onGround = false;
	}

	public void StopForceOff()
	{
		forcedOff--;
		if (forcedOff <= 0)
		{
			onGround = touchingGround;
		}
	}

	public Vector3 ClosestPoint()
	{
		CheckColsOnce();
		if (cols.Count > 0)
		{
			Vector3 result = base.transform.position;
			float num = 999f;
			{
				foreach (Collider col in cols)
				{
					Vector3 vector = col.ClosestPoint(base.transform.position);
					float num2 = Vector3.SqrMagnitude(vector - base.transform.position);
					if (num2 < num && !toIgnore.Contains(col))
					{
						result = vector;
						num = num2;
					}
				}
				return result;
			}
		}
		return base.transform.position;
	}

	public float DistanceToGround()
	{
		if (Physics.Raycast(col.bounds.center, Vector3.down, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
		{
			return hitInfo.distance - col.bounds.extents.y;
		}
		return float.PositiveInfinity;
	}
}
