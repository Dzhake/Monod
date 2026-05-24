using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.ECS.DefaultComponents;

namespace Monod.Graphics.ECS.Sprite;

public class DrawSpriteSystem : QuerySystem<Sprite2D>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(Update);
    }

    private void Update(ref Sprite2D sprite, Entity entity)
    {
        if (sprite.Texture is null) return;

        var data = entity.Data;

        Vector2 position;
        if (data.TryGet<Position2D>(out var position2D))
            position = position2D.Value;
        else
            position = Vector2.Zero;

        float rotation;
        if (data.TryGet<Rotation2D>(out var rotation2D))
            rotation = rotation2D.Angle;
        else
            rotation = 0;

        Vector2 scale;
        if (data.TryGet<Scale2D>(out var scale2D))
            scale = scale2D.Value;
        else
            scale = Vector2.One;

        float depth;
        if (data.TryGet<RenderDepth>(out var renderDepth))
            depth = renderDepth.Depth;
        else
            depth = 0;

        Vector2 origin = new(sprite.Texture.Width / 2f, sprite.Texture.Height / 2f);

        Renderer.DrawTexture(sprite.Texture, position, sprite.color, null, rotation, origin, scale, SpriteEffects.None, depth);
    }
}
