using Friflo.Engine.ECS;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Friflo.EcGui;

internal sealed class ColumnSelector
{
    private string searchMember = "";

    private SelectSortType sortType;

    private ColumnSelectorTab tab;

    private readonly QueryExplorer explorer;

    internal readonly Dictionary<MemberPath, TriState> selection = new Dictionary<MemberPath, TriState>();

    private readonly Dictionary<MemberPath, bool> expandMembers = new Dictionary<MemberPath, bool>();

    private readonly MultiSelector tagSelector;

    private readonly MultiSelection<TagType> tagSelection = new MultiSelection<TagType>(EntityInspector.AllTags, (TagType item) => item.Name);

    private static MemberNode[]? componentMembers;

    private const string Title = "add columns";

    private const ImGuiTreeNodeFlags TreeFlags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.LabelSpanAllColumns;

    private const ImGuiItemFlags ImGuiItemFlags_MixedValue = (ImGuiItemFlags)4096;

    private static MemberNode[] ComponentMembers => componentMembers ??= CreateAllComponentMembers();

    private static MemberNode[] CreateAllComponentMembers()
    {
        var list = new List<MemberNode>();
        foreach (ComponentType value in EntityStore.GetEntitySchema().ComponentTypeByType.Values)
        {
            if (MemberUtils.IsSingleRowComponent(value, out MemberPath memberPath))
            {
                list.Add(new MemberNode(memberPath, Array.Empty<MemberNode>(), value.Name));
                continue;
            }
            var path = MemberPath.Get(value.Type, "");
            MemberNode[] array = CreateChildren(path, 0);
            if (array.Length != 0)
            {
                list.Add(new MemberNode(path, array, value.Name));
            }
        }
        return list.ToArray();
    }

    private static MemberNode[] CreateChildren(MemberPath path, int level)
    {
        MemberPath[] members = MemberUtils.GetMembers(path);
        var list = new List<MemberNode>();
        MemberPath[] array = members;
        foreach (MemberPath memberPath in array)
        {
            if (MemberUtils.GetTypeDrawer(memberPath) is IObjectDrawer)
            {
                if (level < 3)
                {
                    MemberNode[] array2 = CreateChildren(memberPath, level + 1);
                    if (array2.Length != 0)
                    {
                        list.Add(new MemberNode(memberPath, array2, memberPath.name));
                    }
                }
            }
            else
            {
                list.Add(new MemberNode(memberPath, Array.Empty<MemberNode>(), memberPath.name));
            }
        }
        return list.ToArray();
    }

