using System.Collections;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
	private Transform root;

	public Transform rayStart;

	public Transform footTarget;

	private FootController fC;

	[SerializeField]
	private AnimationCurve heightCurve;

	private float maxRayDist = 7f;

	private float tipPassOver = 0.275f;

	[HideInInspector]
	public bool animating;

	private bool needsToMove;

	private Quaternion initialRotRelation;

	private AudioSource aud;

	[SerializeField]
	private AudioClip[] footsteps;

	public Vector3 TipPos { get; private set; }

	public Vector3 TipUpDir { get; private set; }

	public Vector3 RaycastTipPos { get; private set; }

	public Vector3 RaycastTipNormal { get; private set; }

	public float TipDistance { get; private set; }

	private void Awake()
	{
		aud = footTarget.GetComponent<AudioSource>();
		fC = GetComponentInParent<FootController>();
		root = fC.root;
		rayStart.parent = root;
		animating = false;
		initialRotRelation = Quaternion.Inverse(root.rotation) * footTarget.rotation;
	}

	private void OnEnable()
	{
		TipPos = footTarget.position;
		UpdateIKTargetTransform();
	}

	private void Update()
	{
		if (!animating)
		{
			UpdateIKTargetTransform();
		}
		if (!HasHitTargetPosition())
		{
			fC.StartWalk(this);
		}
	}

	public bool HasHitTargetPosition()
	{
		if (Physics.Raycast(rayStart.position, root.up * -1f, out var hitInfo, maxRayDist, fC.canSee))
		{
			RaycastTipPos = hitInfo.point;
			RaycastTipNormal = hitInfo.normal;
		}
		TipDistance = (RaycastTipPos - footTarget.position).magnitude;
		Debug.DrawLine(rayStart.position, RaycastTipPos, Color.green);
		return TipDistance < fC.maxMoveDistance;
	}

	public void StartLegAnimation()
	{
		StartCoroutine(WaitForStopAnimating());
	}

	private IEnumerator WaitForStopAnimating()
	{
		while (animating)
		{
			yield return null;
		}
		StartCoroutine(AnimateLeg());
	}

	private IEnumerator AnimateLeg()
	{
		if (HasHitTargetPosition())
		{
			fC.StartNextFootStep(this);
			yield break;
		}
		animating = true;
		float timer = 0f;
		Vector3 startingTipPos = TipPos;
		Vector3 tipDirVec = RaycastTipPos - TipPos;
		tipDirVec += tipDirVec.normalized * tipPassOver;
		Vector3 normalized = Vector3.Cross(root.up, tipDirVec.normalized).normalized;
		TipUpDir = Vector3.Cross(tipDirVec.normalized, normalized);
		bool startedNextFoot = false;
		while (timer < 1f)
		{
			float num = Mathf.Max((RaycastTipPos - startingTipPos).magnitude / tipDirVec.magnitude, 1f);
			TipPos = startingTipPos + tipDirVec * num * timer;
			TipPos += TipUpDir * heightCurve.Evaluate(timer) * fC.maxFootHeight;
			UpdateIKTargetTransform();
			if (!startedNextFoot && timer > fC.nextStepThreshold)
			{
				fC.StartNextFootStep(this);
				startedNextFoot = true;
			}
			timer += Time.deltaTime * fC.strideFrequency;
			yield return null;
		}
		if (!startedNextFoot)
		{
			fC.StartNextFootStep(this);
		}
		if ((bool)(Object)(object)aud && footsteps.Length != 0)
		{
			aud.clip = footsteps[Random.Range(0, footsteps.Length)];
			aud.SetPitch(Random.Range(0.75f, 1.25f));
			aud.volume = Random.Range(0.15f, 0.35f);
			aud.Play(tracked: true);
		}
		animating = false;
	}

	private void UpdateIKTargetTransform()
	{
		footTarget.position = TipPos;
		footTarget.rotation = root.rotation * initialRotRelation;
	}
}
