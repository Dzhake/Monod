using System;
using System.Collections.Generic;
using System.Linq;

namespace Friflo.EcGui;

internal sealed class SetContainer<TSet, T> : IContainer, IDisposable where TSet : ISet<T>
{
	private int count;

	private TSet? set;

	private IEnumerator<T>? enumerator;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<SetContainer<TSet, T>> Pool = new Stack<SetContainer<TSet, T>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(set);

	public Type ItemType => typeof(T);

	public T Current => enumerator.Current;

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private SetContainer()
	{
	}

	internal static IContainer Get(TSet? set, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out SetContainer<TSet, T> result))
		{
			result = new SetContainer<TSet, T>();
		}
		result.drawValue = drawValue;
		result.count = set?.Count ?? 0;
		result.set = set;
		return result;
	}

	public void Dispose()
	{
		count = -1;
		set = default(TSet);
		enumerator = null;
		index = -1;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
		enumerator = set?.GetEnumerator();
	}

	public bool MoveNext()
	{
		index++;
		return enumerator.MoveNext();
	}

	public void SeekCurrent(int offset)
	{
		IEnumerator<T> enumerator = this.enumerator;
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
		T val = ItemUtils.CreateItem<T>();
		TSet val2 = set;
		TSet val3;
		if (val2 == null)
		{
			val3 = (set = ItemUtils.CreateContainer<TSet>());
			val2 = val3;
			drawValue.SetValue(val2);
		}
		if (val2.Contains(val))
		{
			throw new ArgumentException($"Element '{val}' is already in Set.");
		}
		T[] array = val2.ToArray();
		val2.Clear();
		if (val2 is HashSet<T> hashSet)
		{
			hashSet.EnsureCapacity(array.Length + 1);
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				val2.Add(val);
			}
			ref TSet reference = ref val2;
			val3 = default(TSet);
			if (val3 == null)
			{
				val3 = reference;
				reference = ref val3;
			}
			T item = array[i];
			reference.Add(item);
		}
		if (index == array.Length)
		{
			val2.Add(val);
		}
	}

	public void Remove(int index)
	{
		if (set is null) return;

		if (index == -1)
		{
			set.Clear();
			return;
		}

		int i = 0;
		foreach (T item in set)
		{
			if (i == index)
			{
				set.Remove(item);
				break;
			}
			i++;
		}
	}

	internal void ChangeSetItem(T value)
	{
		T current = Current;
		TSet val = set;
		if (!val.Contains(value) || EqualityComparer<T>.Default.Equals(value, current))
		{
			val.Remove(current);
			val.Add(value);
			enumerator = val.GetEnumerator();
			for (int i = 0; i <= index; i++)
			{
				enumerator.MoveNext();
			}
		}
	}
}