    private void SortItems(SelectSortType sortType)
    {
        this.sortType = sortType;
        MemberNode[] array = ComponentMembers;
        if (sortType == SelectSortType.Alphabetical)
        {
            Array.Sort(array, (MemberNode x, MemberNode y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
            return;
        }
        Array.Sort(array, delegate (MemberNode x, MemberNode y)
        {
            selection.TryGetValue(x.path, out TriState value);
            selection.TryGetValue(y.path, out TriState value2);
            int num = (value != TriState.Unchecked) ? 1 : 0;
            int num2 = ((value2 != TriState.Unchecked) ? 1 : 0) - num;
            return (num2 == 0) ? string.Compare(x.name, y.name, StringComparison.Ordinal) : num2;
        });
    }

    internal ColumnSelector(QueryExplorer explorer)
    {
        this.explorer = explorer;
        tagSelector = new MultiSelector(tagSelection);
    }

    internal void Start()
    {
        UpdateSelections();
        SortItems(sortType);
        tagSelection.selected.Clear();
        foreach (Column activeColumn in explorer.activeColumns)
        {
            if (activeColumn is TagColumn tagColumn)
            {
                tagSelection.selected.Add(tagColumn.tagType);
            }
        }
        tagSelector.Start();
    }

    private void UpdateSelections()
    {
        MemberNode[] array = ComponentMembers;
        foreach (MemberNode node in array)
        {
            TraverseNode(node);
        }
    }

    private void TraverseNode(MemberNode node)
    {
        int num = 0;
        bool flag = false;
        MemberNode[] children = node.children;
        for (int i = 0; i < children.Length; i++)
        {
            MemberNode node2 = children[i];
            TraverseNode(node2);
            selection.TryGetValue(node2.path, out TriState value);
            if (value == TriState.Checked)
            {
                num++;
            }
            if (value != TriState.Unchecked)
            {
                flag = true;
            }
        }
        if (node.children.Length != 0)
        {
            TriState value2 = flag ? TriState.Mixed : TriState.Unchecked;
            if (node.children.Length == num)
            {
                value2 = TriState.Checked;
            }
            selection[node.path] = value2;
        }
    }

    private void SetSelection(MemberNode node, TriState triState)
    {
        selection.TryGetValue(node.path, out TriState value);
        if (triState != value)
        {
            selection[node.path] = triState;
            if (node.children.Length == 0)
            {
                AddRemoveColumn(node, triState);
            }
        }
        MemberNode[] children = node.children;
        foreach (MemberNode node2 in children)
        {
            TraverseNode(node2);
            SetSelection(node2, triState);
        }
    }

    private void AddRemoveColumn(MemberNode node, TriState triState)
    {
        if (triState == TriState.Checked)
        {
            var memberDrawer = MemberDrawer.Create(node.path);
            explorer.AddComponentFieldDrawer(memberDrawer);
        }
        else
        {
            explorer.RemoveComponentFieldDrawer(node.path);
        }
    }

    private static bool TabItemButton(string label, bool active)
    {
        if (active)
        {
            Vector4 col = ImGui.GetStyle().Colors[11];
            ImGui.PushStyleColor(ImGuiCol.Tab, col);
        }
        bool result = ImGui.TabItemButton(label);
        if (active)
        {
            ImGui.PopStyleColor();
        }
        return result;
    }

    internal void Draw()
    {
        float windowWidth = ImGui.GetWindowWidth();
        ImGui.SameLine((windowWidth - ImGui.CalcTextSize("add columns").X) / 2f);
        ImGui.Text("add columns");
        ImGui.Text("");
        ImGui.SameLine(UI.Scl(64f));
        ImGui.Text("Sort");
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        ImGui.SameLine();
        bool active = sortType == SelectSortType.Alphabetical;
        if (ImGui.RadioButton("A-Z", active))
        {
            SortItems(SelectSortType.Alphabetical);
            tagSelector.SortItems(SelectSortType.Alphabetical);
        }
        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        ImGui.Text("   ");
        ImGui.SameLine();
        bool active2 = sortType == SelectSortType.Selected;
        if (ImGui.RadioButton("Selected", active2))
        {
            SortItems(SelectSortType.Selected);
            tagSelector.SortItems(SelectSortType.Selected);
        }
        ImGui.PopItemFlag();
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        ImGui.BeginTabBar("tabs");
        if (TabItemButton("components", tab == ColumnSelectorTab.Components))
        {
            tab = ColumnSelectorTab.Components;
        }
        if (TabItemButton("tags", tab == ColumnSelectorTab.Tags))
        {
            tab = ColumnSelectorTab.Tags;
        }
        ImGui.EndTabBar();
        ImGui.PopItemFlag();
        ImGui.SetNextItemWidth(windowWidth - UI.Scl(20f));
        Vector4 col = (searchMember.Length > 0) ? GlobalColors.activeFilterBg : GlobalColors.frameBg;
        ImGui.PushStyleColor(ImGuiCol.FrameBg, col);
        ImGui.InputText("##search", ref searchMember, 100u, ImGuiInputTextFlags.AutoSelectAll);
        ImGui.PopStyleColor();
        switch (tab)
        {
            case ColumnSelectorTab.Components:
                DrawComponents();
                break;
            case ColumnSelectorTab.Tags:
                DrawTags(windowWidth);
                break;
        }
    }

    private void DrawComponents()
    {
        ImGui.BeginChild("##child", new Vector2(0f, 0f), ImGuiChildFlags.NavFlattened);
        for (int i = 0; i < ComponentMembers.Length; i++)
        {
            MemberNode member = ComponentMembers[i];
            if (member.HasFilterMatches(searchMember))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2f);
                EntityInspector.DrawRectBg(GlobalColors.componentBg);
                EcUtils.ID.PushID(i);
                DrawNode(member);
                EcUtils.ID.PopID();
            }
        }
        ImGui.EndChild();
    }

    private void DrawTags(float width)
    {
        tagSelector.filter = searchMember;
        if (tagSelector.DrawTable(width))
        {
            TagType changeItem = tagSelection.ChangeItem;
            if (tagSelection.changeSelected)
            {
                explorer.activeColumns.Add(new TagColumn(changeItem));
            }
            else
            {
                explorer.RemoveTagColumn(changeItem);
            }
        }
    }

    private void DrawNode(MemberNode member)
    {
        selection.TryGetValue(member.path, out TriState value);
        if (member.children.Length == 0)
        {
            ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
            bool num = ImGui.TreeNodeEx("##leaf", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.LabelSpanAllColumns);
            ImGui.PopItemFlag();
            if (num)
            {
                ImGui.SameLine();
                if (Checkbox(member.name, ref value))
                {
                    SetSelection(member, value);
                    UpdateSelections();
                }
                ImGui.TreePop();
            }
            return;
        }
        expandMembers.TryGetValue(member.path, out bool value2);
        ImGui.SetNextItemOpen(value2);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        bool flag = ImGui.TreeNodeEx("##branch", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.LabelSpanAllColumns);
        ImGui.PopItemFlag();
        if (value2 != flag)
        {
            value2 = expandMembers[member.path] = flag;
        }
        ImGui.SameLine();
        if (Checkbox(member.name, ref value))
        {
            SetSelection(member, value);
            UpdateSelections();
        }
        if (!value2)
        {
            return;
        }
        int num2 = 0;
        MemberNode[] children = member.children;
        for (int i = 0; i < children.Length; i++)
        {
            MemberNode member2 = children[i];
            if (member2.HasFilterMatches(searchMember))
            {
                EcUtils.ID.PushID(num2++);
                DrawNode(member2);
                EcUtils.ID.PopID();
            }
        }
        ImGui.TreePop();
    }

    private static bool Checkbox(string name, ref TriState triState)
    {
        bool num = triState == TriState.Mixed;
        bool v = triState != TriState.Unchecked;
        ImGuiItemFlags imGuiItemFlags = ImGuiItemFlags.NoTabStop;
        if (num)
        {
            imGuiItemFlags |= (ImGuiItemFlags)4096;
        }
        ImGui.PushItemFlag(imGuiItemFlags, enabled: true);
        bool result = ImGui.Checkbox(name, ref v);
        ImGui.PopItemFlag();
        triState = v ? TriState.Checked : TriState.Unchecked;
        return result;
    }
}
