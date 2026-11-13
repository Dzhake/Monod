using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;

namespace Monod.Graphics.Batching;

public class DrawBatcher
{
    public List<DrawBatch> Batches = new();

    public void Add(Entity entity, IDrawer drawer, RenderDepth depth, SystemCtx ctx)
    {
        Batches.Add(new(entity, depth, drawer));
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
