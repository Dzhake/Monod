using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsModule;

namespace Monod.Graphics.ECS.Sprite;

public class UpdateSpriteSystem : QuerySystem<Sprite2D>
{
    private UpdateEach update = new();

    protected override void OnUpdate()
    {
        Query.Each(update);
    }

    public struct UpdateEach : IEach<Sprite2D>
    {
        public void Execute(ref Sprite2D sprite)
        {
            if (sprite.Texture is not null && !Assets.ReloadThisFrame && !sprite.InvalidateTexture) return;
            sprite.InvalidateTexture = false;
            sprite.Texture = string.IsNullOrEmpty(sprite.TexturePath) ? null : new(Assets.Get<Texture2D>(sprite.TexturePath));
        }
    }
}
