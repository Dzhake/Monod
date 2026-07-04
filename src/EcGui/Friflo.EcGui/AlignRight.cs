using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal readonly struct AlignRight
{
    internal readonly string label;

    internal readonly float width;

    internal readonly float left;

    public override string ToString()
    {
        return label;
    }

    internal AlignRight(string label, float right)
    {
        this.label = label;
        width = ImGui.CalcTextSize(label).X;
        left = right - width - SpacingX();
    }

    internal AlignRight(string label, float right, bool fixedWidth, float width)
    {
        this.label = label;
        if (fixedWidth)
        {
            this.width = width + SpacingX();
        }
        else
        {
            this.width = ImGui.CalcTextSize(label).X + width + SpacingX();
        }
        left = right - this.width;
    }

    private static float SpacingX()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        return style.ItemSpacing.X + (2f * style.FramePadding.X);
    }

    internal void SameLine()
    {
        ImGui.SameLine(left);
    }
}
