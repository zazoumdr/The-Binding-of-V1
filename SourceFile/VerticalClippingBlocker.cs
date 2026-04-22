using UnityEngine;

public class VerticalClippingBlocker : MonoBehaviour
{
	private CapsuleCollider col;

	private LayerMask lmask;

	private Rigidbody rb;

	private NewMovement nm;

	private GroundCheckGroup gc;

	[SerializeField]
	private float ceilingCheckDistance = 3f;

	[SerializeField]
	private float heavyFallMaxExtraHeight = 5f;

	private Vector3 lastVelocity;

	private float computedHeavyFallOffset;

	private bool ceilingDetected;

	private void Awake()
	{
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		nm = GetComponent<NewMovement>();
		gc = nm.gc;
		lmask = LayerMaskDefaults.Get(LMD.Environment);
		lmask = (int)lmask | 0x40000;
	}

	private void FixedUpdate()
	{
		if (nm.enabled)
		{
			lastVelocity = rb.velocity;
			if ((bool)gc && gc.heavyFall)
			{
				computedHeavyFallOffset = CalculateHeavyFallOffset();
			}
			else
			{
				computedHeavyFallOffset = 0f;
			}
		}
	}

	private void LateUpdate()
	{
		if (!(Time.timeScale <= 0f))
		{
			bool flag = (bool)gc && gc.heavyFall;
			if (flag)
			{
				ceilingDetected = PerformCeilingCheck();
			}
			PerformClippingCheck(flag);
		}
	}

	private bool PerformCeilingCheck()
	{
		Vector3 origin = base.transform.TransformPoint(col.center);
		RaycastHit hitInfo;
		return Physics.Raycast(new Ray(origin, -rb.GetGravityDirection()), out hitInfo, ceilingCheckDistance, lmask, QueryTriggerInteraction.Ignore);
	}

	public void PerformClippingCheck(bool heavyFall)
	{
		Vector3 gravityDirection = rb.GetGravityDirection();
		Vector3 vector = -gravityDirection;
		Vector3 vector2 = base.transform.TransformPoint(col.center) + vector * (col.height * 0.5f - col.radius);
		Vector3 b = base.transform.TransformPoint(col.center) - vector * (col.height * 0.5f - col.radius);
		if (heavyFall && !ceilingDetected && computedHeavyFallOffset > 0f)
		{
			vector2 += vector * computedHeavyFallOffset;
		}
		Ray ray = new Ray(vector2, gravityDirection);
		float num = Vector3.Distance(vector2, b);
		if (Physics.Raycast(ray, out var hitInfo, num, lmask, QueryTriggerInteraction.Ignore))
		{
			float num2 = num - hitInfo.distance;
			if (num2 > 0f)
			{
				Vector3 position = rb.position;
				float num3 = Mathf.Abs(Vector3.Dot(hitInfo.normal, vector));
				float num4 = num2 * num3;
				Vector3 vector3 = vector * num4;
				Vector3 position2 = position + vector3;
				rb.position = position2;
			}
		}
	}

	private float CalculateHeavyFallOffset()
	{
		float num = 0f - lastVelocity.y;
		if (num <= 0f)
		{
			return 0f;
		}
		return Mathf.Min(num * Time.fixedDeltaTime, heavyFallMaxExtraHeight);
	}
}
