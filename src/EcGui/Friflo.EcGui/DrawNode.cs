using System.Numerics;
using Friflo.EcGui;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal struct DrawNode
{
	internal DrawContext context;

	internal QueryExplorer explorer;

	internal EntityInspector inspector;

	internal Entity Entity => context.entity;

	internal Vector2 Size => context.rect.size;

	internal bool ShowTooltips => context.showTooltips;

	internal void PushClipRect()
	{
		Vector2 vector = ImGui.GetWindowPos() + new Vector2(0f - ImGui.GetScrollX(), ImGui.GetCursorPosY() - ImGui.GetScrollY());
		Vector2 clip_rect_max = vector + new Vector2(context.rect.left - UI.Scl(10f), ImGui.GetFrameHeight());
		ImGui.PushClipRect(vector, clip_rect_max, intersectWithCurrentClipRect: true);
	}

	internal void PopClipRect()
	{
		ImGui.PopClipRect();
	}
}
