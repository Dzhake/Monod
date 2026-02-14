using Microsoft.Xna.Framework;
using Monod.Graphics;

namespace Monod.Graphics.Components;

/// <summary>
/// Allows drawing of simple progress bars based on given numbers.
/// </summary>
public static class ProgressBar
{
    public static void Draw(float fraction, Vector2 position, Vector2 size, Color? color = null, float borderWidth = 1f)
    {
        color ??= Color.White;
        Renderer.DrawHollowRect(position, position + size, color.Value, borderWidth);
        Renderer.DrawRect(position, new(position.X + size.X * fraction, position.Y + size.Y), color.Value);
    }
}