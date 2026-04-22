using System.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public Vector3 rotation;

	public float rotationTime = 1f;

	private Quaternion initialRotation;

	private float rotationProgress;

	public AnimationCurve customCurve;

	public EasingFunction.Ease easingFunction;

	private EasingFunction.Function selectedEasingFunction;

	public UltrakillEvent doAThing;

	public void StartRotate()
	{
		StartCoroutine(Rotate());
	}

	private IEnumerator Rotate()
	{
		rotationProgress = 0f;
		initialRotation = base.transform.rotation;
		selectedEasingFunction = EasingFunction.GetEasingFunction(easingFunction);
		while (rotationProgress < 1f)
		{
			rotationProgress += Time.deltaTime / rotationTime;
			float a = ((customCurve != null) ? customCurve.Evaluate(rotationProgress) : selectedEasingFunction(0f, 1f, rotationProgress));
			a = Mathf.Min(a, 1f);
			base.transform.rotation = Quaternion.LerpUnclamped(initialRotation, initialRotation * Quaternion.Euler(rotation), a);
			yield return null;
		}
		doAThing.Invoke();
	}
}
