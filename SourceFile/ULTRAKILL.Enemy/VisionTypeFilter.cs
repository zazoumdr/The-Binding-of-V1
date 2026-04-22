namespace ULTRAKILL.Enemy;

public struct VisionTypeFilter
{
	private int mask = 0;

	public VisionTypeFilter(params TargetType[] types)
	{
		for (int i = 0; i < types.Length; i++)
		{
			mask |= 1 << (int)types[i];
		}
	}

	public readonly bool HasType(TargetType type)
	{
		return (mask & (1 << (int)type)) != 0;
	}
}
