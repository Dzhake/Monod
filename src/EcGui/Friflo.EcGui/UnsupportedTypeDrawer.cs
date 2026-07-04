using System;
using System.Text;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class UnsupportedTypeDrawer : TypeDrawer
{
	private readonly Type type;

	public override string[] SortFields => Array.Empty<string>();

	internal UnsupportedTypeDrawer(Type type)
	{
		this.type = type;
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		string input = type.Name;
		ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.lightFrameBg);
		ImGui.InputText("##field", ref input, 0u, ImGuiInputTextFlags.ReadOnly);
		ImGui.PopStyleColor();
		StringBuilder stringBuilder = TextUtils.Clear();
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, stringBuilder);
		handler.AppendLiteral("Type '");
		handler.AppendFormatted(input);
		handler.AppendLiteral("' requires a TypeDrawer");
		stringBuilder.Append(ref handler);
		ImGui.SetItemTooltip(TextUtils.StringBufferAsSpan());
		return TypeDrawer.Flags();
	}
}
