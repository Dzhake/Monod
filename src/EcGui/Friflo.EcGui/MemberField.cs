using System;
using System.Text;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class MemberField : IMember
{
	private readonly MemberDrawer fieldDrawer;

	private bool expandField;

	public override string ToString()
	{
		return fieldDrawer.member.ToString();
	}

	internal MemberField(MemberDrawer fieldDrawer)
	{
		this.fieldDrawer = fieldDrawer;
	}

	public void DrawMemberNode(DrawNode drawNode)
	{
		MemberPath member = fieldDrawer.member;
		DrawContext context = drawNode.context;
		if (drawNode.explorer.ActiveColumnMember == member)
		{
			EntityInspector.DrawRectBg(GlobalColors.fieldActiveBg);
		}
		bool num = fieldDrawer.typeDrawer is IExpandable;
		drawNode.PushClipRect();
		if (num)
		{
			ImGui.SetNextItemOpen(expandField);
			ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
			expandField = ImGui.TreeNodeEx(fieldDrawer.label, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanTextWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere);
			ImGui.PopItemFlag();
		}
		else
		{
			ImGui.SetNextItemOpen(expandField);
			ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
			ImGui.SetNextItemWidth(300f);
			expandField = ImGui.TreeNodeEx(fieldDrawer.label, ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanTextWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere);
			ImGui.PopItemFlag();
		}
		drawNode.PopClipRect();
		drawNode.inspector.memberDepth++;
		if (drawNode.ShowTooltips && ImGui.BeginItemTooltip())
		{
			StringBuilder sb = TextUtils.Clear().Append("Type: ");
			TypeUtils.AppendTypeName(sb, fieldDrawer.member.memberType);
			ImGui.Text(TextUtils.AsSpan(sb));
			ImGui.EndTooltip();
		}
		ImGui.SameLine(context.rect.left);
		ImGui.SetNextItemWidth(drawNode.Size.X);
		DrawValue drawValue = new DrawValue(context, in fieldDrawer, DrawValueFlags.Value);
		fieldDrawer.typeDrawer.DrawValue(in drawValue);
		if (EntityInspector.MorePopup("field_more", out var _))
		{
			if (ImGui.MenuItem("Add column"))
			{
				drawNode.explorer.AddComponentFieldDrawer(fieldDrawer);
			}
			ImGui.EndPopup();
		}
		if (num && expandField)
		{
			float indent_w = (float)drawNode.inspector.memberDepth * drawNode.inspector.indentWidth;
			ImGui.Unindent(indent_w);
			Rect rect = context.rect;
			EntityInspector.SetContextRectFullWidth(context);
			drawValue = new DrawValue(context, in fieldDrawer, DrawValueFlags.Expansion);
			fieldDrawer.typeDrawer.DrawValue(in drawValue);
			context.rect = rect;
			ImGui.Indent(indent_w);
		}
		if (expandField)
		{
			ImGui.TreePop();
		}
		drawNode.inspector.memberDepth--;
	}

	public void AddExplorerColumns(QueryExplorer explorer)
	{
		explorer.AddComponentFieldDrawer(fieldDrawer);
	}

	public bool HasFilterMatches(string filter)
	{
		return fieldDrawer.member.path.Contains(filter, StringComparison.OrdinalIgnoreCase);
	}
}
