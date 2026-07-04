using System;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal static class TagSort
{
	private struct TagEntry
	{
		internal int id;

		internal int hasTag;
	}

	private static TagEntry[] _sortTagArray = Array.Empty<TagEntry>();

	private static readonly Comparison<TagEntry> TagAscComparison = (TagEntry e1, TagEntry e2) => e1.hasTag - e2.hasTag;

	private static readonly Comparison<TagEntry> TagDescComparison = (TagEntry e1, TagEntry e2) => e2.hasTag - e1.hasTag;

	internal static void Sort(EntityList entities, TagType tagType, ImGuiSortDirection sortDirection)
	{
		int count = entities.Count;
		if (_sortTagArray.Length < count)
		{
			_sortTagArray = new TagEntry[count];
		}
		Tags tags = new Tags(tagType);
		TagEntry[] sortTagArray = _sortTagArray;
		int num = 0;
		foreach (Entity entity in entities)
		{
			ref TagEntry reference = ref sortTagArray[num++];
			reference.id = entity.Id;
			reference.hasTag = (entity.Tags.HasAll(in tags) ? 1 : 0);
		}
		SortByTag(entities, sortDirection, sortTagArray, count);
	}

	private static void SortByTag(EntityList entities, ImGuiSortDirection direction, TagEntry[] entries, int count)
	{
		new Span<TagEntry>(entries, 0, count).Sort((direction == ImGuiSortDirection.Ascending) ? TagAscComparison : TagDescComparison);
		entities.Clear();
		for (int i = 0; i < count; i++)
		{
			entities.Add(entries[i].id);
		}
	}
}
