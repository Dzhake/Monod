using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework.Graphics;
using Monod.ECS.DefaultComponents;
using Monod.Utils.Extensions;

namespace Monod.Graphics.ECS.Sprite;

public class DrawSpriteSystem : QuerySystem<Sprite2D>
{
    protected override void OnUpdate()
    {
        Renderer.Begin(SpriteSortMode.BackToFront);
        Query.ForEachEntity(Update);
        Renderer.End();
    }

    private void Update(ref Sprite2D sprite, Entity entity)
    {
        if (sprite.Texture is null) return;

        EntityData data = entity.Data;

        Vector2 position = entity.GetWorldPos(out float rotation);
        Vector2 scale;
        if (data.TryGet(out Transform2D transform))
        {
            scale = transform.Scale;
        }
        else
        {
            scale = Vector2.One;
        }

        float depth;
        if (data.TryGet(out RenderDepth renderDepth))
            depth = renderDepth.Depth;
        else
            depth = 0;

        Vector2 origin = new(sprite.Texture.Base.Width / 2f, sprite.Texture.Base.Height / 2f);

        Renderer.DrawTexture(sprite.Texture.Base, position, sprite.color, sprite.Texture.Region, rotation, origin, scale, SpriteEffects.None, depth);
    }
}
