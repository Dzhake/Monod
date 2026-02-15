using MLEM.Font;

namespace Monod.Graphics.Fonts;

/// <summary>
/// Stores <see cref="GenericSpriteFont"/> that are used globally across the entire program.
/// </summary>
public static class GlobalFonts
{
    /// <summary>
    /// Font used for menus (including loading screens).
    /// </summary>
    public static GenericFont? MenuFont;
}