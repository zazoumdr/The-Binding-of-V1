using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
	public bool onWall;

	public Vector3 poc;

	public List<Collider> cols = new List<Collider>();

	private List<Collider> colsToDelete = new List<Collider>();

	public Matrix4x4? portalTravelMatrix;

	public Collider currentCollider;

	public Vector3 GetPointOfCollision()
	{
		Vector3 result = poc;
		if (portalTravelMatrix.HasValue)
		{
			result = portalTravelMatrix.Value.MultiplyPoint3x4(poc);
		}
		return result;
	}

	private void Update()
	{
		if (onWall)
		{
			UpdateOnWall();
		}
	}

	public void UpdateOnWall()
	{
		onWall = CheckForCols();
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) || other.gameObject.layer == 11) && !other.isTrigger && !other.gameObject.CompareTag("Slippery"))
		{
			onWall = true;
			cols.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((LayerMaskDefaults.IsMatchingLayer(other.gameObject.layer, LMD.Environment) || other.gameObject.layer == 17 || other.gameObject.layer == 11) && cols.Contains(other))
		{
			cols.Remove(other);
		}
	}

	public bool CheckForCols()
	{
		bool result = false;
		poc = Vector3.zero;
		float num = 100f;
		CustomGroundProperties component2;
		if (cols.Count > 1)
		{
			foreach (Collider col in cols)
			{
				if ((bool)col && col.enabled && col.gameObject.activeInHierarchy && LayerMaskDefaults.IsMatchingLayer(col.gameObject.layer, LMD.Environment) && !col.isTrigger && !col.gameObject.CompareTag("Slippery") && (!col.TryGetComponent<CustomGroundProperties>(out var component) || component.canWallJump))
				{
					Vector3 a = ColliderUtility.FindClosestPoint(col, base.transform.position, ignoreVerticalTriangles: true);
					if (Vector3.Distance(a, base.transform.position) < num && Vector3.Distance(a, base.transform.position) < 5f)
					{
						num = Vector3.Distance(a, base.transform.position);
						poc = a;
						currentCollider = col;
						result = true;
					}
					else if (Vector3.Distance(a, base.transform.position) >= 5f)
					{
						colsToDelete.Add(col);
					}
				}
				else
				{
					colsToDelete.Add(col);
				}
			}
		}
		else if (cols.Count == 1 && cols[0] != null && cols[0].enabled && cols[0].gameObject.activeInHierarchy && (!cols[0].TryGetComponent<CustomGroundProperties>(out component2) || component2.canWallJump))
		{
			Vector3 a2 = ColliderUtility.FindClosestPoint(cols[0], base.transform.position, ignoreVerticalTriangles: false);
			if (Vector3.Distance(a2, base.transform.position) < 5f)
			{
				poc = a2;
			}
			currentCollider = cols[0];
			result = true;
		}
		else if (cols.Count == 1 && (cols[0] == null || Vector3.Distance(ColliderUtility.FindClosestPoint(cols[0], base.transform.position, ignoreVerticalTriangles: true), base.transform.position) < 5f))
		{
			colsToDelete.Add(cols[0]);
		}
		if (colsToDelete.Count > 0)
		{
			foreach (Collider item in colsToDelete)
			{
				if (cols.Contains(item))
				{
					cols.Remove(item);
				}
			}
		}
		colsToDelete.Clear();
		return result;
	}

	public bool CheckForEnemyCols()
	{
		if ((bool)MonoSingleton<NewMovement>.Instance.ridingRocket)
		{
			return false;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2.5f, 4096, QueryTriggerInteraction.Collide);
		if (array.Length != 0)
		{
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider != null && collider.enabled && collider.gameObject.activeInHierarchy && Vector3.Distance(base.transform.position, collider.transform.position) < 40f)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private void OnDisable()
	{
		onWall = false;
		portalTravelMatrix = null;
		cols.Clear();
		colsToDelete.Clear();
	}
}
