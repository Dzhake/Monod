namespace Monod.MathModule;

/// <summary>
/// <see cref="Math"/> for <see cref="Monod"/>.
/// </summary>
public static class MathM
{
    /// <summary>
    /// A small value, by default 0.001.
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
