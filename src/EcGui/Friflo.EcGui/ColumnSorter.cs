using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal static class ColumnSorter
{
	internal static void Sort(Column column, EntityList entities, ImGuiSortDirection sortDirection)
	{
		if (column is IdColumn)
		{
			SortOrder sortOrder = ((sortDirection == ImGuiSortDirection.Ascending) ? SortOrder.Ascending : SortOrder.Descending);
			entities.SortByEntityId(sortOrder);
			return;
		}
		if (column is FieldColumn fieldColumn)
		{
			MemberPath sortBy = fieldColumn.GetSortBy();
			if (sortBy != null)
			{
				GenericSort.Sort(entities, sortBy, sortDirection);
			}
			return;
		}
		if (column is TagColumn tagColumn)
		{
			TagSort.Sort(entities, tagColumn.tagType, sortDirection);
		}
		if (column is RelationColumn relationColumn)
		{
			RelationsSort.Sort(entities, relationColumn.relationType, sortDirection);
		}
	}
}
