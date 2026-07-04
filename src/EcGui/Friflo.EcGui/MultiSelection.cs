using System;
using System.Collections.Generic;

namespace Friflo.EcGui;

internal abstract class MultiSelection
{
	protected int changeIndex;

	internal bool changeSelected;

	internal SelectionEventType changeType;

	internal bool changed;

	internal abstract int Length { get; }

	internal abstract bool IsSelected(int index);

	internal abstract void Select(int index, bool selected, SelectionEventType type);

	internal abstract string GetName(int index);

	internal abstract void SortItems(SelectSortType sortType);
}
internal sealed class MultiSelection<T> : MultiSelection where T : notnull
{
	internal SelectionEvent<T>? selectionEvent;

	private readonly T[] items;

	internal readonly HashSet<T> selected = new HashSet<T>();

	private readonly Func<T, string> getName;

	internal T ChangeItem => items[changeIndex];

	internal override int Length => items.Length;

	internal override string GetName(int index)
	{
		return getName(items[index]);
	}

	internal override bool IsSelected(int index)
	{
		return selected.Contains(items[index]);
	}

	internal MultiSelection(T[] items, Func<T, string> getName)
	{
		this.items = items;
		this.getName = getName;
	}

	internal override void Select(int index, bool selected, SelectionEventType type)
	{
		changeIndex = index;
		changeSelected = selected;
		changeType = type;
		changed = true;
		T item = items[index];
		if (type == SelectionEventType.Checkbox)
		{
			if (selected)
			{
				this.selected.Add(item);
			}
			else
			{
				this.selected.Remove(item);
			}
		}
		selectionEvent?.Invoke(item, selected, type);
	}

	internal override void SortItems(SelectSortType sortType)
	{
		if (sortType == SelectSortType.Alphabetical)
		{
			Array.Sort(items, (T x, T y) => string.Compare(getName(x), getName(y), StringComparison.Ordinal));
			return;
		}
		Array.Sort(items, delegate(T x, T y)
		{
			bool num = selected.Contains(x);
			bool flag = selected.Contains(y);
			int num2 = (num ? 1 : 0);
			int num3 = (flag ? 1 : 0) - num2;
			return (num3 == 0) ? string.Compare(getName(x), getName(y), StringComparison.Ordinal) : num3;
		});
	}
}
