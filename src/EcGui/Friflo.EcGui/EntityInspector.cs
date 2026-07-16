using Friflo.Engine.ECS;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Friflo.EcGui;

public sealed class EntityInspector
{
    private readonly Dictionary<Type, IInspectorComponent> inspectorComponents = new Dictionary<Type, IInspectorComponent>();

    private readonly QueryExplorer explorer;

    private Entity entity;

    private string idText = "";

    private readonly DrawContext context = new DrawContext(syncTables: false);

    private bool expandTags;

    private bool expandComponents = true;

    private bool expandRelations;

    private SchemaType? highlight;

    private SchemaType? setFocus;

    private int highlightState;

    private long highlightTimeout;

    private long allocation;

    internal string filter = "";

    internal bool filterActive;

    private bool showTooltips;

    private int maxRowCount = 10;

    private bool fixedTabledHeight;

    private bool showAllocation;

    private int labelWidth = 20;

    internal float indentWidth;

    internal int memberDepth;

    private readonly ErrorPopup errorPopup = new ErrorPopup();

    private readonly List<ComponentType> components = new List<ComponentType>();

    private readonly List<ComponentType> relationTypes = new List<ComponentType>();

    private readonly List<TagType> tagTypes = new List<TagType>();

    private readonly MultiSelector addTags;

    private readonly MultiSelection<TagType> tagSelection = new MultiSelection<TagType>(AllTags, (TagType tag) => tag.TagName);

    private readonly MultiSelector addComponents;

    private readonly MultiSelection<ComponentType> componentSelection = new MultiSelection<ComponentType>(AllComponents, (ComponentType type) => type.Name);

    private readonly MultiSelector addRelations;

    private readonly MultiSelection<ComponentType> relationSelection = new MultiSelection<ComponentType>(AllRelations, (ComponentType type) => type.Name);

    internal const float FrameRounding = 5f;

    internal static readonly TagType[] AllTags = EntityStore.GetEntitySchema().TagTypeByType.Values.ToArray();

    internal static readonly ComponentType[] AllComponents = EntityStore.GetEntitySchema().ComponentTypes.ToArray();

    private static readonly ComponentType[] AllRelations = EntityStore.GetEntitySchema().RelationTypes.ToArray();

    internal const float MoreButtonWidth = 41f;

    internal const ImGuiTreeNodeFlags TreeFull = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns;

    internal const ImGuiTreeNodeFlags TreeLabel = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanLabelWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns;

    internal const string LabelPadding = "     ";

    private readonly Dictionary<Type, InspectorRelation> inspectorRelations = new Dictionary<Type, InspectorRelation>();

