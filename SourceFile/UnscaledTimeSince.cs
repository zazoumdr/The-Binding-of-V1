using System;
using UnityEngine;

[Serializable]
public struct UnscaledTimeSince
{
	private float time;

	public const int Now = 0;

	public static implicit operator float(UnscaledTimeSince ts)
	{
		return Time.unscaledTime - ts.time;
	}

	public static implicit operator UnscaledTimeSince(float ts)
	{
		return new UnscaledTimeSince
		{
			time = Time.unscaledTime - ts
		};
	}
}
