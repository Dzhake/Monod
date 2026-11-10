using Monod.ECS.Systems;

namespace DerelictDimension.ECS.Drawing.Batching;

public interface IDrawer
{
    public void Draw(Entity entity, SystemCtx ctx);
}
