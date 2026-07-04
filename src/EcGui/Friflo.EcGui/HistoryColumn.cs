using System;
using System.Numerics;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class HistoryColumn : Column
{
	private readonly string name;

	internal readonly FieldHistories histories;

	private static readonly float[] Buffer = new float[1024];

	internal override string Name => name;

	internal override SchemaType SchemaType => histories.member.componentType;

	internal override MemberPath MemberPath => histories.member;

	internal override bool Sortable => false;

	internal override int GetDefaultWidth(bool expanded)
	{
		return 400;
	}

	public override string ToString()
	{
		return histories.member.ToString();
	}

	internal HistoryColumn(FieldHistories histories, MemberPath member)
	{
		this.histories = histories;
		name = member.componentType.Name + " " + member.path;
	}

	internal override ItemFlags DrawCell(DrawCell drawCell)
	{
		Entity entity = drawCell.Entity;
		if (histories.sampleIndex == 0)
		{
			throw new InvalidOperationException("History requires EcGui.HistorySnapshot() call in main loop");
		}
		int start = 1024 - histories.sampleIndex % 1024;
		if (histories.GetHistory(entity, Buffer, start) < 0)
		{
			return ItemFlags.None;
		}
		Vector2 cursorPos = ImGui.GetCursorPos();
		ImGui.SetNextItemAllowOverlap();
		ImGui.SetCursorPos(cursorPos + new Vector2(0f, 0f));
		ImGui.SetNextItemWidth(drawCell.Size.X);
		ImGui.PlotLines("##plot", ref Buffer[0], Buffer.Length);
		ImGui.SetCursorPos(cursorPos);
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, default(Vector2));
		ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0f, 0f, 0f, 0f));
		ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0f, 0f, 0f, 0f));
		ImGui.Selectable("##field", selected: false, ImGuiSelectableFlags.AllowOverlap, drawCell.Size);
		ImGui.PopStyleVar();
		ImGui.PopStyleColor(2);
		return TypeDrawer.Flags();
	}

	internal override void Insert(Entity entity)
	{
		histories.SelectEntity(entity, select: true);
	}

	internal override void Remove(Entity entity)
	{
		histories.SelectEntity(entity, select: false);
	}

	internal override void ContextMenu(ContextMenu menu)
	{
		bool flag = histories.HasHistory(menu.entity);
		if (ImGui.MenuItem("Add history", "Insert", selected: false, !flag))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Insert, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		if (ImGui.MenuItem("Remove history", "Delete", selected: false, flag))
		{
			menu.explorer.ExecuteCellCommand(CellCommand.Delete, menu.entity, menu.columnIndex, menu.rowIndex);
		}
		ImGui.Checkbox("History for all fields!", ref histories.allFields);
		ImGui.Separator();
		if (ImGui.MenuItem("Remove column"))
		{
			menu.explorer.RemoveColumn(menu.column);
			FieldHistories.ReleaseSubscription(histories);
		}
	}
}
