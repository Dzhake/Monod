using System.Numerics;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class IdColumn : Column
{
	private static string _dummy = "";

	internal override string Name => "Id";

	internal override SchemaType? SchemaType => null;

	internal override MemberPath? MemberPath => null;

	internal override bool Sortable => true;

	internal override int GetDefaultWidth(bool expanded)
	{
		return 110;
	}

	internal override ItemFlags DrawCell(DrawCell drawCell)
	{
		Entity entity = drawCell.Entity;
		Vector2 cursorPos = ImGui.GetCursorPos();
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop | ImGuiItemFlags.NoNav, enabled: true);
		ImGui.SetNextItemWidth(0f);
		ImGui.InputText("##", ref _dummy, 1u);
		ImGui.SameLine();
		ImGui.PopItemFlag();
		if (drawCell.selected && drawCell.explorer.setKeyboardFocus)
		{
			ImGui.SetKeyboardFocusHere();
			drawCell.explorer.setKeyboardFocus = false;
		}
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
		bool isNull = entity.IsNull;
		if (isNull)
		{
			ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
		}
		ImGui.SetCursorPos(cursorPos + new Vector2(0f, 0f - ImGui.GetStyle().FramePadding.Y));
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, default(Vector2));
		ImGui.Selectable(TextUtils.IntAsBytes(entity.Id), drawCell.selected, ImGuiSelectableFlags.None, drawCell.Size);
		ImGui.PopStyleVar();
		if (isNull)
		{
			ImGui.PopStyleColor();
		}
		ImGui.PopItemFlag();
		return TypeDrawer.Flags();
	}

	internal override void ContextMenu(ContextMenu menu)
	{
		Entity entity = menu.entity;
		bool isNull = entity.IsNull;
		if (ImGui.MenuItem("Cut", "Ctrl + X", selected: false, !isNull))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Cut, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Copy", "Ctrl + C", selected: false, !isNull))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Copy, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Paste", "Ctrl + V"))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Paste, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Delete", "Delete", selected: false, !isNull))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Delete, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		ImGui.Separator();
		if (ImGui.MenuItem("Create entity", "Insert"))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Insert, default(Entity), menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Create empty entity", "Ctrl + Insert"))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.InsertEmpty, default(Entity), menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Duplicate entity", "Ctrl + D", selected: false, !isNull))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Duplicate, menu.entity, menu.columnIndex, menu.rowIndex);
		}
	}
}
