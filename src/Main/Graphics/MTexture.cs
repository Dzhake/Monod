using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics;

public class MTexture
{
    public Texture2D Base;
    public Rectangle? Region;

    public int Width => Region?.Width ?? Base.Width;
    public int Height => Region?.Height ?? Base.Height;


    public MTexture(Texture2D baseTexture, Rectangle? region = null)
    {
        Base = baseTexture;
        Region = region;
    }
}
