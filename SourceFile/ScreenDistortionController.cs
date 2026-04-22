using System.Collections.Generic;
using UnityEngine;

public class ScreenDistortionController : MonoSingleton<ScreenDistortionController>
{
	public List<ScreenDistortionField> fields = new List<ScreenDistortionField>();

	private PostProcessV2_Handler pp;

	private int lastCount = int.MaxValue;

	private void Start()
	{
		pp = MonoSingleton<PostProcessV2_Handler>.Instance;
	}

	private void Update()
	{
		_ = lastCount;
		_ = fields.Count;
		if (lastCount != fields.Count)
		{
			pp.WickedEffect(fields.Count > 0);
			lastCount = fields.Count;
		}
		if (fields.Count == 0)
		{
			Shader.SetGlobalFloat("_RandomNoiseStrength", 0f);
			return;
		}
		float num = 0f;
		for (int num2 = fields.Count - 1; num2 >= 0; num2--)
		{
			if (fields[num2] == null)
			{
				fields.RemoveAt(num2);
			}
			else if (fields[num2].currentStrength > num)
			{
				num = fields[num2].currentStrength;
			}
		}
		Shader.SetGlobalFloat("_RandomNoiseStrength", num);
	}
}
