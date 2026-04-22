using System;
using UnityEngine;
using UnityEngine.AI;

public class FootController : MonoBehaviour
{
	public Transform root;

	public Transform body;

	public LayerMask canSee;

	public Vector3 tiltAmounts = new Vector3(10f, 0f, 10f);

	[SerializeField]
	private IKFootSolver[] footSolvers;

	public float maxMoveDistance = 0.7f;

	public float maxFootHeight = 0.5f;

	public float strideFrequency = 10f;

	private Vector3 rootUp;

	private Vector3 rootForward;

	private Vector3 rootRight;

	private Quaternion rootRotation;

	private float PosAdjustRatio = 0.1f;

	public float bodyBobSmoothness = 1f;

	private float RotAdjustRatio = 0.2f;

	public float rotateSpeed = 0.1f;

	private Vector3 initialBodyOffset;

	private Quaternion initialBodyRot;

	[Range(0f, 1f)]
	public float nextStepThreshold = 0.5f;

	private GroundCheck gc;

	private NavMeshAgent nma;

	private bool isWalking;

	private void Awake()
	{
		Vector3 zero = Vector3.zero;
		IKFootSolver[] array = footSolvers;
		foreach (IKFootSolver iKFootSolver in array)
		{
			zero += iKFootSolver.TipPos;
		}
		zero /= (float)footSolvers.Length;
		initialBodyOffset = body.position - zero;
		initialBodyRot = body.localRotation;
		gc = body.GetComponentInChildren<GroundCheck>();
		nma = GetComponentInParent<NavMeshAgent>();
	}

	private void Update()
	{
		BobBodyFromLegs();
		AdjustBodyTransform();
	}

	private void BobBodyFromLegs()
	{
		Quaternion b = initialBodyRot;
		Vector3 zero = Vector3.zero;
		IKFootSolver[] array = footSolvers;
		foreach (IKFootSolver iKFootSolver in array)
		{
			float y = iKFootSolver.RaycastTipPos.y;
			float num = iKFootSolver.TipPos.y - y;
			Vector3 vector = body.InverseTransformPoint(iKFootSolver.footTarget.position);
			vector.Normalize();
			zero += vector * num;
		}
		zero /= (float)footSolvers.Length;
		Vector3 euler = Vector3.Scale(new Vector3(zero.x, zero.y, 0f - zero.z), tiltAmounts);
		b *= Quaternion.Euler(euler);
		body.localRotation = Quaternion.Slerp(body.localRotation, b, bodyBobSmoothness * 0.1f);
	}

	public void StartWalk(IKFootSolver startFoot)
	{
		if (!isWalking)
		{
			startFoot.StartLegAnimation();
			isWalking = true;
		}
	}

	public void StartNextFootStep(IKFootSolver foot)
	{
		bool flag = false;
		IKFootSolver[] array = footSolvers;
		foreach (IKFootSolver iKFootSolver in array)
		{
			flag |= !iKFootSolver.HasHitTargetPosition();
		}
		if (!flag)
		{
			isWalking = false;
			return;
		}
		int num = (Array.IndexOf(footSolvers, foot) + 1) % footSolvers.Length;
		footSolvers[num].StartLegAnimation();
	}

	private void AdjustBodyTransform()
	{
		Vector3 zero = Vector3.zero;
		rootUp = Vector3.zero;
		IKFootSolver[] array = footSolvers;
		foreach (IKFootSolver iKFootSolver in array)
		{
			zero += iKFootSolver.TipPos;
			rootUp += iKFootSolver.TipUpDir + iKFootSolver.RaycastTipNormal;
		}
		if (Physics.Raycast(root.position + root.up, root.up * -1f, out var hitInfo, 10f, canSee))
		{
			rootUp += hitInfo.normal;
			Debug.DrawRay(zero / footSolvers.Length, hitInfo.normal * 10f, Color.red);
		}
		zero /= (float)footSolvers.Length;
		rootUp.Normalize();
		rootRight = Vector3.Cross(rootUp, root.forward);
		rootForward = Vector3.Cross(rootRight, rootUp);
		rootRotation = Quaternion.LookRotation(rootForward, rootUp);
		root.rotation = Quaternion.Slerp(root.rotation, rootRotation, RotAdjustRatio);
	}
}
