using Monod.ECS.Systems;

namespace Monod.Graphics.Batching;

public interface IDrawer
{
    public void Draw(Entity entity, SystemCtx ctx);
}
