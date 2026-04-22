using System;
using System.Collections.Generic;
using plog;
using ULTRAKILL.Enemy;
using UnityEngine;

public class TargetIndex<TKey, TValue> where TValue : IComparable<TValue>
{
	private static readonly Logger Log = new Logger("TargetIndex");

	private readonly Func<TargetData, TValue> comparator;

	private readonly Func<TargetData, TKey> keySelector;

	private readonly Dictionary<TKey, TValue> items;

	public TargetIndex(Func<TargetData, TValue> comp, Func<TargetData, TKey> keySel)
	{
		comparator = comp;
		keySelector = keySel;
		items = new Dictionary<TKey, TValue>();
	}

	public void Reset()
	{
		items.Clear();
	}

	public bool Contains(TKey item)
	{
		return items.ContainsKey(item);
	}

	public TValue Of(TargetData item)
	{
		TKey key = keySelector(item);
		if (items.TryGetValue(key, out var value))
		{
			return value;
		}
		TValue val = comparator(item);
		items[key] = val;
		return val;
	}
}
public static class TargetIndex
{
	public static TargetIndex<Vector3, float> MakeDistanceIndex(Func<Vector3> sourcePosSupllier, bool fromHead = false)
	{
		return new TargetIndex<Vector3, float>((TargetData target) => target.DistanceTo(sourcePosSupllier(), fromHead), (TargetData target) => sourcePosSupllier());
	}
}
