using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class MemberChangedHandlers
{
	private static readonly Dictionary<Type, Delegate> Map = new Dictionary<Type, Delegate>();

	internal static Delegate? Get(in MemberDrawer memberDrawer)
	{
		Map.TryGetValue(memberDrawer.member.declaringType, out Delegate value);
		return value;
	}

	internal static void Add<T>(OnMemberChanged<T> handler)
	{
		Dictionary<Type, Delegate> map = Map;
		Type typeFromHandle = typeof(T);
		if (!map.TryGetValue(typeFromHandle, out var value))
		{
			map.Add(typeFromHandle, handler);
			return;
		}
		OnMemberChanged<T> a = (OnMemberChanged<T>)value;
		a = (OnMemberChanged<T>)Delegate.Combine(a, handler);
		map[typeFromHandle] = a;
	}

	internal static void Remove<T>(OnMemberChanged<T> handler)
	{
		Dictionary<Type, Delegate> map = Map;
		Type typeFromHandle = typeof(T);
		if (map.TryGetValue(typeFromHandle, out var value))
		{
			OnMemberChanged<T> source = (OnMemberChanged<T>)value;
			source = (OnMemberChanged<T>)Delegate.Remove(source, handler);
			if (source == null)
			{
				map.Remove(typeFromHandle);
			}
			else
			{
				map[typeFromHandle] = source;
			}
		}
	}
}
