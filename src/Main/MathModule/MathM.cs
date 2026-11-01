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
    public static readonly float Epsilon = 0.001f;

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

    public static Vector2 VectorFromRot(float rotation)
    {
        // Looks like MG.Extended's Vec->Angle conversion considers 'up' to be the default? Or something like that. Anyway, doing this makes the conversion work correctly both ways.
        float angle = rotation - (MathF.PI / 2);
        return new(MathF.Cos(angle), MathF.Sin(angle));
    }
}
