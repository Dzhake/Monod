using Friflo.Engine.ECS;
using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Friflo.EcGui;

internal sealed class MultiRowComponent : IInspectorComponent
{
    private readonly IMember[] members;

    private readonly ComponentType componentType;

    internal bool expandComponent;

    public override string ToString()
    {
        return componentType.Type.Name;
    }

    internal static string? GetTypeDrawerName(MemberPath memberPath)
    {
        MemberInfo memberInfo = memberPath.memberInfo;
        if (memberInfo is null)
        {
            return null;
        }
        foreach (CustomAttributeData customAttribute in memberInfo.CustomAttributes)
        {
            if (customAttribute.AttributeType == typeof(UiTypeDomainAttribute))
            {
                return (string)customAttribute.ConstructorArguments[0].Value;
            }
        }
        return null;
    }

    internal static bool HideMember(IEnumerable<CustomAttributeData> attributes)
    {
        foreach (CustomAttributeData attribute in attributes)
        {
            if (attribute.AttributeType == typeof(DebuggerBrowsableAttribute))
            {
                return (DebuggerBrowsableState)attribute.ConstructorArguments[0].Value == DebuggerBrowsableState.Never;
            }
            if (attribute.AttributeType == typeof(UiHideAttribute))
            {
                return true;
            }
        }
        return false;
    }

    internal MultiRowComponent(ComponentType componentType, IMember[] members)
    {
        this.componentType = componentType;
        this.members = members;
    }

    public void DrawComponentNode(DrawNode drawNode)
    {
        drawNode.context.multiLine = true;
        ImGui.SetNextItemOpen(expandComponent);
        ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
        ImGuiTreeNodeFlags imGuiTreeNodeFlags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns;
        int num = members.Length;
        if (num == 0)
        {
            imGuiTreeNodeFlags |= ImGuiTreeNodeFlags.Leaf;
        }
        expandComponent = ImGui.TreeNodeEx(componentType.Type.Name, imGuiTreeNodeFlags);
        if (drawNode.ShowTooltips && ImGui.BeginItemTooltip())
        {
            StringBuilder stringBuilder = TextUtils.Clear();
            var handler = new StringBuilder.AppendInterpolatedStringHandler(18, 2, stringBuilder);
            handler.AppendLiteral("members: ");
            handler.AppendFormatted(num);
            handler.AppendLiteral(" - size: ");
            handler.AppendFormatted(componentType.StructSize);
            stringBuilder.Append(ref handler);
            ImGui.Text(TextUtils.AsBytes(stringBuilder));
            ImGui.EndTooltip();
        }
        ImGui.PopItemFlag();
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
        InspectorCommand inspectorCommand = InspectorCommand.None;
        if (EntityInspector.MorePopup("generic_more", out bool isFocused))
        {
            if (ImGui.MenuItem("Add all columns", num > 0))
            {
                IMember[] array = members;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].AddExplorerColumns(drawNode.explorer);
                }
            }
            ImGui.Separator();
            if (ImGui.MenuItem("Remove Component", "Delete"))
            {
                inspectorCommand = InspectorCommand.RemoveComponent;
            }
            ImGui.EndPopup();
        }
        if (isFocused && ImGui.IsKeyPressed(ImGuiKey.Delete))
        {
            inspectorCommand = InspectorCommand.RemoveComponent;
        }
        if (expandComponent)
        {
            drawNode.inspector.Unindent();
            for (int j = 0; j < members.Length; j++)
            {
                IMember member = members[j];
                if (!drawNode.inspector.filterActive || member.HasFilterMatches(drawNode.inspector.filter))
                {
                    EcUtils.ID.PushID(j);
                    member.DrawMemberNode(drawNode);
                    EcUtils.ID.PopID();
                }
            }
            drawNode.inspector.Indent();
            ImGui.TreePop();
        }
        if (inspectorCommand == InspectorCommand.RemoveComponent)
        {
            EntityUtils.RemoveEntityComponent(drawNode.Entity, componentType);
            drawNode.context.Edit();
        }
        ImGui.PopStyleVar();
    }

    public bool HasFilterMatches(string filter)
    {
        if (componentType.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        IMember[] array = members;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].HasFilterMatches(filter))
            {
                return true;
            }
        }
        return false;
    }
}
