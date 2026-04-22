using System.Collections.Generic;

namespace BeamHitInterpolation;

public static class ListPool<T>
{
	private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

	public static List<T> Get()
	{
		if (Pool.Count <= 0)
		{
			return new List<T>();
		}
		return Pool.Pop();
	}

	public static void Release(List<T> list)
	{
		if (list != null)
		{
			list.Clear();
			Pool.Push(list);
		}
	}
}
