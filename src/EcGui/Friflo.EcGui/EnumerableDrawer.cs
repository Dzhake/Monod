using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class EnumerableDrawer<TEnumerable, TItem> : ContainerDrawer where TEnumerable : IEnumerable<TItem>
{
	private ItemMember[]? itemMembers;

	private static string[]? _sortFields;

	public override string[] SortFields => _sortFields ?? (_sortFields = EnumerableDrawerUtils.GetSortFields(typeof(TEnumerable)));

	internal static TypeDrawer CreateEnumerableDrawer()
	{
		return new EnumerableDrawer<TEnumerable, TItem>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(ListDrawerUtils.CreateItemMembers(typeof(EnumerableDrawer<, >), typeof(TEnumerable), typeof(TItem))));
	}

	internal static bool GetListItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		EnumerableContainer<TEnumerable, TItem> enumerableContainer = (EnumerableContainer<TEnumerable, TItem>)drawValue.container;
		MemberPathGetter<TItem, TMember> memberPathGetter = (MemberPathGetter<TItem, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(enumerableContainer.Current);
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
		EnumerableContainer<TEnumerable, TItem> enumerableContainer = (EnumerableContainer<TEnumerable, TItem>)drawValue.container;
		MemberPathSetter<TItem, TMember> memberPathSetter = (MemberPathSetter<TItem, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TItem root = enumerableContainer.Current;
			memberPathSetter(ref root, value);
			enumerableContainer.ChangeEnumerableItem(root);
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
		if (!drawValue.GetValue<TEnumerable>(out TEnumerable value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = EnumerableContainer<TEnumerable, TItem>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
