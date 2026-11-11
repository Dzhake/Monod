using MLEM.Maths;

public struct AABB
{
    public float CenterX;
    public float CenterY;
    public float HalfWidth;
    public float HalfHeight;

    public AABB(float centerX, float centerY, float halfWidth, float halfHeight)
    {
        CenterX = centerX;
        CenterY = centerY;
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
    }

    public Vector2 Center
    {
        readonly get => new(CenterX, CenterY);
        set { CenterX = value.X; CenterY = value.Y; }
    }

    public Vector2 HalfSize
    {
        readonly get => new(HalfWidth, HalfHeight);
        set { HalfWidth = value.X; HalfHeight = value.Y; }
    }

    public Vector2 Size
    {
        readonly get => new(HalfWidth * 2, HalfHeight * 2);
        set { HalfWidth = value.X / 2; HalfHeight = value.Y / 2; }
    }

    public float X { readonly get => CenterX - HalfWidth; set => CenterX = value + HalfWidth; }
    public float Y { readonly get => CenterY - HalfHeight; set => CenterY = value + HalfHeight; }

    public float Width { readonly get => HalfWidth * 2f; set => HalfWidth = value / 2; }
    public float Height { readonly get => HalfHeight * 2f; set => HalfHeight = value / 2; }

    public float Bottom { readonly get => CenterY + HalfHeight; set => CenterY = value - HalfHeight; }
    public float Top { readonly get => CenterY - HalfHeight; set => CenterY = value + HalfHeight; }
    public float Right { readonly get => CenterX + HalfWidth; set => CenterX = value - HalfWidth; }
    public float Left { readonly get => CenterX - HalfWidth; set => CenterX = value + HalfWidth; }

    public readonly Vector2 TopLeft => new(Left, Top);
    public readonly Vector2 TopRight => new(Right, Top);
    public readonly Vector2 BottomLeft => new(Left, Bottom);
    public readonly Vector2 BottomRight => new(Right, Bottom);

    public static explicit operator RectangleF(AABB aabb) => new(aabb.X, aabb.Y, aabb.Width, aabb.Height);
    public static explicit operator Rectangle(AABB aabb) => new((int)aabb.X, (int)aabb.Y, (int)aabb.Width, (int)aabb.Height);

    public static bool operator ==(AABB a, AABB b) => a.CenterX == b.CenterX && a.CenterY == b.CenterY && a.HalfWidth == b.HalfWidth && a.HalfHeight == b.HalfHeight;
    public static bool operator !=(AABB a, AABB b) => a.CenterX != b.CenterX || a.CenterY != b.CenterY || a.HalfWidth != b.HalfWidth || a.HalfHeight != b.HalfHeight;

    public AABB Union(AABB other)
    {
        float left = Math.Min(Left, other.Left);
        float right = Math.Max(Right, other.Right);
        float bottom = Math.Max(Bottom, other.Bottom);
        float top = Math.Min(Top, other.Top);
        return new AABB((left + right) / 2, (bottom + top) / 2, (right - left) / 2, (bottom - top) / 2);
    }

    public override readonly string ToString() => $"CenterX: {CenterX}, CenterY: {CenterY}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}";

    public override readonly bool Equals(object? obj) => obj is AABB aabb && aabb == this;

    public override readonly int GetHashCode() => HashCode.Combine(CenterX, CenterY, HalfWidth, HalfHeight);
}
