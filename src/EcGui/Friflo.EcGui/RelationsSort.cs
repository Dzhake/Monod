using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal static class RelationsSort
{
	private static RelationEntry[] _sortRelationsArray = Array.Empty<RelationEntry>();

	private static readonly Dictionary<Type, RelationsSorter> SorterMap = new Dictionary<Type, RelationsSorter>();

	private static readonly Comparison<RelationEntry> TagAscComparison = (RelationEntry e1, RelationEntry e2) => e1.count - e2.count;

	private static readonly Comparison<RelationEntry> TagDescComparison = (RelationEntry e1, RelationEntry e2) => e2.count - e1.count;

	internal static void Sort(EntityList entities, ComponentType relationType, ImGuiSortDirection sortDirection)
	{
		int count = entities.Count;
		if (_sortRelationsArray.Length < count)
		{
			_sortRelationsArray = new RelationEntry[count];
		}
		if (!SorterMap.TryGetValue(relationType.Type, out RelationsSorter value))
		{
			value = (RelationsSorter)Activator.CreateInstance(typeof(RelationsSorter<>).MakeGenericType(relationType.Type));
			SorterMap.Add(relationType.Type, value);
		}
		value.AddRelations(entities, _sortRelationsArray);
		SortByRelationCount(entities, sortDirection, _sortRelationsArray, count);
	}

	private static void SortByRelationCount(EntityList entities, ImGuiSortDirection direction, RelationEntry[] entries, int count)
	{
		new Span<RelationEntry>(entries, 0, count).Sort((direction == ImGuiSortDirection.Ascending) ? TagAscComparison : TagDescComparison);
		entities.Clear();
		for (int i = 0; i < count; i++)
		{
			entities.Add(entries[i].id);
		}
	}
}
