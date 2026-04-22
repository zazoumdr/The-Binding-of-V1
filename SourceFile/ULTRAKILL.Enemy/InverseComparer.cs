using System.Collections.Generic;

namespace ULTRAKILL.Enemy;

public class InverseComparer : IComparer<float>
{
	public static readonly InverseComparer Default = new InverseComparer();

	public int Compare(float x, float y)
	{
		return y.CompareTo(x);
	}
}
