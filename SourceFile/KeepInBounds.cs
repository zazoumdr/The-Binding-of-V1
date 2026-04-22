using System;
using UnityEngine;

public class KeepInBounds : MonoBehaviour
{
	[Serializable]
	private enum UpdateMode
	{
		None,
		Update,
		FixedUpdate,
		LateUpdate
	}

	[SerializeField]
	private bool useColliderCenter;

	[SerializeField]
	private float maxConsideredDistance;

	[SerializeField]
	private UpdateMode updateMode = UpdateMode.Update;

	private Vector3 previousTracedPosition;

	private Vector3 previousRealPosition;

	private Collider col;

	private Rigidbody rb;

	[SerializeField]
	private bool includePlayerOnly;

	private LayerMask lmask;

	private Vector3 currentPosition;

	private void Awake()
	{
		if (useColliderCenter)
		{
			col = GetComponent<Collider>();
			if (col == null)
			{
				Debug.LogWarning("Unfortunately, the Collider component is missing while useColliderCenter is true. Switching to fallback transform.position tracking", base.gameObject);
				useColliderCenter = false;
			}
		}
		rb = GetComponent<Rigidbody>();
		currentPosition = GetCurrentPosition();
		previousTracedPosition = currentPosition;
		previousRealPosition = ((rb != null) ? rb.position : base.transform.position);
		lmask = LayerMaskDefaults.Get(LMD.Environment);
		if (includePlayerOnly)
		{
			lmask = (int)lmask | 0x40000;
		}
	}

	private void Update()
	{
		if (updateMode == UpdateMode.Update)
		{
			ValidateMove();
		}
	}

	private void FixedUpdate()
	{
		if (updateMode == UpdateMode.FixedUpdate)
		{
			ValidateMove();
		}
	}

	private void LateUpdate()
	{
		if (updateMode == UpdateMode.LateUpdate)
		{
			ValidateMove();
		}
	}

	public void ForceApproveNewPosition()
	{
		previousTracedPosition = GetCurrentPosition();
		previousRealPosition = ((rb != null) ? rb.position : base.transform.position);
	}

	public void ValidateMove()
	{
		bool flag = !Time.inFixedTimeStep && rb != null;
		currentPosition = GetCurrentPosition();
		Vector3 vector = (flag ? rb.position : base.transform.position);
		if (maxConsideredDistance != 0f && Vector3.Distance(previousTracedPosition, currentPosition) > maxConsideredDistance)
		{
			previousTracedPosition = currentPosition;
			previousRealPosition = vector;
		}
		else if (CastCheck())
		{
			ApplyCorrectedPosition(flag, previousRealPosition);
		}
		else
		{
			previousTracedPosition = currentPosition;
			previousRealPosition = vector;
		}
	}

	private Vector3 GetCurrentPosition()
	{
		if (!useColliderCenter)
		{
			return base.transform.position;
		}
		return col.bounds.center;
	}

	private bool CastCheck()
	{
		RaycastHit hitInfo;
		return Physics.Linecast(previousTracedPosition, currentPosition, out hitInfo, lmask, QueryTriggerInteraction.Ignore);
	}

	private void ApplyCorrectedPosition(bool useRB, Vector3 position)
	{
		if (useRB)
		{
			rb.position = position;
		}
		else
		{
			base.transform.position = position;
		}
	}
}
