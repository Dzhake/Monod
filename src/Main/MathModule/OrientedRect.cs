namespace Monod.MathModule;

public record struct OrientedRect
{
    public Vector2 Center;
    public Vector2 Size;
    /// <summary>
    /// Angle in radians.
    /// </summary>
    public float Angle;

    public OrientedRect(Vector2 center, Vector2 size, float angle = 0)
    {
        Center = center;
        Size = size;
        Angle = angle;
    }

    public OrientedRect(float centerX, float centerY, float width, float height, float angle = 0) : this(new(centerX, centerY), new(width, height), angle) { }


    public readonly float Width => Size.X;
    public readonly float Height => Size.Y;
    public override readonly string ToString() => $"Center: {Center}; Width: {Width}; Height: {Height}; Angle: {Angle}";
}
