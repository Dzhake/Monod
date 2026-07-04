using System;
using System.Text;
using Friflo.EcGui.Friflo.EcGui;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class InspectorRelation
{
	private readonly MemberDrawer memberDrawer;

	private readonly ComponentType relationType;

	private readonly string label;

	private bool expandRelation;

	public override string ToString()
	{
		return relationType.Type.Name;
	}

	internal InspectorRelation(MemberDrawer memberDrawer, ComponentType relationType)
	{
		this.memberDrawer = memberDrawer;
		this.relationType = relationType;
		label = relationType.Type.Name + "     ";
	}

	internal bool HasFilterMatches(string filter)
	{
		return relationType.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
	}

	internal void DrawRelationNode(DrawNode drawNode)
	{
		DrawContext context = drawNode.context;
		context.multiLine = false;
		InspectorCommand inspectorCommand = InspectorCommand.None;
		drawNode.PushClipRect();
		ImGui.SetNextItemOpen(expandRelation);
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
		expandRelation = ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanLabelWidth | ImGuiTreeNodeFlags.LabelSpanAllColumns);
		ImGui.PopItemFlag();
		drawNode.PopClipRect();
		ImGui.SameLine(context.rect.left);
		if (drawNode.ShowTooltips && ImGui.BeginItemTooltip())
		{
			StringBuilder sb = TextUtils.Clear();
			TypeUtils.AppendTypeName(sb, memberDrawer.member.memberType);
			ImGui.Text(TextUtils.AsSpan(sb));
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
			if (ImGui.MenuItem("Remove Relations", "Delete"))
			{
				inspectorCommand = InspectorCommand.RemoveComponent;
			}
			ImGui.EndPopup();
		}
		if (expandRelation)
		{
			drawNode.inspector.Unindent();
			Rect rect = context.rect;
			EntityInspector.SetContextRectFullWidth(context);
			drawValue = new DrawValue(context, in memberDrawer, DrawValueFlags.Expansion);
			memberDrawer.typeDrawer.DrawValue(in drawValue);
			context.rect = rect;
			drawNode.inspector.Indent();
		}
		if (expandRelation)
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
			EntityInspector.RemoveRelations(drawNode.Entity, relationType);
			context.Edit();
		}
	}
}
