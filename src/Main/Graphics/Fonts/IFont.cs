using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.Graphics;
using Monod.Graphics.Fonts.Bitmap;

namespace Monod.Graphics.Fonts;

/// <summary>
/// A text font that can be rendered.
/// </summary>
public interface IFont
{
    /// <summary>
    /// Get texture of rendered <paramref name="text"/> for caching. Uses <see cref="Renderer.spriteBatch"/>.
    /// </summary>
    /// <param name="text">Text to render with this <see cref="BitmapFont"/>.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public Texture2D GetStringTexture(string text, Color? color = null, float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f);

    /// <summary>
    /// Draws <paramref name="text"/> via <see cref="Renderer.spriteBatch"/> with specified options using this <see cref="BitmapFont"/>. <see cref="Renderer.spriteBatch"/> must be active.
    /// </summary>
    /// <param name="text">Text to draw.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public void DrawText(string text,Vector2 position, Color? color = null, float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0);
}