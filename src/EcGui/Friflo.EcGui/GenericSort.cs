using System;
using System.Collections.Generic;
using System.Reflection;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal static class GenericSort
{
	private static readonly GenericSortArgs SortArgs = new GenericSortArgs();

	private static readonly object[] SortArgsParam = new object[1] { SortArgs };

	private static readonly Dictionary<SortTypeKey, Type> SortTypes = new Dictionary<SortTypeKey, Type>();

	internal static void Sort(EntityList entities, MemberPath sortPath, ImGuiSortDirection sortDirection)
	{
		SortArgs.entities = entities;
		SortArgs.fieldPath = sortPath.path;
		SortArgs.sortOrder = ((sortDirection == ImGuiSortDirection.Ascending) ? SortOrder.Ascending : SortOrder.Descending);
		SortTypeKey key = new SortTypeKey(sortPath.componentType.Type, sortPath.memberType);
		if (!SortTypes.TryGetValue(key, out Type value))
		{
			value = typeof(GenericSorter<, >).MakeGenericType(sortPath.componentType.Type, sortPath.memberType);
			SortTypes.Add(key, value);
		}
		value.GetMethod("Sort", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, SortArgsParam);
	}
}
