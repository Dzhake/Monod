using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal readonly struct QueryEntry
{
	internal readonly ArchetypeQuery query;

	internal readonly List<Column> columns;

	internal readonly bool saved;

	internal QueryEntry(ArchetypeQuery query)
	{
		saved = false;
		this.query = query;
		List<Column> list = new List<Column>();
		CollectionsMarshal.SetCount(list, 1);
		Span<Column> span = CollectionsMarshal.AsSpan(list);
		int num = 0;
		span[num] = new IdColumn();
		num++;
		columns = list;
		AddQueryComponentsColumns(columns, query);
	}

	internal QueryEntry(ArchetypeQuery query, List<Column> sourceColumns)
	{
		this.query = query;
		List<Column> list = new List<Column>();
		CollectionsMarshal.SetCount(list, 1);
		Span<Column> span = CollectionsMarshal.AsSpan(list);
		int num = 0;
		span[num] = new IdColumn();
		num++;
		columns = list;
		saved = true;
		AppendColumns(columns, sourceColumns);
	}

	internal static void AddQueryComponentsColumns(List<Column> columns, ArchetypeQuery query)
	{
		if (query.ComponentTypes.Count == 0)
		{
			MemberDrawer drawer = MemberDrawer.Create(MemberPath.Get(typeof(EntityName), "value"));
			columns.Add(new FieldColumn(drawer));
			return;
		}
		foreach (ComponentType componentType in query.ComponentTypes)
		{
			if (MemberUtils.IsSingleRowComponent(componentType, out MemberPath memberPath))
			{
				MemberDrawer drawer2 = MemberDrawer.Create(memberPath);
				if (!(drawer2.typeDrawer is IObjectDrawer))
				{
					columns.Add(new FieldColumn(drawer2));
					continue;
				}
			}
			MemberPath[] typeMembers = ItemUtils.GetTypeMembers(componentType.Type);
			for (int i = 0; i < typeMembers.Length; i++)
			{
				MemberDrawer drawer3 = MemberDrawer.Create(typeMembers[i]);
				columns.Add(new FieldColumn(drawer3));
			}
		}
	}

	private static void AppendColumns(List<Column> target, List<Column> sourceColumns)
	{
		foreach (Column sourceColumn in sourceColumns)
		{
			if (!(sourceColumn is FieldColumn fieldColumn))
			{
				if (!(sourceColumn is TagColumn tagColumn))
				{
					if (sourceColumn is RelationColumn relationColumn)
					{
						target.Add(new RelationColumn(relationColumn.memberDrawer, relationColumn.relationType));
					}
				}
				else
				{
					target.Add(new TagColumn(tagColumn.tagType));
				}
			}
			else
			{
				target.Add(new FieldColumn(fieldColumn.memberDrawer));
			}
		}
	}
}
