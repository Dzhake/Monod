using Friflo.Json.Fliox;
using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics.ECS.Sprite;

public struct Sprite2D : IComponent
{
    public string TexturePath;
    public Color color = Color.White;
    public SpriteEffects spriteEffects;
    [Ignore]
    public MTexture? Texture;
    public bool InvalidateTexture;

    public Sprite2D()
    {
    }

    public Sprite2D(string texturePath)
    {
        TexturePath = texturePath;
    }

    public Sprite2D(string texturePath, Color? color = null, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        TexturePath = texturePath;
        this.color = color ?? Color.White;
        this.spriteEffects = spriteEffects;
    }
}
