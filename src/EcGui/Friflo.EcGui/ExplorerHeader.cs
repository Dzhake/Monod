using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Friflo.EcGui;

internal sealed class ExplorerHeader
{
    private readonly QueryExplorer explorer;

    private string? selectedQueryName = "Hello World Store";

    private QuerySystemBase? selectedSystemQuery;

    private string newQueryName = "";

    private string colorStyle = "";

    private bool kickIt;

    private long kickStart;

    private float kickScale;

    private ConfirmPopup confirmSaveQuery = new ConfirmPopup("new query", "Store");

    private bool enableTooltips;

    private int fps;

    private bool fpsShow;

    private long fpsUpdateTime;

    private bool showAllocation;

    private readonly Dictionary<string, QueryEntry> queries = new Dictionary<string, QueryEntry>();

    private readonly Dictionary<string, SystemGroup> systems = new Dictionary<string, SystemGroup>();

    private readonly Dictionary<SystemGroup, bool> collapsedSystems = new Dictionary<SystemGroup, bool>();

    private readonly Dictionary<QuerySystemBase, List<Column>> querySystemColumns = new Dictionary<QuerySystemBase, List<Column>>();

    private const string HelloWorldStore = "Hello World Store";

    private bool expandQuery;

    private bool expandQueries = true;

    private bool expandQueryComponents;

    private bool expandQueryTags;

    private const string HeapTooltip = "(Diagnostics)\r\nThe sum of heap allocations in bytes\r\nto render the Explorer in a single frame.\r\nExpected to be 0 in common cases.";

    private const string EcGuiLink = "https://github.com/friflo/Friflo.Engine.ECS#ecgui";

    private readonly MultiSelector componentSelector;

    private readonly MultiSelection<ComponentType> componentSelection = new MultiSelection<ComponentType>(EntityInspector.AllComponents, (ComponentType item) => item.Name);

    private readonly MultiSelector tagSelector;

    private readonly MultiSelection<TagType> tagSelection = new MultiSelection<TagType>(EntityInspector.AllTags, (TagType item) => item.Name);

    internal QueryEntry FirstQueryEntry => queries.First().Value;

    internal ExplorerHeader(QueryExplorer explorer)
    {
        var entityStore = new EntityStore();
        entityStore.CreateEntity(new EntityName("Hello friflo EcGui"), default);
        entityStore.CreateEntity(new EntityName("--- add stores, queries or systems ---"), default);
        entityStore.CreateEntity(new EntityName("EcGui.AddExplorerStore()"), default);
        entityStore.CreateEntity(new EntityName("EcGui.AddExplorerQuery()"), default);
        entityStore.CreateEntity(new EntityName("EcGui.AddExplorerSystems()"), default);
        ArchetypeQuery query = entityStore.Query();
        this.explorer = explorer;
        queries.Add("Hello World Store", new QueryEntry(query));
        componentSelector = new MultiSelector(componentSelection);
        tagSelector = new MultiSelector(tagSelection);
    }

    internal void AddStore(string name, EntityStore store)
    {
        ArchetypeQuery query = store.Query().WithDisabled();
        bool num = RemoveDemoQuery();
        queries[name] = new QueryEntry(query);
        if (num)
        {
            ChangeQuery(name, null);
        }
    }

    internal void AddSystemQueries(SystemRoot system)
    {
        systems.Add(system.Name, system);
    }

    internal void AddQuery(string name, ArchetypeQuery query)
    {
        bool num = RemoveDemoQuery();
        queries[name] = new QueryEntry(query);
        if (num)
        {
            ChangeQuery(name, null);
        }
    }

    private bool RemoveDemoQuery()
    {
        if (queries.Remove("Hello World Store"))
        {
            return true;
        }
        return false;
    }

