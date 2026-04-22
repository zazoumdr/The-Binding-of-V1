using System.Collections;
using UnityEngine;

public class LegController : MonoBehaviour
{
	[SerializeField]
	private Transform bodyTransform;

	[SerializeField]
	private Leg[] legs;

	private float maxTipWait = 0.7f;

	private bool readySwitchOrder;

	private bool stepOrder = true;

	private float bodyHeightBase = 1.3f;

	private Vector3 bodyPos;

	private Vector3 bodyUp;

	private Vector3 bodyForward;

	private Vector3 bodyRight;

	private Quaternion bodyRotation;

	private float PosAdjustRatio = 0.1f;

	private float RotAdjustRatio = 0.2f;

	private void Start()
	{
		StartCoroutine(AdjustBodyTransform());
	}

	private void Update()
	{
		if (legs.Length < 2)
		{
			return;
		}
		for (int i = 0; i < legs.Length; i++)
		{
			if (legs[i].TipDistance > maxTipWait)
			{
				stepOrder = i % 2 == 0;
				break;
			}
		}
		Leg[] array = legs;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Movable = stepOrder;
			stepOrder = !stepOrder;
		}
		int num = ((!stepOrder) ? 1 : 0);
		if (readySwitchOrder && !legs[num].Animating)
		{
			stepOrder = !stepOrder;
			readySwitchOrder = false;
		}
		if (!readySwitchOrder && legs[num].Animating)
		{
			readySwitchOrder = true;
		}
	}

	private IEnumerator AdjustBodyTransform()
	{
		while (true)
		{
			Vector3 zero = Vector3.zero;
			bodyUp = Vector3.zero;
			Leg[] array = legs;
			foreach (Leg leg in array)
			{
				zero += leg.TipPos;
				bodyUp += leg.TipUpDir + leg.RaycastTipNormal;
			}
			if (Physics.Raycast(bodyTransform.position, bodyTransform.up * -1f, out var hitInfo, 10f))
			{
				bodyUp += hitInfo.normal;
			}
			zero /= (float)legs.Length;
			bodyUp.Normalize();
			bodyPos = zero + bodyUp * bodyHeightBase;
			bodyTransform.position = Vector3.Lerp(bodyTransform.position, bodyPos, PosAdjustRatio);
			bodyRight = Vector3.Cross(bodyUp, bodyTransform.forward);
			bodyForward = Vector3.Cross(bodyRight, bodyUp);
			bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);
			bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, bodyRotation, RotAdjustRatio);
			yield return new WaitForFixedUpdate();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(bodyPos, bodyPos + bodyRight);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(bodyPos, bodyPos + bodyUp);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(bodyPos, bodyPos + bodyForward);
	}
}
