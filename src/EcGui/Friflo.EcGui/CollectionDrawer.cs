using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class CollectionDrawer<TCollection, TItem> : ContainerDrawer where TCollection : ICollection<TItem>
{
	private ItemMember[]? itemMembers;

	public override string[] SortFields => new string[1] { "Count" };

	internal static TypeDrawer CreateCollectionDrawer()
	{
		return new CollectionDrawer<TCollection, TItem>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(ListDrawerUtils.CreateItemMembers(typeof(CollectionDrawer<, >), typeof(TCollection), typeof(TItem))));
	}

	internal static bool GetListItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		CollectionContainer<TCollection, TItem> collectionContainer = (CollectionContainer<TCollection, TItem>)drawValue.container;
		MemberPathGetter<TItem, TMember> memberPathGetter = (MemberPathGetter<TItem, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(collectionContainer.Current);
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
		CollectionContainer<TCollection, TItem> collectionContainer = (CollectionContainer<TCollection, TItem>)drawValue.container;
		MemberPathSetter<TItem, TMember> memberPathSetter = (MemberPathSetter<TItem, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TItem root = collectionContainer.Current;
			memberPathSetter(ref root, value);
			collectionContainer.ChangeCollectionItem(root);
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
		if (!drawValue.GetValue<TCollection>(out TCollection value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = CollectionContainer<TCollection, TItem>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
