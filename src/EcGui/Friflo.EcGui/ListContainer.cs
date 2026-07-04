using System;
using System.Collections.Generic;

namespace Friflo.EcGui;

internal sealed class ListContainer<TList, TItem> : IContainer, IDisposable where TList : IList<TItem>
{
	private int count;

	private TList? list;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<ListContainer<TList, TItem>> Pool = new Stack<ListContainer<TList, TItem>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(list);

	public Type ItemType => typeof(TItem);

	public TItem Current
	{
		get
		{
			ref TList? reference = ref list;
			int num = index;
			return reference[num];
		}
	}

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private ListContainer()
	{
	}

	internal static IContainer Get(TList? container, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out ListContainer<TList, TItem> result))
		{
			result = new ListContainer<TList, TItem>();
		}
		result.drawValue = drawValue;
		result.count = container?.Count ?? 0;
		result.list = container;
		return result;
	}

	public void Dispose()
	{
		index = -1;
		count = -1;
		list = default(TList);
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
		TItem item = ItemUtils.CreateItem<TItem>();
		TList val = list;
		if (val == null)
		{
			list = ItemUtils.CreateContainer<TList>();
		}
		list.Insert(index, item);
		drawValue.SetValue(list);
	}

	public void Remove(int index)
	{
		if (index == -1)
		{
			ref TList reference = ref list;
			TList val = default(TList);
			if (val == null)
			{
				val = reference;
				reference = ref val;
				if (val == null)
				{
					return;
				}
			}
			reference.Clear();
		}
		else
		{
			list.RemoveAt(index);
		}
	}

	internal void ChangeListItem(TItem value)
	{
		ref TList? reference = ref list;
		int num = index;
		reference[num] = value;
		drawValue.SetValue(list);
	}
}
