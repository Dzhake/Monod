using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics;

/// <summary>
/// A single region on the texture, or the entire texture. MTexture stands for monoc- Monod texture, obviously.
/// </summary>
public class MTexture
{
    /// <summary>
    /// Base texture for the region.
    /// </summary>
    public Texture2D Base;

    /// <summary>
    /// Region of <see cref="Base"/>, or <see langword="null"/> if the texture is the same as the <see cref="Base"/>.
    /// </summary>
    public Rectangle? Region;

    /// <summary>
    /// Width of the region.
    /// </summary>
    public int Width => Region?.Width ?? Base.Width;

    /// <summary>
    /// Height of the region.
    /// </summary>
    public int Height => Region?.Height ?? Base.Height;

    /// <summary>
    /// Create a new instance of the <see cref="MTexture"/> with the specified <see cref="Base"/> and <see cref="Region"/>.
    /// </summary>
    /// <param name="baseTexture">Base texture for the region.</param>
    /// <param name="region">Region of <see cref="Base"/>, or <see langword="null"/> if the texture is the same as the <see cref="Base"/>.</param>
    public MTexture(Texture2D baseTexture, Rectangle? region = null)
    {
        Base = baseTexture;
        Region = region;
    }

    /// <summary>
    /// Get the <see cref="Region"/>, or if the <see cref="Region"/> is <see langword="null"/>, rectangle with size of the whole texture.
    /// </summary>
    /// <returns>The <see cref="Region"/>, or if the <see cref="Region"/> is <see langword="null"/>, rectangle with size of the whole texture.</returns>
    public Rectangle GetBounds()
    {
        return Region ?? new(0, 0, Base.Width, Base.Height);
    }
}
