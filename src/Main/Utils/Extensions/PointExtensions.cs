using Microsoft.Xna.Framework;

namespace Monod.Utils.Extensions;

public static class PointExtensions
{
    /// <summary>
    /// Divide each component of the <paramref name="point"/> by the <paramref name="scalar"/>.
    /// </summary>
    /// <param name="point">Point to divide.</param>
    /// <param name="scalar">Scalar value. Must not be 0; otherwise, <see cref="DivideByZeroException"/> will be thrown.</param>
    /// <returns>A new point.</returns>
    public static Point Divide(this Point point, int scalar) => new(point.X / scalar, point.Y / scalar);

    /// <summary>
    /// Multiplies each component of the <paramref name="point"/> by the <paramref name="scalar"/>.
    /// </summary>
    /// <param name="point">Point to multiply.</param>
    /// <param name="scalar">Scalar value.</param>
    /// <returns>A new point.</returns>
    public static Point Mult(this Point point, int scalar) => new(point.X * scalar, point.Y * scalar);
}
