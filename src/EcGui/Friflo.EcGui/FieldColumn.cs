using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class FieldColumn : Column
{
	internal readonly MemberDrawer memberDrawer;

	private readonly MemberPath[] sortableFields;

	private MemberPath? sortBy;

	private readonly string name;

	private readonly string tooltip;

	internal override string Name => name;

	internal override string Tooltip => tooltip;

	internal override SchemaType SchemaType => memberDrawer.componentType;

	internal override MemberPath MemberPath => memberDrawer.member;

	internal override bool Sortable => sortableFields.Length != 0;

	internal ComponentType ComponentType => memberDrawer.componentType;

	public override string ToString()
	{
		return memberDrawer.ToString();
	}

	internal FieldColumn(MemberDrawer drawer)
	{
		memberDrawer = drawer;
		name = drawer.member.path;
		if (name.IndexOf('.') == -1 && MemberUtils.IsSingleRowComponent(drawer.componentType, out MemberPath _))
		{
			name = drawer.componentType.Name;
		}
		tooltip = memberDrawer.member.componentType.Name + " " + memberDrawer.member.path;
		sortableFields = GetSortableFields(drawer.member, drawer.typeDrawer.SortFields);
		if (sortableFields.Length != 0)
		{
			sortBy = sortableFields[0];
		}
	}

	internal override int GetDefaultWidth(bool expanded)
	{
		TypeDrawer typeDrawer = memberDrawer.typeDrawer;
		if (expanded && typeDrawer is IExpandable expandable)
		{
			return expandable.ExpandableWidth;
		}
		return typeDrawer.DefaultWidth;
	}

	internal MemberPath? GetSortBy()
	{
		return sortBy;
	}

	internal override ItemFlags DrawCell(DrawCell drawCell)
	{
		if (drawCell.Entity.Archetype.ComponentTypes.Contains(memberDrawer.componentType))
		{
			DrawValue drawValue = new DrawValue(drawCell.context, in memberDrawer, drawCell.drawValueFlags);
			return memberDrawer.typeDrawer.DrawValue(in drawValue);
		}
		return ItemFlags.None;
	}

	internal override void Insert(Entity entity)
	{
		if (!entity.Archetype.ComponentTypes.Contains(memberDrawer.componentType))
		{
			EntityUtils.AddEntityComponent(entity, memberDrawer.componentType);
		}
	}

	internal override void Remove(Entity entity)
	{
		EntityUtils.RemoveEntityComponent(entity, memberDrawer.componentType);
	}

	internal override void ContextMenu(ContextMenu menu)
	{
		Entity entity = menu.entity;
		if (!entity.IsNull)
		{
			bool flag = entity.Archetype.ComponentTypes.Contains(memberDrawer.componentType);
			if (ImGui.MenuItem("Cut", "Ctrl + X", selected: false, flag))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Cut, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			if (ImGui.MenuItem("Copy", "Ctrl + C", selected: false, flag))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Copy, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			if (ImGui.MenuItem("Paste", "Ctrl + V"))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Paste, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			if (ImGui.MenuItem("Delete", "Delete", selected: false, flag))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Delete, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			if (ImGui.MenuItem("Add " + memberDrawer.componentType.Name, "Insert", selected: false, !flag))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Insert, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			ImGui.Separator();
		}
		if (sortableFields.Length > 1 && Column.GetSortField(sortableFields, sortBy, out MemberPath result))
		{
			sortBy = result;
		}
		if (sortableFields.Length > 1)
		{
			if (ImGui.BeginMenu("Add history"))
			{
				HistoryMenuTooltip();
				MemberPath[] array = sortableFields;
				foreach (MemberPath memberPath in array)
				{
					MemberPath memberPath2 = Friflo.Engine.ECS.MemberPath.Get(memberPath.componentType.Type, memberPath.path);
					bool enabled = MemberUtils.SupportsHistory(memberPath2.memberType);
					if (ImGui.MenuItem(memberPath.path, enabled))
					{
						AddHistoryColumn(entity, menu.explorer, memberPath2);
					}
				}
				ImGui.EndMenu();
			}
		}
		else
		{
			bool enabled2 = MemberUtils.SupportsHistory(memberDrawer.member.memberType);
			if (ImGui.MenuItem("Add history", enabled2))
			{
				AddHistoryColumn(entity, menu.explorer, memberDrawer.member);
			}
			HistoryMenuTooltip();
		}
		if (ImGui.MenuItem("Remove column"))
		{
			menu.explorer.RemoveColumn(menu.column);
		}
	}

	private static void HistoryMenuTooltip()
	{
		if (FieldHistories.globalSampleIndex <= 0)
		{
			ImGui.SetItemTooltip("History requires EcGui.HistorySnapshot() call in main loop");
		}
	}

	private static void AddHistoryColumn(Entity entity, QueryExplorer explorer, MemberPath memberPath)
	{
		foreach (Column activeColumn in explorer.activeColumns)
		{
			if (activeColumn is HistoryColumn historyColumn && historyColumn.histories.member == memberPath)
			{
				historyColumn.histories.SelectEntity(entity, select: true);
				return;
			}
		}
		explorer.AddHistoryColumn(memberPath).histories.SelectEntity(entity, select: true);
	}

	private static MemberPath[] GetSortableFields(MemberPath member, string[] sortFields)
	{
		MemberPath[] array = new MemberPath[sortFields.Length];
		for (int i = 0; i < sortFields.Length; i++)
		{
			string text = sortFields[i];
			string text2 = member.path;
			if (text != "")
			{
				text2 = ((!(text2 == "")) ? (text2 + "." + text) : text);
			}
			array[i] = Friflo.Engine.ECS.MemberPath.Get(member.declaringType, text2);
		}
		return array;
	}
}
