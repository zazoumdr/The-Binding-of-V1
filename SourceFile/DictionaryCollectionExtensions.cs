using System.Collections.Generic;

public static class DictionaryCollectionExtensions
{
	public static void Add<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new TCollection();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
	}

	public static bool Remove<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : ICollection<TValue>, new()
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			return false;
		}
		return value2.Remove(value);
	}
}
