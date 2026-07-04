using System;
using System.Collections.Generic;
using System.Linq;

namespace Friflo.EcGui;

internal sealed class EnumerableContainer<TEnumerable, T> : IContainer, IDisposable where TEnumerable : IEnumerable<T>
{
	private int count;

	private TEnumerable? enumerable;

	private IEnumerator<T>? enumerator;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<EnumerableContainer<TEnumerable, T>> Pool = new Stack<EnumerableContainer<TEnumerable, T>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(enumerable);

	public Type ItemType => typeof(T);

	public T Current => enumerator.Current;

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private EnumerableContainer()
	{
	}

	internal static IContainer Get(TEnumerable? enumerable, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out EnumerableContainer<TEnumerable, T> result))
		{
			result = new EnumerableContainer<TEnumerable, T>();
		}
		result.drawValue = drawValue;
		result.count = ((enumerable != null) ? enumerable.Count() : 0);
		result.enumerable = enumerable;
		return result;
	}

	public void Dispose()
	{
		count = -1;
		enumerable = default(TEnumerable);
		enumerator = null;
		index = -1;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
		enumerator = enumerable?.GetEnumerator();
	}

	public bool MoveNext()
	{
		index++;
		return enumerator.MoveNext();
	}

	public void Add(int index)
	{
		TEnumerable val = enumerable;
		if (val == null)
		{
			val = (enumerable = ItemUtils.CreateContainer<TEnumerable>());
			drawValue.SetValue(val);
		}
		if (val is Stack<T> stack)
		{
			EnumerableStack<T>.Add(stack, index);
		}
		else if (val is Queue<T> queue)
		{
			EnumerableQueue<T>.Add(queue, index);
		}
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

	public void Remove(int index)
	{
		if (enumerable is Stack<T> stack)
		{
			EnumerableStack<T>.Remove(stack, index);
		}
		else if (enumerable is Queue<T> queue)
		{
			EnumerableQueue<T>.Remove(queue, index);
		}
	}

	internal void ChangeEnumerableItem(T value)
	{
		if (enumerable is Stack<T> stack)
		{
			EnumerableStack<T>.ChangeStackItem(stack, value, index);
			SetEnumerator();
		}
		else if (enumerable is Queue<T> queue)
		{
			EnumerableQueue<T>.ChangeQueueItem(queue, value, index);
			SetEnumerator();
		}
	}

	private void SetEnumerator()
	{
		enumerator = enumerable.GetEnumerator();
		for (int i = 0; i <= index; i++)
		{
			enumerator.MoveNext();
		}
	}
}
