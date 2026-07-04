using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class ListDrawer<TList, TItem> : ContainerDrawer where TList : IList<TItem>
{
	private ItemMember[]? itemMembers;

	public override string[] SortFields => new string[1] { "Count" };

	internal static TypeDrawer CreateListDrawer()
	{
		return new ListDrawer<TList, TItem>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(ListDrawerUtils.CreateItemMembers(typeof(ListDrawer<, >), typeof(TList), typeof(TItem))));
	}

	internal static bool GetListItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		ListContainer<TList, TItem> listContainer = (ListContainer<TList, TItem>)drawValue.container;
		MemberPathGetter<TItem, TMember> memberPathGetter = (MemberPathGetter<TItem, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(listContainer.Current);
			return true;
		}
		catch (Exception ex)
		{
			value = default(TMember);
			exception = ex;
			return false;
		}
	}

	internal static bool SetListItemMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception)
	{
		ListContainer<TList, TItem> listContainer = (ListContainer<TList, TItem>)drawValue.container;
		MemberPathSetter<TItem, TMember> memberPathSetter = (MemberPathSetter<TItem, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TItem root = listContainer.Current;
			memberPathSetter(ref root, value);
			listContainer.ChangeListItem(root);
			return true;
		}
		catch (Exception ex)
		{
			exception = ex;
			return false;
		}
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		ItemMember[] array = GetItemMembers();
		if (!drawValue.GetValue<TList>(out TList value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = ListContainer<TList, TItem>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
