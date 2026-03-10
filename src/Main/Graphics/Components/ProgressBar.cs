using Microsoft.Xna.Framework;

namespace Monod.Graphics.Components;

/// <summary>
/// Allows drawing of simple progress bars based on given numbers.
/// </summary>
public static class ProgressBar
{
    /// <summary>
    /// Draw a simple two-rectangle left-to-right progress bar.
    /// </summary>
    /// <param name="fraction">Fraction of a bar that should be filled.</param>
    /// <param name="position">Position of bar's topleft.</param>
    /// <param name="size">Size of the bar in pixels.</param>
    /// <param name="color">Color of the bar.</param>
    /// <param name="borderWidth">Width of the bar's border.</param>
    public static void Draw(float fraction, Vector2 position, Vector2 size, Color? color = null, float borderWidth = 1f)
    {
        color ??= Color.White;
        Renderer.DrawHollowRect(position, position + size, color.Value, borderWidth);
        Renderer.DrawRect(position, new(position.X + size.X * fraction, position.Y + size.Y), color.Value);
    }
}