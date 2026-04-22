using System.Collections;
using UnityEngine;

public class Leg : MonoBehaviour
{
	private LegController legController;

	[SerializeField]
	private Transform bodyTransform;

	[SerializeField]
	private Transform rayOrigin;

	public GameObject ikTarget;

	[SerializeField]
	private AnimationCurve speedCurve;

	[SerializeField]
	private AnimationCurve heightCurve;

	private float tipMaxHeight = 0.2f;

	private float tipAnimationTime = 0.15f;

	private float tipAnimationFrameTime = 1f / 60f;

	private float ikOffset = 1f;

	private float tipMoveDist = 0.55f;

	private float maxRayDist = 7f;

	private float tipPassOver = 0.275f;

	public Vector3 TipPos { get; private set; }

	public Vector3 TipUpDir { get; private set; }

	public Vector3 RaycastTipPos { get; private set; }

	public Vector3 RaycastTipNormal { get; private set; }

	public bool Animating { get; private set; }

	public bool Movable { get; set; }

	public float TipDistance { get; private set; }

	private void Awake()
	{
		legController = GetComponentInParent<LegController>();
		TipPos = ikTarget.transform.position;
	}

	private void Start()
	{
		UpdateIKTargetTransform();
	}

	private void Update()
	{
		Debug.DrawRay(rayOrigin.position, bodyTransform.up.normalized * -1f * maxRayDist, Color.green);
		if (Physics.Raycast(rayOrigin.position, bodyTransform.up.normalized * -1f, out var hitInfo, maxRayDist))
		{
			RaycastTipPos = hitInfo.point;
			RaycastTipNormal = hitInfo.normal;
		}
		TipDistance = (RaycastTipPos - TipPos).magnitude;
		if (!Animating && TipDistance > tipMoveDist && Movable)
		{
			StartCoroutine(AnimateLeg());
		}
	}

	private IEnumerator AnimateLeg()
	{
		Animating = true;
		float timer = 0f;
		Vector3 startingTipPos = TipPos;
		Vector3 tipDirVec = RaycastTipPos - TipPos;
		tipDirVec += tipDirVec.normalized * tipPassOver;
		Vector3 normalized = Vector3.Cross(bodyTransform.up, tipDirVec.normalized).normalized;
		TipUpDir = Vector3.Cross(tipDirVec.normalized, normalized);
		while (timer < tipAnimationTime + tipAnimationFrameTime)
		{
			float num = speedCurve.Evaluate(timer / tipAnimationTime);
			float num2 = Mathf.Max((RaycastTipPos - startingTipPos).magnitude / tipDirVec.magnitude, 1f);
			TipPos = startingTipPos + tipDirVec * num2 * num;
			TipPos += TipUpDir * heightCurve.Evaluate(num) * tipMaxHeight;
			UpdateIKTargetTransform();
			timer += tipAnimationFrameTime;
			yield return new WaitForSeconds(tipAnimationFrameTime);
		}
		Animating = false;
	}

	private void UpdateIKTargetTransform()
	{
		ikTarget.transform.position = TipPos + bodyTransform.up.normalized * ikOffset;
		ikTarget.transform.rotation = Quaternion.LookRotation(TipPos - ikTarget.transform.position) * Quaternion.Euler(90f, 0f, 0f);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(RaycastTipPos, 0.1f);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(TipPos, 0.1f);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(TipPos, RaycastTipPos);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(ikTarget.transform.position, 0.1f);
	}
}
