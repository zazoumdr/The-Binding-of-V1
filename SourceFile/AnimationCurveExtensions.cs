using System.Collections.Generic;
using UnityEngine;

public static class AnimationCurveExtensions
{
	public static float EvaluateTangent(this AnimationCurve curve, float t)
	{
		float num = Mathf.Clamp01(t - 0.0001f);
		float num2 = Mathf.Clamp01(t + 0.0001f);
		float num3 = curve.Evaluate(num);
		float num4 = curve.Evaluate(num2);
		if (!(num2 - num < Mathf.Epsilon))
		{
			return (num4 - num3) / (num2 - num);
		}
		return 0f;
	}

	public static AnimationCurve CreateSubCurve(this AnimationCurve curve, float tStart, float tEnd)
	{
		if (tEnd - tStart <= Mathf.Epsilon)
		{
			float value = curve.Evaluate(tStart);
			return new AnimationCurve(new Keyframe(0f, value), new Keyframe(1f, value));
		}
		float num = tEnd - tStart;
		Keyframe[] keys = curve.keys;
		List<Keyframe> list = new List<Keyframe>(keys.Length + 2);
		float value2 = curve.Evaluate(tStart);
		float num2 = curve.EvaluateTangent(tStart) * num;
		list.Add(new Keyframe(0f, value2, num2, num2));
		Keyframe[] array = keys;
		for (int i = 0; i < array.Length; i++)
		{
			Keyframe keyframe = array[i];
			if (keyframe.time > tStart + Mathf.Epsilon && keyframe.time < tEnd - Mathf.Epsilon)
			{
				float time = (keyframe.time - tStart) / num;
				float inTangent = keyframe.inTangent * num;
				float outTangent = keyframe.outTangent * num;
				Keyframe keyframe2 = new Keyframe(time, keyframe.value, inTangent, outTangent, keyframe.inWeight, keyframe.outWeight);
				keyframe2.weightedMode = keyframe.weightedMode;
				Keyframe item = keyframe2;
				list.Add(item);
			}
		}
		float value3 = curve.Evaluate(tEnd);
		float num3 = curve.EvaluateTangent(tEnd) * num;
		list.Add(new Keyframe(1f, value3, num3, num3));
		return new AnimationCurve(list.ToArray())
		{
			preWrapMode = curve.preWrapMode,
			postWrapMode = curve.postWrapMode
		};
	}
}
