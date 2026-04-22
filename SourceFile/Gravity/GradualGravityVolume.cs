using UnityEngine;

namespace Gravity;

public class GradualGravityVolume : GravityVolume
{
	public Vector3 customGravityDown = Vector3.down;

	public Vector3 proprotionalAxis = Vector3.zero;

	private void Update()
	{
		if (playerRequests > 0)
		{
			MonoSingleton<NewMovement>.Instance.SwitchGravity(CalculateGravityVector());
		}
	}

	protected override Vector3 CalculateGravityVector()
	{
		Vector3 normalized = Physics.gravity.normalized;
		Vector3 normalized2 = customGravityDown.normalized;
		Vector3 position = MonoSingleton<NewMovement>.Instance.transform.position;
		Vector3 position2 = base.transform.position;
		float num = 0f;
		if (proprotionalAxis.x != 0f)
		{
			num += Mathf.Abs(position.x - position2.x) / proprotionalAxis.x;
		}
		if (proprotionalAxis.y != 0f)
		{
			num += Mathf.Abs(position.y - position2.y) / proprotionalAxis.y;
		}
		if (proprotionalAxis.z != 0f)
		{
			num += Mathf.Abs(position.z - position2.z) / proprotionalAxis.z;
		}
		return Vector3.Lerp(normalized2, normalized, num) * normalized.magnitude;
	}

	protected override void OnDrawGizmos()
	{
	}
}
