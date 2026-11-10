using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing.Batching;

public class DrawBatcher
{
    public List<DrawBatch> Batches = new();

    public void Add(Entity entity, IDrawer drawer, SystemCtx ctx)
    {
        Batches.Add(new(entity, entity.Data.Ensure<RenderDepth>(ctx.cb), drawer));
    }

    public void Flush(SystemCtx ctx)
    {
        Batches.Sort();

        IDrawer? prev = null;

        Renderer.Begin();
        foreach (var batch in Batches)
        {
            if (prev != null && prev != batch.Drawer)
            {
                Renderer.End();
                Renderer.Begin();
            }

            batch.Drawer.Draw(batch.entity, ctx);
            prev = batch.Drawer;
        }

        Batches.Clear();
        Renderer.End();
    }
}
