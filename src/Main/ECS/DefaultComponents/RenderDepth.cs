namespace Monod.ECS.DefaultComponents;

[ComponentKey("RenderDepth")]
[ComponentSymbol("D")]
public record struct RenderDepth : IComponent, IComparable<RenderDepth>
{
    public float BaseDepth;

    public float YSortOffset;

    public float PartOffset;

    public RenderDepth(float baseDepth, float partOffset = 0)
    {
        BaseDepth = baseDepth;
        PartOffset = partOffset;
    }

    ///<inheritdoc/>
    public readonly int CompareTo(RenderDepth other)
    {
        int baseCompare = BaseDepth.CompareTo(other.BaseDepth);
        if (baseCompare != 0) return baseCompare;

        int yCompare = YSortOffset.CompareTo(other.YSortOffset);
        if (yCompare != 0) return yCompare;

        return PartOffset.CompareTo(other.PartOffset);
    }
}
