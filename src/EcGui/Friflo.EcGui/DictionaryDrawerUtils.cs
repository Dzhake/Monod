using System;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class DictionaryDrawerUtils
{
	internal static ItemMember[] CreateItemMembers(Type drawerType, Type mapType, Type keyType, Type valueType)
	{
		Type genericDrawerType = drawerType.MakeGenericType(mapType, keyType, valueType);
		MemberPath[] typeMembers = ItemUtils.GetTypeMembers(valueType);
		ItemMember[] array = new ItemMember[typeMembers.Length + 1];
		MemberPath memberPath = MemberPath.Get(keyType, "");
		MemberDrawer drawer = MemberDrawer.Create(memberPath);
		Delegate getter = CreateMapKeyGetter(genericDrawerType, memberPath.memberType);
		Delegate setter = CreateMapKeySetter(genericDrawerType, memberPath.memberType);
		array[0] = new ItemMember(drawer, getter, setter, "key");
		for (int i = 0; i < typeMembers.Length; i++)
		{
			MemberPath memberPath2 = typeMembers[i];
			MemberDrawer drawer2 = MemberDrawer.Create(memberPath2);
			Delegate getter2 = CreateMapValueGetter(genericDrawerType, memberPath2.memberType);
			Delegate setter2 = CreateMapValueSetter(genericDrawerType, memberPath2.memberType);
			array[i + 1] = new ItemMember(drawer2, getter2, setter2, null);
		}
		return array;
	}

	private static Delegate CreateMapKeyGetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("GetMapKeyMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(GetItemMember<>).MakeGenericType(memberType), method);
	}

	private static Delegate CreateMapKeySetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("SetMapKeyMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(SetItemMember<>).MakeGenericType(memberType), method);
	}

	private static Delegate CreateMapValueGetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("GetMapValueMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(GetItemMember<>).MakeGenericType(memberType), method);
	}

	private static Delegate CreateMapValueSetter(Type genericDrawerType, Type memberType)
	{
		MethodInfo method = genericDrawerType.GetMethod("SetMapValueMember", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(memberType);
		return Delegate.CreateDelegate(typeof(SetItemMember<>).MakeGenericType(memberType), method);
	}
}