    internal void DrawExplorerMode()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float right = ImGui.GetWindowWidth() - UI.Scl(4f) - style.WindowPadding.X;
        var alignRight = new AlignRight("...", right, fixedWidth: true, UI.Scl(41f));
        var alignRight2 = new AlignRight(" columns ", alignRight.left);
        var alignRight3 = new AlignRight("find", alignRight2.left, fixedWidth: false, ImGui.GetFrameHeightWithSpacing());
        ExplorerMode mode = explorer.mode;
        if (Button(color: (mode == ExplorerMode.Edit) ? new Vector4(0.2f, 0.2f, 1f, 1f) : GlobalColors.explorerModeBg, text: (mode == ExplorerMode.Edit) ? " edit " : "edit"))
        {
            explorer.mode = ExplorerMode.Edit;
            explorer.queryChanged = true;
        }
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Edit mode / Refresh - Ctrl + R  or  F5\r\n- Show snapshot of query result.\r\n- Executes query when clicked.\r\n- Filter / Sort query result when clicked.");
        }
        ImGui.SameLine();
        StringBuilder stringBuilder = TextUtils.Clear();
        stringBuilder.Append(explorer.EntityCount);
        if (explorer.columnFilter && explorer.GetFilterCount() > 0)
        {
            stringBuilder.Append('*');
        }
        byte[] array = TextUtils.AsBytes(stringBuilder);
        ImGui.SetNextItemWidth(UI.Scl(160f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, default(Vector4));
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop | ImGuiItemFlags.NoNav, enabled: true);
        ImGui.InputText("##edit-count", array, (uint)array.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopItemFlag();
        ImGui.PopStyleColor();
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Entity count of snapshot.\n* filters applied to snapshot.");
        }
        ImGui.SameLine();
        if (Button(color: (mode == ExplorerMode.Live) ? new Vector4(1f, 0.3f, 0.3f, 1f) : GlobalColors.explorerModeBg, text: (mode == ExplorerMode.Live) ? " live " : "live"))
        {
            explorer.mode = ExplorerMode.Live;
        }
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Live mode - F6\r\n- Show live query result.\r\n- Executes query every frame.\r\n- Filter / Sort query result every frame.");
        }
        ImGui.SameLine();
        int v = explorer.activeQuery.Count;
        ImGui.SetNextItemWidth(UI.Scl(160f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, default(Vector4));
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop | ImGuiItemFlags.NoNav, enabled: true);
        ImGui.InputInt("##query-count", ref v, 0, 0);
        ImGui.PopItemFlag();
        ImGui.PopStyleColor();
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Entity count of live query.");
        }
        ImGui.SameLine();
        float num = ImGui.GetCursorPosX() - UI.Scl(50f);
        if (fpsShow && ImGui.GetCursorPosX() + UI.Scl(190f) < alignRight3.left)
        {
            ImGuiIOPtr iO = ImGui.GetIO();
            if (Stopwatch.GetElapsedTime(fpsUpdateTime).TotalMilliseconds > 200.0)
            {
                fpsUpdateTime = Stopwatch.GetTimestamp();
                fps = (int)iO.Framerate;
            }
            ImGui.SameLine();
            ImGui.Text("fps");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(UI.Scl(100f));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, default(Vector4));
            ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop | ImGuiItemFlags.NoNav, enabled: true);
            ImGui.InputInt("##fps", ref fps, 0, 0);
            ImGui.PopItemFlag();
            ImGui.PopStyleColor();
            if (enableTooltips && ImGui.BeginItemTooltip())
            {
                ImGui.Text("Current framerate in frames per second.");
                ImGui.EndTooltip();
            }
        }
        ImGui.SameLine();
        if (showAllocation && ImGui.GetCursorPosX() + UI.Scl(190f) < alignRight3.left)
        {
            int v2 = (int)explorer.allocations;
            ImGui.SameLine();
            ImGui.Text("heap");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(UI.Scl(100f));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, default(Vector4));
            ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop | ImGuiItemFlags.NoNav, enabled: true);
            ImGui.InputInt("##heap", ref v2, 0, 0);
            ImGui.PopItemFlag();
            ImGui.PopStyleColor();
            if (enableTooltips && ImGui.BeginItemTooltip())
            {
                ImGui.Text("(Diagnostics)\r\nThe sum of heap allocations in bytes\r\nto render the Explorer in a single frame.\r\nExpected to be 0 in common cases.");
                ImGui.EndTooltip();
            }
        }
        ImGui.SameLine();
        ImGui.Text("   ");
        if (alignRight3.left > num)
        {
            bool v3 = explorer.columnFilter;
            alignRight3.SameLine();
            if (ImGui.Checkbox(alignRight3.label, ref v3))
            {
                explorer.SetColumnFilter(v3);
            }
            if (enableTooltips)
            {
                ImGui.SetItemTooltip("Column filter - Ctrl + F\r\n- Filter numbers or coordinates by range.   < 10\r\n- Filter strings using * operator.   abc*\r\n- Use & or | to connect filters.   X > 100 & X < 200\r\n- Filter a set of values.   1, 2, 10, 100\r\n- Filter component existence.   * or !");
            }
        }
        if (alignRight2.left > num)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.addBg);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
            alignRight2.SameLine();
            if (ImGui.Button(alignRight2.label))
            {
                explorer.OpenFieldSelector();
            }
            ImGui.PopStyleColor(2);
            explorer.DrawFieldSelector();
        }
        EcUtils.ID.PushID(1);
        if (alignRight.left > num)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.frameBg);
            alignRight.SameLine();
            bool num2 = ImGui.Button(alignRight.label, new Vector2(alignRight.width, ImGui.GetFrameHeight()));
            ImGui.PopStyleColor(1);
            if (num2)
            {
                ImGui.OpenPopup("settings");
            }
        }
        if (ImGui.BeginPopup("settings", ImGuiWindowFlags.None))
        {
            DrawSettings();
            ImGui.EndPopup();
        }
        EcUtils.ID.PopID();
    }

    private void DrawSettings()
    {
        float offset_from_start_x = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.X;
        ImGui.Checkbox("Framerate", ref fpsShow);
        ImGui.SetItemTooltip("Current framerate in frames per second.");
        ImGui.Checkbox("Heap allocation", ref showAllocation);
        ImGui.SetItemTooltip("(Diagnostics)\r\nThe sum of heap allocations in bytes\r\nto render the Explorer in a single frame.\r\nExpected to be 0 in common cases.");
        ImGui.Checkbox("Tooltips", ref enableTooltips);
        ImGui.SetItemTooltip("Show tooltip for controls in the Explorer header.");
        ImGui.Checkbox("Embed expansions", ref explorer.embedExpansions);
        ImGui.SetItemTooltip("(Experimental feature)\r\nDisplay expansions in table cells.\r\nE.g. nested tables of collections instead their length.");
        ImGui.NewLine();
        ImGui.SameLine(offset_from_start_x);
        ImGui.Text("Freeze columns");
        ImGui.SetItemTooltip("Freezed columns are always visible\r\nwhen table is scrolled horizontally.");
        ImGui.SetNextItemWidth(UI.Scl(200f));
        if (ImGui.InputInt("##Freeze", ref explorer.freezeColumns, 1))
        {
            explorer.freezeColumns = Math.Max(0, explorer.freezeColumns);
        }
        ImGui.Separator();
        ImGui.NewLine();
        ImGui.SameLine(offset_from_start_x);
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.windowBg);
        if (ImGui.Button("Style Editor"))
        {
            QueryExplorer.showStyleEditor = true;
        }
        ImGui.PopStyleColor();
        ImGui.Checkbox("Dear ImGui Demo", ref QueryExplorer.showImGuiDemoWindow);
        ImGui.SetItemTooltip("Showcase Dockspace\nat:  Examples > Dockspace");
        ImGui.Separator();
        ImGui.NewLine();
        ImGui.SameLine(offset_from_start_x);
        if (ImGui.BeginMenu("Color Style"))
        {
            if (ImGui.RadioButton("Light", colorStyle == "Light"))
            {
                ImGui.StyleColorsLight();
                SetTheme("Light");
            }
            if (ImGui.RadioButton("Dark", colorStyle == "Dark"))
            {
                ImGui.StyleColorsDark();
                SetTheme("Dark");
            }
            if (ImGui.RadioButton("Classic", colorStyle == "Classic"))
            {
                ImGui.StyleColorsClassic();
                SetTheme("Classic");
            }
            ImGui.EndMenu();
        }
        ImGui.NewLine();
        ImGui.SameLine(offset_from_start_x);
        ImGui.Text("Font Scale");
        ImGui.SetItemTooltip("Font is rendered pixel-perfect if Scale == 1.\r\nScale != 1 result in blurry font rendering.");
        ImGui.SetNextItemWidth(UI.Scl(350f));
        ImGuiIOPtr iO = ImGui.GetIO();
        float v = ImGui.GetStyle().FontScaleMain;
        if (ImGui.DragFloat("##font-scale", ref v, 0.002f, 0.25f, 4f))
        {
            iO = ImGui.GetIO();
            ImGui.GetStyle().FontScaleMain = v;
        }
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.windowBg);
        if (ImGui.Button("Kick it! :)", new Vector2(UI.Scl(350f), ImGui.GetFrameHeight())))
        {
            kickStart = Stopwatch.GetTimestamp();
            if (!kickIt)
            {
                kickIt = true;
                iO = ImGui.GetIO();
                kickScale = ImGui.GetStyle().FontScaleMain;
            }
        }
        ImGui.PopStyleColor();
        if (kickIt)
        {
            float num = (float)Stopwatch.GetElapsedTime(kickStart).TotalSeconds;
            iO = ImGui.GetIO();
            ref float fontGlobalScale = ref ImGui.GetStyle().FontScaleMain;
            if (num > 1f)
            {
                kickIt = false;
                fontGlobalScale = kickScale;
            }
            else
            {
                fontGlobalScale = kickScale + (0.2f * (1f - num) * MathF.Sin(num * 50f));
            }
        }
        ImGui.Separator();
        ImGui.Text("About ");
        ImGui.SameLine();
        ImGui.TextLinkOpenURL("friflo EcGui", "https://github.com/friflo/Friflo.Engine.ECS#ecgui");
    }

    private void SetTheme(string theme)
    {
        colorStyle = theme;
        EcGui.Setup.SetDefaultStyles();
        GlobalColors.UpdateStyles(forceUpdate: true);
    }

    private void ChangeQuery(string? name, QuerySystemBase? querySystem)
    {
        if (selectedQueryName == name && selectedSystemQuery == querySystem)
        {
            return;
        }
        selectedQueryName = name;
        selectedSystemQuery = querySystem;
        explorer.queryChanged = true;
        ArchetypeQuery archetypeQuery;
        List<Column> value;
        if (name != null)
        {
            QueryEntry queryEntry = queries[name];
            archetypeQuery = queryEntry.query;
            value = queryEntry.columns;
        }
        else
        {
            name = querySystem.Name;
            archetypeQuery = querySystem.Queries[0];
            if (!querySystemColumns.TryGetValue(querySystem, out value))
            {
                var list = new List<Column>();
                CollectionsMarshal.SetCount(list, 1);
                Span<Column> span = CollectionsMarshal.AsSpan(list);
                int num = 0;
                span[num] = new IdColumn();
                num++;
                value = list;
                QueryEntry.AddQueryComponentsColumns(value, archetypeQuery);
                querySystemColumns.Add(querySystem, value);
            }
        }
        explorer.activeQueryName = name;
        explorer.activeQuery = archetypeQuery;
        explorer.activeColumns = value;
    }

    internal void DrawQuerySelector()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float right = ImGui.GetWindowWidth() - UI.Scl(4f) - style.WindowPadding.X;
        var alignRight = new AlignRight("friflo EcGui", right);
        ImGui.Dummy(new Vector2(0f, UI.Scl(2f)));
        float y = ImGui.GetStyle().FramePadding.Y;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + y);
        ImGui.SetNextItemOpen(expandQuery);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        expandQuery = ImGui.TreeNodeEx("query  ", ImGuiTreeNodeFlags.SpanLabelWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns);
        ImGui.PopStyleColor();
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("ECS query with component and tag filters");
        }
        ImGui.SameLine();
        float cursorPosX = ImGui.GetCursorPosX();
        ImGui.SetNextItemWidth(Math.Min(Math.Max(UI.Scl(250f), alignRight.left - cursorPosX - UI.Scl(120f)), UI.Scl(800f)));
        string preview_value = selectedQueryName ?? selectedSystemQuery.Name;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - y);
        bool num = ImGui.BeginCombo("##query", preview_value, ImGuiComboFlags.HeightLarge | ImGuiComboFlags.NoArrowButton);
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Active query\r\n    added by\r\nEcGui.AddExplorerStore()\r\nEcGui.AddExplorerQuery()\r\nEcGui.AddExplorerSystems()");
        }
        if (num)
        {
            ImGui.SetNextItemOpen(expandQueries);
            expandQueries = ImGui.TreeNodeEx("Queries", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns);
            string key;
            if (expandQueries)
            {
                int num2 = 0;
                foreach (KeyValuePair<string, QueryEntry> query in queries)
                {
                    query.Deconstruct(out key, out QueryEntry _);
                    string text = key;
                    EcUtils.ID.PushID(num2++);
                    bool selected = text == selectedQueryName;
                    ImGui.Selectable(text, selected);
                    if (ImGui.IsItemFocused())
                    {
                        ChangeQuery(text, null);
                    }
                    EcUtils.ID.PopID();
                }
                ImGui.TreePop();
            }
            if (systems.Count > 0)
            {
                foreach (KeyValuePair<string, SystemGroup> system in systems)
                {
                    system.Deconstruct(out key, out SystemGroup value2);
                    SystemGroup systemGroup = value2;
                    AddSystemGroupQueries(systemGroup);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - y);
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.frameBg);
        if (ImGui.Button("..."))
        {
            ImGui.OpenPopup("query_more");
        }
        ImGui.PopStyleColor();
        if (selectedQueryName != null && queries.TryGetValue(selectedQueryName, out QueryEntry value3) && value3.saved)
        {
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - y);
            if (ImGui.Button("delete"))
            {
                queries.Remove(selectedQueryName);
            }
            if (enableTooltips)
            {
                ImGui.SetItemTooltip("Delete current query");
            }
        }
        DrawQueryPopup();
        ImGui.SameLine();
        if (ImGui.GetCursorPosX() < alignRight.left)
        {
            alignRight.SameLine();
            ImGui.PushStyleColor(ImGuiCol.TextLink, GlobalColors.queryText);
            ImGui.TextLinkOpenURL(alignRight.label, "https://github.com/friflo/Friflo.Engine.ECS#ecgui");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.NewLine();
        }
        if (expandQuery)
        {
            DrawQueryFilter();
            ImGui.TreePop();
        }
    }

    private void DrawQueryPopup()
    {
        bool flag = false;
        if (ImGui.BeginPopup("query_more", ImGuiWindowFlags.None))
        {
            flag = ImGui.MenuItem("Store copy of query");
            ImGui.Separator();
            if (ImGui.MenuItem("Query as C# to clipboard"))
            {
                ImGui.SetClipboardText(QueryUtils.QueryAsCode(explorer.activeQuery));
            }
            if (ImGui.MenuItem("Table as CSV to clipboard"))
            {
                ImGui.SetClipboardText(CsvMemberFormat.Instance.WriteAsString(explorer.activeColumns.ToArray(), explorer.Entities));
            }
            if (ImGui.MenuItem("Table as Markdown to clipboard"))
            {
                ImGui.SetClipboardText(MdMemberFormat.Instance.WriteAsString(explorer.activeColumns.ToArray(), explorer.Entities));
            }
            ImGui.EndPopup();
        }
        if (flag)
        {
            newQueryName = NewQueryName(selectedQueryName ?? selectedSystemQuery.Name);
            confirmSaveQuery.OpenPopup();
            ImGui.SetKeyboardFocusHere();
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(600f), UI.Scl(180f)));
        if (ImGui.BeginPopup(confirmSaveQuery.title, ImGuiWindowFlags.None))
        {
            if (confirmSaveQuery.Draw(ref newQueryName))
            {
                ArchetypeQuery query = QueryUtils.DuplicateQuery(explorer.activeQuery);
                queries[newQueryName] = new QueryEntry(query, explorer.activeColumns);
                ChangeQuery(newQueryName, null);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawQueryFilter()
    {
        ArchetypeQuery activeQuery = explorer.activeQuery;
        QueryFilter filter = activeQuery.Filter;
        QueryFilter.FilterCondition condition = filter.Condition;
        ImGui.SetNextItemOpen(expandQueryComponents);
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns;
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        expandQueryComponents = ImGui.TreeNodeEx("components", flags);
        ImGui.PopStyleColor();
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Defines which components are included in\n(excluded from) the query result.");
        }
        int num = condition.AllComponents.Count + condition.AnyComponents.Count + condition.WithoutAllComponents.Count + condition.WithoutAnyComponents.Count;
        if (activeQuery.ComponentTypes.Count > 0 || num > 0)
        {
            StringBuilder stringBuilder = TextUtils.Clear();
            if (activeQuery.ComponentTypes.Count > 0)
            {
                stringBuilder.Append(" <");
                QueryUtils.AppendComponentTypes(stringBuilder, activeQuery.ComponentTypes);
                stringBuilder.Append('>');
            }
            if (num > 0)
            {
                StringBuilder stringBuilder2 = stringBuilder;
                StringBuilder stringBuilder3 = stringBuilder2;
                var handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
                handler.AppendLiteral(" +");
                handler.AppendFormatted(num);
                stringBuilder3.Append(ref handler);
            }
            ImGui.SameLine();
            ImGui.Text(TextUtils.AsSpan(stringBuilder));
        }
        if (expandQueryComponents)
        {
            if (DrawComponentFilter(1, "with all", condition.AllComponents, out ComponentTypes result))
            {
                filter.AllComponents(in result);
            }
            if (DrawComponentFilter(2, "with any", condition.AnyComponents, out result))
            {
                filter.AnyComponents(in result);
            }
            if (DrawComponentFilter(3, "without all", condition.WithoutAllComponents, out result))
            {
                filter.WithoutAllComponents(in result);
            }
            if (DrawComponentFilter(4, "without any", condition.WithoutAnyComponents, out result))
            {
                filter.WithoutAnyComponents(in result);
            }
            ImGui.TreePop();
        }
        ImGui.SetNextItemOpen(expandQueryTags);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        expandQueryTags = ImGui.TreeNodeEx("tags", flags);
        ImGui.PopStyleColor();
        if (enableTooltips)
        {
            ImGui.SetItemTooltip("Defines which tags are included in\n(excluded from) the query result.");
        }
        int num2 = condition.AllTags.Count + condition.AnyTags.Count + condition.WithoutAllTags.Count + condition.WithoutAnyTags.Count;
        if (num2 > 0)
        {
            StringBuilder stringBuilder2 = TextUtils.Clear();
            StringBuilder stringBuilder4 = stringBuilder2;
            var handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
            handler.AppendLiteral(" +");
            handler.AppendFormatted(num2);
            StringBuilder sb = stringBuilder4.Append(ref handler);
            ImGui.SameLine();
            ImGui.Text(TextUtils.AsSpan(sb));
        }
        if (expandQueryTags)
        {
            if (DrawTagFilter(5, "with all", condition.AllTags, out Tags value))
            {
                filter.AllTags(in value);
            }
            if (DrawTagFilter(6, "with any", condition.AnyTags, out value))
            {
                filter.AnyTags(in value);
            }
            if (DrawTagFilter(7, "without all", condition.WithoutAllTags, out value))
            {
                filter.WithoutAllTags(in value);
            }
            if (DrawTagFilter(8, "without any", condition.WithoutAnyTags, out value))
            {
                filter.WithoutAnyTags(in value);
            }
            ImGui.TreePop();
        }
    }

    private void AddSystemGroupQueries(SystemGroup group)
    {
        collapsedSystems.TryGetValue(group, out bool value);
        bool nextItemOpen = !value;
        ImGui.SetNextItemOpen(nextItemOpen);
        nextItemOpen = ImGui.TreeNodeEx(group.Name, ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns);
        collapsedSystems[group] = !nextItemOpen;
        if (!nextItemOpen)
        {
            return;
        }
        foreach (BaseSystem item in group)
        {
            EcUtils.ID.PushID(item.Id);
            if (!(item is SystemGroup systemGroup))
            {
                if (item is QuerySystemBase querySystemBase)
                {
                    bool selected = selectedSystemQuery == querySystemBase;
                    ImGui.Selectable(querySystemBase.Name, selected);
                    if (ImGui.IsItemFocused())
                    {
                        ChangeQuery(null, querySystemBase);
                    }
                }
            }
            else
            {
                AddSystemGroupQueries(systemGroup);
            }
            EcUtils.ID.PopID();
        }
        ImGui.TreePop();
    }

    private string NewQueryName(string name)
    {
        int num = name.LastIndexOf('-') + 1;
        int num2 = 1;
        if (num == 0)
        {
            name += "-";
        }
        else if (num < name.Length)
        {
            if (int.TryParse(name.Substring(num), out int result))
            {
                num2 = result + 1;
                name = name.Substring(0, num);
            }
            else
            {
                name += "-";
            }
        }
        string text;
        while (true)
        {
            text = name + num2;
            if (!queries.ContainsKey(text))
            {
                break;
            }
            num2++;
        }
        return text;
    }

    private bool DrawComponentFilter(int id, string filter, ComponentTypes types, out ComponentTypes result)
    {
        EcUtils.ID.PushID(id);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        ImGui.Text(filter);
        ImGui.PopStyleColor();
        StringBuilder sb = TextUtils.Clear();
        QueryUtils.AppendComponentTypes(sb, types);
        byte[] array = TextUtils.AsBytes(sb);
        float num = ImGui.GetCursorPosX() + UI.Scl(200f);
        ImGui.SameLine(num);
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - num - UI.Scl(60f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        ImGui.InputText("##", array, (uint)array.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopItemFlag();
        ImGui.SameLine();
        bool num2 = ImGui.Button("...");
        ImGui.PopStyleVar();
        if (num2)
        {
            componentSelection.selected.Clear();
            foreach (ComponentType item in types)
            {
                componentSelection.selected.Add(item);
            }
            componentSelector.Start();
            ImGui.OpenPopup("component_selection");
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(500f), UI.Scl(800f)));
        bool flag = false;
        if (ImGui.BeginPopup("component_selection", ImGuiWindowFlags.None))
        {
            flag = componentSelector.Draw(filter);
            if (flag)
            {
                if (componentSelection.changeType == SelectionEventType.Label)
                {
                    ImGui.CloseCurrentPopup();
                }
                ComponentType changeItem = componentSelection.ChangeItem;
                if (componentSelection.changeSelected)
                {
                    types.Add(new ComponentTypes(changeItem));
                }
                else
                {
                    types.Remove(new ComponentTypes(changeItem));
                }
                explorer.queryChanged = true;
            }
            ImGui.EndPopup();
        }
        EcUtils.ID.PopID();
        result = types;
        return flag;
    }

    private bool DrawTagFilter(int id, string filter, Tags tags, out Tags value)
    {
        EcUtils.ID.PushID(id);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        ImGui.Text(filter);
        ImGui.PopStyleColor();
        StringBuilder sb = TextUtils.Clear();
        QueryUtils.AppendTags(sb, tags);
        byte[] array = TextUtils.AsBytes(sb);
        float num = ImGui.GetCursorPosX() + UI.Scl(200f);
        ImGui.SameLine(num);
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - num - UI.Scl(60f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        ImGui.InputText("##", array, (uint)array.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopItemFlag();
        ImGui.SameLine();
        bool num2 = ImGui.Button("...");
        ImGui.PopStyleVar();
        if (num2)
        {
            tagSelection.selected.Clear();
            foreach (TagType item in tags)
            {
                tagSelection.selected.Add(item);
            }
            tagSelector.Start();
            ImGui.OpenPopup("tag_selection");
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(500f), UI.Scl(800f)));
        bool flag = false;
        if (ImGui.BeginPopup("tag_selection", ImGuiWindowFlags.None))
        {
            flag = tagSelector.Draw(filter);
            if (flag)
            {
                if (tagSelection.changeType == SelectionEventType.Label)
                {
                    ImGui.CloseCurrentPopup();
                }
                TagType changeItem = tagSelection.ChangeItem;
                if (tagSelection.changeSelected)
                {
                    tags.Add(new Tags(changeItem));
                }
                else
                {
                    tags.Remove(new Tags(changeItem));
                }
                explorer.queryChanged = true;
            }
            ImGui.EndPopup();
        }
        EcUtils.ID.PopID();
        value = tags;
        return flag;
    }

    private static bool Button(string text, Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, color);
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
        bool result = ImGui.Button(text);
        ImGui.PopStyleColor(2);
        return result;
    }
}
