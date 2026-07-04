using System;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class ArrayDrawerUtils
{
	internal static ItemMember[] CreateItemMembers(Type drawerType, Type itemType)
	{
		Type genericDrawerType = drawerType.MakeGenericType(itemType);
		MemberPath[] typeMembers = ItemUtils.GetTypeMembers(itemType);
		ItemMember[] array = new ItemMember[typeMembers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			MemberPath memberPath = typeMembers[i];
			MemberDrawer drawer = MemberDrawer.Create(memberPath);
			Delegate getter = CreateItemMemberGetter(genericDrawerType, memberPath.memberType);
			Delegate setter = CreateItemMemberSetter(genericDrawerType, memberPath.memberType);
			array[i] = new ItemMember(drawer, getter, setter, null);
		}
		return array;
	}

	private static Delegate CreateItemMemberGetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("GetArrayItemMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(GetItemMember<>).MakeGenericType(memberType), method);
	}

	private static Delegate CreateItemMemberSetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("SetArrayItemMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(SetItemMember<>).MakeGenericType(memberType), method);
	}
}
