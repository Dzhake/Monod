using System;
using System.Text;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class SingleRowComponent : IInspectorComponent
{
	private readonly MemberDrawer memberDrawer;

	private readonly ComponentType componentType;

	private readonly string label;

	internal bool expandComponent;

	public override string ToString()
	{
		return componentType.Type.Name;
	}

	internal SingleRowComponent(ComponentType componentType, MemberDrawer memberDrawer)
	{
		this.componentType = componentType;
		this.memberDrawer = memberDrawer;
		label = componentType.Type.Name + "     ";
	}

	public void DrawComponentNode(DrawNode drawNode)
	{
		DrawContext context = drawNode.context;
		context.multiLine = false;
		InspectorCommand inspectorCommand = InspectorCommand.None;
		bool num = memberDrawer.typeDrawer is IExpandable;
		drawNode.PushClipRect();
		if (num)
		{
			ImGui.SetNextItemOpen(expandComponent);
			ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
			expandComponent = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanTextWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere);
			ImGui.PopItemFlag();
			ImGui.SameLine(context.rect.left);
		}
		else
		{
			ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
			expandComponent = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanTextWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere);
			ImGui.PopItemFlag();
			ImGui.SameLine(context.rect.left);
			ImGui.SetNextItemWidth(drawNode.Size.X);
		}
		drawNode.PopClipRect();
		if (drawNode.ShowTooltips && ImGui.BeginItemTooltip())
		{
			StringBuilder stringBuilder = TextUtils.Clear();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder);
			handler.AppendFormatted(memberDrawer.member.path);
			handler.AppendLiteral(" : ");
			StringBuilder stringBuilder3 = stringBuilder2.Append(ref handler);
			TypeUtils.AppendTypeName(stringBuilder3, memberDrawer.member.memberType);
			stringBuilder = stringBuilder3;
			StringBuilder stringBuilder4 = stringBuilder;
			handler = new StringBuilder.AppendInterpolatedStringHandler(9, 1, stringBuilder);
			handler.AppendLiteral(" - size: ");
			handler.AppendFormatted(componentType.StructSize);
			stringBuilder4.Append(ref handler);
			ImGui.Text(TextUtils.AsSpan(stringBuilder3));
			ImGui.EndTooltip();
		}
		ImGui.SetNextItemWidth(context.rect.size.X);
		DrawValue drawValue = new DrawValue(context, in memberDrawer, DrawValueFlags.Value);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
		memberDrawer.typeDrawer.DrawValue(in drawValue);
		if (EntityInspector.MorePopup("single_more", out var isFocused))
		{
			if (ImGui.MenuItem("Add column"))
			{
				drawNode.explorer.AddComponentFieldDrawer(memberDrawer);
			}
			ImGui.Separator();
			if (ImGui.MenuItem("Remove Component", "Delete"))
			{
				inspectorCommand = InspectorCommand.RemoveComponent;
			}
			ImGui.EndPopup();
		}
		if (num && expandComponent)
		{
			drawNode.inspector.Unindent();
			Rect rect = context.rect;
			EntityInspector.SetContextRectFullWidth(context);
			drawValue = new DrawValue(context, in memberDrawer, DrawValueFlags.Expansion);
			memberDrawer.typeDrawer.DrawValue(in drawValue);
			context.rect = rect;
			drawNode.inspector.Indent();
		}
		if (expandComponent)
		{
			ImGui.TreePop();
		}
		if (isFocused && ImGui.IsKeyPressed(ImGuiKey.Delete))
		{
			inspectorCommand = InspectorCommand.RemoveComponent;
		}
		ImGui.PopStyleVar();
		if (inspectorCommand == InspectorCommand.RemoveComponent)
		{
			EntityUtils.RemoveEntityComponent(drawNode.Entity, componentType);
			context.Edit();
		}
	}

	public bool HasFilterMatches(string filter)
	{
		if (componentType.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return memberDrawer.member.path.Contains(filter, StringComparison.OrdinalIgnoreCase);
	}
}
