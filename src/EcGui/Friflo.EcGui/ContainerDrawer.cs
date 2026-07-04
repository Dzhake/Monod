using System;

namespace Friflo.EcGui;

internal abstract class ContainerDrawer : TypeDrawer, IExpandable
{
	private int expandableWidth = -1;

	private int tableWidth = -1;

	public int ExpandableWidth => GetExpandableWidth();

	protected abstract ItemMember[] GetItemMembers();

	protected static ItemMember[] Cap(ItemMember[] itemMembers)
	{
		if (itemMembers.Length < 500)
		{
			return itemMembers;
		}
		ItemMember[] array = new ItemMember[500];
		Array.Copy(itemMembers, array, 500);
		return array;
	}

	private int GetExpandableWidth()
	{
		if (expandableWidth >= 0)
		{
			return expandableWidth;
		}
		return expandableWidth = GetTableWidth() + (int)DrawContainer.CalcIndexWidth(99);
	}

	internal int GetTableWidth()
	{
		if (tableWidth >= 0)
		{
			return tableWidth;
		}
		int num = 60;
		ItemMember[] itemMembers = GetItemMembers();
		foreach (ItemMember itemMember in itemMembers)
		{
			num += itemMember.drawer.typeDrawer.DefaultWidth;
		}
		return tableWidth = num;
	}
}