    private void DrawComponentsSection()
    {
        indentWidth = ImGui.GetStyle().IndentSpacing;
        memberDepth = 0;
        ImGui.SetNextItemOpen(expandComponents);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        expandComponents = ImGui.TreeNodeEx("components", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth);
        ImGui.SameLine();
        ComponentTypes componentTypes = entity.Archetype.ComponentTypes;
        ImGui.Text(TextUtils.IntAsBytes(componentTypes.Count));
        ImGui.PopItemFlag();
        ImGui.PopStyleColor();
        if (AddButton(-2))
        {
            addComponents.Start();
            componentSelection.selected.Clear();
            foreach (ComponentType item in componentTypes)
            {
                componentSelection.selected.Add(item);
            }
            ImGui.OpenPopup("add_components");
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(500f), UI.Scl(800f)));
        if (ImGui.BeginPopup("add_components", ImGuiWindowFlags.None))
        {
            addComponents.Draw("add components");
            ImGui.EndPopup();
        }
        if (expandComponents)
        {
            Unindent();
            DrawComponents();
            Indent();
            ImGui.TreePop();
        }
    }

    private void DrawComponents()
    {
        components.Clear();
        foreach (ComponentType componentType in entity.Archetype.ComponentTypes)
        {
            components.Add(componentType);
        }
        components.Sort();
        foreach (ComponentType component in components)
        {
            if (!inspectorComponents.TryGetValue(component.Type, out IInspectorComponent value))
            {
                value = CreateInspectorComponent(component);
            }
            if (!filterActive || value.HasFilterMatches(filter))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2f);
                var drawNode = new DrawNode
                {
                    context = context,
                    explorer = explorer,
                    inspector = this
                };
                BeginComponent(component);
                EcUtils.ID.PushID(component.StructIndex);
                value.DrawComponentNode(drawNode);
                EcUtils.ID.PopID();
                EndComponent(component);
            }
        }
    }

    private void BeginComponent(ComponentType componentType)
    {
        Vector4 bg;
        if (highlight == componentType)
        {
            float num = (float)(highlightTimeout - Stopwatch.GetTimestamp()) / (float)Stopwatch.Frequency;
            bg = (num * GlobalColors.componentAddBg) + ((1f - num) * GlobalColors.componentBg);
        }
        else
        {
            bg = (componentType == explorer.ActiveColumnType) ? GlobalColors.componentActiveBg : GlobalColors.componentBg;
        }
        if (highlightState == 2 && setFocus == componentType)
        {
            ImGui.SetKeyboardFocusHere();
            setFocus = null;
        }
        DrawRectBg(bg);
    }

    private void EndComponent(ComponentType componentType)
    {
        if (highlightState == 1 && setFocus == componentType)
        {
            ImGui.SetScrollHereY(1f);
        }
    }

    private IInspectorComponent CreateInspectorComponent(ComponentType componentType)
    {
        IInspectorComponent inspectorComponent;
        if (MemberUtils.IsSingleRowComponent(componentType, out MemberPath memberPath))
        {
            var memberDrawer = MemberDrawer.Create(memberPath);
            if (!(memberDrawer.typeDrawer is IObjectDrawer))
            {
                inspectorComponent = new SingleRowComponent(componentType, memberDrawer);
                inspectorComponents.Add(componentType.Type, inspectorComponent);
                return inspectorComponent;
            }
        }
        IMember[] members = CreateMembers(MemberPath.Get(componentType.Type, ""), 0);
        inspectorComponent = new MultiRowComponent(componentType, members);
        inspectorComponents.Add(componentType.Type, inspectorComponent);
        return inspectorComponent;
    }

    private static IMember[] CreateMembers(MemberPath basePath, int level)
    {
        MemberPath[] members = MemberUtils.GetMembers(basePath);
        var list = new List<IMember>();
        foreach (MemberPath memberPath in members)
        {
            var fieldDrawer = MemberDrawer.Create(memberPath);
            if (fieldDrawer.typeDrawer is IObjectDrawer objectDrawer)
            {
                if (level < 3)
                {
                    IMember[] array = CreateMembers(memberPath, level + 1);
                    if (array.Length != 0)
                    {
                        var memberDrawer = MemberDrawer.Create(memberPath);
                        list.Add(new MemberObject(memberDrawer, objectDrawer, array));
                    }
                }
            }
            else
            {
                list.Add(new MemberField(fieldDrawer));
            }
        }
        return list.ToArray();
    }

    public EntityInspector(QueryExplorer queryExplorer)
    {
        explorer = queryExplorer;
        tagSelection.selectionEvent = delegate (TagType item, bool selected, SelectionEventType type)
        {
            if (type == SelectionEventType.Label)
            {
                ImGui.CloseCurrentPopup();
            }
            var tags = new Tags(item);
            if (selected)
            {
                entity.AddTags(in tags);
            }
            else
            {
                entity.RemoveTags(in tags);
            }
            context.Edit();
        };
        addTags = new MultiSelector(tagSelection);
        componentSelection.selectionEvent = delegate (ComponentType item, bool selected, SelectionEventType type)
        {
            if (type == SelectionEventType.Label)
            {
                ImGui.CloseCurrentPopup();
            }
            if (selected)
            {
                if (!entity.Archetype.ComponentTypes.HasAll(new ComponentTypes(item)))
                {
                    EntityUtils.AddEntityComponent(entity, item);
                    context.Edit();
                }
                highlight = item;
                setFocus = item;
                highlightState = 0;
                highlightTimeout = Stopwatch.GetTimestamp() + (2 * Stopwatch.Frequency);
            }
            else
            {
                EntityUtils.RemoveEntityComponent(entity, item);
                context.Edit();
            }
        };
        addComponents = new MultiSelector(componentSelection);
        relationSelection.selectionEvent = delegate (ComponentType item, bool selected, SelectionEventType type)
        {
            if (type == SelectionEventType.Label)
            {
                ImGui.CloseCurrentPopup();
            }
            if (selected)
            {
                if (!EntityUtils.GetRelationTypes(entity).HasAll(new ComponentTypes(item)))
                {
                    AddRelation(entity, item);
                    context.Edit();
                }
                highlight = item;
                setFocus = item;
                highlightState = 0;
                highlightTimeout = Stopwatch.GetTimestamp() + (2 * Stopwatch.Frequency);
            }
            else
            {
                RemoveRelations(entity, item);
                context.Edit();
            }
        };
        addRelations = new MultiSelector(relationSelection);
        explorer.OnSelectedEntityChange += delegate (Entity selectedEntity)
        {
            if (selectedEntity.Id != 0)
            {
                idText = selectedEntity.Id.ToString();
            }
        };
        context.onEdit = delegate
        {
            explorer.mode = ExplorerMode.Edit;
        };
        context.onError = errorPopup.OnError;
    }

    internal void Draw()
    {
        long allocatedBytesForCurrentThread = GC.GetAllocatedBytesForCurrentThread();
        GlobalColors.UpdateStyles(forceUpdate: false);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, GlobalColors.popupBg);
        float val = UI.Scl(150 + (5 * labelWidth));
        val = Math.Min(val, ImGui.GetWindowWidth() - UI.Scl(120f));
        context.rect.left = val;
        context.rect.size.X = ImGui.GetWindowWidth() - val - UI.Scl(45f) - ImGui.GetStyle().ScrollbarSize;
        context.rect.size.Y = ImGui.GetFrameHeight();
        context.showTooltips = showTooltips;
        context.maxRowCount = maxRowCount;
        context.fixedTabledHeight = fixedTabledHeight;
        DrawInternal();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        allocation = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesForCurrentThread;
    }

    private void DrawInternal()
    {
        if (!DrawHeader())
        {
            return;
        }
        ImGui.Indent(UI.Scl(40f));
        if (setFocus != null && ImGui.IsWindowFocused())
        {
            highlightState++;
            if (highlightState == 3)
            {
                setFocus = null;
            }
        }
        if (Stopwatch.GetTimestamp() > highlightTimeout)
        {
            highlight = null;
        }
        ImGui.Indent(UI.Scl(-40f));
        Separator();
        DrawTagsSection();
        Separator();
        DrawComponentsSection();
        Separator();
        DrawRelationsSection();
        ImGui.Text("");
        errorPopup.DrawError();
    }

    private bool DrawHeader()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float right = ImGui.GetWindowWidth() - style.ScrollbarSize;
        var alignRight = new AlignRight("...", right, fixedWidth: true, UI.Scl(41f));
        var alignRight2 = new AlignRight("##filter", alignRight.left, fixedWidth: true, UI.Scl(200f));
        var alignRight3 = new AlignRight("filter", alignRight2.left);
        bool isNull = entity.IsNull;
        ImGui.Text(" Id");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(Math.Min(UI.Scl(210f), alignRight.left - UI.Scl(70f)));
        if (isNull)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
        }
        ImGui.InputText("##id", ref idText, 10u);
        if (isNull)
        {
            ImGui.PopStyleColor();
        }
        int.TryParse(idText, out int result);
        bool result2 = explorer.activeQuery.Store.TryGetEntityById(result, out entity);
        context.entity = entity;
        ImGui.SameLine();
        float cursorPosX = ImGui.GetCursorPosX();
        if (showAllocation && ImGui.GetCursorPosX() + UI.Scl(160f) < alignRight3.left)
        {
            ImGui.SameLine();
            ImGui.Text(" heap");
            ImGui.SameLine();
            ImGui.Text(TextUtils.LongAsBytes(allocation));
        }
        if (cursorPosX < alignRight3.left)
        {
            alignRight3.SameLine();
            ImGui.Text(alignRight3.label);
        }
        if (cursorPosX < alignRight2.left)
        {
            ImGui.SetNextItemWidth(UI.Scl(200f));
            alignRight2.SameLine();
            Vector4 col = filterActive ? GlobalColors.activeFilterBg : GlobalColors.frameBg;
            ImGui.PushStyleColor(ImGuiCol.FrameBg, col);
            ImGui.InputText("##filter", ref filter, 50u);
            ImGui.PopStyleColor();
            filterActive = filter.Length > 0;
        }
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.frameBg);
        alignRight.SameLine();
        bool num = ImGui.Button(alignRight.label, new Vector2(alignRight.width, ImGui.GetFrameHeight()));
        ImGui.PopStyleColor(1);
        if (num)
        {
            ImGui.OpenPopup("entity_more");
        }
        if (ImGui.BeginPopup("entity_more", ImGuiWindowFlags.None))
        {
            DrawSettings(isNull, ref result2);
            ImGui.EndPopup();
        }
        return result2;
    }

    private void DrawSettings(bool isNull, ref bool result)
    {
        if (ImGui.MenuItem("Expand all", !isNull))
        {
            ExpandComponents(expand: true);
        }
        if (ImGui.MenuItem("Collapse all", !isNull))
        {
            ExpandComponents(expand: false);
        }
        ImGui.Separator();
        ImGui.Text("Label width");
        ImGui.SetItemTooltip("Width of component and member names.");
        ImGui.SetNextItemWidth(UI.Scl(600f));
        ImGui.SliderInt("##label-width", ref labelWidth, 0, 100, (string?)null, ImGuiSliderFlags.AlwaysClamp);
        ImGui.Text("Max table rows");
        ImGui.SetItemTooltip("The maximum numbers of table rows\r\nto display arrays and collections.");
        ImGui.SetNextItemWidth(UI.Scl(600f));
        ImGui.SliderInt("##max-rows", ref maxRowCount, 3, 30, (string?)null, ImGuiSliderFlags.AlwaysClamp);
        ImGui.Checkbox("Fixed table height", ref fixedTabledHeight);
        ImGui.SetItemTooltip("The height of tables is fixed to\nMax table rows");
        ImGui.Checkbox("Tooltips", ref showTooltips);
        ImGui.SetItemTooltip("Show type information for\ncomponents and their members.");
        ImGui.Checkbox("Heap allocation", ref showAllocation);
        ImGui.SetItemTooltip("(Diagnostics)\r\nThe sum of heap allocations in bytes\r\nto render the Inspector in a single frame.\r\nExpected to be 0 in common cases.");
        ImGui.Separator();
        if (ImGui.MenuItem("Delete Entity", !isNull))
        {
            context.Edit();
            entity.DeleteEntity();
            result = false;
        }
    }

    private void ExpandComponents(bool expand)
    {
        foreach (ComponentType componentType in entity.Archetype.ComponentTypes)
        {
            if (inspectorComponents.TryGetValue(componentType.Type, out IInspectorComponent value))
            {
                if (value is MultiRowComponent multiRowComponent)
                {
                    multiRowComponent.expandComponent = expand;
                }
                if (value is SingleRowComponent singleRowComponent)
                {
                    singleRowComponent.expandComponent = expand;
                }
            }
        }
    }

    internal void Unindent()
    {
        ImGui.Unindent(indentWidth);
    }

    internal void Indent()
    {
        ImGui.Indent(indentWidth);
    }

    internal static void SetContextRectFullWidth(DrawContext context)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        float x = style.WindowPadding.X;
        context.rect.left = x;
        context.rect.size = new Vector2(ImGui.GetWindowWidth() - x - style.ScrollbarSize, ImGui.GetFrameHeightWithSpacing());
    }

    internal static bool MorePopup(string name, out bool isFocused)
    {
        float num = UI.Scl(41f);
        ImGui.SameLine(ImGui.GetWindowWidth() - num - ImGui.GetStyle().ScrollbarSize);
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1f, 0f, 0f, 0f));
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.textLight);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        bool num2 = ImGui.Button("...", new Vector2(num - 4f, ImGui.GetFrameHeight()));
        isFocused = ImGui.IsItemFocused();
        if (num2)
        {
            ImGui.OpenPopup(name);
        }
        ImGui.PopItemFlag();
        ImGui.PopStyleColor(2);
        return ImGui.BeginPopup(name, ImGuiWindowFlags.None);
    }

    private static bool AddButton(int id)
    {
        EcUtils.ID.PushID(id);
        var size = new Vector2(UI.Scl(100f), ImGui.GetTextLineHeight());
        ImGui.SameLine(ImGui.GetWindowWidth() - size.X - UI.Scl(35f));
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.addBg);
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0f, 0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        bool result = ImGui.Button("add", size);
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
        EcUtils.ID.PopID();
        return result;
    }

    internal static void DrawRectBg(Vector4 bg)
    {
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        float y = ImGui.GetStyle().ItemSpacing.Y;
        Vector2 vector = ImGui.GetWindowPos() + new Vector2(0f, ImGui.GetCursorPos().Y - ImGui.GetScrollY() - (y / 2f));
        float y2 = ImGui.GetFrameHeight() + y;
        Vector2 p_max = vector + new Vector2(ImGui.GetWindowWidth(), y2);
        windowDrawList.AddRectFilled(vector, p_max, ImGui.GetColorU32(bg));
    }

    [Conditional("replaced_by_DrawNode_PushClipRect")]
    internal static void DrawMemberBg(in Vector4 bg, in DrawValue drawValue)
    {
        ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
        float num = UI.Scl(6f);
        float num2 = UI.Scl(2f);
        Vector2 vector = ImGui.GetWindowPos() + ImGui.GetCursorPos() + new Vector2(0f - ImGui.GetScrollX() - num, 0f - ImGui.GetScrollY() + num2);
        float frameHeight = ImGui.GetFrameHeight();
        Vector2 p_max = vector + new Vector2(drawValue.Size.X + num, frameHeight - (2f * num2));
        windowDrawList.AddRectFilled(vector, p_max, ImGui.GetColorU32(bg));
    }

    private static void Separator()
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + UI.Scl(5f));
    }

    internal static void AddRelation(Entity entity, ComponentType relationType)
    {
        object obj = TypeUtils.CreateInstance(relationType.Type);
        typeof(RelationExtensions).GetMethod("AddRelation").MakeGenericMethod(relationType.Type).Invoke(null, new object[2] { entity, obj });
    }

    internal static void RemoveRelations(Entity entity, ComponentType relationType)
    {
        typeof(EntityInspector).GetMethod("ClearRelations", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(relationType.Type, relationType.RelationKeyType).Invoke(null, new object[1] { entity });
    }

    internal static TRelation[] ClearRelations<TRelation, TKey>(Entity entity) where TRelation : struct, IRelation<TKey>
    {
        Relations<TRelation> relations = entity.GetRelations<TRelation>();
        var array = new TRelation[relations.Length];
        int num = 0;
        RelationsEnumerator<TRelation> enumerator = relations.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TRelation current = enumerator.Current;
            array[num++] = current;
        }
        TRelation[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            TRelation val = array2[i];
            entity.RemoveRelation<TRelation, TKey>(val.GetRelationKey());
        }
        return array;
    }

    private InspectorRelation CreateInspectorRelation(ComponentType relationType)
    {
        var inspectorRelation = new InspectorRelation(MemberDrawer.Create(MemberPath.Get(typeof(Relations<>).MakeGenericType(relationType.Type), "")), relationType);
        inspectorRelations.Add(relationType.Type, inspectorRelation);
        return inspectorRelation;
    }

    private void DrawRelationsSection()
    {
        ComponentTypes relations = EntityUtils.GetRelationTypes(entity);
        indentWidth = ImGui.GetStyle().IndentSpacing;
        memberDepth = 0;
        ImGui.SetNextItemOpen(expandRelations);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        expandRelations = ImGui.TreeNodeEx("relations", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth);
        ImGui.SameLine();
        ImGui.Text(TextUtils.IntAsBytes(relations.Count));
        ImGui.PopItemFlag();
        ImGui.PopStyleColor();
        if (AddButton(-3))
        {
            addRelations.Start();
            relationSelection.selected.Clear();
            foreach (ComponentType item in relations)
            {
                relationSelection.selected.Add(item);
            }
            ImGui.OpenPopup("add_relations");
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(500f), UI.Scl(800f)));
        if (ImGui.BeginPopup("add_relations", ImGuiWindowFlags.None))
        {
            addRelations.Draw("add relations");
            ImGui.EndPopup();
        }
        if (expandRelations)
        {
            Unindent();
            DrawRelations(relations);
            Indent();
            ImGui.TreePop();
        }
    }

    private void DrawRelations(ComponentTypes relations)
    {
        relationTypes.Clear();
        foreach (ComponentType item in relations)
        {
            relationTypes.Add(item);
        }
        relationTypes.Sort();
        foreach (ComponentType relationType in relationTypes)
        {
            if (!inspectorRelations.TryGetValue(relationType.Type, out InspectorRelation value))
            {
                value = CreateInspectorRelation(relationType);
            }
            if (!filterActive || value.HasFilterMatches(filter))
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2f);
                var drawNode = new DrawNode
                {
                    context = context,
                    explorer = explorer,
                    inspector = this
                };
                BeginComponent(relationType);
                EcUtils.ID.PushID(relationType.StructIndex);
                value.DrawRelationNode(drawNode);
                EcUtils.ID.PopID();
                EndComponent(relationType);
            }
        }
    }

    private void DrawTagsSection()
    {
        ImGui.SetNextItemOpen(expandTags);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.queryText);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        expandTags = ImGui.TreeNodeEx("tags", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth);
        ImGui.SameLine();
        ImGui.Text(TextUtils.IntAsBytes(entity.Tags.Count));
        ImGui.PopItemFlag();
        ImGui.PopStyleColor();
        if (AddButton(-1))
        {
            addTags.Start();
            tagSelection.selected.Clear();
            foreach (TagType tag in entity.Tags)
            {
                tagSelection.selected.Add(tag);
            }
            ImGui.OpenPopup("add_tags");
        }
        ImGui.SetNextWindowSize(new Vector2(UI.Scl(500f), UI.Scl(800f)));
        if (ImGui.BeginPopup("add_tags", ImGuiWindowFlags.None))
        {
            addTags.Draw("add tags");
            ImGui.EndPopup();
        }
        if (expandTags)
        {
            DrawTags();
            ImGui.TreePop();
        }
    }

    private void DrawTags()
    {
        tagTypes.Clear();
        foreach (TagType tag in entity.Tags)
        {
            tagTypes.Add(tag);
        }
        tagTypes.Sort();
        float num = ImGui.GetWindowWidth() - UI.Scl(50f);
        int num2 = Math.Max((int)(num / UI.Scl(250f)), 1);
        float x = (num / (float)num2) - ImGui.GetStyle().ItemSpacing.X;
        var size = new Vector2(x, ImGui.GetTextLineHeightWithSpacing());
        int num3 = 0;
        int num4 = tagTypes.Count;
        if (filterActive)
        {
            num4 = 0;
            foreach (TagType tagType in tagTypes)
            {
                num4 += tagType.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }
        }
        foreach (TagType tagType2 in tagTypes)
        {
            if (filterActive && !tagType2.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            EcUtils.ID.PushID(tagType2.TagIndex);
            ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
            Vector4 col = (tagType2 == explorer.ActiveColumnType) ? GlobalColors.componentActiveBg : GlobalColors.tagBg;
            ImGui.PushStyleColor(ImGuiCol.Button, col);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
            bool num5 = ImGui.Button(tagType2.TagName, size);
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
            if (num5)
            {
                ImGui.OpenPopup("tag-popup");
            }
            ImGui.PopItemFlag();
            if (++num3 < num4 && num3 % num2 != 0)
            {
                ImGui.SameLine();
            }
            if (ImGui.BeginPopup("tag-popup"))
            {
                if (ImGui.MenuItem("Add column"))
                {
                    explorer.AddTagColumn(tagType2);
                }
                ImGui.Separator();
                if (ImGui.MenuItem("Remove Tag"))
                {
                    entity.RemoveTags(new Tags(tagType2));
                    context.Edit();
                }
                ImGui.EndPopup();
            }
            EcUtils.ID.PopID();
        }
    }
}
