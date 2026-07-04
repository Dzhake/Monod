using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class ItemUtils
{
	internal static MemberPath[] GetTypeMembers(Type type)
	{
		List<MemberPath> list = new List<MemberPath>();
		TraversePathMembers(MemberPath.Get(type, ""), list, 0);
		return list.ToArray();
	}

	private static void TraversePathMembers(MemberPath memberPath, List<MemberPath> members, int level)
	{
		if (TypeDrawerUtils.GetTypeDrawer(memberPath.memberType, null) is IObjectDrawer)
		{
			if (level < 3)
			{
				MemberPath[] members2 = MemberUtils.GetMembers(memberPath);
				for (int i = 0; i < members2.Length; i++)
				{
					TraversePathMembers(members2[i], members, level + 1);
				}
			}
		}
		else
		{
			members.Add(memberPath);
		}
	}

	internal static bool IsNull<T>(T container)
	{
		if (typeof(T).IsValueType)
		{
			return false;
		}
		return container == null;
	}

	internal static T CreateContainer<T>()
	{
		return TypeUtils.CreateInstance<T>();
	}

	internal static T CreateItem<T>()
	{
		if (typeof(T) == typeof(string))
		{
			return (T)(object)"";
		}
		return TypeUtils.CreateInstance<T>();
	}

	internal static KeyValuePair<TKey, TValue> CreateKeyValuePair<TKey, TValue>()
	{
		TKey key = CreateItem<TKey>();
		TValue value = CreateItem<TValue>();
		return new KeyValuePair<TKey, TValue>(key, value);
	}
}
