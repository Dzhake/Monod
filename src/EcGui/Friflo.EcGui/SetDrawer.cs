using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class SetDrawer<TSet, TItem> : ContainerDrawer where TSet : ISet<TItem>
{
	private ItemMember[]? itemMembers;

	public override string[] SortFields => new string[1] { "Count" };

	internal static TypeDrawer CreateSetDrawer()
	{
		return new SetDrawer<TSet, TItem>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(ListDrawerUtils.CreateItemMembers(typeof(SetDrawer<, >), typeof(TSet), typeof(TItem))));
	}

	internal static bool GetListItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		SetContainer<TSet, TItem> setContainer = (SetContainer<TSet, TItem>)drawValue.container;
		MemberPathGetter<TItem, TMember> memberPathGetter = (MemberPathGetter<TItem, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(setContainer.Current);
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
		SetContainer<TSet, TItem> setContainer = (SetContainer<TSet, TItem>)drawValue.container;
		MemberPathSetter<TItem, TMember> memberPathSetter = (MemberPathSetter<TItem, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TItem root = setContainer.Current;
			memberPathSetter(ref root, value);
			setContainer.ChangeSetItem(root);
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
		if (!drawValue.GetValue<TSet>(out TSet value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = SetContainer<TSet, TItem>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
