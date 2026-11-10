using Monod.ECS.DefaultComponents;

namespace DerelictDimension.ECS.Drawing.Batching;

public record struct DrawBatch : IComparable<DrawBatch>
{
    public Entity entity;
    public RenderDepth renderDepth;
    public IDrawer Drawer;

    public DrawBatch(Entity entity, RenderDepth renderDepth, IDrawer drawer)
    {
        this.entity = entity;
        this.renderDepth = renderDepth;
        Drawer = drawer;
    }

    ///<inheritdoc/>
    public readonly int CompareTo(DrawBatch other)
    {
        return -renderDepth.CompareTo(other.renderDepth);
    }

    ///<inheritdoc/>
    public static bool operator <(DrawBatch left, DrawBatch right)
    {
        return left.CompareTo(right) < 0;
    }

    ///<inheritdoc/>
    public static bool operator <=(DrawBatch left, DrawBatch right)
    {
        return left.CompareTo(right) <= 0;
    }

    ///<inheritdoc/>
    public static bool operator >(DrawBatch left, DrawBatch right)
    {
        return left.CompareTo(right) > 0;
    }

    ///<inheritdoc/>
    public static bool operator >=(DrawBatch left, DrawBatch right)
    {
        return left.CompareTo(right) >= 0;
    }
}
