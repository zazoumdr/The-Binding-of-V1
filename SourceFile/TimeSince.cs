using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct TimeSince
{
	public float time;

	public const int Now = 0;

	public static implicit operator float(TimeSince ts)
	{
		return Time.time - ts.time;
	}

	public static implicit operator TimeSince(float ts)
	{
		return new TimeSince
		{
			time = Time.time - ts
		};
	}

	public override string ToString()
	{
		return ((float)this).ToString(CultureInfo.InvariantCulture);
	}
}
