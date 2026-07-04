using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class RelationColumn : Column
{
	internal readonly MemberDrawer memberDrawer;

	internal readonly ComponentType relationType;

	internal readonly MemberPath sortBy;

	private readonly string name;

	private readonly string tooltip;

	internal override string Name => name;

	internal override string Tooltip => tooltip;

	internal override SchemaType SchemaType => relationType;

	internal override MemberPath MemberPath => memberDrawer.member;

	internal override bool Sortable => true;

	public override string ToString()
	{
		return memberDrawer.ToString();
	}

	internal RelationColumn(MemberDrawer drawer, ComponentType relationType)
	{
		memberDrawer = drawer;
		this.relationType = relationType;
		name = relationType.Name;
		tooltip = name;
		sortBy = Friflo.Engine.ECS.MemberPath.Get(drawer.member.declaringType, "Length");
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

	internal override ItemFlags DrawCell(DrawCell drawCell)
	{
		if (EntityUtils.GetRelationTypes(drawCell.Entity).Contains(relationType))
		{
			DrawValue drawValue = new DrawValue(drawCell.context, in memberDrawer, drawCell.drawValueFlags);
			return memberDrawer.typeDrawer.DrawValue(in drawValue);
		}
		return ItemFlags.None;
	}

	internal override void Insert(Entity entity)
	{
		if (!EntityUtils.GetRelationTypes(entity).Contains(relationType))
		{
			EntityInspector.AddRelation(entity, relationType);
		}
	}

	internal override void Remove(Entity entity)
	{
		EntityInspector.RemoveRelations(entity, relationType);
	}

	internal override void ContextMenu(ContextMenu menu)
	{
		Entity entity = menu.entity;
		if (!entity.IsNull)
		{
			bool flag = EntityUtils.GetRelationTypes(entity).Contains(relationType);
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
			if (ImGui.MenuItem("Add " + relationType.Name, "Insert", selected: false, !flag))
			{
				menu.explorer.ExecuteCellCommand(CellCommand.Insert, menu.entity, menu.columnIndex, menu.rowIndex);
			}
			ImGui.Separator();
		}
		if (ImGui.MenuItem("Remove column"))
		{
			menu.explorer.RemoveColumn(menu.column);
		}
	}
}
