namespace Monod.Utils.Extensions;

public static class Vector2Extensions
{
    public static void NormalizeSafe(this ref Vector2 v)
    {
        if (v == Vector2.Zero) return;
        v.Normalize();
    }
}
