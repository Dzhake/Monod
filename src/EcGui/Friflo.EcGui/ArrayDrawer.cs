using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class ArrayDrawer<TItem> : ContainerDrawer
{
	private ItemMember[]? itemMembers;

	public override string[] SortFields => new string[1] { "Length" };

	internal static TypeDrawer CreateArrayDrawer()
	{
		return new ArrayDrawer<TItem>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(ArrayDrawerUtils.CreateItemMembers(typeof(ArrayDrawer<>), typeof(TItem))));
	}

	internal static bool GetArrayItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		ArrayContainer<TItem> arrayContainer = (ArrayContainer<TItem>)drawValue.container;
		MemberPathGetter<TItem, TMember> memberPathGetter = (MemberPathGetter<TItem, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(arrayContainer.Current);
			return true;
		}
		catch (Exception ex)
		{
			value = default(TMember);
			exception = ex;
			return false;
		}
	}

	internal static bool SetArrayItemMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception)
	{
		ArrayContainer<TItem> arrayContainer = (ArrayContainer<TItem>)drawValue.container;
		MemberPathSetter<TItem, TMember> memberPathSetter = (MemberPathSetter<TItem, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TItem root = arrayContainer.Current;
			memberPathSetter(ref root, value);
			arrayContainer.ChangeArrayItem(root);
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
		if (!drawValue.GetValue<TItem[]>(out TItem[] value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = ArrayContainer<TItem>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
