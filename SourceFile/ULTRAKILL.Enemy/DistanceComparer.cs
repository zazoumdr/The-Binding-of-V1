using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ULTRAKILL.Enemy;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DistanceComparer : IComparer<TargetIndexAndDistance>
{
	public int Compare(TargetIndexAndDistance x, TargetIndexAndDistance y)
	{
		return x.distance.CompareTo(y.distance);
	}
}
