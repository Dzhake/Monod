using Hexa.NET.ImGui;
using System;
using System.Text;

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
        var handler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, stringBuilder);
        handler.AppendLiteral("Type '");
        handler.AppendFormatted(input);
        handler.AppendLiteral("' requires a TypeDrawer");
        stringBuilder.Append(ref handler);
        ImGui.SetItemTooltip(ref TextUtils.StringBufferAsBytes()[0]);
        return TypeDrawer.Flags();
    }
}
