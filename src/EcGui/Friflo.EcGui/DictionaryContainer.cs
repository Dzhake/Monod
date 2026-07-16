using System;
using System.Collections.Generic;
using System.Linq;

namespace Friflo.EcGui;

internal sealed class DictionaryContainer<TDictionary, TKey, TValue> : IContainer, IDisposable where TDictionary : IDictionary<TKey, TValue> where TKey : notnull
{
	private int count;

	private TDictionary? dictionary;

	private IEnumerator<KeyValuePair<TKey, TValue>>? enumerator;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<DictionaryContainer<TDictionary, TKey, TValue>> Pool = new Stack<DictionaryContainer<TDictionary, TKey, TValue>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(dictionary);

	public Type ItemType => typeof(TValue);

	public KeyValuePair<TKey, TValue> Current => enumerator.Current;

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private DictionaryContainer()
	{
	}

	internal static IContainer Get(TDictionary? dictionary, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out DictionaryContainer<TDictionary, TKey, TValue> result))
		{
			result = new DictionaryContainer<TDictionary, TKey, TValue>();
		}
		result.drawValue = drawValue;
		result.count = dictionary?.Count ?? 0;
		result.dictionary = dictionary;
		return result;
	}

	public void Dispose()
	{
		count = -1;
		dictionary = default(TDictionary);
		enumerator = null;
		index = -1;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
		enumerator = dictionary.GetEnumerator();
	}

	public bool MoveNext()
	{
		index++;
		return enumerator.MoveNext();
	}

	public void SeekCurrent(int offset)
	{
		IEnumerator<KeyValuePair<TKey, TValue>> enumerator = this.enumerator;
		for (int i = 0; i < offset; i++)
		{
			if (!enumerator.MoveNext())
			{
				break;
			}
			index++;
		}
	}

	public void Add(int index)
	{
		KeyValuePair<TKey, TValue> item = ItemUtils.CreateKeyValuePair<TKey, TValue>();
		TDictionary val = this.dictionary;
		TDictionary val2;
		if (val == null)
		{
			val2 = (this.dictionary = ItemUtils.CreateContainer<TDictionary>());
			val = val2;
			drawValue.SetValue(val);
		}
		KeyValuePair<TKey, TValue>[] array = val.ToArray();
		val.Clear();
		if (val is Dictionary<TKey, TValue> dictionary)
		{
			dictionary.EnsureCapacity(array.Length + 1);
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				val.Add(item);
			}
			ref TDictionary reference = ref val;
			val2 = default(TDictionary);
			if (val2 == null)
			{
				val2 = reference;
				reference = ref val2;
			}
			KeyValuePair<TKey, TValue> item2 = array[i];
			reference.Add(item2);
		}
		if (index == array.Length)
		{
			val.Add(item);
		}
	}

	public void Remove(int index)
	{
		if (dictionary == null)
			return;

		if (index == -1)
		{
			dictionary.Clear();
			return;
		}

		int i = 0;
		TKey keyToRemove = default!;

		foreach (var (key, _) in dictionary)
		{
			if (i++ == index)
			{
				keyToRemove = key;
				goto RemoveKey;
			}
		}

		return;

	RemoveKey:
		dictionary.Remove(keyToRemove);
	}

	internal void ChangeDictionaryKey(TKey key)
	{
		var current = Current;

		if (dictionary.ContainsKey(key))
			return;

		dictionary.Remove(current.Key);
		dictionary.Add(key, current.Value);

		enumerator = dictionary.GetEnumerator();

		for (int i = 0; i <= index; i++)
			enumerator.MoveNext();
	}

	internal void ChangeDictionaryValue(TValue value)
	{
		dictionary[Current.Key] = value;
	}
}
