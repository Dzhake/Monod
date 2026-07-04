using System;
using System.Collections.Generic;
using System.Linq;

namespace Friflo.EcGui;

internal sealed class CollectionContainer<TCollection, T> : IContainer, IDisposable where TCollection : ICollection<T>
{
	private int count;

	private TCollection? collection;

	private IEnumerator<T>? enumerator;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<CollectionContainer<TCollection, T>> Pool = new Stack<CollectionContainer<TCollection, T>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(collection);

	public Type ItemType => typeof(T);

	public T Current => enumerator.Current;

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private CollectionContainer()
	{
	}

	internal static IContainer Get(TCollection? collection, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out CollectionContainer<TCollection, T> result))
		{
			result = new CollectionContainer<TCollection, T>();
		}
		result.drawValue = drawValue;
		result.count = collection?.Count ?? 0;
		result.collection = collection;
		return result;
	}

	public void Dispose()
	{
		count = -1;
		collection = default(TCollection);
		enumerator = null;
		index = -1;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
		enumerator = collection?.GetEnumerator();
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
		T item = ItemUtils.CreateItem<T>();
		TCollection val = collection;
		TCollection val2;
		if (val == null)
		{
			val2 = (collection = ItemUtils.CreateContainer<TCollection>());
			val = val2;
			drawValue.SetValue(val);
		}
		T[] array = val.ToArray();
		val.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				val.Add(item);
			}
			ref TCollection reference = ref val;
			val2 = default(TCollection);
			if (val2 == null)
			{
				val2 = reference;
				reference = ref val2;
			}
			T item2 = array[i];
			reference.Add(item2);
		}
		if (index == array.Length)
		{
			val.Add(item);
		}
	}

	public void Remove(int index)
	{
		TCollection val = collection;
		if (index == -1)
		{
			val?.Clear();
			return;
		}
		T[] array = val.ToArray();
		val.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i != index)
			{
				val.Add(array[i]);
			}
		}
	}

	internal void ChangeCollectionItem(T value)
	{
		TCollection val = collection;
		T[] array = val.ToArray();
		val.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				val.Add(value);
			}
			else
			{
				val.Add(array[i]);
			}
		}
		enumerator = val.GetEnumerator();
		for (int j = 0; j <= index; j++)
		{
			enumerator.MoveNext();
		}
	}
}
