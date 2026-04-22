using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct NativeCurve : IDisposable
{
	private NativeArray<float> m_Values;

	private WrapMode m_PreWrapMode;

	private WrapMode m_PostWrapMode;

	public bool isCreated => m_Values.IsCreated;

	private void InitializeValues(int count, Allocator allocator = Allocator.Persistent)
	{
		if (m_Values.IsCreated)
		{
			m_Values.Dispose();
		}
		m_Values = new NativeArray<float>(count, allocator, NativeArrayOptions.UninitializedMemory);
	}

	public void Update(AnimationCurve curve, int resolution)
	{
		if (curve != null)
		{
			m_PreWrapMode = curve.preWrapMode;
			m_PostWrapMode = curve.postWrapMode;
			if (!m_Values.IsCreated || m_Values.Length != resolution)
			{
				InitializeValues(resolution);
			}
			for (int i = 0; i < resolution; i++)
			{
				m_Values[i] = curve.Evaluate((float)i / (float)resolution);
			}
		}
	}

	public float Evaluate(float t)
	{
		int length = m_Values.Length;
		if (length == 1)
		{
			return m_Values[0];
		}
		if (t < 0f)
		{
			switch (m_PreWrapMode)
			{
			default:
				return m_Values[0];
			case WrapMode.Loop:
				t = 1f - math.abs(t) % 1f;
				break;
			case WrapMode.PingPong:
				t = PingPong(t, 1f);
				break;
			}
		}
		else if (t > 1f)
		{
			switch (m_PostWrapMode)
			{
			default:
				return m_Values[length - 1];
			case WrapMode.Loop:
				t %= 1f;
				break;
			case WrapMode.PingPong:
				t = PingPong(t, 1f);
				break;
			}
		}
		float num = t * (float)(length - 1);
		int num2 = (int)num;
		int num3 = num2 + 1;
		if (num3 >= length)
		{
			num3 = length - 1;
		}
		return math.lerp(m_Values[num2], m_Values[num3], num - (float)num2);
	}

	public void Dispose()
	{
		if (m_Values.IsCreated)
		{
			m_Values.Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float Repeat(float t, float length)
	{
		return math.clamp(t - math.floor(t / length) * length, 0f, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float PingPong(float t, float length)
	{
		t = Repeat(t, length * 2f);
		return length - math.abs(t - length);
	}
}
