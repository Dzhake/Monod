using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics.ECS.Sprite;

public struct Sprite2D : IComponent
{
    public string TexturePath;
    public Color color;
    [Ignore]
    public Texture2D? Texture;

    public Sprite2D(string texturePath)
    {
        TexturePath = texturePath;
        color = Color.White;
    }

    public Sprite2D(string texturePath, Color color)
    {
        TexturePath = texturePath;
        this.color = color;
    }
}
