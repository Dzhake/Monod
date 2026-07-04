using System;
using System.Collections.Generic;

namespace Friflo.EcGui;

internal sealed class ArrayContainer<TItem> : IContainer, IDisposable
{
	private int count;

	private TItem[]? array;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<ArrayContainer<TItem>> Pool = new Stack<ArrayContainer<TItem>>();

	public int Count => count;

	public bool IsNull => array == null;

	public Type ItemType => typeof(TItem);

	public TItem Current => array[index];

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private ArrayContainer()
	{
	}

	internal static IContainer Get(TItem[]? container, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out ArrayContainer<TItem> result))
		{
			result = new ArrayContainer<TItem>();
		}
		result.drawValue = drawValue;
		result.count = ((container != null) ? container.Length : 0);
		result.array = container;
		return result;
	}

	public void Dispose()
	{
		index = -1;
		count = -1;
		array = null;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
	}

	public bool MoveNext()
	{
		if (index < count - 1)
		{
			index++;
			return true;
		}
		return false;
	}

	public void SeekCurrent(int offset)
	{
		index += offset;
	}

	public void Add(int index)
	{
		TItem[] array = this.array;
		TItem val = ItemUtils.CreateItem<TItem>();
		TItem[] array2;
		if (array == null)
		{
			array2 = new TItem[1] { val };
		}
		else
		{
			int num = array.Length;
			array2 = new TItem[num + 1];
			Array.Copy(array, 0, array2, 0, index);
			Array.Copy(array, index, array2, index + 1, num - index);
			array2[index] = val;
		}
		drawValue.SetValue(array2);
	}

	public void Remove(int index)
	{
		TItem[] array = this.array;
		if (array != null || index != -1)
		{
			TItem[] array2 = Array.Empty<TItem>();
			if (index >= 0)
			{
				int num = array.Length;
				array2 = new TItem[num - 1];
				Array.Copy(array, 0, array2, 0, index);
				Array.Copy(array, index + 1, array2, index, num - index - 1);
			}
			drawValue.SetValue(array2);
		}
	}

	internal void ChangeArrayItem(TItem value)
	{
		array[index] = value;
	}
}
