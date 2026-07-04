using System;
using System.Numerics;
using System.Text;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal static class DrawContainer
{
	private struct RowState
	{
		internal int contextMenuRow;

		internal int focusedRow;

		internal int moreRow;
	}

	private static string _null = "null";

	private const int MinIndexColumnWidth = 53;

	internal const int CommandColumnWidth = 60;

	private const ImGuiTableColumnFlags ColumnFlags = ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoReorder | ImGuiTableColumnFlags.NoHide;

	private const ImGuiTableFlags TableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoBordersInBodyUntilResize | ImGuiTableFlags.SizingStretchSame;

	private const bool ClipTable = true;

	internal static ItemFlags Draw(IContainer container, ItemMember[] itemMembers, in DrawValue drawValue)
	{
		ItemFlags itemFlags = ItemFlags.None;
		if ((drawValue.flags & DrawValueFlags.Value) != 0)
		{
			ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.frameBg);
			if (container.IsNull)
			{
				ImGui.InputText("##field", ref _null, (uint)_null.Length, ImGuiInputTextFlags.ReadOnly);
			}
			else
			{
				int v = container.Count;
				ImGui.InputInt("##field", ref v, 0, 0);
			}
			ImGui.PopStyleColor();
			itemFlags |= TypeDrawer.Flags();
		}
		if ((drawValue.flags & DrawValueFlags.Expansion) != 0)
		{
			itemFlags |= DrawExpansion(container, itemMembers, in drawValue);
		}
		return itemFlags;
	}

	internal static float CalcIndexWidth(int count)
	{
		return Math.Max(ImGui.CalcTextSize(TextUtils.IntAsBytes((int)MathF.Pow(10f, MathF.Floor(MathF.Log10(MathF.Max(count - 1, 1f)))))).X + UI.Scl(12f), UI.Scl(53f));
	}

	private static ItemFlags DrawHeader(IContainer container, ItemMember[] itemMembers, ref int moreRow, bool scrollX, float indexWidth, out bool addItem)
	{
		addItem = false;
		ItemFlags itemFlags = ItemFlags.None;
		int num = itemMembers.Length + 2;
		ImGuiTableColumnFlags flags = (scrollX ? ImGuiTableColumnFlags.WidthFixed : ImGuiTableColumnFlags.None);
		for (int i = 0; i < num; i++)
		{
			if (i == 0)
			{
				ImGui.TableSetupColumn("index", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoReorder | ImGuiTableColumnFlags.NoHide, indexWidth);
			}
			else if (i == num - 1)
			{
				ImGui.TableSetupColumn("commands", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoReorder | ImGuiTableColumnFlags.NoHide, UI.Scl(60f));
			}
			else
			{
				ItemMember obj = itemMembers[i - 1];
				ImGui.TableSetupColumn(init_width_or_weight: obj.drawer.typeDrawer.DefaultWidth, label: obj.name, flags: flags);
			}
		}
		ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X, 0f));
		Vector2 size = new Vector2(UI.Scl(41f), ImGui.GetFrameHeight());
		ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
		for (int j = 0; j < num; j++)
		{
			ImGui.TableSetColumnIndex(j);
			if (j == 0)
			{
				itemFlags |= TypeDrawer.Flags();
				ImGui.SameLine();
				ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
				ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.addBg);
				ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
				addItem = ImGui.Button("+", size);
				if (!addItem && ImGui.IsItemActive() && ImGui.IsKeyPressed(ImGuiKey.Enter))
				{
					addItem = true;
				}
				itemFlags |= TypeDrawer.Flags();
				ImGui.PopStyleColor(2);
				ImGui.PopStyleVar(1);
				if (ImGui.BeginItemTooltip())
				{
					StringBuilder sb = TextUtils.Clear().Append("Add: ");
					TypeUtils.AppendTypeName(sb, container.ItemType);
					ImGui.Text(TextUtils.AsSpan(sb));
					ImGui.EndTooltip();
				}
			}
			else if (j == num - 1)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
				ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
				ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0f, 0f, 0f, 0f));
				ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.textLight);
				if (ImGui.Button("...", size))
				{
					moreRow = -1;
				}
				itemFlags |= TypeDrawer.Flags();
				ImGui.PopItemFlag();
				ImGui.PopStyleColor(2);
				ImGui.PopStyleVar(1);
			}
			else
			{
				ImGui.TableHeader(itemMembers[j - 1].name);
				itemFlags |= TypeDrawer.Flags();
			}
		}
		ImGui.PopStyleVar(1);
		return itemFlags;
	}

	private unsafe static ImGuiListClipperPtr CreateListClipper()
	{
		return new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
	}

	private static ItemFlags DrawExpansion(IContainer container, ItemMember[] itemMembers, in DrawValue drawValue)
	{
		DrawContext context = drawValue.context;
		Rect rect = context.rect;
		ItemFlags itemFlags = ItemFlags.None;
		int columns = itemMembers.Length + 2;
		int count = container.Count;
		bool syncTables = drawValue.context.syncTables;
		ContainerDrawer obj = (ContainerDrawer)drawValue.memberDrawer.typeDrawer;
		ImGuiStylePtr style = ImGui.GetStyle();
		float scrollbarSize = style.ScrollbarSize;
		float num = CalcIndexWidth(container.Count);
		bool flag = UI.Scl(obj.GetTableWidth()) + num + 2f * scrollbarSize >= rect.size.X;
		int maxRowCount = drawValue.context.maxRowCount;
		bool flag2 = flag || count > maxRowCount;
		int num2 = (drawValue.context.fixedTabledHeight ? maxRowCount : Math.Min(count, maxRowCount));
		ImGuiTableFlags flags = (flag2 ? (ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoBordersInBodyUntilResize | ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY) : (ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoBordersInBodyUntilResize | ImGuiTableFlags.SizingStretchSame));
		float x = (flag2 ? rect.size.X : (rect.size.X + scrollbarSize));
		float y = ImGui.GetFrameHeight() + (float)num2 * (ImGui.GetFrameHeight() + 2f * style.CellPadding.Y) + scrollbarSize;
		if (syncTables)
		{
			EcUtils.ID.PopAll();
		}
		if (!ImGui.BeginTable(container.ItemType.Name, columns, flags, new Vector2(x, y)))
		{
			if (syncTables)
			{
				EcUtils.ID.PushRestore();
			}
			return itemFlags;
		}
		if (syncTables)
		{
			EcUtils.ID.PushRestore();
		}
		if (flag2)
		{
			context.rect.left = 0f;
		}
		ImGui.TableSetupScrollFreeze(1, 1);
		context.rect.size.Y = ImGui.GetFrameHeight();
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
		RowState rowState = new RowState
		{
			moreRow = -2,
			focusedRow = -1,
			contextMenuRow = -2
		};
		itemFlags |= DrawHeader(container, itemMembers, ref rowState.moreRow, flag, num, out var addItem);
		if (!container.IsNull)
		{
			container.StartIterator();
			ImGuiListClipperPtr clipper = CreateListClipper();
			clipper.Begin(count, ImGui.GetFrameHeight() + 2f * ImGui.GetStyle().CellPadding.Y);
			itemFlags |= DrawRows(clipper, container, itemMembers, context, ref rowState);
			clipper.Destroy();
		}
		ImGui.PopStyleVar();
		if (syncTables)
		{
			EcUtils.ID.PopAll();
		}
		ImGui.EndTable();
		if (syncTables)
		{
			EcUtils.ID.PushRestore();
		}
		if (count > drawValue.context.maxRowCount)
		{
			DrawClipBorder(rect.size.X);
		}
		if (rowState.contextMenuRow >= -1)
		{
			ImGui.OpenPopup("item-context");
			context.contextItemRow = rowState.contextMenuRow;
		}
		if (rowState.moreRow >= -1)
		{
			ImGui.OpenPopup("item-context");
			context.contextItemRow = rowState.moreRow;
		}
		if (ImGui.BeginPopup("item-context", ImGuiWindowFlags.None))
		{
			ContextMenu(container, context.contextItemRow, in drawValue);
			ImGui.EndPopup();
		}
		if (addItem)
		{
			Command("Add row", container, count, CellCommand.Insert, in drawValue);
		}
		if (rowState.focusedRow >= 0 && !ImGui.IsAnyItemActive())
		{
			if (ImGui.IsKeyPressed(ImGuiKey.Delete))
			{
				Command("Delete row", container, rowState.focusedRow, CellCommand.Delete, in drawValue);
			}
			if (ImGui.IsKeyPressed(ImGuiKey.Insert))
			{
				Command("Insert row", container, rowState.focusedRow, CellCommand.Insert, in drawValue);
			}
		}
		context.rect = rect;
		return itemFlags;
	}

	private static ItemFlags DrawRows(ImGuiListClipperPtr clipper, IContainer container, ItemMember[] itemMembers, DrawContext context, ref RowState rowState)
	{
		ItemFlags itemFlags = ItemFlags.None;
		int num = -1;
		while (clipper.Step())
		{
			int displayStart = clipper.DisplayStart;
			int displayEnd = clipper.DisplayEnd;
			int offset = displayStart - num - 1;
			container.SeekCurrent(offset);
			num = displayStart - 1;
			while (container.MoveNext())
			{
				if (++num >= displayStart)
				{
					if (num >= displayEnd)
					{
						break;
					}
					ImGui.TableNextRow();
					EcUtils.ID.PushID(num);
					itemFlags |= DrawRow(container, itemMembers, num, context, ref rowState);
					EcUtils.ID.PopID();
				}
			}
		}
		return itemFlags;
	}

	private static ItemFlags DrawRow(IContainer container, ItemMember[] itemMembers, int index, DrawContext context, ref RowState rowState)
	{
		int num = itemMembers.Length + 2;
		ItemFlags itemFlags = ItemFlags.None;
		for (int i = 0; i < num; i++)
		{
			ImGui.TableSetColumnIndex(i);
			float columnWidth = ImGui.GetColumnWidth();
			ImGui.SetNextItemWidth(columnWidth);
			EcUtils.ID.PushID(i);
			if (i == 0)
			{
				ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.windowBg);
				ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.textLight);
				ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
				ImGui.InputInt("##idx", ref index, 0, 0);
				ImGui.PopItemFlag();
				ImGui.PopStyleColor(2);
				ItemFlags itemFlags2 = TypeDrawer.Flags();
				if ((itemFlags2 & ItemFlags.ContextMenu) != ItemFlags.None)
				{
					rowState.contextMenuRow = index;
				}
				if ((itemFlags2 & ItemFlags.Focused) != ItemFlags.None)
				{
					rowState.focusedRow = index;
				}
				itemFlags |= itemFlags2;
			}
			else if (i == num - 1)
			{
				if (DrawMoreButton(out var flags))
				{
					rowState.moreRow = index;
				}
				itemFlags |= flags;
			}
			else
			{
				context.rect.size.X = columnWidth;
				DrawValue drawValue = new DrawValue(context, container, itemMembers[i - 1]);
				ItemFlags itemFlags3 = drawValue.memberDrawer.typeDrawer.DrawValue(in drawValue);
				if ((itemFlags3 & ItemFlags.ContextMenu) != ItemFlags.None)
				{
					rowState.contextMenuRow = index;
				}
				if ((itemFlags3 & ItemFlags.Focused) != ItemFlags.None)
				{
					rowState.focusedRow = index;
				}
				itemFlags |= itemFlags3;
			}
			EcUtils.ID.PopID();
		}
		return itemFlags;
	}

	private static bool DrawMoreButton(out ItemFlags flags)
	{
		ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1f, 0f, 0f, 0f));
		ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.textLight);
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
		bool result = ImGui.Button("...");
		flags = TypeDrawer.Flags();
		ImGui.PopStyleVar();
		ImGui.PopItemFlag();
		ImGui.PopStyleColor(2);
		return result;
	}

	private static void DrawClipBorder(float width)
	{
		Vector4 col = ImGui.GetStyle().Colors[46];
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float num = UI.Scl(2f);
		Vector2 vector = ImGui.GetWindowPos() + ImGui.GetCursorPos() + new Vector2(0f - ImGui.GetScrollX(), 0f - ImGui.GetScrollY() - 2f * num);
		Vector2 p_max = vector + new Vector2(width, num);
		windowDrawList.AddRectFilled(vector, p_max, ImGui.GetColorU32(col));
	}

	private static void ContextMenu(IContainer container, int itemRow, in DrawValue drawValue)
	{
		if (itemRow == -1)
		{
			if (ImGui.MenuItem("Add row"))
			{
				Command("Add row", container, container.Count, CellCommand.Insert, in drawValue);
			}
			ImGui.Separator();
			if (ImGui.MenuItem("Clear"))
			{
				Command("Clear", container, -1, CellCommand.Delete, in drawValue);
			}
			return;
		}
		if (ImGui.MenuItem("Delete row"))
		{
			Command("Delete row", container, itemRow, CellCommand.Delete, in drawValue);
		}
		ImGui.Separator();
		if (ImGui.MenuItem("Insert row"))
		{
			Command("Insert row", container, itemRow, CellCommand.Insert, in drawValue);
		}
		if (ImGui.MenuItem("Add row"))
		{
			Command("Add row", container, container.Count, CellCommand.Insert, in drawValue);
		}
	}

	private static void Command(string name, IContainer container, int itemRow, CellCommand command, in DrawValue drawValue)
	{
		try
		{
			switch (command)
			{
			case CellCommand.Delete:
				container.Remove(itemRow);
				drawValue.context.Edit();
				break;
			case CellCommand.Insert:
				container.Add(itemRow);
				drawValue.context.Edit();
				break;
			}
		}
		catch (Exception exception)
		{
			drawValue.context.Error($"'{name}' error - member: {drawValue.memberDrawer.member}", exception);
		}
	}
}
