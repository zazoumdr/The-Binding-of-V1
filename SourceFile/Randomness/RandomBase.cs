using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Randomness;

public abstract class RandomBase<T> : MonoBehaviour where T : RandomEntry, new()
{
	public bool randomizeOnEnable = true;

	public bool ensureNoRepeats = true;

	public int toBeEnabledCount = 1;

	public T[] entries;

	private T lastPicked;

	private bool firstDeserialization = true;

	private int arrayLength;

	private void OnEnable()
	{
		if (randomizeOnEnable)
		{
			Randomize();
		}
	}

	public virtual void Randomize()
	{
		RandomizeWithCount(toBeEnabledCount);
	}

	public virtual void RandomizeWithCount(int count)
	{
		List<T> list = new List<T>(entries);
		T localLastPicked = lastPicked;
		for (int i = 0; i < count; i++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			List<T> list2;
			if (ensureNoRepeats && localLastPicked != null && list.Count > 1)
			{
				list2 = list.Where((T e) => e != localLastPicked).ToList();
				if (list2.Count == 0)
				{
					list2 = list;
				}
			}
			else
			{
				list2 = list;
			}
			T val = WeightedPick(list2);
			if (val == null)
			{
				break;
			}
			PerformTheAction(val);
			localLastPicked = val;
			list.Remove(val);
		}
		lastPicked = localLastPicked;
	}

	public static T WeightedPick(List<T> pool)
	{
		int num = 0;
		foreach (T item in pool)
		{
			num += item.weight;
		}
		if (num <= 0)
		{
			return null;
		}
		int num2 = Random.Range(0, num);
		int num3 = 0;
		foreach (T item2 in pool)
		{
			num3 += item2.weight;
			if (num2 < num3)
			{
				return item2;
			}
		}
		return null;
	}

	public abstract void PerformTheAction(RandomEntry entry);

	private void OnValidate()
	{
		if (firstDeserialization)
		{
			arrayLength = entries.Length;
			firstDeserialization = false;
		}
		else
		{
			if (entries.Length == arrayLength)
			{
				return;
			}
			if (entries.Length > arrayLength)
			{
				for (int i = arrayLength; i < entries.Length; i++)
				{
					entries[i] = new T();
				}
			}
			arrayLength = entries.Length;
		}
	}
}
