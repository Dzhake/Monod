using System;
using System.Numerics;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class MultiSelector
{
	internal string filter = "";

	private readonly MultiSelection selection;

	private bool popupOpened;

	private SelectSortType sortType;

	internal void Start()
	{
		popupOpened = true;
		SortItems(sortType);
	}

	internal MultiSelector(MultiSelection selection)
	{
		this.selection = selection;
	}

	internal bool Draw(string? title)
	{
		float windowWidth = ImGui.GetWindowWidth();
		if (title != null)
		{
			ImGui.SameLine((windowWidth - ImGui.CalcTextSize(title).X) / 2f);
			ImGui.Text(title);
		}
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
		ImGui.Text("Sort");
		ImGui.SameLine();
		bool active = sortType == SelectSortType.Alphabetical;
		if (ImGui.RadioButton("A-Z", active))
		{
			SortItems(SelectSortType.Alphabetical);
		}
		ImGui.SetItemDefaultFocus();
		ImGui.SameLine();
		ImGui.Text("   ");
		ImGui.SameLine();
		bool active2 = sortType == SelectSortType.Selected;
		if (ImGui.RadioButton("Selected", active2))
		{
			SortItems(SelectSortType.Selected);
		}
		ImGui.PopItemFlag();
		ImGui.SetNextItemWidth(windowWidth - UI.Scl(20f));
		Vector4 col = ((filter.Length > 0) ? GlobalColors.activeFilterBg : GlobalColors.frameBg);
		ImGui.PushStyleColor(ImGuiCol.FrameBg, col);
		ImGui.InputText("##filter", ref filter, 100u, ImGuiInputTextFlags.AutoSelectAll);
		ImGui.PopStyleColor();
		return DrawTable(windowWidth);
	}

	internal bool DrawTable(float width)
	{
		selection.changed = false;
		int num = -1;
		ImGui.BeginChild("##ch", new Vector2(0f, 0f), ImGuiChildFlags.NavFlattened);
		if (ImGui.BeginTable("multi_select", 1, ImGuiTableFlags.None))
		{
			ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed, width);
			for (int i = 0; i < selection.Length; i++)
			{
				string name = selection.GetName(i);
				if (name.Contains(filter, StringComparison.OrdinalIgnoreCase))
				{
					EcUtils.ID.PushID(i);
					ImGui.TableNextRow();
					ImGui.TableSetColumnIndex(0);
					bool v = selection.IsSelected(i);
					ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
					if (ImGui.Checkbox("##enabled", ref v))
					{
						selection.Select(i, v, SelectionEventType.Checkbox);
					}
					ImGui.PopItemFlag();
					if (ImGui.IsItemFocused())
					{
						num = i;
					}
					ImGui.SameLine();
					ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
					if (ImGui.Selectable(name))
					{
						selection.Select(i, selected: true, SelectionEventType.Label);
					}
					ImGui.PopItemFlag();
					if (ImGui.IsItemFocused())
					{
						num = i;
					}
					EcUtils.ID.PopID();
				}
			}
		}
		ImGui.EndTable();
		ImGui.EndChild();
		if (!popupOpened && ImGui.IsKeyPressed(ImGuiKey.Enter))
		{
			ImGui.CloseCurrentPopup();
			if (num != -1)
			{
				selection.Select(num, selected: true, SelectionEventType.Enter);
			}
		}
		popupOpened = false;
		return selection.changed;
	}

	internal void SortItems(SelectSortType sortType)
	{
		this.sortType = sortType;
		selection.SortItems(sortType);
	}
}
