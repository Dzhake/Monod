namespace Monod.MathModule;

/// <summary>
/// <see cref="Math"/> for <see cref="Monod"/>.
/// </summary>
public static class MathM
{
    public static Vector2 VectorUp = new(0, -1);
    public static Vector2 VectorDown = new(0, 1);
    public static Vector2 VectorRight = new(1, 0);
    public static Vector2 VectorLeft = new(-1, 0);

    /// <summary>
    /// A small value.
    /// </summary>
    public static readonly float Epsilon = 0.000001f;

    public static void LerpFloat(ref float from, float to, float amount)
    {
        if (from > to)
        {
            from -= amount;
            if (from < to) from = to;
        }
        else if (from < to)
        {
            from += amount;
            if (from > to) from = to;
        }
    }
}
