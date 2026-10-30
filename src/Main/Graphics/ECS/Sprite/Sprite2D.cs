using Friflo.Json.Fliox;
using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics.ECS.Sprite;

public struct Sprite2D : IComponent
{
    public string TexturePath;
    public Color color = Color.White;
    public SpriteEffects spriteEffects;
    public bool Visible = true;
    public float Rotation;

    [Ignore]
    public MTexture? Texture;
    public bool InvalidateTexture;

    public Sprite2D()
    {
    }

    public Sprite2D(string texturePath = "", Color? color = null, float rotation = 0f, SpriteEffects spriteEffects = SpriteEffects.None)
    {
        TexturePath = texturePath;
        this.color = color ?? Color.White;
        this.spriteEffects = spriteEffects;
        Rotation = rotation;
    }
}
