using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class TagColumn : Column
{
	internal readonly TagType tagType;

	internal readonly Tags tags;

	internal override string Name => tagType.SymbolName;

	internal override SchemaType SchemaType => tagType;

	internal override MemberPath? MemberPath => null;

	internal override bool Sortable => true;

	internal override int GetDefaultWidth(bool expanded)
	{
		return 100;
	}

	internal TagColumn(TagType tagType)
	{
		this.tagType = tagType;
		tags = new Tags(tagType);
	}

	internal override ItemFlags DrawCell(DrawCell drawCell)
	{
		Entity entity = drawCell.Entity;
		bool v = entity.Tags.HasAny(in tags);
		if (v)
		{
			ImGui.SameLine(ImGui.GetColumnWidth() / 2f - UI.Scl(24f));
			if (ImGui.Checkbox("##field", ref v))
			{
				drawCell.context.Edit();
				entity.RemoveTags(in tags);
			}
		}
		else if (ImGui.Selectable("##field", v))
		{
			drawCell.context.Edit();
			entity.AddTags(in tags);
		}
		return TypeDrawer.Flags();
	}

	internal override void Insert(Entity entity)
	{
		entity.AddTags(in tags);
	}

	internal override void Remove(Entity entity)
	{
		entity.RemoveTags(in tags);
	}

	internal override void ContextMenu(ContextMenu menu)
	{
		Entity entity = menu.entity;
		if (!entity.IsNull)
		{
			bool flag = entity.Tags.HasAll(in tags);
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
			if (!flag && ImGui.MenuItem("Add " + tagType.Name, "Insert"))
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
