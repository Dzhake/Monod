using Microsoft.Xna.Framework;

namespace Monod.Graphics.Extensions;

/// <summary>
/// Extensions for <see cref="Color"/>.
/// </summary>
public static class ColorExtensions
{
    public static Color WithAlpha(this Color color, byte alpha) => color with { A = alpha };
}
